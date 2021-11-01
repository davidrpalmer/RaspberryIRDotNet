using System;
using RaspberryIRDotNet.PacketFormats.BinaryConverters;
using RaspberryIRDotNet.PacketFormats.Kasiekyo;

namespace RaspberryIRDotNet.Tests.Unit.PacketFormats.Kasiekyo
{
    class KasiekyoBinaryConverterTests : PulseSpacePairWithOneAndThreeTestBase
    {
        protected override PulseSpaceUnitList NewPulseSpaceUnitListWithLeadIn() => new PulseSpaceUnitList(new byte[] { 8, 4 });

        protected override BinaryConverter NewSubject() => new KasiekyoBinaryConverter();
    }
}
