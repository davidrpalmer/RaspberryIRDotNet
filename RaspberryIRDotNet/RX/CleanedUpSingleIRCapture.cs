using System;
using RaspberryIRDotNet.RX.PulseSpaceSource;

namespace RaspberryIRDotNet.RX
{
    /// <summary>
    /// Capture a single IR message. Filters out noise so only a valid message should be returned.
    /// </summary>
    public class CleanedUpSingleIRCapture : CleanedUpIRCapture
    {
        /// <summary>
        /// IR noise got in the way of a capture.
        /// </summary>
        public event EventHandler<ReceivedPulseSpaceBurstEventArgs> Miss;

        private IRPulseMessage _message;

        /// <param name="captureDevicePath">The IR capture device, example '/dev/lirc0'.</param>
        public CleanedUpSingleIRCapture(string captureDevicePath) : base(captureDevicePath)
        {
        }
        public CleanedUpSingleIRCapture(IPulseSpaceSource source) : base(source)
        {
        }

        /// <summary>
        /// Capture one message that seems to be valid. Anything that is obviously noise will be filtered out. You can add additional checking by overriding <see cref="CleanedUpIRCapture.CheckMessage"/>
        /// </summary>
        /// <returns>
        /// The captured message, or null if capture was stopped before a valid message was captured.
        /// </returns>
        public IRPulseMessage CaptureSingleMessage(ReadCancellationToken cancellationToken)
        {
            _message = null;
            base.Capture(cancellationToken);
            return _message;
        }

        protected new void Capture(ReadCancellationToken cancellationToken)
        {
            throw new Exception($"Do not use this method directly in this class. Call {nameof(CaptureSingleMessage)}() instead.");
        }

        protected sealed override void OnReceiveIRPulseMessage(ReceivedPulseSpaceBurstEventArgs rawData, IRPulseMessage message)
        {
            if (CheckMessage(message))
            {
                if (_message != null)
                {
                    throw new Exception("Message already has a value.");
                }
                _message = message;
                rawData.StopCapture = true; // Got the message, stop receiving IR.
            }
            else
            {
                Miss?.Invoke(this, rawData);
            }
        }
    }

}
