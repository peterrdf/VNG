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

                    return fileInfo.Name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) ||
                        fileInfo.Name.EndsWith(".ttl", StringComparison.OrdinalIgnoreCase);
                });

                List<TaskStatus> xmlTasks = new();
                foreach (var workflowXML in workflowXMLs)
                {
                    if (workflowXML?.PhysicalPath != null)
                    {
                        try
                        {
                            Workflow? workflow = null;
                            if (Path.GetExtension(workflowXML.PhysicalPath).Equals(".xml", StringComparison.OrdinalIgnoreCase))
                            {
                                workflow = WorkflowDeserializer.Deserialize(workflowXML.PhysicalPath);
                                _logger?.LogInformation("De-serialized workflow from XML file: {WorkflowXML}", workflowXML.PhysicalPath);
                            }
                            else if (Path.GetExtension(workflowXML.PhysicalPath).Equals(".ttl", StringComparison.OrdinalIgnoreCase))
                            {
                                workflow = WorkflowTurtleDeserializer.Deserialize(workflowXML.PhysicalPath);
                                _logger?.LogInformation("De-serialized workflow from Turtle file: {WorkflowTurtle}", workflowXML.PhysicalPath);
                            }
                            
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
                                _logger?.LogWarning("Workflow de-serialization returned null for file: {WorkflowXML}", workflowXML.PhysicalPath);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogError(ex, "Error de-serializing workflow XML file: {WorkflowXML}", workflowXML.PhysicalPath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading workflows.");
            }

            return workflows;
        }

        public IDictionary<string, string> LoadSPARQLs()
        {
            Dictionary<string, string> sparqlQueries = new();

            try
            {

                var isLinuxPlatform = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux);
                var fileStorage = isLinuxPlatform ? "FileStorageLinux" : "FileStorage";

                var provider = new PhysicalFileProvider(_configuration![$"{fileStorage}:SPARQLDir"]!);
                var sparqlTTLs = provider.GetDirectoryContents("/").Where((fileInfo) =>
                {
                    if (fileInfo.IsDirectory)
                        return false;

                    return fileInfo.Name.EndsWith(".ttl", StringComparison.OrdinalIgnoreCase);
                });

                foreach (var sparqlTTL in sparqlTTLs)
                {
                    if (sparqlTTL?.PhysicalPath != null)
                    {
                        sparqlQueries[Path.GetFileName(sparqlTTL.PhysicalPath)] = sparqlTTL.PhysicalPath;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading SPARQL queries.");
            }

            return sparqlQueries;
        }

        public IDictionary<string, string> LoadSHACLs()
        {
            Dictionary<string, string> shaclShapes = new();
            try
            {
                var isLinuxPlatform = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux);
                var fileStorage = isLinuxPlatform ? "FileStorageLinux" : "FileStorage";

                var provider = new PhysicalFileProvider(_configuration![$"{fileStorage}:SHACLDir"]!);
                var shaclTTLs = provider.GetDirectoryContents("/").Where((fileInfo) =>
                {
                    if (fileInfo.IsDirectory)
                        return false;

                    return fileInfo.Name.EndsWith(".ttl", StringComparison.OrdinalIgnoreCase);
                });

                foreach (var shaclTTL in shaclTTLs)
                {
                    if (shaclTTL?.PhysicalPath != null)
                    {
                        shaclShapes[Path.GetFileName(shaclTTL.PhysicalPath)] = shaclTTL.PhysicalPath;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading SHACL shapes.");
            }

            return shaclShapes;
        }
    }
}
