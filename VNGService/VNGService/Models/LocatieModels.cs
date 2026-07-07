using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace VNGService.Models
{
    /// <summary>
    /// Response structure for searching geographical locations, containing embedded results and pagination info.
    /// </summary>
    public class LocatieZoekResponse
    {
        // Embedded data containing the list of found location items.
        [JsonPropertyName("_embedded")]
        public EmbeddedData? Embedded { get; set; }

        // Pagination information for traversing search results (e.g., page number, total pages).
        [JsonPropertyName("page")]
        public PageInfo? Page { get; set; }
    }

    /// <summary>
    /// Container for the embedded list of location items found during a search.
    /// </summary>
    public class EmbeddedData
    {
        // List containing all individual location records.
        [JsonPropertyName("locaties")]
        public List<LocatieItem>? Locaties { get; set; }
    }

    /// <summary>
    /// Represents a single found location item during the search process.
    /// </summary>
    public class LocatieItem
    {
        // Unique identifier of the location record.
        [JsonPropertyName("identificatie")]
        public string? Identificatie { get; set; }

        // A sequence or numbering used in documentation (e.g., "A1", "B2").
        [JsonPropertyName("noemer")]
        public string? Noemer { get; set; }

        // TODO: The geometry type is generally included within the GeoJsonGeometry object itself, making this redundant.
        /* [JsonPropertyName("type")]
        public string? Type { get; set; } */

        // The geometric representation (coordinates) of the location.
        [JsonPropertyName("geometrie")]
        public Geometrie? Geometrie { get; set; }
    }

    /// <summary>
    /// Represents a simple geographical geometry structure (a list of coordinates).
    /// </summary>
    public class Geometrie
    {
        // The geometric type (e.g., "Point", "LineString").
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        // Raw JSON element holding the coordinates array structure.
        [JsonPropertyName("coordinates")]
        public JsonElement CoordinatesRaw { get; set; }

        // Public accessor property to retrieve coordinates if the type is Point (double[]).
        public double[]? PointCoordinates =>
            Type == "Point" ? JsonSerializer.Deserialize<double[]>(CoordinatesRaw.GetRawText()) : null;

        // Public accessor property to retrieve coordinates if the type is LineString (double[][]).
        public double[][]? LineStringCoordinates =>
            Type == "LineString" ? JsonSerializer.Deserialize<double[][]>(CoordinatesRaw.GetRawText()) : null;

        // Public accessor property to retrieve coordinates if the type is Polygon (double[][][]).
        public double[][][]? PolygonCoordinates =>
            Type == "Polygon" ? JsonSerializer.Deserialize<double[][][]>(CoordinatesRaw.GetRawText()) : null;

        // Public accessor property to retrieve coordinates if the type is MultiPoint (double[][]).
        public double[][]? MultiPointCoordinates =>
            Type == "MultiPoint" ? JsonSerializer.Deserialize<double[][]>(CoordinatesRaw.GetRawText()) : null;

        // Public accessor property to retrieve coordinates if the type is MultiLineString (double[][][]).
        public double[][][]? MultiLineStringCoordinates =>
            Type == "MultiLineString" ? JsonSerializer.Deserialize<double[][][]>(CoordinatesRaw.GetRawText()) : null;

        // Public accessor property to retrieve coordinates if the type is MultiPolygon (double[][][][]).
        public double[][][][]? MultiPolygonCoordinates =>
            Type == "MultiPolygon" ? JsonSerializer.Deserialize<double[][][][]>(CoordinatesRaw.GetRawText()) : null;
    }

    /// <summary>
    /// Contains pagination metadata for search results.
    /// </summary>
    public class PageInfo
    {
        // The number of items displayed per page (page size).
        [JsonPropertyName("size")]
        public int Size { get; set; }

        // The total count of matching records found across all pages.
        [JsonPropertyName("totalElements")]
        public int TotalElements { get; set; }

        // The total number of available pages.
        [JsonPropertyName("totalPages")]
        public int TotalPages { get; set; }

        // The current page number (0-indexed).
        [JsonPropertyName("number")]
        public int Number { get; set; }
    }

    // ==================================================
    // LEGACY MODELS BELOW
    // ==================================================

    /// <summary>
    /// Legacy response structure for listing multiple documents.
    /// </summary>
    public class DocumentResponse
    {
        [JsonPropertyName("documenten")]
        public List<Document>? Documenten { get; set; }

        // Total count of records available in the document system.
        [JsonPropertyName("totaal")]
        public int Totaal { get; set; }
    }

    /// <summary>
    /// Legacy model representing a single historical document record.
    /// </summary>
    public class Document
    {
        // Unique identifier of the document.
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        // The official title of the document.
        [JsonPropertyName("titel")]
        public string? Titel { get; set; }

        // Document classification type.
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        // Link to the geographical location associated with the document.
        [JsonPropertyName("locatie")]
        public Locatie? Locatie { get; set; }

        // Date when the document was published (optional).
        [JsonPropertyName("publicatiedatum")]
        public DateTime? Publicatiedatum { get; set; }
    }

    /// <summary>
    /// Legacy model containing core geographical location details.
    /// </summary>
    public class Locatie
    {
        // Latitude coordinate (Y).
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        // Longitude coordinate (X).
        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        // The textual street address of the location.
        [JsonPropertyName("adres")]
        public string? Adres { get; set; }
    }

    /// <summary>
    /// Detailed response for a specific physical location, often used when linking resources.
    /// </summary>
    public class LocatieDetailResponse
    {
        // Links metadata referencing the resource itself.
        [JsonPropertyName("_links")]
        public Links? Links { get; set; }

        // Unique identifier of the location record.
        [JsonPropertyName("identificatie")]
        public string? Identificatie { get; set; }

        // Details on how/when the data was registered.
        [JsonPropertyName("geregistreerdMet")]
        public GeregistreerdMet? GeregistreerdMet { get; set; }

        // A numbering designation or sequence number.
        [JsonPropertyName("noemer")]
        public string? Noemer { get; set; }

        // Current operational status of the location data.
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        // Identifier for the bounding box geometry.
        [JsonPropertyName("geometrieIdentificatie")]
        public string? GeometrieIdentificatie { get; set; }

        // The outer rectangular boundary coordinates (min/max X/Y).
        [JsonPropertyName("boundingBox")]
        public BoundingBox? BoundingBox { get; set; }

        // Embedded list of components that make up this location.
        [JsonPropertyName("_embedded")]
        public EmbeddedOmvat? Embedded { get; set; }
    }

    /// <summary>
    /// Collection of links (references) for a location detail object.
    /// </summary>
    public class Links
    {
        // Link pointing back to the current resource instance ('self').
        [JsonPropertyName("self")]
        public LinkInfo? Self { get; set; }
    }

    /// <summary>
    /// Standard HAL link wrapper containing a URI reference.
    /// </summary>
    public class LinkInfo
    {
        // The actual Uniform Resource Identifier (URI) link.
        [JsonPropertyName("href")]
        public string? Href { get; set; }
    }

    /// <summary>
    /// Details on the registration time and status of a location entry.
    /// </summary>
    public class GeregistreerdMet
    {
        // Version number of the registered data.
        [JsonPropertyName("versie")]
        public int Versie { get; set; }

        // Date when the rule/status became active.
        [JsonPropertyName("beginInwerking")]
        public string? BeginInwerking { get; set; }

        // Start date of validity for the data period.
        [JsonPropertyName("beginGeldigheid")]
        public string? BeginGeldigheid { get; set; }

        // Timestamp when the record was registered/observed.
        [JsonPropertyName("tijdstipRegistratie")]
        public string? TijdstipRegistratie { get; set; }

        // Current status of the registration data.
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        // Sub-type or category within the registration context.
        [JsonPropertyName("subType")]
        public string? SubType { get; set; }
    }

    /// <summary>
    /// Defines a bounding box using minimum and maximum coordinates (MinX, MaxX, MinY, MaxY).
    /// </summary>
    public class BoundingBox
    {
        [JsonPropertyName("minX")]
        public double MinX { get; set; }

        [JsonPropertyName("maxX")]
        public double MaxX { get; set; }

        [JsonPropertyName("minY")]
        public double MinY { get; set; }

        [JsonPropertyName("maxY")]
        public double MaxY { get; set; }
    }

    /// <summary>
    /// Contains the embedded list of components that make up a larger location.
    /// </summary>
    public class EmbeddedOmvat
    {
        // List of individual items included within this main location/area.
        [JsonPropertyName("omvat")]
        public List<GebiedDetail>? Omvat { get; set; }
    }

    /// <summary>
    /// Represents a detailed component or sub-area that is contained within a larger location entity.
    /// </summary>
    public class GebiedDetail
    {
        // Links metadata for this specific contained detail.
        [JsonPropertyName("_links")]
        public Links? Links { get; set; }

        // Unique identifier of the sub-area/component.
        [JsonPropertyName("identificatie")]
        public string? Identificatie { get; set; }

        // Registration details for the component.
        [JsonPropertyName("geregistreerdMet")]
        public GeregistreerdMet? GeregistreerdMet { get; set; }

        // Numbering designation or sequence number of the sub-area.
        [JsonPropertyName("noemer")]
        public string? Noemer { get; set; }

        // Current status of the component data.
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        // Identifier for the geometric bounding box of the component.
        [JsonPropertyName("geometrieIdentificatie")]
        public string? GeometrieIdentificatie { get; set; }

        // The outer rectangular boundary coordinates (min/max X/Y) of the component.
        [JsonPropertyName("boundingBox")]
        public BoundingBox? BoundingBox { get; set; }
    }

    // ==================================================
    // GEOJSON GEOMETRY MODELS
    // ==================================================

    /// <summary>
    /// Represents a standardized GeoJSON geometry object wrapper.
    /// </summary>
    public class GeoJsonGeometry
    {
        // The geometric type (e.g., "Point", "Polygon").
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        // Raw JSON element containing the coordinates structure.
        [JsonPropertyName("coordinates")]
        public JsonElement CoordinatesRaw { get; set; }

        // Calculated property: Returns Point coordinates array if the geometry type is "Point".
        public double[]? PointCoordinates =>
            Type == "Point" ? JsonSerializer.Deserialize<double[]>(CoordinatesRaw.GetRawText()) : null;

        // Calculated property: Returns LineString coordinates array if the geometry type is "LineString".
        public double[][]? LineStringCoordinates =>
            Type == "LineString" ? JsonSerializer.Deserialize<double[][]>(CoordinatesRaw.GetRawText()) : null;

        // Calculated property: Returns Polygon coordinates array if the geometry type is "Polygon".
        public double[][][]? PolygonCoordinates =>
            Type == "Polygon" ? JsonSerializer.Deserialize<double[][][]>(CoordinatesRaw.GetRawText()) : null;

        // Public accessor for MultiPoint coordinates (double[][]).
        public double[][]? MultiPointCoordinates =>
            Type == "MultiPoint" ? JsonSerializer.Deserialize<double[][]>(CoordinatesRaw.GetRawText()) : null;

        // Public accessor for MultiLineString coordinates (double[][][]).
        public double[][][]? MultiLineStringCoordinates =>
            Type == "MultiLineString" ? JsonSerializer.Deserialize<double[][][]>(CoordinatesRaw.GetRawText()) : null;

        // Public accessor for MultiPolygon coordinates (double[][][][]).
        public double[][][][]? MultiPolygonCoordinates =>
            Type == "MultiPolygon" ? JsonSerializer.Deserialize<double[][][][]>(CoordinatesRaw.GetRawText()) : null;
    }

    /// <summary>
    /// Helper class to programmatically work with Polygon geometry data.
    /// </summary>
    public class PolygonGeometry
    {
        // The array containing the exterior ring and all interior rings (holes).
        public double[][][] Coordinates { get; set; } = Array.Empty<double[][]>();

        // Gets the coordinates of the outermost boundary (the first ring).
        public double[][]? ExteriorRing => Coordinates?[0];
        // Gets an enumerable list of all inner holes/rings (all rings after the first one).
        public double[][][]? InteriorRings => Coordinates?.Skip(1).ToArray();

        /// <summary>Returns true if the polygon has more than one ring, indicating at least one hole.</summary>
        public bool HasHoles => Coordinates?.Length > 1;

        /// <summary>Gets the total number of rings (exterior + interior).</summary>
        public int RingCount => Coordinates?.Length ?? 0;
    }

    /// <summary>
    /// Helper class to programmatically work with MultiPolygon geometry data.
    /// </summary>
    public class MultiPolygonGeometry
    {
        // The array containing all the individual polygon coordinates.
        public double[][][][]? Coordinates { get; set; }

        /// <summary>Gets the total number of polygons contained in this multi-polygon.</summary>
        public int PolygonCount => Coordinates?.Length ?? 0;

        /// <summary>Iterates over all constituent PolygonGeometry objects.</summary>
        public IEnumerable<PolygonGeometry> GetPolygons()
        {
            if (Coordinates == null) yield break;

            for (int i = 0; i < Coordinates.Length; i++)
            {
                yield return new PolygonGeometry { Coordinates = Coordinates[i] };
            }
        }
    }

    /// <summary>
    /// Represents a simple point structure, holding X and Y coordinates.
    /// </summary>
    public class Point
    {
        // The X coordinate (longitude).
        public double X { get; set; }

        // The Y coordinate (latitude).
        public double Y { get; set; }

        /// <summary>Initializes a new instance of the Point with longitude and latitude.</summary>
        public Point(double[] coords)
        {
            X = coords[0]; // Assuming standard [longitude, latitude] GeoJSON format
            Y = coords[1];
        }
    }
}