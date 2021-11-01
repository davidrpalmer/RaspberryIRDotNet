using System;

namespace RaspberryIRDotNet.Exceptions
{
    /// <summary>
    /// When a packet from the LIRC device is not recognised. Not related to IR packets in the PacketFormats namespace.
    /// </summary>
    public class UnknownLircPacketTypeException : Exception
    {
        /// <summary>
        /// The packet type identifier that was not recognised.
        /// </summary>
        public byte Mode2PacketType { get; }

        public UnknownLircPacketTypeException(Mode2PacketType type) : base($"Unknown packet type {type}.")
        {
            Mode2PacketType = (byte)type;
        }
    }
}
