using System;
using System.Globalization;
using System.IO.Ports;

namespace Printer
{
    class ZebraPrinter : IPrinter
    {
        public const string PlaceHolderDayOfYear = "{DayOfYear}";
        public const string PlaceHolderWeekOfYear = "{WeekOfYear}";
        public const string PlaceHolderDayOfWeek = "{DayOfWeek}";
        public const string PlaceHolderMonthOfYear = "{MonthOfYear}";
        public const string PlaceHolderYear = "{Year}";
        public const string PlaceHolderYearShort = "{YearShort}";
        public const string PlaceHolderDayOfMonth = "{DayOfMonth}";
        public const string PlaceHolderHour = "{Hour}";
        public const string PlaceHolderMinute = "{Minute}";
        public const string PlaceHolderSecond = "{Second}";
        public const string PlaceHolderTimeSeparator = "{TimeSeparator}";
        public const string PlaceHolderDateSeparator = "{DateSeparator}";
        public const string PlaceHolderDailyCounter = "{DailyCounter}";
        public const string PlaceHolderLine1 = "{Line1}";
        public const string PlaceHolderLine2 = "{Line2}";
        public const string PlaceHolderLine3 = "{Line3}";
        public const string PlaceHolderReference = "{Reference}";

        private SerialPort serialPort;
        private readonly string dateSeparator;
        private readonly string timeSeparator;
        private readonly string templateName;
        private readonly string dataMatrixTemplate;
        private readonly Calendar calendar;
        public ZebraPrinter(string portName, string templateName, string dateSeparator, string timeSeparator, string dataMatrixTemplate)
        {
            serialPort = new SerialPort(portName);
            this.templateName = templateName;
            this.dateSeparator = dateSeparator;
            this.timeSeparator = timeSeparator;
            this.dataMatrixTemplate = dataMatrixTemplate;
            calendar = CultureInfo.GetCultureInfo("en-US").Calendar;

            serialPort.Open();
        }

        public void Print(string line1, string line2, string line3, string reference, PermanentCounter counter)
        {
            var now = DateTime.Now;

            serialPort.WriteLine("^XA"); //start of message
            serialPort.WriteLine(string.Format("^XFE:{0}.ZPL^FS", templateName)); //XF = command E:=layot file address  ^FS close messade
            serialPort.WriteLine(string.Format("^FN1^FD{0}^FS", line1)); //first row 15 char max
            serialPort.WriteLine(string.Format("^FN2^FD{0}^FS", line2)); //second row 20 char
            serialPort.WriteLine(string.Format("^FN3^FD{0}^FS", line3)); //third row 20 char
            serialPort.WriteLine(string.Format("^FN4^FD{0}^FS", reference)); //reference al
            serialPort.WriteLine(string.Format("^FN5^FD{0} {1:00} {2}^FS", DayOfTheWeek(now), WeekOfYear(now), now.ToString("yy"))); //date formar day of week week of year year 2 digits
            serialPort.WriteLine(string.Format("^FN6^FD{0:00}{3}{1:00}{3}{2}^FS", now.Day, now.Month, now.Year, dateSeparator));// "^FN6^FD17/01/2019^FS"); //date dd/mm/yyyy
            serialPort.WriteLine(string.Format("^FN7^FD{0:00}{3}{1:00}{3}{2:00}^FS", now.Hour, now.Minute, now.Second, timeSeparator));// "^FN7^FD11:12:02^FS"); //hour hh:mm>ss
            serialPort.WriteLine(string.Format("^FN8^FD{0}^FS", FormatDataMatrix(now, line1, line2, line3, reference, counter))); //datamatrix data
            serialPort.WriteLine("^XZ"); //end of nmessage
        }

        private int DayOfTheWeek(DateTime date)
        {
            return (int)calendar.GetDayOfWeek(date);
        }

        private int WeekOfYear(DateTime date)
        {
            return calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        private string FormatDataMatrix(DateTime date, string line1, string line2, string line3, string reference, PermanentCounter counter)
        {
            string output = dataMatrixTemplate;
            if (output.Contains(PlaceHolderDailyCounter))
            {
                if (counter == null)
                {
                    throw new Exception("If you want use daily counter in datmatrix, you have to specify counter name");
                }
                output = output.Replace(PlaceHolderDailyCounter, counter.StringValue(date));
            }

            return output.Replace(PlaceHolderDayOfYear, date.DayOfYear.ToString("D3"))
                .Replace(PlaceHolderWeekOfYear, WeekOfYear(date).ToString("D2"))
                .Replace(PlaceHolderDayOfWeek, DayOfTheWeek(date).ToString())
                .Replace(PlaceHolderMonthOfYear, date.Month.ToString("D2"))
                .Replace(PlaceHolderYear, date.Year.ToString())
                .Replace(PlaceHolderYearShort, date.ToString("yy"))
                .Replace(PlaceHolderDayOfMonth, date.Day.ToString("D2"))
                .Replace(PlaceHolderHour, date.Hour.ToString("D2"))
                .Replace(PlaceHolderMinute, date.Minute.ToString("D2"))
                .Replace(PlaceHolderSecond, date.Second.ToString("D2"))
                .Replace(PlaceHolderTimeSeparator, timeSeparator)
                .Replace(PlaceHolderDateSeparator, dateSeparator)
                .Replace(PlaceHolderLine1, line1)
                .Replace(PlaceHolderLine2, line2)
                .Replace(PlaceHolderLine3, line3)
                .Replace(PlaceHolderReference, reference);
        }

        public void Dispose()
        {
            if (serialPort.IsOpen)
            {
                serialPort.Close();
            }
            serialPort.Dispose();
        }
    }
}
