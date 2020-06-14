using System;

namespace RaspberryIRDotNet.RX
{
    /// <summary>
    /// Write the output from the IR device to the console.
    /// Basically does the same thing as the "ir-ctl -r" command.
    /// </summary>
    public class PulseSpaceConsoleWriter : PulseSpaceCapture, IIRConsoleWriter
    {
        /// <summary>
        /// Option to override where the output is written to.
        /// </summary>
        public Action<string> ConsoleWriteLine { get; set; } = Console.WriteLine;

        protected override bool OnReceivePulseSpaceBlock(PulseSpaceDurationList buffer)
        {
            bool pulse = true; // or space.
            foreach (int item in buffer)
            {
                string type = pulse ? "PULSE" : "SPACE";

                ConsoleWriteLine($"{type} {item.ToString().PadLeft(5)}");

                pulse = !pulse;
            }
            ConsoleWriteLine("----------------------");
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
