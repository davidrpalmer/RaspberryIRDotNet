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

        [Test]
        public void IsCloseTo()
        {
            var buffer = MakeSmallSampleBuffer();

            Assert.That(() => buffer.IsCloseTo(-1, 100, 50), Throws.Exception.TypeOf<ArgumentOutOfRangeException>());

            Assert.That(buffer.IsCloseTo(0, 100, 0), Is.True);
            Assert.That(buffer.IsCloseTo(0, 100, 10), Is.True);

            Assert.That(buffer.IsCloseTo(0, 90, 10), Is.True);
            Assert.That(buffer.IsCloseTo(0, 110, 10), Is.True);

            Assert.That(buffer.IsCloseTo(0, 89, 10), Is.False);
            Assert.That(buffer.IsCloseTo(0, 111, 10), Is.False);
        }

        [Test]
        public void IsWithinPercent_NegativePercentages()
        {
            var buffer = MakeSmallSampleBuffer();

            Assert.That(() => buffer.IsWithinPercent(0, 100, 0.5), Throws.Nothing);
            Assert.That(() => buffer.IsWithinPercent(0, 100, -0.5), Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => buffer.IsWithinPercent(0, 100, -2), Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void IsWithinPercent()
        {
            var buffer = new PulseSpaceDurationList(System.Linq.Enumerable.Range(0, 200)); // Start from zero so we conveniently have index==value for all items.
            const double percent_5 = 0.05;

            Assert.That(() => buffer.IsWithinPercent(-1, 100, 50), Throws.Exception.TypeOf<ArgumentOutOfRangeException>());

            Assert.That(buffer.IsWithinPercent(99, 100, 0), Is.False);
            Assert.That(buffer.IsWithinPercent(100, 100, 0), Is.True);
            Assert.That(buffer.IsWithinPercent(101, 100, 0), Is.False);

            Assert.That(buffer.IsWithinPercent(94, 100, percent_5), Is.False);
            Assert.That(buffer.IsWithinPercent(95, 100, percent_5), Is.True);
            Assert.That(buffer.IsWithinPercent(96, 100, percent_5), Is.True);
            Assert.That(buffer.IsWithinPercent(99, 100, percent_5), Is.True);
            Assert.That(buffer.IsWithinPercent(100, 100, percent_5), Is.True);
            Assert.That(buffer.IsWithinPercent(101, 100, percent_5), Is.True);
            Assert.That(buffer.IsWithinPercent(104, 100, percent_5), Is.True);
            Assert.That(buffer.IsWithinPercent(105, 100, percent_5), Is.True);
            Assert.That(buffer.IsWithinPercent(106, 100, percent_5), Is.False);
            Assert.That(buffer.IsWithinPercent(107, 100, percent_5), Is.False);

            // 5% of 95 is 4.75 - so here we are basically checking that it is rounded to 5, not 4.
            Assert.That(buffer.IsWithinPercent(89, 95, percent_5), Is.False);
            Assert.That(buffer.IsWithinPercent(90, 95, percent_5), Is.True);
            Assert.That(buffer.IsWithinPercent(94, 95, percent_5), Is.True);
            Assert.That(buffer.IsWithinPercent(95, 95, percent_5), Is.True);
            Assert.That(buffer.IsWithinPercent(96, 95, percent_5), Is.True);
            Assert.That(buffer.IsWithinPercent(99, 95, percent_5), Is.True);
            Assert.That(buffer.IsWithinPercent(100, 95, percent_5), Is.True);
            Assert.That(buffer.IsWithinPercent(101, 95, percent_5), Is.False);

            // 5% of 105 is 5.25 - so here we are basically checking that it is rounded to 5, not 6.
            Assert.That(buffer.IsWithinPercent(99, 105, percent_5), Is.False);
            Assert.That(buffer.IsWithinPercent(104, 105, percent_5), Is.True);
            Assert.That(buffer.IsWithinPercent(105, 105, percent_5), Is.True);
            Assert.That(buffer.IsWithinPercent(106, 105, percent_5), Is.True);
            Assert.That(buffer.IsWithinPercent(110, 105, percent_5), Is.True);
            Assert.That(buffer.IsWithinPercent(111, 105, percent_5), Is.False);
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