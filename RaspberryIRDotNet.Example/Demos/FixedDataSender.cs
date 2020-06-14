using System;

namespace RaspberryIRDotNetExamples.Demos
{
    class FixedDataSender : Demo
    {
        public override string Name => "Fixed Data Sender";

        public override void Run()
        {
            RequireTx();

            var sender = new RaspberryIRDotNet.TX.PulseSpaceTransmitter_AutoOpenClose()
            {
                TransmissionDevice = DemoConfig.GetTxDevice()
            };

            var buffer = new RaspberryIRDotNet.PulseSpaceDurationList()
            {
                9000, // PULSE
                7000, // SPACE
                100, // PULSE
                200, // SPACE
                300, // etc....
                400,
                500,
                600,
                700 // Last one must be a PULSE.
            };

            Console.WriteLine("This demo repeatedly sends the same fixed data over and over again.");
            Console.WriteLine("Use Ctrl+C to stop.");

            while (true)
            {
                sender.Send(buffer);
                System.Threading.Thread.Sleep(500);
            }
        }
    }
}
