using System;
using System.Collections.Generic;

namespace RaspberryIRDotNet.PacketFormats.RC6
{
    public abstract class RC6Packet : IIRFormatPacket
    {
        public RC6Mode Mode { get; set; }

        /// <summary>
        /// The double width bit that follows the mode. The meaning of this bit varies between modes.
        /// </summary>
        public bool Trailer { get; set; }

        public abstract bool Equals(IIRFormatPacket other, bool ignoreVariables);

        public override string ToString()
        {
            return "Mode:" + Mode.ToString();
        }

        protected static bool AreArraysEqual<T>(IList<T> a, IList<T> b)
        {
            if (a.Count != b.Count)
            {
                return false;
            }

            for (int i = 0; i < a.Count; i++)
            {
                if (!a[i].Equals(b[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
