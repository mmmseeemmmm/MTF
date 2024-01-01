using AutomotiveLighting.MTFCommon;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace AutomotiveLighting.MTF.GlobalDataContainer
{
    public enum LoggingStatus
    {
        ERROR,
        OK,
        WARNING
    };

    [MTFKnownClassAttribute]
    public class Logging
    {
        public bool _loggingActive = false;
        public string _logFilePrefix = "";
        public string _logFileAppendix = "";//"_GlobalDataContainer_";
        public string _logFileExtension = ".log";
        public string _logPath { set; get; }
        public string _logFile { set; get; }
        public int _maxLogFiles = 1;
        public int _maxLogFileSizeMB = 15;
        private string _compName = "";
        public Stopwatch _logStopWatch = new Stopwatch();
        public string _logStartTime = "";
        public string _logStopTime = "";

        public Logging(string ComponentName)
        {
            _compName = ComponentName;
            _logPath = @".\_gdclog\";
        }

        public void ActivateLogging(bool StartStopWatch = true, bool SetTimestamp = true)
        {
            _loggingActive = true;
            if (StartStopWatch) { _logStopWatch.Start(); }
            if (SetTimestamp)
            {
                _logStartTime = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss.fff");
                _logFilePrefix = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_";
                _logFile = _logFilePrefix + _compName + _logFileAppendix + _logFileExtension;
            }
        }

        public void DeactivateLogging(bool StopStopWatch = true)
        {
            if (StopStopWatch)
            {
                _logStopWatch.Stop();
                _logStopTime = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss.fff");
            }
            _loggingActive = false;
        }

        public void WriteToLog(string Function, LoggingStatus Status, string LogContent)
        {
            //create directory
            if (!Directory.Exists(_logPath)) { Directory.CreateDirectory(_logPath); }

            //sanity check
            if (!Directory.Exists(_logPath)) throw new Exception("Path to log file (" + _logPath + ") does not exist!");
            if (Path.GetFileName(_logFile) == "") throw new Exception("Log file name can not be empty!");

            //set full path of log file
            var _fullPath = Path.Combine(_logPath, _logFile);

            //check log file size
            //GetLogFileSize()
            if (File.Exists(_fullPath))
            {
                var size = new System.IO.FileInfo(_fullPath).Length;
                if (_maxLogFileSizeMB != 0)
                {
                    //check filesize
                    if (size >= (_maxLogFileSizeMB * 1000000))
                    {
                        #region TODO
                        //find and set next number (appendix)
                        var fullPath = Path.Combine(_logPath, _logFilePrefix + _compName + _logFileExtension);
                        _logFileAppendix = "_" + GetNextFileVersion(fullPath).ToString("D3");
                        //set new logfile name
                        _logFile = _logFilePrefix + _compName + _logFileAppendix + _logFileExtension;

                        //write to log file
                        File.AppendAllText(_fullPath, String.Format("{0}\t[{1}]\t[{2}]\t{3} -> MaxLogFiles: {4} | MaxLogFileSizeMB: {5}\n", DateTime.Now.ToString("yyyy.MM.dd\tHH:mm:ss.fff"), "OK", "Logging", "New logfile " + Path.GetFullPath(Path.Combine(_logPath, _logFile)) + " has been created", _maxLogFiles, _maxLogFileSizeMB));

                        //set complete new path/name
                        _fullPath = Path.Combine(_logPath, _logFile);

                        //check number of log files
                        if (_maxLogFiles > 0)
                        {
                            string[] s = Directory.GetFiles(_logPath).Where(x => x.Contains(_logFilePrefix + _compName)).ToArray();
                            if (s.Count() >= _maxLogFiles)//while -> all over _maxLogFiles are deleted
                            {
                                File.Delete(s[0]);
                                //write to log file
                                File.AppendAllText(_fullPath, String.Format("{0}\t[{1}]\t[{2}]\t{3} -> MaxLogFiles: {4} | MaxLogFileSizeMB: {5}\n", DateTime.Now.ToString("yyyy.MM.dd\tHH:mm:ss.fff"), "OK", "Logging", "Logfile " + Path.GetFullPath(s[0]) + " has been deleted", _maxLogFiles, _maxLogFileSizeMB));
                                s = Directory.GetFiles(_logPath).Where(x => x.Contains(_logFilePrefix + _compName)).ToArray();
                            }
                        }
                        #endregion
                    }


                }
            }

            //get current date and time
            var _dateTime = DateTime.Now.ToString("yyyy.MM.dd\tHH:mm:ss.fff");

            //write to log file
            File.AppendAllText(_fullPath, String.Format("{0}\t[{1}]\t[{2}]\t{3}\n", _dateTime, Enum.GetName(typeof(LoggingStatus), Status), Function, LogContent));
        }

        public string CompressLogfiles(bool DeleteCompressed)
        {
            var outp = MakeZipDirectory(_logPath + _logFilePrefix + _compName + _logFileExtension, DeleteCompressed);
            if (outp != "error") { _logFileAppendix = ""; }
            return outp;
        }

        internal int GetNextFileVersion(string FilePathName)
        {
            int nextVersion = 0;

            if (Directory.Exists(Path.GetDirectoryName(FilePathName)))
            {
                string[] _files = Directory.GetFiles(Path.GetDirectoryName(FilePathName));
                string _fileName = Path.GetFileNameWithoutExtension(FilePathName);
                string _fileExtension = Path.GetExtension(FilePathName);
                string _filePath = Path.GetDirectoryName(FilePathName);

                //find versions of file
                for (int i = 0; i < _files.Count(); i++)
                {
                    if (_files[i].Contains(_fileName + "_"))
                    {
                        bool isVersion;
                        int curVer;
                        //clean string
                        string numVer = _files[i].Replace(_fileName, string.Empty);
                        numVer = numVer.Replace(_fileExtension, string.Empty);
                        numVer = numVer.Replace(_filePath, string.Empty);
                        numVer = numVer.Replace("_", string.Empty);
                        numVer = numVer.Replace("\\", string.Empty);
                        //check if it is a version of file
                        isVersion = int.TryParse(numVer, out curVer);
                        if (isVersion && nextVersion <= curVer) { nextVersion = curVer + 1; }
                    }
                }
                if (nextVersion == 0) { nextVersion = 1; }
            }
            return nextVersion;
        }

        internal string MakeZipDirectory(string SourceFile, bool DeleteCompressed = false)
        {
            if (Directory.Exists(Path.GetDirectoryName(SourceFile)))
            {
                using (ZipArchive zip = System.IO.Compression.ZipFile.Open(SourceFile + ".zip", ZipArchiveMode.Update))
                {
                    foreach (var item in Directory.GetFiles(Path.GetDirectoryName(SourceFile)))
                    {
                        if (item.Contains(Path.GetFileNameWithoutExtension(SourceFile)) && Path.GetExtension(item) != ".zip")
                        {
                            if (_loggingActive) { WriteToLog("9 - Logging", LoggingStatus.OK, "Compress " + Path.GetFileName(item)); }
                            zip.CreateEntryFromFile(item, Path.GetFileName(item), CompressionLevel.Optimal);
                            if (DeleteCompressed) { File.Delete(item); }
                        }
                    }

                }
                return Path.GetFullPath(SourceFile) + ".zip";
            }
            if (_loggingActive) { WriteToLog("9 - Logging", LoggingStatus.ERROR, "Compression: directory (" + Path.GetDirectoryName(SourceFile) + ") does not exist!"); }
            return "error";
        }
    }
}
