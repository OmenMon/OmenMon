  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023-2024 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using System.Runtime.InteropServices;

namespace OmenMon.External {

    // Windows User Interface API (user32.dll) Resources
    // Used for user interface settings and system restart
    public class User32 {

#region Windows User Interface API Data
        // Change display settings
        public const int CDS_TEST           = 0x02;
        public const int CDS_UPDATEREGISTRY = 0x01;

        public const int DISP_CHANGE_FAILED    = -1;
        public const int ENUM_CURRENT_SETTINGS = -1;

        // Exit Windows flags
        public const int EWX_FORCE       = 0x00000004;
        public const int EWX_FORCEIFHUNG = 0x00000010;
        public const int EWX_REBOOT      = 0x00000002;

        // Handle identifying all windows for message broadcast purposes
        public const int HWND_BROADCAST = 0xFFFF;

        // Handle values used for setting window position
        public static readonly IntPtr HWND_BOTTOM    = new IntPtr( 1);
        public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        public static readonly IntPtr HWND_TOP       = new IntPtr( 0);
        public static readonly IntPtr HWND_TOPMOST   = new IntPtr(-1);

        // System commands
        public const int SC_MONITORPOWER = 0xF170;

        // Shutdown reason flags
        public const uint SHTDN_REASON_FLAG_PLANNED   = 0x80000000;
        public const uint SHTDN_REASON_MAJOR_HARDWARE = 0x00010000;
        public const uint SHTDN_REASON_MINOR_RECONFIG = 0x00000004;

        // Show window flags
        public const int SW_HIDE       = 0x00;
        public const int SW_MINIMIZE   = 0x06;
        public const int SW_SHOWNORMAL = 0x01;
        public const int SW_RESTORE    = 0x09;

        // Set window position flags
        public const uint SWP_NOSIZE     = 0x0001;
        public const uint SWP_SHOWWINDOW = 0x0040;
        public const uint SWP_NOZORDER   = 0x0004;

        // Key codes
        public const int VK_ENTER = 0x0D;

        // Window messages
        public const int WM_CHAR                    = 0x0102;
        public const int WM_SYSCOMMAND              = 0x0112;
        public const int WM_CTLCOLOREDIT            = 0x0133;
        public const int WM_CTLCOLORSTATIC          = 0x0138;
        public const int WM_DPICHANGED              = 0x02E0;
        public const int WM_DPICHANGED_AFTERPARENT  = 0x02E3;
        public const int WM_DPICHANGED_BEFOREPARENT = 0x02E2;
        public const int WM_GETDPISCALEDSIZE        = 0x02E4;
        public const int WM_INITDIALOG              = 0x0110;
        public const int WM_POWERBROADCAST          = 0x0218;
        public const int WM_USER                    = 0x0400;

        // Device mode
        [StructLayout(LayoutKind.Sequential)]
        public struct DEVMODE {

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public short dmOrientation;
            public short dmPaperSize;
            public short dmPaperLength;
            public short dmPaperWidth;
            public short dmScale;
            public short dmCopies;
            public short dmDefaultSource;
            public short dmPrintQuality;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;
            public short dmLogPixels;
            public short dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;

        }

        // Device notification type
        public const uint DEVICE_NOTIFY_WINDOW_HANDLE = 0;

        // DPI awareness context
        public enum DPI_AWARENESS_CONTEXT {
            DPI_AWARENESS_CONTEXT_UNAWARE              = -1,
            DPI_AWARENESS_CONTEXT_SYSTEM_AWARE         = -2,
            DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE    = -3,
            DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = -4,
            DPI_AWARENESS_CONTEXT_UNAWARE_GDISCALED    = -5
        }

        // Monitor power
        public enum MONITORPOWER {
            POWERING_ON = -1,
            LOW_POWER   =  1,
            STANDBY     =  2

        }
#endregion

#region Windows User Interface API Imports
        public const string DllName = "user32.dll";

        [DllImport(DllName, CallingConvention = CallingConvention.Winapi)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern int ChangeDisplaySettings(ref DEVMODE devMode, int flags);

        [DllImport(DllName, CallingConvention = CallingConvention.Winapi)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern bool DestroyIcon(IntPtr handle);

        [DllImport(DllName, CallingConvention = CallingConvention.Winapi)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern int EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        [DllImport(DllName, CallingConvention = CallingConvention.Winapi)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern int ExitWindowsEx(int uFlags, uint dwReason);

        [DllImport(DllName, CallingConvention = CallingConvention.Winapi)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport(DllName, CallingConvention = CallingConvention.Winapi)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport(DllName, CallingConvention = CallingConvention.Winapi)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern IntPtr GetDlgItem(IntPtr hDlg, int nIDDlgItem);

        [DllImport(DllName, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern int GetWindowText(IntPtr hWnd, [Out] char[] lpString, int nMaxCount);

        [DllImport(DllName, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport(DllName, CallingConvention = CallingConvention.Winapi)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern bool PostMessage(IntPtr hWnd, [MarshalAs(UnmanagedType.U4)] uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport(DllName, CallingConvention = CallingConvention.Winapi)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern uint RegisterSuspendResumeNotification(IntPtr hWnd, uint flags);

        [DllImport(DllName, CallingConvention = CallingConvention.Winapi)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern uint RegisterWindowMessage(string message);

        [DllImport(DllName, CallingConvention = CallingConvention.Winapi)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport(DllName, CallingConvention = CallingConvention.Winapi)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern int SendMessage(IntPtr hWnd, [MarshalAs(UnmanagedType.U4)] uint msg, IntPtr wParam, IntPtr lParam);

        [DllImportAttribute(DllName, CallingConvention = CallingConvention.Winapi)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImportAttribute(DllName, CallingConvention = CallingConvention.Winapi)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern bool SetProcessDPIAware();

        [DllImportAttribute(DllName, CallingConvention = CallingConvention.Winapi)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern bool SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT value);

        [DllImportAttribute(DllName, CallingConvention = CallingConvention.Winapi)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        [DllImportAttribute(DllName, CallingConvention = CallingConvention.Winapi)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern bool SetWindowText(IntPtr hWnd, string text);

        [DllImportAttribute(DllName, CallingConvention = CallingConvention.Winapi)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImportAttribute(DllName, CallingConvention = CallingConvention.Winapi)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);
#endregion

    }

}
