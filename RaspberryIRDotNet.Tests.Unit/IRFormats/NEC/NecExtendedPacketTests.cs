using System;
using NUnit.Framework;
using RaspberryIRDotNet.PacketFormats.NEC;

namespace RaspberryIRDotNet.Tests.Unit.PacketFormats.NEC
{
    class NecExtendedPacketTests
    {
        [Test]
        public void ChangeValuesAndCalculateHash()
        {
            var subject = new NecExtendedPacket()
            {
                Address = 61234,
                Command = 240
            };

            Assert.That(subject.Address, Is.EqualTo(61234));
            Assert.That(subject.Command, Is.EqualTo(240));
            Assert.That(subject.Command_Inverse, Is.EqualTo(0));
            Assert.That(subject.Validate(), Is.False);

            subject.SetCalculatedValues();

            Assert.That(subject.Address, Is.EqualTo(61234));
            Assert.That(subject.Command, Is.EqualTo(240));
            Assert.That(subject.Command_Inverse, Is.EqualTo(15));
            Assert.That(subject.Validate(), Is.True);

            subject.Command = 5;

            Assert.That(subject.Address, Is.EqualTo(61234));
            Assert.That(subject.Command, Is.EqualTo(5));
            Assert.That(subject.Command_Inverse, Is.EqualTo(15));
            Assert.That(subject.Validate(), Is.False);

            subject.SetCalculatedValues();

            Assert.That(subject.Address, Is.EqualTo(61234));
            Assert.That(subject.Command, Is.EqualTo(5));
            Assert.That(subject.Command_Inverse, Is.EqualTo(250));
            Assert.That(subject.Validate(), Is.True);
        }

        [Test]
        public void ExplicitValuesConstructor()
        {
            // ACT
            var subject = new NecExtendedPacket(61234, 240);

            // ASSERT
            Assert.That(subject.Address, Is.EqualTo(61234));
            Assert.That(subject.Command, Is.EqualTo(240));
            Assert.That(subject.Command_Inverse, Is.EqualTo(15));
            Assert.That(subject.Validate(), Is.True);
        }

        [Test]
        public void FromBytes()
        {
            // ARRANGE
            var subject = new NecExtendedPacket();

            // ACT
            subject.FromBytes(new byte[] { 50, 239, 240, 15 });

            // ASSERT
            Assert.That(subject.Address, Is.EqualTo(61234));
            Assert.That(subject.Command, Is.EqualTo(240));
            Assert.That(subject.Command_Inverse, Is.EqualTo(15));
        }

        [Test]
        public void FromBytes_BadHash()
        {
            // ARRANGE
            var subject = new NecExtendedPacket();

            // ACT
            subject.FromBytes(new byte[] { 50, 239, 240, 200 });
            
            // ASSERT
            Assert.That(subject.Address, Is.EqualTo(61234));
            Assert.That(subject.Command, Is.EqualTo(240));
            Assert.That(subject.Command_Inverse, Is.EqualTo(200)); // The hash is wrong, but the class should not kick up a fuss, it should just hold the data it has been given.
        }

        [Test]
        public void FromBytes_LengthTooShort()
        {
            // ARRANGE
            var subject = new NecExtendedPacket();

            // ACT, ASSERT
            Assert.That(() => subject.FromBytes(new byte[] { 50, 205, 240 }), Throws.Exception.TypeOf<Exceptions.InvalidPacketDataException>());
        }

        [Test]
        public void FromBytes_LengthTooLong()
        {
            // ARRANGE
            var subject = new NecExtendedPacket();

            // ACT, ASSERT
            Assert.That(() => subject.FromBytes(new byte[] { 50, 205, 240, 15, 20 }), Throws.Exception.TypeOf<Exceptions.InvalidPacketDataException>());
        }

        [Test]
        public void FromBytes_Null()
        {
            // ARRANGE
            var subject = new NecExtendedPacket();

            // ACT, ASSERT
            Assert.That(() => subject.FromBytes(null), Throws.ArgumentNullException);
        }

        [Test]
        public void ToBytes()
        {
            // ARRANGE
            byte[] input = new byte[] { 50, 239, 240, 15 };
            var subject = new NecExtendedPacket(input);

            // ACT
            byte[] output = subject.ToBytes();

            // ASSERT
            Assert.That(output, Is.EqualTo(input));
        }

        [Test]
        public void Valid()
        {
            // ARRANGE
            var subject = new NecExtendedPacket();
            subject.Address = 50;
            subject.Command = 100;
            subject.Command_Inverse = 155;

            // ACT, ASSERT
            Assert.That(subject.Validate(), Is.True);
        }

        [Test]
        public void Invalid()
        {
            // ARRANGE
            var subject = new NecExtendedPacket();
            subject.Address = 50;
            subject.Command = 100;
            subject.Command_Inverse = 100;

            // ACT, ASSERT
            Assert.That(subject.Validate(), Is.False);
        }
    }
}
