using VNGService.Models;

namespace VNGService.Services
{
    public static class GeometryHelper
    {
        /// <summary>
        /// Determines whether the given point lies within the supplied GeoJSON geometry.
        /// Supports "Polygon" and "MultiPolygon" geometry types; other types return false.
        /// </summary>
        public static bool GeometryContainsPoint(GeoJsonGeometry? geometry, double pointX, double pointY)
        {
            if (geometry == null)
            {
                return false;
            }

            switch (geometry.Type)
            {
                case "Polygon":
                    var polygon = geometry.PolygonCoordinates;
                    return (polygon != null) && PolygonContainsPoint(polygon, pointX, pointY);

                case "MultiPolygon":
                    var multiPolygon = geometry.MultiPolygonCoordinates;
                    if (multiPolygon == null)
                    {
                        return false;
                    }

                    foreach (var polygonRings in multiPolygon)
                    {
                        if (PolygonContainsPoint(polygonRings, pointX, pointY))
                        {
                            return true;
                        }
                    }

                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        /// A GeoJSON "Polygon" is an array of linear rings: the first is the exterior ring,
        /// any subsequent rings are interior rings (holes) that must be excluded.
        /// </summary>
        private static bool PolygonContainsPoint(double[][][] rings, double pointX, double pointY)
        {
            if (rings.Length == 0)
            {
                return false;
            }

            var exteriorRing = rings[0];
            if (!RingContainsPoint(exteriorRing, pointX, pointY))
            {
                return false;
            }

            for (int i = 1; i < rings.Length; i++)
            {
                if (RingContainsPoint(rings[i], pointX, pointY))
                {
                    // Point falls inside a hole, so it's not actually inside the polygon.
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Standard ray-casting (even-odd rule) point-in-polygon test for a single linear ring.
        /// Coordinates are expected as [x, y] (or [x, y, z], extra values are ignored).
        /// </summary>
        private static bool RingContainsPoint(double[][] ring, double pointX, double pointY)
        {
            bool inside = false;
            int j = ring.Length - 1;

            for (int i = 0; i < ring.Length; i++)
            {
                double xi = ring[i][0], yi = ring[i][1];
                double xj = ring[j][0], yj = ring[j][1];

                bool intersects = ((yi > pointY) != (yj > pointY)) &&
                    (pointX < ((xj - xi) * (pointY - yi) / (yj - yi)) + xi);

                if (intersects)
                {
                    inside = !inside;
                }

                j = i;
            }

            return inside;
        }
    }
}
