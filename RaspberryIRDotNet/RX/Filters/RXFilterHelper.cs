using System;
using System.Linq;

namespace RaspberryIRDotNet.RX.Filters
{
    /// <summary>
    /// Helper class that can be used by other filters to achieve common tasks.
    /// </summary>
    public class RXFilterHelper
    {
        public int? MinPulseSpaceCount { get; set; }
        public int? MaxPulseSpaceCount { get; set; }

        /// <summary>
        /// The percentage of error to allow in the lead-in. For example if this is set to 0.1 (10%) and the lead in sample is 500us then anything between 450us-550us would be allowed.
        /// </summary>
        public double LeadInErrorPercentage
        {
            get { return _leadInErrorPercentage; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Cannot have a negative percentage.");
                }

                if (value > 5)
                {
                    // The IR data will be no use if it's more than 500% out. So assume anyone applying that as a lead-in check made a mistake.
                    throw new ArgumentOutOfRangeException(nameof(value), "Sanity check: the specified percentage is more than 500%. Use decimal form. So if you were aiming for 5% then use 0.05 instead.");
                }

                _leadInErrorPercentage = value;
            }
        }
        private double _leadInErrorPercentage = 0.1;

        public IReadOnlyPulseSpaceDurationList LeadIn { get; set; }

        public void SetLeadInByUnits(IReadOnlyPulseSpaceUnitList units, int unitDuration)
        {
            if (units == null)
            {
                LeadIn = null;
            }
            else
            {
                if (unitDuration < Utility.UnitDurationMinimum || unitDuration > Utility.UnitDurationMaximum)
                {
                    throw new ArgumentOutOfRangeException(nameof(unitDuration));
                }
                LeadIn = new PulseSpaceDurationList(units.Select(x => x * unitDuration));
            }
        }

        public bool Check(IReadOnlyPulseSpaceDurationList pulseSpaceDurations)
        {
            if (MinPulseSpaceCount.HasValue && pulseSpaceDurations.Count < MinPulseSpaceCount.Value)
            {
                return false;
            }
            if (MaxPulseSpaceCount.HasValue && pulseSpaceDurations.Count > MaxPulseSpaceCount.Value)
            {
                return false;
            }

            if (LeadIn?.Count > 0)
            {
                if (LeadIn.Count > pulseSpaceDurations.Count)
                {
                    // The IR data is so short it's less than the expected lead-in.
                    return false;
                }

                for (int i = 0; i < LeadIn.Count; i++)
                {
                    if (!pulseSpaceDurations.IsWithinPercent(i, LeadIn[i], LeadInErrorPercentage))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
