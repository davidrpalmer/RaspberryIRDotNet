using System;
using NUnit.Framework;
using RaspberryIRDotNet.Exceptions;
using RaspberryIRDotNet.PacketFormats.BinaryConverters;

namespace RaspberryIRDotNet.Tests.Unit.PacketFormats
{
    abstract class PulseSpacePairWithOneAndThreeTestBase
    {
        /*
         * Sample data:
         *   Bits are 11000011
         *   Decimal is 195
         *   Hex is 0xC3
         */

        private PulseSpaceUnitList GetSampleIRData()
        {
            byte[] zero = new byte[] { 1, 1 };
            byte[] one = new byte[] { 1, 3 };

            var irData = NewPulseSpaceUnitListWithLeadIn();
            irData.AddRange(one);
            irData.AddRange(one);
            irData.AddRange(zero);
            irData.AddRange(zero);
            irData.AddRange(zero);
            irData.AddRange(zero);
            irData.AddRange(one);
            irData.AddRange(one);
            irData.Add(1); // Trailing pulse.
            return irData;
        }

        private byte[] GetSampleBinaryData()
        {
            return new byte[] { 195 };
        }

        protected abstract PulseSpaceUnitList NewPulseSpaceUnitListWithLeadIn();

        protected abstract BinaryConverter NewSubject();

        [Test]
        public void ToBytes()
        {
            // ARRANGE
            var subject = NewSubject();
            var irData = GetSampleIRData();

            // ACT
            byte[] decoded = subject.ToBytes(irData);

            // ASSERT
            Assert.That(decoded, Is.EqualTo(GetSampleBinaryData()));
        }

        [Test]
        public void ToBytes_Invalid_NoTrailingPulse()
        {
            // ARRANGE
            var subject = NewSubject();

            byte[] zero = new byte[] { 1, 1 };
            byte[] one = new byte[] { 1, 3 };

            var irData = NewPulseSpaceUnitListWithLeadIn();
            irData.AddRange(one);
            irData.AddRange(one);
            irData.AddRange(zero);
            irData.AddRange(zero);
            irData.AddRange(zero);
            irData.AddRange(zero);
            irData.AddRange(one);
            irData.AddRange(one);
            // missing the trailing pulse.

            // ACT, ASSERT
            Assert.That(() => subject.ToBytes(irData), Throws.Exception.TypeOf<InvalidPacketDataException>());
        }

        [Test]
        public void ToBytes_Invalid_BadPulse()
        {
            // ARRANGE
            var subject = NewSubject();

            byte[] zero = new byte[] { 1, 1 };
            byte[] one = new byte[] { 1, 3 };

            var irData = NewPulseSpaceUnitListWithLeadIn();
            irData.AddRange(one);
            irData.AddRange(one);
            irData.AddRange(zero);
            irData.AddRange(new byte[] { 2, 1 });
            irData.AddRange(zero);
            irData.AddRange(zero);
            irData.AddRange(one);
            irData.AddRange(one);
            irData.Add(1); // Trailing pulse.

            // ACT, ASSERT
            Assert.That(() => subject.ToBytes(irData), Throws.Exception.TypeOf<InvalidPacketDataException>());
        }

        [Test]
        public void ToBytes_Invalid_BadSpace()
        {
            // ARRANGE
            var subject = NewSubject();

            byte[] zero = new byte[] { 1, 1 };
            byte[] one = new byte[] { 1, 3 };

            var irData = NewPulseSpaceUnitListWithLeadIn();
            irData.AddRange(one);
            irData.AddRange(one);
            irData.AddRange(zero);
            irData.AddRange(new byte[] { 1, 2 });
            irData.AddRange(zero);
            irData.AddRange(zero);
            irData.AddRange(one);
            irData.AddRange(one);
            irData.Add(1); // Trailing pulse.

            // ACT, ASSERT
            Assert.That(() => subject.ToBytes(irData), Throws.Exception.TypeOf<InvalidPacketDataException>());
        }

        [Test]
        public void ToBytes_Null()
        {
            // ARRANGE
            var subject = NewSubject();
            var irData = (IReadOnlyPulseSpaceUnitList)null;

            // ACT, ASSERT
            Assert.That(() => subject.ToBytes(irData), Throws.ArgumentNullException);
        }

        [Test]
        public void ToBytes_NoLeadIn()
        {
            // ARRANGE
            var subject = NewSubject();

            byte[] zero = new byte[] { 1, 1 };
            byte[] one = new byte[] { 1, 3 };

            var irData = new PulseSpaceUnitList();
            irData.AddRange(one);
            irData.AddRange(one);
            irData.AddRange(zero);
            irData.AddRange(zero);
            irData.AddRange(zero);
            irData.AddRange(zero);
            irData.AddRange(one);
            irData.AddRange(one);
            irData.Add(1); // Trailing pulse.

            // ACT
            byte[] decoded = subject.ToBytes(irData);

            // ASSERT
            Assert.That(decoded, Is.EqualTo(GetSampleBinaryData()));
        }

        [Test]
        public void ToIR()
        {
            // ARRANGE
            var subject = NewSubject();

            // ACT
            var irData = subject.ToIR(GetSampleBinaryData());

            // ASSERT
            Assert.That(irData, Is.EqualTo(GetSampleIRData()));
        }
    }
}
