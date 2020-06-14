using System;

namespace RaspberryIRDotNet
{
    /// <summary>
    /// Defines some of the default settings for the IR device drivers on the Raspberry Pi.
    /// </summary>
    public static class IRDeviceDefaults
    {
        /// <summary>
        /// 50%
        /// </summary>
        /// <remarks>
        /// Default as measured with a scope on a Raspberry Pi 3 running Raspbian Buster, Kernel:4.19
        /// </remarks>
        public const int DutyCycle = 50;

        /// <summary>
        /// 38KHz
        /// </summary>
        /// <remarks>
        /// Default as measured with a scope on a Raspberry Pi 3 running Raspbian Buster, Kernel:4.19
        /// </remarks>
        public const int Frequency = 38000;

        /// <summary>
        /// It's in microseconds, so in milliseconds it's just under 13ms.
        /// </summary>
        /// <remarks>
        /// When the driver on the Raspberry Pi is restarted the value it has is 12667, so presumably that is the default.
        /// </remarks>
        public const int Timeout = 12667;
    }
}
