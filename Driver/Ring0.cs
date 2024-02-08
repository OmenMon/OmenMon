  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023-2024 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

// This portion based on the code from Open Hardware Monitor, Copyright © 2009-2017 Michael Möller
// and its successor Libre Hardware Monitor, Copyright © 2020-2023 Libre Hardware Monitor & Contributors

// Licensed under the terms of the Mozilla Public License (MPL) 2.0: https://mozilla.org/MPL/2.0/

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using OmenMon.External;

namespace OmenMon.Driver {

    // Extracts and configures the driver file as a kernel service using the Driver class
    // Implements specific kernel-mode calls based on general device I/O control methods
    public static class Ring0 {

        private static Driver driver;
        private static string driverFilePath;
        private static readonly StringBuilder log = new();

        public static bool IsOpen => driver != null;

#region Driver Setup Methods
        // Closes the driver and tries to delete the driver file
        public static void Close() {
            if(driver != null) {
                uint refCount = 0;
                driver.DeviceIOControl(Kernel32.IOCTL_OLS_GET_REFCOUNT, null, ref refCount);
                driver.Close();

                // Delete the driver if it is not being used by anything else
                if(refCount <= 1)
                    driver.Delete();

                driver = null;
            }

            // Try to delete the driver file again, in case it failed when opening
            Delete();
        }

        // Tries to delete the driver file
        private static void Delete() {
            try {
                if(driverFilePath != null && File.Exists(driverFilePath))
                    File.Delete(driverFilePath);
                driverFilePath = null;
            } catch {
            }
        }

        // Extracts the driver file
        private static bool Extract(string filePath) {
            Assembly assembly = typeof(Ring0).Assembly;
            long requiredLength = 0;
            try {
                using Stream stream = assembly.GetManifestResourceStream("OmenMon.Driver.sys.gz");
                if(stream != null) {
                    using FileStream target = new(filePath, FileMode.Create);
                    stream.Position = 1; // Skip the first byte (but why?)
                    using var gzipStream = new GZipStream(stream, CompressionMode.Decompress);
                    gzipStream.CopyTo(target);
                    requiredLength = target.Length;
                }
            } catch {
                return false;
            }

            if(HasValidFile())
                return true;

            // Ensure enough time for the extraction to have completed
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (stopwatch.ElapsedMilliseconds < 2000) {
                if(HasValidFile())
                    return true;
                Thread.Yield();
            }
            return false;

            // Checks if the driver file already exists and is of the correct size
            bool HasValidFile() {
                try {
                    return File.Exists(filePath) && new FileInfo(filePath).Length == requiredLength;
                } catch {
                    return false;
                }
            }
        }

        // Does everything that is necessary to load the kernel driver
        public static void Open() {

            log.Length = 0;

            // Do nothing if the driver has been opened already
            if(driver != null)
                return;

            // Try opening an already installed driver first
            driver = new Driver(GetServiceName(), "WinRing0_1_2_0");
            driver.Open();

            // If that did not work out, install the driver
            if(!driver.IsOpen) {
                driverFilePath = GetFilePath();
                if(driverFilePath != null && Extract(driverFilePath)) {
                    // Driver file is present in the determined location, try to install it
                    if(driver.Install(driverFilePath, out string firstError)) {
                        driver.Open();
                        if(!driver.IsOpen)
                            log.AppendLine("Failed to open driver after installation");
                    } else { // Installation failed on the 1st attempt, try again
                        driver.Delete();
                        Thread.Sleep(2000);
                        if(driver.Install(driverFilePath, out string secondError)) { // Retry
                            driver.Open();
                            if(!driver.IsOpen)
                                log.AppendLine("Failed to open driver after re-installation");
                        } else { // Installation failed on the 2nd attempt, treat it as permanent
                            log.Append($"Failed to install driver from file: \"{driverFilePath}\"").AppendLine(File.Exists(driverFilePath) ? " (exists) " : " (non-existent) ");
                            log.Append("1st Try: ").AppendLine(firstError + " ");
                            log.Append("2nd Try: ").AppendLine(secondError + " ");
                        }
                    }

                    if(!driver.IsOpen) {
                        driver.Delete();
                        Delete();
                    }
                } else {
                    log.AppendLine("Failed to extract driver");
                }
            }
            if(!driver.IsOpen)
                driver = null;
        }
#endregion

#region Helper Methods
        // Picks the most suitable location to extract the driver to
        private static string GetFilePath() {
            string filePath = null;

            // 1st Choice: Use the process's image name, replacing the extension with ".sys"
            try {
                ProcessModule processModule = Process.GetCurrentProcess().MainModule;
                if(!string.IsNullOrEmpty(processModule?.FileName)) {
                    filePath = Path.ChangeExtension(processModule.FileName, ".sys");
                    if(CanCreate(filePath))
                        return filePath;
                }
            } catch {
            }

            // 2nd Choice: Use the executing assembly name with a ".sys" extension
            string previousFilePath = filePath;
            filePath = GetPathFromAssembly(Assembly.GetExecutingAssembly());
            if(previousFilePath != filePath && !string.IsNullOrEmpty(filePath) && CanCreate(filePath))
                return filePath;

            // 3rd Choice: Use this class's assembly name with a ".sys" extension
            previousFilePath = filePath;
            filePath = GetPathFromAssembly(typeof(Ring0).Assembly);
            if(previousFilePath != filePath && !string.IsNullOrEmpty(filePath) && CanCreate(filePath))
                return filePath;

            // If all else fails, use a temporary filename, also with a ".sys" extension
            try {
                filePath = Path.GetTempFileName();
                if(!string.IsNullOrEmpty(filePath)) {
                    filePath = Path.ChangeExtension(filePath, ".sys");
                    if(CanCreate(filePath))
                        return filePath;
                }
            } catch {
                return null;
            }
            return null;

            // Retrieves the filesystem path given an assembly
            static string GetPathFromAssembly(Assembly assembly) {
                try {
                    string location = assembly?.Location;
                    return !string.IsNullOrEmpty(location) ? Path.ChangeExtension(location, ".sys") : null;
                } catch {
                    return null;
                }
            }

            // Checks if a file can be created in a given location
            static bool CanCreate(string path) {
                try {
                    using (File.Create(path, 1, FileOptions.DeleteOnClose))
                        return true;
                } catch {
                    return false;
                }
            }
        }

        // Picks the most suitable service name for the kernel driver
        private static string GetServiceName() {
            string name;

            // 1st Choice: Use the process's image name
            try {
                ProcessModule processModule = Process.GetCurrentProcess().MainModule;
                if(!string.IsNullOrEmpty(processModule?.FileName)) {
                    name = Path.GetFileNameWithoutExtension(processModule.FileName);
                    if(!string.IsNullOrEmpty(name))
                        return GetName(name);
                }
            } catch {
            }

            // 2nd Choice: Use the executing assembly name
            name = GetNameFromAssembly(Assembly.GetExecutingAssembly());
            if(!string.IsNullOrEmpty(name))
                return GetName(name);

            // 3rd Choice: Use this class's assembly name
            name = GetNameFromAssembly(typeof(Ring0).Assembly);
            if(!string.IsNullOrEmpty(name))
                return GetName(name);

            // If all else fails, use a hard-coded name
            name = nameof(OmenMon);
            return GetName(name);

            // Retrieves the name of an assembly
            static string GetNameFromAssembly(Assembly assembly) {
                return assembly?.GetName().Name;
            }

            // Sanitizes the picked name before returning it
            static string GetName(string name) {
                return $"R0{name}".Replace(" ", string.Empty).Replace(".", "_");
            }
        }

        // For debugging purposes, this returns step-by-step information
        // if loading the driver failed; otherwise, the log is empty
        public static string GetStatus() {
            return log.ToString();
        }
#endregion

#region Input/Output (I/O) Port Operations
        // Struct for I/O port write call
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct WriteIoPortInput {
            public uint PortNumber;
            public byte Value;
        }

        // Performs a I/O port read operation
        public static byte ReadIoPort(uint port) {
            if(driver == null)
                return 0;

            uint value = 0;
            driver.DeviceIOControl(Kernel32.IOCTL_OLS_READ_IO_PORT_BYTE, port, ref value);
            return (byte) (value & 0xFF);
        }

        // Performs a I/O port write operation
        public static void WriteIoPort(uint port, byte value) {
            if(driver == null)
                return;

            WriteIoPortInput input = new() {
                PortNumber = port,
                Value = value };

            driver.DeviceIOControl(Kernel32.IOCTL_OLS_WRITE_IO_PORT_BYTE, input);
        }
#endregion

#region Model-Specific Register (MSR) Operations
        // Struct for MSR write call
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct WriteMsrInput {
            public uint Register;
            public ulong Value;
        }

        // Performs an MSR read operation
        public static bool ReadMsr(uint index, out uint eax, out uint edx) {
            if(driver == null) {
                eax = 0;
                edx = 0;
                return false;
            }

            ulong buffer = 0;
            bool result = driver.DeviceIOControl(Kernel32.IOCTL_OLS_READ_MSR, index, ref buffer);
            edx = (uint) ((buffer >> 32) & 0xFFFFFFFF);
            eax = (uint) (buffer & 0xFFFFFFFF);
            return result;
        }

        // Performs an MSR write operation
        public static bool WriteMsr(uint index, uint eax, uint edx) {
            if(driver == null)
                return false;

            WriteMsrInput input = new() {
                Register = index,
                Value = ((ulong) edx << 32) | eax };

            return driver.DeviceIOControl(Kernel32.IOCTL_OLS_WRITE_MSR, input);
        }
#endregion

#region Peripheral Component Interconnect (PCI) Operations
        // Struct for PCI read call
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct ReadPciConfigInput {
            public uint PciAddress;
            public uint RegAddress;
        }

        // Struct for PCI write call
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct WritePciConfigInput {
            public uint PciAddress;
            public uint RegAddress;
            public uint Value;
        }

        // Obtains the PCI address of a given device
        public static uint GetPciAddress(byte bus, byte device, byte function) {
            return (uint) (((bus & 0xFF) << 8) | ((device & 0x1F) << 3) | (function & 7));
        }

        // Retrieves the PCI configuration of a given device
        public static bool ReadPciConfig(uint pciAddress, uint regAddress, out uint value) {
            if(driver == null || (regAddress & 3) != 0) {
                value = 0;
                return false;
            }

            ReadPciConfigInput input = new() {
                PciAddress = pciAddress,
                RegAddress = regAddress };

            value = 0;
            return driver.DeviceIOControl(Kernel32.IOCTL_OLS_READ_PCI_CONFIG, input, ref value);
        }

        // Writes the PCI configuration of a given device
        public static bool WritePciConfig(uint pciAddress, uint regAddress, uint value) {
            if(driver == null || (regAddress & 3) != 0)
                return false;

            WritePciConfigInput input = new() {
                PciAddress = pciAddress,
                RegAddress = regAddress,
                Value = value };

            return driver.DeviceIOControl(Kernel32.IOCTL_OLS_WRITE_PCI_CONFIG, input);
        }
#endregion

#region Memory Operations
        // Struct for memory read call
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct ReadMemoryInput {
            public ulong Address;
            public uint UnitSize;
            public uint Count;
        }

        // Reads a memory address into a buffer
        public static bool ReadMemory<T>(ulong address, ref T buffer) {
            if(driver == null)
                return false;

            ReadMemoryInput input = new() {
                Address = address,
                UnitSize = 1,
                Count = (uint) Marshal.SizeOf(buffer) };

            return driver.DeviceIOControl(Kernel32.IOCTL_OLS_READ_MEMORY, input, ref buffer);
        }

        // Reads a memory address into a buffer array
        public static bool ReadMemory<T>(ulong address, ref T[] buffer) {
            if(driver == null)
                return false;

            ReadMemoryInput input = new() {
                Address = address,
                UnitSize = (uint) Marshal.SizeOf(typeof(T)),
                Count = (uint) buffer.Length };

            return driver.DeviceIOControl(Kernel32.IOCTL_OLS_READ_MEMORY, input, ref buffer);
        }
#endregion

        }

}
