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
using NUnit.Framework;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace Azavea.Open.Reprojection.Tests
{
    /// <exclude/>
    [TestFixture]
    public class ReprojectorTests
    {
        // These points are taken from an example in PhillyHistory and Google Earth that
        // were manually checked; that is, the center of a parcel in a shape file, when
        // projected, shows up right where it should be in the Google Earth imagery.
        private readonly Point _paStatePlanePoint = new Point(2691389, 233794);
        private const double _paStatePlaneAccuracy = 1; // accuracy required, in feet

        private readonly Point _wgs84Point = new Point(-75.171409, 39.946146);
        private const double _wgs84Accuracy = .00001; // accuracy required, in degrees

        private readonly Point _nzgd2000PointInNz = new Point(1746525.574, 5428516.364);
        private readonly Point _wgs84PointInNz = new Point(174.74967241287231, -41.281499185755614);

        private readonly Point _webMercatorPointInMeters = new Point(-8368042.9720929, 4858119.44634618);
        private const double _webMercatorAccuracy = 0.5; // accuracy required, in meters

        /// <exclude/>
        [Test]
        public void TestReprojectPAStatePlaneToWGS84()
        {
            Point reprojected = Reprojector.Reproject(Reprojector.PAStatePlane, Reprojector.WGS84, _paStatePlanePoint.X, _paStatePlanePoint.Y);
            double diffX = Math.Abs(reprojected.X - _wgs84Point.X);
            double diffY = Math.Abs(reprojected.Y - _wgs84Point.Y);
            Assert.Less(diffX, _wgs84Accuracy, "Reprojection from PA State Plane to WGS84 was not accurate enough in the X dimension.");
            Assert.Less(diffY, _wgs84Accuracy, "Reprojection from PA State Plane to WGS84 was not accurate enough in the y dimension.");
        }

        /// <exclude/>
        [Test]
        public void TestReprojectPAStatePlaneToWGS84OldSignature()
        {
            Point reprojected = Reprojector.ReprojectPAStatePlaneToWGS84(_paStatePlanePoint.X, _paStatePlanePoint.Y);
            double diffX = Math.Abs(reprojected.X - _wgs84Point.X);
            double diffY = Math.Abs(reprojected.Y - _wgs84Point.Y);
            Assert.Less(diffX, _wgs84Accuracy, "Reprojection from PA State Plane to WGS84 was not accurate enough in the X dimension.");
            Assert.Less(diffY, _wgs84Accuracy, "Reprojection from PA State Plane to WGS84 was not accurate enough in the Y dimension.");
        }

        /// <exclude/>
        [Test]
        public void TestReprojectWGS84ToPAStatePlane()
        {
            Point reprojected = Reprojector.Reproject(Reprojector.WGS84, Reprojector.PAStatePlane, _wgs84Point.X, _wgs84Point.Y);
            double diffX = Math.Abs(reprojected.X - _paStatePlanePoint.X);
            double diffY = Math.Abs(reprojected.Y - _paStatePlanePoint.Y);
            Assert.Less(diffX, _paStatePlaneAccuracy, "Reprojection from WGS84 to PA State Plane was not accurate enough in the X dimension.");
            Assert.Less(diffY, _paStatePlaneAccuracy, "Reprojection from WGS84 to PA State Plane was not accurate enough in the Y dimension.");
        }

        /// <exclude/>
        [Test]
        public void TestReprojectWGS84ToPAStatePlaneOldSignature()
        {
            Point reprojected = Reprojector.ReprojectWGS84ToPAStatePlane(_wgs84Point.X, _wgs84Point.Y);
            double diffX = Math.Abs(reprojected.X - _paStatePlanePoint.X);
            double diffY = Math.Abs(reprojected.Y - _paStatePlanePoint.Y);
            Assert.Less(diffX, _paStatePlaneAccuracy, "Reprojection from WGS84 to PA State Plane was not accurate enough in the X dimension.");
            Assert.Less(diffY, _paStatePlaneAccuracy, "Reprojection from WGS84 to PA State Plane was not accurate enough in the Y dimension.");
        }

        /// <exclude/>
        [Test]
        public void TestReprojectNZGS2000ToWGS84()
        {
            Point reprojected = Reprojector.Reproject(Reprojector.NZGD2000, Reprojector.WGS84, _nzgd2000PointInNz.X, _nzgd2000PointInNz.Y);
            double diffX = Math.Abs(reprojected.X - _wgs84PointInNz.X);
            double diffY = Math.Abs(reprojected.Y - _wgs84PointInNz.Y);
            Assert.Less(diffX, _wgs84Accuracy, "Reprojection from NZGD2000 to WGS84 was not accurate enough in the X dimension.");
            Assert.Less(diffY, _wgs84Accuracy, "Reprojection from NZGD2000 to WGS84 was not accurate enough in the Y dimension.");
        }

        /// <exclude/>
        [Test]
        public void TestReprojectWebMercatorToWGS84()
        {
            Point reprojected = Reprojector.Reproject(Reprojector.WebMercator, Reprojector.WGS84, _webMercatorPointInMeters.X, _webMercatorPointInMeters.Y);
            double diffX = Math.Abs(reprojected.X - _wgs84Point.X);
            double diffY = Math.Abs(reprojected.Y - _wgs84Point.Y);
            Console.WriteLine("X (Longitude) in degrees: " + reprojected.X);
            Console.WriteLine("Y (Latitude) in degrees: " + reprojected.Y);
            Assert.Less(diffX, _wgs84Accuracy, "Reprojection from WebMercator to WGS84 was not accurate enough in the X dimension.");
            Assert.Less(diffY, _wgs84Accuracy, "Reprojection from WebMercator to WGS84 was not accurate enough in the Y dimension.");
        }

        /// <exclude/>
        [Test]
        public void TestReprojectWGS84ToWebMercator()
        {
            Point reprojected = Reprojector.Reproject(Reprojector.WGS84, Reprojector.WebMercator, _wgs84Point.X, _wgs84Point.Y);
            double diffX = Math.Abs(reprojected.X - _webMercatorPointInMeters.X);
            double diffY = Math.Abs(reprojected.Y - _webMercatorPointInMeters.Y);
            Console.WriteLine("X (Longitude) in meters: " + reprojected.X);
            Console.WriteLine("Y (Latitude) in meters: " + reprojected.Y);
            Assert.Less(diffX, _webMercatorAccuracy, "Reprojection from WGS84 to WebMercator was not accurate enough in the X dimension.");
            Assert.Less(diffY, _webMercatorAccuracy, "Reprojection from WGS84 to WebMercator was not accurate enough in the Y dimension.");
        }

        /// <exclude/>
        [Test]
        public void TestReprojectWGS84AndWebMercatorLots()
        {
            AssertWGS84Mercator(0, 0, 0, -7.08115455161362E-10);
            AssertWGS84Mercator(0, 30, 0, 3503549.84350437);
            AssertWGS84Mercator(0, -30, 0, -3503549.84350437);
            AssertWGS84Mercator(0, 60, 0, 8399737.88981836);
            AssertWGS84Mercator(0, -60, 0, -8399737.88981836);
            AssertWGS84Mercator(0, 89, 0, 30240971.9583862);
            AssertWGS84Mercator(0, -89, 0, -30240971.9583862);
            AssertWGS84Mercator(45, 0, 5009377.08569731, -7.08115455161362E-10);
            AssertWGS84Mercator(45, 30, 5009377.08569731, 3503549.84350437);
            AssertWGS84Mercator(45, -30, 5009377.08569731, -3503549.84350437);
            AssertWGS84Mercator(45, 60, 5009377.08569731, 8399737.88981836);
            AssertWGS84Mercator(45, -60, 5009377.08569731, -8399737.88981836);
            AssertWGS84Mercator(45, 89, 5009377.08569731, 30240971.9583862);
            AssertWGS84Mercator(45, -89, 5009377.08569731, -30240971.9583862);
            AssertWGS84Mercator(-45, 0, -5009377.08569731, -7.08115455161362E-10);
            AssertWGS84Mercator(-45, 30, -5009377.08569731, 3503549.84350437);
            AssertWGS84Mercator(-45, -30, -5009377.08569731, -3503549.84350437);
            AssertWGS84Mercator(-45, 60, -5009377.08569731, 8399737.88981836);
            AssertWGS84Mercator(-45, -60, -5009377.08569731, -8399737.88981836);
            AssertWGS84Mercator(-45, 89, -5009377.08569731, 30240971.9583862);
            AssertWGS84Mercator(-45, -89, -5009377.08569731, -30240971.9583862);
            AssertWGS84Mercator(90, 0, 10018754.1713946, -7.08115455161362E-10);
            AssertWGS84Mercator(90, 30, 10018754.1713946, 3503549.84350437);
            AssertWGS84Mercator(90, -30, 10018754.1713946, -3503549.84350437);
            AssertWGS84Mercator(90, 60, 10018754.1713946, 8399737.88981836);
            AssertWGS84Mercator(90, -60, 10018754.1713946, -8399737.88981836);
            AssertWGS84Mercator(90, 89, 10018754.1713946, 30240971.9583862);
            AssertWGS84Mercator(90, -89, 10018754.1713946, -30240971.9583862);
            AssertWGS84Mercator(-90, 0, -10018754.1713946, -7.08115455161362E-10);
            AssertWGS84Mercator(-90, 30, -10018754.1713946, 3503549.84350437);
            AssertWGS84Mercator(-90, -30, -10018754.1713946, -3503549.84350437);
            AssertWGS84Mercator(-90, 60, -10018754.1713946, 8399737.88981836);
            AssertWGS84Mercator(-90, -60, -10018754.1713946, -8399737.88981836);
            AssertWGS84Mercator(-90, 89, -10018754.1713946, 30240971.9583862);
            AssertWGS84Mercator(-90, -89, -10018754.1713946, -30240971.9583862);
            AssertWGS84Mercator(135, 0, 15028131.2570919, -7.08115455161362E-10);
            AssertWGS84Mercator(135, 30, 15028131.2570919, 3503549.84350437);
            AssertWGS84Mercator(135, -30, 15028131.2570919, -3503549.84350437);
            AssertWGS84Mercator(135, 60, 15028131.2570919, 8399737.88981836);
            AssertWGS84Mercator(135, -60, 15028131.2570919, -8399737.88981836);
            AssertWGS84Mercator(135, 89, 15028131.2570919, 30240971.9583862);
            AssertWGS84Mercator(135, -89, 15028131.2570919, -30240971.9583862);
            AssertWGS84Mercator(-135, 0, -15028131.2570919, -7.08115455161362E-10);
            AssertWGS84Mercator(-135, 30, -15028131.2570919, 3503549.84350437);
            AssertWGS84Mercator(-135, -30, -15028131.2570919, -3503549.84350437);
            AssertWGS84Mercator(-135, 60, -15028131.2570919, 8399737.88981836);
            AssertWGS84Mercator(-135, -60, -15028131.2570919, -8399737.88981836);
            AssertWGS84Mercator(-135, 89, -15028131.2570919, 30240971.9583862);
            AssertWGS84Mercator(-135, -89, -15028131.2570919, -30240971.9583862);
            AssertWGS84Mercator(179.99, 0, 20036395.1478813, -7.08115455161362E-10);
            AssertWGS84Mercator(179.99, 30, 20036395.1478813, 3503549.84350437);
            AssertWGS84Mercator(179.99, -30, 20036395.1478813, -3503549.84350437);
            AssertWGS84Mercator(179.99, 60, 20036395.1478813, 8399737.88981836);
            AssertWGS84Mercator(179.99, -60, 20036395.1478813, -8399737.88981836);
            AssertWGS84Mercator(179.99, 89, 20036395.1478813, 30240971.9583862);
            AssertWGS84Mercator(179.99, -89, 20036395.1478813, -30240971.9583862);
            AssertWGS84Mercator(-179.99, 0, -20036395.1478813, -7.08115455161362E-10);
            AssertWGS84Mercator(-179.99, 30, -20036395.1478813, 3503549.84350437);
            AssertWGS84Mercator(-179.99, -30, -20036395.1478813, -3503549.84350437);
            AssertWGS84Mercator(-179.99, 60, -20036395.1478813, 8399737.88981836);
            AssertWGS84Mercator(-179.99, -60, -20036395.1478813, -8399737.88981836);
            AssertWGS84Mercator(-179.99, 89, -20036395.1478813, 30240971.9583862);
            AssertWGS84Mercator(-179.99, -89, -20036395.1478813, -30240971.9583862);
        }

        private static void AssertWGS84Mercator(double lon, double lat, double mercatorX, double mercatorY)
        {
            Point reprojected = Reprojector.ReprojectWGS84ToWebMercator(lon, lat);
            Assert.Less(mercatorX - _webMercatorAccuracy, reprojected.X, "Lat: " + lat + ", Lon: " + lon + ", Reprojected X value was too low.");
            Assert.Greater(mercatorX + _webMercatorAccuracy, reprojected.X, "Lat: " + lat + ", Lon: " + lon + ", Reprojected X value was too high.");
            Assert.Less(mercatorY - _webMercatorAccuracy, reprojected.Y, "Lat: " + lat + ", Lon: " + lon + ", Reprojected Y value was too low.");
            Assert.Greater(mercatorY + _webMercatorAccuracy, reprojected.Y, "Lat: " + lat + ", Lon: " + lon + ", Reprojected Y value was too high.");
            reprojected = Reprojector.ReprojectWebMercatorToWGS84(mercatorX, mercatorY);
            Assert.Less(lon - _wgs84Accuracy, reprojected.X, "X: " + mercatorX + ", Y: " + mercatorY + ", Reprojected longitude value was too low.");
            Assert.Greater(lon + _wgs84Accuracy, reprojected.X, "X: " + mercatorX + ", Y: " + mercatorY + ", Reprojected longitude value was too high.");
            Assert.Less(lat - _wgs84Accuracy, reprojected.Y, "X: " + mercatorX + ", Y: " + mercatorY + ", Reprojected latitude value was too low.");
            Assert.Greater(lat + _wgs84Accuracy, reprojected.Y, "X: " + mercatorX + ", Y: " + mercatorY + ", Reprojected latitude value was too high.");
        }

        /// <exclude/>
        [Test]
        public void TestBadReprojectWGS84ToWebMercator()
        {
            AssertWGS84ToMercatorFails(0, 90);
            AssertWGS84ToMercatorFails(0, -90);
            AssertWGS84ToMercatorFails(0, 150);
            AssertWGS84ToMercatorFails(0, -150);
        }

        private static void AssertWGS84ToMercatorFails(double lon, double lat)
        {
            bool threw = false;
            Point reprojected = null;
            try
            {
                reprojected = Reprojector.ReprojectWGS84ToWebMercator(lon, lat);
            }
            catch (Exception)
            {
                // Good.
                threw = true;
            }
            if (!threw)
            {
                Assert.Fail("No exception when given invalid lat: " + lat + ", lon: " + lon +
                            ".  Reprojected to x: " + reprojected.X + ", y: " + reprojected.Y);
            }
        }

        /// <exclude />
        [Test]
        public void TestGetCoordinateSystemBySRID()
        {
            ICoordinateSystem cs = Reprojector.GetCoordinateSystemBySRID(4326);
            Assert.AreEqual("WGS 84", cs.Name);
        }
    }
}