using System;

namespace RaspberryIRDotNet.PacketFormats.RC6.Mode6A
{
    public class Xbox360Packet : RC6Mode6A16Packet
    {
        public bool Toggle { get; set; }

        public ushort Command { get; set; }

        public Xbox360Packet()
        {
        }

        public Xbox360Packet(RC6Mode6A16Packet basePacket, bool toggle, ushort command) : base(basePacket.Mode, basePacket.Trailer, basePacket.Address)
        {
            Toggle = toggle;
            Command = command;
        }

        public override string ToString()
        {
            var toggleChar = Toggle ? '1' : '0';
            return $"{base.ToString()} {toggleChar} {Command}";
        }

        public override bool Equals(IIRFormatPacket other, bool ignoreVariables)
        {
            return base.Equals(other, ignoreVariables) &&
                other is Xbox360Packet otherXbox &&
                Command == otherXbox.Command &&
                (ignoreVariables || Toggle == otherXbox.Toggle);
        }
    }
}
