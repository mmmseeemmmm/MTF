using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace MTFClientServerCommon.Constants
{
    public static class BaseConstants
    {
        //common paths
        private static readonly string MTFDataRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"AutomotiveLighting\MTF");
        public static readonly string MTFVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();


        public const string GraphicalViewImageExtension = ".mtfimg";
        public const string SequenceExtension = ".sequence";
        public const string GraphicalViewScreenEx = ".png";
        public const string GoldSampleExtension = ".gs";
        public static readonly List<string> ImageExtensions = new List<string> { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff", ".tif" };

        public const string ExecutionStatusOk = "Execution_Status_Ok";
        public const string ExecutionStatusNok = "Execution_Status_Nok";
        public static readonly char[] IllegalFileNameCharacters = {'\0', '/', '\\', ':', '*', '?', '"', '<', '>', '|'};

        //server paths
        private static readonly string ServerDataRoot = Path.Combine(MTFDataRoot, "Server");

        public static readonly string ServerSettingsPath = Path.Combine(ServerDataRoot, "settings.xml");
        public static readonly string ServerBackupSettingsPath = Path.Combine(ServerDataRoot, "settings_backup.xml");

        public static readonly string ServerSystemLogsPath = Path.Combine(ServerDataRoot, "SystemLogs");

        public static readonly string DataPath = ServerDataRoot;
        public static readonly string ClassInstanceConfigBasePath = Path.Combine(MTFVersion, "FunctionalityLibrary");
        public static readonly string SequenceBasePath = Path.Combine(MTFVersion, "Sequences");
        public const string GraphicalViewSources = "GraphicSources";
        public static readonly string LogsBasePath = Path.Combine(MTFVersion, "Logs");
        public const string LogsDbFileName = "Reports.db";
        public const string ReportImageBasePath = "ImageResources";
        public const string ReportGraphicalViewBasePath = "GraphicalViewResources";
        public static string AssembliesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mtfLibs");
        public const string ClientControlAssemblyServerPath = "mtfClientUI";
        public static readonly string GoldSampleBasePath = Path.Combine(ServerDataRoot, @"gs");
        

        //client paths
        private static readonly string ClientDataRoot = Path.Combine(MTFDataRoot, "Client");
        private static readonly string ClientUserSettingsRoot = Path.Combine(ClientDataRoot, "UserSetting");

        public static readonly string ClientSettingsPath = Path.Combine(ClientDataRoot, "settings.xml");
        public static readonly string ClientBackupSettingsPath = Path.Combine(ClientDataRoot, "settings_backup.xml");

        public static readonly string ClientSystemLogsPath = Path.Combine(ClientDataRoot, "SystemLogs");

        public static readonly string BreakPointsLocation = Path.Combine(ClientUserSettingsRoot, "BreakPoint");
        public static readonly string BreakPointsExtension = ".breakpoint";

        public static readonly string SetupModeLocation = Path.Combine(ClientUserSettingsRoot, "Setup");
        public static readonly string SetupModeExtension = ".setup";

        public static readonly string ClientControlAssemblyClientCachePath = Path.Combine(ClientDataRoot, "mtfClientUICache");
    }
}
