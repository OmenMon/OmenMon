  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using System.Runtime.InteropServices;

namespace OmenMon.External {

    // Windows Shell Core API (shcore.dll) Resources
    // Used for DPI scaling settings
    public class ShellCore {

#region Windows Shell API Data
        // DPI awareness
        public enum PROCESS_DPI_AWARENESS {
            PROCESS_DPI_UNAWARE           = 0,
            PROCESS_SYSTEM_DPI_AWARE      = 1,
            PROCESS_PER_MONITOR_DPI_AWARE = 2
        }
#endregion

#region Windows Shell API Imports
        public const string DllName = "shcore.dll";

        [DllImportAttribute(DllName, CallingConvention = CallingConvention.Winapi)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern IntPtr SetProcessDpiAwareness(PROCESS_DPI_AWARENESS value);
#endregion

    }

}
