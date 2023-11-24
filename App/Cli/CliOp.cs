  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using OmenMon.AppCli;
using OmenMon.Hardware.Bios;
using OmenMon.Hardware.Ec;
using OmenMon.Hardware.Platform;
using OmenMon.Library;

namespace OmenMon.AppCli {

    // Implements the main operation loop in the application's CLI mode
    // Note: operations in each specific context implemented in CliOp*.cs
    public static partial class CliOp {

        // State flag used to break out of an infinite loop
        internal static volatile bool IsStop;

        // Processes the command-line operations
        public static void Loop(string[] args) {

            // Iterate through the command-line arguments
            // selecting the context first
            int i = -1;
            while(++i < args.Length)
            switch(args[i].ToLower()) {

                // Perform BIOS actions
                case "-bios":

                    // Initialize the BIOS
                    Hw.BiosInit();

                    // If no argument, since this was the last one already
                    // or next argument indicates another context
                    if(++i == args.Length || args[i].StartsWith("-")) {

                        // Run all the operations that do not make any changes
                        Loop(new string[] {
                            "-Bios",
                            "BornDate", "System", "Gpu", "GpuMode", "Adapter",
                            "HasOverclock", "HasMemoryOverclock", "HasUndervolt",
                            "KbdType", "HasBacklight", "Backlight", "Color", "Anim",
                            "FanCount", "FanMax", "FanType", "FanLevel", "FanTable", "Temp", "Throttling"});

                    } else {

                        // Output the context header only now, otherwise it would appear twice
                        Cli.PrintContext(Config.Locale.Get(Config.L_CLI_BIOS), "-Bios");

                        do {

                            // An assignment operator appears as part of the argument
                            // We're being asked to actually change some settings
                            if(args[i].Contains("=")) {

#region BIOS Assignment Operations
                                // Split the argument into pieces
                                // so that each part can be evaluated independently
                                string[] opArgs = args[i].Split('=');

                                // Select the BIOS operation to be performed
                                // from among those that require arguments
                                switch(opArgs[0].ToLower()) {

                                    // Rewrite the LED animation table with values parsed from the command line
                                    case "anim":
                                        BiosSetStruct(Config.Locale.Get(Config.L_CLI_BIOS + "Anim"), opArgs[1], Hw.Bios.SetAnimTable);
                                        break;

                                    // Toggle keyboard backlight on or off
                                    case "backlight":
                                        BiosSet(Config.Locale.Get(Config.L_CLI_BIOS + "Backlight"), opArgs[1], Hw.Bios.SetBacklight);
                                        break;

                                    // Rewrite the keyboard backlight color table with values parsed from the command line
                                    case "color":
                                        BiosSetStruct(Config.Locale.Get(Config.L_CLI_BIOS + "Color"), opArgs[1], Hw.Bios.SetColorTable);
                                        break;

                                    // Set the CPU Power Limit 1 (PL1) to the value parsed from the command line
                                    case "cpu:pl1":
                                        BiosSet<byte>(Config.Locale.Get(Config.L_CLI_BIOS + "CpuPowerLimit1"), opArgs[1], Hw.Bios.SetCpuPower1, Config.Locale.Get(Config.L_UNIT + "Power"));
                                        break;

                                    // Set the CPU Power Limit 4 (PL4) to the value parsed from the command line
                                    case "cpu:pl4":
                                        BiosSet<byte>(Config.Locale.Get(Config.L_CLI_BIOS + "CpuPowerLimit4"), opArgs[1], Hw.Bios.SetCpuPower4, Config.Locale.Get(Config.L_UNIT + "Power"));
                                        break;

                                    // Set the CPU power limit concurrent with GPU to the value parsed from the command line
                                    case "cpu:plgpu":
                                        BiosSet<byte>(Config.Locale.Get(Config.L_CLI_BIOS + "CpuPowerLimitWithGpu"), opArgs[1], Hw.Bios.SetCpuPowerWithGpu, Config.Locale.Get(Config.L_UNIT + "Power"));
                                        break;

                                    // Set the fan speed levels to the values parsed from the command line
                                    case "fanlevel":
                                        BiosSetFanLevel<BiosData.FanType>(Config.Locale.Get(Config.L_CLI_BIOS + "FanLevelN"), opArgs[1], Hw.Bios.SetFanLevel);
                                        break;

                                    // Toggle maximum fan speed mode on or off
                                    case "fanmax":
                                        BiosSet(Config.Locale.Get(Config.L_CLI_BIOS + "FanMax"), opArgs[1], Hw.Bios.SetMaxFan);
                                        break;

                                    // Set the fan performance mode to the values parsed from the command line
                                    case "fanmode":
                                        BiosSet<BiosData.FanMode>(Config.Locale.Get(Config.L_CLI_BIOS + "FanMode"), opArgs[1], Hw.Bios.SetFanMode, "ErrNeedValueFanMode|DataSyntaxFanMode");
                                        break;

                                    // Rewrite the fan speed-level table with values parsed from the command line
                                    case "fantable":
                                        BiosSetStruct(Config.Locale.Get(Config.L_CLI_BIOS + "FanTable"), opArgs[1], Hw.Bios.SetFanTable);
                                        break;

                                    // Adjust the GPU power
                                    case "gpu":
                                        BiosSetStruct(Config.Locale.Get(Config.L_CLI_BIOS + "GpuPower"), opArgs[1], Hw.Bios.SetGpuPower);
                                        break;

                                    // Switch the graphics mode (a reboot is required)
                                    case "gpumode":
                                        BiosSet<BiosData.GpuMode>(Config.Locale.Get(Config.L_CLI_BIOS + "GpuMode"), opArgs[1], Hw.Bios.SetGpuMode, "ErrNeedValueGpuMode|DataSyntaxGpuMode");
                                        break;

                                    // Toggle idle mode on or off
                                    case "idle":
                                        BiosSet(Config.Locale.Get(Config.L_CLI_BIOS + "Idle"), opArgs[1], Hw.Bios.SetIdle);
                                        break;

                                    // Toggle between the default memory profile and XMP
                                    case "xmp":
                                        BiosSet(Config.Locale.Get(Config.L_CLI_BIOS + "Xmp"), opArgs[1], Hw.Bios.SetMemoryXmp);
                                        break;

                                    // Encountered an unrecognized argument
                                    default:
                                        App.Error("ErrArgUnknown");
                                        goto showUsage; // Interrupt both loops

                            }
#endregion

                            // No input parsing required, all we're asked to do is just
                            // to run some BIOS commands that do not take arguments
                            } else {

#region BIOS Information Retrieval
                                // Select the BIOS operation to be performed
                                // from among those that do not require arguments
                                switch(args[i].ToLower()) {

                                    // Retrieve and interpret the smart power adapter status
                                    case "adapter":
                                        BiosGet<BiosData.AdapterStatus>(Config.Locale.Get(Config.L_CLI_BIOS + "Adapter"), Hw.Bios.GetAdapter);
                                        break;

                                    // Retrieve the LED animation table
                                    case "anim":
                                        BiosGetStruct<BiosData.AnimTable>(Config.Locale.Get(Config.L_CLI_BIOS + "Anim"), Hw.Bios.GetAnimTable);
                                        break;

                                    // Retrieve the keyboard backlight status
                                    case "backlight":
                                        BiosGet<BiosData.Backlight>(Config.Locale.Get(Config.L_CLI_BIOS + "Backlight"), Hw.Bios.GetBacklight);
                                        break;

                                    // Retrieve the "Born-on Date" (BOD)
                                    case "borndate":
                                        BiosGet(Config.Locale.Get(Config.L_CLI_BIOS + "BornDate"), Hw.Bios.GetBornDate, Config.Locale.Get(Config.L_CLI_BIOS + "BornDateNote"));
                                        break;

                                    // Retrieve and interpret the keyboard backlight color table
                                    case "color":
                                        BiosGetStruct<BiosData.ColorTable>(Config.Locale.Get(Config.L_CLI_BIOS + "Color"), Hw.Bios.GetColorTable);
                                        break;

                                    // Retrieve the number of fans
                                    case "fancount":
                                        BiosGet(Config.Locale.Get(Config.L_CLI_BIOS + "FanCount"), Hw.Bios.GetFanCount);
                                        break;

                                    // Retrieve the current speed level for each fan
                                    case "fanlevel":
                                        BiosGetFanLevel<BiosData.FanType>(Config.Locale.Get(Config.L_CLI_BIOS + "FanLevelN"), Hw.Bios.GetFanLevel);
                                        break;

                                    // Check if the fan is currently operating in maximum-speed mode
                                    case "fanmax":
                                        BiosGet(Config.Locale.Get(Config.L_CLI_BIOS + "FanMax"), Hw.Bios.GetMaxFan);
                                        break;

                                    // Retrieve and interpret the fan speed-level table
                                    case "fantable":
                                        BiosGetStruct<BiosData.FanTable>(Config.Locale.Get(Config.L_CLI_BIOS + "FanTable"), Hw.Bios.GetFanTable);
                                        break;

                                    // Retrieve and interpret the fan type register
                                    case "fantype":
                                        BiosGetFanType(Config.Locale.Get(Config.L_CLI_BIOS + "FanType"), Config.Locale.Get(Config.L_CLI_BIOS + "FanTypeN"), Hw.Bios.GetFanType);
                                        break;

                                    // Retrieve and interpret the current GPU power settings
                                    case "gpu":
                                        BiosGetStruct<BiosData.GpuPowerData>(Config.Locale.Get(Config.L_CLI_BIOS + "GpuPower"), Hw.Bios.GetGpuPower);
                                        break;

                                    // Retrieve and interpret the current GPU mode
                                    case "gpumode":
                                        BiosGet<BiosData.GpuMode>(Config.Locale.Get(Config.L_CLI_BIOS + "GpuMode"), Hw.Bios.GetGpuMode);
                                        break;

                                    // Check if keyboard backlight is supported
                                    case "hasbacklight":
                                        BiosGet(Config.Locale.Get(Config.L_CLI_BIOS + "HasBacklight"), Hw.Bios.HasBacklight);
                                        break;

                                    // Check if memory overclocking is supported
                                    case "hasmemoryoverclock":
                                        BiosGet(Config.Locale.Get(Config.L_CLI_BIOS + "HasMemoryOverclock"), Hw.Bios.HasMemoryOverclock);
                                        break;

                                    // Check if overclocking is supported
                                    case "hasoverclock":
                                        BiosGet(Config.Locale.Get(Config.L_CLI_BIOS + "HasOverclock"), Hw.Bios.HasOverclock);
                                        break;

                                    // Check if BIOS supports undervolting
                                    case "hasundervolt":
                                        BiosGet(Config.Locale.Get(Config.L_CLI_BIOS + "HasUndervolt"), Hw.Bios.HasUndervoltBios);
                                        break;

                                    // Retrieve and interpret the keyboard type
                                    case "kbdtype":
                                        BiosGet<BiosData.KbdType>(Config.Locale.Get(Config.L_CLI_BIOS + "KbdType"), Hw.Bios.GetKbdType);
                                        break;

                                    // Retrieve and interpret the system design data
                                    case "system":
                                        BiosGetStruct<BiosData.SystemData>(Config.Locale.Get(Config.L_CLI_BIOS + "System"), Hw.Bios.GetSystem);
                                        break;

                                    // Retrieve the temperature sensor reading
                                    case "temp":
                                        BiosGet(Config.Locale.Get(Config.L_CLI_BIOS + "Temp"), Hw.Bios.GetTemperature, Config.Locale.Get(Config.L_UNIT + "Temperature"));
                                        break;

                                    // Check the system thermal throttling status
                                    case "throttling":
                                        BiosGet<BiosData.Throttling>(Config.Locale.Get(Config.L_CLI_BIOS + "Throttling"), Hw.Bios.GetThrottling);
                                        break;

                                    // Encountered an unrecognized argument
                                    default:
                                        App.Error("ErrArgUnknown");
                                        goto showUsage; // Interrupt both loops

                                }
#endregion

                            }

                        // The loop will continue until there are no more arguments
                        // or a context switch is encountered, but always runs at least once
                        } while(++i < args.Length && !args[i].StartsWith("-"));
                    }

                    // Point the iterator to the next context argument, if any
                    i--;
                    break;

#region Embedded Controller
                // Perform Embedded Controller actions
                case "-ec":
                    Cli.PrintContext(Config.Locale.Get(Config.L_CLI_EC), "-Ec");

                    // Initialize the Embedded Controller
                    Hw.EcInit();

                    // If no argument, since this was the last one already
                    // or next argument indicates another context
                    if(++i == args.Length || args[i].StartsWith("-"))

                        // Just output all register values in a table form
                        EcGetTable();

                    // Otherwise, loop through the arguments until there are
                    // no more of them, or a context switch is encountered
                    else do {

                        // An assignment operator appears as part of the argument
                        // We're being asked to set the value of a specific register
                        if(args[i].Contains("=")) {

                            // Split the argument into pieces
                            // so that each part can be evaluated independently
                            string[] opArgs = args[i].Split('=');

                            // Pass the argument to the method that handles the rest
                            EcSet(opArgs[0], opArgs[1]);

                        // No assignment will be happening, all we're asked to do
                        // is just to retrieve the value of a specific register
                        } else {

                            // Pass the argument to the method that handles the rest
                            EcGet(args[i]);

                        }

                    // The loop will continue until there are no more arguments
                    // or a context switch is encountered but always runs at least once
                    } while(++i < args.Length && !args[i].StartsWith("-"));

                    // Point the iterator to the next context argument, if any
                    i--;
                    break;

                // Start the Embedded Controller monitor
                case "-ecmon":
                    Cli.PrintContext(Config.Locale.Get(Config.L_CLI_EC + "Mon"), "-EcMon");

                    // Initialize the Embedded Controller
                    Hw.EcInit();

                    // Check if the next argument might be a filename to save to
                    string filename = null;
                    if(++i < args.Length && !args[i].StartsWith("-"))
                        filename = args[i];

                    // If the next argument was not a filename
                    else // Point the iterator back towards it
                        i--;

                    EcMon(filename);
                    break;
#endregion

#region Fan Control Program
                // Perform program actions
                case "-prog":
                    Cli.PrintContext(Config.Locale.Get(Config.L_CLI_PROG), "-Prog");

                    // If no argument, since this was the last one already
                    // or next argument indicates another context
                    if(++i == args.Length || args[i].StartsWith("-"))

                        // List all available fan control programs
                        ProgList();

                    // Otherwise, if an argument was given
                    else {

                        // Run the specified program
                        ProgRun(args[i++]);

                    }

                    // Point the iterator to the next context argument, if any
                    --i;
                    break;
#endregion

                // Perform task management actions
                case "-task":

                    // If no argument, since this was the last one already
                    // or next argument indicates another context
                    if(++i == args.Length || args[i].StartsWith("-")) {

                        // Check the status of all tasks
                        Loop(new string[] {
                            "-Task",
                            "Gui", "Key", "Mux"});

                    } else {

                        // Output the context header only now, otherwise it would appear twice
                        Cli.PrintContext(Config.Locale.Get(Config.L_CLI_TASK), "-Task");

                        do {

                            // An assignment operator appears as part of the argument
                            // We're being asked to actually change some settings
                            if(args[i].Contains("=")) {

#region Task Management
                                // Split the argument into pieces
                                // so that each part can be evaluated independently
                                string[] opArgs = args[i].Split('=');

                                // Select the task operation to be performed
                                // from among those that require arguments
                                switch(opArgs[0].ToLower()) {

                                    // Enable or disable the GUI autorun
                                    case "gui":
                                        TaskSet(Config.TaskId.Gui, opArgs[1]);
                                        break;

                                    // Enable or disable the Omen Key task
                                    case "key":
                                        TaskSet(Config.TaskId.Key, opArgs[1]);
                                        break;

                                    // Enable or disable the nVidia Advanced Optimus bug fix task
                                    case "mux":
                                        TaskSet(Config.TaskId.Mux, opArgs[1]);
                                        break;

                                    // Encountered an unrecognized argument
                                    default:
                                        App.Error("ErrArgUnknown");
                                        goto showUsage; // Interrupt both loops

                                }
#endregion

                            // No input parsing required, all we're asked to do is just
                            // to run some operations that do not take arguments
                            } else {

#region Task Status
                                // Select the task-scheduling operation to be performed
                                // from among those that do not require arguments
                                switch(args[i].ToLower()) {

                                    // Query the status of the GUI autorun
                                    case "gui":
                                        TaskGet(Config.TaskId.Gui);
                                        break;

                                    // Query the status of the Omen Key task
                                    case "key":
                                        TaskGet(Config.TaskId.Key);
                                        break;

                                    // Query the status of the nVidia Advanced Optimus bug fix task
                                    case "mux":
                                        TaskGet(Config.TaskId.Mux);
                                        break;

                                    // Encountered an unrecognized argument
                                    default:
                                        App.Error("ErrArgUnknown");
                                        goto showUsage; // Interrupt both loops

                                }
#endregion

                            }

                        // The loop will continue until there are no more arguments
                        // or a context switch is encountered, but always runs at least once
                        } while(++i < args.Length && !args[i].StartsWith("-"));
                    }

                    // Point the iterator to the next context argument, if any
                    i--;
                    break;

                // Show help (usage information)
                case "-h":
                case "-?":
                case "-help":
                case "--help":
                case "-usage":
                case "--usage":
                    Cli.PrintContext(Config.Locale.Get(Config.L_CLI + "Usage"));
                    goto showUsage; // Interrupt the context loop

                // Encountered an unrecognized argument
                default:
                    App.Error("ErrArgUnknown");
                    goto showUsage; // Interrupt the context loop

            } // Context loop

            return;

            showUsage:
            Cli.PrintUsage();

        }

    }

}
