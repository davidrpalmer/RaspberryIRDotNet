using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace RaspberryIRDotNet.Tests.Unit.RX
{
    class LeadInLearnerTests : BaseRxTests
    {
        private readonly int[] _goodSignal1 = new int[]
        {
            15030, //P
            5040, //S
            100,  //P
            100,  //S
            100,  //P
            100,  //S
            100,  //P
            100,  //S
            100,  //P
            100,  //S
            100,  //P
            100,  //S
            100   //P
        };

        private readonly int[] _goodSignal2 = new int[]
        {
            14960, //P
            5020, //S
            100,  //P
            100,  //S
            100,  //P
            100,  //S
            100,  //P
            100,  //S
            100,  //P
            100,  //S
            100,  //P
            100,  //S
            100   //P
        };

        private readonly int[] _badSignal1 = new int[]
        {
            12460, //P
            5020, //S
            100,  //P
            100,  //S
            100,  //P
            100,  //S
            100,  //P
            100,  //S
            100,  //P
            100,  //S
            100,  //P
            100,  //S
            100   //P
        };

        private readonly int[] _badSignal2 = new int[]
        {
            14960, //P
            5090, //S
            100,  //P
            100,  //S
            100,  //P
            100,  //S
            100,  //P
            100,  //S
            100,  //P
            100,  //S
            100,  //P
            100,  //S
            100   //P
        };

        private readonly int[] _badSignal3 = new int[]
        {
            900, //P
            900, //S
            100,  //P
            100,  //S
            100,  //P
            100,  //S
            100,  //P
            100,  //S
            100,  //P
            100,  //S
            100,  //P
            100,  //S
            100   //P
        };

        private readonly int[] _badSignal4 = new int[]
        {
            1000, //P
            1200, //S
            100,  //P
            100,  //S
            100,  //P
            100,  //S
            100,  //P
            100,  //S
            100,  //P
            100,  //S
            100,  //P
            100,  //S
            100   //P
        };

        private readonly int[] _noise = new int[]
        {
            240, //P
            220, //S
            100  //P
        };

        private RaspberryIRDotNet.RX.LeadInLearner NewLeadInLearner(FileSystem.IFileSystem fileSystem)
        {
            var subject = new RaspberryIRDotNet.RX.LeadInLearner()
            {
                CaptureDevice = LircPath,
                MinimumMatchingCaptures = 3,
                ThrowOnUnknownPacket = true
            };
            subject.SetFileSystem(fileSystem);

            var mockDebounce = new Mock<RaspberryIRDotNet.RX.IDebounceTimer>();
            mockDebounce.SetupGet(x => x.ReadyToDoAnother).Returns(true);
            subject.SetDebounceTimer(mockDebounce.Object);

            return subject;
        }

        [Test]
        public void LearnLeadInDurations_Perfect()
        {
            var fileHandle = MakeMockFileHandle(_goodSignal1, _goodSignal2, _goodSignal1);

            LearnLeadInDurations(fileHandle, 3);
        }

        [Test]
        public void LearnLeadInDurations_SomeSignalsThatCouldBeValid()
        {
            var fileHandle = MakeMockFileHandle(_goodSignal1, _badSignal1, _badSignal1, _badSignal2, _goodSignal1, _goodSignal2, /*This last one won't be used*/ _goodSignal1);

            LearnLeadInDurations(fileHandle, 6);
        }

        [Test]
        public void LearnLeadInDurations_BitOfObviousNoise()
        {
            var fileHandle = MakeMockFileHandle(_noise, _goodSignal1, _noise, _goodSignal2, _noise, _noise, _goodSignal1, _noise);

            LearnLeadInDurations(fileHandle, 3);
        }

        private void LearnLeadInDurations(Mock<FileSystem.IOpenFile> fileHandle, int expectedSignals)
        {
            // ARRANGE
            var fileSystem = MockFileSystem(new[] { fileHandle });
            var subject = NewLeadInLearner(fileSystem.Object);

            int hitCount = 0;
            subject.Received += (s, e) => hitCount++;

            // ACT
            PulseSpaceDurationList result = subject.LearnLeadInDurations();

            // ASSERT
            fileSystem.Verify(x => x.OpenRead(It.Is<string>(arg => arg == LircPath)), Times.Once);
            fileHandle.Verify(x => x.Dispose(), Times.Once);

            Assert.That(hitCount, Is.EqualTo(expectedSignals));

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0], Is.InRange(14960, 15040));
            Assert.That(result[1], Is.InRange(4960, 5040));
        }

        [Test]
        public void LearnLeadInDurations_TooManyBadSamples()
        {
            // ARRANGE
            var fileHandle = MakeMockFileHandle(_badSignal1, _badSignal1, _badSignal2, _badSignal2, _goodSignal1, _goodSignal2, _badSignal3, _badSignal3, _badSignal4, _badSignal4);
            var fileSystem = MockFileSystem(new[] { fileHandle });
            var subject = NewLeadInLearner(fileSystem.Object);

            int hitCount = 0;
            subject.Received += (s, e) => hitCount++;

            // ACT, ASSERT
            Assert.That(() => subject.LearnLeadInDurations(), Throws.TypeOf<Exceptions.InsufficientMatchingSamplesException>());

            // ASSERT
            fileSystem.Verify(x => x.OpenRead(It.Is<string>(arg => arg == LircPath)), Times.Once);
            fileHandle.Verify(x => x.Dispose(), Times.Once);

            Assert.That(hitCount, Is.EqualTo(9));
        }
    }
}
