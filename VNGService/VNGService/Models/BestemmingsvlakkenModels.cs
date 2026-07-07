using System.Text.Json.Serialization;

namespace VNGService.Models
{
    /// <summary>
    /// Represents the full response containing a list of designated zoning areas (Bestemmingsvlakken).
    /// </summary>
    public class BestemmingsvlakkenResponse
    {
        // Embedded data containing the actual list of zoning areas.
        [JsonPropertyName("_embedded")]
        public BestemmingsvlakkenEmbedded Embedded { get; set; } = new();

        // General link metadata for navigating the plan records.
        [JsonPropertyName("_links")]
        public PlannenResponseLinks Links { get; set; } = new();
    }

    /// <summary>
    /// Contains the embedded collection of zoning areas found during a query.
    /// </summary>
    public class BestemmingsvlakkenEmbedded
    {
        // List containing all individual zoning area records.
        [JsonPropertyName("bestemmingsvlakken")]
        public List<Bestemmingsvlak> Bestemmingsvlakken { get; set; } = new();
    }

    /// <summary>
    /// Represents a single, detailed zone or designated planning unit within an urban plan.
    /// </summary>
    public class Bestemmingsvlak
    {
        // Unique identification code of the zoning area.
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        // The classification type of the zone (e.g., "Residential", "Commercial").
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        // The official, human-readable name of the zoning area.
        [JsonPropertyName("naam")]
        public string Naam { get; set; } = string.Empty;

        // The main group classification for the zone (e.g., "Residential Core").
        [JsonPropertyName("bestemmingshoofdgroep")]
        public string Bestemmingshoofdgroep { get; set; } = string.Empty;

        // List of functional purposes allowed in this zone (e.g., "Housing", "Retail").
        [JsonPropertyName("bestemmingsfuncties")]
        public List<string> Bestemmingsfuncties { get; set; } = new();

        // The specific article or section number within the plan documentation.
        [JsonPropertyName("artikelnummer")]
        public string Artikelnummer { get; set; } = string.Empty;

        // References to external text documents related to this zone.
        [JsonPropertyName("verwijzingNaarTekst")]
        public List<string> VerwijzingNaarTekst { get; set; } = new();

        // Administrative label or information related to the visualization layer.
        [JsonPropertyName("labelInfo")]
        public string LabelInfo { get; set; } = string.Empty;

        // Style identifier used for rendering/displaying the zone on a map.
        [JsonPropertyName("styleId")]
        public string StyleId { get; set; } = string.Empty;

        // The geometric boundary data (e.g., coordinates, polygon).
        [JsonPropertyName("geometrie")]
        public GeoJsonGeometry? Geometrie { get; set; }

        // Link metadata specific to this individual zoning area record.
        [JsonPropertyName("_links")]
        public BestemmingsvlakLinks Links { get; set; } = new();
    }

    /// <summary>
    /// Collection of links (references) related to a specific designated zoning area.
    /// </summary>
    public class BestemmingsvlakLinks
    {
        // Link pointing back to the current resource instance ('self').
        [JsonPropertyName("self")]
        public HrefLink Self { get; set; } = new();

        // Links referencing supporting textual descriptions.
        [JsonPropertyName("teksten")]
        public List<HrefLink> Teksten { get; set; } = new();

        // Links referencing construction guidelines or indications within the zone.
        [JsonPropertyName("bouwaanduidingen")]
        public List<HrefLink> Bouwaanduidingen { get; set; } = new();

        // Links referencing defined figures or diagrams for the zone.
        [JsonPropertyName("figuren")]
        public List<HrefLink> Figuren { get; set; } = new();

        // Links referencing functional area guidelines (e.g., permissible function areas).
        [JsonPropertyName("functieaanduidingen")]
        public List<HrefLink> Functieaanduidingen { get; set; } = new();

        // Links referencing general area indications within the zone.
        [JsonPropertyName("gebiedsaanduidingen")]
        public List<HrefLink> Gebiedsaanduidingen { get; set; } = new();

        // Links referencing alphabetical or symbolic notations used in the plan.
        [JsonPropertyName("lettertekenaanduidingen")]
        public List<HrefLink> Lettertekenaanduidingen { get; set; } = new();

        // Links referencing specific dimensional measurements recorded for the zone.
        [JsonPropertyName("maatvoeringen")]
        public List<HrefLink> Maatvoeringen { get; set; } = new();

        // Links referencing actual built-up areas or building blocks within the zoning area.
        [JsonPropertyName("bouwvlakken")]
        public List<HrefLink> Bouwvlakken { get; set; } = new();
    }
}