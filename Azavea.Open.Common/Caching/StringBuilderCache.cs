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

using System.Text;

namespace Azavea.Open.Common.Caching
{
    /// <summary>
    /// A cache specific for StringBuilder objects.
    /// </summary>
    public class StringBuilderCache : SimpleCache<StringBuilder>
    {
        /// <summary>
        /// Overridden to remove the contents of the builder before reinserting in the cache.
        /// </summary>
        /// <param name="obj">StringBuilder to return</param>
        public override void Return(StringBuilder obj)
        {
            // Clear it out.
            obj.Remove(0, obj.Length);
            base.Return(obj);
        }

        /// <summary>
        /// Overridden to default the string builder to a fairly large size.
        /// </summary>
        /// <returns>A new empty but large string builder.</returns>
        protected override StringBuilder MakeNewOne()
        {
            return new StringBuilder(1000);
        }
    }
}
