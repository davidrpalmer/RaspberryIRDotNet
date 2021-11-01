using System;
using RaspberryIRDotNet;
using RaspberryIRDotNet.PacketFormats;
using RaspberryIRDotNet.PacketFormats.RC6;
using RaspberryIRDotNet.PacketFormats.RC6.Mode0;
using RaspberryIRDotNet.PacketFormats.RC6.Mode6A;

namespace RaspberryIRDotNetExamples.Demos
{
    public class RC6AutoDetectingConverter : RC6Converter, IPulseSpacePacketConverter<RC6Packet>
    {
        private readonly RC6Mode0Converter _mode0Converter = new RC6Mode0Converter();
        private readonly RC6Mode6ARawPayloadConverter _mode6AConverter = new RC6Mode6ARawPayloadConverter();

        public PulseSpaceUnitList ToIR(RC6Packet packet)
        {
            throw new NotImplementedException();
        }

        public RC6Packet ToPacket(IReadOnlyPulseSpaceUnitList irData)
        {
            var startInfo = ReadStartOfIR(irData, null, null);
            var mode = startInfo.mode;
            var bits = startInfo.bits;

            switch (mode)
            {
                case 0:
                    return _mode0Converter.ToPacket(bits);
                case 6:
                    return _mode6AConverter.ToPacket(bits);
            }

            return new RC6RawBitsPacket(mode, bits);
        }
    }
}
