  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using OmenMon.Hardware.Bios;
using OmenMon.Hardware.Platform;
using OmenMon.Library;

namespace OmenMon.AppCli {

    // Implements CLI-specific functionality
    // This part covers fan control program-specific routines
    public static partial class Cli {

#region Output Methods - Context: Program
        // Outputs a fan control program
        public static void PrintProgList(string name, bool isSet = false) {

            // Action
            PrintAction(isSet);

            // Program name
            Console.Write(" " + Config.Locale.Get(Config.L_CLI_PROG + "Name") + " ");
            PrintColor((ConsoleColor) Color.Emphasis, Config.FanProgram[name].Name);

            // Fan mode
            Console.Write(" (" + Config.Locale.Get(Config.L_CLI_PROG + "FanMode") + " ");
            PrintColor((ConsoleColor) Color.Emphasis, Enum.GetName(typeof(BiosData.FanMode), Config.FanProgram[name].FanMode));

            // GPU power
            Console.Write(", " + Config.Locale.Get(Config.L_CLI_PROG + "GpuPower") + " ");
            PrintColor((ConsoleColor) Color.Emphasis, Enum.GetName(typeof(BiosData.GpuPowerLevel), Config.FanProgram[name].GpuPower));
            Console.Write(")");

            // Explanatory note
            PrintExplanation(Config.Locale.Get(Config.L_CLI + "DetailsFollow"));
            Console.WriteLine();

            // Level entries
            int i = 0;
            foreach(byte temperature in Config.FanProgram[name].Level.Keys)

                // Print a formatted row for each entry
                Cli.PrintFanTableEntry(++i, temperature,
                    Config.FanProgram[name].Level[temperature]);

        }

        // Outputs a status message received from a running fan control program
        public static void PrintProgMessage(FanProgram.Severity severity, string message) {

            // If the loop has been terminated already
            // do not print anything anymore
            if(CliOp.IsStop)
                return;

            // Action
            PrintAction(false);

            // Program name
            Console.Write(" " + Config.Locale.Get(Config.L_CLI_PROG + "Callback") + ": ");

            // Message
            PrintColor((ConsoleColor) Color.Emphasis, message);

            // Timestamp
            PrintColor((ConsoleColor) Color.Deemphasis,
                " [" + Enum.GetName(typeof(FanProgram.Severity), severity)
                + "] @ " + DateTime.Now.ToString(Config.TimestampFormat));

            Console.WriteLine();

        }
#endregion

    }

}
