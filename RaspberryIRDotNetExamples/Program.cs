using System;
using RaspberryIRDotNetExamples.Demos;

namespace RaspberryIRDotNetExamples
{
    class Program
    {
        private static readonly Demo[] _demos = new Demo[]
        {
            new IRDevListing(),
            new ScopeToy(),
            new RXLogger(),
            new TextScopeLogger(),
            new RawRepeater(),
            new FixedDataSender(),
            new LearnAKey(),
            new LogDecodedPackets(),
            new FormatSender(),
        };

        static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                RunDemo(args[0]);
                return;
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Example application to demonstrate how to use the RaspberryIRDotNet library.");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("You can also select a demo function by specifying its number as a command line option.");
            Console.WriteLine("Available demo functions:");
            for (int i = 0; i < _demos.Length; i++)
            {
                Console.WriteLine($"  {(i + 1).ToString().PadLeft(2)}) {_demos[i].Name}");
            }

            Console.Write("Pick a demo function: ");
            RunDemo(Console.ReadLine());
        }

        static void RunDemo(string indexStr)
        {
            if (string.IsNullOrWhiteSpace(indexStr))
            {
                return;
            }
            int index = int.Parse(indexStr.Trim());
            if (index <= 0)
            {
                return;
            }
            var demo = _demos[index - 1];
            Console.WriteLine();
            Console.WriteLine($"Running '{demo.Name}'");
            Console.WriteLine();
            demo.Run();
        }
    }
}
