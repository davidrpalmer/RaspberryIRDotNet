using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;

namespace RaspberryIRDotNet.Tests.Unit.RX
{
    class UnitDurationLearnerTests : BaseRxTests
    {
        private readonly int[] _goodSignal1 = new int[]
        {
            490,  //P
            401,  //S
            105,  //P
            110,  //S
             85,  //P
            302,  //S
            199,  //P
            200,  //S
            405,  //P
        };

        private readonly int[] _goodSignal2 = new int[]
        {
            501,  //P
            401,  //S
            205,  //P
            210,  //S
            206,  //P
            302,  //S
            199,  //P
            200,  //S
            405,  //P
        };

        private readonly int[] _goodSignal3 = new int[]
        {
            500,  //P
            397,  //S
             97,  //P
             94,  //S
             99,  //P
            102,  //S
            109,  //P
            200,  //S
            405,  //P
        };

        private readonly int[] _goodSignalB = new int[]
        {
            500,  //P
            400,  //S
            305,  //P
            610,  //S
            298,  //P
            905,  //S
            302,  //P
            302,  //S
            309,  //P
        };

        private readonly int[] _badLeadIn = new int[]
        {
            620,  //P
            400,  //S
             97,  //P
             94,  //S
             99,  //P
            102,  //S
            109,  //P
            200,  //S
            405,  //P
        };

        [Test]
        public void PerfectSignal()
        {
            var fileHandles = new List<Mock<FileSystem.IOpenFile>>();
            fileHandles.Add(MakeMockFileHandle(_goodSignal1));
            fileHandles.Add(MakeMockFileHandle(_goodSignal2));
            fileHandles.Add(MakeMockFileHandle(_goodSignal3));

            LearnUnitDuration(fileHandles, 3, 0, 100);
        }

        [Test]
        public void PerfectSignal_B()
        {
            var fileHandles = new List<Mock<FileSystem.IOpenFile>>();
            fileHandles.Add(MakeMockFileHandle(_goodSignalB));
            fileHandles.Add(MakeMockFileHandle(_goodSignalB));
            fileHandles.Add(MakeMockFileHandle(_goodSignalB));

            LearnUnitDuration(fileHandles, 3, 0, 300);
        }

        [Test]
        public void SomeNoise()
        {
            var fileHandles = new List<Mock<FileSystem.IOpenFile>>();
            fileHandles.Add(MakeMockFileHandle(_goodSignal1));
            fileHandles.Add(MakeMockFileHandle(_badLeadIn, _badLeadIn, _badLeadIn, _goodSignal2));
            fileHandles.Add(MakeMockFileHandle(_badLeadIn, _goodSignal3));

            LearnUnitDuration(fileHandles, 3, 4, 100);
        }

        private void LearnUnitDuration(List<Mock<FileSystem.IOpenFile>> fileHandles, int expectedHit, int expectedMiss, int expectedDuration)
        {
            // ARRANGE

            var fileSystem = MockFileSystem(fileHandles);
            var subject = NewUnitDurationLearner(fileSystem.Object);

            int waitingCount = 0;
            int hitCount = 0;
            int missCount = 0;
            subject.Waiting += (s, e) => waitingCount++;
            subject.Hit += (s, e) => hitCount++;
            subject.Miss += (s, e) => missCount++;

            // ACT
            int duration = subject.LearnUnitDuration();

            // ASSERT
            fileSystem.Verify(x => x.OpenRead(It.Is<string>(arg => arg == LircPath)), Times.Exactly(fileHandles.Count));
            foreach (var fh in fileHandles)
            {
                fh.Verify(x => x.Dispose(), Times.Once);
            }

            const int errorMargin = 10;
            Assert.That(duration, Is.GreaterThanOrEqualTo(expectedDuration - errorMargin));
            Assert.That(duration, Is.LessThanOrEqualTo(expectedDuration + errorMargin));

            Assert.That(waitingCount, Is.EqualTo(expectedHit));
            Assert.That(hitCount, Is.EqualTo(expectedHit));
            Assert.That(missCount, Is.EqualTo(expectedMiss));

            Console.WriteLine("Duration: " + duration);
        }

        private RaspberryIRDotNet.RX.UnitDurationLearner NewUnitDurationLearner(FileSystem.IFileSystem fileSystem)
        {
            var pulseSpaceSource = new RaspberryIRDotNet.RX.PulseSpaceSource.PulseSpaceCaptureLirc(LircPath, fileSystem)
            {
                ThrowOnUnknownPacket = true
            };

            var subject = new RaspberryIRDotNet.RX.UnitDurationLearner(pulseSpaceSource)
            {
                CaptureDelay = TimeSpan.Zero, // Make unit tests run faster
                LeadInPatternDurations = new PulseSpaceDurationList() { 500, 400 },
                TargetCaptures = 3,
            };

            return subject;
        }
    }
}
