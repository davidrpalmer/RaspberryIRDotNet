using System;

namespace RaspberryIRDotNet.FileSystem
{
    public interface IFileSystem
    {
        string GetFullPath(string path);

        string GetRealPath(string linkPath);

        IOpenFile OpenRead(string path);

        IOpenFile OpenWrite(string path);
    }
}
