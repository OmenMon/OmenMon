  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using OmenMon.AppCli;
using OmenMon.AppGui;
using OmenMon.Library;

namespace OmenMon {

    // The application's main class
    public class App {

        // Application entry point
        [STAThread]
        public static void Main(string[] args) {

            // Set up an event handler to run on exit
            AppDomain.CurrentDomain.ProcessExit += OnExit;

            // Initialize the configuration class
            Config.Initialize();

            // Initialize the hardware class
            Hw.Initialize();

            try {

                // If no arguments were given,
                // run in GUI (Windows Forms) mode
                if(args.Length == 0) {

#region GUI (Windows Forms) Mode
                    // Set up the Windows Forms interface
                    Gui.Initialize();

                    // Only allow a single GUI instance at any given time
                    bool isFirstInstance;
                    using(Mutex mutex = new Mutex(
                        true,
                        Config.LockPathGui,
                        out isFirstInstance)) {

                        if(isFirstInstance) {

                            // Start minimized with just a notification icon
                            Application.Run(new GuiTray());

                            // Release the lock when done
                            mutex.ReleaseMutex();

                        } else {

                            // Unless the application was run automatically
                            if(Environment.GetEnvironmentVariable(Config.EnvVarSelfName) == null
                                || Environment.GetEnvironmentVariable(Config.EnvVarSelfName).Contains(Config.EnvVarSelfValueGui))

                                // Send a message to the running instance
                                // to bring itself to the user's attention
                                Gui.BroadcastMessage(
                                    Gui.MessageId,
                                    Gui.MessageParam.AnotherInstance);

                        }

                    }
#endregion

                // In this special argument case,
                // launch a task in headless mode
                } else if(args[0].ToLower() == "-run") {

                    CliOp.TaskRun(args);

                // As for any other command-line arguments,
                // process them in CLI (Console) mode
                } else {

#region CLI (Console) Mode
                    // If this is the first CLI instance,
                    // relaunch as a console application
                    bool isFirstInstance;
                    using(Mutex mutex = new Mutex(
                        true,
                        Config.LockPathCli,
                        out isFirstInstance)) {

                        if(isFirstInstance) {

                            // Relaunch the process as a console application
                            Cli.Relaunch(args);

                            // Release the lock when done
                            mutex.ReleaseMutex();

                            // Make the command prompt reappear
                            Cli.RestorePrompt();

                        } else {

                            // Attach the console (which is detached by default)
                            Cli.Initialize();

                            // Output the header
                            Cli.PrintHeader();

                            // Process all command-line arguments
                            // and perform the operations as requested
                            CliOp.Loop(args);

                        }

                    }

                }
#endregion

            } catch(Exception e) {

                // Any unhandled errors will result in a pop-up dialog
                // or be output to the console if it is initialized

                Error("ErrUnexpected|EXCEPTION", e);

            }

        }

#region Error & Exit Handlers
        // Handles an error depending on whether the application is running in CLI or GUI mode
        public static void Error(string messageIds, Exception e = null) {

            if(Cli.IsInitialized)

                // Error out to the console
                Cli.PrintError(Config.GetError(messageIds, e), e);

            else

                // Pop up a window
                Gui.ShowError(Config.GetError(messageIds, e), e);

        }

        // Terminates the application
        public static void Exit(Config.ExitStatus code = Config.ExitStatus.NoError) {

            // Running as a console (CLI) application
            if(Cli.IsInitialized)

                // Make the command prompt reappear
                // since that's the end of it
                Cli.RestorePrompt();

            // Running as a Windows Forms (GUI) application
            if(GuiTray.Context != null && GuiTray.Context.Notification != null)
                GuiTray.Context.Notification.Visible = false;

            System.Environment.Exit((int) code);

        }

        // Handler that gets called when the application is about to exit
        private static void OnExit(object sender, EventArgs e) {

                // Close the hardware, if opened
                if(Hw.IsInitialized)
                    Hw.Close();

                // Free the console, if running as a CLI app
                if(Cli.IsInitialized)
                    Cli.Close();

                // Close the forms, if running as a GUI app
                if(Gui.IsInitialized)
                    Gui.Close();

        }
#endregion

    }

}
