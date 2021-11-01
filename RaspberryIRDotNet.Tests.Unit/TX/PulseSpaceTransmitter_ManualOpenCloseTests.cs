using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;

namespace RaspberryIRDotNet.Tests.Unit.TX
{
    class PulseSpaceTransmitter_ManualOpenCloseTests : BaseTxTests
    {
        private RaspberryIRDotNet.TX.PulseSpaceTransmitter_ManualOpenClose MakeSubject(FileSystem.IFileSystem fileSystem)
        {
            var subject = new RaspberryIRDotNet.TX.PulseSpaceTransmitter_ManualOpenClose()
            {
                TransmissionDevice = LircPath
            };
            subject.SetFileSystem(fileSystem);
            return subject;
        }

        [Test]
        public void NoDeviceSet()
        {
            // ARRANGE
            var fileSystem = new Mock<FileSystem.IFileSystem>(MockBehavior.Strict);
            var subject = new RaspberryIRDotNet.TX.PulseSpaceTransmitter_ManualOpenClose();
            subject.SetFileSystem(fileSystem.Object);

            // ACT, ASSERT
            Assert.That(() => subject.Open(), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("TransmissionDevice"));
            Assert.That(subject.IsOpen, Is.False);
            Assert.That(subject.Disposed, Is.False);
        }

        [Test]
        public void NotOpen()
        {
            // ARRANGE
            var fileSystem = new Mock<FileSystem.IFileSystem>(MockBehavior.Strict);
            var subject = MakeSubject(fileSystem.Object);
            subject.SetFileSystem(fileSystem.Object);

            // ACT, ASSERT
            Assert.That(() => subject.Send(MakeDataToSend()), Throws.InvalidOperationException);
            Assert.That(subject.IsOpen, Is.False);
            Assert.That(subject.Disposed, Is.False);
        }

        [Test]
        public void NotATXDevice()
        {
            // ARRANGE
            var fileHandle = MakeMockFileHandle();
            var fileSystem = new Mock<FileSystem.IFileSystem>(MockBehavior.Strict);

            fileHandle
                .Setup(x => x.IoCtlReadUInt32(It.Is<uint>(arg => arg == LircConstants.LIRC_GET_FEATURES)))
                .Returns(0);
            fileSystem
                .Setup(x => x.OpenWrite(It.Is<string>(arg => arg == LircPath)))
                .Returns(fileHandle.Object);

            var subject = MakeSubject(fileSystem.Object);

            // ACT, ASSERT
            Assert.That(() => subject.Open(), Throws.Exception.TypeOf<NotSupportedException>());

            fileSystem.Verify(x => x.OpenWrite(It.Is<string>(arg => arg == LircPath)), Times.Once);
            fileHandle.Verify(x => x.Dispose(), Times.Once);
        }

        [Test]
        public void OpenClose()
        {
            // ARRANGE
            var fileHandle = MakeMockFileHandle((data) => throw new Exception("Should not write anything."));
            var fileSystem = MockFileSystem(fileHandle);
            var subject = MakeSubject(fileSystem.Object);

            // ACT, ASSERT

            subject.Open();
            fileSystem.Verify(x => x.OpenWrite(It.Is<string>(arg => arg == LircPath)), Times.Once);
            fileHandle.Verify(x => x.Dispose(), Times.Never);

            subject.Close();
            fileSystem.Verify(x => x.OpenWrite(It.Is<string>(arg => arg == LircPath)), Times.Once);
            fileHandle.Verify(x => x.Dispose(), Times.Once);
        }

        [Test]
        public void OpenCloseOpenClose()
        {
            // ARRANGE
            var fileHandle = MakeMockFileHandle((data) => throw new Exception("Should not write anything."));
            var fileSystem = MockFileSystem(fileHandle);
            var subject = MakeSubject(fileSystem.Object);

            // ACT, ASSERT

            subject.Open();
            fileSystem.Verify(x => x.OpenWrite(It.Is<string>(arg => arg == LircPath)), Times.Exactly(1));
            fileHandle.Verify(x => x.Dispose(), Times.Never);

            subject.Close();
            fileSystem.Verify(x => x.OpenWrite(It.Is<string>(arg => arg == LircPath)), Times.Exactly(1));
            fileHandle.Verify(x => x.Dispose(), Times.Exactly(1));

            subject.Open();
            fileSystem.Verify(x => x.OpenWrite(It.Is<string>(arg => arg == LircPath)), Times.Exactly(2));
            fileHandle.Verify(x => x.Dispose(), Times.Exactly(1));

            subject.Close();
            fileSystem.Verify(x => x.OpenWrite(It.Is<string>(arg => arg == LircPath)), Times.Exactly(2));
            fileHandle.Verify(x => x.Dispose(), Times.Exactly(2));
        }

        [Test]
        public void OpenDispose()
        {
            // ARRANGE
            var fileHandle = MakeMockFileHandle((data) => throw new Exception("Should not write anything."));
            var fileSystem = MockFileSystem(fileHandle);
            var subject = MakeSubject(fileSystem.Object);

            // ACT, ASSERT

            subject.Open();
            fileSystem.Verify(x => x.OpenWrite(It.Is<string>(arg => arg == LircPath)), Times.Once);
            fileHandle.Verify(x => x.Dispose(), Times.Never);

            subject.Dispose();
            fileSystem.Verify(x => x.OpenWrite(It.Is<string>(arg => arg == LircPath)), Times.Once);
            fileHandle.Verify(x => x.Dispose(), Times.Once);
        }

        [Test]
        public void OpenCloseDispose()
        {
            // ARRANGE
            var fileHandle = MakeMockFileHandle((data) => throw new Exception("Should not write anything."));
            var fileSystem = MockFileSystem(fileHandle);
            var subject = MakeSubject(fileSystem.Object);

            // ACT, ASSERT

            subject.Open();
            fileSystem.Verify(x => x.OpenWrite(It.Is<string>(arg => arg == LircPath)), Times.Once);
            fileHandle.Verify(x => x.Dispose(), Times.Never);

            subject.Close();
            fileSystem.Verify(x => x.OpenWrite(It.Is<string>(arg => arg == LircPath)), Times.Once);
            fileHandle.Verify(x => x.Dispose(), Times.Once);

            subject.Dispose();
            fileSystem.Verify(x => x.OpenWrite(It.Is<string>(arg => arg == LircPath)), Times.Once);
            fileHandle.Verify(x => x.Dispose(), Times.Once);
        }

        [Test]
        public void SendOneIR()
        {
            // ARRANGE
            List<byte[]> writtenData = new List<byte[]>();
            var fileHandle = MakeMockFileHandle((data) => writtenData.Add(data));
            var fileSystem = MockFileSystem(fileHandle);
            var subject = MakeSubject(fileSystem.Object);

            // ACT
            subject.Open();
            subject.Send(MakeDataToSend());
            subject.Close();

            // ASSERT
            fileSystem.Verify(x => x.OpenWrite(It.Is<string>(arg => arg == LircPath)), Times.Once);
            fileHandle.Verify(x => x.Dispose(), Times.Once);

            Assert.That(writtenData.Count, Is.EqualTo(1));
            Assert.That(writtenData[0].Length, Is.EqualTo(28));
        }

        [Test]
        public void SendTwoIR()
        {
            // ARRANGE
            List<byte[]> writtenData = new List<byte[]>();
            var fileHandle = MakeMockFileHandle((data) => writtenData.Add(data));
            var fileSystem = MockFileSystem(fileHandle);
            var subject = MakeSubject(fileSystem.Object);

            // ACT
            subject.Open();
            subject.Send(MakeDataToSend());
            subject.Send(MakeDataToSend());
            subject.Close();

            // ASSERT
            fileSystem.Verify(x => x.OpenWrite(It.Is<string>(arg => arg == LircPath)), Times.Once);
            fileHandle.Verify(x => x.Dispose(), Times.Once);

            Assert.That(writtenData.Count, Is.EqualTo(2));
            Assert.That(writtenData[0].Length, Is.EqualTo(28));
            Assert.That(writtenData[1].Length, Is.EqualTo(28));
        }
    }
}
