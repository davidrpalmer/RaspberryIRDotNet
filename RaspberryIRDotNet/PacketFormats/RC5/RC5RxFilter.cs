using System;
using RaspberryIRDotNet.RX.Filters;

namespace RaspberryIRDotNet.PacketFormats.RC5
{
    /// <summary>
    /// RC5 has no lead-in so this is only checking the number of PULSEs and SPACEs and the duration of each one.
    /// </summary>
    public class RC5RxFilter : IRXFilter
    {
        private readonly RXFilterHelper _helper;

        public RC5RxFilter()
        {
            _helper = new RXFilterHelper()
            {
                MinPulseSpaceCount = 13,
                MaxPulseSpaceCount = 27,
            };
        }

        public void AssertConfigOK()
        {
        }

        public bool Check(IReadOnlyPulseSpaceDurationList pulseSpaceDurations)
        {
            if (!_helper.Check(pulseSpaceDurations))
            {
                return false;
            }

            int expectedUnitDuration = RC5Converter.RC5StandardUnitDurationMicrosecs;
            const double errorPercentage = 0.25;

            for (int i = 0; i < pulseSpaceDurations.Count; i++)
            {
                if (!pulseSpaceDurations.IsWithinPercent(i, expectedUnitDuration, errorPercentage) &&
                    !pulseSpaceDurations.IsWithinPercent(i, expectedUnitDuration * 2, errorPercentage))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
