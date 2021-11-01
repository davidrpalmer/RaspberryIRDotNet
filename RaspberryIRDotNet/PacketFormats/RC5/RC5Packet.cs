using System;

namespace RaspberryIRDotNet.PacketFormats.RC5
{
    public abstract class RC5Packet : IIRFormatPacket
    {
        protected abstract int AddressBitCount { get; }
        protected abstract int CommandBitCount { get; }

        public bool Toggle { get; set; }

        public byte Address
        {
            get => _address;
            set
            {
                int maxValue = BitByteHelper.GetMaxNumberForBits(AddressBitCount);
                if (value > maxValue)
                {
                    throw new ArgumentOutOfRangeException(nameof(Address), value, $"The address is only {AddressBitCount} bits so can be no more than {maxValue}.");
                }
                _address = value;
            }
        }
        private byte _address;

        public byte Command
        {
            get => _command;
            set
            {
                int maxValue = BitByteHelper.GetMaxNumberForBits(CommandBitCount);
                if (value > maxValue)
                {
                    throw new ArgumentOutOfRangeException(nameof(Command), value, $"The command is only {CommandBitCount} bits so can be no more than {maxValue}.");
                }
                _command = value;
            }
        }
        protected byte _command;

        public override string ToString()
        {
            char toggleChar = Toggle ? '1' : '0';
            return $"{toggleChar} {Address} {Command}";
        }

        public bool Equals(IIRFormatPacket other, bool ignoreVariables)
        {
            return other is RC5Packet otherRC5 &&
                Address == otherRC5.Address &&
                Command == otherRC5.Command &&
                (ignoreVariables || Toggle == otherRC5.Toggle);
        }
    }
}
