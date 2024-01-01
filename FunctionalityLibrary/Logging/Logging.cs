using System;
using System.Collections.Generic;
using System.Linq;
using AutomotiveLighting.MTFCommon;
using AutomotiveLighting.MTFCommon.Types;

namespace LoggingDriver
{
    [MTFClass(Icon = MTFIcons.Pencil)]
    [MTFClassCategory("Report")]
    public class Logging : IDisposable
    {
        LoggingControl loggingControl;

        [MTFConstructor]
        public Logging()
        {
            loggingControl = new LoggingControl();
        }

        [MTFMethod]
        [MTFAdditionalParameterInfo(ParameterName = "package", DisplayName = "Package of cDAQ data")]
        [MTFAdditionalParameterInfo(ParameterName = "separatedData", DisplayName = "Others data")]
        public void LogToCsvCDAQ(PackageMeasuredData package, List<MeasuredData> separatedData)
        {
            loggingControl.LogToCSV(package, separatedData);
        }

        [MTFMethod]
        [MTFAdditionalParameterInfo(ParameterName = "package1", DisplayName = "Package of cDAQ1 data")]
        [MTFAdditionalParameterInfo(ParameterName = "package2", DisplayName = "Package of cDAQ2 data")]
        [MTFAdditionalParameterInfo(ParameterName = "separatedData", DisplayName = "Others data")]
        public void LogToCsv2CDAQs(PackageMeasuredData package1, PackageMeasuredData package2, List<MeasuredData> separatedData)
        {
            package1 .Data= package1.Data.Concat(package2.Data).ToList();
            loggingControl.LogToCSV(package1, separatedData);
        }

        [MTFMethod]
        public void LogHeaderToCsv(string[] header)
        {
            loggingControl.LogToCSV(header);
        }

        [MTFMethod]
        public void LogDataToCsv(double[] data)
        {
            loggingControl.LogToCSV(data);
        }

        [MTFMethod]
        public void LogRawDataToCsv(string rawData)
        {
            loggingControl.LogToCSV(rawData);
        }

        [MTFMethod]
        [MTFAdditionalParameterInfo(ParameterName = "filename", DisplayName = "File name")]
        [MTFAdditionalParameterInfo(ParameterName = "appendIfExist", DisplayName = "Append if exists")]
        public void OpenLogfile(string filename, bool appendIfExist)
        {
            loggingControl.OpenLogFile(filename, appendIfExist);
        }

        [MTFMethod]
        [MTFAdditionalParameterInfo(ParameterName = "fileName", DisplayName = "File name")]
        public bool LogFileExists(string fileName)
        {
            return loggingControl.LogFileExist(fileName);
        }

        [MTFMethod]
        public void SetDestinationFolder(string folderPath, bool setDefault)
        {
            loggingControl.SetDestinationFolder(folderPath, setDefault);
        }

        public void Dispose()
        {
            if (loggingControl != null)
            {
                loggingControl.Close();
            }
            GC.SuppressFinalize(this);
        }

        ~Logging()
        {
            this.Dispose();
        }
    }
}
