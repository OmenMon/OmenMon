  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;

namespace OmenMon.Hardware.Platform {

    // Holds platform-related data
    public abstract class PlatformData { 

        // Access type flags
        [Flags]
        public enum AccessType {
            None =  0x00,
            Read =  0x01,
            Write = 0x02
        }

        // Data size type
        public enum DataSize {
            Byte =  0x000000FF,
            Word =  0x0000FFFF
        }

        // Number of fans
        public const int FanCount = 2;

        // Fan manual toggle
        public enum FanManual : byte {
            Off = 0x00,
            On  = 0x06
        }

        // Fan on/off switch
        public enum FanSwitch : byte {
            On  = 0x00,
            Off = 0x02
        }

        // Link type
        public enum LinkType {
            EmbeddedController,
            WmiBios
        }

        // Value trend type
        public enum ValueTrend {
            Descending = -1,
            Unchanged  =  0,
            Ascending  =  1
        }

    }

}
