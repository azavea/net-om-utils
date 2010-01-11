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
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Avencia.Open.Common.Collections
{
    /// <summary>
    /// This is a class of methods that are workarounds for the fact that 
    /// basic system dictionaries don't have useful enough error messages.
    /// Methods on here should do a normal simple operation and throw an exception
    /// with enough data to debug it.
    /// </summary>
    public static class CheckedDictionary
    {
        /// <summary>
        /// This gets a value from a dictionary.  If the key does not exist
        /// in the dictionary, the exception tells you what it was, and the
        /// first few (up to 20) of the valid keys!
        /// It also helpfully casts it to the right type for you (handy if
        /// you're dealing in dictionaries with objects for values).  If it
        /// cannot cast it, the exception tells you what it tried to cast
        /// to and what the value actually was!
        /// </summary>
        /// <typeparam name="T">Type you expect it to be.</typeparam>
        /// <param name="dict">Dictionary to get a value from.</param>
        /// <param name="key">Key to look up in the dictionary.</param>
        /// <returns>The value from the dictionary, cast to type T.</returns>
        public static T Get<T>(IDictionary<string, object> dict, string key)
        {
            return Get<T, string, object>(dict, key);
        }

        /// <summary>
        /// This gets a value from a dictionary.  If the key does not exist
        /// in the dictionary, the exception tells you what it was, and the
        /// first few (up to 20) of the valid keys!
        /// It also helpfully casts it to the right type for you (handy if
        /// you're dealing in dictionaries with objects for values).  If it
        /// cannot cast it, the exception tells you what it tried to cast
        /// to and what the value actually was!
        /// </summary>
        /// <typeparam name="T">Type you expect it to be.</typeparam>
        /// <typeparam name="A">Type of keys in the dictionary.</typeparam>
        /// <param name="dict">Dictionary to get a value from.</param>
        /// <param name="key">Key to look up in the dictionary.</param>
        /// <returns>The value from the dictionary, cast to type T.</returns>
        public static T Get<T, A>(IDictionary<A, object> dict, A key)
        {
            return Get<T, A, object>(dict, key);
        }
        /// <summary>
        /// This gets a value from a dictionary.  If the key does not exist
        /// in the dictionary, the exception tells you what it was, and the
        /// first few (up to 20) of the valid keys!
        /// It also helpfully casts it to the right type for you (handy if
        /// you're dealing in dictionaries with objects for values).  If it
        /// cannot cast it, the exception tells you what it tried to cast
        /// to and what the value actually was!
        /// </summary>
        /// <typeparam name="T">Type you expect it to be.</typeparam>
        /// <typeparam name="A">Type of keys in the dictionary.</typeparam>
        /// <typeparam name="B">Type of values in the dictionary.</typeparam>
        /// <param name="dict">Dictionary to get a value from.</param>
        /// <param name="key">Key to look up in the dictionary.</param>
        /// <returns>The value from the dictionary, cast to type T.</returns>
        public static T Get<T, A, B>(IDictionary<A, B> dict, A key)
        {
            if (dict == null)
            {
                throw new NullReferenceException("Dictionary was null, cannot get a value at key '" + key + "' from it.");
            }
            if (!dict.ContainsKey(key))
            {
                StringBuilder msg = new StringBuilder();
                msg.Append("Key '").Append(key)
                    .Append("' was not found.  Valid keys: ");
                int count = 0;
                foreach (A validKey in dict.Keys)
                {
                    if (count++ != 0)
                    {
                        msg.Append(", ");
                    }
                    msg.Append("'").Append(validKey).Append("'");
                    if (count > 20)
                    {
                        msg.Append(", and ")
                            .Append(dict.Count - count)
                            .Append(" others.");
                    }
                }
                throw new KeyNotFoundException(msg.ToString());
            }
            object untyped = dict[key];
            T retVal;
            try
            {
                retVal = (T)untyped;
            }
            catch (Exception e)
            {
                throw new InvalidCastException("Value at key '" + key + "', '" + untyped +
                    "', from the dictionary could not be cast to type " + typeof(T), e);
            }
            return retVal;
        }
    }

    /// <summary>
    /// This is an IDictionary implementation that throws USEFUL exceptions
    /// when you ask for things that aren't there etc.
    /// </summary>
    /// <typeparam name="K">Type of the dictionary key.</typeparam>
    /// <typeparam name="V">Type of the dictionary value.</typeparam>
    public class CheckedDictionary<K,V> : IDictionary<K,V>
    {
        private readonly IDictionary<K,V> _realDict;

        /// <summary>
        /// Default constructor, creates an empty dictionary.
        /// </summary>
        public CheckedDictionary()
        {
            _realDict = new Dictionary<K, V>();
        }

        /// <summary>
        /// Creates a dictionary using the given comparer.
        /// </summary>
        /// <param name="comparer">Comparer to use to compare keys in the dictionary.</param>
        public CheckedDictionary(IEqualityComparer<K> comparer)
        {
            _realDict = new Dictionary<K, V>(comparer);
        }

        /// <summary>
        /// Copy constructor, creates a new dictionary with the same contents as 
        /// the other collection.
        /// </summary>
        /// <param name="other">The collection to load all our contents from.</param>
        public CheckedDictionary(IEnumerable<KeyValuePair<K, V>> other) : this()
        {
            AddRange(other);
        }

        /// <summary>
        /// Adds all the items from the other collection to this one.  This may throw
        /// an exception of there are items in the other that violate our constraints
        /// (such as two values with the same key).
        /// </summary>
        /// <param name="other">The collection to add values from.</param>
        public void AddRange(IEnumerable<KeyValuePair<K, V>> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other", "Can't add values from a null collection!");
            }
            foreach (KeyValuePair<K, V> kvp in other)
            {
                Add(kvp);
            }
        }

        /// <summary>
        ///                     Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">
        ///                     The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        ///                 </param>
        /// <exception cref="T:System.NotSupportedException">
        ///                     The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        ///                 </exception>
        public void Add(KeyValuePair<K, V> item)
        {
            // ReSharper disable CompareNonConstrainedGenericWithNull
            // We should be OK comparing the key with null, because we're only
            // doing it so we can throw an exception if it is null.
            if (item.Key == null)
            // ReSharper restore CompareNonConstrainedGenericWithNull
            {
                throw new ArgumentNullException("item", "Cannot add a null key.  Value: '" + item.Value + "'.");
            }
            if (_realDict.ContainsKey(item.Key))
            {
                throw new ArgumentException("This dictionary already contains an item with that key: '" +
                    item.Key + "'.\nOld value: '" + _realDict[item.Key] + "'.\nNew (attempted) value: '" + item.Value + "'.");
            }
            _realDict.Add(item);
        }

        /// <summary>
        ///                     Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <param name="key">
        ///                     The object to use as the key of the element to add.
        ///                 </param>
        /// <param name="value">
        ///                     The object to use as the value of the element to add.
        ///                 </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> is null.
        ///                 </exception>
        /// <exception cref="T:System.ArgumentException">
        ///                     An element with the same key already exists in the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        ///                 </exception>
        /// <exception cref="T:System.NotSupportedException">
        ///                     The <see cref="T:System.Collections.Generic.IDictionary`2" /> is read-only.
        ///                 </exception>
        public void Add(K key, V value)
        {
            // ReSharper disable CompareNonConstrainedGenericWithNull
            // We should be OK comparing the key with null, because we're only
            // doing it so we can throw an exception if it is null.
            if (key == null)
            // ReSharper restore CompareNonConstrainedGenericWithNull
            {
                throw new ArgumentNullException("key", "Cannot add a null key.  Value: '" + value + "'.");
            }
            if (_realDict.ContainsKey(key))
            {
                throw new ArgumentException("This dictionary already contains an item with that key: '" +
                    key + "'.\nOld value: '" + _realDict[key] + "'.\nNew (attempted) value: '" + value + "'.");
            } _realDict.Add(key, value);
        }

        /// <summary>
        ///                     Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.
        /// </returns>
        /// <param name="item">
        ///                     The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        ///                 </param>
        public bool Contains(KeyValuePair<K, V> item)
        {
            // ReSharper disable CompareNonConstrainedGenericWithNull
            // We should be OK comparing the key with null, because we're only
            // doing it so we can throw an exception if it is null.
            if (item.Key == null)
            // ReSharper restore CompareNonConstrainedGenericWithNull
            {
                throw new ArgumentNullException("item", "Cannot check for a null key.  Value: '" + item.Value + "'.");
            }
            return _realDict.Contains(item);
        }

        /// <summary>
        ///                     Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the key; otherwise, false.
        /// </returns>
        /// <param name="key">
        ///                     The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        ///                 </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> is null.
        ///                 </exception>
        public bool ContainsKey(K key)
        {
            // ReSharper disable CompareNonConstrainedGenericWithNull
            // We should be OK comparing the key with null, because we're only
            // doing it so we can throw an exception if it is null.
            if (key == null)
            // ReSharper restore CompareNonConstrainedGenericWithNull
            {
                throw new ArgumentNullException("key", "Cannot check for a null key.");
            }
            return _realDict.ContainsKey(key);
        }

        /// <summary>
        ///                     Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="array">
        ///                     The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.
        ///                 </param>
        /// <param name="arrayIndex">
        ///                     The zero-based index in <paramref name="array" /> at which copying begins.
        ///                 </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="array" /> is null.
        ///                 </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex" /> is less than 0.
        ///                 </exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="array" /> is multidimensional.
        ///                     -or-
        ///                 <paramref name="arrayIndex" /> is equal to or greater than the length of <paramref name="array" />.
        ///                     -or-
        ///                     The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.
        ///                     -or-
        ///                     Type cannot be cast automatically to the type of the destination <paramref name="array" />.
        ///                 </exception>
        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array",
                    "Cannot insert into null array at index " + arrayIndex);
            }
            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException("arrayIndex",
                    "Cannot insert into an array at an index less than zero.  Index: " + arrayIndex);
            }
            if (_realDict.Count > 0)
            {
                if (arrayIndex >= array.Length)
                {
                    throw new ArgumentException("Cannot insert into an array at an index past the end of the array.  Index: " +
                                                arrayIndex, "arrayIndex");
                }
                if (arrayIndex + _realDict.Count > array.Length)
                {
                    throw new ArgumentException("Cannot insert " + _realDict.Count + " elements into a " +
                                                  array.Length + "-element array at index " + arrayIndex +
                                                  " because we would go past the end of the array.",
                                                  "arrayIndex");
                }
                _realDict.CopyTo(array, arrayIndex);
            }
        }

        /// <summary>
        ///                     Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </returns>
        /// <param name="item">
        ///                     The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        ///                 </param>
        /// <exception cref="T:System.NotSupportedException">
        ///                     The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        ///                 </exception>
        public bool Remove(KeyValuePair<K, V> item)
        {
            // ReSharper disable CompareNonConstrainedGenericWithNull
            // We should be OK comparing the key with null, because we're only
            // doing it so we can throw an exception if it is null.
            if (item.Key == null)
            // ReSharper restore CompareNonConstrainedGenericWithNull
            {
                throw new ArgumentNullException("item", "Cannot remove a null key.  Value: '" + item.Value + "'.");
            }
            return _realDict.Remove(item);
        }

        /// <summary>
        ///                     Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <returns>
        /// true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key" /> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </returns>
        /// <param name="key">
        ///                     The key of the element to remove.
        ///                 </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> is null.
        ///                 </exception>
        /// <exception cref="T:System.NotSupportedException">
        ///                     The <see cref="T:System.Collections.Generic.IDictionary`2" /> is read-only.
        ///                 </exception>
        public bool Remove(K key)
        {
            // ReSharper disable CompareNonConstrainedGenericWithNull
            // We should be OK comparing the key with null, because we're only
            // doing it so we can throw an exception if it is null.
            if (key == null)
            // ReSharper restore CompareNonConstrainedGenericWithNull
            {
                throw new ArgumentNullException("key", "Cannot remove for a null key.");
            }
            return _realDict.Remove(key);
        }

        /// <summary>
        ///                     Gets the value associated with the specified key.
        /// </summary>
        /// <returns>
        /// true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key; otherwise, false.
        /// </returns>
        /// <param name="key">
        ///                     The key whose value to get.
        ///                 </param>
        /// <param name="value">
        ///                     When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.
        ///                 </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> is null.
        ///                 </exception>
        public bool TryGetValue(K key, out V value)
        {
            // ReSharper disable CompareNonConstrainedGenericWithNull
            // We should be OK comparing the key with null, because we're only
            // doing it so we can throw an exception if it is null.
            if (key == null)
            // ReSharper restore CompareNonConstrainedGenericWithNull
            {
                throw new ArgumentNullException("key", "Cannot try to get a value for a null key.");
            }
            return _realDict.TryGetValue(key, out value);
        }

        /// <summary>
        ///                     Gets or sets the element with the specified key.
        /// </summary>
        /// <returns>
        ///                     The element with the specified key.
        /// </returns>
        /// <param name="key">
        ///                     The key of the element to get or set.
        ///                 </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key" /> is null.
        ///                 </exception>
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">
        ///                     The property is retrieved and <paramref name="key" /> is not found.
        ///                 </exception>
        /// <exception cref="T:System.NotSupportedException">
        ///                     The property is set and the <see cref="T:System.Collections.Generic.IDictionary`2" /> is read-only.
        ///                 </exception>
        public V this[K key]
        {
            get
            {
                // ReSharper disable CompareNonConstrainedGenericWithNull
                // We should be OK comparing the key with null, because we're only
                // doing it so we can throw an exception if it is null.
                if (key == null)
                // ReSharper restore CompareNonConstrainedGenericWithNull
                {
                    throw new ArgumentNullException("key", "Cannot try to get a value for a null key.");
                }
                if (!_realDict.ContainsKey(key))
                {
                    StringBuilder msg = new StringBuilder();
                    msg.Append("Cannot get key '").Append(key)
                        .Append("', it is not in this dictionary.  Valid keys: ");
                    int count = 0;
                    foreach (K validKey in _realDict.Keys)
                    {
                        if (count++ != 0)
                        {
                            msg.Append(", ");
                        }
                        msg.Append("'").Append(validKey).Append("'");
                        if (count > 20)
                        {
                            msg.Append(", and ")
                                .Append(_realDict.Count - count)
                                .Append(" others.");
                        }
                    }
                    throw new KeyNotFoundException(msg.ToString());
                }
                return _realDict[key];
            }
            set
            {
                // ReSharper disable CompareNonConstrainedGenericWithNull
                // We should be OK comparing the key with null, because we're only
                // doing it so we can throw an exception if it is null.
                if (key == null)
                // ReSharper restore CompareNonConstrainedGenericWithNull
                {
                    throw new ArgumentNullException("key",
                        "Cannot set a value for a null key.  Attempted value: " + value);
                }
                _realDict[key] = value;
            }
        }

        #region Pure Delegation
        /// <summary>
        ///                     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///                     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return _realDict.GetEnumerator();
        }

        /// <summary>
        ///                     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///                     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _realDict.GetEnumerator();
        }

        /// <summary>
        ///                     Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">
        ///                     The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only. 
        ///                 </exception>
        public void Clear()
        {
            _realDict.Clear();
        }

        /// <summary>
        ///                     Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <returns>
        ///                     The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </returns>
        public int Count
        {
            get { return _realDict.Count; }
        }

        /// <summary>
        ///                     Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.
        /// </returns>
        public bool IsReadOnly
        {
            get { return _realDict.IsReadOnly; }
        }

        /// <summary>
        ///                     Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <returns>
        ///                     An <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </returns>
        public ICollection<K> Keys
        {
            get { return _realDict.Keys; }
        }

        /// <summary>
        ///                     Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <returns>
        ///                     An <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </returns>
        public ICollection<V> Values
        {
            get { return _realDict.Values; }
        }

        /// <summary>
        /// Get the realy IDictionary
        /// </summary>
        public IDictionary<K, V> RealDictionary
        {
            get { return _realDict; }
        }
        #endregion
    }
}
