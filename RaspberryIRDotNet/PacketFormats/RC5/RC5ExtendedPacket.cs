using System;

namespace RaspberryIRDotNet.PacketFormats.RC5
{
    /// <summary>
    /// RC5X is the same as RC5, but the command has an extra bit.
    /// </summary>
    public class RC5ExtendedPacket : RC5Packet
    {
        protected override int AddressBitCount => 5;
        protected override int CommandBitCount => 7;

        public override string ToString()
        {
            return $"RC5X {base.ToString()}";
        }

        public RC5ExtendedPacket()
        {
        }

        public RC5ExtendedPacket(bool toggle, byte address, byte command)
        {
            Toggle = toggle;
            Address = address;
            Command = command;
        }
    }
}
