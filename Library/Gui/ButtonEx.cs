  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace OmenMon.Library {

    // Implements a button with an optional highlighted inner border
    public class ButtonEx : Button {

#region Variables
        // Stores the highlight path
        protected GraphicsPath Path;

        // Setting values exposed through accessors
        protected byte HighlightBaseTransparencyValue;
        protected int HighlightRadiusValue;
        protected float HighlightWidthValue;
        protected Color HighlightColorLightValue;
        protected Color HighlightColorDarkValue;
        protected LinearGradientMode HighlightGradientModeValue;
        protected bool CheckedValue;
#endregion

#region Accessors
        // Checked state accessor that triggers a repaint on change
        public bool Checked {
            get {
                return this.CheckedValue;
            }
            set { 
                this.CheckedValue = value;
                Invalidate();
            }
        }

        // Highlight base transparency that triggers a repaint on change
        public byte HighlightBaseTransparency {
            get {
                return this.HighlightBaseTransparencyValue;
            }
            set {
                this.HighlightBaseTransparencyValue = value;
                Invalidate();
            }
        }

        // Highlight color dark accessor that triggers a repaint on change
        public Color HighlightColorDark {
            get {
                return this.HighlightColorDarkValue;
            }
            set {
                this.HighlightColorDarkValue = value;
                Invalidate();
            }
        }

        // Highlight color light accessor that triggers a repaint on change
        public Color HighlightColorLight {
            get {
                return this.HighlightColorLightValue;
            }
            set {
                this.HighlightColorLightValue = value;
                Invalidate();
            }
        }

        // Highlight radius accessor that triggers a repaint on change
        public int HighlightRadius {
            get {
                return this.HighlightRadiusValue;
            }
            set {
                this.HighlightRadiusValue = value;
                Invalidate();
            }
        }

        // Highlight width accessor that triggers a repaint on change
        public float HighlightWidth {
            get {
                return this.HighlightWidthValue;
            }
            set {
                this.HighlightWidthValue = value;
                Invalidate();
            }
        }

        // Highlight gradient mode accessor that triggers a repaint on change
        public LinearGradientMode HighlightGradientMode {
            get {
                return this.HighlightGradientModeValue;
            }
            set {
                this.HighlightGradientModeValue = value;
                Invalidate();
            }
        }
#endregion

#region Initialization
        // Constructs a button instance
        public ButtonEx() {

            // Set the default base transparency
            if(!(this.HighlightBaseTransparency > 0))
                this.HighlightBaseTransparency = 0xC0; // = 192

            // Set the default highlight colors
            if(this.HighlightColorDark == Color.Empty)
                this.HighlightColorDark = Color.FromArgb(Config.GuiColorCoolDark);
            if(this.HighlightColorLight == Color.Empty)
                this.HighlightColorLight = Color.FromArgb(Config.GuiColorCoolLite);

            // Set the default highlight width
            if(!(this.HighlightWidth > 0))
                this.HighlightWidth = 4;

            // Set the default radius
            if(!(this.HighlightRadius > 0))
                this.HighlightRadius = 3;

            // Initialize the drawing path
            this.Path = new GraphicsPath();

        }
#endregion

#region Events
        // Handles the event that the button needs to be repainted
        protected override void OnPaint(PaintEventArgs e) {

            // Execute the base handler
            base.OnPaint(e);

            // Add a highlight if the button is checked
            if(this.Checked) {

                // Use anti-aliasing for improved quality
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                // Draw a series of highlighted inner border paths
                for(float f = this.HighlightWidth; f >= 0.01f; f -= 1f) {

                    // Calculate the transparency channel
                    int alpha = (int) (this.HighlightBaseTransparency
                        - this.HighlightBaseTransparency * f * f
                        / (this.HighlightWidth * this.HighlightWidth));

                    // Set up a brush
                    using(LinearGradientBrush Brush = new LinearGradientBrush(
                        this.ClientRectangle,
                        Color.FromArgb(alpha, this.HighlightColorLight),
                        Color.FromArgb(alpha, this.HighlightColorDark),
                        this.HighlightGradientMode)) {

                        // Set up a pen using the brush for color
                        using(Pen Pen = new Pen(Brush, f)) {

		            // Draw the highlighted path
                            Pen.LineJoin = LineJoin.Round;
                            e.Graphics.DrawPath(Pen, this.Path);

                        }

                    }

                }

            }

        }

        // Handles the event when the size of the button is changed
        protected override void OnSizeChanged(EventArgs e) {

            // Execute the base handler
            base.OnSizeChanged(e);

            // Update the highlight path
            this.Path = new GraphicsPath();

            // Helper variables
            int Diameter = 2 * this.HighlightRadius;
            float Padding = this.HighlightWidth / 2;
            const float RoundDown = 0.5f;

            // Top left corner
            RectangleF Box = new RectangleF(
                Padding, Padding,
                Diameter, Diameter);
            Path.AddArc(Box, 180, 90);

            // Top right corner
            Box.X = this.ClientSize.Width - Padding - Diameter - RoundDown;
            Path.AddArc(Box, 270, 90);

            // Bottom right corner
            Box.Y = this.ClientSize.Height - Padding - Diameter - RoundDown;
            Path.AddArc(Box, 0, 90);

	    // Bottom left corner
            Box.X = Padding;
            Path.AddArc(Box, 90, 90);

            // Done drawing the path
            Path.CloseFigure();

        }
#endregion

    }

}
