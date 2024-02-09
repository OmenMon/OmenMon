  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023-2024 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Text;
using System.Xml;
using OmenMon.Hardware.Bios;
using OmenMon.Hardware.Ec;
using OmenMon.Hardware.Platform;
using OmenMon.Library.Locale;

namespace OmenMon.Library {

    // Implements application-wide configuration settings look-up
    // This part only contains the implementing methods
    public static partial class Config {

#region Initialization
        // State flag
        public static bool IsInitialized { get; private set; }

        // Localization strings interface
        public static ILocale Locale;

        // Initializes the configuration class
        public static void Initialize() {

            // Only do it once
            if(!IsInitialized) {

                // Set the configuration file location
                // Note: also used by locale, so must happen before
                try {

                    // Establish where to look for the XML configuration file
                    FilePath = Path.ChangeExtension(AppFile, ".xml");

                } catch { }

                // Initialize the message system
                if(!LocaleInit("Override"))
                    App.Exit(Config.ExitStatus.ErrorLocale);

                // Load the configuration
                Load();

                // Done
                IsInitialized = true;

            }

        }

        // Initializes the locale system
        public static bool LocaleInit() {

            // Instantiate the localization system
            if((Locale = OmenMon.Library.Locale.Locale.Instance) == null) {

                // Show an error if failed
                App.Error("ErrLocaleNull");
                return false;

            }

            return true;

        }

        // Initializes the locale system and sets the language
        public static bool LocaleInit(string language) {

            // Instantiate the locale
            if(!LocaleInit())
                return false;

            // Set the application language
            Locale.SetLanguage(language);

            return true;

        }
#endregion

#region Configuration Retrieval
        // Retrieves a Boolean flag value from the XML configuration file
        private static bool GetBool(XmlDocument xml, string node, out bool value) {
            value = false;
            try {
                if(Conv.GetBool(xml.SelectSingleNode(node).InnerText, out value))
                    return true;
            } catch {  }
            return false;

        }

        // Retrieves a string value from the XML configuration file
        private static string GetString(XmlDocument xml, string node) {
            string value = "";
            try {
                value = xml.SelectSingleNode(node).InnerText;
            } catch {  }
            return (value == null ? "" : value);
        }

        // Retrieves an unsigned word-sized value from the XML configuration file
        private static bool GetWord(XmlDocument xml, string node, out ushort value) {
            value = 0;
            try {
                if(Conv.GetWord(xml.SelectSingleNode(node).InnerText, out value))
                    return true;
            } catch {  }
            return false;

        }

        // Loads the configuration data from the XML file
        public static void Load() {

            // Proceed only if the file exists
            if(FilePath != "" && File.Exists(FilePath)) {

                try {

                    // Load the file
                    XmlDocument xml = new XmlDocument();
                    xml.Load(FilePath);

                    // Replace the hard-coded XML template with a localized one
                    // Only possible once the localizable message class is instantiated
                    XmlTemplate = Config.Locale.Get("_ConfigXmlTemplate");

                    // Read the configuration and parse it into values
                    bool flag;
                    ushort value;

                    if(GetBool(xml, XmlPrefix + "AutoConfig", out flag))
                        AutoConfig = flag;

                    if(GetBool(xml, XmlPrefix + "AutoStartup", out flag))
                        AutoStartup = flag;

                    if(GetBool(xml, XmlPrefix + "BiosErrorReporting", out flag))
                        BiosErrorReporting = flag;

                    if(GetWord(xml, XmlPrefix + "EcFailLimit", out value))
                        EcFailLimit = value;

                    if(GetWord(xml, XmlPrefix + "EcMonInterval", out value))
                        EcMonInterval = value;

                    if(GetWord(xml, XmlPrefix + "EcMutexTimeout", out value))
                        EcMutexTimeout = value;

                    if(GetWord(xml, XmlPrefix + "EcRetryLimit", out value))
                        EcRetryLimit = value;

                    if(GetWord(xml, XmlPrefix + "EcWaitLimit", out value))
                        EcWaitLimit = value;

                    if(GetBool(xml, XmlPrefix + "FanCountdownExtendAlways", out flag))
                        FanCountdownExtendAlways = flag;

                    if(GetWord(xml, XmlPrefix + "FanCountdownExtendInterval", out value))
                        FanCountdownExtendInterval = value;

                    if(GetWord(xml, XmlPrefix + "FanCountdownExtendThreshold", out value))
                        FanCountdownExtendThreshold = value;

                    if(GetWord(xml, XmlPrefix + "FanLevelMax", out value))
                        FanLevelMax = value;

                    if(GetWord(xml, XmlPrefix + "FanLevelMin", out value))
                        FanLevelMin = value;

                    if(GetBool(xml, XmlPrefix + "FanLevelNeedManual", out flag))
                        FanLevelNeedManual = flag;

                    if(GetBool(xml, XmlPrefix + "FanLevelUseEc", out flag))
                        FanLevelUseEc = flag;

                    FanProgramDefault =
                        GetString(xml, XmlPrefix + "FanProgramDefault");

                    FanProgramDefaultAlt =
                        GetString(xml, XmlPrefix + "FanProgramDefaultAlt");

                    if(GetBool(xml, XmlPrefix + "FanProgramModeCheckFirst", out flag))
                        FanProgramModeCheckFirst = flag;

                    if(GetBool(xml, XmlPrefix + "FanProgramSuspend", out flag))
                        FanProgramSuspend = flag;

                    GpuPowerDefault =
                        GetString(xml, XmlPrefix + "GpuPowerDefault");

                    if(GetWord(xml, XmlPrefix + "GpuPowerSetInterval", out value))
                        GpuPowerSetInterval = value;

                    if(GetBool(xml, XmlPrefix + "GuiCloseWindowExit", out flag))
                        GuiCloseWindowExit = flag;

                    if(GetBool(xml, XmlPrefix + "GuiDpiChangeResize", out flag))
                        GuiDpiChangeResize = flag;

                    if(GetBool(xml, XmlPrefix + "GuiDynamicIcon", out flag))
                        GuiDynamicIcon = flag;

                    if(GetBool(xml, XmlPrefix + "GuiDynamicIconHasBackground", out flag))
                        GuiDynamicIconHasBackground = flag;

                    if(GetBool(xml, XmlPrefix + "GuiStayOnTop", out flag))
                        GuiStayOnTop = flag;

                    if(GetWord(xml, XmlPrefix + "GuiSysInfoFontSize", out value))
                        GuiSysInfoFontSize = value;

                    if(GetWord(xml, XmlPrefix + "GuiTipDuration", out value))
                        GuiTipDuration = value;

                    if(GetBool(xml, XmlPrefix + "KeyToggleFanProgram", out flag))
                        KeyToggleFanProgram = flag;

                    if(GetBool(xml, XmlPrefix + "KeyToggleFanProgramCycleAll", out flag))
                        KeyToggleFanProgramCycleAll = flag;

                    if(GetBool(xml, XmlPrefix + "KeyToggleFanProgramShowGuiFirst", out flag))
                        KeyToggleFanProgramShowGuiFirst = flag;

                    if(GetBool(xml, XmlPrefix + "KeyToggleFanProgramSilent", out flag))
                        KeyToggleFanProgramSilent = flag;

                    if(GetWord(xml, XmlPrefix + "PresetRefreshRateHigh", out value))
                        PresetRefreshRateHigh = value;

                    if(GetWord(xml, XmlPrefix + "PresetRefreshRateLow", out value))
                        PresetRefreshRateLow = value;

                    if(GetWord(xml, XmlPrefix + "UpdateIconInterval", out value))
                        UpdateIconInterval = value;

                    if(GetWord(xml, XmlPrefix + "UpdateMonitorInterval", out value))
                        UpdateMonitorInterval = value;

                    if(GetWord(xml, XmlPrefix + "UpdateProgramInterval", out value))
                        UpdateProgramInterval = value;

                    // Load the key custom action settings
                    if(GetBool(xml, XmlPrefixKeyCustomAction + "Enabled", out flag))
                        KeyCustomActionEnabled = flag;

                    KeyCustomActionExecCmd =
                        GetString(xml, XmlPrefixKeyCustomAction + "ExecCmd");

                    KeyCustomActionExecArgs =
                        GetString(xml, XmlPrefixKeyCustomAction + "ExecArgs");

                    if(GetBool(xml, XmlPrefixKeyCustomAction + "Minimized", out flag))
                        KeyCustomActionMinimized = flag;

                    // Load the color presets
                    SortedDictionary<string, BiosData.ColorTable> ColorPresetXml
                        = new SortedDictionary<string, BiosData.ColorTable>();
                    foreach(XmlNode node in xml.SelectNodes(XmlPrefixColorPreset)) {
                        // Invalid entries will be discarded at this step
                        try {
                            BiosData.ColorTable colorTable = new BiosData.ColorTable(node.InnerText);
                            ColorPresetXml[node.Attributes[XmlAttrColorPresetName].Value] = colorTable;
                        } catch { }
                    }

                    // Replace the defaults with configured color presets unless none
                    if(ColorPresetXml.Count > 0)
                        ColorPreset = ColorPresetXml;

                    // Populate the RTF header with colors at run-time
                    SysInfoRtfHeader = SysInfoRtfPreHeader + 
                        "{\\colortbl;"
                        + Conv.GetColorStringRtf(SystemColors.GrayText.ToArgb())  // System Gray
                        + Conv.GetColorStringRtf(0)                               // Black
                        + Conv.GetColorStringRtf(GuiColorTextTeal)                // Teal
                        + Conv.GetColorStringRtf(GuiColorWarmDark)                // Red
                        + Conv.GetColorStringRtf(GuiColorTextBlue)                // Blue
                        + Conv.GetColorStringRtf(GuiColorWarmLite)                // Fuchsia
                        + "}";

                    // Load the temperature sensors
                    bool usable = false;
                    Dictionary<string, TemperatureSensorData> TemperatureSensorXml
                        = new Dictionary<string, TemperatureSensorData>();
                    foreach(XmlNode node in xml.SelectNodes(XmlPrefixTemperatureSensor)) {
                        // Invalid entries will be discarded at this step
                        try {

                            // Abort if more than the maximum number of sensors defined already
                            if(TemperatureSensorXml.Count >= TemperatureSensorMax)
                                break;

                            // Set the optional use flag
                            // based on the XML attribute
                            bool use = true;
                            try {
                                Conv.GetBool(node.Attributes[XmlAttrTemperatureSensorUse].Value, out use);
                            } catch {  }

                            // Check for Embedded Controller sensor source
                            if(node.Attributes[XmlAttrTemperatureSensorSource].Value
                                == XmlAttrTemperatureSensorSourceValueEc)

                                // Adding a sensor sourced from the Embedded Controller
                                TemperatureSensorXml[node.Attributes[XmlAttrTemperatureSensorName].Value] =
                                    new TemperatureSensorData(
                                        PlatformData.LinkType.EmbeddedController,
                                        (byte) Enum.Parse(typeof(EmbeddedControllerData.Register),
                                            node.Attributes[XmlAttrTemperatureSensorName].Value), use);

                            // Check for WMI BIOS sensor source
                            else if(node.Attributes[XmlAttrTemperatureSensorSource].Value
                                == XmlAttrTemperatureSensorSourceValueBios)

                                // Adding a sensor sourced from the WMI BIOS
                                TemperatureSensorXml[XmlAttrTemperatureSensorSourceValueBios] =
                                    new TemperatureSensorData(PlatformData.LinkType.WmiBios, use);

                            // Throw an exception for any unknown sources
                            else throw new ArgumentOutOfRangeException();

                            // Record found usable
                            if(use) usable = true;

                        } catch { }

                    }

                    // Replace the defaults with configured temperature sensors unless none
                    // were configured or not a single sensor was set to actually be used
                    if(TemperatureSensorXml.Count > 0 && usable)
                        TemperatureSensor = TemperatureSensorXml;

                    // Load the fan programs
                    foreach(XmlNode node in xml.SelectNodes(XmlPrefixFanProgram)) {
                        // Invalid entries will be discarded at this step
                        try {

                            // Set up a variable to read the level configuration into
                            SortedDictionary<byte, byte[]> levels =
                                new SortedDictionary<byte, byte[]>();

                            // Iterate through the levels specified in the XML file
                            foreach(XmlNode subnode in node.SelectNodes(XmlElementFanProgramLevel)) {

                                // Populate the level data
                                levels[Conv.GetByte(
                                    subnode.Attributes[XmlAttrFanProgramLevelTemperature].Value)] =
                                        new byte[] {
                                            Conv.GetByte(subnode[XmlElementFanProgramLevelCpu].InnerText),
                                            Conv.GetByte(subnode[XmlElementFanProgramLevelGpu].InnerText)};

                            }

                            // Create a new fan program from the configuration data
                            FanProgram[node.Attributes[XmlAttrFanProgramName].Value] =
                                new FanProgramData(
                                    node.Attributes[XmlAttrFanProgramName].Value,
                                    (BiosData.FanMode) Enum.Parse(typeof(BiosData.FanMode), node[XmlElementFanProgramMode].InnerText),
                                    (BiosData.GpuPowerLevel) Enum.Parse(typeof(BiosData.GpuPowerLevel), node[XmlElementFanProgramPower].InnerText),
                                    levels);

                        } catch { }

                    }

                } catch {

                    // Silently ignore any errors

                }

            }

        }
#endregion

#region Configuration Saving
        // Save the configuration data to the XML file
        public static void Save() {

            // Proceed only if the filename is not empty
            if(FilePath != "") {

                try {

                    // Create a new XML document
                    XmlDocument xml = new XmlDocument();

                    try {

                        // Try to load the existing configuration file
                        xml.Load(FilePath);

                    } catch {

                        // Otherwise, start with a pre-defined template
                        // and do not preserve the formatting
                        xml.LoadXml(Config.XmlTemplate);

                    }

                    // Create or update the configuration values
                    SetBool(xml, XmlPrefix + "AutoConfig", AutoConfig);
                    SetBool(xml, XmlPrefix + "AutoStartup", AutoStartup);
                    SetBool(xml, XmlPrefix + "BiosErrorReporting", BiosErrorReporting);

                    // Color presets (so that the settings are sorted alphabetically)
                    // Ensure the parent element node exists, or create it
                    XmlElement xmlColor = (XmlElement) SetPath(xml, XmlPrefixColorPresets);

                    // Remove all currently-defined presets
                    // (the user might have already deleted some of them)
                    xmlColor.RemoveAll();

                    // Iterate through the color presets
                    foreach(string name in ColorPreset.Keys) {

                        // Create an element for each preset
                        XmlElement node = (XmlElement) xmlColor.AppendChild(
                                xml.CreateElement(XmlElementColorPreset));

                        // Store the preset name in an attribute
                        node.SetAttribute(XmlAttrColorPresetName, name);

                        // Store the preset parameter value as inner text
                        node.InnerText = (Conv.GetColorString((int) ColorPreset[name].Zone[(int) BiosData.KbdZone.Right].ValueReverse)
                            + ":" + Conv.GetColorString((int) ColorPreset[name].Zone[(int) BiosData.KbdZone.Middle].ValueReverse)
                            + ":" + Conv.GetColorString((int) ColorPreset[name].Zone[(int) BiosData.KbdZone.Left].ValueReverse)
                            + ":" + Conv.GetColorString((int) ColorPreset[name].Zone[(int) BiosData.KbdZone.Wasd].ValueReverse))
                                .ToUpper();

                    }

                    // Continue with the configuration values
                    SetUInt(xml, XmlPrefix + "EcFailLimit", (uint) EcFailLimit);
                    SetUInt(xml, XmlPrefix + "EcMonInterval", (uint) EcMonInterval);
                    SetUInt(xml, XmlPrefix + "EcMutexTimeout", (uint) EcMutexTimeout);
                    SetUInt(xml, XmlPrefix + "EcRetryLimit", (uint) EcRetryLimit);
                    SetUInt(xml, XmlPrefix + "EcWaitLimit", (uint) EcWaitLimit);
                    SetBool(xml, XmlPrefix + "FanCountdownExtendAlways", FanCountdownExtendAlways);
                    SetUInt(xml, XmlPrefix + "FanCountdownExtendInterval", (uint) FanCountdownExtendInterval);
                    SetUInt(xml, XmlPrefix + "FanCountdownExtendThreshold", (uint) FanCountdownExtendThreshold);
                    SetUInt(xml, XmlPrefix + "FanLevelMax", (uint) FanLevelMax);
                    SetUInt(xml, XmlPrefix + "FanLevelMin", (uint) FanLevelMin);
                    SetBool(xml, XmlPrefix + "FanLevelNeedManual", FanLevelNeedManual);
                    SetBool(xml, XmlPrefix + "FanLevelUseEc", FanLevelUseEc);
                    SetString(xml, XmlPrefix + "FanProgramDefault", FanProgramDefault);
                    SetString(xml, XmlPrefix + "FanProgramDefaultAlt", FanProgramDefaultAlt);
                    SetBool(xml, XmlPrefix + "FanProgramModeCheckFirst", FanProgramModeCheckFirst);
                    SetBool(xml, XmlPrefix + "FanProgramSuspend", FanProgramSuspend);

                    // Fan programs (again, so that the settings are
                    // sorted alphabetically for the user's convenience)

                    // Ensure the parent element node exists, or create it
                    XmlElement xmlFan = (XmlElement) SetPath(xml, XmlPrefixFanPrograms);

                    // Remove all currently-defined presets
                    // (the user might have already deleted some of them)
                    xmlFan.RemoveAll();

                    // Iterate through the fan programs
                    foreach(string name in FanProgram.Keys) {

                        // Create an element for each program
                        XmlElement node = (XmlElement) xmlFan.AppendChild(
                                xml.CreateElement(XmlElementFanProgram));

                        // Store the program name in an attribute
                        node.SetAttribute(XmlAttrFanProgramName, name);

                        // Create an element to store the fan mode
                        node.AppendChild(xml.CreateElement(XmlElementFanProgramMode)).InnerText =
                            Enum.GetName(typeof(BiosData.FanMode), FanProgram[name].FanMode);

                        // Create an element to store the GPU power level
                        node.AppendChild(xml.CreateElement(XmlElementFanProgramPower)).InnerText =
                            Enum.GetName(typeof(BiosData.GpuPowerLevel), FanProgram[name].GpuPower);

                        // For each programmed fan level
                        foreach(byte temperature in FanProgram[name].Level.Keys) {

                            // Create an element to store the level data
                            XmlElement level = (XmlElement) node.AppendChild(xml.CreateElement(XmlElementFanProgramLevel));

                            // Store the temperature
                            level.SetAttribute(XmlAttrFanProgramLevelTemperature, Conv.GetString(temperature, 2, 10));

                            // Store the CPU fan level
                            level.AppendChild(xml.CreateElement(XmlElementFanProgramLevelCpu)).InnerText =
                                Conv.GetString(FanProgram[name].Level[temperature][0], 2, 10);

                            // Store the GPU fan level
                            level.AppendChild(xml.CreateElement(XmlElementFanProgramLevelGpu)).InnerText =
                                Conv.GetString(FanProgram[name].Level[temperature][1], 2, 10);

                        }

                    }

                    // Continue with the configuration values
                    SetString(xml, XmlPrefix + "GpuPowerDefault", GpuPowerDefault);
                    SetUInt(xml, XmlPrefix + "GpuPowerSetInterval", (uint) GpuPowerSetInterval);
                    SetBool(xml, XmlPrefix + "GuiCloseWindowExit", GuiCloseWindowExit);
                    SetBool(xml, XmlPrefix + "GuiDpiChangeResize", GuiDpiChangeResize);
                    SetBool(xml, XmlPrefix + "GuiDynamicIcon", GuiDynamicIcon);
                    SetBool(xml, XmlPrefix + "GuiDynamicIconHasBackground", GuiDynamicIconHasBackground);
                    SetBool(xml, XmlPrefix + "GuiStayOnTop", GuiStayOnTop);
                    SetUInt(xml, XmlPrefix + "GuiSysInfoFontSize", (uint) GuiSysInfoFontSize);
                    SetUInt(xml, XmlPrefix + "GuiTipDuration", (uint) GuiTipDuration);
                    SetBool(xml, XmlPrefixKeyCustomAction + "Enabled", KeyCustomActionEnabled);
                    SetString(xml, XmlPrefixKeyCustomAction + "ExecCmd", KeyCustomActionExecCmd);
                    SetString(xml, XmlPrefixKeyCustomAction + "ExecArgs", KeyCustomActionExecArgs);
                    SetBool(xml, XmlPrefixKeyCustomAction + "Minimized", KeyCustomActionMinimized);
                    SetBool(xml, XmlPrefix + "KeyToggleFanProgram", KeyToggleFanProgram);
                    SetBool(xml, XmlPrefix + "KeyToggleFanProgramCycleAll", KeyToggleFanProgramCycleAll);
                    SetBool(xml, XmlPrefix + "KeyToggleFanProgramShowGuiFirst", KeyToggleFanProgramShowGuiFirst);
                    SetBool(xml, XmlPrefix + "KeyToggleFanProgramSilent", KeyToggleFanProgramSilent);
                    SetUInt(xml, XmlPrefix + "PresetRefreshRateHigh", (uint) PresetRefreshRateHigh);
                    SetUInt(xml, XmlPrefix + "PresetRefreshRateLow", (uint) PresetRefreshRateLow);

                    // Temperature sensors (alphabetical order maintained)
                    // Ensure the parent element node exists, or create it
                    XmlElement xmlTemperature = (XmlElement) SetPath(xml, XmlPrefixTemperature);

                    // Remove all currently-defined sensor entries
                    xmlTemperature.RemoveAll();

                    // Iterate through the sensor entries
                    foreach(string name in TemperatureSensor.Keys) {

                        // Create an element for each sensor
                        XmlElement node = (XmlElement) xmlTemperature.AppendChild(
                                xml.CreateElement(XmlElementTemperatureSensor));

                        // Store the preset name and source in attributes
                        node.SetAttribute(XmlAttrTemperatureSensorName, name);
                        node.SetAttribute(XmlAttrTemperatureSensorSource,
                            TemperatureSensor[name].Source == PlatformData.LinkType.EmbeddedController ?
                                XmlAttrTemperatureSensorSourceValueEc : XmlAttrTemperatureSensorSourceValueBios);

                        if(!TemperatureSensor[name].Use)
                            node.SetAttribute(XmlAttrTemperatureSensorUse, XmlSaveBoolFalse);

                    }

                    // The remaining configuration values
                    SetUInt(xml, XmlPrefix + "UpdateIconInterval", (uint) UpdateIconInterval);
                    SetUInt(xml, XmlPrefix + "UpdateMonitorInterval", (uint) UpdateMonitorInterval);
                    SetUInt(xml, XmlPrefix + "UpdateProgramInterval", (uint) UpdateProgramInterval);

                    // Save the file
                    XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                    xmlWriterSettings.Encoding = new UTF8Encoding(XmlSaveBom);
                    xmlWriterSettings.Indent = true;
                    xmlWriterSettings.IndentChars = XmlSaveIndent;
                    xmlWriterSettings.NewLineHandling = NewLineHandling.Replace;
                    using(XmlWriter xmlWriter = XmlWriter.Create(FilePath, xmlWriterSettings))
                        xml.Save(xmlWriter);

                } catch {

                    // Show an error message if the settings could not be saved
                    App.Error("ErrConfigSave");

                }

            }

        }

        // Sets a Boolean flag in the XML configuration file
        private static bool SetBool(XmlDocument xml, string node, bool value) {
            try {
                (SetPath(xml, node)).InnerText = value ? XmlSaveBoolTrue : XmlSaveBoolFalse;
                return true;
            } catch {  }
            return false;
        }

        // Ensures all intermediate nodes exist along an XML search path,
        // then returns the requested node as an object
        private static XmlNode SetPath(XmlDocument xml, XmlNode parent, string path) {
            XmlNode node;

            // Split the path into individual node names
            string[] nodes = path.Trim('/').Split('/');

            // If the next node name is empty, return the parent node
            if(string.IsNullOrEmpty(nodes[0]))
                return parent;

            // Create the node if it does not exist
            if((node = parent.SelectSingleNode(nodes[0])) == null)
                node = parent.AppendChild(xml.CreateElement(nodes[0]));

            // Recursively process the remaining nodes along the path
            return SetPath(xml, node,
                path.Length > nodes[0].Length ?
                    path.Substring(nodes[0].Length + 1) : "");

        }

        // Wrapper for SetPath() starting at document root
        private static XmlNode SetPath(XmlDocument xml, string path) {
            return SetPath(xml, (XmlNode) xml, path);
        }

        // Sets a string value in the XML configuration file
        private static bool SetString(XmlDocument xml, string node, string value) {
            try {
                (SetPath(xml, node)).InnerText = value;
                return true;
            } catch {  }
            return false;
        }

        // Sets an unsigned double word-sized value in the XML configuration file
        private static bool SetUInt(XmlDocument xml, string node, uint value, int padding = 1, int nbase = 10) {
            try {
                (SetPath(xml, node)).InnerText = Conv.GetString(value, padding, nbase);
                return true;
            } catch {  }
            return false;
        }
#endregion

#region Error Handling
        // Retrieves a concatenated error message
        public static string GetError(string messageIds, Exception e = null) {

            // A bit of a chicken and egg problem
            if(Config.Locale == null)
                return "Failed to instantiate the localizable message system";

            int messageCount = 0;
            string message = "";

            // Obtain a localization string for each message identifier
            foreach(string messageId in messageIds.Split('|')) {

                // If multiple messages, add a separator
                // between the first and the second only
                message += messageCount > 0 ? messageCount > 1 ? "" : ": " : "";

                // Append the next message
                // Exception message is a special case
                if(messageId == "EXCEPTION")
                    message += e != null ? e.Message : Config.Locale.Get("ErrUnexpectedReally");
                else
                    message += Config.Locale.Get(messageId);

                // Count the messages processed so far
                messageCount++;
            }

            return message;
        }
#endregion

    }

}
