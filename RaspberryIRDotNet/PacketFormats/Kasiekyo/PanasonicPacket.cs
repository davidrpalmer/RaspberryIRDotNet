using System;
using System.Collections.Generic;

namespace RaspberryIRDotNet.PacketFormats.Kasiekyo
{
    /// <summary>
    /// Panasonic variation of Kasiekyo.
    /// </summary>
    public class PanasonicPacket : IIRFormatPacket, IValidateableIRPacket, IByteConvertiblePacket
    {
        public ushort Manufacturer { get; set; }

        public byte Device { get; set; }

        public byte SubDevice { get; set; }

        public byte Function { get; set; }

        public byte Hash { get; set; }

        public PanasonicPacket()
        {
        }

        public PanasonicPacket(ushort manufacturer, byte device, byte subDevice, byte function)
        {
            Manufacturer = manufacturer;
            Device = device;
            SubDevice = subDevice;
            Function = function;
            SetCalculatedValues();
        }

        public PanasonicPacket(IReadOnlyList<byte> data)
        {
            FromBytes(data);
        }

        public void FromBytes(IReadOnlyList<byte> data)
        {
            ArgumentNullException.ThrowIfNull(data);
            if (data.Count != 6) { throw new Exceptions.InvalidPacketDataException($"Data length must be 6, but was {data.Count}."); }

            Manufacturer = data[0];
            Manufacturer |= checked((ushort)(data[1] << 8));
            Device = data[2];
            SubDevice = data[3];
            Function = data[4];
            Hash = data[5];
        }

        public byte[] ToBytes()
        {
            byte man1, man2;

            checked
            {
                man1 = (byte)(Manufacturer & 0x00FF);
                man2 = (byte)((Manufacturer & 0xFF00) >> 8);
            }

            return
            [
                man1,
                man2,
                Device,
                SubDevice,
                Function,
                Hash
            ];
        }

        public void SetCalculatedValues()
        {
            Hash = checked((byte)(Device ^ SubDevice ^ Function));
        }

        /// <summary>
        /// Check the hashed value is correct.
        /// </summary>
        /// <returns>
        /// TRUE if match OK, FALSE if not.
        /// </returns>
        public bool Validate()
        {
            return Hash == checked((byte)(Device ^ SubDevice ^ Function));
        }

        public override string ToString()
        {
            string valid = Validate() ? string.Empty : " (invalid)";
            return $"{Manufacturer:X4} [{Device:X2} {SubDevice:X2}] {Function:X2}{valid}";
        }

        public bool Equals(IIRFormatPacket other, bool ignoreVariables)
        {
            if (other is PanasonicPacket otherPanasonic)
            {
                return
                    Manufacturer == otherPanasonic.Manufacturer &&
                    Device == otherPanasonic.Device &&
                    SubDevice == otherPanasonic.SubDevice &&
                    Function == otherPanasonic.Function &&
                    Hash == otherPanasonic.Hash;
            }

            return false;
        }
    }
}
