  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Win32;
using OmenMon.Hardware.Ec;
using OmenMon.Hardware.Bios;

namespace OmenMon.Library {

    // Implements hardware interaction routines
    // reusable between the CLI and the GUI
    public static class Hw {

#region Initialization & Termination
        // State flag
        public static bool IsInitialized { get; private set; }

        // Initializes the helper class
        public static void Initialize() {

            // Only do it once
            if(!IsInitialized) {

                // Initialization is currently handled individually
                // by calling BiosInit() and EcInit() when required

                // Done
                IsInitialized = true;

            }

        }

        // Closes the hardware
        public static void Close() {

                // Close the BIOS session, if established
                if(Bios != null)
                try {
                    Bios.Close();
                } catch { }

                // Close the embedded controller, if loaded
                if(Ec != null)
                try {
                    Ec.Close();
                } catch { }

        }
#endregion

#region BIOS
        // BIOS Control Interface
        public static IBiosCtl Bios;

        // Prepares the BIOS for use
        public static void BiosInit() {
            Bios = BiosInterface();
            if(Bios == null || !Bios.IsInitialized)
                App.Exit(Config.ExitStatus.ErrorBios);
        }

        // Returns the BIOS interface
        public static IBiosCtl BiosInterface() {
            var bios = BiosCtl.Instance;
            if(bios == null) {
                App.Error("ErrBiosNull");
                return null;
            }
            bios.Initialize();
            if(bios.IsInitialized) {
                return bios;
            } else {
                App.Error("ErrBiosInit");
                bios.Close();
            }
            return null;
        }

        // Performs BIOS operations
        public static void BiosExec(Action<IBiosCtl> callback, IBiosCtl bios) {
            callback(bios);
        }

        // Performs BIOS operations and returns a result
        public static TResult BiosExec<TResult>(Func<IBiosCtl,TResult> callback, IBiosCtl bios) {
            return (TResult) callback(bios);
        }

        // Prepares the BIOS for use and then performs operations
        public static void BiosExec(Action<IBiosCtl> callback) {
            using(Bios = BiosInterface()) {
                if(Bios != null && Bios.IsInitialized) {
                    BiosExec(callback, Bios);
                }
            }
        }

        // Prepares the BIOS for use and then performs operations that return a result
        public static TResult BiosExec<TResult>(Func<IBiosCtl,TResult> callback) {
            using(Bios = BiosInterface()) {
                if(Bios != null && Bios.IsInitialized) {
                    return (TResult) BiosExec(callback, Bios);
                } else {
                    return default(TResult);
                }
            }
        }

        // Performs a BIOS operation and returns a numeric (possibly an enumerated or an array) result
        public static TResult BiosGet<TResult>(Func<TResult> biosMethod) {
            return Hw.BiosExec<TResult>(bios => {
                return (TResult) (object) biosMethod();
            }, Hw.Bios);
        }

        // Performs a BIOS operation and returns a struct result
        public static TResult BiosGetStruct<TResult>(Func<TResult> biosMethod) where TResult : struct {
            return Hw.BiosExec<TResult>(bios => {
                return (TResult) biosMethod();
            }, Hw.Bios);
        }

        // Sets a BIOS toggle to a Boolean value passed as a parameter
        public static void BiosSet(Action<bool> biosMethod, bool flag) {
            Hw.BiosExec(bios => {
                // Send the command to the BIOS
                biosMethod(flag);
            }, Hw.Bios);
        }

        // Sets a BIOS setting to a numerical or enumerated value passed as a parameter
        public static void BiosSet<T>(Action<T> biosMethod, T param) {
            Hw.BiosExec(bios => {
                // Send the command to the BIOS
                biosMethod((T) param);
            }, Hw.Bios);

        }

        // Sets the BIOS LED animation table based on the value passed as a parameter
        public static void BiosSetStruct(Action<BiosData.AnimTable> biosMethod, BiosData.AnimTable animTable) {
            Hw.BiosExec(bios => {
                // Send the updated animation table to the BIOS
                biosMethod(animTable);
            }, Hw.Bios);
        }

        // Sets the BIOS keyboard backlight color table based on the value passed as a parameter
        public static void BiosSetStruct(Action<BiosData.ColorTable> biosMethod, BiosData.ColorTable colorTable) {
            Hw.BiosExec(bios => {
                // Send the updated color table to the BIOS
                biosMethod(colorTable);
            }, Hw.Bios);
        }

        // Sets the BIOS fan table based on the value passed as a parameter
        public static void BiosSetStruct(Action<BiosData.FanTable> biosMethod, BiosData.FanTable fanTable) {
            Hw.BiosExec(bios => {
                // Send the updated fan table to the BIOS
                biosMethod(fanTable);
            }, Hw.Bios);
        }

        // Sets the BIOS GPU power settings based on the value passed as a parameter
        public static void BiosSetStruct(Action<BiosData.GpuPowerData> biosMethod, BiosData.GpuPowerData gpuPowerData) {
            Hw.BiosExec(bios => {
                // Not a mistake: seems this need to run twice to take effect,
                // at least in certain scenarios (such as switching PPAB off)
                biosMethod(gpuPowerData);
                Thread.Sleep(Config.GpuPowerSetInterval);
                biosMethod(gpuPowerData);
            }, Hw.Bios);
        }
#endregion

#region Embedded Controller
        // Embedded Controller interface
        public static IEmbeddedController Ec;

        // Prepares the embedded controller for use
        public static void EcInit() {
            Ec = EcInterface();
            if(Ec == null || !Ec.IsInitialized)
                App.Exit(Config.ExitStatus.ErrorEc);
            }

        // Returns the Embedded Controller interface
        public static IEmbeddedController EcInterface() {
            var ec = EmbeddedController.Instance;
            if(ec == null) {
                App.Error("ErrEcNull");
                return null;
            }
            ec.Initialize();
            if(ec.IsInitialized) {
                return ec;
            } else {
                App.Error("ErrEcInit");
                ec.Close();
            }
            return null;
        }

        // Runs operations while the Embedded Controller is locked for exclusive use
        public static void EcExec(Action<IEmbeddedController> callback, IEmbeddedController ec) {
            if(ec.Request(Config.EcMutexTimeout)) {
                try {
                    callback(ec);
                } finally {
                    ec.Release();
                }
            }
            else {
                App.Error("ErrEcLock");
            }
        }

        // Runs operations while the Embedded Controller is locked for exclusive use and returns a result
        public static TResult EcExec<TResult>(Func<IEmbeddedController,TResult> callback, IEmbeddedController ec) {
            if(ec.Request(Config.EcMutexTimeout)) {
                try {
                    return (TResult) callback(ec);
                } finally {
                    ec.Release();
                }
            } else {
                App.Error("ErrEcLock");
                return default(TResult);
            }
        }

        // Prepares the Embedded Controller and then runs operations while it is locked for exclusive use
        public static void EcExec(Action<IEmbeddedController> callback) {
            using(Ec = EcInterface()) {
                if(Ec != null) {
                    EcExec(callback, Ec);
                }
            }
        }

        // Prepares the Embedded Controller, runs operations while it is locked for exclusive use, and returns a result
        public static TResult EcExec<TResult>(Func<IEmbeddedController,TResult> callback) {
            using(Ec = EcInterface()) {
                if(Ec != null) {
                    return (TResult) EcExec(callback, Ec);
                } else {
                    return default(TResult);
                }
            }
        }

        // Retrieves the value of a specific byte-sized register
        public static byte EcGetByte(byte register) {
            return Hw.EcExec<byte>(ec => {
                return ec.ReadByte(register);
            }, Hw.Ec);
        }

        // Prints out the value of a little-endian word stored in two consecutive registers
        public static ushort EcGetWord(byte register) {
            return Hw.EcExec<ushort>(ec => {
                return ec.ReadWord(register);
            }, Hw.Ec);
        }

        // Performs an Embedded Controller operation
        // and returns a byte-sized numeric (possibly an enumerated) result
        public static TResult EcGet<TResult>(byte register) {
            return (TResult) (object) EcGetByte(register);
        }

        // Sets the value of a specific byte-sized register
        public static void EcSetByte(byte register, byte value) {
            Hw.EcExec(ec => {
                ec.WriteByte(register, value);
            }, Hw.Ec);
        }

        // Sets the value of a little-endian word stored in two consecutive registers
        public static void EcSetWord(byte register, ushort value) {
            Hw.EcExec(ec => {
                ec.WriteWord(register, value);
            }, Hw.Ec);
        }

        // Sets the value of a specific byte-sized register
        public static void EcSet(byte register, byte value) {
            EcSetByte(register, value);
        }

        // Sets the value of a little-endian word stored in two consecutive registers
        public static void EcSet(byte register, ushort value) {
            EcSetWord(register, value);
        }
#endregion

#region Graphics
        // nVidia multiplexer states
        public enum NvMuxState : int {
            Optimus  = 0x00000001,  // Software-switching
            Discrete = 0x00000002   // Discrete GPU only
        }

        // Retrieves the current nVidia multiplexer state from the Registry
        public static NvMuxState NvMuxGetState() {
            using(RegistryKey key = Registry.LocalMachine.OpenSubKey(Config.RegMuxKey, true))
                return (NvMuxState) (int) key.GetValue(Config.RegMuxValue);
        }
#endregion

#region Tasks
        // Returns the status of a specific task
        public static bool TaskGet(Config.TaskId task) {
            return Os.HasTask(Config.TaskFolder, Config.Task[task][0]);
        }

        // Installs or removes a specific task
        public static void TaskSet(Config.TaskId task, bool flag) {
            using WmiEvent wmiEvent = new WmiEvent();
            string taskName = Enum.GetName(typeof(Config.TaskId), task);

            // Remove the given task first, regardless
            try {

                // Delete the task from the Task Scheduler
                Os.DeleteTask(Config.TaskFolder, Config.Task[task][0]);

            } catch { }

            // The GUI task simply uses a logon trigger,
            // so no further steps are needed for removal
            if(task != Config.TaskId.Gui) {

                    // Remove WMI event triggers for the task
                    wmiEvent.DeleteBinding(Config.AppName + taskName + Config.WmiEventSuffixFilter, WmiEvent.BindingLookup.ByFilter);
                    wmiEvent.DeleteConsumer(Config.AppName + taskName + Config.WmiEventSuffixConsumer);
                    wmiEvent.DeleteFilter(Config.AppName + taskName + Config.WmiEventSuffixFilter);

                }

            // If asked to add the task
            if(flag) {

                try {

                    // Every task is added to the Task Scheduler first of all
                    Os.AddTask(
                        Config.TaskFolder, Config.Task[task][0], "", // Executable path, defaults to current process if empty
                        Config.Task[task][1], task == Config.TaskId.Gui ? true : false); // Logon trigger for the GUI task only

                } catch { }

                // The GUI task, triggered by logon, does not have any extra steps
                if(task != Config.TaskId.Gui) {

                    // Other tasks depend on the WMI event filter binding
                    wmiEvent.CreateBinding(

                        wmiEvent.CreateConsumer(new Dictionary<string, object>() {
                            ["CommandLineTemplate"] = Config.TaskRunPath + " "
                                + Config.TaskRunArgs + "\"" + Config.AppName + " " + taskName + "\"",
                            ["ExecutablePath"] = Config.TaskRunPath,
                            ["Name"] = Config.AppName + taskName + Config.WmiEventSuffixConsumer }),

                        wmiEvent.CreateFilter(new Dictionary<string, object>() {
                                ["EventNameSpace"] = Config.Task[task][2],
                                ["Query"] = Config.Task[task][3].Replace("\\", "\\\\"), // The WMI query needs double backlashes
                                ["QueryLanguage"] = Config.WmiQueryLang,
                                ["Name"] = Config.AppName + taskName + Config.WmiEventSuffixFilter }));

                }

            }

        }
#endregion

    }

}
