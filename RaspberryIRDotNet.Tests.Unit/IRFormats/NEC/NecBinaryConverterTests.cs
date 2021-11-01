using System;
using RaspberryIRDotNet.PacketFormats.BinaryConverters;
using RaspberryIRDotNet.PacketFormats.NEC;

namespace RaspberryIRDotNet.Tests.Unit.PacketFormats.NEC
{
    class NecBinaryConverterTests : PulseSpacePairWithOneAndThreeTestBase
    {
        protected override PulseSpaceUnitList NewPulseSpaceUnitListWithLeadIn() => new PulseSpaceUnitList(new byte[] { 16, 8 });

        protected override BinaryConverter NewSubject() => new NecBinaryConverter();
    }
}
