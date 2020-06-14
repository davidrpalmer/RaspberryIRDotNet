using System;
using System.Collections.Generic;
using System.Linq;
using Moq;

namespace RaspberryIRDotNet.Tests.Unit.RX
{
    class BaseRxTests
    {
        protected const string LircPath = "/dev/lirc8";

        protected static Mock<FileSystem.IOpenFile> MakeMockFileHandle(params int[][] pulseSpaceDatas)
        {
            List<byte> readBuffer = new List<byte>();

            foreach (var pulseSpaceData in pulseSpaceDatas)
            {
                bool pulse = true;
                foreach (int data in pulseSpaceData)
                {
                    byte[] bytes = BitConverter.GetBytes(data);

                    if (pulse)
                    {
                        bytes[3] = 1;
                    }
                    else
                    {
                        bytes[3] = 0;
                    }

                    readBuffer.AddRange(bytes);

                    pulse = !pulse;
                }
                readBuffer.Add(0);
                readBuffer.Add(0);
                readBuffer.Add(0);
                readBuffer.Add(3); // Timeout.
            }

            var fileHandle = new Mock<FileSystem.IOpenFile>(MockBehavior.Strict);
            fileHandle.Setup(x => x.Dispose());
            fileHandle.SetupGet(x => x.Stream).Returns(new System.IO.MemoryStream(readBuffer.ToArray()));
            return fileHandle;
        }


        protected static Mock<FileSystem.IFileSystem> MockFileSystem(ICollection<Mock<FileSystem.IOpenFile>> fileHandles)
        {
            var fileSystem = new Mock<FileSystem.IFileSystem>(MockBehavior.Strict);

            fileSystem
                .Setup(x => x.IoCtlReadUInt32(It.IsNotNull<FileSystem.IOpenFile>(), It.Is<uint>(arg => arg == LircConstants.LIRC_GET_FEATURES)))
                .Returns((uint)DeviceFeatures.ReceiveModeMode2);
            fileSystem
                .Setup(x => x.IoCtlReadUInt32(It.IsNotNull<FileSystem.IOpenFile>(), It.Is<uint>(arg => arg == LircConstants.LIRC_GET_REC_MODE)))
                .Returns(LircConstants.LIRC_MODE_MODE2);
            fileSystem
                .Setup(x => x.OpenRead(It.Is<string>(arg => arg == LircPath)))
                .Returns(new Queue<FileSystem.IOpenFile>(fileHandles.Select(x => x.Object)).Dequeue);

            return fileSystem;
        }
    }
}
