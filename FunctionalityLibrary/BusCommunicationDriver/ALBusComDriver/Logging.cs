using System;

namespace ALBusComDriver
{
    public static class Logging
    {
        private static string logFilePrefixALRestbus = "ALRestbus";
        private static string nameLogDirectory = "ALUtilsLogs";

        public static string LogFilePrefixOffBoard = "OffBoard";
        public static string LogFilePrefixBusComServer = "BusComServer";
        public static string LogFilePrefixBusComClient = "BusComClient";

        public static string LogPath = AppDomain.CurrentDomain.BaseDirectory;
        
        public static void LogMessage(string message, string prefixFileName, bool logTimeStamp)
        {
            AL.Utils.Logging.Log.LogMessage(message, prefixFileName, logTimeStamp);
        }

        public static void SortFolder()
        {
            AL.Utils.Logging.Log.SortFolder(LogPath, LogFilePrefixOffBoard);
            AL.Utils.Logging.Log.SortFolder(LogPath, logFilePrefixALRestbus);
            AL.Utils.Logging.Log.SortFolder(LogPath, LogFilePrefixBusComServer);
            AL.Utils.Logging.Log.SortFolder(LogPath, LogFilePrefixBusComClient);
        }

        public static bool IsLoggingEnable()
        {
            return AL.Utils.Logging.Log.GetLoggingEnable();
        }
    }
}
