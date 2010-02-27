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
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

namespace Azavea.Open.Common
{
    /// <summary>
    /// This class consists of all the various static string manipulation helper functions we've
    /// written over the years.  Primarily they have to do with validating that the string (user input
    /// typically) is actually an int, an email address, is blank, etc, but also anything else
    /// that is a common string manipulation operation.
    /// </summary>
    public static class StringHelper
    {
        private const string EmailRegex = @"^([a-zA-Z0-9_\-\.\+]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";

        /// <summary>
        /// This is a regex expression for use in StringHelper.FormatTelephone(string). The expression is public so that it 
        /// can also be used in a RegularExpressionValidator in aspx pages. It is USA centric but supports both USA and International numbers.
        /// USA numbers may contain letters (eg 800-GOT-MILK) and may have optional extension designated by 'x' or 'ext'.
        /// International telphone numbers must follow the pattern: +(country code) (zone code) (3 or 4 numbers) (3 or 4 numbers).
        /// Valid separators are ./- or space. AreaCode or ZoneCode may be surrounded by (parens).
        /// The match if any is grouped into either International or USA match group, which is in turn grouped into respective named groups.
        /// International group contains named groups CountryCode ZoneCode Number1 Number2. eg +XX XX XXXX XXXX.
        /// USA group contains named groups AreaCode Exchange Number. eg XXX XXX XXXX
        /// </summary>
        public const string TelephoneRegex = @"(?<International>[\+](?<CountryCode>\d{1,3})[\s.\-/]*\({0,1}(?<ZoneCode>\d{1,3})\){0,1}[\s.\-/]*(?<Number1>\d{3,4})[\s.\-/]*(?<Number2>\d{3,4}))|(?<USA>\({0,1}(?<AreaCode>[2-9]\d{2})\){0,1}[\s.\-/]*(?<Exchange>[2-9]\d{2}|\w{3})[\s.\-/]*(?<Number>\w{4})[\s.\-/]?[\s.\-/]*(?<Extension>(x|ext)\.*\s*\d+)?)";

        /// <summary>
        /// Validates an email address using a regex expression.
        /// </summary>
        /// <param name="input">A string containing an email to validate.  Can be null (will return false).</param>
        /// <returns>true if the string is an email address</returns>
        public static bool IsEmailAddress(string input)
        {
            bool isValid;
            if (String.IsNullOrEmpty(input))
            {
                isValid = false;
            }
            else
            {
                input = input.Trim();
                isValid = Regex.IsMatch(input, EmailRegex, RegexOptions.IgnorePatternWhitespace);
            }
            return isValid;
        }

        /// <summary>
        /// Formats the case of a string like so: "This Has A Case Of Title Case."
        /// This is not strict title case, but rather a simplified version: The first letter
        /// of every word is upper case, the rest of the word is lower case.
        /// 
        /// Strict title case does not capitalize some short words such as "a", "and", "the",
        /// etc. (unless it is the first word in the sentence).
        /// </summary>
        /// <param name="input">String to format.  Cannot be null.</param>
        /// <returns>The input string with nothing changed but the case of the letters.</returns>
        public static string FormatTitleCase(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input", "Cannot format a null string.");
            }
            string[] words = input.Split(' ');
            string proper = "";
            for (int i = 0; i < words.Length; i++)
            {
                if (i > 0) proper += " ";
                string word = words[i];
                if (word.Length > 1)
                {
                    string first = word.Substring(0, 1).ToUpper();
                    string rest = word.Substring(1).ToLower();
                    proper += first + rest;
                }
                else
                {
                    proper += word.ToUpper();
                }
            }
            return proper;
        }

        /// <summary>
        /// Formats a string into a formatted telephone string if it matches either the international or USA pattern.
        /// </summary>
        /// <param name="telephone"></param>
        /// <returns>A telephone string format as (###) ###-#### [x###] -OR- +## (###) #### #### -OR- empty if no match</returns>
        public static string FormatTelephone(string telephone)
        {
            Regex telephoneRegex = new Regex(TelephoneRegex);
            Match match = telephoneRegex.Match(telephone);
            string formattedTelephone = "";
            if (match.Groups["International"].Value.Length > 0)
            {
                formattedTelephone = String.Format("+{0} ({1}) {2} {3}", match.Groups["CountryCode"].Value, match.Groups["ZoneCode"].Value, match.Groups["Number1"].Value, match.Groups["Number2"].Value).Trim();
            }
            else if (match.Groups["USA"].Value.Length > 0)
            {
                formattedTelephone = String.Format("({0}) {1}-{2} {3}", match.Groups["AreaCode"].Value, match.Groups["Exchange"].Value, match.Groups["Number"].Value, match.Groups["Extension"].Value).Trim();
            }
            // else no match
            return formattedTelephone;
        }

        /// <summary>
        /// Checks if the string is null or contains only whitespace.
        /// Similar to "String.IsNullOrEmpty(string input)", except that
        /// this method will say that "   " is blank, whereas IsNullOrEmpty will
        /// claim it is NOT empty.
        /// </summary>
        /// <param name="input">String to check.  Can be null.</param>
        /// <returns>true if the string contains non-whitespace characters.</returns>
        public static bool IsNonBlank(string input)
        {
            bool retVal = false;
            if ((input != null) && (input.Trim() != ""))
            {
                retVal = true;
            }
            return retVal;
        }

        /// <summary>
        /// Checks if the string contains nothing but letters.  Any non-letter
        /// characters (including whitespace, numbers, etc) will cause this to
        /// return false.
        /// </summary>
        /// <param name="input">String to check.  Can be null.</param>
        /// <returns>true if the string is non-blank and contains only letters.</returns>
        public static bool IsAlpha(string input)
        {
            bool retVal = false;
            if (!String.IsNullOrEmpty(input))
            {
                retVal = true;
                for (int i = 0; i < input.Length; i++)
                {
                    if (!char.IsLetter(input, i))
                    {
                        retVal = false;
                        break;
                    }
                }
            }
            return retVal;
        }

        /// <summary>
        /// Allows alpha numeric chars and . and - and _ and @.
        /// This checks only that this is an acceptable user name, not that it is in fact an
        /// actual user name in any particular system.
        /// </summary>
        /// <param name="input">String to check.  Can be null.</param>
        /// <returns>true if the string is non-blank and contains only acceptable username characters.</returns>
        public static bool IsValidUsername(string input)
        {
            bool isValid = false;
            if (!String.IsNullOrEmpty(input))
            {
                isValid = true;
                for (int i = 0; i < input.Length; i++)
                {
                    if (!char.IsLetterOrDigit(input, i) &&
                        (input[i] != '.') &&
                        (input[i] != '-') &&
                        (input[i] != '_') &&
                        (input[i] != '@'))
                    {
                        isValid = false;
                        break;
                    }
                }
            }
            return isValid;
        }

        /// <summary>
        /// Checks whether the input string is an integer.
        /// </summary>
        /// <param name="input">String to check.  Can be null.</param>
        /// <returns>true if the input is non-empty and can be parsed as an integer.</returns>
        public static bool IsInteger(string input)
        {
            bool retVal = false;
            if (!String.IsNullOrEmpty(input))
            {
                int result;
                retVal = Int32.TryParse(input, out result);
            }
            return retVal;
        }

        /// <summary>
        /// Checks whether the input string is an integer, and if so, whether it
        /// is within the specified range, inclusive.
        /// </summary>
        /// <param name="input">String to check.  Can be null.</param>
        /// <param name="low">Value that the input must be greater than or equal to.</param>
        /// <param name="high">Value that the input must be less than or equal to.</param>
        /// <returns>true if the input can be parsed as an integer, and low &lt;= input &lt;= high.</returns>
        public static bool IsIntWithinRange(string input, int low, int high)
        {
            if (low > high)
            {
                throw new ArgumentException("The low limit (" + low + ") must be less than or equal to the high limit (" + high + ").");
            }
            bool retVal = false;
            if (!String.IsNullOrEmpty(input))
            {
                int result;
                // If we parsed it OK, check the range.
                if (Int32.TryParse(input, out result) && (result >= low) && (result <= high))
                {
                    retVal = true;
                }
            }
            return retVal;
        }

        /// <summary>
        /// Checks whether the input string is a double.
        /// </summary>
        /// <param name="input">String to check.  Can be null.</param>
        /// <returns>true if the input is non-empty and can be parsed as a double.</returns>
        public static bool IsDouble(string input)
        {
            bool retVal = false;
            if (!String.IsNullOrEmpty(input))
            {
                double result;
                retVal = Double.TryParse(input, out result);
            }
            return retVal;
        }

        /// <summary>
        /// Checks whether the input string is a DateTime.  This doesn't support all possible datetime
        /// formats, so you should verify that it recognizes the format you expect before rolling out to
        /// production. (a great way to do that is add a few lines to the unit test!)
        /// </summary>
        /// <param name="input">String to check.  Can be null.</param>
        /// <returns>true if the input is non-empty and can be parsed as a DateTime.</returns>
        public static bool IsDateTime(string input)
        {
            bool retVal = false;
            if (!String.IsNullOrEmpty(input))
            {
                DateTime result;
                retVal = DateTime.TryParse(input, out result);
            }
            return retVal;
        }

        /// <summary>
        /// Ever wondered when to use ==, or .Equals, with strings?  .Equals is safer, because ==
        /// can fail if the compiler doesn't realize both types are strings (one is in an "object"
        /// variable for example).  But .Equals doesn't work on null (NullReferenceException!).
        /// So you can use this instead.  Either or both can be null and it will always return
        /// the right answer (null == null is true).
        /// </summary>
        /// <param name="str1">A string to compare, can be null.</param>
        /// <param name="str2">A string to compare, can be null.</param>
        /// <returns>true if both are null or both are identical strings, false otherwise.</returns>
        public static bool SafeEquals(string str1, string str2)
        {
            if (str1 == null)
            {
                return (str2 == null);
            }
            if (str2 == null)
            {
                return false;
            }
            return str1.Equals(str2);
        }

        /// <summary>
        /// Similar to string.CompareTo, except this handles nulls (null is "less" than a value,
        /// two nulls are equal).
        /// </summary>
        /// <param name="str1">A string to compare, can be null.</param>
        /// <param name="str2">A string to compare, can be null.</param>
        /// <returns>Less than 0 if str1 is less than str2,
        ///          0 if str1 is equal to str2,
        ///          greater than 0 if str1 is greater than str2.</returns>
        public static int SafeCompare(string str1, string str2)
        {
            if (str1 == null)
            {
                return str2 == null ? 0 : -1;
            }
            if (str2 == null)
            {
                return 1;
            }
            return str1.CompareTo(str2);
        }

        /// <summary>
        /// Takes a group of anything and joins based on ToString values, separated by the given
        /// separator.
        /// </summary>
        /// <param name="joinUs">Group of values to concatenate.</param>
        /// <returns>All the values concatenated, or "" if there were no values.</returns>
        public static string Join(IEnumerable joinUs)
        {
            return Join(joinUs, ", ", true);
        }

        /// <summary>
        /// Takes a group of anything and joins based on ToString values, separated by the given
        /// separator.
        /// </summary>
        /// <param name="joinUs">Group of values to concatenate.</param>
        /// <param name="separator">String to insert between each of the values.</param>
        /// <returns>All the values concatenated, or "" if there were no values.</returns>
        public static string Join(IEnumerable joinUs, string separator)
        {
            return Join(joinUs, separator, true);
        }

        /// <summary>
        /// Takes a group of anything and joins based on ToString values, separated by the given
        /// separator.
        /// </summary>
        /// <param name="joinUs">Group of values to concatenate.</param>
        /// <param name="separator">String to insert between each of the values.</param>
        /// <param name="showNulls">If true, null values will be listed as the string "&lt;null&gt;".
        ///                         If false, null values will be left as the default ("").</param>
        /// <returns>All the values concatenated, or "" if there were no values.</returns>
        public static string Join(IEnumerable joinUs, string separator, bool showNulls)
        {
            if (joinUs == null)
            {
                return showNulls ? "<null group>" : "";
            }
            StringBuilder sb = new StringBuilder();
            int count = 0;
            try
            {
                foreach (object obj in joinUs)
                {
                    if (count++ != 0)
                    {
                        sb.Append(separator);
                    }
                    if (showNulls && (obj == null))
                    {
                        sb.Append("<null>");
                    }
                    else
                    {
                        sb.Append(obj);
                    }
                }
                return sb.ToString();
            }
            catch (Exception e)
            {
                throw new LoggingException("Unable to concatenate values, error on element " + count, e);
            }
        }
    }
}
