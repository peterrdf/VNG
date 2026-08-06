using RDF;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace VNGPortal.IFC2RDF
{
    public class Geometry2RDF
    {
        #region Constants
        private const long flagbit0 = 1;   // 2^^0
        private const long flagbit1 = 2;   // 2^^1
        private const long flagbit2 = 4;   // 2^^2
        private const long flagbit3 = 8;   // 2^^3
        private const long flagbit4 = 16;   // 2^^4

        private const string GraphIRI = "https://vng.nl/geometries/";

        // Sanitize a string so it is safe to use as a Turtle local name
        private static string ToSafeTurtleLocalName(string value)
            => Regex.Replace(value, @"[^A-Za-z0-9_\-]", "_");
        #endregion

        #region Fields
        private readonly ILogger _logger;
        #endregion

        public Geometry2RDF(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task Run(string inputFile)
        {
            long sdaiModel = 0;

            try
            {
                if (!File.Exists(inputFile))
                {
                    _logger.LogError("Input file does not exist: {InputFile}", inputFile);
                    throw new FileNotFoundException($"Input file does not exist: {inputFile}", inputFile);
                }

                var outputFile = Path.ChangeExtension(inputFile, null) + "_geometry.trig";

                var model = new IFC.IFCModel();
                if ((model != null) && model.Load(inputFile))
                {
                    sdaiModel = model.Instance;
                    _logger.LogInformation("Model loaded successfully.");
                }
                else
                {
                    _logger.LogError("Failed to load model: {InputFile}", inputFile);
                    throw new InvalidOperationException($"Failed to load model: {inputFile}");
                }

                // Scale
                double dScale = ifcengine.getProjectUnitConversionFactor(sdaiModel, "LENGTHUNIT", out IntPtr _, out IntPtr _, out IntPtr _);
                if (dScale == 0.0)
                {
                    dScale = 1.0;
                }

                long owlModel = 0;
                ifcengine.owlGetModel(sdaiModel, out owlModel);

                long setting = flagbit0 + flagbit4;
                long mask = flagbit0 + flagbit4;
                engine.SetOverrideFileIO(owlModel, setting, mask);

                var sb = new StringBuilder();
                sb.AppendLine("@prefix geom:     <https://vng.nl/geometry/> .");
                sb.AppendLine("@prefix rdf:      <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .");
                sb.AppendLine("@prefix xsd:      <http://www.w3.org/2001/XMLSchema#> .");
                sb.AppendLine();

                // Open named graph
                // https://www.w3.org/TR/trig/
                sb.AppendLine($"GRAPH <{GraphIRI}> {{");
                sb.AppendLine();

                foreach (var prGeometry in model.Geometries)
                {
                    long expressID = ifcengine.internalGetP21Line(prGeometry.Value.Instance);

                    ifcengine.sdaiGetAttrBN(prGeometry.Value.Instance, "GlobalId", ifcengine.sdaiSTRING, out IntPtr ptrGlobalId);
                    string? globalId = string.Empty;
                    if (ptrGlobalId != IntPtr.Zero)
                    {
                        globalId = Marshal.PtrToStringAnsi(ptrGlobalId);
                    }

                    if (prGeometry.Value.OwlInstance != 0 && !string.IsNullOrEmpty(globalId))
                    {
                        var modelPath = Path.Combine(Path.GetTempPath(), "temp.bin");

                        long owlTransformationInstance = CreateMapConversionTransformation(sdaiModel, dScale, out double dEastings, out double dNorthings);
                        if (owlTransformationInstance != 0)
                        {
                            engine.SetObjectProperty(
                                owlTransformationInstance,
                                engine.GetPropertyByName(owlModel, "object"),
                                prGeometry.Value.OwlInstance);
                            engine.SaveInstanceTree(owlTransformationInstance, modelPath);
                            engine.RemoveInstance(owlTransformationInstance);
                        }
                        else
                        {
                            engine.SaveInstanceTree(prGeometry.Value.OwlInstance, modelPath);
                        }

                        //// BBox
                        //double[]? transformationMatrix = null;
                        //double[] startVector = new double[3];
                        //double[] endVector = new double[3];
                        //engine.GetBoundingBox(prGeometry.Value.OwlInstance, transformationMatrix, startVector, endVector);

                        //// Scale
                        //startVector[0] *= dScale; startVector[1] *= dScale; startVector[2] *= dScale;
                        //endVector[0] *= dScale; endVector[1] *= dScale; endVector[2] *= dScale;

                        var base64Content = Convert.ToBase64String(File.ReadAllBytes(modelPath));
                        File.Delete(modelPath);

                        // Sanitize entity & expressID
                        var safeEntityName = ToSafeTurtleLocalName(prGeometry.Value.Entity ?? string.Empty);
                        var safeExpressId = ToSafeTurtleLocalName(expressID.ToString());
                        var subjectName = $"{safeEntityName}_{safeExpressId}";

                        sb.AppendLine($"    geom:{subjectName}");
                        sb.AppendLine($"            rdf:type             geom:Geometry ;");
                        sb.AppendLine($"            geom:globalId        \"{globalId}\" ;");
                        sb.AppendLine($"            geom:eastings        \"{dEastings.ToString(System.Globalization.CultureInfo.InvariantCulture)}\"^^<http://www.w3.org/2001/XMLSchema#double> ;");
                        sb.AppendLine($"            geom:northings        \"{dNorthings.ToString(System.Globalization.CultureInfo.InvariantCulture)}\"^^<http://www.w3.org/2001/XMLSchema#double> ;");
                        sb.AppendLine($"            geom:base64Data      \"{base64Content}\" .");
                        //sb.AppendLine($"            geom:bboxMinX        {startVector[0].ToString(System.Globalization.CultureInfo.InvariantCulture)} ;");
                        //sb.AppendLine($"            geom:bboxMinY        {startVector[1].ToString(System.Globalization.CultureInfo.InvariantCulture)} ;");
                        //sb.AppendLine($"            geom:bboxMinZ        {startVector[2].ToString(System.Globalization.CultureInfo.InvariantCulture)} ;");
                        //sb.AppendLine($"            geom:bboxMaxX        {endVector[0].ToString(System.Globalization.CultureInfo.InvariantCulture)} ;");
                        //sb.AppendLine($"            geom:bboxMaxY        {endVector[1].ToString(System.Globalization.CultureInfo.InvariantCulture)} ;");
                        //sb.AppendLine($"            geom:bboxMaxZ        {endVector[2].ToString(System.Globalization.CultureInfo.InvariantCulture)} .");
                        sb.AppendLine();
                    }
                }

                // Close named graph
                sb.AppendLine("}");

                File.WriteAllText(outputFile, sb.ToString(), Encoding.UTF8);
                _logger.LogInformation("Output written to: {OutputFile}", outputFile);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error: {Message}", ex.Message);
                throw;
            }
            finally
            {
                if (sdaiModel != 0)
                {
                    ifcengine.sdaiCloseModel(sdaiModel);
                }
            }
        }

        private long CreateMapConversionTransformation(long sdaiModel, double dScale, out double dEastings, out double dNorthings)
        {
            // Default: no translation
            dEastings = 0.0;
            dNorthings = 0.0;

            long owlModel = 0;
            ifcengine.owlGetModel(sdaiModel, out owlModel);

            long sdaiAggr = ifcengine.sdaiGetEntityExtentBN(sdaiModel, "IFCMAPCONVERSION");
            if (sdaiAggr == 0)
            {
                _logger.LogWarning("No IFCMAPCONVERSION entity found in the model.");
                return 0;
            }

            ifcengine.engiGetAggrElement(sdaiAggr, 0, ifcengine.sdaiINSTANCE, out long sdaiIfcMapConversionInstance);
            if (sdaiIfcMapConversionInstance == 0)
            {
                _logger.LogWarning("No instance of IFCMAPCONVERSION found in the model.");
                return 0;
            }

            long sdaiIfcMapConversionEntity = ifcengine.sdaiGetInstanceType(sdaiIfcMapConversionInstance);
            if (sdaiIfcMapConversionEntity == 0)
            {
                _logger.LogWarning("Failed to get the entity type of the IFCMAPCONVERSION instance.");
                return 0;
            }

            // Default: no rotation
            double dXAxisAbscissa = 1.0;
            double dXAxisOrdinate = 0.0;

            bool bHasXAxisAbscissa = false;
            bool bHasXAxisOrdinate = false;

            long iIndex = 0;
            long sdaiAttr = ifcengine.engiGetEntityAttributeByIndex(
                sdaiIfcMapConversionEntity,
                iIndex++,
                false,
                true);

            while (sdaiAttr != 0)
            {
                ifcengine.engiGetEntityArgumentName(sdaiIfcMapConversionEntity,
                    iIndex,
                    ifcengine.sdaiSTRING,
                    out IntPtr attributeName);
                if (attributeName != IntPtr.Zero)
                {
                    string? strAttributeName = Marshal.PtrToStringAnsi(attributeName);
                    if (strAttributeName == "Eastings")
                    {
                        ifcengine.sdaiGetAttrBN(sdaiIfcMapConversionInstance, strAttributeName, ifcengine.sdaiREAL, out dEastings);
                    }
                    else if (strAttributeName == "Northings")
                    {
                        ifcengine.sdaiGetAttrBN(sdaiIfcMapConversionInstance, strAttributeName, ifcengine.sdaiREAL, out dNorthings);
                    }
                    else if (strAttributeName == "XAxisAbscissa")
                    {
                        ifcengine.sdaiGetAttrBN(sdaiIfcMapConversionInstance, strAttributeName, ifcengine.sdaiREAL, out dXAxisAbscissa);
                        bHasXAxisAbscissa = true;
                    }
                    else if (strAttributeName == "XAxisOrdinate")
                    {
                        ifcengine.sdaiGetAttrBN(sdaiIfcMapConversionInstance, strAttributeName, ifcengine.sdaiREAL, out dXAxisOrdinate);
                        bHasXAxisOrdinate = true;
                    }
                }

                sdaiAttr = ifcengine.engiGetEntityAttributeByIndex(
                    sdaiIfcMapConversionEntity,
                    iIndex++,
                    false,
                    true);
            } // while (sdaiAttr != 0)

            if ((dEastings == 0.0) && (dNorthings == 0.0) && !bHasXAxisAbscissa && !bHasXAxisOrdinate)
            {
                _logger.LogInformation("No translation or rotation found in IFCMAPCONVERSION. No transformation will be applied.");
                return 0;
            }

            // Calculate rotation angle from XAxisAbscissa and XAxisOrdinate (direction of the local X-axis in the map coordinate system)
            double dRotationAngle = Math.Atan2(dXAxisOrdinate, dXAxisAbscissa);
            double dCos = Math.Cos(dRotationAngle) * dScale;
            double dSin = Math.Sin(dRotationAngle) * dScale;

            long owlMatrixInstance = engine.CreateInstance(engine.GetClassByName(owlModel, "Matrix"));

            // Rotation + scale around Z-axis (yaw), then translation
            List<double> vecMatrix = new List<double>
            {
                dCos,       // _11
                dSin,       // _12
                0.0,        // _13
                -dSin,      // _21
                dCos,       // _22
                0.0,        // _23
                0.0,        // _31
                0.0,        // _32
                dScale,     // _33
                dEastings,  // _41
                dNorthings, // _42
                0.0,        // _43
            };
            engine.SetDatatypeProperty(
                            owlMatrixInstance,
                            engine.GetPropertyByName(owlModel, "coordinates"),
                            vecMatrix.ToArray(),
                            vecMatrix.Count);

            long owlTransformationInstance = engine.CreateInstance(engine.GetClassByName(owlModel, "Transformation"));
            engine.SetObjectProperty(
                                owlTransformationInstance,
                                engine.GetPropertyByName(owlModel, "matrix"),
                                owlMatrixInstance);

            return owlTransformationInstance;
        }
    }
}
