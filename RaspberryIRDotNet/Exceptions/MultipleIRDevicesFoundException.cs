using System;

namespace RaspberryIRDotNet.Exceptions
{
    /// <summary>
    /// When trying to get the only IR device (or the only device matching specific criteria) there were multiple results.
    /// </summary>
    public class MultipleIRDevicesFoundException : Exception
    {
        public MultipleIRDevicesFoundException(string message) : base(message)
        {
        }
    }
}
