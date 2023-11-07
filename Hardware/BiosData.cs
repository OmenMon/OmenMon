  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using System.Runtime.InteropServices;
using OmenMon.Library;

namespace OmenMon.Hardware.Bios {

    // Defines BIOS constants, variables, and structures
    // for subsequent use by the BIOS-handling routines
    public abstract class BiosData {

#region BIOS Access Data
        // Pre-defined shared secret for authorization
        protected static readonly byte[] Sign = new byte[4] {
            0x53, 0x45, 0x43, 0x55 // (= 83, 69, 67, 85)
        };

        // Command identifier
        public enum Cmd : uint {
            Default   = 0x20008,  // Most commands (131080)
            Keyboard  = 0x20009,  // Keyboard-related (131081)
            Legacy    = 0x00001,  // Earliest implemented (1)
            GpuMode   = 0x00002   // Graphics mode switch (2)
        }

        // WMI BIOS routine identifiers, constant
        protected const string BIOS_DATA = "hpqBDataIn";
        protected const string BIOS_DATA_FIELD = "hpqBData";
        protected const string BIOS_METHOD = "hpqBIOSInt";
        protected const string BIOS_METHOD_CLASS = "hpqBIntM";
        protected const string BIOS_METHOD_INSTANCE = "ACPI\\PNP0C14\\0_0";
        protected const string BIOS_NAMESPACE = "root\\wmi";
        protected const string BIOS_RETURN_CODE_FIELD = "rwReturnCode";
#endregion

#region BIOS Control Data
        // Keyboard backlight toggle
        public enum Backlight : byte {
            Off = 0x64,  // 0b01100100 - Keyboard backlight off
            On  = 0xE4   // 0b11100100 - Keyboard backlight on
        }

        // Fan performance mode
        // Source: HP.Omen.Core.Common.PowerControl.PerformanceMode
        public enum FanMode : byte {
            LegacyDefault     =  0,  // 0x00 = 0b00000000
            LegacyPerformance =  1,  // 0x01 = 0b00000001
            LegacyCool        =  2,  // 0x02 = 0b00000010
            LegacyQuiet       =  3,  // 0x03 = 0b00000011
            LegacyExtreme     =  4,  // 0x04 = 0b00000100
            L8                =  4,  // 0x04 = 0b00000100
            L0                = 16,  // 0x10 = 0b00010000
            L5                = 17,  // 0x11 = 0b00010001
            L1                = 32,  // 0x20 = 0b00100000
            L6                = 33,  // 0x21 = 0b00100001
            Default           = 48,  // 0x30 = 0b00110000
            L2                = 48,  // 0x30 = 0b00110000
            Performance       = 49,  // 0x31 = 0b00110001 
            L7                = 49,  // 0x31 = 0b00110001 
            L3                = 64,  // 0x40 = 0b01000000
            Cool              = 80,  // 0x50 = 0b01010000
            L4                = 80   // 0x50 = 0b01010000
        }

        // Fan type (per nibble)
        public enum FanType : byte {
            Unsupported = 0x00,  // No fan support
            Cpu         = 0x01,  // Is a CPU fan
            Gpu         = 0x02,  // Is a GPU fan
            Exhaust     = 0x03,  // Is an exhaust fan
            Pump        = 0x04,  // Is a pump fan
            Intake      = 0x05   // Is an intake fan
        }

        // Graphics mode (predates Advanced Optimus)
        // Source: HP.Omen.Core.Model.DataStructure.Modules.GraphicsSwitcher
        public enum GpuMode : byte {
            Hybrid   = 0x00,  // Hybrid graphics mode (or BIOS call failed)
            Discrete = 0x01,  // Discrete GPU exclusive mode
            Optimus  = 0x02   // nVidia Optimus mode
        }

        // Idle mode status
        public enum Idle : byte {
            Off = 0x00,  // Disabled
            On  = 0x01   // Enabled
        }

        // Keyboard type
        public enum KbdType : byte {
            Standard   = 0x00,  // Standard layout
            WithNumPad = 0x01,  // Standard layout with numerical block
            TenKeyLess = 0x02,  // Extra navigation keys but no numerical block
            PerKeyRgb  = 0x03   // Independently-definiable color for each key (?)
        }

        // Keyboard backlight color zone
        public enum KbdZone : byte {
            Right  = 0x00,  // Arrows, navigation block, right-hand modifier keys
            Middle = 0x01,  // Right-hand QWERTY block (F6-F12), delimited by the keys T, G, and B
            Left   = 0x02,  // Left-hand QWERTY block (F1-F5), delimited by the keys R, F, and V
            Wasd   = 0x03   // The keys W, A, S, and D
        }

        // Smart power adapter status
        public enum AdapterStatus : byte {
            NotSupported     = 0x00,  // No smart power adapter support
            MeetsRequirement = 0x01,  // Sufficient power
            BelowRequirement = 0x02,  // Insufficient power
            BatteryPower     = 0x03,  // Not on AC power
            NotFunctioning   = 0x04,  // Malfunction
            Error            = 0xFF   // Error
        }

        // Throttling
        public enum Throttling : byte {
            Unknown = 0x00,  // Unknown state (BIOS call failed)
            On      = 0x01,  // Thermal throttling enabled
            Default = 0x04   // Observed default state
        }

        // Thermal policy version
        public enum ThermalPolicyVersion : byte {
            V0 = 0x00,  // Legacy devices
            V1 = 0x01,  // Current devices
        }
#endregion

#region BIOS Control Data - Animation Table
        // LED Animation table data type
        // The format is undocumented (only zero values observed)
        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 128)]
        public struct AnimTable {

            // Fan speed level raw data array
            // Note: SizeConst has to be defined at compilation time
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public byte[] Raw;

            // Initializes an empty LED animation table
            public AnimTable() {
                Raw = new byte[128];
            }

            // Initializes a fan table from a 128-byte array
            public AnimTable(byte[] data) {
                Raw = new byte[128];

                // Allow for a smaller array length than the maximum
                Array.Copy(data, 0, Raw, 0, data.Length < 128 ? data.Length : 128);
            }

        }
#endregion

#region BIOS Control Data - Color Table
        // RGB color value 24-bit data type
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct RgbColor {

            public byte Red, Green, Blue;

            // Initializes the data given the value for each color
            public RgbColor(byte red, byte green, byte blue) {
                Red = red;
                Green = green;
                Blue = blue;
            }

            // Initializes the data given a 32-bit RGB color value
            // Alpha channel (transparency information) is discarded
            public RgbColor(uint value) {
                Red = (byte) (value & byte.MaxValue);
                Green = (byte) ((value >> 8) & byte.MaxValue);
                Blue = (byte) ((value >> 16) & byte.MaxValue);
            }

            // Initializes the data given a 32-bit BGR color value
            // Alpha channel (transparency information) is discarded
            public RgbColor(uint value, bool reverse) {
                // The reverse flag is purposefully never checked
                Red = (byte) ((value >> 16) & byte.MaxValue);
                Green = (byte) ((value >> 8) & byte.MaxValue);
                Blue = (byte) (value & byte.MaxValue);
            }

            // Returns a 32-bit RGB color value
            public uint Value { get { return (uint) (Red | ( Green << 8 ) | (Blue << 16)); } }

            // Returns a 32-bit BGR color value
            public uint ValueReverse { get { return (uint) (Blue | ( Green << 8 ) | (Red << 16)); } }

        }

        // Padding of the color table
        // Constant but defined here since it appears several times
        public const int COLOR_TABLE_PAD = 24;

        // Keyboard backlight color table data type
        // Consistent with how the data is stored by the BIOS
        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 128)]
        public struct ColorTable {
            public byte ZoneCount; // Number of color zones

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = COLOR_TABLE_PAD, ArraySubType = UnmanagedType.U1)]
            byte[] Padding; // Has to be this way instead of LayoutKind.Explicit, alignment exception otherwise

            // See KbdZone (enum)
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] // SizeConst has to be defined at compilation time
            public RgbColor[] Zone;

            // Initializes an empty color table
            public ColorTable() {
                ZoneCount = (byte) (KbdZone.GetValues(typeof(KbdZone)).Length - 1);
                Zone = new RgbColor[ZoneCount + 1];
            }

            // Initializes a color table from a byte array
            public ColorTable(byte[] data) {

                // Retrieve the zone count and set up the zones
                ZoneCount = data[0];
                Zone = new RgbColor[ZoneCount + 1];

                // Populate the RGB values for each zone
                for(int i = 0; i <= ZoneCount; i++) {
                    Zone[i] = new RgbColor(
                        data[COLOR_TABLE_PAD + 3 * i + 1],   // Red
                        data[COLOR_TABLE_PAD + 3 * i + 2],   // Green
                        data[COLOR_TABLE_PAD + 3 * i + 3]);  // Blue
                }

            }

            // Initializes a color table from an array
            public ColorTable(int[] color, bool reverse = false) {
                ZoneCount = (byte) (color.Length - 1);
                Zone = new RgbColor[ZoneCount + 1];

                // Only four-zone backlight is supported, bail out otherwise
                if(ZoneCount > 3) throw new ArgumentOutOfRangeException();

                // Populate the color table with data
                for(int i = 0; i < Zone.Length; i++)
                    Zone[i] = new BiosData.RgbColor((uint) color[i], reverse);

            }

            // Initializes a color table from a string
            public ColorTable(string param) {
                ZoneCount = (byte) (KbdZone.GetValues(typeof(KbdZone)).Length - 1);
                Zone = new RgbColor[ZoneCount + 1];
                int i = 0;

                // Populate the color table with data
                foreach(string color in param.Split(':')) {

                    // Only four-zone backlight is supported, bail out otherwise
                    if(i > 3) throw new ArgumentOutOfRangeException();

                    // Add the color data for the zone
                    Zone[i] = new RgbColor(Convert.ToUInt32(color, 16), true);

                    i++;

                }

            }

        }
#endregion

#region BIOS Control Data - Fan Table
        // Fan 1 & 2 speed level for a given temperature readout data type
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct FanLevel {

            public byte Fan1Level, Fan2Level, Temperature;

            // Initializes the data given the value for each element
            public FanLevel(byte fan1Level, byte fan2Level, byte temperature) {
                Fan1Level = fan1Level;
                Fan2Level = fan2Level;
                Temperature = temperature;
            }

            // Initializes the data given a byte array with values
            public FanLevel(byte[] data) {
                Fan1Level = data[0];
                Fan2Level = data[1];
                Temperature = data[2];
            }

        }

        // Fan speed level table data type
        // Consistent with how the data is stored by the BIOS
        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 128)]
        public struct FanTable {
            public byte FanCount; // Number of fans (2)
            public byte LevelCount; // Number of level entries (14)

            // Fan speed level entry array
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)] // SizeConst has to be defined at compilation time
            public FanLevel[] Level;

            // Initializes an empty fan table
            public FanTable() {
                FanCount = (byte) 2;
                Level = new FanLevel[14];
            }

            // Initializes a fan table from a byte array
            public FanTable(byte[] data) {

                // Retrieve the fan count, level count, and set up the levels
                FanCount = data[0];
                LevelCount = data[1];
                Level = new FanLevel[LevelCount];

                // Populate the fan speed level and temperature values for each level
                for(int i = 0; i < LevelCount; i++) {
                    Level[i] = new FanLevel(
                        data[2 + 3 * i + 0],   // Fan 1 speed level
                        data[2 + 3 * i + 1],   // Fan 2 speed level
                        data[2 + 3 * i + 2]);  // Temperature
                }

            }

        }
#endregion

#region BIOS Control Data - Power - CPU
        // CPU power settings data structure
        // Consistent with how the data is stored by the BIOS
        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4)]
        public struct CpuPowerData {
            public byte Limit1;        // Power Limit 1 (PL1)
            public byte Limit2;        // Power Limit 2 (PL2) [Observed being set equal to PL1]
            public byte Limit4;        // Power Limit 4 (PL4)
            public byte LimitWithGpu;  // Concurrent power limit shared with the GPU

            // Initializes an empty GPU power state structure
            public CpuPowerData() {
                // 0xFF - No change
                Limit1 = 0xFF;
                Limit2 = 0xFF;
                Limit4 = 0xFF;
                LimitWithGpu = 0xFF;
            }

            // Initializes the GPU power state structure from a data array
            public CpuPowerData(byte[] data) {
                Limit1 = data[0];
                Limit2 = data[1];
                Limit4 = data[2];
                LimitWithGpu = data[3];
            }
        }
#endregion

#region BIOS Control Data - Power - GPU
        // Custom Total Graphics Power (TGP) limit switch
        public enum GpuCustomTgp : byte {
            Off = 0x00,  // Base TGP only
            On  = 0x01   // Custom TGP enabled
        }

        // GPU device power state list
        public enum GpuDState : byte {
            D1 = 0x01,  // Device power state 1
            D2 = 0x02,  // Device power state 2
            D3 = 0x03,  // Device power state 3
            D4 = 0x04,  // Device power state 4
            D5 = 0x05   // Device power state 5
        }

        // Processing Power AI Boost (PPAB) switch
        public enum GpuPpab : byte {
            Off = 0x00,  // Boost disabled
            On  = 0x01   // Boost enabled
        }

        // GPU Power Settings
        public enum GpuPowerLevel : byte {
            Minimum = 0x00,  // Base TGP only
            Medium  = 0x01,  // Custom TGP
            Maximum = 0x02   // Custom TGP & PPAB
        }

        // Graphics power settings data structure
        // Consistent with how the data is stored by the BIOS
        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4)]
        public struct GpuPowerData {
            public GpuCustomTgp CustomTgp;  // Custom Total Graphics Power (TGP) limit
            public GpuPpab Ppab;            // Processing Power AI Boost (PPAB)
            public GpuDState DState;        // GPU device power state
            public byte PeakTemperature;    // Sensor threshold, observed: 75°C (0x4B), 87°C (0x57)

            // Initializes an empty GPU power state structure
            public GpuPowerData() {
                CustomTgp = GpuCustomTgp.Off;
                Ppab = GpuPpab.Off;
                DState = GpuDState.D1;
                PeakTemperature = 0;
            }

            // Initializes the GPU power state structure based on a preset
            public GpuPowerData(GpuPowerLevel Level) {
                CustomTgp = Level == GpuPowerLevel.Minimum ? GpuCustomTgp.Off : GpuCustomTgp.On;
                Ppab = Level == GpuPowerLevel.Maximum ? GpuPpab.On : GpuPpab.Off;
                DState = GpuDState.D1;
                PeakTemperature = 0;
            }

            // Initializes the GPU power state structure from a data array
            public GpuPowerData(byte[] data) {
                CustomTgp = (GpuCustomTgp) data[0];
                Ppab = (GpuPpab) data[1];
                DState = (GpuDState) data[2];
                PeakTemperature = data[3];
            }

        }
#endregion

#region BIOS Control Data - System Design Data
        // BIOS-defined overclocking support
        // Observed: 0x00
        public enum SysBiosOc : byte {
            NotSupported = 0x00,  // No
            Supported    = 0x01   // Yes
        }

        // Graphics switching support
        // Observed: 0x0C = 0b00001100
        [Flags]
        public enum SysGpuModeSwitch : byte {
            Unset0    = 0x01,  // Bit #0: Observed 0: Unset
            Unset1    = 0x02,  // Bit #1: Observed 0: Unset
            Unknown2  = 0x04,  // Bit #2: Observed 1: Set
            Supported = 0x08,  // Bit #3: Observed 1: Set - Supported
            Unset4    = 0x10,  // Bit #4: Observed 0: Unset
            Unset5    = 0x20,  // Bit #5: Observed 0: Unset
            Unset6    = 0x40,  // Bit #6: Observed 0: Unset
            Unset7    = 0x80   // Bit #7: Observed 0: Unset
        }

        // System support flags
        // Observed: 0x01
        [Flags]
        public enum SysSupportFlags : byte {
            SwFanCtl          = 0x01,  // Bit #0: Software fan control supported
            ExtremeMode       = 0x02,  // Bit #1: Extreme Mode supported
            ExtremeModeUnlock = 0x04   // Bit #2: Extreme Mode unlocked
        }

        // System status flags
        // Observed: 0x00E6 = 0b0000000011100110
        //        >= 0x0118 = 0b0000000100011000 - PPAB check
        //        >= 0x00C8 = 0b0000000011001000 - BIOS Performance Mode check
        [Flags]
        public enum SysStatusFlags : ushort {
            Unset0        = 0x0001,  // Bit #0: Observed 0: Unset
            Unknown1      = 0x0002,  // Bit #1: Observed 1: Set
            Unknown2      = 0x0004,  // Bit #2: Observed 1: Set
            BiosPerfPpab3 = 0x0008,  // Bit #3: BIOS Performance Mode or PPAB check common flag (Observed 0: Unset)
            Ppab4         = 0x0010,  // Bit #4: PPAB check flag #2 of 3 (Observed 0: Unset)
            Unknown5      = 0x0020,  // Bit #5: Observed 1: Set
            BiosPerf6     = 0x0040,  // Bit #6: BIOS Performance Mode check flag #2 of 3 (Observed 1: Set)
            BiosPerf7     = 0x0080,  // Bit #7: BIOS Performance Mode check flag #3 of 3 (Observed 1: Set)
            Ppab8         = 0x0100,  // Bit #8: PPAB check flag #3 of 3 (Observed 0: Unset)
            Unset9        = 0x0200,  // Bit #9: Observed 0: Unset
            UnsetA        = 0x0400,  // Bit #A: Observed 0: Unset
            UnsetB        = 0x0800,  // Bit #B: Observed 0: Unset
            UnsetC        = 0x1000,  // Bit #C: Observed 0: Unset
            UnsetD        = 0x2000,  // Bit #D: Observed 0: Unset
            UnsetE        = 0x4000,  // Bit #E: Observed 0: Unset
            UnsetF        = 0x8000   // Bit #F: Observed 0: Unset
        }

        // System design data structure
        // Consistent with how the data is stored by the BIOS
        // Observed: E6 00 35 01 01 D7 00 0C 00 ..
        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 128)]
        public struct SystemData {

            // Bytes #0 & #1: Status flags
            public SysStatusFlags StatusFlags;

            // Byte #2: Unknown
            // Observed: 0x35
            public byte Unknown2;

            // Byte #3: Thermal policy version
            public ThermalPolicyVersion ThermalPolicy;

            // Byte #4: Support flags
            public SysSupportFlags SupportFlags;

            // Byte #5: CPU Power Limit 4 default value
            // Observed: 0xD7 == 215 [W]
            public byte DefaultCpuPowerLimit4;

            // Byte #6: BIOS-defined overclocking support
            public SysBiosOc BiosOc;

            // Byte #7: Graphics switching support
            public SysGpuModeSwitch GpuModeSwitch;

            // Byte #8: CPU Concurrent Power Limit with GPU default value
            // Observed: 0x00, apparently applicable from Cybug 23C1 (2023 Omen 17) onwards
            public byte DefaultCpuPowerLimitWithGpu;

            // Unknown block observed empty as of now
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 119)]
            public byte[] RawBlock;

            // Initializes the system design data structure from a data array
            public SystemData(byte[] data) {
                StatusFlags = (SysStatusFlags) (ushort) (data[1] << 8 | data[0]);
                Unknown2 = data[2];
                ThermalPolicy = (ThermalPolicyVersion) data[3];
                SupportFlags = (SysSupportFlags) data[4];
                DefaultCpuPowerLimit4 = data[5];
                BiosOc = (SysBiosOc) data[6];
                GpuModeSwitch = (SysGpuModeSwitch) data[7];
                DefaultCpuPowerLimitWithGpu = data[8];
                RawBlock = new byte[119];

                // Copy over the rest of the array, in case
                // it ends up being populated too in future versions
                Array.Copy(data, 9, RawBlock, 0, data.Length < 128 ? data.Length - 9 : 119);
            }

        }
#endregion

    }

}
