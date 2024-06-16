using System;
using System.IO;
using System.IO.Pipes;
using RaspberryIRDotNet.RX;

namespace RaspberryIRDotNet.FileSystem
{
    class RealOpenFile : IOpenFile
    {
        private const int _readCancelIndicator = -1;

        private readonly FileStream _fileStream;

        private AnonymousPipeServerStream _pipeServer;
        private AnonymousPipeClientStream _pipeClient;

        private bool _disposedValue;

        public RealOpenFile(FileStream fileStream)
        {
            _fileStream = fileStream ?? throw new ArgumentNullException(nameof(fileStream));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _fileStream.Dispose();

                    _pipeClient?.Dispose();
                    _pipeServer?.DisposeLocalCopyOfClientHandle();
                    _pipeServer?.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Unlike <see cref="FileStream.Write"/> this method waits for the IR device to transmit before returning.
        /// </summary>
        public void WriteToDevice(byte[] buffer)
        {
            ArgumentNullException.ThrowIfNull(buffer);
            ObjectDisposedException.ThrowIf(_disposedValue, this);
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
            ArgumentNullException.ThrowIfNull(buffer);
            ArgumentNullException.ThrowIfNull(cancellationToken);
            ObjectDisposedException.ThrowIf(_disposedValue, this);
            if (!_fileStream.CanRead)
            {
                throw new InvalidOperationException("Device not opened for reading.");
            }

            if (_pipeServer == null)
            {
                // We could create a new pipe (and dispose it) for each read, but reads are often done in tight loops so better to cache the pipe.
                _pipeServer = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.None);
                _pipeClient = new AnonymousPipeClientStream(PipeDirection.In, _pipeServer.ClientSafePipeHandle);
            }

            void CancelEventHandler(object sender, EventArgs e)
            {
                _pipeServer.WriteByte(1); // Unblock poll()
            }

            cancellationToken.CancellationRequested += CancelEventHandler;
            try
            {
                if (!cancellationToken.AddReference())
                {
                    return _readCancelIndicator;
                }
                try
                {
                    return ReadFromDeviceInner(buffer, cancellationToken);
                }
                finally
                {
                    cancellationToken.ReleaseReference();
                }
            }
            finally
            {
                cancellationToken.CancellationRequested -= CancelEventHandler;
            }
        }

        private int ReadFromDeviceInner(byte[] buffer, ReadCancellationToken cancellationToken)
        {
            Native.pollfd[] fds =
            [
                new()
                {
                    fd = _pipeClient.SafePipeHandle.DangerousGetHandle().ToInt32(),
                    events = Native.POLL_EVENTS.POLLIN
                },
                new()
                {
                    fd = _fileStream.SafeFileHandle.DangerousGetHandle().ToInt32(),
                    events = Native.POLL_EVENTS.POLLIN
                }
            ];

            while (true)
            {
                var pollResult = Native.Poll(fds, fds.Length, -1);
                ThrowIfIOError(pollResult);

                if (fds[0].revents.HasFlag(Native.POLL_EVENTS.POLLIN))
                {
                    _pipeClient.ReadByte(); // Reset the pipe for next time
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    return _readCancelIndicator;
                }

                if (fds[1].revents.HasFlag(Native.POLL_EVENTS.POLLIN))
                {
                    return _fileStream.Read(buffer, 0, buffer.Length);
                }
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

        private static void ThrowIfIOError(int returnCode)
        {
            if (returnCode < 0)
            {
                throw new System.ComponentModel.Win32Exception();
            }
        }
    }
}
