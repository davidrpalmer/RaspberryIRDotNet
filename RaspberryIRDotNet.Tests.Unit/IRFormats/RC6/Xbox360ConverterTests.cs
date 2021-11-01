using System;
using NUnit.Framework;
using RaspberryIRDotNet.PacketFormats.RC6;
using RaspberryIRDotNet.PacketFormats.RC6.Mode6A;

namespace RaspberryIRDotNet.Tests.Unit.PacketFormats.RC6
{
    class Xbox360ConverterTests
    {
        private const string _xboxRed_press1 = "6,2,1,1,1,1,1,2,1,2,3,2,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,2,1,1,1,1,1,1,1,1,1,1,1,1,1,1,2,2,2,1,1,1,1,1,1,2,2,1,1,2,2,2";
        private const string _xboxRed_press2 = "6,2,1,1,1,1,1,2,1,2,3,2,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,2,1,1,1,1,1,1,2,2,1,1,1,1,2,2,2,1,1,1,1,1,1,2,2,1,1,2,2,2";

        private Xbox360Converter NewSubject() => new Xbox360Converter();

        [Test]
        public void ToXboxPacket()
        {
            // ARRANGE
            var subject = NewSubject();
            var irPress1 = PulseSpaceUnitList.LoadFromString(_xboxRed_press1);
            var irPress2 = PulseSpaceUnitList.LoadFromString(_xboxRed_press2);

            // ACT
            var decodedXbox1 = subject.ToPacket(irPress1);
            var decodedXbox2 = subject.ToPacket(irPress2);

            // ASSERT
            Assert.That(decodedXbox1.Mode, Is.EqualTo(new RC6Mode(6)));
            Assert.That(decodedXbox2.Mode, Is.EqualTo(new RC6Mode(6)));
            Assert.That(decodedXbox1.SubMode, Is.EqualTo('A'));
            Assert.That(decodedXbox2.SubMode, Is.EqualTo('A'));
            Assert.That(decodedXbox1.Trailer, Is.False);
            Assert.That(decodedXbox2.Trailer, Is.False);
            Assert.That(decodedXbox1.Address, Is.EqualTo(32783));
            Assert.That(decodedXbox2.Address, Is.EqualTo(32783));
            Assert.That(decodedXbox1.Command, Is.EqualTo(29733));
            Assert.That(decodedXbox2.Command, Is.EqualTo(29733));

            Assert.That(decodedXbox1.Toggle, Is.True);
            Assert.That(decodedXbox2.Toggle, Is.False);
        }

        [Test]
        public void ToRaw16PacketToXboxPacket()
        {
            // ARRANGE
            var subject = NewSubject();
            var rawConverter = new RC6Mode6ARawPayloadConverter();
            var irPress1 = PulseSpaceUnitList.LoadFromString(_xboxRed_press1);
            var irPress2 = PulseSpaceUnitList.LoadFromString(_xboxRed_press2);

            var decodedRaw1 = rawConverter.To16Packet(irPress1);
            var decodedRaw2 = rawConverter.To16Packet(irPress2);

            // ACT
            var decodedXbox1 = subject.ToPacket(decodedRaw1);
            var decodedXbox2 = subject.ToPacket(decodedRaw2);

            // ASSERT
            Assert.That(decodedXbox1.Mode, Is.EqualTo(new RC6Mode(6)));
            Assert.That(decodedXbox2.Mode, Is.EqualTo(new RC6Mode(6)));
            Assert.That(decodedXbox1.SubMode, Is.EqualTo('A'));
            Assert.That(decodedXbox2.SubMode, Is.EqualTo('A'));
            Assert.That(decodedXbox1.Trailer, Is.False);
            Assert.That(decodedXbox2.Trailer, Is.False);
            Assert.That(decodedXbox1.Address, Is.EqualTo(32783));
            Assert.That(decodedXbox2.Address, Is.EqualTo(32783));
            Assert.That(decodedXbox1.Command, Is.EqualTo(29733));
            Assert.That(decodedXbox2.Command, Is.EqualTo(29733));

            Assert.That(decodedXbox1.Toggle, Is.True);
            Assert.That(decodedXbox2.Toggle, Is.False);
        }

        [Test]
        public void XboxRoundTrip()
        {
            // ARRANGE
            var subject = NewSubject();
            var irPress1 = PulseSpaceUnitList.LoadFromString(_xboxRed_press1);
            var irPress2 = PulseSpaceUnitList.LoadFromString(_xboxRed_press2);

            // ACT
            var decoded1 = subject.ToPacket(irPress1);
            var decoded2 = subject.ToPacket(irPress2);

            string encoded1 = subject.ToIR(decoded1).SaveToString();
            string encoded2 = subject.ToIR(decoded2).SaveToString();

            // ASSERT
            Assert.That(encoded1, Is.EqualTo(_xboxRed_press1));
            Assert.That(encoded2, Is.EqualTo(_xboxRed_press2));
        }

        [Test]
        public void ToXboxPacket_TooShort()
        {
            // ARRANGE
            var subject = NewSubject();
            var irData = PulseSpaceUnitList.LoadFromString("6,2,1,1,1,1,1,2,1,2,3,2,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,2,1,1,1,1,1,1,1,1,1,1,1,1,1,1,2,2,2,1,1,1,1,1,1,2,2,1,1,2");

            // ACT, ASSERT
            Assert.That(() => subject.ToPacket(irData), Throws.TypeOf<Exceptions.InvalidPacketDataException>());
        }

        [Test]
        public void XboxPacketToIR()
        {
            // ARRANGE
            var subject = NewSubject();
            var goodPacket = new Xbox360Packet(new RC6Mode6A16Packet(500), false, 1);
            var badPacket = new Xbox360Packet(new RC6Mode6A16Packet(500), false, 65000); // Command is too big, it will overwrite the toggle bit.
            subject.ToIR(goodPacket); // Sanity check that the good packet works (other tests should cover this).

            // ACT, ASSERT
            Assert.That(() => subject.ToIR(badPacket), Throws.TypeOf<Exceptions.InvalidPacketDataException>());
        }
    }
}
