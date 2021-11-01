using System;
using NUnit.Framework;
using RaspberryIRDotNet.PacketFormats;

namespace RaspberryIRDotNet.Tests.Unit.PacketFormats
{
    class PointInTimeSamplerTests
    {
        private PointInTimeSampler NewSubject() => new PointInTimeSampler();

        [Test]
        public void FromIR()
        {
            // ARRANGE
            var subject = NewSubject();

            var input = new PulseSpaceUnitList()
            {
                3, // P
                2, // S
                1 // P
            };

            // ACT
            var result = subject.FromIR(input);

            // ASSERT
            Assert.That(result.Count, Is.EqualTo(6));
            Assert.That(result[0], Is.True);
            Assert.That(result[1], Is.True);
            Assert.That(result[2], Is.True);
            Assert.That(result[3], Is.False);
            Assert.That(result[4], Is.False);
            Assert.That(result[5], Is.True);
        }

        [Test]
        public void FromIR_StartAt()
        {
            // ARRANGE
            var subject = NewSubject();

            var input = new PulseSpaceUnitList()
            {
                3, // P
                2, // S
                1 // P
            };

            // ACT
            var result = subject.FromIR(input, startAtIndex: 1);

            // ASSERT
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result[0], Is.False);
            Assert.That(result[1], Is.False);
            Assert.That(result[2], Is.True);
        }

        [Test]
        public void FromIR_StartAt_TooLow()
        {
            // ARRANGE
            var subject = NewSubject();

            var input = new PulseSpaceUnitList()
            {
                3, // P
                2, // S
                1 // P
            };

            // ACT, ASSERT
            Assert.That(() => subject.FromIR(input, startAtIndex: -1), Throws.TypeOf<ArgumentOutOfRangeException>().With.Property("ParamName").EqualTo("startAtIndex"));
        }

        [Test]
        public void FromIR_StartAt_TooHigh()
        {
            // ARRANGE
            var subject = NewSubject();

            var input = new PulseSpaceUnitList()
            {
                3, // P
                2, // S
                1 // P
            };

            // ACT
            var result = subject.FromIR(input, startAtIndex: 5);

            // ASSERT
            Assert.That(result.Count, Is.EqualTo(0));
        }
    }
}
