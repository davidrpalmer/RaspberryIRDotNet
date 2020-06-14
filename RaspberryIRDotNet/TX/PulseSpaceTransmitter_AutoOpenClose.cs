using System;

namespace RaspberryIRDotNet.TX
{
    /// <summary>
    /// Allows sending IR data. The IR device is opened and closed for each transmission.
    /// </summary>
    public class PulseSpaceTransmitter_AutoOpenClose : PulseSpaceTransmitter, IIRSender
    {
        public void Send(IRPulseMessage message)
        {
            Send(message.PulseSpaceDurations);
        }

        public void Send(IReadOnlyPulseSpaceDurationList buffer)
        {
            using (var irDevice = OpenDevice())
            {
                WriteToDevice(irDevice, buffer);
            }
        }
    }
}
