using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;

namespace RaspberryIRDotNet.Tests.Unit.RX
{
    class IRMessageLearnTests : BaseRxTests
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

        private readonly int[] _goodSignalB1 = new int[]
        {
            500, //P5
            400, //S9
            210, // P11
            195, // S13
            220, // P15
        };

        private readonly int[] _goodSignalB2 = new int[]
        {
            510, //P5
            405, //S9
            180, // P11
            195, // S13
            240, // P15
        };

        private readonly int[] _badSignal1 = new int[]
        {
            500, //P5
            400, //S9
            155, // P11
            100, // S12
            100, // P13
            105, // S14
            100 // P15
        };

        private readonly int[] _badSignal2 = new int[]
        {
            500, //P5
            400, //S9
            200, // P11
            100, // S12
            300, // P15
        };

        private readonly int[] _badSignal3 = new int[]
        {
            500, //P5
            400, //S9
            100, // P10
            100, // S11
            100, // P12
            105, // S13
            200 // P15
        };


        private readonly int[] _noise_TooFewPulseSpaces = new int[]
        {
            530, //P
            377, //S
            145, //P
        };

        private readonly int[] _noise_VeryShort = new int[]
        {
            530,
        };

        private readonly int[] _noise_WrongLeadIn = new int[]
        {
            104, //P
            970, //S
            145, //P
            55, //S
            220, //P
            105, //S
            100 //P
        };


        private RaspberryIRDotNet.RX.IRMessageLearn NewIRMessageLearn(FileSystem.IFileSystem fileSystem)
        {
            var pulseSpaceSource = new RaspberryIRDotNet.RX.PulseSpaceSource.PulseSpaceCaptureLirc(LircPath, fileSystem)
            {
                ThrowOnUnknownPacket = true
            };

            var subject = new RaspberryIRDotNet.RX.IRMessageLearn(pulseSpaceSource)
            {
                CaptureDelay = TimeSpan.Zero,
                UnitDurationMicrosecs = 100,
                MinimumPulseSpaceCount = 4,
                MinimumMatchingCaptures = 3,
                ErrorAfterBadCaptureCount = 5,
            };
            subject.SetLeadInPatternFilterByUnits(new PulseSpaceUnitList() { 5, 4 });
            return subject;
        }

        [Test]
        public void LearnMessage_Perfect()
        {
            var fileHandles = new List<Mock<FileSystem.IOpenFile>>();
            fileHandles.Add(MakeMockFileHandle(_goodSignal1));
            fileHandles.Add(MakeMockFileHandle(_goodSignal2));
            fileHandles.Add(MakeMockFileHandle(_goodSignal3));

            LearnMessage(fileHandles, 3, 0);
        }

        [Test]
        public void LearnMessage_BitOfNoise1()
        {
            var fileHandles = new List<Mock<FileSystem.IOpenFile>>();
            fileHandles.Add(MakeMockFileHandle(_goodSignal1));
            fileHandles.Add(MakeMockFileHandle(_noise_WrongLeadIn, _goodSignal2));
            fileHandles.Add(MakeMockFileHandle(_noise_TooFewPulseSpaces, _goodSignal3));

            LearnMessage(fileHandles, 3, 2);
        }

        [Test]
        public void LearnMessage_BitOfNoise2()
        {
            var fileHandles = new List<Mock<FileSystem.IOpenFile>>();
            fileHandles.Add(MakeMockFileHandle(_noise_WrongLeadIn, _goodSignal1, _noise_WrongLeadIn, _noise_WrongLeadIn, _noise_WrongLeadIn, _noise_WrongLeadIn));
            fileHandles.Add(MakeMockFileHandle(_noise_TooFewPulseSpaces, _noise_VeryShort, _noise_TooFewPulseSpaces, _noise_VeryShort, _noise_TooFewPulseSpaces, _goodSignal2));
            fileHandles.Add(MakeMockFileHandle(_noise_TooFewPulseSpaces, _goodSignal3));

            LearnMessage(fileHandles, 3, 7);
        }

        [Test]
        public void LearnMessage_BadMessagePress1()
        {
            var fileHandles = new List<Mock<FileSystem.IOpenFile>>();
            fileHandles.Add(MakeMockFileHandle(_goodSignal1));
            fileHandles.Add(MakeMockFileHandle(_badSignal1));
            fileHandles.Add(MakeMockFileHandle(_goodSignal2));
            fileHandles.Add(MakeMockFileHandle(_goodSignal3));

            LearnMessage(fileHandles, 4, 0);
        }

        [Test]
        public void LearnMessage_BadMessagePress2()
        {
            var fileHandles = new List<Mock<FileSystem.IOpenFile>>();
            fileHandles.Add(MakeMockFileHandle(_badSignal1));
            fileHandles.Add(MakeMockFileHandle(_goodSignal1));
            fileHandles.Add(MakeMockFileHandle(_badSignal2));
            fileHandles.Add(MakeMockFileHandle(_goodSignal2));
            fileHandles.Add(MakeMockFileHandle(_goodSignal3));

            LearnMessage(fileHandles, 5, 0);
        }

        [Test]
        public void LearnMessage_ToggleBit()
        {
            var fileHandles = new List<Mock<FileSystem.IOpenFile>>();
            fileHandles.Add(MakeMockFileHandle(_noise_WrongLeadIn, _goodSignal1));
            fileHandles.Add(MakeMockFileHandle(_noise_TooFewPulseSpaces, _goodSignalB1));
            fileHandles.Add(MakeMockFileHandle(_noise_TooFewPulseSpaces, _goodSignal2));
            fileHandles.Add(MakeMockFileHandle(_noise_TooFewPulseSpaces, _goodSignalB2));
            fileHandles.Add(MakeMockFileHandle(_goodSignal3));

            LearnMessage(fileHandles, 5, 4);
        }

        private void LearnMessage(List<Mock<FileSystem.IOpenFile>> fileHandles, int expectedHit, int expectedMiss)
        {
            // ARRANGE
            var fileSystem = MockFileSystem(fileHandles);
            var subject = NewIRMessageLearn(fileSystem.Object);

            int waitingCount = 0;
            int hitCount = 0;
            int missCount = 0;
            subject.Waiting += (s, e) => waitingCount++;
            subject.Hit += (s, e) => hitCount++;
            subject.Miss += (s, e) => missCount++;

            // ACT
            var result = subject.LearnMessage();

            // ASSERT
            fileSystem.Verify(x => x.OpenRead(It.Is<string>(arg => arg == LircPath)), Times.Exactly(fileHandles.Count));
            foreach (var fh in fileHandles)
            {
                fh.Verify(x => x.Dispose(), Times.Once);
            }

            Assert.That(result, Is.Not.Null);
            Assert.That(result.UnitCount, Is.EqualTo(15));
            Assert.That(result.UnitDuration, Is.EqualTo(100));

            Assert.That(result.PulseSpaceDurations[0], Is.EqualTo(500));
            Assert.That(result.PulseSpaceDurations[1], Is.EqualTo(400));
            Assert.That(result.PulseSpaceDurations[2], Is.EqualTo(100));
            Assert.That(result.PulseSpaceDurations[3], Is.EqualTo(100));
            Assert.That(result.PulseSpaceDurations[4], Is.EqualTo(200));
            Assert.That(result.PulseSpaceDurations[5], Is.EqualTo(100));
            Assert.That(result.PulseSpaceDurations[6], Is.EqualTo(100));

            Assert.That(result.PulseSpaceUnits[0], Is.EqualTo(5));
            Assert.That(result.PulseSpaceUnits[1], Is.EqualTo(4));
            Assert.That(result.PulseSpaceUnits[2], Is.EqualTo(1));
            Assert.That(result.PulseSpaceUnits[3], Is.EqualTo(1));
            Assert.That(result.PulseSpaceUnits[4], Is.EqualTo(2));
            Assert.That(result.PulseSpaceUnits[5], Is.EqualTo(1));
            Assert.That(result.PulseSpaceUnits[6], Is.EqualTo(1));


            Assert.That(waitingCount, Is.EqualTo(expectedHit));
            Assert.That(hitCount, Is.EqualTo(expectedHit));
            Assert.That(missCount, Is.EqualTo(expectedMiss));
        }

        [Test]
        public void LearnMessage_TooNoisy()
        {
            // ARRANGE
            var fileHandles = new List<Mock<FileSystem.IOpenFile>>();
            fileHandles.Add(MakeMockFileHandle(_goodSignal1));
            fileHandles.Add(MakeMockFileHandle(_goodSignal2));
            fileHandles.Add(MakeMockFileHandle(_badSignal1));
            fileHandles.Add(MakeMockFileHandle(_badSignal2));
            fileHandles.Add(MakeMockFileHandle(_badSignal3));

            var fileSystem = MockFileSystem(fileHandles);

            var subject = NewIRMessageLearn(fileSystem.Object);

            // ACT, ASSERT
            Assert.That(() => subject.LearnMessage(), Throws.Exception.TypeOf<Exceptions.InsufficientMatchingSamplesException>());

            // ASSERT
            fileSystem.Verify(x => x.OpenRead(It.Is<string>(arg => arg == LircPath)), Times.Exactly(fileHandles.Count));
            foreach (var fh in fileHandles)
            {
                fh.Verify(x => x.Dispose(), Times.Once);
            }
        }

        [Test]
        public void NotARXDevice()
        {
            // ARRANGE
            var fileHandle = MakeMockFileHandle();
            var fileSystem = new Mock<FileSystem.IFileSystem>(MockBehavior.Strict);

            fileHandle
                .Setup(x => x.IoCtlReadUInt32(It.Is<uint>(arg => arg == LircConstants.LIRC_GET_FEATURES)))
                .Returns((uint)DeviceFeatures.SendModePulse);
            fileSystem
                .Setup(x => x.OpenRead(It.Is<string>(arg => arg == LircPath)))
                .Returns(fileHandle.Object);

            var subject = NewIRMessageLearn(fileSystem.Object);

            // ACT, ASSERT
            Assert.That(() => subject.LearnMessage(), Throws.Exception.TypeOf<NotSupportedException>());

            fileSystem.Verify(x => x.OpenRead(It.Is<string>(arg => arg == LircPath)), Times.Once);
            fileHandle.Verify(x => x.Dispose(), Times.Once);
        }

        [Test]
        public void LearnMultipleMessages()
        {
            // ARRANGE
            var fileHandles = new List<Mock<FileSystem.IOpenFile>>();
            fileHandles.Add(MakeMockFileHandle(_goodSignal1));
            fileHandles.Add(MakeMockFileHandle(_goodSignal2));
            fileHandles.Add(MakeMockFileHandle(_goodSignal3));
            fileHandles.Add(MakeMockFileHandle(_goodSignalB1)); // Won't be used until the second capture
            fileHandles.Add(MakeMockFileHandle(_goodSignalB1));
            fileHandles.Add(MakeMockFileHandle(_goodSignalB2));

            var fileSystem = MockFileSystem(fileHandles);
            var subject = NewIRMessageLearn(fileSystem.Object);

            {
                // ACT
                var result1 = subject.LearnMessage();

                // ASSERT
                fileSystem.Verify(x => x.OpenRead(It.Is<string>(arg => arg == LircPath)), Times.Exactly(3));

                Assert.That(result1, Is.Not.Null);
                Assert.That(result1.UnitCount, Is.EqualTo(15));
                Assert.That(result1.UnitDuration, Is.EqualTo(100));

                Assert.That(result1.PulseSpaceDurations.Count, Is.EqualTo(7));
                Assert.That(result1.PulseSpaceDurations[0], Is.EqualTo(500));
                Assert.That(result1.PulseSpaceDurations[1], Is.EqualTo(400));
                Assert.That(result1.PulseSpaceDurations[2], Is.EqualTo(100));
                Assert.That(result1.PulseSpaceDurations[3], Is.EqualTo(100));
                Assert.That(result1.PulseSpaceDurations[4], Is.EqualTo(200));
                Assert.That(result1.PulseSpaceDurations[5], Is.EqualTo(100));
                Assert.That(result1.PulseSpaceDurations[6], Is.EqualTo(100));

                Assert.That(result1.PulseSpaceUnits.Count, Is.EqualTo(7));
                Assert.That(result1.PulseSpaceUnits[0], Is.EqualTo(5));
                Assert.That(result1.PulseSpaceUnits[1], Is.EqualTo(4));
                Assert.That(result1.PulseSpaceUnits[2], Is.EqualTo(1));
                Assert.That(result1.PulseSpaceUnits[3], Is.EqualTo(1));
                Assert.That(result1.PulseSpaceUnits[4], Is.EqualTo(2));
                Assert.That(result1.PulseSpaceUnits[5], Is.EqualTo(1));
                Assert.That(result1.PulseSpaceUnits[6], Is.EqualTo(1));
            }

            {
                // ACT
                var result2 = subject.LearnMessage();

                // ASSERT
                fileSystem.Verify(x => x.OpenRead(It.Is<string>(arg => arg == LircPath)), Times.Exactly(6));
                foreach (var fh in fileHandles)
                {
                    fh.Verify(x => x.Dispose(), Times.Once);
                }

                Assert.That(result2, Is.Not.Null);
                Assert.That(result2.UnitCount, Is.EqualTo(15));
                Assert.That(result2.UnitDuration, Is.EqualTo(100));

                Assert.That(result2.PulseSpaceDurations.Count, Is.EqualTo(5));
                Assert.That(result2.PulseSpaceDurations[0], Is.EqualTo(500));
                Assert.That(result2.PulseSpaceDurations[1], Is.EqualTo(400));
                Assert.That(result2.PulseSpaceDurations[2], Is.EqualTo(200));
                Assert.That(result2.PulseSpaceDurations[3], Is.EqualTo(200));
                Assert.That(result2.PulseSpaceDurations[4], Is.EqualTo(200));

                Assert.That(result2.PulseSpaceUnits.Count, Is.EqualTo(5));
                Assert.That(result2.PulseSpaceUnits[0], Is.EqualTo(5));
                Assert.That(result2.PulseSpaceUnits[1], Is.EqualTo(4));
                Assert.That(result2.PulseSpaceUnits[2], Is.EqualTo(2));
                Assert.That(result2.PulseSpaceUnits[3], Is.EqualTo(2));
                Assert.That(result2.PulseSpaceUnits[4], Is.EqualTo(2));
            }
        }
    }
}
