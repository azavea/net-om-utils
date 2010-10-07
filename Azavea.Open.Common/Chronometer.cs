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
using System.Text;
using System.Threading;
using Azavea.Open.Common.Caching;
using log4net;

namespace Azavea.Open.Common
{
    /// <summary>
    /// A class for timing how long things take.  You can instantiate a Chronometer,
    /// or just use the static methods for timing named operations.
    /// </summary>
	public class Chronometer
	{
        private readonly static ILog _log = LogManager.GetLogger(
            new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().DeclaringType.Namespace);
        
        DateTime _startTime;
		DateTime _lastSplitTime;

		/// <summary>
		/// Chronometer reporting enumeration.
		/// </summary>
		public enum ReportType {
			/// <summary>
			/// Display output as a text table.
			/// </summary>
			TextTable,
			/// <summary>
			/// Generate XML fragments for each key.
			/// </summary>
			XmlElements,
			/// <summary>
			/// Generate a CSV list for each key.
			/// </summary>
			TextCsv
		}

        /// <summary>
        /// Creates (and starts) the timer.
        /// </summary>
		public Chronometer()
		{
			_startTime = DateTime.Now;
			_lastSplitTime = _startTime;
		}

        /// <summary>
        /// Restarts the timer.
        /// </summary>
		public void Start()
		{
			_startTime = DateTime.Now;
			_lastSplitTime = _startTime;
		}

        /// <summary>
        /// Restarts the timer (same as calling Start() again).
        /// </summary>
		public void Reset()
		{
			_startTime = DateTime.Now;
			_lastSplitTime = _startTime;
		}

        /// <summary>
        /// How long has the timer been running.
        /// </summary>
        /// <returns>The TimeSpan since the timer was created or Start or Reset was last called.</returns>
		public TimeSpan GetRunTime()
		{
			TimeSpan runTime = new TimeSpan(DateTime.Now.Ticks - _startTime.Ticks);
			return runTime;
		}

        /// <summary>
        /// How long has the timer been running since you last called GetSplit.
        /// </summary>
        /// <returns>The TimeSpan since GetSplit was last called, or if it has never been
        ///          called, since the timer was created or Start or Reset was last called.</returns>
        public TimeSpan GetSplit()
		{
			DateTime thisSplitTime = DateTime.Now;
			TimeSpan split = new TimeSpan(thisSplitTime.Ticks - _lastSplitTime.Ticks);
			_lastSplitTime = thisSplitTime;
			return split;
		}

        /// <summary>
        /// For display purposes, reports the split time in milliseconds.
        /// </summary>
        /// <returns>The total split time in ms, as a string "# ms".</returns>
		public string GetSplitMilliseconds()
		{
			TimeSpan split = GetSplit();
			return split.TotalMilliseconds + " ms.";
		}

        /// <summary>
        /// For display purposes, reports the split time in seconds.
        /// </summary>
        /// <returns>The total split time in seconds, as a string "# s".</returns>
        public string GetSplitSeconds()
		{
			TimeSpan split = GetSplit();
			return split.TotalSeconds + " sec.";
		}

        /// <summary>
        /// For display purposes, reports the run time as a long string.
        /// </summary>
        /// <returns>The total run time, as a string "# days, # hours, # minutes, # seconds, # milliseconds".</returns>
        public string GetElapsedTime()
		{
			TimeSpan runTime = GetRunTime();
			return runTime.Days + " days, " + runTime.Hours + " hours, " + runTime.Minutes +
                " minutes, " + runTime.Seconds + " seconds, " + runTime.Milliseconds + " ms";
		}

        /// <summary>
        /// The time the timer was created, or the last time Start or Reset was called.
        /// </summary>
		public DateTime StartTime
		{
			get{return _startTime;}
		}

        /// <summary>
        /// The time GetSplit was last called, or the timer was created,
        /// or the last time Start or Reset was called.
        /// </summary>
        public DateTime LastSplit
		{
			get{return _lastSplitTime;}
		}

        /// <summary>
        /// Cache the StringBuilders we use to minimize the amount of time added
        /// by the timing code itself.
        /// </summary>
        private readonly static StringBuilderCache _sbCache = new StringBuilderCache();

        /// <summary>
        /// The times of the operations currently "started" via the static methods.
        /// </summary>
        private readonly static Dictionary<string, long> _operationStartTimes =
            new Dictionary<string, long>();

        /// <summary>
        /// The total times for all the operations that have been "ended" via
        /// the static methods at least once.
        /// </summary>
        private readonly static Dictionary<string, List<long>> _operationTotalTimes =
            new Dictionary<string, List<long>>();

        /// <summary>
        /// Starts timing the specified named operation.  Each time the same
        /// named operation is completed the time will be added to our list, so
        /// that a number of calls to the same command will give you totals,
        /// average, and number of executions.
        /// </summary>
        /// <param name="keyBuilder">A StringBuilder containing a string that uniquely
        ///                          identifies the operation.</param>
        public static void BeginTiming(StringBuilder keyBuilder)
        {
            try
            {
                StringBuilder keyWithThread = _sbCache.Get();
                keyWithThread.Append(keyBuilder);
                keyWithThread.Append(Thread.CurrentThread.Name);
                ReallyBeginTiming(keyWithThread.ToString());
                _sbCache.Return(keyWithThread);
            }
            catch (Exception e)
            {
                _log.Warn("Chronometer failed to begin timing " + keyBuilder, e);
            }
        }
        /// <summary>
        /// Starts timing the specified named operation.  Each time the same
        /// named operation is completed the time will be added to our list, so
        /// that a number of calls to the same command will give you totals,
        /// average, and number of executions.
        /// </summary>
        /// <param name="key">A string that uniquely identifies the operation.</param>
        public static void BeginTiming(string key)
        {
            try
            {
                StringBuilder keyWithThread = _sbCache.Get();
                keyWithThread.Append(key);
                keyWithThread.Append(Thread.CurrentThread.Name);
                ReallyBeginTiming(keyWithThread.ToString());
                _sbCache.Return(keyWithThread);
            }
            catch (Exception e)
            {
                _log.Warn("Chronometer failed to begin timing " + key, e);
            }
        }
        /// <summary>
        /// Behind the scenes we tack onto the key the name of the thread, in case 
        /// multiple threads are running the same named operation at the same time.
        /// </summary>
        /// <param name="keyWithThread">Key consisting of the original key plus the thread name.</param>
        private static void ReallyBeginTiming(string keyWithThread)
        {
            lock (_operationStartTimes)
            {
                _operationStartTimes[keyWithThread] = DateTime.Now.Ticks;
            }
        }

        /// <summary>
        /// Stops timing the given item and updates the timing report.
        /// </summary>
        /// <param name="keyBuilder">A StringBuilder containing a string that uniquely
        ///                          identifies the operation.</param>
        /// <returns>The duration just measured, in ticks, or -1 if not valid.</returns>
        public static long EndTiming(StringBuilder keyBuilder)
        {
            StringBuilder keyWithThread = _sbCache.Get();
            try
            {
                keyWithThread.Append(keyBuilder);
                keyWithThread.Append(Thread.CurrentThread.Name);
                return ReallyEndTiming(keyBuilder.ToString(), keyWithThread.ToString());
            }
            catch (Exception e)
            {
                _log.Warn("Chronometer failed to end timing " + keyBuilder, e);
                return -1;
            }
            finally
            {
                _sbCache.Return(keyWithThread);
            }
        }
        /// <summary>
        /// Stops timing the given item and updates the timing report.
        /// </summary>
        /// <param name="key">A string that uniquely identifies the operation.</param>
        /// <returns>The duration just measured, in ticks, or -1 if not valid.</returns>
        public static long EndTiming(string key)
        {
            StringBuilder keyWithThread = _sbCache.Get();
            try
            {
                keyWithThread.Append(key);
                keyWithThread.Append(Thread.CurrentThread.Name);
                return ReallyEndTiming(key, keyWithThread.ToString());
            }
            catch (Exception e)
            {
                _log.Warn("Chronometer failed to end timing " + key, e);
                return -1;
            }
            finally
            {
                _sbCache.Return(keyWithThread);
            }
        }
        /// <summary>
        /// Behind the scenes we tack onto the key the name of the thread, in case 
        /// multiple threads are running the same named operation at the same time.
        /// </summary>
        /// <param name="originalKey">The key provided by the client.</param>
        /// <param name="keyWithThread">Key consisting of the original key plus the thread name.</param>
        /// <returns>The duration just measured, in ticks, or -1 if not valid.</returns>
        private static long ReallyEndTiming(string originalKey, string keyWithThread)
        {
            long duration = 0;
            bool durationValid = false;
            lock (_operationStartTimes)
            {
                if (_operationStartTimes.ContainsKey(keyWithThread))
                {
                    long startTime = _operationStartTimes[keyWithThread];
                    _operationStartTimes.Remove(keyWithThread);
                    duration = DateTime.Now.Ticks - startTime;
                    durationValid = true;
                } // else forget it, we can't time this one, oh well.
            }
            if (durationValid)
            {
                lock (_operationTotalTimes)
                {
                    if (_operationTotalTimes.ContainsKey(originalKey))
                    {
                        // Add the duration and increment the count.
                        _operationTotalTimes[originalKey].Add(duration);
                    }
                    else
                    {
                        // Not there yet, so just add the first values.
                        _operationTotalTimes[originalKey] = new List<long>(new long[]{duration});
                    }
                }
            }
            return durationValid ? duration : -1;
        }

        /// <summary>
        /// Reports all the times for all the operations that both BeginTiming and
        /// EndTiming were called for.  This is imperfect as any call that failed may
        /// or may not have a time reported.  It should,
        /// however, give a rough idea what operations are taking how long.
        /// </summary>
		/// <param name="printDisclaimer">Print a disclaimer notice along with the times.</param>
		/// <param name="extendedStats">Show extended stats: Min, Max, StdDev, Variance.</param>
		/// <param name="outType">The type of output to report.</param>
        public static void ReportTimes(bool extendedStats, ReportType outType, bool printDisclaimer)
        {
			IReporter reporter;
        	switch (outType)
        	{
				case ReportType.XmlElements:
					reporter = new XmlElementReporter();
					break;
				case ReportType.TextCsv:
					reporter = new TextCsvReporter();
					break;
        		//case ReportType.TextTable:
				default:
					reporter = new TextTableReporter();
					break;
        	}

			reporter.ReportPrologue( printDisclaimer );

            lock (_operationTotalTimes)
            {
                foreach (string key in _operationTotalTimes.Keys)
                {
                    long totalTicks = 0;
					long minTicks = Int64.MaxValue;
					long maxTicks = Int64.MinValue;
					foreach (long tick in _operationTotalTimes[key])
					{
						totalTicks += tick;
						if ( tick > maxTicks )
							maxTicks = tick;
						if ( tick < minTicks )
							minTicks = tick;
					}
                    int count = _operationTotalTimes[key].Count;
					long average = totalTicks / count;
					double averageSec = new TimeSpan(average).TotalSeconds;
					double variance = 0;
					foreach (long tick in _operationTotalTimes[key])
					{
						double sec = new TimeSpan(tick).TotalSeconds;
						variance += Math.Pow(sec - averageSec,2);
					}
					variance /= (count - 1);
					double stddev = Math.Sqrt(variance);

					reporter.ReportKey( key, count, totalTicks, average, 
						minTicks, maxTicks, stddev, variance, extendedStats );
                }
            }

			reporter.ReportEpilogue();
        }

        /// <summary>
        /// Reports all the times for all the operations that both BeginTiming and
        /// EndTiming were called for.  This is imperfect as any call that failed may
        /// or may not have a time reported.  It should,
        /// however, give a rough idea what operations are taking how long.
        /// </summary>
		public static void ReportTimes()
		{
			ReportTimes(false, ReportType.TextTable, true);
		}

        /// <summary>
        /// Reports all the times for all the operations that both BeginTiming and
        /// EndTiming were called for.  This is imperfect as any call that failed may
        /// or may not have a time reported.  It should,
        /// however, give a rough idea what operations are taking how long.
        /// </summary>
		/// <param name="extendedStats">Show extended stats: Min, Max, StdDev, Variance.</param>
		public static void ReportTimes(bool extendedStats)
		{
			ReportTimes(extendedStats, ReportType.TextTable, true);
		}

        /// <summary>
        /// Reports all the times for all the operations that both BeginTiming and
        /// EndTiming were called for.  This is imperfect as any call that failed may
        /// or may not have a time reported.  It should,
        /// however, give a rough idea what operations are taking how long.
        /// </summary>
		/// <param name="extendedStats">Show extended stats: Min, Max, StdDev, Variance.</param>
		/// <param name="reportType">Print a disclaimer notice along with the times.</param>
        public static void ReportTimes(bool extendedStats, ReportType reportType)
		{
			ReportTimes(extendedStats, reportType, true);
		}

		/// <summary>
		/// Define a private interface for a reported used by ReportTimes.
		/// </summary>
		private interface IReporter
		{
			void ReportPrologue(bool printDisclaimer);
			void ReportKey(string key, int count, long totalTicks, long average, long minTicks, long maxTicks, double stddev, double variance, bool extended);
			void ReportEpilogue();
		}

		/// <summary>
		/// A class that writes a nicely human readable text table of timing reports.
		/// </summary>
		private class TextTableReporter : IReporter
		{
			/// <summary>
			/// Display a text prologue.
			/// </summary>
			/// <param name="printDisclaimer">Show the disclaimer text.</param>
			public void ReportPrologue(bool printDisclaimer)
			{
	            _log.Info("Begin Timing Report:");
				if ( printDisclaimer ) {
					_log.Info("DISCLAIMER: Operations that ran a very small number of times, yet supposedly");
					_log.Info("            took a very long time on average, may be ones that blocked on");
					_log.Info("            another call or operation for a long time.  For example:");
					_log.Info("            if every row that is read from a database is sent to a stone");
					_log.Info("            tablet engraver that takes 5 minutes per tablet, and the next");
					_log.Info("            row isn't read until the first one is finished being written,");
					_log.Info("            this report will say it took 15 minutes to read 3 rows.  That");
					_log.Info("            is technically true, but has nothing to do with the database");
					_log.Info("            or the code.  This report is to give you a general idea where");
					_log.Info("            to look for performance problems, not to be the One True ");
					_log.Info("            Benchmarking Tool.");
				}
			}

			/// <summary>
			/// Report the timing information for 1 key.
			/// </summary>
			/// <param name="key">The name of the key.</param>
			/// <param name="count">The number of times timed.</param>
			/// <param name="totalTicks">The total time over all times timed.</param>
			/// <param name="average">The average tick time.</param>
			/// <param name="minTicks">The minimum tick time.</param>
			/// <param name="maxTicks">The maximum tick time.</param>
			/// <param name="stddev">The standard deviation in tick times.</param>
			/// <param name="variance">The variance in tick times.</param>
			/// <param name="extended">Show extended stats (min, max, stddev, variance).</param>
			public void ReportKey( string key, int count, long totalTicks, long average, long minTicks, long maxTicks, double stddev, double variance, bool extended )
			{
                TimeSpan averageSpan = new TimeSpan(average);
				_log.Info(String.Format("    Count: {0},      Key: {1}", count.ToString().PadLeft(16), key));
                _log.Info(String.Format(
					"    Total: {0},  Average: {1}",
					new TimeSpan(totalTicks).ToString().PadRight(16),
					averageSpan.ToString().PadRight(16))
				);
				if ( extended ) {
					_log.Info(String.Format(
						"  Minimum: {0},  Maximum: {1}",
						new TimeSpan(minTicks).ToString().PadRight(16),
						new TimeSpan(maxTicks).ToString().PadRight(16))
					);
					_log.Info(String.Format(
						"   StdDev: {0}, Variance: {1}",
						stddev.ToString("F7").PadLeft(16),
						variance.ToString("F7").PadLeft(16))
					);
				}
			}

			/// <summary>
			/// Display a text epilogue.
			/// </summary>
			public void ReportEpilogue(){
	            _log.Info("End Timing Report.");
			}
		}

		/// <summary>
		/// A class that writes a nicely machine readable text table of timing reports.
		/// </summary>
		private class TextCsvReporter : IReporter
		{
			/// <summary>
			/// Display a text prologue.
			/// </summary>
			/// <param name="printDisclaimer">Show the disclaimer text.</param>
			public void ReportPrologue(bool printDisclaimer)
			{
				if ( printDisclaimer ) {
					_log.Info("\"DISCLAIMER: Operations that ran a very small number of times, yet supposedly " +
						"took a very long time on average, may be ones that blocked on " +
						"another call or operation for a long time.  For example: " +
						"if every row that is read from a database is sent to a stone " +
						"tablet engraver that takes 5 minutes per tablet, and the next " +
						"row isn't read until the first one is finished being written, " +
						"this report will say it took 15 minutes to read 3 rows.  That " +
						"is technically true, but has nothing to do with the database " +
						"or the code.  This report is to give you a general idea where " +
						"to look for performance problems, not to be the One True Benchmarking Tool.\"");
				}
			}

			/// <summary>
			/// Report the timing information for 1 key.
			/// </summary>
			/// <param name="key">The name of the key.</param>
			/// <param name="count">The number of times timed.</param>
			/// <param name="totalTicks">The total time over all times timed.</param>
			/// <param name="average">The average tick time.</param>
			/// <param name="minTicks">The minimum tick time.</param>
			/// <param name="maxTicks">The maximum tick time.</param>
			/// <param name="stddev">The standard deviation in tick times.</param>
			/// <param name="variance">The variance in tick times.</param>
			/// <param name="extended">Show extended stats (min, max, stddev, variance).</param>
			public void ReportKey( string key, int count, long totalTicks, long average, long minTicks, long maxTicks, double stddev, double variance, bool extended )
			{
				TimeSpan averageSpan = new TimeSpan(average);
				string extStats = (extended)?
					String.Format(",{0},{1},{2},{3}",
						new TimeSpan(minTicks),
						new TimeSpan(maxTicks),
						stddev.ToString("F7"),
						variance.ToString("F7")) :
					String.Empty;
				_log.Info(String.Format("\"{0}\",{1},{2},{3}{4}", 
					key, count,
					new TimeSpan(totalTicks),
					averageSpan,
					extStats));
			}

			/// <summary>
			/// Display a text epilogue.
			/// </summary>
			public void ReportEpilogue(){
			}
		}

		/// <summary>
		/// A class that writes a nicely machine readable text table of timing reports.
		/// </summary>
		private class XmlElementReporter : IReporter
		{
			/// <summary>
			/// Display an xml prologue.
			/// </summary>
			/// <param name="printDisclaimer">Show the disclaimer text.</param>
			public void ReportPrologue(bool printDisclaimer)
			{
				if ( printDisclaimer ) {
					_log.Info("<!-- DISCLAIMER: Operations that ran a very small number of times, yet supposedly\n" +
					          "                 took a very long time on average, may be ones that blocked on\n" +
					          "                 another call or operation for a long time.  For example:\n" +
					          "                 if every row that is read from a database is sent to a stone\n" +
					          "                 tablet engraver that takes 5 minutes per tablet, and the next\n" +
					          "                 row isn't read until the first one is finished being written,\n" +
					          "                 this report will say it took 15 minutes to read 3 rows.  That\n" +
					          "                 is technically true, but has nothing to do with the database\n" +
					          "                 or the code.  This report is to give you a general idea where\n" +
					          "                 to look for performance problems, not to be the One True\n" +
					          "                 Benchmarking Tool.");
				}
			}

			/// <summary>
			/// Report the timing information for 1 key.
			/// </summary>
			/// <param name="key">The name of the key.</param>
			/// <param name="count">The number of times timed.</param>
			/// <param name="totalTicks">The total time over all times timed.</param>
			/// <param name="average">The average tick time.</param>
			/// <param name="minTicks">The minimum tick time.</param>
			/// <param name="maxTicks">The maximum tick time.</param>
			/// <param name="stddev">The standard deviation in tick times.</param>
			/// <param name="variance">The variance in tick times.</param>
			/// <param name="extended">Show extended stats (min, max, stddev, variance).</param>
			public void ReportKey( string key, int count, long totalTicks, long average, long minTicks, long maxTicks, double stddev, double variance, bool extended )
			{
				StringBuilder sb = new StringBuilder();

				sb.Append( "<ChronometerTime>" )
					.Append( "<key value=\"" ).Append( key ).Append("\"/>" )
					.Append( "<count value=\"" ).Append( count ).Append("\"/>")
					.Append( "<total value=\"" ).Append( new TimeSpan(totalTicks) ).Append("\"/>")
					.Append( "<average value\"").Append( new TimeSpan(average) ).Append("\"/>");

				if ( extended ) {
					sb.Append( "<minmum value=\"" ).Append( new TimeSpan(minTicks)).Append("\"/>")
						.Append( "<maximum value=\"" ).Append( new TimeSpan(maxTicks) ).Append( "\"/>")
						.Append( "<standarddeviation value=\"" ).Append( stddev.ToString("F7") ).Append( "\"/>" )
						.Append( "<variance value=\"").Append( variance.ToString("F7") ).Append("\"/>");
				}

				sb.Append( "</ChronometerTime>" );

				_log.Info(sb.ToString());
			}

			/// <summary>
			/// Display a text epilogue.
			/// </summary>
			public void ReportEpilogue(){
			}
		}
	}
}
