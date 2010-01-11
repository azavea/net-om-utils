// Copyright (c) 2004-2009 Avencia, Inc.
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

namespace Avencia.Open.Common.Collections
{
    /// <summary>
    /// This is a class of methods that are workarounds for the fact that 
    /// basic system lists don't have useful enough error messages.
    /// Methods on here should do a normal simple operation and throw an exception
    /// with enough data to debug it.
    /// </summary>
    public class CheckedList
    {
        /// <summary>
        /// Attempts to get a value from that index within the list.
        /// If unable to, throws an exception that says what index it tried to get
        /// and how long the list was.
        /// </summary>
        /// <typeparam name="T">Type of value in the list.</typeparam>
        /// <param name="list">list to get a value from.</param>
        /// <param name="index">Which value do you want.</param>
        /// <returns>The value.</returns>
        public static T Get<T>(IList<object> list, int index)
        {
            return Get<T, object>(list, index);
        }

        /// <summary>
        /// Attempts to get a value from that index within the list.
        /// If unable to, throws an exception that says what index it tried to get
        /// and how long the list was.
        /// </summary>
        /// <typeparam name="T">Type of value you want back.</typeparam>
        /// <typeparam name="A">Type of values in the list</typeparam>
        /// <param name="list">list to get a value from.</param>
        /// <param name="index">Which value do you want.</param>
        /// <returns>The value.</returns>
        public static T Get<T, A>(IList<A> list, int index)
        {
            if (list == null)
            {
                throw new NullReferenceException("List was null, cannot get a value at index '" + index + "' from it.");
            }
            if (list.Count <= index)
            {
                throw new IndexOutOfRangeException("You tried to access index " + index +
                    " in an list that is only " + list.Count + " element long.");
            }
            object untyped = list[index];
            T retVal;
            try
            {
                retVal = (T)untyped;
            }
            catch (Exception e)
            {
                throw new InvalidCastException("Value at index " + index + ", '" + untyped +
                    "', from the list could not be cast to type " + typeof(T), e);
            }
            return retVal;
        }
    }
}
