  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023-2024 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using System.Collections.Generic;
using OmenMon.Library;

namespace OmenMon.Library.Locale {

    // Defines locale constants, variables, and structures
    // for subsequent use by the localization routines
    public abstract class LocaleData {

        // Language list
        public enum Language : int {
                Fallback,  // Default fallback
                Override   // Loaded from file
            }

        // Default fallback message data
        protected Dictionary<string, string> msgFallback 
            = new Dictionary<string, string>() {

                // CLI
                ["CliHeader"] = "Hardware Monitoring & Control Utility",
                ["CliHeaderVersion"] = "Version",
                ["CliActionGet"] = "-",
                ["CliActionSet"] = "+",
                ["CliDetailsFollow"] = Conv.GetChar(Conv.SpecialChar.ArrowDown),
                ["CliStateOn"] = "Yes",
                ["CliStateOff"] = "No",
                ["CliTranslated"] = "", // Only filled out for translations

                // CLI: BIOS
                ["CliBios"] = "BIOS",
                ["CliBiosAdapter"] = "Smart Power Adapter Status",
                ["CliBiosAnim"] = "LED Animation Table",
                ["CliBiosBacklight"] = "Keyboard Backlight",
                ["CliBiosBornDate"] = "Born-On Date",
                ["CliBiosBornDateNote"] = "YYYYMMDD",
                ["CliBiosColor"] = "Keyboard Backlight Color Table",
                ["CliBiosColorZones"] = "Zones",
                ["CliBiosCpuPowerLimit1"] = "CPU Power Limit 1",
                ["CliBiosCpuPowerLimit4"] = "CPU Power Limit 4",
                ["CliBiosCpuPowerLimitWithGpu"] = "CPU Power Limit Concurrent with GPU",
                ["CliBiosFanCount"] = "Fan Count",
                ["CliBiosFanLevelN"] = "Fan #{0} Level",
                ["CliBiosFanMax"] = "Maximum Fan Speed",
                ["CliBiosFanMode"] = "Fan Mode",
                ["CliBiosFanTable"] = "Fan Speed Level Table",
                ["CliBiosFanTableFans"] = "Fans",
                ["CliBiosFanTableLevels"] = "Levels",
                ["CliBiosFanType"] = "Fan Type",
                ["CliBiosFanTypeN"] = "Fan #{0} Type",
                ["CliBiosGpuMode"] = "Graphics Mode (Legacy)",
                ["CliBiosGpuPower"] = "GPU Power Settings",
                ["CliBiosGpuPowerCustomTgp"] = "GPU Custom Total Graphics Power (cTGP)",
                ["CliBiosGpuPowerDState"] = "GPU Device Power State (DState)",
                ["CliBiosGpuPowerPeakTemperature"] = "GPU Peak Temperature Sensor Threshold",
                ["CliBiosGpuPowerPpab"] = "GPU Processing Performance AI Boost (PPAB)",
                ["CliBiosHasBacklight"] = "Keyboard Backlight Support",
                ["CliBiosHasMemoryOverclock"] = "Memory Overclocking Support",
                ["CliBiosHasOverclock"] = "Overclocking Support",
                ["CliBiosHasUndervolt"] = "BIOS Undervolt Support",
                ["CliBiosIdle"] = "Idle Mode",
                ["CliBiosKbdType"] = "Keyboard Type",
                ["CliBiosSystem"] = "System Design Data",
                ["CliBiosSystemBiosOc"] = "BIOS-Defined Overclocking",
                ["CliBiosSystemDefaultCpuPowerLimit4"] = "Default CPU Power Limit 4",
                ["CliBiosSystemDefaultCpuPowerLimitWithGpu"] = "Default CPU Concurrent Power Limit w/GPU",
                ["CliBiosSystemDefaultCpuPowerLimitWithGpuNote"] = "Cybug 23C1 Onwards",
                ["CliBiosSystemGpuModeSwitch"] = "Graphics Mode Switching Support",
                ["CliBiosSystemStatusFlags"] = "Status Flags",
                ["CliBiosSystemSupportFlags"] = "Support Flags",
                ["CliBiosSystemThermalPolicyVersion"] = "Thermal Policy Version",
                ["CliBiosSystemUnknown2"] = "Unknown Byte",
                ["CliBiosSystemUnknown2Note"] = "Observed Constant 0x35 = 53",
                ["CliBiosTemp"] = "Temperature",
                ["CliBiosThrottling"] = "Thermal Throttling Status",
                ["CliBiosXmp"] = "Memory XMP Profile",

                // CLI: Embedded Controller
                ["CliEc"] = "Embedded Controller",
                ["CliEcMon"] = "Embedded Controller Monitor",
                ["CliEcByte"] = "Byte",
                ["CliEcRegister"] = "Register",
                ["CliEcWord"] = "Word",
                ["CliEcWordNote"] = "(Little-Endian)",

                // CLI: Program
                ["CliProg"] = "Program",
                ["CliProgCallback"] = "Callback",
                ["CliProgName"] = "Program",
                ["CliProgFanMode"] = "Fan Mode",
                ["CliProgGpuPower"] = "GPU Power",

                // CLI: Task
                ["CliTask"] = "Task Scheduling",
                ["CliTaskGui"] = "Autorun on User Logon",
                ["CliTaskKey"] = "Omen Key Interception",
                ["CliTaskMux"] = "Advanced Optimus Bug Fix",

                // CLI: Usage
                ["CliUsage"] = "Usage Information",
                ["CliUsageText"] =
                    "Usage: {0} [-<Arg1> [...] [-<ArgN> [...]]]" + Environment.NewLine +
                    "Where:" + Environment.NewLine +
                    "<Arg#>" + Environment.NewLine +
                    "  -Bios                     Run all the BIOS operations that only retrieve information" + Environment.NewLine +
                    "  -Bios <BiosOp>[=<Data>]+  Perform one or more BIOS operations with optional parameters" + Environment.NewLine +
                    "  -Ec                       Get the value of all Embedded Controller registers in a table format" + Environment.NewLine +
                    "  -Ec [<Reg>][=<Byte>]+     Get or set byte value(s) of one or more specific registers" + Environment.NewLine +
                    "  -Ec [<Reg>(2)][=<Word>]+  Get or set word value(s) of one or more pair(s) of consecutive specific registers" + Environment.NewLine +
                    "  -EcMon [FileName]         Monitor the values of all registers for changes and report, optionally save to file" + Environment.NewLine +
                    "  -Prog                     List available fan control programs loaded from the configuration file" + Environment.NewLine +
                    "  -Prog <Name>              Run a specified fan control program" + Environment.NewLine +
                    "  -Run <TName> [<Args>]     Run a specified task (in headless mode, no console output)" + Environment.NewLine +
                    "  -Task                     Check the status of all scheduled tasks" + Environment.NewLine +
                    "  -Task <TName>[=<Flag>]+   Enable or disable a scheduled task" + Environment.NewLine +
                    "  -?|-H|[-]-Help|[-]-Usage  Show usage information" + Environment.NewLine +
                    "<BiosOp>" + Environment.NewLine +
                    "  Cpu:PL1=<Byte> Cpu:PL4=<Byte> Cpu:PLGpu=<Byte> Gpu[=<GpuPreset>] GpuMode[=<GpuMode>] Xmp=<Flag>" + Environment.NewLine +
                    "  FanCount FanLevel[=<FanLevel>] FanMax[=<Flag>] FanMode=<FanMode> FanTable[=<FanTable>] FanType" + Environment.NewLine +
                    "  Idle[=<Flag>] Temp Throttling BornDate System Adapter HasOverclock HasMemoryOverclock HasUndervolt" + Environment.NewLine +
                    "  KbdType HasBacklight Backlight[=<Flag>] Color[=<Color>] Anim[=<ByteArray>]" + Environment.NewLine +
                    "<Data>" + Environment.NewLine +
                    "{1}" +
                    "Arguments are case-insensitive. Any argument can appear any number of times.",

                // GUI
                ["GuiAlreadyRunning"] = "Already running in the background: click on the notification area icon or run OmenMon -Usage for command-line parameters",
                ["GuiBtnDel"] = Conv.GetChar(Conv.SpecialChar.HeavyMultiplication),
                ["GuiBtnSet"] = Conv.GetChar(Conv.SpecialChar.HeavyCheckmark),
                ["GuiPromptReboot"] = "A system restart is required\r\nfor the change to take effect\r\n\r\nRestart now?",
                ["GuiTranslated"] = "", // Only filled out for translations

                // GUI: About (doubles as an error form)
                ["GuiAboutTitle"] = "About OmenMon",
                ["GuiAboutTitleError"] = "OmenMon Error",
                ["GuiAboutCaption"] = "Omen Hardware Monitoring & Control",
                ["GuiAboutText"] = "{\\rtf1\\ansi Monitor temperature and control fan speeds using WMI BIOS and the Embedded Controller. Lightweight, runs in the background with minimal footprint. Has a command-line mode too.}",
                ["GuiAboutTextErrorPrefix"] = "{\\rtf1\\ansi\\deff0{\\colortbl;\\red255\\green0\\blue0;}\\cf1",
                ["GuiAboutTextErrorSuffix"] = "}",

                // GUI: Main
                ["GuiMainFan"] = "Fan Monitoring & Control",
                ["GuiMainFan0"] = "CPU",
                ["GuiMainFan1"] = "GPU",
                ["GuiMainFanAuto"] = "Auto",
                ["GuiMainFanConst"] = "Const",
                ["GuiMainFanMax"] = "Max",
                ["GuiMainFanProg"] = "Prog",
                ["GuiMainFanProgSet"] = "Set Fan Program",
                ["GuiMainFanProgSetNoSel"] = "No program selected",
                ["GuiMainFanOff"] = "Off",
                ["GuiMainKbd"] = "Keyboard Backlight & Color",
                ["GuiMainKbdColorPickLeft"] = "Left Zone Color",
                ["GuiMainKbdColorPickMiddle"] = "Middle Zone Color",
                ["GuiMainKbdColorPickRight"] = "Right Zone Color",
                ["GuiMainKbdColorPickWasd"] = "WASD Keys Color",
                ["GuiMainKbdColorPresetAdd"] = "Save Preset",
                ["GuiMainKbdColorPresetAddValueDefault"] = "New Preset",
                ["GuiMainKbdColorPresetDel"] = "Delete Preset",
                ["GuiMainKbdColorPresetDelConfirm"] = "Are you sure?",
                ["GuiMainKbdColorPresetDelNoSel"] = "No preset selected",
                ["GuiMainKbdColorPresetDelPrompt"] = "Delete",
                ["GuiMainSys"] = "System Status & Information",
                ["GuiMainSysAdapterNotSupported"] = Conv.RTF_CF1 + "AC Unknown",
                ["GuiMainSysAdapterMeetsRequirement"] = Conv.RTF_CF3 + "AC Power OK",
                ["GuiMainSysAdapterBelowRequirement"] = Conv.RTF_CF4 + "AC Power Low",
                ["GuiMainSysAdapterBatteryPower"] = Conv.RTF_CF1 + "No AC Power",
                ["GuiMainSysAdapterNotFunctioning"] = Conv.RTF_CF4 + "AC Fail",
                ["GuiMainSysAdapterError"] = Conv.RTF_CF4 + "AC Error",
                ["GuiMainSysBorn"] = "*",
                ["GuiMainSysGpu"] = "GPU",
                ["GuiMainSysGpuPpab"] = "PPAB",
                ["GuiMainSysGpuCustomTgp"] = "cTGP",
                ["GuiMainSysGpuDState"] = "DState",
                ["GuiMainSysThrottlingUnknown"] = Conv.RTF_CF1 + "",
                ["GuiMainSysThrottlingDefault"] = Conv.RTF_CF5 + "Not Throttling",
                ["GuiMainSysThrottlingOn"] = Conv.RTF_CF4 + "Throttling",
                ["GuiMainSysMsgWelcome"] = "Welcome!",
                ["GuiMainTitle"] = "Omen Hardware Monitoring & Control",
                ["GuiMainTmp"] = "Temperature Sensor Readings",
                ["GuiMainTmpCPUT"] = "CPUT",
                ["GuiMainTmpGPTM"] = "GPTM",
                ["GuiMainTmpIRSN"] = "IRSN",
                ["GuiMainTmpRTMP"] = "RTMP",
                ["GuiMainTmpTMP1"] = "TMP1",
                ["GuiMainTmpTNT2"] = "TNT2",
                ["GuiMainTmpTNT3"] = "TNT3",
                ["GuiMainTmpTNT4"] = "TNT4",
                ["GuiMainTmpTNT5"] = "TNT5",

                // GUI: Menu
                ["GuiMenuSubFan"] = "Fan",
                ["GuiMenuActFanMax"] = "Maximum",
                ["GuiMenuActFanModeCool"] = "Cool",
                ["GuiMenuActFanModeDefault"] = "Default",
                ["GuiMenuActFanModeL0"] = "Legacy Level 0",
                ["GuiMenuActFanModeL1"] = "Legacy Level 1",
                ["GuiMenuActFanModeL2"] = "Legacy Level 2",
                ["GuiMenuActFanModeL3"] = "Legacy Level 3",
                ["GuiMenuActFanModeL4"] = "Legacy Level 4",
                ["GuiMenuActFanModeL5"] = "Legacy Level 5",
                ["GuiMenuActFanModeL6"] = "Legacy Level 6",
                ["GuiMenuActFanModeL7"] = "Legacy Level 7",
                ["GuiMenuActFanModeL8"] = "Legacy Level 8",
                ["GuiMenuActFanModeLegacyCool"] = "Legacy Cool",
                ["GuiMenuActFanModeLegacyDefault"] = "Legacy Default",
                ["GuiMenuActFanModeLegacyExtreme"] = "Legacy Extreme",
                ["GuiMenuActFanModeLegacyPerformance"] = "Legacy Performance",
                ["GuiMenuActFanModeLegacyQuiet"] = "Legacy Quiet",
                ["GuiMenuActFanModePerformance"] = "Performance",
                ["GuiMenuActFanOff"] = "Off",
                ["GuiMenuSubGpu"] = "Graphics",
                ["GuiMenuActGpuDisplayColor"] = "Reload Color Profile",
                ["GuiMenuActGpuDisplayOff"] = "Set Display Off",
                ["GuiMenuActGpuPowerMin"] = "Base Power",
                ["GuiMenuActGpuPowerMed"] = "Extra Power",
                ["GuiMenuActGpuPowerMax"] = "Extra Power with Boost",
                ["GuiMenuActGpuRefreshHigh"] = "High Refresh Rate",
                ["GuiMenuActGpuRefreshLow"] = "Standard Refresh Rate",
                ["GuiMenuActGpuModeDiscrete"] = "Discrete Exclusive",
                ["GuiMenuActGpuModeOptimus"] = "Optimus Soft-Switching",
                ["GuiMenuSubKbd"] = "Keyboard",
                ["GuiMenuActKbdBacklight"] = "Backlight",
                ["GuiMenuActKbdColorPresetDefaultApp"] = "OmenMon Cool",
                ["GuiMenuActKbdColorPresetDefaultOem"] = "OEM Default",
                ["GuiMenuSubSet"] = "Settings",
                ["GuiMenuActSetStayTop"] = "Stay on Top",
                ["GuiMenuActSetIconDyn"] = "Dynamic Icon",
                ["GuiMenuActSetIconDynBg"] = "Dynamic Background",
                ["GuiMenuActSetTaskGui"] = "Start with Windows",
                ["GuiMenuActSetAutoconfig"] = "Apply Settings on Startup",
                ["GuiMenuActSetTaskKey"] = "Intercept Omen Key",
                ["GuiMenuActSetTaskMux"] = "Advanced Optimus Fix",
                ["GuiMenuActToggleFormMain"] = "Show Monitor",
                ["GuiMenuActToggleFormMainHide"] = "Hide Monitor",
                ["GuiMenuActExit"] = "Exit",

                // GUI: Tooltips
                ["GuiTipBtnAccept"] = "Confirm and proceed",
                ["GuiTipBtnCancel"] = "Cancel and close the dialog",
                ["GuiTipFan0Cap"] = "The left-hand side shows the first (CPU) fan readings",
                ["GuiTipFan1Cap"] = "The right-hand side shows the second (GPU) fan readings",
                ["GuiTipFanUnitVal"] = "Fan speed is measured in revolutions per minute (rpm)",
                ["GuiTipFan0Val"] = "Real-time CPU fan speed reading [rpm]",
                ["GuiTipFan1Val"] = "Real-time GPU fan speed reading [rpm]",
                ["GuiTipFanUnitRte"] = "Fan relative rate is measured in percent (%)",
                ["GuiTipFan0Rte"] = "CPU fan relative rate [%]",
                ["GuiTipFan0RteBar"] = "CPU fan relative rate illustrated on a bar scale",
                ["GuiTipFan1Rte"] = "GPU fan relative rate [%]",
                ["GuiTipFan1RteBar"] = " GPU fan relative rate illustrated on a bar scale" + Environment.NewLine + " Note the origin is on the right-hand side",
                ["GuiTipFan0Lvl"] = "CPU fan level [krpm]" + Environment.NewLine + "Custom speed: move slider" + Environment.NewLine + "and click button to apply",
                ["GuiTipFan1Lvl"] = "GPU fan level [krpm]" + Environment.NewLine + "Custom speed: move slider" + Environment.NewLine + "and click button to apply",
                ["GuiTipFanCountdown"] = "If applicable, this area shows the countdown until" + Environment.NewLine + "the BIOS reverts back to the automatic defaults" + Environment.NewLine + "Select Const to prevent the timer from running out",
                ["GuiTipFanProg"] = "Fan program" + Environment.NewLine + "Speed will follow temperature" + Environment.NewLine + "according to your preferences",
                ["GuiTipFanProgCmb"] = "Choose a fan program from the drop-down list",
                ["GuiTipFanAuto"] = "Automatic mode (the default setting)",
                ["GuiTipFanMode"] = "Choose a fan mode from the drop-down list",
                ["GuiTipFanConst"] = "Constant speed mode" + Environment.NewLine + "Use trackbars to set each fan level",
                ["GuiTipFanMax"] = "Maximum speed mode" + Environment.NewLine + "Fans operate at maximum speed" + Environment.NewLine + "(5,500 and 5,700 rpm)",
                ["GuiTipFanOff"] = "Fans off" + Environment.NewLine + "Power off the fans completely",
                ["GuiTipFanSet"] = "Click to apply current settings" + Environment.NewLine + "Button is highlighted when settings have changed",
                ["GuiTipKbdBacklight"] = "Toggle keyboard backlight on and off",
                ["GuiTipKbdColorPreset"] = "Choose a color preset to apply" + Environment.NewLine + "from the drop-down box",
                ["GuiTipKbdColorPresetDel"] = "Delete the currently-selected preset",
                ["GuiTipKbdColorPresetSet"] = "Save the current settings as a preset",
                ["GuiTipKbdColorVal"] = "Adjust colors using their hexadecimal values with this parameter" + Environment.NewLine + "Set colors from the command line with: OmenMon -Bios Color=<Param>",
                ["GuiTipKbdPic"] = "Click on a zone to change the color for that zone" + Environment.NewLine + "with a color picker, changes take place immediately",
                ["GuiTipSys"] = "System status information is shown here",
                ["GuiTipTmpCPUT"] = "CPU Temperature",
                ["GuiTipTmpGPTM"] = "GPU Temperature",
                ["GuiTipTmpBIOS"] = "Temperature reported by the BIOS" + Environment.NewLine + "Values observed are much lower" + Environment.NewLine + "than for any other sensor",
                ["GuiTipTmpIRSN"] = "Infrared Sensor Temperature",
                ["GuiTipTmpRTMP"] = "Platform Controller Hub Temperature",
                ["GuiTipTmpTMP1"] = "Memory Temperature",
                ["GuiTipTmpTNT2"] = "Interpretation Unknown",
                ["GuiTipTmpTNT3"] = "Storage",
                ["GuiTipTmpTNT4"] = "Storage",
                ["GuiTipTmpTNT5"] = "Interpretation Unknown",
                ["GuiTipTmpUnknown"] = "Custom Sensor",
                ["GuiTipTxtInput"] = "Enter the value",

                // Data formats
                ["DataTypeBool"] = "<Flag>",
                ["DataSyntaxBool"] = "<On|True|Yes|1> | <Off|False|No|0>",

                ["DataTypeByte"] = "<Byte>",
                ["DataSyntaxByte"] = "<0-255|0x00-0xFF|0b00000000-0b11111111>",

                ["DataTypeByteArray"] = "<ByteArray>",
                ["DataSyntaxByteArray"] = "<00-FF>+",

                ["DataTypeColor4"] = "<Color>",
                ["DataSyntaxColor4"] = "<PresetName> | <RGB0>:<RGB1>:<RGB2>:<RGB3> (<RGB#>: 000000-FFFFFF)",

                ["DataTypeFanLevel"] = "<FanLevel>",
                ["DataSyntaxFanLevel"] = "<Fan1>,<Fan2> (<Fan#>: 0-255|0x00-0xFF|0b00000000-0b11111111)",

                ["DataTypeFanMode"] = "<FanMode>",
                ["DataSyntaxFanMode"] = "<FanModeId|0-255|0x00-0xFF|0b...> (<FanModeId>: Default|Performance|Cool|L#, <#>: 0-8)",

                ["DataTypeFanTable"] = "<FanTable>",
                ["DataSyntaxFanTable"] = "<Fan1>,<Fan2>,<Temp>[:...[:...]] (<Fan#>, <Temp>: <Byte>)",

                ["DataTypeGpuMode"] = "<GpuMode>",
                ["DataSyntaxGpuMode"] = "<GpuModeId|0-255|0x00-0xFF|0b...> (<GpuModeId>: Hybrid|Discrete|Optimus)",

                ["DataTypeGpuPowerLevel"] = "<GpuPreset>",
                ["DataSyntaxGpuPowerLevel"] = "Max[imum] | Med[ium]|Mid[dle] | Min[imum]",

                ["DataTypeReg"] = "<Reg>",
                ["DataSyntaxReg"] = "<NAME|0-255|0x00-0xFF|0b00000000-0b11111111>",
                ["DataSyntaxOrTwo"] = "[(2)]",

                ["DataTypeTName"] = "<TName>",
                ["DataSyntaxTName"] = "Autorun (GUI) | Key (Omen Key Capture) | Mux (Advanced Optimus Fix)",

                ["DataTypeWord"] = "<Word>",
                ["DataSyntaxWord"] = "<0-65535|0x0000-0xFFFF|0b0000000000000000-0b1111111111111111>",

                // Error messages
                ["ErrArgUnknown"] = "Unknown argument",
                ["ErrBiosCall"] = "BIOS call failed",
                ["ErrBiosInit"] = "Failed to initialize the BIOS controls. Please make sure you have a compatible HP system, and that the ACPI\\PNP0C14 driver is installed.",
                ["ErrBiosNull"] = "Failed to instantiate the BIOS controls",
                ["ErrBiosSend"] = "Failed to make the BIOS call",
                ["ErrBiosSendCommand"] = "Command not available",
                ["ErrBiosSendSize"] = "Input or output size too small",
                ["ErrBiosSendUnknown"] = "Unknown response from BIOS: {0}",
                ["ErrConfigLoad"] = "Failed to load configuration data",
                ["ErrConfigSave"] = "Failed to save configuration data",
                ["ErrEcInit"] = "Failed to initialize the embedded controller",
                ["ErrEcLock"] = "Failed to acquire embedded controller exclusive lock",
                ["ErrEcNull"] = "Failed to instantiate the embedded controller",
                ["ErrFileSave"] = "Failed to save the file",
                ["ErrLocaleNull"] = "Failed to instantiate the localizable message system",
                ["ErrLocaleLoad"] = "Failed to load localizable messages from the external file",
                ["ErrNeedRegisterRead"] = "Expected a register to read from",
                ["ErrNeedRegisterWrite"] = "Expected a register to write to",
                ["ErrNeedValueBool"] = "Expected a Boolean flag",
                ["ErrNeedValueByte"] = "Expected a byte value to set",
                ["ErrNeedValueByteArray"] = "Expected a byte array value to set",
                ["ErrNeedValueColor4"] = "Expected an array of four color values",
                ["ErrNeedValueFanLevel"] = "Expected a pair of fan speed levels",
                ["ErrNeedValueFanMode"] = "Expected a fan mode",
                ["ErrNeedValueFanTable"] = "Expected an array of fan table entries",
                ["ErrNeedValueGpuMode"] = "Expected a GPU mode",
                ["ErrNeedValueGpuPowerLevel"] = "Expected a GPU power preset",
                ["ErrNeedValueWord"] = "Expected a word value to set",
                ["ErrNotImplemented"] = "Not implemented",
                ["ErrProgName"] = "No such program",
                ["ErrProgNone"] = "No programs configured",
                ["ErrUnexpected"] = "Exception",
                ["ErrUnexpectedReally"] = "No details available",

                // Program
                ["Prog"] = "Program",
                ["ProgAlt"] = "[Alt]",
                ["ProgEnd"] = "Program Ended",
                ["ProgFans"] = "Fans",
                ["ProgLvl"] = "Lvl",
                ["ProgT"] = "T",
                ["ProgSubMax"] = "max",

                // Units
                ["UnitFrequency"] = "Hz",
                ["UnitPercent"] = "%",
                ["UnitPower"] = "W",
                ["UnitRotationRate"] = "rpm",
                ["UnitRotationRate_CustomFont"] = Conv.GetChar(Conv.SpecialChar.Prime1) + Conv.GetChar(Conv.SpecialChar.SupMinus) + Conv.GetChar(Conv.SpecialChar.Sup1),
                ["UnitTemperature"] = "°C",
                ["UnitTemperature_CustomFont"] = Conv.GetChar(Conv.SpecialChar.DegreeCelsius),
                ["UnitTimeSecond_CustomFont"] = Conv.GetChar(Conv.SpecialChar.SpacePerEm6) + Conv.GetChar(Conv.SpecialChar.Prime2),

                // XML
                ["_ConfigXmlTemplate"] =
                    "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine +
                    "<OmenMon>" + Environment.NewLine +
                    "    <!-- Automatically generated because no prior configuration file was found." + Environment.NewLine +
                    "         A version annotated with extensive comments is distributed with OmenMon.  -->" + Environment.NewLine +
                    "    <Config/>" + Environment.NewLine +
                    "    <Messages>" + Environment.NewLine +
                    "    </Messages>" + Environment.NewLine +
                    "</OmenMon>" + Environment.NewLine,

                // Language identifier
                ["_Language"] = "Fallback"

        };

    }

}
