using System;
using NUnit.Framework;

namespace RaspberryIRDotNet.Tests.Unit
{
    public class PulseSpaceDurationListTests
    {
        [Test]
        public void Constructor_Capacity()
        {
            var buffer = new PulseSpaceDurationList(5);
            Assert.That(buffer.Capacity, Is.EqualTo(5));
            Assert.That(buffer.Count, Is.EqualTo(0));
        }

        [Test]
        public void Constructor_WithValues()
        {
            var buffer = MakeSmallSampleBuffer();

            Assert.That(buffer.Count, Is.EqualTo(3));
            Assert.That(buffer[0], Is.EqualTo(100));
            Assert.That(buffer[1], Is.EqualTo(200));
            Assert.That(buffer[2], Is.EqualTo(150));
        }

        [Test]
        public void Constructor_FromUnitList1()
        {
            var buffer = new PulseSpaceDurationList(100, new PulseSpaceUnitList() { 1, 2, 3, 1  });

            Assert.That(buffer.Count, Is.EqualTo(4));
            Assert.That(buffer[0], Is.EqualTo(100));
            Assert.That(buffer[1], Is.EqualTo(200));
            Assert.That(buffer[2], Is.EqualTo(300));
            Assert.That(buffer[3], Is.EqualTo(100));
        }

        [Test]
        public void Constructor_FromUnitList2()
        {
            var buffer = new PulseSpaceDurationList(500, new PulseSpaceUnitList() { 1, 2, 3, 1, 1 });

            Assert.That(buffer.Count, Is.EqualTo(5));
            Assert.That(buffer[0], Is.EqualTo(500));
            Assert.That(buffer[1], Is.EqualTo(1000));
            Assert.That(buffer[2], Is.EqualTo(1500));
            Assert.That(buffer[3], Is.EqualTo(500));
            Assert.That(buffer[4], Is.EqualTo(500));
        }

        [Test]
        public void Constructor_FromUnitListEmpty()
        {
            var buffer = new PulseSpaceDurationList(100, new PulseSpaceUnitList());

            Assert.That(buffer.Count, Is.EqualTo(0));
        }

        [Test]
        public void Copy()
        {
            var buffer = MakeSmallSampleBuffer().Copy();

            Assert.That(buffer[0], Is.EqualTo(100));
            Assert.That(buffer[1], Is.EqualTo(200));
            Assert.That(buffer[2], Is.EqualTo(150));
        }

        [Test]
        public void CopyWithRounding()
        {
            var buffer = new PulseSpaceDurationList()
            {
                97,
                120,
                180,
                3000,
                3009
            }.Copy(100);

            Assert.That(buffer[0], Is.EqualTo(100));
            Assert.That(buffer[1], Is.EqualTo(100));
            Assert.That(buffer[2], Is.EqualTo(200));
            Assert.That(buffer[3], Is.EqualTo(3000));
            Assert.That(buffer[4], Is.EqualTo(3000));
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

        private static PulseSpaceDurationList MakeSmallSampleBuffer()
        {
            return new PulseSpaceDurationList()
            {
                100,
                200,
                150
            };
        }
    }
}