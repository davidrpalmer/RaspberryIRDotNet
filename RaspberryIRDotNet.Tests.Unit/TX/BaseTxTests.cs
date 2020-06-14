using System;
using Moq;

namespace RaspberryIRDotNet.Tests.Unit.TX
{
    class BaseTxTests
    {
        protected const string LircPath = "/dev/lirc5";

        protected static Mock<FileSystem.IOpenFile> MakeMockFileHandle()
        {
            var fileHandle = new Mock<FileSystem.IOpenFile>(MockBehavior.Strict);
            fileHandle.Setup(x => x.Dispose());
            return fileHandle;
        }


        protected static Mock<FileSystem.IFileSystem> MockFileSystem(Mock<FileSystem.IOpenFile> fileHandle, Action<byte[]> writeCallBack = null)
        {
            var fileSystem = new Mock<FileSystem.IFileSystem>(MockBehavior.Strict);

            fileSystem
                .Setup(x => x.IoCtlReadUInt32(It.IsNotNull<FileSystem.IOpenFile>(), It.Is<uint>(arg => arg == LircConstants.LIRC_GET_FEATURES)))
                .Returns((uint)DeviceFeatures.SendModePulse);
            fileSystem
                .Setup(x => x.IoCtlReadUInt32(It.IsNotNull<FileSystem.IOpenFile>(), It.Is<uint>(arg => arg == LircConstants.LIRC_GET_SEND_MODE)))
                .Returns(LircConstants.LIRC_MODE_PULSE);
            fileSystem
                .Setup(x => x.OpenWrite(It.Is<string>(arg => arg == LircPath)))
                .Returns(fileHandle.Object);
            var writeMock = fileSystem
                .Setup(x => x.WriteToDevice(It.Is<FileSystem.IOpenFile>(arg => arg == fileHandle.Object), It.IsNotNull<byte[]>()));
            if (writeCallBack != null)
            {
                Action<FileSystem.IOpenFile, byte[]> callback = (file, data) => writeCallBack(data);
                writeMock.Callback(callback);
            }


            return fileSystem;
        }

        protected IReadOnlyPulseSpaceDurationList MakeDataToSend()
        {
            return new PulseSpaceDurationList()
            {
                800, //P 1-8
                500, //S 9-13
                100, //P 14
                300, //S 15-17
                100, //P 18
                300, //S 19-21
                100  //P 22
            };
        }
    }
}
