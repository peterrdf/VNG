using System.Text.Json.Serialization;

namespace VNGService.Models;

/// <summary>
/// Represents a geographical point using Latitude (Lat) and Longitude (Lon).
/// </summary>
public record GeoPoint(double Lat, double Lon);

/// <summary>
/// Represents a standardized geometry object (GeoJSON format).
/// Contains the type of geometry (e.g., Point, Polygon) and its coordinates array.
/// </summary>
public record BagGeometry(
    // The type of geometric structure (e.g., "Point", "Polygon").
    [property: JsonPropertyName("type")] string Type,
    // The coordinates data (an array of coordinate arrays).
    [property: JsonPropertyName("coordinates")] double[][][] Coordinates
);

/// <summary>
/// A wrapper record for a single property (building) result.
/// </summary>
public record BagPandWrapper(
    // The actual property object containing details.
    [property: JsonPropertyName("pand")] BagPand? Pand,
    // Links metadata specific to this individual property result.
    [property: JsonPropertyName("_links")] BagPandLinks? Links
);

/// <summary>
/// Contains general links (references) related to a single property object.
/// </summary>
public record BagPandLinks(
    // Link pointing back to the current resource instance ('self').
    [property: JsonPropertyName("self")] BagHref? Self
);

/// <summary>
/// Represents detailed information about a real estate property (building).
/// This corresponds to the 'pand' object in an address context.
/// </summary>
public record BagPand(
    // The unique identification code of the property.
    [property: JsonPropertyName("identificatie")] string Id,
    // The domain or system where the ID originates.
    [property: JsonPropertyName("domein")] string Domein,
    // Geometric representation (coordinates) of the property.
    [property: JsonPropertyName("geometrie")] BagGeometry? Geometry,
    // Original construction year as a string.
    [property: JsonPropertyName("oorspronkelijkBouwjaar")] string BuildYear,
    // The current status of the property (e.g., "occupied", "empty").
    [property: JsonPropertyName("status")] string Status,
    // Date when the information was recorded/detected.
    [property: JsonPropertyName("geconstateerd")] string Geconstateerd,
    // Document date associated with the data source.
    [property: JsonPropertyName("documentdatum")] string Documentdatum,
    // Unique document number from the data source.
    [property: JsonPropertyName("documentnummer")] string Documentnummer,
    // Optional structured details regarding detection or occurrence (e.g., evidence).
    [property: JsonPropertyName("voorkomen")] BagVoorkomen? Voorkomen
);

/// <summary>
/// Contains the embedded list of properties found during a search.
/// </summary>
public record BagPandenEmbedded(
    // List containing all wrapped property results.
    [property: JsonPropertyName("panden")] List<BagPandWrapper>? Panden
);

/// <summary>
/// The comprehensive response object for a list of properties (buildings).
/// </summary>
public record BagPandenResponse(
    // Embedded collection containing the actual list of property results.
    [property: JsonPropertyName("_embedded")] BagPandenEmbedded? Embedded,
    // General links metadata for the whole property set.
    [property: JsonPropertyName("_links")] BagPandLinks? Links
);