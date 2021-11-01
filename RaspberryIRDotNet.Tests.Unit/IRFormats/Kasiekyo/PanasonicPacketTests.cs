using System;
using NUnit.Framework;
using RaspberryIRDotNet.PacketFormats.Kasiekyo;

namespace RaspberryIRDotNet.Tests.Unit.PacketFormats.Kasiekyo
{
    class PanasonicPacketTests
    {
        [Test]
        public void ChangeValuesAndCalculateHash()
        {
            var subject = new PanasonicPacket()
            {
                Manufacturer = 61234,
                Device = 100,
                SubDevice = 50,
                Function = 4
            };

            Assert.That(subject.Manufacturer, Is.EqualTo(61234));
            Assert.That(subject.Device, Is.EqualTo(100));
            Assert.That(subject.SubDevice, Is.EqualTo(50));
            Assert.That(subject.Function, Is.EqualTo(4));
            Assert.That(subject.Hash, Is.EqualTo(0));
            Assert.That(subject.Validate(), Is.False);

            subject.SetCalculatedValues();

            Assert.That(subject.Manufacturer, Is.EqualTo(61234));
            Assert.That(subject.Device, Is.EqualTo(100));
            Assert.That(subject.SubDevice, Is.EqualTo(50));
            Assert.That(subject.Function, Is.EqualTo(4));
            Assert.That(subject.Hash, Is.EqualTo(82));
            Assert.That(subject.Validate(), Is.True);

            subject.Function = 5;

            Assert.That(subject.Manufacturer, Is.EqualTo(61234));
            Assert.That(subject.Device, Is.EqualTo(100));
            Assert.That(subject.SubDevice, Is.EqualTo(50));
            Assert.That(subject.Function, Is.EqualTo(5));
            Assert.That(subject.Hash, Is.EqualTo(82));
            Assert.That(subject.Validate(), Is.False);

            subject.SetCalculatedValues();

            Assert.That(subject.Manufacturer, Is.EqualTo(61234));
            Assert.That(subject.Device, Is.EqualTo(100));
            Assert.That(subject.SubDevice, Is.EqualTo(50));
            Assert.That(subject.Function, Is.EqualTo(5));
            Assert.That(subject.Hash, Is.EqualTo(83));
            Assert.That(subject.Validate(), Is.True);
        }

        [Test]
        public void ExplicitValuesConstructor()
        {
            // ACT
            var subject = new PanasonicPacket(61234, 100, 50, 4);

            // ASSERT
            Assert.That(subject.Manufacturer, Is.EqualTo(61234));
            Assert.That(subject.Device, Is.EqualTo(100));
            Assert.That(subject.SubDevice, Is.EqualTo(50));
            Assert.That(subject.Function, Is.EqualTo(4));
            Assert.That(subject.Hash, Is.EqualTo(82));
            Assert.That(subject.Validate(), Is.True);
        }

        [Test]
        public void FromBytes()
        {
            // ARRANGE
            var subject = new PanasonicPacket();

            // ACT
            subject.FromBytes(new byte[] { 50, 239, 100, 50, 4, 82 });

            // ASSERT
            Assert.That(subject.Manufacturer, Is.EqualTo(61234));
            Assert.That(subject.Device, Is.EqualTo(100));
            Assert.That(subject.SubDevice, Is.EqualTo(50));
            Assert.That(subject.Function, Is.EqualTo(4));
            Assert.That(subject.Hash, Is.EqualTo(82));
        }

        [Test]
        public void FromBytes_BadHash()
        {
            // ARRANGE
            var subject = new PanasonicPacket();

            // ACT
            subject.FromBytes(new byte[] { 50, 239, 100, 50, 4, 80 });

            // ASSERT
            Assert.That(subject.Manufacturer, Is.EqualTo(61234));
            Assert.That(subject.Device, Is.EqualTo(100));
            Assert.That(subject.SubDevice, Is.EqualTo(50));
            Assert.That(subject.Function, Is.EqualTo(4));
            Assert.That(subject.Hash, Is.EqualTo(80)); // The hash is wrong, but the class should not kick up a fuss, it should just hold the data it has been given.
        }

        [Test]
        public void FromBytes_LengthTooShort()
        {
            // ARRANGE
            var subject = new PanasonicPacket();

            // ACT, ASSERT
            Assert.That(() => subject.FromBytes(new byte[] { 50, 239, 100, 50, 4 }), Throws.Exception.TypeOf<Exceptions.InvalidPacketDataException>());
        }

        [Test]
        public void FromBytes_LengthTooLong()
        {
            // ARRANGE
            var subject = new PanasonicPacket();

            // ACT, ASSERT
            Assert.That(() => subject.FromBytes(new byte[] { 50, 239, 100, 50, 4, 80, 55 }), Throws.Exception.TypeOf<Exceptions.InvalidPacketDataException>());
        }

        [Test]
        public void FromBytes_Null()
        {
            // ARRANGE
            var subject = new PanasonicPacket();

            // ACT, ASSERT
            Assert.That(() => subject.FromBytes(null), Throws.ArgumentNullException);
        }

        [Test]
        public void ToBytes()
        {
            // ARRANGE
            byte[] input = new byte[] { 50, 239, 100, 50, 4, 82 };
            var subject = new PanasonicPacket(input);

            // ACT
            byte[] output = subject.ToBytes();

            // ASSERT
            Assert.That(output, Is.EqualTo(input));
        }

        [Test]
        public void Valid()
        {
            // ARRANGE
            var subject = new PanasonicPacket();
            subject.Device = 100;
            subject.SubDevice = 50;
            subject.Function = 4;
            subject.Hash = 82;

            // ACT, ASSERT
            Assert.That(subject.Validate(), Is.True);
        }

        [Test]
        public void Invalid()
        {
            // ARRANGE
            var subject = new PanasonicPacket();
            subject.Device = 100;
            subject.SubDevice = 50;
            subject.Function = 4;
            subject.Hash = 81;

            // ACT, ASSERT
            Assert.That(subject.Validate(), Is.False);
        }
    }
}
