using System;
using System.Collections.Generic;

namespace RaspberryIRDotNet.PacketFormats.BinaryConverters.BitByteConverters
{
    public interface IBitByteConverter
    {
        bool[] ToBits(IReadOnlyList<byte> bytes);

        bool[] ToBits(byte oneByte);

        byte[] ToBytes(IReadOnlyList<bool> bits);
    }
}
