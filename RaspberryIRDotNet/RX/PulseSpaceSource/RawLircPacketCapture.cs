using System;

namespace RaspberryIRDotNet.RX.PulseSpaceSource
{
    /// <summary>
    /// Capture the raw packets from the LIRC device.
    /// </summary>
    public abstract class RawLircPacketCapture
    {
        private readonly FileSystem.IFileSystem _fileSystem;

        private readonly object _capturingLocker = new object();

        /// <summary>
        /// The IR capture device, example '/dev/lirc0'.
        /// </summary>
        public string CaptureDevice { get; set; }

        /// <summary>
        /// Set the IR driver timeout value. Or set to -1 to not set the value (leave the driver in what ever state it was already in). The default (what the driver defaults to not what this property defaults to) is stored in the constant <see cref="IRDeviceDefaults.Timeout"/>.
        /// </summary>
        public int TimeoutMicrosecs { get; set; } = -1;

        protected RawLircPacketCapture(FileSystem.IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        /// <summary>
        /// When the device is opened this is called to check that the device is OK (e.g. has the right capabilities, etc).
        /// </summary>
        private void CheckDevice(FileSystem.IOpenFile irDevice, DeviceFeatures deviceFeatures)
        {
            if (!deviceFeatures.CanReceive())
            {
                throw new NotSupportedException("This IR device cannot receive.");
            }

            if (!deviceFeatures.HasFlag(DeviceFeatures.ReceiveModeMode2))
            {
                throw new NotSupportedException("Only MODE2 is supported for capture, but this device does not support MODE2.");
            }

            uint recMode = irDevice.IoCtlReadUInt32(LircConstants.LIRC_GET_REC_MODE);
            if (recMode != LircConstants.LIRC_MODE_MODE2)
            {
                // The Raspberry Pi seems to always default to MODE2 when opening, but in case that changes got the option to set the mode.
                try
                {
                    irDevice.IoCtlWrite(LircConstants.LIRC_SET_REC_MODE, LircConstants.LIRC_MODE_MODE2);
                }
                catch (System.ComponentModel.Win32Exception err)
                {
                    throw new NotSupportedException("Unable to set receive mode to MODE2.", err);
                }
            }
        }

        public void Capture(ReadCancellationToken cancellationToken)
        {
            if (cancellationToken == null)
            {
                cancellationToken = new ReadCancellationToken();
            }

            if (!cancellationToken.AddReference()) // Add a reference so that anyone waiting on the token won't get a result until we have fully closed the LIRC device.
            {
                return;
            }
            try
            {
                if (!System.Threading.Monitor.TryEnter(_capturingLocker))
                {
                    throw new InvalidOperationException("Capture already in progress.");
                }
                try
                {
                    OnBeforeRawCapture();

                    if (string.IsNullOrWhiteSpace(CaptureDevice))
                    {
                        throw new ArgumentNullException(nameof(CaptureDevice), $"The capture device must be set.");
                    }

                    using (var irDevice = _fileSystem.OpenRead(CaptureDevice))
                    {
                        DeviceFeatures deviceFeatures = Utility.GetFeatures(irDevice);
                        CheckDevice(irDevice, deviceFeatures);

                        if (TimeoutMicrosecs >= 0)
                        {
                            Utility.SetRxTimeout(irDevice, TimeoutMicrosecs);
                        }
                        byte[] buffer = new byte[4];
                        while (true)
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                return;
                            }
                            int readCount = irDevice.ReadFromDevice(buffer, cancellationToken); // Note if "modprobe -r gpio_ir_recv" is used to unload the driver while this is reading then we get an IOException.
                            if (readCount == -1 || cancellationToken.IsCancellationRequested)
                            {
                                // The read was cancelled.
                                return;
                            }
                            if (readCount != 4)
                            {
                                throw new System.IO.IOException($"Read {readCount} bytes instead of the expected 4.");
                            }

                            Mode2PacketType packetType = (Mode2PacketType)buffer[3];
                            buffer[3] = 0;

                            Lazy<int> number = new Lazy<int>(() => BitConverter.ToInt32(buffer));

                            if (!OnReceivePacket(packetType, number))
                            {
                                return;
                            }
                        }
                    }
                }
                finally
                {
                    System.Threading.Monitor.Exit(_capturingLocker);
                }
            }
            finally
            {
                cancellationToken.ReleaseReference();
            }
        }

        /// <returns>
        /// Return TRUE to continue capturing IR or FALSE to stop.
        /// </returns>
        protected abstract bool OnReceivePacket(Mode2PacketType packetType, Lazy<int> packetData);

        /// <summary>
        /// Occurs just before capturing starts. Override if you need to do any set up in your class before capture.
        /// </summary>
        protected virtual void OnBeforeRawCapture()
        {
        }
    }
}
