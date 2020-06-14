using System;

namespace RaspberryIRDotNet.FileSystem
{
    public interface IFileSystem
    {
        string GetFullPath(string path);

        string GetRealPath(string linkPath);

        IOpenFile OpenRead(string path);

        IOpenFile OpenWrite(string path);

        void WriteToDevice(IOpenFile file, byte[] buffer);

        uint IoCtlReadUInt32(IOpenFile file, uint request);

        void IoCtlWrite(IOpenFile file, uint request, uint data);
    }
}
