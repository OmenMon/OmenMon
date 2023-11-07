  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using OmenMon.External;
using OmenMon.Library;

namespace OmenMon.AppGui {

    // Manages the notification icon
    public class GuiIcon {

#region Data
        // Icon background types
        public enum BackgroundType : byte {
            Empty   = 0x00,  // No background
            Outline = 0x01,  // Diamond outline only
            Cool    = 0x02,  // Filled diamond, cool gradient
            Warm    = 0x03,  // Filled diamond, warm gradient
            Default = 0xFF   // Default icon
        }

        // Holds the currently-selected background
        public BackgroundType Background { get; private set; }

        // State flags
        public bool IsConfigured { get; private set; }
        public bool IsDynamic { get; private set; }
        public bool IsDynamicBackground { get; private set; }

        // Parent class reference
        private GuiTray Context;

        // Cached data
        private IntPtr LastHandle;
        private Size Size;
        private Rectangle Box;
        private Bitmap Template;
        private Font TextFont;
        private StringFormat TextFormat;
        private string LastMessage;
#endregion

#region Configuration
        // Initializes the class
        public GuiIcon(GuiTray context) {

            // Initialize the parent class reference
            this.Context = context;

            // Determine the icon size
            SetSize();

            // Set up the text format
            SetTextFormat();

            // Configure the initial setting whether the dynamic icon has a background
            SetDynamicBackground(Config.GuiDynamicIconHasBackground);

            // Configure whether the icon is initially dynamic
            // This also triggers an initialization of all the other settings
            SetDynamic(Config.GuiDynamicIcon);

        }

        // Prepares an intermediate snapshot for the dynamic notification icon
        // so that the same steps do not have to be repeated on every redraw
        public void Configure() {

            // Let go of the previous template, if any
            if(this.Template != null)
                this.Template.Dispose();

            // Empty the last message
            this.LastMessage = null;

            // Most of the configuration steps only apply to a dynamic icon
            if(this.IsDynamic) {

                // Set up the drawing surface, either using a background
                // or starting with an empty slate
                this.Template = this.Background == BackgroundType.Outline ?
                    new Bitmap(OmenMon.Resources.IconTrayOutline.ToBitmap(), this.Size)
                    : new Bitmap(this.Size.Width, this.Size.Height);
                using Graphics Canvas = Graphics.FromImage(Template);

                // Set up the drawing properties
                Canvas.SmoothingMode = SmoothingMode.HighQuality; // Does not apply to text
                Canvas.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;

                // Note: ClearTypeGridFit yields suboptimal results,
                // it's a tie between AntiAliasGridFit or SingleBitPerPixelGridFit

                // Render either the cool or warm gradient as a background, if requested
                if(this.IsDynamicBackground)
                    Canvas.FillPolygon(
                        new LinearGradientBrush(
                            this.Box, // The entire drawing surface
                            Color.FromArgb(this.Background == BackgroundType.Cool ? Config.GuiColorCoolDark : Config.GuiColorWarmDark),
                            Color.FromArgb(this.Background == BackgroundType.Cool ? Config.GuiColorCoolLite : Config.GuiColorWarmLite),
                            this.Background == BackgroundType.Cool ? LinearGradientMode.Vertical : LinearGradientMode.Horizontal),
                        new Point[] { // The shape of a diamond
                            new Point(Size.Width / 2, 0),
                            new Point(Size.Width, Size.Height / 2),
                            new Point(Size.Width / 2, Size.Height),
                            new Point(0, Size.Height / 2),
                            new Point(Size.Width / 2, 0)
                        });

                // Otherwise still have to draw something, or the background
                // will remain all black until the next update (GDI bug?)
                else if(this.Background != BackgroundType.Outline)
                   this.Template.SetPixel(0, 0, Color.Black);

            }

            // Set the icon as configured
            this.IsConfigured = true;
            Update("");

        }

        // Reset the icon to the default one
        public void Reset() {

            // Load the icon from a resource
            this.Context.Notification.Icon = OmenMon.Resources.IconTray;

        }

        // Sets the dynamic icon background
        // Invalidates current icon configuration if background changed
        public void SetBackground(BackgroundType newBackground) {

            // Only if the background changed
            if(this.Background != newBackground) {

                // Set the new background
                // Mark the icon as not configured
                this.Background = newBackground;
                this.IsConfigured = false;

            }

        }

        // Sets whether the icon is dynamic or not
        // Invalidates current icon configuration if flag changed
        public void SetDynamic(bool flag) {

            // Only if the dynamic flag changed
            if(this.IsDynamic != flag) {

                // Mark the icon as not configured
                // Update the flags
                this.IsConfigured = false;
                this.IsDynamic = flag;

                // Configure, and implicitly update
                Configure();

            }

        }

        // Sets whether the dynamic icon has a background or not
        // Invalidates current icon configuration if flag changed
        public void SetDynamicBackground(bool flag) {

            // Only if the dynamic background flag changed
            if(this.IsDynamicBackground != flag) {
 
                // Update the dynamic background flag
                this.IsDynamicBackground = flag;

                // Set the dynamic background
                if(!this.IsDynamicBackground)
                    SetBackground(BackgroundType.Empty);
                else
                    SetBackground(BackgroundType.Cool);

                // Mark the icon as not configured
                this.IsConfigured = false;

            }

        }

        // Sets the notification icon dimensions
        // Invalidates current icon configuration
        public void SetSize() {

            // Determine the correct dimensions for a notification icon
            this.Size = SystemInformation.SmallIconSize;
            this.Size.Height = this.Size.Width *= Config.GuiDynamicIconUpscaleRatio;
            this.Box = new Rectangle(Point.Empty, Size);

            // Note: The system default is twice the small icon size
            // in each dimension, possible values are: 32, 40, 48, 60, 64 px
            // for 96-192 dpi (100-200% scaling) in 24 dpi (+25%) increments

            // Mark the icon as not configured
            this.IsConfigured = false;

        }

        // Sets the notification icon text formatting
        // Invalidates current icon configuration
        public void SetTextFormat() {

            // Retrieve the custom font loaded from a resource
            this.TextFont = new Font(
                GdiFont.Get(0),
                (int) Math.Round(Config.GuiDynamicIconFontSizeRatio * Size.Width),
                FontStyle.Regular,
                GraphicsUnit.Pixel);

            // Set up the text alignment so that the text is centered
            this.TextFormat = new StringFormat(StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip | StringFormatFlags.NoWrap);
            this.TextFormat.Alignment = StringAlignment.Center;
            this.TextFormat.LineAlignment = StringAlignment.Center;
            this.TextFormat.Trimming = StringTrimming.None;

            // Mark the icon as not configured
            this.IsConfigured = false;

        }
#endregion

#region Update
        // Updates the notification icon to a dynamically-rendered image
        public void Update(string message) {

            // Configure if necessary
            if(!this.IsConfigured)
                Configure();

            // Reset if not dynamic
            if(!this.IsDynamic)
                Reset();

            // Only proceed if the message is different than previously,
            // or if there is no previous message (first run or state changed)
            else if(this.LastMessage == null || this.LastMessage != message) {

                // Make a local copy of the template bitmap
                using Bitmap NewIcon = (Bitmap) this.Template.Clone();
                using Graphics Canvas = Graphics.FromImage(NewIcon);

                // The end result is better when filling a path
                // as opposed to using DrawString() directly
                using GraphicsPath CanvasPath = new GraphicsPath();
                CanvasPath.AddString(message, TextFont.FontFamily, (int) FontStyle.Regular, (Canvas.DpiY * TextFont.SizeInPoints / 72), Box, TextFormat);
                Canvas.FillPath(Brushes.White, CanvasPath);

                // Update the icon now that it is ready
                User32.DestroyIcon(this.LastHandle);
                this.LastHandle = NewIcon.GetHicon();
                this.Context.Notification.Icon = System.Drawing.Icon.FromHandle(this.LastHandle);

                // Note: freeing the allocated memory with User32.DestroyIcon() here also
                // causes the icon to vanish should the notification text be updated

                // If not doing this causes a memory leak, the notification text can also
                // be cached until the next time the icon is updated but otherwise, it's
                // preferable that the user can see it as soon as possible

                // Update: As a possible workaround for a "general GDI+ failure"
                // exception, changed to now release the previous (last) handle
                // before creating a new one

            }

        }
#endregion

    }

}
