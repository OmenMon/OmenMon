  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023-2024 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using OmenMon.External;
using OmenMon.Hardware.Bios;
using OmenMon.Hardware.Platform;
using OmenMon.Library;

namespace OmenMon.AppGui {

    // The main GUI form
    public partial class GuiFormMain : Form {

#region Variables
        // Color picker dialog stored globally to preserve user colors
        private ColorDialogEx ColorPicker;

        // Color preset data source
        private List<Object> ColorPresets;

        // Fan mode data source
        private List<Object> FanModes;

        // Fan program data source
        private List<Object> FanPrograms;

        // Holds the custom font
        private Font FigureFont;

        // Stores the class managing the recolored keyboard drawing
        internal GuiKbd Kbd;

        // Stores the previous DPI value, for dynamically-updated scaling
        private int LastDpi;

        // Stores both parts of the system status, so that the other part
        // does not have to be regenerated every time one changes
        private string SysInfo;
        private string SysStatus;

        // Parent class reference
        private GuiTray Context;

        // Stores the component container
        private System.ComponentModel.IContainer Components;
#endregion Variables

#region Construction & Disposal
        // Constructs the form
        public GuiFormMain() {

            // Initialize the parent class reference
            this.Context = GuiTray.Context;

            // Initialize the component model container
            this.Components = new System.ComponentModel.Container();

            // Initialize the data sources
            ColorPresets = new List<Object>();
            FanModes = new List<Object>();
            FanPrograms = new List<Object>();

            // Initialize the form components
            Initialize();

            // Pre-populate the last DPI setting to the value at launch
            this.LastDpi = (int) Gui.GetDeviceContextDpi(IntPtr.Zero);

            // For keyboards that support backlight and color settings
            if(Context.Op.Platform.System.GetKbdBacklightSupport()
                && Context.Op.Platform.System.GetKbdColorSupport()) {

                // Initialize the keyboard color management class
                this.Kbd = new GuiKbd(this.Context);

                // Update the keyboard picture
                this.PicKbd.Image = Kbd.GetImage();

                // Initialize the color picker
                this.ColorPicker = new ColorDialogEx(UpdateKbdCallback);

                // Pre-populate the custom colors for the color picker
                this.ColorPicker.CustomColors = Kbd.UpdateColorPicker(Config.GuiColorPickerCustom);

            } else

                // Show a static disabled keyboard image if unsupported
                this.PicKbd.Image = OmenMon.Resources.KeyboardOff;

            // Set up the fan presets, system status data
            // and temperature readout captions (static)
            SetupFanCtl();
            SetupSys();
            SetupTmp();

            // Update the controls to reflect the initial hardware state
            UpdateAll();

            // Post a status message as a welcome
            UpdateSysMsg(
                Conv.RTF_CF6 + Config.AppName + " "
                + Conv.RTF_CF5 + Config.AppVersion + " "
                + Conv.RTF_CF2 + Config.Locale.Get(Config.L_GUI_MAIN + Gui.G_SYS + "MsgWelcome"));

        }

        // Handles component disposal
        protected override void Dispose(bool isDisposing) {
	
            if(isDisposing && Components != null)
                Components.Dispose();
	
            // Perform the usual tasks
            base.Dispose(isDisposing);
	
        }

        // Makes the F1 key open the About dialog
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            if(keyData == Keys.F1) {
                GuiOp.About();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        // Makes the Esc key close the form
        protected override bool ProcessDialogKey(Keys keyData) {
            if(Form.ModifierKeys == Keys.None && keyData == Keys.Escape) {
                this.Close();
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

        // Overrides the window procedure to capture a DPI-change event
        // Catching this via the .NET API is conditional upon a configuration setting,
        // which would require a *.exe.config file to be present in the same location
        // Much better to keep everything contained to a single file, so not an option
        protected override void WndProc(ref Message m) {

            // Alternatively, override DefWndProc to intercept WM_GETDPISCALEDSIZE,
            // which gets dispatched before any DPI changes will have happened
            if(m.Msg == User32.WM_DPICHANGED)

                    // Update the form to match the new scaling
                    // Parameter is the horizontal DPI, but vertical is always the same
                    UpdateDpi(m.WParam.ToInt32() & 0xFFFF);

            // Run the base procedure
            base.WndProc(ref m);

        }
#endregion

#region Event Actions
        // Toggles the keyboard backlight on or off
        private void EventActionBacklight(object sender, EventArgs e) {

            if(Kbd != null) // Use the keyboard class
                Kbd.SetBacklight(!this.ChkKbdBacklight.Checked);

            else // Fallback case for no customizable backlight color, only backlight toggle
                Context.Op.Platform.System.SetKbdBacklight(!this.ChkKbdBacklight.Checked);

            UpdateKbd();
        }

        // Deletes a color preset
        private void EventActionColorPresetDel(object sender, EventArgs e) {

            // No preset selected
            if(this.CmbKbdColorPreset.SelectedValue == null || (string) this.CmbKbdColorPreset.SelectedValue == "")
                MessageBox.Show(
                    this, // Modal
                    Config.Locale.Get(Config.L_GUI_MAIN + "KbdColorPresetDelNoSel"),
                    Config.Locale.Get(Config.L_GUI_MAIN + "KbdColorPresetDel"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);

            else if(MessageBox.Show(
                    this, // Modal
                    Config.Locale.Get(Config.L_GUI_MAIN + "KbdColorPresetDelPrompt") + ": "
                        + ((object) this.CmbKbdColorPreset.SelectedItem)
                            .GetType()
                            .GetProperty("Text")
                            .GetValue(this.CmbKbdColorPreset.SelectedItem, null)
                        + Environment.NewLine
                        + Config.Locale.Get(Config.L_GUI_MAIN + "KbdColorPresetDelConfirm"),
                    Config.Locale.Get(Config.L_GUI_MAIN + "KbdColorPresetDel"),
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Asterisk,
                    MessageBoxDefaultButton.Button2) == DialogResult.Yes)

                Config.ColorPreset.Remove((string) this.CmbKbdColorPreset.SelectedValue);

            // Update the user interface
            Context.Menu.Create();
            this.CmbKbdColorPreset.DataSource = null;
            UpdateKbd();

            // Save the configuration
            Config.Save();

        }

        // Saves a color preset
        private void EventActionColorPresetSet(object sender, EventArgs e) {
            string name;

            // Ask for a name
            if((name = Gui.ShowPromptInputText(
                Config.Locale.Get(Config.L_GUI_MAIN + Gui.G_KBD + "ColorPresetAdd"),
                Config.Locale.Get(Config.L_GUI_MAIN + Gui.G_KBD + "ColorPresetAddValueDefault"),
                this)) != "")

                // Save the preset (possibly overwriting it)
                Config.ColorPreset[name] = new BiosData.ColorTable(Kbd.GetColors(), true);

            // Update the user interface
            Context.Menu.Create();
            this.CmbKbdColorPreset.DataSource = null;
            UpdateKbd();

            // Save the configuration
            Config.Save();

        }

        // Handles the fan settings button being clicked
        private void EventActionFanSet(object sender, EventArgs e) {

            // Query fan state
            bool isFanMax = Context.Op.Platform.Fans.GetMax();
            bool isFanOff = Context.Op.Platform.Fans.GetOff();

            // Enable fan program
            if(this.RdoFanProg.Checked) {

                if(this.CmbFanProg.SelectedValue != null
                    && (string) this.CmbFanProg.SelectedValue != ""
                    && Config.FanProgram.ContainsKey(
                        (string) this.CmbFanProg.SelectedValue)) {

                    if(isFanOff) // Re-enable fan if off first
                        Context.Op.Platform.Fans.SetOff(false);

                    if(isFanMax) // Disable maximum speed first
                        Context.Op.Platform.Fans.SetMax(false);

                    // Start the selected program
                    Context.Op.Program.Run((string) this.CmbFanProg.SelectedValue);

                    // Reset the program update counter
                    Context.UpdateProgramTick = 1;

                } else {

                    MessageBox.Show(
                        this, // Modal
                        Config.Locale.Get(Config.L_GUI_MAIN + Gui.G_FAN + "ProgSetNoSel"),
                        Config.Locale.Get(Config.L_GUI_MAIN + Gui.G_FAN + "ProgSet"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);

                }

            // Switch off the fan
            } else if(this.RdoFanOff.Checked) {

                // Terminate any running fan program
                Context.Op.Program.Terminate();

                if(!isFanOff) { // Skip if already off

                    if(isFanMax) // Disable maximum speed first
                        Context.Op.Platform.Fans.SetMax(false);

                    // Switch off the fan
                    Context.Op.Platform.Fans.SetOff(true);

                }

            // Set fan to maximum speed
            } else if(this.RdoFanMax.Checked) {

                // Terminate any running fan program
                Context.Op.Program.Terminate();

                if(!isFanMax) { // Skip if already maximum speed

                     if(isFanOff) // Re-enable fan if off first
                         Context.Op.Platform.Fans.SetOff(false);

                     // Set the fan to maximum speed
                     Context.Op.Platform.Fans.SetMax(true);

                }

            // Enable fan constant speed
            } else if(this.RdoFanConst.Checked) {

                // Terminate any running fan program
                Context.Op.Program.Terminate();

                // The BIOS mandates that at least one fan
                // be left running at any given time, so if the user
                // wants both of them off, we use another way to do so
                if(this.TrkFan0Lvl.Value == this.TrkFan0Lvl.Minimum
                    && this.TrkFan1Lvl.Value == this.TrkFan1Lvl.Minimum) {

                    // Switch the fans off
                    if(!isFanOff) // If not already off
                        Context.Op.Platform.Fans.SetOff(true);

                // Conversely, if the user wants maximum speed setting
                // for both fans, we just set it explicitly instead
                } else if(this.TrkFan0Lvl.Value == this.TrkFan0Lvl.Maximum
                    && this.TrkFan1Lvl.Value == this.TrkFan1Lvl.Maximum) {

                    // Set the fans to maximum speed
                    if(!isFanMax) // If not already at maximum speed
                        Context.Op.Platform.Fans.SetMax(true);

                // Otherwise, we just set the speed levels normally
                } else {

                    if(isFanMax) // Disable maximum speed first
                        Context.Op.Platform.Fans.SetMax(false);
		    
                    if(isFanOff) // Re-enable fan if off first
                        Context.Op.Platform.Fans.SetOff(false);

                    // Set each fan to the user-selected level
                    // or to zero, if the minimum value is selected
                    Context.Op.Platform.Fans.SetLevels(new byte[] {
                        this.TrkFan0Lvl.Value == this.TrkFan0Lvl.Minimum ? (byte) 0 : (byte) this.TrkFan0Lvl.Value,
                        this.TrkFan1Lvl.Value == this.TrkFan1Lvl.Minimum ? (byte) 0 : (byte) this.TrkFan1Lvl.Value});
                    // Note: this won't work for setting both fans to zero
                    // but that case will have already been handled at this point

                    // Reset the fan mode so that the settings are applied
                    Context.Op.Platform.Fans.SetMode(
                        Context.Op.Platform.Fans.GetMode());

                }

            // Enable automatic fan mode (default)
            } else if(this.RdoFanAuto.Checked) {

                // Terminate any running fan program
                Context.Op.Program.Terminate();

                // Query the current and requested mode
                BiosData.FanMode fanModeNow = Context.Op.Platform.Fans.GetMode();
                BiosData.FanMode fanModeAsk = (BiosData.FanMode) Enum.Parse(
                    typeof(BiosData.FanMode), 
                    (string) this.CmbFanMode.SelectedValue);

                if(isFanOff) // Re-enable fan if off first
                    Context.Op.Platform.Fans.SetOff(false);

                if(isFanMax) // Disable maximum speed first
                    Context.Op.Platform.Fans.SetMax(false);

                // Set the levels to 0xFF to clear any custom speed settings
                Context.Op.Platform.Fans.SetLevels(new byte[] {Byte.MaxValue, Byte.MaxValue});

                // Enable automatic fan in the selected mode
                Context.Op.Platform.Fans.SetMode(fanModeAsk);

            }

            // Restore the default button look
            this.BtnFanSet.Checked = false;

        }

        // Handles the help button being clicked
        private void EventActionHelp(object sender, EventArgs e) {

            // Show the About dialog
            GuiOp.About();

            // Cancel the help cursor
            ((System.ComponentModel.CancelEventArgs) e).Cancel = true;
 
        }
#endregion

#region Events
        // Handles the event of a color parameter being input into the text box
        private void EventColorInput(object sender, EventArgs e) {
            try {
                Kbd.SetColors(new BiosData.ColorTable(this.TxtKbdColorVal.Text));
                this.TxtKbdColorVal.ForeColor = Color.Empty;
            } catch {
                this.TxtKbdColorVal.ForeColor = Color.Red;
            }
        }

        // Handles the event of a color zone being clicked
        private void EventColorPick(object sender, MouseEventArgs e) {

            // No action if backlight off or no support
            if(Kbd == null || !Kbd.GetBacklight())
                return;

            // Determine the clicked zone from co-ordinates
            // while also setting the dialog title in one go
            this.ColorPicker.Title = Config.Locale.Get(Config.L_GUI_MAIN + "KbdColorPick" + Kbd.SetZone(e.X, e.Y).ToString());
            
            // Set the start color to the current color
            this.ColorPicker.Color = Color.FromArgb(Kbd.GetColor());

            // Update the current backlight color in the custom colors
            this.ColorPicker.CustomColors = Kbd.UpdateColorPicker(this.ColorPicker.CustomColors);

            // Show the dialog
            this.ColorPicker.ShowDialog();

            // Note: Color is updated in real time,
            // so there is nothing more to check here

        }

        // Handles the event of a color preset being selected from a drop-down list
        private void EventColorPreset(object sender, EventArgs e) {

            // Apply the new color preset
            Context.FormMain.Kbd.SetColors(
                Config.ColorPreset[(string) ((ComboBox) sender).SelectedValue]);

            // Update the parameter textbox
            this.TxtKbdColorVal.Text = Kbd.GetParam();

        }

        // Handles the event when the form is about to be closed
        private void EventFormClosing(object sender, FormClosingEventArgs e) {

            // If the reason is user request
            if (e.CloseReason == CloseReason.UserClosing) {

                // Cancel the application closure
                // and hide the window instead
                e.Cancel = true;
                this.Hide();

            }

        }

        // Handles the event when the fan mode list has been interacted with
        private void EventFanModeChanged(object sender, EventArgs e) {

            // Highlight the Set button to remind
            // the user of a pending unapplied change
            this.BtnFanSet.Checked = true;

            // Switch the selected radio button to Auto
            this.RdoFanAuto.Checked = true;

        }

        // Handles the event when the fan program list has been interacted with
        private void EventFanProgramChanged(object sender, EventArgs e) {

            // Highlight the Set button to remind
            // the user of a pending unapplied change
            this.BtnFanSet.Checked = true;

            // Switch the selected radio button to Program
            this.RdoFanProg.Checked = true;

        }

        // Handles the event when the fan radio button has been selected or deselected
        private void EventFanRdoChanged(object sender, EventArgs e) {

            if(((Control) sender).Name ==  Gui.T_RDO + Gui.G_FAN + "Const")

                // If the radio button is set to constant speed,
                // unlock the trackbars so that the speed can be set
                if(((RadioButton) sender).Checked) {
                    this.TrkFan0Lvl.Enabled = true;
                    this.TrkFan1Lvl.Enabled = true;
	        
                // The trackbars are locked in any other situation
                } else {
                    this.TrkFan0Lvl.Enabled = false;
                    this.TrkFan1Lvl.Enabled = false;
                }

            // Highlight the Set button to remind
            // the user of a pending unapplied change
            this.BtnFanSet.Checked = true;

        }

        // Handles the event when the fan radio button has been selected or deselected
        private void EventFanTrkChanged(object sender, EventArgs e) {

            // Only if the trackbar is enabled
            // for constant fan speed mode setting
            if(((Control) sender).Enabled)
                this.BtnFanSet.Checked = true;

        }

        // Handles the event when the form visibility changes
        private void EventFormVisibleChanged(object sender, EventArgs e) {

            // Reset the update counter,
            // the form is always updated immediately as it opens
            Context.UpdateMonitorTick = 0;

            // Update everything
            if(this.Visible)
                UpdateAll();

        }
#endregion

#region Setup
        // Set up the fan control combo boxes
        public void SetupFanCtl() {

            // Trackbars are disabled by default, until
            // constant-speed mode is explicitly enabled
            this.TrkFan0Lvl.Enabled = false;
            this.TrkFan1Lvl.Enabled = false;

            // Clear the fan mode list
            this.CmbFanMode.BeginUpdate();
            this.CmbFanMode.DataSource = null;
            FanModes.Clear();

            // Populate the fan mode list
            // The most useful modes are on top,
            // the rest (legacy modes) is sorted alphabetically
            List<string> fanModes = Config.FanModesSticky;
            string[] fanModesMore = Enum.GetNames(typeof(BiosData.FanMode));
            Array.Sort(fanModesMore);
            fanModes.AddRange(fanModesMore);
            foreach(string name in new HashSet<string>(fanModes))
                FanModes.Add(new {
                    Text = Config.Locale.Get(Config.L_GUI_MENU + Gui.M_ACT + Gui.G_FAN + "Mode" + name),
                    Value = name });
            this.CmbFanMode.DataSource = FanModes;
            this.CmbFanMode.DisplayMember = "Text";
            this.CmbFanMode.ValueMember = "Value";
            this.CmbFanMode.EndUpdate();

            // Clear the fan mode list
            this.CmbFanProg.BeginUpdate();
            this.CmbFanProg.DataSource = null;
            FanPrograms.Clear();

            // Populate the fan program list
            foreach(string name in Config.FanProgram.Keys)
                FanPrograms.Add( new { Text = name, Value = name } );
            if(FanPrograms.Count > 0)
               this.CmbFanProg.DataSource = FanPrograms;
            this.CmbFanProg.DisplayMember = "Text";
            this.CmbFanProg.ValueMember = "Value";
            this.CmbFanProg.EndUpdate();

        }

        // Set up the system information group
        public void SetupSys() {

            // Set both system information and status to empty
            this.SysInfo = "";
            this.SysStatus = "";

            // Apply the update
            this.UpdateSysRtf();

        }

        // Set up the temperature readout description (only has to be done once)
        public void SetupTmp() {

            // Iterate through all temperature sensor and initialize each
            for(int i = 0; i < Context.Op.Platform.Temperature.Length; i++)
                SetupTmpItem(i);

        }

        // Set up the description of an item within the temperature group
        public void SetupTmpItem(int index) {

            // Retrieve caption candidates
            string indexString = index.ToString();
            string captionOriginal = Context.Op.Platform.Temperature[index].GetName();
            string captionLocaleId = Config.L_GUI_MAIN + Gui.G_TMP + captionOriginal;
            string captionLocalized = Config.Locale.Get(captionLocaleId);

            // Locate the pertinent caption label
            Label label = ((Label) this.GrpTmp.Controls.Find(
                Gui.T_LBL + Gui.G_TMP + indexString + Gui.S_CAP, false)[0]);

            // Also locate the value label
            Label labelValue =
                ((Label) this.GrpTmp.Controls[this.GrpTmp.Controls.IndexOf(label) + 1]);

            // Strike-through sensors set not to be used
            if(!Context.Op.Platform.TemperatureUse[index])
                label.Font = new Font(label.Font, FontStyle.Strikeout);

            // Determine the best caption and apply it
            label.Text = captionLocalized == captionLocaleId ?
                captionOriginal : captionLocalized;

            // Check if a specific localized tooltip is available,
            // and set it; otherwise use the default fallback tooltip
            string toolTipLocaleId = Config.L_GUI_TIP + Gui.G_TMP + captionOriginal;
            string toolTipLocalized = Config.Locale.Get(toolTipLocaleId);
            string toolTip = toolTipLocalized != toolTipLocaleId ?
                toolTipLocalized : Config.Locale.Get(Config.L_GUI_TIP + Gui.G_TMP + "Unknown");

            this.Tip.SetToolTip(label, toolTip);
            this.Tip.SetToolTip(labelValue, toolTip);

        }
#endregion

#region Updates
        // Updates all of the form
        public void UpdateAll() {

            // Update form dimensions following a scaling change
            UpdateDpi(this.LastDpi);

            // Update the fan group monitoring section
            UpdateFan();

            // Update the fan group controls section
            UpdateFanCtl();

            // Update the keyboard group
            UpdateKbd();

            // Update the system status group
            UpdateSys();

            // Update the temperature group
            UpdateTmp();

        }

        // Updates the form dimensions following a scaling change
        private void UpdateDpi(int dpi) {

            // Skip if configured not to resize
            if(Config.GuiDpiChangeResize) {

                // Suspend the layout
                this.SuspendLayout();
                this.AutoSize = false;

                // Adjust the client size to account for differences
                // if the form gets scaled dynamically to another DPI setting
                this.Size = new Size(
                    (int) (1080 + ((dpi - 96) * Config.DpiSizeAdjFactorX / 100)),
                    (int) (441 + ((dpi - 96) * Config.DpiSizeAdjFactorY / 100)));

                // Resume the layout
                this.AutoSize = true;
                this.ResumeLayout(false);

                // Update the last DPI value for the next rescaling
                this.LastDpi = dpi;

            }

        }

        // Updates the fan group monitoring section
        public void UpdateFan() {

            // Update the platform fan readings
            Context.Op.Platform.UpdateFans();

            // Update the fan speed [rpm]
            try {
                this.LblFan0Val.Text = Context.Op.Platform.Fans.Fan[0].GetSpeed().ToString(Config.FormatFanSpeed);
                this.LblFan1Val.Text = Context.Op.Platform.Fans.Fan[1].GetSpeed().ToString(Config.FormatFanSpeed);
            } catch { }

            // Update the fan level [krpm]
            // Hold if the controls are not unlocked for the user to set the speed manually
            try {
                if(!this.TrkFan0Lvl.Enabled)
                    this.TrkFan0Lvl.Value = Conv.GetConstrained(
                        Context.Op.Platform.Fans.Fan[0].GetLevel(), this.TrkFan0Lvl.Minimum, this.TrkFan1Lvl.Maximum);
                if(!this.TrkFan1Lvl.Enabled)
                    this.TrkFan1Lvl.Value = Conv.GetConstrained(
                        Context.Op.Platform.Fans.Fan[1].GetLevel(), this.TrkFan1Lvl.Minimum, this.TrkFan1Lvl.Maximum);
            } catch { }

            // Update the fan rate [%]
            try {
                this.BarFan0Rte.Value = Context.Op.Platform.Fans.Fan[0].GetRate();
                this.BarFan1Rte.Value = Context.Op.Platform.Fans.Fan[1].GetRate();
                this.LblFan0Rte.Text = this.BarFan0Rte.Value.ToString();
                this.LblFan1Rte.Text = this.BarFan1Rte.Value.ToString();
            } catch { }

            // Show the countdown, if applicable
            int countdown = Context.Op.Platform.Fans.GetCountdown();
            this.LblFanCountdown.Text = countdown > 0 ?
                countdown.ToString() + Config.Locale.Get(Config.L_UNIT + "TimeSecond" + Config.LS_CUSTOM_FONT) : "";

            // In constant-speed mode, keep resetting the countdown, while also reapplying the current mode
            if(this.RdoFanConst.Checked == true && countdown < Config.UpdateMonitorInterval + Config.FanCountdownExtendThreshold) {
                Context.Op.Platform.Fans.SetMode(Context.Op.Platform.Fans.GetMode());
                Context.Op.Platform.Fans.SetCountdown(Config.FanCountdownExtendInterval);
            }

            // Update the current fan mode
            // Hold if the Set button is already highlighted or the list is currently open
            if(!this.BtnFanSet.Checked && !this.CmbFanMode.DroppedDown)
            try {
                this.CmbFanMode.SelectedValue = Enum.GetName(typeof(BiosData.FanMode), Context.Op.Platform.Fans.GetMode());
            } catch { }

            // Update the current fan program
            // Hold if the Set button is already highlighted or the list is currently open
            if(!this.BtnFanSet.Checked && !this.CmbFanProg.DroppedDown)
            try {
                this.CmbFanProg.SelectedValue = Context.Op.Program.GetName();
            } catch { }


        }

        // Updates the fan group controls section
        public void UpdateFanCtl() {

            // Query and retrieve fan control state
            bool isFanMax = Context.Op.Platform.Fans.GetMax();
            bool isFanOff = Context.Op.Platform.Fans.GetOff();

            // Fan program is active if a flag to that effect is set
            // This takes precedence over all the other queries
            if(Context.Op.Program.IsEnabled)
                this.RdoFanProg.Checked = true;

            // If the fan is switched off, the setting should reflect that
            else if(isFanOff)
                this.RdoFanOff.Checked = true;

            // If the fan is set to maximum mode, the setting should reflect that
            else if(isFanMax)
                this.RdoFanMax.Checked = true;

            // If trackbars are unlocked, we are in constant fan speed mode
            else if(this.TrkFan0Lvl.Enabled)
                this.RdoFanConst.Checked = true;

            // If none of the above, the fan is in the default automatic state
            else
                this.RdoFanAuto.Checked = true;

            // Restore the Set button default look
            this.BtnFanSet.Checked = false;

        }

        // Updates the keyboard group
        public void UpdateKbd() {

            // Restore the default color of the color as parameter text box
            this.TxtKbdColorVal.ForeColor = Color.Empty;

            // Disable the backlight toggle for unsupported devices,
            // otherwise update the keyboard backlight status
            if(!Context.Op.Platform.System.GetKbdBacklightSupport()) {
                this.ChkKbdBacklight.Checked = false;
                this.ChkKbdBacklight.Enabled = false;
            } else if(Kbd != null) // Use the keyboard class
                this.ChkKbdBacklight.Checked = Kbd.GetBacklight();
            else // Fallback case for no customizable backlight color
                this.ChkKbdBacklight.Checked =
                    Context.Op.Platform.System.GetKbdBacklight() == BiosData.Backlight.On ? true : false;

            // Disable the interface when backlight is off or no support
            if(Kbd == null || !Kbd.GetBacklight()) {

                this.CmbKbdColorPreset.DataSource = null;
                this.CmbKbdColorPreset.Enabled = false;
                this.TxtKbdColorVal.Enabled = false;
                this.TxtKbdColorVal.Text = "";
                this.BtnKbdColorPresetDel.Enabled = false;
                this.BtnKbdColorPresetSet.Enabled = false;
                this.PicKbd.Cursor = Cursors.Default;

            } else {

                // Enable the interface when backlight is on
                this.CmbKbdColorPreset.BeginUpdate();
                ColorPresets.Clear();
                foreach(string name in Config.ColorPreset.Keys)
                    ColorPresets.Add( new { Text = name.StartsWith(Config.ColorPresetDefaultPrefix) ?
                        Config.Locale.Get(Config.L_GUI_MENU + Gui.M_ACT + Gui.G_KBD + "ColorPreset" + name) : name, Value = name } );
                this.CmbKbdColorPreset.DataSource = ColorPresets;
                this.CmbKbdColorPreset.DisplayMember = "Text";
                this.CmbKbdColorPreset.ValueMember = "Value";
                this.CmbKbdColorPreset.SelectedValue = Kbd.GetPreset();
                this.CmbKbdColorPreset.Enabled = true;
                this.CmbKbdColorPreset.EndUpdate();
                this.TxtKbdColorVal.Enabled = true;
                this.TxtKbdColorVal.Text = Kbd.GetParam();
                this.BtnKbdColorPresetDel.Enabled = true;
                this.BtnKbdColorPresetSet.Enabled = true;
                this.PicKbd.Cursor = Cursors.Hand;

            }

        }

        // Keeps updating the color as it changes in the Color Picker dialog
        public void UpdateKbdCallback(int color) {
            Kbd.SetColor(ColorTranslator.FromWin32(color).ToArgb());
            this.TxtKbdColorVal.Text = Kbd.GetParam();
            this.CmbKbdColorPreset.SelectedValue = Kbd.GetPreset();
        }

        // Update the system information while preserving the status message
        public void UpdateSys() {

            // Update the platform system information
            Context.Op.Platform.UpdateSystem();

            // Update the system info string
            this.SysInfo = ""
                + Conv.RTF_CF6 + Context.Op.Platform.System.GetManufacturer() + " "
                + Conv.RTF_CF5 + Context.Op.Platform.System.GetProduct() + " "
                + Conv.RTF_CF1 + Context.Op.Platform.System.GetVersion() + " "
                + Config.Locale.Get(Config.L_GUI_MAIN + Gui.G_SYS + "Born") + " "
                    + Context.Op.Platform.System.GetBornDate() + " "
                + (Context.Op.Platform.System.GetDefaultCpuPowerLimit4() == 0 ? ""
                    : Conv.RTF_CF5 + Context.Op.Platform.System.GetDefaultCpuPowerLimit4().ToString()
                    + Conv.RTF_CF1 + Config.Locale.Get(Config.L_UNIT + "Power") + " ")
                + Conv.RTF_CF1 + (Context.Op.Platform.System.IsFullPower() ?
                    Config.Locale.Get(Config.L_GUI_MAIN + Gui.G_SYS + "Adapter"
                        + Enum.GetName(typeof(BiosData.AdapterStatus),
                            Context.Op.Platform.System.GetAdapterStatus()))
                    : Config.Locale.Get(Config.L_GUI_MAIN + Gui.G_SYS + "AdapterBatteryPower"))
                    + Conv.RTF_LINE
                + Conv.RTF_CF1 + Config.Locale.Get(Config.L_GUI_MAIN + Gui.G_SYS + "Gpu") + " " 
                    + Conv.RTF_CF5 + Enum.GetName(typeof(BiosData.GpuMode), Context.Op.Platform.System.GetGpuMode(true)) + " "
                + Conv.RTF_CF1 + Config.Locale.Get(Config.L_GUI_MAIN + Gui.G_SYS + "GpuDState") + " "
                    + Conv.RTF_CF5 + Enum.GetName(typeof(BiosData.GpuDState), Context.Op.Platform.System.GetGpuDState(true)) + " "
                + (Context.Op.Platform.System.GetGpuCustomTgp() == BiosData.GpuCustomTgp.On ?
                    Conv.RTF_CF6 + Config.Locale.Get(Config.L_GUI_MAIN + Gui.G_SYS + "GpuCustomTgp")
                    : Conv.RTF_CF1 + Conv.RTF_STRIKE1 + Config.Locale.Get(Config.L_GUI_MAIN + Gui.G_SYS + "GpuCustomTgp") + Conv.RTF_STRIKE0) + " "
                + (Context.Op.Platform.System.GetGpuPpab() == BiosData.GpuPpab.On ?
                    Conv.RTF_CF6 + Config.Locale.Get(Config.L_GUI_MAIN + Gui.G_SYS + "GpuPpab")
                    : Conv.RTF_CF1 + Conv.RTF_STRIKE1 + Config.Locale.Get(Config.L_GUI_MAIN + Gui.G_SYS + "GpuPpab") + Conv.RTF_STRIKE0) + " "
                + Conv.RTF_CF1 + Config.Locale.Get(
                    Config.L_GUI_MAIN + Gui.G_SYS + "Throttling"
                        + Enum.GetName(typeof(BiosData.Throttling),
                    Context.Op.Platform.System.GetThrottling())) + Conv.RTF_LINE
                + Conv.RTF_CF2;

            // Apply the update
            UpdateSysRtf();

        }

        // Update the system status message only
        public void UpdateSysMsg(string message = "") {

            // Add timestamp if the message is not empty
            if(message != "")
                message = message + Conv.RTF_CF1 + " @ " + DateTime.Now.ToString(Config.TimestampFormat);

            // Set the status message
            this.SysStatus = message;

            // Apply the update
            UpdateSysRtf();

        }

        // Update the system status rich-text field
        private void UpdateSysRtf() {
            this.RtfSysInfo.Rtf =
                Config.SysInfoRtfHeader
                + Conv.GetUnicodeStringRtf(this.SysInfo)
                + Conv.GetUnicodeStringRtf(this.SysStatus)
                + Config.SysInfoRtfFooter;
        }

        // Updates the temperature group
        public void UpdateTmp() {

            // Update the temperature readings
            Context.Op.Platform.UpdateTemperature();

            // Update the form data representation
            for(int i = 0; i < Context.Op.Platform.Temperature.Length; i++)
                UpdateTmpItem(i);

        }

        // Updates an item within the temperature group
        private void UpdateTmpItem(int index) {

            // Prepare the data
            string prefix = Gui.T_LBL + Gui.G_TMP + index.ToString();
            int value = Context.Op.Platform.Temperature[index].GetValue();
            PlatformData.ValueTrend valueTrend = Context.Op.Platform.Temperature[index].GetValueTrend();

            // Locate the pertinent labels
            Label labelCaption = ((Label) this.GrpTmp.Controls.Find(prefix + Gui.S_CAP, false)[0]);
            Label labelValue = ((Label) this.GrpTmp.Controls[this.GrpTmp.Controls.IndexOf(labelCaption) + 1]);

            // Update the status
            labelCaption.Enabled = value > 0;

            // Update the value
            labelValue.Text = value == 0 ? "" :
                value.ToString() + Config.Locale.Get(Config.L_UNIT + "Temperature" + Config.LS_CUSTOM_FONT)
                + (valueTrend == PlatformData.ValueTrend.Unchanged ?
                    Conv.GetChar(Conv.SpecialChar.SpaceEn) : valueTrend == PlatformData.ValueTrend.Ascending ?
                        Conv.GetChar(Conv.SpecialChar.SupPlus) : Conv.GetChar(Conv.SpecialChar.SupMinus));

        }
#endregion
 
    }

}
