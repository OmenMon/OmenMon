  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using System.IO;
using OmenMon.Library;

namespace OmenMon.Library {

    // Sets a state that persists until reboot only
    public class OnlyOnce {

        private readonly string FileName;
        private bool IsChecked;
        private bool IsFirstTime;

        // Initializes the class
        public OnlyOnce(string name) {

            // Determine the lock file name
            FileName = Config.OnlyOncePath + "\\" + name + Config.OnlyOnceFileExt;

            // Check if this is the first run after reboot
            // and set the lock if that's the case
            if(IsFirstTime = Check())
                Set();

        }

        // Checks the state
        public bool Check() {

            // Only check if we haven't already
            if(!IsChecked) {
                try {

                    // Not the first time if the file exists
                    IsFirstTime = !File.Exists(FileName);

                } catch {
                } finally {

                    // Remember we checked
                    IsChecked = true;

                }
            }

            // Return the answer
            return IsFirstTime;

        }

        // Sets the state
        private void Set() {

            try {

                // Create the file
                File.Create(FileName).Close();

                // Reset the state on reboot
                Os.RemoveOnReboot(FileName);

            } catch { }

        }

    }

}
