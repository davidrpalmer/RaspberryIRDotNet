using System;

namespace RaspberryIRDotNet.PacketFormats
{
    public interface IPulseSpacePacketConverter<TPacket> where TPacket : class, IIRFormatPacket
    {
        TPacket ToPacket(IReadOnlyPulseSpaceUnitList irData);

        TPacket ToPacket(IRPulseMessage irData) => ToPacket(irData.PulseSpaceUnits);

        PulseSpaceUnitList ToIR(TPacket packet);
    }
}
