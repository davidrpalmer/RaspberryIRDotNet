using System;

namespace RaspberryIRDotNet.RX
{
    /// <summary>
    /// Common interface for <see cref="PulseSpaceConsoleWriter"/> and <see cref="FilteredPulseSpaceConsoleWriter"/>.
    /// </summary>
    public interface IIRConsoleWriter
    {
        Action<string> ConsoleWriteLine { get; }

        void Start();

        void StopWhenPossible();
    }
}
