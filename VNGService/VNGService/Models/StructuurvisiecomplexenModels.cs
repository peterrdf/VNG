using System.Drawing.Drawing2D;
using System.Text.Json.Serialization;

namespace VNGService.Models
{
    /// <summary>
    /// Represents the full response containing a collection of structural vision complexes, which are high-level planning concepts.
    /// </summary>
    public class StructuurvisiecomplexenResponse
    {
        // Embedded data containing the list of structural vision complex records.
        [JsonPropertyName("_embedded")]
        public StructuurvisiecomplexenEmbedded? Embedded { get; set; }

        // General link metadata for navigating related plan resources.
        [JsonPropertyName("_links")]
        public PlannenResponseLinks? Links { get; set; }
    }

    /// <summary>
    /// Contains the embedded list of structural vision complexes found during a query.
    /// </summary>
    public class StructuurvisiecomplexenEmbedded
    {
        // List containing all individual structural vision complex records.
        [JsonPropertyName("structuurvisiecomplexen")]
        public List<Structuurvisiecomplex> Structuurvisiecomplexen { get; set; } = new();
    }

    /// <summary>
    /// Represents a single, high-level conceptual planning framework or structural vision for an area.
    /// </summary>
    public class Structuurvisiecomplex
    {
        // Unique identification code of the complex plan record.
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        // The official name or descriptive title of the structural vision.
        [JsonPropertyName("naam")]
        public string Naam { get; set; } = string.Empty;

        // List of thematic concepts or topics addressed by this complex plan.
        [JsonPropertyName("thema")]
        public List<string> Thema { get; set; } = new();

        // List of specific policy statements, principles, or guidelines adopted in the vision.
        [JsonPropertyName("beleid")]
        public List<Beleid> Beleid { get; set; } = new();

        // References to external text documentation supporting the structural vision plan.
        [JsonPropertyName("verwijzingNaarTekst")]
        public List<string> VerwijzingNaarTekst { get; set; } = new();

        // A list of visual assets or illustrations used to depict the vision (e.g., conceptual maps).
        [JsonPropertyName("illustraties")]
        public List<Illustratie> Illustraties { get; set; } = new();

        // References to relationships with other external planning documents.
        [JsonPropertyName("relatiesMetExternePlannen")]
        public RelatiesMetExternePlannen RelatiesMetExternePlannen { get; set; } = new();

        // Information regarding cartographic details, like map numbers and scales used in the vision.
        [JsonPropertyName("cartografieInfo")]
        public List<CartografieInfo> CartografieInfo { get; set; } = new();

        // Optional style identifier used for rendering/displaying the complex area on a map.
        [JsonPropertyName("styleId")]
        public string? StyleId { get; set; }

        // The geometric boundary data (coordinates) defining the entire structural vision area.
        [JsonPropertyName("geometrie")]
        public List<GeoJsonGeometry> Geometrie { get; set; } = new();

        // Link metadata specific to this individual structural vision complex record.
        [JsonPropertyName("_links")]
        public StructuurvisiecomplexLinks? Links { get; set; }
    }

    /// <summary>
    /// Details how this plan relates to other external planning documents in the ecosystem.
    /// </summary>
    public class RelatiesMetExternePlannen
    {
        // List of plans that this vision draws information from (e.g., "Uses Information From").
        [JsonPropertyName("gebruiktInformatieUit")]
        public List<string> GebruiktInformatieUit { get; set; } = new();

        // List of planned changes derived from or following another plan ("Follows From").
        [JsonPropertyName("tenGevolgeVan")]
        public List<string> TenGevolgeVan { get; set; } = new();

        // List of plans that need to incorporate information into this vision ("To Be Developed In").
        [JsonPropertyName("uitTeWerkenIn")]
        public List<string> UitTeWerkenIn { get; set; } = new();

        // List of plans already incorporating elements from this vision ("Developed In").
        [JsonPropertyName("uitgewerktIn")]
        public List<string> UitgewerktIn { get; set; } = new();
    }

    /// <summary>
    /// Provides administrative information related to the map context for the structural vision.
    /// </summary>
    public class CartografieInfo
    {
        // The official map number associated with this layer/view.
        [JsonPropertyName("kaartnummer")]
        public int Kaartnummer { get; set; }

        // The descriptive name of the map sheet or card.
        [JsonPropertyName("kaartnaam")]
        public string Kaartnaam { get; set; } = string.Empty;

        // The structural level or zoom depth at which this data is visible.
        [JsonPropertyName("niveau")]
        public int Niveau { get; set; }

        // A specific code used for symbology on the map.
        [JsonPropertyName("symboolCode")]
        public string SymboolCode { get; set; } = string.Empty;
    }

    /// <summary>
    /// Collection of links (references) related to a specific structural vision complex.
    /// </summary>
    public class StructuurvisiecomplexLinks
    {
        // Link pointing back to the current resource instance ('self').
        [JsonPropertyName("self")]
        public HrefLink Self { get; set; } = new();

        // Links referencing supporting textual descriptions (e.g., rationale).
        [JsonPropertyName("teksten")]
        public List<HrefLink> Teksten { get; set; } = new();

        // Links pointing to specific structural vision areas that make up this complex.
        [JsonPropertyName("structuurvisiegebieden")]
        public List<HrefLink> Structuurvisiegebieden { get; set; } = new();

        // Links referencing detailed structural vision explanations or rationale documents.
        [JsonPropertyName("structuurvisieverklaringen")]
        public List<HrefLink> Structuurvisieverklaringen { get; set; } = new();
    }
}