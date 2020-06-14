using System;

namespace RaspberryIRDotNet.RX
{
    /// <summary>
    /// Rather than raw PULSE/SPACE messages as they come from the driver, this class presents PULSE/SPACE messages that have been checked for a lead-in pattern (which removes almost all the false triggers) and also cleans up the signal by rounding PULSES & SPACES to the nearest expected duration.
    /// </summary>
    public abstract class CleanedUpIRCapture : PulseSpaceCapture
    {
        /// <summary>
        /// The duration (in microseconds) for one unit. This will vary between manufacturers, a typical value is 400-600.
        /// </summary>
        public int UnitDurationMicrosecs { get; set; }

        /// <summary>
        /// IR signals start with a lead-in pattern. Specify the pattern here so that signals may be distinguished from background noise. The first is a pulse, the second is a space. If there is a 3rd then that is a pulse and so on. This is defined as the number of units. So a Panasonic lead-in (which is apparently 8 units ON and 4 units OFF) would be set as { 8, 4 }.
        /// </summary>
        public IReadOnlyPulseSpaceUnitList LeadInPattern { get; set; }

        public void SetLeadInPatternAsMicrosecs(IReadOnlyPulseSpaceDurationList microsecs)
        {
            if (UnitDurationMicrosecs < 1)
            {
                throw new InvalidOperationException($"Must define {nameof(UnitDurationMicrosecs)} first.");
            }

            LeadInPattern = new PulseSpaceUnitList(UnitDurationMicrosecs, microsecs);
        }

        protected sealed override bool OnReceivePulseSpaceBlock(PulseSpaceDurationList buffer)
        {
            return OnReceiveIRPulseMessage(buffer, new IRPulseMessage(buffer, UnitDurationMicrosecs));
        }

        /// <summary>
        /// Handle in incoming IR message. Typically the first thing to do is to call <see cref="CheckMessage"/> to see if it is any good.
        /// </summary>
        /// <param name="rawDataBuffer">The raw PULSE/SPACE data received from the IR driver without any rounding. Typically only useful for diagnostics. Note this buffer is not yours to keep! Once this method returns this buffer will be reused by the calling code. So copy the data if you want to keep it.</param>
        /// <param name="message">The rounded data. This one you can keep.</param>
        /// <returns>
        /// Return TRUE to continue capturing IR or FALSE to stop.
        /// </returns>
        protected abstract bool OnReceiveIRPulseMessage(PulseSpaceDurationList rawDataBuffer, IRPulseMessage message);

        protected override void OnBeforeRawCapture()
        {
            if (LeadInPattern == null)
            {
                throw new ArgumentNullException(nameof(LeadInPattern));
            }
            foreach (int leadIn in LeadInPattern)
            {
                if (leadIn > 50)
                {
                    // A lead-in pattern of 50 or more units is very unlikely, so assume it is wrong.
                    throw new ArgumentException($"The lead-in pattern specification looks invalid. Did you specify it in microseconds? It should be specified in units. If you want to specify microseconds then use the {nameof(SetLeadInPatternAsMicrosecs)} method.");
                }
            }
            base.OnBeforeRawCapture();
        }

        /// <summary>
        /// Check to see if the received message is valid. Override this to add your own validation rules.
        /// </summary>
        protected virtual bool CheckMessage(IRPulseMessage message)
        {
            for (int i = 0; i < LeadInPattern.Count; i++)
            {
                if (message.PulseSpaceUnits[i] != LeadInPattern[i])
                {
                    // Lead-in pattern is wrong, assume this is noise.
                    return false;
                }
            }

            return true;
        }
    }
}
