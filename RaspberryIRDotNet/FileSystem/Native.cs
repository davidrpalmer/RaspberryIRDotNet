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

        [DllImport("libc", EntryPoint = "poll", SetLastError = true)]
        public static extern int Poll([In, Out] pollfd[] fds, int fdsCount, int timeout);

        [Flags]
        public enum POLL_EVENTS : ushort
        {
            NONE = 0x0000,
            POLLIN = 0x001,
            POLLPRI = 0x002,
            POLLOUT = 0x004,
            POLLMSG = 0x400,
            POLLREMOVE = 0x1000,
            POLLRDHUP = 0x2000,
            // output only
            POLLERR = 0x008,
            POLLHUP = 0x010,
            POLLNVAL = 0x020
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct pollfd
        {
            public int fd;
            public POLL_EVENTS events;
            public POLL_EVENTS revents;
        }
    }
}
