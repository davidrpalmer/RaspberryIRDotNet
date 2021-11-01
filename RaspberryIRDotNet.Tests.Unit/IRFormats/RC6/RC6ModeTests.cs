using System;
using System.Linq;
using NUnit.Framework;
using RaspberryIRDotNet.PacketFormats.RC6;

namespace RaspberryIRDotNet.Tests.Unit.PacketFormats.RC6
{
    class RC6ModeTests
    {
        [Test]
        public void RC6Mode_ConstructorGoodRange([Range(0, 7)] byte input)
        {
            // ACT / ASSERT
            var mode = new RC6Mode(input);

            Assert.That(mode, Is.EqualTo(input));
        }

        [Test]
        public void RC6Mode_ConstructorBadRange([Range(8, 10), Range(250, byte.MaxValue)] byte input)
        {
            // ACT / ASSERT
            Assert.That(() => new RC6Mode(input), Throws.TypeOf<ArgumentOutOfRangeException>().With.Property("ParamName").EqualTo("value").And.Property("ActualValue").EqualTo(input));
        }


        [Test]
        public void RC6Mode_CastFromByteGoodRange([Range(0, 7)] byte input)
        {
            // ACT / ASSERT
            var mode = (RC6Mode)input;

            Assert.That(mode, Is.EqualTo(input));
        }

        [Test]
        public void RC6Mode_CastFromByteBadRange([Range(8, 10), Range(250, byte.MaxValue)] byte input)
        {
            // ACT / ASSERT
            Assert.That(() => (RC6Mode)input, Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void RC6Mode_CastToByte()
        {
            // ARRANGE
            const byte input = 3;
            var mode = new RC6Mode(input);

            // ACT
            byte b = mode;

            // ASSERT
            Assert.That(b, Is.EqualTo(input));
        }

        [Test]
        public void RC6Mode_CastToInt()
        {
            // ARRANGE
            const byte input = 3;
            var mode = new RC6Mode(input);

            // ACT
            int i = mode;

            // ASSERT
            Assert.That(i, Is.EqualTo(input));
        }

        [Test]
        public void RC6Mode_GetHashCode()
        {
            // ARRANGE / ACT
            var hashes1 = new int[]
            {
                new RC6Mode(0).GetHashCode(),
                new RC6Mode(1).GetHashCode(),
                new RC6Mode(2).GetHashCode(),
                new RC6Mode(3).GetHashCode(),
                new RC6Mode(4).GetHashCode(),
                new RC6Mode(5).GetHashCode(),
                new RC6Mode(6).GetHashCode(),
                new RC6Mode(7).GetHashCode()
            };

            var hashes2 = new int[]
            {
                new RC6Mode(0).GetHashCode(),
                new RC6Mode(1).GetHashCode(),
                new RC6Mode(2).GetHashCode(),
                new RC6Mode(3).GetHashCode(),
                new RC6Mode(4).GetHashCode(),
                new RC6Mode(5).GetHashCode(),
                new RC6Mode(6).GetHashCode(),
                new RC6Mode(7).GetHashCode()
            };

            // ASSERT

            for (int i = 0; i < hashes1.Length; i++)
            {
                Assert.That(hashes1[i], Is.EqualTo(hashes2[i]), "Hash is not reproducible.");
            }

            if (hashes1.Distinct().Count() != hashes1.Length)
            {
                Assert.Fail("Not all hash codes are unique.");
            }
        }

        [Test]
        public void RC6Mode_EqualsSelf()
        {
            // ARRANGE
            var modeA = new RC6Mode(5);
            var modeB = new RC6Mode(5);
            var modeC = new RC6Mode(6);

            // ACT / ASSERT
            if (modeA == modeC)
            {
                Assert.Fail();
            }

            if (modeA != modeB)
            {
                Assert.Fail();
            }

            if (!modeA.Equals(modeB))
            {
                Assert.Fail();
            }

            if (modeA.Equals(modeC))
            {
                Assert.Fail();
            }

            Assert.That(modeA, Is.EqualTo(modeB));
            Assert.That(modeA, Is.Not.EqualTo(modeC));
        }

        [Test]
        public void RC6Mode_EqualsOtherObject()
        {
            // ARRANGE
            var modeA = new RC6Mode(5);
            var modeB = new RC6Mode(5);
            var modeC = new RC6Mode(6);

            // ACT / ASSERT

            if (!modeA.Equals((object)modeB))
            {
                Assert.Fail();
            }

            if (modeA.Equals((object)modeC))
            {
                Assert.Fail();
            }

            if (!modeA.Equals((object)5))
            {
                Assert.Fail();
            }

            if (modeA.Equals((object)6))
            {
                Assert.Fail();
            }

            if (modeA.Equals("Hi"))
            {
                Assert.Fail();
            }

            if (modeA.Equals(null))
            {
                Assert.Fail();
            }
        }

        [Test]
        public void RC6Mode_EqualsInt()
        {
            // ARRANGE
            const int five = 5;
            const int six = 6;

            var modeA = new RC6Mode(5);

            // ACT / ASSERT
            if (modeA == six)
            {
                Assert.Fail();
            }

            if (six == modeA)
            {
                Assert.Fail();
            }

            if (modeA != five)
            {
                Assert.Fail();
            }

            if (!(modeA != six))
            {
                Assert.Fail();
            }

            if (!modeA.Equals(five))
            {
                Assert.Fail();
            }

            if (modeA.Equals(six))
            {
                Assert.Fail();
            }

            if (modeA.Equals(261)) // larger than byte, also 261 wraps around to be 5.
            {
                Assert.Fail();
            }

            Assert.That(modeA, Is.EqualTo(five));
            Assert.That(modeA, Is.Not.EqualTo(six));
        }

        [Test]
        public void RC6Mode_EqualsByte()
        {
            // ARRANGE
            const byte five = 5;
            const byte six = 6;

            var modeA = new RC6Mode(5);

            // ACT / ASSERT
            if (modeA == six)
            {
                Assert.Fail();
            }

            if (six == modeA)
            {
                Assert.Fail();
            }

            if (modeA != five)
            {
                Assert.Fail();
            }

            if (!(modeA != six))
            {
                Assert.Fail();
            }

            if (!modeA.Equals(five))
            {
                Assert.Fail();
            }

            if (modeA.Equals(six))
            {
                Assert.Fail();
            }

            Assert.That(modeA, Is.EqualTo(five));
            Assert.That(modeA, Is.Not.EqualTo(six));
        }
    }
}
