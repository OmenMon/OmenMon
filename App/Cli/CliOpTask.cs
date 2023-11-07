  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using System.Diagnostics;
using OmenMon.AppGui;
using OmenMon.Library;

namespace OmenMon.AppCli {

    // Implements the main operation loop in the application's CLI mode
    // This part covers task scheduling-specific routines
    public static partial class CliOp {

#region Task Query
        // Prints out the status of a specific task
        private static void TaskGet(Config.TaskId task) {

            // Determine the status for the specified task and output it
            Cli.PrintTaskResult(false, task, Hw.TaskGet(task));

        }
#endregion

#region Task Execution
        // Launch a specified task in headless mode (no output)
        public static void TaskRun(string[] args) {

            // Determine the task to be executed
            Config.TaskId taskId = default(Config.TaskId);
            try {
                taskId = (Config.TaskId) Enum.Parse(typeof(Config.TaskId), args[1]);
            } catch {
                App.Exit(Config.ExitStatus.ErrorTask);
            }

            switch(taskId) {

                case Config.TaskId.Gui: // The application is starting during boot
                case Config.TaskId.Key: // Responding to the Omen Key being pressed

                    // Start the OmenMon GUI if not running
                    if(Process.GetProcessesByName(Config.AppName).Length == 1) {
                        Process gui = new Process();
                        gui.StartInfo.Environment.Add(Config.EnvVarSelfName, taskId == Config.TaskId.Gui ?
                                Config.EnvVarSelfValueGui : Config.EnvVarSelfValueKey);
                        gui.StartInfo.FileName = Config.AppFile;
                        gui.StartInfo.UseShellExecute = false; // Required for environment change
                        gui.StartInfo.WindowStyle = taskId == Config.TaskId.Gui ?
                            ProcessWindowStyle.Minimized : ProcessWindowStyle.Normal;
                        gui.Start();
                    }

                    // Broadcast a message to the GUI that this is an automatic run
                    Gui.Initialize();
                    Gui.BroadcastMessage(
                        Gui.MessageId,
                        taskId == Config.TaskId.Gui ?
                            Gui.MessageParam.Gui : Gui.MessageParam.Key);
                    break;

                case Config.TaskId.Mux: // Apply the Advanced Optimus bug fix

                    // The fix is only applicable to discrete graphics mode
                    // and it persists until reboot, so only needs to run once
                    if(Hw.NvMuxGetState() == Hw.NvMuxState.Discrete
                        && (new OnlyOnce(Config.LockNameMux)).Check()) {

                        // Reload the color settings
                        // Fixes the color profile not being applied
                        Os.ReloadColorSettings();

                        // Restart the Explorer shell
                        // Fixes screen stutter
                        Os.RestartShell();

                        // Restart nVidia Display Container service
                        // Fixes no Advanced Optimus icon following shell restart
                        Os.RestartService(Config.NvDisplayContainerService);

                    }

                    break;

            }

        }
#endregion

#region Task Management
        // Installs or removes a specific task
        private static void TaskSet(Config.TaskId task, string value) {
            bool flag;

            // Try to parse as a numerical value
            if(!Conv.GetBool(value, out flag))

                // If the value could not be parsed, error out
                App.Error("ErrNeedValueBool|DataSyntaxBool");

            else {

                // Change the task status for the specified task
                Hw.TaskSet(task, flag);

                // Output the new setting
                Cli.PrintTaskResult(true, task, flag);

            }

        }
#endregion

    }

}
