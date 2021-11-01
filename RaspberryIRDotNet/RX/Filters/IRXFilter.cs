using System;

namespace RaspberryIRDotNet.RX.Filters
{
    /// <summary>
    /// Used for filtering out IR noise or detecting the message format.
    /// </summary>
    public interface IRXFilter
    {
        /// <returns>
        /// TRUE if the message passes the filter, FALSE if it should be blocked.
        /// </returns>
        bool Check(IReadOnlyPulseSpaceDurationList pulseSpaceDurations);

        /// <summary>
        /// Throws an <see cref="ArgumentException"/> (or a type that inherits from it) if the config for this filter is invalid.
        /// </summary>
        void AssertConfigOK();
    }
}
