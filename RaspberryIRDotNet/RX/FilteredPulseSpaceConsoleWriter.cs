using System;
using System.Linq;
using RaspberryIRDotNet.RX.PulseSpaceSource;

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

        /// <param name="captureDevicePath">The IR capture device, example '/dev/lirc0'.</param>
        public FilteredPulseSpaceConsoleWriter(string captureDevicePath) : base(captureDevicePath)
        {
        }
        public FilteredPulseSpaceConsoleWriter(IPulseSpaceSource source) : base(source)
        {
        }

        protected override void OnReceiveIRPulseMessage(ReceivedPulseSpaceBurstEventArgs rawData, IRPulseMessage message)
        {
            if (CheckMessage(message))
            {
                bool pulse = true; // or space.

                if (rawData.Buffer.Count != message.PulseSpaceDurations.Count)
                {
                    throw new Exception("These should be the same length.");
                }

                var merged = rawData.Buffer
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
        }

        /// <summary>
        /// Start logging to the console.
        /// </summary>
        public void Start(ReadCancellationToken cancellationToken)
        {
            Capture(cancellationToken);
        }
    }
}
