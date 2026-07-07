using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using VNGService.Models;

namespace VNGService.Services;

public sealed class BagApiService : IDisposable
{
    private IConfiguration _configuration;
    private readonly ILogger _logger;

    private const string BaseUrl = "https://api.bag.kadaster.nl/lvbag/individuelebevragingen/v2/";
    private readonly HttpClient _httpClient;

    public BagApiService(IConfiguration configuration, ILogger logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        SettingsManager settingsManager = new SettingsManager(_configuration, _logger);
        var apiSettings = settingsManager.LoadAPISettings();
        var APIKey = apiSettings.ContainsKey("bag") ? apiSettings["bag"] : throw new InvalidOperationException("BAG API key is not configured.");

        _httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        _httpClient.DefaultRequestHeaders.Add("X-Api-Key", APIKey);
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/hal+json");
        _httpClient.DefaultRequestHeaders.Add("Content-Crs", "epsg:28992");
        _httpClient.DefaultRequestHeaders.Add("Accept-Crs", "epsg:28992");
    }

    public async Task<BagAdresseerbareObjectenResponse?> GetAddressableObjectsByBoundingBoxAsync(GeoPoint point1, GeoPoint point2, CancellationToken ct = default)
    {
        // GeoPoint.Lat = Northing (Y), GeoPoint.Lon = Easting (X) in RD New context
        double minX = Math.Min(point1.Lon, point2.Lon);
        double minY = Math.Min(point1.Lat, point2.Lat);
        double maxX = Math.Max(point1.Lon, point2.Lon);
        double maxY = Math.Max(point1.Lat, point2.Lat);

        var inv = System.Globalization.CultureInfo.InvariantCulture;
        var bbox = $"{minX.ToString(inv)},{minY.ToString(inv)},{maxX.ToString(inv)},{maxY.ToString(inv)}";
        var url = $"adresseerbareobjecten?bbox={bbox}";

        var response = await _httpClient.GetAsync(url, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            throw new Exception($"BAG Error: {response.StatusCode}\n{errorContent}");
        }

        //#test
        //var responseContent = await response.Content.ReadAsStringAsync();

        var result = await response.Content.ReadFromJsonAsync<BagAdresseerbareObjectenResponse>(cancellationToken: ct);
        return result;
    }

    public async Task<BagAdressenResponse?> GetAddressByAddressableObjectIdAsync(string id, CancellationToken ct = default)
    {
        var url = $"adressen?adresseerbaarObjectIdentificatie={id}";

        var response = await _httpClient.GetAsync(url, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            throw new Exception($"BAG Error: {response.StatusCode}\n{errorContent}");
        }

        //#test
        //var responseContent = await response.Content.ReadAsStringAsync();

        var result = await response.Content.ReadFromJsonAsync<BagAdressenResponse>(cancellationToken: ct);
        return result;
    }

    public async Task<BagAdressenResponse?> GetAddressByPandIdAsync(string id, CancellationToken ct = default)
    {
        var url = $"adressen?pandIdentificatie={id}";

        var response = await _httpClient.GetAsync(url, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            throw new Exception($"BAG Error: {response.StatusCode}\n{errorContent}");
        }

        //#test
        //var responseContent = await response.Content.ReadAsStringAsync();

        var result = await response.Content.ReadFromJsonAsync<BagAdressenResponse>(cancellationToken: ct);
        return result;
    }

    public async Task<BagAdressenUitgebreidResponse?> GetAddressExtendedByAddressableObjectIdAsync(string id, CancellationToken ct = default)
    {
        var url = $"adressenuitgebreid?adresseerbaarObjectIdentificatie={id}";

        var response = await _httpClient.GetAsync(url, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            throw new Exception($"BAG Error: {response.StatusCode}\n{errorContent}");
        }

        //#test
        //var responseContent = await response.Content.ReadAsStringAsync();

        var result = await response.Content.ReadFromJsonAsync<BagAdressenUitgebreidResponse>(cancellationToken: ct);
        return result;
    }

    /// <summary>
    /// Returns buildings (panden) within the bounding box defined by two RD New (EPSG:28992) points.
    /// bbox format expected by API: minX, minY, maxX, maxY (Easting, Northing)
    /// </summary>
    public async Task<List<BagPand>> GetPandenByBoundingBoxAsync(GeoPoint point1, GeoPoint point2, CancellationToken ct = default)
    {
        // GeoPoint.Lat = Northing (Y), GeoPoint.Lon = Easting (X) in RD New context
        double minX = Math.Min(point1.Lon, point2.Lon);
        double minY = Math.Min(point1.Lat, point2.Lat);
        double maxX = Math.Max(point1.Lon, point2.Lon);
        double maxY = Math.Max(point1.Lat, point2.Lat);

        var inv = System.Globalization.CultureInfo.InvariantCulture;
        var bbox = $"{minX.ToString(inv)},{minY.ToString(inv)},{maxX.ToString(inv)},{maxY.ToString(inv)}";
        var url = $"panden?bbox={bbox}";

        var response = await _httpClient.GetAsync(url, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            throw new Exception($"BAG Error: {response.StatusCode}\n{errorContent}");
        }

        //#test
        //var responseContent = await response.Content.ReadAsStringAsync();

        var result = await response.Content.ReadFromJsonAsync<BagPandenResponse>(cancellationToken: ct);
        return result?.Embedded?.Panden?
            .Where(w => w.Pand != null)
            .Select(w => w.Pand!)
            .ToList() ?? [];
    }

    public async Task<BagPand?> GetPandByIdAsync(string id, CancellationToken ct = default)
    {
        var url = $"panden/{id}";
        var response = await _httpClient.GetAsync(url, ct);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            throw new Exception($"BAG Error: {response.StatusCode}\n{errorContent}");
        }

        //#test
        //var responseContent = await response.Content.ReadAsStringAsync();

        var result = await response.Content.ReadFromJsonAsync<BagPandWrapper>(cancellationToken: ct);
        return result?.Pand;
    }

    public async Task<BagOpenbareRuimte?> GetOpenbareRuimtenByIdAsync(string id, CancellationToken ct = default)
    {
        var url = $"openbareruimten/{id}";
        var response = await _httpClient.GetAsync(url, ct);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            throw new Exception($"BAG Error: {response.StatusCode}\n{errorContent}");
        }

        //#test
        //var responseContent = await response.Content.ReadAsStringAsync();

        var result = await response.Content.ReadFromJsonAsync<BagOpenbareRuimteWrapper>(cancellationToken: ct);
        return result?.OpenbareRuimte;
    }

    public async Task<BagVerblijfsobjectenResponse?> GetResidentialObjectsByBoundingBoxAsync(GeoPoint point1, GeoPoint point2, CancellationToken ct = default)
    {
        // GeoPoint.Lat = Northing (Y), GeoPoint.Lon = Easting (X) in RD New context
        double minX = Math.Min(point1.Lon, point2.Lon);
        double minY = Math.Min(point1.Lat, point2.Lat);
        double maxX = Math.Max(point1.Lon, point2.Lon);
        double maxY = Math.Max(point1.Lat, point2.Lat);

        var inv = System.Globalization.CultureInfo.InvariantCulture;
        var bbox = $"{minX.ToString(inv)},{minY.ToString(inv)},{maxX.ToString(inv)},{maxY.ToString(inv)}";
        var url = $"verblijfsobjecten?bbox={bbox}";

        var response = await _httpClient.GetAsync(url, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            throw new Exception($"BAG Error: {response.StatusCode}\n{errorContent}");
        }

        //#test
        //var responseContent = await response.Content.ReadAsStringAsync();

        var result = await response.Content.ReadFromJsonAsync<BagVerblijfsobjectenResponse>(cancellationToken: ct);
        return result;
    }

    public async Task<BagNummeraanduidingenResponse?> GetNumberIdentifierByPremiseIdAsync(string id, CancellationToken ct = default)
    {
        var url = $"nummeraanduidingen?pandIdentificatie={id}";
        var response = await _httpClient.GetAsync(url, ct);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            throw new Exception($"BAG Error: {response.StatusCode}\n{errorContent}");
        }

        //#test
        //var responseContent = await response.Content.ReadAsStringAsync();

        var result = await response.Content.ReadFromJsonAsync<BagNummeraanduidingenResponse>(cancellationToken: ct);
        return result;
    }

    public void Dispose() => _httpClient.Dispose();
}