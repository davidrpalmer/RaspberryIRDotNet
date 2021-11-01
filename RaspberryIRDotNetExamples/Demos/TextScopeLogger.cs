using System;
using System.Text;
using RaspberryIRDotNet;
using RaspberryIRDotNet.RX;
using RaspberryIRDotNet.RX.PulseSpaceSource;

namespace RaspberryIRDotNetExamples.Demos
{
    class TextScopeLogger : Demo
    {
        public override string Name => "Log IR to an on screen scope";

        public override void Run()
        {
            RequireRx();

            Console.WriteLine("This demo will (in text) the PULSE/SPACE units. 1 character = 1 unit");

            Console.Write("Unit duration (microsecs):");
            int unitDuration = int.Parse(Console.ReadLine());

            var receive = new TextScopeLoggerWorker(DemoConfig.GetRxDevice())
            {
                UnitDurationMicrosecs = unitDuration,
                WidthLimit = Console.BufferWidth
            };

            Console.WriteLine();
            receive.Start();
        }
    }

    public class TextScopeLoggerWorker : CleanedUpIRCapture
    {
        /// <summary>
        /// Set this to less than the console screen width.
        /// </summary>
        public int WidthLimit { get; set; } = 100;

        public TextScopeLoggerWorker(string captureDevicePath) : base(captureDevicePath)
        {
        }

        protected override void OnReceiveIRPulseMessage(ReceivedPulseSpaceBurstEventArgs rawData, IRPulseMessage message)
        {
            if (CheckMessage(message))
            {
                StringBuilder line = new StringBuilder();

                for (int i = 0; i < WidthLimit && i < message.PulseSpaceUnits.Count; i++)
                {
                    char c = message.PulseSpaceUnits.IsPulse(i) ? '█' : '░';
                    for (int i2 = 0; i2 < message.PulseSpaceUnits[i]; i2++)
                    {
                        line.Append(c);
                    }
                }
                Console.WriteLine(line);
            }
        }

        public void Start()
        {
            Capture(null);
        }
    }
}
