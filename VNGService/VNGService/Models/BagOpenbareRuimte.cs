using System.Text.Json.Serialization;

namespace VNGService.Models;

/// <summary>
/// Wrapper class containing a single Public Space object and its associated links.
/// </summary>
public sealed class BagOpenbareRuimteWrapper
{
    // The core public space details.
    [JsonPropertyName("openbareRuimte")]
    public BagOpenbareRuimte? OpenbareRuimte { get; set; }

    // Link metadata specific to this individual public space result.
    [JsonPropertyName("_links")]
    public BagOpenbareRuimteLinks? Links { get; set; }
}

/// <summary>
/// Represents detailed information about a Public Space (e.g., a street, road).
/// </summary>
public sealed class BagOpenbareRuimte
{
    // Unique identification code of the public space.
    [JsonPropertyName("identificatie")]
    public string? Identificatie { get; set; }

    // The domain or system where the ID originates.
    [JsonPropertyName("domein")]
    public string? Domein { get; set; }

    // The official name of the public space (e.g., "Damrak").
    [JsonPropertyName("naam")]
    public string? Naam { get; set; }

    // The type classification of the public space (e.g., "street", "square").
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    // The current status of the public space data.
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    // A short, commonly used name for the public space.
    [JsonPropertyName("korteNaam")]
    public string? KorteNaam { get; set; }

    // Date when this record was detected or confirmed.
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
    public BagOpenbareRuimteVoorkomen? Voorkomen { get; set; }

    // The settlement/municipality this public space is located within.
    [JsonPropertyName("ligtIn")]
    public string? LigtIn { get; set; }
}

/// <summary>
/// Provides contextual information regarding the detection or occurrence of the public space data.
/// </summary>
public sealed class BagOpenbareRuimteVoorkomen
{
    // Timestamp when the record was registered/observed.
    [JsonPropertyName("tijdstipRegistratie")]
    public string? TijdstipRegistratie { get; set; }

    // Version number of this occurrence data point.
    [JsonPropertyName("versie")]
    public int Versie { get; set; }

    // Start date/time of validity for the public space record segment.
    [JsonPropertyName("beginGeldigheid")]
    public string? BeginGeldigheid { get; set; }

    // Timestamp registration according to the LV system (if applicable).
    [JsonPropertyName("tijdstipRegistratieLV")]
    public string? TijdstipRegistratieLV { get; set; }
}

/// <summary>
/// Collection of links (references) related to a specific Public Space record.
/// </summary>
public sealed class BagOpenbareRuimteLinks
{
    // Link pointing back to the current resource instance ('self').
    [JsonPropertyName("self")]
    public BagHref? Self { get; set; }

    // Link referencing the municipality/settlement where the public space lies within.
    [JsonPropertyName("ligtInWoonplaats")]
    public BagHref? LigtInWoonplaats { get; set; }
}