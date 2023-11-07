  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using OmenMon.Hardware.Bios;
using OmenMon.Library;

namespace OmenMon.AppCli {

    // Implements CLI-specific functionality
    // This part covers BIOS-specific routines
    public static partial class Cli {

#region Output Methods - Context: BIOS
        // Outputs the prompt of a BIOS operation
        public static void PrintBiosPrompt(bool isActionSet, string message) {

            // Action name
            PrintAction(isActionSet);
            Console.Write(" ");

            // Descriptive message
            Console.Write(message);
            Console.Write(": ");

        }

        // Outputs the result of a BIOS operation with a Boolean state
        public static void PrintBiosResult(bool isActionSet, string message, bool state) {

            // Prompt
            PrintBiosPrompt(isActionSet, message);

            // State
            PrintState((bool) state);

            Console.WriteLine();
        }

        // Outputs the result of a BIOS operation with a numerical value and an optional explanation
        public static void PrintBiosResult(bool isActionSet, string message, uint value, string explanation = null, byte bits = 0) {

            // Prompt
            PrintBiosPrompt(isActionSet, message);

            // Numerical value
            PrintValue(value, ValueFlag.Color | ValueFlag.Hex | ValueFlag.Bin | ValueFlag.Dec | ValueFlag.Prefix, bits);

            // Explanation
            PrintExplanation(explanation);
            Console.WriteLine();

        }

        // Outputs the result of a BIOS operation with a string value
        public static void PrintBiosResult(bool isActionSet, string message, string value, string explanation = null) {

            // Prompt
            PrintBiosPrompt(isActionSet, message);

            // Text value
            PrintColor((ConsoleColor) Color.Emphasis, value);

            // Explanation
            PrintExplanation(explanation);

            Console.WriteLine();

        }

        // Outputs the result of a BIOS operation with a BIOS LED animation table
        public static void PrintBiosResult(bool isActionSet, string message, BiosData.AnimTable data) {

            // Prompt
            PrintBiosPrompt(isActionSet, message);
            Console.WriteLine("");

            // Raw data
            PrintValue(Conv.GetByteArray(data));
            Console.WriteLine("");

        }

        // Outputs the result of a BIOS operation with a BIOS keyboard backlight color table
        public static void PrintBiosResult(bool isActionSet, string message, BiosData.ColorTable data) {

            // Prompt
            PrintBiosPrompt(isActionSet, message);

            // Zone count
            PrintColor((ConsoleColor) Color.Emphasis, Conv.GetString((uint) (data.ZoneCount + 1), 0, 10));
            Console.Write(" " + Config.Locale.Get(Config.L_CLI_BIOS + "ColorZones") + " - ");

            // Zones
            for(int i = 0; i <= data.ZoneCount; i++) {

                // Zone name
                PrintColor((ConsoleColor) Color.Emphasis, Enum.GetName(typeof(BiosData.KbdZone), i));
                Console.Write(": ");

                // Color values
                PrintColor(ConsoleColor.Red, Conv.GetString(data.Zone[i].Red));
                PrintColor(ConsoleColor.Green, Conv.GetString(data.Zone[i].Green));
                PrintColor(ConsoleColor.Blue, Conv.GetString(data.Zone[i].Blue));
                Console.Write(i == data.ZoneCount ? "" : ", ");

            }

            // Also show preset if exists
            foreach(string key in Config.ColorPreset.Keys) {

                // Compare each to the current colors
                if(data.Zone[0].Value == Config.ColorPreset[key].Zone[0].Value
                    && data.Zone[1].Value == Config.ColorPreset[key].Zone[1].Value
                    && data.Zone[2].Value == Config.ColorPreset[key].Zone[2].Value
                    && data.Zone[3].Value == Config.ColorPreset[key].Zone[3].Value)

                    // Output the preset name if matches
                    PrintExplanation(key); 

            }

            Console.WriteLine();

        }

        // Outputs the result of a BIOS operation with a fan speed level table
        public static void PrintBiosResult(bool isActionSet, string message, BiosData.FanTable data) {

            // Prompt
            PrintBiosPrompt(isActionSet, message);

            // Entry count
            PrintColor((ConsoleColor) Color.Emphasis, Conv.GetString((uint) (data.FanCount), 0, 10));
            Console.Write(" " + Config.Locale.Get(Config.L_CLI_BIOS + "FanTableFans") + ", ");
            PrintColor((ConsoleColor) Color.Emphasis, Conv.GetString((uint) (data.LevelCount), 0, 10));
            Console.WriteLine(" " + Config.Locale.Get(Config.L_CLI_BIOS + "FanTableLevels"));

            // Entries
            for(int i = 0; i < data.LevelCount; i++)
	    
                // Print a formatted row for each entry
                Cli.PrintFanTableEntry(i, data.Level[i].Temperature,
                    new byte[] { data.Level[i].Fan1Level, data.Level[i].Fan2Level });

        }

        // Outputs the result of a BIOS operation with a GPU power data table
        public static void PrintBiosResult(bool isActionSet, string message, BiosData.GpuPowerData data) {

            // No prompt

            // GPU setting details
            PrintBiosResult(isActionSet, Config.Locale.Get(Config.L_CLI_BIOS + "GpuPowerCustomTgp"),
                (byte) data.CustomTgp, Enum.GetName(typeof(BiosData.GpuCustomTgp), data.CustomTgp));

            PrintBiosResult(isActionSet, Config.Locale.Get(Config.L_CLI_BIOS + "GpuPowerPpab"),
                (byte) data.Ppab, Enum.GetName(typeof(BiosData.GpuPpab), data.Ppab));

            PrintBiosResult(isActionSet, Config.Locale.Get(Config.L_CLI_BIOS + "GpuPowerDState"),
                (byte) data.DState, Enum.GetName(typeof(BiosData.GpuDState), data.DState));

            PrintBiosResult(isActionSet, Config.Locale.Get(Config.L_CLI_BIOS + "GpuPowerPeakTemperature"),
                (byte) data.PeakTemperature, Config.Locale.Get(Config.L_UNIT + "Temperature"));

        }

        // Outputs the result of a BIOS operation with a system design data table
        public static void PrintBiosResult(bool isActionSet, string message, BiosData.SystemData data) {

            // Prompt
            PrintBiosPrompt(isActionSet, message);

            // Raw data
            PrintValue(Conv.GetByteArray(data, 16));
            PrintExplanation(Config.Locale.Get(Config.L_CLI + "DetailsFollow"));
            Console.WriteLine();

            // Detailed analysis	    
            PrintBiosResult(false, Config.Locale.Get(Config.L_CLI_BIOS + "SystemStatusFlags"),
                (ushort) data.StatusFlags, data.StatusFlags.ToString(), 16);

            PrintBiosResult(false, Config.Locale.Get(Config.L_CLI_BIOS + "SystemUnknown2"),
                data.Unknown2, Config.Locale.Get(Config.L_CLI_BIOS + "SystemUnknown2Note"));

            PrintBiosResult(false, Config.Locale.Get(Config.L_CLI_BIOS + "SystemThermalPolicyVersion"),
                (byte) data.ThermalPolicy, Enum.GetName(typeof(BiosData.ThermalPolicyVersion), data.ThermalPolicy));

            PrintBiosResult(false, Config.Locale.Get(Config.L_CLI_BIOS + "SystemSupportFlags"),
                (byte) data.SupportFlags, data.SupportFlags.ToString());

            PrintBiosResult(false, Config.Locale.Get(Config.L_CLI_BIOS + "SystemDefaultCpuPowerLimit4"),
                data.DefaultCpuPowerLimit4, Config.Locale.Get(Config.L_UNIT + "Power"));

            PrintBiosResult(false, Config.Locale.Get(Config.L_CLI_BIOS + "SystemBiosOc"),
                (byte) data.BiosOc, Enum.GetName(typeof(BiosData.SysBiosOc), data.BiosOc));

            PrintBiosResult(false, Config.Locale.Get(Config.L_CLI_BIOS + "SystemGpuModeSwitch"),
                (byte) data.GpuModeSwitch, data.GpuModeSwitch.ToString());

            PrintBiosResult(false, Config.Locale.Get(Config.L_CLI_BIOS + "SystemDefaultCpuPowerLimitWithGpu"),
                data.DefaultCpuPowerLimitWithGpu, Config.Locale.Get(Config.L_UNIT + "Power") + "] [" + Config.Locale.Get(Config.L_CLI_BIOS + "SystemDefaultCpuPowerLimitWithGpuNote"));

        }
#endregion

    }

}
