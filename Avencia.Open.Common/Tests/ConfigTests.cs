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
using System.Xml;
using NUnit.Framework;

namespace Avencia.Open.Common.Tests
{
    ///<exclude/>
    [TestFixture]
    public class ConfigTests
    {
        private readonly string VALID_FILE = "..\\..\\Tests\\ConfigTest.config";
        private readonly string VALID_APPNAME = "ConfigTests";
        ///<exclude/>
        [Test]
        public void TestAppNameConstructor()
        {
            new Config(VALID_APPNAME);
        }
        ///<exclude/>
        [Test]
        [Ignore("Performance tests aren't stable enough to be automated unit tests.")]
        public void TestGetConfig()
        {
            DateTime start = DateTime.Now;
            Config.GetConfig(VALID_APPNAME);
            DateTime end = DateTime.Now;
            TimeSpan firstLoadTime = end - start;
            start = DateTime.Now;
            Config.GetConfig(VALID_APPNAME);
            end = DateTime.Now;
            TimeSpan secondLoadTime = end - start;
            Assert.Greater(firstLoadTime, secondLoadTime, "Should be faster the second time.");
        }
        ///<exclude/>
        [Test]
        public void TestAppNameConstructorInvalidName()
        {
            bool threw = false;
            try
            {
                new Config("NotThere");
            }
            catch (Exception e)
            {
                threw = true;
                Assert.IsTrue(e.Message.Contains("NotThere"), "Exception didn't contain the appName");
            }
            Assert.IsTrue(threw, "Failed to throw an exception when given an invalid appName.");
        }
        ///<exclude/>
        [Test]
        public void TestAppNameConstructorNameWithNoFile()
        {
            bool threw = false;
            try
            {
                new Config("NoFileName");
            }
            catch (Exception e)
            {
                threw = true;
                Assert.IsTrue(e.Message.Contains("NoFileName"), "Exception didn't contain the appName");
            }
            Assert.IsTrue(threw, "Failed to throw an exception when getting an empty string from the config.");
        }
        ///<exclude/>
        [Test]
        public void TestAppNameConstructorNullName()
        {
            bool threw = false;
            try
            {
                new Config(null);
            }
            catch (ArgumentNullException)
            {
                threw = true;
            }
            Assert.IsTrue(threw, "Failed to throw an exception when given a null appName.");
        }
        ///<exclude/>
        [Test]
        public void TestAppNameConstructorBlankName()
        {
            bool threw = false;
            try
            {
                new Config("");
            }
            catch (ArgumentException)
            {
                threw = true;
            }
            Assert.IsTrue(threw, "Failed to throw an exception when given a blank appName.");
        }

        ///<exclude/>
        [Test]
        public void TestFileNameConstructor()
        {
            new Config(VALID_FILE, "DoesntMatter");
        }
        ///<exclude/>
        [Test]
        public void TestFileNameConstructorInvalidFile()
        {
            bool threw = false;
            try
            {
                new Config("NotThere.config", VALID_APPNAME);
            }
            catch (Exception e)
            {
                threw = true;
                Assert.IsTrue(e.Message.Contains(VALID_APPNAME), "Exception didn't contain the appName");
                Assert.IsTrue(e.Message.Contains("NotThere.config"), "Exception didn't contain the file name");
            }
            if (!threw)
            {
                Assert.Fail("Failed to throw an exception when given an invalid filename.");
            }
        }
        ///<exclude/>
        [Test]
        public void TestFileNameConstructorNullAppName()
        {
            bool threw = false;
            try
            {
                new Config(VALID_FILE, (string)null);
            }
            catch (ArgumentNullException e)
            {
                threw = true;
                Assert.IsTrue(e.Message.Contains(VALID_FILE), "Exception didn't contain the file");
            }
            Assert.IsTrue(threw, "Failed to throw an exception when given a null appName.");
        }
        ///<exclude/>
        [Test]
        public void TestFileNameConstructorBlankAppName()
        {
            bool threw = false;
            try
            {
                new Config(VALID_FILE, "");
            }
            catch (ArgumentException e)
            {
                threw = true;
                Assert.IsTrue(e.Message.Contains(VALID_FILE), "Exception didn't contain the file");
            }
            Assert.IsTrue(threw, "Failed to throw an exception when given a blank appName.");
        }
        ///<exclude/>
        [Test]
        public void TestFileNameConstructorNullFileName()
        {
            bool threw = false;
            try
            {
                new Config(null, "appNameBob");
            }
            catch (Exception e)
            {
                threw = true;
                Assert.IsTrue(e.Message.Contains("appNameBob"), "Exception didn't contain the appname");
            }
            Assert.IsTrue(threw, "Failed to throw an exception when given a null file name.");
        }
        ///<exclude/>
        [Test]
        public void TestFileNameConstructorBlankFileName()
        {
            bool threw = false;
            try
            {
                new Config("", "appname");
            }
            catch (ArgumentException e)
            {
                threw = true;
                Assert.IsTrue(e.Message.Contains("appname"), "Exception didn't contain the appname");
            }
            Assert.IsTrue(threw, "Failed to throw an exception when given a null file name.");
        }

        ///<exclude/>
        [Test]
        public void TestXmlDocumentConstructor()
        {
            const string xml =
                @"<components>
                    <component name='Comp1'>
		                <parameter name='Param1' value='1_1'/>
		                <parameter name='Param2' value='1_2'/>
		                <parameter name='Param3' value='1_3'/>
                    </component>
                </components>";
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            Config cfg = new Config("SomeApp", doc);
            Assert.IsTrue(cfg.ComponentExists("Comp1"));
            Assert.AreEqual(3, cfg.GetParametersAsList("Comp1").Count);
            Assert.AreEqual("1_1", cfg.GetParameter("Comp1", "Param1"));
        }

        ///<exclude/>
        [Test]
        public void TestReplaceEnvUnchanged()
        {
            Assert.AreEqual("Test", Config.ReplaceEnvironmentVariables("Test", true),
                "Changed string with no variables.");
            Assert.IsNull(Config.ReplaceEnvironmentVariables(null, true),
                "Null input, non-null output.");
            Assert.AreEqual("Testing %NOT_A_REAL_VAR% Testing",
                Config.ReplaceEnvironmentVariables("Testing %NOT_A_REAL_VAR% Testing", true),
                "Changed string with fake variable.");
        }
        ///<exclude/>
        [Test]
        public void TestReplaceEnvChanged()
        {
            // Our two test variables.
            string os = Environment.GetEnvironmentVariable("OS");
            // NOTE: This may fail in mono?
            string windir = Environment.GetEnvironmentVariable("windir");

            Assert.AreEqual(windir, Config.ReplaceEnvironmentVariables("%windir%", true),
                "Failed to substitute just a var.");
            Assert.AreEqual("Testing " + windir + " Testing",
                Config.ReplaceEnvironmentVariables("Testing %windir% Testing", true),
                "Failed to substitute a var and text.");
            Assert.AreEqual("Testing " + windir + os + " Testing",
                Config.ReplaceEnvironmentVariables("Testing %windir%%os% Testing", true),
                "Failed to substitute two vars.");
            Assert.AreEqual("Testing " + windir + "os% Testing",
                Config.ReplaceEnvironmentVariables("Testing %windir%os% Testing", true),
                "Failed to substitute first var with missing % for second var.");
        }
        ///<exclude/>
        [Test]
        public void TestReplaceEnvAdvanced()
        {
            // Our two test variables.
            string os = Environment.GetEnvironmentVariable("OS");
            // NOTE: This may fail in mono?
            string windir = Environment.GetEnvironmentVariable("windir");

            Assert.AreEqual("%fake" + windir, Config.ReplaceEnvironmentVariables("%fake%windir%", true),
                "Failed to substitute a var with a leading extraneous %.");
            Assert.AreEqual("start%fake" + windir + "%%%" + os + "end",
                Config.ReplaceEnvironmentVariables("start%fake%windir%%%%%os%end", true),
                "Failed to vars with many extraneous %s.");
        }
        ///<exclude/>
        [Test]
        public void TestReplaceEnvIntolerantExceptions()
        {
            bool threw = false;
            try
            {
                Config.ReplaceEnvironmentVariables("%fake%windir%", false);

            }

            catch (Exception e)
            {
                threw = true;
                Assert.IsTrue(e.Message.Contains("%fake%windir%"),
                    "Exception didn't contain the value");
            }
            Assert.IsTrue(threw, "Didn't throw with leading extraneous %.");
            threw = false;
            try
            {
                Config.ReplaceEnvironmentVariables("start%fake%windir%%%%%os%end", false);

            }

            catch (Exception e)
            {
                threw = true;
                Assert.IsTrue(e.Message.Contains("start%fake%windir%%%%%os%end"),
                    "Exception didn't contain the value");
            }
            Assert.IsTrue(threw, "Didn't throw with many extraneous %s.");
            threw = false;
            try
            {
                Config.ReplaceEnvironmentVariables("Testing %windir%os% Testing", false);

            }

            catch (Exception e)
            {
                threw = true;
                Assert.IsTrue(e.Message.Contains("Testing %windir%os% Testing"),
                    "Exception didn't contain the value");
            }
            Assert.IsTrue(threw, "Didn't throw with missing % for second var.");
            threw = false;
            try
            {
                Config.ReplaceEnvironmentVariables("Testing %NOT_A_REAL_VAR% Testing", false);

            }

            catch (Exception e)
            {
                threw = true;
                Assert.IsTrue(e.Message.Contains("Testing %NOT_A_REAL_VAR% Testing"),
                    "Exception didn't contain the value");
            }
            Assert.IsTrue(threw, "Didn't throw with fake variable.");
        }
        ///<exclude/>
        [Test]
        public void TestReplaceEnvVarIntolerantUnchanged()
        {
            Assert.AreEqual("Test", Config.ReplaceEnvironmentVariables("Test", false),
                "Changed string with no variables.");
            Assert.IsNull(Config.ReplaceEnvironmentVariables(null, false),
                "Null input, non-null output.");
        }
        ///<exclude/>
        [Test]
        public void TestReplaceEnvVarIntolerantChanged()
        {
            string os = Environment.GetEnvironmentVariable("OS");
            string windir = Environment.GetEnvironmentVariable("windir");
            Assert.AreEqual(windir, Config.ReplaceEnvironmentVariables("%windir%", false),
                "Failed to substitute just a var.");
            Assert.AreEqual("Testing " + windir + " Testing",
                Config.ReplaceEnvironmentVariables("Testing %windir% Testing", false),
                "Failed to substitute a var and text.");
            Assert.AreEqual("Testing " + windir + os + " Testing",
                Config.ReplaceEnvironmentVariables("Testing %windir%%os% Testing", false),
                "Failed to substitute two vars.");
        }
        ///<exclude/>
        [Test]
        public void TestGetParamValid()
        {
            Config cfg = new Config(VALID_APPNAME);
            for (int comp = 1; comp < 3; comp++)
            {
                for (int parm = 1; parm < 4; parm++)
                {
                    Assert.AreEqual(comp + "_" + parm,
                        cfg.GetParameter("Comp" + comp, "Param" + parm),
                        "Wrong value for comp " + comp + " param " + parm);
                }
            }
        }
        ///<exclude/>
        [Test]
        public void TestCaseInsensitivity()
        {
            // Try all uppercase, should work fine.
            new Config(VALID_APPNAME.ToUpper());
            // Try all lowercase, should also work fine.
            Config cfg = new Config(VALID_APPNAME.ToLower());
            // Try getting parameters using different cases.
            for (int comp = 1; comp < 3; comp++)
            {
                for (int parm = 1; parm < 4; parm++)
                {
                    Assert.AreEqual(comp + "_" + parm,
                        cfg.GetParameter("COMP" + comp, "PARAM" + parm),
                        "Wrong value for uppercase comp " + comp + " param " + parm);
                    Assert.AreEqual(comp + "_" + parm,
                        cfg.GetParameter("comp" + comp, "param" + parm),
                        "Wrong value for lowercase comp " + comp + " param " + parm);
                    Assert.AreEqual(comp + "_" + parm,
                        cfg.GetParameter("cOmP" + comp, "pArAm" + parm),
                        "Wrong value for mixed case comp " + comp + " param " + parm);
                }
            }
        }
        ///<exclude/>
        [Test]
        public void TestEmptyValue() {
            Config cfg = new Config(VALID_APPNAME);
            Assert.AreEqual("", cfg.GetParameter("Comp2", "OnlyIn2"),
                "Wrong value for empty string only in 2");
        }
        ///<exclude/>
        [Test]
        public void TestGetParamNulls()
        {
            Config cfg = new Config(VALID_APPNAME);
            try
            {
                cfg.GetParameter("Comp1", null);
                Assert.Fail("Didn't throw with null param.");
            }
            catch (ArgumentNullException e)
            {
                Assert.IsTrue(e.Message.Contains("Comp1"), "Message didn't contain component name.");
            }
            try
            {
                cfg.GetParameter(null, "Param1");
                Assert.Fail("Didn't throw with null component.");
            }
            catch (ArgumentNullException e)
            {
                Assert.IsTrue(e.Message.Contains("Param1"), "Message didn't contain param name.");
            }
            try
            {
                cfg.GetParameter(null, null);
                Assert.Fail("Didn't throw with two nulls.");
            }
            catch (ArgumentNullException)
            {
                // good
            }
        }
        ///<exclude/>
        [Test]
        public void TestGetParamInvalid()
        {
            Config cfg = new Config(VALID_APPNAME);
            GetParamInvalidHelper(cfg, "Comp1", "NoParam", "fake param name");
            GetParamInvalidHelper(cfg, "Comp1", "OnlyIn2", "param name from other component");
            GetParamInvalidHelper(cfg, "Comp1", "", "empty param name");
            GetParamInvalidHelper(cfg, "Comp3", "Param1", "fake component name");
            GetParamInvalidHelper(cfg, "", "Param1", "empty param name");
        }
        /// <summary>
        /// Call GetParameter with invalid params, catch the exception and check the message.
        /// </summary>
        private static void GetParamInvalidHelper(Config cfg, string comp, string parm, string testDesc)
        {
            try
            {
                cfg.GetParameter(comp, parm);
                Assert.Fail("Test '" + testDesc + " didn't throw with comp '" + comp + "' param '" +
                    parm + "'.");
            }
            catch (Exception e)
            {
                Assert.IsTrue(e.Message.Contains(comp), "Test '" + testDesc +
                    " threw an exception without comp '" + comp +
                    "' in the description (param was'" + parm + "'.");
                Assert.IsTrue(e.Message.Contains(parm), "Test '" + testDesc +
                    " threw an exception without param '" + parm +
                    "' in the description (comp was'" + comp + "'.");
            }
        }
        ///<exclude/>
        [Test]
        public void TestGetParamNonAttr()
        {
            Config cfg = new Config(VALID_APPNAME);
            
            Assert.AreEqual("\"Non-Attribute Value\"", cfg.GetParameter("NonAttr", "AParam"));
        }
    }
}
