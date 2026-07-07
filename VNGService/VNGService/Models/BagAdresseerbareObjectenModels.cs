using System.Text.Json.Serialization;

namespace VNGService.Models;

/// <summary>
/// Root response wrapper for addressable objects.
/// </summary>
// Root response
public class BagAdresseerbareObjectenResponse
{
    /// <summary>
    /// Embedded data containing the list of addressable objects.
    /// JSON property name: "_embedded"
    /// </summary>
    [JsonPropertyName("_embedded")]
    public BagAdresseerbareObjectenEmbedded? Embedded { get; set; }

    /// <summary>
    /// Links metadata for the response.
    /// JSON property name: "_links"
    /// </summary>
    [JsonPropertyName("_links")]
    public BagLinks? Links { get; set; }
}

/// <summary>
/// Embedded container holding the list of addressable objects (the 'adresseerbareObjecten' array).
/// </summary>
public class BagAdresseerbareObjectenEmbedded
{
    /// <summary>
    /// The list of addressable object wrappers. JSON property name: "adresseerbareObjecten"
    /// </summary>
    [JsonPropertyName("adresseerbareObjecten")]
    public List<BagAdresseerbaarObjectWrapper>? AdresseerbareObjecten { get; set; }
}

/// <summary>
/// Outer wrapper for a single addressable object. Represents the structure {"verblijfsobject": { ... }}
/// </summary>
// Outer wrapper: {"verblijfsobject": { ... }}
public class BagAdresseerbaarObjectWrapper
{
    /// <summary>
    /// The actual container holding the detailed stay/dwelling object information. JSON property name: "verblijfsobject"
    /// </summary>
    [JsonPropertyName("verblijfsobject")]
    public BagVerblijfsobjectContainer? Verblijfsobject { get; set; }
}

/// <summary>
/// Inner container holding the detailed stay/dwelling object and its links. Represents {"verblijfsobject": { ... }, "_links": { ... }}
/// </summary>
// Inner container: {"verblijfsobject": { ... }, "_links": { ... }}
public class BagVerblijfsobjectContainer
{
    /// <summary>
    /// The primary detailed dwelling object. JSON property name: "verblijfsobject"
    /// </summary>
    [JsonPropertyName("verblijfsobject")]
    public BagVerblijfsobject? Verblijfsobject { get; set; }

    /// <summary>
    /// Links metadata specifically for the dwelling object. JSON property name: "_links"
    /// </summary>
    [JsonPropertyName("_links")]
    public BagVerblijfsobjectLinks? Links { get; set; }
}

/// <summary>
/// Container for geographic coordinates information.
/// </summary>
public class BagGeometrie
{
    /// <summary>
    /// The geographical point data contained within. JSON property name: "punt"
    /// </summary>
    [JsonPropertyName("punt")]
    public BagPunt? Punt { get; set; }
}

/// <summary>
/// Represents a geographic point structure, typically containing coordinates.
/// </summary>
public class BagPunt
{
    /// <summary>
    /// The type of the geometry (e.g., Point). JSON property name: "type"
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// The coordinates [longitude, latitude]. JSON property name: "coordinates"
    /// </summary>
    [JsonPropertyName("coordinates")]
    public List<double>? Coordinates { get; set; }
}

/// <summary>
/// Contains various timestamps and versioning information related to the record.
/// (Includes Registration Timestamp, Version, Validity Period).
/// </summary>
public class BagVoorkomen
{
    /// <summary>
    /// The timestamp when the record was registered. JSON property name: "tijdstipRegistratie"
    /// </summary>
    [JsonPropertyName("tijdstipRegistratie")]
    public string? TijdstipRegistratie { get; set; }

    /// <summary>
    /// The version number of the record. JSON property name: "versie"
    /// </summary>
    [JsonPropertyName("versie")]
    public int? Versie { get; set; }

    /// <summary>
    /// The start date/time of validity period. JSON property name: "beginGeldigheid"
    /// </summary>
    [JsonPropertyName("beginGeldigheid")]
    public string? BeginGeldigheid { get; set; }

    /// <summary>
    /// Timestamp for the Local Time (LV) registration. JSON property name: "tijdstipRegistratieLV"
    /// </summary>
    [JsonPropertyName("tijdstipRegistratieLV")]
    public string? TijdstipRegistratieLV { get; set; }
}

/// <summary>
/// Standard link structure used in API responses.
/// </summary>
public class BagLinks
{
    /// <summary>
    /// A self-reference link to the current resource. JSON property name: "self"
    /// </summary>
    [JsonPropertyName("self")]
    public BagHref? Self { get; set; }
}

/// <summary>
/// Represents a URI link structure.
/// </summary>
public class BagHref
{
    /// <summary>
    /// The actual URL of the resource link. JSON property name: "href"
    /// </summary>
    [JsonPropertyName("href")]
    public string? Href { get; set; }
}

/// <summary>
/// Represents a standardized address record. (Contains Street Name, House Number, Postcode, and City).
/// </summary>
// Note: Using 'record' keyword requires using property syntax for JsonProperty handling.
public record BagAdres(
    [property: JsonPropertyName("openbareRuimteNaam")] string StreetName, // JSON Key: openbareRuimteNaam (Street Name)
    [property: JsonPropertyName("huisnummer")] int HouseNumber,       // JSON Key: huisnummer (House Number)
    [property: JsonPropertyName("postcode")] string Postcode,         // JSON Key: postcode (Postal Code)
    [property: JsonPropertyName("woonplaatsNaam")] string City      // JSON Key: woonplaatsNaam (City/Municipality Name)
);

/// <summary>
/// Wrapper response for a list of addresses.
/// </summary>
public record BagAdressenResponse(
    [property: JsonPropertyName("_embedded")] BagAdressenEmbedded? Embedded // Root response wrapper containing embedded data
);

/// <summary>
/// Embedded container holding the list of actual address records. JSON key: "adressen"
/// </summary>
public record BagAdressenEmbedded(
    [property: JsonPropertyName("adressen")] List<BagAdres>? Adressen // The list of addresses
);