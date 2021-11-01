using System;

namespace RaspberryIRDotNet.PacketFormats.RC6.Mode6A
{
    /// <summary>
    /// Represents the start of an RC6 Mode 6A packet with an 16 bit address. The command length and format is OEM defined so is not in this class. Use a derived type for that.
    /// </summary>
    public class RC6Mode6A16Packet : RC6Mode6APacket<ushort>
    {
        public RC6Mode6A16Packet()
        {
        }

        /// <param name="ab">The double width trailer bit after the mode. FALSE=0/A, TRUE=1/B.</param>
        public RC6Mode6A16Packet(RC6Mode mode, bool ab, ushort address) : base(mode, ab, address)
        {
        }

        public RC6Mode6A16Packet(ushort address) : base(address)
        {
        }
    }
}
