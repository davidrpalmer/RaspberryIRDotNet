﻿using System;
using System.Collections.Generic;
using static RaspberryIRDotNet.Utility;

namespace RaspberryIRDotNet
{
    /// <summary>
    /// Represents PULSE/SPACE/PULSE/SPACE.... IR data. First is a PULSE, second is a SPACE, third is a PULSE and so on. Each integer value represents the duration of the pulse or space in microseconds.
    /// </summary>
    public class PulseSpaceDurationList : List<int>, IReadOnlyPulseSpaceDurationList
    {
        public PulseSpaceDurationList()
        {
        }

        public PulseSpaceDurationList(int capacity) : base(capacity)
        {
        }

        public PulseSpaceDurationList(IEnumerable<int> collection) : base(collection)
        {
        }

        public PulseSpaceDurationList(int unitDuration, IReadOnlyPulseSpaceUnitList pulseSpaceUnits) : this(pulseSpaceUnits.Count)
        {
            foreach (var units in pulseSpaceUnits)
            {
                Add(units * unitDuration);
            }
        }

        /// <summary>
        /// Create a new buffer with the same data.
        /// </summary>
        public PulseSpaceDurationList Copy()
        {
            return new PulseSpaceDurationList(this);
        }

        /// <summary>
        /// Create a new buffer with the same data, but rounded off.
        /// </summary>
        public PulseSpaceDurationList Copy(int roundTo)
        {
            if (roundTo <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(roundTo));
            }

            var copy = new PulseSpaceDurationList(Count);
            foreach (var original in this)
            {
                copy.Add(RoundMicrosecs(original, roundTo));
            }
            return copy;
        }

        public bool IsSpace(int index)
        {
            if (index < 0)
            {
                // Negative value is most likely a mistake so make the user aware of it.
                throw new ArgumentOutOfRangeException(nameof(index), 0, "Index cannot be negative.");
            }
            // Can't see any reason to block it so we still return true/false even if the index is larger than the buffer size.
            return index % 2 != 0;
        }

        public bool IsPulse(int index)
        {
            return !IsSpace(index);
        }
    }

    public interface IReadOnlyPulseSpaceDurationList : IReadOnlyList<int>
    {
        bool IsSpace(int index);
        bool IsPulse(int index);

        PulseSpaceDurationList Copy();
        PulseSpaceDurationList Copy(int roundTo);
    }
}
