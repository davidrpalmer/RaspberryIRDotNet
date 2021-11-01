using System;
using System.Collections.Generic;

namespace RaspberryIRDotNet.PacketFormats.RC6.Mode6A
{
    public class RC6Mode6ARawPayloadConverter : RC6Mode6AConverter, IPulseSpacePacketConverter<RC6Mode6A8RawBitsPacket>, IPulseSpacePacketConverter<RC6Mode6A16RawBitsPacket>, IPulseSpacePacketConverter<RC6Packet>
    {
        public PulseSpaceUnitList ToIR(RC6Mode6A8RawBitsPacket packet)
        {
            return ToIR(packet, packet.PayloadBits);
        }

        public PulseSpaceUnitList ToIR(RC6Mode6A16RawBitsPacket packet)
        {
            return ToIR(packet, packet.PayloadBits);
        }

        RC6Mode6A8RawBitsPacket IPulseSpacePacketConverter<RC6Mode6A8RawBitsPacket>.ToPacket(IReadOnlyPulseSpaceUnitList irData) => To8Packet(irData);

        RC6Mode6A16RawBitsPacket IPulseSpacePacketConverter<RC6Mode6A16RawBitsPacket>.ToPacket(IReadOnlyPulseSpaceUnitList irData) => To16Packet(irData);

        /// <summary>
        /// Decode an RC6 Mode 6A packet with an 8 bit address.
        /// </summary>
        public RC6Mode6A8RawBitsPacket To8Packet(IReadOnlyPulseSpaceUnitList irData, bool throwIfWrongMode = true)
        {
            var startInfo = ReadHeaderAndAddress(irData, throwIfWrongMode);
            if (startInfo.start is RC6Mode6A8Packet basicPacket)
            {
                return new RC6Mode6A8RawBitsPacket(basicPacket, startInfo.payload.ToArray());
            }

            throw new Exceptions.InvalidPacketDataException("Not an RC6 Mode 6A with an 8 bit address packet.");
        }

        /// <summary>
        /// Decode an RC6 Mode 6A packet with a 16 bit address.
        /// </summary>
        public RC6Mode6A16RawBitsPacket To16Packet(IReadOnlyPulseSpaceUnitList irData, bool throwIfWrongMode = true)
        {
            var startInfo = ReadHeaderAndAddress(irData, throwIfWrongMode);
            if (startInfo.start is RC6Mode6A16Packet basicPacket)
            {
                return new RC6Mode6A16RawBitsPacket(basicPacket, startInfo.payload.ToArray());
            }

            throw new Exceptions.InvalidPacketDataException("Not an RC6 Mode 6A with a 16 bit address packet.");
        }

        /// <summary>
        /// Convert to the right type (8 vs 16 bit address) automatically.
        /// </summary>
        /// <param name="bits">All the bits that make up the RC6 mode 6 packet. You need to check this is mode 6 before calling this method.</param>
        /// <returns>
        /// Either <see cref="RC6Mode6A8RawBitsPacket"/> or <see cref="RC6Mode6A16RawBitsPacket"/>
        /// </returns>
        public RC6Packet ToPacket(IList<bool> bits)
        {
            var (basicPacket, payload) = ReadHeaderAndAddress(bits, new RC6Mode(6), true);
            return ToPacket(basicPacket, payload);
        }

        /// <summary>
        /// Convert to the right type (8 vs 16 bit address) automatically.
        /// </summary>
        /// <returns>
        /// Either <see cref="RC6Mode6A8RawBitsPacket"/> or <see cref="RC6Mode6A16RawBitsPacket"/>
        /// </returns>
        public RC6Packet ToPacket(IReadOnlyPulseSpaceUnitList irData)
        {
            var (basicPacket, payload) = ReadHeaderAndAddress(irData, true);
            return ToPacket(basicPacket, payload);
        }

        private RC6Packet ToPacket(RC6Packet basicPacket, List<bool> payload)
        {
            if (basicPacket is RC6Mode6A8Packet basic8Packet)
            {
                return new RC6Mode6A8RawBitsPacket(basic8Packet, payload.ToArray());
            }
            else if (basicPacket is RC6Mode6A16Packet basic16Packet)
            {
                return new RC6Mode6A16RawBitsPacket(basic16Packet, payload.ToArray());
            }
            else
            {
                throw new Exception("Unexpected packet type.");
            }
        }

        PulseSpaceUnitList IPulseSpacePacketConverter<RC6Packet>.ToIR(RC6Packet packet)
        {
            if (packet is RC6Mode6A8RawBitsPacket packet8)
            {
                return ToIR(packet8);
            }
            else if (packet is RC6Mode6A16RawBitsPacket packet16)
            {
                return ToIR(packet16);
            }

            throw new ArgumentException("Cannot convert this type of packet.");
        }
    }
}
