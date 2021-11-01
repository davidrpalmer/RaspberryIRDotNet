using System;
using System.Linq;
using RaspberryIRDotNet.RX.Filters;

namespace RaspberryIRDotNetExamples.Demos
{
    class LearnAKey_Format : LearnAKey_KeyLearner
    {
        private readonly RaspberryIRDotNet.PacketFormats.FormatGuesser.Format _formatInfo;

        public LearnAKey_Format(RaspberryIRDotNet.PacketFormats.FormatGuesser.Format formatInfo)
        {
            _formatInfo = formatInfo ?? throw new ArgumentNullException(nameof(formatInfo));
        }

        public override (RaspberryIRDotNet.IRPulseMessage key1, RaspberryIRDotNet.IRPulseMessage key2) LearnKeys(RaspberryIRDotNet.RX.PulseSpaceSource.PreRecordedSource capturedIR)
        {
            (RaspberryIRDotNet.IRPulseMessage key1, RaspberryIRDotNet.IRPulseMessage key2) keysFromLeaner;


            if (_formatInfo.Converter is RaspberryIRDotNet.PacketFormats.NEC.NecBinaryConverter necConverter)
            {
                Console.WriteLine("NEC format detected.");

                bool couldBeBasic = capturedIR.Bursts
                    .Filter(_formatInfo.Filter)
                    .Select(burst => necConverter.ToBasicPacket(new RaspberryIRDotNet.IRPulseMessage(burst, _formatInfo.UnitDurationMicrosecs).PulseSpaceUnits))
                    .All(x => x.Validate());

                if (couldBeBasic)
                {
                    Console.WriteLine("This looks like it is probably the basic NEC format.");
                }
                else
                {
                    bool couldBeExtended = capturedIR.Bursts
                        .Filter(_formatInfo.Filter)
                        .Select(burst => necConverter.ToExtendedPacket(new RaspberryIRDotNet.IRPulseMessage(burst,_formatInfo.UnitDurationMicrosecs).PulseSpaceUnits))
                        .All(x => x.Validate());

                    if (couldBeExtended)
                    {
                        Console.WriteLine("This looks like it is probably the extended NEC format.");
                    }
                }

                Console.WriteLine("Please confirm, is this the basic or extended NEC format?");
                bool basicFormat = AskBasicOrExtended();

                if (basicFormat)
                {
                    keysFromLeaner = LearnKeysAsFormattedMessages<RaspberryIRDotNet.PacketFormats.NEC.NecBasicPacket>(necConverter);
                }
                else
                {
                    keysFromLeaner = LearnKeysAsFormattedMessages<RaspberryIRDotNet.PacketFormats.NEC.NecExtendedPacket>(necConverter);
                }
            }
            else if (_formatInfo.Converter is RaspberryIRDotNet.PacketFormats.Kasiekyo.KasiekyoBinaryConverter kasiekyoConverter)
            {
                //Panasonic is the only variant of Kasiekyo we currently understand.
                Console.WriteLine("Panasonic Kasiekyo format detected.");
                keysFromLeaner = LearnKeysAsFormattedMessages<RaspberryIRDotNet.PacketFormats.Kasiekyo.PanasonicPacket>(kasiekyoConverter);
            }
            else if (_formatInfo.Converter is RaspberryIRDotNet.PacketFormats.RC5.RC5Converter rc5Converter)
            {
                Console.WriteLine("RC5 format detected. However cannot automatically detect if it is basic or extended.");
                Console.WriteLine("Basic mode can have up to 64 command, extended mode can have up to 128.");
                Console.WriteLine("If you don't know which to choose then go for extended. Only choose basic if you are sure.");

                Console.WriteLine("Please confirm, is this the basic or extended RC5 format?");
                bool basicFormat = AskBasicOrExtended();

                if (basicFormat)
                {
                    keysFromLeaner = LearnKeysAsFormattedMessages<RaspberryIRDotNet.PacketFormats.RC5.RC5BasicPacket>(rc5Converter);
                }
                else
                {
                    keysFromLeaner = LearnKeysAsFormattedMessages<RaspberryIRDotNet.PacketFormats.RC5.RC5ExtendedPacket>(rc5Converter);
                }
            }
            else if (_formatInfo.Converter is RaspberryIRDotNet.PacketFormats.RC6.RC6ModeReader rc6ModeReader)
            {
                var firstBurst = capturedIR.Bursts.Filter(_formatInfo.Filter).First();
                var header = rc6ModeReader.ReadRC6HeaderGetConverter(new RaspberryIRDotNet.PulseSpaceUnitList(_formatInfo.UnitDurationMicrosecs, firstBurst));
                if (header.converter is RaspberryIRDotNet.PacketFormats.RC6.Mode0.RC6Mode0Converter mode0Converter)
                {
                    Console.WriteLine("RC6 Mode0 format detected.");
                    keysFromLeaner = LearnKeysAsFormattedMessages<RaspberryIRDotNet.PacketFormats.RC6.Mode0.RC6Mode0Packet>(mode0Converter);
                }
                else if (header.converter is RaspberryIRDotNet.PacketFormats.RC6.Mode6A.RC6Mode6ARawPayloadConverter mode6ARawConverter)
                {
                    var samplePacket = mode6ARawConverter.ToPacket(header.bits);
                    if (samplePacket is RaspberryIRDotNet.PacketFormats.RC6.Mode6A.RC6Mode6A8RawBitsPacket)
                    {
                        Console.WriteLine("RC6 Mode6A format with 8 bit address detected.");
                        keysFromLeaner = LearnKeysAsFormattedMessages<RaspberryIRDotNet.PacketFormats.RC6.Mode6A.RC6Mode6A8RawBitsPacket>(mode6ARawConverter);
                    }
                    else if (samplePacket is RaspberryIRDotNet.PacketFormats.RC6.Mode6A.RC6Mode6A16RawBitsPacket)
                    {
                        Console.WriteLine("RC6 Mode6A format with 16 bit address detected.");
                        keysFromLeaner = LearnKeysAsFormattedMessages<RaspberryIRDotNet.PacketFormats.RC6.Mode6A.RC6Mode6A16RawBitsPacket>(mode6ARawConverter);
                    }
                    else
                    {
                        throw new Exception("Unhandled RC6A format.");
                    }
                }
                else if (header.converter is RaspberryIRDotNet.PacketFormats.RC6.RC6RawBitsConverter rawConverter)
                {
                    Console.WriteLine("RC6 (mode unknown) format detected.");
                    keysFromLeaner = LearnKeysAsFormattedMessages<RaspberryIRDotNet.PacketFormats.RC6.RC6RawBitsPacket>(rawConverter);
                }
                else
                {
                    throw new Exception("Unhandled RC6 format.");
                }
            }
            else
            {
                throw new Exception("Unhandled format.");
            }

            return (keysFromLeaner.key1, keysFromLeaner.key2);
        }

        private (RaspberryIRDotNet.IRPulseMessage key1, RaspberryIRDotNet.IRPulseMessage key2) LearnKeysAsFormattedMessages<TPacket>(RaspberryIRDotNet.PacketFormats.IPulseSpacePacketConverter<TPacket> converter) where TPacket : class, RaspberryIRDotNet.PacketFormats.IIRFormatPacket
        {
            Breather();
            Console.WriteLine("Now ready to learn individual buttons.");

            var learner = new RaspberryIRDotNet.RX.FormattedIRLearn<TPacket>(DemoConfig.GetRxDevice())
            {
                PacketConverter = converter,
                UnitDurationMicrosecs = _formatInfo.UnitDurationMicrosecs
            };
            learner.RXFilters.Filters.Add(_formatInfo.Filter);
            SetUpRxFeedback(learner);

            var key1 = LearnKey("1", learner);
            var key2 = LearnKey("2", learner);

            var key1IR = new RaspberryIRDotNet.IRPulseMessage(converter.ToIR((TPacket)key1), _formatInfo.UnitDurationMicrosecs);
            var key2IR = new RaspberryIRDotNet.IRPulseMessage(converter.ToIR((TPacket)key2), _formatInfo.UnitDurationMicrosecs);

            return (key1IR, key2IR);
        }


        private RaspberryIRDotNet.PacketFormats.IIRFormatPacket LearnKey(string keyName, RaspberryIRDotNet.RX.FormattedIRLearn learner)
        {
            WriteKeyPressInstructions($"the '{keyName}' key", false);

            var result = learner.LearnMessageGeneric();

            Console.WriteLine($"{keyName} key captured: {result}");
            Breather();
            return result;
        }

        /// <summary>
        /// Wait for a key press and return TRUE if it is B or FALSE if it is E.
        /// This method does print any question text (do that before). This just indicates which keys to press and waits for input.
        /// </summary>
        private bool AskBasicOrExtended()
        {
            Console.Write("[B]asic / [E]xtended: ");
            while (true)
            {
                var key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.B:
                        Console.WriteLine("B");
                        return true;
                    case ConsoleKey.E:
                        Console.WriteLine("E");
                        return false;
                }
            }
        }
    }
}
