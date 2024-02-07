  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023-2024 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using System.Runtime.InteropServices;

namespace OmenMon.External {

    // Windows User Interface API (gdi32.dll) Resources
    // Used for font loading as well as color setting and retrieval
    public class Gdi32 {

#region Windows Graphics Device Interface API Data
        // Device capability query index
        public enum DeviceCap {
            DRIVERVERSION   =   0,  // Device driver version
            TECHNOLOGY      =   2,  // Device classification
            HORZSIZE        =   4,  // Horizontal size [mm]
            VERTSIZE        =   6,  // Vertical size [mm]
            HORZRES         =   8,  // Horizontal width [px]
            VERTRES         =  10,  // Vertical height [px]
            BITSPIXEL       =  12,  // Number of bits per pixel
            PLANES          =  14,  // Number of planes
            NUMBRUSHES      =  16,  // Number of brushes the device has
            NUMPENS         =  18,  // Number of pens the device has
            NUMMARKERS      =  20,  // Number of markers the device has
            NUMFONTS        =  22,  // Number of fonts the device has
            NUMCOLORS       =  24,  // Number of colors the device supports
            PDEVICESIZE     =  26,  // Size required for device descriptor
            CURVECAPS       =  28,  // Curve capabilities
            LINECAPS        =  30,  // Line capabilities
            POLYGONALCAPS   =  32,  // Polygonal capabilities
            TEXTCAPS        =  34,  // Text capabilities
            CLIPCAPS        =  36,  // Clipping capabilities
            RASTERCAPS      =  38,  // Bit-block transfer capabilities
            ASPECTX         =  40,  // Length of the X leg
            ASPECTY         =  42,  // Length of the Y leg
            ASPECTXY        =  44,  // Length of the hypotenuse
            SHADEBLENDCAPS  =  45,  // Shading and Blending caps
            LOGPIXELSX      =  88,  // Logical pixels inch in X
            LOGPIXELSY      =  90,  // Logical pixels inch in Y
            SIZEPALETTE     = 104,  // Number of entries in physical palette
            NUMRESERVED     = 106,  // Number of reserved entries in palette
            COLORRES        = 108,  // Actual color resolution
            PHYSICALWIDTH   = 110,  // Print physical width in device units
            PHYSICALHEIGHT  = 111,  // Print physical height in device units
            PHYSICALOFFSETX = 112,  // Physical printable Area X margin
            PHYSICALOFFSETY = 113,  // Physical printable Area Y margin
            SCALINGFACTORX  = 114,  // Print scaling factor X
            SCALINGFACTORY  = 115,  // Print scaling factor Y
            VREFRESH        = 116,  // Display vertical refresh rate [Hz]
            DESKTOPVERTRES  = 117,  // Vertical desktop height [px]
            DESKTOPHORZRES  = 118,  // Horizontal desktop width [px]
            BLTALIGNMENT    = 119   // Preferred bit-lock transfer alignment
        }

        // Handle identifying all windows for message broadcast purposes
        public const int HWND_BROADCAST = 0xFFFF;

        // Show window flags
        public const int SW_SHOWNORMAL = 0x01;
        public const int SW_RESTORE    = 0x09;

        // Key codes
        public const int VK_ENTER = 0x0D;

        // Window messages
        public const int WM_CHAR = 0x0102;
#endregion

#region Windows Graphics Device Interface API Imports
        public const string DllName = "gdi32.dll";

        [DllImport(DllName, CallingConvention = CallingConvention.Winapi)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern IntPtr AddFontMemResourceEx(IntPtr pFileView, uint cjSize, IntPtr pvReserved, [In] ref uint pNumFonts);

        [DllImport(DllName, CallingConvention = CallingConvention.Winapi)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern int GetDeviceCaps(IntPtr hdc, DeviceCap nIndex);

        [DllImport(DllName, CallingConvention = CallingConvention.Winapi)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern int GetPixel(IntPtr hDC, int x, int y);

        [DllImport(DllName, CallingConvention = CallingConvention.Winapi)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern bool RemoveFontMemResourceEx(IntPtr h);
#endregion

    }

}
