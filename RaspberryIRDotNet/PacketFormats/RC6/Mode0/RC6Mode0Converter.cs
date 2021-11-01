using System;
using System.Collections.Generic;
using System.Linq;

namespace RaspberryIRDotNet.PacketFormats.RC6.Mode0
{
    public class RC6Mode0Converter : RC6Converter, IPulseSpacePacketConverter<RC6Mode0Packet>
    {
        public PulseSpaceUnitList ToIR(RC6Mode0Packet packet)
        {
            var payloadBits = _bitByteConverter.ToBits(packet.Address).Concat(_bitByteConverter.ToBits(packet.Command));
            return ToIR(packet.Mode, packet.Trailer, payloadBits);
        }

        public RC6Mode0Packet ToPacket(IReadOnlyPulseSpaceUnitList irData) => ToPacket(irData, true);

        public RC6Mode0Packet ToPacket(IList<bool> bits) => ToPacket(bits, new RC6Mode(0));

        public RC6Mode0Packet ToPacket(IReadOnlyPulseSpaceUnitList irData, bool throwIfWrongMode)
        {
            var startInfo = ReadStartOfIR(irData, 21, throwIfWrongMode ? new RC6Mode(0) : (RC6Mode?)null);
            var mode = startInfo.mode;
            var bits = startInfo.bits;

            return ToPacket(bits, mode);
        }

        private RC6Mode0Packet ToPacket(IList<bool> bits, RC6Mode mode)
        {
            bool toggle = bits[4];

            const int fieldSize = 8;
            byte address = _bitByteConverter.ToBytes(bits.Skip(5).Take(fieldSize).ToList()).Single();
            byte command = _bitByteConverter.ToBytes(bits.Skip(5 + fieldSize).Take(fieldSize).ToList()).Single();

            return new RC6Mode0Packet(mode, toggle, address, command);
        }
    }
}
