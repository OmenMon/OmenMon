  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using OmenMon.External;

namespace OmenMon.Library {

    // Handles operating system calls
    public static class Os {

#region Console
        // Checks if the parent process is a PowerShell console
        public static bool IsConsolePowerShell() {

            // Initialize variables
            uint[] list = new uint[1];
            uint count = Kernel32.GetConsoleProcessList(list, 1);

            // If no console, return false
            if(count <= 0)
                return false;

            // Otherwise, try again with the appropriate list size
            list = new uint[count];
            count = Kernel32.GetConsoleProcessList(list, count);

            // Check image name of each process attached to the console
            for(int i = 0; i < list.Length; i++)
                if(Process.GetProcessById((int) list[i]).ProcessName.ToLower().Contains("cmd"))
                    return false;

            // If none of the image names of the processes associated with the console
            // contain the "cmd" string, then it is likely a PowerShell console
            // (might be "powershell" or "pwsh", which is why we're checking this way)
            return true;

        }
#endregion

#region Display Control
        // Retrieves the current display refresh rate
        public static int GetRefreshRate() {

            User32.DEVMODE d = new User32.DEVMODE();

            d.dmDeviceName = new string(new char[32]);
            d.dmFormName = new string(new char[32]);
            d.dmSize = (short) Marshal.SizeOf(d);

            User32.EnumDisplaySettings(null, User32.ENUM_CURRENT_SETTINGS, ref d);

            return d.dmDisplayFrequency;

        }

        public static List<int> GetAvailableRefreshRates()
        {
            User32.DEVMODE d = new User32.DEVMODE();
            d.dmDeviceName = new string(new char[32]);
            d.dmFormName = new string(new char[32]);
            d.dmSize = (short)Marshal.SizeOf(d);
            List<int> result = new List<int>();
            var modenum = 0;
            while (User32.EnumDisplaySettings(null, modenum++, ref d) == 1)
            {
                if (result.Contains(d.dmDisplayFrequency))
                    break;
                result.Add(d.dmDisplayFrequency);
            }

            result.Sort();
            return result;
        }
        
        // Re-applies Windows color settings
        public static void ReloadColorSettings() {

            // Turn calibration off and then on again
            ColorMgmt.WcsSetCalibrationManagementState(false);
            ColorMgmt.WcsSetCalibrationManagementState(true);

            // Alternatively, there is a COM object {B210D694-C8DF-490D-9576-9E20CDBC20BD} that
            // runs from a Task Scheduler entry: Microsoft\Windows\WindowsColorSystem\Calibration Loader

        }

        // Sets the display to standby
        public static void SetDisplayOff() {

            // Send a system command message
            User32.SendMessage(
                (IntPtr) User32.HWND_BROADCAST,
                User32.WM_SYSCOMMAND,
                (IntPtr) User32.SC_MONITORPOWER,
                (IntPtr) User32.MONITORPOWER.STANDBY);

        }

        // Sets the display refresh rate to a given value
        public static void SetRefreshRate(int frequency) {

            User32.DEVMODE d = new User32.DEVMODE();

            d.dmDeviceName = new string(new char[32]);
            d.dmFormName = new string(new char[32]);
            d.dmSize = (short) Marshal.SizeOf(d);

            User32.EnumDisplaySettings(null, User32.ENUM_CURRENT_SETTINGS, ref d);

            d.dmDisplayFrequency = frequency;

            // Check if change can be performed first, only then proceed
            if(User32.ChangeDisplaySettings(ref d, User32.CDS_TEST) != User32.DISP_CHANGE_FAILED)
                User32.ChangeDisplaySettings(ref d, User32.CDS_UPDATEREGISTRY);

        }
#endregion

#region Restart & Reload
        // Attempts to enable a token privilege
        public static void EnableTokenPrivilege(string value) {

            // Retrieve the current process token
            IntPtr handle = IntPtr.Zero;
            AdvApi32.OpenProcessToken(Process.GetCurrentProcess().Handle, AdvApi32.TOKEN_ADJUST_PRIVILEGES | AdvApi32.TOKEN_QUERY, ref handle);

            // Look up the locally-unique identifier of the requested privilege
            AdvApi32.LUID luid = new AdvApi32.LUID();
            AdvApi32.LookupPrivilegeValue("", value, ref luid);

            // Pack data into a privilege-adjustment structure 
            AdvApi32.TOKEN_PRIVILEGES privileges;
            privileges.PrivilegeCount = 1;
            privileges.Privileges.Attributes = AdvApi32.SE_PRIVILEGE_ENABLED;
            privileges.Privileges.Luid = luid;

            // Request privilege adjustment
            AdvApi32.TOKEN_PRIVILEGES privilegesOut = new AdvApi32.TOKEN_PRIVILEGES();
            int size = 4;
            AdvApi32.AdjustTokenPrivileges(handle, 0, ref privileges, 4 + (12 * privileges.PrivilegeCount), ref privilegesOut, ref size);

        }

        // Removes a file on reboot
        public static void RemoveOnReboot(string name) {
            Kernel32.MoveFileEx(name, null, Kernel32.MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT);

            // Stored in HKLM\SYSTEM\CurrentControlSet\Control\Session Manager
            // under "PendingFileRenameOperations" (REG_MULTI_SZ)

        }

        // Stops and then starts again a service
        public static void RestartService(string name) {
            IntPtr manager, service;

            // Establish a Service Control Manager session
            if((manager = AdvApi32.OpenSCManager(null, null, AdvApi32.SC_MANAGER_ACCESS_MASK.SC_MANAGER_ALL_ACCESS)) == IntPtr.Zero)
                return;

            // Open the requested service
            if((service = AdvApi32.OpenService(manager, name, AdvApi32.SERVICE_ACCESS_MASK.SERVICE_ALL_ACCESS)) == IntPtr.Zero) {
                AdvApi32.CloseServiceHandle(manager);
                return;
            }

            // Instruct the service to stop
            AdvApi32.SERVICE_STATUS status = new();
            AdvApi32.ControlService(service, AdvApi32.SERVICE_CONTROL.SERVICE_CONTROL_STOP, ref status);

            // Wait until the service stops
            while(status.dwCurrentState != (uint) AdvApi32.SERVICE_STATE.SERVICE_STOPPED) {
                AdvApi32.QueryServiceStatus(service, ref status);
                Thread.Sleep(Config.WaitToStopService);
            }

            // Start the service again
            AdvApi32.StartService(service, 0, null);

            // Close the handles
            AdvApi32.CloseServiceHandle(service);
            AdvApi32.CloseServiceHandle(manager);

        }

        // Window message identifier to restart Explorer shell
        // Equivalent of right-clicking on the taskbar while holding Ctrl-Shift
        // and choosing the "Exit Explorer" context-menu option
        public const int WM_SHELL_RESTART = User32.WM_USER + 0x01B4;

        // Explorer shell window class
        public const string WC_SHELL = "Shell_TrayWnd";

        // Restarts the user shell (Explorer process)
        public static void RestartShell() {

            // Get the handle to the Explorer shell window
            IntPtr handle = User32.FindWindow(WC_SHELL, null);

            // Send a message telling the shell to close
            User32.PostMessage(handle, WM_SHELL_RESTART, (IntPtr) 0, (IntPtr) 0);

            // Give it some time to do so
            do {

                // If the handle can no longer be found, we're done
                if((handle = User32.FindWindow(WC_SHELL, null)) == (IntPtr) 0)
                    break;

                // Wait a second
                Thread.Sleep(Config.WaitToStopProcess);

            } while(true);

            // Obtain the shell executable name from the Registry
            using(RegistryKey key = Registry.LocalMachine.OpenSubKey(Config.RegShellKey, true)) {

                // Start the shell process
                Process shell = new Process();
                shell.StartInfo.FileName =
                    Environment.GetEnvironmentVariable(Config.EnvVarSysRoot)
                    + "\\" + (string) key.GetValue(Config.RegShellValue);
                shell.StartInfo.UseShellExecute = true;
                shell.Start();

            }

        }

        // Initiates a system restart
        public static int RestartSystem(bool force = false) {

            // Obtain the required shutdown privilege
            EnableTokenPrivilege("SeShutdownPrivilege");

            // Execute a planned shutdown
            // for hardware reconfiguration reasons
            return User32.ExitWindowsEx(
                force ? User32.EWX_FORCE | User32.EWX_REBOOT : User32.EWX_REBOOT,
                User32.SHTDN_REASON_MAJOR_HARDWARE
                | User32.SHTDN_REASON_MINOR_RECONFIG
                | User32.SHTDN_REASON_FLAG_PLANNED);

        }
#endregion

#region Task Scheduling
        // Adds a scheduled task
        public static void AddTask(string folderName, string taskName, string command = "", string args = "", bool logonTrigger = false) {

            // Set up a Task Service instance and connect to it
            TaskSchd.ITaskService service = (TaskSchd.ITaskService) new TaskSchd.TaskSchedulerClass();
            service.Connect();

            // Create a new task definition
            TaskSchd.ITaskDefinition definition = service.NewTask(0);

            // Add task settings to definition
            definition.Principal.LogonType = TaskSchd.LogonType.InteractiveToken;
            definition.Principal.RunLevel = TaskSchd.RunLevel.Highest;
            definition.RegistrationInfo.Author = null;
            definition.Settings.AllowDemandStart = true;
            definition.Settings.AllowHardTerminate = true;
            definition.Settings.Compatibility = TaskSchd.Compatibility.V2;
            definition.Settings.DisallowStartIfOnBatteries = false;
            definition.Settings.Enabled = true;
            definition.Settings.ExecutionTimeLimit = "PT0S";
            definition.Settings.Hidden = false;
            definition.Settings.IdleSettings.IdleDuration = "";
            definition.Settings.IdleSettings.StopOnIdleEnd = false;
            definition.Settings.IdleSettings.RestartOnIdle = false;
            definition.Settings.IdleSettings.WaitTimeout = "";
            definition.Settings.MultipleInstances = TaskSchd.InstancesPolicy.IgnoreNew;
            definition.Settings.Priority = TaskSchd.Priority.Normal;
            definition.Settings.RestartCount = 0;
            definition.Settings.RunOnlyIfIdle = false;
            definition.Settings.RunOnlyIfNetworkAvailable = false;
            definition.Settings.StartWhenAvailable = true;
            definition.Settings.StopIfGoingOnBatteries = false;
            definition.Settings.WakeToRun = false;

            // Add task action to definition
            TaskSchd.IAction action = definition.Actions.Create(TaskSchd.ActionType.Execute);
            ((TaskSchd.IExecAction) action).Path = command == "" ? Config.AppFile : command;

            // Add arguments if not empty
            if(args != "")
                ((TaskSchd.IExecAction) action).Arguments = args;

            // Add logon trigger to definition, if requested
            TaskSchd.ITrigger trigger = null;
            if(logonTrigger) {
                trigger = definition.Triggers.Create(TaskSchd.TriggerType.Logon);
                trigger.Enabled = true;
            }

            // Open the specified task folder
            TaskSchd.ITaskFolder folder = service.GetFolder(folderName);

            // Expect the call may not be succesful
            try {

                // Register the task
                folder.RegisterTaskDefinition(
                    taskName,
                    definition,
                    TaskSchd.Registration.CreateOrUpdate,
                    null,
                    null,
                    TaskSchd.LogonType.None,
                    null);

            // Clean up even if the call failed
            } finally {

                // Release the COM objects
                if(logonTrigger)
                    Marshal.ReleaseComObject(trigger);
                Marshal.ReleaseComObject(action);
                Marshal.ReleaseComObject(definition);
                Marshal.ReleaseComObject(folder);
                Marshal.ReleaseComObject(service);

            }

        }

        // Deletes a scheduled task
        public static void DeleteTask(string folderName, string taskName) {

            // Set up a Task Service instance and connect to it
            TaskSchd.ITaskService service = (TaskSchd.ITaskService) new TaskSchd.TaskSchedulerClass();
            service.Connect();

            // Open the specified task folder
            TaskSchd.ITaskFolder folder = service.GetFolder(folderName);

            // Expect the call may not be succesful
            try {

                // Delete the task
                folder.DeleteTask(taskName, 0);

            // Clean up even if the call failed
            } finally {

                // Release the COM objects
                Marshal.ReleaseComObject(folder);
                Marshal.ReleaseComObject(service);

            }

        }

        // Checks if a scheduled task exists
        public static bool HasTask(string folderName, string taskName) {

            // Set up a Task Service instance and connect to it
            TaskSchd.ITaskService service = (TaskSchd.ITaskService) new TaskSchd.TaskSchedulerClass();
            service.Connect();

            // Open the specified task folder
            TaskSchd.ITaskFolder folder = service.GetFolder(folderName);

            // The call will be succesful only if the task exists
            try {

                // Attempt to retrieve the task details
                folder.GetTask(taskName);
                return true;

            } catch {

                // For our purposes, it's enough to interpret
                // any error as an indication the task doesn't exist
                return false;

            // Clean up even if the call failed
            } finally {

                // Release the COM objects
                Marshal.ReleaseComObject(folder);
                Marshal.ReleaseComObject(service);

            }

        }
#endregion

#region Window Manipulation
        // Retrieves the color of a given pixel given a window handle
        public static int GetPixel(IntPtr hWnd, Point point) {
            IntPtr hDC = User32.GetDC(hWnd);
            int color = Gdi32.GetPixel(hDC, point.X, point.Y);
            User32.ReleaseDC(hWnd, hDC);
            return color;
        }

        // Retrieves the text associated with a control
        public static string GetWindowText(IntPtr handle) {

            // Query the necessary buffer length
            int length = User32.GetWindowTextLength(handle);

            // Allocate the buffer
            char[] buffer = new char[length + 2];

            // Retrieve the text
            User32.GetWindowText(handle, buffer, buffer.Length);

            // Return the buffer as a string
            return new string(buffer);

        }

        // Maximum notification icon tooltip text length
        public const int NOTIFY_ICON_TEXT_MAXLEN = 128;

        // Sets the text of the notification icon bypassing the 64-character limit
        public static void SetNotifyIconText(NotifyIcon icon, string text) {

            // There is still a limit, only higher
            if(text.Length >= NOTIFY_ICON_TEXT_MAXLEN)
                throw new ArgumentOutOfRangeException(); // 127 is the new 63

            // Retrieve the field and method using reflection
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            Type type = typeof(NotifyIcon);

            // Set the new text
            type.GetField("text", flags).SetValue(icon, text);

            // Update the icon
            if((bool) type.GetField("added", flags).GetValue(icon))
                type.GetMethod("UpdateIcon", flags)
                    .Invoke(icon, new object[] { true });

        }
#endregion

    }

}
