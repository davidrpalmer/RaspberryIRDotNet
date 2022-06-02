using System;

namespace RaspberryIRDotNet
{
    /// <summary>
    /// An IR message represented as pulses and spaces.
    /// </summary>
    public class IRPulseMessage
    {
        /// <summary>
        /// First and last must be a pulse. The 2nd is a space, the 3rd is a pulse, 4th a space, etc. The integer value is the duration in microseconds.
        /// </summary>
        public IReadOnlyPulseSpaceDurationList PulseSpaceDurations => _durations;

        /// <summary>
        /// Represents the same as <see cref="PulseSpaceDurations"/> but as a number of units, rather than as a number of microseconds.
        /// </summary>
        public IReadOnlyPulseSpaceUnitList PulseSpaceUnits => _units;

        private readonly PulseSpaceUnitList _units;

        private readonly PulseSpaceDurationList _durations;

        /// <summary>
        /// How many units are in this message.
        /// </summary>
        public int UnitCount { get; }

        /// <summary>
        /// How many PULSEs and SPACEs are in this message. For example a message that goes PULSE,SPACE,PULSE would be 3.
        /// </summary>
        public int PulseSpaceCount { get; }

        /// <summary>
        /// How long each unit lasts for, in microseconds.
        /// </summary>
        public int UnitDuration { get; }

        /// <summary>
        /// The total time it would take to transmit this message (all PULSEs and SPACEs added together).
        /// </summary>
        public int TotalDuration => UnitCount * UnitDuration;

        /// <param name="pulsesAndSpacesAsDurations">The pulse/space durations. These will be rounded to multiples of the <paramref name="unitDuration"/>.</param>
        /// <param name="unitDuration">How long each unit lasts, in microseconds. The input durations will be rounded to this.</param>
        public IRPulseMessage(IReadOnlyPulseSpaceDurationList pulsesAndSpacesAsDurations, int unitDuration)
        {
            if (pulsesAndSpacesAsDurations == null)
            {
                throw new ArgumentNullException(nameof(pulsesAndSpacesAsDurations));
            }
            if (unitDuration < Utility.UnitDurationMinimum || unitDuration > Utility.UnitDurationMaximum)
            {
                throw new ArgumentOutOfRangeException(nameof(unitDuration), unitDuration, "Unit duration is invalid.");
            }

            UnitDuration = unitDuration;

            _durations = pulsesAndSpacesAsDurations.Copy(unitDuration); // Create a copy because the original buffer might be modified by the caller after we return.

            _units = new PulseSpaceUnitList(unitDuration, pulsesAndSpacesAsDurations);
            UnitCount = _units.UnitCount;
            PulseSpaceCount = _units.Count;
        }

        /// <param name="pulsesAndSpacesAsNumberOfUnits">Each item represents a PULSE or a SPACE. Each value represents the number of units.</param>
        /// <param name="unitDuration">How long each unit lasts, in microseconds.</param>
        public IRPulseMessage(IReadOnlyPulseSpaceUnitList pulsesAndSpacesAsNumberOfUnits, int unitDuration)
        {
            if (pulsesAndSpacesAsNumberOfUnits == null)
            {
                throw new ArgumentNullException(nameof(pulsesAndSpacesAsNumberOfUnits));
            }
            if (unitDuration < Utility.UnitDurationMinimum || unitDuration > Utility.UnitDurationMaximum)
            {
                throw new ArgumentOutOfRangeException(nameof(unitDuration), unitDuration, "Unit duration is invalid.");
            }

            UnitDuration = unitDuration;

            _units = pulsesAndSpacesAsNumberOfUnits.Copy(); // Make a copy so if the input is modified later on it won't affect us.

            _durations = new PulseSpaceDurationList(unitDuration, pulsesAndSpacesAsNumberOfUnits);

            UnitCount = _units.UnitCount;
            PulseSpaceCount = _units.Count;
        }
    }
}
