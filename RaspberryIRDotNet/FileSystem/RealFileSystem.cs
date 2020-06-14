using System;
using System.IO;
using Microsoft.Win32.SafeHandles;

namespace RaspberryIRDotNet.FileSystem
{
    class RealFileSystem : IFileSystem
    {
        public string GetFullPath(string path)
        {
            return Path.GetFullPath(path);
        }

        public string GetRealPath(string linkPath)
        {
            return Mono.Unix.UnixPath.GetRealPath(linkPath);
        }

        public IOpenFile OpenRead(string path)
        {
            return new RealFileStreamWrapper(File.Open(path, FileMode.Open, FileAccess.Read));
        }

        public IOpenFile OpenWrite(string path)
        {
            return new RealFileStreamWrapper(File.Open(path, FileMode.Open, FileAccess.Write));
        }

        /// <summary>
        /// Unlike <see cref="FileStream.Write"/> this method waits for the IR device to transmit before returning.
        /// </summary>
        public void WriteToDevice(IOpenFile file, byte[] buffer)
        {
            var handle = FileToHandle(file);

            int result = Native.Write(handle, buffer, buffer.Length);
            ThrowIfIOError(result);

            if (result != buffer.Length)
            {
                throw new IOException($"Did not write the expected number of bytes to the IR device. Expected={buffer.Length}, Actual={result}");
            }
        }

        public uint IoCtlReadUInt32(IOpenFile file, uint request)
        {
            var handle = FileToHandle(file);

            int ioCtlResult = Native.IOCtlRead(handle, request, out uint data);
            ThrowIfIOError(ioCtlResult);
            return data;
        }

        public void IoCtlWrite(IOpenFile file, uint request, uint data)
        {
            var handle = FileToHandle(file);

            int ioCtlResult = Native.IOCtlWrite(handle, request, ref data);
            ThrowIfIOError(ioCtlResult);
        }

        private SafeFileHandle FileToHandle(IOpenFile file)
        {
            if (file == null) { throw new ArgumentNullException(nameof(file)); }
            return ((RealFileStreamWrapper)file).FileStream.SafeFileHandle;
        }

        private void ThrowIfIOError(int returnCode)
        {
            if (returnCode < 0)
            {
                int lastError = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
                throw new System.ComponentModel.Win32Exception(lastError); // The even though the name is Win32 this exception actually works for Linux and gives the right messages.
            }
        }
    }
}
