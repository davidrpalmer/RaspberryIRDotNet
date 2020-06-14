using System;
using System.Collections.Generic;
using System.Linq;
using RaspberryIRDotNet.Exceptions;

namespace RaspberryIRDotNet.RX
{
    /// <summary>
    /// Capture the infrared signal for a remote control button.
    /// </summary>
    public class IRMessageLearn : CleanedUpSingleIRCapture, IMultipleCapture
    {
        /// <summary>
        /// Ready for the user to press a button.
        /// </summary>
        public event EventHandler Waiting;

        /// <summary>
        /// Captured a clean IR message.
        /// </summary>
        public event EventHandler Hit;

        /// <summary>
        /// How many captures must be the same for us to believe they are right.
        /// </summary>
        public int MinimumMatchingCaptures
        {
            get => _minimumMatchingCaptures;
            set
            {
                const int min = 3;
                if (value < min)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), $"Cannot be less than {min}.");
                }

                _minimumMatchingCaptures = value;
            }
        }
        private int _minimumMatchingCaptures = 6;

        /// <summary>
        /// If this many non-matching captures are received then something is wrong, throw an error. Note this does not count noise, only captures that seem valid, but do not match the other captures.
        /// </summary>
        public int ErrorAfterBadCaptureCount { get; set; } = 10;

        /// <summary>
        /// Discount any signals that have fewer units than this.
        /// </summary>
        public int MessageMinimumUnitCount { get; set; } = 1;

        /// <summary>
        /// Discount any signals that have more units than this.
        /// </summary>
        public int MessageMaximumUnitCount { get; set; } = 500;

        /// <summary>
        /// Wait this long in between captures to allow the user time to let go of the button and get ready to press it again.
        /// </summary>
        public TimeSpan CaptureDelay { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Learn an IR message and return it.
        /// </summary>
        public IRPulseMessage LearnMessage()
        {
            List<IRPulseMessage> allCaptues = new List<IRPulseMessage>();
            while (true)
            {
                Waiting?.Invoke(this, EventArgs.Empty);
                allCaptues.Add(CaptureSingleMessage());
                Hit?.Invoke(this, EventArgs.Empty);

                var chosenSample = CheckSamples(allCaptues);
                if (chosenSample != null)
                {
                    return chosenSample;
                }
                if (allCaptues.Count >= ErrorAfterBadCaptureCount)
                {
                    throw new InsufficientMatchingSamplesException("Not finding any matches. There is probably too much IR noise. Note the signal has already been cleaned up so any differences here represent a significant variation in the IR signal.");
                }

                if (CaptureDelay > TimeSpan.Zero)
                {
                    System.Threading.Thread.Sleep(CaptureDelay);
                }
            }
        }

        private IRPulseMessage CheckSamples(ICollection<IRPulseMessage> allCaptues)
        {
            if (allCaptues.Count < MinimumMatchingCaptures)
            {
                return null; // Not got enough samples yet.
            }

            ICollection<IRPulseMessage> bestSamples;
            if (MessageMinimumUnitCount == MessageMaximumUnitCount)
            {
                if (allCaptues.Any(x => x.UnitCount != MessageMinimumUnitCount))
                {
                    throw new Exception($"Only messages with {MessageMinimumUnitCount} units should have got into here.");
                }

                bestSamples = allCaptues;
            }
            else
            {
                if (allCaptues.Any(x => x.UnitCount < MessageMinimumUnitCount || x.UnitCount > MessageMaximumUnitCount))
                {
                    throw new Exception($"Only messages with {MessageMinimumUnitCount}-{MessageMaximumUnitCount} units should have got into here.");
                }

                bestSamples = allCaptues
                    .GroupBy(x => x.UnitCount) // Group by the number of units in the message.
                    .OrderByDescending(g => g.Count())
                    .First() // Get the most popular unit size and assume that is the right one.
                    .ToList();

                if (bestSamples.Count < MinimumMatchingCaptures)
                {
                    // Not enough of the samples have the same number of units.
                    return null;
                }
            }

            foreach (var sample in bestSamples)
            {
                int matchingCount = bestSamples.Count(x => DoMessagesMatch(sample, x));

                if (matchingCount >= MinimumMatchingCaptures)
                {
                    return sample;
                }
            }

            return null; // Not got enough matching samples yet.
        }

        private static bool DoMessagesMatch(IRPulseMessage messageA, IRPulseMessage messageB)
        {
            if (messageA.UnitCount != messageB.UnitCount)
            {
                // The messages were already supposed to be matched by unit count. So if any here have different unit counts then something went wrong.
                throw new Exception("Messages have different unit counts.");
            }
            if (messageA.PulseSpaceDurations.Count != messageB.PulseSpaceDurations.Count)
            {
                return false;
            }

            for (int i = 0; i < messageA.PulseSpaceDurations.Count; i++)
            {
                if (messageA.PulseSpaceDurations[i] != messageB.PulseSpaceDurations[i])
                {
                    return false;
                }
            }

            return true;
        }

        protected override bool CheckMessage(IRPulseMessage message)
        {
            if (message.UnitCount < MessageMinimumUnitCount || message.UnitCount > MessageMaximumUnitCount)
            {
                // Wrong number of units, assume this is noise.
                return false;
            }

            return base.CheckMessage(message);
        }
    }
}
