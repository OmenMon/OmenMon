  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace OmenMon.External {

    // Windows NT Kernel (kernel32.dll) resources
    // Used for console manipulation
    // and hardware operations with a kernel driver
    public class Kernel32 {

#region Windows NT Kernel Data
        // Console manipulation

        public const uint ATTACH_PARENT_PROCESS = 0xFFFFFFFF;

        // File operations

        public enum MoveFileFlags : uint {

            MOVEFILE_REPLACE_EXISTING      = 0x00000001,
            MOVEFILE_COPY_ALLOWED          = 0x00000002,
            MOVEFILE_DELAY_UNTIL_REBOOT    = 0x00000004,
            MOVEFILE_WRITE_THROUGH         = 0x00000008,
            MOVEFILE_CREATE_HARDLINK       = 0x00000010,
            MOVEFILE_FAIL_IF_NOT_TRACKABLE = 0x00000020

        }

        // Hardware operations with a kernel driver

        public const int ERROR_SERVICE_ALREADY_RUNNING = unchecked((int) 0x80070420);
        public const int ERROR_SERVICE_EXISTS          = unchecked((int) 0x80070431);

        public const uint OLS_TYPE = 40000;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IOControlCode {
            public uint Code { get; }

            public IOControlCode(uint deviceType, uint function, Access access) : this(deviceType, function, Method.Buffered, access) { }

            public IOControlCode(uint deviceType, uint function, Method method, Access access) {
                Code = (deviceType << 16) | ((uint) access << 14) | (function << 2) | (uint) method;
            }

            public enum Method : uint {
                Buffered  = 0,
                InDirect  = 1,
                OutDirect = 2,
                Neither   = 3
            }

            public enum Access : uint {
                Any   = 0,
                Read  = 1,
                Write = 2
            }
        }

        public static readonly IOControlCode IOCTL_OLS_GET_REFCOUNT = new(OLS_TYPE, 0x801, Kernel32.IOControlCode.Access.Any);
        public static readonly IOControlCode IOCTL_OLS_READ_MSR = new(OLS_TYPE, 0x821, Kernel32.IOControlCode.Access.Any);
        public static readonly IOControlCode IOCTL_OLS_WRITE_MSR = new(OLS_TYPE, 0x822, Kernel32.IOControlCode.Access.Any);
        public static readonly IOControlCode IOCTL_OLS_READ_IO_PORT_BYTE = new(OLS_TYPE, 0x833, Kernel32.IOControlCode.Access.Read);
        public static readonly IOControlCode IOCTL_OLS_WRITE_IO_PORT_BYTE = new(OLS_TYPE, 0x836, Kernel32.IOControlCode.Access.Write);
        public static readonly IOControlCode IOCTL_OLS_READ_PCI_CONFIG = new(OLS_TYPE, 0x851, Kernel32.IOControlCode.Access.Read);
        public static readonly IOControlCode IOCTL_OLS_WRITE_PCI_CONFIG = new(OLS_TYPE, 0x852, Kernel32.IOControlCode.Access.Write);
        public static readonly IOControlCode IOCTL_OLS_READ_MEMORY = new(OLS_TYPE, 0x841, Kernel32.IOControlCode.Access.Read);
#endregion

#region Windows NT Kernel Imports
        public const string DllName = "kernel32.dll";

        // Console manipulation

        [DllImport(DllName, CallingConvention = CallingConvention.Winapi)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern bool AttachConsole(uint dwProcessId);

        [DllImport(DllName, CallingConvention = CallingConvention.Winapi)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern bool AllocConsole();

        [DllImport(DllName, CallingConvention = CallingConvention.Winapi)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern bool FreeConsole();

        [DllImport(DllName, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        public static extern uint GetConsoleProcessList(uint[] processList, uint processCount);

        [DllImport(DllName, CallingConvention = CallingConvention.Winapi)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern IntPtr GetConsoleWindow();

        // File operations

        [DllImport(DllName, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, MoveFileFlags dwFlags);

        // Hardware operations with a kernel driver

        [DllImport(DllName, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern IntPtr CreateFile (
            string lpFileName,
            uint dwDesiredAccess,
            FileShare dwShareMode,
            IntPtr lpSecurityAttributes,
            FileMode dwCreationDisposition,
            FileAttributes dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport(DllName, CallingConvention = CallingConvention.Winapi)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern bool DeviceIoControl (
            SafeFileHandle device,
            IOControlCode ioControlCode,
            [MarshalAs(UnmanagedType.AsAny)] [In] object inBuffer,
            uint inBufferSize,
            [MarshalAs(UnmanagedType.AsAny)] [Out] object outBuffer,
            uint nOutBufferSize,
            out uint bytesReturned,
            IntPtr overlapped);
#endregion

    }

}
