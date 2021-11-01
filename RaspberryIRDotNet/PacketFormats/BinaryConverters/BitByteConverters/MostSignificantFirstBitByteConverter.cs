using System;
using System.Collections.Generic;

namespace RaspberryIRDotNet.PacketFormats.BinaryConverters.BitByteConverters
{
    public class MostSignificantFirstBitByteConverter : IBitByteConverter
    {
        public byte[] ToBytes(IReadOnlyList<bool> bits)
        {
            if (bits.Count % 8 != 0)
            {
                throw new ArgumentException("Bit count must be a multiple of 8.");
            }

            byte[] result = new byte[bits.Count / 8];

            for (int byteIndex = 0, bitIndex = 0; bitIndex < bits.Count; byteIndex++, bitIndex += 8)
            {
                sbyte offset = 7; // Needs to be a signed type, not byte, because it will be decreased before checking the for condition, so will end up at -1.
                for (byte i = 0; i < 8; i++, offset--)
                {
                    if (offset < 0 || offset > 7)
                    {
                        throw new Exception("Offset is out of range.");
                    }
                    if (bits[bitIndex + i])
                    {
                        result[byteIndex] |= checked((byte)(1 << offset));
                    }
                }
            }

            return result;
        }

        public bool[] ToBits(byte oneByte) => ToBits(new byte[] { oneByte });

        public bool[] ToBits(IReadOnlyList<byte> bytes)
        {
            var bits = new bool[bytes.Count * 8];
            int bitIndex = 0;

            foreach (byte b in bytes)
            {
                for (byte i = 128; i > 0; i /= 2)
                {
                    bits[bitIndex] = (b & i) != 0;
                    bitIndex++;
                }
            }
            return bits;
        }
    }
}
