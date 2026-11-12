using AngleSharp.Dom;
using Lucene.Net.Search;
using Microsoft.Extensions.Logging;
using RDF;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using VNGPortal.IFC2RDF;
using VNGPortal.Pages;
using VNGPortal.Services;
using static System.Net.Mime.MediaTypeNames;

namespace VNGPortal.Workflows
{
    public class VNGWorkflow : _Workflow
    {
        #region Methods
        public VNGWorkflow(IConfiguration configuration, ILogger logger, ISignalRStatusService signalRStatus, string groupName, Workflow workflow)
            : base(configuration, logger, signalRStatus, groupName, workflow)
        {
        }

        public override async Task<bool> ExecuteAsync(TaskDescriptor taskDescriptor)
        {
            TaskDescriptor = taskDescriptor;

            await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 0, $"Workflow for model '{taskDescriptor.Model}' started...", false);

            var currentStep = 0;
            var stepsCount = _workflow.Steps.Count;

            var isLinuxPlatform = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux);
            var fileStorage = isLinuxPlatform ? "FileStorageLinux" : "FileStorage";

            var modelsDir = _configuration[$"{fileStorage}:ModelsDir"]!;
            var idsDir = _configuration[$"{fileStorage}:IDSDir"]!;
            var sparqlDir = _configuration[$"{fileStorage}:SPARQLDir"]!;
            var shaclDir = _configuration[$"{fileStorage}:SHACLDir"]!;

            ModelDir = Path.Combine(modelsDir, taskDescriptor.TaskId);
            var modelPath = Path.Combine(ModelDir, taskDescriptor.Model);

            SPARQLServer = new SPARQL.Server(_logger);
            DatasetName = "test1"; //#todo modelId or taskId instead of hardcoded "test1"

            //
            // Execute workflow steps
            //

            WorkflowStorage workflowStorage = new WorkflowStorage(_configuration, _logger);
            SPARQLQueries = workflowStorage.LoadSPARQLQueries();
            var shaclShapes = workflowStorage.LoadSHACLShapes();

            for (int i = 0; _workflow.Steps != null && i < _workflow.Steps.Count; i++)
            {
                var step = _workflow.Steps[i];
                currentStep++;

                string message = $"(Step {currentStep}/{stepsCount}) Executing workflow step: '{step.Name}'...";
                if (!string.IsNullOrEmpty(step.Description))
                {
                    message += $"<br />ℹ {step.Description}";
                }
                await _signalRStatus.SendProgressUpdate(
                    taskDescriptor.GroupName,
                    (float)currentStep / stepsCount,
                    message,
                    false);

                try
                {
                    switch (step.Type)
                    {
                        case "IFC2RDF":
                            {
                                var jarPath = Path.Combine(Directory.GetCurrentDirectory(), "IFC2RDF", "ifc2rdf-1.4.7-shaded.jar");
                                var javaPath = _configuration[$"{(isLinuxPlatform ? "ToolsLinux" : "Tools")}:JavaPath"]!;
                                var jvmArgs = isLinuxPlatform
                                    ? "-Xms8g -Xmx8g "
                                    : string.Empty;
                                var (output, error, exitCode) = await ExecuteProcess(
                                    exePath: javaPath,
                                    args: $"{jvmArgs}-jar \"{jarPath}\" --baseURI http://vng.nl/geometry/ --dir \"{ModelDir}\""
                                );
                                if (exitCode == 0)
                                {
                                    _logger.LogInformation("Workflow step: '{StepName}' executed successfully.", step.Name);
                                    await _signalRStatus.SendProgressUpdate(
                                        taskDescriptor.GroupName,
                                        (float)currentStep / stepsCount,
                                        $"(Step {currentStep}/{stepsCount}) Workflow step: '{step.Name}' executed successfully.",
                                        false);
                                }
                                else
                                {
                                    throw new Exception($"Workflow step: '{step.Name}' failed for model {taskDescriptor.TaskId} with exit code {exitCode}");
                                }
                            }
                            break;

                        case "IFCGEOMETRY2RDF":
                            {
                                var geometry2RDF = new Geometry2RDF(_logger);
                                await geometry2RDF.Run(modelPath);

                                _logger.LogInformation("Workflow step: '{StepName}' executed successfully.", step.Name);
                                await _signalRStatus.SendProgressUpdate(
                                    taskDescriptor.GroupName,
                                    (float)currentStep / stepsCount,
                                    $"(Step {currentStep}/{stepsCount}) Workflow step: '{step.Name}' executed successfully.",
                                    false);
                            }
                            break;

                        case "CREATE-SPARQL-DATASET":
                            {
                                // Add data to the Jena-Fuseki database
                                SPARQLServer.CreateDataset(DatasetName);
                                if (SPARQLServer.AddData(DatasetName, new List<string>
                                    {
                                        Path.Combine(ModelDir, Path.GetFileNameWithoutExtension(modelPath) + ".ttl"),
                                        Path.Combine(ModelDir, Path.GetFileNameWithoutExtension(modelPath) + "_geometry.trig")
                                    }))
                                {
                                    _logger.LogInformation("Workflow step: '{StepName}' executed successfully.", step.Name);
                                    await _signalRStatus.SendProgressUpdate(
                                        taskDescriptor.GroupName,
                                        (float)currentStep / stepsCount,
                                        $"(Step {currentStep}/{stepsCount}) Workflow step: '{step.Name}' executed successfully.",
                                        false);
                                }
                                else
                                {
                                    throw new Exception($"Workflow step: '{step.Name}' failed for model {taskDescriptor.TaskId}");
                                }
                            }
                            break;

                        case "SPARQL":
                            {
                                string query = string.Empty;
                                if (step.Parameters.ContainsKey("query"))
                                {
                                    query = step.Parameters["query"];
                                }

                                if (string.IsNullOrEmpty(query))
                                {
                                    if (step.Parameters.ContainsKey("queryRef"))
                                    {
                                        string queryRef = step.Parameters["queryRef"];
                                        if (!string.IsNullOrEmpty(queryRef) &&
                                            SPARQLQueries.TryGetValue(queryRef, out var sparqlQuery))
                                        {
                                            if (!string.IsNullOrEmpty(sparqlQuery.QueryUri))
                                            {
                                                // #todo: use http source instead of local file
                                                var queryFilePath = Path.Combine(sparqlDir, sparqlQuery.QueryUri);
                                                _logger.LogInformation("Loading SPARQL query from file: '{QueryFilePath}'", queryFilePath);
                                                query = await File.ReadAllTextAsync(queryFilePath);
                                                _logger.LogInformation("SPARQL query loaded: '{Query}'", query);
                                            }
                                            else
                                            {
                                                query = sparqlQuery.Query;
                                            }
                                        }
                                    }
                                }

                                if (string.IsNullOrEmpty(query))
                                {
                                    throw new Exception($"SPARQL query is empty for workflow step: '{step.Name}'");
                                }

                                await _signalRStatus.SendQueryUpdate(taskDescriptor.GroupName, "SPARQL Query", query, "", false);
                                if (await SPARQLServer.ExecuteInsertAsync(DatasetName, query))
                                {
                                    _logger.LogInformation("Workflow step {StepName} executed successfully.", step.Name);
                                    await _signalRStatus.SendProgressUpdate(
                                        taskDescriptor.GroupName,
                                        (float)currentStep / stepsCount,
                                        $"(Step {currentStep}/{stepsCount}) Workflow step: '{step.Name}' executed successfully.",
                                        false);
                                }
                                else
                                {
                                    throw new Exception($"Workflow step: '{step.Name}' failed for model {taskDescriptor.TaskId}");
                                }
                            }
                            break;

                        case "3DVIEW":
                            {
                                if (!step.Parameters.ContainsKey("queries"))
                                {
                                    throw new Exception($"3DVIEW workflow step: '{step.Name}' is missing 'queries' parameter.");
                                }

                                var geometryQueries = step.Parameters.GetGeometryQueries();
                                if (geometryQueries == null || geometryQueries.Count == 0)
                                {
                                    throw new Exception($"3DVIEW workflow step: '{step.Name}' has no valid geometry queries.");
                                }

                                // Geo-reference
                                double dX = 0.0;
                                double dY = 0.0;
                                GetGeoReference(modelPath, out dX, out dY);

                                long owlModel = Engine.CreateModel();
                                Engine.SetOverrideFileIO(owlModel, Engine.flagbit0 + Engine.flagbit4, Engine.flagbit0 + Engine.flagbit4);
                                try
                                {
                                    foreach (var geometryQuery in geometryQueries)
                                    {
                                        await _signalRStatus.SendQueryUpdate(taskDescriptor.GroupName, "SPARQL Query", geometryQuery.Query, "", false);

                                        var queryResult = await SPARQLServer.ExecuteQueryAsync(DatasetName, geometryQuery.Query);
                                        await SPARQLServer.RetrieveGeometry(queryResult, geometryQuery.GeometryVariable, owlModel);
                                    }

                                    var viewModel = $"3dview_{Guid.NewGuid().ToString()}.bin";
                                    var viewModelPath = Path.Combine(ModelDir, viewModel);
                                    Engine.SaveModel(owlModel, viewModelPath);

                                    await _signalRStatus.Send3DViewUpdate(
                                        taskDescriptor.GroupName,
                                        new _3DViewUpdate
                                        {
                                            Title = "3D View",
                                            WorkflowId = taskDescriptor.TaskId,
                                            Model = viewModel,
                                            ZoomToX = dX,
                                            ZoomToY = dY,
                                            View = "top",
                                            Error = false
                                        });
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Error executing 3DVIEW workflow step: '{StepName}'", step.Name);
                                    await _signalRStatus.SendProgressUpdate(
                                        taskDescriptor.GroupName,
                                        0,
                                        $"Error executing 3DVIEW workflow step: '{step.Name}'.",
                                        true);
                                    throw;
                                }
                            }
                            break;

                        case "TEXTVIEW":
                            {
                                if (!step.Parameters.ContainsKey("queries"))
                                {
                                    throw new Exception($"TEXTVIEW workflow step: '{step.Name}' is missing 'queries' parameter.");
                                }

                                var variableQueries = step.Parameters.GetVariableQueries();
                                if (variableQueries == null || variableQueries.Count == 0)
                                {
                                    throw new Exception($"TEXTVIEW workflow step: '{step.Name}' has no valid geometry queries.");
                                }

                                long owlModel = Engine.CreateModel();
                                try
                                {
                                    foreach (var variableQuery in variableQueries)
                                    {
                                        await _signalRStatus.SendQueryUpdate(taskDescriptor.GroupName, "SPARQL Query", variableQuery.Query, "", false);

                                        var queryResult = await SPARQLServer.ExecuteQueryAsync(DatasetName, variableQuery.Query);
                                        var text = variableQuery.ValueVariableLabel + ": ";
                                        text += await SPARQLServer.RetrieveValue(queryResult, variableQuery.ValueVariable, owlModel);
                                        text += "\n";

                                        await _signalRStatus.SendProgressUpdate(
                                            taskDescriptor.GroupName,
                                            (float)currentStep / stepsCount,
                                            text,
                                            false);
                                    }

                                    _logger.LogInformation("Workflow step {StepName} executed successfully.", step.Name);
                                    await _signalRStatus.SendProgressUpdate(
                                        taskDescriptor.GroupName,
                                        (float)currentStep / stepsCount,
                                        $"(Step {currentStep}/{stepsCount}) Workflow step: '{step.Name}' executed successfully.",
                                        false);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Error executing TEXTVIEW workflow step: '{StepName}'", step.Name);
                                    await _signalRStatus.SendProgressUpdate(
                                        taskDescriptor.GroupName,
                                        0,
                                        $"Error executing TEXTVIEW workflow step: '{step.Name}'.",
                                        true);
                                    throw;
                                }
                            }
                            break;

                        case "SHACL":
                            {
                                string shape = string.Empty;
                                if (step.Parameters.ContainsKey("shape"))
                                {
                                    shape = step.Parameters["shape"];
                                }

                                SHACLShape? shaclShape = null;
                                if (string.IsNullOrEmpty(shape))
                                {
                                    if (step.Parameters.ContainsKey("shapeRef"))
                                    {
                                        var shapeRef = step.Parameters["shapeRef"];
                                        if (!string.IsNullOrEmpty(shapeRef) &&
                                            shaclShapes.TryGetValue(shapeRef, out shaclShape))
                                        {
                                            if (!string.IsNullOrEmpty(shaclShape.ShapeUri))
                                            {
                                                // #todo: use http source instead of local file
                                                var shapeFilePath = Path.Combine(shaclDir, shaclShape.ShapeUri);
                                                _logger.LogInformation("Loading SHACL shape from file: '{ShapeFilePath}'", shapeFilePath);
                                                shape = await File.ReadAllTextAsync(shapeFilePath);
                                                _logger.LogInformation("SHACL shape loaded: '{Shape}'", shape);
                                            }
                                            else
                                            {
                                                shape = shaclShape.Shape;
                                            }
                                        }
                                    }
                                }

                                if (string.IsNullOrEmpty(shape))
                                {
                                    throw new Exception($"SHACL shape is empty for workflow step: '{step.Name}'");
                                }

                                var result = await SPARQLServer.ExecuteSHACLAsync(DatasetName, "https://vng.nl/geometries/", shape);
                                if (!string.IsNullOrEmpty(result))
                                {
                                    bool hasViolations = result.IndexOf("sh:Violation") != -1;
                                    await _signalRStatus.SendQueryUpdate(taskDescriptor.GroupName, $"{step.Name} - SHACL Validation Report", "", result, hasViolations);
                                    await CreateSHACLViews(
                                        shaclShape,
                                        result);
                                }

                                _logger.LogInformation("Workflow step: {StepName} executed successfully.", step.Name);
                                await _signalRStatus.SendProgressUpdate(
                                    taskDescriptor.GroupName,
                                    (float)currentStep / stepsCount,
                                    $"(Step {currentStep}/{stepsCount}) Workflow step: '{step.Name}' executed successfully.",
                                    false);
                            }
                            break;

                        case "IDS":
                            {
                                string idsFile = step.Parameters["ids_file"];
                                var idsPath = Path.Combine(idsDir, idsFile);
                                var idsFileContent = await File.ReadAllTextAsync(idsPath);
                                await _signalRStatus.SendQueryUpdate(taskDescriptor.GroupName, $"IDS File: '{idsFile}'", FormatXML(idsFileContent), "", false);

                                var (output, error, exitCode) = await ExecuteProcess(
                                    exePath: isLinuxPlatform ? "./IDSValidator" : "./IDSValidator.exe",
                                    args: $"\"{modelPath}\" \"{idsPath}\""
                                );
                                if (exitCode == 0)
                                {
                                    _logger.LogInformation("Workflow step: '{StepName}' executed successfully.", step.Name);
                                    await _signalRStatus.SendQueryUpdate(taskDescriptor.GroupName, $"{step.Name} - IDS Validation Report", "", output, output.IndexOf("ERROR") != -1);
                                    await _signalRStatus.SendProgressUpdate(
                                        taskDescriptor.GroupName,
                                        (float)currentStep / stepsCount,
                                        $"(Step {currentStep}/{stepsCount}) Workflow step: '{step.Name}' executed successfully.",
                                        false);
                                }
                                else
                                {
                                    await _signalRStatus.SendQueryUpdate(taskDescriptor.GroupName, $"{step.Name} - IDS Validation Report", "", output, true);
                                }
                            }
                            break;

                        default:
                            _logger.LogError("Unknown Step Type: {StepType}.", step.Type);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing workflow step: '{StepName}'", step.Name);
                    await _signalRStatus.SendProgressUpdate(
                        taskDescriptor.GroupName,
                        0,
                        $"Error executing workflow step: '{step.Name}'.",
                        true);
                    throw;
                }
            } // for (int i = 0; _workflow.Steps != null && i < _workflow.Steps.Count; i++)

            await _signalRStatus.SendProgressUpdate(
                taskDescriptor.GroupName,
                1,
                $"Workflow for model '{taskDescriptor.Model}' completed successfully.",
                false);

            return true;
        }

        private async Task CreateSHACLViews(
            SHACLShape? shaclShape,
            string shaclResult)
        {
            if (TaskDescriptor == null)
            {
                throw new InvalidOperationException("Task Descriptor is not initialized.");
            }

            if (SPARQLServer == null)
            {
                throw new InvalidOperationException("SPARQL Server is not initialized.");
            }

            if (string.IsNullOrEmpty(DatasetName))
            {
                throw new InvalidOperationException("Dataset name is not initialized.");
            }

            if (string.IsNullOrEmpty(ModelDir))
            {
                throw new InvalidOperationException("Mode Directory is not initialized.");
            }

            if (shaclShape == null)
            {
                return;
            }

            var modelPath = Path.Combine(ModelDir, TaskDescriptor.Model);

            double dX = 0.0;
            double dY = 0.0;
            GetGeoReference(modelPath, out dX, out dY);

            //
            // Violation Views
            //
            bool hasViolations = shaclResult.IndexOf("sh:Violation") != -1;
            if (hasViolations)
            {
                var shaclViolationViews = shaclShape.Views.FindAll(v => v.Type == "violation").ToList();
                if ((shaclViolationViews != null) && (shaclViolationViews.Count > 0))
                {
                    foreach (var shaclViolationView in shaclViolationViews)
                    {
                        if (shaclViolationView == null)
                        {
                            continue;
                        }

                        long owlModel = Engine.CreateModel();
                        try
                        {
                            foreach (var geometryQuery in shaclViolationView.Queries)
                            {
                                string query = geometryQuery.Query;
                                if (string.IsNullOrEmpty(query))
                                {
                                    throw new Exception($"SPARQL query is empty for SHACL Validation - SPARQL Query: '{shaclShape.Id} - {geometryQuery.Id}'");
                                }
                                _logger.LogInformation("SPARQL Query: '{ShapeId} - {QueryId}' - Query: '{Query}'", shaclShape.Id, geometryQuery.Id, query);

                                string templateArgRegExpr = string.Empty;
                                if (geometryQuery.Parameters.ContainsKey("templateArgRegExpr"))
                                {
                                    templateArgRegExpr = geometryQuery.Parameters["templateArgRegExpr"];
                                }

                                string templateArgRegExprMatch = string.Empty;
                                if (geometryQuery.Parameters.ContainsKey("templateArgRegExprMatch"))
                                {
                                    templateArgRegExprMatch = geometryQuery.Parameters["templateArgRegExprMatch"];
                                }

                                //
                                // Template Arguments
                                //
                                string templateArgName = string.Empty;
                                if (geometryQuery.Parameters.ContainsKey("templateArgName"))
                                {
                                    templateArgName = geometryQuery.Parameters["templateArgName"];
                                }

                                if (!string.IsNullOrEmpty(templateArgRegExpr) &&
                                    !string.IsNullOrEmpty(templateArgRegExprMatch) &&
                                    !string.IsNullOrEmpty(templateArgName))
                                {
                                    var templateArgValue = string.Join(" ",
                                    Regex.Matches(shaclResult, templateArgRegExpr)
                                        .Select(m => $"'{m.Groups[templateArgRegExprMatch].Value}'"));
                                    query = query.Replace(templateArgName, templateArgValue);
                                    _logger.LogInformation("SHACL Validation - SPARQL Query: '{ShapeId} - {QueryId}' - Template Argument: '{TemplateArgName}' = '{TemplateArgValue}'", shaclShape.Id, geometryQuery.Id, templateArgName, templateArgValue);
                                    _logger.LogInformation("SPARQL Query: '{ShapeId} - {QueryId}' - Query: '{Query}'", shaclShape.Id, geometryQuery.Id, query);
                                }

                                var queryResult = await SPARQLServer.ExecuteQueryAsync(DatasetName, query);
                                await SPARQLServer.RetrieveGeometry(queryResult, geometryQuery.GeometryVariable, owlModel);
                            }

                            var shaclErrorsModel = $"shacl_errors_{Guid.NewGuid().ToString()}.bin";
                            var shaclErrorsModelPath = Path.Combine(ModelDir, shaclErrorsModel);
                            Engine.SaveModel(owlModel, shaclErrorsModelPath);

                            await _signalRStatus.Send3DViewUpdate(
                                TaskDescriptor.GroupName,
                                new _3DViewUpdate
                                {
                                    Title = "SHACL Violations View",
                                    WorkflowId = TaskDescriptor.TaskId,
                                    Model = shaclErrorsModel,
                                    ZoomToX = dX,
                                    ZoomToY = dY,
                                    View = "top",
                                    Error = false
                                });
                        }
                        finally
                        {
                            Engine.CloseModel(owlModel);
                        }
                    }
                } // foreach (var shaclViolationView in shaclViolationViews)
            } // if (hasViolations)

            //
            // Success Views
            //
            var shaclSuccessViews = shaclShape.Views.FindAll(v => v.Type == "success").ToList();
            if ((shaclSuccessViews != null) && (shaclSuccessViews.Count > 0))
            {
                throw new NotImplementedException("Success Views");
            }
        }

        public bool GetGeoReference(string filePath, out double dX, out double dY)
        {
            dX = 0.0;
            dY = 0.0;

            // Add null terminator to byte arrays for P/Invoke marshaling
            var filePathBytes = Encoding.UTF8.GetBytes(filePath + "\0");
            var schemaBytes = Encoding.UTF8.GetBytes("\0");
            var sdaiModel = IFCEngine.sdaiOpenModelBN(0, filePathBytes, schemaBytes);
            if (sdaiModel == 0)
            {
                _logger.LogError("Failed to open IFC model: {FilePath}", filePath);
                return false;
            }

            double dEastings = 0.0;
            double dNorthings = 0.0;

            long owlModel = 0;
            IFCEngine.owlGetModel(sdaiModel, out owlModel);

            long sdaiAggr = IFCEngine.sdaiGetEntityExtentBN(sdaiModel, "IFCMAPCONVERSION");
            if (sdaiAggr == 0)
            {
                _logger.LogWarning("No IFCMAPCONVERSION entity found in the model.");
                return false;
            }

            IFCEngine.engiGetAggrElement(sdaiAggr, 0, IFCEngine.sdaiINSTANCE, out long sdaiIfcMapConversionInstance);
            if (sdaiIfcMapConversionInstance == 0)
            {
                _logger.LogWarning("No instance of IFCMAPCONVERSION found in the model.");
                return false;
            }

            long sdaiIfcMapConversionEntity = IFCEngine.sdaiGetInstanceType(sdaiIfcMapConversionInstance);
            if (sdaiIfcMapConversionEntity == 0)
            {
                _logger.LogWarning("Failed to get the entity type of the IFCMAPCONVERSION instance.");
                return false;
            }

            long iIndex = 0;
            long sdaiAttr = IFCEngine.engiGetEntityAttributeByIndex(
                sdaiIfcMapConversionEntity,
                iIndex++,
                false,
                true);

            while (sdaiAttr != 0)
            {
                IFCEngine.engiGetEntityArgumentName(sdaiIfcMapConversionEntity,
                    iIndex,
                    IFCEngine.sdaiSTRING,
                    out IntPtr attributeName);
                if (attributeName != IntPtr.Zero)
                {
                    string? strAttributeName = Marshal.PtrToStringAnsi(attributeName);
                    if (strAttributeName == "Eastings")
                    {
                        if (IFCEngine.sdaiGetAttrBN(sdaiIfcMapConversionInstance, strAttributeName, IFCEngine.sdaiREAL, out dEastings) == 0)
                        {
                            _logger.LogWarning("Failed to get Eastings attribute.");
                            return false;
                        }

                        dX = dEastings;
                    }
                    else if (strAttributeName == "Northings")
                    {
                        if (IFCEngine.sdaiGetAttrBN(sdaiIfcMapConversionInstance, strAttributeName, IFCEngine.sdaiREAL, out dNorthings) == 0)
                        {
                            _logger.LogWarning("Failed to get Northings attribute.");
                            return false;
                        }

                        dY = dNorthings;
                    }
                }

                sdaiAttr = IFCEngine.engiGetEntityAttributeByIndex(
                    sdaiIfcMapConversionEntity,
                    iIndex++,
                    false,
                    true);
            } // while (sdaiAttr != 0)

            IFCEngine.sdaiCloseModel(sdaiModel);

            return true;
        }

        private string FormatXML(string xml)
        {
            try
            {
                // Format XML with indentation
                var xmlDoc = new System.Xml.XmlDocument();
                xmlDoc.LoadXml(xml);

                var settings = new System.Xml.XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "  ",
                    NewLineChars = "\n",
                    NewLineHandling = System.Xml.NewLineHandling.Replace,
                    OmitXmlDeclaration = false
                };

                using var stringWriter = new System.IO.StringWriter();
                using var xmlWriter = System.Xml.XmlWriter.Create(stringWriter, settings);
                xmlDoc.Save(xmlWriter);

                return stringWriter.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error formatting XML.");
                return xml; // Return the original XML if formatting fails
            }
        }
        #endregion

        #region Properties
        public override string Name => "VNG";
        public override string Description => "VNG Workflow";

        private TaskDescriptor? TaskDescriptor { get; set; } = null;
        private SPARQL.Server? SPARQLServer { get; set; } = null;
        private string DatasetName { get; set; } = string.Empty;
        private IDictionary<string, SPARQLQuery> SPARQLQueries { get; set; } = new Dictionary<string, SPARQLQuery>();
        private string ModelDir { get; set; } = string.Empty;
        #endregion

    }
}
