using System;
using RaspberryIRDotNet.Exceptions;

namespace RaspberryIRDotNet.RX.PulseSpaceSource
{
    /// <summary>
    /// Collect LIRC packets into PULSE/SPACE bursts.
    /// </summary>
    public class PulseSpaceCaptureLirc : RawLircPacketCapture, IPulseSpaceSource
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

        public bool RealTime => true;

        private readonly PulseSpaceDurationList _pulseSpaceBuffer = new(100);

        public event EventHandler<ReceivedPulseSpaceBurstEventArgs> ReceivedPulseSpaceBurst;

        /// <param name="captureDevicePath">The IR capture device, example '/dev/lirc0'. If this isn't known yet then this can be left at null for now, but the property must be set before capture starts.</param>
        public PulseSpaceCaptureLirc(string captureDevicePath) : this(captureDevicePath, new FileSystem.RealFileSystem())
        {
        }
        /// <summary>
        /// Internal (rather than private) for unit tests.
        /// </summary>
        internal PulseSpaceCaptureLirc(string captureDevicePath, FileSystem.IFileSystem fileSystem) : base(fileSystem)
        {
            CaptureDevice = captureDevicePath;
        }

        protected sealed override bool OnReceivePacket(Mode2PacketType packetType, Lazy<int> packetData)
        {
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
                        var eventArgs = new ReceivedPulseSpaceBurstEventArgs(_pulseSpaceBuffer);
                        ReceivedPulseSpaceBurst?.Invoke(this, eventArgs);
                        if (eventArgs.StopCapture)
                        {
                            return false;
                        }
                        _pulseSpaceBuffer.Clear();
                    }
                    break;
                default:
                    if (ThrowOnUnknownPacket)
                    {
                        throw new UnknownLircPacketTypeException(packetType);
                    }
                    break;
            }

            return true; // Keep on capturing packets.
        }

        protected override void OnBeforeRawCapture()
        {
            _pulseSpaceBuffer.Clear();
            base.OnBeforeRawCapture();
        }
    }
}
