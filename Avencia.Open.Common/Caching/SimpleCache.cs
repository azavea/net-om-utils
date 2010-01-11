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

namespace Avencia.Open.Common.Caching
{
    /// <summary>
    /// This class caches objects for you, that you might not want to create and
    /// delete all the time.  The cache tolerates leaks, meaning it will construct
    /// new objects as necessary to satisfy requests.
    /// 
    /// It has a max size (number of objects cached) that defaults to 1000.  This
    /// prevents someone from accidentally returning too many objects and the cache
    /// consuming a rediculous amount of memory.
    /// 
    /// One assumption in this class is that the objects are either stateless, state
    /// is unimportant, or state is reset by the client.
    /// 
    /// This class is thread-safe.
    /// </summary>
    /// <typeparam name="T">Type of object to cache.  The only requirement is that the
    ///                     object have a parameterless constructor.</typeparam>
    public class SimpleCache<T> where T : class, new()
    {
        /// <summary>
        /// What the max size of the cache will be if not otherwise specified.
        /// </summary>
        protected const int _defaultMaxSize = 1000;
        /// <summary>
        /// The actual collection holding the cached objects.
        /// </summary>
        protected readonly Stack<T> _cache;
        /// <summary>
        /// The actual max size.
        /// </summary>
        protected readonly int _maxSize;

        /// <summary>
        /// Construct the cache with a default max size.
        /// </summary>
        public SimpleCache()
        {
            _maxSize = _defaultMaxSize;
            _cache = new Stack<T>(_defaultMaxSize);
        }

        /// <summary>
        /// Construct the cache with a custom max size.
        /// </summary>
        /// <param name="maxSize">The maximum number of objects to keep in the cache at one time.</param>
        public SimpleCache(int maxSize)
        {
            _maxSize = maxSize;
            _cache = new Stack<T>(maxSize);
        }

        /// <summary>
        /// Gets an object from the cache.  If there are currently no objects actually
        /// stored in the cache, this will return a brand new one.
        /// </summary>
        /// <returns>An object, hopefully from the cache.</returns>
        public virtual T Get()
        {
            T retVal = null;
            lock (_cache)
            {
                if (_cache.Count > 0)
                {
                    retVal = _cache.Pop();
                }
            }
            if (retVal == null)
            {
                retVal = MakeNewOne();
            }
            return retVal;
        }

        /// <summary>
        /// Returns an object to the cache.  It's up to the client to remember to call this.
        /// This will discard the object if the cache already has the maximum number of
        /// objects.
        /// </summary>
        /// <param name="obj">The object that is done, hopefully has had its state returned
        ///                   to some variation on "blank" or "unset", and will be put back
        ///                   in the cache.</param>
        public virtual void Return(T obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj",
                    "Cannot return a null object to the collection!");
            }
            lock (_cache)
            {
                if (_cache.Count < _maxSize)
                {
                    _cache.Push(obj);
                }
            }
        }

        /// <summary>
        /// By default this just returns a new object, however there may be some pre-
        /// or post-construction steps that need to be done, so it may be overridden in
        /// a child class.
        /// </summary>
        /// <returns>A new object, all ready to go.</returns>
        protected virtual T MakeNewOne()
        {
            return new T();
        }
    }
}
