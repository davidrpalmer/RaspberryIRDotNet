using System;

namespace RaspberryIRDotNet.PacketFormats.BinaryConverters
{
    /// <summary>
    /// For converting IR data into a byte array and vice versa.
    /// </summary>
    public abstract class BinaryConverter
    {
        /// <summary>
        /// The lead-in pattern. This must have an even number of entries, otherwise the PULSE/SPACE offset will be wrong.
        /// </summary>
        protected abstract IReadOnlyPulseSpaceUnitList _leadInPattern { get; }

        protected bool HasLeadIn(IReadOnlyPulseSpaceUnitList input)
        {
            if (input == null) { throw new ArgumentNullException(nameof(input)); }

            if (_leadInPattern == null || _leadInPattern.Count < 1)
            {
                throw new InvalidOperationException("Lead-in is not defined.");
            }

            if (input.Count < _leadInPattern.Count)
            {
                return false;
            }

            for (int i = 0; i < _leadInPattern.Count; i++)
            {
                if (_leadInPattern[i] != input[i])
                {
                    return false;
                }
            }

            return true;
        }

        public abstract byte[] ToBytes(IReadOnlyPulseSpaceUnitList irData);

        public byte[] ToBytes(IRPulseMessage irData) => ToBytes(irData.PulseSpaceUnits);

        public abstract PulseSpaceUnitList ToIR(byte[] bytes);
    }
}
