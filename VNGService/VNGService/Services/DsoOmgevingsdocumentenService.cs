using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using VNGService.Models;

namespace VNGService.Services
{
    public class DsoOmgevingsdocumentenService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        /* 
         * https://developer.omgevingswet.overheid.nl/api-register/api/omgevingsdocument-presenteren/
         * Presenting an environmental document
         * 
         * The Presenting Environmental Documents API is intended for the object-oriented disclosure of environmental documents 
         * within the Digital System of the Environment and Planning Act (DSO).
         * */
        private const string PresenterenUrl = "https://service.omgevingswet.overheid.nl/publiek/omgevingsdocumenten/api/presenteren/v8/";

        /* 
         * https://developer.omgevingswet.overheid.nl/api-register/api/omgevingsdocument-toepasbaaropvragen/
         * Environmental document Applicable request
         * 
         * This specific API is based on CIM-OW, but focused on the information needs for the execution of Applicable Rules. 
         * To that end, the API unlocks 2 resources: Activities and Locations. 
         * In addition to the CIM-OW attributes, the resource 'Activities' contains the Organisation Identification Number (OIN) of the competent
         * authority and the administrative layer to which the competent authority belongs.
         * */
        private const string ToepasbaarUrl = "https://service.omgevingswet.overheid.nl/publiek/omgevingsdocumenten/api/toepasbaaropvragen/v7/";

        /* 
         * https://developer.omgevingswet.overheid.nl/api-register/api/omgevingsdocument-geometrieopvragen/
         * Requesting geometry environmental documents
         * 
         * The Geometry Query Environment Documents API provides users with the ability to query all geometries known in the DSO. 
         * Using the unique geometry identification, a call to this API results in a GeoJSON response 
         * containing the coordinates that describe the geometry.
         * */
        private const string GeometrieUrl = "https://service.omgevingswet.overheid.nl/publiek/omgevingsdocumenten/api/geometrieopvragen/v1/";
        
        private const string CrsParam = "crs=http%3A%2F%2Fwww.opengis.net%2Fdef%2Fcrs%2FEPSG%2F0%2F28992";
        private const string ContentCrs = "http://www.opengis.net/def/crs/EPSG/0/28992";

        private readonly HttpClient _presenterenClient;
        private readonly HttpClient _toepasbaarClient;
        private readonly HttpClient _geometrieClient;

        public DsoOmgevingsdocumentenService(IConfiguration configuration, ILogger logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            SettingsManager settingsManager = new SettingsManager(_configuration, _logger);
            var apiSettings = settingsManager.LoadAPISettings();
            var APIKey = apiSettings.ContainsKey("dso") ? apiSettings["dso"] : throw new InvalidOperationException("DSO API key is not configured.");

            _presenterenClient = new HttpClient { BaseAddress = new Uri(PresenterenUrl) };
            _presenterenClient.DefaultRequestHeaders.Add("X-API-Key", APIKey);
            _presenterenClient.DefaultRequestHeaders.Add("Accept", "application/hal+json");

            _toepasbaarClient = new HttpClient { BaseAddress = new Uri(ToepasbaarUrl) };
            _toepasbaarClient.DefaultRequestHeaders.Add("X-API-Key", APIKey);
            _toepasbaarClient.DefaultRequestHeaders.Add("Accept", "application/hal+json");
            _toepasbaarClient.DefaultRequestHeaders.Add("Accept-Crs", "epsg:28992");

            _geometrieClient = new HttpClient { BaseAddress = new Uri(GeometrieUrl) };
            _geometrieClient.DefaultRequestHeaders.Add("X-API-Key", APIKey);
            _geometrieClient.DefaultRequestHeaders.Add("Accept", "application/hal+json");
        }

        public async Task<List<string>?> FindRegulationsIdByPointAsync(double pointX, double pointY)
        {
            var path = $"regelingen/_zoek";
            var body = JsonSerializer.Serialize(new
            {
                typeBevoegdGezag = new[] { "gemeente" },
                geometrie = new { type = "Point", coordinates = new[] { (int)pointX, (int)pointY } }
            });

            using var request = new HttpRequestMessage(HttpMethod.Post, path);
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            request.Headers.Add("Content-Crs", ContentCrs);

            var response = await _presenterenClient.SendAsync(request);

            /*
            * https://developer.omgevingswet.overheid.nl/api-register/api/omgevingsdocument-presenteren/
            * Throttling: This API has a query limit of 200 per second.
            */
            await Task.Delay(50);

            if (response.IsSuccessStatusCode)
            {
                //#test
                //var responseText = await response.Content.ReadAsStringAsync();
                var result = await response.Content.ReadFromJsonAsync<DsoRegelingenZoekResponse>();
                if ((result == null) || (result.AllRegelingen == null) || !result.AllRegelingen.Any())
                {
                    return new List<string>();
                }

                return result.AllRegelingen.Where(r => r.BFFId != null).Select(r => r.BFFId!).ToList();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error: {response.StatusCode}\n{errorContent}");
            }
        }

        public async Task<List<DsoGebiedsaanwijzing>> GetRuleTextAnnotationsAsync(
            string regelingId,
            string? groupFilter,
            string? typeFilter)
        {
            var annotations = await FetchRuleTextAnnotationsAsync(regelingId);
            if ((annotations == null) || (annotations.Gebiedsaanwijzingen == null))
            {
                return new List<DsoGebiedsaanwijzing>();
            }

            var results = new List<DsoGebiedsaanwijzing>();
            foreach (var item in annotations.Gebiedsaanwijzingen)
            {
                if ((item.LocatieRefs == null) ||
                    !MatchesGroup(item, groupFilter) ||
                    !MatchesType(item, typeFilter))
                {
                    continue;
                }

                foreach (var locRef in item.LocatieRefs)
                {
                    var geometries = await FetchGeometryByLocationIdAsync(locRef);
                    if (geometries == null || !geometries.Any())
                    {
                        continue;
                    }

                    foreach (var geometrie in geometries)
                    {
                        var gebiedsaanwijzingItem = new DsoGebiedsaanwijzing
                        {
                            Identificatie = item.Identificatie,
                            Naam = item.Naam ?? item.Identificatie,
                            Groep = item.Groep,
                            Type = item.Type,
                            Geometrie = geometrie
                        };
                        results.Add(gebiedsaanwijzingItem);
                    }
                }
            }

            return results;
        }

        private async Task<DsoRegeltekstAnnotatiesResponse?> FetchRuleTextAnnotationsAsync(string regelingId)
        {
            var url = $"regelingen/{Uri.EscapeDataString(regelingId)}/regeltekstannotaties";

            var response = await _presenterenClient.GetAsync(url);

            /*
            * https://developer.omgevingswet.overheid.nl/api-register/api/omgevingsdocument-presenteren/
            * Throttling: This API has a query limit of 200 per second.
            */
            await Task.Delay(50);

            if (response.IsSuccessStatusCode)
            {
                //#test
                //var responseText = await response.Content.ReadAsStringAsync();
                //File.WriteAllText("regeltekstannotaties_response.json", responseText);
                var result = await response.Content.ReadFromJsonAsync<DsoRegeltekstAnnotatiesResponse>();

                return result;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error: {response.StatusCode}\n{errorContent}");
            }
        }

        private async Task<List<GeoJsonGeometry>?> FetchGeometryByLocationIdAsync(string id)
        {
            var url = $"locaties/{id}";

            var response = await _toepasbaarClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                //#test
                //var responseText = await response.Content.ReadAsStringAsync();
                var detail = await response.Content.ReadFromJsonAsync<LocatieDetailResponse>();

                // Single Geometry
                if (detail?.GeometrieIdentificatie != null)
                {
                    var geometry = await FetchGeometryAsync(detail.GeometrieIdentificatie);
                    if (geometry != null)
                    {
                        return new List<GeoJsonGeometry> { geometry };
                    }
                }

                // Multiple Geometries
                if (detail?.Embedded?.Omvat?.Count > 0)
                {
                    var geometries = new List<GeoJsonGeometry>();
                    foreach (var item in detail.Embedded.Omvat)
                    {
                        if (item.GeometrieIdentificatie == null)
                        {
                            continue;
                        }

                        var geo = await FetchGeometryAsync(item.GeometrieIdentificatie);
                        if (geo != null)
                        {
                            geometries.Add(geo);
                        }
                    }

                    return geometries;
                }

                return null;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error: {response.StatusCode}\n{errorContent}");
            }
        }

        private async Task<GeoJsonGeometry?> FetchGeometryAsync(string geometrieIdentificatie)
        {
            var url = $"geometrieen/{Uri.EscapeDataString(geometrieIdentificatie)}?{CrsParam}";

            var response = await _geometrieClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                //#test
                //var responseText = await response.Content.ReadAsStringAsync();
                var geometry = await response.Content.ReadFromJsonAsync<GeoJsonGeometry>();

                return geometry;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error: {response.StatusCode}\n{errorContent}");
            }
        }

        private static bool MatchesGroup(DsoGebiedsaanwijzing g, string? typeFilter)
        {
            if (string.IsNullOrWhiteSpace(typeFilter))
            {
                return true;
            }

            return (g.Groep?.Waarde?.ToLowerInvariant() == typeFilter.ToLowerInvariant());
        }

        private static bool MatchesType(DsoGebiedsaanwijzing g, string? typeFilter)
        {
            if (string.IsNullOrWhiteSpace(typeFilter))
            {
                return true;
            }

            return (g.Type?.Waarde?.ToLowerInvariant() == typeFilter.ToLowerInvariant());
        }
    }
}