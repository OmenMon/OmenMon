  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OmenMon.Library;

namespace OmenMon.AppGui {

    // The About dialog GUI form
    public class GuiFormAbout : Form {

#region Form Components
        // Form components
        private ButtonEx BtnAccept;          // Button to close the dialog
        private Label LblAppName;            // Application name label
        private Label LblAppVersion;         // Application version label
        private LinkLabel LnkAppLink;        // Homepage link label
        private PictureBox PicLogo;          // Logo picture
        private RichTextBox RtfAppInfo;      // More information rich-text field
        private TableLayoutPanel TblLayout;  // Table layout
#endregion

        // Stores the component container
        private System.ComponentModel.IContainer Components;

#region Construction & Disposal
        // Constructs the form
        public GuiFormAbout(string title = "", string text = "") {

            // Initialize the component model container
            this.Components = new System.ComponentModel.Container();

            // Initialize the form components
            Initialize();

            this.RtfAppInfo.Rtf = text != "" ? text : Config.Locale.Get(Config.L_GUI_ABOUT + "Text");
            this.Text = title != "" ? title : Config.Locale.Get(Config.L_GUI_ABOUT + "Title");

        }

        // Handles component disposal
        protected override void Dispose(bool isDisposing) {
	
            if(isDisposing && Components != null)
                Components.Dispose();
	
            // Perform the usual tasks
            base.Dispose(isDisposing);
	
        }

        // Makes the Esc key close the form
        protected override bool ProcessDialogKey(Keys keyData) {
            if(Form.ModifierKeys == Keys.None && keyData == Keys.Escape) {
                this.Close();
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }
#endregion

#region Initialization
        // Creates the components and applies their initial settings values
        private void Initialize() {

#region Form Component Instantiation
            // Instantiate form components
            this.BtnAccept = new ButtonEx();
            this.LblAppName = new Label();
            this.LblAppVersion = new Label();
            this.LnkAppLink = new LinkLabel();
            this.PicLogo = new PictureBox();
            this.RtfAppInfo = new RichTextBox();
            this.TblLayout = new TableLayoutPanel();
#endregion

#region Suspend Layout
            // Suspend the layout before applying the settings
            this.TblLayout.SuspendLayout();
            this.SuspendLayout();

            // Initialize the components that specifically require it
            ((System.ComponentModel.ISupportInitialize) this.PicLogo).BeginInit();
#endregion

#region Component Setup
            // Table layout
            this.TblLayout.ColumnCount = 2;
            this.TblLayout.ColumnStyles.Add(new ColumnStyle());
            this.TblLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 35F));
            this.TblLayout.Dock = DockStyle.Fill;
            this.TblLayout.Location = new Point(6, 6);
            this.TblLayout.Name = Gui.T_TBL + "Layout";
            this.TblLayout.RowCount = 5;
            this.TblLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 164F));
            this.TblLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 14F));
            this.TblLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 14F));
            this.TblLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            this.TblLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 27F));
            this.TblLayout.Size = new Size(372, 269);
            this.TblLayout.TabIndex = 1;

            // Table layout components
            this.TblLayout.Controls.Add(this.BtnAccept, 1, 4);
            this.TblLayout.Controls.Add(this.LblAppName, 0, 1);
            this.TblLayout.Controls.Add(this.LblAppVersion, 0, 2);
            this.TblLayout.Controls.Add(this.LnkAppLink, 0, 4);
            this.TblLayout.Controls.Add(this.PicLogo, 0, 0);
            this.TblLayout.Controls.Add(this.RtfAppInfo, 0, 3);

            // Logo picture
            this.PicLogo.Dock = DockStyle.Fill;
            this.PicLogo.Image = OmenMon.Resources.Logo;
            this.PicLogo.Location = new Point(3, 3);
            this.PicLogo.Name = Gui.T_PIC + "Logo";
            this.PicLogo.Size = new Size(366, 158);
            this.PicLogo.SizeMode = PictureBoxSizeMode.Zoom;
            this.PicLogo.TabIndex = 2;
            this.PicLogo.TabStop = false;
            this.TblLayout.SetColumnSpan(this.PicLogo, 2);

            // Application name label
            this.LblAppName.Dock = DockStyle.Fill;
            this.LblAppName.Location = new Point(3, 164);
            this.LblAppName.Name = Gui.T_LBL + "AppName";
            this.LblAppName.Size = new Size(366, 14);
            this.LblAppName.TabIndex = 3;
            this.LblAppName.TextAlign = ContentAlignment.MiddleCenter;
            this.TblLayout.SetColumnSpan(this.LblAppName, 2);

            // Application version label
            this.TblLayout.SetColumnSpan(this.LblAppVersion, 2);
            this.LblAppVersion.Dock = DockStyle.Fill;
            this.LblAppVersion.Location = new Point(3, 178);
            this.LblAppVersion.Name = Gui.T_LBL + "AppVersion";
            this.LblAppVersion.Size = new Size(366, 14);
            this.LblAppVersion.TabIndex = 4;
            this.LblAppVersion.TextAlign = ContentAlignment.MiddleCenter;

            // More information rich-text field
            this.RtfAppInfo.BackColor = SystemColors.Control;
            this.RtfAppInfo.BorderStyle = BorderStyle.None;
            this.TblLayout.SetColumnSpan(this.RtfAppInfo, 2);
            this.RtfAppInfo.DetectUrls = false;
            this.RtfAppInfo.Dock = DockStyle.Fill;
            this.RtfAppInfo.Enabled = false;
            this.RtfAppInfo.Location = new Point(30, 195);
            this.RtfAppInfo.Margin = new Padding(30, 3, 30, 3);
            this.RtfAppInfo.Name = Gui.T_RTF + "AppInfo";
            this.RtfAppInfo.ReadOnly = true;
            this.RtfAppInfo.ScrollBars = RichTextBoxScrollBars.None;
            this.RtfAppInfo.Size = new Size(312, 44);
            this.RtfAppInfo.TabIndex = 5;
            this.RtfAppInfo.TabStop = false;

            // Homepage link label
            this.LnkAppLink.Dock = DockStyle.Fill;
            this.LnkAppLink.LinkBehavior = LinkBehavior.HoverUnderline;
            this.LnkAppLink.Location = new Point(3, 242);
            this.LnkAppLink.Name = Gui.T_LNK + "AppLink";
            this.LnkAppLink.Size = new Size(331, 27);
            this.LnkAppLink.TabIndex = 6;
            this.LnkAppLink.TabStop = false;
            this.LnkAppLink.TextAlign = ContentAlignment.MiddleRight;

            // Button to close the dialog
            this.BtnAccept.Anchor = ((AnchorStyles) (AnchorStyles.Bottom | AnchorStyles.Right));
            this.BtnAccept.Checked = true;
            this.BtnAccept.HighlightColorDark = Color.FromArgb(Config.GuiColorCoolDark);
            this.BtnAccept.HighlightColorLight = Color.FromArgb(Config.GuiColorCoolLite);
            this.BtnAccept.HighlightGradientMode = LinearGradientMode.ForwardDiagonal;
            this.BtnAccept.HighlightRadius = 2;
            this.BtnAccept.HighlightWidth = 5;
            this.BtnAccept.Location = new Point(344, 245);
            this.BtnAccept.Name = Gui.T_BTN + "Accept";
            this.BtnAccept.Size = new Size(25, 21);
            this.BtnAccept.TabIndex = 0;
            this.BtnAccept.Text = Config.Locale.Get(Config.L_GUI + Gui.T_BTN + "Set");
#endregion

#region Form Setup
            // Main form components
            this.Controls.Add(this.TblLayout);

            // Main form settings
            this.AcceptButton = this.BtnAccept;
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(384, 281);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = Gui.T_FRM + "About";
            this.Padding = new Padding(6);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterScreen;
#endregion

#region Resume Layout
            // End the initialization of components that specifically require it
            ((System.ComponentModel.ISupportInitialize) this.PicLogo).EndInit();

            // Resume the layout
            this.TblLayout.ResumeLayout(false);
            this.ResumeLayout(false);
#endregion

#region Events
            this.BtnAccept.Click += EventActionClose;
            this.LnkAppLink.LinkClicked += EventActionHomepage;
#endregion

#region Dynamic Settings
            this.LblAppName.Text = Config.AppName + " " + Config.AppVersion;
            this.LblAppVersion.Text = Config.Locale.Get(Config.L_GUI_ABOUT + "Caption").Replace("&", "&&");
            this.LnkAppLink.Text = Config.AppHomepageLink.Substring(((string) "https://").Length);
#endregion

        }
#endregion

#region Event Handlers
        // Handles a click on the only button
        private void EventActionClose(object sender, EventArgs e) {
            this.Close();
        }

        // Handles a click on the homepage link
        private void EventActionHomepage(object sender, LinkLabelLinkClickedEventArgs e) {
            try {
                Process.Start(
                    new ProcessStartInfo {
                        FileName = Config.AppHomepageLink,
                        UseShellExecute = true });
            } catch { }
        }
#endregion

    }

}
