using System.Text.Json.Serialization;
using System.Linq; // Added for Enumeral extension methods used in calculated properties

namespace VNGService.Models
{
    /// <summary>
    /// Represents the full response containing multiple zoning regulations (DSO).
    /// </summary>
    public class DsoRegelingenZoekResponse
    {
        // Embedded data containing the list of search results for regulations.
        [JsonPropertyName("_embedded")]
        public DsoRegelingenEmbedded? Embedded { get; set; }

        // Calculated property to easily access all retrieved regulations as an IEnumerable.
        [JsonIgnore]
        public IEnumerable<DsoRegeling> AllRegelingen =>
            Embedded?.Regelingen ?? Enumerable.Empty<DsoRegeling>();
    }

    /// <summary>
    /// Contains the embedded list of zoning regulations (DSO).
    /// </summary>
    public class DsoRegelingenEmbedded
    {
        // List containing all retrieved regulation records.
        public List<DsoRegeling>? Regelingen { get; set; }
    }

    /// <summary>
    /// Represents a single zoning or planning regulation record.
    /// </summary>
    public class DsoRegeling
    {
        // Unique identification code of the regulation.
        public string? Identificatie { get; set; } = null;
        // The official, full title of the regulation.
        public string? OfficieleTitel { get; set; } = null;
        // A citation or secondary descriptive title for the regulation.
        public string? CiteerTitel { get; set; } = null;
        // The specific classification type of the regulation.
        public DsoRegelingType? Type { get; set; } = null;
        // Information regarding which entity provided this regulation (e.g., municipality).
        public DsoAangeleverdDoorEen? AangeleverdDoorEen { get; set; } = null;

        /// <summary>
        /// Calculates the normalized and standardized ID format used in BFF services,
        /// converting slashes ('/') to underscores ('_').
        /// </summary>
        [JsonIgnore]
        public string? BFFId => Identificatie?.Replace('/', '_');
    }

    /// <summary>
    /// Defines a category or type for the zoning regulation.
    /// </summary>
    public class DsoRegelingType
    {
        // Machine-readable code identifying the type (e.g., "RES").
        public string? Code { get; set; }
        // Human-readable value description of the type.
        public string? Waarde { get; set; }
    }

    /// <summary>
    /// Defines a group or category for regulations, typically used for filtering/grouping.
    /// </summary>
    public class DsoRegelingGroep
    {
        // Machine-readable code identifying the group.
        public string? Code { get; set; }
        // Human-readable value description of the group.
        public string? Waarde { get; set; }
    }

    /// <summary>
    /// Information on the administrative entity that supplied the regulation data.
    /// </summary>
    public class DsoAangeleverdDoorEen
    {
        // Name of the supplying organization or body.
        public string? Naam { get; set; }
        // The hierarchical level of the authority (e.g., "National", "Municipal").
        public string? Bestuurslaag { get; set; }
        // Code associated with the administrative entity.
        public string? Code { get; set; } = null;
    }

    /// <summary>
    /// Response structure for searching geographical locations in a planning context.
    /// </summary>
    public class DsoLocatieZoekResponse
    {
        // Embedded data containing the list of found location items.
        [JsonPropertyName("_embedded")]
        public LocatieZoekEmbedded? Embedded { get; set; }

        // Pagination information for traversing results (e.g., total count, next/prev links).
        public PageInfo? Page { get; set; } = null;
    }

    /// <summary>
    /// Contains the embedded list of located items found during a location search.
    /// </summary>
    public class LocatieZoekEmbedded
    {
        // List containing all individual location records found.
        public List<LocatieItem>? Locaties { get; set; } = null;
    }

    /// <summary>
    /// Contains annotations or textual descriptions that apply to a specific geographical area (location).
    /// </summary>
    public class DsoRegeltekstAnnotatiesResponse
    {
        // List of annotated areas described by regulations.
        public List<DsoGebiedsaanwijzing>? Gebiedsaanwijzingen { get; set; }
        // List of specific locations or points referenced by the regulations.
        public List<DsoLocatieRef>? Locaties { get; set; }
    }

    /// <summary>
    /// Represents a reference link pointing to a geographical location within a regulation text.
    /// </summary>
    public class DsoLocatieRef
    {
        // Link metadata for the current resource instance.
        [JsonPropertyName("_links")]
        public DsoLocatieLinks? Links { get; set; }

        // Unique identifier of the referenced location.
        public string? Identificatie { get; set; }
        // A numerical reference number used in documentation (e.g., "L1", "N2").
        public string? Noemer { get; set; }
        // Descriptive type of the location (e.g., "Corner", "Street segment").
        public string? LocatieType { get; set; }
        // Identifier used for geometry representation.
        public string? GeometrieIdentificatie { get; set; }
        // The geometric shape defining the location reference area.
        public GeoJsonGeometry? Geometrie { get; set; }
    }

    /// <summary>
    /// Collection of links (references) related to a specific location reference.
    /// </summary>
    public class DsoLocatieLinks
    {
        // Link pointing back to the current resource instance ('self').
        public DsoHalLink? Self { get; set; }
    }

    /// <summary>
    /// Standard link wrapper for HAL (Hypermedia As The Engine of Application State) protocol.
    /// </summary>
    public class DsoHalLink
    {
        // The actual URI/URL pointer to the resource.
        public string? Href { get; set; }
    }

    /// <summary>
    /// Detailed data structure for a specific geographical location point or area reference.
    /// </summary>
    public class DsoLocatieResponse
    {
        // Unique identifier of the physical location.
        public string? Identificatie { get; set; }
        // Descriptive name of the location (e.g., "Main Street").
        public string? Naam { get; set; }
        // Identifier used for geometry representation.
        public string? GeometrieIdentificatie { get; set; }
        // The geometric shape defining the location.
        public GeoJsonGeometry? Geometrie { get; set; }
    }

    /// <summary>
    /// Represents an annotation applied to a specific geographical area, detailing required zoning rules.
    /// </summary>
    public class DsoGebiedsaanwijzing
    {
        // Unique identifier of the area designation.
        public string? Identificatie { get; set; }
        // Name or description of the annotated area.
        public string? Naam { get; set; }
        // The group classification this annotation belongs to.
        public DsoRegelingGroep? Groep { get; set; }
        // The specific regulation type that applies to this area.
        public DsoRegelingType? Type { get; set; }
        // List of location references (DsoLocatieRef) included within this annotated area.
        public List<string>? LocatieRefs { get; set; }
        // The geometric shape defining the annotated area.
        public GeoJsonGeometry? Geometrie { get; set; }
    }

    /// <summary>
    /// Response structure for searching and retrieving multiple delineated area annotations within a plan.
    /// </summary>
    public class DsoGebiedsaanwijzingZoekResponse
    {
        // Explicit list of retrieved area designations.
        public List<DsoGebiedsaanwijzing>? Gebiedsaanwijzingen { get; set; }

        // Embedded data containing the main list of results (standardized API response format).
        [JsonPropertyName("_embedded")]
        public DsoGebiedsaanwijzingEmbedded? Embedded { get; set; }

        /// <summary>
        /// Calculates and returns all retrieved area designations from both explicit properties and embedded sections.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<DsoGebiedsaanwijzing> AllGebiedsaanwijzingen =>
            Gebiedsaanwijzingen ?? Embedded?.Gebiedsaanwijzingen ?? Enumerable.Empty<DsoGebiedsaanwijzing>();
    }

    /// <summary>
    /// Contains the embedded list of area designations found during a search.
    /// </summary>
    public class DsoGebiedsaanwijzingEmbedded
    {
        // List containing all individual area designation records.
        public List<DsoGebiedsaanwijzing>? Gebiedsaanwijzingen { get; set; }
    }

    /// <summary>
    /// Container for items that are included within a larger geographical location/area.
    /// </summary>
    public class LocatieDetailEmbedded
    {
        // List of specific identifiers or segments composing the overall location area.
        public List<LocatieOmvatItem>? Omvat { get; set; }
    }

    /// <summary>
    /// Represents a single item that is included within a larger defined location area.
    /// </summary>
    public class LocatieOmvatItem
    {
        // Unique identifier of the encompassed item.
        public string? Identificatie { get; set; }
        // Descriptive type of the contained item (e.g., "building", "plot").
        public string? LocatieType { get; set; }
        // Identifier used for geometry representation of the contained item.
        public string? GeometrieIdentificatie { get; set; }
    }
}