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
            StringBuilder realPath = new StringBuilder();
            if (Native.RealPath(linkPath, realPath) == 0)
            {
                ThrowLastNativeError();
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

        private void ThrowLastNativeError()
        {
            int lastError = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
            throw new System.ComponentModel.Win32Exception(lastError); // The even though the name is Win32 this exception actually works for Linux and gives the right messages.
        }
    }
}
