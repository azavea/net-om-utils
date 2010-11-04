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

using System.Collections;
using System.Collections.Generic;

namespace Azavea.Open.Common.Collections
{
    /// <summary>
    /// The System.Collections.CaseInsensitiveComparer does not actually implement
    /// IEqualityComparer, so we have to do it for them.
    /// </summary>
    public class CaseInsensitiveStringComparer : IEqualityComparer<string>
    {
        /// <summary>
        /// We save this on construction because apparently calling .Default
        /// does some real work, it was taking a substantial amount of time
        /// during performance profiling.  Doing this once produced about
        /// a 60% reduction in time spent calling Equals(x,y).
        /// </summary>
        private readonly CaseInsensitiveComparer _systemComparer = CaseInsensitiveComparer.Default;
        /// <summary>
        ///                     Determines whether the specified objects are equal.
        /// </summary>
        /// <returns>
        /// true if the specified objects are equal; otherwise, false.
        /// </returns>
        /// <param name="x">
        ///                     The first string to compare.
        ///                 </param>
        /// <param name="y">
        ///                     The second string to compare.
        ///                 </param>
        public bool Equals(string x, string y)
        {
            return _systemComparer.Compare(x, y) == 0;
        }

        /// <summary>
        ///                     Returns a hash code for the specified object.
        /// </summary>
        /// <returns>
        ///                     A hash code for the specified object.
        /// </returns>
        /// <param name="obj">
        ///                     The <see cref="T:System.Object" /> for which a hash code is to be returned.
        ///                 </param>
        /// <exception cref="T:System.ArgumentNullException">
        ///                     The type of <paramref name="obj" /> is a reference type and <paramref name="obj" /> is null.
        ///                 </exception>
        public int GetHashCode(string obj)
        {
            return obj.ToUpper().GetHashCode();
        }
    }
}
