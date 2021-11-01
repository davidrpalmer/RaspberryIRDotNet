using System;

namespace RaspberryIRDotNet.RX.Filters
{
    /// <summary>
    /// Filter out IR if any PULSE or SPACE is either too short or too long.
    /// </summary>
    /// <remarks>
    /// IR background noise is often shorter than valid signals, so setting the <see cref="Minimum"/> property is a very simple and effective way of cutting out the majority of IR noise.
    /// <para />
    /// Setting a minimum is easy if you know the unit duration - just take the unit duration, subtract a margin of error and use that.<br/>
    /// Setting a maximum can be a bit harder because sometimes a PULSE or a SPACE will be double (or maybe more) in length. You will need to look at the IR data to determine what the longest value should be.
    /// </remarks>
    public class DurationFilter : IRXFilter
    {
        /// <summary>
        /// The minimum duration (in microseconds) any PULSE or SPACE can be. NULL to not check.
        /// </summary>
        public int? Minimum { get; set; }

        /// <summary>
        /// The maximum duration (in microseconds) any PULSE or SPACE can be. NULL to not check.
        /// </summary>
        public int? Maximum { get; set; }

        public void AssertConfigOK()
        {
        }

        public bool Check(IReadOnlyPulseSpaceDurationList pulseSpaceDurations)
        {
            foreach (var duration in pulseSpaceDurations)
            {
                if (duration < Minimum || duration > Maximum)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
