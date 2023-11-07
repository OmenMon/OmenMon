  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace OmenMon.Library {

    // Implements frequently-reused conversion and evaluation methods
    public static class Conv {

#region Data
        // Formatting sequences for rich-text fields
        public const string RTF_CF1 = "\\cf1 ";             // Font 1
        public const string RTF_CF2 = "\\cf2 ";             // Font 2
        public const string RTF_CF3 = "\\cf3 ";             // Font 3
        public const string RTF_CF4 = "\\cf4 ";             // Font 4
        public const string RTF_CF5 = "\\cf5 ";             // Font 5
        public const string RTF_CF6 = "\\cf6 ";             // Font 6
        public const string RTF_LINE = "\\line ";           // New line
        public const string RTF_STRIKE0 = "\\strike0 ";     // Strikethrough end
        public const string RTF_STRIKE1 = "\\strike ";      // Strikethrough begin
        public const string RTF_SUB1 = "\\sub ";            // Subscript begin
        public const string RTF_SUBSUP0 = "\\nosupersub ";  // Subscript and superscript end

        // Special Unicode characters
        public enum SpecialChar : uint {

            ArrowDown            = 0x2193,   // Not in the custom font
            Asterisk6            = 0x1f7b7,
            Asterisk8            = 0x2733,
            Asterisk8Medium      = 0x1f7bc,
            AsteriskLow          = 0x204e,
            Bullet               = 0x2022,
            BulletOp             = 0x2219,
            Degree               = 0x00b0,
            DegreeCelsius        = 0x2103,
            DiamondEmpty         = 0x25c7,
            DiamondHalfBottom    = 0x2b19,
            DiamondHalfLeft      = 0x2b16,
            DiamondHalfRight     = 0x2b17,
            DiamondHalfTop       = 0x2b18,
            DiamondMedium        = 0x2b25,
            DiamondMediumEmpty   = 0x2b26,
            DiamondOp            = 0x22c4,
            Division             = 0x00f7,
            DotOp                = 0x22c5,
            FastForwardRight     = 0x23e9,
            FastForwardUp        = 0x23eb,
            FourVerticalDots     = 0x205e,
            GuillemotDoubleLeft  = 0x00ab,
            GuillemotDoubleRight = 0x00bb,
            GuillemotSingleLeft  = 0x2039,
            GuillemotSingleRight = 0x203a,
            HeavyCheckmark       = 0x2714,   // Not in the custom font
            HeavyMultiplication  = 0x2716,   // Not in the custom font
            LeftToRight          = 0x200E,   // Not in the custom font
            Middot               = 0x00b7,
            Multiplication       = 0x00d7,
            Pause                = 0x23f8,
            PlayPause            = 0x23ef,
            PlusMinus            = 0x00b1,
            PowerOff             = 0x2b58,
            PowerOn              = 0x23fd,
            PowerOnOff           = 0x23fb,
            Prime1               = 0x2032,
            Prime2               = 0x2033,
            Record               = 0x23fa,
            RewindDown           = 0x23ec,
            RewindLeft           = 0x23ea,
            Ring                 = 0x2218,
            Saltire              = 0x1f7ab,
            SpaceEm              = 0x2003,
            SpaceEn              = 0x2002,
            SpacePerEm3          = 0x2004,
            SpacePerEm4          = 0x2005,
            SpacePerEm6          = 0x2006,
            Stop                 = 0x23f9,
            Sub0                 = 0x2080,
            Sub1                 = 0x2081,
            Sub2                 = 0x2082,
            Sub3                 = 0x2083,
            Sub4                 = 0x2084,
            Sub5                 = 0x2085,
            Sub6                 = 0x2086,
            Sub7                 = 0x2087,
            Sub8                 = 0x2088,
            Sub9                 = 0x2089,
            SubMinus             = 0x208b,
            SubPlus              = 0x208a,
            Sup0                 = 0x2070,
            Sup1                 = 0x00b9,
            Sup2                 = 0x00b2,
            Sup3                 = 0x00b3,
            Sup4                 = 0x2074,
            Sup5                 = 0x2075,
            Sup6                 = 0x2076,
            Sup7                 = 0x2077,
            Sup8                 = 0x2078,
            Sup9                 = 0x2079,
            SupMinus             = 0x207b,
            SupPlus              = 0x207a,
            TrackNext            = 0x23ed,
            TrackPrev            = 0x23ee,
            TriangleDown         = 0x23f7,
            TriangleNext         = 0x23f5,
            TrianglePrev         = 0x23f4,
            TriangleUp           = 0x23f6,
            Tricolon             = 0x205d

        }
#endregion

#region Conversion & Evaluation Methods
        // Checks if two arrays are equal
        public static bool ArraysEqual<T>(T[] a1, T[] a2) {

            // Same if both reference the same object
            if(ReferenceEquals(a1, a2))
                return true;

            // Different if either is null
            if(a1 == null || a2 == null)
                return false;

            // Different if not the same length
            if(a1.Length != a2.Length)
                return false;

            // Otherwise, compare each element
            EqualityComparer<T> c = EqualityComparer<T>.Default;
            for(int i = 0; i < a1.Length; i++)
                if(!c.Equals(a1[i], a2[i]))

                    // Bail out at first difference
                    return false;

            // No differences
            return true;

        }

        // Checks if a given bit is set in a byte
        public static bool GetBit(byte b, int bit) {
           return ((b >> bit) & 1U) != 0;
        }

        // Counts and returns the number of bits in a value
        public static byte GetBitCount(ulong value) {
            ulong result = value - ((value >> 1) & 0x5555555555555555UL);
            result = (result & 0x3333333333333333UL) + ((result >> 2) & 0x3333333333333333UL);
            return (byte)(unchecked(((result + (result >> 4)) & 0xF0F0F0F0F0F0F0FUL) * 0x101010101010101UL) >> 56);
        }

        // Converts a Boolean string representation to a Boolean value
        public static bool GetBool(string arg, out bool value) {
            value = false;
            switch(arg.ToLower()) {

                // Possible true values
                case "1":
                case "on":
                case "true":
                case "yes":
                    value = true;
                    return true;

                // Possible false values
                case "0":
                case "off":
                case "false":
                case "no":
                    return true;

                // Anything other than that?
                default: // Bail out and fail
                    return false;

            }
        }

        // Converts a binary, decimal or hexadecimal string argument to a byte value and returns it
        public static byte GetByte(string arg) {
            byte value = 0;
            GetByte(arg, out value);
            return value;
        }

        // Converts a binary, decimal or hexadecimal string argument to a byte value and returns status
        public static bool GetByte(string arg, out byte value) {
            value = 0;
            try {
                if(arg.StartsWith("0x")) // Hexadecimal input
                   value = Convert.ToByte(arg.Substring(2), 16);
                else if(arg.StartsWith("0b")) // Binary input
                   value = Convert.ToByte(arg.Substring(2), 2);
                else // Try decimal input
                   value = Convert.ToByte(arg, 10);
                return true;
            } catch {
                return false;
                }
        }

        // Serializes an object into a byte array
        public static byte[] GetByteArray(object data, int length = 0) {

            // Retrieve the size of an object
            int bufferSize = Marshal.SizeOf(data);

            // Check if set to only copy a part of it
            int copySize = length > 0 ? length < bufferSize ? length : bufferSize : bufferSize;

            // Allocate memory equivalent to the size
            byte[] result = new byte[copySize];
            IntPtr pointer = Marshal.AllocHGlobal(bufferSize);

            // Convert the object
            Marshal.StructureToPtr(data, pointer, true);
            Marshal.Copy(pointer, result, 0, copySize);

            // Release the allocated memory
            Marshal.FreeHGlobal(pointer);
            return result;
        }

        // Transforms a string containing a hexadecimal
        // representation of binary data into a byte array
        public static byte[] GetByteArray(string text) {
            byte[] result = new byte[text.Length / 2];
            int i = -1;

            // Iterate through the string and evaluate each nibble (half-byte)
            while(2 * ++i < text.Length)
                result[i] = (byte)
                    (GetNibble((byte) text[2 * i]) << 4 // High nibble
                    | GetNibble((byte) text[(2 * i) + 1])); // Low nibble
            return result;

            // Converts a character to its nibble value
            byte GetNibble(int value) {

                value -= 0x30; // 0x30 to 0x39
                if((byte) value <= 9) // '0' to '9'
                    return (byte) value;

                value -= 0x11; // 0x41 to 0x46
                if((byte) value <= 5) // 'A' to 'F'
                    return (byte) (value + 0x0A);

                value -= 0x20; // 0x61 to 0x66
                if((byte) value <= 5) // 'a' to 'f'
                    return (byte) (value + 0x0A);

                // Illegal character encountered
                // Superfluous character check not implemented
                return 0xFF;
            }
        }

        // Retrieves a Unicode special character given its enumerated name
        public static string GetChar(SpecialChar charId) {
            return System.Char.ConvertFromUtf32((int) charId);
        }

        // Retrieves a Unicode special character given its sequential number
        public static string GetChar(int charId) {
            return System.Char.ConvertFromUtf32(charId);
        }

        // Retrieves a color component (red, green or blue) from a color
        public static uint GetColorComponent(int color, int part) {
            // Ugly but faster than solving the general case:
            // return (color & (0x100 ** (part + 1) - 1)) >> 8 * part;
            if(part == 0)
                return (uint) color & 0x000000FF;
            else if(part == 1)
                return ((uint) color & 0x0000FF00) >> 8;
            else if(part == 2)
                return ((uint) color & 0x00FF0000) >> 16;
            else
                return ((uint) color & 0xFF000000) >> 24;
        }

        // Adds maximum opacity to an ARGB or ABGR color value
        public static int GetColorMaxAlpha(int color) {
            return color | unchecked((int) 0xFF000000);
        }

        // Removes opacity from an ARGB or ABGR color value
        public static int GetColorNoAlpha(int color) {
            return color & unchecked((int) 0x00FFFFFF);
        }

        // Converts between RGB and BGR color values
        public static int GetColorReverse(int color) {
            return (color & 0x00FF00) | ((color & 0xFF0000) >> 16) | ((color & 0x0000FF) << 16);
        }

        // Gets the color value as a hexadecimal string, opacity is discarded
        public static string GetColorString(int color) {
            return Conv.GetString((uint) Conv.GetColorNoAlpha(color), 6, 16);
        }

        // Gets the color value as a rich-text field string, opacity is discarded
        public static string GetColorStringRtf(int color) {
            return "\\red" + GetString((uint) GetColorComponent(color, 2), 1, 10)
                + "\\green" + GetString((uint) GetColorComponent(color, 1), 1, 10)
                + "\\blue" + GetString((uint) GetColorComponent(color, 0), 1, 10) + ";";
        }

        // Returns the parameter unless out of bounds, in which case returns the bound
        public static int GetConstrained(int value, int min, int max) {
            return value > min ?
                value < max ?
                   value : max : min;
        }

        // Converts a byte array to a hexadecimal string representation
        public static string GetString(byte[] value) {
            return BitConverter.ToString(value).Replace("-", "").ToLower();
        }

        // Converts a byte or word to a string, using the given numerical base and alignment
        public static string GetString(uint value, int padding = 2, int nbase = 16) {
            return Convert.ToString(value, nbase).PadLeft(padding, '0');
        }

        // Converts a binary, decimal or hexadecimal string argument to a word value
        public static bool GetWord(string arg, out ushort value) {
            value = 0;
            try {
                if(arg.StartsWith("0x")) // Hexadecimal input
                   value = Convert.ToUInt16(arg.Substring(2), 16);
                else if(arg.StartsWith("0b")) // Binary input
                   value = Convert.ToUInt16(arg.Substring(2), 2);
                else // Try decimal input
                   value = Convert.ToUInt16(arg, 10);
                return true;
            } catch {
                return false;
                }
        }
#endregion

    }

}
