using System;
using System.Text;

namespace RaspberryIRDotNet.DeviceAssessment
{
    public class AssessmentResult
    {
        /// <summary>
        /// The full path to the device.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// If <see cref="Path"/> actually points to a link then this will reveal the target of that link.
        /// </summary>
        public string RealPath { get; }

        /// <summary>
        /// Is this the actual device, or just a symlink to the device.
        /// </summary>
        public bool IsLink
        {
            get
            {
                return Path != RealPath;
            }
        }

        public DeviceFeatures Features { get; }

        public bool CanSend => Features.CanSend();
        public bool CanReceive => Features.CanReceive();

        /// <summary>
        /// Measured in microseconds.
        /// </summary>
        public uint? MinimumReceiveTimeout { get; }

        /// <summary>
        /// Measured in microseconds.
        /// </summary>
        public uint? MaximumReceiveTimeout { get; }

        /// <summary>
        /// Measured in microseconds. Some IR devices reset the timeout every time they are opened. Others (like the one on the Raspberry Pi) seem to keep the value until the driver is restarted.
        /// </summary>
        public uint? CurrentReceiveTimeout { get; }

        private const string _defaultIndent = "  ";

        public AssessmentResult(string path, string realPath, DeviceFeatures features, uint? minTimeout, uint? maxTimeout, uint? currentTimeout)
        {
            Path = path;
            RealPath = realPath;
            Features = features;

            MinimumReceiveTimeout = minTimeout;
            MaximumReceiveTimeout = maxTimeout;
            CurrentReceiveTimeout = currentTimeout;
        }

        public string WriteToString(string indent = _defaultIndent)
        {
            StringBuilder sb = new StringBuilder();
            WriteToString(text => sb.AppendLine(text), indent);
            return sb.ToString();
        }

        public void WriteToString(Action<string> lineWriter, string indent = _defaultIndent)
        {
            if (IsLink)
            {
                lineWriter($"{Path} => {RealPath}");
            }
            else
            {
                lineWriter(Path);
            }

            lineWriter(indent + "Can send: " + CanSend);
            lineWriter(indent + "Can receive: " + CanReceive);

            lineWriter(indent + "Features: " + Features.ToString());

            if (CanReceive)
            {
                const int numberWidth = 7;
                lineWriter(indent + "Receive timeout (microseconds):");
                lineWriter(indent + "      Minimum: " + MinimumReceiveTimeout.ToString().PadLeft(numberWidth));
                lineWriter(indent + "      Maximum: " + MaximumReceiveTimeout.ToString().PadLeft(numberWidth));
                lineWriter(indent + "      Current: " + CurrentReceiveTimeout.ToString().PadLeft(numberWidth));
            }
        }
    }
}
