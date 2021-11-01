using System;
using System.Collections.Generic;
using System.Linq;

namespace RaspberryIRDotNet.PacketFormats.RC6
{
    public class RC6RawBitsPacket : RC6Packet
    {
        public bool[] DataBits { get; set; }

        public RC6RawBitsPacket()
        {
        }

        public RC6RawBitsPacket(RC6Mode mode, IList<bool> dataBits)
        {
            //dataBits[0], dataBits[1], dataBits[2] are the mode. But that has already been decoded.
            Mode = mode;
            Trailer = dataBits[3];
            DataBits = dataBits.Skip(4).ToArray();
        }

        public override string ToString()
        {
            char trailerChar = Trailer ? '1' : '0';
            string dataStr = new string(DataBits.Select((b, i) => b ? '1' : '0').ToArray());
            return $"{base.ToString()}  {trailerChar} {dataStr}";
        }

        public override bool Equals(IIRFormatPacket other, bool ignoreVariables)
        {
            return other is RC6RawBitsPacket otherRC6 && Mode == otherRC6.Mode && Trailer == otherRC6.Trailer && AreArraysEqual(DataBits, otherRC6.DataBits);
        }
    }
}
