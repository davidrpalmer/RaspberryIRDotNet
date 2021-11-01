using System;
using RaspberryIRDotNet.RX.Filters;

namespace RaspberryIRDotNet.PacketFormats.Kasiekyo
{
    public class PanasonicKasiekyoRxFilter : IRXFilter
    {
        private readonly RXFilterHelper _helper;

        public PanasonicKasiekyoRxFilter()
        {
            _helper = new RXFilterHelper()
            {
                MinPulseSpaceCount = 95, // Approx value
                MaxPulseSpaceCount = 195, // Approx value
            };
            _helper.SetLeadInByUnits(KasiekyoBinaryConverter.KasiekyoStandardLeadInPattern, KasiekyoBinaryConverter.KasiekyoStandardUnitDurationMicrosecs);
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
