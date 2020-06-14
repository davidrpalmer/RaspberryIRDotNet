using System;

namespace RaspberryIRDotNet.Exceptions
{
    /// <summary>
    /// When searching for the IR device, but there isn't one.
    /// </summary>
    public class NoIRDevicesFoundException : Exception
    {
        public NoIRDevicesFoundException(string message) : base(message)
        {
        }
    }
}
