using System;
using System.Linq;

namespace RaspberryIRDotNet.RX
{
    /// <summary>
    /// Basically the same as <see cref="PulseSpaceConsoleWriter"/> but with some filtering and clean up.
    /// Output columns are: Type, Original duration, Cleaned duration
    /// </summary>
    public class FilteredPulseSpaceConsoleWriter : CleanedUpIRCapture, IIRConsoleWriter
    {
        /// <summary>
        /// Option to override where the output is written to.
        /// </summary>
        public Action<string> ConsoleWriteLine { get; set; } = Console.WriteLine;

        protected override bool OnReceiveIRPulseMessage(PulseSpaceDurationList rawDataBuffer, IRPulseMessage message)
        {
            if (CheckMessage(message))
            {
                bool pulse = true; // or space.

                if (rawDataBuffer.Count != message.PulseSpaceDurations.Count)
                {
                    throw new Exception("These should be the same length.");
                }

                var merged = rawDataBuffer
                    .Zip(message.PulseSpaceDurations, (r, m) => new { Raw = r, Clean = m })
                    .Zip(message.PulseSpaceUnits, (durations, units) => new { Raw = durations.Raw, Clean = durations.Clean, Units = units });

                foreach (var item in merged)
                {
                    string type = pulse ? "PULSE" : "SPACE";

                    ConsoleWriteLine($"{type} {item.Raw.ToString().PadLeft(5)} {item.Clean.ToString().PadLeft(5)} {item.Units}");

                    pulse = !pulse;
                }
                ConsoleWriteLine($"----------------------   UnitCount:{message.UnitCount}");
            }

            return true;
        }

        /// <summary>
        /// Start logging to the console.
        /// </summary>
        public void Start()
        {
            CaptureFromDevice();
        }

        /// <summary>
        /// Stop the logging whenever possible, however it may be some time before the thread actually stops, if it ever stops.
        /// 
        /// Think of this as more of a notification that the logging is no longer needed and it MAY stop, rather than an instruction that it MUST stop.
        /// </summary>
        public void StopWhenPossible()
        {
            CancelReceiveWhenPossible();
        }
    }
}
