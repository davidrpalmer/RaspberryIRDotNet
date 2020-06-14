using System;

namespace RaspberryIRDotNet.Exceptions
{
    /// <summary>
    /// Occurs when setting a device option (such as timeout or duty cycle) fails.
    /// </summary>
    public class DeviceOptionSetException : Exception
    {
        public enum Option
        {
            Timeout,
            DutyCycle,
            CarrierFrequency
        }

        public Option FailedOption { get; }

        public DeviceOptionSetException(Option option, Exception innerException) : base($"Failed to set the {option} on the device.", innerException)
        {
            FailedOption = option;
        }
    }
}
