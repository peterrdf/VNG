using VNGPortal.IFC2RDF;
using VNGPortal.Services;

namespace VNGPortal.Workflows
{
    public class VNGWorkflow : _Workflow
    {
        #region SPARQL Queries
        private const string InsertBuildingsQuery = """
            PREFIX vng:     <http://vng.nl/geometry-ext.ttl#>
            PREFIX bldg:    <http://vng.nl/buildings/>
            PREFIX ifc:     <https://standards.buildingsmart.org/IFC/DEV/IFC4/ADD2/OWL#>
            PREFIX express: <https://w3id.org/express#>
            PREFIX rdf:     <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

            DROP SILENT GRAPH <buildings.ttl> ;

            INSERT {
              GRAPH <buildings.ttl> {
                ?buildingUri  a             bldg:Building ;
                              bldg:id    	?id ;
                              bldg:name  	?name ;
            				  bldg:geometry ?geometry .
              }
            }
            WHERE {
              {
                SELECT ?e ?n WHERE {
                  ?mc rdf:type                          ifc:IfcMapConversion ;
                      ifc:eastings_IfcMapConversion     ?le ;
                      ifc:northings_IfcMapConversion    ?ln .
                  ?le express:hasDouble                 ?e .
                  ?ln express:hasDouble                 ?n .
                } LIMIT 1
              }
              ("Buildings" ?e ?n) vng:pdokFeature (?id ?name ?geometry) .
              BIND(IRI(CONCAT(STR(bldg:), ?id)) AS ?buildingUri)
            }
            """;

        private const string InsertParcelsQuery = """
            PREFIX vng:     <http://vng.nl/geometry-ext.ttl#>
            PREFIX pcl:     <http://vng.nl/parcels/>
            PREFIX ifc:     <https://standards.buildingsmart.org/IFC/DEV/IFC4/ADD2/OWL#>
            PREFIX express: <https://w3id.org/express#>
            PREFIX rdf:     <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

            DROP SILENT GRAPH <parcels.ttl> ;

            INSERT {
              GRAPH <parcels.ttl> {
                ?parcelUri  a          		pcl:Parcel ;
                            pcl:id    		?id ;
                            pcl:name		?name ;
            				pcl:geometry 	?geometry .
              }
            }
            WHERE {
              {
                SELECT ?e ?n WHERE {
                  ?mc rdf:type                          ifc:IfcMapConversion ;
                      ifc:eastings_IfcMapConversion     ?le ;
                      ifc:northings_IfcMapConversion    ?ln .
                  ?le express:hasDouble                 ?e .
                  ?ln express:hasDouble                 ?n .
                } LIMIT 1
              }
              ("Parcels" ?e ?n) vng:pdokFeature (?id ?name ?geometry) .
              BIND(IRI(CONCAT(STR(pcl:), ?id)) AS ?parcelUri)
            }            
            """;

        private const string InsertAreasQuery = """
            PREFIX ifc:     <https://standards.buildingsmart.org/IFC/DEV/IFC4/ADD2/OWL#>
            PREFIX rdf:     <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
            PREFIX express: <https://w3id.org/express#>
            PREFIX vng:     <http://vng.nl/geometry-ext.ttl#>
            PREFIX dso:     <http://vng.nl/areas/>

            DROP SILENT GRAPH <areas.ttl> ;

            INSERT {
              GRAPH <areas.ttl> {
                ?areaUri  a dso:area ;
                            dso:id    		?id ;
                            dso:name		?name ;
            				dso:group		?group ;
            				dso:type		?type ;
            				dso:geometry 	?geometry .
              }
            }
            WHERE {
              {
                SELECT ?e ?n WHERE {
                  ?mc rdf:type                          ifc:IfcMapConversion ;
                      ifc:eastings_IfcMapConversion     ?le ;
                      ifc:northings_IfcMapConversion    ?ln .
                  ?le express:hasDouble                 ?e .
                  ?ln express:hasDouble                 ?n .
                } LIMIT 1
              }
              (?e ?n) vng:dsoArea (?id ?name ?group ?type ?geometry) .
              BIND(IRI(CONCAT(STR(dso:), ?id)) AS ?areaUri)
            }            
            """;

        private const string InsertBBoxesQuery = """
            PREFIX geom: <https://vng.nl/geometry/>
            PREFIX pf:   <java:org.example.>

            INSERT {
                GRAPH <https://vng.nl/geometries/> {
                    ?instance geom:bboxMinX ?minX ;
                              geom:bboxMinY ?minY ;
                              geom:bboxMinZ ?minZ ;
                              geom:bboxMaxX ?maxX ;
                              geom:bboxMaxY ?maxY ;
                              geom:bboxMaxZ ?maxZ .
                }
            }
            WHERE {
                GRAPH <https://vng.nl/geometries/> {
                    ?instance a geom:Geometry ;
                              geom:base64Data ?base64Data .
                    FILTER NOT EXISTS { ?instance geom:bboxMinX ?any }
                }
                (?minX ?minY ?minZ ?maxX ?maxY ?maxZ) pf:BBoxPropFunction ?base64Data .
            }            
            """;

        private const string InsertProjectionsQuery = """
            PREFIX geom:    <https://vng.nl/geometry/>
            PREFIX ext:     <http://vng.nl/geometry-ext.ttl#>

            INSERT {
                GRAPH <https://vng.nl/geometries/> {
                    ?instance geom:projectionBase64Data ?projection .
                }
            }
            WHERE {
                GRAPH <https://vng.nl/geometries/> {
                    ?instance   a               geom:Geometry ;
                                geom:base64Data ?base64Data .
                    BIND(ext:projection(?base64Data) AS ?projection)
                    FILTER NOT EXISTS { ?instance geom:projectionBase64Data ?any }
                }
            }
            """;

        private const string InsertTopologicalRelationsQuery = """
            PREFIX rdf:     <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
            PREFIX ifc:     <https://standards.buildingsmart.org/IFC/DEV/IFC4/ADD2/OWL#>
            PREFIX express: <https://w3id.org/express#>
            PREFIX geom:    <https://vng.nl/geometry/>
            PREFIX dso:     <http://vng.nl/areas/>
            PREFIX ext:     <http://vng.nl/geometry-ext.ttl#>
            PREFIX topo:    <http://vng.nl/topology/>

            DELETE {
                GRAPH <https://vng.nl/geometries/> {
                    ?geometry geom:topologicalRelation ?oldRelation .
                }
            }
            INSERT {
                GRAPH <https://vng.nl/geometries/> {
                    ?geometry geom:topologicalRelation ?topologicalRelation .
                }
            }
            WHERE {
                VALUES ?ifcType { ifc:IfcWall }

                ?ifcElement     rdf:type                    ?ifcType ;
                                ifc:globalId_IfcRoot        ?globalIdNode .
                ?globalIdNode   rdf:type                    ifc:IfcGloballyUniqueId ;
                                express:hasString           ?globalIdValue .

                GRAPH <https://vng.nl/geometries/> {
                    ?geometry   rdf:type                    geom:Geometry ;
                                geom:globalId               ?globalIdValue ;
                                geom:projectionBase64Data   ?base64Data1 .
                }

                {
                    SELECT (GROUP_CONCAT(?base64Data2; SEPARATOR="|") AS ?allBase64)
                    WHERE {
                        GRAPH <areas.ttl> {
                            [] a             dso:area ;
                               dso:group     "wonen" ;
            				   dso:geometry  ?base64Data2 .
                        }
                    }
                }

                BIND(ext:topologicalRelation(?base64Data1, ?allBase64) AS ?topologicalRelation)

                OPTIONAL {
                    GRAPH <https://vng.nl/geometries/> {
                        ?geometry geom:topologicalRelation ?oldRelation .
                    }
                }
            }            
            """;

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
        public VNGWorkflow(IConfiguration configuration, ILogger logger, ISignalRStatusService signalRStatus, string groupName, Dictionary<string, string>? options)
            : base(configuration, logger, signalRStatus, groupName, options)
        {
        }

        public override async Task<bool> ExecuteAsync(TaskDescriptor taskDescriptor)
        {
            await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 100, "Workflow started...", false);

            var isLinuxPlatform = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux);
            var fileStorage = isLinuxPlatform ? "FileStorageLinux" : "FileStorage";

            var modelsDir = _configuration[$"{fileStorage}:ModelsDir"]!;

            var modelDir = Path.Combine(modelsDir, taskDescriptor.TaskId);
            var filePath = Path.Combine(modelDir, taskDescriptor.Model);

            // IFC to RDF conversion
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
            await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 8, "(Step 1/12) IFC to RDF conversion completed successfully.", false);

            // Geometry to RDF conversion
            try
            {
                var geometry2RDF = new Geometry2RDF(_logger);
                await geometry2RDF.Run(filePath);
                _logger.LogInformation("Geometry to RDF conversion completed successfully.");
                await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 16, "(Step 2/12) Geometry to RDF conversion completed successfully.", false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Geometry to RDF conversion failed for model {ModelId}", taskDescriptor.TaskId);
                await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 0, "Geometry to RDF conversion failed.", true);
                throw;
            }

            // Create Jena-Fuseki database for the model
            var sparqlServer = new SPARQL.Server(_logger);
            sparqlServer.CreateDataset("test1");

            // Add data to the Jena-Fuseki database
            sparqlServer.AddData("test1", new List<string>
            {
                Path.Combine(modelDir, Path.GetFileNameWithoutExtension(filePath) + ".ttl"),
                Path.Combine(modelDir, Path.GetFileNameWithoutExtension(filePath) + "_geometry.trig")
            });
            _logger.LogInformation("IFC file processed and data added to SPARQL dataset.");
            await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 24, "(Step 3/12) IFC file processed and data added to SPARQL dataset.", false);

            var datasetName = "test1"; //#todo modelId or taskId instead of hardcoded "test1"

            // Insert data into the Jena-Fuseki database
            try
            {
                await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 32, "(Step 4/12) Inserting Buildings into SPARQL dataset...", false);
                await _signalRStatus.SendQueryUpdate(taskDescriptor.GroupName, "SPARQL Query", InsertBuildingsQuery, "", false);

                if (!await sparqlServer.ExecuteInsertAsync(datasetName, InsertBuildingsQuery))
                {
                    return false;
                }

                _logger.LogInformation("Buildings inserted into SPARQL dataset successfully.");
                await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 32, "(Step 4/12) Buildings inserted into SPARQL dataset successfully.", false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting Buildings into SPARQL dataset.");
                await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 0, "Error inserting Buildings into SPARQL dataset.", true);
                throw;
            }

            // Insert data into the Jena-Fuseki database
            try
            {
                await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 40, "(Step 5/12) Inserting Parcels into SPARQL dataset...", false);
                await _signalRStatus.SendQueryUpdate(taskDescriptor.GroupName, "SPARQL Query", InsertParcelsQuery, "", false);

                if (!await sparqlServer.ExecuteInsertAsync(datasetName, InsertParcelsQuery))
                {
                    return false;
                }

                _logger.LogInformation("Parcels inserted into SPARQL dataset successfully.");
                await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 40, "(Step 5/12) Parcels inserted into SPARQL dataset successfully.", false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting Parcels into SPARQL dataset.");
                await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 0, "Error inserting Parcels into SPARQL dataset.", true);
                throw;
            }

            // Insert data into the Jena-Fuseki database
            try
            {
                await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 48, "(Step 6/12) Inserting Areas into SPARQL dataset...", false);
                await _signalRStatus.SendQueryUpdate(taskDescriptor.GroupName, "SPARQL Query", InsertAreasQuery, "", false);

                if (!await sparqlServer.ExecuteInsertAsync(datasetName, InsertAreasQuery))
                {
                    return false;
                }

                _logger.LogInformation("Areas inserted into SPARQL dataset successfully.");
                await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 48, "(Step 6/12) Areas inserted into SPARQL dataset successfully.", false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting Areas into SPARQL dataset.");
                await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 0, "Error inserting Areas into SPARQL dataset.", true);
                throw;
            }

            // Insert data into the Jena-Fuseki database
            try
            {
                await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 56, "(Step 7/12) Inserting BBoxes into SPARQL dataset...", false);
                await _signalRStatus.SendQueryUpdate(taskDescriptor.GroupName, "SPARQL Query", InsertBBoxesQuery, "", false);

                if (!await sparqlServer.ExecuteInsertAsync(datasetName, InsertBBoxesQuery))
                {
                    return false;
                }

                _logger.LogInformation("BBoxes inserted into SPARQL dataset successfully.");
                await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 56, "(Step 7/12) BBoxes inserted into SPARQL dataset successfully.", false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting BBoxes into SPARQL dataset.");
                await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 0, "Error inserting BBoxes into SPARQL dataset.", true);
                throw;
            }

            // Insert data into the Jena-Fuseki database
            try
            {
                await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 64, "(Step 8/12) Inserting Projections into SPARQL dataset...", false);
                await _signalRStatus.SendQueryUpdate(taskDescriptor.GroupName, "SPARQL Query", InsertProjectionsQuery, "", false);

                if (!await sparqlServer.ExecuteInsertAsync(datasetName, InsertProjectionsQuery))
                {
                    return false;
                }

                _logger.LogInformation("Projections inserted into SPARQL dataset successfully.");
                await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 64, "(Step 8/12) Projections inserted into SPARQL dataset successfully.", false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting Projections into SPARQL dataset.");
                await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 0, "Error inserting Projections into SPARQL dataset.", true);
                throw;
            }

            // Insert data into the Jena-Fuseki database
            try
            {
                await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 72, "(Step 9/12) Inserting Topological Relations into SPARQL dataset...", false);
                await _signalRStatus.SendQueryUpdate(taskDescriptor.GroupName, "SPARQL Query", InsertTopologicalRelationsQuery, "", false);

                if (!await sparqlServer.ExecuteInsertAsync(datasetName, InsertTopologicalRelationsQuery))
                {
                    return false;
                }

                _logger.LogInformation("Topological Relations inserted into SPARQL dataset successfully.");
                await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 72, "(Step 9/12) Topological Relations inserted into SPARQL dataset successfully.", false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting Topological Relations into SPARQL dataset.");
                await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 0, "Error inserting Topological Relations into SPARQL dataset.", true);
                throw;
            }

            // Validate data in the Jena-Fuseki database
            try
            {
                await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 80, "(Step 10/12) Validating Topological Relations in SPARQL dataset...", false);
                await _signalRStatus.SendQueryUpdate(taskDescriptor.GroupName, "SHACL Shape", ValidateTopologicalRelationsSHACL, "", false);

                var result = await sparqlServer.ExecuteSHACLAsync(datasetName, "https://vng.nl/geometries/", ValidateTopologicalRelationsSHACL);
                if (!string.IsNullOrEmpty(result))
                {
                    await _signalRStatus.SendQueryUpdate(taskDescriptor.GroupName, "SHACL Validation Report", "", result, result.IndexOf("sh:Violation") != -1);
                }

                _logger.LogInformation("Topological Relations validated.");
                await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 80, "(Step 10/12) Topological Relations validated.", false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Topological Relations in SPARQL dataset.");
                await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 0, "Error validating Topological Relations in SPARQL dataset.", true);
                throw;
            }

            // Validate data in the Jena-Fuseki database
            try
            {
                await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 88, "(Step 11/12) Validating BBoxes in SPARQL dataset...", false);
                await _signalRStatus.SendQueryUpdate(taskDescriptor.GroupName, "SHACL Shape", ValidateBBoxesSHACL, "", false);

                var result = await sparqlServer.ExecuteSHACLAsync(datasetName, "https://vng.nl/geometries/", ValidateBBoxesSHACL);
                if (!string.IsNullOrEmpty(result))
                {
                    await _signalRStatus.SendQueryUpdate(taskDescriptor.GroupName, "SHACL Validation Report", "", result, result.IndexOf("sh:Violation") != -1);
                }

                _logger.LogInformation("BBoxes validated.");
                await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 88, "(Step 11/12) BBoxes validated.", false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating BBoxes in SPARQL dataset.");
                await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 0, "Error validating BBoxes in SPARQL dataset.", true);
                throw;
            }

            // Validate data in the Jena-Fuseki database
            try
            {
                await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 100, "(Step 12/12) Validating Model BBox in SPARQL dataset...", false);
                await _signalRStatus.SendQueryUpdate(taskDescriptor.GroupName, "SHACL Shape", ValidateModelBBoxSHACL, "", false);

                var result = await sparqlServer.ExecuteSHACLAsync(datasetName, "https://vng.nl/geometries/", ValidateModelBBoxSHACL);
                if (!string.IsNullOrEmpty(result))
                {
                    await _signalRStatus.SendQueryUpdate(taskDescriptor.GroupName, "SHACL Validation Report", "", result, result.IndexOf("sh:Violation") !=- 1);
                }

                _logger.LogInformation("Model BBox validated.");
                await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 100, "(Step 12/12) Model BBox validated.", false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Model BBox in SPARQL dataset.");
                await _signalRStatus.SendProgressUpdate(taskDescriptor.GroupName, 0, "Error validating Model BBox in SPARQL dataset.", true);
                throw;
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
