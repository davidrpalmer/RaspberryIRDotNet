using System;
using RaspberryIRDotNet.RX.Filters;
using RaspberryIRDotNet.RX.PulseSpaceSource;

namespace RaspberryIRDotNet.RX
{
    /// <summary>
    /// Rather than raw PULSE/SPACE messages as they come from the driver, this class presents PULSE/SPACE messages that have been filtered and also cleans up the signal by rounding PULSES & SPACES to the nearest expected duration.
    /// </summary>
    public abstract class CleanedUpIRCapture
    {
        /// <summary>
        /// The duration (in microseconds) for one unit. This will vary between manufacturers, a typical value is 400-600.
        /// </summary>
        public int UnitDurationMicrosecs { get; set; }

        public MultipleFilters RXFilters { get; set; } = new MultipleFilters();

        private readonly IPulseSpaceSource _captureSource;

        /// <param name="captureDevicePath">The IR capture device, example '/dev/lirc0'.</param>
        protected CleanedUpIRCapture(string captureDevicePath) : this(new PulseSpaceCaptureLirc(captureDevicePath))
        {
            if (string.IsNullOrWhiteSpace(captureDevicePath))
            {
                throw new ArgumentNullException(nameof(captureDevicePath));
            }
        }
        protected CleanedUpIRCapture(IPulseSpaceSource source)
        {
            _captureSource = source ?? throw new ArgumentNullException(nameof(source));
            _captureSource.ReceivedPulseSpaceBurst += ReceivedPulseSpaceBurst;
        }

        /// <summary>
        /// Shortcut method to make adding a lead-in filter quicker.
        /// </summary>
        public void SetLeadInPatternFilterByUnits(IReadOnlyPulseSpaceUnitList units)
        {
            if (units == null) { throw new ArgumentNullException(); }

            if (UnitDurationMicrosecs < Utility.UnitDurationMinimum)
            {
                throw new InvalidOperationException("Must set the unit duration first.");
            }

            RXFilters.SetSingleInstanceFilter(new LeadInPatternFilter(units, UnitDurationMicrosecs), addToTop: true);
        }

        private void ReceivedPulseSpaceBurst(object sender, ReceivedPulseSpaceBurstEventArgs e)
        {
            OnReceiveIRPulseMessage(e, new IRPulseMessage(e.Buffer, UnitDurationMicrosecs));
        }

        /// <summary>
        /// Handle in incoming IR message. Typically the first thing to do is to call <see cref="CheckMessage"/> to see if it is any good.
        /// </summary>
        /// <param name="rawData">The raw PULSE/SPACE data received from the IR driver without any rounding. Typically only useful for diagnostics. Can also use this to cancel.</param>
        /// <param name="message">The rounded data. This one you can keep.</param>
        protected abstract void OnReceiveIRPulseMessage(ReceivedPulseSpaceBurstEventArgs rawData, IRPulseMessage message);

        /// <summary>
        /// Check to see if the received message is valid.
        /// </summary>
        /// <remarks>
        /// Override this to add your own validation rules. Be sure to call the base implementation first when you override.
        /// </remarks>
        protected virtual bool CheckMessage(IRPulseMessage message)
        {
            foreach (var unitCount in message.PulseSpaceUnits)
            {
                if (unitCount <= 0)
                {
                    // A PULSE or a SPACE was so short that it was rounded down to zero. Assume either noise, or a corrupted transmission.
                    return false;
                }
            }

            return RXFilters.Check(message.PulseSpaceDurations);
        }

        /// <summary>
        /// Open the IR device and begin reading. This is a blocking call that will not return until capturing ends.
        /// </summary>
        protected void Capture(ReadCancellationToken cancellationToken)
        {
            if (RXFilters == null)
            {
                throw new ArgumentNullException(nameof(RXFilters));
            }

            RXFilters.AssertConfigOK();

            if (UnitDurationMicrosecs < 5) // Nothing special about 5, but it's definitely not accurate enough for 5 so we can block it.
            {
                throw new ArgumentOutOfRangeException(nameof(UnitDurationMicrosecs));
            }

            _captureSource.Capture(cancellationToken);
        }
    }
}
