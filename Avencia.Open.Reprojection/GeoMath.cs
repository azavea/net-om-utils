// Copyright (c) 2004-2010 Avencia, Inc.
// 
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using GeoAPI.Geometries;

namespace Avencia.Open.Reprojection
{
    /// <summary>
    /// A class full of static methods for doing geometry-related math.
    /// </summary>
    public static class GeoMath
    {
        /// <summary>
        /// How many feet make up a meter.
        /// </summary>
        public const double FEET_PER_METER = 3.2808399;
        /// <summary>
        /// How many miles make up a meter.
        /// </summary>
        public const double MILES_PER_METER = 0.000621371192;
        /// <summary>
        /// How many kilometers make up a meter. (Yeah it's easy, but we've got
        /// the other constants, so why not?)
        /// </summary>
        public const double KM_PER_METER = 0.001;
        /// <summary>
        /// The approximate average radius of the Earth, in meters.
        /// </summary>
        public const double EARTH_RADIUS_AVERAGE_METERS = 6371000;
        /// <summary>
        /// The radius of the earth at the equator.
        /// </summary>
        public const double EARTH_RADIUS_AT_EQUATOR_METERS = 6378137;
        /// <summary>
        /// The "A" value for the WGS84 ellipsoid.
        /// </summary>
        public const double WGS84_ELLIPSOID_A = EARTH_RADIUS_AT_EQUATOR_METERS;
        /// <summary>
        /// The "B" value for the WGS84 ellipsoid.
        /// </summary>
        public const double WGS84_ELLIPSOID_B = 6356752.3142;
        /// <summary>
        /// The "F" value for the WGS84 ellipsoid.
        /// </summary>
        public const double WGS84_ELLIPSOID_F = 1 / 298.257223563;

        /// <summary>
        /// Converts a value in radians into degrees.
        /// </summary>
        /// <param name="radians">A value in radians.</param>
        /// <returns>The equivilent value in degrees.</returns>
        public static double ConvertToDegrees(double radians)
        {
            double degrees = radians * (180 / Math.PI);
            return degrees;
        }

        /// <summary>
        /// Converts a value in degrees into radians.
        /// </summary>
        /// <param name="degrees">A value in degrees.</param>
        /// <returns>The equivilent value in radians.</returns>
        public static double ConvertToRadians(double degrees)
        {
            double radians = degrees * (Math.PI / 180);
            return radians;
        }

        /// <summary>
        /// Uses the Haversine formula, assuming a spherical Earth, to calculate
        /// distance (in meters) between two lat/lon points.
        /// 
        /// Haversine is faster but less accurate than Vicenty.
        /// 
        /// You may multiply the results by the handy constants MILES_PER_METER,
        /// FEET_PER_METER, etc. to get the distance in those units instead.
        /// </summary>
        /// <param name="lon1">First point's longitude</param>
        /// <param name="lat1">First point's latitude</param>
        /// <param name="lon2">Second point's longitude</param>
        /// <param name="lat2">Second point's latitude</param>
        /// <returns>The distance between the two points, in meters.</returns>
        public static double HaversineDistanceMeters(double lon1, double lat1, double lon2, double lat2)
        {
            // Implementation based on formula description and examples at:
            // http://www.movable-type.co.uk/scripts/latlong.html
            double latDiffRadians = ConvertToRadians(lat2 - lat1);
            double lonDiffRadians = ConvertToRadians(lon2 - lon1);
            double a = Math.Sin(latDiffRadians / 2) * Math.Sin(latDiffRadians / 2) +
                Math.Cos(ConvertToRadians(lat1)) * Math.Cos(ConvertToRadians(lat2)) *
                Math.Sin(lonDiffRadians / 2) * Math.Sin(lonDiffRadians / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return EARTH_RADIUS_AT_EQUATOR_METERS * c;
        }
        /// <summary>
        /// Uses the Haversine formula, assuming a spherical Earth, to calculate
        /// distance (in meters) between two lat/lon coordinates.
        /// 
        /// Haversine is faster but less accurate than Vicenty.
        /// 
        /// You may multiply the results by the handy constants MILES_PER_METER,
        /// FEET_PER_METER, etc. to get the distance in those units instead.
        /// </summary>
        /// <param name="coord1">First coordinate</param>
        /// <param name="coord2">Second coordinate</param>
        /// <returns>The distance between the two points, in meters.</returns>
        public static double HaversineDistanceMeters(ICoordinate coord1, ICoordinate coord2)
        {
            return HaversineDistanceMeters(coord1.X, coord1.Y, coord2.X, coord2.Y);
        }
        /// <summary>
        /// Uses the Haversine formula, assuming a spherical Earth, to calculate
        /// distance (in meters) between two lat/lon points.
        /// 
        /// Haversine is faster but less accurate than Vicenty.
        /// 
        /// You may multiply the results by the handy constants MILES_PER_METER,
        /// FEET_PER_METER, etc. to get the distance in those units instead.
        /// </summary>
        /// <param name="point1">First point</param>
        /// <param name="point2">Second point</param>
        /// <returns>The distance between the two points, in meters.</returns>
        public static double HaversineDistanceMeters(IPoint point1, IPoint point2)
        {
            return HaversineDistanceMeters(point1.X, point1.Y, point2.X, point2.Y);
        }

        /// <summary>
        /// Uses the Vicenty formula, assuming a ellopsoidal Earth, to calculate
        /// distance (in meters) between two lat/lon points.
        /// 
        /// Vicenty is slower but more accurate than Haversine.
        /// 
        /// You may multiply the results by the handy constants MILES_PER_METER,
        /// FEET_PER_METER, etc. to get the distance in those units instead.
        /// </summary>
        /// <param name="lon1">First point's longitude</param>
        /// <param name="lat1">First point's latitude</param>
        /// <param name="lon2">Second point's longitude</param>
        /// <param name="lat2">Second point's latitude</param>
        /// <returns>The distance between the two points, in meters.</returns>
        public static double VicentyDistanceMeters(double lon1, double lat1, double lon2, double lat2)
        {
            // Implementation based on formula description and examples at:
            // http://www.movable-type.co.uk/scripts/latlong-vincenty.html
            double lonDiffRadians = ConvertToRadians(lon2 - lon1);
            double U1 = Math.Atan((1 - WGS84_ELLIPSOID_F) * Math.Tan(ConvertToRadians(lat1)));
            double U2 = Math.Atan((1 - WGS84_ELLIPSOID_F) * Math.Tan(ConvertToRadians(lat2)));
            double sinU1 = Math.Sin(U1), cosU1 = Math.Cos(U1);
            double sinU2 = Math.Sin(U2), cosU2 = Math.Cos(U2);

            double lambda = lonDiffRadians;
            double lambdaP;
            double iterLimit = 100;
            double cosSqAlpha;
            double sinSigma;
            double cosSigma;
            double cos2SigmaM;
            double sigma;
            do
            {
                double sinLambda = Math.Sin(lambda), cosLambda = Math.Cos(lambda);
                sinSigma = Math.Sqrt((cosU2*sinLambda)*(cosU2*sinLambda) +
                                     (cosU1*sinU2 - sinU1*cosU2*cosLambda)*(cosU1*sinU2 - sinU1*cosU2*cosLambda));
                if (sinSigma == 0)
                {
                    return 0; // co-incident points
                }
                cosSigma = sinU1*sinU2 + cosU1*cosU2*cosLambda;
                sigma = Math.Atan2(sinSigma, cosSigma);
                double sinAlpha = cosU1*cosU2*sinLambda/sinSigma;
                cosSqAlpha = 1 - sinAlpha*sinAlpha;
                cos2SigmaM = cosSigma - 2*sinU1*sinU2/cosSqAlpha;
                if (double.IsNaN(cos2SigmaM))
                {
                    cos2SigmaM = 0; // equatorial line: cosSqAlpha=0 (§6)
                }
                double C = WGS84_ELLIPSOID_F / 16 * cosSqAlpha * (4 + WGS84_ELLIPSOID_F * (4 - 3 * cosSqAlpha));
                lambdaP = lambda;
                lambda = lonDiffRadians + (1 - C) * WGS84_ELLIPSOID_F * sinAlpha *
                             (sigma + C*sinSigma*(cos2SigmaM + C*cosSigma*(-1 + 2*cos2SigmaM*cos2SigmaM)));
            } while (Math.Abs(lambda - lambdaP) > 1e-12 && --iterLimit > 0);

            if (iterLimit == 0)
            {
                return double.NaN; // formula failed to converge
            }
            double uSq = cosSqAlpha*(WGS84_ELLIPSOID_A*WGS84_ELLIPSOID_A - WGS84_ELLIPSOID_B*WGS84_ELLIPSOID_B)/(WGS84_ELLIPSOID_B*WGS84_ELLIPSOID_B);
            double A = 1 + uSq/16384*(4096 + uSq*(-768 + uSq*(320 - 175*uSq)));
            double B = uSq/1024*(256 + uSq*(-128 + uSq*(74 - 47*uSq)));
            double deltaSigma = B*sinSigma*(cos2SigmaM + B/4*(cosSigma*(-1 + 2*cos2SigmaM*cos2SigmaM) -
                                                              B/6*cos2SigmaM*(-3 + 4*sinSigma*sinSigma)*
                                                              (-3 + 4*cos2SigmaM*cos2SigmaM)));
            double s = WGS84_ELLIPSOID_B*A*(sigma - deltaSigma);

            return s;
        }

        /// <summary>
        /// Uses the Vicenty formula, assuming a ellopsoidal Earth, to calculate
        /// distance (in meters) between two lat/lon coordinates.
        /// 
        /// Vicenty is slower but more accurate than Haversine.
        /// 
        /// You may multiply the results by the handy constants MILES_PER_METER,
        /// FEET_PER_METER, etc. to get the distance in those units instead.
        /// </summary>
        /// <param name="coord1">First coordinate</param>
        /// <param name="coord2">Second coordinate</param>
        /// <returns>The distance between the two points, in meters.</returns>
        public static double VicentyDistanceMeters(ICoordinate coord1, ICoordinate coord2)
        {
            return VicentyDistanceMeters(coord1.X, coord1.Y, coord2.X, coord2.Y);
        }
        /// <summary>
        /// Uses the Vicenty formula, assuming a ellopsoidal Earth, to calculate
        /// distance (in meters) between two lat/lon points.
        /// 
        /// Vicenty is slower but more accurate than Haversine.
        /// 
        /// You may multiply the results by the handy constants MILES_PER_METER,
        /// FEET_PER_METER, etc. to get the distance in those units instead.
        /// </summary>
        /// <param name="point1">First point</param>
        /// <param name="point2">Second point</param>
        /// <returns>The distance between the two points, in meters.</returns>
        public static double VicentyDistanceMeters(IPoint point1, IPoint point2)
        {
            return VicentyDistanceMeters(point1.X, point1.Y, point2.X, point2.Y);
        }
    }
}
