using System;

namespace RaspberryIRDotNet
{
    internal static class LinuxErrorCodes
    {
        /// <summary>
        /// Inappropriate ioctl for device
        /// </summary>
        public const int ENOTTY = 25;

        /// <summary>
        /// Function not implemented
        /// </summary>
        public const int ENOSYS = 38;

        /// <summary>
        /// Invalid argument
        /// </summary>
        public const int EINVAL = 22;
    }
}
