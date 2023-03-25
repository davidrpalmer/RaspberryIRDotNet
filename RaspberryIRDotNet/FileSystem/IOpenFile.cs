using System;
using RaspberryIRDotNet.RX;

namespace RaspberryIRDotNet.FileSystem
{
    public interface IOpenFile : IDisposable
    {
        void WriteToDevice(byte[] buffer);

        /// <summary>
        /// Blocks until data is read from the device, or until cancelled.
        /// </summary>
        /// <param name="buffer">The buffer to populate with the read data.</param>
        /// <returns>
        /// The number of bytes that have been read, or -1 if the read was cancelled before data was read.
        /// </returns>
        int ReadFromDevice(byte[] buffer, ReadCancellationToken cancellationToken);

        uint IoCtlReadUInt32(uint request);

        void IoCtlWrite(uint request, uint data);
    }
}
