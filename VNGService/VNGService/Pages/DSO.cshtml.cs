using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RDF;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using VNGService.Models;
using VNGService.Services;

namespace VNGService.Pages
{
    public class DSOModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DSOModel> _logger;

        public DSOModel(IConfiguration configuration, ILogger<DSOModel> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnGetAreas(double eastings, double northings)
        {
            var areas = await GetZoningAreas(eastings, northings);
            if (areas?.Count > 0)
            {
                return new JsonResult(areas);
            }

            areas = await GetRegulationTextAnnotations(eastings, northings);

            return new JsonResult(areas);
        }

        public async Task<List<Area>> GetRegulationTextAnnotations(double eastings, double northings)
        {
            List<Area> areas = new();

            var isLinuxPlatform = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            var FileStorage = isLinuxPlatform ? "FileStorageLinux" : "FileStorage";

            var modelsDir = _configuration[$"{FileStorage}:ModelsDir"];
            if (string.IsNullOrEmpty(modelsDir))
            {
                _logger.LogError("Models path is not configured.");
                return areas;
            }

            DsoOmgevingsdocumentenService dsoService = new(_configuration, _logger);

            long owlModel = Engine.CreateModel();
            Engine.SetOverrideFileIO(owlModel, Engine.flagbit0 + Engine.flagbit4, Engine.flagbit0 + Engine.flagbit4);

            // Unicode support for property values
            Engine.SetCharacterSerialization(owlModel, 0, 0, false);
            _ = Engine.CreateProperty(
                owlModel,
                DATATYPEPROPERTY_TYPE_WCHAR_T_ARRAY,
                "tag");

            try
            {
                var regulationIds = await dsoService.FindRegulationsIdByPointAsync(eastings, northings);
                if ((regulationIds == null) || (regulationIds.Count == 0))
                {
                    return new List<Area>();
                }

                foreach (var regulationId in regulationIds)
                {
                    var regulationTextAnnotations = await dsoService.GetRegulationTextAnnotationsAsync(
                        regulationId,
                        eastings,
                        northings,                        
                        groupFilter: "wonen", //#test
                        typeFilter: null);

                    foreach (var item in regulationTextAnnotations)
                    {
                        if (item.Geometrie == null)
                        {
                            continue;
                        }

                        var name = item.Naam ?? "$";
                        name += " - ";
                        name += item.Groep?.Waarde ?? "$";
                        name += " - ";
                        name += item.Type?.Waarde ?? "$";

                        long geometryInstance = RetrieveGeometry(
                            owlModel,
                            name,
                            item.Geometrie);

                        if (geometryInstance != 0)
                        {
                            using var memoryStream = new MemoryStream();
                            Engine.SaveInstanceTreeS(geometryInstance, (value, size) =>
                            {
                                byte[] buffer = new byte[size];
                                Marshal.Copy(value, buffer, 0, (int)size);

                                memoryStream.Write(buffer, 0, (int)size);
                            }, 20000);

                            areas.Add(new Area
                            {
                                Id = item.Identificatie ?? "$",
                                Name = item.Naam ?? "$",
                                Group = item.Groep?.Waarde ?? "$",
                                Type = item.Type?.Waarde ?? "$",
                                Geometry = System.Text.Encoding.UTF8.GetString(memoryStream.GetBuffer(), 0, (int)memoryStream.Length)
                            });
                        }
                    }
                }

                return areas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating building geometries");
                throw;
            }
            finally
            {
                Engine.CloseModel(owlModel);
            }
        }

        public async Task<IActionResult> OnGetZoningAreas(double eastings, double northings)
        {
            var zoningAreas = await GetZoningAreas(eastings, northings);

            return new JsonResult(zoningAreas);
        }

        public async Task<List<Area>> GetZoningAreas(double eastings, double northings)
        {
            long owlModel = Engine.CreateModel(); 
            Engine.SetOverrideFileIO(owlModel, Engine.flagbit0 + Engine.flagbit4, Engine.flagbit0 + Engine.flagbit4);

            // Unicode support for property values
            Engine.SetCharacterSerialization(owlModel, 0, 0, false);
            _ = Engine.CreateProperty(
                owlModel,
                DATATYPEPROPERTY_TYPE_WCHAR_T_ARRAY,
                "tag");

            try
            {
                SettingsManager settingsManager = new SettingsManager(_configuration, _logger);
                var apiSettings = settingsManager.LoadAPISettings();
                var APIKey = apiSettings.ContainsKey("sp") ? apiSettings["sp"] : throw new InvalidOperationException("Spatial Planning API key is not configured.");

                using var httpClient = new HttpClient
                {
                    BaseAddress = new Uri("https://ruimte.omgevingswet.overheid.nl/ruimtelijke-plannen/api/opvragen/v4/")
                };

                httpClient.DefaultRequestHeaders.Add("X-API-Key", APIKey);
                httpClient.DefaultRequestHeaders.Add("Accept", "application/hal+json");
                httpClient.DefaultRequestHeaders.Add("Content-Crs", "epsg:28992");
                httpClient.DefaultRequestHeaders.Add("Accept-Crs", "epsg:28992");

                var requestBody = new
                {
                    _geo = new
                    {
                        contains = new
                        {
                            type = "Point",
                            coordinates = new[] { Math.Round(eastings, 3), Math.Round(northings, 3) }
                        }
                    }
                };

                var jsonContent = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                });

                List<Plan> plans = new();

                var firstPageResult = await FetchFirstPage(httpClient, jsonContent);
                if (firstPageResult?.Embedded != null)
                {
                    for (int i = 0; i < firstPageResult.Embedded.Plannen.Count; i++)
                    {
                        var plan = firstPageResult.Embedded.Plannen[i];
                        if (plan != null)
                        {
                            plans.Add(plan);
                        }
                    }

                    var nextPage = firstPageResult.Links.Next;
                    while (nextPage != null)
                    {
                        var nextPageResult = await FetchNextPage(httpClient, jsonContent, nextPage.Href);
                        if (nextPageResult?.Embedded != null)
                        {
                            for (int j = 0; j < nextPageResult.Embedded.Plannen.Count; j++)
                            {
                                var plan = nextPageResult.Embedded.Plannen[j];
                                if (plan != null)
                                {
                                    plans.Add(plan);
                                }
                            }
                        }

                        nextPage = nextPageResult?.Links?.Next;
                    }

                    _logger.LogInformation($"Retrieved {plans.Count} plans.");
                }

                if (plans.Count == 0)
                {
                    _logger.LogInformation("No plans found for the given coordinates.");
                }

                return await CreateZoningAreas(owlModel, plans);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating building geometries");
            }
            finally
            {
                Engine.CloseModel(owlModel);
            }

            return new List<Area>();
        }

        private async Task<PlannenResponse?> FetchFirstPage(
           HttpClient httpClient,
           string jsonContent)
        {
            var queryParams = new List<KeyValuePair<string, string>>
            {
                new("page", "1")
            };

            var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
            var requestUrl = $"plannen/_zoek?{queryString}";
            using var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(requestUrl, content);

            Console.WriteLine($"Status Code: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                //#test
                //var responseContent = await response.Content.ReadAsStringAsync();
                var result = await response.Content.ReadFromJsonAsync<PlannenResponse>();

                return result;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Error: {response.StatusCode}\n{errorContent}");

                return null;
            }
        }

        private async Task<PlannenResponse?> FetchNextPage(
            HttpClient httpClient,
            string jsonContent,
            string requestUrl)
        {
            using var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(requestUrl, content);

            Console.WriteLine($"Status Code: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                //#test
                //var responseContent = await response.Content.ReadAsStringAsync();
                var result = await response.Content.ReadFromJsonAsync<PlannenResponse>();

                return result;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Error: {response.StatusCode}\n{errorContent}");

                return null;
            }
        }

        private async Task<List<Area>> CreateZoningAreas(long owlModel, List<Plan> plans)
        {
            if (plans.Count == 0)
            {
                return new List<Area>();
            }

            SettingsManager settingsManager = new SettingsManager(_configuration, _logger);
            var apiSettings = settingsManager.LoadAPISettings();
            var APIKey = apiSettings.ContainsKey("sp") ? apiSettings["sp"] : throw new InvalidOperationException("Spatial Planning API key is not configured.");

            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://ruimte.omgevingswet.overheid.nl/ruimtelijke-plannen/api/opvragen/v4/")
            };

            httpClient.DefaultRequestHeaders.Add("X-API-Key", APIKey);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/hal+json");
            httpClient.DefaultRequestHeaders.Add("Content-Crs", "epsg:28992");
            httpClient.DefaultRequestHeaders.Add("Accept-Crs", "epsg:28992");

            var queryParams = new List<KeyValuePair<string, string>>
            {
                new("expand", "geometrie")
            };

            List<Area> zoningAreas = new();
            for (int i = 0; i < plans.Count; i++)
            {
                var plan = plans[i];

                var planZoningAreas = await CreatePlanZoningAreas(
                    httpClient, queryParams, owlModel, plan,
                    groupFilter: "wonen", //#test
                    typeFilter: null);
                if (planZoningAreas.Count > 0)
                {
                    zoningAreas.AddRange(planZoningAreas);
                }
            }

            return zoningAreas;
        }

        private async Task<List<Area>> CreatePlanZoningAreas(
            HttpClient httpClient,
            List<KeyValuePair<string, string>> queryParams,
            long owlModel,
            Plan plan, 
            string? groupFilter,
            string? typeFilter)
        {
            List<Area> zoningAreas = new();

            var isLinuxPlatform = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            var FileStorage = isLinuxPlatform ? "FileStorageLinux" : "FileStorage";

            var modelsDir = _configuration[$"{FileStorage}:ModelsDir"];
            if (string.IsNullOrEmpty(modelsDir))
            {
                _logger.LogError("Models path is not configured.");
                return zoningAreas;
            }

            var planDetails = await FetchZoningAreas(httpClient, queryParams, plan.Id);
            if (planDetails?.Embedded.Bestemmingsvlakken.Count > 0)
            {
                long verwijzingNaarTekstProperty = Engine.CreateProperty(
                   owlModel,
                   DATATYPEPROPERTY_TYPE_WCHAR_T_ARRAY,
                   "VerwijzingNaarTekst");

                long owlDefaultMaterialInstance = Material.GetPlanDefaultMaterial(owlModel);

                for (int j = 0; j < planDetails.Embedded.Bestemmingsvlakken.Count; j++)
                {
                    if (planDetails.Embedded.Bestemmingsvlakken[j].Geometrie != null)
                    {
                        bool match = true;
                        if (!string.IsNullOrEmpty(groupFilter))
                        {
                            match = groupFilter.Equals(planDetails.Embedded.Bestemmingsvlakken[j].Bestemmingshoofdgroep, StringComparison.OrdinalIgnoreCase);
                        }

                        if (match && !string.IsNullOrEmpty(typeFilter))
                        {
                            match = typeFilter.Equals(planDetails.Embedded.Bestemmingsvlakken[j].Type, StringComparison.OrdinalIgnoreCase);
                        }

                        if (!match)
                        {
                            continue;
                        }

                        long geometryInstance = RetrieveGeometry(
                            owlModel,
                            planDetails.Embedded.Bestemmingsvlakken[j].Naam,
                            planDetails.Embedded.Bestemmingsvlakken[j].Geometrie!,
                            owlDefaultMaterialInstance,
                            0.0);
                        if (geometryInstance != 0)
                        {
                            Debug.Assert(planDetails.Embedded.Bestemmingsvlakken[j].VerwijzingNaarTekst.Count == 1);
                            for (int k = 0; k < planDetails.Embedded.Bestemmingsvlakken[j].VerwijzingNaarTekst.Count; k++)
                            {
                                var verwijzing = planDetails.Embedded.Bestemmingsvlakken[j].VerwijzingNaarTekst[k];
                                SetStringPropertyW(geometryInstance, verwijzingNaarTekstProperty, verwijzing);
                            }

                            using var memoryStream = new MemoryStream();
                            Engine.SaveInstanceTreeS(geometryInstance, (value, size) =>
                            {
                                byte[] buffer = new byte[size];
                                Marshal.Copy(value, buffer, 0, (int)size);

                                memoryStream.Write(buffer, 0, (int)size);
                            }, 20000);

                            zoningAreas.Add(new Area
                            {
                                Id = planDetails.Embedded.Bestemmingsvlakken[j].Id,
                                Name = planDetails.Embedded.Bestemmingsvlakken[j].Naam,
                                Group = planDetails.Embedded.Bestemmingsvlakken[j].Bestemmingshoofdgroep,
                                Type = planDetails.Embedded.Bestemmingsvlakken[j].Type,
                                Geometry = System.Text.Encoding.UTF8.GetString(memoryStream.GetBuffer(), 0, (int)memoryStream.Length)
                            });
                        }
                    }
                }

                var nextPage = planDetails.Links.Next;
                while (nextPage != null)
                {
                    var nextPlanDetails = await FetchNextZoningAreas(httpClient, nextPage.Href);
                    if (nextPlanDetails?.Embedded.Bestemmingsvlakken.Count > 0)
                    {
                        for (int j = 0; j < nextPlanDetails.Embedded.Bestemmingsvlakken.Count; j++)
                        {
                            if (nextPlanDetails.Embedded.Bestemmingsvlakken[j].Geometrie != null)
                            {
                                bool match = true;
                                if (!string.IsNullOrEmpty(groupFilter))
                                {
                                    match = groupFilter.Equals(nextPlanDetails.Embedded.Bestemmingsvlakken[j].Bestemmingshoofdgroep, StringComparison.OrdinalIgnoreCase);
                                }

                                if (match && !string.IsNullOrEmpty(typeFilter))
                                {
                                    match = typeFilter.Equals(nextPlanDetails.Embedded.Bestemmingsvlakken[j].Type, StringComparison.OrdinalIgnoreCase);
                                }

                                if (!match)
                                {
                                    continue;
                                }

                                long geometryInstance = RetrieveGeometry(
                                    owlModel,
                                    nextPlanDetails.Embedded.Bestemmingsvlakken[j].Naam,
                                    nextPlanDetails.Embedded.Bestemmingsvlakken[j].Geometrie!,
                                    owlDefaultMaterialInstance,
                                    0.0);
                                if (geometryInstance != 0)
                                {
                                    for (int k = 0; k < nextPlanDetails.Embedded.Bestemmingsvlakken[j].VerwijzingNaarTekst.Count; k++)
                                    {
                                        var verwijzing = nextPlanDetails.Embedded.Bestemmingsvlakken[j].VerwijzingNaarTekst[k];
                                        SetStringPropertyW(geometryInstance, verwijzingNaarTekstProperty, verwijzing);
                                    }

                                    using var memoryStream = new MemoryStream();
                                    Engine.SaveInstanceTreeS(geometryInstance, (value, size) =>
                                    {
                                        byte[] buffer = new byte[size];
                                        Marshal.Copy(value, buffer, 0, (int)size);

                                        memoryStream.Write(buffer, 0, (int)size);
                                    }, 20000);

                                    zoningAreas.Add(new Area
                                    {
                                        Id = nextPlanDetails.Embedded.Bestemmingsvlakken[j].Id,
                                        Name = nextPlanDetails.Embedded.Bestemmingsvlakken[j].Naam,
                                        Group = nextPlanDetails.Embedded.Bestemmingsvlakken[j].Bestemmingshoofdgroep,
                                        Type = nextPlanDetails.Embedded.Bestemmingsvlakken[j].Type,
                                        Geometry = System.Text.Encoding.UTF8.GetString(memoryStream.GetBuffer(), 0, (int)memoryStream.Length)
                                    });
                                }
                            }
                        }
                    }

                    nextPage = nextPlanDetails?.Links?.Next;
                }
            }

            return zoningAreas;
        }

        private async Task<BestemmingsvlakkenResponse?> FetchZoningAreas(
            HttpClient httpClient,
            List<KeyValuePair<string, string>> queryParams,
            string planId)
        {
            var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
            var requestUrl = $"plannen/{planId}/bestemmingsvlakken?{queryString}";
            var response = await httpClient.GetAsync(requestUrl);

            if (response.IsSuccessStatusCode)
            {
                //#test: log raw response content for debugging
                //var responseContent = await response.Content.ReadAsStringAsync();
                var result = await response.Content.ReadFromJsonAsync<BestemmingsvlakkenResponse>();

                return result;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Error: {response.StatusCode}\n{errorContent}");

                return null;
            }
        }

        private async Task<BestemmingsvlakkenResponse?> FetchNextZoningAreas(
            HttpClient httpClient,
            string requestUrl)
        {
            var response = await httpClient.GetAsync(requestUrl);

            if (response.IsSuccessStatusCode)
            {
                //#test: log raw response content for debugging
                //var responseContent = await response.Content.ReadAsStringAsync();
                var result = await response.Content.ReadFromJsonAsync<BestemmingsvlakkenResponse>();

                return result;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Error: {response.StatusCode}\n{errorContent}");

                return null;
            }
        }

        long RetrieveGeometry(long owlModel, string name, GeoJsonGeometry geometry, long owlMaterialInstance = 0, double z = 0.0)
        {
            switch (geometry.Type)
            {
                case "Polygon":
                    if (geometry.PolygonCoordinates != null)
                    {
                        var polygon = geometry.PolygonCoordinates;

                        long owlBoundaryRepresentationInstance = Engine.CreateInstance(Engine.GetClassByName(owlModel, "BoundaryRepresentation"));
                        SetStringPropertyW(
                            owlBoundaryRepresentationInstance,
                            Engine.GetPropertyByName(owlModel, "tag"),
                            name);

                        int index = 0;
                        List<long> indices = new();
                        List<double> vertices = new();
                        for (int j = 0; j < polygon.Length; j++)
                        {
                            var ring = polygon[j];
                            for (int k = 0; k < ring.Length; k++)
                            {
                                var point = ring[k];
                                vertices.Add(point[0]);
                                vertices.Add(point[1]);
                                vertices.Add(z); // Z coordinate
                                indices.Add(index++);
                            }

                            if (j == 0)
                            {
                                indices.Add(-1); // Mark end of exterior ring
                            }
                            else
                            {
                                indices.Add(-2); // Mark end of interior ring
                            }
                        }

                        Engine.SetDatatypeProperty(
                            owlBoundaryRepresentationInstance,
                            Engine.GetPropertyByName(owlModel, "indices"),
                            indices.ToArray(),
                            indices.Count);

                        Engine.SetDatatypeProperty(
                            owlBoundaryRepresentationInstance,
                            Engine.GetPropertyByName(owlModel, "vertices"),
                            vertices.ToArray(),
                            vertices.Count);

                        if (owlMaterialInstance != 0)
                        {
                            Engine.SetObjectProperty(
                                owlBoundaryRepresentationInstance,
                                Engine.GetPropertyByName(owlModel, "material"),
                                owlMaterialInstance);
                        }

                        return owlBoundaryRepresentationInstance;
                    }
                    break;

                case "MultiPolygon":
                    if (geometry.MultiPolygonCoordinates != null)
                    {
                        List<long> polygonInstances = new();
                        for (int i = 0; i < geometry.MultiPolygonCoordinates.Length; i++)
                        {
                            var polygon = geometry.MultiPolygonCoordinates[i];

                            long owlBoundaryRepresentationInstance = Engine.CreateInstance(Engine.GetClassByName(owlModel, "BoundaryRepresentation"));
                            polygonInstances.Add(owlBoundaryRepresentationInstance);

                            int index = 0;
                            List<long> indices = new();
                            List<double> vertices = new();
                            for (int j = 0; j < polygon.Length; j++)
                            {
                                var ring = polygon[j];
                                for (int k = 0; k < ring.Length; k++)
                                {
                                    var point = ring[k];
                                    vertices.Add(point[0]);
                                    vertices.Add(point[1]);
                                    vertices.Add(z); // Z coordinate
                                    indices.Add(index++);
                                }

                                if (j == 0)
                                {
                                    indices.Add(-1); // Mark end of exterior ring
                                }
                                else
                                {
                                    indices.Add(-2); // Mark end of interior ring
                                }
                            }

                            Engine.SetDatatypeProperty(
                                owlBoundaryRepresentationInstance,
                                Engine.GetPropertyByName(owlModel, "indices"),
                                indices.ToArray(),
                                indices.Count);

                            Engine.SetDatatypeProperty(
                                owlBoundaryRepresentationInstance,
                                Engine.GetPropertyByName(owlModel, "vertices"),
                                vertices.ToArray(),
                                vertices.Count);

                            if (owlMaterialInstance != 0)
                            {
                                Engine.SetObjectProperty(
                                    owlBoundaryRepresentationInstance,
                                    Engine.GetPropertyByName(owlModel, "material"),
                                    owlMaterialInstance);
                            }
                        }

                        long owlCollectionInstance = Engine.CreateInstance(Engine.GetClassByName(owlModel, "Collection"));
                        SetStringPropertyW(
                            owlCollectionInstance,
                            Engine.GetPropertyByName(owlModel, "tag"),
                            name);
                        Engine.SetObjectProperty(
                                owlCollectionInstance,
                                Engine.GetPropertyByName(owlModel, "objects"),
                                polygonInstances.ToArray(),
                                polygonInstances.Count);

                        return owlCollectionInstance;
                    }
                    break;

                case "Point":
                    if (geometry.PointCoordinates != null)
                    {
                        long owlPointInstance = Engine.CreateInstance(Engine.GetClassByName(owlModel, "Point3D"));
                        SetStringPropertyW(
                            owlPointInstance,
                            Engine.GetPropertyByName(owlModel, "tag"),
                            name);

                        double value = geometry.PointCoordinates[0];
                        Engine.SetDatatypeProperty(
                            owlPointInstance,
                            Engine.GetPropertyByName(owlModel, "x"),
                            ref value,
                            1);
                        value = geometry.PointCoordinates[1];
                        Engine.SetDatatypeProperty(
                            owlPointInstance,
                            Engine.GetPropertyByName(owlModel, "y"),
                            ref value,
                            1);
                        value = z;
                        Engine.SetDatatypeProperty(
                            owlPointInstance,
                            Engine.GetPropertyByName(owlModel, "z"),
                            ref value,
                            1);

                        return owlPointInstance;
                    }
                    break;

                case "LineString":
                    if (geometry.LineStringCoordinates != null)
                    {
                        List<double> vertices = new();
                        for (int i = 0; i < geometry.LineStringCoordinates.Length; i++)
                        {
                            var point = geometry.LineStringCoordinates[i];
                            vertices.Add(point[0]);
                            vertices.Add(point[1]);
                            vertices.Add(z); // Z coordinate
                        }

                        long owlPolyLineInstance = Engine.CreateInstance(Engine.GetClassByName(owlModel, "PolyLine3D"));
                        SetStringPropertyW(
                            owlPolyLineInstance,
                            Engine.GetPropertyByName(owlModel, "tag"),
                            name);

                        Engine.SetDatatypeProperty(
                            owlPolyLineInstance,
                            Engine.GetPropertyByName(owlModel, "coordinates"),
                            vertices.ToArray(),
                            vertices.Count);

                        return owlPolyLineInstance;
                    }
                    break;

                case "MultiPoint":
                case "MultiLineString":
                    Debug.Assert(false, $"Unexpected geometry type: {geometry.Type}");
                    break;

                default:
                    Debug.Assert(false, $"Unexpected geometry type: {geometry.Type}");
                    break;
            }

            return 0;
        }

        #region OWL API Interop
        const long DATATYPEPROPERTY_TYPE_WCHAR_T_ARRAY = 8;

        [DllImport(Engine.enginedll, EntryPoint = "SetDatatypeProperty")]
        public static extern Int64 SetDatatypeProperty(Int64 owlInstance, Int64 owlDatatypeProperty, IntPtr values, Int64 card);

        private static void SetStringPropertyW(long owlInstance, long property, string value)
        {
            IntPtr wcharPtr = Marshal.StringToHGlobalUni(value);

            try
            {
                IntPtr ptrToPtr = Marshal.AllocHGlobal(IntPtr.Size);

                try
                {
                    Marshal.WriteIntPtr(ptrToPtr, wcharPtr);

                    SetDatatypeProperty(owlInstance, property, ptrToPtr, 1);
                }
                finally
                {
                    Marshal.FreeHGlobal(ptrToPtr);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(wcharPtr);
            }
        }
        #endregion // OWL API Interop
    }
}
