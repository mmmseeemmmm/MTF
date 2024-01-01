using System;
using AutomotiveLighting.MTFCommon;

namespace Printer
{
    [MTFClass(Icon = MTFIcons.Printer)]
    [MTFClassCategory("Printer")]
    public class Printer : IDisposable
    {
        private readonly IPrinter printer;
        private readonly PermanentCounter counter;

        [MTFConstructor(Description = "Zebra printer connected on serial port")]
        [MTFAdditionalParameterInfo(ParameterName = "portName", Description = "Serial port name like COM1 ...")]
        [MTFAdditionalParameterInfo(ParameterName = "templateName", Description = "Name of tempalte stored in printer. Name without .ZPL.")]
        [MTFAdditionalParameterInfo(ParameterName = "dateSeparator", DefaultValue = "/", Description = "Separator used by rendering date string")]
        [MTFAdditionalParameterInfo(ParameterName = "timeSeparator", DefaultValue = ":", Description = "Separator used by rendering time string")]
        [MTFAdditionalParameterInfo(ParameterName = "dataMatrixTemplate", DefaultValue = "{DayOfMonth}{DateSeparator}{MonthOfYear}{DateSeparator}{YearShort}-{Hour}{TimeSeparator}{Minute}{TimeSeparator}{Second}-{DailyCounter}", 
            Description = "Template for datamatrix with following placeholders\n" +
                        " • " + ZebraPrinter.PlaceHolderDayOfYear + "\tDay of year - 3 digits\n" +
                        " • " + ZebraPrinter.PlaceHolderWeekOfYear + "\tWeek of year - 2 digits\n" +
                        " • " + ZebraPrinter.PlaceHolderDayOfWeek + "\tDay of week - 1 digits\n" +
                        " • " + ZebraPrinter.PlaceHolderMonthOfYear + "\tMonth of year - 2 digits\n" +
                        " • " + ZebraPrinter.PlaceHolderYear + "\t\tYear - 4 digits\n" +
                        " • " + ZebraPrinter.PlaceHolderYearShort + "\tYear - 2 digits\n" +
                        " • " + ZebraPrinter.PlaceHolderDayOfMonth + "\tDay of month - 2 digits\n" +
                        " • " + ZebraPrinter.PlaceHolderHour + "\t\tHour - 2 digits\n" +
                        " • " + ZebraPrinter.PlaceHolderMinute + "\tMinute - 2 digits\n" +
                        " • " + ZebraPrinter.PlaceHolderSecond + "\tSecond - 2 digits\n" +
                        " • " + ZebraPrinter.PlaceHolderTimeSeparator + "\tTime separator\n" +
                        " • " + ZebraPrinter.PlaceHolderDateSeparator + "\tDate separator\n" +
                        " • " + ZebraPrinter.PlaceHolderDailyCounter + "\tDaily counter reseted on midnight - 5 digits\n" +
                        " • " + ZebraPrinter.PlaceHolderLine1 + "\t\tLine1\n" +
                        " • " + ZebraPrinter.PlaceHolderLine2 + "\t\tLine2\n" +
                        " • " + ZebraPrinter.PlaceHolderLine3 + "\t\tLine3\n" +
                        " • " + ZebraPrinter.PlaceHolderReference + "\tReference\n"
                        )]
        [MTFAdditionalParameterInfo(ParameterName = "permanentCounterName", Description = "Permanent counter name is used for unique identification of persistent counter data.")]
        [MTFAdditionalParameterInfo(ParameterName = "permanentCounterStringLength", DefaultValue = "5", Description = "Length of string representation of permanent counter value.")]
        public Printer(string portName, string templateName, string dateSeparator, string timeSeparator, string dataMatrixTemplate, string permanentCounterName, int permanentCounterStringLength)
        {
            printer = new ZebraPrinter(portName, templateName, dateSeparator, timeSeparator, dataMatrixTemplate);
            if (!string.IsNullOrEmpty(permanentCounterName))
            {
                counter = new PermanentCounter(permanentCounterName, permanentCounterStringLength);
            }
        }

        [MTFMethod]
        public void Print(string line1, string line2, string line3, string reference)
        {
            printer.Print(line1, line2, line3, reference, counter);
        }

        public void Dispose()
        {
            printer.Dispose();
        }
    }
}
