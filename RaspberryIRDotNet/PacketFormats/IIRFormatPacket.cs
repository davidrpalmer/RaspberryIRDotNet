using System;

namespace RaspberryIRDotNet.PacketFormats
{
    public interface IIRFormatPacket
    {
        /// <param name="ignoreVariables">TRUE to ignore toggle bits or similar variations.</param>
        bool Equals(IIRFormatPacket other, bool ignoreVariables);
    }
}
