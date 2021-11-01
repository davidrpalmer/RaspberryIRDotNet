using System;
using System.Collections.Generic;
using System.Text;

namespace RaspberryIRDotNetExamples.Demos
{
    class LearnAKey_Raw : LearnAKey_KeyLearner
    {
        public override (RaspberryIRDotNet.IRPulseMessage key1, RaspberryIRDotNet.IRPulseMessage key2) LearnKeys(RaspberryIRDotNet.RX.PulseSpaceSource.PreRecordedSource capturedIR)
        {
            Console.WriteLine("Format not recognised. Falling back to basic PULSE/SPACE capture.");

            RaspberryIRDotNet.IReadOnlyPulseSpaceDurationList leadInDurations = null;
            int unitDuration = 0;

            try
            {
                var leadinLearner = new RaspberryIRDotNet.RX.LeadInLearner(capturedIR);
                leadInDurations = leadinLearner.LearnLeadInDurations();
                Console.WriteLine("Lead-in pattern (in microseconds) is: " + string.Join(",", leadInDurations));

                var unitDurationLearner = new RaspberryIRDotNet.RX.UnitDurationLearner(capturedIR)
                {
                    LeadInPatternDurations = leadInDurations,
                };
                unitDuration = unitDurationLearner.LearnUnitDuration();
                Console.WriteLine($"Unit duration is: {unitDuration} microseconds.");
            }
            catch (System.IO.EndOfStreamException)
            {
                // The pre-recorded data was not enough. We need to ask the user for more data.

                Console.WriteLine();
                Console.WriteLine();

                if (leadInDurations == null)
                {
                    leadInDurations = LearnExpectedLeadInDurations();
                }

                if (unitDuration <= 0)
                {
                    unitDuration = LearnExpectedUnitDuration(leadInDurations);
                }
            }


            Console.WriteLine("IR parameters learnt. Now ready to learn individual buttons.");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();

            var key1 = LearnKey("1", leadInDurations, unitDuration);
            var key2 = LearnKey("2", leadInDurations, unitDuration);

            return (key1, key2);
        }

        private RaspberryIRDotNet.IRPulseMessage LearnKey(string keyName, RaspberryIRDotNet.IReadOnlyPulseSpaceDurationList leadInDurations, int unitDuration)
        {
            var recorder = new RaspberryIRDotNet.RX.IRMessageLearn(DemoConfig.GetRxDevice())
            {
                UnitDurationMicrosecs = unitDuration
            };
            recorder.RXFilters.Filters.Add(new RaspberryIRDotNet.RX.Filters.LeadInPatternFilter(leadInDurations));

            SetUpRxFeedback(recorder);
            WriteKeyPressInstructions($"the '{keyName}' key", false);

            var result = recorder.LearnMessage();
            Console.WriteLine("Key captured.");
            Breather();
            return result;
        }

        private int LearnExpectedUnitDuration(RaspberryIRDotNet.IReadOnlyPulseSpaceDurationList leadInDurations)
        {
            var learner = new RaspberryIRDotNet.RX.UnitDurationLearner(DemoConfig.GetRxDevice())
            {
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

        private RaspberryIRDotNet.IReadOnlyPulseSpaceDurationList LearnExpectedLeadInDurations()
        {
            var learner = new RaspberryIRDotNet.RX.LeadInLearner(DemoConfig.GetRxDevice());
            learner.Received += (s, e) => Console.Write("#");

            Console.WriteLine("This step will try to learn the lead-in pattern that prefixes each IR message.");
            Console.WriteLine("Press buttons at random (but don't hold them). A # will appear each time a signal is recognised.");

            var leadIn = learner.LearnLeadInDurations();
            Console.WriteLine();
            Console.WriteLine("Done, lead-in pattern (in microseconds) is: " + string.Join(",", leadIn));
            Breather();
            return leadIn;
        }

    }
}
