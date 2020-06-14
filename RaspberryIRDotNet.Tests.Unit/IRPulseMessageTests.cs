using System;
using NUnit.Framework;

namespace RaspberryIRDotNet.Tests.Unit
{
    class IRPulseMessageTests
    {
        [Test]
        public void Constructor_FromDuration()
        {
            // ACT
            IRPulseMessage message = new IRPulseMessage(new PulseSpaceDurationList()
            {
                88,
                106,
                200
            },
            100);

            // ASSERT
            Assert.That(message.PulseSpaceDurations[0], Is.EqualTo(100));
            Assert.That(message.PulseSpaceDurations[1], Is.EqualTo(100));
            Assert.That(message.PulseSpaceDurations[2], Is.EqualTo(200));
            Assert.That(message.PulseSpaceUnits[0], Is.EqualTo(1));
            Assert.That(message.PulseSpaceUnits[1], Is.EqualTo(1));
            Assert.That(message.PulseSpaceUnits[2], Is.EqualTo(2));
            Assert.That(message.UnitCount, Is.EqualTo(4));
            Assert.That(message.UnitDuration, Is.EqualTo(100));
        }

        [Test]
        public void Constructor_FromUnit()
        {
            // ACT
            IRPulseMessage message = new IRPulseMessage(new PulseSpaceUnitList()
            {
                1,
                1,
                2
            },
            100);

            // ASSERT
            Assert.That(message.PulseSpaceDurations[0], Is.EqualTo(100));
            Assert.That(message.PulseSpaceDurations[1], Is.EqualTo(100));
            Assert.That(message.PulseSpaceDurations[2], Is.EqualTo(200));
            Assert.That(message.PulseSpaceUnits[0], Is.EqualTo(1));
            Assert.That(message.PulseSpaceUnits[1], Is.EqualTo(1));
            Assert.That(message.PulseSpaceUnits[2], Is.EqualTo(2));
            Assert.That(message.UnitCount, Is.EqualTo(4));
            Assert.That(message.UnitDuration, Is.EqualTo(100));
        }

        [Test]
        public void Constructor_FromDuration_NullDurations()
        {
            IReadOnlyPulseSpaceDurationList list = null;
            Assert.That(() => new IRPulseMessage(list, 100), Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_FromUnit_NullUnits()
        {
            IReadOnlyPulseSpaceUnitList list = null;
            Assert.That(() => new IRPulseMessage(list, 100), Throws.ArgumentNullException);
        }

        [Test]
        [TestCase(int.MinValue)]
        [TestCase(-10)]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(2000000)]
        [TestCase(int.MaxValue)]
        public void Constructor_FromUnit_OutOfRangeUnitDuration(int unitDuration)
        {
            IReadOnlyPulseSpaceUnitList list = new PulseSpaceUnitList()
            {
                1,
                1,
                2
            };
            Assert.That(() => new IRPulseMessage(list, unitDuration), Throws.Exception.TypeOf<ArgumentOutOfRangeException>().With.Property("ParamName").EqualTo("unitDuration"));
        }

        [Test]
        [TestCase(int.MinValue)]
        [TestCase(-10)]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(2000000)]
        [TestCase(int.MaxValue)]
        public void Constructor_FromDuration_OutOfRangeUnitDuration(int unitDuration)
        {
            IReadOnlyPulseSpaceDurationList list = new PulseSpaceDurationList()
            {
                88,
                106,
                200
            };
            Assert.That(() => new IRPulseMessage(list, unitDuration), Throws.Exception.TypeOf<ArgumentOutOfRangeException>().With.Property("ParamName").EqualTo("unitDuration"));
        }

        private IRPulseMessage MakeSampleIRPulseMessage()
        {
            return new IRPulseMessage(new PulseSpaceDurationList()
            {
                88,
                106,
                200
            },
            100);
        }
    }
}
