  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using OmenMon.Library;

namespace OmenMon.Hardware.Ec {

    // Defines Embedded Controller constants, variables, and structures
    // for subsequent use by the Embedded Controller-handling routines
    public abstract class EmbeddedControllerData {

#region Embedded Controller Access Data
        // Commands sent to the Embedded Controller
        protected enum Command : byte {
            Read     = 0x80,  // RD_EC
            Write    = 0x81,  // WR_EC
            BurstOn  = 0x82,  // BE_EC
            BurstOff = 0x83,  // BD_EC
            Query    = 0x84   // QR_EC
        }

        // Port numbers to send commands and data to
        protected enum Port : byte {
            Command  = 0x66,  // EC_SC
            Data     = 0x62   // EC_DATA
        }

        // Status values returned by the Embedded Controller
        [Flags]
        protected enum Status : byte {
            OutFull  = 0x01,  // Bit #0: EC_OBF
            InFull   = 0x02,  // Bit #1: EC_IBF
                              // Bit #2: n/a
            Command  = 0x08,  // Bit #3: CMD
            Burst    = 0x10,  // Bit #4: BURST
            SciEvt   = 0x20,  // Bit #5: SCI_EVT
            SmiEvt   = 0x40   // Bit #6: SMI_EVT
                              // Bit #7: n/a
        }
#endregion

#region Embedded Controller Register Information
        // Embedded Controller register identifiers
        // Labels based on ACPI DSDT for HP 08A14 (Ralph 21C2) except:
        // SMxx - Single 32-bit register starting with SMD0 (until SMEF)
        // BFCD, BADD, MCUS, MBRN, MBCW - Second byte for word-sized registers BFCC, BADC, MCUR, MBRM, MBCV
        // BXXX, GSXX, SHXX, SXXX - Composite register where all identifiers start with the same letter
        // RXnc - Composite registers with varying identifiers, where <n> - # of bits, <c> - sequential count
        // Xxxx - Registers with no DSDT label where purpose identified, and <xxx> is a descriptive string

        public enum Register : byte {

            // Identified
            XSS1 = 0x2C,  // L Fan Set Speed [%]
            XSS2 = 0x2D,  // R Fan Set Speed [%]
            XGS1 = 0x2E,  // L Fan Get Speed [%]
            XGS2 = 0x2F,  // R Fan Get Speed [%]
            SRP1 = 0x34,  // L Fan Set Speed [krpm]
            SRP2 = 0x35,  // R Fan Set Speed [krpm]
            TNT2 = 0x47,  // Temperature [°C]
            TNT3 = 0x48,  // Temperature [°C]
            TNT4 = 0x49,  // Temperature [°C]
            IRSN = 0x4A,  // Temperature [°C]
            TNT5 = 0x4B,  // Temperature [°C]
            CPUT = 0x57,  // Temperature: CPU [°C]
            RTMP = 0x58,  // Temperature [°C]
            TMP1 = 0x59,  // Temperature [°C]
            XHID = 0x5F,  // HID Disable Toggle
            OMCC = 0x62,  // Manual Fan Control
            XFCD = 0x63,  // Manual Fan Auto Countdown [s]
            HPCM = 0x95,  // Performance Mode
            XBCH = 0x96,  // Battery Charge Level
            QBHK = 0xA0,  // Last Hotkey
            QBBB = 0xA2,  // HID-Related (?)
            RPM1 = 0xB0,  // L Fan Get Speed [rpm] 1/2
            RPM2 = 0xB1,  // L Fan Get Speed [rpm] 2/2
            RPM3 = 0xB2,  // R Fan Get Speed [rpm] 1/2
            RPM4 = 0xB3,  // R Fan Get Speed [rpm] 2/2
            GPTM = 0xB7,  // Temperature: GPU [°C]
            CLOW = 0xBA,  // Minimum Power State
            CMAX = 0xBB,  // Maximum Power State
            FFFF = 0xEC,  // Max Fan Speed Toggle
            SFAN = 0xF4,  // Fan Toggle
            FTHM = 0xF9,  // Bit #4: GFXM, #7: FTHM Thermal Threshold Reached
            
            // Unidentified but mentioned in DSDT
            SMPR = 0x00,
            SMST = 0x01,
            SMAD = 0x02,
            SMCM = 0x03,
            SMD0 = 0x04,  // SMD0 01/32
            SMD1 = 0x04,  // SMD0 02/32
            SMD2 = 0x05,  // SMD0 03/32
            SMD3 = 0x06,  // SMD0 04/32
            SMD4 = 0x07,  // SMD0 05/32
            SMD5 = 0x08,  // SMD0 06/32
            SMD6 = 0x09,  // SMD0 07/32
            SMD7 = 0x0A,  // SMD0 08/32
            SMD8 = 0x0B,  // SMD0 09/32
            SMD9 = 0x0C,  // SMD0 10/32
            SMDA = 0x0D,  // SMD0 11/32
            SMDB = 0x0E,  // SMD0 12/32
            SMDC = 0x0F,  // SMD0 13/32
            SMDD = 0x10,  // SMD0 14/32
            SMDE = 0x11,  // SMD0 15/32
            SMDF = 0x12,  // SMD0 16/32
            SME0 = 0x13,  // SMD0 17/32
            SME1 = 0x14,  // SMD0 18/32
            SME2 = 0x15,  // SMD0 19/32
            SME3 = 0x16,  // SMD0 20/32
            SME4 = 0x17,  // SMD0 21/32
            SME5 = 0x18,  // SMD0 22/32
            SME6 = 0x19,  // SMD0 23/32
            SME7 = 0x1A,  // SMD0 24/32
            SME8 = 0x1B,  // SMD0 25/32
            SME9 = 0x1C,  // SMD0 26/32
            SMEA = 0x1E,  // SMD0 27/32
            SMEB = 0x1F,  // SMD0 28/32
            SMEC = 0x20,  // SMD0 29/32
            SMED = 0x21,  // SMD0 30/32
            SMEE = 0x22,  // SMD0 31/32
            SMEF = 0x23,  // SMD0 32/32
            BCNT = 0x24,
            SMAA = 0x25,
            BTPL = 0x30,  // Word together with BTPL
            BTPH = 0x31,  // Word together with BTPH
            BCLC = 0x32,
            ECL1 = 0x37,
            ECL2 = 0x38,
            ECL4 = 0x39,
            EL1R = 0x3A,
            EL2R = 0x3B,
            EL4R = 0x3C,
            RX3A = 0x40,  // Bit #0: SW2S, #3: ACCC, #4: TRPM
            RX4A = 0x41,  // Bit #0: W7OS, #1: QWOS, #3: SUSE, #4: RFLG
            RX2A = 0x42,  // Bit #1: CALS, #4: KBBL
            RX3B = 0x43,  // Bit #2: ACPS, #3: ACKY, #4: GFXT
            DSMB = 0x44,
            STRM = 0x4C,
            LIDE = 0x4E,
            RX4B = 0x50,  // Bit #2: PTHM, #4: S3CA, #5: DPTL, #6: IHEF
            ECLS = 0x52,
            CPHK = 0x53,
            EC45 = 0x55,
            HPTC = 0x5B,
            SHPM = 0x61,
            RX3C = 0x67,  // Bit #0: LDBG, #2: GC6R, #3: IGC6
            PLGS = 0x68,
            BXXX = 0x69,  // Bit #4: BCTF, #5: BMNF, #6: BTVD, #7: BF10
            GWKR = 0x6C,
            BADC = 0x70,  // Word together with BADD
            BADD = 0x71,  // Word together with BADC
            BFCC = 0x72,  // Word together with BFCD
            BFCD = 0x72,  // Word together with BFCC
            BVLB = 0x74,
            BVHB = 0x75,
            BDVO = 0x76,
            ECTB = 0x7F,
            MBST = 0x82,
            MCUR = 0x83,  // Word together with MCUS
            MCUS = 0x84,  // Word together with MCUR
            MBRM = 0x85,  // Word together with MBRN
            MBRN = 0x86,  // Word together with MBRM
            MBCV = 0x87,  // Word together with MBCW
            MBCW = 0x88,  // Word together with MBCV
            GPUT = 0x89,
            LEDM = 0x8B,
            MBFC = 0x8D,
            NVDO = 0x90,
            ECDO = 0x91,
            GSXX = 0x94,  // Bit #0: GSSU, #1: GSMS
            ADPX = 0xA3,
            RX2B = 0xA4,  // Bit #0: MBTS, #7: BACR
            MBDC = 0xA5,
            RX2C = 0xA7,  // Bit #0: ENWD, #1: TMPR
            SXXX = 0xAA,  // Bit #1: SMSZ, #2: SE1N, #3: SE2N, #4: SOIE, #7 RCDS
            SADP = 0xAD,
            EPWM = 0xB8,
            DPPC = 0xC1,
            SHXX = 0xC5,  // Bit #0: SHB1, #1: SHB2, #2: SHB3, #3: SHB4, #4: SHOK, #5: SHFL, #6: SHNP, #7: SHEN
            CVTS = 0xC6,
            CSFG = 0xCA,
            EBPL = 0xD0,
            S1A1 = 0xD2,
            S2A1 = 0xD3,
            PSHD = 0xD4,
            PSLD = 0xD5,
            DBPL = 0xD6,
            STSP = 0xD7,  
            PSIN = 0xDA,
            RX4C = 0xDB,  // Bit #0: PSKB0, #1: PSTP, #3: PWOL, #4: RTCE
            S1A0 = 0xDC,
            S2A0 = 0xDD,
            NVDX = 0xDE,
            ECDX = 0xDF,
            DLYT = 0xE0,
            DLY2 = 0xE1,
            KBT0 = 0xE2,  
            SFHK = 0xE6,
            DTMT = 0xE9,
            PL12 = 0xEA,
            ETMT = 0xEB,
            RX2D = 0xF0,  // Bit #0: PARS, #7: MUCR
            RX2E = 0xF2,  // Bit #0: ZPDD, #7: ENPA
            HDMI = 0xF7,
            NVDS = 0xF8
        }
#endregion

    }

}
