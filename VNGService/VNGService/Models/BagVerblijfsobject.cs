using System.Text.Json.Serialization;

namespace VNGService.Models;

/// <summary>
/// Represents the full response containing a list of dwelling units or residence objects.
/// </summary>
public sealed class BagVerblijfsobjectenResponse
{
    // Embedded data containing the actual list of dwelling unit records.
    [JsonPropertyName("_embedded")]
    public BagVerblijfsobjectenEmbedded? Embedded { get; set; }

    // General links metadata for pagination and resource references.
    [JsonPropertyName("_links")]
    public BagVerblijfsobjectenLinks? Links { get; set; }
}

/// <summary>
/// Contains the embedded collection of dwelling units found during a search.
/// </summary>
public sealed class BagVerblijfsobjectenEmbedded
{
    // List containing all wrapped individual dwelling unit results.
    [JsonPropertyName("verblijfsobjecten")]
    public List<BagVerblijfsobjectWrapper>? Verblijfsobjecten { get; set; }
}

/// <summary>
/// Wrapper class for a single residence object, linking the core data and its associated links.
/// </summary>
public sealed class BagVerblijfsobjectWrapper
{
    // The actual dwelling unit object containing details.
    [JsonPropertyName("verblijfsobject")]
    public BagVerblijfsobject? Verblijfsobject { get; set; }

    // Link metadata specific to this individual residence result.
    [JsonPropertyName("_links")]
    public BagVerblijfsobjectLinks? Links { get; set; }
}

/// <summary>
/// Represents detailed information about a dwelling unit or residential building structure component.
/// </summary>
public sealed class BagVerblijfsobject
{
    // The type classification of the residence object (e.g., "apartment", "house").
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    // Unique identification code of the dwelling unit.
    [JsonPropertyName("identificatie")]
    public string? Identificatie { get; set; }

    // The domain or system where the ID originates.
    [JsonPropertyName("domein")]
    public string? Domein { get; set; }

    // Geographical representation (coordinates) of the residence unit.
    [JsonPropertyName("geometrie")]
    public BagVerblijfsobjectGeometrie? Geometrie { get; set; }

    // List of intended uses for this dwelling unit (e.g., "residential", "commercial").
    [JsonPropertyName("gebruiksdoelen")]
    public List<string>? Gebruiksdoelen { get; set; }

    // The measured surface area in square meters.
    [JsonPropertyName("oppervlakte")]
    public int Oppervlakte { get; set; }

    // The current status of the dwelling unit (e.g., "occupied", "vacant").
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    // Date when the information was detected/confirmed.
    [JsonPropertyName("geconstateerd")]
    public string? Geconstateerd { get; set; }

    // Document date associated with the data source.
    [JsonPropertyName("documentdatum")]
    public string? Documentdatum { get; set; }

    // Unique document number from the data source.
    [JsonPropertyName("documentnummer")]
    public string? Documentnummer { get; set; }

    // Optional structured details regarding detection or occurrence (e.g., evidence).
    [JsonPropertyName("voorkomen")]
    public BagVerblijfsobjectVoorkomen? Voorkomen { get; set; }

    // List of identifiers that this dwelling unit is part of (e.g., a larger complex ID).
    [JsonPropertyName("maaktDeelUitVan")]
    public List<string>? MaaktDeelUitVan { get; set; }

    // The identifier of the main/principal address associated with this unit.
    [JsonPropertyName("heeftAlsHoofdAdres")]
    public string? HeeftAlsHoofdAdres { get; set; }
}

/// <summary>
/// Wraps the geometric representation (coordinates) for a residence object.
/// </summary>
public sealed class BagVerblijfsobjectGeometrie
{
    // The point geometry data structure itself.
    [JsonPropertyName("punt")]
    public BagVerblijfsobjectPunt? Punt { get; set; }
}

/// <summary>
/// Represents a geographical point (latitude/longitude) used for the residence object's location.
/// </summary>
public sealed class BagVerblijfsobjectPunt
{
    // The type of geometry (e.g., "Point").
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    // The coordinates list [Longitude, Latitude].
    [JsonPropertyName("coordinates")]
    public List<double>? Coordinates { get; set; }
}

/// <summary>
/// Provides contextual information regarding the detection or occurrence of the dwelling unit data.
/// </summary>
public sealed class BagVerblijfsobjectVoorkomen
{
    // Timestamp when the record was registered/observed.
    [JsonPropertyName("tijdstipRegistratie")]
    public string? TijdstipRegistratie { get; set; }

    // Version number of this occurrence data point.
    [JsonPropertyName("versie")]
    public int Versie { get; set; }

    // Start date/time of validity for the dwelling unit record segment.
    [JsonPropertyName("beginGeldigheid")]
    public string? BeginGeldigheid { get; set; }

    // Timestamp registration according to the LV system (if applicable).
    [JsonPropertyName("tijdstipRegistratieLV")]
    public string? TijdstipRegistratieLV { get; set; }
}

/// <summary>
/// Contains links (references) related to a specific dwelling unit record.
/// </summary>
public sealed class BagVerblijfsobjectLinks
{
    // Link pointing back to the current resource instance ('self').
    [JsonPropertyName("self")]
    public BagHref? Self { get; set; }

    // Link referencing the main/principal address associated with this unit.
    [JsonPropertyName("heeftAlsHoofdAdres")]
    public BagHref? HeeftAlsHoofdAdres { get; set; }

    // List of links pointing to other resources that make up this object (e.g., related structures).
    [JsonPropertyName("maaktDeelUitVan")]
    public List<BagHref>? MaaktDeelUitVan { get; set; }
}

/// <summary>
/// Pagination and overall link metadata for the collection of dwelling units.
/// </summary>
public sealed class BagVerblijfsobjectenLinks
{
    // Link pointing back to the current resource instance ('self').
    [JsonPropertyName("self")]
    public BagHref? Self { get; set; }

    // Link pointing to the next page/set of results.
    [JsonPropertyName("next")]
    public BagHref? Next { get; set; }

    // Link pointing to the last page/set of results.
    [JsonPropertyName("last")]
    public BagHref? Last { get; set; }
}