using System;

namespace RaspberryIRDotNetExamples.Demos
{
    class IRDevListing : Demo
    {
        public override string Name => "List IR devices and info about them";

        public override void Run()
        {
            var results = new RaspberryIRDotNet.DeviceAssessment.DeviceAssessor().AssessAll();
            foreach (var result in results)
            {
                result.WriteToString(Console.WriteLine);
                Console.WriteLine();
            }
        }
    }
}
