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

            var modelDir = Path.Combine(modelsDir, taskDescriptor.TaskId);
            var filePath = Path.Combine(modelDir, taskDescriptor.Model);

            //
            // IFC to RDF conversion
            //
            currentStep++;

            var jarPath = Path.Combine(Directory.GetCurrentDirectory(), "IFC2RDF", "ifc2rdf-1.4.7-shaded.jar");
            var javaPath = _configuration[$"{(isLinuxPlatform ? "ToolsLinux" : "Tools")}:JavaPath"]!;
            int exitCode = await ExecuteProcess(
                exePath: javaPath,
                args: $"-jar \"{jarPath}\" --baseURI http://vng.nl/geometry/ --dir \"{modelDir}\""
            );
            if (exitCode != 0)
            {                
                _logger.LogError("IFC to RDF conversion failed for model {ModelId} with exit code {ExitCode}", taskDescriptor.TaskId, exitCode);
                await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 0, "IFC to RDF conversion failed.", true);
                throw new Exception($"IFC to RDF conversion failed for model {taskDescriptor.TaskId} with exit code {exitCode}");
            }
            _logger.LogInformation("IFC to RDF conversion completed successfully.");
            await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, (float)currentStep / stepsCount, $"(Step {currentStep}/{stepsCount}) IFC to RDF conversion completed successfully.", false);

            //
            // Geometry to RDF conversion
            //
            currentStep++;

            try
            {
                var geometry2RDF = new Geometry2RDF(_logger);
                await geometry2RDF.Run(filePath);
                _logger.LogInformation("Geometry to RDF conversion completed successfully.");
                await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, (float)currentStep / stepsCount, $"(Step {currentStep}/{stepsCount}) Geometry to RDF conversion completed successfully.", false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Geometry to RDF conversion failed for model {ModelId}", taskDescriptor.TaskId);
                await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 0, "Geometry to RDF conversion failed.", true);
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
                Path.Combine(modelDir, Path.GetFileNameWithoutExtension(filePath) + ".ttl"),
                Path.Combine(modelDir, Path.GetFileNameWithoutExtension(filePath) + "_geometry.trig")
            });
            _logger.LogInformation("IFC file processed and data added to SPARQL dataset.");
            await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, (float)currentStep / stepsCount, $"(Step {currentStep}/{stepsCount}) IFC file processed and data added to SPARQL dataset.", false);

            var datasetName = "test1"; //#todo modelId or taskId instead of hardcoded "test1"

            //
            // Execute workflow steps
            //

            for (int i = 0; _workflow.Steps != null && i < _workflow.Steps.Count; i++)
            {
                var step = _workflow.Steps[i]; 
                currentStep++;

                try
                {
                    switch (step.Type)
                    {
                        case "SPARQL":
                            string query = step.Parameters["query"];

                            await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, (float)currentStep / stepsCount, $"(Step {currentStep}/{stepsCount}) Executing workflow step: '{step.Name}'...", false);
                            await _signalRStatus.SendQueryUpdate(taskDescriptor.GroupName, "SPARQL Query", query, "", false);
                            if (!await sparqlServer.ExecuteInsertAsync(datasetName, query))
                            {
                                return false;
                            }
                            _logger.LogInformation("Workflow step {StepName} executed successfully.", step.Name);
                            await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, (float)currentStep / stepsCount, $"(Step {currentStep}/{stepsCount}) Workflow step: '{step.Name}' executed successfully.", false);
                            break;

                        case "SHACL":
                            string shape = step.Parameters["shape"];

                            await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, (float)currentStep / stepsCount, $"(Step {currentStep}/{stepsCount}) Executing workflow step: '{step.Name}'...", false);
                            await _signalRStatus.SendQueryUpdate(taskDescriptor.GroupName, "SHACL Shape", shape, "", false);

                            var result = await sparqlServer.ExecuteSHACLAsync(datasetName, "https://vng.nl/geometries/", shape);
                            if (!string.IsNullOrEmpty(result))
                            {
                                await _signalRStatus.SendQueryUpdate(taskDescriptor.GroupName, "SHACL Validation Report", "", result, result.IndexOf("sh:Violation") != -1);
                            }

                            _logger.LogInformation("Workflow step {StepName} executed successfully.", step.Name);
                            await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, (float)currentStep / stepsCount, $"(Step {currentStep}/{stepsCount}) Workflow step: '{step.Name}' executed successfully.", false);
                            break;

                        default:
                            _logger.LogError("Unknown Step Type: {StepType}.", step.Type);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing workflow step: '{StepName}'",  step.Name);
                    await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 0, $"Error executing workflow step: '{step.Name}'.", true);
                    throw;
                }
            }

            await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 100, "Workflow completed successfully.", false);

            return true;
        }
        #endregion

        #region Properties
        public override string Name => "VNG";
        public override string Description => "VNG Workflow";
        #endregion

    }
}
