  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using OmenMon.Library;

namespace OmenMon.Library {

    // Renders an improved progress bar with custom colors
    public class ProgressBarEx : ProgressBar {

#region Initialization
        // Inner margin value
        public int Inset;

        // Linear gradient mode
        public LinearGradientMode LinearGradientMode;

        // Constructs a progress bar instance
        public ProgressBarEx() {

           // Initialize the inset setting with the default value
           if(this.Inset <= 0)
               this.Inset = Config.GuiProgressBarInset;

           // Set the user-painted flag for the control
           // Mitigate the flicker due to OnPaintBackground
           this.SetStyle(
               ControlStyles.AllPaintingInWmPaint
               | ControlStyles.OptimizedDoubleBuffer
               | ControlStyles.UserPaint, true);

           // Note: OptimizedDoubleBuffer has the added benefit of automatically
           // mirroring the control if RightToLeftLayout is set to true

        }
#endregion

#region Rendering
        // Paints the progress bar in customized colors
        protected override void OnPaint(PaintEventArgs e) {

            // Initialize the disposable objects
            using Image SurfaceImage = new Bitmap(this.Width, this.Height);
            using Graphics Surface = Graphics.FromImage(SurfaceImage);

            // Make the drawing area slightly narrower
            // Essential for proper right-to-left rendering
            Rectangle Box = ClientRectangle;
            Box.Width--;

            // Draw the progress bar background
            // Depends upon OS visual-style support
            if(ProgressBarRenderer.IsSupported)
                ProgressBarRenderer.DrawHorizontalBar(Surface, Box);

            // Make the progress bar drawing smaller
            // than the background by a specified inner margin
            Box.Inflate(- this.Inset, - this.Inset);

            // Calculate the progress bar width and draw it
            // if the resulting width is larger than zero
            if((Box.Width = Box.Width
                * (this.Value - this.Minimum)
                / (this.Maximum - this.Minimum)) > 0) {

                // Create the brush
                using LinearGradientBrush Brush = new LinearGradientBrush(
                    Box, this.BackColor, this.ForeColor, this.LinearGradientMode);

                // Fill the space using the brush
                Surface.FillRectangle(Brush, this.Inset, this.Inset, Box.Width, Box.Height);

            }

            // Draw the image
            e.Graphics.DrawImage(SurfaceImage, Point.Empty);

        }
#endregion

    }

}
