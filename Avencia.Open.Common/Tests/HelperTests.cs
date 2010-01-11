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
using NUnit.Framework;

namespace Avencia.Open.Common.Tests
{
    /// <exclude/>
    [TestFixture]
    public class HelperTests
    {
        /// <exclude/>
        [Test]
        public void TestValidEmails()
        {
            AssertGoodEmail("j_adams@avencia.com");
            AssertGoodEmail("j.adams@avencia.com");
            AssertGoodEmail("j.a.d.a.m.s@avencia.com");
            AssertGoodEmail("j.adams@www.lots.of.dots.avencia.com");
            AssertGoodEmail("jadamsy@avencia.com");
            AssertGoodEmail("j.@avencia.com");
        }
        /// <exclude/>
        [Test]
        public void TestInvalidEmails()
        {
            AssertBadEmail("j@adams@avencia.com");
            AssertBadEmail("j$adams@avencia.com");
            AssertBadEmail("@avencia.com");
            AssertBadEmail("jadams@");
            AssertBadEmail("j@adams@.com");
            AssertBadEmail("jadams@avencia_underscore.com");
            AssertBadEmail(null);
        }
        internal static void AssertGoodEmail(string email)
        {
            Assert.IsTrue(StringHelper.IsEmailAddress(email), "Should have said " + email + " was good.");
        }
        private static void AssertBadEmail(string email)
        {
            Assert.IsFalse(StringHelper.IsEmailAddress(email), "Should have said " + email + " was bad.");
        }

        /// <exclude/>
        [Test]
        public void TestValidTelephone()
        {
            AssertGoodTelephone("800-GOT-MILK", "(800) GOT-MILK");
            AssertGoodTelephone("2159526263", "(215) 952-6263");
            AssertGoodTelephone("+33-1-4623-6060 ", "+33 (1) 4623 6060");//France
            AssertGoodTelephone("+33 1 46236060 ", "+33 (1) 4623 6060");//France
            AssertGoodTelephone("+593-2-254-0531", "+593 (2) 254 0531"); //Ecuador
            AssertGoodTelephone("+55 21 2221-5359", "+55 (21) 2221 5359");//Brazil
            AssertGoodTelephone("+86-10-6554-1618", "+86 (10) 6554 1618");//China
            AssertGoodTelephone("+27 11 238 6300", "+27 (11) 238 6300");//South Africa

        }
        /// <exclude/>
        [Test]
        public void TestInvalidTelephone()
        {
            AssertBadTelephone("Not A Number");
            AssertBadTelephone("9 9 9");
            AssertBadTelephone("+33 05 10 20 30 40");
            AssertBadTelephone("234-1234");
            
        }
        private static void AssertGoodTelephone(string telephone, string expected)
        {
            Assert.IsTrue(StringHelper.FormatTelephone(telephone).Equals(expected), "Should have said " + telephone + " was good.");
        }
        private static void AssertBadTelephone(string telephone)
        {
            Assert.IsTrue(StringHelper.FormatTelephone(telephone).Equals(""), "Should have said " + telephone + " was bad.");
        }


        /// <exclude/>
        [Test]
        public void TestTitleCase()
        {
            Assert.AreEqual("Here's A Test", StringHelper.FormatTitleCase("HERE'S a tesT"));
            Assert.AreEqual("A", StringHelper.FormatTitleCase("a"));
            Assert.AreEqual("A", StringHelper.FormatTitleCase("A"));
            Assert.AreEqual("Reallyreallyreallyreallyreallyreallyreallyreallyreallyreallyreallylong", StringHelper.FormatTitleCase("ReallyREALLYReallyReallyReallyReallyReallyReallyReallyReallyReallyLong"));
            Assert.AreEqual("A   Bee C Da E", StringHelper.FormatTitleCase("a   bee c da E"));
            Assert.AreEqual("", StringHelper.FormatTitleCase(""));
            try
            {
                StringHelper.FormatTitleCase(null);
                Assert.Fail("Should have thrown an ArgumentNullException when passed null.");
            }
            catch (ArgumentNullException)
            {
                // Correct.
            }
        }

        /// <exclude/>
        [Test]
        public void TestNonEmpty()
        {
            AssertNonEmpty("a");
            AssertNonEmpty("    a");
            AssertNonEmpty("blah\nblah");
            AssertNonEmpty("1");
            AssertNonEmpty("         @    ");
            AssertNonEmpty("\ta\t");
            AssertEmpty("\t\t");
            AssertEmpty("   \t   \t");
            AssertEmpty(" ");
            AssertEmpty(null);
            AssertEmpty("\n");
        }
        private static void AssertNonEmpty(string input)
        {
            Assert.IsTrue(StringHelper.IsNonBlank(input), "String '" + input + "' is not empty!");
        }
        private static void AssertEmpty(string input)
        {
            Assert.IsFalse(StringHelper.IsNonBlank(input), "String '" + input + "' is empty!");
        }

        /// <exclude/>
        [Test]
        public void TestAlpha()
        {
            AssertAlpha("jeff");
            AssertAlpha("jeffWroteThisTest");
            AssertAlpha("BlahBlahBlah");
            AssertAlpha("ReallyReallyReallyReallyReallyReallyReallyReallyReallyReallyReallyReallyReallyReallyLong");
            AssertNonAlpha("jeff wrote this test");
            AssertNonAlpha("jeff.");
            AssertNonAlpha("");
            AssertNonAlpha(null);
        }
        private static void AssertNonAlpha(string input)
        {
            Assert.IsFalse(StringHelper.IsAlpha(input), "String '" + input + "' is not alpha!");
        }
        private static void AssertAlpha(string input)
        {
            Assert.IsTrue(StringHelper.IsAlpha(input), "String '" + input + "' is alpha!");
        }

        /// <exclude/>
        [Test]
        public void TestUsername()
        {
            AssertUserName("jeff");
            AssertUserName("jeffWroteThisTest");
            AssertUserName("BlahBlahBlah");
            AssertUserName("ReallyReallyReallyReallyReallyReallyReallyReallyReallyReallyReallyReallyReallyReallyLong");
            AssertUserName("jeff@avencia.com");
            AssertUserName("jeff_wrote_this_test");
            AssertNonUsername("jeff wrote this test");
            AssertNonUsername("jeff$");
            AssertNonUsername("l337$p34|<");
            AssertNonUsername("");
            AssertNonUsername(null);
        }
        private static void AssertNonUsername(string input)
        {
            Assert.IsFalse(StringHelper.IsValidUsername(input), "String '" + input + "' is not alpha!");
        }
        private static void AssertUserName(string input)
        {
            Assert.IsTrue(StringHelper.IsValidUsername(input), "String '" + input + "' is alpha!");
        }

        /// <exclude/>
        [Test]
        public void TestInteger()
        {
            AssertInteger("1");
            AssertInteger("283947974");
            AssertInteger("-2387");
            AssertInteger("0");
            AssertNonInteger("Jeff");
            AssertNonInteger("$#(@(&#(^)%^*#)$* OTSLMDKV ZP(# WFUoinkvs j8392 uba52%Q(%&*(#*)!@ OP$KJQR;lszv987u3#");
            AssertNonInteger("negative five");
            AssertNonInteger("-1.5");
            AssertNonInteger("");
            AssertNonInteger(null);
        }
        private static void AssertNonInteger(string input)
        {
            Assert.IsFalse(StringHelper.IsInteger(input), "String '" + input + "' is not int!");
        }
        private static void AssertInteger(string input)
        {
            Assert.IsTrue(StringHelper.IsInteger(input), "String '" + input + "' is int!");
        }

        /// <exclude/>
        [Test]
        public void TestIntRange()
        {
            try
            {
                StringHelper.IsIntWithinRange("1", 2, 1);
                Assert.Fail("Didn't throw when given an invalid low and high.");
            }
            catch (ArgumentException)
            {
                // Correct;
            }
            AssertIntRange("1", 1, 1);
            AssertIntRange("283947974", 0, Int32.MaxValue);
            AssertIntRange("-2387", -3000, -2000);
            AssertIntRange("0", -1, 0);
            AssertNonIntRange("Jeff", 1, 1);
            AssertNonIntRange("$#(@(&#(^)%^*#)$* OTSLMDKV ZP(# WFUoinkvs j8392 uba52%Q(%&*(#*)!@ OP$KJQR;lszv987u3#", 1, 1);
            AssertNonIntRange("negative five", 1, 1);
            AssertNonIntRange("-1.5", 1, 1);
            AssertNonIntRange("", 1, 1);
            AssertNonIntRange(null, 1, 1);
            AssertNonIntRange("2", 1, 1);
            AssertNonIntRange("-2", 1, 1);
            AssertNonIntRange("23478", 1, 1);
            AssertNonIntRange("100", -99, 99);
        }
        private static void AssertNonIntRange(string input, int low, int high)
        {
            Assert.IsFalse(StringHelper.IsIntWithinRange(input, low, high), "String '" + input +
                "' is not int between " + low + " and " + high);
        }
        private static void AssertIntRange(string input, int low, int high)
        {
            Assert.IsTrue(StringHelper.IsIntWithinRange(input, low, high), "String '" + input + 
                "' is int between " + low + " and " + high);
        }

        /// <exclude/>
        [Test]
        public void TestDouble()
        {
            AssertDouble("1");
            AssertDouble("28394.7974");
            AssertDouble("-238.7");
            AssertDouble("0");
            AssertDouble("-1.5");
            AssertNonDouble("Jeff");
            AssertNonDouble("$#(@(&#(^)%^*#)$* OTSLMDKV ZP(# WFUoinkvs j8392 uba52%Q(%&*(#*)!@ OP$KJQR;lszv987u3#");
            AssertNonDouble("negative five");
            AssertNonDouble("1.1.1");
            AssertNonDouble("");
            AssertNonDouble(null);
        }
        private static void AssertNonDouble(string input)
        {
            Assert.IsFalse(StringHelper.IsDouble(input), "String '" + input + "' is not double!");
        }
        private static void AssertDouble(string input)
        {
            Assert.IsTrue(StringHelper.IsDouble(input), "String '" + input + "' is double!");
        }

        /// <exclude/>
        [Test]
        public void TestDateTime()
        {
            AssertDateTime("09/09/09");
            AssertDateTime("15:36 05/05/05");
            AssertDateTime("29 MAR 2007");
            // This format isn't OK.
            AssertNonDateTime("11:00 AM January 5th, 2007");
            AssertNonDateTime("Jeff");
            AssertNonDateTime("$#(@(&#(^)%^*#)$* OTSLMDKV ZP(# WFUoinkvs j8392 uba52%Q(%&*(#*)!@ OP$KJQR;lszv987u3#");
            AssertNonDateTime("01/36/03");
            AssertNonDateTime("-1.5");
            AssertNonDateTime("");
            AssertNonDateTime(null);
        }
        private static void AssertNonDateTime(string input)
        {
            Assert.IsFalse(StringHelper.IsDateTime(input), "String '" + input + "' is not DateTime!");
        }
        private static void AssertDateTime(string input)
        {
            Assert.IsTrue(StringHelper.IsDateTime(input), "String '" + input + "' is DateTime!");
        }
    }
}
