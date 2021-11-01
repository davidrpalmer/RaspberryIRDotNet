using System;

namespace RaspberryIRDotNet.PacketFormats.RC5
{
    /// <summary>
    /// The classic RC5 (not RC5X) format.
    /// </summary>
    public class RC5BasicPacket : RC5Packet
    {
        protected override int AddressBitCount => 5;
        protected override int CommandBitCount => 6;

        public override string ToString()
        {
            return $"RC5 {base.ToString()}";
        }

        public RC5BasicPacket()
        {
        }

        public RC5BasicPacket(bool toggle, byte address, byte command)
        {
            Toggle = toggle;
            Address = address;
            Command = command;
        }
    }
}
