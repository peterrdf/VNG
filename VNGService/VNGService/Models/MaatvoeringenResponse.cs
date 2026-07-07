using System.Text.Json.Serialization;

namespace VNGService.Models
{
    /// <summary>
    /// Represents the full response containing a list of dimensional measurements or spatial requirements.
    /// </summary>
    public class MaatvoeringenResponse
    {
        // Embedded data containing the actual list of measurement records.
        [JsonPropertyName("_embedded")]
        public MaatvoeringenEmbedded Embedded { get; set; } = new();

        // General link metadata for navigating related plan resources.
        [JsonPropertyName("_links")]
        public PlannenResponseLinks Links { get; set; } = new();
    }

    /// <summary>
    /// Contains the embedded list of dimensional measurements found during a query.
    /// </summary>
    public class MaatvoeringenEmbedded
    {
        // List containing all individual measurement records.
        [JsonPropertyName("maatvoeringen")]
        public List<Maatvoering> Maatvoeringen { get; set; } = new();
    }

    /// <summary>
    /// Represents a single, detailed dimensional measurement or spatial guideline within an urban plan.
    /// </summary>
    public class Maatvoering
    {
        // Unique identification code of the measurement.
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        // The official name or descriptive title of the measurement (e.g., "Minimum setback").
        [JsonPropertyName("naam")]
        public string Naam { get; set; } = string.Empty;

        // List of specific dimensions and values associated with this guideline (e.g., "Min Width: 10m").
        [JsonPropertyName("omvang")]
        public List<Omvang>? Omvang { get; set; }

        // Reference to external text documentation defining the measurement rules.
        [JsonPropertyName("verwijzingNaarTekst")]
        public object? VerwijzingNaarTekst { get; set; }

        // Style identifier used for rendering/displaying the measurement area on a map.
        [JsonPropertyName("styleId")]
        public string? StyleId { get; set; }

        // The geometric boundary (area) covered by this measurement rule.
        [JsonPropertyName("geometrie")]
        public GeoJsonGeometry? Geometrie { get; set; }

        // Link metadata specific to this individual measurement record.
        [JsonPropertyName("_links")]
        public MaatvoeringLinks? Links { get; set; }
    }

    /// <summary>
    /// Represents a single dimensional component, linking a name and a measured value.
    /// </summary>
    public class Omvang
    {
        // The descriptive name of the dimension (e.g., "Width", "Height").
        [JsonPropertyName("naam")]
        public string Naam { get; set; } = string.Empty;

        // The measured value associated with the dimension (including units, if specified).
        [JsonPropertyName("waarde")]
        public string Waarde { get; set; } = string.Empty;
    }

    /// <summary>
    /// Collection of links (references) related to a specific measurement guideline.
    /// </summary>
    public class MaatvoeringLinks
    {
        // Link pointing back to the current resource instance ('self').
        [JsonPropertyName("self")]
        public HrefLink? Self { get; set; }

        // Links referencing supporting textual descriptions associated with this measurement.
        [JsonPropertyName("teksten")]
        public List<HrefLink>? Teksten { get; set; }
    }
}