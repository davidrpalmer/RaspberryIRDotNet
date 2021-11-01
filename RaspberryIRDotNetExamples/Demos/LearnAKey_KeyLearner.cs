using System;

namespace RaspberryIRDotNetExamples.Demos
{
    abstract class LearnAKey_KeyLearner
    {
        public abstract (RaspberryIRDotNet.IRPulseMessage key1, RaspberryIRDotNet.IRPulseMessage key2) LearnKeys(RaspberryIRDotNet.RX.PulseSpaceSource.PreRecordedSource capturedIR);

        /// <param name="keyInfo">Examples: "any key", the "the 5 key"</param>
        protected void WriteKeyPressInstructions(string keyInfo, bool varyKeys)
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

        protected void SetUpRxFeedback(RaspberryIRDotNet.RX.IMultipleCapture ir)
        {
            ir.Waiting += (s, e) => Console.Write(">");
            ir.Hit += (s, e) => Console.WriteLine(" +");
            ir.Miss += (s, e) => Console.Write(" -");
        }

        /// <summary>
        /// Give the user chance to stop pressing buttons in between tests so we don't roll from one test straight into another.
        /// </summary>
        protected void Breather()
        {
            System.Threading.Thread.Sleep(1000);
            Console.WriteLine();
            Console.WriteLine();
        }
    }
}
