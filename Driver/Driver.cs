  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

// This portion based on the code from Open Hardware Monitor, Copyright © 2009-2017 Michael Möller
// and its successor Libre Hardware Monitor, Copyright © 2020-2023 Libre Hardware Monitor & Contributors

// Licensed under the terms of the Mozilla Public License (MPL) 2.0: https://mozilla.org/MPL/2.0/

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using Microsoft.Win32.SafeHandles;
using OmenMon.External;

namespace OmenMon.Driver {

    // Implements the functionality for loading a driver into the kernel
    // and provides general wrapper device I/O control methods for it
    public class Driver {

        private readonly string id;
        private readonly string name;
        private SafeFileHandle handle;

        public bool IsOpen => handle != null;

        public Driver(string serviceName, string driverId) {
            name = serviceName;
            id = driverId;
        }

#region Driver Setup
        // Closes the kernel driver
        public void Close() {
            if(handle != null) {
                handle.Close();
                handle.Dispose();
                handle = null;
            }
        }

        // Deletes the kernel driver (not the driver file, handled within Driver.cs)
        public bool Delete() {
            IntPtr manager = AdvApi32.OpenSCManager(null, null, AdvApi32.SC_MANAGER_ACCESS_MASK.SC_MANAGER_CONNECT);
            if(manager == IntPtr.Zero)
                return false;

            IntPtr service = AdvApi32.OpenService(manager, name, AdvApi32.SERVICE_ACCESS_MASK.SERVICE_ALL_ACCESS);
            if(service == IntPtr.Zero) {
                AdvApi32.CloseServiceHandle(manager);
                return true;
            }

            AdvApi32.SERVICE_STATUS status = new();
            AdvApi32.ControlService(service, AdvApi32.SERVICE_CONTROL.SERVICE_CONTROL_STOP, ref status);
            AdvApi32.DeleteService(service);
            AdvApi32.CloseServiceHandle(service);
            AdvApi32.CloseServiceHandle(manager);
            return true;
        }

        // Installs the kernel driver (creates a service)
        public bool Install(string path, out string errorMessage) {
            IntPtr manager = AdvApi32.OpenSCManager(null, null, AdvApi32.SC_MANAGER_ACCESS_MASK.SC_MANAGER_CREATE_SERVICE);

            if(manager == IntPtr.Zero) {
                int errorCode = Marshal.GetLastWin32Error();
                errorMessage = $"OpenSCManager Error: {errorCode:X8}.";
                return false;
            }

            IntPtr service = AdvApi32.CreateService(
                manager,
                name,
                name,
                AdvApi32.SERVICE_ACCESS_MASK.SERVICE_ALL_ACCESS,
                AdvApi32.SERVICE_TYPE.SERVICE_KERNEL_DRIVER,
                AdvApi32.SERVICE_START.SERVICE_DEMAND_START,
                AdvApi32.SERVICE_ERROR.SERVICE_ERROR_NORMAL,
                path,
                null,
                null,
                null,
                null,
                null);

            if(service == IntPtr.Zero) {
                int errorCode = Marshal.GetLastWin32Error();
                if(errorCode == Kernel32.ERROR_SERVICE_EXISTS) {
                    errorMessage = "Service already exists";
                    return false;
                }
                errorMessage = $"CreateService Error: {errorCode:X8}.";
                AdvApi32.CloseServiceHandle(manager);
                return false;
            }

            if(!AdvApi32.StartService(service, 0, null)) {
                int errorCode = Marshal.GetLastWin32Error();
                if(errorCode != Kernel32.ERROR_SERVICE_ALREADY_RUNNING) {
                    errorMessage = $"StartService Error: {errorCode:X8}.";
                    AdvApi32.CloseServiceHandle(service);
                    AdvApi32.CloseServiceHandle(manager);
                    return false;
                }
            }

            AdvApi32.CloseServiceHandle(service);
            AdvApi32.CloseServiceHandle(manager);

            try { // Restrict driver access to System (SY) and Built-in Administrator (BA)
                FileInfo fileInfo = new(@"\\.\" + id);
                FileSecurity fileSecurity = fileInfo.GetAccessControl();
                fileSecurity.SetSecurityDescriptorSddlForm("O:BAG:SYD:(A;;FA;;;SY)(A;;FA;;;BA)");
                fileInfo.SetAccessControl(fileSecurity);
            } catch {
            }

            errorMessage = null;
            return true;
        }

        // Opens the kernel driver
        public bool Open() {
            IntPtr fileHandle = Kernel32.CreateFile(@"\\.\" + id, 0xC0000000, FileShare.None, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);

            handle = new SafeFileHandle(fileHandle, true);
            if(handle.IsInvalid)
                Close();

            return handle != null;
        }
#endregion

#region Device I/O Control Methods
        // Performs I/O control operations given a buffer
        public bool DeviceIOControl(Kernel32.IOControlCode ioControlCode, object inBuffer) {
            return handle != null && Kernel32.DeviceIoControl(
                handle,
                ioControlCode,
                inBuffer,
                inBuffer == null ? 0 : (uint) Marshal.SizeOf(inBuffer),
                null,
                0,
                out uint _,
                IntPtr.Zero);
        }

        // Performs I/O control operations given a buffer reference
        public bool DeviceIOControl<T>(Kernel32.IOControlCode ioControlCode, object inBuffer, ref T outBuffer) {
            if(handle == null)
                return false;

            object boxedOutBuffer = outBuffer;
            bool b = Kernel32.DeviceIoControl(
                handle,
                ioControlCode,
                inBuffer,
                inBuffer == null ? 0 : (uint) Marshal.SizeOf(inBuffer),
                boxedOutBuffer,
                (uint) Marshal.SizeOf(boxedOutBuffer),
                out uint _,
                IntPtr.Zero);

            outBuffer = (T)boxedOutBuffer;
            return b;
        }

        // Performs I/O control operations given a buffer array reference
        public bool DeviceIOControl<T>(Kernel32.IOControlCode ioControlCode, object inBuffer, ref T[] outBuffer) {
            if(handle == null)
                return false;

            object boxedOutBuffer = outBuffer;
            bool b = Kernel32.DeviceIoControl(
                handle,
                ioControlCode,
                inBuffer,
                inBuffer == null ? 0 : (uint) Marshal.SizeOf(inBuffer),
                boxedOutBuffer,
                (uint) (Marshal.SizeOf(typeof(T)) * outBuffer.Length),
                out uint _,
                IntPtr.Zero);

            outBuffer = (T[])boxedOutBuffer;
            return b;
        }
#endregion

    }

}
