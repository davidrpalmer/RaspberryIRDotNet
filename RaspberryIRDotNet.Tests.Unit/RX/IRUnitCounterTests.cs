using System;
using System.Linq;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;

namespace RaspberryIRDotNet.Tests.Unit.RX
{
    class IRUnitCounterTests : BaseRxTests
    {
        private readonly int[] _goodSignal1 = new int[]
        {
            530, //P5
            377, //S9
            145, // P10
            55, // S11
            220, // P13
            105, // S14
            100 // P15
        };

        private readonly int[] _goodSignal2 = new int[]
        {
            540, //P5
            397, //S9
            75, // P10
            75, // S11
            180, // P13
            109, // S14
            80 // P15
        };

        private readonly int[] _goodSignal3 = new int[]
        {
            530, //P5
            377, //S9
            145, // P10
            55, // S11
            220, // P13
            105, // S14
            100 // P15
        };

        private readonly int[] _badSignal_tooShort = new int[]
        {
            530, //P5
            377, //S9
            145, // P10
            55, // S11
            148, // P12 - This is too short, it should have been 2 units.
            105, // S13
            100 // P14
        };

        private readonly int[] _badSignal_tooLong = new int[]
        {
            530, //P5
            377, //S9
            145, // P10
            55, // S11
            220, // P13
            195, // S15 - This is too long, it takes 2 units instead of 1.
            100 // P16
        };

        private readonly int[] _badSignal_missingPulseSpace = new int[]
        {
            530, //P5
            377, //S9
            145, // P10
            55, // S11
            220, // P13
            // The last space and last pulse are missing.
        };


        private readonly int[] _noise_WrongLeadIn = new int[]
        {
            104, //P5
            970, //P9
            145, // P10
            55, // S11
            220, // P13
            105, // S14
            100 // P15
        };

        private RaspberryIRDotNet.RX.IRUnitCounter NewIRUnitCounter(FileSystem.IFileSystem fileSystem)
        {
            var subject = new RaspberryIRDotNet.RX.IRUnitCounter()
            {
                CaptureDevice = LircPath,
                CaptureDelay = TimeSpan.Zero,
                LeadInPattern = new PulseSpaceUnitList() { 5, 4 },
                UnitDurationMicrosecs = 100,
                TargetCaptures = 5,
                ThrowOnUnknownPacket = true
            };
            subject.SetFileSystem(fileSystem);
            return subject;
        }

        [Test]
        public void Capture_GoodSignals()
        {
            var fileHandles = new List<Mock<FileSystem.IOpenFile>>();
            fileHandles.Add(MakeMockFileHandle(_goodSignal1));
            fileHandles.Add(MakeMockFileHandle(_goodSignal1));
            fileHandles.Add(MakeMockFileHandle(_goodSignal2));
            fileHandles.Add(MakeMockFileHandle(_goodSignal2));
            fileHandles.Add(MakeMockFileHandle(_goodSignal3));

            var result = Capture(fileHandles, 5, 0);
            Assert.That(result.Length, Is.EqualTo(1));
            Assert.That(result.Single().Key, Is.EqualTo(15));
            Assert.That(result.Single().All(x => x.UnitCount == 15));
        }

        [Test]
        public void Capture_SomeNoise()
        {
            var fileHandles = new List<Mock<FileSystem.IOpenFile>>();
            fileHandles.Add(MakeMockFileHandle(_badSignal_tooShort));
            fileHandles.Add(MakeMockFileHandle(_goodSignal1, _noise_WrongLeadIn, _noise_WrongLeadIn, _noise_WrongLeadIn));
            fileHandles.Add(MakeMockFileHandle(_noise_WrongLeadIn, _badSignal_missingPulseSpace));
            fileHandles.Add(MakeMockFileHandle(_noise_WrongLeadIn, _goodSignal2));
            fileHandles.Add(MakeMockFileHandle(_badSignal_tooLong));

            var result = Capture(fileHandles, 5, 2);
            Assert.That(result.Length, Is.EqualTo(4));
            Assert.That(result.OrderByDescending(x => x.Count()).First().Key, Is.EqualTo(15));
        }

        private IGrouping<int, IRPulseMessage>[] Capture(List<Mock<FileSystem.IOpenFile>> fileHandles, int expectedHit, int expectedMiss)
        {
            // ARRANGE
            var fileSystem = MockFileSystem(fileHandles);
            var subject = NewIRUnitCounter(fileSystem.Object);

            int waitingCount = 0;
            int hitCount = 0;
            int missCount = 0;
            subject.Waiting += (s, e) => waitingCount++;
            subject.Hit += (s, e) => hitCount++;
            subject.Miss += (s, e) => missCount++;

            // ACT
            var result = subject.Capture();

            // ASSERT
            fileSystem.Verify(x => x.OpenRead(It.Is<string>(arg => arg == LircPath)), Times.Exactly(fileHandles.Count));
            foreach (var fh in fileHandles)
            {
                fh.Verify(x => x.Dispose(), Times.Once);
            }

            Assert.That(result, Is.Not.Null);
            Assert.That(waitingCount, Is.EqualTo(expectedHit));
            Assert.That(hitCount, Is.EqualTo(expectedHit));
            Assert.That(missCount, Is.EqualTo(expectedMiss));
            return result;
        }

        [Test]
        public void CaptureAndGetMostCommonUnitCount_GoodSignal()
        {
            // ARRANGE
            var fileHandles = new List<Mock<FileSystem.IOpenFile>>();
            fileHandles.Add(MakeMockFileHandle(_goodSignal1));
            fileHandles.Add(MakeMockFileHandle(_goodSignal1));
            fileHandles.Add(MakeMockFileHandle(_goodSignal2));
            fileHandles.Add(MakeMockFileHandle(_goodSignal2));
            fileHandles.Add(MakeMockFileHandle(_goodSignal3));

            var fileSystem = MockFileSystem(fileHandles);
            var subject = NewIRUnitCounter(fileSystem.Object);

            // ACT
            int result = subject.CaptureAndGetMostCommonUnitCount();

            // ASSERT
            fileSystem.Verify(x => x.OpenRead(It.Is<string>(arg => arg == LircPath)), Times.Exactly(fileHandles.Count));
            foreach (var fh in fileHandles)
            {
                fh.Verify(x => x.Dispose(), Times.Once);
            }

            Assert.That(result, Is.EqualTo(15));
        }

        [Test]
        public void CaptureAndGetMostCommonUnitCount_BitOfBadSignal()
        {
            // ARRANGE
            var fileHandles = new List<Mock<FileSystem.IOpenFile>>();
            fileHandles.Add(MakeMockFileHandle(_goodSignal1));
            fileHandles.Add(MakeMockFileHandle(_badSignal_missingPulseSpace));
            fileHandles.Add(MakeMockFileHandle(_noise_WrongLeadIn, _goodSignal2));
            fileHandles.Add(MakeMockFileHandle(_noise_WrongLeadIn, _goodSignal2));
            fileHandles.Add(MakeMockFileHandle(_goodSignal3));

            var fileSystem = MockFileSystem(fileHandles);
            var subject = NewIRUnitCounter(fileSystem.Object);

            // ACT
            int result = subject.CaptureAndGetMostCommonUnitCount();

            // ASSERT
            fileSystem.Verify(x => x.OpenRead(It.Is<string>(arg => arg == LircPath)), Times.Exactly(fileHandles.Count));
            foreach (var fh in fileHandles)
            {
                fh.Verify(x => x.Dispose(), Times.Once);
            }

            Assert.That(result, Is.EqualTo(15));
        }

        [Test]
        public void CaptureAndGetMostCommonUnitCount_BadSignal()
        {
            // ARRANGE
            var fileHandles = new List<Mock<FileSystem.IOpenFile>>();
            fileHandles.Add(MakeMockFileHandle(_goodSignal1));
            fileHandles.Add(MakeMockFileHandle(_goodSignal2));
            fileHandles.Add(MakeMockFileHandle(_goodSignal3));
            fileHandles.Add(MakeMockFileHandle(_badSignal_missingPulseSpace));
            fileHandles.Add(MakeMockFileHandle(_badSignal_missingPulseSpace));

            var fileSystem = MockFileSystem(fileHandles);
            var subject = NewIRUnitCounter(fileSystem.Object);

            // ACT
            Assert.That(() => subject.CaptureAndGetMostCommonUnitCount(), Throws.TypeOf<Exceptions.InsufficientMatchingSamplesException>());

            // ASSERT
            fileSystem.Verify(x => x.OpenRead(It.Is<string>(arg => arg == LircPath)), Times.Exactly(fileHandles.Count));
            foreach (var fh in fileHandles)
            {
                fh.Verify(x => x.Dispose(), Times.Once);
            }
        }

    }
}
