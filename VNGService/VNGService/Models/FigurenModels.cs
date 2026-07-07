using System.Text.Json.Serialization;

namespace VNGService.Models
{
    /// <summary>
    /// Represents the full response containing a list of plan figures or diagrams used in zoning documentation.
    /// </summary>
    public class FigurenResponse
    {
        // Embedded data containing the actual list of figure records.
        [JsonPropertyName("_embedded")]
        public FigurenEmbedded Embedded { get; set; } = new();

        // General link metadata for navigating related plan resources.
        [JsonPropertyName("_links")]
        public PlannenResponseLinks Links { get; set; } = new();
    }

    /// <summary>
    /// Contains the embedded collection of figure records found during a query.
    /// </summary>
    public class FigurenEmbedded
    {
        // List containing all individual figure records.
        [JsonPropertyName("figuren")]
        public List<Figuur> Figuren { get; set; } = new();
    }

    /// <summary>
    /// Represents a single, detailed visual element or diagram within an urban plan.
    /// </summary>
    public class Figuur
    {
        // Unique identification code of the figure.
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        // The official name or descriptive title of the figure.
        [JsonPropertyName("naam")]
        public string Naam { get; set; } = string.Empty;

        // List of article numbers from the plan where this figure is relevant/referenced.
        [JsonPropertyName("artikelnummers")]
        public List<string> Artikelnummers { get; set; } = new();

        // Optional administrative label information for display purposes.
        [JsonPropertyName("labelInfo")]
        public object? LabelInfo { get; set; }

        // References to external text documentation associated with this figure (can be complex object list).
        [JsonPropertyName("verwijzingNaarTekst")]
        public List<object> VerwijzingNaarTekst { get; set; } = new();

        // Optional detailed visual descriptions or auxiliary illustrations.
        [JsonPropertyName("illustraties")]
        public object? Illustraties { get; set; }

        // Style identifier used for rendering/displaying the figure on a map.
        [JsonPropertyName("styleId")]
        public string StyleId { get; set; } = string.Empty;

        // The geometric boundary data (e.g., coordinates, polygon) of the figure area.
        [JsonPropertyName("geometrie")]
        public GeoJsonGeometry? Geometrie { get; set; }

        // Link metadata specific to this individual figure record.
        [JsonPropertyName("_links")]
        public FiguurLinks Links { get; set; } = new();
    }

    /// <summary>
    /// Collection of links (references) related to a specific figure diagram.
    /// </summary>
    public class FiguurLinks
    {
        // Link pointing back to the current resource instance ('self').
        [JsonPropertyName("self")]
        public HrefLink Self { get; set; } = new();

        // Links referencing supporting textual descriptions associated with this figure.
        [JsonPropertyName("teksten")]
        public List<object> Teksten { get; set; } = new();
    }
}