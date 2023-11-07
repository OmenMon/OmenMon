  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using System.Windows.Forms;
using OmenMon.Library;

namespace OmenMon.AppGui {

    // Sets up a message filter for the tray application context
    public class GuiFilter : IMessageFilter {

        // Last-received identifier, to distinguish duplicate messages
        private IntPtr LastId;

        // Actions depend on the previously-received message as well
        private Gui.MessageParam LastParam;

        // Parent class reference
        private GuiTray Context;

        // Initialize the parent class reference
        public GuiFilter(GuiTray context) {
            this.Context = context;
        }

#region GUI Message Filter
        // Filtering routine
        public bool PreFilterMessage(ref Message m) {

            // Look for the custom message identifier
            if(m.Msg == Gui.MessageId) {

                // Ignore duplicate instances of the same message
                // Unable to filter by window handle that we officially might not have
                if(m.WParam == LastId)
                    return true;

                switch((Gui.MessageParam) m.LParam) {

                    // Another instance has been run
                    case Gui.MessageParam.AnotherInstance:

                        // Unless ran as a task
                        if(LastParam != Gui.MessageParam.Gui
                            && LastParam != Gui.MessageParam.Key)

                            // Bring focus to the current instance
                            Context.BringFocus();

                        break;

                    // Launched from a task on boot
                    case Gui.MessageParam.Gui:

                        // Run quietly
                        break;

                    // Omen Key event has been registered
                    case Gui.MessageParam.Key:

                        // Launch the Omen key handler
                        Context.Op.KeyHandler(LastParam);
                        break;

                }

                // Store the previous parameter values
                LastId = m.WParam;                  
                LastParam = (Gui.MessageParam) m.LParam;

                // Terminate any further processing
                // for messages of this type
                return true;

            }

            // Process other messages normally
            return false;

        }
#endregion

    }

}
