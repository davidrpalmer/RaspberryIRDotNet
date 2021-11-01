using System;

namespace RaspberryIRDotNet.RX.Filters
{
    public class LeadInPatternFilter : IRXFilter
    {
        /// <summary>
        /// IR signals start with a lead-in pattern. Specify the pattern here so that signals may be distinguished from background noise. The first is a pulse, the second is a space. If there is a 3rd then that is a pulse and so on.
        /// </summary>
        public IReadOnlyPulseSpaceDurationList LeadInPattern { get => _helper.LeadIn; set => _helper.LeadIn = value; }

        /// <summary>
        /// The percentage of error to allow in the lead-in. For example if this is set to 0.1 (10%) and the lead in sample is 500us then anything between 450us-550us would be allowed.
        /// </summary>
        public double LeadInErrorPercentage { get => _helper.LeadInErrorPercentage; set => _helper.LeadInErrorPercentage = value; }

        private readonly RXFilterHelper _helper = new RXFilterHelper();

        public LeadInPatternFilter()
        {
        }

        public LeadInPatternFilter(IReadOnlyPulseSpaceUnitList units, int unitDuration)
        {
            _helper.SetLeadInByUnits(units, unitDuration);
        }

        public LeadInPatternFilter(IReadOnlyPulseSpaceDurationList microsecs)
        {
            LeadInPattern = microsecs;
        }

        /// <param name="unitDuration">The duration of each unit in microseconds.</param>
        public void SetLeadInByUnits(IReadOnlyPulseSpaceUnitList units, int unitDuration)
        {
            _helper.SetLeadInByUnits(units, unitDuration);
        }

        public void AssertConfigOK()
        {
            if (LeadInPattern == null)
            {
                throw new ArgumentNullException(nameof(LeadInPattern));
            }
            foreach (int leadIn in LeadInPattern)
            {
                if (leadIn < 50)
                {
                    // A lead-in pattern of with a duration less than 50 microseconds is very unlikely, so assume it is wrong. Most likely it has been set as a number of units instead.
                    throw new ArgumentException($"The lead-in pattern specification looks invalid (at least one item is too short). Did you specify it in units? It should be specified in microseconds. If you want to specify units then use the {nameof(SetLeadInByUnits)} method.");
                }

                if (leadIn > Utility.UnitDurationMaximum)
                {
                    throw new ArgumentException($"The lead-in pattern specification looks invalid (at least one item is too long).");
                }
            }
        }

        public bool Check(IReadOnlyPulseSpaceDurationList pulseSpaceDurations)
        {
            return _helper.Check(pulseSpaceDurations);
        }
    }
}
