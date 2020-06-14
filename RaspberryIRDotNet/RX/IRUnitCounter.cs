using System;
using System.Collections.Generic;
using System.Linq;
using RaspberryIRDotNet.Exceptions;

namespace RaspberryIRDotNet.RX
{
    /// <summary>
    /// Counts the number of units that are received in IR messages.
    /// </summary>
    public class IRUnitCounter : CleanedUpSingleIRCapture, IMultipleCapture
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
        /// How many keys should be captured before results are returned. Better to get a large number like 20 to lessen the impact of errors and noise.
        /// </summary>
        public int TargetCaptures { get; set; } = 20;

        /// <summary>
        /// Wait this long in between captures to allow the user time to let go of the button and get ready to press it again.
        /// </summary>
        public TimeSpan CaptureDelay { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Capture and return the full results.
        /// </summary>
        /// <returns>
        /// Group Key = The unit count
        /// Values in group = All the IR messages with this unit count.
        /// </returns>
        public IGrouping<int, IRPulseMessage>[] Capture()
        {
            List<IRPulseMessage> allCaptues = new List<IRPulseMessage>();
            for (int i = 0; i < TargetCaptures; i++)
            {
                Waiting?.Invoke(this, EventArgs.Empty);
                allCaptues.Add(CaptureSingleMessage());
                Hit?.Invoke(this, EventArgs.Empty);

                if (CaptureDelay > TimeSpan.Zero)
                {
                    System.Threading.Thread.Sleep(CaptureDelay);
                }
            }

            return allCaptues.GroupBy(x => x.UnitCount).ToArray();
        }

        /// <summary>
        /// Same as <see cref="Capture"/> but also works out the most common unit count.
        /// </summary>
        public int CaptureAndGetMostCommonUnitCount()
        {
            var results = Capture();
            if (results.Length == 1)
            {
                // There was no variation, so just return the value we are now certain is right.
                return results.First().Key;
            }
            var resultsOrdered = results.OrderByDescending(x => x.Count()).ToList();

            var first = resultsOrdered.First();
            var second = resultsOrdered.Skip(1).First();

            int diff = first.Count() - second.Count();

            if (diff <= 2)
            {
                throw new InsufficientMatchingSamplesException("There is no clear winner. Try again. Make sure you hold the button until it is captured, then let go before pressing it again.");
            }

            return first.Key;
        }
    }
}
