  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023-2024 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using OmenMon.Driver;
using OmenMon.Library;

namespace OmenMon.Hardware.Ec {

#region Interface
    // Defines an interface for interacting with the Embedded Controller
    public interface IEmbeddedController : IDisposable {

        public bool IsInitialized { get; }

        public void Initialize();
        public void Close();

        // Lock
        public bool Request(int timeout);
        public void Release();

        // Read
        public byte ReadByte(byte register);
        public ushort ReadWord(byte register);

        // Write
        public void WriteByte(byte register, byte value);
        public void WriteWord(byte register, ushort value);

    }
#endregion

    // Implements as much of Embedded Controller functionality as possible
    // without getting into the kernel driver-specific routines
    // Builds up on the Embedded Controller data values and structures defined earlier
    public abstract class EmbeddedControllerAbstract 
        : EmbeddedControllerData, IEmbeddedController {

        public bool IsInitialized { get; protected set; }

        // Global counter of failed waiting to read attempts
        protected int WaitReadFailCount = 0;

#region Abstract Methods
        // Initialization and disposal
        // Implementation is driver-specific
        public abstract void Initialize();
        public abstract void Close();

        // Mutex lock request and release
        // Implementation depends on the EmbeddedControllerMutex class
        public abstract bool Request(int timeout);
        public abstract void Release();

        // Actual driver-specific read and write routines
        protected abstract byte ReadIoPort(Port port);
        protected abstract void WriteIoPort(Port port, byte value);

        // Dispose() is just a wrapper for Close()
        public virtual void Dispose() {
            Close();
        }
#endregion

#region Public Read & Write Methods
        // Wrapper to read a byte from an Embedded Controller register
        public virtual byte ReadByte(byte register) {
            int count = 0;
            byte value = 0;
            while(count < Config.EcRetryLimit) {
                if(ReadByteImpl(register, out value))
                    return value;
                count++;
            }
            return value;
        }

        // Wrapper to read a word (two bytes) from an Embedded Controller register
        public virtual ushort ReadWord(byte register) {
            int count = 0;
            ushort value = 0;
            while(count < Config.EcRetryLimit) {
                if(ReadWordImpl(register, out value))
                    return (ushort) value;
                count++;
            }
            return (ushort) value;
        }

        // Wrapper to write a byte to an Embedded Controller register
        public virtual void WriteByte(byte register, byte value) {
            int count = 0;
            while(count < Config.EcRetryLimit) {
                if(WriteByteImpl(register, value))
                    return;
                count++;
            }
        }

        // Wrapper to write a word (two bytes) to an Embedded Controller register
        public virtual void WriteWord(byte register, ushort value) {
            int count = 0;
            while(count < Config.EcRetryLimit) {
                if(WriteWordImpl(register, value))
                    return;
                count++;
            }
        }
#endregion

#region Protected Read & Write Implementation Methods
        // Reads a byte from an Embedded Controller register
        protected bool ReadByteImpl(byte register, out byte value) {
            if(WaitWrite()) {
                WriteIoPort(Port.Command, (byte) Command.Read);
                if(WaitWrite()) {
                    WriteIoPort(Port.Data, register);
                    if(WaitWrite() && WaitRead()) {
                        value = ReadIoPort(Port.Data);
                        return true;
                    }
                }
            }
            value = 0;
            return false;
        }

        // Reads a word (two bytes) from an Embedded Controller register
        protected bool ReadWordImpl(byte register, out ushort value) {
            byte result = 0;
            value = 0;
            if(!ReadByteImpl(register, out result))
                return false;
            value = result;
            if(!ReadByteImpl((byte) (register + 1), out result))
                return false;
            value |= (ushort) (result << 8);
            return true;
        }

        // Writes a byte to an Embedded Controller register
        protected bool WriteByteImpl(byte register, byte value) {
            if(WaitWrite()) {
                WriteIoPort(Port.Command, (byte) Command.Write);
                if(WaitWrite()) {
                    WriteIoPort(Port.Data, register);
                    if(WaitWrite()) {
                        WriteIoPort(Port.Data, value);
                        return true;
                    }
                }
            }
            return false;
        }

        // Writes a word (two bytes) to an Embedded Controller register
        protected bool WriteWordImpl(byte register, ushort value) {
            byte high = (byte) (value >> 8);
            byte low = (byte) value;
            if(!WriteByteImpl(register, low))
                return false;
            if(!WriteByteImpl((byte) (register + 1), high))
                return false;
            return true;
        }
#endregion

#region Protected Wait Methods
        // Waits until the Embedded Controller is in a suitable state
        protected bool Wait(Status status, bool isSet) {
            for (int i = 0; i < Config.EcWaitLimit; i++) {
                byte value = ReadIoPort(Port.Command);

                if(isSet)
                    value = (byte) ~value;

                if(((byte) status & value) == 0)
                    return true;

                // Alternatively, a much less legible one-liner:
                // if(((byte) status & (isSet ? (byte) ~value : value)) == 0)
                //     return true;

                // Also in the updated version:
                // Thread.Sleep(1); // using System.Threading;
            }
            return false;
        }

        // Waits for a read operation
        protected bool WaitRead() {
            if(WaitReadFailCount > Config.EcFailLimit) {
                return true;
            } else if(Wait(Status.OutFull, true)) {
                WaitReadFailCount = 0;
                return true;
            } else {
                WaitReadFailCount++;
                return false;
            }
        }

        // Waits for a write operation
        protected bool WaitWrite() {
            return Wait(Status.InFull, false);
        }
#endregion

    }

#region Driver Implementation
    // Links the abstract Embedded Controller implementation
    // to the low-level routines in the Ring0 kernel driver
    public sealed class EmbeddedController : EmbeddedControllerAbstract, IEmbeddedController {

        // The following three statements ensure the class can be instantiated only once
        private static readonly EmbeddedController instance = new EmbeddedController();

        private EmbeddedController() { }

        public static EmbeddedController Instance {
            get { return instance; }
        }

        // Initializes the kernel driver and creates a lock on the Embedded Controller
        public override void Initialize() {
            if(!this.IsInitialized) {
                Ring0.Open();
                if(Ring0.IsOpen) {
                    this.IsInitialized = true;
                    EmbeddedControllerMutex.Open();
                } else {
                    // Report driver installation failure details
                    App.Error(Ring0.GetStatus());
                }
            }
        }

        // Closes the kernel driver and clears the Embedded Controller lock
        public override void Close() {
            if(this.IsInitialized) {
                this.IsInitialized = false;
                try {
                    EmbeddedControllerMutex.Close();
                } catch {
                }
                try {
                    Ring0.Close();
                } catch {
                }
            }
        }

        // Requests a lock on the Embedded Controller
        public override bool Request(int timeout) {
            return EmbeddedControllerMutex.Wait(timeout);
        }

        // Releases a lock on the Embedded Controller
        public override void Release() {
            EmbeddedControllerMutex.Release();
        }

        // Wrapper for the I/O port read routine in the kernel driver
        protected override byte ReadIoPort(Port port) {
            return Ring0.ReadIoPort((uint) port);
        }

        // Wrapper for the I/O port write routine in the kernel driver
        protected override void WriteIoPort(Port port, byte value) {
            Ring0.WriteIoPort((uint) port, value);
        }

    }
#endregion

}
