using System;
using RaspberryIRDotNet.PacketFormats.BinaryConverters;
using RaspberryIRDotNet.PacketFormats.BinaryConverters.BitByteConverters;

namespace RaspberryIRDotNet.PacketFormats.NEC
{
    public class NecBinaryConverter : PulseSpacePairBinaryConverter, IPulseSpacePacketConverter<NecBasicPacket>, IPulseSpacePacketConverter<NecExtendedPacket>
    {
        public static IReadOnlyPulseSpaceUnitList NecStandardLeadInPattern { get; } = new PulseSpaceUnitList(new byte[] { 16, 8 }); // Not aware of any variations that use a different lead-in, but they might exist.

        public static int NecStandardUnitDurationMicrosecs { get; } = 562;

        public static int NecStandardFrequency { get; } = 38000;

        protected override IReadOnlyPulseSpaceUnitList _leadInPattern { get; } = NecStandardLeadInPattern;

        protected override IBitByteConverter _bitByteConverter => new LeastSignificantFirstBitByteConverter();

        protected override PulseSpacePairBitConverter _pulseSpacePairBitConverter { get; } = PulseSpacePairBitConverter.CreateOneAndThree();

        public PulseSpaceUnitList ToIR(NecBasicPacket packet) => ToIR(packet.ToBytes());

        public PulseSpaceUnitList ToIR(NecExtendedPacket packet) => ToIR(packet.ToBytes());

        NecBasicPacket IPulseSpacePacketConverter<NecBasicPacket>.ToPacket(IReadOnlyPulseSpaceUnitList irData) => ToBasicPacket(irData);

        NecExtendedPacket IPulseSpacePacketConverter<NecExtendedPacket>.ToPacket(IReadOnlyPulseSpaceUnitList irData) => ToExtendedPacket(irData);

        public NecBasicPacket ToBasicPacket(IReadOnlyPulseSpaceUnitList irData) => new NecBasicPacket(ToBytes(irData));

        public NecExtendedPacket ToExtendedPacket(IReadOnlyPulseSpaceUnitList irData) => new NecExtendedPacket(ToBytes(irData));
    }
}
