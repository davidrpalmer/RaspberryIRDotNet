using System;
using System.Collections.Generic;
using RaspberryIRDotNet.RX.Filters;

namespace RaspberryIRDotNet.PacketFormats
{
    /// <summary>
    /// Try to guess the format of an IR packet.
    /// </summary>
    /// <remarks>
    /// IR formats are generally intended to be received by something that is expecting a specific format, so trying to
    /// determine the format without any clue as to what to expect is a little tricky in some cases. For example, NEC
    /// packets can be detected fairly easily by the lead in, however knowing if it is a basic or extended NEC message
    /// is not possible. You could guess that it is extended if the 2nd address byte is not an inverse of the first,
    /// but it could also just be that the message was corrupted.
    /// </remarks>
    public class FormatGuesser
    {
        public List<Format> Formats { get; set; }

        public FormatGuesser()
        {
            Formats =
            [
                new Format()
                {
                     Filter = new NEC.NecRxFilter(),
                     Converter = new NEC.NecBinaryConverter(),
                     UnitDurationMicrosecs = NEC.NecBinaryConverter.NecStandardUnitDurationMicrosecs
                },
                new Format()
                {
                     Filter = new Kasiekyo.PanasonicKasiekyoRxFilter(),
                     Converter = new Kasiekyo.KasiekyoBinaryConverter(),
                     UnitDurationMicrosecs = Kasiekyo.KasiekyoBinaryConverter.KasiekyoStandardUnitDurationMicrosecs
                },
                new Format()
                {
                     Filter = new RC5.RC5RxFilter(),
                     Converter = new RC5.RC5Converter(),
                     UnitDurationMicrosecs = RC5.RC5Converter.RC5StandardUnitDurationMicrosecs
                },
                new Format()
                {
                     Filter = new RC6.RC6RxFilter(),
                     Converter = new RC6.RC6ModeReader(),
                     UnitDurationMicrosecs = RC6.RC6Converter.RC6StandardUnitDurationMicrosecs
                },
            ];

            foreach(var format in Formats)
            {
                format.Filter.AssertConfigOK();
            }
        }

        public Format GuessFormat(IReadOnlyPulseSpaceDurationList irDurations)
        {
            foreach(var format in Formats)
            {
                if (format.Filter.Check(irDurations))
                {
                    return format;
                }
            }

            return null;
        }

        public class Format
        {
            public IRXFilter Filter { get; set; }

            public object Converter { get; set; }

            public int UnitDurationMicrosecs { get; set; }
        }
    }
}
