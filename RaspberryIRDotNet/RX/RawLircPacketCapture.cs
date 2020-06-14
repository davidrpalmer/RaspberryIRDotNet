using System;

namespace RaspberryIRDotNet.RX
{
    /// <summary>
    /// Capture the raw packets from the LIRC device.
    /// </summary>
    public abstract class RawLircPacketCapture
    {
        private protected FileSystem.IFileSystem _fileSystem;
        private protected Utility _utility;

        private readonly object _capturingLocker = new object();

        /// <summary>
        /// The IR capture device, example '/dev/lirc0'.
        /// </summary>
        public string CaptureDevice { get; set; }

        /// <summary>
        /// Set the IR driver timeout value. Or set to -1 to not set the value (leave the driver in what ever state it was already in). The default (what the driver defaults to not what this property defaults to) is stored in the constant <see cref="IRDeviceDefaults.Timeout"/>.
        /// </summary>
        public int TimeoutMicrosecs { get; set; } = -1;

        protected RawLircPacketCapture()
        {
            SetFileSystem(new FileSystem.RealFileSystem());
        }

        /// <summary>
        /// Can be used to abstract away the file system for unit testing.
        /// </summary>
        internal void SetFileSystem(FileSystem.IFileSystem newFS)
        {
            _fileSystem = newFS;
            _utility = new Utility(newFS);
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

            uint recMode = _fileSystem.IoCtlReadUInt32(irDevice, LircConstants.LIRC_GET_REC_MODE);
            if (recMode != LircConstants.LIRC_MODE_MODE2)
            {
                // The Raspberry Pi seems to always default to MODE2 when opening, but in case that changes got the option to set the mode.
                try
                {
                    _fileSystem.IoCtlWrite(irDevice, LircConstants.LIRC_SET_REC_MODE, LircConstants.LIRC_MODE_MODE2);
                }
                catch (System.ComponentModel.Win32Exception err)
                {
                    throw new NotSupportedException("Unable to set receive mode to MODE2.", err);
                }
            }
        }

        /// <summary>
        /// Open the IR device and begin reading.
        /// </summary>
        protected void CaptureFromDevice()
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
                    DeviceFeatures deviceFeatures = _utility.GetFeatures(irDevice);
                    CheckDevice(irDevice, deviceFeatures);

                    if (TimeoutMicrosecs >= 0)
                    {
                        _utility.SetRxTimeout(irDevice, TimeoutMicrosecs);
                    }
                    byte[] buffer = new byte[4];
                    while (true)
                    {
                        int readCount = irDevice.Stream.Read(buffer, 0, buffer.Length); // Note if "modprobe -r gpio_ir_recv" is used to unload the driver while this is reading then we get an IOException.
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
