  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using OmenMon.Hardware.Bios;
using OmenMon.Library;

namespace OmenMon.AppGui {

    // Recolors the keyboard line art when the backlight colors change
    public class GuiKbd {

#region Data
        // Parent class reference
        private GuiTray Context;

        // Cached data used on every redraw
        private ImageAttributes Attributes;
        private readonly ColorMap[] Map;
        private Bitmap Template;

        // State data
        private int[] ColorArray;
        private bool IsBacklight;
        private BiosData.KbdZone Zone;

        // Last-rendered image and its colors
        private Bitmap Image;
#endregion

#region Initialization
        // Constructs an instance
        // Prepares the data structures that persist between redraws
        public GuiKbd(GuiTray context) {

            // Initialize the parent class reference
            this.Context = context;

            // Set up the new template that serves as a base for every redraw
            this.Template = new Bitmap(OmenMon.Resources.Keyboard);

            // Populate the color remap table with original colors to be replaced
            this.Map = new ColorMap[4];
            for(int i = 0; i < 4; i++) {
                this.Map[i] = new ColorMap();
                this.Map[i].OldColor = Color.FromArgb(Config.GuiColorKbdZoneOrig[i]);
            }

            // Set up the attribute structure
            this.Attributes = new ImageAttributes();

            // Initialize the color arrays
            this.ColorArray = new int[4];

            // Get the current state from hardware
            GetHw();

        }
#endregion

#region Backlight
        // Retrieves the current backlight state
        public bool GetBacklight() {
            return this.IsBacklight;
        }

        // Sets the backlight state
        public void SetBacklight(bool flag, bool deferUpdate = false) {
            if(this.IsBacklight != flag) {
                this.IsBacklight = flag;

                // Allow for deferring update
                if(!deferUpdate)
                    Update();

            }
        }
#endregion

#region Color
        // Gets the color of the current zone
        public int GetColor() {
            return GetColor(this.Zone);
        }

        // Gets the color of a given zone
        public int GetColor(BiosData.KbdZone zone) {
            return this.ColorArray[(int) zone];
        }

        // Gets the color of a given zone in a reverse order
        public int GetColorReverse(BiosData.KbdZone zone) {
            return Conv.GetColorReverse(this.ColorArray[(int) zone]);
        }

        // Gets all the colors as an array
        public int[] GetColors() {
            return ColorArray;
        }

        // Sets the color of the current zone
        public void SetColor(int color) {
            SetColor(this.Zone, color);
        }

        // Sets the color of a given zone
        public void SetColor(BiosData.KbdZone zone, int color) {
            if(GetColor(zone) != color) {
                this.ColorArray[(int) zone] = color;
                Update();
            }
        }

        // Sets the colors for all zones
        public void SetColors(int[] color) {
            if(!Conv.ArraysEqual(this.ColorArray, color)) {
                this.ColorArray = color;
                Update();
            }
        }

        // Sets the color for all zones to the same color value
        public void SetColors(int color) {

            // Call the main update method setting all zones to the same color
            SetColors(new int[] { color, color, color, color });

        }

        // Sets the colors for all zones given a BIOS color table
        public void SetColors(BiosData.ColorTable colorTable) {

            // Read the colors from the structure
            int[] color = new int[4];
            for(int i = 0; i < 4; i++)
                color[i] = (int) colorTable.Zone[i].ValueReverse;

            // Set the colors
            SetColors(color);

        }
#endregion

#region Current Zone
        // Retrieves the zone currently being modified
        public BiosData.KbdZone GetZone() {
            return this.Zone;
        }

        // Sets the zone currently being modified
        public void SetZone(BiosData.KbdZone zone) {
            this.Zone = zone;
        }

        // Sets the zone given the co-ordinates
        public BiosData.KbdZone SetZone(int x, int y) {

            // Calculate the relative horizontal co-ordinate
            int rx = 100 * x / this.Context.FormMain.PicKbd.Width;

            if(rx > 72)

                // 72% = 860 / 1200 [px]
                this.Zone = BiosData.KbdZone.Right;

            else if(rx > 33)

                // 33% = 400 / 1200 [px]
                this.Zone = BiosData.KbdZone.Middle;

            else {

                // Calculate the relative vertical co-ordinate
                // only when it's necessary to do so
                int ry = 100 * y / this.Context.FormMain.PicKbd.Height;

                if(ry > 66 || ry < 31)

                    // 31% ... 66% = 120 ... 260 / 393 [px]
                    this.Zone = BiosData.KbdZone.Left;

                else

                    // By elimination, must be the last zone
                    this.Zone = BiosData.KbdZone.Wasd;
            }

            // Also return the zone value
            return this.Zone;

        }
#endregion

#region Hardware
        // Gets the current state from hardware
        public void GetHw() {

            // Get the backlight state
            SetBacklight(Context.Op.Platform.System.GetKbdBacklight()
                == BiosData.Backlight.On ? true : false, true); // Defer update

            // Get the color table
            SetColors(Context.Op.Platform.System.GetKbdColor());

        }

        // Sets the hardware to the current state
        public void SetHw() {

            // Set the backlight state
            Context.Op.Platform.System.SetKbdBacklight(this.IsBacklight);

            // Set the color table
            Context.Op.Platform.System.SetKbdColor(new BiosData.ColorTable(ColorArray, true));

        }
#endregion

#region Result
        // Retrieves the current image
        public Bitmap GetImage() {
            return this.Image;
        }

        // Gets the color value for a zone as a string
        public string GetColorString(BiosData.KbdZone zone) {
            return Conv.GetColorString(ColorArray[(int) zone]);
        }

        // Retrieves the parameter that re-creates the current settings
        public string GetParam() {
            return GetColorString(BiosData.KbdZone.Right)
                + ":" + GetColorString(BiosData.KbdZone.Middle)
                + ":" + GetColorString(BiosData.KbdZone.Left)
                + ":" + GetColorString(BiosData.KbdZone.Wasd);
        }

        // Checks if the current color settings match any preset
        public string GetPreset() {
            foreach(string name in Config.ColorPreset.Keys) 

                // Compare each preset to the current colors
                if(ColorArray[0] == Config.ColorPreset[name].Zone[0].ValueReverse
                    && ColorArray[1] == Config.ColorPreset[name].Zone[1].ValueReverse
                    && ColorArray[2] == Config.ColorPreset[name].Zone[2].ValueReverse
                    && ColorArray[3] == Config.ColorPreset[name].Zone[3].ValueReverse)

                    return name;

            return "";
        }
#endregion

#region Update
        // Redraws the picture in new colors given an array of color values
        public void Update() {

            // Fill out the color map with new color values
            for(int i = 0; i < 4; i++)
                Map[i].NewColor = Color.FromArgb(this.IsBacklight ?
                    Conv.GetColorMaxAlpha(this.ColorArray[i])
                    : Config.GuiColorKbdBacklightOff);

            // Update the remap table within the persistent attribute structure
            this.Attributes.SetRemapTable(Map, ColorAdjustType.Default);

            // Let go of the previously-used last image, if any
            if(this.Image != null)
                this.Image.Dispose();

            // Clone off the template and draw on a graphics surface over it
            this.Image = (Bitmap) this.Template.Clone();
            using(Graphics Canvas = Graphics.FromImage(this.Image)) {

                // Recolor the picture
                Canvas.DrawImage(
                    this.Image,
                    new Rectangle(0, 0, this.Template.Width, this.Template.Height),
                    0, 0, this.Template.Width, this.Template.Height, // Source
                    GraphicsUnit.Pixel, Attributes);

                // Update the picture
                if(this.Context.FormMain != null)
                    this.Context.FormMain.PicKbd.Image = Image;

            }

            // Update the hardware
            SetHw();

        }

        // Updates the custom colors in the color picker
        public int[] UpdateColorPicker(int[] currentColors) {

            // Start with the current colors
            int[] customColors = currentColors;

            // Update the values for the four zones
            customColors[0] = GetColorReverse(BiosData.KbdZone.Right);
            customColors[1] = GetColorReverse(BiosData.KbdZone.Middle);
            customColors[8] = GetColorReverse(BiosData.KbdZone.Left);
            customColors[9] = GetColorReverse(BiosData.KbdZone.Wasd);

            // Return the updated colors
            return customColors;

        }
#endregion

    }

}
