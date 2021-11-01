using System;
using NUnit.Framework;
using RaspberryIRDotNet.RX.Filters;

namespace RaspberryIRDotNet.Tests.Unit.RX.Filters
{
    class DurationFilterTests
    {
        private static PulseSpaceDurationList MakeSampleIRData()
        {
            return new PulseSpaceDurationList()
            {
                508,
                200,
                95,
                120
            };
        }

        [Test]
        public void Check_Default()
        {
            // ARRANGE
            var subject = new DurationFilter();
            var irData = MakeSampleIRData();

            // ACT
            var result = subject.Check(irData);

            // ASSERT
            Assert.That(result, Is.True);
        }

        [Test]
        [TestCase(94, ExpectedResult = true)]
        [TestCase(95, ExpectedResult = true)]
        [TestCase(96, ExpectedResult = false)]
        public bool Check_Minimum(int minimum)
        {
            // ARRANGE
            var subject = new DurationFilter()
            {
                Minimum = minimum
            };
            var irData = MakeSampleIRData();

            // ACT
            return subject.Check(irData);
        }

        [Test]
        [TestCase(507, ExpectedResult = false)]
        [TestCase(508, ExpectedResult = true)]
        [TestCase(509, ExpectedResult = true)]
        public bool Check_Maximum(int maximum)
        {
            // ARRANGE
            var subject = new DurationFilter()
            {
                Maximum = maximum
            };
            var irData = MakeSampleIRData();

            // ACT
            return subject.Check(irData);
        }
    }
}
