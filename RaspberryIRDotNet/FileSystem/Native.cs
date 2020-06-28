using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace RaspberryIRDotNet.FileSystem
{
    internal static class Native
    {
        [DllImport("libc", EntryPoint = "ioctl", SetLastError = true)]
        public static extern int IOCtlRead(SafeFileHandle handle, uint request, out uint data);

        [DllImport("libc", EntryPoint = "ioctl", SetLastError = true)]
        public static extern int IOCtlWrite(SafeFileHandle handle, uint request, ref uint data); // Must be a ref, even though we don't need the result.

        [DllImport("libc", EntryPoint = "write", SetLastError = true)]
        public static extern int Write(SafeFileHandle handle, byte[] data, int length);

        [DllImport("libc", EntryPoint = "realpath", SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int RealPath(string fileName, System.Text.StringBuilder resolvedName);
    }
}
