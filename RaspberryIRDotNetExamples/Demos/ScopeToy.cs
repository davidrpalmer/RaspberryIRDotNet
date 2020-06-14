using System;
using System.Threading;

namespace RaspberryIRDotNetExamples.Demos
{
    class ScopeToy : Demo
    {
        public override string Name => "Scope Toy";

        public override void Run()
        {
            RequireTx();
            var test = new RaspberryIRDotNet.TX.TestTransmitter()
            {
                TransmissionDevice = DemoConfig.GetTxDevice(),
                Frequency = 9000,
                DutyCycle = 50,
                Gap = TimeSpan.Zero,
            };

            Console.WriteLine($"This demo demonstrates how to use the {nameof(RaspberryIRDotNet.TX.TestTransmitter)} class by playing with the frequency and duty cycle. Connect the Pi's IR output pin to an oscilloscope to see the results.");
            Console.WriteLine("Press any key to start.");
            Console.ReadKey(true);

            while (true)
            {
                Run_Inner(test);
                Console.WriteLine();
                Console.Write("Run again?");
                bool again = Console.ReadKey(true).Key == ConsoleKey.Y;
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();
                if (!again)
                {
                    return;
                }
            }
        }

        private void Run_Inner(RaspberryIRDotNet.TX.TestTransmitter test)
        {
            test.StartInBackground();
            Console.WriteLine("Transmitting.");
            Thread.Sleep(1000);
            Console.WriteLine("Testing carrier frequency.");
            while (true)
            {
                test.Frequency += 1000;
                Console.WriteLine($"  {test.Frequency / 1000}KHz");
                Thread.Sleep(200);
                if (test.Frequency >= 90000)
                {
                    break;
                }
            }
            Console.WriteLine("Finished carrier frequency test.");
            Thread.Sleep(2500);
            Console.WriteLine("Testing duty cycle.");
            test.Frequency = RaspberryIRDotNet.IRDeviceDefaults.Frequency;
            test.DutyCycle = 5;
            Thread.Sleep(2500);
            while (true)
            {
                test.DutyCycle += 1;
                Console.WriteLine($"  {test.DutyCycle.ToString().PadLeft(2)}%");
                Thread.Sleep(100);
                if (test.DutyCycle >= 90)
                {
                    break;
                }
            }
            Console.WriteLine("Finished duty cycle test.");
            test.DutyCycle = RaspberryIRDotNet.IRDeviceDefaults.DutyCycle;
            Thread.Sleep(1500);
            test.Stop();
            Console.WriteLine("Finished all tests, stopped transmission.");
        }
    }
}
