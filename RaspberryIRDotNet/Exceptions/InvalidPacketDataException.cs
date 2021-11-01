using System;

namespace RaspberryIRDotNet.Exceptions
{
    /// <summary>
    /// When a the data for an IR packet in the PacketFormats namespace is invalid. Not related to the packets that come from the LIRC device.
    /// </summary>
    public class InvalidPacketDataException : Exception
    {
        public InvalidPacketDataException(string message) : base(message)
        {
        }

        public InvalidPacketDataException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
