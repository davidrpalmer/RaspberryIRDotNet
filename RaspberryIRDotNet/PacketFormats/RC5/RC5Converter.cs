using System;
using System.Collections.Generic;
using System.Linq;
using RaspberryIRDotNet.PacketFormats.BinaryConverters.BitByteConverters;

namespace RaspberryIRDotNet.PacketFormats.RC5
{
    public class RC5Converter : IPulseSpacePacketConverter<RC5BasicPacket>, IPulseSpacePacketConverter<RC5ExtendedPacket>
    {
        private readonly PointInTimeSampler _pointInTimeSampler = new PointInTimeSampler();
        protected readonly MostSignificantFirstBitByteConverter _bitByteConverter = new MostSignificantFirstBitByteConverter();

        public static int RC5StandardUnitDurationMicrosecs { get; } = 889;

        public static int RC5StandardFrequency { get; } = 36000;

        private bool[] ByteToBits(byte from, int bitCount)
        {
            int maxValue = BitByteHelper.GetMaxNumberForBits(bitCount);

            if (from > maxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(from), from, $"The value {from} is too large to fit into {bitCount} bits.");
            }

            bool[] bits = _bitByteConverter.ToBits(from);

            bool[] result = new bool[bitCount];
            Array.Copy(bits, 8 - bitCount, result, 0, bitCount);
            return result;
        }

        private byte BitsToByte(List<bool> bits, int startAt, int length)
        {
            if (length > 8)
            {
                throw new ArgumentException("Can only use 8 bits.");
            }

            bool[] paddedBits = new bool[8];
            bits.CopyTo(startAt, paddedBits, 8 - length, length);

            var result = _bitByteConverter.ToBytes(paddedBits);
            if (result.Length != 1)
            {
                throw new Exception("Unexpected byte count.");
            }

            return result[0];
        }

        public PulseSpaceUnitList ToIR(RC5BasicPacket packet)
        {
            const int addressBitCount = 5;
            const int commandBitCount = 6;

            var allBits = new bool[] { true, packet.Toggle }
                .Concat(ByteToBits(packet.Address, addressBitCount))
                .Concat(ByteToBits(packet.Command, commandBitCount));

            return ToIR(allBits);
        }


        public PulseSpaceUnitList ToIR(RC5ExtendedPacket packet)
        {
            const int addressBitCount = 5;

            var commandBits = ByteToBits(packet.Command, 7);

            var allBits = new bool[] { !commandBits[0], packet.Toggle }
                .Concat(ByteToBits(packet.Address, addressBitCount))
                .Concat(commandBits.Skip(1));

            return ToIR(allBits);
        }

        private PulseSpaceUnitList ToIR(IEnumerable<bool> allBits)
        {
            var result = new PulseSpaceUnitList();

            int bitIndex = 0;
            bool makingPulse = true; // Indicates PULSE (true) or SPACE (false).
            byte count = 1; // How many units have been the same type in a row (PULSE/SPACE). Reset when previous changes.
            foreach (var bit in allBits)
            {
                if (bit) // 1 (SPACE,PULSE)
                {
                    if (makingPulse)
                    {
                        result.Add(count); // Add the data before as one long PULSE.

                        result.Add(1); // Add a SPACE
                        // Start a new PULSE....
                        makingPulse = true;
                        count = 1;
                    }
                    else
                    {
                        count += 1;
                        result.Add(count); // Add the data before and here as one long SPACE.
                        // Now start a new PULSE....
                        makingPulse = true;
                        count = 1;
                    }
                }
                else // 0 (PULSE,SPACE)
                {
                    if (makingPulse)
                    {
                        count += 1;
                        result.Add(count); // Add the data before and here as one long PULSE.
                        // Now start a new SPACE....
                        makingPulse = false;
                        count = 1;
                    }
                    else
                    {
                        result.Add(count); // Add the data before as one long SPACE.

                        result.Add(1); // Add a PULSE
                        // Start a new SPACE....
                        makingPulse = false;
                        count = 1;
                    }
                }
                bitIndex++;
            }
            if (makingPulse)
            {
                result.Add(count);
            }

            return result;
        }

        RC5BasicPacket IPulseSpacePacketConverter<RC5BasicPacket>.ToPacket(IReadOnlyPulseSpaceUnitList irData) => ToRC5BasicPacket(irData);
        RC5ExtendedPacket IPulseSpacePacketConverter<RC5ExtendedPacket>.ToPacket(IReadOnlyPulseSpaceUnitList irData) => ToRC5ExtendedPacket(irData);

        public RC5BasicPacket ToRC5BasicPacket(IReadOnlyPulseSpaceUnitList irData)
        {
            var highLowSamples = _pointInTimeSampler.FromIR(irData);
            var bits = HighLowSamplesToBits(highLowSamples);

            if (bits.Count != 13)
            {
                throw new Exceptions.InvalidPacketDataException($"Invalid data length ({bits.Count} bits) for RC5 or RC5X.");
            }

            if (!bits[0])
            {
                throw new Exceptions.InvalidPacketDataException("The start of the RC5 packet is invalid.");
            }

            var packet = new RC5BasicPacket();
            packet.Toggle = bits[1];
            const int addressStartAt = 2;
            const int addressBitCount = 5;
            const int commandBitCount = 6;
            packet.Address = BitsToByte(bits, addressStartAt, addressBitCount);
            packet.Command = BitsToByte(bits, addressStartAt + addressBitCount, commandBitCount);
            return packet;
        }

        public RC5ExtendedPacket ToRC5ExtendedPacket(IReadOnlyPulseSpaceUnitList irData)
        {
            var highLowSamples = _pointInTimeSampler.FromIR(irData);
            var bits = HighLowSamplesToBits(highLowSamples);

            if (bits.Count != 13)
            {
                throw new Exceptions.InvalidPacketDataException($"Invalid data length ({bits.Count} bits) for RC5 or RC5X.");
            }

            var packet = new RC5ExtendedPacket();
            packet.Toggle = bits[1];
            const int addressStartAt = 2;
            const int addressBitCount = 5;
            packet.Address = BitsToByte(bits, addressStartAt, addressBitCount);

            // The command is the 2nd start bit (which we actually have as the first bit since we ignore the 1st bit) then 6 bits from the end of the IR signal.
            List<bool> commandBits = new List<bool>(bits.GetRange(addressStartAt + addressBitCount, 6));
            commandBits.Insert(0, !bits[0]); // Must invert the first bit.
            packet.Command = BitsToByte(commandBits, 0, commandBits.Count);

            return packet;
        }

        protected List<bool> HighLowSamplesToBits(IReadOnlyList<bool> highLowSamples)
        {
            List<bool> bits = new List<bool>();

            for (int i = 1; i < highLowSamples.Count; i += 2)
            {
                var first = highLowSamples[i];
                bool? second = i >= (highLowSamples.Count - 1) ? (bool?)null : highLowSamples[i + 1];

                if (!first && second == true)
                {
                    bits.Add(true);
                }
                else if (first && second != true)
                {
                    bits.Add(false);
                }
                else
                {
                    string type1 = first ? "PULSE" : "FALSE";
                    string type2 = !second.HasValue ? "NULL" : second.Value ? "PULSE" : "FALSE";
                    throw new Exceptions.InvalidPacketDataException($"Expected a PULSE/SPACE pair. But got {type1},{type2}.");
                }
            }

            return bits;
        }
    }
}
