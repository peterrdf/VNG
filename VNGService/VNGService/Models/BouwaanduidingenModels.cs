using System.Text.Json.Serialization;

namespace VNGService.Models
{
    /// <summary>
    /// Represents the full response containing a list of building designation areas or guidelines.
    /// </summary>
    public class BouwaanduidingenResponse
    {
        // Embedded data containing the actual list of building designation records.
        [JsonPropertyName("_embedded")]
        public BouwaanduidingenEmbedded Embedded { get; set; } = new();

        // General link metadata for navigating the plan records.
        [JsonPropertyName("_links")]
        public PlannenResponseLinks Links { get; set; } = new();
    }

    /// <summary>
    /// Contains the embedded collection of building designations found during a query.
    /// </summary>
    public class BouwaanduidingenEmbedded
    {
        // List containing all individual building designation records.
        [JsonPropertyName("bouwaanduidingen")]
        public List<Bouwaanduiding> Bouwaanduidingen { get; set; } = new();
    }

    /// <summary>
    /// Represents a single, detailed guideline or designated area for construction/building within a plan.
    /// </summary>
    public class Bouwaanduiding
    {
        // Unique identification code of the building designation unit.
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        // The official name of the guideline/area.
        [JsonPropertyName("naam")]
        public string Naam { get; set; } = string.Empty;

        // Optional descriptive label information for display purposes.
        [JsonPropertyName("labelInfo")]
        public string? LabelInfo { get; set; }

        // Reference to external text documentation associated with this guideline (using 'object' as generic reference).
        [JsonPropertyName("verwijzingNaarTekst")]
        public object? VerwijzingNaarTekst { get; set; }

        // Style identifier used for rendering/displaying the area on a map.
        [JsonPropertyName("styleId")]
        public string StyleId { get; set; } = string.Empty;

        // The geometric boundary data (e.g., coordinates, polygon).
        [JsonPropertyName("geometrie")]
        public GeoJsonGeometry? Geometrie { get; set; }

        // Link metadata specific to this individual building designation record.
        [JsonPropertyName("_links")]
        public BouwaanduidingLinks Links { get; set; } = new();
    }

    /// <summary>
    /// Collection of links (references) related to a specific building designation unit.
    /// </summary>
    public class BouwaanduidingLinks
    {
        // Link pointing back to the current resource instance ('self').
        [JsonPropertyName("self")]
        public HrefLink? Self { get; set; }

        // Links referencing supporting textual descriptions associated with this designation.
        [JsonPropertyName("teksten")]
        public List<HrefLink> Teksten { get; set; } = new();
    }
}