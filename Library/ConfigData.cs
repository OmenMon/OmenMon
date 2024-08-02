  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023-2024 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OmenMon.Hardware.Bios;
using OmenMon.Hardware.Ec;
using OmenMon.Hardware.Platform;
using OmenMon.Library.Locale;

namespace OmenMon.Library {

    // Implements application-wide configuration settings look-up
    // This part only defines the configuration variables
    public static partial class Config {

        // Application metadata
        public static string AppFile = Process.GetCurrentProcess().MainModule.FileName;
        public static string AppName = typeof(App).Assembly.GetName().Name;
        public static string AppVersion = Application.ProductVersion;
        public static int AppProcessId = Process.GetCurrentProcess().Id;
        public const string AppHomepageLink = "https://omenmon.github.io/";

        // Automatically apply settings on start
        public static bool AutoConfig = false;

        // Automatically start up with Windows
        public static bool AutoStartup = false;

        // Ignore BIOS errors if false (for not fully compatible devices)
        public static bool BiosErrorReporting = true;

        // Color presets (overriden at runtime if found in the configuration file)
        public static SortedDictionary<string, BiosData.ColorTable> ColorPreset =
            new SortedDictionary<string, BiosData.ColorTable>() {
                ["DefaultOem"] = new BiosData.ColorTable("0F84FA:710FFA:F9350F:FAAC0F"),
                ["DefaultApp"] = new BiosData.ColorTable("0080FF:00FF00:00FF00:FFFFFF") };

        // Prefix for default color presets, name to be resolved through locale
        public const string ColorPresetDefaultPrefix = "Default";

        // DPI scaling factors, for responding to system DPI changes while the application is running
        public const int DpiSizeAdjFactorX = 10; // Divided by 100
        public const int DpiSizeAdjFactorY = 33; // Divided by 100

        // Embedded Controller operation parameters
        public static int EcMonInterval  = 1000; // Embedded Controller monitoring interval
        public static int EcMutexTimeout =  200; // How long before bailing out trying to get a mutex

        public static int EcFailLimit  = 15;  // Maximum number of failed attempts waiting to read
        public static int EcRetryLimit =  3;  // Maximum number of read and write attempts
        public static int EcWaitLimit  = 30;  // Iterations before waiting fails each time

        // Environment variable settings
        public static string EnvVarSelfName = AppName;
        public const string EnvVarSelfValueGui = "Quiet";
        public const string EnvVarSelfValueKey = "Key";
        public const string EnvVarSysRoot = "SystemRoot";

        // Exit status
        public enum ExitStatus : int {
            NoError     = 0,  // Default
            ErrorBios   = 1,  // BIOS initialization error
            ErrorEc     = 2,  // Embedded Controller initialization error
            ErrorLocale = 3,  // Localizable message system error
            ErrorTask   = 4   // Invalid task identifier
        }

        // Whether to always extend the fan countdown timer, even with no fan program
        // running, or constant-speed button selected, and the main window hidden
        public static bool FanCountdownExtendAlways = false;

        // Fan countdown extension threshold and interval [s]
        public static int FanCountdownExtendInterval = 120;
        public static int FanCountdownExtendThreshold = 5;

        // Fan level thresholds (for custom level setting with trackbars in Const mode)
        public static int FanLevelMax = 55;
        public static int FanLevelMin = 20;

        // Set manual fan mode first using the Embedded Controller before setting fan levels
        public static bool FanLevelNeedManual = false;

        // Whether to use the Embedded Controller instead of a BIOS call to set the fan level
        public static bool FanLevelUseEc = false;

        // Fan modes that should always be placed on top of the list
        // (the rest are legacy modes, irrelevant but kept for completeness)
        public static List<string> FanModesSticky = new List<string> { "Default", "Performance", "Cool" };

        // Fan programs (populated at runtime)
        public static SortedList<string, FanProgramData> FanProgram =
            new SortedList<string, FanProgramData>();

        // Default fan program, which might be loaded on startup
        public static string FanProgramDefault; // Unset by default, since there is no default fan program

        // Default alternative fan program when the system is not on AC power
        public static string FanProgramDefaultAlt; // Unset by default

        // Whether to check first (using the EC) if the fan mode is not set already
        // before setting it (using a BIOS WMI call) when a fan program is running
        public static bool FanProgramModeCheckFirst = false;

        // If true, fan program will be suspended whenever the system enters low-power mode
        // such as sleep, standby or hibernation, to be automatically re-enabled upon resume
        public static bool FanProgramSuspend = true;

        // Configuration XML file path
        public static string FilePath = "";

        // Fan speed string format (adds a thousand separator)
        public const string FormatFanSpeed = "N0";

        // Default GPU power setting, which might be loaded on startup
        public static string GpuPowerDefault = "Maximum";

        // Interval between applying the GPU power settings again
        // (repeated, since they don't always take effect the first time)
        public static int GpuPowerSetInterval = 200;

        // Pairs of color values to create either a warm or a cool gradient
        // Note: for some reason, Color.FromArgb() only takes signed input
        // even though it would make much more sense to be unsigned
        public const int GuiColorCoolDark = unchecked((int) 0xFF8804FF); // Magenta
        public const int GuiColorCoolLite = unchecked((int) 0xFF03EF9B); // Teal
        public const int GuiColorWarmDark = unchecked((int) 0xFFFF0802); // Red
        public const int GuiColorWarmLite = unchecked((int) 0xFFAC02FF); // Orange

        // Two additional colors for the RTF text box with better readability
        public const int GuiColorTextBlue = unchecked((int) 0xFF4182C9); // Blue
        public const int GuiColorTextTeal = unchecked((int) 0xFF0C9D7A); // Teal

        // Color to draw the keyboard in if the backlight is off
        public static int GuiColorKbdBacklightOff = System.Drawing.SystemColors.Control.ToArgb();

        // Original colors for each zone, to be replaced in the keyboard drawing
        // Zones as defined in BiosData.KbdZone
        public static int[] GuiColorKbdZoneOrig = new int[4] {
            unchecked((int) 0xFF444444),   // Zone 0: Right
            unchecked((int) 0xFF888888),   // Zone 1: Middle
            unchecked((int) 0xFFCCCCCC),   // Zone 2: Left
            unchecked((int) 0xFFFFFFFF)};  // Zone 3: WASD 

        // Predefined custom colors for the color picker

        // Note #1: values must be in 0x00BBGGRR format

        // Note #2: four values (#0, #1, #8 and #9) are
        // reserved for the current backlight at runtime,
        // these are set to black here

        // The colors are:

        // Row #1:
        // #60F70F  ( 96, 247,  15)  Lime
        // #00CF35  (  0, 207,  53)  Green
        // #3AE5E7  ( 58, 229, 231)  Cyan
        // #26A1D5  ( 38, 161, 213)  Sky Blue
        // #061AFF  (  6,  26, 255)  Deep Blue

        // Row #2:
        // #FDE005  (253, 224,   5)  Yellow
        // #FE6006  (254,  96,   6)  Orange
        // #F90D1B  (249,  13,  27)  Red
        // #FF0080  (255,   0, 128)  Purple 
        // #9C1AE7  (156,  26, 231)  Violet

        public static int[] GuiColorPickerCustom = new int[16] {
            0x000000, 0x000000, 0xFFFFFF, 0x0FF760, 0x35CF00, 0xE7E53A, 0xD5A126, 0xFF1A06,
            0x000000, 0x000000, 0xFFFFFF, 0x05E0FD, 0x0660FE, 0x1B0DF9, 0x8000FF, 0xE71A9C };

        // Whether closing the window closes the whole application
        public static bool GuiCloseWindowExit = false;

        // Whether to resize the main window if DPI changes
        public static bool GuiDpiChangeResize = false;

        // Whether to use a dynamic notification icon by default
        public static bool GuiDynamicIcon = false;

        // Whether the dynamic icon has a background or not
        public static bool GuiDynamicIconHasBackground = false;

        // Font to size ratio, based on empirical values of 23/32, 29/40, 35/48, 44/60, 46/64
        public const float GuiDynamicIconFontSizeRatio = 0.71875f;

        // Multiplier at which to render the dynamic notification icon, defaults to 2
        public const byte GuiDynamicIconUpscaleRatio = 2;

        // Size for the custom font used for figures [px]
        public const int GuiFigureFontSize = 23;

        // Name under which a custom message identifier is registered for cross-instance communication
        public const string GuiMessageId = "WM_OMENMON_FOCUS";

        // Inset for the customized progress bar
        public const int GuiProgressBarInset = 2;

        // Whether the main form remains on top of other windows when shown
        public static bool GuiStayOnTop = false;

        // Override System Information font size (leave 0 for the default)
        public static int GuiSysInfoFontSize = 0;

        // Timer interval, determines how frequently a tick occurs [ms]
        public const int GuiTimerInterval = 1000;
        
        // Timer interval, determines how frequently heartbeat event sent [ms]
        public const int HeartbeatTimerInterval = 30000;

        // How long to show a tip in the notification area, disabled if set to 0
        public static int GuiTipDuration = 30000;

        // Custom action for the Omen key handler
        public static bool KeyCustomActionEnabled = false;
        public static string KeyCustomActionExecCmd = "";
        public static string KeyCustomActionExecArgs = "";
        public static bool KeyCustomActionMinimized = false;

        // Use the Omen key to control fan program
        // (as long as KeyCustomAction is set to false)
        public static bool KeyToggleFanProgram = false;

        // If true, Omen key cycles through all fan programs,
        // instead of toggling the default fan program on and off
        public static bool KeyToggleFanProgramCycleAll = true;

        // Show window first Omen key press (if not shown already),
        // before using subsequent keypresses to control fan program
        public static bool KeyToggleFanProgramShowGuiFirst = true;

        // Do not show a balloon tip notification when changing programs
        public static bool KeyToggleFanProgramSilent = false;

        // Localizable string prefixes and suffixes
        public const string L_CLI = "Cli";
        public const string L_CLI_BIOS = "CliBios";
        public const string L_CLI_EC = "CliEc";
        public const string L_CLI_PROG = "CliProg";
        public const string L_CLI_TASK = "CliTask";
        public const string L_DATATYPE_NAME = "DataType";
        public const string L_DATATYPE_SYNTAX = "DataSyntax";
        public const string L_GUI = "Gui";
        public const string L_GUI_ABOUT = "GuiAbout";
        public const string L_GUI_MAIN = "GuiMain";
        public const string L_GUI_MENU = "GuiMenu";
        public const string L_GUI_TIP = "GuiTip";
        public const string L_PROG = "Prog";
        public const string L_UNIT = "Unit";
        public const string LS_CUSTOM_FONT = "_CustomFont"; // Suffix

        // Exclusivity lock names or paths
        public static string LockNameMux = AppName + "-Mux";
        public const string LockPathEc = "Global\\Access_EC"; // Commonly-observed value
        public const string LockPathCli = "Global\\OmenMonCli";
        public const string LockPathGui = "Global\\OmenMonGui";

        // Maximum believable speed percent over maximum value when reading from the Embedded Controller (used for fan speed)
        public const int MaxBelievableFanSpeedPercentOverMax = 10;

        // Maximum believable percent value when reading from the Embedded Controller (used for fan rate)
        public const int MaxBelievablePercent = 100;

        // Maximum believable temperature value when reading from the Embedded Controller
        public const int MaxBelievableTemperature = 99;

        // nVidia Display Container service name
        public const string NvDisplayContainerService = "NVDisplay.ContainerLocalSystem";

        // Location for temporary files (must be declared before OnlyOncePath)
        public static string PathTemp = Environment.GetEnvironmentVariable("TEMP");

        // Parameters for the persistent state until reboot flag implementation
        public static string OnlyOnceFileExt = ".txt";
        public static string OnlyOncePath = PathTemp;

        // Display refresh rate values [Hz]
        public static int PresetRefreshRateHigh = 165;
        public static int PresetRefreshRateLow = 60;

        // Registry hive prefix
        public const string RegHiveMachine = "HKEY_LOCAL_MACHINE";

        // nVidia Advanced Optimus multiplexer status registry location
        public const string RegMuxKey = "SYSTEM\\CurrentControlSet\\Services\\nvlddmkm\\Global\\NvHybrid\\Persistence\\ACE";
        public const string RegMuxValue = "InternalMuxState";

        // Default shell executable registry location
        public const string RegShellKey = "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon";
        public const string RegShellValue = "Shell";

        // System information rich-text field settings
        public const string SysInfoRtfPreHeader = "{\\rtf1\\ansi\\ansicpg1252\\deff0";
        public static string SysInfoRtfHeader = SysInfoRtfPreHeader +
            "{\\colortbl;" // Overriden at runtime, in case the color values changed (currently won't)
            + Conv.GetColorStringRtf(SystemColors.GrayText.ToArgb())  // System Gray
            + Conv.GetColorStringRtf(0)                               // Black
            + Conv.GetColorStringRtf(GuiColorTextTeal)                // Teal
            + Conv.GetColorStringRtf(GuiColorWarmDark)                // Red
            + Conv.GetColorStringRtf(GuiColorTextBlue)                // Blue
            + Conv.GetColorStringRtf(GuiColorWarmLite)                // Fuchsia
            + "}";
        public const string SysInfoRtfFooter = " }";

        // Folder where scheduled tasks are stored
        public const string TaskFolder = "\\";
        public static string TaskRunPath = Environment.GetEnvironmentVariable(EnvVarSysRoot) + "\\System32\\schtasks.exe";
        public const string TaskRunArgs = "/run /tn ";

        // Structure to hold temperature sensor information
        public struct TemperatureSensorData {

            // Resolved numerical value
            // to be passed to the source
            public byte Register;

            // Where the data originates from
            public PlatformData.LinkType Source;

            // Whether the sensor is used
            // or only being displayed
            public bool Use;

            // Constructor with all parameters
            public TemperatureSensorData(
                PlatformData.LinkType source,
                byte register = 0,
                bool use = true) {

                // Do not accept empty register values
                // if the source is the Embedded Controller
                if(source == PlatformData.LinkType.EmbeddedController
                    && register == 0)

                    // Throw an exception if that is the case
                    throw new ArgumentOutOfRangeException();

                // Set the structure data
                this.Source = source;
                this.Register = register;
                this.Use = use;

            }

            // Constructor with no register
            public TemperatureSensorData(
                PlatformData.LinkType source,
                bool use = true) : this(source, 0, use) { }

        }

        // Temperature sensors (overriden at runtime if found in the configuration file)
        public static Dictionary<string, TemperatureSensorData> TemperatureSensor =
            new Dictionary<string, TemperatureSensorData> {

                // CPU temperature
                ["CPUT"] = new TemperatureSensorData(
                    PlatformData.LinkType.EmbeddedController,
                    (byte) EmbeddedControllerData.Register.CPUT),

                // GPU temperature
                ["GPTM"] = new TemperatureSensorData(
                    PlatformData.LinkType.EmbeddedController,
                    (byte) EmbeddedControllerData.Register.GPTM),

                // Temperature reported by the BIOS
                // (values more or less a third lower than other readings,
                // thus currently makes no sense to use for maximum check)
                ["BIOS"] = new TemperatureSensorData(
                    PlatformData.LinkType.WmiBios, false),

                // Platform Controller Hub temperature
                ["RTMP"] = new TemperatureSensorData(
                    PlatformData.LinkType.EmbeddedController,
                    (byte) EmbeddedControllerData.Register.RTMP),

                // Memory temperature
                ["TMP1"] = new TemperatureSensorData(
                    PlatformData.LinkType.EmbeddedController,
                    (byte) EmbeddedControllerData.Register.TMP1),

                // Auxilliary EC temperature probe #2
                ["TNT2"] = new TemperatureSensorData(
                    PlatformData.LinkType.EmbeddedController,
                    (byte) EmbeddedControllerData.Register.TNT2),

                // Auxilliary EC temperature probe #3
                ["TNT3"] = new TemperatureSensorData(
                    PlatformData.LinkType.EmbeddedController,
                    (byte) EmbeddedControllerData.Register.TNT3),

                // Auxilliary EC temperature probe #4
                ["TNT4"] = new TemperatureSensorData(
                    PlatformData.LinkType.EmbeddedController,
                    (byte) EmbeddedControllerData.Register.TNT4),

                // Auxilliary EC temperature probe #5
                ["TNT5"] = new TemperatureSensorData(
                    PlatformData.LinkType.EmbeddedController,
                    (byte) EmbeddedControllerData.Register.TNT5) };

        // Maximum number of temperature sensors
        public const int TemperatureSensorMax = 9;

        // Timestamp format in fan program status messages
        public const string TimestampFormat = "HH:mm:ss";

        // Scheduled task identifiers
        public enum TaskId {
            Gui,  // Autorun GUI on Windows startup
            Key,  // Omen key capture task
            Mux   // nVidia Advanced Optimus bug fix task
        }

        // Scheduled task data
        public static Dictionary<TaskId, string[]> Task =
            new Dictionary<TaskId, string[]>() {
                [TaskId.Gui] = new string[] { AppName, "-Run Gui" },
                [TaskId.Key] = new string[] { AppName + " Key", "-Run Key", "root\\wmi", "SELECT * FROM hpqBEvnt WHERE eventData = 8613 AND eventId = 29" },
                [TaskId.Mux] = new string[] { AppName + " Mux", "-Run Mux", "root\\default", "SELECT * FROM RegistryValueChangeEvent WHERE Hive = \"" + RegHiveMachine + "\" AND KeyPath = \"" + RegMuxKey + "\" AND ValueName = \"" + RegMuxValue + "\"" }
        };


        // How often the dynamic notification icon is updated (in ticks)
        public static int UpdateIconInterval = 3;

        // How often the monitoring data on the main form is updated (in ticks)
        public static int UpdateMonitorInterval = 3;

        // How often the program settings are updated (in ticks)
        public static int UpdateProgramInterval = 15;

        // Wait duration when stopping a process (Explorer shell) or a service (nVidia Container)
        public const int WaitToStopProcess = 1000;
        public const int WaitToStopService = 500;

        // WMI event settings
        public const string WmiEventSuffixConsumer = "Consumer";
        public const string WmiEventSuffixFilter = "Filter";
        public const string WmiQueryLang = "WQL";

        // Configuration XML elements and attributes
        private const string XmlElementColorPresets = "ColorPresets";
        private const string XmlElementColorPreset = "Preset";
        private const string XmlElementFanPrograms = "FanPrograms";
        private const string XmlElementFanProgram = "Program";
        private const string XmlElementFanProgramMode = "FanMode";
        private const string XmlElementFanProgramPower = "GpuPower";
        private const string XmlElementFanProgramLevel = "Level";
        private const string XmlElementFanProgramLevelCpu = "Cpu";
        private const string XmlElementFanProgramLevelGpu = "Gpu";
        private const string XmlElementTemperature = "Temperature";
        private const string XmlElementTemperatureSensor = "Sensor";
        private const string XmlAttrColorPresetName = "Name";
        private const string XmlAttrFanProgramName = "Name";
        private const string XmlAttrFanProgramLevelTemperature = "Temperature";
        private const string XmlAttrTemperatureSensorName = "Name";
        private const string XmlAttrTemperatureSensorSource = "Source";
        private const string XmlAttrTemperatureSensorSourceValueBios = "BIOS";
        private const string XmlAttrTemperatureSensorSourceValueEc = "EC";
        private const string XmlAttrTemperatureSensorUse = "Use";
        private const string XmlElementConfig = "Config";
        private const string XmlElementKeyCustomAction = "KeyCustomAction";

        // Configuration XML node prefixes
        private static string XmlPrefix = AppName + "/" + XmlElementConfig + "/"; // Must end with a slash
        private static string XmlPrefixColorPresets = XmlPrefix + XmlElementColorPresets + "/"; // Slash
        private static string XmlPrefixColorPreset = XmlPrefixColorPresets + XmlElementColorPreset; // No slash
        private static string XmlPrefixFanPrograms = XmlPrefix + XmlElementFanPrograms + "/"; // Slash
        private static string XmlPrefixFanProgram = XmlPrefixFanPrograms + XmlElementFanProgram; // No slash
        private static string XmlPrefixKeyCustomAction = XmlPrefix + XmlElementKeyCustomAction + "/"; // Slash
        private static string XmlPrefixTemperature = XmlPrefix + XmlElementTemperature + "/"; // Slash
        private static string XmlPrefixTemperatureSensor = XmlPrefixTemperature + XmlElementTemperatureSensor; // No slash

        // Whether to skip the annoying Byte Order Mark (BOM) when saving the XML configuration
        private const bool XmlSaveBom = false;

        // Strings representing Boolean flags used when saving to XML
        // Note: must be one of the values from Library.Conv.GetBool()
        private const string XmlSaveBoolFalse = "false";
        private const string XmlSaveBoolTrue = "true";
        private const string XmlSaveIndent = "    ";

        // Template XML configuration file (rudimentary, a better version replaces this when locale is loaded)
        private static string XmlTemplate = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine + "<OmenMon/>";

    }

}
