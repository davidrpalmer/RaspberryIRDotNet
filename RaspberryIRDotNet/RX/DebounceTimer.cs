using System;
using System.Diagnostics;

namespace RaspberryIRDotNet.RX
{
    internal interface IDebounceTimer
    {
        bool ReadyToDoAnother { get; }

        void Restart();
    }

    internal class DebounceTimer : IDebounceTimer
    {
        private readonly TimeSpan _debounceTime;

        public virtual bool ReadyToDoAnother => _timeSinceLastCapture.Elapsed > _debounceTime;

        private readonly Stopwatch _timeSinceLastCapture = new Stopwatch();

        public void Restart()
        {
            _timeSinceLastCapture.Restart();
        }

        public DebounceTimer(TimeSpan debounceTime)
        {
            _debounceTime = debounceTime;
        }
    }
}
