using System;
using System.Collections.Generic;
using System.Linq;

namespace RaspberryIRDotNet.PacketFormats.RC6.Mode6A
{
    public abstract class RC6Mode6AConverter : RC6Converter
    {
        /// <summary>
        /// Read the RC6 header and the address and separate it out from the payload bits that come after.
        /// </summary>
        /// <param name="throwIfWrongMode">Throw an exception if not a mode 6A packet.</param>
        /// <returns>
        /// start - cast this to a more specific type. Either <see cref="RC6Mode6A8Packet"/> or <see cref="RC6Mode6A16Packet"/>.
        /// payload - the decoded payload bits. You need to parse these into whatever your OEM format is.
        /// </returns>
        protected (RC6Packet start, List<bool> payload) ReadHeaderAndAddress(IReadOnlyPulseSpaceUnitList irData, bool throwIfWrongMode = true)
        {
            var (bits, mode) = ReadStartOfIR(irData, null, throwIfWrongMode ? new RC6Mode(6) : (RC6Mode?)null);

            return ReadHeaderAndAddress(bits, mode, throwIfWrongMode);
        }

        /// <summary>
        /// Read the RC6 header and the address and separate it out from the payload bits that come after.
        /// </summary>
        /// <param name="throwIfWrongMode">Throw an exception if not a mode 6A packet.</param>
        /// <returns>
        /// start - cast this to a more specific type. Either <see cref="RC6Mode6A8Packet"/> or <see cref="RC6Mode6A16Packet"/>.
        /// payload - the decoded payload bits. You need to parse these into whatever your OEM format is.
        /// </returns>
        protected (RC6Packet start, List<bool> payload) ReadHeaderAndAddress(IList<bool> bits, RC6Mode mode, bool throwIfWrongMode = true)
        {
            if (bits.Count < 13)
            {
                throw new Exceptions.InvalidPacketDataException("Not enough bits to be a valid RC6 Mode 6 packet.");
            }

            bool trailer = bits[4];

            if (trailer && throwIfWrongMode)
            {
                throw new Exceptions.InvalidPacketDataException("Expected an RC6 mode 6A packet, but got mode 6B.");
            }

            int addressBitCount, addressByteCount;
            if (!bits[5]) // 8 bit address
            {
                addressBitCount = 8;
                addressByteCount = 1;
            }
            else // 16 bit address
            {
                addressBitCount = 16;
                addressByteCount = 2;
            }

            var addressBits = bits.Skip(5).Take(addressBitCount).ToList();
            if (addressBits.Count != addressBitCount)
            {
                throw new Exceptions.InvalidPacketDataException("Not enough bits for the address.");
            }
            var addressBytes = _bitByteConverter.ToBytes(addressBits);
            var payloadBits = bits.Skip(5 + addressBitCount).ToList();

            if (addressBytes.Length != addressByteCount)
            {
                throw new Exception($"Expecting a {addressBitCount} bit address.");
            }

            RC6Packet start;

            if (addressByteCount == 1)
            {
                start = new RC6Mode6A8Packet(mode, trailer, addressBytes[0]);
            }
            else if (addressByteCount == 2)
            {
                ushort address = UInt16FromBytes(addressBytes);

                start = new RC6Mode6A16Packet(mode, trailer, address);
            }
            else
            {
                throw new Exception();
            }

            return (start, payloadBits);
        }

        protected PulseSpaceUnitList ToIR(RC6Mode6A8Packet packet, bool[] payloadBits)
        {
            bool[] addressBits = _bitByteConverter.ToBits(packet.Address);

            return ToIR(packet.Mode, packet.Trailer, addressBits.Concat(payloadBits));
        }

        protected PulseSpaceUnitList ToIR(RC6Mode6A16Packet packet, bool[] payloadBits)
        {
            bool[] addressBits = _bitByteConverter.ToBits(UInt16ToBytes(packet.Address));

            return ToIR(packet.Mode, packet.Trailer, addressBits.Concat(payloadBits));
        }
    }
}
