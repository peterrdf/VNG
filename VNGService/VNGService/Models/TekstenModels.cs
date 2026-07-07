using System.Text.Json.Serialization;

namespace VNGService.Models
{
    /// <summary>
    /// Represents the full response containing all available textual descriptions or explanatory texts for a plan.
    /// </summary>
    public class TekstenResponse
    {
        // Embedded data containing the list of text records.
        [JsonPropertyName("_embedded")]
        public TekstenEmbedded Embedded { get; set; } = new();

        // General link metadata for navigating the plans response.
        [JsonPropertyName("_links")]
        public PlannenResponseLinks Links { get; set; } = new();
    }

    /// <summary>
    /// Contains the embedded list of text descriptions found during a query.
    /// </summary>
    public class TekstenEmbedded
    {
        // List containing all individual text records.
        [JsonPropertyName("teksten")]
        public List<Tekst> Teksten { get; set; } = new();
    }

    /// <summary>
    /// Represents a single, detailed textual description unit within the planning documentation.
    /// </summary>
    public class Tekst
    {
        // Unique identifier of the text document/section.
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        // The primary title or subject matter of the text.
        [JsonPropertyName("titel")]
        public string Titel { get; set; } = string.Empty;

        // The main body content of the explanatory text (the narrative).
        [JsonPropertyName("inhoud")]
        public string? Inhoud { get; set; }

        // Sequential number or chapter/section number within the document.
        [JsonPropertyName("volgnummer")]
        public int Volgnummer { get; set; }

        // Reference to an external source or primary document related to this text.
        [JsonPropertyName("externeReferentie")]
        public ExterneReferentie? ExterneReferentie { get; set; }

        // Breadcrumb trail showing the hierarchical location of this text within the plan structure.
        [JsonPropertyName("kruimelpad")]
        public List<Kruimelpad> Kruimelpad { get; set; } = new();

        // Link metadata specific to this individual text record.
        [JsonPropertyName("_links")]
        public TekstLinks Links { get; set; } = new();
    }

    /// <summary>
    /// Contains external reference details for the main text document.
    /// </summary>
    public class ExterneReferentie
    {
        // URL linking to the external information object.
        [JsonPropertyName("informatieobjectUrl")]
        public string? InformatieobjectUrl { get; set; }

        // Human-readable label describing the external information object.
        [JsonPropertyName("informatieobjectLabel")]
        public string? InformatieobjectLabel { get; set; }

        // A document number used for cross-referencing with external dossiers.
        [JsonPropertyName("dossiernummer")]
        public string? Dossiernummer { get; set; }
    }

    /// <summary>
    /// Represents a step in the textual hierarchy (breadcrumb).
    /// </summary>
    public class Kruimelpad
    {
        // Unique identifier for this breadcrumb segment.
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        // The display title of the section or chapter represented by this step.
        [JsonPropertyName("titel")]
        public string Titel { get; set; } = string.Empty;

        // Sequential number within the breadcrumb path.
        [JsonPropertyName("volgnummer")]
        public int Volgnummer { get; set; }
    }

    /// <summary>
    /// Collection of various hypermedia links (references) pointing to related plan elements from this text description.
    /// </summary>
    public class TekstLinks
    {
        // Link pointing back to the current resource instance ('self').
        [JsonPropertyName("self")]
        public HrefLink Self { get; set; } = new();

        // Links to child sections or sub-topics within this text.
        [JsonPropertyName("children")]
        public List<HrefLink> Children { get; set; } = new();

        // Link referencing the bestemmingsvlakken (zoning zones) covered by this text.
        [JsonPropertyName("bestemmingsvlakken")]
        public List<HrefLink> Bestemmingsvlakken { get; set; } = new();

        // Link referencing associated building guidelines.
        [JsonPropertyName("bouwaanduidingen")]
        public List<HrefLink> Bouwaanduidingen { get; set; } = new();

        // Link referencing plan figures or diagrams illustrated by this text.
        [JsonPropertyName("figuren")]
        public List<HrefLink> Figuren { get; set; } = new();

        // Link referencing functional areas defined in the plan.
        [JsonPropertyName("functieaanduidingen")]
        public List<HrefLink> Functieaanduidingen { get; set; } = new();

        // Link referencing general designated areas covered by this text.
        [JsonPropertyName("gebiedsaanduidingen")]
        public List<HrefLink> Gebiedsaanduidingen { get; set; } = new();

        // Link referencing dimensional measurements defined in the plan.
        [JsonPropertyName("maatvoeringen")]
        public List<HrefLink> Maatvoeringen { get; set; } = new();

        // Link referencing specific decision zones/plots (besluitvlakken).
        [JsonPropertyName("besluitvlakken")]
        public List<HrefLink> Besluitvlakken { get; set; } = new();

        // Link referencing sub-decision areas.
        [JsonPropertyName("besluitsubvlakken")]
        public List<HrefLink> Besluitsubvlakken { get; set; } = new();

        // Link referencing structural vision complexes related to this text.
        [JsonPropertyName("structuurvisiecomplexen")]
        public List<HrefLink> Structuurvisiecomplexen { get; set; } = new();

        // Link referencing structural vision areas covered by this text.
        [JsonPropertyName("structuurvisiegebieden")]
        public List<HrefLink> Structuurvisiegebieden { get; set; } = new();

        // Link referencing detailed statements/rationales for the structural vision.
        [JsonPropertyName("structuurvisieverklaringen")]
        public List<HrefLink> Structuurvisieverklaringen { get; set; } = new();

        // Link referencing areas defined by lettering (e.g., "Sector A").
        [JsonPropertyName("lettertekenaanduidingen")]
        public List<HrefLink> Lettertekenaanduidingen { get; set; } = new();
    }
}