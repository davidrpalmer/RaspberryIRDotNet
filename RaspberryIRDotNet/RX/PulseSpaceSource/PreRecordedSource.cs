using System;
using System.Collections.Generic;

namespace RaspberryIRDotNet.RX.PulseSpaceSource
{
    /// <summary>
    /// Returns IR bursts from the list. Note that each new call to <see cref="Capture"/> will restart from the first burst, it will not continue from where a previous capture ended.
    /// </summary>
    /// <remarks>
    /// Use <see cref="RawRecorder"/> to create an instance of this class by capturing IR data.
    /// </remarks>
    public class PreRecordedSource : IPulseSpaceSource
    {
        public List<IReadOnlyPulseSpaceDurationList> Bursts { get; set; } = [];

        public bool RealTime => false;

        public event EventHandler<ReceivedPulseSpaceBurstEventArgs> ReceivedPulseSpaceBurst;

        
        public void Capture(ReadCancellationToken cancellationToken)
        {
            if (Bursts == null)
            {
                throw new ArgumentNullException(nameof(Bursts));
            }

            if (cancellationToken == null)
            {
                cancellationToken = new ReadCancellationToken();
            }

            if (!cancellationToken.AddReference())
            {
                return;
            }
            try
            {
                foreach (var burst in Bursts)
                {
                    var eventArgs = new ReceivedPulseSpaceBurstEventArgs(burst);
                    ReceivedPulseSpaceBurst?.Invoke(this, eventArgs);
                    if (cancellationToken.IsCancellationRequested || eventArgs.StopCapture)
                    {
                        return;
                    }
                }
            }
            finally
            {
                cancellationToken.ReleaseReference();
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                // Reading from an LIRC device has no end, so this method should not be allowed to end without being cancelled.
                throw new System.IO.EndOfStreamException("End of pre-recorded source.");
            }
        }
    }
}
