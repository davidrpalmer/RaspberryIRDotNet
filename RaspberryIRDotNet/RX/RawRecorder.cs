using System;
using RaspberryIRDotNet.RX.PulseSpaceSource;

namespace RaspberryIRDotNet.RX
{
    /// <summary>
    /// Collect raw IR data for a period of time so it can be replayed later. Has some very basic noise filtering, but mostly just captures everything.
    /// </summary>
    public class RawRecorder
    {
        public class ProgressEventArgs : EventArgs
        {
            /// <summary>
            /// Preview of what has been recorded so far.
            /// </summary>
            public PreRecordedSource CapturedSoFar { get; }

            /// <summary>
            /// Event handlers can set this to TRUE to indicate that capturing should stop now.
            /// </summary>
            public bool StopCapture { get; set; }

            public ProgressEventArgs(PreRecordedSource captured)
            {
                CapturedSoFar = captured ?? throw new ArgumentNullException(nameof(captured));
            }
        }

        private readonly IPulseSpaceSource _captureSource;

        /// <summary>
        /// Some filters to apply to the IR. Only IR that passes the filters will be stored.
        /// </summary>
        public Filters.MultipleFilters Filters { get; set; } = new Filters.MultipleFilters();

        /// <summary>
        /// Some IR was received. Don't know if it was a signal or noise but we saw it.
        /// </summary>
        public event EventHandler<ProgressEventArgs> Received;

        /// <summary>
        /// Some IR was received but was filtered out.
        /// </summary>
        public event EventHandler<ProgressEventArgs> Filtered;

        private readonly IDebounceTimer _debounceTimer = new DebounceTimer(TimeSpan.FromMilliseconds(400));

        private PreRecordedSource _captured;

        /// <param name="captureDevicePath">The IR capture device, example '/dev/lirc0'.</param>
        public RawRecorder(string captureDevicePath) : this(new PulseSpaceCaptureLirc(captureDevicePath))
        {
            if (string.IsNullOrWhiteSpace(captureDevicePath))
            {
                throw new ArgumentNullException(nameof(captureDevicePath));
            }
        }
        public RawRecorder(IPulseSpaceSource source)
        {
            _captureSource = source ?? throw new ArgumentNullException(nameof(source));
            _captureSource.ReceivedPulseSpaceBurst += ReceivedPulseSpaceBurst;

            // Set some default filters that should cut out most background noise.
            Filters.Filters.Add(new Filters.DurationFilter()
            {
                Minimum = 200
            });
            Filters.Filters.Add(new Filters.PulseSpaceCountFilter()
            {
                Minimum = 10
            });
        }

        private void ReceivedPulseSpaceBurst(object sender, ReceivedPulseSpaceBurstEventArgs e)
        {
            var outboundEventArgs = new ProgressEventArgs(_captured);
            try
            {
                try
                {
                    if (_captureSource.RealTime && !_debounceTimer.ReadyToDoAnother) // Allow the user time to let go of the button.
                    {
                        Filtered?.Invoke(this, outboundEventArgs);
                        return;
                    }
                }
                finally
                {
                    _debounceTimer.Restart();
                }

                if (!Filters.Check(e.Buffer))
                {
                    // Noise.
                    Filtered?.Invoke(this, outboundEventArgs);
                    return;
                }

                _captured.Bursts.Add(e.Buffer.Copy());

                Received?.Invoke(this, outboundEventArgs);
            }
            finally
            {
                e.StopCapture |= outboundEventArgs.StopCapture;
            }
        }

        /// <summary>
        /// Collect IR data until cancelled.
        /// </summary>
        /// <returns>
        /// The collected data.
        /// </returns>
        public PreRecordedSource Record(ReadCancellationToken cancellationToken)
        {
            if (_captured != null)
            {
                throw new InvalidOperationException("Already started recording. This class can only record once, then it must be recreated.");
            }
            _captured = new PreRecordedSource();
            _debounceTimer.Restart();

            _captureSource.Capture(cancellationToken);

            return _captured;
        }
    }
}
