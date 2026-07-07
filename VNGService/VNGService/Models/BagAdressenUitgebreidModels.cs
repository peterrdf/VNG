using System.Text.Json.Serialization;

// Class representing comprehensive models for address data retrieval (Bag = Bag of, Uitgebreid = Expanded)
namespace VNGService.Models;

/// <summary>
/// Represents the full response container for expanded address search results.
/// </summary>
public class BagAdressenUitgebreidResponse
{
    // Property containing embedded data objects (e.g., addresses list).
    [JsonPropertyName("_embedded")]
    public BagAdressenUitgebreidEmbedded? Embedded { get; set; }

    // Property containing link metadata (related resources).
    [JsonPropertyName("_links")]
    public BagLinks? Links { get; set; }
}

/// <summary>
/// Contains the embedded collection of addresses.
/// </summary>
public class BagAdressenUitgebreidEmbedded
{
    // List containing all the detailed address entries found in the result.
    [JsonPropertyName("adressen")]
    public List<BagAdresUitgebreid>? Adressen { get; set; }
}

/// <summary>
/// Represents a single, highly detailed record for an address (adresiable object).
/// </summary>
public class BagAdresUitgebreid
{
    // The name of the public space (e.g., street name).
    [JsonPropertyName("openbareRuimteNaam")]
    public string? OpenbareRuimteNaam { get; set; }

    // The house number.
    [JsonPropertyName("huisnummer")]
    public int? Huisnummer { get; set; }

    // The letter associated with the house number (e.g., A, B).
    [JsonPropertyName("huisletter")]
    public string? Huisletter { get; set; }

    // The postal code.
    [JsonPropertyName("postcode")]
    public string? Postcode { get; set; }

    // The name of the settlement (city/municipality).
    [JsonPropertyName("woonplaatsNaam")]
    public string? WoonplaatsNaam { get; set; }

    // Unique identification for the number designation (addressing number).
    [JsonPropertyName("nummeraanduidingIdentificatie")]
    public string? NummeraanduidingIdentificatie { get; set; }

    // Unique identification for the public space.
    [JsonPropertyName("openbareRuimteIdentificatie")]
    public string? OpenbareRuimteIdentificatie { get; set; }

    // Unique identification for the settlement/municipality.
    [JsonPropertyName("woonplaatsIdentificatie")]
    public string? WoonplaatsIdentificatie { get; set; }

    // Supplemental address line 5 data.
    [JsonPropertyName("adresregel5")]
    public string? Adresregel5 { get; set; }

    // Supplemental address line 6 data.
    [JsonPropertyName("adresregel6")]
    public string? Adresregel6 { get; set; }

    // A short, descriptive name for the addressable object.
    [JsonPropertyName("korteNaam")]
    public string? KorteNaam { get; set; }

    // Unique identification of the entire addressable object.
    [JsonPropertyName("adresseerbaarObjectIdentificatie")]
    public string? AdresseerbaarObjectIdentificatie { get; set; }

    // The current status or state of the addressable object.
    [JsonPropertyName("adresseerbaarObjectStatus")]
    public string? AdresseerbaarObjectStatus { get; set; }

    // Defines what kind of object this address represents (e.g., building, lot).
    [JsonPropertyName("typeAdresseerbaarObject")]
    public string? TypeAdresseerbaarObject { get; set; }

    // The measured surface area in square meters.
    [JsonPropertyName("oppervlakte")]
    public int? Oppervlakte { get; set; }

    // List of purposes for which the address can be used (e.g., residence, commercial).
    [JsonPropertyName("gebruiksdoelen")]
    public List<string>? Gebruiksdoelen { get; set; }

    // Original construction year(s) for the building/object.
    [JsonPropertyName("oorspronkelijkBouwjaar")]
    public List<string>? OorspronkelijkBouwjaar { get; set; }

    // A list of identifiers related to real estate properties associated with this address.
    [JsonPropertyName("pandIdentificaties")]
    public List<string>? PandIdentificaties { get; set; }

    // A list detailing the status of associated real estate properties (e.g., occupied, empty).
    [JsonPropertyName("pandStatussen")]
    public List<string>? PandStatussen { get; set; }

    // Geometrical representation (coordinates) of the addressable object.
    [JsonPropertyName("adresseerbaarObjectGeometrie")]
    public BagGeometrie? AdresseerbaarObjectGeometrie { get; set; }

    // Links metadata specific to this individual address record.
    [JsonPropertyName("_links")]
    public BagAdresUitgebreidLinks? Links { get; set; }
}

/// <summary>
/// Collection of links (references) related to a single detailed address object.
/// </summary>
public class BagAdresUitgebreidLinks
{
    // Link pointing back to the current resource instance ('self').
    [JsonPropertyName("self")]
    public BagHref? Self { get; set; }

    // Link referencing the general addressable object details.
    [JsonPropertyName("adresseerbaarObject")]
    public BagHref? AdresseerbaarObject { get; set; }

    // Link referencing the numbering designation (number + letter).
    [JsonPropertyName("nummeraanduiding")]
    public BagHref? Nummeraanduiding { get; set; }

    // Link referencing the public space (street/road).
    [JsonPropertyName("openbareRuimte")]
    public BagHref? OpenbareRuimte { get; set; }

    // Link referencing the settlement or municipality.
    [JsonPropertyName("woonplaats")]
    public BagHref? Woonplaats { get; set; }

    // Links pointing to associated properties (buildings) at this address.
    [JsonPropertyName("panden")]
    public List<BagHref>? Panden { get; set; }
}