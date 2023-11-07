  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using System.Runtime.InteropServices;

namespace OmenMon.External {

    // Microsoft Color Matching System DLL (mscms.dll) Resources
    // Used for reapplying the color profile (also part of nVidia Advanced Optimus fix)
    public class ColorMgmt {

#region Microsoft Color Matching System DLL Imports
        public const string DllName = "mscms.dll";

        [DllImport(DllName, CallingConvention = CallingConvention.Winapi)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern bool WcsSetCalibrationManagementState(bool bIsEnabled);
#endregion

    }

}
