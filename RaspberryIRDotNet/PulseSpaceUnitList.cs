using System;
using System.Linq;
using System.Collections.Generic;

namespace RaspberryIRDotNet
{
    /// <summary>
    /// Represents PULSE/SPACE/PULSE/SPACE.... IR data. First is a PULSE, second is a SPACE, third is a PULSE and so on. Each integer value represents the number of units that the pulse or space lasts for.
    /// </summary>
    public class PulseSpaceUnitList : List<byte>, IReadOnlyPulseSpaceUnitList
    {
        public PulseSpaceUnitList()
        {
        }

        public PulseSpaceUnitList(int capacity) : base(capacity)
        {
        }

        public PulseSpaceUnitList(IEnumerable<byte> collection) : base(collection)
        {
        }

        public PulseSpaceUnitList(int unitDuration, IReadOnlyPulseSpaceDurationList pulseSpaceDurations) : this(pulseSpaceDurations.Count)
        {
            foreach(var duration in pulseSpaceDurations)
            {
                int rounded = Utility.RoundMicrosecs(duration, unitDuration);
                Add(Convert.ToByte(rounded / unitDuration));
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the list. Which is the number or PULSE/SPACE signals.
        /// </summary>
        public new int Count => base.Count;

        /// <summary>
        /// Get the total number of units (every item in this list added together).
        /// </summary>
        public int UnitCount
        {
            get
            {
                int result = 0;
                foreach (var value in this)
                {
                    result += value;
                }
                return result;
            }
        }

        /// <summary>
        /// Create a new list with the same data.
        /// </summary>
        public PulseSpaceUnitList Copy()
        {
            return new PulseSpaceUnitList(this);
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

        public string SaveToString()
        {
            return string.Join(',', this);
        }

        public static PulseSpaceUnitList LoadFromString(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            IEnumerable<byte> pulsesAndSpacesUnitCount = text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .Select(s => byte.Parse(s));

            return new PulseSpaceUnitList(pulsesAndSpacesUnitCount);
        }
    }

    public interface IReadOnlyPulseSpaceUnitList : IReadOnlyList<byte>
    {
        bool IsSpace(int index);
        bool IsPulse(int index);

        PulseSpaceUnitList Copy();

        string SaveToString();
    }
}
