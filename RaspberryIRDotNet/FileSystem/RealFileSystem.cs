using System;
using System.IO;
using System.Text;

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
            StringBuilder realPath = new();
            if (Native.RealPath(linkPath, realPath) == 0)
            {
                throw new System.ComponentModel.Win32Exception();
            }
            return realPath.ToString();
        }

        public IOpenFile OpenRead(string path)
        {
            return new RealOpenFile(File.Open(path, FileMode.Open, FileAccess.Read));
        }

        public IOpenFile OpenWrite(string path)
        {
            return new RealOpenFile(File.Open(path, FileMode.Open, FileAccess.Write));
        }
    }
}
