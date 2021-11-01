using System;
using System.Linq;

namespace RaspberryIRDotNetExamples.Demos
{
    class RawRepeater : Demo
    {
        public override string Name => "Raw IR Repeater";

        private RaspberryIRDotNet.TX.PulseSpaceTransmitter_ManualOpenClose _sender;
        private int? _roundTo;

        public override void Run()
        {
            RequireRx();
            RequireTx();

            Console.WriteLine("This demo will transmit any received IR signals. There is no filtering of");
            Console.WriteLine("signals, so noise will also be repeated.");
            Console.WriteLine();
            Console.WriteLine("If you want you can have the signals rounded off to a known PULSE/SPACE");
            Console.WriteLine("duration to help correct errors. Or you can just allow the raw signals through.");
            Console.Write("Round to (or leave blank): ");
            _roundTo = ReadInt32();

            Console.WriteLine();
            Console.WriteLine();

            _sender = new RaspberryIRDotNet.TX.PulseSpaceTransmitter_ManualOpenClose()
            {
                TransmissionDevice = DemoConfig.GetTxDevice()
            };
            var receiver = new RaspberryIRDotNet.RX.PulseSpaceSource.PulseSpaceCaptureLirc(DemoConfig.GetRxDevice());
            receiver.ReceivedPulseSpaceBurst += OnRx;

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Starting repeater. Use Ctrl+C to stop.");
            Console.WriteLine();
            Console.WriteLine();
            _sender.Open();
            receiver.Capture(null);
        }

        private void OnRx(object s, RaspberryIRDotNet.RX.PulseSpaceSource.ReceivedPulseSpaceBurstEventArgs e)
        {
            if (_roundTo.HasValue)
            {
                var roundedBuffer = e.Buffer.Copy(_roundTo.Value);
                if (roundedBuffer.Any(x => x <= 0))
                {
                    /// The rounding process has produced a zero. Either <see cref="_roundTo"/> has been set to the wrong value, or this was background noise.
                    /// We need to drop it because if we try to transmit this we will get an error since zero length PULSES/SPACES are not valid.
                    Console.Write("-");
                    return;
                }
                _sender.Send(roundedBuffer);
            }
            else
            {
                _sender.Send(e.Buffer);
            }
            Console.Write("+");
        }

        private int? ReadInt32()
        {
            string str = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(str))
            {
                return null;
            }
            return int.Parse(str);
        }
    }
}
