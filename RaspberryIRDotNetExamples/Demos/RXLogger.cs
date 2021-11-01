using System;

namespace RaspberryIRDotNetExamples.Demos
{
    class RXLogger : Demo
    {
        public override string Name => "Receive Logger";

        public override void Run()
        {
            RequireRx();

            Console.WriteLine("This demo will basically do the same as the \"ir-ctl -r\" command.");

            bool filter;
            while (true)
            {
                Console.WriteLine("Press F to start in filtered mode or U to start unfiltered mode..");
                var key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.U)
                {
                    filter = false;
                    break;
                }
                if (key.Key == ConsoleKey.F)
                {
                    filter = true;
                    break;
                }
            }
            Console.WriteLine();
            Console.WriteLine();

            if (filter)
            {
                Console.Write("Unit duration (microsecs):");
                int unitDuration = int.Parse(Console.ReadLine());

                Console.Write("Lead in pattern (example: 16, 4):");
                var leadIn = RaspberryIRDotNet.PulseSpaceUnitList.LoadFromString(Console.ReadLine());

                var receive = new RaspberryIRDotNet.RX.FilteredPulseSpaceConsoleWriter(DemoConfig.GetRxDevice())
                {
                    UnitDurationMicrosecs = unitDuration
                };
                receive.SetLeadInPatternFilterByUnits(leadIn);

                Console.WriteLine();
                receive.Start(null);
            }
            else
            {
                var receive = new RaspberryIRDotNet.RX.PulseSpaceConsoleWriter(DemoConfig.GetRxDevice());
                receive.Start(null);
            }

        }
    }
}
