  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using System.Text;
using OmenMon.Library;

namespace OmenMon.Hardware.Bios {

#region Interface
    // Defines an interface for using the BIOS for low-level hardware control
    // Builds up on the interface for interacting with the BIOS defined in IBios
    public interface IBiosCtl : IBios {

        // Backlight Control

        public BiosData.AnimTable GetAnimTable();
        public void SetAnimTable(BiosData.AnimTable data);

        public BiosData.Backlight GetBacklight();
        public void SetBacklight(BiosData.Backlight value);
        public void SetBacklight(bool value);

        public BiosData.ColorTable GetColorTable();
        public void SetColorTable(BiosData.ColorTable data);

        // Capability Query

        public BiosData.AdapterStatus GetAdapter();
        public BiosData.KbdType GetKbdType();
        public string GetMfgDate();
        public BiosData.SystemData GetSystem();

        public bool HasBacklight();
        public byte HasMemoryOverclock();
        public byte HasOverclock();
        public byte HasUndervoltBios();

        // Performance Control

        public void SetCpuPower(BiosData.CpuPowerData data);
        public void SetCpuPower1(byte value);
        public void SetCpuPower4(byte value);
        public void SetCpuPowerWithGpu(byte value);

        public BiosData.GpuMode GetGpuMode();
        public void SetGpuMode(BiosData.GpuMode value);

        public BiosData.GpuPowerData GetGpuPower();
        public void SetGpuPower(BiosData.GpuPowerData data);
        public void SetGpuPower(BiosData.GpuPowerLevel value);

        public void SetIdle(BiosData.Idle value);
        public void SetIdle(bool value);

        public void SetMemoryXmp(bool value);

        // Thermal Control

        public byte GetFanCount();
        public byte GetFanType();

        public byte[] GetFanLevel();
        public void SetFanLevel(byte[] data);

        public void SetFanMode(BiosData.FanMode value);

        public BiosData.FanTable GetFanTable();
        public void SetFanTable(BiosData.FanTable data);

        public bool GetMaxFan();
        public void SetMaxFan(bool value);

        public byte GetTemperature();

        public BiosData.Throttling GetThrottling();

    }
#endregion

    // Implements the functionality for adjusting hardware settings via the BIOS
    // Builds up on the functionality to make BIOS calls via CIM (WMI) defined in Bios
    // Indirectly builds up on the BIOS data constants & structures defined in BiosData
    // Retains the implementation from Bios to make arbitrary BIOS calls as defined in IBios
    public class BiosCtl : Bios, IBiosCtl {

#region Initialization & Disposal Methods
        // The following three statements ensure the class can be instantiated only once
        private static readonly BiosCtl instance = new BiosCtl();

        protected BiosCtl() { }

        public new static BiosCtl Instance {
            get { return instance; }
        }
#endregion

#region Backlight Control Methods
        // Retrieves the LED animation table
        public AnimTable GetAnimTable() {
            byte[] outData;
            Check(Send(Cmd.Keyboard, 0x06, new byte[4] {0x00, 0x00, 0x00, 0x00}, 128, out outData));
            // See: ColorTable (struct)
            return new AnimTable(outData);
        }

        // Updates the LED animation table
        public void SetAnimTable(AnimTable data) {
            // Updating the table yields no error but takes no effect
            Check(Send(Cmd.Keyboard, 0x07, Conv.GetByteArray(data)));
        }

        // Retrieves the keyboard backlight status
        public Backlight GetBacklight() {
            byte[] outData;
            Check(Send(Cmd.Keyboard, 0x04, new byte[4] {0x00, 0x00, 0x00, 0x00}, 4, out outData));
            // See: Backlight (enum)
            return (Backlight) outData[0];
        }

        // Sets the keyboard backlight status
        public void SetBacklight(Backlight value) {
            Check(Send(Cmd.Keyboard, 0x05, new byte[4] {(byte) value, 0x00, 0x00, 0x00}));
        }

        // Toggles the keyboard backlight status
        public void SetBacklight(bool value) {
            SetBacklight(value ? Backlight.On : Backlight.Off);
        }

        // Retrieves the keyboard backlight color table
        public ColorTable GetColorTable() {
            byte[] outData;
            Check(Send(Cmd.Keyboard, 0x02, new byte[4] {0x00, 0x00, 0x00, 0x00}, 128, out outData));
            // See: ColorTable (struct)
            return new ColorTable(outData);
        }

        // Updates the keyboard backlight color table
        public void SetColorTable(ColorTable data) {
            Check(Send(Cmd.Keyboard, 0x03, Conv.GetByteArray(data)));
        }
#endregion

#region Capability Query Methods
        // Retrieves the smart power adapter status
        public AdapterStatus GetAdapter() {
            byte[] outData;
            Check(Send(Cmd.Legacy, 0x0F, new byte[4] {0x00, 0x00, 0x00, 0x00}, 4, out outData));
            // See: AdapterStatus (enum)
            return (AdapterStatus) (byte) outData[0];
        }

        // Retrieves the keyboard type
        public KbdType GetKbdType() {
            byte[] outData;
            Check(Send(Cmd.Default, 0x2B, new byte[4] {0x00, 0x00, 0x00, 0x00}, 4, out outData));
            // See: KbdType (enum)
            return (KbdType) outData[0];
        }

        // Retrieves the manufacturing date, aka "Born-on Date" (BOD)
        public string GetMfgDate() {
            byte[] outData;
            Check(Send(Cmd.Legacy, 0x10, null, 128, out outData));
            // Bytes #0-#7: ASCII String "YYYYMMDD"
            return System.Text.Encoding.ASCII.GetString(outData, 0, 8);
        }

        // Retrieves the system design data
        public SystemData GetSystem() {
            byte[] outData;
            Check(Send(Cmd.Default, 0x28, null, 128, out outData));
            // See: SystemData (struct)
            return new SystemData(outData);
        }

        // Checks if keyboard backlight is supported
        public bool HasBacklight() {
            byte[] outData;
            Check(Send(Cmd.Keyboard, 0x01, new byte[4] {0x00, 0x00, 0x00, 0x00}, 4, out outData));
            // Byte #0 Bit #0: 0 - No Backlight Support, 1 - Backlight Support (Observed: 1)
            // Byte #0: Unknown (Observed: 0x07)
            // Byte #1: Unknown (Observed: 0x21)
            return Conv.GetBit(outData[0], 0);
        }

        // Checks if memory overclocking is supported
        public byte HasMemoryOverclock() {
            byte[] outData;
            Check(Send(Cmd.Default, 0x18, new byte[4] {0x00, 0x00, 0x00, 0x00}, 128, out outData));
            // Byte #2: 0x01 - Memory Overclocking Support (Observed: 0x00)
            return outData[2];
        }

        // Checks if overclocking is supported
        public byte HasOverclock() {
            byte[] outData;
            Check(Send(Cmd.Default, 0x35, new byte[4] {0x00, 0x00, 0x00, 0x00}, 128, out outData));
            // Byte #2: 0x00 - No Support (Observed: 0x03)
            return outData[2];
        }

        // Checks if the BIOS supports undervolting
        public byte HasUndervoltBios() {
            byte[] outData;
            Check(Send(Cmd.Default, 0x35, new byte[4] {0x00, 0x00, 0x00, 0x00}, 128, out outData));
            // Byte #2: 0x01 - BIOS Undervolting Support (Observed: 0x03)
            return outData[2];
        }
#endregion

#region Performance Control Methods
        // Updates the CPU power settings
        public void SetCpuPower(CpuPowerData data) {
            Check(Send(Cmd.Default, 0x29, Conv.GetByteArray(data)));
            // See: CpuPowerData (struct)
        }

        // Updates the CPU Power Limit 1 (PL1)
        public void SetCpuPower1(byte value) {
            CpuPowerData data = new CpuPowerData();
            data.Limit1 = value;
            data.Limit2 = value;
            SetCpuPower(data);
        }

        // Updates the CPU Power Limit 4 (PL4)
        public void SetCpuPower4(byte value) {
            CpuPowerData data = new CpuPowerData();
            data.Limit4 = value;
            SetCpuPower(data);
        }

        // Updates the concurrent CPU power limit shared with the GPU
        public void SetCpuPowerWithGpu(byte value) {
            CpuPowerData data = new CpuPowerData();
            data.LimitWithGpu = value;
            SetCpuPower(data);
        }

        // Retrieves the current graphics mode
        public GpuMode GetGpuMode() {
            byte[] outData;
            // This call returns BIOS error 4 on unsupported devices,
            // so do not check return state, just report "Hybrid" (0)
            Send(Cmd.Legacy, 0x52, null, 4, out outData);
            // See: GpuMode (enum)
            // Also see: GetSystem() Byte #7 Bit #3
            return (GpuMode) outData[0];
        }

        // Sets the graphics mode
        public void SetGpuMode(GpuMode value) {
            Check(Send(Cmd.GpuMode, 0x52, new byte[4] {(byte) value, 0x00, 0x00, 0x00}));
            // Note: This is not Advanced Optimus, a reboot is required
            // The functionality is equivalent to changing the setting in the BIOS
        }

        // Retrieves the current GPU power settings
        public GpuPowerData GetGpuPower() {
            byte[] outData;
            Check(Send(Cmd.Default, 0x21, new byte[4] {0x00, 0x00, 0x00, 0x00}, 4, out outData));
            // See: GpuPowerData (struct)
            return new GpuPowerData(outData);
        }

        // Updates the GPU power settings to those passed in a structure
        public void SetGpuPower(GpuPowerData data) {
            Check(Send(Cmd.Default, 0x22, Conv.GetByteArray(data)));
        }

        // Updates the GPU power settings to one of the presets
        public void SetGpuPower(GpuPowerLevel value) {
            SetGpuPower(new GpuPowerData(value));
        }

        // Sets the idle mode status
        public void SetIdle(Idle value) {
            byte[] outData;
            Check(Send(Cmd.Default, 0x31, new byte[4] {(byte) value, 0x00, 0x00, 0x00}, 4, out outData));
            // See Idle (enum)
        }

        // Toggles the idle mode on or off
        public void SetIdle(bool value) {
            SetIdle(value ? Idle.On : Idle.Off);
        }

        // Switches the memory between the XMP and the default profile
        public void SetMemoryXmp(bool value) {
            Check(Send(Cmd.Default, 0x19, new byte[4] {value ? (byte) 0x01 : (byte) 0x00, 0x00, 0x00, 0x00}));
            // Byte #0: 0x00 - Default Profile, 0x01 - XMP
        }
#endregion

#region Thermal Control Methods
        // Retrieves the number of fans
        public byte GetFanCount() {
            byte[] outData;
            Check(Send(Cmd.Default, 0x10, new byte[4] {0x00, 0x00, 0x00, 0x00}, 4, out outData));
            // Byte #0: Number of Fans (Observed: 0x02)
            return outData[0];
        }

        // Query the features of each fan
        public byte GetFanType() {
            byte[] outData;
            Check(Send(Cmd.Default, 0x2C, new byte[4] {0x00, 0x00, 0x00, 0x00}, 128, out outData));
            // Byte #0 Bits #0-3: Fan #1 Type (Observed: 0b0001 - CPU)
            // Byte #0 Bits #4-7: Fan #2 Type (Observed: 0b0010 - GPU)
            return outData[0];
        }

        // Retrieves the current speed level for each fan
        public byte[] GetFanLevel() {
            byte[] outData;
            Check(Send(Cmd.Default, 0x2D, new byte[4] {0x00, 0x00, 0x00, 0x00}, 128, out outData));
            // Byte #0: CPU Fan Speed Level (Observed: 0x00, 0x15 ... 0x37)
            // Byte #1: GPU Fan Speed Level (Observed: 0x00, 0x15 ... 0x39)
            return new byte[2] {outData[0], outData[1]};
        }

        // Updates the current speed level for each fan
        public void SetFanLevel(byte[] data) {
            Check(Send(Cmd.Default, 0x2E, new byte[4] {(byte) data[0], (byte) data[1], 0x00, 0x00}));
        }

        // Sets the active fan performance mode
        public void SetFanMode(FanMode value) {
            Check(Send(Cmd.Default, 0x1A, new byte[4] {0xFF, (byte) value, 0x00, 0x00}));
            // Input Byte #0: 0xFF - Constant (?)
            // See: FanMode (enum)
        }

        // Retrieves the fan speed level table
        public FanTable GetFanTable() {
            byte[] outData;
            Check(Send(Cmd.Default, 0x2F, new byte[4] {0x00, 0x00, 0x00, 0x00}, 128, out outData));
            // See: FanTable (struct)
            return new FanTable(outData);
        }

        // Updates the fan speed level table
        public void SetFanTable(FanTable data) {
            // Note: This appears to be a desktop-only functionality
            // Updating the table yields no error but takes no effect
            Check(Send(Cmd.Default, 0x32, Conv.GetByteArray(data)));
        }

        // Checks if the fans are running in maximum speed mode
        public bool GetMaxFan() {
            byte[] outData;
            Check(Send(Cmd.Default, 0x26, new byte[4] {0x00, 0x00, 0x00, 0x00}, 4, out outData));
            // Byte #0: 0x00 - Max Fan Speed Off, 0x01 - Max Fan Speed On
            return Conv.GetBit(outData[0], 0);
        }

        // Toggles the maximum fan speed mode on and off
        public void SetMaxFan(bool value) {
            Check(Send(Cmd.Default, 0x27, new byte[4] {value ? (byte) 0x01 : (byte) 0x00, 0x00, 0x00, 0x00}));
        }

        // Retrieves the current thermal sensor value
        public byte GetTemperature() {
            byte[] outData;
            Check(Send(Cmd.Default, 0x23, new byte[4] {0x01, 0x00, 0x00, 0x00}, 4, out outData));
            // Input Byte #0 & #1: Whether 0x00 or 0x01 all yield the same result
            // Output Byte #0: Thermal sensor value (Observed: 0x1D ... 0x31)
            return outData[0];
        }

        // Checks if the system is in performance throttling mode
        public Throttling GetThrottling() {
            byte[] outData;
            // This call returns BIOS error 6 on 2023 devices,
            // so do not check return state, just report "Unknown" (0)
            Send(Cmd.Default, 0x35, new byte[4] {0x00, 0x04, 0x00, 0x00}, 128, out outData);
            // Byte #1: See Throttling (enum)
            return (Throttling) outData[1];
        }
#endregion

    }

}
