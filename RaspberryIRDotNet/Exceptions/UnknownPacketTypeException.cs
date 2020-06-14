using System;

namespace RaspberryIRDotNet.Exceptions
{
    public class UnknownPacketTypeException : Exception
    {
        /// <summary>
        /// The packet type identifier that was not recognised.
        /// </summary>
        public byte Mode2PacketType { get; }

        public UnknownPacketTypeException(Mode2PacketType type) : base($"Unknown packet type {type}.")
        {
            Mode2PacketType = (byte)type;
        }
    }
}
