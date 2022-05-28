using System;
using RaspberryIRDotNet.Exceptions;

namespace RaspberryIRDotNet.TX
{
    public abstract class PulseSpaceTransmitter
    {
        private FileSystem.IFileSystem _fileSystem;

        /// <summary>
        /// The IR transmission device, example '/dev/lirc0'.
        /// </summary>
        public string TransmissionDevice { get; set; }

        /// <summary>
        /// Set the IR driver duty cycle (1-99). Or set to -1 to not set the value (leave the driver in what ever state it was already in). The default (what the driver defaults to not what this property defaults to) is stored in the constant <see cref="IRDeviceDefaults.DutyCycle"/>.
        /// </summary>
        public int DutyCycle { get; set; } = -1;

        /// <summary>
        /// Set the IR driver frequency. Or set to -1 to not set the value (leave the driver in what ever state it was already in). The default (what the driver defaults to not what this property defaults to) is stored in the constant <see cref="IRDeviceDefaults.Frequency"/>.
        /// </summary>
        public int Frequency { get; set; } = -1;

        protected PulseSpaceTransmitter()
        {
            SetFileSystem(new FileSystem.RealFileSystem());
        }

        /// <summary>
        /// Can be used to abstract away the file system for unit testing.
        /// </summary>
        internal void SetFileSystem(FileSystem.IFileSystem newFS)
        {
            _fileSystem = newFS;
        }

        protected void WriteToDevice(FileSystem.IOpenFile file, IReadOnlyPulseSpaceDurationList buffer)
        {
            byte[] txBytesBuffer = GetTxBytes(buffer);
            try
            {
                file.WriteToDevice(txBytesBuffer);
            }
            catch (System.ComponentModel.Win32Exception err) when (err.NativeErrorCode == LinuxErrorCodes.EINVAL)
            {
                if (buffer.TotalDuration() >= Utility.MessageDurationMaximum)
                {
                    throw new InvalidPulseSpaceDataException("The IR device rejected the IR data, probably because the data duration was too long.", err);
                }

                throw new InvalidPulseSpaceDataException("The IR device rejected the IR data.", err);
            }
        }

        /// <summary>
        /// Open the IR device. Dispose the object this returns to close it.
        /// </summary>
        protected FileSystem.IOpenFile OpenDevice()
        {
            if (string.IsNullOrWhiteSpace(TransmissionDevice))
            {
                throw new ArgumentNullException(nameof(TransmissionDevice), $"The transmission device must be set.");
            }

            var irDevice = _fileSystem.OpenWrite(TransmissionDevice);
            try
            {
                DeviceFeatures deviceFeatures = Utility.GetFeatures(irDevice);
                CheckDevice(irDevice, deviceFeatures);
                ApplySettings(irDevice);
            }
            catch
            {
                irDevice.Dispose();
                throw;
            }

            return irDevice;
        }

        /// <summary>
        /// Get bytes that must be sent to an IR device in order to transmit a buffer.
        /// </summary>
        private byte[] GetTxBytes(IReadOnlyPulseSpaceDurationList buffer)
        {
            const int bytesIn32BitInt = 4;

            if (buffer.Count % 2 == 0)
            {
                throw new InvalidPulseSpaceDataException("The buffer starts with a PULSE and must end with a PULSE, so there must be an odd number of items in the buffer. But there is an even number, so it ends with a SPACE.");
            }

            byte[] result = new byte[buffer.Count * bytesIn32BitInt];

            int i = 0;
            foreach (int packet in buffer)
            {
                if (packet < Utility.UnitDurationMinimum)
                {
                    throw new InvalidPulseSpaceDataException("TX buffer contains invalid data, duration is too short.");
                }
                if (packet > Utility.UnitDurationMaximum)
                {
                    throw new InvalidPulseSpaceDataException("TX buffer contains invalid data, duration is too long.");
                }

                Array.Copy(BitConverter.GetBytes(packet), 0, result, i, bytesIn32BitInt);

                i += bytesIn32BitInt;
            }

            return result;
        }

        /// <summary>
        /// When the device is opened this is called to check that the device is OK (e.g. has the right capabilities, etc).
        /// </summary>
        protected virtual void CheckDevice(FileSystem.IOpenFile irDevice, DeviceFeatures deviceFeatures)
        {
            if (!deviceFeatures.CanSend())
            {
                throw new NotSupportedException("This IR device cannot send.");
            }

            if (!deviceFeatures.HasFlag(DeviceFeatures.SendModePulse))
            {
                throw new NotSupportedException("Only PULSE mode is supported for sending, but this device does not support PULSE.");
            }

            uint sendMode = irDevice.IoCtlReadUInt32(LircConstants.LIRC_GET_SEND_MODE);
            if (sendMode != LircConstants.LIRC_MODE_PULSE)
            {
                // The Raspberry Pi only supports PULSE mode and so it is the default mode, but incase that changes got the option to set the mode.
                try
                {
                    irDevice.IoCtlWrite(LircConstants.LIRC_SET_SEND_MODE, LircConstants.LIRC_MODE_PULSE);
                }
                catch (System.ComponentModel.Win32Exception err)
                {
                    throw new NotSupportedException("Unable to set send mode to PULSE.", err);
                }
            }
        }

        /// <summary>
        /// When the device is opened this is called to apply the configuration. Child classes may also call this while the device is open to re-apply the settings.
        /// </summary>
        protected virtual void ApplySettings(FileSystem.IOpenFile irDevice)
        {
            if (DutyCycle >= 0)
            {
                SetDutyCycle(irDevice, DutyCycle);
            }
            if (Frequency >= 0)
            {
                SetFrequency(irDevice, Frequency);
            }
        }

        protected void SetDutyCycle(FileSystem.IOpenFile irDevice, int cycle)
        {
            if (cycle < 0) { throw new ArgumentOutOfRangeException(); }
            SetDutyCycle(irDevice, checked((uint)cycle));
        }

        protected void SetDutyCycle(FileSystem.IOpenFile irDevice, uint cycle)
        {
            try
            {
                irDevice.IoCtlWrite(LircConstants.LIRC_SET_SEND_DUTY_CYCLE, cycle);
            }
            catch (System.ComponentModel.Win32Exception err)
            {
                throw new DeviceOptionSetException(DeviceOptionSetException.Option.DutyCycle, err);
            }
        }

        protected void SetFrequency(FileSystem.IOpenFile irDevice, int frequency)
        {
            if (frequency < 0) { throw new ArgumentOutOfRangeException(); }
            SetFrequency(irDevice, checked((uint)frequency));
        }
        protected void SetFrequency(FileSystem.IOpenFile irDevice, uint frequency)
        {
            try
            {
                irDevice.IoCtlWrite(LircConstants.LIRC_SET_SEND_CARRIER, frequency);
            }
            catch (System.ComponentModel.Win32Exception err)
            {
                throw new DeviceOptionSetException(DeviceOptionSetException.Option.CarrierFrequency, err);
            }
        }
    }
}
