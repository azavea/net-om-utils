using System;

namespace Avencia.Open.Common.Caching
{
    /// <summary>
    /// Represents a piece of data plus a timestamp that is relevant to that data.
    /// </summary>
    /// <typeparam name="T">The type of data held in this item.</typeparam>
    public class TimestampedData<T>
    {
        /// <summary>
        /// The data that you're timestamping.
        /// </summary>
        public readonly T Data;
        /// <summary>
        /// This is the time associated with that data.
        /// </summary>
        public readonly DateTime Time;

        /// <summary>
        /// Timestamp some data.  The timestamp used will be 'DateTime.Now'.
        /// </summary>
        /// <param name="data">The data to timestamp.</param>
        public TimestampedData(T data)
        {
            Data = data;
            Time = DateTime.Now;
        }
    }
}