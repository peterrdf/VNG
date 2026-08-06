using VNGPortal.IFC2RDF;
using VNGPortal.Services;

namespace VNGPortal.Workflows
{
    public class VNGWorkflow : _Workflow
    {
        #region SPARQL Queries

        private const string ValidateTopologicalRelationsSHACL = """"
            @prefix sh:   <http://www.w3.org/ns/shacl#> .
            @prefix geom: <https://vng.nl/geometry/> .
            @prefix rdf:  <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .
            @prefix xsd:  <http://www.w3.org/2001/XMLSchema#> .

            geom:GeometryShape
                a sh:NodeShape ;
                sh:targetClass geom:Geometry ;

                sh:sparql [
                    sh:prefixes [
                        sh:declare [
                            sh:prefix "geom" ;
                            sh:namespace "https://vng.nl/geometry/"^^xsd:anyURI
                        ] ;
                        sh:declare [
                            sh:prefix "rdf" ;
                            sh:namespace "http://www.w3.org/1999/02/22-rdf-syntax-ns#"^^xsd:anyURI
                        ]
                    ] ;
                    sh:select """
                        SELECT $this ?value ?globalId
                        WHERE {
                            $this geom:globalId            ?globalId ;
                                  geom:topologicalRelation ?value .
                            FILTER(?value != "CONTAINED BY")
                        }
                    """ ;
                    sh:message "geom:topologicalRelation is NOT 'CONTAINED BY'. GlobalId: {?globalId}" ;
                ] ;
                sh:severity sh:Violation .
            """";

        private const string ValidateBBoxesSHACL = """"
            @prefix sh:   <http://www.w3.org/ns/shacl#> .
            @prefix geom: <https://vng.nl/geometry/> .
            @prefix rdf:  <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .
            @prefix xsd:  <http://www.w3.org/2001/XMLSchema#> .

            geom:GeometryShape
                a sh:NodeShape ;
                sh:targetClass geom:Geometry ;

                sh:sparql [
                    sh:prefixes [
                        sh:declare [
                            sh:prefix "geom" ;
                            sh:namespace "https://vng.nl/geometry/"^^xsd:anyURI
                        ]
                    ] ;
                    sh:select """
                        SELECT $this ?globalId ?deltaX ?deltaY ?deltaZ
                        WHERE {
                            $this   geom:globalId   ?globalId ;
                                    geom:bboxMinX   ?minX ;
                                    geom:bboxMaxX   ?maxX ;
                                    geom:bboxMinY   ?minY ;
                                    geom:bboxMaxY   ?maxY ;
                                    geom:bboxMinZ   ?minZ ;
                                    geom:bboxMaxZ   ?maxZ .
                            BIND((?maxX - ?minX) AS ?deltaX)
                            BIND((?maxY - ?minY) AS ?deltaY)
                            BIND((?maxZ - ?minZ) AS ?deltaZ)

                            FILTER(
                                ?deltaX < 1 || ?deltaX > 100 ||
                                ?deltaY < 1 || ?deltaY > 100 ||
                                ?deltaZ < 1 || ?deltaZ > 100
                            )
                        }
                    """ ;
                    sh:message "BBox violation for GlobalId {?globalId}: deltaX={?deltaX}, deltaY={?deltaY}, deltaZ={?deltaZ}. All axis deltas must be in [1, 100]." ;
                ] ;
                sh:severity sh:Violation .
            """";

        private const string ValidateModelBBoxSHACL = """"
            @prefix sh:   <http://www.w3.org/ns/shacl#> .
            @prefix geom: <https://vng.nl/geometry/> .
            @prefix rdf:  <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .
            @prefix xsd:  <http://www.w3.org/2001/XMLSchema#> .

            geom:DatasetBBoxShape
                a sh:NodeShape ;
                sh:targetNode <$graphUri> ;

                sh:sparql [
                    sh:prefixes [
                        sh:declare [ sh:prefix "geom" ; sh:namespace "https://vng.nl/geometry/"^^xsd:anyURI ]
                    ] ;
                    sh:select """
                        SELECT ?this ?globalMinX ?globalMaxX ?globalMinY ?globalMaxY ?globalMinZ ?globalMaxZ
                        WHERE {

                            {
                                SELECT
                                    (MIN(?bboxMinX - ?eastings)  AS ?globalMinX)
                                    (MAX(?bboxMaxX - ?eastings)  AS ?globalMaxX)
                                    (MIN(?bboxMinY - ?northings) AS ?globalMinY)
                                    (MAX(?bboxMaxY - ?northings) AS ?globalMaxY)
                                    (MIN(?bboxMinZ)              AS ?globalMinZ)
                                    (MAX(?bboxMaxZ)              AS ?globalMaxZ)
                                WHERE {
                                    ?geom a              geom:Geometry ;
                                          geom:eastings  ?eastings ;
                                          geom:northings ?northings ;
                                          geom:bboxMinX  ?bboxMinX ;
                                          geom:bboxMaxX  ?bboxMaxX ;
                                          geom:bboxMinY  ?bboxMinY ;
                                          geom:bboxMaxY  ?bboxMaxY ;
                                          geom:bboxMinZ  ?bboxMinZ ;
                                          geom:bboxMaxZ  ?bboxMaxZ .
                                }
                            }

                            FILTER(
                                ?globalMaxX - ?globalMinX < 1 || ?globalMaxX - ?globalMinX > 100 ||
                                ?globalMaxY - ?globalMinY < 1 || ?globalMaxY - ?globalMinY > 100 ||
                                ?globalMaxZ - ?globalMinZ < 1 || ?globalMaxZ - ?globalMinZ > 100
                            )
                        }
                    """ ;
                    sh:message "Dataset BBox violation: globalMinX={?globalMinX}, globalMaxX={?globalMaxX}, globalMinY={?globalMinY}, globalMaxY={?globalMaxY}, globalMinZ={?globalMinZ}, globalMaxZ={?globalMaxZ}. All values must be in [1, 100]." ;
                ] ;
                sh:severity sh:Violation .
            """";
        #endregion

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

                            await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, (float)currentStep / stepsCount, $"(Step {currentStep}/{stepsCount}) Executing workflow step: {step.Name}...", false);
                            await _signalRStatus.SendQueryUpdate(taskDescriptor.GroupName, "SPARQL Query", query, "", false);
                            if (!await sparqlServer.ExecuteInsertAsync(datasetName, query))
                            {
                                return false;
                            }
                            _logger.LogInformation("Workflow step {StepName} executed successfully.", step.Name);
                            await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, (float)currentStep / stepsCount, $"(Step {currentStep}/{stepsCount}) Workflow step: {step.Name} executed successfully.", false);
                            break;

                        case "SHACL":
                            break;

                        default:
                            _logger.LogError("Unknown Step Type: {StepType}.", step.Type);
                            continue;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing workflow step: {StepName}",  step.Name);
                    await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 0, $"Error executing workflow step: {step.Name}.", true);
                    throw;
                }                   
            }

            //// Validate data in the Jena-Fuseki database
            //try
            //{
            //    await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 80, "(Step 10/12) Validating Topological Relations in SPARQL dataset...", false);
            //    await _signalRStatus.SendQueryUpdate(taskDescriptor.GroupName, "SHACL Shape", ValidateTopologicalRelationsSHACL, "", false);

            //    var result = await sparqlServer.ExecuteSHACLAsync(datasetName, "https://vng.nl/geometries/", ValidateTopologicalRelationsSHACL);
            //    if (!string.IsNullOrEmpty(result))
            //    {
            //        await _signalRStatus.SendQueryUpdate(taskDescriptor.GroupName, "SHACL Validation Report", "", result, result.IndexOf("sh:Violation") != -1);
            //    }

            //    _logger.LogInformation("Topological Relations validated.");
            //    await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 80, "(Step 10/12) Topological Relations validated.", false);
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, "Error validating Topological Relations in SPARQL dataset.");
            //    await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 0, "Error validating Topological Relations in SPARQL dataset.", true);
            //    throw;
            //}

            //// Validate data in the Jena-Fuseki database
            //try
            //{
            //    await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 88, "(Step 11/12) Validating BBoxes in SPARQL dataset...", false);
            //    await _signalRStatus.SendQueryUpdate(taskDescriptor.GroupName, "SHACL Shape", ValidateBBoxesSHACL, "", false);

            //    var result = await sparqlServer.ExecuteSHACLAsync(datasetName, "https://vng.nl/geometries/", ValidateBBoxesSHACL);
            //    if (!string.IsNullOrEmpty(result))
            //    {
            //        await _signalRStatus.SendQueryUpdate(taskDescriptor.GroupName, "SHACL Validation Report", "", result, result.IndexOf("sh:Violation") != -1);
            //    }

            //    _logger.LogInformation("BBoxes validated.");
            //    await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 88, "(Step 11/12) BBoxes validated.", false);
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, "Error validating BBoxes in SPARQL dataset.");
            //    await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 0, "Error validating BBoxes in SPARQL dataset.", true);
            //    throw;
            //}

            //// Validate data in the Jena-Fuseki database
            //try
            //{
            //    await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 100, "(Step 12/12) Validating Model BBox in SPARQL dataset...", false);
            //    await _signalRStatus.SendQueryUpdate(taskDescriptor.GroupName, "SHACL Shape", ValidateModelBBoxSHACL, "", false);

            //    var result = await sparqlServer.ExecuteSHACLAsync(datasetName, "https://vng.nl/geometries/", ValidateModelBBoxSHACL);
            //    if (!string.IsNullOrEmpty(result))
            //    {
            //        await _signalRStatus.SendQueryUpdate(taskDescriptor.GroupName, "SHACL Validation Report", "", result, result.IndexOf("sh:Violation") !=- 1);
            //    }

            //    _logger.LogInformation("Model BBox validated.");
            //    await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 100, "(Step 12/12) Model BBox validated.", false);
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, "Error validating Model BBox in SPARQL dataset.");
            //    await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 0, "Error validating Model BBox in SPARQL dataset.", true);
            //    throw;
            //}

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
