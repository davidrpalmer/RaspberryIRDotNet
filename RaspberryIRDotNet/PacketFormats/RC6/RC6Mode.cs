using System;

namespace RaspberryIRDotNet.PacketFormats.RC6
{
    public struct RC6Mode : IEquatable<RC6Mode>, IEquatable<byte>, IEquatable<int>
    {
        private readonly byte _value;

        public RC6Mode(byte value)
        {
            if (value > 7)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Mode is only 3 bits, so cannot be larger than 7.");
            }
            _value = value;
        }

        public static implicit operator byte(RC6Mode mode) => mode._value;
        public static explicit operator RC6Mode(byte value) => new RC6Mode(value);

        public override string ToString() => _value.ToString();

        public override int GetHashCode() => _value.GetHashCode();

        public override bool Equals(object obj)
        {
            if (obj is RC6Mode mode)
            {
                return Equals(mode);
            }
            if (obj is byte b)
            {
                return _value.Equals(b);
            }
            if (obj is int int32)
            {
                if (int32 < byte.MinValue || int32 > byte.MaxValue)
                {
                    return false;
                }
                return _value.Equals((byte)int32);
            }
            if (obj is long int64)
            {
                if (int64 < byte.MinValue || int64 > byte.MaxValue)
                {
                    return false;
                }
                return _value.Equals((byte)int64);
            }

            return _value.Equals(obj);
        }

        public bool Equals(RC6Mode other) => _value.Equals(other._value);

        public bool Equals(byte other) => _value.Equals(other);
        public bool Equals(int other)
        {
            if (other < byte.MinValue || other > byte.MaxValue)
            {
                return false;
            }

            return _value.Equals((byte)other);
        }

        public static bool operator ==(RC6Mode a, RC6Mode b) => a.Equals(b);
        public static bool operator !=(RC6Mode a, RC6Mode b) => !a.Equals(b);

        public static bool operator ==(RC6Mode a, int b) => a.Equals(b);
        public static bool operator !=(RC6Mode a, int b) => !a.Equals(b);
        public static bool operator ==(int a, RC6Mode b) => b.Equals(a);
        public static bool operator !=(int a, RC6Mode b) => !b.Equals(a);

        public static bool operator ==(RC6Mode a, byte b) => a.Equals(b);
        public static bool operator !=(RC6Mode a, byte b) => !a.Equals(b);
        public static bool operator ==(byte a, RC6Mode b) => b.Equals(a);
        public static bool operator !=(byte a, RC6Mode b) => !b.Equals(a);
    }
}
