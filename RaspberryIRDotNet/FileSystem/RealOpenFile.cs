using System;
using System.IO;
using RaspberryIRDotNet.RX;

namespace RaspberryIRDotNet.FileSystem
{
    class RealOpenFile : IOpenFile
    {
        private readonly FileStream _fileStream;

        public RealOpenFile(FileStream fileStream)
        {
            if (fileStream == null) { throw new ArgumentNullException(); }
            _fileStream = fileStream;
        }

        public void Dispose()
        {
            _fileStream.Dispose();
        }

        /// <summary>
        /// Unlike <see cref="FileStream.Write"/> this method waits for the IR device to transmit before returning.
        /// </summary>
        public void WriteToDevice(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }
            if (_disposedValue)
            {
                throw new ObjectDisposedException(nameof(RealOpenFile));
            }
            if (!_fileStream.CanWrite)
            {
                throw new InvalidOperationException("Device not opened for writing.");
            }

            var handle = _fileStream.SafeFileHandle;

            int result = Native.Write(handle, buffer, buffer.Length);
            ThrowIfIOError(result);

            if (result != buffer.Length)
            {
                throw new IOException($"Did not write the expected number of bytes to the IR device. Expected={buffer.Length}, Actual={result}");
            }
        }

        public int ReadFromDevice(byte[] buffer, ReadCancellationToken cancellationToken)
        {
            const int cancelIndicator = -1;
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }
            if (cancellationToken == null)
            {
                throw new ArgumentNullException(nameof(cancellationToken));
            }
            if (_disposedValue)
            {
                throw new ObjectDisposedException(nameof(RealOpenFile));
            }
            if (!_fileStream.CanRead)
            {
                throw new InvalidOperationException("Device not opened for reading.");
            }

            if (!cancellationToken.AddReference())
            {
                return cancelIndicator;
            }
            try
            {
                Native.pollfd[] fds = new Native.pollfd[]
                {
                    new Native.pollfd()
                    {
                         fd = _fileStream.SafeFileHandle.DangerousGetHandle().ToInt32(),
                         events = Native.POLL_EVENTS.POLLIN
                    }
                };

                const int pollTime = 1000; // How responsive vs how wasteful should the cancel be. Lower is more responsive, but wastes CPU time.

                while (true)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return cancelIndicator;
                    }

                    var pollResult = Native.Poll(fds, fds.Length, pollTime);
                    ThrowIfIOError(pollResult);

                    if (cancellationToken.IsCancellationRequested)
                    {
                        return cancelIndicator;
                    }

                    if (pollResult > 0)
                    {
                        return _fileStream.Read(buffer, 0, buffer.Length);
                    }
                }
            }
            finally
            {
                cancellationToken.ReleaseReference();
            }
        }

        public uint IoCtlReadUInt32(uint request)
        {
            var handle = _fileStream.SafeFileHandle;

            int ioCtlResult = Native.IOCtlRead(handle, request, out uint data);
            ThrowIfIOError(ioCtlResult);
            return data;
        }

        public void IoCtlWrite(uint request, uint data)
        {
            var handle = _fileStream.SafeFileHandle;

            int ioCtlResult = Native.IOCtlWrite(handle, request, ref data);
            ThrowIfIOError(ioCtlResult);
        }

        private void ThrowIfIOError(int returnCode)
        {
            if (returnCode < 0)
            {
                throw new System.ComponentModel.Win32Exception();
            }
        }
    }
}
