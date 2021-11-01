using System;
using System.Collections.Generic;

namespace RaspberryIRDotNet.PacketFormats.RC6
{
    public class RC6ModeReader : RC6Converter
    {
        public (List<bool> bits, RC6Mode mode) ReadRC6Header(IReadOnlyPulseSpaceUnitList irData)
        {
            // This method assumes that all modes have a trailer bit. Don't know if that is really the case or not.

            return ReadStartOfIR(irData, null, null);
        }

        public (List<bool> bits, RC6Mode mode, object converter) ReadRC6HeaderGetConverter(IReadOnlyPulseSpaceUnitList irData)
        {
            var headerData = ReadRC6Header(irData);
            object converter = null;

            switch (headerData.mode)
            {
                case 0:
                    converter = new Mode0.RC6Mode0Converter();
                    break;
                case 6:
                    if (!headerData.bits[0]) // mode 6A
                    {
                        converter = new Mode6A.RC6Mode6ARawPayloadConverter();
                    }
                    break;
            }

            if (converter == null)
            {
                converter = new RC6RawBitsConverter();
            }

            return (headerData.bits, headerData.mode, converter);
        }
    }
}
