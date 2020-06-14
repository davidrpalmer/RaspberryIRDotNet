using System;
using Moq;
using NUnit.Framework;
using RaspberryIRDotNet.DeviceAssessment;

namespace RaspberryIRDotNet.Tests.Unit.DeviceAssessment
{
    class DeviceAssessorTests
    {
        private const string LircPath = "/dev/lirc8";

        [Test]
        public void AssessDevice_TX()
        {
            // ARRANGE
            const string relativePath = "../.." + LircPath;
            var fileSystem = new Mock<FileSystem.IFileSystem>(MockBehavior.Strict);
            var fileHandle = new Mock<FileSystem.IOpenFile>(MockBehavior.Strict);

            fileSystem
                .Setup(x => x.IoCtlReadUInt32(It.Is<FileSystem.IOpenFile>(arg => arg == fileHandle.Object), It.Is<uint>(arg => arg == LircConstants.LIRC_GET_FEATURES)))
                .Returns((uint)(DeviceFeatures.SendModePulse | DeviceFeatures.SetSendCarrier));
            fileSystem
                .Setup(x => x.OpenRead(It.Is<string>(arg => arg == LircPath)))
                .Returns(fileHandle.Object);
            fileSystem
                .Setup(x => x.GetFullPath(It.Is<string>(arg => arg == relativePath)))
                .Returns(LircPath);
            fileSystem
                .Setup(x => x.GetRealPath(It.Is<string>(arg => arg == LircPath)))
                .Returns(LircPath);

            fileHandle.Setup(x => x.Dispose());

            var assessor = new DeviceAssessor(fileSystem.Object);

            // ACT
            var result = assessor.AssessDevice(relativePath);

            // ASSERT
            fileSystem.Verify(x => x.OpenRead(It.IsAny<string>()), Times.Once);
            fileSystem.Verify(x => x.GetFullPath(It.IsAny<string>()), Times.Once);
            fileSystem.Verify(x => x.GetRealPath(It.IsAny<string>()), Times.Once);
            fileHandle.Verify(x => x.Dispose(), Times.Once);
            fileSystem.Verify(x => x.IoCtlReadUInt32(It.IsAny<FileSystem.IOpenFile>(), It.Is<uint>(arg => arg == LircConstants.LIRC_GET_FEATURES)), Times.Once);

            Assert.That(result.Path, Is.EqualTo(LircPath));
            Assert.That(result.RealPath, Is.EqualTo(LircPath));
            Assert.That(result.IsLink, Is.False);
            Assert.That(result.CanSend, Is.True);
            Assert.That(result.CanReceive, Is.False);
            Assert.That(result.Features.HasFlag(DeviceFeatures.SendModePulse), Is.True);
            Assert.That(result.Features.HasFlag(DeviceFeatures.SetSendCarrier), Is.True);
            Assert.That(result.Features.HasFlag(DeviceFeatures.SetSendDutyCycle), Is.False);
            Assert.That(result.CurrentReceiveTimeout, Is.Null);
            Assert.That(result.MinimumReceiveTimeout, Is.Null);
            Assert.That(result.MaximumReceiveTimeout, Is.Null);
        }

        private void SetUpTimeoutQuery(Mock<FileSystem.IFileSystem> fileSystem, Mock<FileSystem.IOpenFile> fileHandle, uint command, uint result, bool timeoutQueriesWork)
        {
            var setup = fileSystem.Setup(x => x.IoCtlReadUInt32(It.Is<FileSystem.IOpenFile>(arg => arg == fileHandle.Object), It.Is<uint>(arg => arg == command)));
            if (timeoutQueriesWork)
            {
                setup.Returns(result);
            }
            else
            {
                setup.Throws(new System.ComponentModel.Win32Exception(25));
            }
        }

        [Test]
        public void AssessDevice_RX_TimeoutQueryResponds() => AssessDevice_RX(true);
        [Test]
        public void AssessDevice_RX_TimeoutQueryFails() => AssessDevice_RX(false);

        private void AssessDevice_RX(bool timeoutQueriesWork)
        {
            // ARRANGE
            const string relativePath = "../.." + LircPath;
            const uint minTimeout = 20;
            const uint maxTimeout = 500;
            const uint currentTimeout = 100;
            var fileSystem = new Mock<FileSystem.IFileSystem>(MockBehavior.Strict);
            var fileHandle = new Mock<FileSystem.IOpenFile>(MockBehavior.Strict);

            fileSystem
                .Setup(x => x.IoCtlReadUInt32(It.Is<FileSystem.IOpenFile>(arg => arg == fileHandle.Object), It.Is<uint>(arg => arg == LircConstants.LIRC_GET_FEATURES)))
                .Returns((uint)(DeviceFeatures.ReceiveModeMode2 | DeviceFeatures.ReceiveLircCode | DeviceFeatures.UseWidebandReceiver));
            fileSystem
                .Setup(x => x.OpenRead(It.Is<string>(arg => arg == LircPath)))
                .Returns(fileHandle.Object);
            fileSystem
                .Setup(x => x.GetFullPath(It.Is<string>(arg => arg == relativePath)))
                .Returns(LircPath);
            fileSystem
                .Setup(x => x.GetRealPath(It.Is<string>(arg => arg == LircPath)))
                .Returns(LircPath);

            SetUpTimeoutQuery(fileSystem, fileHandle, LircConstants.LIRC_GET_MIN_TIMEOUT, minTimeout, timeoutQueriesWork);
            SetUpTimeoutQuery(fileSystem, fileHandle, LircConstants.LIRC_GET_MAX_TIMEOUT, maxTimeout, timeoutQueriesWork);
            SetUpTimeoutQuery(fileSystem, fileHandle, LircConstants.LIRC_GET_REC_TIMEOUT, currentTimeout, timeoutQueriesWork);

            fileHandle.Setup(x => x.Dispose());

            var assessor = new DeviceAssessor(fileSystem.Object);

            // ACT
            var result = assessor.AssessDevice(relativePath);

            // ASSERT
            fileSystem.Verify(x => x.OpenRead(It.IsAny<string>()), Times.Once);
            fileSystem.Verify(x => x.GetFullPath(It.IsAny<string>()), Times.Once);
            fileSystem.Verify(x => x.GetRealPath(It.IsAny<string>()), Times.Once);
            fileHandle.Verify(x => x.Dispose(), Times.Once);
            fileSystem.Verify(x => x.IoCtlReadUInt32(It.IsAny<FileSystem.IOpenFile>(), It.Is<uint>(arg => arg == LircConstants.LIRC_GET_FEATURES)), Times.Once);
            fileSystem.Verify(x => x.IoCtlReadUInt32(It.IsAny<FileSystem.IOpenFile>(), It.Is<uint>(arg => arg == LircConstants.LIRC_GET_MIN_TIMEOUT)), Times.Once);
            fileSystem.Verify(x => x.IoCtlReadUInt32(It.IsAny<FileSystem.IOpenFile>(), It.Is<uint>(arg => arg == LircConstants.LIRC_GET_MAX_TIMEOUT)), Times.Once);
            fileSystem.Verify(x => x.IoCtlReadUInt32(It.IsAny<FileSystem.IOpenFile>(), It.Is<uint>(arg => arg == LircConstants.LIRC_GET_REC_TIMEOUT)), Times.Once);

            Assert.That(result.Path, Is.EqualTo(LircPath));
            Assert.That(result.RealPath, Is.EqualTo(LircPath));
            Assert.That(result.IsLink, Is.False);
            Assert.That(result.CanSend, Is.False);
            Assert.That(result.CanReceive, Is.True);
            Assert.That(result.Features.HasFlag(DeviceFeatures.ReceiveModeMode2), Is.True);
            Assert.That(result.Features.HasFlag(DeviceFeatures.ReceiveLircCode), Is.True);
            Assert.That(result.Features.HasFlag(DeviceFeatures.UseWidebandReceiver), Is.True);
            Assert.That(result.Features.HasFlag(DeviceFeatures.SetReceiveTimeout), Is.False);

            if (timeoutQueriesWork)
            {
                Assert.That(result.MinimumReceiveTimeout, Is.EqualTo(minTimeout));
                Assert.That(result.MaximumReceiveTimeout, Is.EqualTo(maxTimeout));
                Assert.That(result.CurrentReceiveTimeout, Is.EqualTo(currentTimeout));
            }
            else
            {
                Assert.That(result.MinimumReceiveTimeout, Is.Null);
                Assert.That(result.MaximumReceiveTimeout, Is.Null);
                Assert.That(result.CurrentReceiveTimeout, Is.Null);
            }
        }

        [Test]
        public void AssessDevice_NotIR()
        {
            // ARRANGE
            const string relativePath = "../.." + LircPath;
            var fileSystem = new Mock<FileSystem.IFileSystem>(MockBehavior.Strict);
            var fileHandle = new Mock<FileSystem.IOpenFile>(MockBehavior.Strict);

            fileSystem
                .Setup(x => x.IoCtlReadUInt32(It.Is<FileSystem.IOpenFile>(arg => arg == fileHandle.Object), It.Is<uint>(arg => arg == LircConstants.LIRC_GET_FEATURES)))
                .Throws(new System.ComponentModel.Win32Exception(25));
            fileSystem
                .Setup(x => x.OpenRead(It.Is<string>(arg => arg == LircPath)))
                .Returns(fileHandle.Object);
            fileSystem
                .Setup(x => x.GetFullPath(It.Is<string>(arg => arg == relativePath)))
                .Returns(LircPath);
            fileSystem
                .Setup(x => x.GetRealPath(It.Is<string>(arg => arg == LircPath)))
                .Returns(LircPath);

            fileHandle.Setup(x => x.Dispose());

            var assessor = new DeviceAssessor(fileSystem.Object);

            // ACT, ASSERT
            Assert.That(() => assessor.AssessDevice(relativePath), Throws.Exception.TypeOf<Exceptions.NotAnIRDeviceException>());

            // ASSERT
            fileSystem.Verify(x => x.OpenRead(It.IsAny<string>()), Times.Once);
            fileHandle.Verify(x => x.Dispose(), Times.Once);
            fileSystem.Verify(x => x.IoCtlReadUInt32(It.IsAny<FileSystem.IOpenFile>(), It.Is<uint>(arg => arg == LircConstants.LIRC_GET_FEATURES)), Times.Once);
        }

        [Test]
        public void AssessDevice_OtherFailure()
        {
            // ARRANGE
            var fileSystem = new Mock<FileSystem.IFileSystem>(MockBehavior.Strict);
            var fileHandle = new Mock<FileSystem.IOpenFile>(MockBehavior.Strict);

            fileSystem
                .Setup(x => x.IoCtlReadUInt32(It.Is<FileSystem.IOpenFile>(arg => arg == fileHandle.Object), It.Is<uint>(arg => arg == LircConstants.LIRC_GET_FEATURES)))
                .Throws(new System.ComponentModel.Win32Exception(22));
            fileSystem
                .Setup(x => x.OpenRead(It.Is<string>(arg => arg == LircPath)))
                .Returns(fileHandle.Object);
            fileSystem
                .Setup(x => x.GetFullPath(It.Is<string>(arg => arg == LircPath)))
                .Returns(LircPath);
            fileSystem
                .Setup(x => x.GetRealPath(It.Is<string>(arg => arg == LircPath)))
                .Returns(LircPath);

            fileHandle.Setup(x => x.Dispose());

            var assessor = new DeviceAssessor(fileSystem.Object);

            // ACT, ASSERT
            Assert.That(() => assessor.AssessDevice(LircPath), Throws.TypeOf<System.ComponentModel.Win32Exception>());

            // ASSERT
            fileSystem.Verify(x => x.OpenRead(It.IsAny<string>()), Times.Once);
            fileHandle.Verify(x => x.Dispose(), Times.Once);
        }

        [Test]
        public void AssessDevice_DeviceNotExist()
        {
            // ARRANGE
            var fileSystem = new Mock<FileSystem.IFileSystem>(MockBehavior.Strict);

            fileSystem
                .Setup(x => x.OpenRead(It.Is<string>(arg => arg == LircPath)))
                .Throws(new System.IO.FileNotFoundException("Unit test exception to say this file is not here."));
            fileSystem
                .Setup(x => x.GetFullPath(It.Is<string>(arg => arg == LircPath)))
                .Returns(LircPath);
            fileSystem
                .Setup(x => x.GetRealPath(It.Is<string>(arg => arg == LircPath)))
                .Returns(LircPath);

            var assessor = new DeviceAssessor(fileSystem.Object);

            // ACT, ASSERT
            Assert.That(() => assessor.AssessDevice(LircPath), Throws.TypeOf<System.IO.FileNotFoundException>());
        }

        [Test]
        [TestCase("../../lirc-tx")]
        [TestCase("/dev/lirc-tx")]
        public void AssessDevice_SymLink(string path)
        {
            // ARRANGE
            const string absolutePath = "/dev/lirc-tx";
            var fileSystem = new Mock<FileSystem.IFileSystem>(MockBehavior.Strict);
            var fileHandle = new Mock<FileSystem.IOpenFile>(MockBehavior.Strict);

            fileSystem
                .Setup(x => x.IoCtlReadUInt32(It.Is<FileSystem.IOpenFile>(arg => arg == fileHandle.Object), It.Is<uint>(arg => arg == LircConstants.LIRC_GET_FEATURES)))
                .Returns((uint)(DeviceFeatures.SendModePulse | DeviceFeatures.SetSendCarrier));
            fileSystem
                .Setup(x => x.OpenRead(It.Is<string>(arg => arg == absolutePath)))
                .Returns(fileHandle.Object);
            fileSystem
                .Setup(x => x.GetFullPath(It.Is<string>(arg => arg == path)))
                .Returns(absolutePath);
            fileSystem
                .Setup(x => x.GetRealPath(It.Is<string>(arg => arg == absolutePath)))
                .Returns(LircPath);


            fileHandle.Setup(x => x.Dispose());

            var assessor = new DeviceAssessor(fileSystem.Object);

            // ACT
            var result = assessor.AssessDevice(path);

            // ASSERT
            fileSystem.Verify(x => x.OpenRead(It.IsAny<string>()), Times.Once);
            fileSystem.Verify(x => x.GetFullPath(It.IsAny<string>()), Times.Once);
            fileSystem.Verify(x => x.GetRealPath(It.IsAny<string>()), Times.Once);
            fileHandle.Verify(x => x.Dispose(), Times.Once);

            Assert.That(result.Path, Is.EqualTo(absolutePath));
            Assert.That(result.RealPath, Is.EqualTo(LircPath));
            Assert.That(result.IsLink, Is.True);
        }

    }
}
