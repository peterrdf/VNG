using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using VNGService.Models;

namespace VNGService.Services;

public enum BgtLayer
{
    Wegdeel,            // Road surface polygons (streets, cycle paths, pavements)
    OpenbareRuimte,     // Public space polygons
    Waterdeel,          // Waterways
    Pand,               // Building footprints
    OverigBouwwerk      // Other structures (bridges, tunnels, etc.)
}

public sealed record BgtWegdeel(
    [property: JsonPropertyName("identificatie")] string? Identificatie,
    [property: JsonPropertyName("plus-fysiekVoorkomen")] string? Function,
    [property: JsonPropertyName("relatieveHoogteligging")] int? RelatieveHoogteligging,
    [property: JsonPropertyName("opTalud")] bool? OpTalud,
    [property: JsonPropertyName("tijdstipRegistratie")] DateTime? TijdstipRegistratie,
    [property: JsonPropertyName("eindRegistratie")] DateTime? EindRegistratie);

file sealed record LocatieserverResponse(
    [property: JsonPropertyName("response")] LocatieserverDocs? Response);

file sealed record LocatieserverDocs(
    [property: JsonPropertyName("docs")] List<LocatieserverDoc>? Docs);

file sealed record LocatieserverDoc(
    [property: JsonPropertyName("straatnaam")] string? Straatnaam,
    [property: JsonPropertyName("woonplaatsnaam")] string? Woonplaatsnaam,
    [property: JsonPropertyName("gemeentenaam")] string? Gemeentenaam);

public sealed class PdokBgtService : IDisposable
{
    private readonly ILogger _logger;

    // OGC API Features; URL pattern is /collections/{id}/items
    private const string OgcBaseUrl = "https://api.pdok.nl/lv/bgt/ogc/v1";

    // PDOK Locatieserver – returns BAG straatnaam/woonplaats for a WGS84 coordinate
    private const string LocatieserverReverseUrl =
        "https://api.pdok.nl/bzk/locatieserver/search/v3_1/reverse";

    // Collection IDs as listed by /collections?f=json
    private static readonly Dictionary<BgtLayer, string> CollectionIds = new()
    {
        [BgtLayer.Wegdeel] = "wegdeel",
        [BgtLayer.OpenbareRuimte] = "openbareruimte",
        [BgtLayer.Waterdeel] = "waterdeel",
        [BgtLayer.Pand] = "pand",
        [BgtLayer.OverigBouwwerk] = "overigbouwwerk"
    };

    private readonly HttpClient _httpClient;

    public PdokBgtService(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _httpClient = new HttpClient();
    }

    public async Task<JsonDocument?> GetCollectionsAsync(CancellationToken ct = default)
    {
        var url = $"{OgcBaseUrl}/collections?f=jsonfg";
        var response = await _httpClient.GetAsync(url, ct);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError($"PDOK BGT OGC Error: {response.StatusCode}\n{errorContent}");
            return null;
        }

        var json = await response.Content.ReadAsStringAsync(ct);
        return JsonDocument.Parse(json);
    }

    /// <summary>
    /// Returns the street name (straatnaam) nearest to the given WGS84 coordinate,
    /// using the PDOK Locatieserver reverse geocoder.
    /// </summary>
    public async Task<string?> GetStraatnaamAsync(
        GeoPoint point,
        CancellationToken ct = default)
    {
        var inv = System.Globalization.CultureInfo.InvariantCulture;
        var url = $"{LocatieserverReverseUrl}" +
                  $"?lat={point.Lat.ToString(inv)}" +
                  $"&lon={point.Lon.ToString(inv)}" +
                  $"&type=weg" +
                  $"&rows=1" +
                  $"&fl=straatnaam,woonplaatsnaam,gemeentenaam";

        var response = await _httpClient.GetAsync(url, ct);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError($"PDOK Locatieserver Error: {response.StatusCode}\n{errorContent}");
            return null;
        }

        var result = await response.Content
            .ReadFromJsonAsync<LocatieserverResponse>(cancellationToken: ct);

        return result?.Response?.Docs?.FirstOrDefault()?.Straatnaam;
    }

    // OGC API Features BBox is WGS84 (EPSG:4326): minLon, minLat, maxLon, maxLat
    public async Task<PdokFeatureCollection?> GetFeaturesByBoundingBoxAsync(
        BgtLayer layer,
        GeoPoint point1,
        GeoPoint point2,
        int limit = 100,
        CancellationToken ct = default)
    {
        var collectionId = CollectionIds[layer];
        var bbox = BuildBbox(point1, point2);
        var url = BuildOgcUrl(collectionId, bbox, limit);

        List<PdokFeature> allFeatures = [];
        PdokFeatureCollection? lastCollection = null;

        while (url is not null)
        {
            var response = await _httpClient.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError($"PDOK BGT OGC Error: {response.StatusCode}\n{errorContent}");
                return null;
            }

            //#test
            //var responseContent = await response.Content.ReadAsStringAsync(ct);
            
            var collection = await response.Content.ReadFromJsonAsync<PdokFeatureCollection>(cancellationToken: ct);
            if (collection is null)
                break;

            lastCollection = collection;
            allFeatures.AddRange(collection.Features);

            url = collection.Links?
                .FirstOrDefault(l => l.Rel == "next")?.Href;
        }

        return lastCollection is null ? null : lastCollection with { Features = allFeatures };
    }

    // OGC API Features BBox is WGS84 (EPSG:4326): minLon, minLat, maxLon, maxLat
    private static string BuildBbox(GeoPoint point1, GeoPoint point2)
    {
        var inv = System.Globalization.CultureInfo.InvariantCulture;
        double minX = Math.Min(point1.Lon, point2.Lon);
        double minY = Math.Min(point1.Lat, point2.Lat);
        double maxX = Math.Max(point1.Lon, point2.Lon);
        double maxY = Math.Max(point1.Lat, point2.Lat);
        return $"{minX.ToString(inv)},{minY.ToString(inv)},{maxX.ToString(inv)},{maxY.ToString(inv)}";
    }

    private static string BuildOgcUrl(string collectionId, string bbox, int limit) =>
        $"{OgcBaseUrl}/collections/{collectionId}/items" +
        $"?f=json" +
        $"&bbox={bbox}" +
        $"&crs=http://www.opengis.net/def/crs/EPSG/0/28992" +
        $"&limit={limit}";

    public void Dispose() => _httpClient.Dispose();
}