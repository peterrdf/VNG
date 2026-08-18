using System.Text.Json.Serialization;
using System.Linq; // Added for Enumeral extension methods used in calculated properties

namespace VNGService.Models
{
    /// <summary>
    /// Response returned by the DSO "regelingen zoeken" (search regulations) endpoint.
    /// </summary>
    public class DsoRegelingenZoekResponse
    {
        /// <summary>
        /// HAL "_embedded" section containing the actual list of regulations.
        /// </summary>
        [JsonPropertyName("_embedded")]
        public DsoRegelingenEmbedded? Embedded { get; set; }

        /// <summary>
        /// Convenience accessor returning all regulations, or an empty sequence if none are present.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<DsoRegeling> AllRegelingen =>
            Embedded?.Regelingen ?? Enumerable.Empty<DsoRegeling>();
    }

    /// <summary>
    /// HAL "_embedded" wrapper containing the list of regulations ("Regelingen").
    /// </summary>
    public class DsoRegelingenEmbedded
    {
        /// <summary>
        /// The list of regulations returned by the search.
        /// </summary>
        public List<DsoRegeling>? Regelingen { get; set; }
    }

    /// <summary>
    /// Represents a single regulation ("Regeling") as returned by the DSO API.
    /// </summary>
    public class DsoRegeling
    {
        /// <summary>
        /// Unique AKN identifier of the regulation.
        /// </summary>
        public string? Identificatie { get; set; } = null;

        /// <summary>
        /// Official title of the regulation.
        /// </summary>
        public string? OfficieleTitel { get; set; } = null;

        /// <summary>
        /// Short/citation title of the regulation.
        /// </summary>
        public string? CiteerTitel { get; set; } = null;

        /// <summary>
        /// Type/category of the regulation.
        /// </summary>
        public DsoRegelingType? Type { get; set; } = null;

        /// <summary>
        /// Information about the authority that provided/published the regulation.
        /// </summary>
        public DsoAangeleverdDoorEen? AangeleverdDoorEen { get; set; } = null;

        /// <summary>
        /// AKN => BFF, e.g. "/akn/nl/act/gm0995/2020/omgevingsplan" => "nl_act_gm0995_2020_omgevingsplan". 
        /// </summary>
        [JsonIgnore]
        public string? BFFId => Identificatie?.Replace('/', '_');
    }

    /// <summary>
    /// Code/value pair describing the type of a regulation.
    /// </summary>
    public class DsoRegelingType
    {
        /// <summary>
        /// Machine-readable code for the type.
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// Human-readable value/description for the type.
        /// </summary>
        public string? Waarde { get; set; }
    }

    /// <summary>
    /// Code/value pair describing the group ("Groep") a regulation, norm, or value belongs to.
    /// </summary>
    public class DsoRegelingGroep
    {
        /// <summary>
        /// Machine-readable code for the group.
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// Human-readable value/description for the group.
        /// </summary>
        public string? Waarde { get; set; }
    }

    /// <summary>
    /// Generic reusable {code, waarde} pair used throughout the DSO API
    /// (idealisatie, thema, eenheid, activiteitregelkwalificatie, instructieregelTaakuitoefening,
    /// instructieregelInstrument, etc.).
    /// </summary>
    public class DsoCodeWaarde
    {
        /// <summary>
        /// Machine-readable code.
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// Human-readable value/description.
        /// </summary>
        public string? Waarde { get; set; }
    }

    /// <summary>
    /// Information about the authority ("bestuursorgaan") that delivered/published a regulation.
    /// </summary>
    public class DsoAangeleverdDoorEen
    {
        /// <summary>
        /// Name of the authority.
        /// </summary>
        public string? Naam { get; set; }

        /// <summary>
        /// Administrative layer/level of the authority (e.g. municipality, province).
        /// </summary>
        public string? Bestuurslaag { get; set; }

        /// <summary>
        /// Code identifying the authority.
        /// </summary>
        public string? Code { get; set; } = null;
    }

    /// <summary>
    /// Response returned by the DSO "locaties zoeken" (search locations) endpoint.
    /// </summary>
    public class DsoLocatieZoekResponse
    {
        /// <summary>
        /// HAL "_embedded" section containing the actual list of locations.
        /// </summary>
        [JsonPropertyName("_embedded")]
        public LocatieZoekEmbedded? Embedded { get; set; }

        /// <summary>
        /// Pagination information for the search results.
        /// </summary>
        public PageInfo? Page { get; set; } = null;
    }

    /// <summary>
    /// HAL "_embedded" wrapper containing the list of locations ("Locaties").
    /// </summary>
    public class LocatieZoekEmbedded
    {
        /// <summary>
        /// The list of locations returned by the search.
        /// </summary>
        public List<LocatieItem>? Locaties { get; set; } = null;
    }

    /// <summary>
    /// Represents a single location item as returned by the location search endpoint.
    /// </summary>
    public class LocatieItem
    {
        /// <summary>
        /// Unique identifier of the location.
        /// </summary>
        public string? Identificatie { get; set; }

        /// <summary>
        /// Display name ("Noemer") of the location.
        /// </summary>
        public string? Noemer { get; set; }

        /// <summary>
        /// Type of the location (e.g. gebied, object).
        /// </summary>
        public string? LocatieType { get; set; }
    }

    /// <summary>
    /// Versioning/registration metadata attached to almost every object returned
    /// by the "regeltekstannotaties" endpoint.
    /// </summary>
    public class DsoGeregistreerdMet
    {
        /// <summary>
        /// Version number of the registered object.
        /// </summary>
        public int? Versie { get; set; }

        /// <summary>
        /// Date on which the object takes legal effect ("inwerkingtreding").
        /// </summary>
        public DateOnly? BeginInwerking { get; set; }

        /// <summary>
        /// Start date of validity of the object.
        /// </summary>
        public DateOnly? BeginGeldigheid { get; set; }

        /// <summary>
        /// End date of validity of the object.
        /// </summary>
        public DateOnly? EindGeldigheid { get; set; }

        /// <summary>
        /// Timestamp at which the object was registered.
        /// </summary>
        public DateTimeOffset? TijdstipRegistratie { get; set; }

        /// <summary>
        /// Timestamp at which the registration ended, if applicable.
        /// </summary>
        public DateTimeOffset? EindRegistratie { get; set; }
    }

    /// <summary>
    /// Response returned by the DSO "regeltekstannotaties" (rule text annotations) endpoint,
    /// containing all annotation objects related to a regulation's rule text.
    /// </summary>
    public class DsoRegeltekstAnnotatiesResponse
    {
        /// <summary>
        /// HAL "_links" section with related resource links.
        /// </summary>
        [JsonPropertyName("_links")]
        public DsoRegeltekstAnnotatiesLinks? Links { get; set; }

        /// <summary>
        /// List of rule text fragments ("Regelteksten").
        /// </summary>
        public List<DsoRegeltekst>? Regelteksten { get; set; }

        /// <summary>
        /// List of generic rules applicable to everyone ("RegelsVoorIedereen").
        /// </summary>
        public List<DsoRegelVoorIedereen>? RegelsVoorIedereen { get; set; }

        /// <summary>
        /// List of environmental value rules ("Omgevingswaarderegels").
        /// </summary>
        public List<DsoOmgevingswaarderegel>? Omgevingswaarderegels { get; set; }

        /// <summary>
        /// List of instruction rules ("Instructieregels").
        /// </summary>
        public List<DsoInstructieregel>? Instructieregels { get; set; }

        /// <summary>
        /// List of area designations ("Gebiedsaanwijzingen").
        /// </summary>
        public List<DsoGebiedsaanwijzing>? Gebiedsaanwijzingen { get; set; }

        /// <summary>
        /// List of location references ("Locaties").
        /// </summary>
        public List<DsoLocatieRef>? Locaties { get; set; }

        /// <summary>
        /// List of activities ("Activiteiten").
        /// </summary>
        public List<DsoActiviteit>? Activiteiten { get; set; }

        /// <summary>
        /// List of environmental norms ("Omgevingsnormen").
        /// </summary>
        public List<DsoOmgevingsnorm>? Omgevingsnormen { get; set; }

        /// <summary>
        /// List of environmental values ("Omgevingswaarden").
        /// </summary>
        public List<DsoOmgevingswaarde>? Omgevingswaarden { get; set; }

        /// <summary>
        /// List of maps ("Kaarten").
        /// </summary>
        public List<DsoKaart>? Kaarten { get; set; }
    }

    /// <summary>
    /// HAL links associated with a rule text annotations response.
    /// </summary>
    public class DsoRegeltekstAnnotatiesLinks
    {
        /// <summary>
        /// Link to the current resource itself.
        /// </summary>
        public DsoHalLink? Self { get; set; }

        /// <summary>
        /// Link to the related regulation ("Regeling").
        /// </summary>
        public DsoHalLink? Regeling { get; set; }

        /// <summary>
        /// Link to the document structure ("Documentstructuur") of the regulation.
        /// </summary>
        public DsoHalLink? Documentstructuur { get; set; }
    }

    /// <summary>
    /// Represents a single fragment of rule text ("Regeltekst").
    /// </summary>
    public class DsoRegeltekst
    {
        /// <summary>
        /// Unique identifier of the rule text.
        /// </summary>
        public string? Identificatie { get; set; }

        /// <summary>
        /// Work identifier ("WId") referencing the position within the source document.
        /// </summary>
        public string? WId { get; set; }

        /// <summary>
        /// Registration/versioning metadata for this rule text.
        /// </summary>
        public DsoGeregistreerdMet? GeregistreerdMet { get; set; }
    }

    /// <summary>
    /// Represents a rule applicable to everyone ("RegelVoorIedereen").
    /// </summary>
    public class DsoRegelVoorIedereen
    {
        /// <summary>
        /// Unique identifier of the rule.
        /// </summary>
        public string? Identificatie { get; set; }

        /// <summary>
        /// Idealization/classification of the rule.
        /// </summary>
        public DsoCodeWaarde? Idealisatie { get; set; }

        /// <summary>
        /// Reference to the associated rule text ("RegeltekstRef").
        /// </summary>
        public string? RegeltekstRef { get; set; }

        /// <summary>
        /// Themes ("Themas") associated with the rule.
        /// </summary>
        public List<DsoCodeWaarde>? Themas { get; set; }

        /// <summary>
        /// Activity location indications associated with the rule.
        /// </summary>
        public List<DsoActiviteitLocatieaanduiding>? ActiviteitLocatieaanduidingen { get; set; }

        /// <summary>
        /// References to related locations.
        /// </summary>
        public List<string>? LocatieRefs { get; set; }

        /// <summary>
        /// References to related area designations.
        /// </summary>
        public List<string>? GebiedsaanwijzingRefs { get; set; }

        /// <summary>
        /// References to related environmental norms.
        /// </summary>
        public List<string>? OmgevingsnormRefs { get; set; }

        /// <summary>
        /// References to related maps.
        /// </summary>
        public List<string>? KaartRefs { get; set; }

        /// <summary>
        /// Registration/versioning metadata for this rule.
        /// </summary>
        public DsoGeregistreerdMet? GeregistreerdMet { get; set; }
    }

    /// <summary>
    /// Represents an activity location indication ("ActiviteitLocatieaanduiding"),
    /// linking an activity to one or more locations with a qualifier.
    /// </summary>
    public class DsoActiviteitLocatieaanduiding
    {
        /// <summary>
        /// Unique identifier of the indication.
        /// </summary>
        public string? Identificatie { get; set; }

        /// <summary>
        /// Reference to the related activity ("ActiviteitRef").
        /// </summary>
        public string? ActiviteitRef { get; set; }

        /// <summary>
        /// Qualification of the activity rule (e.g. permitted/forbidden).
        /// </summary>
        public DsoCodeWaarde? Activiteitregelkwalificatie { get; set; }

        /// <summary>
        /// References to related locations.
        /// </summary>
        public List<string>? LocatieRefs { get; set; }

        /// <summary>
        /// Symbolization/styling information for map rendering.
        /// </summary>
        public DsoSymbolisatie? Symbolisatie { get; set; }
    }

    /// <summary>
    /// Represents an environmental value rule ("Omgevingswaarderegel").
    /// </summary>
    public class DsoOmgevingswaarderegel
    {
        /// <summary>
        /// Unique identifier of the rule.
        /// </summary>
        public string? Identificatie { get; set; }

        /// <summary>
        /// Idealization/classification of the rule.
        /// </summary>
        public DsoCodeWaarde? Idealisatie { get; set; }

        /// <summary>
        /// Reference to the associated rule text.
        /// </summary>
        public string? RegeltekstRef { get; set; }

        /// <summary>
        /// Themes associated with the rule.
        /// </summary>
        public List<DsoCodeWaarde>? Themas { get; set; }

        /// <summary>
        /// References to related locations.
        /// </summary>
        public List<string>? LocatieRefs { get; set; }

        /// <summary>
        /// References to related area designations.
        /// </summary>
        public List<string>? GebiedsaanwijzingRefs { get; set; }

        /// <summary>
        /// References to related environmental values.
        /// </summary>
        public List<string>? OmgevingswaardeRefs { get; set; }

        /// <summary>
        /// References to related maps.
        /// </summary>
        public List<string>? KaartRefs { get; set; }

        /// <summary>
        /// Registration/versioning metadata for this rule.
        /// </summary>
        public DsoGeregistreerdMet? GeregistreerdMet { get; set; }
    }

    /// <summary>
    /// Represents an instruction rule ("Instructieregel").
    /// </summary>
    public class DsoInstructieregel
    {
        /// <summary>
        /// Unique identifier of the rule.
        /// </summary>
        public string? Identificatie { get; set; }

        /// <summary>
        /// Idealization/classification of the rule.
        /// </summary>
        public DsoCodeWaarde? Idealisatie { get; set; }

        /// <summary>
        /// Reference to the associated rule text.
        /// </summary>
        public string? RegeltekstRef { get; set; }

        /// <summary>
        /// Themes associated with the rule.
        /// </summary>
        public List<DsoCodeWaarde>? Themas { get; set; }

        /// <summary>
        /// The tasks/duties ("Taakuitoefeningen") this instruction rule applies to.
        /// </summary>
        public List<DsoCodeWaarde>? InstructieregelTaakuitoefeningen { get; set; }

        /// <summary>
        /// The instruments this instruction rule applies to.
        /// </summary>
        public List<DsoCodeWaarde>? InstructieregelInstrumenten { get; set; }

        /// <summary>
        /// References to related locations.
        /// </summary>
        public List<string>? LocatieRefs { get; set; }

        /// <summary>
        /// References to related area designations.
        /// </summary>
        public List<string>? GebiedsaanwijzingRefs { get; set; }

        /// <summary>
        /// References to related environmental norms.
        /// </summary>
        public List<string>? OmgevingsnormRefs { get; set; }

        /// <summary>
        /// References to related maps.
        /// </summary>
        public List<string>? KaartRefs { get; set; }

        /// <summary>
        /// Registration/versioning metadata for this rule.
        /// </summary>
        public DsoGeregistreerdMet? GeregistreerdMet { get; set; }
    }

    /// <summary>
    /// Represents a construction/building dimension rule ("Bouwmaatvoering").
    /// </summary>
    public class DsoBouwmaatvoering
    {
        /// <summary>
        /// Identifier of the related environmental norm.
        /// </summary>
        public string? OmgevingsnormIdentificatie { get; set; }

        /// <summary>
        /// Name of the building dimension rule.
        /// </summary>
        public string? Naam { get; set; }

        /// <summary>
        /// Type/category of the rule.
        /// </summary>
        public DsoRegelingType? Type { get; set; }

        /// <summary>
        /// Group the rule belongs to.
        /// </summary>
        public DsoRegelingGroep? Groep { get; set; }

        /// <summary>
        /// Quantitative value of the dimension (numeric).
        /// </summary>
        public double? KwantitatieveWaarde { get; set; }

        /// <summary>
        /// Qualitative value of the dimension (textual).
        /// </summary>
        public string? KwalitatieveWaarde { get; set; }

        /// <summary>
        /// Unit of measurement for the value.
        /// </summary>
        public string? Eenheid { get; set; }

        /// <summary>
        /// Geometry associated with this dimension rule.
        /// </summary>
        public GeoJsonGeometry? Geometrie { get; set; }
    }

    /// <summary>
    /// Represents an environmental norm ("Omgevingsnorm").
    /// </summary>
    public class DsoOmgevingsnorm
    {
        /// <summary>
        /// Unique identifier of the norm.
        /// </summary>
        public string? Identificatie { get; set; }

        /// <summary>
        /// Name of the norm.
        /// </summary>
        public string? Naam { get; set; }

        /// <summary>
        /// Type/category of the norm.
        /// </summary>
        public DsoRegelingType? Type { get; set; }

        /// <summary>
        /// Unit of measurement for the norm's values.
        /// </summary>
        public DsoCodeWaarde? Eenheid { get; set; }

        /// <summary>
        /// Group the norm belongs to.
        /// </summary>
        public DsoRegelingGroep? Groep { get; set; }

        /// <summary>
        /// List of norm values ("Normwaarden") associated with this norm.
        /// </summary>
        public List<DsoNormwaarde>? Normwaarden { get; set; }

        /// <summary>
        /// Registration/versioning metadata for this norm.
        /// </summary>
        public DsoGeregistreerdMet? GeregistreerdMet { get; set; }
    }

    /// <summary>
    /// Represents an environmental value ("Omgevingswaarde").
    /// </summary>
    public class DsoOmgevingswaarde
    {
        /// <summary>
        /// Unique identifier of the value.
        /// </summary>
        public string? Identificatie { get; set; }

        /// <summary>
        /// Name of the value.
        /// </summary>
        public string? Naam { get; set; }

        /// <summary>
        /// Type/category of the value.
        /// </summary>
        public DsoRegelingType? Type { get; set; }

        /// <summary>
        /// Unit of measurement for the value.
        /// </summary>
        public DsoCodeWaarde? Eenheid { get; set; }

        /// <summary>
        /// Group the value belongs to.
        /// </summary>
        public DsoRegelingGroep? Groep { get; set; }

        /// <summary>
        /// List of norm values ("Normwaarden") associated with this environmental value.
        /// </summary>
        public List<DsoNormwaarde>? Normwaarden { get; set; }

        /// <summary>
        /// Registration/versioning metadata for this environmental value.
        /// </summary>
        public DsoGeregistreerdMet? GeregistreerdMet { get; set; }
    }

    /// <summary>
    /// Represents a specific norm/threshold value ("Normwaarde") for a norm or environmental value.
    /// </summary>
    public class DsoNormwaarde
    {
        /// <summary>
        /// Unique identifier of the norm value.
        /// </summary>
        public string? Identificatie { get; set; }

        /// <summary>
        /// Quantitative value (numeric).
        /// </summary>
        public double? KwantitatieveWaarde { get; set; }

        /// <summary>
        /// Qualitative value (textual).
        /// </summary>
        public string? KwalitatieveWaarde { get; set; }

        /// <summary>
        /// The value exactly as written in the rule text.
        /// </summary>
        public string? WaardeInRegeltekst { get; set; }

        /// <summary>
        /// References to related locations.
        /// </summary>
        public List<string>? LocatieRefs { get; set; }

        /// <summary>
        /// Symbolization/styling information for map rendering.
        /// </summary>
        public DsoSymbolisatie? Symbolisatie { get; set; }

        /// <summary>
        /// Unit of measurement for the value.
        /// </summary>
        public string? Eenheid { get; set; }

        /// <summary>
        /// Registration/versioning metadata for this norm value.
        /// </summary>
        public DsoGeregistreerdMet? GeregistreerdMet { get; set; }
    }

    /// <summary>
    /// Describes how a rule/value should be symbolized ("Symbolisatie") when rendered on a map.
    /// </summary>
    public class DsoSymbolisatie
    {
        /// <summary>
        /// Type of geometry to render (e.g. point, line, polygon).
        /// </summary>
        public string? GeometrieType { get; set; }

        /// <summary>
        /// Symbol code used to render this element on a map.
        /// </summary>
        public string? Symboolcode { get; set; }
    }

    /// <summary>
    /// Represents a reference to a location ("LocatieRef"), including its own geometry
    /// and links, as returned within the rule text annotations response.
    /// </summary>
    public class DsoLocatieRef
    {
        /// <summary>
        /// HAL "_links" section for this location reference.
        /// </summary>
        [JsonPropertyName("_links")]
        public DsoLocatieLinks? Links { get; set; }

        /// <summary>
        /// Unique identifier of the location.
        /// </summary>
        public string? Identificatie { get; set; }

        /// <summary>
        /// Display name of the location.
        /// </summary>
        public string? Noemer { get; set; }

        /// <summary>
        /// Type of the location.
        /// </summary>
        public string? LocatieType { get; set; }

        /// <summary>
        /// Identifier of the geometry associated with this location.
        /// </summary>
        public string? GeometrieIdentificatie { get; set; }

        /// <summary>
        /// References to sub-locations contained within this location.
        /// </summary>
        public List<string>? LocatieRefs { get; set; }

        /// <summary>
        /// Registration/versioning metadata for this location.
        /// </summary>
        public DsoGeregistreerdMet? GeregistreerdMet { get; set; }

        /// <summary>
        /// Geometry of the location.
        /// </summary>
        public GeoJsonGeometry? Geometrie { get; set; }

        /// <summary>
        /// Additional metadata about the location, such as its bounding box.
        /// </summary>
        [JsonPropertyName("_extraInfo")]
        public DsoExtraInfo? ExtraInfo { get; set; }
    }

    /// <summary>
    /// Additional (non-standard) information attached to a location reference.
    /// </summary>
    public class DsoExtraInfo
    {
        /// <summary>
        /// Bounding box surrounding the location's geometry.
        /// </summary>
        public DsoBoundingBox? BoundingBox { get; set; }
    }

    /// <summary>
    /// Represents a rectangular bounding box in map coordinates.
    /// </summary>
    public class DsoBoundingBox
    {
        /// <summary>
        /// Minimum X coordinate.
        /// </summary>
        public double MinX { get; set; }

        /// <summary>
        /// Minimum Y coordinate.
        /// </summary>
        public double MinY { get; set; }

        /// <summary>
        /// Maximum X coordinate.
        /// </summary>
        public double MaxX { get; set; }

        /// <summary>
        /// Maximum Y coordinate.
        /// </summary>
        public double MaxY { get; set; }
    }

    /// <summary>
    /// HAL links associated with a location reference.
    /// </summary>
    public class DsoLocatieLinks
    {
        /// <summary>
        /// Link to the current resource itself.
        /// </summary>
        public DsoHalLink? Self { get; set; }
    }

    /// <summary>
    /// Generic HAL link containing a URL reference ("Href").
    /// </summary>
    public class DsoHalLink
    {
        /// <summary>
        /// The URL of the linked resource.
        /// </summary>
        public string? Href { get; set; }
    }

    /// <summary>
    /// Response returned when fetching a single location by its identifier.
    /// </summary>
    public class DsoLocatieResponse
    {
        /// <summary>
        /// Unique identifier of the location.
        /// </summary>
        public string? Identificatie { get; set; }

        /// <summary>
        /// Name of the location.
        /// </summary>
        public string? Naam { get; set; }

        /// <summary>
        /// Identifier of the geometry associated with this location.
        /// </summary>
        public string? GeometrieIdentificatie { get; set; }

        /// <summary>
        /// Geometry of the location.
        /// </summary>
        public GeoJsonGeometry? Geometrie { get; set; }
    }

    /// <summary>
    /// Represents an area designation ("Gebiedsaanwijzing") applied to one or more locations.
    /// </summary>
    public class DsoGebiedsaanwijzing
    {
        /// <summary>
        /// Unique identifier of the area designation.
        /// </summary>
        public string? Identificatie { get; set; }

        /// <summary>
        /// Name of the area designation.
        /// </summary>
        public string? Naam { get; set; }

        /// <summary>
        /// Group the area designation belongs to.
        /// </summary>
        public DsoRegelingGroep Groep { get; set; }

        /// <summary>
        /// Type/category of the area designation.
        /// </summary>
        public DsoRegelingType? Type { get; set; }

        /// <summary>
        /// Symbol codes used for rendering this area designation, keyed by identifier.
        /// </summary>
        public Dictionary<string, string>? Symboolcodes { get; set; }

        /// <summary>
        /// References to related locations.
        /// </summary>
        public List<string>? LocatieRefs { get; set; }

        /// <summary>
        /// Registration/versioning metadata for this area designation.
        /// </summary>
        public DsoGeregistreerdMet? GeregistreerdMet { get; set; }

        /// <summary>
        /// Geometry of the area designation.
        /// </summary>
        public GeoJsonGeometry? Geometrie { get; set; }
    }

    /// <summary>
    /// Response returned by the DSO "gebiedsaanwijzingen zoeken" (search area designations) endpoint.
    /// </summary>
    public class DsoGebiedsaanwijzingZoekResponse
    {
        /// <summary>
        /// List of area designations returned directly at the root of the response, if present.
        /// </summary>
        public List<DsoGebiedsaanwijzing>? Gebiedsaanwijzingen { get; set; }

        /// <summary>
        /// HAL "_embedded" section containing the area designations, if returned in embedded form.
        /// </summary>
        [JsonPropertyName("_embedded")]
        public DsoGebiedsaanwijzingEmbedded? Embedded { get; set; }

        /// <summary>
        /// Convenience accessor returning all area designations regardless of whether
        /// they were returned at the root or within the "_embedded" section.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<DsoGebiedsaanwijzing> AllGebiedsaanwijzingen =>
            Gebiedsaanwijzingen ?? Embedded?.Gebiedsaanwijzingen ?? Enumerable.Empty<DsoGebiedsaanwijzing>();
    }

    /// <summary>
    /// HAL "_embedded" wrapper containing the list of area designations ("Gebiedsaanwijzingen").
    /// </summary>
    public class DsoGebiedsaanwijzingEmbedded
    {
        /// <summary>
        /// The list of area designations returned by the search.
        /// </summary>
        public List<DsoGebiedsaanwijzing>? Gebiedsaanwijzingen { get; set; }
    }

    /// <summary>
    /// Represents an activity ("Activiteit") that can be regulated by rules.
    /// </summary>
    public class DsoActiviteit
    {
        /// <summary>
        /// Unique identifier of the activity.
        /// </summary>
        public string? Identificatie { get; set; }

        /// <summary>
        /// Name of the activity.
        /// </summary>
        public string? Naam { get; set; }

        /// <summary>
        /// Group the activity belongs to.
        /// </summary>
        public DsoRegelingGroep? Groep { get; set; }

        /// <summary>
        /// Symbol codes used for rendering this activity, keyed by identifier.
        /// </summary>
        public Dictionary<string, string>? Symboolcodes { get; set; }

        /// <summary>
        /// Reference to the parent activity ("BovenliggendeActiviteit"), if any.
        /// </summary>
        public string? BovenliggendeActiviteitRef { get; set; }

        /// <summary>
        /// References to other related activities.
        /// </summary>
        public List<string>? GerelateerdeActiviteitRefs { get; set; }

        /// <summary>
        /// Registration/versioning metadata for this activity.
        /// </summary>
        public DsoGeregistreerdMet? GeregistreerdMet { get; set; }
    }

    /// <summary>
    /// Represents a map ("Kaart") used to visualize rules and locations.
    /// </summary>
    public class DsoKaart
    {
        /// <summary>
        /// Unique identifier of the map.
        /// </summary>
        public string? Identificatie { get; set; }

        /// <summary>
        /// Name of the map.
        /// </summary>
        public string? Naam { get; set; }

        /// <summary>
        /// Map number/reference.
        /// </summary>
        public string? Nummer { get; set; }

        /// <summary>
        /// Extent/bounding area covered by the map ("Uitsnede").
        /// </summary>
        public DsoUitsnede? Uitsnede { get; set; }

        /// <summary>
        /// Registration/versioning metadata for this map.
        /// </summary>
        public DsoGeregistreerdMet? GeregistreerdMet { get; set; }

        /// <summary>
        /// List of map layers ("Kaartlagen") contained in this map.
        /// </summary>
        public List<DsoKaartlaag>? Kaartlagen { get; set; }
    }

    /// <summary>
    /// Represents the geographic extent/bounding box of a map ("Uitsnede").
    /// </summary>
    public class DsoUitsnede
    {
        /// <summary>
        /// Minimum X coordinate.
        /// </summary>
        public double MinX { get; set; }

        /// <summary>
        /// Minimum Y coordinate.
        /// </summary>
        public double MinY { get; set; }

        /// <summary>
        /// Maximum X coordinate.
        /// </summary>
        public double MaxX { get; set; }

        /// <summary>
        /// Maximum Y coordinate.
        /// </summary>
        public double MaxY { get; set; }
    }

    /// <summary>
    /// Represents a single layer ("Kaartlaag") within a map, grouping related references.
    /// </summary>
    public class DsoKaartlaag
    {
        /// <summary>
        /// Unique identifier of the map layer.
        /// </summary>
        public string? Identificatie { get; set; }

        /// <summary>
        /// Name of the map layer.
        /// </summary>
        public string? Naam { get; set; }

        /// <summary>
        /// Rendering level/order ("Niveau") of the layer.
        /// </summary>
        public int? Niveau { get; set; }

        /// <summary>
        /// References to area designations shown on this layer.
        /// </summary>
        public List<string>? GebiedsaanwijzingRefs { get; set; }

        /// <summary>
        /// References to environmental norms shown on this layer.
        /// </summary>
        public List<string>? OmgevingsnormRefs { get; set; }

        /// <summary>
        /// References to environmental values shown on this layer.
        /// </summary>
        public List<string>? OmgevingswaardeRefs { get; set; }

        /// <summary>
        /// References to activity location indications shown on this layer.
        /// </summary>
        public List<string>? ActiviteitLocatieaanduidingRefs { get; set; }
    }

    /// <summary>
    /// Response returned when fetching detailed information for a single location,
    /// including the sub-locations it contains ("Omvat").
    /// </summary>
    public class DsoLocatieDetailResponse
    {
        /// <summary>
        /// Unique identifier of the location.
        /// </summary>
        public string? Identificatie { get; set; }

        /// <summary>
        /// Type of the location.
        /// </summary>
        public string? LocatieType { get; set; }

        /// <summary>
        /// Identifier of the geometry associated with this location.
        /// </summary>
        public string? GeometrieIdentificatie { get; set; }

        /// <summary>
        /// HAL "_embedded" section containing the sub-locations this location contains.
        /// </summary>
        [JsonPropertyName("_embedded")]
        public LocatieDetailEmbedded? Embedded { get; set; }
    }

    /// <summary>
    /// HAL "_embedded" wrapper containing the sub-locations ("Omvat") of a location.
    /// </summary>
    public class LocatieDetailEmbedded
    {
        /// <summary>
        /// List of sub-locations contained within the parent location.
        /// </summary>
        public List<LocatieOmvatItem>? Omvat { get; set; }
    }

    /// <summary>
    /// Represents a single sub-location item ("Omvat") contained within a parent location.
    /// </summary>
    public class LocatieOmvatItem
    {
        /// <summary>
        /// Unique identifier of the sub-location.
        /// </summary>
        public string? Identificatie { get; set; }

        /// <summary>
        /// Type of the sub-location.
        /// </summary>
        public string? LocatieType { get; set; }

        /// <summary>
        /// Identifier of the geometry associated with this sub-location.
        /// </summary>
        public string? GeometrieIdentificatie { get; set; }
    }
}