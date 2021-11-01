using System;
using System.Collections.Generic;

namespace RaspberryIRDotNet.PacketFormats
{
    /// <summary>
    /// Get a list of bools (for PULSE / SPACE) where each row in the list represents one unit of time.
    /// Helps if a format can have multiple PULSES or multiple SPACES but all are the same length.
    /// </summary>
    /// <remarks>
    /// If the unit time is 50us and the IR data is 100us PULSE, 100us SPACE, 50usPULSE then the result
    /// of this would be [true, true, false, false, true].
    /// </remarks>
    class PointInTimeSampler
    {
        public List<bool> FromIR(IReadOnlyPulseSpaceUnitList irData, int startAtIndex = 0)
        {
            if (irData == null)
            {
                throw new ArgumentNullException(nameof(irData));
            }

            if (startAtIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(startAtIndex));
            }

            var result = new List<bool>();

            for (int i = startAtIndex; i < irData.Count; i++)
            {
                result.AddRange(System.Linq.Enumerable.Repeat(irData.IsPulse(i), irData[i]));
            }

            return result;
        }
    }
}
