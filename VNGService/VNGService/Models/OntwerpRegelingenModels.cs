using System.Text.Json.Serialization;

namespace VNGService.Models
{
    /// <summary>
    /// Represents the response container for draft zoning regulations, detailing planned changes before official adoption.
    /// </summary>
    public class OntwerpRegelingenResponse
    {
        // List containing all retrieved draft regulation records.
        [JsonPropertyName("ontwerpregelingen")]
        public List<OntwerpRegeling> Ontwerpregelingen { get; set; } = new();
    }

    /// <summary>
    /// Represents a single record of an anticipated or proposed zoning regulation.
    /// </summary>
    public class OntwerpRegeling
    {
        // Links metadata specific to this individual draft regulation record.
        [JsonPropertyName("_links")]
        public OntwerpRegelingLinks Links { get; set; } = new();

        // A technical identifier used for system processing or linkage (e.g., internal project ID).
        [JsonPropertyName("technischId")]
        public string TechnischId { get; set; } = string.Empty;

        // The official regulatory plan that this draft regulation is intended to replace (if any).
        [JsonPropertyName("beoogdeOpvolgerVan")]
        public string? BeoogdeOpvolgerVan { get; set; }

        // List of specific planned activities allowed by this draft zoning rule.
        [JsonPropertyName("activiteiten")]
        public List<Activiteit>? Activiteiten { get; set; }

        // List of thematic concepts or topics relevant to this regulation.
        [JsonPropertyName("themas")]
        public List<Thema>? Themas { get; set; }

        // List of specific designated areas covered by this draft regulation.
        [JsonPropertyName("gebiedsaanwijzingen")]
        public List<Gebiedsaanwijzing>? Gebiedsaanwijzingen { get; set; }
    }

    /// <summary>
    /// Collection of links (references) related to a specific draft zoning regulation record.
    /// </summary>
    public class OntwerpRegelingLinks
    {
        // Link pointing back to the current resource instance ('self').
        [JsonPropertyName("ontwerpregeling")]
        public HrefLink Ontwerpregeling { get; set; } = new();

        // Link referencing the structural document used for drafting this regulation.
        [JsonPropertyName("documentstructuur")]
        public HrefLink Documentstructuur { get; set; } = new();

        // Optional link pointing to the regulations that are expected to be superseded by this draft.
        [JsonPropertyName("beoogdeOpvolgerVan")]
        public HrefLink? BeoogdeOpvolgerVan { get; set; }
    }

    /// <summary>
    /// Standard HAL link wrapper containing a URI reference.
    /// </summary>
    public class HrefLink
    {
        // The actual Uniform Resource Identifier (URI) link.
        [JsonPropertyName("href")]
        public string Href { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a specific activity type permitted or restricted within the zoning plan.
    /// </summary>
    public class Activiteit
    {
        // Unique identifier for the activity (e.g., "RES", "COMM").
        [JsonPropertyName("identificatie")]
        public string Identificatie { get; set; } = string.Empty;

        // The descriptive name of the permitted activity.
        [JsonPropertyName("naam")]
        public string Naam { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a thematic concept or topic associated with the regulation.
    /// </summary>
    public class Thema
    {
        // Machine-readable code for the theme (e.g., "ENVIRONMENT").
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        // Human-readable value description of the theme.
        [JsonPropertyName("waarde")]
        public string Waarde { get; set; } = string.Empty;

        // Optional delta or change information related to this theme.
        [JsonPropertyName("_delta")]
        public Delta? Delta { get; set; }
    }

    /// <summary>
    /// Represents a designated area within the plan structure, often derived from zoning regulations.
    /// </summary>
    public class Gebiedsaanwijzing
    {
        // Unique identifier of the designated area.
        [JsonPropertyName("identificatie")]
        public string Identificatie { get; set; } = string.Empty;

        // The descriptive name of the designated area.
        [JsonPropertyName("naam")]
        public string Naam { get; set; } = string.Empty;

        // Optional delta or change information related to this area designation.
        [JsonPropertyName("_delta")]
        public Delta? Delta { get; set; }
    }

    /// <summary>
    /// Represents a generic delta object, used for tracking changes in plan versions.
    /// </summary>
    public class Delta
    {
        // Description of the change that occurred (e.g., "Added", "Modified").
        [JsonPropertyName("bewerking")]
        public string Bewerking { get; set; } = string.Empty;
    }
}