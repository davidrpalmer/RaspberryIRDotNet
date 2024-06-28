using System;

namespace RaspberryIRDotNet.RX.Filters
{
    public class PulseSpaceCountFilter : IRXFilter
    {
        private readonly RXFilterHelper _helper = new();

        /// <summary>
        /// The minimum number of PULSEs and SPACEs. NULL to not check.
        /// </summary>
        public int? Minimum { get => _helper.MinPulseSpaceCount; set => _helper.MinPulseSpaceCount = value; }

        /// <summary>
        /// The maximum number of PULSEs and SPACEs. NULL to not check.
        /// </summary>
        public int? Maximum { get => _helper.MaxPulseSpaceCount; set => _helper.MaxPulseSpaceCount = value; }

        public void AssertConfigOK()
        {
        }

        public bool Check(IReadOnlyPulseSpaceDurationList pulseSpaceDurations)
        {
            return _helper.Check(pulseSpaceDurations);
        }
    }
}
