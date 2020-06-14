using System;

namespace RaspberryIRDotNet.TX
{
    public interface IIRSender
    {
        void Send(IRPulseMessage message);

        void Send(IReadOnlyPulseSpaceDurationList buffer);
    }
}
