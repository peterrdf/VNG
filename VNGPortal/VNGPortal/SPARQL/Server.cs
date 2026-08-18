using System.Net;
using VDS.RDF.Query;

namespace VNGPortal.SPARQL
{
    public class Server
    {
        #region Constants
        private readonly string _sparqlEndpoint = "http://vngtriplestore:3030/"; // Docker container name for Fuseki server
        //private readonly string _sparqlEndpoint = "http://localhost:3030/"; // Windows local development
        private readonly string _user = "admin";
        private readonly string _password = "admin123";
        #endregion

        #region Fields
        private readonly ILogger _logger;
        #endregion

        public Server(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool CreateDataset(string datasetName)
        {
            try
            {
                // Attempt to delete the dataset if it exists
                DeleteDataset(datasetName);

                using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(30) };

                var requestUri = $"{_sparqlEndpoint}$/datasets";
                var byteArray = System.Text.Encoding.ASCII.GetBytes($"{_user}:{_password}");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                // Create the assembler configuration
                var assemblerConfig = $@"@prefix :        <#> .
@prefix fuseki:  <http://jena.apache.org/fuseki#> .
@prefix rdf:     <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .
@prefix rdfs:    <http://www.w3.org/2000/01/rdf-schema#> .
@prefix tdb2:    <http://jena.apache.org/2016/tdb#> .
@prefix ja:      <http://jena.hpl.hp.com/2005/11/Assembler#> .
@prefix shacl:   <http://www.w3.org/ns/shacl#> .

:service rdf:type fuseki:Service ;
    fuseki:name ""{datasetName}"" ;
    fuseki:endpoint [ fuseki:operation fuseki:query ] ;
    fuseki:endpoint [ fuseki:operation fuseki:update ] ;
    fuseki:endpoint [ fuseki:operation fuseki:gsp-rw ] ;
    fuseki:endpoint [ fuseki:operation fuseki:shacl ;
                     fuseki:name ""shacl"" ] ;
    fuseki:dataset :dataset .

:dataset rdf:type tdb2:DatasetTDB ;
    tdb2:location ""{datasetName}"" .
";

                // Create multipart/form-data content
                using var content = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes(assemblerConfig));
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                content.Add(fileContent, "file", $"{datasetName}-assembler.ttl");

                var response = client.PostAsync(requestUri, content).Result;

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Dataset '{DatasetName}' created successfully with SHACL support.", datasetName);
                    return true;
                }
                else
                {
                    _logger.LogError("Failed to create dataset '{DatasetName}'. Status code: {StatusCode}", datasetName, response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while creating dataset '{DatasetName}'", datasetName);
                return false;
            }
        }

        public bool DeleteDataset(string datasetName)
        {
            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(30) };
                var byteArray = System.Text.Encoding.ASCII.GetBytes($"{_user}:{_password}");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                // First, drop all graphs to clear the data
                var updateUri = $"{_sparqlEndpoint}{datasetName}";
                var dropAllQuery = "DROP ALL";
                var updateContent = new StringContent($"update={Uri.EscapeDataString(dropAllQuery)}", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");

                var dropResponse = client.PostAsync(updateUri, updateContent).Result;
                if (dropResponse.IsSuccessStatusCode)
                {
                    _logger.LogInformation("All graphs dropped from dataset '{DatasetName}'.", datasetName);
                }
                else
                {
                    _logger.LogWarning("Failed to drop graphs from dataset '{DatasetName}'. Status code: {StatusCode}", datasetName, dropResponse.StatusCode);
                }

                // Then delete the dataset from Fuseki
                var requestUri = $"{_sparqlEndpoint}$/datasets/{datasetName}";
                var response = client.DeleteAsync(requestUri).Result;

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Dataset '{DatasetName}' deleted from Fuseki server.", datasetName);
                    return true;
                }
                else
                {
                    _logger.LogError("Failed to delete dataset '{DatasetName}'. Status code: {StatusCode}", datasetName, response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while deleting dataset '{DatasetName}'", datasetName);
                return false;
            }
        }

        public bool AddData(string datasetName, List<string> ttlFiles)
        {
            try
            {
                foreach (var ttlFile in ttlFiles)
                {
                    if (!File.Exists(ttlFile))
                    {
                        _logger.LogWarning("TTL file '{TTLFile}' not found. Skipping.", ttlFile);
                        continue;
                    }

                    using var handler = new HttpClientHandler
                    {
                        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                    };
                    using var client = new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(30) };

                    // Use Basic Authentication header
                    var byteArray = System.Text.Encoding.ASCII.GetBytes($"{_user}:{_password}");
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                    var requestUri = $"{_sparqlEndpoint}{datasetName}/";

                    using var content = new MultipartFormDataContent();

                    using var fileStream = File.OpenRead(ttlFile);
                    var streamContent = new StreamContent(fileStream);
                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                    content.Add(streamContent, "files[]", Path.GetFileName(ttlFile));

                    var response = client.PostAsync(requestUri, content).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Data from file '{TTLFile}' added to dataset '{DatasetName}' successfully.", ttlFile, datasetName);
                    }
                    else
                    {
                        var responseBody = response.Content.ReadAsStringAsync().Result;
                        _logger.LogError("Failed to add data from file '{TTLFile}' to dataset '{DatasetName}'. Status code: {StatusCode}, Response: {Response}", ttlFile, datasetName, response.StatusCode, responseBody);
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while adding data to dataset '{DatasetName}'.", datasetName);
                return false;
            }
        }

        public async Task<bool> ExecuteInsertAsync(string datasetName, string query)
        {
            try
            {
                var updateUri = $"{_sparqlEndpoint}{datasetName}";

                using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(60) };

                // Add basic authentication
                var byteArray = System.Text.Encoding.ASCII.GetBytes($"{_user}:{_password}");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                // Send as SPARQL Update
                var content = new StringContent($"update={Uri.EscapeDataString(query)}", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");

                var response = await client.PostAsync(updateUri, content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Insert executed successfully.");
                    return true;
                }
                else
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Insert failed. Status code: {response.StatusCode}, Response: {responseBody}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while insetting into dataset '{DatasetName}'", datasetName);
                throw;
            }
        }

        public async Task<string> ExecuteSHACLAsync(string datasetName, string graph, string shacl)
        {
            try
            {
                var shaclUri = $"{_sparqlEndpoint}{datasetName}/shacl?graph={Uri.EscapeDataString(graph)}";

                using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(30) };

                // Add basic authentication
                var byteArray = System.Text.Encoding.ASCII.GetBytes($"{_user}:{_password}");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                // Send as SPARQL Update
                var content = new StringContent(shacl, System.Text.Encoding.UTF8, "text/turtle");

                var response = await client.PostAsync(shaclUri, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Validation executed successfully.");
                    return responseBody;
                }
                else
                {
                    throw new Exception($"Validation failed. Status code: {response.StatusCode}, Response: {responseBody}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while validating dataset '{DatasetName}'", datasetName);
                throw;
            }
        }
    }
}
