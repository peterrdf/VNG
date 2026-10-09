using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RDF;
using System;

namespace VNGService.Pages
{
    [IgnoreAntiforgeryToken]
    public class GeometryModelingModel : PageModel
    {
        private const long flagbit0 = 1;   // 2^^0
        private const long flagbit4 = 16;  // 2^^4

        private readonly IConfiguration _configuration;
        private readonly ILogger<DSOModel> _logger;

        public GeometryModelingModel(IConfiguration configuration, ILogger<DSOModel> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public void OnGet()
        {
        }

        public IActionResult OnGetCreateCylinder(
            double length, double radius, long segmentationParts,
            double? r = null, double? g = null, double? b = null, double? t = null)
        {
            var base64Content = string.Empty;

            long owlModel = engine.CreateModel();
            long setting = flagbit0 + flagbit4;
            long mask = flagbit0 + flagbit4;
            engine.SetOverrideFileIO(owlModel, setting, mask);

            try
            {
                long owlClassCylinder = engine.GetClassByName(owlModel, "Cylinder");
                long owlInstanceCylinder = engine.CreateInstance(owlClassCylinder, "");
                engine.SetDatatypeProperty(owlInstanceCylinder, engine.GetPropertyByName(owlModel, "length"), length);
                engine.SetDatatypeProperty(owlInstanceCylinder, engine.GetPropertyByName(owlModel, "radius"), radius);
                engine.SetDatatypeProperty(owlInstanceCylinder, engine.GetPropertyByName(owlModel, "segmentationParts"), segmentationParts);

                AssignMaterialToInstance(owlModel, owlInstanceCylinder, r, g, b, t);

                var modelPath = Path.Combine(Path.GetTempPath(), $"temp_{Guid.NewGuid()}.bin");
                engine.SaveInstanceTree(owlInstanceCylinder, modelPath);
                base64Content = Convert.ToBase64String(System.IO.File.ReadAllBytes(modelPath));
                System.IO.File.Delete(modelPath);

                return new JsonResult(new { Geometry = base64Content });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Cylinder model.");
                throw;
            }
            finally
            {
                engine.CloseModel(owlModel);
            }
        }

        public IActionResult OnGetCreateSphere(
            double radius, long segmentationParts,
            double? r = null, double? g = null, double? b = null, double? t = null)
        {
            var base64Content = string.Empty;

            long owlModel = engine.CreateModel();
            long setting = flagbit0 + flagbit4;
            long mask = flagbit0 + flagbit4;
            engine.SetOverrideFileIO(owlModel, setting, mask);

            try
            {
                long owlClassSphere = engine.GetClassByName(owlModel, "Sphere");
                long owlInstanceSphere = engine.CreateInstance(owlClassSphere, "");
                engine.SetDatatypeProperty(owlInstanceSphere, engine.GetPropertyByName(owlModel, "radius"), radius);
                engine.SetDatatypeProperty(owlInstanceSphere, engine.GetPropertyByName(owlModel, "segmentationParts"), segmentationParts);

                AssignMaterialToInstance(owlModel, owlInstanceSphere, r, g, b, t);

                var modelPath = Path.Combine(Path.GetTempPath(), $"temp_{Guid.NewGuid()}.bin");
                engine.SaveInstanceTree(owlInstanceSphere, modelPath);
                base64Content = Convert.ToBase64String(System.IO.File.ReadAllBytes(modelPath));
                System.IO.File.Delete(modelPath);

                return new JsonResult(new { Geometry = base64Content });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Sphere model.");
                throw;
            }
            finally
            {
                engine.CloseModel(owlModel);
            }
        }

        public IActionResult OnGetCreateBox(
            double length, double width, double height,
            double? r = null, double? g = null, double? b = null, double? t = null)
        {
            var base64Content = string.Empty;

            long owlModel = engine.CreateModel();
            long setting = flagbit0 + flagbit4;
            long mask = flagbit0 + flagbit4;
            engine.SetOverrideFileIO(owlModel, setting, mask);

            try
            {
                long owlClassBox = engine.GetClassByName(owlModel, "Box");
                long owlInstanceBox = engine.CreateInstance(owlClassBox, "");
                engine.SetDatatypeProperty(owlInstanceBox, engine.GetPropertyByName(owlModel, "length"), length);
                engine.SetDatatypeProperty(owlInstanceBox, engine.GetPropertyByName(owlModel, "width"), width);
                engine.SetDatatypeProperty(owlInstanceBox, engine.GetPropertyByName(owlModel, "height"), height);

                AssignMaterialToInstance(owlModel, owlInstanceBox, r, g, b, t);

                var modelPath = Path.Combine(Path.GetTempPath(), $"temp_{Guid.NewGuid()}.bin");
                engine.SaveInstanceTree(owlInstanceBox, modelPath);
                base64Content = Convert.ToBase64String(System.IO.File.ReadAllBytes(modelPath));
                System.IO.File.Delete(modelPath);

                return new JsonResult(new { Geometry = base64Content });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Box model.");
                throw;
            }
            finally
            {
                engine.CloseModel(owlModel);
            }
        }

        public IActionResult OnPostCreateTranslation([FromForm] string base64Content, [FromForm] double _41, [FromForm] double _42, [FromForm] double _43)
        {
            var modelPath = Path.Combine(Path.GetTempPath(), $"temp_{Guid.NewGuid()}.bin");
            System.IO.File.WriteAllBytes(modelPath, Convert.FromBase64String(base64Content));

            long owlModel = engine.CreateModel();
            long setting = flagbit0 + flagbit4;
            long mask = flagbit0 + flagbit4;
            engine.SetOverrideFileIO(owlModel, setting, mask);

            try
            {
                long owlRootInstance = engine.ImportModel(owlModel, modelPath);

                //
                // Translate
                //
                long owlMatrixInstance = engine.CreateInstance(engine.GetClassByName(owlModel, "Matrix"));
                List<double> vecMatrix = new List<double>
                {
                    1.0,    // _11
                    0.0,    // _12
                    0.0,    // _13
                    0.0,    // _21
                    1.0,    // _22
                    0.0,    // _23
                    0.0,    // _31
                    0.0,    // _32
                    1.0,    // _33
                    _41,    // _41
                    _42,    // _42
                    _43,    // _43
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

                engine.SetObjectProperty(
                    owlTransformationInstance,
                    engine.GetPropertyByName(owlModel, "object"),
                    owlRootInstance);

                var translatedModelPath = Path.Combine(Path.GetTempPath(), $"translated_{Guid.NewGuid()}.bin");
                engine.SaveInstanceTree(owlTransformationInstance, translatedModelPath);
                var translatedBase64Content = Convert.ToBase64String(System.IO.File.ReadAllBytes(translatedModelPath));
                System.IO.File.Delete(translatedModelPath);

                return new JsonResult(new { Geometry = translatedBase64Content });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Translating model.");
                throw;
            }
            finally
            {
                engine.CloseModel(owlModel);
                System.IO.File.Delete(modelPath);
            }
        }

        public IActionResult OnPostCreateRotation([FromForm] string base64Content, [FromForm] double alpha, [FromForm] double beta, [FromForm] double gamma)
        {
            var modelPath = Path.Combine(Path.GetTempPath(), $"temp_{Guid.NewGuid()}.bin");
            System.IO.File.WriteAllBytes(modelPath, Convert.FromBase64String(base64Content));

            long owlModel = engine.CreateModel();
            long setting = flagbit0 + flagbit4;
            long mask = flagbit0 + flagbit4;
            engine.SetOverrideFileIO(owlModel, setting, mask);

            try
            {
                long owlRootInstance = engine.ImportModel(owlModel, modelPath);

                //
                // Translate
                //
                long owlMatrixInstance = engine.CreateInstance(engine.GetClassByName(owlModel, "Matrix"));
                double[] vecMatrix = MatrixRotateByEulerAngles(alpha, beta, gamma);
                engine.SetDatatypeProperty(
                    owlMatrixInstance,
                    engine.GetPropertyByName(owlModel, "coordinates"),
                    vecMatrix,
                    vecMatrix.Length);

                long owlTransformationInstance = engine.CreateInstance(engine.GetClassByName(owlModel, "Transformation"));
                engine.SetObjectProperty(
                    owlTransformationInstance,
                    engine.GetPropertyByName(owlModel, "matrix"),
                    owlMatrixInstance);

                engine.SetObjectProperty(
                    owlTransformationInstance,
                    engine.GetPropertyByName(owlModel, "object"),
                    owlRootInstance);

                var rotatedModelPath = Path.Combine(Path.GetTempPath(), $"rotated_{Guid.NewGuid()}.bin");
                engine.SaveInstanceTree(owlTransformationInstance, rotatedModelPath);
                var rotatedBase64Content = Convert.ToBase64String(System.IO.File.ReadAllBytes(rotatedModelPath));
                System.IO.File.Delete(rotatedModelPath);

                return new JsonResult(new { Geometry = rotatedBase64Content });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Translating model.");
                throw;
            }
            finally
            {
                engine.CloseModel(owlModel);
                System.IO.File.Delete(modelPath);
            }
        }

        public IActionResult OnPostCreateBooleanOperation([FromForm] string base64Content1, [FromForm] string base64Content2, long type)
        {
            var modelPath1 = Path.Combine(Path.GetTempPath(), $"temp_{Guid.NewGuid()}.bin");
            System.IO.File.WriteAllBytes(modelPath1, Convert.FromBase64String(base64Content1));

            var modelPath2 = Path.Combine(Path.GetTempPath(), $"temp_{Guid.NewGuid()}.bin");
            System.IO.File.WriteAllBytes(modelPath2, Convert.FromBase64String(base64Content2));

            long owlModel = engine.CreateModel();
            long setting = flagbit0 + flagbit4;
            long mask = flagbit0 + flagbit4;
            engine.SetOverrideFileIO(owlModel, setting, mask);

            try
            {
                long owlRootInstance1 = engine.ImportModel(owlModel, modelPath1);
                long owlRootInstance2 = engine.ImportModel(owlModel, modelPath2);

                //
                // Union
                //
                long owlInstanceBooleanOperation = engine.CreateInstance(engine.GetClassByName(owlModel, "BooleanOperation"));
                engine.SetObjectProperty(owlInstanceBooleanOperation, engine.GetPropertyByName(owlModel, "firstObject"), owlRootInstance1);
                engine.SetObjectProperty(owlInstanceBooleanOperation, engine.GetPropertyByName(owlModel, "secondObject"), owlRootInstance2);
                engine.SetDatatypeProperty(owlInstanceBooleanOperation, engine.GetPropertyByName(owlModel, "type"), type);
                engine.CalculateInstance(owlInstanceBooleanOperation, out long _, out long _);

                var unionModelPath = Path.Combine(Path.GetTempPath(), $"union_{Guid.NewGuid()}.bin");
                engine.SaveInstanceTree(owlInstanceBooleanOperation, unionModelPath);
                var unionBase64Content = Convert.ToBase64String(System.IO.File.ReadAllBytes(unionModelPath));
                System.IO.File.Delete(unionModelPath);

                return new JsonResult(new { Geometry = unionBase64Content });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Translating model.");
                throw;
            }
            finally
            {
                engine.CloseModel(owlModel);
                System.IO.File.Delete(modelPath1);
                System.IO.File.Delete(modelPath2);
            }
        }

        private static void AssignMaterialToInstance(long owlModel, long owlInstance, double? r, double? g, double? b, double? t)
        {
            if (r.HasValue && g.HasValue && b.HasValue)
            {
                long owlClassColorComponent = engine.GetClassByName(owlModel, "ColorComponent");
                long owlInstanceColorComponent = engine.CreateInstance(owlClassColorComponent, "");
                engine.SetDatatypeProperty(owlInstanceColorComponent, engine.GetPropertyByName(owlModel, "R"), r.Value);
                engine.SetDatatypeProperty(owlInstanceColorComponent, engine.GetPropertyByName(owlModel, "G"), g.Value);
                engine.SetDatatypeProperty(owlInstanceColorComponent, engine.GetPropertyByName(owlModel, "B"), b.Value);

                long owlClassColor = engine.GetClassByName(owlModel, "Color");
                long owlInstanceColor = engine.CreateInstance(owlClassColor, "");
                engine.SetObjectProperty(owlInstanceColor, engine.GetPropertyByName(owlModel, "ambient"), owlInstanceColorComponent);
                engine.SetDatatypeProperty(owlInstanceColor, engine.GetPropertyByName(owlModel, "transparency"), (t ?? 1.0));

                long owlClassMaterial = engine.GetClassByName(owlModel, "Material");
                long owlInstanceMaterial = engine.CreateInstance(owlClassMaterial, "");
                engine.SetObjectProperty(owlInstanceMaterial, engine.GetPropertyByName(owlModel, "color"), owlInstanceColor);

                engine.SetObjectProperty(owlInstance, engine.GetPropertyByName(owlModel, "material"), owlInstanceMaterial);
            }
        }

        /// <summary>
        /// Builds a rotation matrix from Euler angles (radians).
        /// https://en.wikipedia.org/wiki/Rotation_matrix
        /// Returns the coefficients in the order: _11, _12, _13, _21, _22, _23, _31, _32, _33, _41, _42, _43.
        /// </summary>
        private static double[] MatrixRotateByEulerAngles(double alpha, double beta, double gamma)
        {
            double cosAlpha = Math.Cos(alpha), sinAlpha = Math.Sin(alpha);
            double cosBeta = Math.Cos(beta), sinBeta = Math.Sin(beta);
            double cosGamma = Math.Cos(gamma), sinGamma = Math.Sin(gamma);

            return
                [
                    // _11, _12, _13
                    cosBeta * cosGamma,
                    cosBeta * sinGamma,
                    -sinBeta,

                    // _21, _22, _23
                    sinAlpha * sinBeta * cosGamma - cosAlpha * sinGamma,
                    sinAlpha * sinBeta * sinGamma + cosAlpha * cosGamma,
                    sinAlpha * cosBeta,

                    // _31, _32, _33
                    cosAlpha * sinBeta * cosGamma + sinAlpha * sinGamma,
                    cosAlpha * sinBeta * sinGamma - sinAlpha * cosGamma,
                    cosAlpha * cosBeta,

                    // _41, _42, _43
                    0.0,
                    0.0,
                    0.0,
                ];
        }
    }
}
