using System.Text.Json.Serialization;

namespace VNGService.Models
{
    /// <summary>
    /// Represents the full response containing multiple zoning or planning documents (Plans).
    /// </summary>
    public class PlannenResponse
    {
        // Embedded data containing the list of plan records.
        [JsonPropertyName("_embedded")]
        public PlannenEmbedded Embedded { get; set; } = new();

        // General link metadata for navigating the plans response.
        [JsonPropertyName("_links")]
        public PlannenResponseLinks Links { get; set; } = new();
    }

    /// <summary>
    /// Contains the embedded list of plan documents found during a query.
    /// </summary>
    public class PlannenEmbedded
    {
        // List containing all individual plan records.
        [JsonPropertyName("plannen")]
        public List<Plan> Plannen { get; set; } = new();
    }

    /// <summary>
    /// Represents a single planning document or policy area definition.
    /// </summary>
    public class Plan
    {
        // Unique identifier of the plan.
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        // The type classification of the plan (e.g., "Zoning", "Spatial").
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        // The official name of the plan.
        [JsonPropertyName("naam")]
        public string Naam { get; set; } = string.Empty;

        // Information about the governmental body primarily responsible for setting this plan (Policies).
        [JsonPropertyName("beleidsmatigVerantwoordelijkeOverheid")]
        public Overheid BeleidsmatigVerantwoordelijkeOverheid { get; set; } = new();

        // Information about the governmental body that officially published the plan.
        [JsonPropertyName("publicerendBevoegdGezag")]
        public Overheid PublicerendBevoegdGezag { get; set; } = new();

        // List of names/titles covered by this plan (e.g., "Residential Area A", "Commercial Zone B").
        [JsonPropertyName("locatienamen")]
        public List<string> Locatienamen { get; set; } = new();

        // Information regarding the current status and date of the plan.
        [JsonPropertyName("planstatusInfo")]
        public PlanstatusInfo PlanstatusInfo { get; set; } = new();

        // A list of supporting visual assets or illustrations associated with the plan.
        [JsonPropertyName("illustraties")]
        public List<Illustratie> Illustraties { get; set; } = new();

        // Optional reference to the formal decision decree that established the plan.
        [JsonPropertyName("verwijzingNaarVaststellingsbesluit")]
        public string? VerwijzingNaarVaststellingsbesluit { get; set; }

        // The unique official number of the planning decision document.
        [JsonPropertyName("besluitnummer")]
        public string? Besluitnummer { get; set; }

        // List of external standards or regulations referenced by this plan.
        [JsonPropertyName("verwijzingNorm")]
        public List<string> VerwijzingNorm { get; set; } = new();

        // List of names/identifiers of the originating authorities whose norms are used in this plan.
        [JsonPropertyName("normadressant")]
        public List<string> Normadressant { get; set; } = new();

        // List of underlying geological or subsurface data layers included in the plan.
        [JsonPropertyName("ondergronden")]
        public List<Ondergrond> Ondergronden { get; set; } = new();

        // List of associated major sub-components that make up this overall plan document.
        [JsonPropertyName("heeftOnderdelen")]
        public List<PlanOnderdeel> HeeftOnderdelen { get; set; } = new();

        // The legal or regulatory status of the plan (e.g., "Active", "Draft").
        [JsonPropertyName("regelStatus")]
        public string? RegelStatus { get; set; }

        // Determines the binding nature of the regulations within this plan.
        [JsonPropertyName("regelBinding")]
        public string RegelBinding { get; set; } = string.Empty;

        // Indicates if the plan is historical or archived.
        [JsonPropertyName("isHistorisch")]
        public bool IsHistorisch { get; set; }

        // Date when the plan was officially withdrawn or superseded.
        [JsonPropertyName("verwijderdOp")]
        public string? VerwijderdOp { get; set; }

        // Indicates if this is a temporary planning version (TAM Plan).
        [JsonPropertyName("isTamPlan")]
        public bool IsTamPlan { get; set; }

        // Information regarding appeal and objection rights associated with the plan.
        [JsonPropertyName("beroepEnBezwaar")]
        public string BeroepEnBezwaar { get; set; } = string.Empty;

        // Indicates if the plan acts as a comprehensive umbrella plan (Parapluplan).
        [JsonPropertyName("isParapluplan")]
        public bool IsParapluplan { get; set; }

        // Date when the legal validity of the plan expires.
        [JsonPropertyName("eindeRechtsgeldigheid")]
        public string? EindeRechtsgeldigheid { get; set; }

        // The geometric boundary or footprint of the entire planning area.
        [JsonPropertyName("geometrie")]
        public GeoJsonGeometry? Geometrie { get; set; }

        // Details on relationships with other external (non-local) plans.
        [JsonPropertyName("relatiesMetExternePlannen")]
        public PlanRelaties RelatiesMetExternePlannen { get; set; } = new();

        // Details on relationships originating from this plan to other sources.
        [JsonPropertyName("relatiesVanuitExternePlannen")]
        public PlanRelaties RelatiesVanuitExternePlannen { get; set; } = new();

        // Container for administrative dossier information related to the plan's creation and history.
        [JsonPropertyName("dossier")]
        public Dossier Dossier { get; set; } = new();

        // Link metadata specific to this individual plan record.
        [JsonPropertyName("_links")]
        public PlanLinks Links { get; set; } = new();

        // Reference URL pointing to the GML (Geography Markup Language) source data.
        [JsonPropertyName("verwijzingNaarGml")]
        public string VerwijzingNaarGml { get; set; } = string.Empty;
    }

    /// <summary>
    /// Information about a governmental authority, detailing its type and jurisdiction.
    /// </summary>
    public class Overheid
    {
        // Type classification of the government body (e.g., "National", "Municipal").
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        // Internal code for the government body.
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        // The official name of the governmental authority.
        [JsonPropertyName("naam")]
        public string Naam { get; set; } = string.Empty;
    }

    /// <summary>
    /// Provides status and date information about the planning document.
    /// </summary>
    public class PlanstatusInfo
    {
        // The current operational status of the plan (e.g., "Active", "Draft").
        [JsonPropertyName("planstatus")]
        public string Planstatus { get; set; } = string.Empty;

        // Date when the current status was recorded or effective.
        [JsonPropertyName("datum")]
        public string Datum { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a supporting visual asset (image, map snippet) related to the plan.
    /// </summary>
    public class Illustratie
    {
        // URI link to access the illustration file or resource.
        [JsonPropertyName("href")]
        public string Href { get; set; } = string.Empty;

        // Type classification of the illustration (e.g., "Aerial", "Map").
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        // Optional human-readable name for the illustration.
        [JsonPropertyName("naam")]
        public string? Naam { get; set; }

        // Name used in the map legend.
        [JsonPropertyName("legendanaam")]
        public string? Legendanaam { get; set; }
    }

    /// <summary>
    /// Represents a layer of subsurface data relevant to planning (e.g., groundwater, utilities).
    /// </summary>
    public class Ondergrond
    {
        // Type classification of the sub-surface data (e.g., "Utility Line", "Groundwater Level").
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        // Date associated with this subsurface model layer.
        [JsonPropertyName("datum")]
        public string Datum { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a major component or subsystem that is included within the overall plan document (e.g., "Zoning Layer", "Utility Network").
    /// </summary>
    public class PlanOnderdeel
    {
        // Type classification of the plan component.
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        // List of external references pointing to related datasets or documents.
        [JsonPropertyName("externeReferenties")]
        public List<string> ExterneReferenties { get; set; } = new();

        // Detailed text descriptions that apply specifically to objects within this component.
        [JsonPropertyName("heeftObjectgerichteTeksten")]
        public List<ObjectgerichteTekst> HeeftObjectgerichteTeksten { get; set; } = new();
    }

    /// <summary>
    /// A text description tied directly to a specific object within the plan.
    /// </summary>
    public class ObjectgerichteTekst
    {
        // Type classification of the descriptive text.
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        // The main title or subject of the text block.
        [JsonPropertyName("titel")]
        public string Titel { get; set; } = string.Empty;

        // URI link to access the full source documentation for the text.
        [JsonPropertyName("href")]
        public string Href { get; set; } = string.Empty;
    }

    /// <summary>
    /// Defines how this plan relates to other plans in the planning hierarchy (e.g., "Uses Information From", "Replaces").
    /// </summary>
    public class PlanRelaties
    {
        // References planning information drawn from other sources.
        [JsonPropertyName("gebruiktInformatieUit")]
        public List<PlanReferentie> GebruiktInformatieUit { get; set; } = new();

        // Indicates partial revision from another plan.
        [JsonPropertyName("gedeeltelijkeHerzieningVan")]
        public List<PlanReferentie> GedeeltelijkeHerzieningVan { get; set; } = new();

        // Indicates that this plan modifies or changes existing regulations in a previous plan.
        [JsonPropertyName("muteert")]
        public List<PlanReferentie> Muteert { get; set; } = new();

        // Indicates that the regulation is resulting from a change defined in another plan.
        [JsonPropertyName("tenGevolgeVan")]
        public List<PlanReferentie> TenGevolgeVan { get; set; } = new();

        // Indicates plans that need to be included or factored into this plan's scope.
        [JsonPropertyName("uitTeWerkenIn")]
        public List<PlanReferentie> UitTeWerkenIn { get; set; } = new();

        // Indicates plans that have already incorporated the contents of this plan.
        [JsonPropertyName("uitgewerktIn")]
        public List<PlanReferentie> UitgewerktIn { get; set; } = new();

        // Indicates a direct replacement relationship with another previous plan.
        [JsonPropertyName("vervangt")]
        public List<PlanReferentie> Vervangt { get; set; } = new();
    }

    /// <summary>
    /// A standardized reference to another planning document, used within the PlanRelaties structure.
    /// </summary>
    public class PlanReferentie
    {
        // Descriptive name of the referenced plan.
        [JsonPropertyName("naam")]
        public string Naam { get; set; } = string.Empty;

        // Unique identifier of the referenced plan.
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        // The full dossier information (metadata) of the referenced plan.
        [JsonPropertyName("dossier")]
        public Dossier Dossier { get; set; } = new();

        // Status and date info related to the referenced plan.
        [JsonPropertyName("planstatusInfo")]
        public PlanstatusInfo PlanstatusInfo { get; set; } = new();

        // URI link pointing to the full details of the referenced plan.
        [JsonPropertyName("href")]
        public string Href { get; set; } = string.Empty;
    }

    /// <summary>
    /// Contains administrative metadata about the entire plan dossier or project folder.
    /// </summary>
    public class Dossier
    {
        // Unique identifier for the plan's administrative dossier.
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        // Current overall status of the plan documentation (e.g., "Draft", "Approved").
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Collection of general links related to this planning document instance.
    /// </summary>
    public class PlanLinks
    {
        // Link pointing back to the current resource instance ('self').
        [JsonPropertyName("self")]
        public HrefLink Self { get; set; } = new();
    }

    /// <summary>
    /// General pagination links for fetching multiple plans.
    /// </summary>
    public class PlannenResponseLinks
    {
        // Link to the next page of results.
        [JsonPropertyName("next")]
        public HrefLink? Next { get; set; }

        // Link pointing back to the current resource instance ('self').
        [JsonPropertyName("self")]
        public HrefLink Self { get; set; } = new();
    }
}