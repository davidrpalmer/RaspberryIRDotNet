using System;

namespace RaspberryIRDotNetExamples.Demos
{
    class FormatSender : Demo
    {
        public override string Name => "Format Sender";

        public override void Run()
        {
            RequireTx();

            Console.WriteLine("This demo will send some hard coded IR for various packet formats.");
            Console.WriteLine("It's not much use as an interactive demo, but the source code shows how to send");
            Console.WriteLine("formatted IR packets with hard coded values.");

            Panasonic();
            NEC();
            RC5();
            RC6Mode0();
            RC6Mode6AXbox360();
        }

        private void Panasonic()
        {
            Console.WriteLine();
            Console.WriteLine("Kasiekyo Panasonic");

            var converter = new RaspberryIRDotNet.PacketFormats.Kasiekyo.KasiekyoBinaryConverter();
            var unitDuration = RaspberryIRDotNet.PacketFormats.Kasiekyo.KasiekyoBinaryConverter.KasiekyoStandardUnitDurationMicrosecs;
            var frequency = RaspberryIRDotNet.PacketFormats.Kasiekyo.KasiekyoBinaryConverter.KasiekyoStandardFrequency;

            var packet1 = new RaspberryIRDotNet.PacketFormats.Kasiekyo.PanasonicPacket(500, 1, 2, 3);
            var packet2 = new RaspberryIRDotNet.PacketFormats.Kasiekyo.PanasonicPacket(500, 10, 20, 30);

            SendIR(packet1, packet2, converter, unitDuration, frequency);
        }

        private void NEC()
        {
            Console.WriteLine();
            Console.WriteLine("NEC");

            var converter = new RaspberryIRDotNet.PacketFormats.NEC.NecBinaryConverter();
            var unitDuration = RaspberryIRDotNet.PacketFormats.NEC.NecBinaryConverter.NecStandardUnitDurationMicrosecs;
            var frequency = RaspberryIRDotNet.PacketFormats.NEC.NecBinaryConverter.NecStandardFrequency;

            var packet1 = new RaspberryIRDotNet.PacketFormats.NEC.NecExtendedPacket(123, 1);
            var packet2 = new RaspberryIRDotNet.PacketFormats.NEC.NecExtendedPacket(123, 2);

            SendIR(packet1, packet2, converter, unitDuration, frequency);
        }

        private void RC5()
        {
            Console.WriteLine();
            Console.WriteLine("RC5");

            var converter = new RaspberryIRDotNet.PacketFormats.RC5.RC5Converter();
            var unitDuration = RaspberryIRDotNet.PacketFormats.RC5.RC5Converter.RC5StandardUnitDurationMicrosecs;
            var frequency = RaspberryIRDotNet.PacketFormats.RC5.RC5Converter.RC5StandardFrequency;

            var packet1 = new RaspberryIRDotNet.PacketFormats.RC5.RC5ExtendedPacket(true, 10, 1);
            var packet2 = new RaspberryIRDotNet.PacketFormats.RC5.RC5ExtendedPacket(false, 10, 2);

            SendIR(packet1, packet2, converter, unitDuration, frequency);
        }

        private void RC6Mode0()
        {
            Console.WriteLine();
            Console.WriteLine("RC6 Mode 0");
            var converter = new RaspberryIRDotNet.PacketFormats.RC6.Mode0.RC6Mode0Converter();

            var packet1 = new RaspberryIRDotNet.PacketFormats.RC6.Mode0.RC6Mode0Packet(true, 123, 1);
            var packet2 = new RaspberryIRDotNet.PacketFormats.RC6.Mode0.RC6Mode0Packet(false, 123, 2);

            SendRC6(packet1, packet2, converter);
        }

        private void RC6Mode6AXbox360()
        {
            Console.WriteLine();
            Console.WriteLine("RC6 Mode 6A - Xbox 360");
            var converter = new RaspberryIRDotNet.PacketFormats.RC6.Mode6A.Xbox360Converter();

            var basePacket = new RaspberryIRDotNet.PacketFormats.RC6.Mode6A.RC6Mode6A16Packet(32783);
            var packet1 = new RaspberryIRDotNet.PacketFormats.RC6.Mode6A.Xbox360Packet(basePacket, true, 29733); // B / red
            var packet2 = new RaspberryIRDotNet.PacketFormats.RC6.Mode6A.Xbox360Packet(basePacket, false, 29798); // A / green

            SendRC6(packet1, packet2, converter);
        }

        private void SendRC6<TPacket>(TPacket packet1, TPacket packet2, RaspberryIRDotNet.PacketFormats.IPulseSpacePacketConverter<TPacket> packetConverter) where TPacket : RaspberryIRDotNet.PacketFormats.RC6.RC6Packet
        {
            var unitDuration = RaspberryIRDotNet.PacketFormats.RC6.RC6Converter.RC6StandardUnitDurationMicrosecs;
            var frequency = RaspberryIRDotNet.PacketFormats.RC6.RC6Converter.RC6StandardFrequency;

            SendIR(packet1, packet2, packetConverter, unitDuration, frequency);
        }

        private void SendIR<TPacket>(TPacket packet1, TPacket packet2, RaspberryIRDotNet.PacketFormats.IPulseSpacePacketConverter<TPacket> packetConverter, int unitDurationMicrosecs, int frequency) where TPacket : class, RaspberryIRDotNet.PacketFormats.IIRFormatPacket
        {
            var sender = new RaspberryIRDotNet.TX.PulseSpaceTransmitter_AutoOpenClose()
            {
                TransmissionDevice = DemoConfig.GetTxDevice(),
                Frequency = frequency,
                DutyCycle = RaspberryIRDotNet.IRDeviceDefaults.DutyCycle
            };

            var irAsUnits1 = packetConverter.ToIR(packet1);
            var irMessage1 = new RaspberryIRDotNet.IRPulseMessage(irAsUnits1, unitDurationMicrosecs);
            Console.WriteLine("  Send packet: " + packet1.ToString());
            sender.Send(irMessage1);

            var irAsUnits2 = packetConverter.ToIR(packet2);
            var irMessage2 = new RaspberryIRDotNet.IRPulseMessage(irAsUnits2, unitDurationMicrosecs);
            Console.WriteLine("  Send packet: " + packet2.ToString());
            sender.Send(irMessage2);
        }
    }
}
