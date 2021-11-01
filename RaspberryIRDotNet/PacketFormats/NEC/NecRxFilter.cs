using System;
using RaspberryIRDotNet.RX.Filters;

namespace RaspberryIRDotNet.PacketFormats.NEC
{
    public class NecRxFilter : IRXFilter
    {
        private readonly RXFilterHelper _helper;

        public NecRxFilter()
        {
            _helper = new RXFilterHelper()
            {
                MinPulseSpaceCount = 60, // Approx value
                MaxPulseSpaceCount = 130, // Approx value
            };
            _helper.SetLeadInByUnits(NecBinaryConverter.NecStandardLeadInPattern, NecBinaryConverter.NecStandardUnitDurationMicrosecs);
        }

        public void AssertConfigOK()
        {
        }

        public bool Check(IReadOnlyPulseSpaceDurationList pulseSpaceDurations)
        {
            return _helper.Check(pulseSpaceDurations);
        }
    }
}
