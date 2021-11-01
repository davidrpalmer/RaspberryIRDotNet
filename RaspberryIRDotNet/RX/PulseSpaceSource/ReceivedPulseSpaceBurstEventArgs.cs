using System;

namespace RaspberryIRDotNet.RX.PulseSpaceSource
{
    public class ReceivedPulseSpaceBurstEventArgs : EventArgs
    {
        /// <summary>
        /// The PULSE/SPACE data received from the IR driver. Note this buffer is not yours to keep! Once this method returns this buffer will be reused by the calling code. So copy the data if you want to keep it. Creating an <see cref="IRPulseMessage"/> is one way to get a copy, or you can use <see cref="PulseSpaceDurationList.Copy"/>.
        /// </summary>
        public IReadOnlyPulseSpaceDurationList Buffer { get; }

        /// <summary>
        /// Event handlers can set this to TRUE to indicate that capturing should stop now. If FALSE then we will wait for the next IR message.
        /// </summary>
        public bool StopCapture { get; set; }

        public ReceivedPulseSpaceBurstEventArgs(IReadOnlyPulseSpaceDurationList buffer)
        {
            Buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
        }
    }
}
