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
            var stepsCount = 3/*Pre-processing*/ + _workflow.Steps.Count;

            var isLinuxPlatform = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux);
            var fileStorage = isLinuxPlatform ? "FileStorageLinux" : "FileStorage";

            var modelsDir = _configuration[$"{fileStorage}:ModelsDir"]!;
            var idsDir = _configuration[$"{fileStorage}:IDSDir"]!;

            var modelDir = Path.Combine(modelsDir, taskDescriptor.TaskId);
            var modelPath = Path.Combine(modelDir, taskDescriptor.Model);

            //
            // IFC to RDF conversion
            //
            currentStep++;

            await _signalRStatus.SendProgressUpdate(
                                taskDescriptor.GroupName,
                                (float)currentStep / stepsCount,
                                $"(Step {currentStep}/{stepsCount}) Executing workflow step: 'IFC to RDF conversion'...",
                                false);

            var jarPath = Path.Combine(Directory.GetCurrentDirectory(), "IFC2RDF", "ifc2rdf-1.4.7-shaded.jar");
            var javaPath = _configuration[$"{(isLinuxPlatform ? "ToolsLinux" : "Tools")}:JavaPath"]!;
            var jvmArgs = isLinuxPlatform
                ? "-Xms8g -Xmx8g "
                : string.Empty;
            var (output, error, exitCode) = await ExecuteProcess(
                exePath: javaPath,
                args: $"{jvmArgs}-jar \"{jarPath}\" --baseURI http://vng.nl/geometry/ --dir \"{modelDir}\""
            );
            if (exitCode != 0)
            {
                _logger.LogError("Workflow step: 'IFC to RDF conversion' failed for model {ModelId} with exit code {ExitCode}", taskDescriptor.TaskId, exitCode);
                await _signalRStatus.SendProgressUpdate(
                    taskDescriptor.GroupName,
                    0,
                    "Workflow step: 'IFC to RDF conversion' failed.",
                    true);
                throw new Exception($"Workflow step: 'IFC to RDF conversion' failed for model {taskDescriptor.TaskId} with exit code {exitCode}");
            }
            _logger.LogInformation("Workflow step: 'IFC to RDF conversion' completed successfully.");
            await _signalRStatus.SendProgressUpdate(
                taskDescriptor.GroupName,
                (float)currentStep / stepsCount,
                $"(Step {currentStep}/{stepsCount}) Workflow step: 'IFC to RDF conversion' completed successfully.",
                false);

            //
            // Geometry to RDF conversion
            //
            currentStep++;

            try
            {
                await _signalRStatus.SendProgressUpdate(
                                taskDescriptor.GroupName,
                                (float)currentStep / stepsCount,
                                $"(Step {currentStep}/{stepsCount}) Executing workflow step: 'Geometry to RDF conversion'...",
                                false);

                var geometry2RDF = new Geometry2RDF(_logger);
                await geometry2RDF.Run(modelPath);

                _logger.LogInformation("Workflow step: 'Geometry to RDF conversion' completed successfully.");
                await _signalRStatus.SendProgressUpdate(
                    taskDescriptor.GroupName,
                    (float)currentStep / stepsCount,
                    $"(Step {currentStep}/{stepsCount}) Workflow step: 'Geometry to RDF conversion' completed successfully.",
                    false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Workflow step: 'Geometry to RDF conversion' failed for model {ModelId}", taskDescriptor.TaskId);
                await _signalRStatus.SendProgressUpdate(
                    taskDescriptor.GroupName,
                    (float)currentStep / stepsCount,
                    $"(Step {currentStep}/{stepsCount}) Workflow step: 'Geometry to RDF conversion' failed.",
                    true);
                throw;
            }

            //
            // Create Jena-Fuseki database for the model
            //
            currentStep++;

            var sparqlServer = new SPARQL.Server(_logger);
            sparqlServer.CreateDataset("test1");

            // Add data to the Jena-Fuseki database
            sparqlServer.AddData("test1", new List<string>
            {
                Path.Combine(modelDir, Path.GetFileNameWithoutExtension(modelPath) + ".ttl"),
                Path.Combine(modelDir, Path.GetFileNameWithoutExtension(modelPath) + "_geometry.trig")
            });
            _logger.LogInformation("IFC file processed and data added to SPARQL dataset.");
            await _signalRStatus.SendProgressUpdate(
                taskDescriptor.GroupName,
                (float)currentStep / stepsCount,
                $"(Step {currentStep}/{stepsCount}) IFC file processed and data added to SPARQL dataset.",
                false);

            var datasetName = "test1"; //#todo modelId or taskId instead of hardcoded "test1"

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

                try
                {
                    switch (step.Type)
                    {
                        case "SPARQL":
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
                                _logger.LogWarning("SPARQL query is empty for workflow step: '{StepName}'", step.Name);
                                await _signalRStatus.SendProgressUpdate(
                                    taskDescriptor.GroupName,
                                    (float)currentStep / stepsCount,
                                    $"(Step {currentStep}/{stepsCount}) SPARQL query is empty for workflow step: '{step.Name}'.",
                                    true);
                                continue;
                            }

                            await _signalRStatus.SendProgressUpdate(
                                taskDescriptor.GroupName,
                                (float)currentStep / stepsCount,
                                $"(Step {currentStep}/{stepsCount}) Executing workflow step: '{step.Name}'...<br />ℹ {step.Description}",
                                false);
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
                                _logger.LogError("SPARQL query execution failed for model {ModelId}.", taskDescriptor.TaskId);
                                await _signalRStatus.SendProgressUpdate(
                                    taskDescriptor.GroupName,
                                    (float)currentStep / stepsCount,
                                    $"(Step {currentStep}/{stepsCount}) Workflow step: '{step.Name}' failed.",
                                    true);
                            }
                            break;

                        case "SHACL":
                            string shape = string.Empty;
                            if (step.Parameters.ContainsKey("shape"))
                            {
                                shape = step.Parameters["shape"];
                            }

                            if (string.IsNullOrEmpty(shape) && step.Parameters.ContainsKey("shapeRef"))
                            {
                                var shapeRef = step.Parameters["shapeRef"];
                                if (!string.IsNullOrEmpty(shapeRef) &&
                                    shaclShapes.TryGetValue(shapeRef, out var shaclShape))
                                {
                                    shape = shaclShape.Shape;
                                }
                            }

                            if (string.IsNullOrEmpty(shape))
                            {
                                _logger.LogWarning("SHACL shape is empty for workflow step: '{StepName}'", step.Name);
                                await _signalRStatus.SendProgressUpdate(
                                    taskDescriptor.GroupName,
                                    (float)currentStep / stepsCount,
                                    $"(Step {currentStep}/{stepsCount}) SHACL shape is empty for workflow step: '{step.Name}'.",
                                    true);
                                continue;
                            }

                            await _signalRStatus.SendProgressUpdate(
                                taskDescriptor.GroupName,
                                (float)currentStep / stepsCount,
                                $"(Step {currentStep}/{stepsCount}) Executing workflow step: '{step.Name}'...<br />ℹ {step.Description}",
                                false);
                            await _signalRStatus.SendQueryUpdate(taskDescriptor.GroupName, "SHACL Shape", shape, "", false);

                            var result = await sparqlServer.ExecuteSHACLAsync(datasetName, "https://vng.nl/geometries/", shape);
                            if (!string.IsNullOrEmpty(result))
                            {
                                _logger.LogInformation("Workflow step: '{StepName}' completed for model {ModelId}. Result: {Result}", step.Name, taskDescriptor.TaskId, result);
                                await _signalRStatus.SendQueryUpdate(taskDescriptor.GroupName, "SHACL Validation Report", "", result, result.IndexOf("sh:Violation") != -1);
                            }

                            _logger.LogInformation("Workflow step: {StepName} executed successfully.", step.Name);
                            await _signalRStatus.SendProgressUpdate(
                                taskDescriptor.GroupName,
                                (float)currentStep / stepsCount,
                                $"(Step {currentStep}/{stepsCount}) Workflow step: '{step.Name}' executed successfully.",
                                false);
                            break;

                        case "IDS":
                            string idsFile = step.Parameters["ids_file"];
                            var idsPath = Path.Combine(idsDir, idsFile);
                            var idsFileContent = await File.ReadAllTextAsync(idsPath);

                            await _signalRStatus.SendProgressUpdate(
                                taskDescriptor.GroupName,
                                (float)currentStep / stepsCount,
                                $"(Step {currentStep}/{stepsCount}) Executing workflow step: '{step.Name}'...",
                                false);
                            await _signalRStatus.SendQueryUpdate(taskDescriptor.GroupName, $"IDS File: '{idsFile}'", FormatXML(idsFileContent), "", false);

                            (output, error, exitCode) = await ExecuteProcess(
                                    exePath: isLinuxPlatform ? "./IDSValidator" : "./IDSValidator.exe",
                                    args: $"\"{modelPath}\" \"{idsPath}\""
                                );
                            if (exitCode == 0)
                            {
                                _logger.LogInformation("Workflow step: '{StepName}' completed successfully.", step.Name);
                                await _signalRStatus.SendQueryUpdate(taskDescriptor.GroupName, "IDS Validation Report", "", output, output.IndexOf("ERROR") != -1);
                                await _signalRStatus.SendProgressUpdate(
                                    taskDescriptor.GroupName,
                                    (float)currentStep / stepsCount,
                                    $"(Step {currentStep}/{stepsCount}) Workflow step: '{step.Name}' executed successfully.",
                                    false);
                            }
                            else
                            {
                                _logger.LogError("Workflow step: '{StepName}' failed for model {ModelId} with exit code {ExitCode}", step.Name, taskDescriptor.TaskId, exitCode);
                                await _signalRStatus.SendQueryUpdate(taskDescriptor.GroupName, "IDS Validation Report", "", output, true);
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
            }

            await _signalRStatus.SendProgressUpdate(
                taskDescriptor.GroupName,
                100,
                "Workflow completed successfully.",
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
