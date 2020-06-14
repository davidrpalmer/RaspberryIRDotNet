using System;

namespace RaspberryIRDotNet.Exceptions
{
    /// <summary>
    /// Thrown when trying to learn an IR code but the samples are too different to work out the correct pattern.
    /// </summary>
    public class InsufficientMatchingSamplesException : Exception
    {
        public InsufficientMatchingSamplesException(string message) : base(message)
        {
        }
    }
}
