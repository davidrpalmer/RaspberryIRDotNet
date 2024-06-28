using System;

namespace RaspberryIRDotNet
{
    /// <summary>
    /// Built from the LIRC header file found on the Raspberry Pi at /usr/include/linux/lirc.h
    /// </summary>
    internal static class LircConstants
    {
        public const uint PULSE_BIT = 0x01000000;
        public const uint PULSE_MASK = 0x00ffffff;

        /// Not using these in favour of <see cref="Mode2PacketType"/>
        /// In that enum I've changed these from the original values so they can be compared as a single byte rather than as part of a uint.
        //public const uint LIRC_MODE2_SPACE = 0x00000000;
        //public const uint LIRC_MODE2_PULSE = 0x01000000;
        //public const uint LIRC_MODE2_FREQUENCY = 0x02000000;
        //public const uint LIRC_MODE2_TIMEOUT = 0x03000000;
        //public const uint LIRC_MODE2_OVERFLOW = 0x04000000;


        public const uint LIRC_VALUE_MASK = 0x00ffffff;
        public const uint LIRC_MODE2_MASK = 0xff000000;

        //lirc compatible hardware features
        public const uint LIRC_MODE_RAW = 0x00000001;
        public const uint LIRC_MODE_PULSE = 0x00000002;
        public const uint LIRC_MODE_MODE2 = 0x00000004;
        public const uint LIRC_MODE_SCANCODE = 0x00000008;
        public const uint LIRC_MODE_LIRCCODE = 0x00000010;


        public const uint LIRC_CAN_SEND_RAW = 0x00000001;
        public const uint LIRC_CAN_SEND_PULSE = 0x00000002;
        public const uint LIRC_CAN_SEND_MODE2 = 0x00000004;
        public const uint LIRC_CAN_SEND_LIRCCODE = 0x00000010;
        public const uint LIRC_CAN_SEND_MASK = 0x0000003f;
        public const uint LIRC_CAN_SET_SEND_CARRIER = 0x00000100;
        public const uint LIRC_CAN_SET_SEND_DUTY_CYCLE = 0x00000200;
        public const uint LIRC_CAN_SET_TRANSMITTER_MASK = 0x00000400;
        public const uint LIRC_CAN_REC_RAW = 0x00010000;
        public const uint LIRC_CAN_REC_PULSE = 0x00020000;
        public const uint LIRC_CAN_REC_MODE2 = 0x00040000;
        public const uint LIRC_CAN_REC_SCANCODE = 0x00080000;
        public const uint LIRC_CAN_REC_LIRCCODE = 0x00100000;
        public const uint LIRC_CAN_REC_MASK = 0x003f0000;
        public const uint LIRC_CAN_SET_REC_CARRIER = 0x01000000;
        public const uint LIRC_CAN_SET_REC_CARRIER_RANGE = 0x80000000;
        public const uint LIRC_CAN_GET_REC_RESOLUTION = 0x20000000;
        public const uint LIRC_CAN_SET_REC_TIMEOUT = 0x10000000;
        public const uint LIRC_CAN_MEASURE_CARRIER = 0x02000000;
        public const uint LIRC_CAN_USE_WIDEBAND_RECEIVER = 0x04000000;

        // IO CTL requests.
        public const uint LIRC_GET_FEATURES = 0x80046900;
        public const uint LIRC_GET_SEND_MODE = 0x80046901;
        public const uint LIRC_GET_REC_MODE = 0x80046902;
        public const uint LIRC_GET_REC_RESOLUTION = 0x80046907; // Seems to not work on the Raspberry Pi
        public const uint LIRC_GET_MIN_TIMEOUT = 0x80046908;
        public const uint LIRC_GET_MAX_TIMEOUT = 0x80046909;
        public const uint LIRC_GET_LENGTH = 0x8004690f; // code length in bits, currently only for LIRC_MODE_LIRCCODE
        public const uint LIRC_SET_SEND_MODE = 0x40046911;
        public const uint LIRC_SET_REC_MODE = 0x40046912;
        public const uint LIRC_SET_SEND_CARRIER = 0x40046913;
        public const uint LIRC_SET_REC_CARRIER = 0x40046914; // Seems to not work on the Raspberry Pi
        public const uint LIRC_SET_SEND_DUTY_CYCLE = 0x40046915;
        public const uint LIRC_SET_TRANSMITTER_MASK = 0x40046917; // Seems to not work on the Raspberry Pi
        /// <summary>
        /// When a timeout != 0 is set the driver will send a LIRC_MODE2_TIMEOUT data packet, otherwise LIRC_MODE2_TIMEOUT is never sent.
        /// The original header says "timeout is disabled by default", but it seems to be enabled by default on the RasPi.
        /// </summary>
        public const uint LIRC_SET_REC_TIMEOUT = 0x40046918;
        /// <summary>
        /// 1 enables, 0 disables timeout reports in MODE2
        /// </summary>
        public const uint LIRC_SET_REC_TIMEOUT_REPORTS = 0x40046919; // Seems to not work on the Raspberry Pi - The command succeeds, but has no effect.
        /// <summary>
        /// If enabled (set to 1) from the next key press on the driver will send LIRC_MODE2_FREQUENCY packets
        /// </summary>
        public const uint LIRC_SET_MEASURE_CARRIER_MODE = 0x4004691d; // Seems to not work on the Raspberry Pi
        /// <summary>
        /// To set a range use LIRC_SET_REC_CARRIER_RANGE with the lower bound first and later LIRC_SET_REC_CARRIER with the upper bound.
        /// </summary>
        public const uint LIRC_SET_REC_CARRIER_RANGE = 0x4004691f; // Seems to not work on the Raspberry Pi
        public const uint LIRC_SET_WIDEBAND_RECEIVER = 0x40046923; // Seems to not work on the Raspberry Pi
        /// <summary>
        /// Return the recording timeout, which is either set by the ioctl LIRC_SET_REC_TIMEOUT or by the kernel after setting the protocols.
        /// </summary>
        public const uint LIRC_GET_REC_TIMEOUT = 0x80046924;


        public const uint LIRC_SCANCODE_FLAG_TOGGLE = 0x00000001;
        public const uint LIRC_SCANCODE_FLAG_REPEAT = 0x00000002;


        public static bool LIRC_CAN_SEND(uint features) => (features & LIRC_CAN_SEND_MASK) != 0;
        public static bool LIRC_CAN_REC(uint features) => (features & LIRC_CAN_REC_MASK) != 0;


    }
}
