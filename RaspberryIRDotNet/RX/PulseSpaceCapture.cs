using System;
using RaspberryIRDotNet.Exceptions;

namespace RaspberryIRDotNet.RX
{
    /// <summary>
    /// Capture raw PULSE/SPACE data until a timeout is hit, then notify via <see cref="OnReceivePulseSpaceBlock"/>.
    /// </summary>
    public abstract class PulseSpaceCapture : RawLircPacketCapture
    {
        /// <summary>
        /// True to throw an exception if an unknown packet is received from the IR device. False to just ignore it.
        /// </summary>
        public bool ThrowOnUnknownPacket { get; set; }
#if DEBUG
        = true;
#else
        = false;
#endif

        private readonly PulseSpaceDurationList _pulseSpaceBuffer = new PulseSpaceDurationList(100);

        private volatile bool _cancel = false;

        /// <summary>
        /// Signal that the receiving should stop as soon as possible. Due to the blocking read syscall this will only actually cancel
        /// the next time data is read from the file. So if no IR is received at all then the cancel will never happen!
        /// 
        /// If the cancel does work then a <see cref="OperationCanceledException"/> will be thrown on the receiving thread, unless it
        /// just so happened to end naturally anyway.
        /// </summary>
        protected void CancelReceiveWhenPossible()
        {
            _cancel = true;
        }

        protected sealed override bool OnReceivePacket(Mode2PacketType packetType, Lazy<int> packetData)
        {
            if (_cancel)
            {
                throw new OperationCanceledException("IR receive cancelled.");
            }

            switch (packetType)
            {
                case Mode2PacketType.Space:
                    if (_pulseSpaceBuffer.Count > 0)
                    {
                        _pulseSpaceBuffer.Add(packetData.Value);
                    }
                    break;
                case Mode2PacketType.Pulse:
                    _pulseSpaceBuffer.Add(packetData.Value);
                    break;
                case Mode2PacketType.Timeout:
                    if (_pulseSpaceBuffer.Count > 0)
                    {
                        if (!OnReceivePulseSpaceBlock(_pulseSpaceBuffer))
                        {
                            return false;
                        }
                        _pulseSpaceBuffer.Clear();
                    }
                    break;
                default:
                    if (ThrowOnUnknownPacket)
                    {
                        throw new UnknownPacketTypeException(packetType);
                    }
                    break;
            }

            return true; // Keep on capturing packets.
        }

        protected override void OnBeforeRawCapture()
        {
            _cancel = false;
            _pulseSpaceBuffer.Clear();
            base.OnBeforeRawCapture();
        }

        /// <summary>
        /// When a block of PULSE/SPACE signals are received this is called. A block is defined as some PULSE/SPACE signals followed by a timeout. So this method is called every time there is a timeout event from the IR device.
        /// </summary>
        /// <param name="buffer">The PULSE/SPACE data received from the IR driver. Note this buffer is not yours to keep! Once this method returns this buffer will be reused by the calling code. So copy the data if you want to keep it. Creating an <see cref="IRPulseMessage"/> is one way to get a copy, or you can use <see cref="PulseSpaceDurationList.Copy"/>.</param>
        /// <returns>
        /// Return TRUE to continue capturing IR or FALSE to stop.
        /// </returns>
        protected abstract bool OnReceivePulseSpaceBlock(PulseSpaceDurationList buffer);
    }
}
