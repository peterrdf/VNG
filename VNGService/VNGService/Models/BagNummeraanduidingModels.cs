using System.Text.Json.Serialization;

namespace VNGService.Models;

/// <summary>
/// Represents the full response containing a list of number designations (Nummeraanduidingen).
/// </summary>
public record BagNummeraanduidingenResponse(
    // Embedded data containing the actual list of number designation objects.
    [property: JsonPropertyName("_embedded")] BagNummeraanduidingenEmbedded? Embedded,
    // General links metadata for the entire set of results.
    [property: JsonPropertyName("_links")] BagLinks? Links
);

/// <summary>
/// Contains the embedded collection of number designation objects.
/// </summary>
public record BagNummeraanduidingenEmbedded(
    // List containing all wrapped number designation records.
    [property: JsonPropertyName("nummeraanduidingen")] List<BagNummeraanduidingWrapper>? Nummeraanduidingen
);

/// <summary>
/// Wrapper class for a single address numbering result, linking the core object and its links.
/// </summary>
public class BagNummeraanduidingWrapper
{
    // The core detailed number designation object.
    [JsonPropertyName("nummeraanduiding")]
    public BagNummeraanduiding? Nummeraanduiding { get; set; }

    // Link metadata specific to this individual numbering result.
    [JsonPropertyName("_links")]
    public BagNummeraanduidingLinks? Links { get; set; }
}

/// <summary>
/// Represents detailed information for a single address number designation (street address + number).
/// </summary>
public class BagNummeraanduiding
{
    // Unique identification code of the numbering result.
    [JsonPropertyName("identificatie")]
    public string? Identificatie { get; set; }

    // The domain or system where the ID originates.
    [JsonPropertyName("domein")]
    public string? Domein { get; set; }

    // The house number (integer part).
    [JsonPropertyName("huisnummer")]
    public int? Huisnummer { get; set; }

    // The letter associated with the house number.
    [JsonPropertyName("huisletter")]
    public string? Huisletter { get; set; }

    // An optional suffix or addition to the primary house number (e.g., "A-1").
    [JsonPropertyName("huisnummertoevoeging")]
    public string? Huisnummertoevoeging { get; set; }

    // The postal code.
    [JsonPropertyName("postcode")]
    public string? Postcode { get; set; }

    // The type of addressable object (e.g., "building", "lot").
    [JsonPropertyName("typeAdresseerbaarObject")]
    public string? TypeAdresseerbaarObject { get; set; }

    // The current status of the address number designation.
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

    // Describes what street or public space this address belongs to (Ligt Aan = Lies on).
    [JsonPropertyName("ligtAan")]
    public string? LigtAan { get; set; }

    // Describes which municipality/settlement this address is located in (Ligt In = Lies in).
    [JsonPropertyName("ligtIn")]
    public string? LigtIn { get; set; }

    // Optional structured details regarding detection or occurrence (e.g., evidence).
    [JsonPropertyName("voorkomen")]
    public BagVoorkomen? Voorkomen { get; set; }
}

/// <summary>
/// Links metadata specific to a number designation result, providing navigational links.
/// </summary>
public class BagNummeraanduidingLinks
{
    // Link pointing back to the current resource instance ('self').
    [JsonPropertyName("self")]
    public BagHref? Self { get; set; }

    // Link referencing the public space (street/road) the address lies on.
    [JsonPropertyName("ligtAanOpenbareRuimte")]
    public BagHref? LigtAanOpenbareRuimte { get; set; }

    // Link referencing the municipality/settlement the address is located in.
    [JsonPropertyName("ligtInWoonplaats")]
    public BagHref? LigtInWoonplaats { get; set; }
}