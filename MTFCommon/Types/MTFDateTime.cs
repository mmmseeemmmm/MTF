using System;

namespace AutomotiveLighting.MTFCommon.Types
{
    [MTFKnownClass]
    public class MTFDateTime
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Second { get; set; }
        public int Milisecond { get; set; }

        public static MTFDateTime SystemDateTimeToMTFDateTime(DateTime sysTime)
        {
            MTFDateTime mtfTime = new MTFDateTime();

            mtfTime.Year = sysTime.Year;
            mtfTime.Month = sysTime.Month;
            mtfTime.Day = sysTime.Day;
            mtfTime.Hour = sysTime.Hour;
            mtfTime.Minute = sysTime.Minute;
            mtfTime.Second = sysTime.Second;
            mtfTime.Milisecond = sysTime.Millisecond;

            return mtfTime;
        }

        public static MTFDateTime Now()
        {
            return SystemDateTimeToMTFDateTime(DateTime.Now);
        }

        public static double GetRelativeTime(MTFDateTime baseTime, MTFDateTime actualTime)
        {
            DateTime zeroTime=new DateTime(baseTime.Year, baseTime.Month, baseTime.Day,baseTime.Hour, baseTime.Minute,baseTime.Second, baseTime.Milisecond);
            DateTime currTime = new DateTime(actualTime.Year, actualTime.Month, actualTime.Day, actualTime.Hour, actualTime.Minute, actualTime.Second, actualTime.Milisecond);
            TimeSpan diff = currTime - zeroTime;
            
            //
            //int baseYearDays;
            //int actualYearDays;
            //if ((baseTime.Year % 4 == 0) && (baseTime.Year % 100 != 0) || (baseTime.Year % 400 == 0))
            //{
            //    baseYearDays=266;
            //}
            //else
            //{
            //    baseYearDays = 265;
            //}
            //if ((actualTime.Year % 4 == 0) && (actualTime.Year % 100 != 0) || (actualTime.Year % 400 == 0))
            //{
            //    actualYearDays = 266;
            //}
            //else
            //{
            //    actualYearDays = 265;
            //}
            //double relTime = (actualTime.Year * actualYearDays - baseTime.Year * baseYearDays) * 24;
            //return SystemDateTimeToMTFDateTime(DateTime.Now);
            return diff.TotalHours;
        }

        public override string ToString()
        {
            return string.Format("{0}-{1:00}-{2:00} {3:00}:{4:00}:{5:00}{6}{7:000} ",
                Year,Month,Day,Hour,Minute,Second,System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator,Milisecond);
        }
    }
}
