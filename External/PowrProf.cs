  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2024 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using System.Runtime.InteropServices;

namespace OmenMon.External {

    // Microsoft Power Profile Helper DLL (powrprof.dll) Resources
    // Used for receiving suspend and resume power event notifications
    public class PowrProf {

#region Microsoft Power Profile Helper DLL Data
        // Notification type constant
        public const uint DEVICE_NOTIFY_CALLBACK    = 0x0002;

        // Power management event constants
        public const uint PBT_APMPOWERSTATUSCHANGE  = 0x000A;  // Power status change
        public const uint PBT_APMRESUMEAUTOMATIC    = 0x0012;  // Resume from low-power state
        public const uint PBT_APMRESUMESUSPEND      = 0x0007;  // User-triggered resume (follows PBT_APMRESUMEAUTOMATIC)
        public const uint PBT_APMSUSPEND            = 0x0004;  // Suspend initiated
        public const uint PBT_POWERSETTINGCHANGE    = 0x8013;  // Power settings change

        // Callback routine delegate
        public delegate uint DeviceNotifyCallbackRoutine(IntPtr Context, uint Type, IntPtr Setting);

        // Device notification subscription
        [StructLayout(LayoutKind.Sequential)]
        public struct DEVICE_NOTIFY_SUBSCRIBE_PARAMETERS {
            public DeviceNotifyCallbackRoutine Callback;
            public IntPtr Context;
        }
#endregion

#region Microsoft Power Profile Helper DLL Imports
        public const string DllName = "powrprof.dll";

        [DllImport(DllName, CallingConvention = CallingConvention.Winapi)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern uint PowerRegisterSuspendResumeNotification(
            uint Flags,
            ref DEVICE_NOTIFY_SUBSCRIBE_PARAMETERS Recipient,
            ref IntPtr RegistrationHandle);

        [DllImport(DllName, CallingConvention = CallingConvention.Winapi)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern uint PowerUnregisterSuspendResumeNotification(
            IntPtr RegistrationHandle);
#endregion

    }

}
