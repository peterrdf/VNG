using System.Text.Json.Serialization;

namespace VNGService.Models
{
    /// <summary>
    /// Represents the full response containing a list of designated building plots or developable areas (Bouwvlakken).
    /// </summary>
    public class BouwvlakkenResponse
    {
        // Embedded data containing the actual list of building plot records.
        [JsonPropertyName("_embedded")]
        public BouwvlakkenEmbedded? Embedded { get; set; }

        // General link metadata for navigating the plan records.
        [JsonPropertyName("_links")]
        public PlannenResponseLinks? Links { get; set; }
    }

    /// <summary>
    /// Contains the embedded collection of building plots found during a query.
    /// </summary>
    public class BouwvlakkenEmbedded
    {
        // List containing all individual building plot records.
        [JsonPropertyName("bouwvlakken")]
        public List<Bouwvlak> Bouwvlakken { get; set; } = new();
    }

    /// <summary>
    /// Represents a single, detailed designated building plot (a demarcated area suitable for construction).
    /// </summary>
    public class Bouwvlak
    {
        // Unique identification code of the building plot.
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        // The official name or identifier of the plot/area.
        [JsonPropertyName("naam")]
        public string Naam { get; set; } = string.Empty;

        // Style identifier used for rendering/displaying the plot on a map.
        [JsonPropertyName("styleId")]
        public string StyleId { get; set; } = string.Empty;

        // The geometric boundary data (e.g., coordinates, polygon) of the plot.
        [JsonPropertyName("geometrie")]
        public GeoJsonGeometry? Geometrie { get; set; }

        // Link metadata specific to this individual building plot record.
        [JsonPropertyName("_links")]
        public BouwvlakLinks? Links { get; set; }
    }

    /// <summary>
    /// Collection of links (references) related to a specific building plot, linking to various details within the plan.
    /// </summary>
    public class BouwvlakLinks
    {
        // Link pointing back to the current resource instance ('self').
        [JsonPropertyName("self")]
        public HrefLink? Self { get; set; }

        // Links referencing construction guidelines found within this plot.
        [JsonPropertyName("bouwaanduidingen")]
        public List<HrefLink> Bouwaanduidingen { get; set; } = new();

        // Links referencing defined figures or diagrams associated with the plot.
        [JsonPropertyName("figuren")]
        public List<HrefLink> Figuren { get; set; } = new();

        // Links referencing alphabetical or symbolic notations used within the plot boundaries.
        [JsonPropertyName("lettertekenaanduidingen")]
        public List<HrefLink> Lettertekenaanduidingen { get; set; } = new();

        // Links referencing specific dimensional measurements recorded for the plot.
        [JsonPropertyName("maatvoeringen")]
        public List<HrefLink> Maatvoeringen { get; set; } = new();
    }
}