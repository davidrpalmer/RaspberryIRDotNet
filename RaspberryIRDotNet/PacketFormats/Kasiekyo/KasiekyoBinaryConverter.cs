using System;
using RaspberryIRDotNet.PacketFormats.BinaryConverters;
using RaspberryIRDotNet.PacketFormats.BinaryConverters.BitByteConverters;

namespace RaspberryIRDotNet.PacketFormats.Kasiekyo
{
    public class KasiekyoBinaryConverter : PulseSpacePairBinaryConverter, IPulseSpacePacketConverter<PanasonicPacket>
    {
        public static IReadOnlyPulseSpaceUnitList KasiekyoStandardLeadInPattern { get; } = new PulseSpaceUnitList(new byte[] { 8, 4 }); // Not aware of any variations that use a different lead-in, but they might exist.

        public static int KasiekyoStandardUnitDurationMicrosecs { get; } = 432;

        public static int KasiekyoStandardFrequency { get; } = 37000;

        protected override IReadOnlyPulseSpaceUnitList _leadInPattern { get; } = KasiekyoStandardLeadInPattern;

        protected override IBitByteConverter _bitByteConverter => new LeastSignificantFirstBitByteConverter();

        protected override PulseSpacePairBitConverter _pulseSpacePairBitConverter { get; } = PulseSpacePairBitConverter.CreateOneAndThree();

        public PulseSpaceUnitList ToIR(PanasonicPacket packet) => ToIR(packet.ToBytes());

        public PanasonicPacket ToPanasonicPacket(IReadOnlyPulseSpaceUnitList irData) => new PanasonicPacket(ToBytes(irData));

        PanasonicPacket IPulseSpacePacketConverter<PanasonicPacket>.ToPacket(IReadOnlyPulseSpaceUnitList irData) => ToPanasonicPacket(irData);
    }
}
