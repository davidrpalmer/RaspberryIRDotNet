using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;

namespace RaspberryIRDotNet.Tests.Unit.TX
{
    class PulseSpaceTransmitter_AutoOpenCloseTests : BaseTxTests
    {
        private RaspberryIRDotNet.TX.PulseSpaceTransmitter_AutoOpenClose MakeSubject(FileSystem.IFileSystem fileSystem)
        {
            var subject = new RaspberryIRDotNet.TX.PulseSpaceTransmitter_AutoOpenClose()
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
            var subject = new RaspberryIRDotNet.TX.PulseSpaceTransmitter_AutoOpenClose();
            subject.SetFileSystem(fileSystem.Object);

            // ACT, ASSERT
            Assert.That(() => subject.Send(MakeDataToSend()), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("TransmissionDevice"));
        }

        [Test]
        public void NotATXDevice()
        {
            // ARRANGE
            var fileHandle = MakeMockFileHandle();
            var fileSystem = new Mock<FileSystem.IFileSystem>(MockBehavior.Strict);

            fileHandle
                .Setup(x => x.IoCtlReadUInt32(It.Is<uint>(arg => arg == LircConstants.LIRC_GET_FEATURES)))
                .Returns((uint)DeviceFeatures.ReceiveModeMode2);

            fileSystem
                .Setup(x => x.OpenWrite(It.Is<string>(arg => arg == LircPath)))
                .Returns(fileHandle.Object);

            var subject = MakeSubject(fileSystem.Object);

            // ACT, ASSERT
            Assert.That(() => subject.Send(MakeDataToSend()), Throws.Exception.TypeOf<NotSupportedException>());

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
            subject.Send(MakeDataToSend());

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
            subject.Send(MakeDataToSend());
            subject.Send(MakeDataToSend());

            // ASSERT
            fileSystem.Verify(x => x.OpenWrite(It.Is<string>(arg => arg == LircPath)), Times.Exactly(2));
            fileHandle.Verify(x => x.Dispose(), Times.Exactly(2));

            Assert.That(writtenData.Count, Is.EqualTo(2));
            Assert.That(writtenData[0].Length, Is.EqualTo(28));
            Assert.That(writtenData[1].Length, Is.EqualTo(28));
        }

    }
}
