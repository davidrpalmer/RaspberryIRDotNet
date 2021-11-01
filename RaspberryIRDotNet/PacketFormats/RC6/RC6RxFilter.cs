using System;
using RaspberryIRDotNet.RX.Filters;

namespace RaspberryIRDotNet.PacketFormats.RC6
{
    public class RC6RxFilter : IRXFilter
    {
        private readonly RXFilterHelper _helper;

        public RC6RxFilter()
        {
            _helper = new RXFilterHelper()
            {
                MinPulseSpaceCount = 10, // Rough number, not exactly calculated.
                LeadIn = new PulseSpaceDurationList() { 2666, 889 }
            };
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
