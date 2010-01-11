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
using Avencia.Open.Common.Collections;
using NUnit.Framework;

namespace Avencia.Open.Common.Tests
{
    /// <exclude/>
    [TestFixture]
    public class CollectionTests
    {
        private readonly IDictionary<string, string> _testOld = new Dictionary<string, string>();
        private readonly IDictionary<string, string> _testChecked = new CheckedDictionary<string, string>();

        /// <exclude/>
        [Test]
        public void TestDictionaryAdd()
        {
            AssertDictionaryException(delegate(IDictionary<string, string> dict)
            {
                dict.Add(new KeyValuePair<string, string>(null, "two"));
            }, new[] { "null", "key", "two" });
            AssertDictionaryException(delegate(IDictionary<string, string> dict)
            {
                // ReSharper disable AssignNullToNotNullAttribute
                // This is sort of the point.
                dict.Add(null, "two");
                // ReSharper restore AssignNullToNotNullAttribute
            }, new[] { "null", "key", "two" });
            AssertDictionaryException(delegate(IDictionary<string, string> dict)
            {
                dict.Clear();
                dict.Add(new KeyValuePair<string, string>("one", "two"));
                dict.Add(new KeyValuePair<string, string>("one", "three"));
            }, new[] { "one", "two", "three" });
            AssertDictionaryException(delegate(IDictionary<string, string> dict)
            {
                dict.Clear();
                dict.Add("one", "two");
                dict.Add("one", "three");
            }, new[] { "one", "two", "three" });
        }

        /// <exclude/>
        [Test]
        public void TestDictionaryContains()
        {
            AssertDictionaryException(delegate(IDictionary<string, string> dict)
            {
                dict.Contains(new KeyValuePair<string, string>(null, "two"));
            }, new[] { "null", "key", "two" });
            AssertDictionaryException(delegate(IDictionary<string, string> dict)
            {
                // ReSharper disable AssignNullToNotNullAttribute
                // This is sort of the point.
                dict.ContainsKey(null);
                // ReSharper restore AssignNullToNotNullAttribute
            }, new[] { "null", "key" });
            AssertDictionaryNoException(delegate(IDictionary<string, string> dict)
            {
                dict.Contains(new KeyValuePair<string, string>("blah", "two"));
            });
            AssertDictionaryNoException(delegate(IDictionary<string, string> dict)
            {
                dict.ContainsKey("blah");
            });
        }

        /// <exclude/>
        [Test]
        public void TestDictionaryCopyTo()
        {
            AssertDictionaryException(delegate(IDictionary<string, string> dict)
            {
                dict.CopyTo(null, 10);
            }, new[] { "10", "null", "array" });
            AssertDictionaryException(delegate(IDictionary<string, string> dict)
            {
                KeyValuePair<string,string>[] arr = new KeyValuePair<string, string>[10];
                dict.CopyTo(arr, -1);
            }, new[] { "less", "zero", "array", "-1" });
            AssertDictionaryNoException(delegate(IDictionary<string, string> dict)
            {
                KeyValuePair<string, string>[] arr = new KeyValuePair<string, string>[10];
                dict.Clear();
                dict.CopyTo(arr, 10);
            });
            AssertDictionaryException(delegate(IDictionary<string, string> dict)
            {
                KeyValuePair<string, string>[] arr = new KeyValuePair<string, string>[10];
                dict["one"] = "two";
                dict.CopyTo(arr, 10);
            }, new[] { "10", "end", "array" });
            AssertDictionaryException(delegate(IDictionary<string, string> dict)
            {
                dict.Clear();
                dict["one"] = "two";
                dict["two"] = "three";
                dict["three"] = "four";
                dict["four"] = "five";
                KeyValuePair<string, string>[] arr = new KeyValuePair<string, string>[10];
                dict.CopyTo(arr, 7);
            }, new[] { "10", "7", "end", "array", "4" });
        }

        /// <exclude/>
        [Test]
        public void TestDictionaryRemove()
        {
            AssertDictionaryException(delegate(IDictionary<string, string> dict)
            {
                dict.Remove(new KeyValuePair<string, string>(null, "two"));
            }, new[] { "null", "key", "two" });
            AssertDictionaryException(delegate(IDictionary<string, string> dict)
            {
                // ReSharper disable AssignNullToNotNullAttribute
                // This is sort of the point.
                dict.Remove(null);
                // ReSharper restore AssignNullToNotNullAttribute
            }, new[] { "null", "key" });
            AssertDictionaryNoException(delegate(IDictionary<string, string> dict)
            {
                dict.Clear();
                dict.Remove(new KeyValuePair<string, string>("blah", "two"));
            });
            AssertDictionaryNoException(delegate(IDictionary<string, string> dict)
            {
                dict.Clear();
                dict.Remove("blah");
            });
        }

        /// <exclude/>
        [Test]
        public void TestDictionaryTryGetValue()
        {
            AssertDictionaryException(delegate(IDictionary<string, string> dict)
            {
                string value;
                // ReSharper disable AssignNullToNotNullAttribute
                // This is sort of the point.
                dict.TryGetValue(null, out value);
                // ReSharper restore AssignNullToNotNullAttribute
            }, new[] { "null", "key" });
            AssertDictionaryNoException(delegate(IDictionary<string, string> dict)
            {
                dict.Clear();
                string value;
                dict.TryGetValue("notthere", out value);
                Assert.IsNull(value, "Value should be set to null now.");
            });
        }

        /// <exclude/>
        [Test]
        public void TestDictionaryBracketGet()
        {
            AssertDictionaryException(delegate(IDictionary<string, string> dict)
            {
                // ReSharper disable AssignNullToNotNullAttribute
                // This is sort of the point.
#pragma warning disable 168
                string x = dict[null];
#pragma warning restore 168
                // ReSharper restore AssignNullToNotNullAttribute
            }, new[] { "null", "key", "get" });
            AssertDictionaryException(delegate(IDictionary<string, string> dict)
            {
                dict.Clear();
                dict["two"] = "a";
                dict["three"] = "b";
#pragma warning disable 168
                string x = dict["one"];
#pragma warning restore 168
            }, new[] { "one", "key", "get", "two", "three" });
        }

        /// <exclude/>
        [Test]
        public void TestDictionaryBracketSet()
        {
            AssertDictionaryException(delegate(IDictionary<string, string> dict)
            {
                // ReSharper disable AssignNullToNotNullAttribute
                // This is sort of the point.
                dict[null] = "one";
                // ReSharper restore AssignNullToNotNullAttribute
            }, new[] { "null", "key", "set", "one"});
            AssertDictionaryNoException(delegate(IDictionary<string, string> dict)
            {
                dict.Clear();
                string value;
                dict.TryGetValue("notthere", out value);
                Assert.IsNull(value, "Value should be set to null now.");
            });
        }

        private void AssertDictionaryNoException(DictionaryCaller callMe)
        {
            try
            {
                callMe.Invoke(_testOld);
            }
            catch (Exception e)
            {
                Assert.Fail("Original dictionary threw an exception: " + e);
            }
            try
            {
                callMe.Invoke(_testChecked);
            }
            catch (Exception e)
            {
                Assert.Fail("Checked dictionary threw an exception: " + e);
            }
        }
        
        private void AssertDictionaryException(DictionaryCaller callMe, IEnumerable<string> messageComponents)
        {
            Exception oldEx = null;
            try
            {
                callMe.Invoke(_testOld);
            }
            catch(Exception e)
            {
                oldEx = e;
            }
            Assert.IsNotNull(oldEx, "Original dictionary didn't throw exception.");
            Exception newEx = null;
            try
            {
                callMe.Invoke(_testChecked);
            }
            catch (Exception e)
            {
                newEx = e;
            }
            Assert.IsNotNull(newEx, "Checked dictionary didn't throw exception.");
            Assert.AreEqual(oldEx.GetType(), newEx.GetType(), "Threw wrong type of exception: " + newEx);
            foreach (string component in messageComponents)
            {
                Assert.Greater(newEx.Message.IndexOf(component), -1, "Message did not contain '" +
                    component + "'.  Exception: " + newEx);
            }
            Console.WriteLine("Threw correct exception: " + newEx);
        }

        private delegate void DictionaryCaller(IDictionary<string, string> dict);
    }
}
