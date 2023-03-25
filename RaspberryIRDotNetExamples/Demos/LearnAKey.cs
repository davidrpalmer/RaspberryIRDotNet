using System;
using System.Linq;

namespace RaspberryIRDotNetExamples.Demos
{
    class LearnAKey : Demo
    {
        public override string Name => "Learn a Key, then play it back";

        public override void Run()
        {
            RaspberryIRDotNet.IRPulseMessage key1, key2;

            RequireRx();

            RaspberryIRDotNet.RX.PulseSpaceSource.PreRecordedSource capturedIR;
            RaspberryIRDotNet.PacketFormats.FormatGuesser.Format guessedFormat = null;

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("Press different buttons at random (but don't hold them).");
                Console.WriteLine("A plus symbol will appear each time a potentially good burst is recognised. A minus symbol will appear for each obviously bad burst that has been filtered out.");
                Console.Write("Press any key on the Pi's keyboard when you are ready to begin.");
                Console.ReadKey(true);
                Console.WriteLine();
                Console.Write("> ");
                var recorder = new RaspberryIRDotNet.RX.RawRecorder(DemoConfig.GetRxDevice());
                recorder.Received += (s, e) =>
                {
                    Console.Write("+");

                    guessedFormat = GuessFormat(e.CapturedSoFar);
                    if (guessedFormat != null)
                    {
                        // We already know what format it is, no need to capture any more.
                        e.StopCapture = true;
                    }
                };
                recorder.Filtered += (s, e) => Console.Write("-");
                capturedIR = recorder.Record(new RaspberryIRDotNet.RX.ReadCancellationToken(TimeSpan.FromSeconds(55)));
                if (guessedFormat == null)
                {
                    guessedFormat = GuessFormat(capturedIR);
                }
                if (guessedFormat == null && capturedIR.Bursts.Count < 30)
                {
                    Console.WriteLine("Not enough samples in the allotted time. Press space or enter to try again. Any other key to quit.");
                    var userAnswer = Console.ReadKey(true).Key;
                    switch (userAnswer)
                    {
                        case ConsoleKey.Enter:
                        case ConsoleKey.Spacebar:
                            break;
                        default:
                            return;
                    }
                }
                else
                {
                    break;
                }
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();

            LearnAKey_KeyLearner keyLearner;
            if (guessedFormat != null)
            {
                keyLearner = new LearnAKey_Format(guessedFormat);
            }
            else
            {
                keyLearner = new LearnAKey_Raw();
            }

            (key1, key2) = keyLearner.LearnKeys(capturedIR);


            Console.WriteLine("IR codes learnt. Now press 1 or 2 on the Pi's keyboard to transmit those IR codes.");

            RequireTx();

            using (var sender = new RaspberryIRDotNet.TX.PulseSpaceTransmitter_ManualOpenClose()
            {
                TransmissionDevice = DemoConfig.GetTxDevice()
            })
            {
                sender.Open();
                while (true)
                {
                    var choice = Console.ReadKey();
                    if (choice.Key == ConsoleKey.Escape)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Escape pressed, quitting.");
                        return;
                    }
                    else if (choice.KeyChar == '1')
                    {
                        sender.Send(key1);
                    }
                    else if (choice.KeyChar == '2')
                    {
                        sender.Send(key2);
                    }
                    else
                    {
                        Console.WriteLine();
                        Console.WriteLine("Press 1, 2 or escape.");
                    }
                }
            }
        }

        private static RaspberryIRDotNet.PacketFormats.FormatGuesser.Format GuessFormat(RaspberryIRDotNet.RX.PulseSpaceSource.PreRecordedSource capturedIR)
        {
            var formatGuesser = new RaspberryIRDotNet.PacketFormats.FormatGuesser();
            var formatGroups = capturedIR.Bursts.Select(burst => formatGuesser.GuessFormat(burst))
                .Where(format => format != null)
                .GroupBy(format => format)
                .OrderByDescending(x => x.Count())
                .ToArray();

            if (formatGroups.Length > 0 && formatGroups[0].Count() > 5)
            {
                return formatGroups[0].First();
            }

            return null;
        }
    }
}
