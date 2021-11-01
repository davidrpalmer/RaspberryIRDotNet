using System;
using System.Collections.Generic;

namespace RaspberryIRDotNet.PacketFormats.NEC
{
    /// <summary>
    /// NEC packet with an 16bit address and redundancy for only the command.
    /// </summary>
    public class NecExtendedPacket : IValidateableIRPacket, IByteConvertiblePacket, IIRFormatPacket
    {
        public ushort Address { get; set; }

        public byte Command { get; set; }
        public byte Command_Inverse { get; set; }

        public NecExtendedPacket()
        {
        }

        public NecExtendedPacket(ushort address, byte command)
        {
            Address = address;
            Command = command;
            SetCalculatedValues();
        }

        public NecExtendedPacket(IReadOnlyList<byte> data)
        {
            FromBytes(data);
        }

        public void FromBytes(IReadOnlyList<byte> data)
        {
            if (data == null) { throw new ArgumentNullException(); }
            if (data.Count != 4) { throw new Exceptions.InvalidPacketDataException($"Data length must be 4, but was {data.Count}."); }

            Address = data[0];
            Address |= checked((ushort)(data[1] << 8));
            Command = data[2];
            Command_Inverse = data[3];
        }

        public byte[] ToBytes()
        {
            byte address1, address2;

            checked
            {
                address1 = (byte)(Address & 0x00FF);
                address2 = (byte)((Address & 0xFF00) >> 8);
            }

            return new byte[]
            {
                address1,
                address2,
                Command,
                Command_Inverse
            };
        }

        public override string ToString()
        {
            string valid = Validate() ? string.Empty : " (invalid)";
            return $"{Address:X4} {Command:X2}{valid}";
        }

        /// <summary>
        /// Calculate and set the inverse command value based on the normal value.
        /// </summary>
        public void SetCalculatedValues()
        {
            Command_Inverse = unchecked((byte)~Command);
        }

        /// <summary>
        /// Check the inverted command value matches the normal value.
        /// </summary>
        /// <returns>
        /// TRUE if match OK, FALSE if not.
        /// </returns>
        public bool Validate()
        {
            if (unchecked((byte)~Command) != Command_Inverse)
            {
                return false;
            }

            return true;
        }

        public bool Equals(IIRFormatPacket other, bool ignoreVariables)
        {
            if (other is NecExtendedPacket otherNecX)
            {
                return Address == otherNecX.Address &&
                    Command == otherNecX.Command &&
                    Command_Inverse == otherNecX.Command_Inverse;
            }

            if (other is NecBasicPacket otherNecB)
            {
                return Address == otherNecB.Address &&
                    Command == otherNecB.Command &&
                    Command_Inverse == otherNecB.Command_Inverse;
            }

            return false;
        }
    }
}
