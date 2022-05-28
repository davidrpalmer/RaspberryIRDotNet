using System;
using NUnit.Framework;
using Moq;
using System.ComponentModel;

namespace RaspberryIRDotNet.Tests.Unit
{
    class UtilityTests
    {
        [Test]
        public void GetFeatures_ValidValue()
        {
            // ARRANGE
            var fileHandle = new Mock<FileSystem.IOpenFile>(MockBehavior.Strict);
            fileHandle
                .Setup(x => x.IoCtlReadUInt32(It.Is<uint>(arg => arg == LircConstants.LIRC_GET_FEATURES)))
                .Returns((uint)(DeviceFeatures.ReceiveModeMode2 | DeviceFeatures.SetSendCarrier));

            // ACT
            var features = Utility.GetFeatures(fileHandle.Object);

            // ASSERT
            Assert.That(features.HasFlag(DeviceFeatures.ReceiveModeMode2));
            Assert.That(features.HasFlag(DeviceFeatures.SetSendCarrier));
        }

        [Test]
        public void GetFeatures_NotAnIRDevice()
        {
            // ARRANGE
            var fileHandle = new Mock<FileSystem.IOpenFile>(MockBehavior.Strict);
            fileHandle
                .Setup(x => x.IoCtlReadUInt32(It.Is<uint>(arg => arg == LircConstants.LIRC_GET_FEATURES)))
                .Throws(new Win32Exception(LinuxErrorCodes.ENOTTY)); // This exception code means we don't support the IOCTL request here.

            // ACT, ASSERT
            Assert.That(() => Utility.GetFeatures(fileHandle.Object), Throws.TypeOf<Exceptions.NotAnIRDeviceException>().With.InnerException.TypeOf<Win32Exception>().With.InnerException.Property("NativeErrorCode").EqualTo(25));
        }

        [Test]
        public void GetFeatures_UnexpectedIOCTLError()
        {
            // ARRANGE
            const int errorCode = 500; // Just make up a code here that should not be handled by anything else.
            var fileHandle = new Mock<FileSystem.IOpenFile>(MockBehavior.Strict);
            fileHandle
                .Setup(x => x.IoCtlReadUInt32(It.Is<uint>(arg => arg == LircConstants.LIRC_GET_FEATURES)))
                .Throws(new Win32Exception(errorCode));

            // ACT, ASSERT
            Assert.That(() => Utility.GetFeatures(fileHandle.Object), Throws.TypeOf<Win32Exception>().With.Property("NativeErrorCode").EqualTo(errorCode));
        }

        [Test]
        [TestCase(-1)]
        [TestCase(int.MinValue)]
        public void SetRxTimeout_Negative(int value)
        {
            // ARRANGE
            var fileHandle = new Mock<FileSystem.IOpenFile>(MockBehavior.Strict);

            // ACT, ASSERT
            Assert.That(() => Utility.SetRxTimeout(fileHandle.Object, value), Throws.TypeOf<ArgumentOutOfRangeException>().With.Property("ActualValue").EqualTo(value));
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(1000)]
        [TestCase(int.MaxValue)]
        public void SetRxTimeout_ValidInt32Value(int input)
        {
            // ARRANGE
            var fileHandle = new Mock<FileSystem.IOpenFile>(MockBehavior.Strict);
            fileHandle.Setup(x => x.IoCtlWrite(
                    It.Is<uint>(arg => arg == LircConstants.LIRC_SET_REC_TIMEOUT),
                    It.Is<uint>(arg => arg == input)
                    ));

            // ACT
            Utility.SetRxTimeout(fileHandle.Object, input);

            // ASSERT
            fileHandle.Verify(x => x.IoCtlWrite(It.IsAny<uint>(), It.IsAny<uint>()), Times.Once);
        }

        [Test]
        [TestCase(0U)]
        [TestCase(1U)]
        [TestCase(1000U)]
        [TestCase(uint.MaxValue)]
        public void SetRxTimeout_ValidUInt32Value(uint input)
        {
            // ARRANGE
            var fileHandle = new Mock<FileSystem.IOpenFile>(MockBehavior.Strict);
            fileHandle.Setup(x => x.IoCtlWrite(
                    It.Is<uint>(arg => arg == LircConstants.LIRC_SET_REC_TIMEOUT),
                    It.Is<uint>(arg => arg == input)
                    ));

            // ACT
            Utility.SetRxTimeout(fileHandle.Object, input);

            // ASSERT
            fileHandle.Verify(x => x.IoCtlWrite(It.IsAny<uint>(), It.IsAny<uint>()), Times.Once);
        }

        [Test]
        [TestCase(0, ExpectedResult = 0)]
        [TestCase(49, ExpectedResult = 0)]
        [TestCase(51, ExpectedResult = 100)]
        [TestCase(190, ExpectedResult = 200)]
        [TestCase(4095, ExpectedResult = 4100)]
        [TestCase(2147483247, ExpectedResult = 2147483200)] // A large number but not in danger of overflowing.
        public int RoundMicroseconds(int input)
        {
            return Utility.RoundMicrosecs(input, 100);
        }

        [Test]
        [TestCase(2147483647)] /// <see cref="int.MaxValue"/>
        [TestCase(2147483646)]
        [TestCase(2147483645)]
        public void RoundMicroseconds_InDangerOfOverflow(int input)
        {
            Assert.That(() => Utility.RoundMicrosecs(input, 10), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void RoundMicroseconds_Negative()
        {
            Assert.That(() => Utility.RoundMicrosecs(-1, 10), Throws.TypeOf<ArgumentOutOfRangeException>());
        }
    }
}
