using System;

namespace RaspberryIRDotNet.Exceptions
{
    public class InvalidPulseSpaceDataException : Exception
    {
        public InvalidPulseSpaceDataException(string message) : base(message)
        {
        }

        public InvalidPulseSpaceDataException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
