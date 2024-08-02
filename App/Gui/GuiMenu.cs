  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023-2024 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using OmenMon.Hardware.Bios;
using OmenMon.Hardware.Ec;
using OmenMon.Library;

namespace OmenMon.AppGui {

#region Renderer Override
    // Overrides the menu renderer
    public class GuiMenuCustomRenderer : ToolStripProfessionalRenderer {

        // Checks if an item is tagged as not selectable
        private bool IsNoSelect(object item) {
            try {
                if(((string) item).Contains(GuiMenu.MENU_TAG_NO_SELECT))
                    return true;
            } catch { }
            return false;
        }

        // Overrides the default renderer to disable menu header selection
        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e) {

            // Skip highlighting of menu items tagged not to be selectable
            if(!IsNoSelect(e.Item.Tag))
               base.OnRenderMenuItemBackground(e);

        }

    }
#endregion

    // Handles the notification icon context menu
    public class GuiMenu {

#region Constants
        // Menu item prefixes, interfixes, and suffixes
        private const string P_FAN_MODE = Gui.M_ACT + Gui.G_FAN + "Mode";
        private const string X_FAN_MODE_LEGACY = "L";

        private const string P_FAN_PROG = Gui.M_ACT + Gui.G_FAN + "Prog";

        private const string P_GPU_POWER = Gui.M_ACT + Gui.G_GPU + "Power";
        private const string S_GPU_POWER_MAX = "Max";
        private const string S_GPU_POWER_MED = "Med";
        private const string S_GPU_POWER_MIN = "Min";

        private const string P_GPU_REFRESH = Gui.M_ACT + Gui.G_GPU + "Refresh";
        private const string S_GPU_REFRESH_HIGH = "High";
        private const string S_GPU_REFRESH_LOW = "Low";

        private const string P_GPU_MODE = Gui.M_ACT + Gui.G_GPU + "Mode";
        private const string S_GPU_MODE_DISCRETE = "Discrete";
        private const string S_GPU_MODE_OPTIMUS = "Optimus";

        private const string P_KBD_COLOR_PRESET = Gui.M_ACT + Gui.G_KBD + "ColorPreset";
        private const string X_KBD_COLOR_PRESET_DEFAULT = Config.ColorPresetDefaultPrefix;

        private const string P_SET_TASK = Gui.M_ACT + Gui.G_SET + "Task";
        private const string S_SET_TASK_GUI = "Gui";
        private const string S_SET_TASK_KEY = "Key";
        private const string S_SET_TASK_MUX = "Mux";

        private const string S_TOGGLE_FORM_MAIN_HIDE = "Hide";

        // Menu item identifiers
        private const string I_APPNAME = Gui.M_HDR + "App";
        private const string I_APPLANG = Gui.M_HDR + "AppLang";

        private const string I_FAN = Gui.M_SUB + Gui.G_FAN;
        private const string I_FAN_MAX = Gui.M_ACT + Gui.G_FAN + "Max";
        private const string I_FAN_OFF = Gui.M_ACT + Gui.G_FAN + "Off";

        private const string I_GPU = Gui.M_SUB + Gui.G_GPU;
        private const string I_GPU_DISPLAY_COLOR = Gui.M_ACT + Gui.G_GPU + "DisplayColor";
        private const string I_GPU_DISPLAY_OFF = Gui.M_ACT + Gui.G_GPU + "DisplayOff";
        private const string I_GPU_POWER_MAX = P_GPU_POWER + S_GPU_POWER_MAX;
        private const string I_GPU_POWER_MED = P_GPU_POWER + S_GPU_POWER_MED;
        private const string I_GPU_POWER_MIN = P_GPU_POWER + S_GPU_POWER_MIN;
        private const string I_GPU_MODE_DISCRETE = P_GPU_MODE + S_GPU_MODE_DISCRETE;
        private const string I_GPU_MODE_OPTIMUS = P_GPU_MODE + S_GPU_MODE_OPTIMUS;

        private const string I_KBD = Gui.M_SUB + Gui.G_KBD;
        private const string I_KBD_BACKLIGHT = Gui.M_ACT + Gui.G_KBD + "Backlight";

        private const string I_SET = Gui.M_SUB + Gui.G_SET;
        private const string I_SET_STAY_TOP = Gui.M_ACT + Gui.G_SET + "StayTop";
        private const string I_SET_ICON_DYN = Gui.M_ACT + Gui.G_SET + "IconDyn";
        private const string I_SET_ICON_DYN_BG = Gui.M_ACT + Gui.G_SET + "IconDynBg";
        private const string I_SET_TASK_GUI = P_SET_TASK + S_SET_TASK_GUI;
        private const string I_SET_AUTOCONFIG = Gui.M_ACT + Gui.G_SET + "Autoconfig";
        private const string I_SET_TASK_KEY = P_SET_TASK + S_SET_TASK_KEY;
        private const string I_SET_TASK_MUX = P_SET_TASK + S_SET_TASK_MUX;

        private const string I_TOGGLE_FORM_MAIN = Gui.M_ACT + "ToggleFormMain";
        private const string I_EXIT = Gui.M_ACT + "Exit";

        // Menu tag identifiers
        internal const string MENU_TAG_PERSIST = "Persist";
        internal const string MENU_TAG_NO_SELECT = "NoSelect";
#endregion

#region Data & Initialization
        // Parent class reference
        private GuiTray Context;

        // Flag that marks the menu not to be closed due to the last-clicked item
        private bool IsPersistent;

        // Sub-menu branch data
        private ToolStripMenuItem MenuFan, MenuGpu, MenuKbd, MenuSettings;

        // Constructs the menu class
        public GuiMenu(GuiTray context) {

            // Initialize the parent class reference
            this.Context = context;

            // Create the menu
            Create();

        }
#endregion

#region Menu Action Events
        // Shows the about dialog
        private void EventActionAbout(object sender, EventArgs e) {

            // Show the dialog
            GuiOp.About();

        }

        // Toggles the keyboard backlight on or off
        private void EventActionBacklight(object sender, EventArgs e) {

            // If the main form and the keyboard class have been initialized
            if(Context.FormMain != null && Context.FormMain.Kbd != null) {

                // Use the main form routine to update
                Context.FormMain.Kbd.SetBacklight(!((ToolStripMenuItem) sender).Checked);

                // Update the main form
                Context.FormMain.UpdateKbd();

            } else

                // Make a platform call otherwise
                Context.Op.Platform.System.SetKbdBacklight(!((ToolStripMenuItem) sender).Checked);

            // Update the menu section
            UpdateKbdBacklight();

        }

        // Changes the keyboard backlight color
        private void EventActionBacklightColor(object sender, EventArgs e) {

            // If the main form and the keyboard class have been initialized
            if(Context.FormMain != null && Context.FormMain.Kbd != null) {

                // Use the main form routine to update
                Context.FormMain.Kbd.SetColors(
                    Config.ColorPreset[((ToolStripMenuItem) sender).Name.Remove(0, P_KBD_COLOR_PRESET.Length)]);

                // Update the main form
                Context.FormMain.UpdateKbd();

            } else

                // Make a platform call otherwise
                Context.Op.Platform.System.SetKbdColor(
                    Config.ColorPreset[((ToolStripMenuItem) sender).Name.Remove(0, P_KBD_COLOR_PRESET.Length)]);

            // Update the menu section
            UpdateKbdColorPreset();

        }

        // Switches the display off
        internal void EventActionDisplayOff(object sender, EventArgs e) {

            // Make the system call
            Os.SetDisplayOff();

            // Close the menu
            ((ToolStripDropDownItem) ((ToolStripMenuItem) sender).OwnerItem).DropDown.AutoClose = true;
            Context.Notification.ContextMenuStrip.Hide();

        }

        // Exits the application
        internal void EventActionExit(object sender, EventArgs e) {

            // Make the exit call
            Application.Exit();

        }

        // Toggles the maximum fan speed on and off
        private void EventActionFanMax(object sender, EventArgs e) {

            // Toggle the maximum fan speed
            Context.Op.Platform.Fans.SetMax(!((ToolStripMenuItem) sender).Checked);

            // Update the main form, if available
            if(Context.FormMain != null)
                Context.FormMain.UpdateFanCtl();

            // Update the menu section
            UpdateFan();

        }

        // Toggles the fan on and off entirely
        private void EventActionFanOff(object sender, EventArgs e) {

            // Toggle the fan on or off
            Context.Op.Platform.Fans.SetOff(!((ToolStripMenuItem) sender).Checked);

            // Update the main form, if available
            if(Context.FormMain != null)
                Context.FormMain.UpdateFanCtl();

            // Update the menu section
            UpdateFan();

        }

        // Switches the fan mode
        private void EventActionFanMode(object sender, EventArgs e) {

            // Retrieve the current fan mode
            BiosData.FanMode fanModeNow = Context.Op.Platform.Fans.GetMode();

            // Retrieve the requested fan mode
            BiosData.FanMode fanModeAsk = (BiosData.FanMode) Enum.Parse(
                typeof(BiosData.FanMode), 
                ((ToolStripMenuItem) sender).Name.Remove(0, P_FAN_MODE.Length));

            // Proceed only if the requested mode
            // is different than the current one
            if(fanModeAsk != fanModeNow) {

                // Set the requested fan mode
                Context.Op.Platform.Fans.SetMode(fanModeAsk);

                // Update the main form, if available
                if(Context.FormMain != null)
                    Context.FormMain.UpdateFanCtl();

                // Update the menu section
                UpdateFan();

            }

        }

        // Switches the fan program
        private void EventActionFanProg(object sender, EventArgs e) {

            // Retrieve the current fan program
            string fanProgNameNow = Context.Op.Program.GetName();

            // Retrieve the requested fan program
            string fanProgNameAsk =
                ((ToolStripMenuItem) sender).Name.Remove(0, P_FAN_PROG.Length);

            // If the requested mode is the same as the current one
            // and the program is already enabled
            if(fanProgNameAsk == fanProgNameNow
                && Context.Op.Program.IsEnabled)

                // Terminate the current program
                Context.Op.Program.Terminate();

            else

                // Start a new program
                Context.Op.Program.Run(fanProgNameAsk);

            // Reset the tick counter
            Context.UpdateProgramTick = 1;

            // Update the main form
            if(Context.FormMain != null)
                Context.FormMain.UpdateFanCtl();

            // Update the menu section
            UpdateFan();

        }

        // Reloads the display color profile
        private void EventActionDisplayColor(object sender, EventArgs e) {

            // Call the native method
            Os.ReloadColorSettings();

        }

        // Switches the GPU mode
        private void EventActionGpuMode(object sender, EventArgs e) {

            // Retrieve the current GPU mode
            BiosData.GpuMode gpuModeNow = Context.Op.Platform.System.GetGpuMode(true);
            BiosData.GpuMode gpuModeAsk =
                ((ToolStripMenuItem) sender).Name.EndsWith(S_GPU_MODE_DISCRETE) ?
                    BiosData.GpuMode.Discrete :
                       Context.Op.Platform.System.GetSystemData()
                       .GpuModeSwitch.HasFlag(BiosData.SysGpuModeSwitch.Supported8) ?
                           BiosData.GpuMode.Optimus : BiosData.GpuMode.Hybrid;

            // Proceed only if the mode is different than now
            if(gpuModeAsk != gpuModeNow) {

                // Set the requested GPU mode
                Context.Op.Platform.System.SetGpuMode(gpuModeAsk);

                // Update the menu section
                UpdateGpuMode();

                // A restart is needed for the change to take effect
                Gui.ShowPromptReboot();

            }

        }

        // Changes the GPU power settings
        private void EventActionGpuPower(object sender, EventArgs e) {
            BiosData.GpuPowerData gpuPowerData;

            // Determine the GPU power data from the selected menu option
            if(((ToolStripMenuItem) sender).Name.EndsWith(S_GPU_POWER_MAX))
                gpuPowerData = new BiosData.GpuPowerData(BiosData.GpuPowerLevel.Maximum);
            else if(((ToolStripMenuItem) sender).Name.EndsWith(S_GPU_POWER_MED))
                gpuPowerData = new BiosData.GpuPowerData(BiosData.GpuPowerLevel.Medium);
            else
                gpuPowerData = new BiosData.GpuPowerData(BiosData.GpuPowerLevel.Minimum);

            // Set the requested GPU power
            Context.Op.Platform.System.SetGpuPower(gpuPowerData);

            // Update the menu section
            UpdateGpuPower();

        }

        // Changes the display refresh rate
        private void EventActionGpuRefresh(object sender, EventArgs e)
        {
            // Set the requested refresh rate
            int refreshRate = int.Parse(((ToolStripMenuItem)sender).Name.TrimStart(P_GPU_REFRESH.ToCharArray()));
            Os.SetRefreshRate(refreshRate);

            // Update the menu section
            UpdateGpuRefresh();
        }

        // Toggles the main GUI form
        internal void EventActionShowFormMain(object sender, EventArgs e) {

            // Show the form if it's hidden,
            // hide it otherwise
            Context.ToggleFormMain();
 
       }

        // Toggles whether to stay on top of other windows or not
        private void EventActionToggleStayOnTop(object sender, EventArgs e) {

            // Toggle between the two options
            Config.GuiStayOnTop = !Config.GuiStayOnTop;

            // Update the form, if available
            if(Context.FormMain != null && Context.FormMain.Visible)
                Context.FormMain.TopMost = Config.GuiStayOnTop;

            // Save the settings
            Config.Save();

            // Update the menu
            ((ToolStripMenuItem) sender).Checked = Config.GuiStayOnTop;

        }

        // Toggles the dynamic notification icon on or off
        private void EventActionToggleIconDynamic(object sender, EventArgs e) {

            // Toggle between the dynamic and static icon
            Config.GuiDynamicIcon = !Config.GuiDynamicIcon;
            Context.Icon.SetDynamic(Config.GuiDynamicIcon);

            // Reset the icon tick counter,
            // the change takes effect immediately
            Context.UpdateIconTick = 0;

            // Save the settings
            Config.Save();

            // Update the menu
            ((ToolStripMenuItem) sender).Checked = Context.Icon.IsDynamic;

            // Toggle the setting for dynamic icon background
            // so that it is disabled if the icon is static
            ((ToolStripDropDownItem) ((ToolStripItem) sender).OwnerItem)
                .DropDownItems[I_SET_ICON_DYN_BG].Enabled = Context.Icon.IsDynamic;

        }

        // Toggles the dynamic background for the dynamic notification icon on or off
        private void EventActionToggleIconDynamicBackground(object sender, EventArgs e) {

            // Toggle the dynamic icon background on or off
            Config.GuiDynamicIconHasBackground = !Config.GuiDynamicIconHasBackground;
            Context.Icon.SetDynamicBackground(Config.GuiDynamicIconHasBackground);

            // Reset the icon tick counter,
            // the change takes effect immediately
            Context.UpdateIconTick = 0;

            // Save the settings
            Config.Save();

            // Update the menu
            ((ToolStripMenuItem) sender).Checked = Context.Icon.IsDynamicBackground;

        }

        // Toggles whether the application will automatically apply the configuration on startup
        private void EventActionToggleAutoConfig(object sender, EventArgs e) {

            // Toggle the setting
            Config.AutoConfig = !((ToolStripMenuItem) sender).Checked;

            // Save the settings
            Config.Save();

            // Update the menu
            ((ToolStripMenuItem) sender).Checked = Config.AutoConfig;

        }

        // Toggles whether the application will start automatically on Windows startup
        private void EventActionToggleTask(object sender, EventArgs e) {

            // Retrieve the requested task identifier
            Config.TaskId taskId = (Config.TaskId) Enum.Parse(
                typeof(Config.TaskId), 
                ((ToolStripMenuItem) sender).Name.Remove(0, P_SET_TASK.Length));

            // Make the change
            Hw.TaskSet(taskId, !((ToolStripMenuItem) sender).Checked);

            // Update the menu
            ((ToolStripMenuItem) sender).Checked = Hw.TaskGet(taskId);

        }
#endregion

#region Menu Handling Events
        // Sets the cursor to hand when it's over the application name header
        private void EventAppHeaderMouseEnter(object sender, EventArgs e) {
            Context.Notification.ContextMenuStrip.Cursor = Cursors.Hand;
        }

        // Sets the cursor back to default when it's no longer over the application name header
        private void EventAppHeaderMouseLeave(object sender, EventArgs e) {
            Context.Notification.ContextMenuStrip.Cursor = Cursors.Default;
        }

        // Prevents the menu from closing automatically (as long as the cursor is over it)
        private void EventDropDownMouseEnter(object sender, EventArgs e) {
            ((ToolStripDropDown) sender).AutoClose = false;
        }

        // Lets the menu close automatically (as soon as the cursor is no longer over it)
        private void EventDropDownMouseLeave(object sender, EventArgs e) {
            ((ToolStripDropDown) sender).AutoClose = true;
        }

        // Dynamically updates only the submenu that is just about to be opened
        private void EventDropDownOpening(object sender, CancelEventArgs e) {

            switch(((ToolStripDropDown) sender).OwnerItem.Name) {

                case I_FAN:
                    // Update the fan menu
                    UpdateFan();
                    break;

                case I_GPU:
                    // Update the graphics menu
                    UpdateGpuMode();
                    UpdateGpuPower();
                    UpdateGpuRefresh();
                    break;

                case I_KBD:
                    // Update the keyboard menu
                    UpdateKbdBacklight();
                    UpdateKbdColorPreset();
                    break;

                case I_SET:
                    // Update the settings menu
                    UpdateSettings();
                    break;

            }

        }

        // Handles the menu closing event, refuses to close it for items tagged persistent
        internal void EventClosing(object sender, ToolStripDropDownClosingEventArgs e) {

            // Only if the reason for closing the menu is an item click
            if(e.CloseReason == ToolStripDropDownCloseReason.ItemClicked) {

                // If the last-clicked item marked it as persistent
                if(IsPersistent) {

                    // Hold the menu open
                    e.Cancel = true;

                    // Reset the persistence flag
                    IsPersistent = false;

                }

            }

        }

        // Handles a menu item click event
        // Sets the menu to stay open if the item is tagged as persistent
        internal void EventItemClicked(object sender, ToolStripItemClickedEventArgs e) {

            try {

                // The clicked item is tagged with a persistent flag
                if(((string) e.ClickedItem.Tag).Contains(MENU_TAG_PERSIST))

                    // Set the persistence flag
                    IsPersistent = true;

            } catch { }

        }

        // Dynamically updates the menu just before it is about to be opened
        internal void EventOpening(object sender, CancelEventArgs e) {

            // Since we populate the menu dynamically, it might be empty the first time,
            // in which case the system "optimizes" its cancel flag to true, overriden here
            e.Cancel = false;

            // Update the main menu
            UpdateMain();

        }
#endregion

#region Menu Setup
        // Generates the initial menu
        public void Create() {
            Delete();

            // Set a custom renderer override
            Context.Notification.ContextMenuStrip.Renderer = new GuiMenuCustomRenderer();

            // Define the fan menu
            MenuFan = new ToolStripMenuItem(Config.Locale.Get(Config.L_GUI_MENU + I_FAN), null, null, I_FAN);
            MenuFan.DropDown.MouseEnter += EventDropDownMouseEnter;
            MenuFan.DropDown.MouseLeave += EventDropDownMouseLeave;
            MenuFan.DropDown.Opening += EventDropDownOpening;
            MenuFan.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem(Config.Locale.Get(Config.L_GUI_MENU + I_FAN_MAX), null, EventActionFanMax, I_FAN_MAX),
                new ToolStripSeparator(),
            });

            // Add fan programs only if the list is not empty,
            // and end the section with an extra separator
            if(Config.FanProgram.Keys.Count > 0) {
                foreach(string name in Config.FanProgram.Keys) {
                    MenuFan.DropDownItems.Add(new ToolStripMenuItem(
                        name,
                        null, EventActionFanProg, P_FAN_PROG + name));
                }
                MenuFan.DropDownItems.Add(new ToolStripSeparator());
            }

            // Continue with the remaining part of the fan menu
            foreach(string name in Enum.GetNames(typeof(BiosData.FanMode))) {
                MenuFan.DropDownItems.Add(new ToolStripMenuItem(
                    Config.Locale.Get(Config.L_GUI_MENU + P_FAN_MODE + name),
                    null, EventActionFanMode, P_FAN_MODE + name));
            }
            MenuFan.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripSeparator(),
                new ToolStripMenuItem(Config.Locale.Get(Config.L_GUI_MENU + I_FAN_OFF), null, EventActionFanOff, I_FAN_OFF),
            });

            // Define the graphics menu
            MenuGpu = new ToolStripMenuItem(Config.Locale.Get(Config.L_GUI_MENU + I_GPU), null, null, I_GPU);
            MenuGpu.DropDown.MouseEnter += EventDropDownMouseEnter;
            MenuGpu.DropDown.MouseLeave += EventDropDownMouseLeave;
            MenuGpu.DropDown.Opening += EventDropDownOpening;
            var refreshRates = Os.GetAvailableRefreshRates();
            foreach (var refreshRate in refreshRates)
            {
                MenuGpu.DropDownItems.Add(
                    new ToolStripMenuItem(refreshRate.ToString() + " " + Config.Locale.Get(Config.L_UNIT + "Frequency"),
                        null, EventActionGpuRefresh, P_GPU_REFRESH + refreshRate.ToString()));
            }

            MenuGpu.DropDownItems.AddRange(new ToolStripItem[]
            {
                new ToolStripSeparator(),
                new ToolStripMenuItem(Config.Locale.Get(Config.L_GUI_MENU + I_GPU_POWER_MIN), null, EventActionGpuPower, I_GPU_POWER_MIN),
                new ToolStripMenuItem(Config.Locale.Get(Config.L_GUI_MENU + I_GPU_POWER_MED), null, EventActionGpuPower, I_GPU_POWER_MED),
                new ToolStripMenuItem(Config.Locale.Get(Config.L_GUI_MENU + I_GPU_POWER_MAX), null, EventActionGpuPower, I_GPU_POWER_MAX),
                new ToolStripSeparator(),
                new ToolStripMenuItem(Config.Locale.Get(Config.L_GUI_MENU + I_GPU_DISPLAY_COLOR), null, EventActionDisplayColor, I_GPU_DISPLAY_COLOR),
                new ToolStripMenuItem(Config.Locale.Get(Config.L_GUI_MENU + I_GPU_DISPLAY_OFF), null, EventActionDisplayOff, I_GPU_DISPLAY_OFF),
                new ToolStripSeparator(),
                new ToolStripMenuItem(Config.Locale.Get(Config.L_GUI_MENU + I_GPU_MODE_DISCRETE), null, EventActionGpuMode, I_GPU_MODE_DISCRETE),
                new ToolStripMenuItem(Config.Locale.Get(Config.L_GUI_MENU + I_GPU_MODE_OPTIMUS), null, EventActionGpuMode, I_GPU_MODE_OPTIMUS),
            });

            // Define the keyboard menu
            MenuKbd = new ToolStripMenuItem(Config.Locale.Get(Config.L_GUI_MENU + I_KBD), null, null, I_KBD);
            MenuKbd.DropDown.MouseEnter += EventDropDownMouseEnter;
            MenuKbd.DropDown.MouseLeave += EventDropDownMouseLeave;
            MenuKbd.DropDown.Opening += EventDropDownOpening;
            MenuKbd.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem(Config.Locale.Get(Config.L_GUI_MENU + I_KBD_BACKLIGHT), null, EventActionBacklight, I_KBD_BACKLIGHT),
                new ToolStripSeparator(),
            });

            // Add keyboard color presets
            foreach(string name in Config.ColorPreset.Keys)
                MenuKbd.DropDownItems.Add(new ToolStripMenuItem(
                    name.StartsWith(X_KBD_COLOR_PRESET_DEFAULT) ? Config.Locale.Get(Config.L_GUI_MENU + P_KBD_COLOR_PRESET + name) : name,
                    null, EventActionBacklightColor, P_KBD_COLOR_PRESET + name));

            // Define the settings menu
            MenuSettings = new ToolStripMenuItem(Config.Locale.Get(Config.L_GUI_MENU + I_SET), null, null, I_SET);
            MenuSettings.DropDown.MouseEnter += EventDropDownMouseEnter;
            MenuSettings.DropDown.MouseLeave += EventDropDownMouseLeave;
            MenuSettings.DropDown.Opening += EventDropDownOpening;
            MenuSettings.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem(Config.Locale.Get(Config.L_GUI_MENU + I_SET_STAY_TOP), null, EventActionToggleStayOnTop, I_SET_STAY_TOP),
                new ToolStripSeparator(),
                new ToolStripMenuItem(Config.Locale.Get(Config.L_GUI_MENU + I_SET_ICON_DYN), null, EventActionToggleIconDynamic, I_SET_ICON_DYN),
                new ToolStripMenuItem(Config.Locale.Get(Config.L_GUI_MENU + I_SET_ICON_DYN_BG), null, EventActionToggleIconDynamicBackground, I_SET_ICON_DYN_BG),
                new ToolStripSeparator(),
                new ToolStripMenuItem(Config.Locale.Get(Config.L_GUI_MENU + I_SET_TASK_GUI), null, EventActionToggleTask, I_SET_TASK_GUI),
                new ToolStripMenuItem(Config.Locale.Get(Config.L_GUI_MENU + I_SET_AUTOCONFIG), null, EventActionToggleAutoConfig, I_SET_AUTOCONFIG),
                new ToolStripSeparator(),
                new ToolStripMenuItem(Config.Locale.Get(Config.L_GUI_MENU + I_SET_TASK_KEY), null, EventActionToggleTask, I_SET_TASK_KEY),
                new ToolStripMenuItem(Config.Locale.Get(Config.L_GUI_MENU + I_SET_TASK_MUX), null, EventActionToggleTask, I_SET_TASK_MUX)
            });

            // Define the top-level menu items
            Context.Notification.ContextMenuStrip.Items.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem(Config.AppName, OmenMon.Resources.Icon.ToBitmap(), EventActionAbout, I_APPNAME),
                new ToolStripMenuItem(Config.Locale.Get(Config.L_GUI + "Translated"), null, null, I_APPLANG),
                new ToolStripSeparator(),
                MenuFan,
                MenuGpu,
                MenuKbd,
                MenuSettings,
                new ToolStripSeparator(),
                new ToolStripMenuItem(Config.Locale.Get(Config.L_GUI_MENU + I_TOGGLE_FORM_MAIN), null, EventActionShowFormMain, I_TOGGLE_FORM_MAIN),
                new ToolStripMenuItem(Config.Locale.Get(Config.L_GUI_MENU + I_EXIT), null, EventActionExit, I_EXIT)
            });

            // Define the top-level menu settings
            ((ToolStripMenuItem) Context.Notification.ContextMenuStrip.Items[I_APPNAME]).ShortcutKeyDisplayString = Config.AppVersion;
            Context.Notification.ContextMenuStrip.Items[I_APPNAME].MouseEnter += EventAppHeaderMouseEnter;
            Context.Notification.ContextMenuStrip.Items[I_APPNAME].MouseLeave += EventAppHeaderMouseLeave;
            Context.Notification.ContextMenuStrip.Items[I_APPNAME].Tag = MENU_TAG_NO_SELECT;
            Context.Notification.ContextMenuStrip.Items[I_APPLANG].Tag = MENU_TAG_NO_SELECT + MENU_TAG_PERSIST;
            Context.Notification.ContextMenuStrip.Items[I_APPLANG].Enabled = false;
            Context.Notification.ContextMenuStrip.Items[I_APPLANG].Visible = Config.Locale.Get(Config.L_GUI + "Translated") == "" ? false : true;

        }

        // Resets the menu
        public void Delete() {

            // Re-create the sub-menu data
            MenuFan = null;
            MenuGpu = null;
            MenuKbd = null;
            MenuSettings = null;

            // Purge the main menu
            Context.Notification.ContextMenuStrip.Items.Clear();

        }
#endregion

#region Menu Update
        // Updates the checkboxes in the fan mode section
        public void UpdateFan() {

            // Retrieve the current fan mode
            BiosData.FanMode fanModeNow = Context.Op.Platform.Fans.GetMode();
            string fanModeNameNow = Enum.GetName(typeof(BiosData.FanMode), fanModeNow);
            string fanProgNameNow = Context.Op.Program.GetName();
            bool isFanMax = Context.Op.Platform.Fans.GetMax();
            bool isFanProg = Context.Op.Program.IsEnabled;
            bool isFanOff = Context.Op.Platform.Fans.GetOff();

            // Set the checked and enabled state on individual menu items
            ((ToolStripMenuItem) MenuFan.DropDownItems[I_FAN_MAX]).Checked = isFanMax;
            ((ToolStripMenuItem) MenuFan.DropDownItems[I_FAN_OFF]).Checked = isFanOff;

            ((ToolStripMenuItem) MenuFan.DropDownItems[I_FAN_MAX]).Enabled = !isFanOff && !isFanProg;
            ((ToolStripMenuItem) MenuFan.DropDownItems[I_FAN_OFF]).Enabled = !isFanMax && !isFanProg;

            // Iterate through fan modes
            foreach(ToolStripItem item in MenuFan.DropDownItems)
            if(item is ToolStripMenuItem && item.Name.StartsWith(P_FAN_MODE)) {

                // Disable if either maximum fan mode is on
                // or the fan is off entirely
                ((ToolStripMenuItem) item).Enabled = !isFanMax && !isFanOff && !isFanProg;

                // Compare each entry to the current mode
                string fanModeName = item.Name.Remove(0, P_FAN_MODE.Length);
                if(fanModeName == fanModeNameNow) {

                    // Mark as checked if the mode names are the same
                    ((ToolStripMenuItem) item).Checked = true;
                    ((ToolStripMenuItem) item).Visible = true;

                } else {

                    // Uncheck any other fan modes
                    ((ToolStripMenuItem) item).Checked = false;

                    // Hide legacy fan modes
                    if(fanModeName.StartsWith(X_FAN_MODE_LEGACY))
                        ((ToolStripMenuItem) item).Visible = false;

                }
            }

            // Iterate through fan programs
            foreach(ToolStripItem item in MenuFan.DropDownItems)
            if(item is ToolStripMenuItem && item.Name.StartsWith(P_FAN_PROG)) {

                // Disable if either maximum fan mode is on
                // or the fan is off entirely
                ((ToolStripMenuItem) item).Enabled = !isFanMax && !isFanOff;

                // Compare each entry to the current mode
                string fanProgName = item.Name.Remove(0, P_FAN_PROG.Length);
                if(isFanProg && fanProgName == fanProgNameNow)

                    // Mark as checked if the program names are the same
                    // and the program is enabled
                    ((ToolStripMenuItem) item).Checked = true;

                else

                    // Uncheck any other fan programs
                    ((ToolStripMenuItem) item).Checked = false;

            }
        }

        // Updates the checkboxes in the GPU mode switch section
        public void UpdateGpuMode() {

            // Only if GPU mode switching is supported
            if(Context.Op.Platform.System.GetGpuModeSupport()) {

                // Retrieve the current GPU mode
                BiosData.GpuMode gpuMode = Context.Op.Platform.System.GetGpuMode(true);

                // Set the checked status accordingly
                ((ToolStripMenuItem) MenuGpu.DropDownItems[I_GPU_MODE_DISCRETE]).Checked =
                    gpuMode == BiosData.GpuMode.Discrete;
                ((ToolStripMenuItem) MenuGpu.DropDownItems[I_GPU_MODE_OPTIMUS]).Checked =
                    (gpuMode == BiosData.GpuMode.Hybrid || gpuMode == BiosData.GpuMode.Optimus);

            } else {

                // Disable GPU mode switching menu items
                ((ToolStripMenuItem) MenuGpu.DropDownItems[I_GPU_MODE_DISCRETE]).Enabled = false;
                ((ToolStripMenuItem) MenuGpu.DropDownItems[I_GPU_MODE_OPTIMUS]).Enabled = false;

            }

        }

        // Updates the checkboxes in the GPU power settings section
        public void UpdateGpuPower() {

            // Retrieve the current graphics setting table
            BiosData.GpuPowerData gpuPowerData = Context.Op.Platform.System.GetGpuPower(true);

            // Compare the values to presets
            if(gpuPowerData.CustomTgp == BiosData.GpuCustomTgp.On) {

                // If custom TGP is enabled, it's at least the medium setting
                // which is determined by whether PPAB is enabled or disabled
                ((ToolStripMenuItem) MenuGpu.DropDownItems[I_GPU_POWER_MAX]).Checked =
                    gpuPowerData.Ppab == BiosData.GpuPpab.On;
                ((ToolStripMenuItem) MenuGpu.DropDownItems[I_GPU_POWER_MED]).Checked =
                    gpuPowerData.Ppab == BiosData.GpuPpab.Off;
                ((ToolStripMenuItem) MenuGpu.DropDownItems[I_GPU_POWER_MIN]).Checked = false;

            } else {

                // Otherwise, consider it set to the minimum
                ((ToolStripMenuItem) MenuGpu.DropDownItems[I_GPU_POWER_MAX]).Checked = false;
                ((ToolStripMenuItem) MenuGpu.DropDownItems[I_GPU_POWER_MED]).Checked = false;
                ((ToolStripMenuItem) MenuGpu.DropDownItems[I_GPU_POWER_MIN]).Checked = true;

                // Note: this does not cover all eventualities,
                // only those that can be set through the same menu
                // It's possible to set PPAB on with custom TGP off

            }
        }

        // Updates the checkboxes in the display refresh rate section
        public void UpdateGpuRefresh()
        {
            // Retrieve the current refresh rate
            var currentRefreshRate = Os.GetRefreshRate();
            var refreshRates = Os.GetAvailableRefreshRates();

            foreach (var refreshRate in refreshRates)
            {
                var item = (ToolStripMenuItem)MenuGpu.DropDownItems[P_GPU_REFRESH + refreshRate.ToString()];
                item.Checked = currentRefreshRate == refreshRate;
            }

        }

        // Updates the keyboard backlight checkbox
        public void UpdateKbdBacklight() {

            if(Context.Op.Platform.System.GetKbdBacklightSupport())

                // Set the checked status accordingly
                ((ToolStripMenuItem) MenuKbd.DropDownItems[I_KBD_BACKLIGHT]).Checked =
                    Context.Op.Platform.System.GetKbdBacklight() == BiosData.Backlight.On ? true : false;

            else

                // Disable on unsupported devices
                ((ToolStripMenuItem) MenuKbd.DropDownItems[I_KBD_BACKLIGHT]).Enabled = false;

        }

        // Updates the checkboxes in the keyboard color menu
        public void UpdateKbdColorPreset() {

            // Check for unsupported devices
            if(!Context.Op.Platform.System.GetKbdBacklightSupport()
                || !Context.Op.Platform.System.GetKbdColorSupport()) {

                // Disable all items
                foreach(ToolStripItem item in MenuKbd.DropDownItems)
                    if(item is ToolStripMenuItem && item.Name.StartsWith(P_KBD_COLOR_PRESET))
                        ((ToolStripMenuItem) item).Enabled = false;

                // Over
                return;

            }

            // Retrieve the current color table
            BiosData.ColorTable colorNow = Context.Op.Platform.System.GetKbdColor();

            // Iterate through the color presets
            foreach(ToolStripItem item in MenuKbd.DropDownItems)
            if(item is ToolStripMenuItem && item.Name.StartsWith(P_KBD_COLOR_PRESET)) {

                string presetName = item.Name.Remove(0, P_KBD_COLOR_PRESET.Length);
 
                // Compare each to the current colors
                if(colorNow.Zone[0].Value == Config.ColorPreset[presetName].Zone[0].Value
                    && colorNow.Zone[1].Value == Config.ColorPreset[presetName].Zone[1].Value
                    && colorNow.Zone[2].Value == Config.ColorPreset[presetName].Zone[2].Value
                    && colorNow.Zone[3].Value == Config.ColorPreset[presetName].Zone[3].Value)

                    // Mark as checked if the color values are the same
                    ((ToolStripMenuItem) item).Checked = true;

                else

                    // Remove the checked status from any other items
                    ((ToolStripMenuItem) item).Checked = false;

            }

        }

        // Updates the main menu
        public void UpdateMain() {

            // Set the toggle main GUI form text to say either show or hide
            // depending on whether the form is already being shown or not
            ((ToolStripMenuItem) Context.Notification.ContextMenuStrip.Items[I_TOGGLE_FORM_MAIN]).Text =
                Config.Locale.Get(Config.L_GUI_MENU + I_TOGGLE_FORM_MAIN
                + (Context.FormMain == null || !Context.FormMain.Visible ? "" : S_TOGGLE_FORM_MAIN_HIDE));

        }

        // Updates the settings menu
        public void UpdateSettings() {

            // Stay on top of other windows
            ((ToolStripMenuItem) MenuSettings.DropDownItems[I_SET_STAY_TOP]).Checked = Config.GuiStayOnTop;

            // Dynamic icon and background
            ((ToolStripMenuItem) MenuSettings.DropDownItems[I_SET_ICON_DYN]).Checked = Context.Icon.IsDynamic;
            ((ToolStripMenuItem) MenuSettings.DropDownItems[I_SET_ICON_DYN_BG]).Checked = Context.Icon.IsDynamicBackground;
            ((ToolStripMenuItem) MenuSettings.DropDownItems[I_SET_ICON_DYN_BG]).Enabled = Context.Icon.IsDynamic;

            // Automatic configuration
            ((ToolStripMenuItem) MenuSettings.DropDownItems[I_SET_AUTOCONFIG]).Checked = Config.AutoConfig;

            // Tasks
            ((ToolStripMenuItem) MenuSettings.DropDownItems[I_SET_TASK_GUI]).Checked = Hw.TaskGet(Config.TaskId.Gui);
            ((ToolStripMenuItem) MenuSettings.DropDownItems[I_SET_TASK_KEY]).Checked = Hw.TaskGet(Config.TaskId.Key);
            ((ToolStripMenuItem) MenuSettings.DropDownItems[I_SET_TASK_MUX]).Checked = Hw.TaskGet(Config.TaskId.Mux);

        }
#endregion

    }

}
