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
using System.Threading;
using NUnit.Framework;

namespace Avencia.Open.Common.Tests
{
    /// <exclude/>
    [TestFixture]
    public class ChronoTests
    {
        /// <exclude/>
        // NOTE: Thread.Sleep() appears to wake up anywhere up to 6.25 ms before it's supposed to
        // (not every time, but fairly reliably).  So the checks in here have to allow pretty wide
        // time variations.  We'll use 8ms for a tolerance.
        [Test]
        public void TestSplitMS()
        {
            Chronometer chrono = new Chronometer();
            chrono.Start();
            Thread.Sleep(100);
            Assert.Less(92.0, Convert.ToDouble(chrono.GetSplitMilliseconds().Split(' ')[0]),
                "Chrono is running slow.");
        }
        /// <exclude/>
        [Test]
        public void TestSplitS()
        {
            Chronometer chrono = new Chronometer();
            chrono.Start();
            Thread.Sleep(1000);
            Assert.Less(0.92, Convert.ToDouble(chrono.GetSplitSeconds().Split(' ')[0]),
                "Chrono is running slow.");
        }
        /// <exclude/>
        [Test]
        public void TestSplit()
        {
            Chronometer chrono = new Chronometer();
            chrono.Start();
            Thread.Sleep(100);
            Assert.Less(920000, (int)chrono.GetSplit().Ticks,
                "Chrono is running slow.");
            Thread.Sleep(100);
            int ticks = (int)chrono.GetSplit().Ticks;
            Assert.Greater(ticks, 920000, "Chrono is running slow second time.");

        }
        /// <exclude/>
        [Test]
        public void TestReset()
        {
            Chronometer chrono = new Chronometer();
            chrono.Start();
            Thread.Sleep(100);
            Assert.Less(920000, (int)chrono.GetSplit().Ticks,
                "Chrono is running slow.");
            chrono.Reset();
            Thread.Sleep(50);
            TimeSpan split = chrono.GetSplit();
            Assert.Greater((int)split.Ticks, 430000, "Chrono is running slow after reset.");
        }
        /// <exclude/>
        [Test]
        public void TestRunTime()
        {
            Chronometer chrono = new Chronometer();
            chrono.Start();
            Thread.Sleep(100);
            Assert.Less(920000, (int)chrono.GetRunTime().Ticks,
                "Chrono is running slow.");
            Thread.Sleep(100);
            Assert.Less(1840000, (int)chrono.GetRunTime().Ticks,
                "Chrono is running slow.");
            chrono.Reset();
            Thread.Sleep(50);
            TimeSpan split = chrono.GetRunTime();
            Assert.Greater((int)split.Ticks, 430000, "Chrono is running slow after reset.");
        }
    }
}
