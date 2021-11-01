using System;
using System.Linq;

namespace RaspberryIRDotNet.PacketFormats.RC6.Mode6A
{
    public class RC6Mode6A8RawBitsPacket : RC6Mode6A8Packet
    {
        public bool[] PayloadBits { get; set; }

        public RC6Mode6A8RawBitsPacket()
        {
        }

        public RC6Mode6A8RawBitsPacket(RC6Mode6A8Packet basicPacket, bool[] payloadBits) : base(basicPacket.Mode, basicPacket.Trailer, basicPacket.Address)
        {
            PayloadBits = payloadBits;
        }

        public RC6Mode6A8RawBitsPacket(byte address, bool[] payloadBits) : base(address)
        {
            PayloadBits = payloadBits;
        }

        public override string ToString()
        {
            string payloadStr = new string(PayloadBits.Select((b, i) => b ? '1' : '0').ToArray());
            return $"{base.ToString()} {payloadStr}";
        }

        public override bool Equals(IIRFormatPacket other, bool ignoreVariables)
        {
            return base.Equals(other, ignoreVariables) &&
                other is RC6Mode6A16RawBitsPacket otherRC6 &&
                AreArraysEqual(PayloadBits, otherRC6.PayloadBits);
        }
    }
}
