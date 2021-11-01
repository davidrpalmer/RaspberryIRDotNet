using System;
using RaspberryIRDotNet.PacketFormats;
using RaspberryIRDotNet.RX.PulseSpaceSource;

namespace RaspberryIRDotNet.RX
{
    public abstract class FormattedIRCapture
    {
        public FormatGuesser FormatGuesser { get; set; } = new FormatGuesser();

        private readonly IPulseSpaceSource _captureSource;

        /// <param name="captureDevicePath">The IR capture device, example '/dev/lirc0'.</param>
        protected FormattedIRCapture(string captureDevicePath) : this(new PulseSpaceCaptureLirc(captureDevicePath))
        {
            if (string.IsNullOrWhiteSpace(captureDevicePath))
            {
                throw new ArgumentNullException(nameof(captureDevicePath));
            }
        }
        protected FormattedIRCapture(IPulseSpaceSource source)
        {
            _captureSource = source ?? throw new ArgumentNullException(nameof(source));
            _captureSource.ReceivedPulseSpaceBurst += ReceivedPulseSpaceBurst;
        }

        private void ReceivedPulseSpaceBurst(object sender, ReceivedPulseSpaceBurstEventArgs e)
        {
            var formatInfo = FormatGuesser.GuessFormat(e.Buffer);
            if (formatInfo != null)
            {
                OnReceiveIRFormattedMessage(e, new IRPulseMessage(e.Buffer, formatInfo.UnitDurationMicrosecs), formatInfo.Converter);
            }
            else
            {
                OnReceiveBadIR(e);
            }
        }

        /// <summary>
        /// Handle an incoming IR message.
        /// </summary>
        /// <param name="rawDataBuffer">The raw PULSE/SPACE data received from the IR driver without any rounding. Typically only useful for diagnostics. Note this buffer is not yours to keep! Once this method returns this buffer will be reused by the calling code. So copy the data if you want to keep it.</param>
        /// <param name="message">The rounded data. This one you can keep.</param>
        /// <param name="formatConverter">A converter object that can convert from the IR message into a formatted message such as NEC.</param>
        /// <returns>
        /// Return TRUE to continue capturing IR or FALSE to stop.
        /// </returns>
        protected abstract void OnReceiveIRFormattedMessage(ReceivedPulseSpaceBurstEventArgs rawData, IRPulseMessage message, object formatConverter);

        /// <summary>
        /// Handle incoming IR noise or corrupted message.
        /// </summary>
        /// <param name="rawDataBuffer">The raw PULSE/SPACE data received that was not recognised as any kind of IR message format.</param>
        /// <returns>
        /// Return TRUE to continue capturing IR or FALSE to stop.
        /// </returns>
        protected virtual void OnReceiveBadIR(ReceivedPulseSpaceBurstEventArgs rawData)
        {
        }

        /// <summary>
        /// Open the IR device and begin reading. This is a blocking call that will not return until capturing ends.
        /// </summary>
        protected void Capture(ReadCancellationToken cancellationToken)
        {
            if (FormatGuesser == null)
            {
                throw new ArgumentNullException(nameof(FormatGuesser));
            }
            _captureSource.Capture(cancellationToken);
        }
    }
}
