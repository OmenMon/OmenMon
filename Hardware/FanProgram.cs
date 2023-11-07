  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using System.Collections.Generic;
using OmenMon.Hardware.Bios;
using OmenMon.Library;

namespace OmenMon.Hardware.Platform {

#region Data
    // Stores fan program data
    public class FanProgramData {

        // Fan mode to maintain while the program is running
        public BiosData.FanMode FanMode;

        // GPU power level to set when the program starts
        public BiosData.GpuPowerLevel GpuPower;

        // Stores the mapping of temperature thresholds
        // to fan levels for each fan
        public SortedDictionary<byte, byte[]> Level;

        // Fan program name
        public string Name;

        // Constructs a fan program data instance
        public FanProgramData(
            string name,
            BiosData.FanMode fanMode,
            BiosData.GpuPowerLevel gpuPower,
            SortedDictionary<byte, byte[]> level) {

            this.Name = name;
            this.FanMode = fanMode;
            this.GpuPower = gpuPower;
            this.Level = level;

        }

    }
#endregion

    // Stores fan program logic
    public class FanProgram {

#region Variables & Initialization
        // Status callback importance rating
        public enum Severity {
            Verbose,    // Only for reference
            Notice,     // Might be shown if possible
            Important   // Need to be shown to the user
        }

        // Callback method for status updates
        private Action<FanProgram.Severity, string> Callback;

        // GPU power data for the current program
        private BiosData.GpuPowerData GpuPowerData;

        // State flags
        public bool IsEnabled { get; private set; }

        // Last fan mode and GPU power data before the program started
        private BiosData.FanMode LastFanMode;
        private BiosData.GpuPowerData LastGpuPowerData;

        // Level list for the current program
        // to facilitate threshold look-ups
        private List<byte> Levels;

        // Name of the last running program
        private string Name;

        // Parent class reference
        private Platform Platform;

        // Constructs a fan program instance
        public FanProgram(
            Platform platform,
            Action<FanProgram.Severity, string> callback) {

            this.Callback = callback;
            this.GpuPowerData = default(BiosData.GpuPowerData);
            this.IsEnabled = false;
            this.LastFanMode = BiosData.FanMode.Default;
            this.LastGpuPowerData = default(BiosData.GpuPowerData);
            this.Levels = new List<byte>();
            this.Name = "";
            this.Platform = platform;

        }
#endregion

#region Public Methods
        // Retrieves the name of the current fan program
        public string GetName() {

            return this.Name;

        }

        // Starts a fan program given its name
        public bool Run(string name) {

            // Note: no need to terminate
            // the previous program first, if any

            // Try to set up the new program
            // bail out if not succesful
            if(!Setup(name))
                return false;

            // Enable manual fan mode
            Platform.Fans.SetManual(true);

            // Set the state flag
            this.IsEnabled = true;

            // Save the last fan mode
            this.LastFanMode = Platform.Fans.GetMode();

            // Save the last GPU power state
            this.LastGpuPowerData = Platform.System.GetGpuPower();

            // Update the program
            Update();

            // Report success
            return true;

        }

        // Terminates the running fan program, if any is running
        public bool Terminate() {

            // Fail if no program active
            if(!this.IsEnabled)
                return false;

            // Reset fan speed
            SetFanLevel(new byte[] { Byte.MaxValue, Byte.MaxValue } );

            // Disable manual fan mode
            Platform.Fans.SetManual(false);

            // Restore the previous fan mode
            UpdateFanMode(true, this.LastFanMode);

            // Restore the previous GPU power settings
            UpdateGpuPower(true, this.LastGpuPowerData);

            // Set the state flag
            this.IsEnabled = false;

            // Reset data
            Reset();

            // Update the status
            Status(Severity.Notice, Config.Locale.Get(Config.L_PROG + "End"));

            // Report success
            return true;

        }

        // Updates the fan program, if any is running
        public bool Update() {

            // Fail if no program active
            if(!this.IsEnabled)
                return false;

            // Find out the current maximum temperature,
            // the temperature level for the given temperature,
            // and the target fan levels for the given level
            byte temperature = Platform.GetMaxTemperature(true);
            byte level = GetTemperatureLevel(temperature);
            byte[] fans = GetFanLevel(level);

            // Note: the above could all be accomplished with
            // a single nested call, except we also want to report
            Status(Severity.Notice,
                Config.Locale.Get(Config.L_PROG + "T") + Config.Locale.Get(Config.L_PROG + "SubMax") + " " 
                + Conv.GetString(temperature, 2, 10) + Config.Locale.Get(Config.L_UNIT + "Temperature") + " "
                + Config.Locale.Get(Config.L_PROG + "Lvl") + " " + Conv.GetString(level, 2, 10) + " "
                + Config.Locale.Get(Config.L_PROG + "Fans") + " "
                + Conv.GetString(fans[0], 2, 10) + ", " + Conv.GetString(fans[1], 2, 10));

            // Set the fan level
            SetFanLevel(fans);

            // Perform other updates, only if necessary
            UpdateFanMode();
            UpdateGpuPower();
            UpdateCountdown();

            // Report success
            return true;

        }
#endregion

#region Private Methods
        // Obtains the fan levels for the given temperature level
        private byte[] GetFanLevel(byte level) {

            // Retrieve the value from the configuration data
            return Config.FanProgram[this.Name].Level[level];

        }

        // Obtains the program level for the given temperature
        private byte GetTemperatureLevel(byte temperature) {
            int value;

            // Binary-search the level list
            // If the result is non-zero, an index was found
            if((value = Levels.BinarySearch(temperature)) >= 0)

                // Return the item at the index directly
                return this.Levels[value];

            // The result is a bitwise complement of the next larger item index,
            // or the index of the last element of the list if no larger item exists
            else

                // Return the item at the binary complement index less one
                return this.Levels[~value - 1];

        }

        // Resets the state data when terminating a program
        private void Reset() {

            // Clear the GPU power data
            this.GpuPowerData = default(BiosData.GpuPowerData);

            // Clear the last fan mode
            this.LastFanMode = BiosData.FanMode.Default;

            // Note: do not clear the last program name since leaving it
            // is harmless, and also can be used to show a more meaningful
            // status message using GetName() even after the program ended

            // Clear the level keys
            this.Levels = new List<byte>();

        }

        // Set the fan levels to the given parameter
        private void SetFanLevel(byte[] level) {

            // Set the fan levels
            this.Platform.Fans.SetLevels(level);

            // Note: this does not currently handle the case when both fans are being set to 0
            // since this is not allowed by the hardware through this call, and would depend
            // on calling Fans.SetOff(true) instead; not implemented due to possible long-term
            // implications of the fans being switched off for extended periods of time

        }

        // Sets up a new fan program to be run
        private bool Setup(string name) {

            // Bail out if referring to a non-existent program
            if(!Config.FanProgram.ContainsKey(name))
                return false;

            // Set up the program name
            this.Name = name;

            // Set up the level keys
            this.Levels = new List<byte>(Config.FanProgram[this.Name].Level.Keys);

            // Set up the target GPU power data
            this.GpuPowerData = new BiosData.GpuPowerData(Config.FanProgram[this.Name].GpuPower);

            // Report success
            return true;

        }

        // Reports fan program status
        private void Status(Severity severity, string message) {

            // Report status via the callback method
            Callback(severity, message);

        }

        // Updates the fan manual mode countdown, optionally only if necessary
        private void UpdateCountdown(bool forceUpdate = false) {

            // Skip if there still is enough time to do it
            // during the next update, unless forced not to
            if(forceUpdate
                || this.Platform.Fans.GetCountdown()
                    < (Config.UpdateProgramInterval
                        + Config.FanCountdownExtendThreshold))

               // Set the fan countdown
               this.Platform.Fans.SetCountdown(Config.FanCountdownExtendInterval);

        }

        // Updates the fan mode, optionally only if different than the desired target state
        private void UpdateFanMode(
            bool forceUpdate = false,
            BiosData.FanMode? mode = null) {

            // Set the default mode if empty
            if(mode == null)
                mode = Config.FanProgram[this.Name].FanMode;

            // Skip if the settings are the same already, unless forced not to
            if(forceUpdate
                || this.Platform.Fans.GetMode() != mode)

               // Set the fan mode
               this.Platform.Fans.SetMode((BiosData.FanMode) mode);

        }

        // Updates the GPU power, optionally only if different than the desired target state
        private void UpdateGpuPower(
            bool forceUpdate = false,
            BiosData.GpuPowerData? power = null) {

            // Set the default GPU power data if empty
            if(power == null)
                power = this.GpuPowerData;

            // Skip if the settings are the same already, unless forced not to
            if(forceUpdate
                || this.Platform.System.GetGpuCustomTgp() != this.GpuPowerData.CustomTgp
                || this.Platform.System.GetGpuPpab() != this.GpuPowerData.Ppab)

                // Set the GPU power
                this.Platform.System.SetGpuPower((BiosData.GpuPowerData) power);

        }

#endregion
    }

}
