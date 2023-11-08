  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using OmenMon.External;
using OmenMon.Hardware.Bios;
using OmenMon.Hardware.Ec;
using OmenMon.Library;

namespace OmenMon.AppCli {

    // Implements CLI-specific functionality
    // Note: partially defined in Cli*.cs for each specific context
    public static partial class Cli {

        public static bool IsInitialized { get; private set; }
        public static bool IsPowerShell { get; private set; }

        private static ConsoleColor OriginalBackgroundColor;

        // Portable Executable (PE) Common Object File Format (COFF) header constants
        private const UInt16 IMAGE_OPTIONAL_HEADER_SUBSYSTEM = 0x00DC;
        private const byte IMAGE_SUBSYSTEM_WINDOWS_GUI = 0x02;
        private const byte IMAGE_SUBSYSTEM_WINDOWS_CUI = 0x03;

        // Setting flags for printing numerical values
        // Note: not all flag combinations are implemented for all data types
        [Flags]
        public enum ValueFlag : byte {
            Bin = 0x01,     // Show the binary value
            Dec = 0x02,     // Show the decimal value
            Hex = 0x04,     // Show the hexadecimal value
            Color = 0x08,   // Color the hexadecimal or binary value
            Prefix = 0x10   // Show numerical base prefixes (ignored for decimal)
        }

#region Color Settings
        // Console output color values
        public enum Color : int {

            ActionGet      = (int) ConsoleColor.DarkMagenta,  // Retrieval action (read)
            ActionSet      = (int) ConsoleColor.Magenta,      // Assignment action (write)
            Context        = (int) ConsoleColor.DarkCyan,     // Top-level argument header
            Deemphasis     = (int) ConsoleColor.DarkGray,     // Less important portion
            Emphasis       = (int) ConsoleColor.White,        // More important portion
            Error          = (int) ConsoleColor.Red,          // Error message
            HeaderCaption  = (int) ConsoleColor.DarkMagenta,  // Header application summary
            HeaderTitle    = (int) ConsoleColor.Blue,         // Header application name
            HeaderVersion  = (int) ConsoleColor.Magenta,      // Header application version 
            StateOff       = (int) ConsoleColor.Red,          // Disabled state
            StateOn        = (int) ConsoleColor.Green,        // Enabled state
            TableHeader    = (int) ConsoleColor.DarkMagenta,  // Table column and row headers
            Value          = (int) ConsoleColor.Blue,         // Any value except those below
            ValueBinUnset  = (int) ConsoleColor.DarkCyan,     // Unset bit in a binary value
            ValueEmpty     = (int) ConsoleColor.DarkGray,     // Value == 0x00
            ValueFull      = (int) ConsoleColor.DarkYellow,   // Value == 0xFF
            ValueSingleBit = (int) ConsoleColor.DarkRed       // PopCount(Value) == 1

        }
#endregion

#region Initialization & Termination Methods
        // Enables a Windows Forms (GUI) app
        // to work with console (with some caveats)
        public static void Initialize() {

            // Attach to console window, which may modify the standard handles
            if(!Kernel32.AttachConsole(Kernel32.ATTACH_PARENT_PROCESS))
                Kernel32.AllocConsole(); // Using an attached console
            else { // Using an existing console

                // Save the original background color and set it to black
                OriginalBackgroundColor = Console.BackgroundColor;
                Console.BackgroundColor = ConsoleColor.Black;

                // Check if we are using a PowerShell console
                if(Os.IsConsolePowerShell()) {

                    IsPowerShell = true;

                    // Basic workaround only
                    Console.Clear();

                } else {

                    // Clear the last two rows, and make sure we end up
                    // at the first column of the row before the last one
                    Console.Error.Write("\r" + new string(' ', Console.BufferWidth));
                    Console.SetCursorPosition(0, Console.CursorTop == 0 ? 0 : Console.CursorTop - 1);
                    Console.Write("\r" + new string(' ', Console.BufferWidth) + "\r");

                }
    
            }

            IsInitialized = true;
       }

        // Releases the console when no longer needed
        public static void Close() {

            // Try to move the cursor to the bottom of the window,
            // which does not happen automatically in a PowerShell session
            if(IsPowerShell)
                try {
                    Console.SetCursorPosition(0,
                        Console.WindowHeight >= Console.BufferHeight ?
                            Console.BufferHeight - 1 : Console.WindowHeight);
                } catch {
                }

            // Restore the original background color
            Console.BackgroundColor = OriginalBackgroundColor;

            IsInitialized = false;
            Kernel32.FreeConsole();

        }

        // Relaunches the process as a console application
        public static void Relaunch(string[] args) {
            byte[] data;

            // Read the image of our own process into an array
            using(FileStream dataIn = new FileStream(
                Config.AppFile,
                FileMode.Open, FileAccess.Read)) {

                data = new byte[dataIn.Length];
                dataIn.Read(data, 0, data.Length);

            }

            // Modify the PE header to run as a console application
            data[IMAGE_OPTIONAL_HEADER_SUBSYSTEM] = IMAGE_SUBSYSTEM_WINDOWS_CUI;

            // Launch ourselves again
            Assembly ass = Assembly.Load(data);
            MethodInfo m = ass.EntryPoint;
            m.Invoke(null, new[] { args });

            // Note: this is still not enough to run as a proper console application
            // Would need to launch a separate process or perhaps Assembly.LoadFile()

        }

        // Makes the command prompt reappear when the application is done in CLI mode
        public static void RestorePrompt() {

            // Skip if a PowerShell session
            if(!Os.IsConsolePowerShell()) {

                // Make the command prompt appear again
                // by simulating a keystroke (an ugly hack)
                Console.CursorTop -= 1; // Go back one row to avoid leaving blank space
                User32.SendMessage(
                    Kernel32.GetConsoleWindow(),
                    User32.WM_CHAR,
                    (IntPtr) User32.VK_ENTER,
                    IntPtr.Zero);

            }

        }
#endregion

#region Output Methods - General
        // Outputs a string in a given color, then reverts back to the original color
        public static void PrintColor(ConsoleColor color, string text) {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = originalColor;
        }

        // Outputs a formatted byte array
        public static void PrintValue(byte[] value, ValueFlag flags = ValueFlag.Hex | ValueFlag.Color, byte bits = 8) {

            // Output a hexadecimal representation
            if(flags.HasFlag(ValueFlag.Hex))
            for(int i = 1; i <= value.Length; i++) {
                if(flags.HasFlag(ValueFlag.Color))
                    PrintValueHexColor(value[i - 1], 8);
                else
                    Console.Write(Conv.GetString(value[i - 1]));
                if(i * 8 % bits == 0 && i != value.Length)
                    Console.Write(" ");
                if(i % 16 == 0 && i != value.Length)
                    Console.WriteLine();

            // Output a binary representation
            } else if(flags.HasFlag(ValueFlag.Bin))
            for(int i = 1; i <= value.Length; i++) {
                if(flags.HasFlag(ValueFlag.Color))
                    PrintValueBinColor(value[i - 1], 8);
                else
                    Console.Write(Conv.GetString(value[i - 1], 8, 2));
                if(i * 8 % bits == 0 && i != value.Length)
                    Console.Write(" ");
                if(i % 8 == 0 && i != value.Length)
                    Console.WriteLine();
            }
        }

        // Outputs a formatted hexadecimal, decimal and/or binary value
        public static void PrintValue(uint value, ValueFlag flags = ValueFlag.Hex, byte bits = 0) {

            // Set the number of bits to use when padding values
            int bytes = bits == 0 ? value > byte.MaxValue ? value > ushort.MaxValue ? 4 : 2 : 1 : bits / 8;

            // Used to separate entries in different numerical bases
            bool needSeparator = false;

            // Check if asked to output the hexadecimal value
            if(flags.HasFlag(ValueFlag.Hex)) {

                // Output the hexadecimal prefix if requested
                if(flags.HasFlag(ValueFlag.Prefix))
                    PrintColor((ConsoleColor) Color.Deemphasis, "0x");

                // Output the hexadecimal value
                if(flags.HasFlag(ValueFlag.Color))
                    PrintValueHexColor(value, bytes * 8);
                else
                    PrintColor((ConsoleColor) Color.Emphasis, Conv.GetString(value, bytes * 2, 16));

                // Request the separator before the next output (if any)
                needSeparator = true;
            }

            // Check if asked to output the binary value
            if(flags.HasFlag(ValueFlag.Bin)) {

                // Output the separator first if necessary
                PrintValueSeparator(ref needSeparator);

                // Output the binary prefix if requested
                if(flags.HasFlag(ValueFlag.Prefix))
                    PrintColor((ConsoleColor) Color.Deemphasis, "0b");

                // Output the binary value
                if(flags.HasFlag(ValueFlag.Color))
                    PrintValueBinColor(value, bytes * 8);
                else
                    PrintColor((ConsoleColor) Color.Emphasis, Conv.GetString(value, bytes * 8, 2));

                // Request the separator before the next output (if any)
                needSeparator = true;
            }

            // Check if asked to output the decimal value
            if(flags.HasFlag(ValueFlag.Dec)) {

                // Output the separator first if necessary
                PrintValueSeparator(ref needSeparator);

                // Output the decimal value
                PrintColor((ConsoleColor) Color.Emphasis, Conv.GetString(value, 0, 10));
            }

            // Prints out the separator and resets the request
            void PrintValueSeparator(ref bool needSeparator) {
                if(needSeparator) {
                    Console.Write(" = ");
                    needSeparator = false;
                }
            }

        }

        // Outputs a color-formatted binary value (without prefix)
        public static void PrintValueBinColor(uint value, int bits = 8) {

            // The color for set bits depends on whether only a single bit is set
            ConsoleColor setColor = Conv.GetBitCount(value) == 1 ?
                (ConsoleColor) Color.ValueSingleBit : (ConsoleColor) Color.Value;

            // Iterate through the string and output the color for each bit
            foreach(char c in Conv.GetString(value, bits, 2)) {
                PrintColor(c == '1' ? setColor : (ConsoleColor) Color.ValueBinUnset, c.ToString());
            }

        }
        // Outputs a color-formatted hexadecimal value (without prefix)
        public static void PrintValueHexColor(uint value, int bits = 8) {

            // Pick the color
            ConsoleColor valueColor =
                value != 0 ? // Special color for empty and full values
                (value != byte.MaxValue && value != ushort.MaxValue && value != uint.MaxValue) ? 
                Conv.GetBitCount(value) == 1 ? // Terribly inefficient
                (ConsoleColor) Color.ValueSingleBit : (ConsoleColor) Color.Value : (ConsoleColor) Color.ValueFull : (ConsoleColor) Color.ValueEmpty;

            // Output the value in color
            PrintColor(valueColor, Conv.GetString(value, bits / 4, 16));

        }
#endregion

#region Output Methods - Application-Specific
        // Outputs an action keyword, depending on the action
        public static void PrintAction(bool isSet = false) {
            if(isSet)
                PrintColor((ConsoleColor) Color.ActionSet, Config.Locale.Get(Config.L_CLI + "ActionSet"));
            else
                PrintColor((ConsoleColor) Color.ActionGet, Config.Locale.Get(Config.L_CLI + "ActionGet"));
        }

        // Outputs the context (top-level command-line argument)
        public static void PrintContext(string header, string argument = null) {
            PrintColor((ConsoleColor) Color.Context, header);
            if(argument != null) {
                Console.Write(" (" + argument + ")");
            }
            Console.WriteLine();
        }

        // Outputs an error message
        public static void PrintError(string message, Exception e = null) {
            PrintColor((ConsoleColor) Color.Error, message + Environment.NewLine);
            if(e != null) {
                Console.Error.WriteLine(
                    Environment.NewLine +
                    "{0}: {1}" + Environment.NewLine +
                    "{2}", e.Source, e.TargetSite, e.StackTrace);

            }

        }

        // Outputs explanatory information for the value when present
        public static void PrintExplanation(string explanation = null) {
            if(explanation != null) {
                Console.Write(" [");
                Console.Write(explanation);
                Console.Write("]");
            }
        }

        // Prints a formatted row of a fan table entry
        public static void PrintFanTableEntry(int number, byte temperature, byte[] level) {

            // Level number
            PrintColor((ConsoleColor) Color.Deemphasis, "# ");
            PrintColor((ConsoleColor) Color.Emphasis, Conv.GetString((uint) number, 2, 10));

            // Fan #1 speed level value
            Console.Write(": " + Enum.GetName(typeof(BiosData.FanType), 1) + " ");
            PrintColor((ConsoleColor) Color.Emphasis, Conv.GetString((uint) level[0] * 100, 4, 10));
            PrintColor((ConsoleColor) Color.Deemphasis, " " + Config.Locale.Get(Config.L_UNIT + "RotationRate"));

            // Fan #2 speed level value
            Console.Write(" " + Enum.GetName(typeof(BiosData.FanType), 2) + " ");
            PrintColor((ConsoleColor) Color.Emphasis, Conv.GetString((uint) level[1] * 100, 4, 10));
            PrintColor((ConsoleColor) Color.Deemphasis, " " + Config.Locale.Get(Config.L_UNIT + "RotationRate"));

            // Temperature value
            Console.Write(" @ ");
            PrintColor((ConsoleColor) Color.Emphasis, Conv.GetString(temperature, 2, 10));
            PrintColor((ConsoleColor) Color.Deemphasis, " " + Config.Locale.Get(Config.L_UNIT + "Temperature"));
	    
            Console.WriteLine("");

        }


        // Prints out the header in command-line mode
        public static void PrintHeader() {
            PrintColor((ConsoleColor) Color.HeaderTitle, Config.AppName);
            PrintColor((ConsoleColor) Color.HeaderCaption, " " + Config.Locale.Get(Config.L_CLI + "Header") + " ");
            PrintColor((ConsoleColor) Color.HeaderVersion, Config.Locale.Get(Config.L_CLI + "HeaderVersion") + " " + Config.AppVersion);

            // Output translation credit
            string translationCredit = Config.Locale.Get(Config.L_CLI + "Translated");
            if(translationCredit != "")
                Console.Write(Environment.NewLine + Config.Locale.Get(Config.L_CLI + "Translated"));

            Console.WriteLine();
        }

        // Outputs an action keyword, depending on the action
        public static void PrintState(bool isSet = false) {
            if(isSet)
                PrintColor((ConsoleColor) Color.StateOn, Config.Locale.Get(Config.L_CLI + "StateOn"));
            else
                PrintColor((ConsoleColor) Color.StateOff, Config.Locale.Get(Config.L_CLI + "StateOff"));
        }

        // Prints out the usage information in command-line mode
        public static void PrintUsage() {

            string data =
                "  " + Config.Locale.Get(Config.L_DATATYPE_NAME + "Byte") + ", " + Config.Locale.Get(Config.L_DATATYPE_NAME + "Reg") + ": " + Config.Locale.Get(Config.L_DATATYPE_SYNTAX + "Byte") + Environment.NewLine +
                "  " + Config.Locale.Get(Config.L_DATATYPE_NAME + "ByteArray") + ": " + Config.Locale.Get(Config.L_DATATYPE_SYNTAX + "ByteArray") + ", " +
                Config.Locale.Get(Config.L_DATATYPE_NAME + "Bool") + ": " + Config.Locale.Get(Config.L_DATATYPE_SYNTAX + "Bool") + Environment.NewLine +
                "  " + Config.Locale.Get(Config.L_DATATYPE_NAME + "Color4") + ": " + Config.Locale.Get(Config.L_DATATYPE_SYNTAX + "Color4") + Environment.NewLine +
                "  " + Config.Locale.Get(Config.L_DATATYPE_NAME + "FanLevel") + ": " + Config.Locale.Get(Config.L_DATATYPE_SYNTAX + "FanLevel") + Environment.NewLine +
                "  " + Config.Locale.Get(Config.L_DATATYPE_NAME + "FanMode") + ": " + Config.Locale.Get(Config.L_DATATYPE_SYNTAX + "FanMode") + Environment.NewLine +
                "  " + Config.Locale.Get(Config.L_DATATYPE_NAME + "FanTable") + ": " + Config.Locale.Get(Config.L_DATATYPE_SYNTAX + "FanTable") + Environment.NewLine +
                "  " + Config.Locale.Get(Config.L_DATATYPE_NAME + "GpuMode") + ": " + Config.Locale.Get(Config.L_DATATYPE_SYNTAX + "GpuMode") + Environment.NewLine +
                "  " + Config.Locale.Get(Config.L_DATATYPE_NAME + "GpuPowerLevel") + ": " + Config.Locale.Get(Config.L_DATATYPE_SYNTAX + "GpuPowerLevel") + Environment.NewLine +
                "  " + Config.Locale.Get(Config.L_DATATYPE_NAME + "TName") + ": " + Config.Locale.Get(Config.L_DATATYPE_SYNTAX + "TName") + Environment.NewLine +
                "  " + Config.Locale.Get(Config.L_DATATYPE_NAME + "Word") + ": " + Config.Locale.Get(Config.L_DATATYPE_SYNTAX + "Word") + Environment.NewLine;

            Console.WriteLine(Config.Locale.Get(Config.L_CLI + "UsageText"), Config.AppName, data);

        }
#endregion

    }

}
