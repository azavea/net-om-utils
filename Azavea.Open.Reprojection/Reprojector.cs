// Copyright (c) 2004-2010 Azavea, Inc.
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
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using SharpMap.CoordinateSystems.Transformations;
using System.Collections.Generic;

namespace Azavea.Open.Reprojection
{
    /// <summary>
    /// Static helper class for reprojecting points between various projection systems.
    /// </summary>
    public static class Reprojector
    {
        private static readonly Dictionary<ICoordinateSystem[], ICoordinateTransformation> _transformationCache = 
            new Dictionary<ICoordinateSystem[], ICoordinateTransformation>();

        const string PAStatePlane_WKT = "PROJCS[\"NAD83 / Pennsylvania South (ftUS)\",GEOGCS[\"NAD83\",DATUM[\"North_American_Datum_1983\",SPHEROID[\"GRS 1980\",6378137,298.257222101,AUTHORITY[\"EPSG\",\"7019\"]],AUTHORITY[\"EPSG\",\"6269\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.01745329251994328,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4269\"]],PROJECTION[\"Lambert_Conformal_Conic_2SP\"],PARAMETER[\"standard_parallel_1\",40.96666666666667],PARAMETER[\"standard_parallel_2\",39.93333333333333],PARAMETER[\"latitude_of_origin\",39.33333333333334],PARAMETER[\"central_meridian\",-77.75],PARAMETER[\"false_easting\",1968500],PARAMETER[\"false_northing\",0],UNIT[\"US survey foot\",0.3048006096012192,AUTHORITY[\"EPSG\",\"9003\"]],AUTHORITY[\"EPSG\",\"2272\"]]";
        const string WGS84_WKT = "GEOGCS[\"WGS 84\",DATUM[\"WGS_1984\",SPHEROID[\"WGS 84\",6378137,298.257223563,AUTHORITY[\"EPSG\",\"7030\"]],AUTHORITY[\"EPSG\",\"6326\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.01745329251994328,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4326\"]]";
        const string NZGD_2000 = "PROJCS[\"NZGD_2000_New_Zealand_Transverse_Mercator\",GEOGCS[\"GCS_NZGD_2000\",DATUM[\"D_NZGD_2000\",SPHEROID[\"GRS_1980\",6378137.0,298.257222101]],PRIMEM[\"Greenwich\",0.0],UNIT[\"Degree\",0.0174532925199433]],PROJECTION[\"Transverse_Mercator\"],PARAMETER[\"False_Easting\",1600000.0],PARAMETER[\"False_Northing\",10000000.0],PARAMETER[\"Central_Meridian\",173.0],PARAMETER[\"Scale_Factor\",0.9996],PARAMETER[\"Latitude_Of_Origin\",0.0],UNIT[\"Meter\",1.0]]";

        // These are all equivalent WKT for Web Mercator projections, but while producing good results in X dimension, they are producing bad results for Y dimension with Proj.Net
        //const string WEB_MERCATOR_WKT = "PROJCS[\"WGS_1984_Web_Mercator\",GEOGCS[\"GCS_WGS_1984_Major_Auxiliary_Sphere\",DATUM[\"D_WGS_1984_Major_Auxiliary_Sphere\",SPHEROID[\"WGS_1984_Major_Auxiliary_Sphere\",6378137.0,0.0]],PRIMEM[\"Greenwich\",0.0],UNIT[\"Degree\",0.0174532925199433]],PROJECTION[\"Mercator\"],PARAMETER[\"False_Easting\",0.0],PARAMETER[\"False_Northing\",0.0],PARAMETER[\"Central_Meridian\",0.0],PARAMETER[\"Standard_Parallel_1\",0.0],PARAMETER[\"Latitude_Of_Origin\",0.0],UNIT[\"Meter\",1.0]]";
        //const string WEB_MERCATOR_WKT = "PROJCS[\"WGS_1984_Web_Mercator\", GEOGCS[\"GCS_WGS_1984_Major_Auxiliary_Sphere\", DATUM[\"D_WGS_1984_Major_Auxiliary_Sphere\", SPHEROID[\"WGS_1984_Major_Auxiliary_Sphere\", 6378137.0, 0.0, AUTHORITY[\"EPSG\",\"7059\"]], TOWGS84[0, 0, 0, 0, 0, 0, 0], AUTHORITY[\"EPSG\",\"6055\"]], PRIMEM[\"Greenwich\", 0, AUTHORITY[\"EPSG\", \"8901\"]], UNIT[\"degree\", 0.0174532925199433, AUTHORITY[\"EPSG\", \"9102\"]], AXIS[\"E\", EAST], AXIS[\"N\", NORTH], AUTHORITY[\"EPSG\",\"4055\"]], PROJECTION[\"Mercator\"], PARAMETER[\"False_Easting\", 0], PARAMETER[\"False_Northing\", 0], PARAMETER[\"Central_Meridian\", 0], PARAMETER[\"Latitude_of_origin\", 0], UNIT[\"metre\", 1.0, AUTHORITY[\"EPSG\", \"9001\"]], AXIS[\"East\", EAST], AXIS[\"North\", NORTH], AUTHORITY[\"EPSG\",\"3785\"]]";

        // None of the WKT options work for Proj.net, but we need to have one so we can create the ICoordinateSystem
        // later on.  See the comment on the 'WebMercator' attribute as well.
        const string BROKEN_WEB_MERCATOR_WKT = "PROJCS[\"Mercator Spheric\",GEOGCS[\"WGS84basedSpheric_GCS\",DATUM[\"WGS84basedSpheric_Datum\",SPHEROID[\"WGS84based_Sphere\",6378137.0,0.0],TOWGS84[0, 0, 0, 0, 0, 0, 0]],PRIMEM[\"Greenwich\",0.0],UNIT[\"Degree\",0.0174532925199433 ],],PROJECTION[\"Mercator\"], PARAMETER[\"False_Easting\",0.0], PARAMETER[\"False_Northing\",0.0], PARAMETER[\"Central_Meridian\",0.0], PARAMETER[\"Standard_Parallel_1\",0.0], PARAMETER[\"Latitude_Of_Origin\",0.0], UNIT[\"Meter\", 1.0]]";
        //private const string WEB_MERCATOR_EPSG_3785 =
        //    "PROJCS[\"Popular Visualisation CRS / Mercator\", GEOGCS[\"Popular Visualisation CRS\", DATUM[\"Popular Visualisation Datum\", SPHEROID[\"Popular Visualisation Sphere\", 6378137, 0, AUTHORITY[\"EPSG\",7059]], TOWGS84[0, 0, 0, 0, 0, 0, 0], AUTHORITY[\"EPSG\",6055]], PRIMEM[\"Greenwich\", 0, AUTHORITY[\"EPSG\", \"8901\"]], UNIT[\"degree\", 0.0174532925199433, AUTHORITY[\"EPSG\", \"9102\"]], AXIS[\"E\", EAST], AXIS[\"N\", NORTH], AUTHORITY[\"EPSG\",4055]], PROJECTION[\"Mercator\"], PARAMETER[\"False_Easting\", 0], PARAMETER[\"False_Northing\", 0], PARAMETER[\"Central_Meridian\", 0], PARAMETER[\"Latitude_of_origin\", 0], UNIT[\"metre\", 1, AUTHORITY[\"EPSG\", \"9001\"]], AXIS[\"East\", EAST], AXIS[\"North\", NORTH], AUTHORITY[\"EPSG\",3785]]";

        /// <summary>
        /// Coordinate system for Pennsylvania State Plane South.
        /// </summary>
        public static readonly ICoordinateSystem PAStatePlane = SharpMap.Converters.WellKnownText.CoordinateSystemWktReader.Parse(PAStatePlane_WKT) as ICoordinateSystem;
        /// <summary>
        /// Coordinate system for WGS84 (most common lat/lon projection).
        /// </summary>
        public static readonly ICoordinateSystem WGS84 = SharpMap.Converters.WellKnownText.CoordinateSystemWktReader.Parse(WGS84_WKT) as ICoordinateSystem;
        /// <summary>
        /// Coordinate system for New Zealand?.
        /// </summary>
        public static readonly ICoordinateSystem NZGD2000 = SharpMap.Converters.WellKnownText.CoordinateSystemWktReader.Parse(NZGD_2000) as ICoordinateSystem;
        
        /// <summary>
        /// Coordinate system for Web Mercator.  This is not well defined, and needs a hack in the reprojector, but
        /// we still want to have the CoordinateSystem around nominally, so it can be used in the standard way.
        /// </summary>
        public static readonly ICoordinateSystem WebMercator = SharpMap.Converters.WellKnownText.CoordinateSystemWktReader.Parse(BROKEN_WEB_MERCATOR_WKT) as ICoordinateSystem;

        /// <summary>
        /// Given a pair of coordinate systems, projects a point from one to another.
        /// </summary>
        /// <param name="from">The coordinate system from which to convert the point.</param>
        /// <param name="to"></param>
        /// <param name="xLon">The X or Longitude coordinate of the point to be reprojected</param>
        /// <param name="yLat">The Y or Latitude coordinate of the point to be reprojected</param>
        /// <returns>A new point, in the projection requested</returns>
        public static Point Reproject(ICoordinateSystem from, ICoordinateSystem to, double xLon, double yLat)
        {
            // If we're asked to go to/from web mercator, use the WM-specific methods to go through WGS84.
            if (from.Name == WebMercator.Name)
            {
                Point fromInWgs84 = ReprojectWebMercatorToWGS84(xLon, yLat);
                return Reproject(WGS84, to, fromInWgs84.X, fromInWgs84.Y);
            }
            if (to.Name == WebMercator.Name)
            {
                Point toInWgs84 = Reproject(from, WGS84, xLon, yLat);
                return ReprojectWGS84ToWebMercator(toInWgs84.X, toInWgs84.Y);
            }

            ICoordinateSystem[] key = new [] {from, to};
            ICoordinateTransformation trans = 
                _transformationCache.ContainsKey(key) ?
                                                          _transformationCache[key] :
                                                                                        new CoordinateTransformationFactory().CreateFromCoordinateSystems(from, to);
            double[] result = trans.MathTransform.Transform(new [] { xLon, yLat });
            return new Point(result[0], result[1]);
        }

        /// <summary>
        /// Given a couple coordinate systems, projects a point from one to another.
        /// </summary>
        /// <param name="from">The coordinate system which the point is in originally</param>
        /// <param name="to">The coordinate system into which the point will be reprojected</param>
        /// <param name="point">The point to be reprojected</param>
        /// <returns>A new point, in the projection requested</returns>
        public static IPoint Reproject(ICoordinateSystem from, ICoordinateSystem to, IPoint point)
        {
            return Reproject(from, to, point.X, point.Y);
        }

        #region commonly used reprojections
        /// <summary>
        /// Convert lat/long decimal degree coordinates (WGS84 datum) to X / Y coordinates in PA State Plane South projection (feet).
        /// </summary>
        /// <param name="lon">longitude in decimal degrees based on WGS84 horizontal datum</param>
        /// <param name="lat">latitude in decimal degrees based on WGS84 horizontal datum</param>
        /// <returns>A new point, in the PA State Plane coordinate system</returns>
        public static Point ReprojectWGS84ToPAStatePlane(double lon, double lat)
        {
            return Reproject(WGS84, PAStatePlane, lon, lat);
        }

        /// <summary>
        /// Convert X / Y coordinates in PA State Plane South projection (feet) to lat/long decimal degree coordinates (WGS84 datum).
        /// </summary>
        /// <param name="x">x value in feet for PA State Plane South projection</param>
        /// <param name="y">y value in feet for PA State Plane South projection</param>
        /// <returns>A new point, in the WGS 84 coordinate system</returns>
        public static Point ReprojectPAStatePlaneToWGS84(double x, double y)
        {
            return Reproject(PAStatePlane, WGS84, x, y);
        }

        /// <summary>
        /// Convert lat/long decimal degree coordinates (WGS84 datum) to X / Y coordinates in Web Mercator projection (meters).  Note that this is a manual re-projection and does not use PROJ.Net.
        /// </summary>
        /// <param name="lon">longitude in decimal degrees based on WGS84 horizontal datum</param>
        /// <param name="lat">latitude in decimal degrees based on WGS84 horizontal datum</param>
        /// <returns>A new point, in the Web Mercator coordinate system</returns>
        public static Point ReprojectWGS84ToWebMercator(double lon, double lat)
        {
            return new Point(ConvertLongitudeToMercatorX(lon), ConvertLatitudeToMercatorY(lat));
        }

        /// <summary>
        /// Convert X / Y coordinates in Web Mercator projection (meters) to lat/long decimal degree coordinates (WGS84 datum).  Note that this is a manual re-projection and does not use PROJ.Net.
        /// </summary>
        /// <param name="x">x value in meters for Spherical Mercator projection</param>
        /// <param name="y">y value in meters for Spherical Mercator projection</param>
        /// <returns>A new point, in the WGS 84 coordinate system</returns>
        public static Point ReprojectWebMercatorToWGS84(double x, double y)
        {
            return new Point(ConvertMercatorXToLongitude(x), ConvertMercatorYToLatitude(y));
        }
        #endregion

        #region formulas for Spherical Mercator
        private static double ConvertLongitudeToMercatorX(double longitudeDegrees)
        {
            double longitude = GeoMath.ConvertToRadians(longitudeDegrees);
            return (GeoMath.EARTH_RADIUS_AT_EQUATOR_METERS * longitude);
        }

        private static double ConvertLatitudeToMercatorY(double latitudeDegrees)
        {
            if ((latitudeDegrees > 89.999999) || (latitudeDegrees < -89.999999))
            {
                throw new ArgumentException("Latitude " + latitudeDegrees +
                                            " is outside the valid range, would produce infinity or NaN.");
            }
            double latitude = GeoMath.ConvertToRadians(latitudeDegrees);
            double y = GeoMath.EARTH_RADIUS_AT_EQUATOR_METERS / 2.0 *
                       Math.Log((1.0 + Math.Sin(latitude)) /
                                (1.0 - Math.Sin(latitude)));
            return y;
        }

        private static double ConvertMercatorXToLongitude(double x)
        {
            double longRadians = x / GeoMath.EARTH_RADIUS_AT_EQUATOR_METERS;
            double longDegrees = GeoMath.ConvertToDegrees(longRadians);

            /* The user could have panned around the world a lot of times.
            Lat long goes from -180 to 180.  So every time a user gets 
            to 181 we want to subtract 360 degrees.  Every time a user
            gets to -181 we want to add 360 degrees. */

            double rotations = Math.Floor((longDegrees + 180) / 360);
            double longitude = longDegrees - (rotations * 360);
            return longitude;
        }

        private static double ConvertMercatorYToLatitude(double y)
        {
            double latitude = (Math.PI / 2) -
                              (2 * Math.Atan(
                                       Math.Exp(-y / GeoMath.EARTH_RADIUS_AT_EQUATOR_METERS)));
            return GeoMath.ConvertToDegrees(latitude);
        }

        #endregion

        /// <summary>
        /// Return a coordinate system object given a SRID.
        /// </summary>
        /// <param name="srid">Spatial Reference ID of the desired coordinate system</param>
        /// <returns>The coordinate system with the SRID requested, if defined, or 
        /// null if it is not defined</returns>
        public static ICoordinateSystem GetCoordinateSystemBySRID(int srid)
        {
            foreach (ICoordinateSystem cs in new ICoordinateSystem[]
                                                 {
                                                     PAStatePlane,
                                                     WGS84,
                                                     NZGD2000,
                                                     WebMercator
                                                 })
            {
                if (cs.AuthorityCode == srid)
                {
                    return cs;
                }
            }
            return null;
        }
    }
}