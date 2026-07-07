using VNGService.Models;

namespace VNGService.Services;

/// <summary>
/// Converts RD New (EPSG:28992) coordinates to WGS84 (EPSG:4326).
/// Kadaster formula:
/// https://www.kadaster.nl/-/koepelnotitie-rd-stelsel-en-nap
/// </summary>
public static class RdToWgs84Converter
{
    // Origin of RD New in WGS84
    private const double AmersfoortLat = 52.15517440;
    private const double AmersfoortLon = 5.38720621;

    // RD New origin in meters
    private const double RdOriginX = 155000.0;
    private const double RdOriginY = 463000.0;

    private static readonly double[,] KpCoeff =
    {
        // p, q, K
        { 0, 1,  3235.65389 },
        { 2, 0,  -32.58297  },
        { 0, 2,  -0.24750   },
        { 2, 1,  -0.84978   },
        { 0, 3,  -0.06550   },
        { 2, 2,  -0.01709   },
        { 1, 0,  -0.00738   },
        { 4, 0,   0.00530   },
        { 2, 3,  -0.00039   },
        { 4, 1,   0.00033   },
        { 1, 1,  -0.00012   },
    };

    private static readonly double[,] LpCoeff =
    {
        // p, q, L
        { 1, 0,  5260.52916 },
        { 1, 1,  105.94684  },
        { 1, 2,   2.45656   },
        { 3, 0,  -0.81885   },
        { 1, 3,   0.05594   },
        { 3, 1,  -0.05607   },
        { 0, 1,   0.01199   },
        { 3, 2,  -0.00256   },
        { 1, 4,   0.00128   },
        { 0, 2,   0.00022   },
        { 2, 0,  -0.00022   },
        { 5, 0,   0.00026   },
    };

    /// <summary>
    /// Converts an RD New (EPSG:28992) point (x=Lon, y=Lat in meters) to WGS84.
    /// Note: in RD New, X is the easting (west-east) and Y is the northing (south-north).
    /// </summary>
    /// <param name="rdX">RD New X (easting), stored as <see cref="GeoPoint.Lon"/>.</param>
    /// <param name="rdY">RD New Y (northing), stored as <see cref="GeoPoint.Lat"/>.</param>
    /// <returns>WGS84 <see cref="GeoPoint"/> with true Lat/Lon in decimal degrees.</returns>
    public static GeoPoint Convert(double rdX, double rdY)
    {
        double dX = (rdX - RdOriginX) * 1e-5;
        double dY = (rdY - RdOriginY) * 1e-5;

        double lat = AmersfoortLat;
        double lon = AmersfoortLon;

        for (int i = 0; i < KpCoeff.GetLength(0); i++)
        {
            double p = KpCoeff[i, 0];
            double q = KpCoeff[i, 1];
            double k = KpCoeff[i, 2];
            lat += k * Math.Pow(dX, p) * Math.Pow(dY, q) / 3600.0;
        }

        for (int i = 0; i < LpCoeff.GetLength(0); i++)
        {
            double p = LpCoeff[i, 0];
            double q = LpCoeff[i, 1];
            double l = LpCoeff[i, 2];
            lon += l * Math.Pow(dX, p) * Math.Pow(dY, q) / 3600.0;
        }

        return new GeoPoint(Lat: lat, Lon: lon);
    }

    /// <summary>Converts a <see cref="GeoPoint"/> where Lon=RD X and Lat=RD Y.</summary>
    public static GeoPoint Convert(GeoPoint rdPoint) => Convert(rdPoint.Lon, rdPoint.Lat);
}