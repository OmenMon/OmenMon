  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OmenMon.External;

namespace OmenMon.Library {

    // Custom extension of the system color dialog
    public class ColorDialogEx : ColorDialog {

        // Additional customizable settings
        private Action<int> Callback;
        public Point Position;
        public string Title = "";

        // Control identifier of the Cancel button
        private const int ID_CANCEL = 0x0002;

        // "Define Custom Colors" button control identifier
        private int COLOR_MIX = 0x02CF;

        // Current color box control identifier
        private int COLOR_CURRENT = 0x02C5;

        // Stores the handle to the COLOR_CURRENT control
        private IntPtr CurrentColor;

        // Constructs the dialog
        public ColorDialogEx(Action<int> callback, string title = "") {

            // Override the default settings
            this.AllowFullOpen = true;
            this.AnyColor = true;
            this.FullOpen = true;
            this.ShowHelp = false;
            this.SolidColorOnly = true;

            // Store the callback method
            this.Callback = callback;

            // Save the requested window title
            this.Title = title;

        }

        // Hooks into and extends the common dialog box
        protected override IntPtr HookProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam) {

            // Call the base hook procedure first
            IntPtr result = base.HookProc(hWnd, msg, wParam, lParam);

            // Respond to events
            switch(msg) {

                // One of the non-editable controls is being updated
                case User32.WM_CTLCOLORSTATIC:

                    // If the control is the current-color box
                    if(lParam == this.CurrentColor)

                        // Get the updated current color and notify the parent class with a callback
                        Callback(Os.GetPixel(this.CurrentColor, Point.Empty));

                    break;

                // Dialog is being initialized
                case User32.WM_INITDIALOG:

                    // Store the handle to the current color control for later use
                    this.CurrentColor = User32.GetDlgItem(hWnd, COLOR_CURRENT);

                    // Set the position
                    User32.SetWindowPos(
                        hWnd,                              // Dialog window handle
                        User32.HWND_TOP,                   // Ignored per SWP_NOZORDER
                        this.Position.X, this.Position.Y,  // Initial position co-ordinates
                        0, 0,                              // Ignored per SWP_NOSIZE
                        User32.SWP_NOSIZE | User32.SWP_NOZORDER | User32.SWP_SHOWWINDOW);

                    // Set the title
                    User32.SetWindowText(hWnd, this.Title);

                    // Hide the "Cancel" button, since changes take place real-time
                    User32.ShowWindow(User32.GetDlgItem(hWnd, ID_CANCEL), User32.SW_HIDE);

                    // Hide the "Define Custom Colors" button, which is disabled anyway
                    User32.ShowWindow(User32.GetDlgItem(hWnd, COLOR_MIX), User32.SW_HIDE);

                    break;

            }

            // Return the base hook result
            return result;

        }

    }

}
