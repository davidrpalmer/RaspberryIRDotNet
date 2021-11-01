using System;

namespace RaspberryIRDotNet.PacketFormats.RC6.Mode0
{
    public class RC6Mode0Packet : RC6Packet
    {
        public bool Toggle => Trailer;

        public byte Address { get; set; }

        public byte Command { get; set; }

        public RC6Mode0Packet()
        {
        }

        public RC6Mode0Packet(RC6Mode mode, bool toggle, byte address, byte command)
        {
            Mode = mode;
            Trailer = toggle;
            Address = address;
            Command = command;
        }

        public RC6Mode0Packet(bool toggle, byte address, byte command) : this(new RC6Mode(0), toggle, address, command)
        {
        }

        public override string ToString()
        {
            char toggle = Toggle ? 'Y' : 'N';
            return $"{Mode} {toggle} {Address} {Command}";
        }

        public override bool Equals(IIRFormatPacket other, bool ignoreVariables)
        {
            return other is RC6Mode0Packet otherRC6 &&
                Mode == otherRC6.Mode &&
                Address == otherRC6.Address &&
                Command == otherRC6.Command &&
                (ignoreVariables || Toggle == otherRC6.Toggle);
        }
    }
}
