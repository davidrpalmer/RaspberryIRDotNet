using System;
using System.Collections.Generic;

namespace RaspberryIRDotNet.PacketFormats.NEC
{
    /// <summary>
    /// NEC packet with an 8bit address and redundancy for both the address and the command.
    /// </summary>
    public class NecBasicPacket : IValidateableIRPacket, IByteConvertiblePacket, IIRFormatPacket
    {
        public byte Address { get; set; }
        public byte Address_Inverse { get; set; }

        public byte Command { get; set; }
        public byte Command_Inverse { get; set; }

        public NecBasicPacket()
        {
        }

        public NecBasicPacket(byte address, byte command)
        {
            Address = address;
            Command = command;
            SetCalculatedValues();
        }

        public NecBasicPacket(IReadOnlyList<byte> data)
        {
            FromBytes(data);
        }

        public void FromBytes(IReadOnlyList<byte> data)
        {
            ArgumentNullException.ThrowIfNull(data);
            if (data.Count != 4) { throw new Exceptions.InvalidPacketDataException($"Data length must be 4, but was {data.Count}."); }

            Address = data[0];
            Address_Inverse = data[1];
            Command = data[2];
            Command_Inverse = data[3];
        }

        public byte[] ToBytes()
        {
            return
            [
                Address,
                Address_Inverse,
                Command,
                Command_Inverse
            ];
        }

        public override string ToString()
        {
            string valid = Validate() ? string.Empty : " (invalid)";
            return $"{Address:X2} {Command:X2}{valid}";
        }

        /// <summary>
        /// Calculate and set the inverse values based on the normal values.
        /// </summary>
        public void SetCalculatedValues()
        {
            Address_Inverse = unchecked((byte)~Address);
            Command_Inverse = unchecked((byte)~Command);
        }

        public bool Validate()
        {
            if (unchecked((byte)~Address) != Address_Inverse)
            {
                return false;
            }

            if (unchecked((byte)~Command) != Command_Inverse)
            {
                return false;
            }

            return true;
        }

        public bool Equals(IIRFormatPacket other, bool ignoreVariables)
        {
            if (other is NecBasicPacket otherNecB)
            {
                return Address == otherNecB.Address &&
                    Address_Inverse == otherNecB.Address_Inverse &&
                    Command == otherNecB.Command &&
                    Command_Inverse == otherNecB.Command_Inverse;
            }

            if (other is NecExtendedPacket otherNecX)
            {
                return Address == otherNecX.Address &&
                    Command == otherNecX.Command &&
                    Command_Inverse == otherNecX.Command_Inverse;
            }

            return false;
        }
    }
}
