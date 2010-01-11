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

namespace Avencia.Open.Common.Caching
{
    /// <summary>
    /// Represents a piece of data plus a timestamp that is relevant to that data.
    /// </summary>
    /// <typeparam name="T">The type of data held in this item.</typeparam>
    public class TimestampedData<T>
    {
        /// <summary>
        /// The data that you're timestamping.
        /// </summary>
        public readonly T Data;
        /// <summary>
        /// This is the time associated with that data.
        /// </summary>
        public readonly DateTime Time;

        /// <summary>
        /// Timestamp some data.  The timestamp used will be 'DateTime.Now'.
        /// </summary>
        /// <param name="data">The data to timestamp.</param>
        public TimestampedData(T data)
        {
            Data = data;
            Time = DateTime.Now;
        }
    }
}
