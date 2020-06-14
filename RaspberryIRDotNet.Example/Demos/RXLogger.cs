using System;

namespace RaspberryIRDotNetExamples.Demos
{
    class RXLogger : Demo
    {
        public override string Name => "Receive Logger";

        public override void Run()
        {
            RequireRx();
            var receive = new RaspberryIRDotNet.RX.PulseSpaceConsoleWriter() /// Could also use <see cref="RaspberryIRDotNet.RX.FilteredPulseSpaceConsoleWriter"/>
            {
                CaptureDevice = DemoConfig.GetRxDevice()
            };

            Console.WriteLine("This demo will basically do the same as the \"ir-ctl -r\" command.");
            Console.WriteLine("Press any key to start.");
            Console.ReadKey(true);
            Console.WriteLine();
            Console.WriteLine();

            receive.Start();
        }
    }
}
