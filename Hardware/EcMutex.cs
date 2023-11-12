  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using OmenMon.Library;

namespace OmenMon.Hardware.Ec {

    // Implements an exclusive lock mechanism for accessing the Embedded Controller
    public static class EmbeddedControllerMutex {

        private static Mutex m;

        // Closes the lock
        public static void Close() {
            m?.Close(); }

        // Sets up a new lock
        public static void Open() {
            m = CreateOrOpenExistingMutex(Config.LockPathEc);

            static Mutex CreateOrOpenExistingMutex(string name) {
                try {
                    MutexSecurity security = new MutexSecurity();
                    security.AddAccessRule(
                        new MutexAccessRule(
                            new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                            MutexRights.FullControl, AccessControlType.Allow));
                    return new Mutex(false, name, out _, security);
                } catch(UnauthorizedAccessException) {
                    try {
                        return Mutex.OpenExisting(name, MutexRights.Synchronize);
                    } catch {
                    }
                }
                return null;
            }
        }

        // Tries to release the lock
        // Will throw an exception if no lock was set
        public static void Release() {
            try {
                m?.ReleaseMutex();
            } catch {
            }
        }

        // Waits until the lock is released
        public static bool Wait(int timeout) {
            try {
                return m.WaitOne(timeout, false);
            } catch(AbandonedMutexException) {
                return true;
            } catch(InvalidOperationException) {
                return false;
            }
        }

    }

}
