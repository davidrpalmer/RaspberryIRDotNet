using System;
using System.Collections.Generic;

namespace RaspberryIRDotNet.PacketFormats.RC6.Mode6A
{
    public class Xbox360Converter : RC6Mode6AConverter, IPulseSpacePacketConverter<Xbox360Packet>
    {
        public PulseSpaceUnitList ToIR(Xbox360Packet packet)
        {
            var payload = UInt16ToBytes(packet.Command);

            if ((payload[0] & 128) > 0)
            {
                throw new Exceptions.InvalidPacketDataException("The command is invalid (toggle bit overwritten).");
            }

            if (packet.Toggle)
            {
                payload[0] |= 128;
            }

            var payloadBits = _bitByteConverter.ToBits(payload);

            return ToIR(packet, payloadBits);
        }

        public Xbox360Packet ToPacket(IList<bool> bits)
        {
            var (basicPacket, payload) = ReadHeaderAndAddress(bits, new RC6Mode(6));
            return ToPacketInternal(basicPacket, payload);
        }

        public Xbox360Packet ToPacket(IReadOnlyPulseSpaceUnitList irData)
        {
            var (basicPacket, payload) = ReadHeaderAndAddress(irData);
            return ToPacketInternal(basicPacket, payload);
        }

        public Xbox360Packet ToPacket(RC6Mode6A16Packet basicPacket, List<bool> payload) => ToPacketInternal(basicPacket, payload);

        public Xbox360Packet ToPacket(RC6Mode6A16RawBitsPacket basicPacket) => ToPacketInternal(basicPacket, basicPacket.PayloadBits);

        private Xbox360Packet ToPacketInternal(RC6Packet basicPacket, IList<bool> payload)
        {
            if (basicPacket is RC6Mode6A16Packet basicPacket16)
            {
                if (payload.Count != 16)
                {
                    throw new Exceptions.InvalidPacketDataException("Expected a 16 bit command for an Xbox.");
                }

                List<bool> commandBits = new List<bool>(payload)
                {
                    [0] = false // mask out the toggle bit.
                };

                ushort command = UInt16FromBytes(_bitByteConverter.ToBytes(commandBits));

                return new Xbox360Packet(basicPacket16, payload[0], command);
            }

            throw new Exceptions.InvalidPacketDataException("Expected a 16 bit address for an Xbox.");
        }
    }
}
