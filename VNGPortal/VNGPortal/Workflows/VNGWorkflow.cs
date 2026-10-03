using RDF;
using VNGPortal.IFC2RDF;
using VNGPortal.Services;

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
            await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 0, "Workflow started...", false);

            var currentStep = 0;
            var stepsCount = _workflow.Steps.Count;

            var isLinuxPlatform = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux);
            var fileStorage = isLinuxPlatform ? "FileStorageLinux" : "FileStorage";

            var modelsDir = _configuration[$"{fileStorage}:ModelsDir"]!;
            var idsDir = _configuration[$"{fileStorage}:IDSDir"]!;

            var modelDir = Path.Combine(modelsDir, taskDescriptor.TaskId);
            var modelPath = Path.Combine(modelDir, taskDescriptor.Model);

            var datasetName = "test1"; //#todo modelId or taskId instead of hardcoded "test1"

            var sparqlServer = new SPARQL.Server(_logger);

            //
            // Execute workflow steps
            //

            WorkflowStorage workflowStorage = new WorkflowStorage(_configuration, _logger);
            var sparqlQueries = workflowStorage.LoadSPARQLQueries();
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
                                    args: $"{jvmArgs}-jar \"{jarPath}\" --baseURI http://vng.nl/geometry/ --dir \"{modelDir}\""
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
                                sparqlServer.CreateDataset(datasetName);
                                if (sparqlServer.AddData(datasetName, new List<string>
                                    {
                                        Path.Combine(modelDir, Path.GetFileNameWithoutExtension(modelPath) + ".ttl"),
                                        Path.Combine(modelDir, Path.GetFileNameWithoutExtension(modelPath) + "_geometry.trig")
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

                                if (string.IsNullOrEmpty(query) && step.Parameters.ContainsKey("queryRef"))
                                {
                                    string queryRef = step.Parameters["queryRef"];
                                    if (!string.IsNullOrEmpty(queryRef) &&
                                        sparqlQueries.TryGetValue(queryRef, out var sparqlQuery))
                                    {
                                        query = sparqlQuery.Query;
                                    }
                                }

                                if (string.IsNullOrEmpty(query))
                                {
                                    throw new Exception($"SPARQL query is empty for workflow step: '{step.Name}'");
                                }

                                await _signalRStatus.SendQueryUpdate(taskDescriptor.GroupName, "SPARQL Query", query, "", false);
                                if (await sparqlServer.ExecuteInsertAsync(datasetName, query))
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

                        case "SHACL":
                            {
                                string shape = string.Empty;
                                if (step.Parameters.ContainsKey("shape"))
                                {
                                    shape = step.Parameters["shape"];
                                }

                                SHACLShape? shaclShape = null;
                                if (string.IsNullOrEmpty(shape) && step.Parameters.ContainsKey("shapeRef"))
                                {
                                    var shapeRef = step.Parameters["shapeRef"];
                                    if (!string.IsNullOrEmpty(shapeRef) &&
                                        shaclShapes.TryGetValue(shapeRef, out shaclShape))
                                    {
                                        shape = shaclShape.Shape;
                                    }
                                }

                                if (string.IsNullOrEmpty(shape))
                                {
                                    throw new Exception($"SHACL shape is empty for workflow step: '{step.Name}'");
                                }

                                var result = await sparqlServer.ExecuteSHACLAsync(datasetName, "https://vng.nl/geometries/", shape);
                                if (!string.IsNullOrEmpty(result))
                                {
                                    bool hasViolation = result.IndexOf("sh:Violation") != -1;
                                    await _signalRStatus.SendQueryUpdate(taskDescriptor.GroupName, "SHACL Validation Report", "", result, hasViolation);
                                    if (hasViolation)
                                    {
                                        if (shaclShape != null)
                                        {
                                            var shaclViolationViews = shaclShape.Views.FindAll(v => v.Type == "violation").ToList();
                                            if ((shaclViolationViews != null) && (shaclViolationViews.Count > 0))
                                            {
                                                // Use the first view for now, but we could potentially handle multiple views in the future
                                                var shaclViolationView = shaclViolationViews?.First();
                                                if (shaclViolationView != null)
                                                {
                                                    long owlModel = engine.CreateModel();
                                                    try
                                                    {
                                                        foreach (var query in shaclViolationView.Queries)
                                                        {
                                                            var queryResult = await sparqlServer.ExecuteQueryAsync(datasetName, query);
                                                            await sparqlServer.RetrieveGeometry(queryResult, "base64Data", owlModel); //#todo: make "base64Data" configurable in the future
                                                        }

                                                        var shaclErrorsModel = $"shacl_errors_{Guid.NewGuid().ToString()}.bin";
                                                        var shaclErrorsModelPath = Path.Combine(modelDir, shaclErrorsModel);
                                                        engine.SaveModel(owlModel, shaclErrorsModelPath);

                                                        await _signalRStatus.Send3DViewUpdate(
                                                            taskDescriptor.GroupName,
                                                            "SHACL Violations View",
                                                            taskDescriptor.TaskId,
                                                            shaclErrorsModel,
                                                            true);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        _logger.LogError(ex, "Error executing SHACL error view queries for workflow step: '{StepName}'", step.Name);
                                                        await _signalRStatus.SendProgressUpdate(
                                                            taskDescriptor.GroupName,
                                                            0,
                                                            $"Error executing SHACL error view queries for workflow step: '{step.Name}'.",
                                                            true);
                                                    }
                                                    finally
                                                    {
                                                        engine.CloseModel(owlModel);
                                                    }
                                                }
                                            }
                                        }
                                    }
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
                                    await _signalRStatus.SendQueryUpdate(taskDescriptor.GroupName, "IDS Validation Report", "", output, output.IndexOf("ERROR") != -1);
                                    await _signalRStatus.SendProgressUpdate(
                                        taskDescriptor.GroupName,
                                        (float)currentStep / stepsCount,
                                        $"(Step {currentStep}/{stepsCount}) Workflow step: '{step.Name}' executed successfully.",
                                        false);
                                }
                                else
                                {
                                    await _signalRStatus.SendQueryUpdate(taskDescriptor.GroupName, "IDS Validation Report", "", output, true);
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
                100,
                "Workflow executed successfully.",
                false);

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
        #endregion

    }
}
