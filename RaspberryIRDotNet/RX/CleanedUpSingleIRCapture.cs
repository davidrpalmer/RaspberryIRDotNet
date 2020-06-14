using System;

namespace RaspberryIRDotNet.RX
{
    /// <summary>
    /// Capture a single IR message. Filters out noise so only a valid message should be returned.
    /// To use this class just inherit from it and call <see cref="CaptureSingleMessage"/>.
    /// </summary>
    public abstract class CleanedUpSingleIRCapture : CleanedUpIRCapture
    {
        /// <summary>
        /// IR noise got in the way of a capture.
        /// </summary>
        public event EventHandler Miss;

        private IRPulseMessage _message;

        /// <summary>
        /// Capture one message that seems to be valid. Anything that is obviously noise will be filtered out. You can add additional checking by overriding <see cref="CleanedUpIRCapture.CheckMessage"/>
        /// </summary>
        protected IRPulseMessage CaptureSingleMessage()
        {
            if (UnitDurationMicrosecs <= 0)
            {
                throw new InvalidOperationException($"Must set a value for {nameof(UnitDurationMicrosecs)}.");
            }

            if (LeadInPattern == null || LeadInPattern.Count <= 0)
            {
                throw new InvalidOperationException($"The {nameof(LeadInPattern)} must be set. Without this background noise would keep triggering the IR sensor.");
            }

            _message = null;
            base.CaptureFromDevice();
            return _message;
        }

        protected new void CaptureFromDevice()
        {
            throw new Exception($"Do not use this method directly in this class. Call {nameof(CaptureSingleMessage)}() instead.");
        }

        protected override bool OnReceiveIRPulseMessage(PulseSpaceDurationList rawDataBuffer, IRPulseMessage message)
        {
            if (CheckMessage(message))
            {
                if (_message != null)
                {
                    throw new Exception("Message already has a value.");
                }
                _message = message;
                return false; // Got the message, stop receiving IR.
            }
            else
            {
                Miss?.Invoke(this, EventArgs.Empty);
                return true; // Not a good IR signal, keep trying.
            }
        }
    }

}
