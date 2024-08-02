  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using OmenMon.Library;

namespace OmenMon.AppGui {

    // The main GUI form
    public partial class GuiFormMain : Form {

#region Form Components
        // Form components
        private ButtonEx BtnFanSet;           // Button to apply fan settings
        private Button BtnKbdColorPresetDel;  // Button to delete color preset
        private Button BtnKbdColorPresetSet;  // Button to save color preset
        private CheckBox ChkKbdBacklight;     // Checkbox to toggle backlight
        private ComboBox CmbFanMode;          // Fan mode (built-in) list
        private ComboBox CmbFanProg;          // Fan program (custom) list
        private ComboBox CmbKbdColorPreset;   // Color preset list
        private GroupBox GrpFan;              // Fan group box
        private GroupBox GrpKbd;              // Keyboard group box
        private GroupBox GrpSys;              // System group box
        private GroupBox GrpTmp;              // Temperature group box
        private Label LblFan0Cap;             // Fan #0 caption ("CPU")
        private Label LblFan0Rte;             // Fan #0 rate [%]
        private Label LblFan0Val;             // Fan #0 value [rpm]
        private Label LblFan1Cap;             // Fan #1 caption ("GPU")
        private Label LblFan1Rte;             // Fan #1 rate [%]
        private Label LblFan1Val;             // Fan #1 value [rpm]
        private Label LblFanCountdown;        // Custom settings fan countdown
        private Label LblFanUnitRte;          // Fan rate unit ("%")
        private Label LblFanUnitVal;          // Fan value unit ("rpm")
        private Label LblTmp0Cap;             // Temperature sensor #0 caption
        private Label LblTmp0Val;             // Temperature sensor #0 value [°C]
        private Label LblTmp1Cap;             // Temperature sensor #1 caption
        private Label LblTmp1Val;             // Temperature sensor #1 value [°C]
        private Label LblTmp2Cap;             // Temperature sensor #2 caption
        private Label LblTmp2Val;             // Temperature sensor #2 value [°C]
        private Label LblTmp3Cap;             // Temperature sensor #3 caption
        private Label LblTmp3Val;             // Temperature sensor #3 value [°C]
        private Label LblTmp4Cap;             // Temperature sensor #4 caption
        private Label LblTmp4Val;             // Temperature sensor #4 value [°C]
        private Label LblTmp5Cap;             // Temperature sensor #5 caption
        private Label LblTmp5Val;             // Temperature sensor #5 value [°C]
        private Label LblTmp6Cap;             // Temperature sensor #6 caption
        private Label LblTmp6Val;             // Temperature sensor #6 value [°C]
        private Label LblTmp7Cap;             // Temperature sensor #7 caption
        private Label LblTmp7Val;             // Temperature sensor #7 value [°C]
        private Label LblTmp8Cap;             // Temperature sensor #8 caption
        private Label LblTmp8Val;             // Temperature sensor #8 value [°C]
        internal PictureBox PicKbd;           // Keyboard graphics for color preset demonstration
        private ProgressBarEx BarFan0Rte;     // Fan #0 rate bar [%]
        private ProgressBarEx BarFan1Rte;     // Fan #1 rate bar [%]
        private RadioButton RdoFanAuto;       // Fan auto setting radio button
        private RadioButton RdoFanConst;      // Fan constant setting radio button
        private RadioButton RdoFanMax;        // Fan maximum setting radio button
        private RadioButton RdoFanOff;        // Fan off setting radio button
        private RadioButton RdoFanProg;       // Fan program setting radio button
        private RichTextBox RtfSysInfo;       // System status information text
        private TextBox TxtKbdColorVal;       // Keyboard color definition text input/output field
        private ToolTip Tip;                  // Shows pop-up explanations when hovering over items
        private TrackBar TrkFan0Lvl;        // Fan #0 level [krpm]
        private TrackBar TrkFan1Lvl;        // Fan #1 level [krpm]
#endregion

#region Initialization
        // Creates the components and applies their initial settings values
        private void Initialize() {

            // Add a handler to run when the form is about to be closed
            this.FormClosing += Config.GuiCloseWindowExit ?
                Context.Menu.EventActionExit : new FormClosingEventHandler(EventFormClosing); 

            // Add a handler when the form visibility changes
            this.VisibleChanged += EventFormVisibleChanged;

            // Add a handler to handle the help button being clicked
            this.HelpButtonClicked += EventActionHelp;

            // Prepare the custom font object so that it can be referenced later
            this.FigureFont = new Font(
                GdiFont.Get(0),
                Config.GuiFigureFontSize,
                FontStyle.Regular,
                GraphicsUnit.Pixel);

#region Form Component Instantiation
            // Instantiate form components
            this.BarFan0Rte = new ProgressBarEx();
            this.BarFan1Rte = new ProgressBarEx();
            this.BtnFanSet = new ButtonEx();
            this.BtnKbdColorPresetDel = new Button();
            this.BtnKbdColorPresetSet = new Button();
            this.ChkKbdBacklight = new CheckBox();
            this.CmbFanMode = new ComboBox();
            this.CmbFanProg = new ComboBox();
            this.CmbKbdColorPreset = new ComboBox();
            this.GrpFan = new GroupBox();
            this.GrpKbd = new GroupBox();
            this.GrpSys = new GroupBox();
            this.GrpTmp = new GroupBox();
            this.LblFan0Cap = new Label();
            this.LblFan0Rte = new Label();
            this.LblFan0Val = new Label();
            this.LblFan1Cap = new Label();
            this.LblFan1Rte = new Label();
            this.LblFan1Val = new Label();
            this.LblFanCountdown = new Label();
            this.LblFanUnitRte = new Label();
            this.LblFanUnitVal = new Label();
            this.LblTmp0Cap = new Label();
            this.LblTmp0Val = new Label();
            this.LblTmp1Cap = new Label();
            this.LblTmp1Val = new Label();
            this.LblTmp2Cap = new Label();
            this.LblTmp2Val = new Label();
            this.LblTmp3Cap = new Label();
            this.LblTmp3Val = new Label();
            this.LblTmp4Cap = new Label();
            this.LblTmp4Val = new Label();
            this.LblTmp5Cap = new Label();
            this.LblTmp5Val = new Label();
            this.LblTmp6Cap = new Label();
            this.LblTmp6Val = new Label();
            this.LblTmp7Cap = new Label();
            this.LblTmp7Val = new Label();
            this.LblTmp8Cap = new Label();
            this.LblTmp8Val = new Label();
            this.PicKbd = new PictureBox();
            this.RdoFanAuto = new RadioButton();
            this.RdoFanConst = new RadioButton();
            this.RdoFanMax = new RadioButton();
            this.RdoFanOff = new RadioButton();
            this.RdoFanProg = new RadioButton();
            this.RtfSysInfo = new RichTextBox();
            this.Tip = new ToolTip(this.Components);
            this.TrkFan0Lvl = new TrackBar();
            this.TrkFan1Lvl = new TrackBar();
            this.TxtKbdColorVal = new TextBox();
#endregion

#region Suspend Layout
            // Suspend the layout before applying the settings
            this.GrpFan.SuspendLayout();
            this.GrpKbd.SuspendLayout();
            this.GrpSys.SuspendLayout();
            this.GrpTmp.SuspendLayout();
            this.SuspendLayout();

            // Initialize the components that specifically require it
            ((System.ComponentModel.ISupportInitialize) this.PicKbd).BeginInit();
            ((System.ComponentModel.ISupportInitialize) this.TrkFan0Lvl).BeginInit();
            ((System.ComponentModel.ISupportInitialize) this.TrkFan1Lvl).BeginInit();
#endregion

#region Fan Group
#region Fan Group - Monitor
            // Fan #0 caption ("CPU")
            this.LblFan0Cap.Location = new Point(6, 16);
            this.LblFan0Cap.Name = Gui.T_LBL + Gui.G_FAN + "0" + Gui.S_CAP;
            this.LblFan0Cap.Size = new Size(34, 14);
            this.LblFan0Cap.TabIndex = 0;
            this.LblFan0Cap.Text = Config.Locale.Get(Config.L_GUI_MAIN + "Fan0");
            this.LblFan0Cap.TextAlign = ContentAlignment.TopCenter;

            // Fan #1 caption ("GPU")
            this.LblFan1Cap.Location = new Point(247, 16);
            this.LblFan1Cap.Name = Gui.T_LBL + Gui.G_FAN + "1" + Gui.S_CAP;
            this.LblFan1Cap.Size = new Size(34, 14);
            this.LblFan1Cap.TabIndex = 1;
            this.LblFan1Cap.Text = Config.Locale.Get(Config.L_GUI_MAIN + "Fan1");
            this.LblFan1Cap.TextAlign = ContentAlignment.TopCenter;

            // Fan value unit ("rpm")
            this.LblFanUnitVal.Font = this.FigureFont;
            this.LblFanUnitVal.Location = new Point(121, 16);
            this.LblFanUnitVal.Name = Gui.T_LBL + Gui.G_FAN + Gui.X_UNIT + Gui.S_VAL;
            this.LblFanUnitVal.Size = new Size(44, 27);
            this.LblFanUnitVal.TabIndex = 2;
            this.LblFanUnitVal.Text = Config.Locale.Get(Config.L_UNIT + "RotationRate" + Config.LS_CUSTOM_FONT);
            this.LblFanUnitVal.TextAlign = ContentAlignment.MiddleCenter;

            // Fan #0 value [rpm]
            this.LblFan0Val.Font = this.FigureFont;
            this.LblFan0Val.Location = new Point(40, 16);
            this.LblFan0Val.Name = Gui.T_LBL + Gui.G_FAN + "0" + Gui.S_VAL;
            this.LblFan0Val.Size = new Size(87, 27);
            this.LblFan0Val.TabIndex = 3;
            this.LblFan0Val.TextAlign = ContentAlignment.MiddleCenter;

            // Fan #1 value [rpm]
            this.LblFan1Val.Font = this.FigureFont;
            this.LblFan1Val.Location = new Point(177, 16);
            this.LblFan1Val.Name = Gui.T_LBL + Gui.G_FAN + "1" + Gui.S_CAP;
            this.LblFan1Val.Size = new Size(66, 27);
            this.LblFan1Val.TabIndex = 4;
            this.LblFan1Val.TextAlign = ContentAlignment.MiddleCenter;

            // Fan rate numerical value unit ("%")
            this.LblFanUnitRte.Font = this.FigureFont;
            this.LblFanUnitRte.Location = new Point(121, 43);
            this.LblFanUnitRte.Name = Gui.T_LBL + Gui.G_FAN + Gui.X_UNIT + Gui.S_RTE;
            this.LblFanUnitRte.Size = new Size(44, 27);
            this.LblFanUnitRte.TabIndex = 5;
            this.LblFanUnitRte.Text = Config.Locale.Get(Config.L_UNIT + "Percent");
            this.LblFanUnitRte.TextAlign = ContentAlignment.MiddleCenter;

            // Fan #0 rate numerical value [%]
            this.LblFan0Rte.Font = this.FigureFont;
            this.LblFan0Rte.Location = new Point(40, 43);
            this.LblFan0Rte.Name = Gui.T_LBL + Gui.G_FAN + "0" + Gui.S_RTE;
            this.LblFan0Rte.Size = new Size(87, 27);
            this.LblFan0Rte.TabIndex = 6;
            this.LblFan0Rte.TextAlign = ContentAlignment.MiddleCenter;

            // Fan #0 rate bar [%]
            this.BarFan0Rte.BackColor = Color.FromArgb(Config.GuiColorCoolLite);
            this.BarFan0Rte.ForeColor = Color.FromArgb(Config.GuiColorWarmLite);
            this.BarFan0Rte.LinearGradientMode = LinearGradientMode.ForwardDiagonal;
            this.BarFan0Rte.Location = new Point(40, 75);
            this.BarFan0Rte.Name = Gui.T_BAR + Gui.G_FAN + "0" + Gui.S_RTE;
            this.BarFan0Rte.Size = new Size(202, 10);
            this.BarFan0Rte.Style = ProgressBarStyle.Continuous;
            this.BarFan0Rte.TabIndex = 7;

            // Fan #1 rate numerical value [%]
            this.LblFan1Rte.Font = this.FigureFont;
            this.LblFan1Rte.Location = new Point(177, 43);
            this.LblFan1Rte.Name = Gui.T_LBL + Gui.G_FAN + "1" + Gui.S_RTE;
            this.LblFan1Rte.Size = new Size(66, 27);
            this.LblFan1Rte.TabIndex = 8;
            this.LblFan1Rte.TextAlign = ContentAlignment.MiddleCenter;

            // Fan #1 rate bar [%]
            this.BarFan1Rte.BackColor = Color.FromArgb(Config.GuiColorCoolDark);
            this.BarFan1Rte.ForeColor = Color.FromArgb(Config.GuiColorWarmDark);
            this.BarFan1Rte.LinearGradientMode = LinearGradientMode.ForwardDiagonal;
            this.BarFan1Rte.Location = new Point(40, 90);
            this.BarFan1Rte.Name = Gui.T_BAR + Gui.G_FAN + "1" + Gui.S_RTE;
            this.BarFan1Rte.RightToLeft = RightToLeft.Yes;
            this.BarFan1Rte.RightToLeftLayout = true;
            this.BarFan1Rte.Size = new Size(202, 10);
            this.BarFan1Rte.Style = ProgressBarStyle.Continuous;
            this.BarFan1Rte.TabIndex = 9;

            // Fan #0 level (user-controllable) [krpm]
            this.TrkFan0Lvl.AutoSize = false;
            this.TrkFan0Lvl.Cursor = Cursors.SizeNS;
            this.TrkFan0Lvl.Location = new Point(6, 30);
            this.TrkFan0Lvl.Maximum = Config.FanLevelMax;
            this.TrkFan0Lvl.Minimum = Config.FanLevelMin;
            this.TrkFan0Lvl.Name = Gui.T_TRK + Gui.G_FAN + "0" + Gui.S_LVL;
            this.TrkFan0Lvl.Orientation = Orientation.Vertical;
            this.TrkFan0Lvl.Size = new Size(34, 150);
            this.TrkFan0Lvl.TabIndex = 10;
            this.TrkFan0Lvl.TabStop = false;
            this.TrkFan0Lvl.TickFrequency = 5;

            // Fan #1 level (user-controllable) [krpm]
            this.TrkFan1Lvl.AutoSize = false;
            this.TrkFan1Lvl.Cursor = Cursors.SizeNS;
            this.TrkFan1Lvl.Location = new Point(247, 30);
            this.TrkFan1Lvl.Maximum = Config.FanLevelMax;
            this.TrkFan1Lvl.Minimum = Config.FanLevelMin;
            this.TrkFan1Lvl.Name = Gui.T_TRK + Gui.G_FAN + "0" + Gui.S_LVL;
            this.TrkFan1Lvl.Orientation = Orientation.Vertical;
            this.TrkFan1Lvl.Size = new Size(34, 150);
            this.TrkFan1Lvl.TabIndex = 11;
            this.TrkFan1Lvl.TabStop = false;
            this.TrkFan1Lvl.TickFrequency = 5;
            this.TrkFan1Lvl.TickStyle = TickStyle.TopLeft;

            // Custom settings fan countdown
            this.LblFanCountdown.Font = this.FigureFont;
            this.LblFanCountdown.Location = new Point(177, 101);
            this.LblFanCountdown.Name = Gui.T_LBL + Gui.G_FAN + "Countdown";
            this.LblFanCountdown.Size = new Size(66, 27);
            this.LblFanCountdown.TabIndex = 12;
            this.LblFanCountdown.TextAlign = ContentAlignment.MiddleRight;
#endregion
#region Fan Group - Control
            // Fan program setting radio button
            this.RdoFanProg.Location = new Point(38, 107);
            this.RdoFanProg.Name = Gui.T_RDO + Gui.G_FAN + "Prog";
            this.RdoFanProg.Size = new Size(55, 21);
            this.RdoFanProg.TabIndex = 13;
            this.RdoFanProg.Text = Config.Locale.Get(Config.L_GUI_MAIN + Gui.G_FAN + "Prog");
            this.RdoFanProg.UseVisualStyleBackColor = true;

            // Fan program (custom) list
            this.CmbFanProg.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CmbFanProg.FormattingEnabled = true;
            this.CmbFanProg.ItemHeight = 13;
            this.CmbFanProg.Location = new Point(95, 107);
            this.CmbFanProg.Name = Gui.T_CMB + Gui.G_FAN + "Prog";
            this.CmbFanProg.Size = new Size(88, 21);
            this.CmbFanProg.TabIndex = 14;

            // Fan auto setting radio button
            this.RdoFanAuto.Location = new Point(38, 130);
            this.RdoFanAuto.Name = "RdoFanAuto";
            this.RdoFanAuto.Size = new Size(55, 21);
            this.RdoFanAuto.TabIndex = 15;
            this.RdoFanAuto.Text = Config.Locale.Get(Config.L_GUI_MAIN + Gui.G_FAN + "Auto");
            this.RdoFanAuto.UseVisualStyleBackColor = true;

            // Fan mode (built-in) list
            this.CmbFanMode.DropDownStyle = ComboBoxStyle.DropDownList;
            this.CmbFanMode.FormattingEnabled = true;
            this.CmbFanMode.ItemHeight = 13;
            this.CmbFanMode.Location = new Point(95, 130);
            this.CmbFanMode.Name = Gui.T_CMB + Gui.G_FAN + "Mode";
            this.CmbFanMode.Size = new Size(148, 21);
            this.CmbFanMode.TabIndex = 16;

            // Fan maximum setting radio button
            this.RdoFanMax.Location = new Point(38, 153);
            this.RdoFanMax.Name = Gui.T_RDO + Gui.G_FAN + "Max";
            this.RdoFanMax.Size = new Size(55, 21);
            this.RdoFanMax.TabIndex = 17;
            this.RdoFanMax.Text = Config.Locale.Get(Config.L_GUI_MAIN + Gui.G_FAN + "Max");
            this.RdoFanMax.UseVisualStyleBackColor = true;

            // Fan constant setting radio button
            this.RdoFanConst.Location = new Point(94, 153);
            this.RdoFanConst.Name = Gui.T_RDO + Gui.G_FAN + "Const";
            this.RdoFanConst.Size = new Size(70, 21);
            this.RdoFanConst.TabIndex = 18;
            this.RdoFanConst.Text = Config.Locale.Get(Config.L_GUI_MAIN + Gui.G_FAN + "Const");
            this.RdoFanConst.TextAlign = ContentAlignment.MiddleCenter;
            this.RdoFanConst.UseVisualStyleBackColor = true;

            // Fan off setting radio button
            this.RdoFanOff.Location = new Point(165, 153);
            this.RdoFanOff.Name = Gui.T_RDO + Gui.G_FAN + "Off";
            this.RdoFanOff.Size = new Size(50, 21);
            this.RdoFanOff.TabIndex = 19;
            this.RdoFanOff.Text = Config.Locale.Get(Config.L_GUI_MAIN + Gui.G_FAN + "Off");
            this.RdoFanOff.TextAlign = ContentAlignment.MiddleCenter;
            this.RdoFanOff.UseVisualStyleBackColor = true;

            // Button to apply fan settings
            this.BtnFanSet.HighlightColorDark = Color.FromArgb(Config.GuiColorWarmDark);
            this.BtnFanSet.HighlightColorLight = Color.FromArgb(Config.GuiColorWarmLite);
            this.BtnFanSet.HighlightGradientMode = LinearGradientMode.BackwardDiagonal;
            this.BtnFanSet.HighlightRadius = 2;
            this.BtnFanSet.HighlightWidth = 5;
            this.BtnFanSet.Location = new Point(218, 153);
            this.BtnFanSet.Name = Gui.T_BTN + Gui.G_FAN + "Set";
            this.BtnFanSet.Size = new Size(25, 21);
            this.BtnFanSet.TabIndex = 20;
            this.BtnFanSet.Text = Config.Locale.Get(Config.L_GUI + Gui.T_BTN + "Set");
#endregion

            // Fan group components
            this.GrpFan.Controls.Add(this.BarFan0Rte);
            this.GrpFan.Controls.Add(this.BarFan1Rte);
            this.GrpFan.Controls.Add(this.BtnFanSet);
            this.GrpFan.Controls.Add(this.CmbFanMode);
            this.GrpFan.Controls.Add(this.CmbFanProg);
            this.GrpFan.Controls.Add(this.LblFan0Cap);
            this.GrpFan.Controls.Add(this.LblFan0Rte);
            this.GrpFan.Controls.Add(this.LblFan0Val);
            this.GrpFan.Controls.Add(this.LblFan1Cap);
            this.GrpFan.Controls.Add(this.LblFan1Rte);
            this.GrpFan.Controls.Add(this.LblFan1Val);
            this.GrpFan.Controls.Add(this.LblFanCountdown);
            this.GrpFan.Controls.Add(this.LblFanUnitRte);
            this.GrpFan.Controls.Add(this.LblFanUnitVal);
            this.GrpFan.Controls.Add(this.RdoFanAuto);
            this.GrpFan.Controls.Add(this.RdoFanConst);
            this.GrpFan.Controls.Add(this.RdoFanMax);
            this.GrpFan.Controls.Add(this.RdoFanOff);
            this.GrpFan.Controls.Add(this.RdoFanProg);
            this.GrpFan.Controls.Add(this.TrkFan0Lvl);
            this.GrpFan.Controls.Add(this.TrkFan1Lvl);

            // Fan group settings
            this.GrpFan.Location = new Point(416, 68);
            this.GrpFan.Name = Gui.T_GRP + Gui.G_FAN;
            this.GrpFan.Size = new Size(287, 185);
            this.GrpFan.TabIndex = 3;
            this.GrpFan.TabStop = false;
            this.GrpFan.Text = Config.Locale.Get(Config.L_GUI_MAIN + Gui.G_FAN).Replace("&", "&&");
#endregion

#region Keyboard Group
            // Checkbox to toggle backlight
            this.ChkKbdBacklight.AutoCheck = false;
            this.ChkKbdBacklight.Location = new Point(6, 17);
            this.ChkKbdBacklight.Name = Gui.T_CHK + Gui.G_KBD + "Backlight";
            this.ChkKbdBacklight.Size = new Size(17, 21);
            this.ChkKbdBacklight.TabIndex = 0;
            this.ChkKbdBacklight.UseVisualStyleBackColor = true;

            // Color preset list
            this.CmbKbdColorPreset.DropDownStyle = ComboBoxStyle.DropDownList;
            this.CmbKbdColorPreset.FormattingEnabled = true;
            this.CmbKbdColorPreset.ItemHeight = 13;
            this.CmbKbdColorPreset.Location = new Point(26, 17);
            this.CmbKbdColorPreset.Name = Gui.T_CMB + Gui.G_KBD + "ColorPreset";
            this.CmbKbdColorPreset.Size = new Size(140, 21);
            this.CmbKbdColorPreset.TabIndex = 1;

            // Keyboard color definition text input/output field
            this.TxtKbdColorVal.CharacterCasing = CharacterCasing.Upper;
            this.TxtKbdColorVal.Location = new Point(170, 18);
            this.TxtKbdColorVal.MaxLength = 27;
            this.TxtKbdColorVal.Name = Gui.T_TXT + Gui.G_KBD + "Color" + Gui.S_VAL;
            this.TxtKbdColorVal.Size = new Size(175, 20);
            this.TxtKbdColorVal.TabIndex = 2;
            this.TxtKbdColorVal.TextAlign = HorizontalAlignment.Center;

            // Button to delete the color preset
            this.BtnKbdColorPresetDel.Location = new Point(348, 16);
            this.BtnKbdColorPresetDel.Name = Gui.T_BTN + Gui.G_KBD + "ColorPresetDel";
            this.BtnKbdColorPresetDel.Size = new Size(25, 21);
            this.BtnKbdColorPresetDel.TabIndex = 3;
            this.BtnKbdColorPresetDel.Text = Config.Locale.Get(Config.L_GUI + "BtnDel");
            this.BtnKbdColorPresetDel.UseVisualStyleBackColor = true;

            // Button to save the current colors as a preset
            this.BtnKbdColorPresetSet.Location = new Point(375, 16);
            this.BtnKbdColorPresetSet.Name = Gui.T_BTN + Gui.G_KBD + "ColorPresetSet";
            this.BtnKbdColorPresetSet.Size = new Size(25, 21);
            this.BtnKbdColorPresetSet.TabIndex = 4;
            this.BtnKbdColorPresetSet.Text = Config.Locale.Get(Config.L_GUI + "BtnSet");
            this.BtnKbdColorPresetSet.UseVisualStyleBackColor = true;

            // Keyboard graphics line art for color preset demo
            this.PicKbd.Location = new Point(6, 43);
            this.PicKbd.Name = Gui.T_PIC + Gui.G_KBD;
            this.PicKbd.Size = new Size(393, 130);
            this.PicKbd.SizeMode = PictureBoxSizeMode.Zoom;
            this.PicKbd.TabIndex = 5;
            this.PicKbd.TabStop = false;

            // Keyboard group controls
            this.GrpKbd.Controls.Add(this.BtnKbdColorPresetDel);
            this.GrpKbd.Controls.Add(this.BtnKbdColorPresetSet);
            this.GrpKbd.Controls.Add(this.ChkKbdBacklight);
            this.GrpKbd.Controls.Add(this.CmbKbdColorPreset);
            this.GrpKbd.Controls.Add(this.PicKbd);
            this.GrpKbd.Controls.Add(this.TxtKbdColorVal);

            // Keyboard group settings
            this.GrpKbd.Location = new Point(6, 68);
            this.GrpKbd.Name = Gui.T_GRP + Gui.G_KBD;
            this.GrpKbd.Size = new Size(405, 185);
            this.GrpKbd.TabIndex = 2;
            this.GrpKbd.TabStop = false;
            this.GrpKbd.Text = Config.Locale.Get(Config.L_GUI_MAIN + Gui.G_KBD).Replace("&", "&&");
#endregion

#region System Status Group
            // System status information text
            this.RtfSysInfo.BackColor = SystemColors.Control;
            this.RtfSysInfo.BorderStyle = BorderStyle.None;
            this.RtfSysInfo.Cursor = Cursors.Arrow;
            this.RtfSysInfo.DetectUrls = false;
            this.RtfSysInfo.Enabled = false;
            this.RtfSysInfo.Location = new Point(6, 16);
            this.RtfSysInfo.Name = Gui.T_RTF + Gui.G_SYS + "Info";
            this.RtfSysInfo.ReadOnly = true;
            this.RtfSysInfo.ScrollBars = RichTextBoxScrollBars.None;
            this.RtfSysInfo.ShortcutsEnabled = false;
            this.RtfSysInfo.Size = new Size(277, 50);
            this.RtfSysInfo.TabIndex = 0;
            this.RtfSysInfo.TabStop = false;
            this.RtfSysInfo.WordWrap = false;

            // Override default font selection if set
            if(Config.GuiSysInfoFontSize > 0)
                this.RtfSysInfo.Font = new Font(
                    Gui.DIALOG_FONT, Config.GuiSysInfoFontSize, FontStyle.Regular, GraphicsUnit.Pixel);

            // System status group components
            this.GrpSys.Controls.Add(this.RtfSysInfo);

            // System status group settings
            this.GrpSys.Location = new Point(6, 3);
            this.GrpSys.Name = Gui.T_GRP + Gui.G_SYS;
            this.GrpSys.Size = new Size(287, 65);
            this.GrpSys.TabIndex = 0;
            this.GrpSys.TabStop = false;
            this.GrpSys.Text = Config.Locale.Get(Config.L_GUI_MAIN + Gui.G_SYS).Replace("&", "&&");
#endregion

#region Temperature Group
            // Temperature sensor #0 caption
            this.LblTmp0Cap.Location = new Point(6, 16);
            this.LblTmp0Cap.Name = Gui.T_LBL + Gui.G_TMP + "0" + Gui.S_CAP;
            this.LblTmp0Cap.Size = new Size(44, 14);
            this.LblTmp0Cap.TabIndex = 0;
            this.LblTmp0Cap.TextAlign = ContentAlignment.TopCenter;

            // Temperature sensor #0 value [°C]
            this.LblTmp0Val.Font = this.FigureFont;
            this.LblTmp0Val.Location = new Point(6, 30);
            this.LblTmp0Val.Name = Gui.T_LBL + Gui.G_TMP + "0" + Gui.S_VAL;
            this.LblTmp0Val.Size = new Size(44, 27);
            this.LblTmp0Val.TabIndex = 1;
            this.LblTmp0Val.TextAlign = ContentAlignment.MiddleCenter;

            // Temperature sensor #1 caption
            this.LblTmp1Cap.Location = new Point(50, 16);
            this.LblTmp1Cap.Name = Gui.T_LBL + Gui.G_TMP + "1" + Gui.S_CAP;
            this.LblTmp1Cap.Size = new Size(44, 14);
            this.LblTmp1Cap.TabIndex = 2;
            this.LblTmp1Cap.TextAlign = ContentAlignment.TopCenter;

            // Temperature sensor #1 value [°C]
            this.LblTmp1Val.Font = this.FigureFont;
            this.LblTmp1Val.Location = new Point(50, 30);
            this.LblTmp1Val.Name = Gui.T_LBL + Gui.G_TMP + "1" + Gui.S_VAL;
            this.LblTmp1Val.Size = new Size(44, 27);
            this.LblTmp1Val.TabIndex = 3;
            this.LblTmp1Val.TextAlign = ContentAlignment.MiddleCenter;

            // Temperature sensor #2 caption
            this.LblTmp2Cap.Location = new Point(94, 16);
            this.LblTmp2Cap.Name = Gui.T_LBL + Gui.G_TMP + "2" + Gui.S_CAP;
            this.LblTmp2Cap.Size = new Size(44, 14);
            this.LblTmp2Cap.TabIndex = 4;
            this.LblTmp2Cap.TextAlign = ContentAlignment.TopCenter;

            // Temperature sensor #2 value [°C]
            this.LblTmp2Val.Font = this.FigureFont;
            this.LblTmp2Val.Location = new Point(94, 30);
            this.LblTmp2Val.Name = Gui.T_LBL + Gui.G_TMP + "2" + Gui.S_VAL;
            this.LblTmp2Val.Size = new Size(44, 27);
            this.LblTmp2Val.TabIndex = 5;
            this.LblTmp2Val.TextAlign = ContentAlignment.MiddleCenter;

            // Temperature sensor #3 caption
            this.LblTmp3Cap.Location = new Point(138, 16);
            this.LblTmp3Cap.Name = Gui.T_LBL + Gui.G_TMP + "3" + Gui.S_CAP;
            this.LblTmp3Cap.Size = new Size(44, 14);
            this.LblTmp3Cap.TabIndex = 6;
            this.LblTmp3Cap.TextAlign = ContentAlignment.TopCenter;

            // Temperature sensor #3 value [°C]
            this.LblTmp3Val.Font = this.FigureFont;
            this.LblTmp3Val.Location = new Point(138, 30);
            this.LblTmp3Val.Name = Gui.T_LBL + Gui.G_TMP + "3" + Gui.S_VAL;
            this.LblTmp3Val.Size = new Size(44, 27);
            this.LblTmp3Val.TabIndex = 7;
            this.LblTmp3Val.TextAlign = ContentAlignment.MiddleCenter;

            // Temperature sensor #4 caption
            this.LblTmp4Cap.Location = new Point(182, 16);
            this.LblTmp4Cap.Name = Gui.T_LBL + Gui.G_TMP + "4" + Gui.S_CAP;
            this.LblTmp4Cap.Size = new Size(44, 14);
            this.LblTmp4Cap.TabIndex = 8;
            this.LblTmp4Cap.TextAlign = ContentAlignment.TopCenter;

            // Temperature sensor #4 value [°C]
            this.LblTmp4Val.Font = this.FigureFont;
            this.LblTmp4Val.Location = new Point(182, 30);
            this.LblTmp4Val.Name = Gui.T_LBL + Gui.G_TMP + "4" + Gui.S_VAL;
            this.LblTmp4Val.Size = new Size(44, 27);
            this.LblTmp4Val.TabIndex = 9;
            this.LblTmp4Val.TextAlign = ContentAlignment.MiddleCenter;

            // Temperature sensor #5 caption
            this.LblTmp5Cap.Location = new Point(226, 16);
            this.LblTmp5Cap.Name = Gui.T_LBL + Gui.G_TMP + "5" + Gui.S_CAP;
            this.LblTmp5Cap.Size = new Size(44, 14);
            this.LblTmp5Cap.TabIndex = 10;
            this.LblTmp5Cap.TextAlign = ContentAlignment.TopCenter;

            // Temperature sensor #5 value [°C]
            this.LblTmp5Val.Font = this.FigureFont;
            this.LblTmp5Val.Location = new Point(226, 30);
            this.LblTmp5Val.Name = Gui.T_LBL + Gui.G_TMP + "5" + Gui.S_VAL;
            this.LblTmp5Val.Size = new Size(44, 27);
            this.LblTmp5Val.TabIndex = 11;
            this.LblTmp5Val.TextAlign = ContentAlignment.MiddleCenter;

            // Temperature sensor #6 caption
            this.LblTmp6Cap.Location = new Point(270, 16);
            this.LblTmp6Cap.Name = Gui.T_LBL + Gui.G_TMP + "6" + Gui.S_CAP;
            this.LblTmp6Cap.Size = new Size(44, 14);
            this.LblTmp6Cap.TabIndex = 12;
            this.LblTmp6Cap.TextAlign = ContentAlignment.TopCenter;

            // Temperature sensor #6 value [°C]
            this.LblTmp6Val.Font = this.FigureFont;
            this.LblTmp6Val.Location = new Point(270, 30);
            this.LblTmp6Val.Name = Gui.T_LBL + Gui.G_TMP + "6" + Gui.S_VAL;
            this.LblTmp6Val.Size = new Size(44, 27);
            this.LblTmp6Val.TabIndex = 13;
            this.LblTmp6Val.TextAlign = ContentAlignment.MiddleCenter;

            // Temperature sensor #7 caption
            this.LblTmp7Cap.Location = new Point(314, 16);
            this.LblTmp7Cap.Name = Gui.T_LBL + Gui.G_TMP + "7" + Gui.S_CAP;
            this.LblTmp7Cap.Size = new Size(44, 14);
            this.LblTmp7Cap.TabIndex = 14;
            this.LblTmp7Cap.TextAlign = ContentAlignment.TopCenter;

            // Temperature sensor #7 value [°C]
            this.LblTmp7Val.Font = this.FigureFont;
            this.LblTmp7Val.Location = new Point(314, 30);
            this.LblTmp7Val.Name = Gui.T_LBL + Gui.G_TMP + "7" + Gui.S_VAL;
            this.LblTmp7Val.Size = new Size(44, 27);
            this.LblTmp7Val.TabIndex = 15;
            this.LblTmp7Val.TextAlign = ContentAlignment.MiddleCenter;

            // Temperature sensor #8 caption
            this.LblTmp8Cap.Location = new Point(358, 16);
            this.LblTmp8Cap.Name = Gui.T_LBL + Gui.G_TMP + "8" + Gui.S_CAP;
            this.LblTmp8Cap.Size = new Size(44, 14);
            this.LblTmp8Cap.TabIndex = 16;
            this.LblTmp8Cap.TextAlign = ContentAlignment.TopCenter;

            // Temperature sensor #8 value [°C]
            this.LblTmp8Val.Font = this.FigureFont;
            this.LblTmp8Val.Location = new Point(358, 30);
            this.LblTmp8Val.Name = Gui.T_LBL + Gui.G_TMP + "8" + Gui.S_VAL;
            this.LblTmp8Val.Size = new Size(44, 27);
            this.LblTmp8Val.TabIndex = 17;
            this.LblTmp8Val.TextAlign = ContentAlignment.MiddleCenter;

            // Temperature group components
            this.GrpTmp.Controls.Add(this.LblTmp0Cap);
            this.GrpTmp.Controls.Add(this.LblTmp0Val);
            this.GrpTmp.Controls.Add(this.LblTmp1Cap);
            this.GrpTmp.Controls.Add(this.LblTmp1Val);
            this.GrpTmp.Controls.Add(this.LblTmp2Cap);
            this.GrpTmp.Controls.Add(this.LblTmp2Val);
            this.GrpTmp.Controls.Add(this.LblTmp3Cap);
            this.GrpTmp.Controls.Add(this.LblTmp3Val);
            this.GrpTmp.Controls.Add(this.LblTmp4Cap);
            this.GrpTmp.Controls.Add(this.LblTmp4Val);
            this.GrpTmp.Controls.Add(this.LblTmp5Cap);
            this.GrpTmp.Controls.Add(this.LblTmp5Val);
            this.GrpTmp.Controls.Add(this.LblTmp6Cap);
            this.GrpTmp.Controls.Add(this.LblTmp6Val);
            this.GrpTmp.Controls.Add(this.LblTmp7Cap);
            this.GrpTmp.Controls.Add(this.LblTmp7Val);
            this.GrpTmp.Controls.Add(this.LblTmp8Cap);
            this.GrpTmp.Controls.Add(this.LblTmp8Val);

            // Temperature group settings
            this.GrpTmp.Location = new Point(298, 3);
            this.GrpTmp.Name = Gui.T_GRP + Gui.G_TMP;
            this.GrpTmp.Size = new Size(405, 65);
            this.GrpTmp.TabIndex = 1;
            this.GrpTmp.TabStop = false;
            this.GrpTmp.Text = Config.Locale.Get(Config.L_GUI_MAIN + Gui.G_TMP).Replace("&", "&&");
#endregion

#region Main Form
            // Main form components
            this.Controls.Add(this.GrpSys);
            this.Controls.Add(this.GrpKbd);
            this.Controls.Add(this.GrpFan);
            this.Controls.Add(this.GrpTmp);

            // Main form settings
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.HelpButton = true;
            this.Icon = OmenMon.Resources.Icon;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = Gui.T_FRM + "Main";
            this.Size = new Size(725, 300);
            this.SizeGripStyle = SizeGripStyle.Hide;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = Config.Locale.Get(Config.L_GUI_MAIN + "Title");
#endregion

#region Tool Tips
            // Common settings
            this.Tip.InitialDelay = 0;
            this.Tip.ReshowDelay = 0;
            this.Tip.AutoPopDelay = 5000;

            // Fan control
            this.Tip.SetToolTip(this.RdoFanProg, Config.Locale.Get(Config.L_GUI_TIP + Gui.G_FAN + "Prog"));
            this.Tip.SetToolTip(this.CmbFanProg, Config.Locale.Get(Config.L_GUI_TIP + Gui.G_FAN + "Prog" + Gui.T_CMB));
            this.Tip.SetToolTip(this.RdoFanAuto, Config.Locale.Get(Config.L_GUI_TIP + Gui.G_FAN + "Auto"));
            this.Tip.SetToolTip(this.CmbFanMode, Config.Locale.Get(Config.L_GUI_TIP + Gui.G_FAN + "Mode"));
            this.Tip.SetToolTip(this.RdoFanConst, Config.Locale.Get(Config.L_GUI_TIP + Gui.G_FAN + "Const"));
            this.Tip.SetToolTip(this.RdoFanMax, Config.Locale.Get(Config.L_GUI_TIP + Gui.G_FAN + "Max"));
            this.Tip.SetToolTip(this.RdoFanOff, Config.Locale.Get(Config.L_GUI_TIP + Gui.G_FAN + "Off"));
            this.Tip.SetToolTip(this.BtnFanSet, Config.Locale.Get(Config.L_GUI_TIP + Gui.G_FAN + "Set"));

            // Fan monitor
            this.Tip.SetToolTip(this.LblFan0Cap, Config.Locale.Get(Config.L_GUI_TIP + Gui.G_FAN + "0" + Gui.S_CAP));
            this.Tip.SetToolTip(this.LblFan1Cap, Config.Locale.Get(Config.L_GUI_TIP + Gui.G_FAN + "1" + Gui.S_CAP));
            this.Tip.SetToolTip(this.LblFanUnitVal, Config.Locale.Get(Config.L_GUI_TIP + Gui.G_FAN + Gui.X_UNIT + Gui.S_VAL));
            this.Tip.SetToolTip(this.LblFan0Val, Config.Locale.Get(Config.L_GUI_TIP + Gui.G_FAN + "0" + Gui.S_VAL));
            this.Tip.SetToolTip(this.LblFan1Val, Config.Locale.Get(Config.L_GUI_TIP + Gui.G_FAN + "1" + Gui.S_VAL));
            this.Tip.SetToolTip(this.LblFanUnitRte, Config.Locale.Get(Config.L_GUI_TIP + Gui.G_FAN + Gui.X_UNIT + Gui.S_RTE));
            this.Tip.SetToolTip(this.LblFan0Rte, Config.Locale.Get(Config.L_GUI_TIP + Gui.G_FAN + "0" + Gui.S_RTE));
            this.Tip.SetToolTip(this.BarFan0Rte, Config.Locale.Get(Config.L_GUI_TIP + Gui.G_FAN + "0" + Gui.S_RTE + Gui.T_BAR));
            this.Tip.SetToolTip(this.LblFan1Rte, Config.Locale.Get(Config.L_GUI_TIP + Gui.G_FAN + "1" + Gui.S_RTE));
            this.Tip.SetToolTip(this.BarFan1Rte, Config.Locale.Get(Config.L_GUI_TIP + Gui.G_FAN + "1" + Gui.S_RTE + Gui.T_BAR));
            this.Tip.SetToolTip(this.TrkFan0Lvl, Config.Locale.Get(Config.L_GUI_TIP + Gui.G_FAN + "0" + Gui.S_LVL));
            this.Tip.SetToolTip(this.TrkFan1Lvl, Config.Locale.Get(Config.L_GUI_TIP + Gui.G_FAN + "1" + Gui.S_LVL));
            this.Tip.SetToolTip(this.LblFanCountdown, Config.Locale.Get(Config.L_GUI_TIP + Gui.G_FAN + "Countdown"));

            // Keyboard
            this.Tip.SetToolTip(this.ChkKbdBacklight, Config.Locale.Get(Config.L_GUI_TIP + Gui.G_KBD + "Backlight"));
            this.Tip.SetToolTip(this.CmbKbdColorPreset, Config.Locale.Get(Config.L_GUI_TIP + Gui.G_KBD + "ColorPreset"));
            this.Tip.SetToolTip(this.TxtKbdColorVal, Config.Locale.Get(Config.L_GUI_TIP + Gui.G_KBD + "ColorVal"));
            this.Tip.SetToolTip(this.BtnKbdColorPresetDel, Config.Locale.Get(Config.L_GUI_TIP + Gui.G_KBD + "ColorPresetDel"));
            this.Tip.SetToolTip(this.BtnKbdColorPresetSet, Config.Locale.Get(Config.L_GUI_TIP + Gui.G_KBD + "ColorPresetSet"));
            this.Tip.SetToolTip(this.PicKbd, Config.Locale.Get(Config.L_GUI_TIP + Gui.G_KBD + Gui.T_PIC));

            // System status
            this.Tip.SetToolTip(this.GrpSys, Config.Locale.Get(Config.L_GUI_TIP + Gui.G_SYS));

            // System status rich-text field is not enabled, and would not show its tooltip
            // Temperature group has dynamic tooltips generated at runtime
#endregion

#region Resume Layout
            // End the initialization of components that specifically require it
            ((System.ComponentModel.ISupportInitialize) this.PicKbd).EndInit();
            ((System.ComponentModel.ISupportInitialize) this.TrkFan0Lvl).EndInit();
            ((System.ComponentModel.ISupportInitialize) this.TrkFan1Lvl).EndInit();

            // Resume the layout
            this.GrpFan.ResumeLayout(false);
            this.GrpKbd.ResumeLayout(false);
            this.GrpSys.ResumeLayout(false);
            this.GrpTmp.ResumeLayout(false);
            this.ResumeLayout(false);

            // Also make a call to perform layout where necessary
            this.GrpKbd.PerformLayout();
#endregion

#region Component Events
            // Fan
            this.CmbFanProg.SelectionChangeCommitted += EventFanProgramChanged;
            this.CmbFanMode.SelectionChangeCommitted += EventFanModeChanged;
            this.RdoFanAuto.CheckedChanged += EventFanRdoChanged;
            this.RdoFanConst.CheckedChanged += EventFanRdoChanged;
            this.RdoFanOff.CheckedChanged += EventFanRdoChanged;
            this.RdoFanMax.CheckedChanged += EventFanRdoChanged;
            this.RdoFanProg.CheckedChanged += EventFanRdoChanged;
            this.TrkFan0Lvl.ValueChanged += EventFanTrkChanged;
            this.TrkFan1Lvl.ValueChanged += EventFanTrkChanged;
            this.BtnFanSet.Click += EventActionFanSet;

            // Keyboard
            this.ChkKbdBacklight.Click += EventActionBacklight;
            this.CmbKbdColorPreset.SelectionChangeCommitted += EventColorPreset;
            this.TxtKbdColorVal.TextChanged += EventColorInput;
            this.BtnKbdColorPresetDel.Click += EventActionColorPresetDel;
            this.BtnKbdColorPresetSet.Click += EventActionColorPresetSet;
            this.PicKbd.MouseClick += EventColorPick;
#endregion

        }
#endregion

    }

}
