using System;

namespace RaspberryIRDotNet.PacketFormats.RC6
{
    public class RC6RawBitsConverter : RC6Converter, IPulseSpacePacketConverter<RC6RawBitsPacket>
    {
        public PulseSpaceUnitList ToIR(RC6RawBitsPacket packet)
        {
            return ToIR(packet.Mode, packet.Trailer, packet.DataBits);
        }

        public RC6RawBitsPacket ToPacket(IReadOnlyPulseSpaceUnitList irData)
        {
            var startInfo = ReadStartOfIR(irData, null, null);
            var mode = startInfo.mode;
            var bits = startInfo.bits;

            return new RC6RawBitsPacket(mode, bits);
        }
    }
}
