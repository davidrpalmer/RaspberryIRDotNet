using System;

namespace RaspberryIRDotNet.PacketFormats.BinaryConverters
{
    /// <summary>
    /// Handles the conversion from IR PULSE/SPACE data into binary bits.
    /// </summary>
    /// <remarks>
    /// For use with the <see cref="PulseSpacePairBinaryConverter"/> class.
    /// </remarks>
    public class PulseSpacePairBitConverter
    {
        /// <summary>
        /// Shortcut method to create a common format where the PULSE is always one and the SPACE is either 1 or 3.
        /// </summary>
        public static PulseSpacePairBitConverter CreateOneAndThree()
        {
            return new PulseSpacePairBitConverter()
            {
                Zero_PulseUnitCount = 1,
                Zero_SpaceUnitCount = 1,
                One_PulseUnitCount = 1,
                One_SpaceUnitCount = 3
            };
        }

        public byte Zero_PulseUnitCount { get; set; }
        public byte Zero_SpaceUnitCount { get; set; }
        public byte One_PulseUnitCount { get; set; }
        public byte One_SpaceUnitCount { get; set; }

        public bool PulseSpaceToBit(byte pulseUnitCount, byte spaceUnitCount)
        {
            if (Zero_PulseUnitCount == One_PulseUnitCount)
            {
                if (Zero_SpaceUnitCount == One_SpaceUnitCount)
                {
                    throw new InvalidOperationException("All PULSE/SPACE unit counts are the same. This object has probably not been initialised.");
                }

                if (pulseUnitCount != Zero_PulseUnitCount)
                {
                    throw new Exceptions.InvalidPacketDataException($"There is a PULSE with an unexpected value. Expected={Zero_PulseUnitCount}, Actual={pulseUnitCount}");
                }

                return SpaceToBit(spaceUnitCount);
            }
            else if (Zero_SpaceUnitCount == One_SpaceUnitCount)
            {
                if (spaceUnitCount != Zero_SpaceUnitCount)
                {
                    throw new Exceptions.InvalidPacketDataException($"There is a SPACE with an unexpected value. Expected={Zero_SpaceUnitCount}, Actual={spaceUnitCount}");
                }

                return PulseToBit(pulseUnitCount);
            }
            else
            {
                bool pulseIsOne = PulseToBit(pulseUnitCount);
                bool spaceIsOne = SpaceToBit(spaceUnitCount);

                if (pulseIsOne == spaceIsOne)
                {
                    return spaceIsOne;
                }
                else
                {
                    throw new Exceptions.InvalidPacketDataException($"The PULSE ({pulseUnitCount}) and SPACE ({spaceUnitCount}) do not match to either a 0 or a 1.");
                }
            }
        }

        private bool PulseToBit(byte pulseUnitCount)
        {
            if (pulseUnitCount == Zero_PulseUnitCount)
            {
                return false;
            }
            else if (pulseUnitCount == One_PulseUnitCount)
            {
                return true;
            }
            else
            {
                throw new Exceptions.InvalidPacketDataException($"There is a PULSE with an unexpected value. Expected=[{Zero_PulseUnitCount} or {One_PulseUnitCount}], Actual={pulseUnitCount}");
            }
        }

        private bool SpaceToBit(byte spaceUnitCount)
        {
            if (spaceUnitCount == Zero_SpaceUnitCount)
            {
                return false;
            }
            else if (spaceUnitCount == One_SpaceUnitCount)
            {
                return true;
            }
            else
            {
                throw new Exceptions.InvalidPacketDataException($"There is a SPACE with an unexpected value. Expected=[{Zero_SpaceUnitCount} or {One_SpaceUnitCount}], Actual={spaceUnitCount}");
            }
        }


        /// <returns>
        /// An array with a length of 2 to represent a PULSE and a SPACE.
        /// </returns>
        public byte[] BitToPulseSpace(bool bit)
        {
            if (!bit) // 0
            {
                return [Zero_PulseUnitCount, Zero_SpaceUnitCount];
            }
            else // 1
            {
                return [One_PulseUnitCount, One_SpaceUnitCount];
            }
        }
    }
}
