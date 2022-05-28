using System;
using RaspberryIRDotNet.Exceptions;
using RaspberryIRDotNet.FileSystem;

namespace RaspberryIRDotNet
{
    internal static class Utility
    {
        public const int UnitDurationMinimum = 1;
        public const int UnitDurationMaximum = 500000;
        public const int MessageDurationMaximum = 500000; // The whole IR transmission cannot exceed this duration. Individual units can be any size so long as the total is less than this.

        public static int RoundMicrosecs(int sample, int expectedUnitDuration)
        {
            if (sample < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sample), sample, "Cannot use negative numbers.");
            }
            if (sample + (long)expectedUnitDuration >= int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(sample), sample, "Number is too close to overflowing to round.");
            }
            return (int)Math.Round(sample / (double)expectedUnitDuration) * expectedUnitDuration;
        }

        /// <summary>
        /// Set the integer value for IR inactivity timeout (microseconds). A value of 0 (if supported by the hardware) disables all hardware timeouts and data should be reported as soon as possible. If the exact value cannot be set, then the next possible value greater than the given value should be set by the driver.
        /// </summary>
        public static void SetRxTimeout(IOpenFile file, int timeoutMicrosecs)
        {
            if (timeoutMicrosecs < 0) { throw new ArgumentOutOfRangeException(nameof(timeoutMicrosecs), timeoutMicrosecs, "Timeout must be a positive integer."); }
            uint timeoutUnsigned = checked((uint)timeoutMicrosecs);
            SetRxTimeout(file, timeoutUnsigned);
        }

        /// <summary>
        /// Set the integer value for IR inactivity timeout (microseconds). A value of 0 (if supported by the hardware) disables all hardware timeouts and data should be reported as soon as possible. If the exact value cannot be set, then the next possible value greater than the given value should be set by the driver.
        /// </summary>
        public static void SetRxTimeout(IOpenFile file, uint timeoutMicrosecs)
        {
            try
            {
                file.IoCtlWrite(LircConstants.LIRC_SET_REC_TIMEOUT, timeoutMicrosecs);
            }
            catch (System.ComponentModel.Win32Exception err)
            {
                throw new DeviceOptionSetException(DeviceOptionSetException.Option.Timeout, err);
            }
        }

        public static DeviceFeatures GetFeatures(IOpenFile file)
        {
            try
            {
                return (DeviceFeatures)file.IoCtlReadUInt32(LircConstants.LIRC_GET_FEATURES);
            }
            catch (System.ComponentModel.Win32Exception err) when (err.NativeErrorCode == LinuxErrorCodes.ENOTTY)
            {
                throw new NotAnIRDeviceException($"The {nameof(LircConstants.LIRC_GET_FEATURES)} command not supported at this path. All IR devices must support this command so perhaps this path is not an IR device? IR devices are typically at a path like /dev/lirc0.", err);
            }
        }
    }
}
