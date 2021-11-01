using System;
using RaspberryIRDotNet.RX;
using RaspberryIRDotNet.RX.Filters;
using RaspberryIRDotNet.PacketFormats.NEC;
using RaspberryIRDotNet.PacketFormats.Kasiekyo;
using RaspberryIRDotNet.PacketFormats.RC6;
using System.Linq;
using RaspberryIRDotNet.PacketFormats.RC5;

namespace RaspberryIRDotNetExamples.Demos
{
    class LogDecodedPackets : Demo
    {
        public override string Name => "Capture IR data and decode it into a packet format";

        public override void Run()
        {
            RequireRx();

            //var receiver = MakeNecBasicReceiver();
            var receiver = MakeNecExtendedReceiver();
            //var receiver = MakePanasonicReceiver();
            //var receiver = MakeRC5Receiver();
            //var receiver = MakeRC6Receiver();

            string packetType = receiver.GetType().GenericTypeArguments.Single().Name;

            Console.WriteLine($"Packet type: {packetType}       (uncomment another line in the code to use another format)");
            Console.WriteLine("Use a remote to send signals to the Pi.");
            Console.WriteLine("IR signals that match the filter(s) but are invalid packet data will cause an error.");
            Console.WriteLine();

            receiver.Start(null);
        }

        private PacketConsoleWriter<NecBasicPacket> MakeNecBasicReceiver()
        {
            var rx = new PacketConsoleWriter<NecBasicPacket>(new NecBinaryConverter(), DemoConfig.GetRxDevice())
            {
                UnitDurationMicrosecs = NecBinaryConverter.NecStandardUnitDurationMicrosecs
            };
            rx.RXFilters.Filters.Add(new NecRxFilter());
            return rx;
        }

        private PacketConsoleWriter<NecExtendedPacket> MakeNecExtendedReceiver()
        {
            var rx = new PacketConsoleWriter<NecExtendedPacket>(new NecBinaryConverter(), DemoConfig.GetRxDevice())
            {
                UnitDurationMicrosecs = NecBinaryConverter.NecStandardUnitDurationMicrosecs
            };
            rx.RXFilters.Filters.Add(new NecRxFilter());
            return rx;
        }

        private PacketConsoleWriter<PanasonicPacket> MakePanasonicReceiver()
        {
            var rx = new PacketConsoleWriter<PanasonicPacket>(new KasiekyoBinaryConverter(), DemoConfig.GetRxDevice())
            {
                UnitDurationMicrosecs = KasiekyoBinaryConverter.KasiekyoStandardUnitDurationMicrosecs
            };
            rx.RXFilters.Filters.Add(new PanasonicKasiekyoRxFilter());
            return rx;
        }

        private PacketConsoleWriter<RC5ExtendedPacket> MakeRC5Receiver()
        {
            // Using RC5 extended rather than basic since extended is backwards compatible for low value commands.
            var rx = new PacketConsoleWriter<RC5ExtendedPacket>(new RC5Converter(), DemoConfig.GetRxDevice())
            {
                UnitDurationMicrosecs = RC5Converter.RC5StandardUnitDurationMicrosecs
            };
            rx.RXFilters.Filters.Add(new RC5RxFilter());
            return rx;
        }

        private PacketConsoleWriter<RC6Packet> MakeRC6Receiver()
        {
            var rx = new PacketConsoleWriter<RC6Packet>(new RC6AutoDetectingConverter(), DemoConfig.GetRxDevice())
            {
                UnitDurationMicrosecs = RC6Converter.RC6StandardUnitDurationMicrosecs
            };
            rx.RXFilters.Filters.Add(new RC6RxFilter());
            return rx;
        }
    }
}
