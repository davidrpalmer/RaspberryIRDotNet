using System;

namespace RaspberryIRDotNetExamples.Demos
{
    class LearnAKey : Demo
    {
        public override string Name => "Learn a Key, then play it back";

        public override void Run()
        {
            RequireRx();

            RaspberryIRDotNet.IReadOnlyPulseSpaceDurationList leadInDurations = LearnExpectedLeadInDurations();
            int unitDuration = LearnExpectedUnitDuration(leadInDurations);
            int unitCount = LearnExpectedUnitCount(leadInDurations, unitDuration);

            Console.WriteLine("IR parameters learnt. Now ready to learn individual buttons.");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();

            var key1 = LearnKey("1", leadInDurations, unitDuration, unitCount);
            var key2 = LearnKey("2", leadInDurations, unitDuration, unitCount);

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


        /// <param name="keyInfo">Examples: "any key", the "the 5 key"</param>
        private void WriteKeyPressInstructions(string keyInfo, bool varyKeys)
        {
            Console.WriteLine($"When the '>' symbol appears press {keyInfo} on the remote.");
            if (varyKeys)
            {
                Console.WriteLine($"Press different keys at random.");
            }
            Console.WriteLine("If there is an error then a '-' will appear, try pressing the button again.");
            Console.WriteLine("Once the signal has been received a '+' will appear. When this happens let");
            Console.WriteLine("go of the button. Do not let go until you see the '+'.");
            Console.WriteLine("Wait for the '>' symbol to appear again and repeat.");
        }

        private RaspberryIRDotNet.IRPulseMessage LearnKey(string keyName, RaspberryIRDotNet.IReadOnlyPulseSpaceDurationList leadInDurations, int unitDuration, int unitCount)
        {
            var recorder = new RaspberryIRDotNet.RX.IRMessageLearn()
            {
                CaptureDevice = DemoConfig.GetRxDevice(),
                UnitDurationMicrosecs = unitDuration,
                MessageMinimumUnitCount = unitCount,
                MessageMaximumUnitCount = unitCount
            };
            recorder.SetLeadInPatternAsMicrosecs(leadInDurations);

            SetUpRxFeedback(recorder);
            WriteKeyPressInstructions($"the '{keyName}' key", false);

            var result = recorder.LearnMessage();
            Console.WriteLine("Key captured.");
            Breather();
            return result;
        }

        private RaspberryIRDotNet.IReadOnlyPulseSpaceDurationList LearnExpectedLeadInDurations()
        {
            var learner = new RaspberryIRDotNet.RX.LeadInLearner()
            {
                CaptureDevice = DemoConfig.GetRxDevice()
            };
            learner.Received += (s, e) => Console.Write("#");

            Console.WriteLine("This step will try to learn the lead-in pattern that prefixes each IR message.");
            Console.WriteLine("Press buttons at random (but don't hold them). A # will appear each time a signal is recognised.");

            var leadIn = learner.LearnLeadInDurations();
            Console.WriteLine();
            Console.WriteLine("Done, lead-in pattern (in microseconds) is: " + string.Join(",", leadIn));
            Breather();
            return leadIn;
        }

        private int LearnExpectedUnitCount(RaspberryIRDotNet.IReadOnlyPulseSpaceDurationList leadInDurations, int unitDuration)
        {
            var counter = new RaspberryIRDotNet.RX.IRUnitCounter()
            {
                CaptureDevice = DemoConfig.GetRxDevice(),
                UnitDurationMicrosecs = unitDuration,
            };
            counter.SetLeadInPatternAsMicrosecs(leadInDurations);

            SetUpRxFeedback(counter);

            Console.WriteLine("This step will try to learn how many units each IR message is.");
            WriteKeyPressInstructions("any key", true);

            int unitCount = counter.CaptureAndGetMostCommonUnitCount();
            Console.WriteLine();
            Console.WriteLine("Done, unit count is " + unitCount);
            Breather();
            return unitCount;
        }

        private void SetUpRxFeedback(RaspberryIRDotNet.RX.IMultipleCapture ir)
        {
            ir.Waiting += (s, e) => Console.Write(">");
            ir.Hit += (s, e) => Console.WriteLine(" +");
            ir.Miss += (s, e) => Console.Write(" -");
        }

        private int LearnExpectedUnitDuration(RaspberryIRDotNet.IReadOnlyPulseSpaceDurationList leadInDurations)
        {
            var learner = new RaspberryIRDotNet.RX.UnitDurationLearner()
            {
                CaptureDevice = DemoConfig.GetRxDevice(),
                LeadInPatternDurations = leadInDurations,
            };
            SetUpRxFeedback(learner);

            Console.WriteLine("This step will try to learn the unit duration of the IR message.");
            WriteKeyPressInstructions("any key", true);

            int duration = learner.LearnUnitDuration();
            Console.WriteLine();
            Console.WriteLine($"Done, unit duration is: {duration} microseconds.");
            Breather();
            return duration;
        }

        /// <summary>
        /// Give the user chance to stop pressing buttons in between tests so we don't roll from one test straight into another.
        /// </summary>
        private void Breather()
        {
            System.Threading.Thread.Sleep(1000);
            Console.WriteLine();
            Console.WriteLine();
        }
    }
}
