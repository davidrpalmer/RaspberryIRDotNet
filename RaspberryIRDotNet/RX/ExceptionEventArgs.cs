using System;

namespace RaspberryIRDotNet.RX
{
    public class ExceptionEventArgs(Exception exception) : EventArgs
    {
        public Exception Exception { get; } = exception ?? throw new ArgumentNullException(nameof(exception));
    }
}
