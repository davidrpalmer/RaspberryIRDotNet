using System;
using NUnit.Framework;

namespace RaspberryIRDotNet.Tests.Unit
{
    public class PulseSpaceUnitListTests
    {
        [Test]
        public void Constructor_Capacity()
        {
            var buffer = new PulseSpaceUnitList(5);
            Assert.That(buffer.Capacity, Is.EqualTo(5));
            Assert.That(buffer.Count, Is.EqualTo(0));
            Assert.That(buffer.UnitCount, Is.EqualTo(0));
        }

        [Test]
        public void Constructor_WithValues()
        {
            var buffer = MakeSmallSampleBuffer();

            Assert.That(buffer.Count, Is.EqualTo(3));
            Assert.That(buffer.UnitCount, Is.EqualTo(4));
            Assert.That(buffer[0], Is.EqualTo(1));
            Assert.That(buffer[1], Is.EqualTo(2));
            Assert.That(buffer[2], Is.EqualTo(1));
        }

        [Test]
        public void Constructor_FromDurationList()
        {
            var buffer = new PulseSpaceUnitList(100, new PulseSpaceDurationList() { 100, 200, 310, 90 });

            Assert.That(buffer.Count, Is.EqualTo(4));
            Assert.That(buffer.UnitCount, Is.EqualTo(7));
            Assert.That(buffer[0], Is.EqualTo(1));
            Assert.That(buffer[1], Is.EqualTo(2));
            Assert.That(buffer[2], Is.EqualTo(3));
            Assert.That(buffer[3], Is.EqualTo(1));
        }

        [Test]
        public void Constructor_FromDurationListEmpty()
        {
            var buffer = new PulseSpaceUnitList(100, new PulseSpaceDurationList());

            Assert.That(buffer.Count, Is.EqualTo(0));
            Assert.That(buffer.UnitCount, Is.EqualTo(0));
        }

        [Test]
        public void Copy()
        {
            var buffer = MakeSmallSampleBuffer().Copy();

            Assert.That(buffer[0], Is.EqualTo(1));
            Assert.That(buffer[1], Is.EqualTo(2));
            Assert.That(buffer[2], Is.EqualTo(1));
        }

        [Test]
        public void IsPulseIsSpace()
        {
            var buffer = MakeSmallSampleBuffer();

            Assert.That(() => buffer.IsPulse(-1), Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => buffer.IsSpace(-1), Throws.Exception.TypeOf<ArgumentOutOfRangeException>());

            Assert.That(buffer.IsPulse(0), Is.True);
            Assert.That(buffer.IsSpace(0), Is.False);

            Assert.That(buffer.IsPulse(1), Is.False);
            Assert.That(buffer.IsSpace(1), Is.True);

            Assert.That(buffer.IsPulse(2), Is.True);
            Assert.That(buffer.IsSpace(2), Is.False);
        }

        [Test]
        public void ToFromString()
        {
            // ARRANGE
            var buffer = MakeSmallSampleBuffer();

            // ACT
            string str = buffer.SaveToString();
            PulseSpaceUnitList buffer2 = PulseSpaceUnitList.LoadFromString(str);

            // ASSERT
            Assert.That(buffer.Count, Is.EqualTo(3));
            Assert.That(buffer.UnitCount, Is.EqualTo(4));
            Assert.That(buffer[0], Is.EqualTo(1));
            Assert.That(buffer[1], Is.EqualTo(2));
            Assert.That(buffer[2], Is.EqualTo(1));

            Assert.That(buffer2.Count, Is.EqualTo(3));
            Assert.That(buffer2.UnitCount, Is.EqualTo(4));
            Assert.That(buffer2[0], Is.EqualTo(1));
            Assert.That(buffer2[1], Is.EqualTo(2));
            Assert.That(buffer2[2], Is.EqualTo(1));
        }

        private static PulseSpaceUnitList MakeSmallSampleBuffer()
        {
            return new PulseSpaceUnitList()
            {
                1,
                2,
                1
            };
        }
    }
}
