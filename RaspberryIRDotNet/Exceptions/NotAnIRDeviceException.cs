using System;

namespace RaspberryIRDotNet.Exceptions
{
    public class NotAnIRDeviceException : Exception
    {
        public NotAnIRDeviceException(string message) : base(message)
        {
        }

        public NotAnIRDeviceException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
