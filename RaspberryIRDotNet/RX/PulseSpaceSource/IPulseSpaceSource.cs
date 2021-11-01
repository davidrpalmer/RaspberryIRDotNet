using System;

namespace RaspberryIRDotNet.RX.PulseSpaceSource
{
    public interface IPulseSpaceSource
    {
        /// <summary>
        /// TRUE for real time capture devices such as an IR sensor. FALSE for other devices such as a pre-recorded capture of IR data.
        /// </summary>
        /// <remarks>
        /// Some IR receiving & processing classes use this to disable debounce timers for non-real time devices.
        /// </remarks>
        bool RealTime { get; }

        /// <summary>
        /// When a burst of PULSE/SPACE signals are received this event is raised. A burst is defined as some PULSE/SPACE signals followed by a timeout. So this event is fired every time there is a timeout event from the IR device.
        /// </summary>
        event EventHandler<ReceivedPulseSpaceBurstEventArgs> ReceivedPulseSpaceBurst;

        /// <summary>
        /// Open the IR device and begin reading. This is a blocking call that will not return until capturing ends.
        /// </summary>
        /// <param name="cancellationToken">(optional) Provide a token that can be used later on to cancel the capture. Note capturing may also be cancelled using the <see cref="ReceivedPulseSpaceBurst"/> event EventArgs.</param>
        /// <exception cref="System.IO.EndOfStreamException">If all the IR bursts are read before the capture is cancelled. (This won't happen on a real LIRC device, it would just wait for more IR. But it can happen on something like a pre-recorded IR source.)</exception>
        void Capture(ReadCancellationToken cancellationToken);
    }
}
