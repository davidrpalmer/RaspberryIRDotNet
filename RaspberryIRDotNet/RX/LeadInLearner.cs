using System;
using System.Collections.Generic;
using System.Linq;
using RaspberryIRDotNet.RX.PulseSpaceSource;

namespace RaspberryIRDotNet.RX
{
    /// <summary>
    /// Learn the lead-in pattern, as durations.
    /// </summary>
    public class LeadInLearner
    {
        private readonly List<PulseSpaceDurationList> _received = new List<PulseSpaceDurationList>();

        /// <summary>
        /// How many captures must be the same before we believe they accurately represent the lead-in pattern.
        /// </summary>
        public int MinimumMatchingCaptures { get; set; } = 10;

        /// <summary>
        /// Some IR was received. Don't know if it was a signal or noise but we saw it.
        /// </summary>
        public event EventHandler Received;

        private PulseSpaceDurationList _foundLeadIn;

        private IDebounceTimer _debounceTimer = new DebounceTimer(TimeSpan.FromMilliseconds(400));

        /// <summary>
        /// For unit testing.
        /// </summary>
        internal void SetDebounceTimer(IDebounceTimer timer)
        {
            _debounceTimer = timer;
        }

        private readonly IPulseSpaceSource _captureSource;

        /// <param name="captureDevicePath">The IR capture device, example '/dev/lirc0'.</param>
        public LeadInLearner(string captureDevicePath) : this(new PulseSpaceCaptureLirc(captureDevicePath))
        {
            if (string.IsNullOrWhiteSpace(captureDevicePath))
            {
                throw new ArgumentNullException(nameof(captureDevicePath));
            }
        }
        /// <param name="enableDebounceTimer">Set to TRUE for a real time source. Set to FALSE for a pre-recorded source.</param>
        public LeadInLearner(IPulseSpaceSource source)
        {
            _captureSource = source ?? throw new ArgumentNullException(nameof(source));
            _captureSource.ReceivedPulseSpaceBurst += ReceivedPulseSpaceBurst;
        }

        private void ReceivedPulseSpaceBurst(object sender, ReceivedPulseSpaceBurstEventArgs e)
        {
            if (e.Buffer.Count < 10) // If the message is short then assume it is noise or a key repeat signal.
            {
                return;
            }

            if (e.Buffer[0] < 600 || e.Buffer[1] < 400) // The lead-in is usually quite long (typically a few thousand microsecs for the pulse at least). So if the signals are short then it is probably noise.
            {
                return;
            }

            if (_captureSource.RealTime && !_debounceTimer.ReadyToDoAnother) // Allow the user time to let go of the button.
            {
                return;
            }

            _received.Add(e.Buffer.Copy());
            Received?.Invoke(this, EventArgs.Empty);
            _debounceTimer.Restart();

            if (_received.Count < MinimumMatchingCaptures)
            {
                // Not even worth trying to work it out before we have enough.
                return;
            }

            _foundLeadIn = WorkOutLeadIn();
            if (_foundLeadIn == null)
            {
                if (_received.Count >= MinimumMatchingCaptures * 3)
                {
                    throw new Exceptions.InsufficientMatchingSamplesException("Got 3 times the target number of captures and still can't find a pattern. Either there is too much noise or there is no lead-in.");
                }
                return;
            }

            e.StopCapture = true; // Got it, we can stop now.
        }

        private PulseSpaceDurationList WorkOutLeadIn()
        {
            const int roundTo = 100; // Assume the error won't be so bad that we can't reliably round to 100.
            var rounded = _received.Select(r => Tuple.Create(r, r.Copy(roundTo))).ToList();
            var mostPopularGroup = rounded
                .GroupBy(x => new { Pulse = x.Item2[0], Space = x.Item2[1] })
                .OrderByDescending(x => x.Count())
                .First();

            if (mostPopularGroup.Count() < MinimumMatchingCaptures)
            {
                return null;
            }

            
            return new PulseSpaceDurationList()
            {
                (int)mostPopularGroup.Select(x => x.Item1[0]).Average(),
                (int)mostPopularGroup.Select(x => x.Item1[1]).Average()
            };
        }

        /// <returns>
        /// The lead-in pattern, expressed as microseconds.
        /// </returns>
        /// <remarks>
        /// This can't return the pattern as a number of units because the unit duration is not known in this class.
        /// </remarks>
        public PulseSpaceDurationList LearnLeadInDurations()
        {
            _debounceTimer.Restart();
            _foundLeadIn = null;
            _received.Clear();

            if (MinimumMatchingCaptures <= 2)
            {
                throw new ArgumentOutOfRangeException(nameof(MinimumMatchingCaptures));
            }

            _captureSource.Capture(null);
            return _foundLeadIn;
        }
    }
}
