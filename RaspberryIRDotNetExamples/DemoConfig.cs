using System;

namespace RaspberryIRDotNetExamples
{
    static class DemoConfig
    {
        /// <summary>
        /// Set the path to the TX device on your Pi. Set blank if you don't have one, or null to auto detect. Example: /dev/lirc0
        /// </summary>
        private static string TxDevice = null;

        /// <summary>
        /// Set the path to the RX device on your Pi. Set blank if you don't have one, or null to auto detect. Example: /dev/lirc0
        /// </summary>
        private static string RxDevice = null;

        public static string GetTxDevice()
        {
            if (TxDevice == null)
            {
                TxDevice = new RaspberryIRDotNet.DeviceAssessment.DeviceAssessor().GetPathToTheTransmitterDevice();
            }

            return TxDevice;
        }

        public static string GetRxDevice()
        {
            if (RxDevice == null)
            {
                RxDevice = new RaspberryIRDotNet.DeviceAssessment.DeviceAssessor().GetPathToTheReceiverDevice();
            }

            return RxDevice;
        }
    }
}
