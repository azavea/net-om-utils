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
using System.Collections.Generic;
using Avencia.Open.Common.Caching;
using NUnit.Framework;

namespace Avencia.Open.Common.Tests
{
    /// <exclude/>
    [TestFixture]
    public class TimedCacheTests
    {
        /// <exclude/>
        [Test]
        public void TryGetTest1()
        {
            TimedCache<int, object> cache = new TimedCache<int, object>(new TimeSpan(0, 1, 0));

            object o = null;
            Assert.IsFalse(cache.TryGet(1, ref o));
            Assert.IsNull(o);

            cache.Set(1, o);
            Assert.IsTrue(cache.TryGet(1, ref o));
            Assert.IsNull(o);

            o = new object();
            cache.Set(1, o);
            Assert.IsTrue(cache.TryGet(1, ref o));
            Assert.IsNotNull(o);
        }

        /// <exclude/>
        [Test]
        public void TryGetTest2()
        {
            TimedCache<int, int> cache = new TimedCache<int, int>(new TimeSpan(0, 1, 0));

            int output = 5;
            Assert.IsFalse(cache.TryGet(1, ref output));
            Assert.AreEqual(5, output);

            cache.Set(10, output);
            int output2 = Int32.MinValue;
            Assert.IsTrue(cache.TryGet(10, ref output2));
            Assert.AreEqual(5, output2);

            output = 15;
            cache.Set(10, output);
            int output3 = Int32.MinValue;
            Assert.IsTrue(cache.TryGet(10, ref output3));
            Assert.AreEqual(15, output3);
        }

        /// <exclude/>
        [Test]
        public void ResetTest()
        {
            TimedCache<int, int> cache = new TimedCache<int, int>(new TimeSpan(0, 1, 0));
            cache.Set(1, 2);
            cache.Reset(1);
        }

        /// <exclude/>
        [Test, ExpectedException(typeof(KeyNotFoundException))]
        public void ResetTest2()
        {
            TimedCache<int, int> cache = new TimedCache<int, int>(new TimeSpan(0, 1, 0));

            cache.Reset(1);
        }

        /// <exclude/>
        [Ignore("This test relies on Thread.Sleep, which is notoriously inconsistent.  Unignore it and test locally if you make changes to the code.")]
        [Test, ExpectedException(typeof(KeyNotFoundException))]
        public void ResetTest3()
        {
            TimedCache<int, int> cache = new TimedCache<int, int>(new TimeSpan(0, 0, 5));
            cache.Set(1, 2);
            System.Threading.Thread.Sleep(5100);
            cache.Reset(1);
        }

        /// <exclude/>
        [Test]
        public void TryResetTest()
        {
            TimedCache<int, int> cache = new TimedCache<int, int>(new TimeSpan(0, 1, 0));
            cache.Set(1, 2);
            Assert.IsTrue(cache.TryReset(1));
        }

        /// <exclude/>
        [Test]
        public void TryResetTest2()
        {
            TimedCache<int, int> cache = new TimedCache<int, int>(new TimeSpan(0, 1, 0));

            Assert.IsFalse(cache.TryReset(1));
        }

        /// <exclude/>
        [Test]
        [Ignore("This test relies on Thread.Sleep, which is notoriously inconsistent.  Unignore it and test locally if you make changes to the code.")]
        public void TryResetTest3()
        {
            TimedCache<int, int> cache = new TimedCache<int, int>(new TimeSpan(0, 0, 5));
            cache.Set(1, 2);
            System.Threading.Thread.Sleep(5100);
            Assert.IsFalse(cache.TryReset(1));
        }
    }
}
