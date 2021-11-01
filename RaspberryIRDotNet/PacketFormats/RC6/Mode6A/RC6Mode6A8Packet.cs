using System;

namespace RaspberryIRDotNet.PacketFormats.RC6.Mode6A
{
    /// <summary>
    /// Represents the start of an RC6 Mode 6A packet with an 8 bit address. The command length and format is OEM defined so is not in this class. Use a derived type for that.
    /// </summary>
    public class RC6Mode6A8Packet : RC6Mode6APacket<byte>
    {
        public RC6Mode6A8Packet()
        {
        }

        /// <param name="ab">The double width trailer bit after the mode. FALSE=0/A, TRUE=1/B.</param>
        public RC6Mode6A8Packet(RC6Mode mode, bool ab, byte address) : base(mode, ab, address)
        {
        }

        public RC6Mode6A8Packet(byte address) : base(address)
        {
        }
    }
}
