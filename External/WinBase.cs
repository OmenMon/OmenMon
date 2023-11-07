  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using System.Runtime.InteropServices;

namespace OmenMon.External {

    // Windows Base API Data Structures
    // Used by more than one import
    public class WinBase {

#region Windows Base API Data
        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        public struct SYSTEMTIME : IConvertible {
            public ushort Year;
            public ushort Month;
            public ushort DayOfWeek;
            public ushort Day;
            public ushort Hour;
            public ushort Minute;
            public ushort Second;
            public ushort Milliseconds;

            public static readonly SYSTEMTIME MinValue, MaxValue;

            static SYSTEMTIME() {
                MinValue = new SYSTEMTIME(1601, 1, 1);
                MaxValue = new SYSTEMTIME(30827, 12, 31, 23, 59, 59, 999);
            }

            public SYSTEMTIME(
                ushort year,
                ushort month,
                ushort day,
                ushort hour = 0,
                ushort minute = 0,
                ushort second = 0,
                ushort millisecond = 0) {

                Year = year;
                Month = month;
                Day = day;
                Hour = hour;
                Minute = minute;
                Second = second;
                Milliseconds = millisecond;
                DayOfWeek = 0;

            }

            public SYSTEMTIME(DateTime dateTime) {
                dateTime = dateTime.ToLocalTime();
                Year = Convert.ToUInt16(dateTime.Year);
                Month = Convert.ToUInt16(dateTime.Month);
                DayOfWeek = Convert.ToUInt16(dateTime.DayOfWeek);
                Day = Convert.ToUInt16(dateTime.Day);
                Hour = Convert.ToUInt16(dateTime.Hour);
                Minute = Convert.ToUInt16(dateTime.Minute);
                Second = Convert.ToUInt16(dateTime.Second);
                Milliseconds = Convert.ToUInt16(dateTime.Millisecond);
            }

            public static implicit operator DateTime(SYSTEMTIME systemTime) {

                if (systemTime.Year == 0 || systemTime == MinValue)
                    return DateTime.MinValue;

                if (systemTime == MaxValue)
                    return DateTime.MaxValue;

                return new DateTime(
                    systemTime.Year, systemTime.Month, systemTime.Day,
                    systemTime.Hour, systemTime.Minute, systemTime.Second, systemTime.Milliseconds,
                    DateTimeKind.Local);

            }

            public static implicit operator SYSTEMTIME(DateTime dateTime) => new SYSTEMTIME(dateTime);

            public static bool operator ==(SYSTEMTIME systemTime1, SYSTEMTIME systemTime2) => (
                systemTime1.Year == systemTime2.Year 
                && systemTime1.Month == systemTime2.Month
                && systemTime1.Day == systemTime2.Day
                && systemTime1.Hour == systemTime2.Hour
                && systemTime1.Minute == systemTime2.Minute
                && systemTime1.Second == systemTime2.Second
                && systemTime1.Milliseconds == systemTime2.Milliseconds);

            public static bool operator !=(SYSTEMTIME systemTime1, SYSTEMTIME systemTime2) => !(systemTime1 == systemTime2);

            public override bool Equals(object input) {

                if(input is SYSTEMTIME)
                    return ((SYSTEMTIME) input) == this;

                if(input is DateTime)
                    return ((DateTime) this).Equals(input);

                return base.Equals(input);

            }

            public override int GetHashCode() => ((DateTime) this).GetHashCode();
            public override string ToString() => ((DateTime) this).ToString();

            TypeCode IConvertible.GetTypeCode() => ((IConvertible) (DateTime) this).GetTypeCode();

            bool     IConvertible.ToBoolean(IFormatProvider provider)  => ((IConvertible) (DateTime) this).ToBoolean(provider);
            byte     IConvertible.ToByte(IFormatProvider provider)     => ((IConvertible) (DateTime) this).ToByte(provider);
            char     IConvertible.ToChar(IFormatProvider provider)     => ((IConvertible) (DateTime) this).ToChar(provider);
            DateTime IConvertible.ToDateTime(IFormatProvider provider) => (DateTime) this;
            decimal  IConvertible.ToDecimal(IFormatProvider provider)  => ((IConvertible) (DateTime) this).ToDecimal(provider);
            double   IConvertible.ToDouble(IFormatProvider provider)   => ((IConvertible) (DateTime) this).ToDouble(provider);
            short    IConvertible.ToInt16(IFormatProvider provider)    => ((IConvertible) (DateTime) this).ToInt16(provider);
            int      IConvertible.ToInt32(IFormatProvider provider)    => ((IConvertible) (DateTime) this).ToInt32(provider);
            long     IConvertible.ToInt64(IFormatProvider provider)    => ((IConvertible) (DateTime) this).ToInt64(provider);
            sbyte    IConvertible.ToSByte(IFormatProvider provider)    => ((IConvertible) (DateTime) this).ToSByte(provider);
            float    IConvertible.ToSingle(IFormatProvider provider)   => ((IConvertible) (DateTime) this).ToSingle(provider);
            string   IConvertible.ToString(IFormatProvider provider)   => ((IConvertible) (DateTime) this).ToString(provider);
            ushort   IConvertible.ToUInt16(IFormatProvider provider)   => ((IConvertible) (DateTime) this).ToUInt16(provider);
            uint     IConvertible.ToUInt32(IFormatProvider provider)   => ((IConvertible) (DateTime) this).ToUInt32(provider);
            ulong    IConvertible.ToUInt64(IFormatProvider provider)   => ((IConvertible) (DateTime) this).ToUInt64(provider);

            object IConvertible.ToType(Type conversionType, IFormatProvider provider) => ((IConvertible) (DateTime) this).ToType(conversionType, provider);

        }
#endregion

    }

}
