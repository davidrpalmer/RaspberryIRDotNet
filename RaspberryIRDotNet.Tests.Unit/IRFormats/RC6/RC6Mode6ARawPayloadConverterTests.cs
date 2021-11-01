using System;
using NUnit.Framework;
using RaspberryIRDotNet.Exceptions;
using RaspberryIRDotNet.PacketFormats.RC6.Mode6A;

namespace RaspberryIRDotNet.Tests.Unit.PacketFormats.RC6
{
    class RC6Mode6ARawPayloadConverterTests
    {
        private RC6Mode6ARawPayloadConverter NewSubject() => new RC6Mode6ARawPayloadConverter();

        [Test]
        public void EndingPulseOrSpace()
        {
            // ARRANGE
            var subject = NewSubject();
            var packetEndWithPulse = new RC6Mode6A8RawBitsPacket(1, new bool[] { false });
            var packetEndWithSpace = new RC6Mode6A8RawBitsPacket(1, new bool[] { true });

            // ACT
            var ir1 = subject.ToIR(packetEndWithPulse);
            var ir2 = subject.ToIR(packetEndWithSpace);

            // ASSERT
            Assert.That(ir2.UnitCount, Is.EqualTo(ir1.UnitCount - 1), "When it ends with a SPACE the SPACE is just the end of the data, it isn't actually included in the data. So it should be one unit shorter.");
        }

        [Test]
        public void RoundTrip()
        {
            // ARRANGE
            var subject = NewSubject();
            var irData1In = "6,2,1,1,1,1,1,2,1,2,2,1,1,1,1,1,1,1,1,1,1,1,1,1,2,2,1";
            var irData2In = "6,2,1,1,1,1,1,2,1,2,2,1,1,1,1,1,1,1,1,1,1,1,1,1,2,1,1";

            var irData1Units = PulseSpaceUnitList.LoadFromString(irData1In);
            var irData2Units = PulseSpaceUnitList.LoadFromString(irData2In);

            // ACT
            var ir1 = subject.To8Packet(irData1Units);
            var ir2 = subject.To8Packet(irData2Units);

            string irData1Out = subject.ToIR(ir1).SaveToString();
            string irData2Out = subject.ToIR(ir2).SaveToString();

            // ASSERT
            Assert.That(ir1.PayloadBits.Length, Is.EqualTo(1));
            Assert.That(ir2.PayloadBits.Length, Is.EqualTo(1));
            Assert.That(ir1.PayloadBits[0], Is.False);
            Assert.That(ir2.PayloadBits[0], Is.True);

            Assert.That(irData1Out, Is.EqualTo(irData1In));
            Assert.That(irData2Out, Is.EqualTo(irData2In));
        }

        [Test]
        public void InvalidData_TooManyConsecutivePulsesOrSpaces()
        {
            // ARRANGE
            var subject = NewSubject();
            var irDataIn = PulseSpaceUnitList.LoadFromString("6,2,1,1,1,1,1,2,1,2,2,1,1,1,1,1,1,1,1,1,1,1,1,1,2,5,1");

            // ACT, ASSERT
            Assert.That(() => subject.To8Packet(irDataIn), Throws.Exception.TypeOf<InvalidPacketDataException>());
        }

        [Test]
        [TestCase("6,2,1,1,1,1,1,2,1,2,2,1")]
        [TestCase("6,2,1,1,1,1,1,2,1,2,2,1,1,1,1,1,1,1,1,1")]
        [TestCase("6,2,1,1,1,1,1,2,1,2,2,1,1,1,1,1,1,1,1,1,1")]
        [TestCase("6,2,1,1,1,1,1,2,1,2,2,1,1,1,1,1,1,1,1,1,1,1")]
        [TestCase("6,2,1,1,1,1,1,2,1,2,2,1,1,1,1,1,1,1,1,1,1,1,1")]
        [TestCase("6,2,1,1,1,1,1,2,1,2,2,1,1,1,1,1,1,1,1,1,1,1,1,1")]
        public void InvalidData_TooShort(string shortIRString)
        {
            // ARRANGE
            var subject = NewSubject();
            var irDataIn = PulseSpaceUnitList.LoadFromString(shortIRString);

            // ACT, ASSERT
            Assert.That(() => subject.To8Packet(irDataIn), Throws.Exception.TypeOf<InvalidPacketDataException>());
        }
    }
}
