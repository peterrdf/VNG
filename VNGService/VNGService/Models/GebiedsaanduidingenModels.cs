using System.Text.Json.Serialization;

namespace VNGService.Models
{
    /// <summary>
    /// Represents the full response containing a list of general designated areas within a plan zone.
    /// </summary>
    public class GebiedsaanduidingenResponse
    {
        // Embedded data containing the actual list of area designation records.
        [JsonPropertyName("_embedded")]
        public GebiedsaanduidingenEmbedded Embedded { get; set; } = new();

        // General link metadata for navigating related plan resources.
        [JsonPropertyName("_links")]
        public PlannenResponseLinks Links { get; set; } = new();
    }

    /// <summary>
    /// Contains the embedded collection of area designations found during a query.
    /// </summary>
    public class GebiedsaanduidingenEmbedded
    {
        // List containing all individual area designation records.
        [JsonPropertyName("gebiedsaanduidingen")]
        public List<Gebiedsaanduiding> Gebiedsaanduidingen { get; set; } = new();
    }

    /// <summary>
    /// Represents a single, detailed designated large-scale area within an urban plan.
    /// </summary>
    public class Gebiedsaanduiding
    {
        // Unique identification code of the general area designation.
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        // The official name or title of the designated area.
        [JsonPropertyName("naam")]
        public string Naam { get; set; } = string.Empty;

        // The group classification for this general area designation (e.g., "Commercial Zone").
        [JsonPropertyName("gebiedsaanduidinggroep")]
        public string Gebiedsaanduidinggroep { get; set; } = string.Empty;

        // List of plan articles or sections where this designation applies.
        [JsonPropertyName("artikelnummers")]
        public List<string> Artikelnummers { get; set; } = new();

        // Optional descriptive label information used for display purposes.
        [JsonPropertyName("labelInfo")]
        public string? LabelInfo { get; set; }

        // References to external text documentation defining the area's scope.
        [JsonPropertyName("verwijzingNaarTekst")]
        public List<string> VerwijzingNaarTekst { get; set; } = new();

        // The geometric boundary data (e.g., coordinates, polygon) of the general area.
        [JsonPropertyName("geometrie")]
        public GeoJsonGeometry? Geometrie { get; set; }

        // List of functional purposes included within this large designated area.
        [JsonPropertyName("bestemmingsfuncties")]
        public List<string> Bestemmingsfuncties { get; set; } = new();

        // Style identifier used for rendering/displaying the area on a map.
        [JsonPropertyName("styleId")]
        public string StyleId { get; set; } = string.Empty;

        // Link metadata specific to this individual general area record.
        [JsonPropertyName("_links")]
        public GebiedsaanduidingLinks Links { get; set; } = new();
    }

    /// <summary>
    /// Collection of links (references) related to a specific general designated area.
    /// </summary>
    public class GebiedsaanduidingLinks
    {
        // Link pointing back to the current resource instance ('self').
        [JsonPropertyName("self")]
        public HrefLink Self { get; set; } = new();

        // Links referencing general textual descriptions related to this area.
        [JsonPropertyName("teksten")]
        public List<HrefLink> Teksten { get; set; } = new();

        // Links referencing specific construction guidelines applicable within the zone.
        [JsonPropertyName("bouwaanduidingen")]
        public List<HrefLink> Bouwaanduidingen { get; set; } = new();

        // Links referencing plan figures or diagrams related to this area.
        [JsonPropertyName("figuren")]
        public List<HrefLink> Figuren { get; set; } = new();

        // Links referencing specific functional areas defined within the zone (e.g., commercial function).
        [JsonPropertyName("functieaanduidingen")]
        public List<HrefLink> Functieaanduidingen { get; set; } = new();

        // Links referencing dimensional measurements applicable to this area.
        [JsonPropertyName("maatvoeringen")]
        public List<HrefLink> Maatvoeringen { get; set; } = new();
    }
}