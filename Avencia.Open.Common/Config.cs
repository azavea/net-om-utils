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
using System.Configuration;
using System.IO;
using System.Text;
using System.Xml;
using Avencia.Open.Common.Collections;
using log4net;

namespace Avencia.Open.Common
{
    /// <summary>
    /// This class reads configuration parameters from a standalone config file
    /// identified in the app.config or web.config's "appSettings" section.
    /// </summary>
	public class Config
	{
        /// <summary>
        /// A logger that can be used by child classes as well.
        /// </summary>
        protected static readonly ILog _log = LogManager.GetLogger(
            new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().DeclaringType.Namespace);
        /// <summary>
        /// We keep a cache of Config objects that can be accessed via the GetConfig method.
        /// </summary>
        private static readonly Dictionary<string, Config> _configCache =
            new Dictionary<string, Config>(new CaseInsensitiveStringComparer());

        /// <summary>
        /// The filename (including path) of the config file.
        /// </summary>
        public readonly string ConfigFile;

        /// <summary>
        /// The contents of the config file as an XmlDocument.
        /// </summary>
        public readonly XmlDocument ConfigXmlDoc;

        /// <summary>
        /// The app name passed in when constructing this object.
        /// </summary>
        public readonly string Application;

        /// <summary>
        /// This is a Dictionary of groups of parameters (key/value pairs),
        /// keyed by component (component = one section in the config file).
        /// The groups are dictionaries to facilitate fast lookups by
        /// key.
        /// </summary>
        protected readonly IDictionary<string, IDictionary<string, string>> _paramsByComponent;

        /// <summary>
        /// This is a Dictionary of groups of parameters (key/value pairs),
        /// keyed by component (component = one section in the config file).
        /// The groups are lists, for times when the order of the parameters
        /// matters.
        /// </summary>
        protected readonly IDictionary<string, IList<KeyValuePair<string, string>>> _orderedParamsByComponent;

        /// <summary>
        /// This is a Dictionary of the XML contents of each component/section,
        /// keyed by component (component = one section in the config file).
        /// The values are XML strings.
        /// </summary>
        protected readonly IDictionary<string, string> _xmlByComponent;

        /// <summary>
        /// This allows you to avoid reading the same config file over and over again.
        /// Since Config objects are read-only, we can read the file once and hand the
        /// same object out over and over without worrying about threading issues.
        /// </summary>
        /// <param name="appName">Identifies which config file we want.</param>
        /// <returns>The config object representing that config file.</returns>
        public static Config GetConfig(string appName)
        {
            if (!StringHelper.IsNonBlank(appName))
            {
                throw new ArgumentNullException("appName", "Cannot construct a config with a blank/null app name.");
            }
            Config retVal = null;
            lock (_configCache)
            {
                if (_configCache.ContainsKey(appName))
                {
                    retVal = _configCache[appName];
                }
            }
            if (retVal == null)
            {
                retVal = new Config(appName);
                lock (_configCache)
                {
                    _configCache[appName] = retVal;
                }
            }
            return retVal;
        }

        /// <summary>
        /// Constructs a config class given the "appName", or the key to look up
        /// in the app/web.config's "appSettings" section.  The value for that key
        /// is the path to the config file we're interested in.
        /// </summary>
        /// <param name="appName">Identifies which config file we want.</param>
		public Config(string appName)
            : this(null, appName, null) {}

        /// <summary>
        /// Constructs a config class given a specific config file to load.
        /// </summary>
        /// <param name="configFileName">The file name of the configuration file to load.</param>
        /// <param name="appName">Since the config file is specified, this app name is just
        ///                       used for identification in log/error messages.</param>
		public Config(string configFileName, string appName)
            : this(configFileName, appName, null) { }

        /// <summary>
        /// Construct a config directly from an XML document rather than from a file.
        /// </summary>
        /// <param name="appName">App name (I.E. config file name or whatever), used for logging.</param>
        /// <param name="configXml">The XML containing all the config information.</param>
        public Config(string appName, XmlDocument configXml)
            : this(null, appName, configXml) { }

        /// <summary>
        /// Construct a config class.  The app name is used for logging, and to get
        /// the config file name if the file name was not specified and the XML was
        /// not passed directly.
        /// 
        /// If you provide the XML, the app name is just used for logging, and the filename
        /// is stored but not used for anything (and may be blank).
        /// </summary>
        /// <param name="configFileName">The file name of the configuration file to load.</param>
        /// <param name="appName">App name (I.E. config file name or whatever), used for logging.</param>
        /// <param name="configXml">The XML containing all the config information.</param>
        public Config(string configFileName, string appName, XmlDocument configXml)
            : this(configFileName, appName, configXml, false) { }

        /// <summary>
        /// The default behavior of Config is to throw an exception if the config file
        /// does not exist.  This constructor allows a child class to override that,
        /// in which case a missing file will be treated as though it were empty
        /// (no values loaded, but no exception thrown).
        /// </summary>
        /// <param name="configFileName">The file name of the configuration file to load.</param>
        /// <param name="appName">App name (I.E. config file name or whatever), used for logging.</param>
        /// <param name="configXml">The XML containing all the config information.</param>
        /// <param name="treatMissingFileAsEmpty">If true, a missing config file will not cause an exception.</param>
        protected Config(string configFileName, string appName, XmlDocument configXml,
            bool treatMissingFileAsEmpty)
            : this(configFileName, appName, configXml, treatMissingFileAsEmpty,
            new CheckedDictionary<string, IDictionary<string, string>>(new CaseInsensitiveStringComparer()),
            new CheckedDictionary<string, IList<KeyValuePair<string, string>>>(new CaseInsensitiveStringComparer()),
            new CheckedDictionary<string, string>(new CaseInsensitiveStringComparer())) { }

        /// <summary>
        /// This signature lets a child class provide more complicated or specific types of
        /// collections.
        /// 
        /// Reminder: You probably want to use CaseInsensitiveStringComparers in your
        /// dictionaries!
        /// </summary>
        /// <param name="configFileName">The file name of the configuration file to load.</param>
        /// <param name="appName">App name (I.E. config file name or whatever), used for logging.</param>
        /// <param name="configXml">The XML containing all the config information.</param>
        /// <param name="treatMissingFileAsEmpty">If true, a missing config file will not cause an exception.</param>
        /// <param name="paramsByComponent">The dictionary that will hold the parameters keyed by component.</param>
        /// <param name="orderedParamsByComponent">The dictionary that will hold the parameters in order from the file, keyed by component.</param>
        /// <param name="xmlByComponent">The dictionary that will hold XML chunks from the file, keyed by component.</param>
        protected Config(string configFileName, string appName, XmlDocument configXml,
            bool treatMissingFileAsEmpty,
            IDictionary<string, IDictionary<string, string>> paramsByComponent,
            IDictionary<string, IList<KeyValuePair<string, string>>> orderedParamsByComponent,
            IDictionary<string, string> xmlByComponent)
        {
            // First populate the attributes.
            _paramsByComponent = paramsByComponent;
            _orderedParamsByComponent = orderedParamsByComponent;
            _xmlByComponent = xmlByComponent;

            if (appName == null)
            {
                throw new ArgumentNullException(appName, "AppName cannot be null, config filename: " + configFileName + ".");
            }
            if (appName.Length == 0)
            {
                throw new ArgumentException("AppName cannot be blank, config filename: " + configFileName + ".");
            }
            Application = appName;

            if (configXml == null)
            {
                // If the XML wasn't passed directly, load it from the filename instead.
                if (configFileName != null)
                {
                    if (configFileName.Length == 0)
                    {
                        throw new ArgumentException("No XML was provided and config filename was blank, appName: " +
                            appName + ".");
                    }
                }
                else
                {
                    try
                    {
                        configFileName = ConfigurationManager.AppSettings[appName];
                    }
                    catch (Exception ex)
                    {
                        ReThrowException("No XML was provided and we were unable to retrieve config file name for app", new Object[] { appName }, ex);
                    }
                    if (configFileName == null)
                    {
                        throw new LoggingException("No XML was provided and we got a null config file name for app '" + appName + "'.");
                    }
                    if (configFileName.Length == 0)
                    {
                        throw new LoggingException("No XML was provided and we got a blank config file name for app '" + appName + "'.");
                    }
                }
				// Replace any environment variables in the config file name.
                configFileName = ReplaceEnvironmentVariables(configFileName, false);

                // Start with a blank doc, we'll either load a file into it or not depending
                // on if the file exists.
                ConfigXmlDoc = new XmlDocument();
                if (!File.Exists(configFileName))
                {
                    if (!treatMissingFileAsEmpty)
                    {
                        throw new LoggingException("No XML was provided and the specified config file (" +
                            configFileName + ") does not exist, app: '" + appName + "'.");
                    }
                }
                else
                {
                    try
                    {
                        ConfigXmlDoc.Load(configFileName);
                    }
                    catch (Exception ex)
                    {
                        ReThrowException("Unable to load config file.",
                                         new object[] { Application, configFileName }, ex);
                    }
                }
            }
            else
            {
                ConfigXmlDoc = configXml;
            }

            ConfigFile = configFileName;

            try
            {
                ParseConfigXml();
            }
            catch (Exception ex)
            {
                ReThrowException("Unable to parse config XML: " + ConfigXmlDoc.OuterXml,
                    new object[] { Application, ConfigFile }, ex);
            } 
        }

        /// <summary>
        /// This is a ugly hack at the moment.  This allows child classes to
        /// override the internal type of collection we use.  This will be
        /// removed when we refactor the architecture to have an abstract
        /// base class so we can have a "writeable" version of Config that does not
        /// conflict with the implementation of this "readonly" Config.
        /// </summary>
        /// <returns>A dictionary to use to hold parameters we've read from the
        ///          config file.</returns>
        protected virtual IDictionary<string, string> MakeParameterCollection()
        {
            return new CheckedDictionary<string, string>(new CaseInsensitiveStringComparer());
        }

        /// <summary>
        /// Reads the XML and populates the various attributes based on it
        /// (lists of params, dictionaries of params, etc).
        /// </summary>
        private void ParseConfigXml()
        {
            try
            {
                XmlNodeList nodeList = ConfigXmlDoc.GetElementsByTagName("component");
                foreach (XmlNode node in nodeList)
                {
                    string componentName = node.Attributes["name"].Value;

                    // Save the entire XML section for the GetConfigXml method
                    _xmlByComponent[componentName] = node.OuterXml;

                    if (_paramsByComponent.ContainsKey(componentName))
                    {
                        throw new LoggingException("Component '" + componentName +
                                                   "' is defined twice in this config file!");
                    }
                    IDictionary<string, string> componentParams = MakeParameterCollection();
                    _paramsByComponent[componentName] = componentParams;
                    IList<KeyValuePair<string, string>> orderedComponentParams =
                        new List<KeyValuePair<string, string>>();
                    _orderedParamsByComponent[componentName] = orderedComponentParams;
                    // Now save all the individual parameters.
                    foreach (XmlNode paramNode in node.ChildNodes)
                    {
                        // Ignore any child nodes that aren't parameters.
                        if (StringHelper.SafeEquals("parameter", paramNode.Name))
                        {
                            string paramName = paramNode.Attributes["name"].Value;
                            if (componentParams.ContainsKey(paramName))
                            {
                                throw new LoggingException("Component '" + componentName +
                                                           "' has parameter '" + paramName + "' defined twice!");
                            }
                            XmlAttribute valueAttr = paramNode.Attributes["value"];
                            string paramValue;
                            if (valueAttr != null)
                            {
                                paramValue = valueAttr.Value;
                            }
                            else
                            {
                                XmlNodeList paramKids = paramNode.ChildNodes;
                                if (paramKids.Count == 1)
                                {
                                    XmlNode paramKid = paramKids[0];
                                    if (paramKid.NodeType == XmlNodeType.Text || paramKid.NodeType == XmlNodeType.CDATA)
                                    {
                                        paramValue = paramKid.Value;
                                    }
                                    else
                                    {
                                        throw new LoggingException("Component '" + componentName +
                                                                   "', parameter '" + paramName +
                                                                   "', has invalid nested XML ('" + paramNode.InnerText +
                                                                   "').  Only text is supported inside a parameter tag.");
                                    }
                                }
                                else
                                {
                                    throw new LoggingException("Component '" + componentName +
                                                               "', parameter '" + paramName +
                                                               "', has invalid nested XML ('" + paramNode.InnerText +
                                                               "').  Only text is supported inside a parameter tag.");
                                }
                            }
                            componentParams[paramName] = paramValue;
                            orderedComponentParams.Add(new KeyValuePair<string, string>(paramName, paramValue));
                        }
                    }
                }
            } 
            catch (Exception e)
            {
                ReThrowException("Error parsing config XML.",
                    new object[] { Application, ConfigFile }, e);
            }
        }

        /// <summary>
        /// If there are any environment variables (in the form %VAR%) in the
        /// input string, replaces them with the values from the environment.
        /// 
        /// This method can be tolerant or intolerant of errors, so:
        /// "abc" -> "abc"
        /// "abc%windir%abc" -> "abcC:\WINDOWSabc"
        /// "abc%abc" -> exception (intolerant) or "abc%abc" (tolerant)
        /// "abc%nosuchvar%abc" -> exception (intolerant) or "abc%nosuchvar%abc" (tolerant)
        /// "abc%windir%abc%" -> exception (intolerant) or "abcC:\WINDOWSabc%" (tolerant)
        /// 
        /// Calling this method with "false" for tolerant matches the previous behavior.
        /// 
        /// Methods like File.Exists do not parse environment variables, so this
        /// method should be called before attempting to use filenames etc.
        /// </summary>
        /// <param name="val">Input string to search for environment vars.</param>
        /// <param name="tolerant">If true, this method logs warnings.  If false, it
        ///                        throws exceptions.</param>
        /// <returns>The string with variables replaced with values, or the
        ///          unmodified string if there are no valid variables in it.
        ///          (Note: input of null means output of null).</returns>
        public static string ReplaceEnvironmentVariables(string val, bool tolerant)
        {
            string retVal = null;

            if (val != null)
            {
                // Seperate all pieces that might be env vars.
                string[] parts = val.Split('%');
                if (parts.Length == 1) {
                    // No % in the string, couldn't substitute any vars.
                    retVal = val;
                }
                else if (parts.Length == 2)
                {
                    string message = "There was one % in the string, couldn't substitute any vars: '" +
                            val + "'.";
                    if (tolerant)
                    {
                        retVal = val;
                    }
                    else
                    {
                        throw new LoggingException(message);
                    }
                }
                else
                {
                    StringBuilder returnString = new StringBuilder();

                    // There's no % before the first part.
                    bool leadingPerc = false;

                    // For each part (except the last, which can't be), 
                    // see if it's an environment variable.
                    for (int index = 0; index < (parts.Length - 1); index++)
                    {
                        if (leadingPerc)
                        {
                            string envValue = Environment.GetEnvironmentVariable(
                                parts[index].ToUpper());
                            if (string.IsNullOrEmpty(envValue))
                            {
                                string message = "Value '" + val + "' has substring '%" + parts[index] + "%'" +
                                    " which is not an environment variable.";
                                if (tolerant)
                                {
                                    _log.Warn(message);
                                    // Put it back in the output string unchanged.
                                    returnString.Append("%"); // % was stripped out by the 'split' call.
                                    returnString.Append(parts[index]);
                                    // This isn't a variable, so it isn't using the next % either.
                                    // So leave leadingPerc set to 'true',
                                }
                                else
                                {
                                    throw new LoggingException(message);
                                }
                            }
                            else
                            {
                                returnString.Append(envValue);
                                // We just used the following %.
                                leadingPerc = false;
                            }
                        }
                        else
                        {
                            // No preceding %, this part isn't a variable.
                            returnString.Append(parts[index]);
                            // Since this isn't a variable, it didn't use the following %.
                            leadingPerc = true;
                        }
                    }

                    // Now add the last part, with a preceding % if there is one.
                    if (leadingPerc)
                    {
                        string message = "Unmatched % near the end of the input string: " + val;
                        if (tolerant)
                        {
                            _log.Warn(message);
                            returnString.Append("%");
                        }
                        else
                        {
                            throw new LoggingException(message);
                        }
                    }
                    returnString.Append(parts[parts.Length - 1]);

                    retVal = returnString.ToString();
                }
            }

            return retVal;
        }

        /// <summary>
        /// Returns the config parameter for the given component.  Throws an exception
        /// if there is no such parameter.  If you want to know if the parameter exists,
        /// call ParameterExists(...).
        /// </summary>
        /// <param name="component">The component or section of the config file, used to
        ///                         locate the parameter.</param>
        /// <param name="parameter">The name of the config parameter.</param>
        /// <returns>The string value for the parameter.  Will never be null, if no
        ///          value could be found this method will throw.  Could be ""
        ///          since "" is a valid thing to have in a config file.</returns>
		public string GetParameter(string component, string parameter)
		{
            if (component == null)
            {
                throw new ArgumentNullException("component", "Component cannot be null.  Parameter was '" +
                    parameter + "'.");
            }
            if (parameter == null)
            {
                throw new ArgumentNullException("parameter", "Parameter cannot be null.  Component was '" +
                    component + "'.");
            }
            try
            {
                return _paramsByComponent[component][parameter];
            }
            catch (Exception ex)
            {
                ReThrowException("Unable to read parameter.",
                    new object[] { ConfigFile, component, parameter }, ex);
                // that throws, but the compiler wants a return statement.
                return "never gets here";
            }
        }
        /// <summary>
        /// Similar to the regular GetParameter method, except it will substitute 
        /// environment variables in the values if present.
        /// </summary>
        /// <param name="component">The component or section of the config file, used to
        ///                         locate the parameter.</param>
        /// <param name="parameter">The name of the config parameter.</param>
        /// <param name="tolerant">If true, this method logs warnings for unmatched environment
        ///                        variables.  If false, it throws exceptions.</param>
        /// <returns>The string value for the parameter.  Will never be null, if no
        ///          value could be found this method will throw.  Could be ""
        ///          since "" is a valid thing to have in a config file.</returns>
        public string GetParameterWithSubstitution(string component, string parameter, bool tolerant)
        {
            string value = GetParameter(component, parameter);
            return ReplaceEnvironmentVariables(value, tolerant);
        }

        /// <summary>
        /// Similar to GetParameter, except rather than throwing an exception if a parameter
        /// doesn't exist, returns the default value.
        /// </summary>
        /// <param name="component">The component or section of the config file, used to
        ///                         locate the parameter.</param>
        /// <param name="parameter">The name of the config parameter.</param>
        /// <param name="def">Value to return if the parameter doesn't exist.</param>
        /// <returns>The parameter from the config, or the default.</returns>
        public string GetParameterWithDefault(string component, string parameter, string def)
        {
            if (ParameterExists(component, parameter))
            {
                return GetParameter(component, parameter);
            }
            return def;
        }

        /// <summary>
        /// Method to check if a parameter exists, prior to calling GetParameter (which
        /// throws exceptions if you request an invalid parameter).
        /// </summary>
        /// <param name="component">The component or section of the config file, used to
        ///                         locate the parameter.</param>
        /// <param name="parameter">The name of the config parameter.</param>
        /// <returns>True if the component has a section in the config file, and if that
        ///          section has the parameter.  False otherwise.</returns>
		public bool ParameterExists(string component, string parameter)
		{
            if (component == null)
            {
                throw new ArgumentNullException("component", "Component cannot be null.  Parameter was '" +
                    parameter + "'.");
            }
            if (parameter == null)
            {
                throw new ArgumentNullException("parameter", "Parameter cannot be null.  Component was '" +
                    component + "'.");
            }
            try
			{
                if (_paramsByComponent.ContainsKey(component))
                {
                    if (_paramsByComponent[component].ContainsKey(parameter))
                    {
                        return true;
                    }
                }
			    return false;
			}
			catch(Exception ex)
			{
				ReThrowException("Unable to check if parameter exists.",
                    new object[] { ConfigFile, component, parameter }, ex);
				// that throws, but the compiler wants a return statement.
				return false;
			}
		}

        /// <summary>
        /// Method to check if a config section exists for a component, prior to calling
        /// GetConfigXml or GetParametersAsHashTable (which throw exceptions if you request
        /// an invalid component name).
        /// </summary>
        /// <param name="component">The component or section of the config file, used to
        ///                         locate the parameter.</param>
        /// <returns>True if the component has a section in the config file.  False otherwise.</returns>
        public bool ComponentExists(string component)
        {
            if (component == null)
            {
                throw new ArgumentNullException("component", "Component cannot be null.");
            }
            try
            {
                if (_paramsByComponent.ContainsKey(component))
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                ReThrowException("Unable to check if component exists.",
                    new object[] { ConfigFile, component }, ex);
                // that throws, but the compiler wants a return statement.
                return false;
            }
        }

        /// <summary>
        /// Gets you the XML section for the component, allowing you to do any special
        /// parsing that may be necessary.
        /// </summary>
        /// <param name="component">The component or section of the config file, used to
        ///                         locate the parameter.</param>
        /// <returns>The XML section (inclusive of the "component" tag) for the
        /// given component.</returns>
		public virtual string GetConfigXml(string component)
		{
            if (component == null)
            {
                throw new ArgumentNullException("component", "Component cannot be null.");
            }
			try
			{
			    return _xmlByComponent[component];
			}
			catch(Exception ex)
			{
				ReThrowException("Error while getting config XML for component.",
                    new object[] { ConfigFile, component }, ex);
                // that throws, but the compiler wants a return statement.
                return null;
            }
		}

        /// <summary>
        /// Gets you a Dictionary of all the parameters for the component.
        /// </summary>
        /// <param name="component">The component or section of the config file, used to
        ///                         locate the parameter.</param>
        /// <returns>A Dictionary, with string values keyed (case insentively) by string parameter names.</returns>
        public Dictionary<string, string> GetParametersAsDictionary(string component)
        {
            if (component == null)
            {
                throw new ArgumentNullException("component", "Component cannot be null.");
            }
            try
            {
                // Convert to upper for case insensitivity.
                return new Dictionary<string, string>(_paramsByComponent[component],
                    new CaseInsensitiveStringComparer());
            }
            catch (Exception ex)
            {
                ReThrowException("Error while getting params as Dictionary.",
                    new object[] { ConfigFile, component }, ex);
                // that throws, but the compiler wants a return statement.
                return null;
            }
        }

        /// <summary>
        /// Gets you a list of all the parameters for the component as key-value-pairs.
        /// This preserves the order of parameters from the config file.
        /// </summary>
        /// <param name="component">The component or section of the config file, used to
        ///                         locate the parameter.</param>
        /// <returns>A list of key-value-pairs, with string values keyed by string parameter names.</returns>
        public virtual IList<KeyValuePair<string, string>> GetParametersAsList(string component)
        {
            if (component == null)
            {
                throw new ArgumentNullException("component", "Component cannot be null.");
            }
            try
            {
                // Convert to upper for case insensitivity.
                return new List<KeyValuePair<string, string>>(_orderedParamsByComponent[component]);
            }
            catch (Exception ex)
            {
                ReThrowException("Error while getting params as IList.",
                    new object[] { ConfigFile, component }, ex);
                // that throws, but the compiler wants a return statement.
                return null;
            }
        }

        /// <summary>
        /// All the checks for null are because there was an issue where something about
        /// a caught exception was null, which caused the error handling code to bomb.
        /// Since error handling code is the worst place to bomb (you lose the original
        /// exception), to be safe we manually convert null values into "null" strings.
        /// </summary>
        protected static void ReThrowException(string msg, object[] paramList, Exception orig)
        {
            if (msg == null)
            {
                msg = "null message";
            }
            if ((paramList != null) && (paramList.Length > 0))
            {
                msg += " (" + ((paramList[0] == null) ? "null" : paramList[0].ToString());
                for (int x = 1; x < paramList.Length; x++)
                {
                    msg += ", " + ((paramList[x] == null) ? "null" : paramList[x].ToString());
                }
                msg += ")\n";
            }
            if (orig == null)
            {
                msg += "Exception was null.";
            }
            else
            {
                msg += "Exception Message: " + (orig.Message ?? "null") + "\n";
                msg += "Exception StackTrace: " + (orig.StackTrace ?? "null");
            }
            throw new LoggingException(msg);
        }

        ///<summary>
        /// Two Configs with the same config file and appname are Equal.
        ///</summary>
        ///
        ///<returns>
        ///true if the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />; otherwise, false.
        ///</returns>
        ///
        ///<param name="obj">The <see cref="T:System.Object" /> to compare with the current <see cref="T:System.Object" />. </param>
        ///<exception cref="T:System.NullReferenceException">The <paramref name="obj" /> parameter is null.</exception><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (obj is Config)
            {
                return StringHelper.SafeEquals(ConfigFile, ((Config)obj).ConfigFile) &&
                    StringHelper.SafeEquals(Application, ((Config)obj).Application);
            }
            return false;
        }

        ///<summary>
        ///Hash code, based on the config file name and app name.
        ///</summary>
        ///
        ///<returns>
        ///A hash code for the current <see cref="T:System.Object" />.
        ///</returns>
        public override int GetHashCode()
        {
            return (ConfigFile + "_" + Application).GetHashCode();
        }

        ///<summary>
        ///Returns the config file name and app name./>.
        ///</summary>
        ///
        ///<returns>
        ///A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
        ///</returns>
        public override string ToString()
        {
            return ConfigFile + "_" + Application;
        }
	}
}
