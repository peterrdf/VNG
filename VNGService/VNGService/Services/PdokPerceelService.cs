using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using VNGService.Models;

namespace VNGService.Services;

public enum KadastraleKaartLayer
{
    Perceel,                    // Parcels
    Bebouwing,                  // Buildings
    KadastraleGrens,            // Cadastral boundaries
    OpenbareRuimteNaam,         // Public space names (street names, etc.)
    Kadastrale_gemeente,        // Cadastral municipality boundaries
    Kadastrale_gemeente_label,  // Cadastral municipality labels
    Sectie,                     // Sections (subdivisions of cadastral municipalities)
    Sectie_label,               // Section labels
}

public sealed record PdokGeometry(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("coordinates")] JsonElement Coordinates)
{
    /// <summary>Point — [x, y]</summary>
    public double[]? AsPoint() =>
        Type == "Point" ? Coordinates.Deserialize<double[]>() : null;

    /// <summary>LineString — [[x,y], ...]</summary>
    public double[][]? AsLineString() =>
        Type == "LineString" ? Coordinates.Deserialize<double[][]>() : null;

    /// <summary>Polygon — [[[x,y], ...], ...]</summary>
    public double[][][]? AsPolygon() =>
        Type == "Polygon" ? Coordinates.Deserialize<double[][][]>() : null;

    /// <summary>MultiPolygon — [[[[x,y], ...], ...], ...]</summary>
    public double[][][][]? AsMultiPolygon() =>
        Type == "MultiPolygon" ? Coordinates.Deserialize<double[][][][]>() : null;
}

public sealed record PdokPerceel(
    [property: JsonPropertyName("identificatieNamespace")] string? IdentificatieNamespace,
    [property: JsonPropertyName("identificatieLokaalID")] string? IdentificatieLokaalID,
    [property: JsonPropertyName("beginGeldigheid")] DateTime? BeginGeldigheid,
    [property: JsonPropertyName("tijdstipRegistratie")] DateTime? TijdstipRegistratie,
    [property: JsonPropertyName("volgnummer")] int? Volgnummer,
    [property: JsonPropertyName("statusHistorieCode")] string? StatusHistorieCode,
    [property: JsonPropertyName("statusHistorieWaarde")] string? StatusHistorieWaarde,
    [property: JsonPropertyName("kadastraleGemeenteCode")] string? KadastraleGemeenteCode,
    [property: JsonPropertyName("kadastraleGemeenteWaarde")] string? KadastraleGemeenteWaarde,
    [property: JsonPropertyName("sectie")] string? Sectie,
    [property: JsonPropertyName("AKRKadastraleGemeenteCodeCode")] string? AKRKadastraleGemeenteCodeCode,
    [property: JsonPropertyName("AKRKadastraleGemeenteCodeWaarde")] string? AKRKadastraleGemeenteCodeWaarde,
    [property: JsonPropertyName("kadastraleGrootteWaarde")] double? KadastraleGrootteWaarde,
    [property: JsonPropertyName("soortGrootteCode")] string? SoortGrootteCode,
    [property: JsonPropertyName("soortGrootteWaarde")] string? SoortGrootteWaarde,
    [property: JsonPropertyName("perceelnummer")] int? Perceelnummer,
    [property: JsonPropertyName("perceelnummerRotatie")] double? PerceelnummerRotatie,
    [property: JsonPropertyName("perceelnummerVerschuivingDeltaX")] double? PerceelnummerVerschuivingDeltaX,
    [property: JsonPropertyName("perceelnummerVerschuivingDeltaY")] double? PerceelnummerVerschuivingDeltaY,
    [property: JsonPropertyName("perceelnummerPlaatscoordinaatX")] double? PerceelnummerPlaatscoordinaatX,
    [property: JsonPropertyName("perceelnummerPlaatscoordinaatY")] double? PerceelnummerPlaatscoordinaatY);

public sealed record PdokFeature(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("properties")] JsonElement Properties,
    [property: JsonPropertyName("bbox")] double[]? Bbox,
    [property: JsonPropertyName("geometry")] PdokGeometry? Geometry);

public sealed record PdokLink(
    [property: JsonPropertyName("rel")] string Rel,
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("href")] string Href);

public sealed record PdokFeatureCollection(
    [property: JsonPropertyName("numberMatched")] int NumberMatched,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("features")] List<PdokFeature> Features,
    [property: JsonPropertyName("bbox")] double[]? Bbox,
    [property: JsonPropertyName("links")] List<PdokLink>? Links);

public sealed class PdokKadastraleKaartService : IDisposable
{
    private const string WfsBaseUrl = "https://service.pdok.nl/kadaster/kadastralekaart/wfs/v5_0";

    private readonly ILogger _logger;

    private static readonly Dictionary<KadastraleKaartLayer, string> LayerTypeNames = new()
    {
        [KadastraleKaartLayer.Perceel] = "kadastralekaart:Perceel",
        [KadastraleKaartLayer.Bebouwing] = "kadastralekaart:Bebouwing",
        [KadastraleKaartLayer.KadastraleGrens] = "kadastralekaart:KadastraleGrens",
        [KadastraleKaartLayer.OpenbareRuimteNaam] = "kadastralekaart:OpenbareRuimteNaam",
        [KadastraleKaartLayer.Kadastrale_gemeente] = "kadastralekaart:Kadastrale_gemeente",
        [KadastraleKaartLayer.Kadastrale_gemeente_label] = "kadastralekaart:Kadastrale_gemeente_label",
        [KadastraleKaartLayer.Sectie] = "kadastralekaart:Sectie",
        [KadastraleKaartLayer.Sectie_label] = "kadastralekaart:Sectie_label",
    };

    private readonly HttpClient _httpClient;

    public PdokKadastraleKaartService(ILogger logger)
    {
        _logger = logger;
        _httpClient = new HttpClient();
    }

    public async Task<PdokFeatureCollection?> GetFeaturesByBoundingBoxAsync(
        KadastraleKaartLayer layer,
        GeoPoint point1,
        GeoPoint point2,
        CancellationToken ct = default)
    {
        var typeName = LayerTypeNames[layer];
        var bbox = BuildBbox(point1, point2);
        var url = BuildWfsUrl(typeName, bbox);

        var response = await _httpClient.GetAsync(url, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            throw new Exception($"PDOK Error: {response.StatusCode}\n{errorContent}");
        }

        //#test
        //var responseContent = await response.Content.ReadAsStringAsync(ct);

        return await response.Content.ReadFromJsonAsync<PdokFeatureCollection>(cancellationToken: ct);
    }

    public async Task<List<(PdokFeature Feature, PdokPerceel Perceel)>> GetParcelsByBoundingBoxAsync(
        GeoPoint point1,
        GeoPoint point2,
        CancellationToken ct = default)
    {
        var collection = await GetFeaturesByBoundingBoxAsync(KadastraleKaartLayer.Perceel, point1, point2, ct);
        if (collection is null)
            return [];

        return collection.Features
            .Select(f => (f, f.Properties.Deserialize<PdokPerceel>()))
            .Where(t => t.Item2 is not null)
            .Select(t => (t.f, t.Item2!))
            .ToList();
    }

    private static string BuildBbox(GeoPoint point1, GeoPoint point2)
    {
        var inv = System.Globalization.CultureInfo.InvariantCulture;
        double minX = Math.Min(point1.Lon, point2.Lon);
        double minY = Math.Min(point1.Lat, point2.Lat);
        double maxX = Math.Max(point1.Lon, point2.Lon);
        double maxY = Math.Max(point1.Lat, point2.Lat);
        return $"{minX.ToString(inv)},{minY.ToString(inv)},{maxX.ToString(inv)},{maxY.ToString(inv)},EPSG:28992";
    }

    private static string BuildWfsUrl(string typeName, string bbox) =>
        $"{WfsBaseUrl}" +
        $"?SERVICE=WFS" +
        $"&VERSION=2.0.0" +
        $"&REQUEST=GetFeature" +
        $"&TYPENAMES={typeName}" +
        $"&BBOX={bbox}" +
        $"&SRSNAME=EPSG:28992" +
        $"&outputFormat=application/json";

    public void Dispose() => _httpClient.Dispose();
}