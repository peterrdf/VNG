using System.Text.Json.Serialization;

namespace VNGService.Models
{
    // Structural Vision Areas
    /// <summary>
    /// Represents the full response containing a collection of defined structural vision areas within a plan.
    /// </summary>
    public class StructuurvisiegebiedenResponse
    {
        [JsonPropertyName("_embedded")]
        public StructuurvisiegebiedenEmbedded? Embedded { get; set; }

        // Links metadata for general navigation related to plans.
        [JsonPropertyName("_links")]
        public PlannenResponseLinks? Links { get; set; }
    }

    /// <summary>
    /// Contains the embedded list of structural vision areas found during a query.
    /// </summary>
    public class StructuurvisiegebiedenEmbedded
    {
        [JsonPropertyName("structuurvisiegebieden")]
        // List containing all individual structural vision area records.
        public List<Structuurvisiegebied> Structuurvisiegebieden { get; set; } = new();
    }

    /// <summary>
    /// Represents a single, highly detailed defined geographic area corresponding to a specific part of the structural vision.
    /// </summary>
    public class Structuurvisiegebied
    {
        // Unique identification code for the area designation.
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        // The official name or descriptive title of the area.
        [JsonPropertyName("naam")]
        public string Naam { get; set; } = string.Empty;

        // List of thematic concepts associated with this specific area.
        [JsonPropertyName("thema")]
        public List<string> Thema { get; set; } = new();

        // A list of defined policy statements or principles governing this area.
        [JsonPropertyName("beleid")]
        public List<Beleid> Beleid { get; set; } = new();

        // References to external text documents providing legal definitions and details for the area.
        [JsonPropertyName("verwijzingNaarTekst")]
        public List<string> VerwijzingNaarTekst { get; set; } = new();

        // A list of supporting visual assets or illustrations used in relation to this area.
        [JsonPropertyName("illustraties")]
        public List<Illustratie> Illustraties { get; set; } = new();

        // References to relationships with other external planning documents (if available).
        [JsonPropertyName("relatiesMetExternePlannen")]
        public RelatiesMetExternePlannen RelatiesMetExternePlannen { get; set; } = new();

        // Information regarding cartographic context, such as map numbers and levels.
        [JsonPropertyName("cartografieInfo")]
        public List<CartografieInfo> CartografieInfo { get; set; } = new();

        // Optional style identifier used for rendering/displaying the area on a map.
        [JsonPropertyName("styleId")]
        public string? StyleId { get; set; }

        // The geometric boundary data (coordinates) defining the physical extent of the area.
        [JsonPropertyName("geometrie")]
        public List<GeoJsonGeometry> Geometrie { get; set; } = new();

        // Link metadata specific to this individual structural vision area record.
        [JsonPropertyName("_links")]
        public StructuurvisiegebiedLinks? Links { get; set; }
    }

    /// <summary>
    /// Represents a key policy statement or principle that governs the development of the area.
    /// </summary>
    public class Beleid
    {
        // The core importance or significance of the policy point.
        [JsonPropertyName("belang")]
        public string Belang { get; set; } = string.Empty;

        // The role this policy plays in the overall vision (e.g., "Mandatory", "Guideline").
        [JsonPropertyName("rol")]
        public string Rol { get; set; } = string.Empty;

        // A specific planning instrument that implements this policy (if applicable).
        [JsonPropertyName("instrument")]
        public string? Instrument { get; set; }
    }

    // The following classes are provided here for context but appear to be commented out/legacy in the original file, so I maintain their structure while adding comments.

    /* 
    /// <summary>
    /// Details how this plan relates to other external planning documents (Legacy/Commented Out).
    /// </summary>
    public class RelatiesMetExternePlannen
    {
        [JsonPropertyName("gebruiktInformatieUit")]
        public List<string> GebruiktInformatieUit { get; set; } = new();

        [JsonPropertyName("tenGevolgeVan")]
        public List<string> TenGevolgeVan { get; set; } = new();

        [JsonPropertyName("uitTeWerkenIn")]
        public List<string> UitTeWerkenIn { get; set; } = new();

        [JsonPropertyName("uitgewerktIn")]
        public List<string> UitgewerktIn { get; set; } = new();
    }
    */

    /* 
    /// <summary>
    /// Provides cartographic details for the area, such as map numbers and levels. (Legacy/Commented Out).
    /// </summary>
    public class CartografieInfo
    {
        [JsonPropertyName("kaartnummer")]
        public int Kaartnummer { get; set; }

        [JsonPropertyName("kaartnaam")]
        public string Kaartnaam { get; set; } = string.Empty;

        [JsonPropertyName("niveau")]
        public int Niveau { get; set; }

        [JsonPropertyName("symboolCode")]
        public string? SymboolCode { get; set; }
    }
    */

    /// <summary>
    /// Collection of links (references) related to a specific structural vision area.
    /// </summary>
    public class StructuurvisiegebiedLinks
    {
        // Link pointing back to the current resource instance ('self').
        [JsonPropertyName("self")]
        public HrefLink? Self { get; set; }

        // Links referencing supporting textual descriptions associated with this area.
        [JsonPropertyName("teksten")]
        public List<HrefLink> Teksten { get; set; } = new();
    }
}