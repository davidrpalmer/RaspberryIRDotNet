using System;
using static RaspberryIRDotNet.LircConstants;

namespace RaspberryIRDotNet
{
    [Flags]
    public enum DeviceFeatures : uint
    {
        /// <summary>
        /// The driver is capable of receiving using LIRC_MODE_RAW.
        /// </summary>
        ReceiveModeRaw = LIRC_CAN_REC_RAW,

        /// <summary>
        /// The driver is capable of receiving using LIRC_MODE_PULSE.
        /// </summary>
        ReceiveModePulse = LIRC_CAN_REC_PULSE,

        /// <summary>
        /// The driver is capable of receiving using LIRC_MODE_MODE2.
        /// </summary>
        ReceiveModeMode2 = LIRC_CAN_REC_MODE2,

        /// <summary>
        /// The driver is capable of receiving using LIRC_MODE_LIRCCODE.
        /// </summary>
        ReceiveLircCode = LIRC_CAN_REC_LIRCCODE,

        /// <summary>
        /// The driver supports changing the modulation frequency using LIRC_SET_SEND_CARRIER.
        /// </summary>
        SetSendCarrier = LIRC_CAN_SET_SEND_CARRIER,

        /// <summary>
        /// The driver supports changing the duty cycle using LIRC_SET_SEND_DUTY_CYCLE.
        /// </summary>
        SetSendDutyCycle = LIRC_CAN_SET_SEND_DUTY_CYCLE,

        /// <summary>
        /// The driver supports changing the active transmitter(s) using LIRC_SET_TRANSMITTER_MASK.
        /// </summary>
        SetTransmitterMask = LIRC_CAN_SET_TRANSMITTER_MASK,

        /// <summary>
        /// The driver supports setting the receive carrier frequency using LIRC_SET_REC_CARRIER.
        /// </summary>
        SetReceiveCarrier = LIRC_CAN_SET_REC_CARRIER,

        /// <summary>
        /// The driver supports LIRC_SET_REC_DUTY_CYCLE_RANGE.
        /// </summary>
        SetReceiveDutyCycleRange = LIRC_CAN_SET_REC_DUTY_CYCLE_RANGE,

        /// <summary>
        /// The driver supports LIRC_SET_REC_CARRIER_RANGE.
        /// </summary>
        SetReceiveCarrierRange = LIRC_CAN_SET_REC_CARRIER_RANGE,

        /// <summary>
        /// The driver supports LIRC_GET_REC_RESOLUTION.
        /// </summary>
        GetReceiveResolution = LIRC_CAN_GET_REC_RESOLUTION,

        /// <summary>
        /// The driver supports LIRC_SET_REC_TIMEOUT.
        /// </summary>
        SetReceiveTimeout = LIRC_CAN_SET_REC_TIMEOUT,

        /// <summary>
        /// The driver supports LIRC_SET_REC_FILTER.
        /// </summary>
        SetReceiveFilter = LIRC_CAN_SET_REC_FILTER,

        /// <summary>
        /// The driver supports measuring of the modulation frequency using LIRC_SET_MEASURE_CARRIER_MODE.
        /// </summary>
        MeasureCarrier = LIRC_CAN_MEASURE_CARRIER,

        /// <summary>
        /// The driver supports learning mode using LIRC_SET_WIDEBAND_RECEIVER.
        /// </summary>
        UseWidebandReceiver = LIRC_CAN_USE_WIDEBAND_RECEIVER,

        /// <summary>
        /// The driver supports LIRC_NOTIFY_DECODE.
        /// </summary>
        NotifyDecode = LIRC_CAN_NOTIFY_DECODE,

        /// <summary>
        /// The driver supports sending using LIRC_MODE_RAW.
        /// </summary>
        SendModeRaw = LIRC_CAN_SEND_RAW,

        /// <summary>
        /// The driver supports sending using LIRC_MODE_PULSE.
        /// </summary>
        SendModePulse = LIRC_CAN_SEND_PULSE,

        /// <summary>
        /// The driver supports sending using LIRC_MODE_MODE2.
        /// </summary>
        SendModeMode2 = LIRC_CAN_SEND_MODE2,

        /// <summary>
        /// The driver supports sending. (This is uncommon, since LIRCCODE drivers reflect hardware like TV-cards which usually dos not support sending.)
        /// </summary>
        SendModeLircCode = LIRC_CAN_SEND_LIRCCODE
    }

    public static class DeviceFeaturesExtensions
    {
        public static bool CanSend(this DeviceFeatures f) => LIRC_CAN_SEND((uint)f);

        public static bool CanReceive(this DeviceFeatures f) => LIRC_CAN_REC((uint)f);
    }
}
