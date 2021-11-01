using System;

namespace RaspberryIRDotNet.PacketFormats.RC6.Mode6A
{
    /// <summary>
    /// Typically this class is not used directly, see <see cref="RC6Mode6A8Packet"/> and <see cref="RC6Mode6A16Packet"/>.
    /// </summary>
    /// <typeparam name="TAddress">The type of the address. In the RC6 Mode 6A standard this can be either <see cref="byte"/> or <see cref="ushort"/>.</typeparam>
    public class RC6Mode6APacket<TAddress> : RC6Packet where TAddress : IEquatable<TAddress>
    {
        public TAddress Address { get; set; }

        /// <summary>
        /// The sub mode is indicated as A or B depending on if the trailer bit is a 0 (A) or a 1 (B).
        /// </summary>
        public char SubMode => Trailer ? 'B' : 'A';

        public RC6Mode6APacket()
        {
        }

        /// <param name="ab">The double width trailer bit after the mode. FALSE=0/A, TRUE=1/B.</param>
        public RC6Mode6APacket(RC6Mode mode, bool ab, TAddress address)
        {
            Mode = mode;
            Trailer = ab;
            Address = address;
        }

        public RC6Mode6APacket(TAddress address) : this(new RC6Mode(6), false, address)
        {
        }

        public override string ToString()
        {
            return $"{Mode}{SubMode} {Address}";
        }

        public override bool Equals(IIRFormatPacket other, bool ignoreVariables)
        {
            return other is RC6Mode6APacket<TAddress> otherRC6 &&
                Mode == otherRC6.Mode &&
                Trailer == otherRC6.Trailer &&
                Address.Equals(otherRC6.Address);
        }
    }
}
