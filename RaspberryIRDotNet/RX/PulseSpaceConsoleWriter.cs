using System;
using RaspberryIRDotNet.RX.PulseSpaceSource;

namespace RaspberryIRDotNet.RX
{
    /// <summary>
    /// Write the output from the IR device to the console.
    /// Basically does the same thing as the "ir-ctl -r" command.
    /// </summary>
    public class PulseSpaceConsoleWriter : IIRConsoleWriter
    {
        /// <summary>
        /// Option to override where the output is written to.
        /// </summary>
        public Action<string> ConsoleWriteLine { get; set; } = Console.WriteLine;

        private readonly IPulseSpaceSource _captureSource;

        /// <param name="captureDevicePath">The IR capture device, example '/dev/lirc0'.</param>
        public PulseSpaceConsoleWriter(string captureDevicePath) : this(new PulseSpaceCaptureLirc(captureDevicePath))
        {
            if (string.IsNullOrWhiteSpace(captureDevicePath))
            {
                throw new ArgumentNullException(nameof(captureDevicePath));
            }
        }
        public PulseSpaceConsoleWriter(IPulseSpaceSource source)
        {
            _captureSource = source ?? throw new ArgumentNullException(nameof(source));
            _captureSource.ReceivedPulseSpaceBurst += ReceivedPulseSpaceBurst;
        }

        private void ReceivedPulseSpaceBurst(object sender, ReceivedPulseSpaceBurstEventArgs e)
        {
            bool pulse = true; // or space.
            foreach (int item in e.Buffer)
            {
                string type = pulse ? "PULSE" : "SPACE";

                ConsoleWriteLine($"{type} {item.ToString().PadLeft(5)}");

                pulse = !pulse;
            }
            ConsoleWriteLine("----------------------");
        }

        /// <summary>
        /// Start logging to the console.
        /// </summary>
        public void Start(ReadCancellationToken cancellationToken)
        {
            _captureSource.Capture(cancellationToken);
        }
    }
}
