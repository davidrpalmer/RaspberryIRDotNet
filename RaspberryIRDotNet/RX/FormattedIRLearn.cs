using System;
using System.Collections.Generic;
using RaspberryIRDotNet.Exceptions;
using RaspberryIRDotNet.PacketFormats;

namespace RaspberryIRDotNet.RX
{
    public abstract class FormattedIRLearn : CleanedUpSingleIRCapture, IMultipleCapture
    {
        /// <summary>
        /// Ready for the user to press a button.
        /// </summary>
        public event EventHandler Waiting;
        protected void RaiseWaitingEvent() => Waiting?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Captured a clean IR message.
        /// </summary>
        public event EventHandler Hit;
        protected void RaiseHitEvent() => Hit?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// When the IR fails to decode into a packet, even though it looked valid.
        /// </summary>
        public event EventHandler<ExceptionEventArgs> ToPacketException;
        protected void RaiseToPacketExceptionEvent(ExceptionEventArgs e) => ToPacketException?.Invoke(this, e);

        /// <summary>
        /// How many captures must be the same for us to believe they are right.
        /// If this is one then the first capture will be assumed to be correct.
        /// </summary>
        public int MinimumMatchingCaptures { get; set; } = 2;

        /// <summary>
        /// If this many non-matching captures are received then something is wrong, throw an error.
        /// </summary>
        /// <remarks>
        /// Note this does not count noise, only captures that seem valid, but do not match the other captures.
        /// </remarks>
        public int ErrorAfterBadCaptureCount { get; set; } = 12;

        /// <summary>
        /// Wait this long in between captures to allow the user time to let go of the button and get ready to press it again.
        /// </summary>
        public TimeSpan CaptureDelay { get; set; } = TimeSpan.FromSeconds(1);

        /// <param name="captureDevicePath">The IR capture device, example '/dev/lirc0'.</param>
        public FormattedIRLearn(string captureDevicePath) : base(captureDevicePath)
        {
        }
        public FormattedIRLearn(PulseSpaceSource.IPulseSpaceSource source) : base(source)
        {
        }

        public abstract IIRFormatPacket LearnMessageGeneric();
    }

    /// <summary>
    /// Learn a formatted IR packet.
    /// </summary>
    public class FormattedIRLearn<TPacket> : FormattedIRLearn where TPacket : class, IIRFormatPacket
    {
        public IPulseSpacePacketConverter<TPacket> PacketConverter { get; set; }

        private TPacket _packet;

        /// <param name="captureDevicePath">The IR capture device, example '/dev/lirc0'.</param>
        public FormattedIRLearn(string captureDevicePath) : base(captureDevicePath)
        {
        }
        public FormattedIRLearn(PulseSpaceSource.IPulseSpaceSource source) : base(source)
        {
        }

        public override IIRFormatPacket LearnMessageGeneric() => LearnMessage();

        /// <summary>
        /// Learn a formatted IR message and return it.
        /// </summary>
        public TPacket LearnMessage()
        {
            if (PacketConverter == null)
            {
                throw new ArgumentNullException(nameof(PacketConverter));
            }

            List<TPacket> allCaptues = new List<TPacket>();
            while (true)
            {
                RaiseWaitingEvent();
                allCaptues.Add(CaptureSinglePacket());
                RaiseHitEvent();

                var chosenSample = CheckSamples(allCaptues);
                if (chosenSample != null)
                {
                    return chosenSample;
                }
                if (allCaptues.Count >= ErrorAfterBadCaptureCount)
                {
                    throw new InsufficientMatchingSamplesException("Not finding any matches. There is probably too much IR noise. Note the signal has already been cleaned up so any differences here represent a significant variation in the IR signal.");
                }

                if (CaptureDelay > TimeSpan.Zero)
                {
                    System.Threading.Thread.Sleep(CaptureDelay);
                }
            }
        }

        private TPacket CheckSamples(ICollection<TPacket> allCaptues)
        {
            if (allCaptues.Count < MinimumMatchingCaptures)
            {
                return null; // Not got enough samples yet.
            }

            int bestMatchCount = 0;
            TPacket bestMatch = null;

            foreach (var packet1 in allCaptues)
            {
                int matches = 0;

                foreach (var packet2 in allCaptues)
                {
                    if (packet1.Equals(packet2, ignoreVariables: true))
                    {
                        matches++;
                    }
                }

                if (matches > bestMatchCount)
                {
                    bestMatchCount = matches;
                    bestMatch = packet1;
                }
            }

            if (bestMatchCount < MinimumMatchingCaptures)
            {
                // Not enough of the samples are the same.
                return null;
            }

            return bestMatch;
        }

        protected override bool CheckMessage(IRPulseMessage message)
        {
            if (!base.CheckMessage(message))
            {
                return false;
            }

            try
            {
                _packet = PacketConverter.ToPacket(message);
                if (_packet is IValidateableIRPacket validateable && !validateable.Validate())
                {
                    throw new InvalidPacketDataException("The packet failed the format specific validation. This could be a checksum or something similar.");
                }
                return true;
            }
            catch (InvalidPacketDataException err)
            {
                RaiseToPacketExceptionEvent(new ExceptionEventArgs(err));
                return false;
            }
        }

        private TPacket CaptureSinglePacket()
        {
            _packet = null;
            CaptureSingleMessage(null);
            if (_packet == null)
            {
                throw new Exception("Failed to get a packet.");
            }
            return _packet;
        }
    }
}
