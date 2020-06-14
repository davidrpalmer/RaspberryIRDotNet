using System;
using System.IO;

namespace RaspberryIRDotNet.FileSystem
{
    public interface IOpenFile : IDisposable
    {
        Stream Stream { get; }
    }
}
