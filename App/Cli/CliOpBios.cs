  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using OmenMon.Hardware.Bios;
using OmenMon.Library;

namespace OmenMon.AppCli {

    // Implements the main operation loop in the application's CLI mode
    // This part covers BIOS-specific routines
    public static partial class CliOp {

#region BIOS Information Retrieval
        // Performs a BIOS operation and outputs a Boolean result
        private static void BiosGet(string message, Func<bool> biosMethod) {
            bool value = (bool) (object) Hw.BiosGet<bool>(biosMethod);
            Cli.PrintBiosResult(false, message, value);
        }

        // Performs a BIOS operation and outputs a string result
        private static void BiosGet(string message, Func<string> biosMethod, string extra = null) {
            string value = (string) (object) Hw.BiosGet<string>(biosMethod);
            Cli.PrintBiosResult(false, message, value, extra);
        }

        // Performs a BIOS operation and outputs a numeric (possibly an enumerated) result
        private static void BiosGet<TResult>(string message, Func<TResult> biosMethod, string extra = null) {
            byte value = (byte) (object) Hw.BiosGet<TResult>(biosMethod);
            Cli.PrintBiosResult(false, message, value, typeof(TResult).IsEnum ? Enum.GetName(typeof(TResult), value) : extra);
        }

        // Queries the BIOS for the fan level of each fan, outputs and interprets the response
        private static void BiosGetFanLevel<TFanType>(string message, Func<byte[]> biosMethod) {
            byte[] data = Hw.BiosGet(biosMethod);
            Cli.PrintBiosResult(false, String.Format(message, 1), data[0], Enum.GetName(typeof(TFanType), 1));
            Cli.PrintBiosResult(false, String.Format(message, 2), data[1], Enum.GetName(typeof(TFanType), 2));
        }

        // Queries the BIOS for the fan type, outputs and interprets the response
        private static void BiosGetFanType(string messageCommon, string messageEach, Func<byte> biosMethod) {
            byte value = Hw.BiosGet(biosMethod);
            Cli.PrintBiosResult(false, messageCommon, value, Config.Locale.Get(Config.L_CLI + "DetailsFollow"));
            Cli.PrintBiosResult(false, String.Format(messageEach, 1), (byte) (value & 0x0F), Enum.GetName(typeof(BiosData.FanType), value & 0x0F));
            Cli.PrintBiosResult(false, String.Format(messageEach, 2), (byte) (value >> 4), Enum.GetName(typeof(BiosData.FanType), value >> 4));
        }

        // Performs a BIOS operation and outputs a struct result
        private static void BiosGetStruct<TResult>(string message, Func<TResult> biosMethod, string extra = null) where TResult : struct {
            dynamic data = Hw.BiosGetStruct<TResult>(biosMethod);
            Cli.PrintBiosResult(false, message, data);
        }
#endregion

#region BIOS Assignment Operations
        // Sets a BIOS toggle to a Boolean value parsed from the parameter
        private static void BiosSet(string message, string param, Action<bool> biosMethod) {
            bool value;
            // Try to parse as a numerical value
            if(!Conv.GetBool(param, out value))
                // If the value could not be parsed, error out
                App.Error("ErrNeedValueBool|DataSyntaxBool");
            else {
                // Output the new setting and send it to the BIOS
                Cli.PrintBiosResult(true, message, value);
                Hw.BiosSet(biosMethod, value);
            }
        }

        // Sets a BIOS setting to a numerical or enumerated value parsed from the parameter
        private static void BiosSet<T>(string message, string param, Action<T> biosMethod, string extra) {
            bool valueSet = false;
            byte value = 0xFF; // Cannot leave unassigned

            // Only if the data type is an Enum
            if(typeof(T).IsEnum) try {
                // Try to parse the value as a string identifier first
                // This may easily fail if there is no such identifier
                value = (byte) (object) (T) Enum.Parse(typeof(T), param);
                valueSet = true;
            } catch { }

            // Otherwise, try to parse as a numerical value
            if(!valueSet && !Conv.GetByte(param, out value))
                // If the value could not be parsed as an identifier or a number, error out
                // The 4th parameter is a custom error identifier for an Enum data type
                App.Error(typeof(T).IsEnum ? extra : "ErrNeedValueByte|DataSyntaxByte");
            else {
                // Output the new setting and send it to the BIOS
                // The 4th paramter is an explanation for a non-Enum data type
                Cli.PrintBiosResult(true, message, value, typeof(T).IsEnum ? Enum.GetName(typeof(T), value) : extra);
                Hw.BiosSet<T>(biosMethod, (T) (object) value);
            }
        }

        // Sets the BIOS fan speed level for each fan based on the values parsed from the command line
        private static void BiosSetFanLevel<TFanType>(string message, string param, Action<byte[]> biosMethod) {
            try {
                // Split the parameter into per-fan values
                string[] level = param.Split(',');

                // Parse the per-fan values
                byte[] levelValue = new byte[2];
                Conv.GetByte(level[0], out levelValue[0]);
                Conv.GetByte(level[1], out levelValue[1]);

                // Output the updated fan levels and send them to the BIOS
                Cli.PrintBiosResult(true, String.Format(message, 1), levelValue[0], Enum.GetName(typeof(TFanType), 1));
                Cli.PrintBiosResult(true, String.Format(message, 2), levelValue[1], Enum.GetName(typeof(TFanType), 2));
                Hw.BiosSet(biosMethod, levelValue);
            } catch {
                App.Error("ErrNeedValueFanLevel|DataSyntaxFanLevel");
            }
        }

        // Sets the BIOS LED animation table based on the values parsed from the command line
        private static void BiosSetStruct(string message, string param, Action<BiosData.AnimTable> biosMethod) {
            try {
                // Parse the result
                byte[] result = Conv.GetByteArray(param);

                // Size is constrained to 128 bytes
                if(result.Length > 128) throw new ArgumentOutOfRangeException();

                // Set up a new LED animation table
                BiosData.AnimTable AnimTable = new BiosData.AnimTable(result);

                // Parse the raw data input
                Cli.PrintBiosResult(true, message, AnimTable);

                // Send the new fan table to the BIOS
                Hw.BiosSetStruct(biosMethod, AnimTable);
            } catch {
                App.Error("ErrNeedValueByteArray|DataSyntaxByteArray");
            }
        }

        // Sets the BIOS keyboard backlight color table based on the values parsed from the command line
        private static void BiosSetStruct(string message, string param, Action<BiosData.ColorTable> biosMethod) {
            BiosData.ColorTable colorTable;

            // Try to parse the value as a preset name first,
            // optionally replacing spaces with underscores, if any
            // If no preset by such name, try to parse as a color array
            try {

                // Initialize a new color table
                if(Config.ColorPreset.ContainsKey(param))
                    colorTable = Config.ColorPreset[param];
                else if(param.Contains(" ") && Config.ColorPreset.ContainsKey(param.Replace('_', ' ')))
                    colorTable = Config.ColorPreset[param.Replace('_', ' ')];
                else
                    colorTable = new BiosData.ColorTable(param);

                // Output the updated color table
                Cli.PrintBiosResult(true, message, colorTable);

                // Send the updated color table to the BIOS
                Hw.BiosSetStruct(biosMethod, colorTable);
            } catch {
                App.Error("ErrNeedValueColor4|DataSyntaxColor4");
            }

        }

        // Sets the BIOS fan table based on the values parsed from the command line
        private static void BiosSetStruct(string message, string param, Action<BiosData.FanTable> biosMethod) {
            // Set up a new fan table
            BiosData.FanTable fanTable = new BiosData.FanTable();
            int level = 0;
            try {
                // Parse the value for each entry
                foreach(string levelEntry in param.Split(':')) {

                    // Parse the per-fan level and temperature within each entry
                    string[] levelArray = levelEntry.Split(',');
                    byte[] levelValues = new byte[3];
                    Conv.GetByte(levelArray[0], out levelValues[0]);
                    Conv.GetByte(levelArray[1], out levelValues[1]);
                    Conv.GetByte(levelArray[2], out levelValues[2]);

                    // Populate the new fan table
                    fanTable.Level[level] = new BiosData.FanLevel(levelValues);
                    level++;
                }
                // Set the number of entries and print out the new fan table
                fanTable.LevelCount = (byte) level;
                Cli.PrintBiosResult(true, message, fanTable);

                // A table of more than 14 entries is not supported
                if(level > 14) throw new ArgumentOutOfRangeException();

                // Send the new fan table to the BIOS
                Hw.BiosSetStruct(biosMethod, fanTable);
            } catch {
                App.Error("ErrNeedValueFanTable|DataSyntaxFanTable");
            }
        }

        // Sets the BIOS GPU power settings based on the values parsed from the command line
        private static void BiosSetStruct(string message, string param, Action<BiosData.GpuPowerData> biosMethod) {
            bool valueSet = false;
            BiosData.GpuPowerData value = new BiosData.GpuPowerData();
            switch(param.ToLower()) {
                // Possible true values
                case "max":
                case "maximum":
                    value = new BiosData.GpuPowerData(BiosData.GpuPowerLevel.Maximum);
                    valueSet = true;
                    break;

                case "med":
                case "medium":
                case "mid":
                case "middle":
                    value = new BiosData.GpuPowerData(BiosData.GpuPowerLevel.Medium);
                    valueSet = true;
                    break;

                case "default":
                case "min":
                case "minimum":
                    value = new BiosData.GpuPowerData(BiosData.GpuPowerLevel.Minimum);
                    valueSet = true;
                    break;

                default:
                    App.Error("ErrNeedValueGpuPowerLevel|DataSyntaxGpuPowerLevel");
                    break;

            }
            if(valueSet) {
                Cli.PrintBiosResult(true, message, value);
                Hw.BiosSetStruct(biosMethod, value);
                }

        }
#endregion

    }

}
