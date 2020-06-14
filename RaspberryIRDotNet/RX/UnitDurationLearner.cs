using System;
using System.Collections.Generic;
using System.Linq;

namespace RaspberryIRDotNet.RX
{
    public class UnitDurationLearner : PulseSpaceCapture, IMultipleCapture
    {
        private PulseSpaceDurationList _lastReceived;

        /// <summary>
        /// IR signals start with a lead-in pattern. Specify the pattern here so that signals may be distinguished
        /// from background noise. The first is a pulse, the second is a space. If there is a 3rd then that is a
        /// pulse and so on.
        /// 
        /// This is defined as the *duration* of the PULSES and SPACES in microseconds. READ THAT LAST SENTENCE AGAIN,
        /// this class is different from other classes in this library as the others all use units rather than
        /// durations to specify the pattern.
        /// </summary>
        public IReadOnlyPulseSpaceDurationList LeadInPatternDurations { get; set; }

        /// <summary>
        /// Ready for the user to press a button.
        /// </summary>
        public event EventHandler Waiting;

        /// <summary>
        /// Captured a clean IR message.
        /// </summary>
        public event EventHandler Hit;

        /// <summary>
        /// IR noise got in the way of a capture.
        /// </summary>
        public event EventHandler Miss;

        /// <summary>
        /// How many keys should be captured before results are returned.
        /// </summary>
        public int TargetCaptures { get; set; } = 10;

        /// <summary>
        /// Wait this long in between captures to allow the user time to let go of the button and get ready to press it again.
        /// </summary>
        public TimeSpan CaptureDelay { get; set; } = TimeSpan.FromSeconds(1);

        protected override bool OnReceivePulseSpaceBlock(PulseSpaceDurationList buffer)
        {
            if (buffer.Count < (LeadInPatternDurations.Count + 5))
            {
                return true;
            }

            for (int i = 0; i < LeadInPatternDurations.Count; i++)
            {
                const int errorMagin = 100;
                if (buffer[i] < (LeadInPatternDurations[i] - errorMagin) || buffer[i] > (LeadInPatternDurations[i] + errorMagin))
                {
                    Miss?.Invoke(this, EventArgs.Empty);
                    return true; // Wrong pattern, discard this IR message.
                }
            }

            _lastReceived = buffer.Copy();
            return false;
        }

        public int LearnUnitDuration()
        {
            if (LeadInPatternDurations == null)
            {
                throw new ArgumentNullException(nameof(LeadInPatternDurations));
            }

            _lastReceived = null;

            foreach (int i in LeadInPatternDurations)
            {
                if (i < 30)
                {
                    // A lead-in pattern is unlikely to be more than 30 units, but is also unlikely to be less than 30 microseconds. So we can use this as a sanity check.
                    throw new ArgumentException("The lead-in pattern is specified in microseconds (not units) in this class.");
                }
            }

            var receivedMessages = CaptureIR();

            int smallest = Min_SkipVerySmallest(receivedMessages.SelectMany(b => b.Copy(100)));
            var smallSamples = receivedMessages.SelectMany(x => x).Where(x => x < (smallest + 80) && x > (smallest - 80));
            return (int)smallSamples.Average();
        }

        private IList<PulseSpaceDurationList> CaptureIR()
        {
            List<PulseSpaceDurationList> receivedMessages = new List<PulseSpaceDurationList>();
            while (receivedMessages.Count < TargetCaptures)
            {
                Waiting?.Invoke(this, EventArgs.Empty);
                CaptureFromDevice();
                Hit?.Invoke(this, EventArgs.Empty);
                if (_lastReceived == null)
                {
                    throw new Exception("Expected this to have a value.");
                }
                receivedMessages.Add(_lastReceived);
                if (CaptureDelay > TimeSpan.Zero)
                {
                    System.Threading.Thread.Sleep(CaptureDelay);
                }
            }
            return receivedMessages;
        }

        private static int Min_SkipVerySmallest(IEnumerable<int> numbers)
        {
            // Get the smallest, but skip the lower 10%
            return numbers.OrderBy(x => x).Skip(numbers.Count() / 10).First();
        }
    }
}
