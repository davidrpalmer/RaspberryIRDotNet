using System;
using NUnit.Framework;
using RaspberryIRDotNet.RX.Filters;

namespace RaspberryIRDotNet.Tests.Unit.RX.Filters
{
    class RXFilterHelperTests
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
        public void LeadInErrorPercentage_DefaultValueIsOK()
        {
            // ARRANGE, ACT
            var value = new RXFilterHelper().LeadInErrorPercentage;

            // ASSERT
            Assert.That(value, Is.GreaterThanOrEqualTo(0.01).And.LessThanOrEqualTo(0.8)); // Arbitrary sanity check.
        }

        [Test]
        public void LeadInErrorPercentage_Set()
        {
            // ARRANGE
            var subject = new RXFilterHelper();

            // ACT, ASSERT
            Assert.That(() => subject.LeadInErrorPercentage = 0, Throws.Nothing);
            Assert.That(subject.LeadInErrorPercentage, Is.EqualTo(0));

            Assert.That(() => subject.LeadInErrorPercentage = 0.001, Throws.Nothing);
            Assert.That(subject.LeadInErrorPercentage, Is.EqualTo(0.001));

            Assert.That(() => subject.LeadInErrorPercentage = -0.001, Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(subject.LeadInErrorPercentage, Is.EqualTo(0.001));

            Assert.That(() => subject.LeadInErrorPercentage = 2.004, Throws.Nothing);
            Assert.That(subject.LeadInErrorPercentage, Is.EqualTo(2.004));

            Assert.That(() => subject.LeadInErrorPercentage = 9, Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(subject.LeadInErrorPercentage, Is.EqualTo(2.004));
        }

        [Test]
        public void Check_Default()
        {
            // ARRANGE
            var subject = new RXFilterHelper();
            var irData = MakeSampleIRData();

            // ACT
            var result = subject.Check(irData);

            // ASSERT
            Assert.That(result, Is.True);
        }

        [Test]
        [TestCase(-1, ExpectedResult = true)]
        [TestCase(0, ExpectedResult = true)]
        [TestCase(3, ExpectedResult = true)]
        [TestCase(4, ExpectedResult = true)]
        [TestCase(5, ExpectedResult = false)]
        [TestCase(6, ExpectedResult = false)]
        public bool Check_MinPulseSpaceCount(int minPulseSpaceCount)
        {
            // ARRANGE
            var subject = new RXFilterHelper()
            {
                MinPulseSpaceCount = minPulseSpaceCount
            };
            var irData = MakeSampleIRData();

            // ACT
            return subject.Check(irData);
        }

        [Test]
        [TestCase(3, ExpectedResult = false)]
        [TestCase(4, ExpectedResult = true)]
        [TestCase(5, ExpectedResult = true)]
        public bool Check_MaxPulseSpaceCount(int maxPulseSpaceCount)
        {
            // ARRANGE
            var subject = new RXFilterHelper()
            {
                MaxPulseSpaceCount = maxPulseSpaceCount
            };
            var irData = MakeSampleIRData();

            // ACT
            return subject.Check(irData);
        }

        [Test]
        public void Check_LeadIn_Match()
        {
            // ARRANGE
            var subject = new RXFilterHelper()
            {
                LeadIn = new PulseSpaceDurationList()
                {
                    500,
                    200,
                },
                LeadInErrorPercentage = 0.1
            };
            var irData = MakeSampleIRData();

            // ACT
            var result = subject.Check(irData);

            // ASSERT
            Assert.That(result, Is.True);
        }

        [Test]
        public void Check_LeadIn_NoMatch()
        {
            // ARRANGE
            var subject = new RXFilterHelper()
            {
                LeadIn = new PulseSpaceDurationList()
                {
                    500,
                    200,
                },
                LeadInErrorPercentage = 0.01 // 1% of 500 is 5. So because the actual IR data is 508 it should fail
            };
            var irData = MakeSampleIRData();

            // ACT
            var result = subject.Check(irData);

            // ASSERT
            Assert.That(result, Is.False);
        }

        [Test]
        public void SetLeadInByUnits()
        {
            // ARRANGE
            var subject = new RXFilterHelper();

            // ACT
            subject.SetLeadInByUnits(new PulseSpaceUnitList() { 3, 2 }, 100);

            // ASSERT
            Assert.That(subject.LeadIn, Is.Not.Null);
            Assert.That(subject.LeadIn.Count, Is.EqualTo(2));
            Assert.That(subject.LeadIn[0], Is.EqualTo(300));
            Assert.That(subject.LeadIn[1], Is.EqualTo(200));
        }

        [Test]
        [TestCase(-5)]  // The unit duration should be ignored, so any value should be OK.
        [TestCase(0)]
        [TestCase(100)]
        public void SetLeadInByUnits_Null(int unitDuration)
        {
            // ARRANGE
            var subject = new RXFilterHelper()
            {
                LeadIn = new PulseSpaceDurationList()
                {
                    500,
                    200,
                }
            };

            // ACT
            subject.SetLeadInByUnits(null, unitDuration);

            // ASSERT
            Assert.That(subject.LeadIn, Is.Null);
        }

        [Test]
        [TestCase(-5)]
        [TestCase(0)]
        [TestCase(Utility.UnitDurationMaximum + 1)]
        public void SetLeadInByUnits_BadUnitDuration(int unitDuration)
        {
            // ARRANGE
            var subject = new RXFilterHelper();

            // ACT, ASSERT
            Assert.That(() => subject.SetLeadInByUnits(new PulseSpaceUnitList() { 3, 2 }, unitDuration), Throws.TypeOf<ArgumentOutOfRangeException>());
        }
    }
}
