  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023-2024 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using System.Windows.Forms;
using Microsoft.Win32;
using OmenMon.Hardware.Bios;
using OmenMon.Hardware.Ec;
using OmenMon.Library;

namespace OmenMon.AppGui {

    // Implements an application context running in the background
    // with an icon in the notification (system tray) area
    public class GuiTray : ApplicationContext {

#region Data
        // Stores the first instance's context,
        // so that it can be accessed from elsewhere
        internal static GuiTray Context { get; private set; }

        // Stores the component container for later disposal
        private System.ComponentModel.IContainer Components;

        // Stores the message filter so that it can be removed upon exit
        private IMessageFilter Filter;

        // Stores the main GUI form
        internal GuiFormMain FormMain;

        // Stores the class managing the dynamic notification icon
        internal GuiIcon Icon;

        // Stores the menu
        internal GuiMenu Menu;

        // Stores the notification area icon class
        internal NotifyIcon Notification;

        // Stores the operation-running class
        internal GuiOp Op;

        // Stores the system timer
        private System.Windows.Forms.Timer TrayUpdateTimer;
        private System.Windows.Forms.Timer HeartbeatTimer;

        // Stores the number of ticks elapsed since
        // the last update of a particular category
        internal int UpdateIconTick;
        internal int UpdateMonitorTick;
        internal int UpdateProgramTick;
#endregion

#region Construction & Disposal
        // Constructs the tray notification application context
        public GuiTray () {

            // Retain the context for future use
            if(Context == null)
                Context = this;

            // Initialize the component model container
            this.Components = new System.ComponentModel.Container();

            // Create the notification icon
            this.Notification = new NotifyIcon(this.Components) {
                ContextMenuStrip = new ContextMenuStrip(Components),
                Icon = OmenMon.Resources.IconTray,
                Text = Config.AppName + " " + Config.AppVersion,
                Visible = true
            };

            // Initialize the operation-running class
            this.Op = new GuiOp(Context);

            // Initialize the icon management class
            this.Icon = new GuiIcon(Context);
            Update();

            // Initialize the menu class
            this.Menu = new GuiMenu(Context);

            // Define event handlers
            this.Notification.ContextMenuStrip.Closing += Menu.EventClosing;
            this.Notification.ContextMenuStrip.ItemClicked += Menu.EventItemClicked;
            this.Notification.ContextMenuStrip.Opening += Menu.EventOpening;
            this.Notification.MouseClick += EventIconMouseClick;

            // Add a filter to intercept any custom messages
            this.Filter = new GuiFilter(Context);
            Application.AddMessageFilter(this.Filter);

            // Receive suspend and resume event notifications
            // only if configured to suspend and resume fan program
            if(Config.FanProgramSuspend)
                Gui.RegisterSuspendResumeNotification(this.Op.SuspendResumeCallback);

            // Set up the timer
            this.TrayUpdateTimer = new System.Windows.Forms.Timer(Components);
            this.TrayUpdateTimer.Interval = Config.GuiTimerInterval;
            this.TrayUpdateTimer.Tick += EventTrayUpdateTimerTick;
            this.TrayUpdateTimer.Enabled = true;
            
            this.HeartbeatTimer = new System.Windows.Forms.Timer(Components);
            this.HeartbeatTimer.Interval = Config.HeartbeatTimerInterval;
            this.HeartbeatTimer.Tick += HardwareHeartbeatTick;
            this.HeartbeatTimer.Enabled = true;

            // Show the main form if requested by the environment variable
            if(Environment.GetEnvironmentVariable(Config.EnvVarSelfName) != null
                && Environment.GetEnvironmentVariable(Config.EnvVarSelfName).Contains(Config.EnvVarSelfValueKey))

                this.Op.KeyHandler(Gui.MessageParam.NoLastParam);

            // Unset the environment variable, so that
            // it does not propagate to child processes
            Environment.SetEnvironmentVariable(Config.EnvVarSelfName, null);

            // Automatically apply settings, if enabled
            if(Config.AutoConfig)
                this.Op.AutoConfigRun();

            // Register the power-mode change event handler
            SystemEvents.PowerModeChanged += EventPowerChange;

        }

        // Handles component disposal
        protected override void Dispose(bool isDisposing) {

            if(isDisposing && this.Components != null)
                this.Components.Dispose();

            // Perform the usual tasks
            base.Dispose(isDisposing);

        }

        // Handles exit tasks
        protected override void ExitThreadCore() {

            // The icon has to be removed beforehand,
            // otherwise it will linger in the tray
            if(this.Notification != null)
                this.Notification.Visible = false;

            // Remove the message filter
            Application.RemoveMessageFilter(this.Filter);
            this.Filter = null;

            // Unregister the power-mode change event handler
            SystemEvents.PowerModeChanged -= EventPowerChange;

            // Stop receiving power event notifications
            Gui.UnregisterSuspendResumeNotification();

            // Terminate the fan program, if any
            if(this.Op.Program.IsEnabled)
                this.Op.Program.Terminate();

            // Perform the usual tasks
            base.ExitThreadCore();

        }
#endregion

#region Event Handlers
        // Handles a click event on the notification icon
        private void EventIconMouseClick(object sender, MouseEventArgs e) {

            // Toggle the main GUI form on left click
            // Note: right click is reserved for the context menu
            if(e.Button == MouseButtons.Left)
                ToggleFormMain();

        }

        // Handles a power-mode change event
        private void EventPowerChange(object sender, PowerModeChangedEventArgs e) {

            // Only respond to status change events,
            // which excludes Resume and Suspend
            if(e.Mode == PowerModes.StatusChange)
                this.Op.PowerChange();

        }

        // Handles a timer tick
        private void EventTrayUpdateTimerTick(object sender, EventArgs e)
        {
            // Perform the updates as scheduled
            Update();
        }

        private void HardwareHeartbeatTick(object sender, EventArgs e)
        {
            // send a heartbeat to the bios for new devices
            BiosCtl.Instance.GetFanCount();
        }
#endregion

#region Visual Methods
        // Brings the already-running application instance to the user's attention
        public void BringFocus() {

            // Show a balloon notification
            ShowBalloonTip(Config.Locale.Get(Config.L_GUI + "AlreadyRunning"));

            // Show the main GUI form
            ShowFormMain();

        }

        // Sets the notification icon tooltip text
        public void SetNotifyText(string text = "") {

            // Use reflection to bypass the 64-character limit
            Os.SetNotifyIconText(Context.Notification, text);

        }

        // Shows a balloon tip above the notification area icon
        public void ShowBalloonTip(string message, string title = null, ToolTipIcon icon = ToolTipIcon.None) {

            // Show the notification only if the duration is not set to 0
            if(Config.GuiTipDuration > 0) {

                // Populate the data from the parameters
                this.Notification.BalloonTipIcon = icon;
                this.Notification.BalloonTipText = message;
                this.Notification.BalloonTipClicked += Menu.EventActionShowFormMain;

                // Also change the title if passed as a parameter
                if(title != null)
                    this.Notification.BalloonTipTitle = title;

                // Show the tip for a specified duration
                this.Notification.ShowBalloonTip(Config.GuiTipDuration);

            }

        }

        // Shows the main GUI form
        public void ShowFormMain() {

            // Set up the form first if it hasn't been created yet
            if(this.FormMain == null)
                this.FormMain = new GuiFormMain();

            // Show the form if not visible
            if(!this.FormMain.Visible) {
                Gui.ShowToFront(this.FormMain.Handle);
                this.FormMain.Show();
                }

            // Briefly set to show in front of everything
            this.FormMain.TopMost = true;

            // Activate it
            this.FormMain.Activate();

            // Note: all this is in order to bring the application into focus
            // even if started from a background process (the Task Scheduler)

            // Reset the top-most state, unless set to remain on top
            this.FormMain.TopMost = Config.GuiStayOnTop;

        }

        // Toggles the main GUI form
        public void ToggleFormMain() {

                // Show the form if it's not visible already
                if(this.FormMain == null || !this.FormMain.Visible)
                    ShowFormMain();

                // Hide the form if it's visible
                else
                    this.FormMain.Hide();

        }
#endregion

        // Performs update operations as scheduled
        // This method is called periodically by a timer event
        public void Update() {

            // Reset the tick counters
            if(this.UpdateIconTick >= Config.UpdateIconInterval)
                this.UpdateIconTick = 0;
            if(this.UpdateMonitorTick >= Config.UpdateMonitorInterval)
                this.UpdateMonitorTick = 0;
            if(this.UpdateProgramTick >= Config.UpdateProgramInterval)
                this.UpdateProgramTick = 0;

            // Update the fan program or extend the countdown
            if(this.UpdateProgramTick++ == 0) {

                // Update the program, if active
                if(this.Op.Program.IsEnabled)
                    this.Op.Program.Update();

                // Alternatively, update any non-zero countdown
                // depending on the configuration settings
                else if(Config.FanCountdownExtendAlways)
                    this.Op.Program.UpdateCountdown(false, true);

            }

            // Update the main form, only if visible
            if(this.FormMain != null && this.FormMain.Visible && this.UpdateMonitorTick++ == 0) {
                this.FormMain.UpdateFan();
                this.FormMain.UpdateSys();
                this.FormMain.UpdateTmp();
            }

            // Update the notification icon, if dynamic
            if(this.Icon.IsDynamic && this.UpdateIconTick++ == 0) {

                // Update the background depending on the fan mode
                this.Icon.SetBackground(
                    this.Op.Platform.Fans.GetMode() == BiosData.FanMode.Performance ?
                        GuiIcon.BackgroundType.Warm : GuiIcon.BackgroundType.Cool);

                // Update the icon text with the temperature
                this.Icon.Update(
                    Conv.GetString(
                        this.Op.Platform.GetMaxTemperature(
                            // Only force sensor update if neither the main form
                            // nor the currently-running fan program did so
                            (this.FormMain == null || !this.FormMain.Visible)
                            && (!this.Op.Program.IsEnabled || this.UpdateProgramTick != 1)),
                        2, 10)
                    + Config.Locale.Get(
                        Config.L_UNIT + "Temperature" + Config.LS_CUSTOM_FONT));

            }

        }

    }

}
