using System;
using System.IO;

namespace RaspberryIRDotNet.FileSystem
{
    class RealFileStreamWrapper : IOpenFile
    {
        public FileStream FileStream { get; }

        Stream IOpenFile.Stream => FileStream;

        public RealFileStreamWrapper(FileStream fileStream)
        {
            if (fileStream == null) { throw new ArgumentNullException(); }
            FileStream = fileStream;
        }

        public void Dispose()
        {
            FileStream.Dispose();
        }
    }
}
