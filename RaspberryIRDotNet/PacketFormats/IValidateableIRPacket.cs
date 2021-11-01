using System;

namespace RaspberryIRDotNet.PacketFormats
{
    /// <summary>
    /// An IR packet that has some validation built into it.
    /// </summary>
    public interface IValidateableIRPacket : IIRFormatPacket
    {
        /// <summary>
        /// Check that any auto calculated values or checksums are set correctly.
        /// </summary>
        /// <returns>
        /// TRUE if auto calculated values match OK, FALSE if not.
        /// </returns>
        bool Validate();

        /// <summary>
        /// Calculate and set the auto calculated or checksum values.
        /// </summary>
        public void SetCalculatedValues();
    }
}
