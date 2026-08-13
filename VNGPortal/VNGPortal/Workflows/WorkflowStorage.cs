using Microsoft.Extensions.FileProviders;
using System.Threading.Tasks;
using System.Xml;

namespace VNGPortal.Workflows
{
    public class WorkflowStorage
    {
        private IConfiguration? _configuration;
        private readonly ILogger? _logger;

        public WorkflowStorage(IConfiguration configuration, ILogger logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IDictionary<string, Workflow> LoadWorkflows()
        {
            Dictionary<string, Workflow> workflows = new();

            try
            {

                var isLinuxPlatform = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux);
                var fileStorage = isLinuxPlatform ? "FileStorageLinux" : "FileStorage";

                var provider = new PhysicalFileProvider(_configuration![$"{fileStorage}:WorkflowsDir"]!);
                var workflowXMLs = provider.GetDirectoryContents("/").Where((fileInfo) =>
                {
                    if (fileInfo.IsDirectory)
                        return false;

                    if (!fileInfo.Name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                        return false;

                    return true;
                });

                List<TaskStatus> xmlTasks = new();
                foreach (var workflowXML in workflowXMLs)
                {
                    if (workflowXML?.PhysicalPath != null)
                    {
                        try
                        {
                            Workflow workflow = WorkflowDeserializer.Deserialize(workflowXML.PhysicalPath);
                            if (workflow != null)
                            {
                                if (string.IsNullOrWhiteSpace(workflow.Id))
                                {
                                    _logger?.LogWarning("Workflow ID is null or empty for file: {WorkflowXML}", workflowXML.PhysicalPath);
                                    continue;
                                }

                                if (workflows.ContainsKey(workflow.Id))
                                {
                                    _logger?.LogWarning("Duplicate workflow ID '{WorkflowId}' found in file: {WorkflowXML}. Skipping this workflow.", workflow.Id, workflowXML.PhysicalPath);
                                    continue;
                                }

                                workflows[workflow.Id] = workflow;
                            }
                            else
                            {
                                _logger?.LogWarning("Workflow deserialization returned null for file: {WorkflowXML}", workflowXML.PhysicalPath);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogError(ex, "Error deserializing workflow XML file: {WorkflowXML}", workflowXML.PhysicalPath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading workflows from XML files.");
            }

            return workflows;
        }

        public IDictionary<string, SPARQLQuery> LoadSPARQLQueries()
        {
            Dictionary<string, SPARQLQuery> sparqlQueries = new();

            try
            {

                var isLinuxPlatform = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux);
                var fileStorage = isLinuxPlatform ? "FileStorageLinux" : "FileStorage";

                var provider = new PhysicalFileProvider(_configuration![$"{fileStorage}:SPARQLDir"]!);
                var sparqlXMLs = provider.GetDirectoryContents("/").Where((fileInfo) =>
                {
                    if (fileInfo.IsDirectory)
                        return false;

                    if (!fileInfo.Name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                        return false;

                    return true;
                });

                List<TaskStatus> xmlTasks = new();
                foreach (var sparqlXML in sparqlXMLs)
                {
                    if (sparqlXML?.PhysicalPath != null)
                    {
                        try
                        {
                            SPARQLQuery sparqlQuery = SPARQLQueryDeserializer.Deserialize(sparqlXML.PhysicalPath);
                            if (sparqlQuery != null)
                            {
                                if (string.IsNullOrWhiteSpace(sparqlQuery.Id))
                                {
                                    _logger?.LogWarning("SPARQLQuery ID is null or empty for file: {SPARQLXML}", sparqlXML.PhysicalPath);
                                    continue;
                                }

                                if (sparqlQueries.ContainsKey(sparqlQuery.Id))
                                {
                                    _logger?.LogWarning("Duplicate SPARQLQuery ID '{SPARQLQueryId}' found in file: {SPARQLXML}. Skipping this SPARQLQuery.", sparqlQuery.Id, sparqlXML.PhysicalPath);
                                    continue;
                                }

                                sparqlQueries[sparqlQuery.Id] = sparqlQuery;
                            }
                            else
                            {
                                _logger?.LogWarning("SPARQLQuery deserialization returned null for file: {SPARQLXML}", sparqlXML.PhysicalPath);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogError(ex, "Error deserializing SPARQLQuery XML file: {SPARQLXML}", sparqlXML.PhysicalPath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading workflows from XML files.");
            }

            return sparqlQueries;
        }

        public IDictionary<string, SHACLShape> LoadSHACLShapes()
        {
            Dictionary<string, SHACLShape> shaclShapes = new();
            try
            {
                var isLinuxPlatform = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux);
                var fileStorage = isLinuxPlatform ? "FileStorageLinux" : "FileStorage";

                var provider = new PhysicalFileProvider(_configuration![$"{fileStorage}:SHACLDir"]!);
                var shaclXMLs = provider.GetDirectoryContents("/").Where((fileInfo) =>
                {
                    if (fileInfo.IsDirectory)
                        return false;
                    if (!fileInfo.Name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                        return false;
                    return true;
                });
                List<TaskStatus> xmlTasks = new();
                foreach (var shaclXML in shaclXMLs)
                {
                    if (shaclXML?.PhysicalPath != null)
                    {
                        try
                        {
                            SHACLShape shaclShape = SHACLShapeDeserializer.Deserialize(shaclXML.PhysicalPath);
                            if (shaclShape != null)
                            {
                                if (string.IsNullOrWhiteSpace(shaclShape.Id))
                                {
                                    _logger?.LogWarning("SHACLShape ID is null or empty for file: {SHACLXML}", shaclXML.PhysicalPath);
                                    continue;
                                }
                                if (shaclShapes.ContainsKey(shaclShape.Id))
                                {
                                    _logger?.LogWarning("Duplicate SHACLShape ID '{SHACLShapeId}' found in file: {SHACLXML}. Skipping this SHACLShape.", shaclShape.Id, shaclXML.PhysicalPath);
                                    continue;
                                }
                                shaclShapes[shaclShape.Id] = shaclShape;
                            }
                            else
                            {
                                _logger?.LogWarning("SHACLShape deserialization returned null for file: {SHACLXML}", shaclXML.PhysicalPath);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogError(ex, "Error deserializing SHACLShape XML file: {SHACLXML}", shaclXML.PhysicalPath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading workflows from XML files.");
            }

            return shaclShapes;
        }
    }
}
