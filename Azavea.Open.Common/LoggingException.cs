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
using log4net;

namespace Azavea.Open.Common
{
    /// <summary>
    /// This exception logs its message and parent exception (if any) as
    /// "debug" log messages, allowing run-time debugging if it is discovered
    /// that an exception is being eaten somewhere.
    /// 
    /// Normally an exception will be logged by client code, so there is no
    /// need to log a warning everywhere one is generated.
    /// </summary>
    public class LoggingException : ApplicationException
    {
        /// <summary>
        /// A log4net logger that can be used to log info about the exception.
        /// </summary>
        protected readonly ILog _log = LogManager.GetLogger(
            new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().DeclaringType.Namespace);

        /// <summary>
        /// Creates the exception, with an inner exception.
        /// </summary>
        /// <param name="message">What went wrong?</param>
        /// <param name="e">Another exception that caused this one to be thrown.</param>
        public LoggingException(string message, Exception e)
            : base(message, e)
        {
            _log.Debug(message, e);
        }
        /// <summary>
        /// Creates the exception.
        /// </summary>
        /// <param name="message">What went wrong?</param>
        public LoggingException(string message)
            : base(message)
        {
            _log.Debug(message);
        }
    }
}
