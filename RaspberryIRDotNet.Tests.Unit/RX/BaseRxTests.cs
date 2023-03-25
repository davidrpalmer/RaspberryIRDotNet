using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using RaspberryIRDotNet.RX;

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
            fileHandle
                .Setup(x => x.IoCtlReadUInt32(It.Is<uint>(arg => arg == LircConstants.LIRC_GET_FEATURES)))
                .Returns((uint)DeviceFeatures.ReceiveModeMode2);
            fileHandle
                .Setup(x => x.IoCtlReadUInt32(It.Is<uint>(arg => arg == LircConstants.LIRC_GET_REC_MODE)))
                .Returns(LircConstants.LIRC_MODE_MODE2);

            ReadFromDeviceMockHelper readHelper = new ReadFromDeviceMockHelper(readBuffer);
            fileHandle.Setup(x => x.ReadFromDevice(It.IsNotNull<byte[]>(), It.IsAny<ReadCancellationToken>()))
                .Returns<byte[], ReadCancellationToken>(readHelper.DoNextRead);

            return fileHandle;
        }

        class ReadFromDeviceMockHelper
        {
            private readonly List<byte> _readBuffer = new List<byte>();

            private int _nextIndex = 0;

            public ReadFromDeviceMockHelper(List<byte> readBuffer)
            {
                _readBuffer = readBuffer ?? throw new ArgumentNullException(nameof(readBuffer));
            }

            public int DoNextRead(byte[] buffer, ReadCancellationToken cancellationToken)
            {
                _readBuffer.CopyTo(_nextIndex, buffer, 0, buffer.Length);
                _nextIndex += buffer.Length;
                return buffer.Length;
            }
        }


        protected static Mock<FileSystem.IFileSystem> MockFileSystem(ICollection<Mock<FileSystem.IOpenFile>> fileHandles)
        {
            var fileSystem = new Mock<FileSystem.IFileSystem>(MockBehavior.Strict);

            fileSystem
                .Setup(x => x.OpenRead(It.Is<string>(arg => arg == LircPath)))
                .Returns(new Queue<FileSystem.IOpenFile>(fileHandles.Select(x => x.Object)).Dequeue);

            return fileSystem;
        }
    }
}
