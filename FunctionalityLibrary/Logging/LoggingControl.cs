using AutomotiveLighting.MTFCommon.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LoggingDriver
{
    class LoggingControl
    {
        public LoggingControl()
        {

        }

        public void Close()
        {
            if (sw != null)
            {
                sw.Close();
                sw.Dispose();
                sw = null;
            }
        }

        bool headerExists = false;
        List<string> csvFileOrganizer;
        System.IO.StreamWriter sw = null;
        string fullName;
        private bool setDestinationFolder;
        private string destinationFolder;

        public void LogToCSV(PackageMeasuredData package, List<MeasuredData> separatedData)
        {
            if ((package == null || package.Data == null || package.Data.Count == 0) && (separatedData == null || separatedData.Count == 0))
            {
                throw new Exception("No Data to log!");
            }

            string timeStamp = string.Empty;

            List<MeasuredData> dataTogether = new List<MeasuredData>();

            if (package != null && package.Data != null && package.Data.Count > 0)
            {
                foreach (MeasuredTypedData data in package.Data)
                {
                    dataTogether.Add(data);
                }

                timeStamp = package.Time.ToString();
            }
            else
            {
                timeStamp = AutomotiveLighting.MTFCommon.Types.MTFDateTime.SystemDateTimeToMTFDateTime(System.DateTime.Now).ToString();
            }


            if (separatedData != null)
            {
                foreach (MeasuredData data in separatedData)
                {
                    dataTogether.Add(data);
                }
            }

            if (headerExists)
            {
                logJustValues(dataTogether, timeStamp);
            }
            else
            {
                createFileAndHeaders(dataTogether, timeStamp);
            }
        }

        public void LogToCSV(IEnumerable<string> header)
        {
            logMessage(string.Join(";" ,header));
        }

        public void LogToCSV(IEnumerable<double> data)
        {
            logMessage(string.Join(";", data));
        }

        public void LogToCSV(string rawLine)
        {
            logMessage(rawLine);
        }

        internal void OpenLogFile(string filename, bool appendIfExist)
        {
            if (sw != null)
            {
                sw.Close();
                sw.Dispose();
                sw = null;
            }

            var directory = getDirectory();
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            fullName = Path.Combine(directory, filename);
            fullName = Path.ChangeExtension(fullName, "csv");

            if (!appendIfExist)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fullName);
                int i = 1;
                while (File.Exists(fullName))
                {
                    fullName = Path.Combine(directory, fileNameWithoutExtension + "-" + i);
                    fullName = Path.ChangeExtension(fullName, "csv");
                    i++;
                } 
            }

            sw = new StreamWriter(fullName, appendIfExist, Encoding.GetEncoding(1252));
            headerExists = false;
        }

        public bool LogFileExist(string fileName)
        {
            var directory = getDirectory();
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            fullName = Path.Combine(directory, fileName);
            fullName = Path.ChangeExtension(fullName, "csv");

            return File.Exists(fullName);
        }

        private void logMessage(string message)
        {
            if (sw == null)
            {
                string logFileName = "DataLogger_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm");
                OpenLogFile(logFileName, false);
            }

            sw.WriteLine(message);
            sw.Flush();
        }

        private void createFileAndHeaders(List<MeasuredData> dataTogether, string timeStamp)
        {
            csvFileOrganizer = new List<string>();
            csvFileOrganizer.Add("timestamp");
            string line1 = "timestamp;";
            string line2 = "[YYYY-MM-DD-HH-MM-SS.MS];";


            //headers
            if (dataTogether != null)
            {
                foreach (MeasuredData data in dataTogether)
                {
                    csvFileOrganizer.Add(data.Alias);
                    line1 += data.Alias + ";";
                    line2 += data.Unit + ";";
                }
            }

            logMessage(line1);
            logMessage(line2);

            headerExists = true;

            logJustValues(dataTogether, timeStamp);

        }

        private void logJustValues(List<MeasuredData> dataTogether, string timeStamp)
        {
            if (createNewFilePartIfHigherThan(50000000))
            {
                createFileAndHeaders(dataTogether, timeStamp);
                return;
            }
            string line = timeStamp + ";";
            string newAliases = string.Empty;
            string newUnits = string.Empty;
            List<int> newPossitions = new List<int>();

            if (dataTogether != null)
            {
                //check new aliasses
                bool areEmptyCellsAlreadyAdded = false;
                for (int i = 0; i < dataTogether.Count; i++)
                {
                    if (!csvFileOrganizer.Contains(dataTogether[i].Alias))
                    {
                        csvFileOrganizer.Add(dataTogether[i].Alias);
                        newPossitions.Add(i);
                        if (!areEmptyCellsAlreadyAdded)
                        {
                            for (int j = 0; j < csvFileOrganizer.Count - 1; j++)
                            {
                                newAliases += ";";
                                newUnits += ";";
                            }
                            areEmptyCellsAlreadyAdded = true;
                        }
                    }
                }

                //add new headers and units
                if (newPossitions.Count > 0)
                {
                    for (int i = 0; i < newPossitions.Count; i++)
                    {
                        newAliases += dataTogether[newPossitions[i]].Alias + ";";
                        newUnits += dataTogether[newPossitions[i]].Unit + ";";
                    }
                    logMessage(newAliases);
                    logMessage(newUnits);
                }

                //log all values
                foreach (string alias in csvFileOrganizer)
                {
                    if (alias == "timestamp")
                    {
                        continue;
                    }
                    bool finded = false;
                    foreach (MeasuredData data in dataTogether)
                    {
                        finded = false;
                        if (alias == data.Alias)
                        {
                            line += data.sValue + ";";
                            finded = true;
                            break;
                        }
                    }
                    if (!finded)
                    {
                        line += ";";
                    }
                }
                logMessage(line);
            }
        }

        private bool createNewFilePartIfHigherThan(long bytes)
        {
            FileInfo fileInfo = new FileInfo(fullName);

            if (fileInfo.Length > bytes)
            {
                string fileName = Path.GetFileNameWithoutExtension(fullName);
                if (fileName.Contains("_part"))
                {
                    string partNumber = fileName.Substring(fileName.IndexOf("_part"));
                    partNumber = partNumber.Substring(5);
                    int part = int.Parse(partNumber);
                    fileName = fileName.Replace("_part" + part.ToString(), "_part" + (part+1).ToString());
                }
                else
                {
                    fileName = fileName + "_part2";
                }
                OpenLogFile(fileName, false);
                return true;
            }
            return false;
        }

        private string getDirectory()
        {
            return setDestinationFolder && !string.IsNullOrEmpty(destinationFolder) ? destinationFolder : AppDomain.CurrentDomain.BaseDirectory + @"\data\logs\";
        }

        public void SetDestinationFolder(string folderPath, bool setDefault)
        {
            destinationFolder = folderPath;
            setDestinationFolder = !setDefault;
        }
    }
}
