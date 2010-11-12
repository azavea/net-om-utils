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
using System.Collections.Generic;

namespace Azavea.Open.Common.Caching
{
    /// <summary>
    /// A cache that basically acts like a dictionary, but with a max duration.
    /// NOTE: This does NOT clear values that are expired, it just won't return them.
    /// It is intended for cases when you are reusing keys.
    /// If you are using an infinite set of keys, you will run out of memory eventually.
    /// For example:
    /// If you're using filenames as keys and you have a finite set of files, you're fine.
    /// If you're using timestamps as keys and you always use "now", you'll run out of
    /// memory eventually because the old ones don't get cleared.
    /// 
    /// This class uses locking to ensure thread-safety.
    /// </summary>
    /// <typeparam name="K">Type of the key.</typeparam>
    /// <typeparam name="T">Type of the data.</typeparam>
    public class TimedCache<K,T>
    {
        /// <summary>
        /// Stores the actual data (with a timestamp for when it was added
        /// to the cache) for each key.
        /// </summary>
        protected readonly Dictionary<K, TimestampedData<T>> _innerCache =
            new Dictionary<K, TimestampedData<T>>();

        /// <summary>
        /// The amount of time data for a given key is guaranteed to
        /// remain valid for after a call to ContainsKey(key) returns true.
        /// </summary>
        protected readonly TimeSpan _getGuaranteeTime = new TimeSpan(0, 0, 1);
        /// <summary>
        /// How long should the cache keep things before discarding them.
        /// </summary>
        protected readonly TimeSpan _cacheRealDuration;
        /// <summary>
        /// How long ContainsKey(key) will return true after data for
        /// that key is added to the cache.  (_cacheRealDuration - _getGuaranteeTime).
        /// </summary>
        protected readonly TimeSpan _cacheCheckDuration;

        /// <summary>
        /// Construct the cache.
        /// </summary>
        /// <param name="cacheDuration">How long should the cache keep things before discarding them.</param>
        public TimedCache(TimeSpan cacheDuration)
        {
            if (cacheDuration < new TimeSpan(0, 0, 5))
            {
                throw new ArgumentException(
                    "This cache was not designed to function with extremely short cache durations, such as " + cacheDuration,
                    "cacheDuration");
            }
            _cacheRealDuration = cacheDuration;
            _cacheCheckDuration = cacheDuration - _getGuaranteeTime;
        }

        /// <summary>
        /// Returns true only if we have a cached data item for this key and
        /// that data has not expired (and is not about to immediately).
        /// 
        /// NOTE: A true response only guarauntees GetData(key) will work for 1 second after this call.
        /// It is possible to call this, get true, wait 2 seconds,
        /// call GetData, and get a KeyNotFoundException.  Thems the breaks
        /// when using a cache with data that expires.
        /// </summary>
        /// <param name="key">Key that identifies the data you want.</param>
        /// <returns>True if GetData(key) will return a valid data item.</returns>
        public bool ContainsKey(K key)
        {
            TimestampedData<T> datum = null;
            // Remember to lock the inner cache for the minimum possible amount of time.
            lock (_innerCache)
            {
                if (_innerCache.ContainsKey(key))
                {
                    datum = _innerCache[key];
                }
            }
            if (datum != null)
            {
                // We use _cacheCheckDuration, which is shorter, because we
                // want to be sure if we return true it stays true for long
                // enough for them to get the data if they want it.
                if (datum.Time > (DateTime.Now - _cacheCheckDuration))
                {
                    // Still young enough.
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Returns the data for the given cache key.  Throws KeyNotFoundException
        /// if we don't have that key or if the value is expired.
        /// </summary>
        /// <param name="key">Key that identifies the data you want.</param>
        /// <returns>The object that was cached.</returns>
        public T Get(K key)
        {
            TimestampedData<T> datum;
            // Remember to lock the inner cache for the minimum possible amount of time.
            lock (_innerCache)
            {
                datum = _innerCache[key];
            }
            // We use _cacheRealDuration, which is longer, because we
            // want to be sure only to throw an exception if it's really expired.
            if (datum.Time > (DateTime.Now - _cacheRealDuration))
            {
                // Still young enough.
                return datum.Data;
            }
            throw new KeyNotFoundException("Data at key " + key + " was expired.");
        }
        /// <summary>
        /// Attempts to return data for the given cache key.  Returns False
        /// if we don't have that key or if the value is expired, True otherwise.
        /// </summary>
        /// <param name="key">Key that identifies the data you want.</param>
        /// <param name="value">Will update this with the object that was cached
        /// if the key was found and the value was not expired.</param>
        /// <returns>True if the key was found and the value was not expired, False otherwise.</returns>
        public bool TryGet(K key, ref T value)
        {
            TimestampedData<T> datum = null;
            // Remember to lock the inner cache for the minimum possible amount of time.
            lock (_innerCache)
            {
                if (_innerCache.ContainsKey(key))
                {
                    datum = _innerCache[key];
                }
            }
            // We use _cacheRealDuration, which is longer, because we
            // want to be sure only to throw an exception if it's really expired.
            if (datum != null)
            {
                if (datum.Time > (DateTime.Now - _cacheRealDuration))
                {
                    // Still young enough.
                    value = datum.Data;
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Saves the given data into the cache with the given key.
        /// </summary>
        /// <param name="key">Key to save the data under.</param>
        /// <param name="data">Data to save.</param>
        public void Set(K key, T data)
        {
            TimestampedData<T> newDatum = new TimestampedData<T>(data);
            lock (_innerCache)
            {
                // Since sets occur much more rarely (in theory anyway) than gets, we'll purge
                // expired cache items from the cache on sets.  This will hopefully help
                // prevent the cache from growing out of control over time (another reason
                // to do it here, since a get never increases the size of the cache).
                DateTime cutoff = DateTime.Now - _cacheRealDuration;
                List<K> deleteUs = new List<K>();
                foreach (KeyValuePair<K, TimestampedData<T>> kvp in _innerCache)
                {
                    if (kvp.Value.Time < cutoff)
                    {
                        deleteUs.Add(kvp.Key);
                    }
                }
                // Can't delete these in the previous loop because you'll get a
                // "collection modified" exception from the iterator.
                foreach (K oldKey in deleteUs)
                {
                    _innerCache.Remove(oldKey);
                }

                // Now add the new one.
                _innerCache[key] = newDatum;
            }
        }

        /// <summary>
        /// Extends the lifetime of cached data by resetting the timestamp
        /// for a key's value to the current time.
        /// </summary>
        /// <param name="key">Key to reset.</param>
        public void Reset(K key)
        {
            TimestampedData<T> datum;
            // Remember to lock the inner cache for the minimum possible amount of time.
            lock (_innerCache)
            {
                datum = _innerCache[key];

                // We use _cacheRealDuration, which is longer, because we
                // want to be sure only to throw an exception if it's really expired.
                if (datum.Time > (DateTime.Now - _cacheRealDuration))
                {
                    // Still young enough.
                    _innerCache[key] = new TimestampedData<T>(datum.Data);
                }
                else
                {
                    // expired
                    throw new KeyNotFoundException("Data at key " + key + " was expired.");
                }
            }
        }
        /// <summary>
        /// Extends the lifetime of cached data by resetting the timestamp
        /// for a key's value to the current time.
        /// </summary>
        /// <param name="key">Key to reset.</param>
        /// <returns>False if the key was not found or the value was expired, True otherwise.</returns>
        public bool TryReset(K key)
        {
            TimestampedData<T> datum;
            // Remember to lock the inner cache for the minimum possible amount of time.
            lock (_innerCache)
            {
                if (_innerCache.ContainsKey(key))
                {
                    datum = _innerCache[key];

                    // We use _cacheRealDuration, which is longer, because we
                    // want to be sure only to throw an exception if it's really expired.
                    if (datum.Time > (DateTime.Now - _cacheRealDuration))
                    {
                        // Still young enough.
                        _innerCache[key] = new TimestampedData<T>(datum.Data);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
