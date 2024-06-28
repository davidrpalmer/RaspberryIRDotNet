using System;
using RaspberryIRDotNet.FileSystem;

namespace RaspberryIRDotNet.TX
{
    /// <summary>
    /// Allows sending IR data. The IR device is opened manually, then multiple IR messages can be sent, then the device can be closed.
    /// </summary>
    public class PulseSpaceTransmitter_ManualOpenClose : PulseSpaceTransmitter, IIRSender, IDisposable
    {
        private IOpenFile _openDeviceHandle = null;

        public bool IsOpen => _openDeviceHandle != null;

        public bool Disposed { get; private set; }

        public void Send(IRPulseMessage message)
        {
            Send(message.PulseSpaceDurations);
        }

        public void Send(IReadOnlyPulseSpaceDurationList buffer)
        {
            AssertNotDisposed();
            AssertOpen();
            WriteToDevice(_openDeviceHandle, buffer);
        }

        public void Open()
        {
            AssertNotDisposed();
            if (IsOpen) { throw new InvalidOperationException("Already open."); }

            _openDeviceHandle = OpenDevice();
        }

        /// <summary>
        /// Close the IR device. It can still be opened again in the future (unlike Dispose).
        /// </summary>
        public void Close()
        {
            _openDeviceHandle?.Dispose();
            _openDeviceHandle = null;
        }

        protected void AssertNotDisposed() => ObjectDisposedException.ThrowIf(Disposed, this);

        protected void AssertOpen()
        {
            if (!IsOpen) { throw new InvalidOperationException("Must open the device first."); }
        }

        /// <summary>
        /// Apply the Frequency and DutyCycle settings to the IR device (unless they are -1).
        /// </summary>
        public void ApplySettings()
        {
            AssertNotDisposed();
            AssertOpen();
            ApplySettings(_openDeviceHandle);
        }

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    _openDeviceHandle?.Dispose();
                }

                Disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
