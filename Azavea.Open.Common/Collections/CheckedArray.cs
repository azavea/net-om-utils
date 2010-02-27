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

namespace Azavea.Open.Common.Collections
{
    /// <summary>
    /// This is a class of methods that are workarounds for the fact that 
    /// basic system arrays don't have useful enough error messages.
    /// Methods on here should do a normal simple operation and throw an exception
    /// with enough data to debug it.
    /// </summary>
    public class CheckedArray
    {
        /// <summary>
        /// Attempts to get a value from that index within the array.
        /// If unable to, throws an exception that says what index it tried to get
        /// and how long the array was.
        /// </summary>
        /// <typeparam name="T">Type of value in the array.</typeparam>
        /// <param name="arr">Array to get a value from.</param>
        /// <param name="index">Which value do you want.</param>
        /// <returns>The value.</returns>
        public static T Get<T>(T[] arr, int index)
        {
            return Get<T, T>(arr, index);
        }

        /// <summary>
        /// Attempts to get a value from that index within the array.
        /// If unable to, throws an exception that says what index it tried to get
        /// and how long the array was.
        /// </summary>
        /// <typeparam name="T">Type of value you want back.</typeparam>
        /// <typeparam name="A">Type of values in the array</typeparam>
        /// <param name="arr">Array to get a value from.</param>
        /// <param name="index">Which value do you want.</param>
        /// <returns>The value.</returns>
        public static T Get<T, A>(A[] arr, int index)
        {
            if (arr == null)
            {
                throw new NullReferenceException("Array was null, cannot get a value at index '" + index + "' from it.");
            }
            if (arr.Length <= index)
            {
                throw new IndexOutOfRangeException("You tried to access index " + index +
                    " in an array that is only " + arr.Length + " element long.");
            }
            object untyped = arr[index];
            T retVal;
            try
            {
                retVal = (T)untyped;
            }
            catch (Exception e)
            {
                throw new InvalidCastException("Value at index " + index + ", '" + untyped +
                    "', from the array could not be cast to type " + typeof(T), e);
            }
            return retVal;
        }
    }
}
