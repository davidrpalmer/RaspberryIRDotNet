using System;
using System.Collections.Generic;
using RaspberryIRDotNet.PacketFormats.BinaryConverters.BitByteConverters;

namespace RaspberryIRDotNet.PacketFormats.BinaryConverters
{
    /// <summary>
    /// For IR that takes a PULSE & SPACE pair to encode a single bit.
    /// </summary>
    public abstract class PulseSpacePairBinaryConverter : BinaryConverter
    {
        protected abstract PulseSpacePairBitConverter _pulseSpacePairBitConverter { get; }

        protected abstract IBitByteConverter _bitByteConverter { get; }

        private List<bool> ToBits(IReadOnlyPulseSpaceUnitList irPulseSpaceList)
        {
            int startAtIndex = HasLeadIn(irPulseSpaceList) ? _leadInPattern.Count : 0;

            if (!_leadInPattern.IsPulse(startAtIndex))
            {
                throw new Exception("The start is not a pulse. The lead-in data is probably invalid.");
            }

            // It will end with a pulse, but it has no meaning. So the last useful pulse is actually the 2nd to last pulse.
            // So... minus 1 because Count is always one bigger than index. Minus 1 for the last pulse we will throw away
            // and finally minus 1 to get from the last space to the pulse that came before it. Total of minus 3.
            int lastPulseIndex = irPulseSpaceList.Count - 3;

            if (lastPulseIndex < startAtIndex)
            {
                throw new Exceptions.InvalidPacketDataException("Not enough IR data.");
            }

            if (!_leadInPattern.IsPulse(lastPulseIndex))
            {
                // It's not actually this PULSE that is the last one, but if the last thing is a SPACE then this will also be wrong.
                throw new Exceptions.InvalidPacketDataException("The IR data length is invalid. It must start and end with a PULSE.");
            }

            List<bool> binary = new List<bool>();

            for (int i = startAtIndex; i <= lastPulseIndex; i += 2)
            {
                binary.Add(_pulseSpacePairBitConverter.PulseSpaceToBit(irPulseSpaceList[i], irPulseSpaceList[i + 1]));
            }

            return binary;
        }

        public override byte[] ToBytes(IReadOnlyPulseSpaceUnitList irMessage) => _bitByteConverter.ToBytes(ToBits(irMessage));

        public override PulseSpaceUnitList ToIR(byte[] bytes) => ToIR(_bitByteConverter.ToBits(bytes));

        private PulseSpaceUnitList ToIR(bool[] bits)
        {
            PulseSpaceUnitList pulseSpace = new PulseSpaceUnitList((bits.Length * 2) + _leadInPattern.Count);
            pulseSpace.AddRange(_leadInPattern);
            foreach (var bit in bits)
            {
                var bitAsIR = _pulseSpacePairBitConverter.BitToPulseSpace(bit);
                if (bitAsIR.Length != 2)
                {
                    throw new Exception("Must be exactly 2 - a PULSE and a SPACE.");
                }
                pulseSpace.AddRange(bitAsIR);
            }
            pulseSpace.Add(1); // Trailing PULSE - without this the receiver would not know how long the last SPACE was.
            return pulseSpace;
        }
    }
}
