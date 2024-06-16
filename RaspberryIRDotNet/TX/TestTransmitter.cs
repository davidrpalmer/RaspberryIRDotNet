using System;
using System.Threading;

namespace RaspberryIRDotNet.TX
{
    /// <summary>
    /// For use with an oscilloscope. Transmits data and allows live adjustment (of gap, data, duty cycle and carrier frequency) while transmitting.
    /// </summary>
    public class TestTransmitter : PulseSpaceTransmitter
    {
        /// <summary>
        /// How long to leave between each transmission.
        /// </summary>
        public TimeSpan Gap { get; set; } = TimeSpan.FromMilliseconds(100);

        private Thread _thread;

        private bool _keepGoing = false;

        private readonly object _startStopLocker = new();

        /// <summary>
        /// The data to transmit, or null to use default data. This object must not be modified while transmitting, although it may be replaced either before or while transmitting.
        /// </summary>
        public IReadOnlyPulseSpaceDurationList DataToSend { get; set; }

        /// <summary>
        /// Start a worker thread and return - so that the original thread is free to tweak settings.
        /// </summary>
        public void StartInBackground()
        {
            lock (_startStopLocker)
            {
                if (_thread?.IsAlive == true)
                {
                    throw new InvalidOperationException("Already started.");
                }

                if (DataToSend == null)
                {
                    DataToSend = new PulseSpaceDurationList()
                    {
                        500,
                        500,
                        500
                    };
                }

                _thread = new Thread(new ThreadStart(Loop))
                {
                    Name = "RaspberryIRDotNet - TestTransmitter - " + TransmissionDevice,
                    IsBackground = true
                };
                _keepGoing = true;
                _thread.Start();
            }
        }

        /// <summary>
        /// Signal the worker thread to stop, then wait for it to actually stop before returning.
        /// </summary>
        public void Stop()
        {
            lock (_startStopLocker)
            {
                if (_thread == null)
                {
                    return;
                }
                _keepGoing = false;
                _thread.Join();
                _thread = null;
            }
        }

        private void Loop()
        {
            int lastFrequency = Frequency;
            int lastDutyCycle = DutyCycle;

            using (var irDevice = OpenDevice())
            {
                while (_keepGoing)
                {
                    if (Frequency != lastFrequency && Frequency >= 0)
                    {
                        SetFrequency(irDevice, Frequency);
                        lastFrequency = Frequency;
                    }

                    if (DutyCycle != lastDutyCycle && DutyCycle >= 0)
                    {
                        SetDutyCycle(irDevice, DutyCycle);
                        lastDutyCycle = DutyCycle;
                    }

                    WriteToDevice(irDevice, DataToSend);

                    if (Gap > TimeSpan.Zero)
                    {
                        Thread.Sleep(Gap);
                    }
                }
            }
        }

    }
}
