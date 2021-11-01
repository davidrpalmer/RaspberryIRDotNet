using System;
using System.Collections.Generic;

namespace RaspberryIRDotNet.PacketFormats
{
    /// <summary>
    /// A packet that can convert to/from a byte array. Some packets don't always use 8 bits together so can't be converted to bytes.
    /// </summary>
    public interface IByteConvertiblePacket : IIRFormatPacket
    {
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exceptions.InvalidPacketDataException">When the data does not form a valid packet. Example: wrong length.</exception>
        void FromBytes(IReadOnlyList<byte> data);

        byte[] ToBytes();
    }
}
