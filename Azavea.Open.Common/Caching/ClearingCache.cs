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
using System.Data;
using System.Reflection;

namespace Azavea.Open.Common.Caching
{
    /// <summary>
    /// This class extends the SimpleCache and uses reflection (once, during construction)
    /// to try and find a method "void Clear()" on the type it is instantiated with.
    /// 
    /// The Clear method will be called whenever an object is returned to the cache, relieving
    /// the client of the responsibility of reseting the state of the object before
    /// returning it.
    /// 
    /// In particular, this is for caching Collection classes (I.E. ArrayLists,
    /// Dictionaries, etc) which all have a Clear method.  But it will work for anything
    /// else with such a method as well.
    /// </summary>
    /// <typeparam name="T">Type to cache.  Unfortunately we have to use reflection, so
    ///                     the compiler will allow types without a "Clear()" method, but
    ///                     it better be there or we'll throw an exception at run time.</typeparam>
    public class ClearingCache<T> : SimpleCache<T> where T : class, new()
    {
        /// <summary>
        /// The info on the method to call before putting an object back in the cache.
        /// </summary>
        protected readonly MethodInfo _clearMethod;
        /// <summary>
        /// The method has zero params, so we create the empty param list one time and reuse it.
        /// </summary>
        protected readonly object[] _clearParams = new object[0];

        /// <summary>
        /// Construct the cache with a default max size.
        /// </summary>
        public ClearingCache()
        {
            _clearMethod = GetClearMethod();
        }

        /// <summary>
        /// Construct the cache with a custom max size.
        /// </summary>
        /// <param name="maxSize">The maximum number of objects to keep in the cache at one time.</param>
        public ClearingCache(int maxSize) : base(maxSize)
        {
            _clearMethod = GetClearMethod();
        }

        /// <summary>
        /// Uses reflection to get the MethodInfo for the method called "Clear" and save
        /// it as a class attribute.  Throws if the type doesn't have a Clear method.
        /// </summary>
        protected MethodInfo GetClearMethod()
        {
            MethodInfo retVal = typeof(T).GetMethod("Clear", new Type[0]);
            if (retVal == null)
            {
                throw new ConstraintException("A generic ClearingCache can only be created for a type with a Clear() method.  Type " +
                                              typeof(T) + " does not have one.");
            }
            return retVal;
        }

        /// <summary>
        /// Overridden to call Clear() on the object before putting it back in the cache.
        /// </summary>
        /// <param name="obj">The object that is done, it will have its Clear() method
        ///                   called, and will be put back in the cache.</param>
        public override void Return(T obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj",
                                                "Cannot return a null object to the collection!");
            }
            _clearMethod.Invoke(obj, _clearParams);
            base.Return(obj);
        }
    }
}
