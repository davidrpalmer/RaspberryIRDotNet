using System;
using RaspberryIRDotNet.PacketFormats;
using RaspberryIRDotNet.RX.PulseSpaceSource;

namespace RaspberryIRDotNet.RX
{
    /// <summary>
    /// Capture IR data and format it into a packet, then log it.
    /// </summary>
    public class PacketConsoleWriter<TPacket> : CleanedUpIRCapture, IIRConsoleWriter where TPacket : class, IIRFormatPacket
    {
        /// <summary>
        /// Option to override where the output is written to.
        /// </summary>
        public Action<string> ConsoleWriteLine { get; set; } = Console.WriteLine;

        /// <summary>
        /// Option to override how packets are converted into strings.
        /// </summary>
        public Func<TPacket, string> PacketToString { get; set; } = (p) => p.ToString();

        private readonly IPulseSpacePacketConverter<TPacket> _packetConverter;

        /// <param name="filter">The filter for this packet format.</param>
        /// <param name="captureDevicePath">The IR capture device, example '/dev/lirc0'.</param>
        public PacketConsoleWriter(IPulseSpacePacketConverter<TPacket> packetConverter, Filters.IRXFilter filter, string captureDevicePath) : this(packetConverter, captureDevicePath)
        {
            RXFilters.Filters.Add(filter);
        }
        /// <param name="captureDevicePath">The IR capture device, example '/dev/lirc0'.</param>
        public PacketConsoleWriter(IPulseSpacePacketConverter<TPacket> packetConverter, string captureDevicePath) : base(captureDevicePath)
        {
            _packetConverter = packetConverter ?? throw new ArgumentNullException(nameof(packetConverter));
        }
        public PacketConsoleWriter(IPulseSpacePacketConverter<TPacket> packetConverter, IPulseSpaceSource source) : base(source)
        {
            _packetConverter = packetConverter ?? throw new ArgumentNullException(nameof(packetConverter));
        }

        protected override void OnReceiveIRPulseMessage(ReceivedPulseSpaceBurstEventArgs rawData, IRPulseMessage message)
        {
            if (CheckMessage(message))
            {
                TPacket packet;
                try
                {
                    packet = _packetConverter.ToPacket(message);
                }
                catch (Exception err) when (err is Exceptions.InvalidPulseSpaceDataException || err is Exceptions.InvalidPacketDataException)
                {
                    ConsoleWriteLine("Error: " + err.Message);
                    return;
                }

                ConsoleWriteLine(PacketToString(packet));
            }
        }

        /// <summary>
        /// Start logging to the console.
        /// </summary>
        public void Start(ReadCancellationToken cancellationToken)
        {
            Capture(cancellationToken);
        }
    }
}
