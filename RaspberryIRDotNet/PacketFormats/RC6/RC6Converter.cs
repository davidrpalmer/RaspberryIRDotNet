using System;
using System.Collections.Generic;
using System.Linq;
using RaspberryIRDotNet.PacketFormats.BinaryConverters.BitByteConverters;

namespace RaspberryIRDotNet.PacketFormats.RC6
{
    public abstract class RC6Converter
    {
        /// <summary>
        /// The lead-in pattern.
        /// </summary>
        /// <remarks>
        /// This must have an even number of entries, otherwise the PULSE/SPACE offset will be wrong.
        /// </remarks>
        private static readonly IReadOnlyPulseSpaceUnitList _rc6LeadInPattern = new PulseSpaceUnitList(new byte[] { 6, 2 });

        public static int RC6StandardUnitDurationMicrosecs { get; } = 444;

        public static int RC6StandardFrequency { get; } = 36000;


        private readonly PointInTimeSampler _pointInTimeSampler = new PointInTimeSampler();
        protected readonly MostSignificantFirstBitByteConverter _bitByteConverter = new MostSignificantFirstBitByteConverter();

        protected PulseSpaceUnitList ToIR(RC6Mode mode, bool trailer, IEnumerable<bool> payloadBits)
        {
            var modeBits = _bitByteConverter.ToBits(mode).Skip(5);

            var trailerBit = new bool[] { trailer };

            var allBits = modeBits.Concat(trailerBit).Concat(payloadBits);

            var result = new PulseSpaceUnitList(_rc6LeadInPattern);

            result.Add(1); // Add the PULSE of the first bit that is always 1. The SPACE that follows it will be begun before we start the loop, but won't end until the loop finds a PULSE.

            int bitIndex = 0;
            bool makingPulse = false; // Indicates PULSE (true) or SPACE (false).
            byte count = 1; // How many units have been the same type in a row (PULSE/SPACE). Reset when previous changes.
            foreach (var bit in allBits)
            {
                byte unitSize = (bitIndex == 3) ? (byte)2 : (byte)1; // The unit size is normally 1, except for bit 4 (the trailer bit) which is double width.

                if (!bit) // 0 (SPACE,PULSE)
                {
                    if (makingPulse)
                    {
                        result.Add(count); // Add the data before as one long PULSE.

                        result.Add(unitSize); // Add a SPACE
                        // Start a new PULSE....
                        makingPulse = true;
                        count = unitSize;
                    }
                    else
                    {
                        count += unitSize;
                        result.Add(count); // Add the data before and here as one long SPACE.
                        // Now start a new PULSE....
                        makingPulse = true;
                        count = unitSize;
                    }
                }
                else // 1 (PULSE,SPACE)
                {
                    if (makingPulse)
                    {
                        count += unitSize;
                        result.Add(count); // Add the data before and here as one long PULSE.
                        // Now start a new SPACE....
                        makingPulse = false;
                        count = unitSize;
                    }
                    else
                    {
                        result.Add(count); // Add the data before as one long SPACE.

                        result.Add(unitSize); // Add a PULSE
                        // Start a new SPACE....
                        makingPulse = false;
                        count = unitSize;
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

        protected (List<bool> bits, RC6Mode mode) ReadStartOfIR(IReadOnlyPulseSpaceUnitList irData, int? expectedBitCount, RC6Mode? expectedMode)
        {
            int startAtIndex = _rc6LeadInPattern.Count;

            var highLowSamples = _pointInTimeSampler.FromIR(irData, startAtIndex);
            var bits = HighLowSamplesToBits(highLowSamples);

            if (bits.Count < 8) // Check we have enough to reach the mode.
            {
                throw new Exceptions.InvalidPacketDataException("Not enough bits decoded from the IR to be a valid RC6 packet.");
            }

            if (!bits[0])
            {
                throw new Exceptions.InvalidPacketDataException("Expected first bit to be 1, but it isn't.");
            }

            var mode = ReadMode(bits, 1);

            if (expectedMode.HasValue && mode != expectedMode)
            {
                throw new Exceptions.InvalidPacketDataException($"This looks like RC6 IR data, but is mode {mode}. Expected mode {expectedMode}.");
            }

            if (expectedBitCount.HasValue && bits.Count != expectedBitCount.Value)
            {
                throw new Exceptions.InvalidPacketDataException($"Expected {expectedBitCount.Value} bits from the IR (excluding lead-in), but got {bits.Count}.");
            }

            return (bits, mode);
        }


        /// <summary>
        /// Read the mode from the bit array. If the bit array is too long then the extra data will be ignored.
        /// </summary>
        protected RC6Mode ReadMode(IReadOnlyList<bool> bits, int startAt)
        {
            int count = bits.Count - startAt;
            if (count < 3) { throw new ArgumentException($"Mode is 3 bits, but there are only {count} bits."); }

            bool[] wholeByte = new bool[8];
            wholeByte[5] = bits[startAt];
            wholeByte[6] = bits[startAt + 1];
            wholeByte[7] = bits[startAt + 2];

            var bytes = _bitByteConverter.ToBytes(wholeByte);
            if (bytes.Length != 1)
            {
                throw new Exception("Expected a single byte.");
            }
            return new RC6Mode(bytes[0]);
        }

        /// <summary>
        /// Convert high/low samples into bits.
        /// </summary>
        /// <param name="highLowSamples">Each element in this array should be one unit. TRUE if this unit was during a PULSE, false if during a SPACE.</param>
        /// <remarks>
        /// This assumes that the double width bit is the same for all RC6 modes (it is for modes 0 and 6).
        /// </remarks>
        protected List<bool> HighLowSamplesToBits(IReadOnlyList<bool> highLowSamples)
        {
            if (highLowSamples.Count < 12) // Make sure we have at least up to the double width bit.
            {
                throw new Exceptions.InvalidPacketDataException("Not enough IR samples for RC6.");
            }

            List<bool> bits = new List<bool>();

            for (int i = 0; i < highLowSamples.Count; i += 2)
            {
                if (i == 8)
                {
                    var firstA = highLowSamples[i];
                    var firstB = highLowSamples[i + 1];
                    var secondA = highLowSamples[i + 2];
                    var secondB = highLowSamples[i + 3];

                    if (firstA != firstB)
                    {
                        throw new Exceptions.InvalidPacketDataException("Double width bit units 1 & 2 are not the same.");
                    }
                    if (secondA != secondB)
                    {
                        throw new Exceptions.InvalidPacketDataException("Double width bit units 3 & 4 are not the same.");
                    }

                    if (!firstA && secondA)
                    {
                        bits.Add(false);
                    }
                    else if (firstA && !secondA)
                    {
                        bits.Add(true);
                    }
                    else
                    {
                        string type = firstA ? "PULSE" : "FALSE";
                        throw new Exceptions.InvalidPacketDataException($"Double width bit units are all {type}.");
                    }
                    i += 2;
                }
                else if (i == 9 || i == 10 || i == 11)
                {
                    throw new Exception("Should skip these (part of the double width bit).");
                }
                else
                {
                    var first = highLowSamples[i];
                    bool? second = i >= (highLowSamples.Count - 1) ? (bool?)null : highLowSamples[i + 1];

                    if (!first && second == true)
                    {
                        bits.Add(false);
                    }
                    else if (first && second != true)
                    {
                        bits.Add(true);
                    }
                    else
                    {
                        string type1 = first ? "PULSE" : "SPACE";
                        string type2 = !second.HasValue ? "NULL" : second.Value ? "PULSE" : "SPACE";
                        throw new Exceptions.InvalidPacketDataException($"Expected a PULSE/SPACE pair. But got {type1},{type2}.");
                    }
                }
            }

            return bits;
        }

        protected byte[] UInt16ToBytes(ushort number)
        {
            byte[] result = new byte[2];
            checked
            {
                result[1] = (byte)(number & 0x00FF);
                result[0] = (byte)((number & 0xFF00) >> 8);
            }
            return result;
        }

        protected ushort UInt16FromBytes(IList<byte> bytes)
        {
            ushort result = bytes[1];
            result |= checked((ushort)(bytes[0] << 8));
            return result;
        }
    }
}
