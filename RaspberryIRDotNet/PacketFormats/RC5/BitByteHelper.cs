using System;

namespace RaspberryIRDotNet.PacketFormats.RC5
{
    static class BitByteHelper
    {
        public static int GetMaxNumberForBits(int bitCount)
        {
            if (bitCount < 0 || bitCount > 8)
            {
                throw new ArgumentOutOfRangeException(nameof(bitCount), bitCount, "Bit count must be 0-8.");
            }
            return byte.MaxValue >> (8 - bitCount);
        }
    }
}
