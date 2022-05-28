using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using RaspberryIRDotNet.Exceptions;
using static RaspberryIRDotNet.LinuxErrorCodes;

namespace RaspberryIRDotNet.DeviceAssessment
{
    /// <summary>
    /// Assess the capabilities of IR devices.
    /// </summary>
    public class DeviceAssessor
    {
        private readonly FileSystem.IFileSystem _fileSystem;

        public DeviceAssessor() : this(new FileSystem.RealFileSystem())
        {
        }

        internal DeviceAssessor(FileSystem.IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        /// <summary>
        /// Assess a single IR device.
        /// </summary>
        /// <param name="path">Example: /dev/lirc0</param>
        /// <exception cref="System.IO.FileNotFoundException"></exception>
        /// <exception cref="System.IO.DirectoryNotFoundException"></exception>
        /// <exception cref="NotAnIRDeviceException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public AssessmentResult AssessDevice(string path)
        {
            string fullPath = _fileSystem.GetFullPath(path);
            using (var irDevice = _fileSystem.OpenRead(fullPath))
            {
                DeviceFeatures features = Utility.GetFeatures(irDevice); /// If this is not an IR device then expect a <see cref="NotAnIRDeviceException"/> exception from this.

                string realPath = _fileSystem.GetRealPath(fullPath);

                uint? minTimeOut = null;
                uint? maxTimeOut = null;
                uint? currentTimeOut = null;
                if (features.CanReceive()) // Only try this query for receive devices since it is not applicable to transmit devices so is very unlikely to work.
                {
                    minTimeOut = GetIoCtlValueIfImplemented(irDevice, LircConstants.LIRC_GET_MIN_TIMEOUT);
                    maxTimeOut = GetIoCtlValueIfImplemented(irDevice, LircConstants.LIRC_GET_MAX_TIMEOUT);
                    currentTimeOut = GetIoCtlValueIfImplemented(irDevice, LircConstants.LIRC_GET_REC_TIMEOUT);
                }

                return new AssessmentResult(fullPath, realPath, features, minTimeOut, maxTimeOut, currentTimeOut);
            }
        }

        private uint? GetIoCtlValueIfImplemented(FileSystem.IOpenFile irDevice, uint requestCode)
        {
            try
            {
                return irDevice.IoCtlReadUInt32(requestCode);
            }
            catch (System.ComponentModel.Win32Exception err) when (err.NativeErrorCode == ENOSYS || err.NativeErrorCode == ENOTTY)
            {
                return null;
            }
        }

        /// <summary>
        /// Search for IR devices and assess them all.
        /// </summary>
        public AssessmentResult[] AssessAll()
        {
            var dir = new System.IO.DirectoryInfo("/dev/");
            if (!dir.Exists)
            {
                throw new System.IO.DirectoryNotFoundException("Cannot find the /dev folder.");
            }
            System.IO.EnumerationOptions options = new System.IO.EnumerationOptions()
            {
                MatchCasing = System.IO.MatchCasing.CaseInsensitive,
                MatchType = System.IO.MatchType.Simple,
                RecurseSubdirectories = false
            };
            var lircDevices = dir.GetFiles("lirc*", options);

            return lircDevices
                .Select(x =>
                {
                    try
                    {
                        return AssessDevice(x.FullName);
                    }
                    catch (NotAnIRDeviceException)
                    {
                        return null;
                    }
                })
                .Where(x => x != null)
                .ToArray();
        }

        private class RealPathComp : IEqualityComparer<AssessmentResult>
        {
            public bool Equals([AllowNull] AssessmentResult x, [AllowNull] AssessmentResult y)
            {
                return string.Equals(x.RealPath, y.RealPath);
            }

            public int GetHashCode([DisallowNull] AssessmentResult obj)
            {
                return obj.RealPath.GetHashCode();
            }
        }

        /// <summary>
        /// Get the path to the only IR device that matches the filter.
        /// </summary>
        /// <param name="typeName">transmit / receive</param>
        /// <param name="filter">Example: x => x.CanSend</param>
        private string GetPathToSingleDevice(string typeName, Func<AssessmentResult, bool> filter)
        {
            var options = AssessAll().Where(filter).ToList();
            if (options.Count <= 0)
            {
                throw new NoIRDevicesFoundException($"There is no IR {typeName} device.");
            }
            if (options.Count > 1)
            {
                var distinctRealPath = options.Distinct(new RealPathComp()).ToList(); // Maybe there are symbolic links to the IR devices? Perhaps udev has made a link.
                if (distinctRealPath.Count > 1)
                {
                    throw new MultipleIRDevicesFoundException($"There are multiple IR {typeName} devices.");
                }

                return distinctRealPath.Single().RealPath;
            }
            return options.Single().RealPath;
        }

        /// <summary>
        /// Get the only IR transmitter on this Pi, throws an exception if there is not exactly one TX device.
        /// </summary>
        /// <returns>
        /// Something like /dev/lirc0
        /// </returns>
        public string GetPathToTheTransmitterDevice()
        {
            return GetPathToSingleDevice("transmit", x => x.CanSend);
        }

        /// <summary>
        /// Get the only IR receiver on this Pi, throws an exception if there is not exactly one RX device.
        /// </summary>
        /// <returns>
        /// Something like /dev/lirc0
        /// </returns>
        public string GetPathToTheReceiverDevice()
        {
            return GetPathToSingleDevice("receive", x => x.CanReceive);
        }
    }
}
