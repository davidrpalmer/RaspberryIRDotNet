using System;
using NUnit.Framework;
using RaspberryIRDotNet.Exceptions;
using RaspberryIRDotNet.PacketFormats.RC5;

namespace RaspberryIRDotNet.Tests.Unit.PacketFormats.RC5
{
    class RC5ConverterTests
    {
        private const string _sampleRC5Basic = "1,1,2,2,2,2,2,1,1,2,1,1,2,2,1,1,2";
        private const string _sampleRC5Extended = "2,1,1,2,2,2,2,1,1,2,1,1,2,2,1,1,2"; // Same as basic, but with the 2nd starter bit flipped to change the command from 54 to 118.

        private RC5Converter NewSubject() => new RC5Converter();

        [Test]
        public void RC5EndingPulseOrSpace()
        {
            // ARRANGE
            var subject = NewSubject();
            var packetEndWithPulse = new RC5BasicPacket(false, 20, 1);
            var packetEndWithSpace = new RC5BasicPacket(false, 20, 0);

            // ACT
            var ir1 = subject.ToIR(packetEndWithPulse);
            var ir2 = subject.ToIR(packetEndWithSpace);

            // ASSERT
            Assert.That(ir2.UnitCount, Is.EqualTo(ir1.UnitCount - 1), "When it ends with a SPACE the SPACE is just the end of the data, it isn't actually included in the data. So it should be one unit shorter.");
        }

        [Test]
        public void RC5XEndingPulseOrSpace()
        {
            // ARRANGE
            var subject = NewSubject();
            var packetEndWithPulse = new RC5ExtendedPacket(false, 20, 1);
            var packetEndWithSpace = new RC5ExtendedPacket(false, 20, 0);

            // ACT
            var ir1 = subject.ToIR(packetEndWithPulse);
            var ir2 = subject.ToIR(packetEndWithSpace);

            // ASSERT
            Assert.That(ir2.UnitCount, Is.EqualTo(ir1.UnitCount - 1), "When it ends with a SPACE the SPACE is just the end of the data, it isn't actually included in the data. So it should be one unit shorter.");
        }

        [Test]
        public void RC5RoundTrip()
        {
            // ARRANGE
            var subject = NewSubject();
            var irDataUnits = PulseSpaceUnitList.LoadFromString(_sampleRC5Basic);

            // ACT
            var packet = subject.ToRC5BasicPacket(irDataUnits);

            string irDataOut = subject.ToIR(packet).SaveToString();

            // ASSERT
            Assert.That(packet.Toggle, Is.False);
            Assert.That(packet.Address, Is.EqualTo(20));
            Assert.That(packet.Command, Is.EqualTo(54));

            Assert.That(irDataOut, Is.EqualTo(_sampleRC5Basic));
        }

        [Test]
        public void RC5XRoundTrip_RC5Compatible()
        {
            // ARRANGE
            var subject = NewSubject();
            var irDataUnits = PulseSpaceUnitList.LoadFromString(_sampleRC5Basic);

            // ACT
            var packet = subject.ToRC5ExtendedPacket(irDataUnits);

            string irDataOut = subject.ToIR(packet).SaveToString();

            // ASSERT
            Assert.That(packet.Toggle, Is.False);
            Assert.That(packet.Address, Is.EqualTo(20));
            Assert.That(packet.Command, Is.EqualTo(54));

            Assert.That(irDataOut, Is.EqualTo(_sampleRC5Basic));
        }

        [Test]
        public void RC5XRoundTrip_RC5Incompatible()
        {
            // ARRANGE
            var subject = NewSubject();
            var irDataUnits = PulseSpaceUnitList.LoadFromString(_sampleRC5Extended);

            // ACT
            var packet = subject.ToRC5ExtendedPacket(irDataUnits);

            string irDataOut = subject.ToIR(packet).SaveToString();

            // ASSERT
            Assert.That(packet.Toggle, Is.False);
            Assert.That(packet.Address, Is.EqualTo(20));
            Assert.That(packet.Command, Is.EqualTo(118));

            Assert.That(irDataOut, Is.EqualTo(_sampleRC5Extended));
        }

        [Test]
        public void InvalidData_TooManyConsecutivePulsesOrSpaces()
        {
            // ARRANGE
            var subject = NewSubject();
            var irDataIn = PulseSpaceUnitList.LoadFromString("1,1,2,2,2,2,2,1,1,2,1,1,2,2,1,1,3");

            // ACT, ASSERT
            Assert.That(() => subject.ToRC5BasicPacket(irDataIn), Throws.Exception.TypeOf<InvalidPacketDataException>());
            Assert.That(() => subject.ToRC5ExtendedPacket(irDataIn), Throws.Exception.TypeOf<InvalidPacketDataException>());
        }

        [Test]
        [TestCase("1,1,2,2,2,2,2,1,1,2,1,1,2,2,1")]
        [TestCase("1,1,2,2,2,2,2,1,1,2,1,1,2,2,1,1")]
        [TestCase("1,1,2,2,2,2,2,1,1,2,1,1,2,2,1,1,1")] // The last digit is a PULSE. It should be a 2, but is a 1.
        public void InvalidData_TooShort(string shortIRString)
        {
            // ARRANGE
            var subject = NewSubject();
            var irDataIn = PulseSpaceUnitList.LoadFromString(shortIRString);

            // ACT, ASSERT
            Assert.That(() => subject.ToRC5BasicPacket(irDataIn), Throws.Exception.TypeOf<InvalidPacketDataException>());
            Assert.That(() => subject.ToRC5ExtendedPacket(irDataIn), Throws.Exception.TypeOf<InvalidPacketDataException>());
        }
    }
}
