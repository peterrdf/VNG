using System.Text.Json.Serialization;

namespace VNGService.Models
{
    /// <summary>
    /// Represents the full response containing a list of functional area designations within a plan.
    /// </summary>
    public class FunctieaanduidingenResponse
    {
        // Embedded data containing the actual list of function designation records.
        [JsonPropertyName("_embedded")]
        public FunctieaanduidingenEmbedded Embedded { get; set; } = new();

        // General link metadata for navigating related plan resources.
        [JsonPropertyName("_links")]
        public PlannenResponseLinks Links { get; set; } = new();
    }

    /// <summary>
    /// Contains the embedded collection of functional area designations found during a query.
    /// </summary>
    public class FunctieaanduidingenEmbedded
    {
        // List containing all individual function designation records.
        [JsonPropertyName("functieaanduidingen")]
        public List<Functieaanduiding> Functieaanduidingen { get; set; } = new();
    }

    /// <summary>
    /// Represents a single, detailed functional area designation within an urban plan.
    /// This defines the permissible function of a specific zone (e.g., residential, commercial).
    /// </summary>
    public class Functieaanduiding
    {
        // Unique identification code of the function designation area.
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        // The official name or descriptive title of the functional area (e.g., "Residential").
        [JsonPropertyName("naam")]
        public string Naam { get; set; } = string.Empty;

        // Reference to external text documentation providing definitions for this function type.
        [JsonPropertyName("verwijzingNaarTekst")]
        public string? VerwijzingNaarTekst { get; set; }

        // Optional administrative label information used for display purposes.
        [JsonPropertyName("labelInfo")]
        public string? LabelInfo { get; set; }

        // Style identifier used for rendering/displaying the area on a map.
        [JsonPropertyName("styleId")]
        public string? StyleId { get; set; }

        // The geometric boundary data (e.g., coordinates, polygon) defining the functional area.
        [JsonPropertyName("geometrie")]
        public GeoJsonGeometry? Geometrie { get; set; }

        // Link metadata specific to this individual function designation record.
        [JsonPropertyName("_links")]
        public FunctieaanduidingLinks Links { get; set; } = new();
    }

    /// <summary>
    /// Collection of links (references) related to a specific functional area designation.
    /// </summary>
    public class FunctieaanduidingLinks
    {
        // Link pointing back to the current resource instance ('self').
        [JsonPropertyName("self")]
        public LinkInfo Self { get; set; } = new();

        // Links referencing supporting textual descriptions associated with this function.
        [JsonPropertyName("teksten")]
        public List<object> Teksten { get; set; } = new();
    }
}