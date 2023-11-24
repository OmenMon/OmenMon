  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using OmenMon.Hardware.Bios;
using OmenMon.Library;

namespace OmenMon.Hardware.Platform {

    // Defines an interface for obtaining system information
    public interface ISettings {

        // API queries
        public bool IsFullPower();

        // BIOS raw data
        public Nullable<BiosData.GpuMode> GpuMode { get; }
        public Nullable<BiosData.GpuPowerData> GpuPower { get; }
        public Nullable<BiosData.SystemData> SystemData { get; }

        // BIOS queries
        public BiosData.AdapterStatus GetAdapterStatus();  // Smart AC adapter status
        public string GetBornDate();                       // "Born-on" date
        public byte GetDefaultCpuPowerLimit4();            // CPU Power Limit 4 default value
        public BiosData.SystemData GetSystemData();        // System information from the BIOS
        public BiosData.Throttling GetThrottling();        // Whether the system is throttling

        // BIOS GPU queries
        public BiosData.GpuMode GetGpuMode(bool forceUpdate = false);            // Optimus or Discrete
        public bool GetGpuModeSupport();
        public void SetGpuMode(BiosData.GpuMode value);
        public BiosData.GpuCustomTgp GetGpuCustomTgp(bool forceUpdate = false);  // Custom Total Graphics Power
        public BiosData.GpuDState GetGpuDState(bool forceUpdate = false);        // Device power state
        public BiosData.GpuPpab GetGpuPpab(bool forceUpdate = false);            // Processing Power AI Boost
        public BiosData.GpuPowerData GetGpuPower(bool forceUpdate = false);      // DState, Custom TGP & PPAB
        public void SetGpuPower(BiosData.GpuPowerData value);

        // BIOS keyboard queries
        public BiosData.Backlight GetKbdBacklight();            // Backlight status
        public bool GetKbdBacklightSupport();
        public void SetKbdBacklight(bool flag);
        public void SetKbdBacklight(BiosData.Backlight value);
        public BiosData.ColorTable GetKbdColor();               // Backlight color
        public bool GetKbdColorSupport();
        public void SetKbdColor(BiosData.ColorTable value);
        public BiosData.KbdType GetKbdType();                   // Keyboard type

        // WMI raw data
        public Dictionary<string, string> BaseBoard { get; }

        // WMI queries
        public string GetManufacturer();  // Baseboard manufacturer name
        public string GetProduct();       // Baseboard product identifier
        public string GetSerial();        // Baseboard serial number
        public string GetVersion();       // Baseboard version identifier

    }

    // Implements a mechanism for obtaining system information
    public class Settings : ISettings {

        // WMI data identifiers
        public const string WMI_BASEBOARD_MANUFACTURER = "Manufacturer";
        public const string WMI_BASEBOARD_PRODUCT = "Product";
        public const string WMI_BASEBOARD_SERIAL = "SerialNumber";
        public const string WMI_BASEBOARD_VERSION = "Version";

        // WMI raw information
        public Dictionary<string, string> BaseBoard { get; private set; }

        // BIOS raw information
        private string BornDate;
        public Nullable<BiosData.GpuMode> GpuMode { get; private set; }
        public Nullable<BiosData.GpuPowerData> GpuPower { get; private set; }
        public Nullable<BiosData.KbdType> KbdType { get; private set; }
        public Nullable<BiosData.SystemData> SystemData { get; private set; }

        // Constructs a system information instance
        public Settings() {

            // Set cached data to initial values
            this.BornDate = "";
            this.GpuMode = null;
            this.GpuPower = null;
            this.SystemData = null;

            // Create an instance to query WMI for information
            using WmiInfo wmiInfo = new WmiInfo();

            // Obtain baseboard information
            this.BaseBoard = wmiInfo.GetBaseBoard();

            // Handle the case of empty baseboard data
            if(!this.BaseBoard.ContainsKey(WMI_BASEBOARD_MANUFACTURER))
                this.BaseBoard[WMI_BASEBOARD_MANUFACTURER] = "?";
            if(!this.BaseBoard.ContainsKey(WMI_BASEBOARD_PRODUCT))
                this.BaseBoard[WMI_BASEBOARD_PRODUCT] = "?";
            if(!this.BaseBoard.ContainsKey(WMI_BASEBOARD_SERIAL))
                this.BaseBoard[WMI_BASEBOARD_SERIAL] = "?";
            if(!this.BaseBoard.ContainsKey(WMI_BASEBOARD_VERSION))
                this.BaseBoard[WMI_BASEBOARD_VERSION] = "?";

        }

        // Checks if the system is running with full power
        // (AC power check now, can extend to smart AC adapter status)
        public bool IsFullPower() {
            return SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online;
        }

        // Retrieves the smart AC adapter status
        public BiosData.AdapterStatus GetAdapterStatus() {
            return Hw.BiosGet<BiosData.AdapterStatus>(Hw.Bios.GetAdapter);
        }

        // Retrieves the "born-on" date
        public string GetBornDate() {
            if(this.BornDate == "")
                this.BornDate = Hw.BiosGet(Hw.Bios.GetBornDate);
            return this.BornDate;
        }

        // Retrieves the default CPU Power Limit 4 value
        public byte GetDefaultCpuPowerLimit4() {
            return GetSystemData().DefaultCpuPowerLimit4;
        }

        // Queries the Custom Total Graphics Power state
        public BiosData.GpuCustomTgp GetGpuCustomTgp(bool forceUpdate = false) {
            return GetGpuPower(forceUpdate).CustomTgp;
        }

        // Queries the device power state
        public BiosData.GpuDState GetGpuDState(bool forceUpdate = false) {
            return GetGpuPower(forceUpdate).DState;
        }

        // Queries the GPU mode (Optimus or Discrete)
        public BiosData.GpuMode GetGpuMode(bool forceUpdate = false) {
            if(forceUpdate || this.GpuMode == null)
                this.GpuMode = Hw.BiosGet<BiosData.GpuMode>(Hw.Bios.GetGpuMode);
            return (BiosData.GpuMode) this.GpuMode;
        }

        // Checks whether GPU mode switching is supported
        public bool GetGpuModeSupport() {
            // BiosData.SysGpuModeSwitch
            // == 0x0C: Observed on model 8A14 where switching is supported
            // == 0x08: Interpreted as support flag based on original information
            // == 0x06: Also means supported based on user reports, add 0x04 to flags
            return ((byte) (GetSystemData().GpuModeSwitch &
                (BiosData.SysGpuModeSwitch.Supported4 | BiosData.SysGpuModeSwitch.Supported8)) != 0);
        }

        // Sets the GPU mode (Optimus or Discrete)
        public void SetGpuMode(BiosData.GpuMode value) {
            Hw.BiosSet<BiosData.GpuMode>(Hw.Bios.SetGpuMode, value);
        }

        // GPU power (Custom TGP & PPAB)
        public BiosData.GpuPowerData GetGpuPower(bool forceUpdate = false) {
            if(forceUpdate || this.GpuPower == null)
                this.GpuPower = Hw.BiosGet<BiosData.GpuPowerData>(Hw.Bios.GetGpuPower);
            return (BiosData.GpuPowerData) this.GpuPower;
        }

        // Sets the GPU power (Custom TGP & PPAB)
        public void SetGpuPower(BiosData.GpuPowerData value) {
            Hw.BiosSetStruct(Hw.Bios.SetGpuPower, value);
        }

        // Queries the Processing Power AI Boost state
        public BiosData.GpuPpab GetGpuPpab(bool forceUpdate = false) {
            return GetGpuPower(forceUpdate).Ppab;
        }

        // Queries the keyboard backlight statuss
        public BiosData.Backlight GetKbdBacklight() {
            return Hw.BiosGet<BiosData.Backlight>(Hw.Bios.GetBacklight);
        }

        // Checks whether keyboard backlight toggling is supported
        public bool GetKbdBacklightSupport() {
            // Per-key RGB keyboards currently not supported,
            // as well as any devices that report no backlight
            // (or if the BIOS call fails when trying to query)
            return GetKbdType() != BiosData.KbdType.PerKeyRgb
                && Hw.BiosGet<bool>(Hw.Bios.HasBacklight);
        }

        // Sets the keyboard backlight status given an enumerated value
        public void SetKbdBacklight(BiosData.Backlight value) {
            Hw.BiosSet(Hw.Bios.SetBacklight, value);
        }

        // Sets the keyboard backlight status given a Boolean flag
        public void SetKbdBacklight(bool flag) {
            Hw.BiosSet(Hw.Bios.SetBacklight, flag ?
               BiosData.Backlight.On : BiosData.Backlight.Off);
        }

        // Queries the keyboard backlight color
        public BiosData.ColorTable GetKbdColor() {
            return Hw.BiosGetStruct<BiosData.ColorTable>(Hw.Bios.GetColorTable);
        }

        // Checks whether keyboard color switching is supported
        public bool GetKbdColorSupport() {
            // All keyboards that don't support backlight toggling,
            // don't support color setting either
            return GetKbdBacklightSupport();
        }

        // Sets the keyboard backlight color
        public void SetKbdColor(BiosData.ColorTable value) {
            Hw.BiosSetStruct(Hw.Bios.SetColorTable, value);

        }

        // Retrieves keyboard type from the BIOS
        public BiosData.KbdType GetKbdType() {
            if(this.KbdType == null)
                this.KbdType = Hw.BiosGet<BiosData.KbdType>(Hw.Bios.GetKbdType);
            return (BiosData.KbdType) this.KbdType;
        }

        // Retrieves the baseboard manufacturer name
        public string GetManufacturer() {
            return this.BaseBoard[WMI_BASEBOARD_MANUFACTURER];
        }

        // Retrieves the baseboard product identifier
        public string GetProduct() {
            return this.BaseBoard[WMI_BASEBOARD_PRODUCT];
        }

        // Retrieves the baseboard serial number
        public string GetSerial() {
            return this.BaseBoard[WMI_BASEBOARD_SERIAL];
        }

        // Retrieves system data from the BIOS
        public BiosData.SystemData GetSystemData() {
            if(this.SystemData == null)
                this.SystemData = Hw.BiosGetStruct<BiosData.SystemData>(Hw.Bios.GetSystem);
            return (BiosData.SystemData) this.SystemData;
        }

        // Queries whether the system is throttling
        public BiosData.Throttling GetThrottling() {
            return Hw.BiosGet<BiosData.Throttling>(Hw.Bios.GetThrottling);
        }

        // Retrieves the baseboard version identifier
        public string GetVersion() {
            return this.BaseBoard[WMI_BASEBOARD_VERSION];
        }

    }

}
