Write-Host ""
Write-Host "Move MTF user data"
Write-Host "------------------"

$Assem = (
"System"
)
$Source = @"

using System;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Security.AccessControl;

namespace AutomotiveLighting.Tools 
{ 
    public static class MoveUserDataHelper
    {
        private static readonly string MtfVersion = FileVersionInfo.GetVersionInfo(Path.Combine(Environment.CurrentDirectory, ServerCommonPath)).FileVersion;

        private static readonly string MTFDataRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"AutomotiveLighting\MTF");
        private static readonly string OldMTFDataRoot = Environment.CurrentDirectory;

        private static readonly string ClientDataRoot = Path.Combine(MTFDataRoot, "Client");
        private static readonly string OldClientDataRoot = Path.Combine(OldMTFDataRoot, "Client");

        private static readonly string ClientSettings = Path.Combine(ClientDataRoot, "settings.xml");
        private static readonly string OldClientSettings = Path.Combine(OldClientDataRoot, "settings.xml");

        private static readonly string ClientUserSettingsRoot = Path.Combine(ClientDataRoot, "UserSetting");
        private static readonly string OldClientUserSettingsRoot = Path.Combine(OldClientDataRoot, "UserSetting");


        private static readonly string ServerDataRoot = Path.Combine(MTFDataRoot, "Server");
        private static readonly string OldServerDataRoot = Path.Combine(OldMTFDataRoot, "Server");

        private static readonly string ServerSettings = Path.Combine(ServerDataRoot, "settings.xml");
        private static readonly string OldServerSettings = Path.Combine(OldServerDataRoot, "settings.xml");

        private static readonly string ServerFunctionalityLibrary = Path.Combine(ServerDataRoot, MtfVersion, "FunctionalityLibrary");
        private static readonly string OldServerFunctionalityLibrary = Path.Combine(OldServerDataRoot, "data", "FunctionalityLibrary");

        private static readonly string ServerSequences = Path.Combine(ServerDataRoot, MtfVersion, "Sequences");
        private static readonly string OldServerSequences = Path.Combine(OldServerDataRoot, "data", "Sequences");

        private static readonly string ServerLogs = Path.Combine(ServerDataRoot, MtfVersion, "Logs");
        private static readonly string OldServerLogs = Path.Combine(OldServerDataRoot, "data", "Logs");

        private static readonly string ServerGraphicSources = Path.Combine(ServerDataRoot, "GraphicSources");
        private static readonly string OldServerGraphicSources = Path.Combine(OldServerDataRoot, "data", "GraphicSources");

        private static readonly string ServerImageResources = Path.Combine(ServerDataRoot, "ImageResources");
        private static readonly string OldServerImageResources = Path.Combine(OldServerDataRoot, "data", "ImageResources");

        private static readonly string ServerGraphicalViewResources = Path.Combine(ServerDataRoot, "GraphicalViewResources");
        private static readonly string OldServerGraphicalViewResources = Path.Combine(OldServerDataRoot, "data", "GraphicalViewResources");

        private static readonly string ServerGS = Path.Combine(ServerDataRoot, "gs");
        private static readonly string OldServerGS = Path.Combine(OldServerDataRoot, "gs");

        private const string ServerCommonPath = "Server\\MTFCommon.dll";

        public static void Work()
        {
          Console.WriteLine(Environment.CurrentDirectory);

          Console.WriteLine("MTF version: " + MtfVersion);

          Console.WriteLine("Copy client data");
          Console.WriteLine("");
          CopyClientData();
          Console.WriteLine("");

          Console.WriteLine("Copy server data");
          Console.WriteLine("");
          CopyServerData();
          Console.WriteLine("");

        }

        private static void CopyClientData()
        {
            CreateDirectory(ClientDataRoot);

            CopyFile(OldClientSettings, ClientSettings);
            CopyDirectory(OldClientUserSettingsRoot, ClientUserSettingsRoot, true);
        }

        private static void CopyServerData()
        {
            CreateDirectory(ServerDataRoot);

            CopyFile(OldServerSettings, ServerSettings);
            CopyDirectory(OldServerFunctionalityLibrary, ServerFunctionalityLibrary, true);
            CopyDirectory(OldServerSequences, ServerSequences, true);
            CopyDirectory(OldServerLogs, ServerLogs, true);
            CopyDirectory(OldServerGraphicSources, ServerGraphicSources, true);
            CopyDirectory(OldServerImageResources, ServerImageResources, true);
            CopyDirectory(OldServerGraphicalViewResources, ServerGraphicalViewResources, true);
            CopyDirectory(OldServerGS, ServerGS, true);
        }

        public static void CreateDirectory(string directoryName)
        {
            if (!Directory.Exists(directoryName))
            {
                string[] pathParts = directoryName.Split('\\');

                for (int i = 0; i < pathParts.Length; i++)
                {
                    // Correct part for drive letters
                    if (i == 0 && pathParts[i].Contains(":"))
                    {
                        pathParts[i] = pathParts[i] + "\\";
                    }
                    if (i > 0)
                    {
                        pathParts[i] = Path.Combine(pathParts[i - 1], pathParts[i]);
                    }
                    if (!Directory.Exists(pathParts[i]))
                    {
                        Directory.CreateDirectory(pathParts[i]);
                        SetDirectoryForEveryone(pathParts[i]);
                    }
                }
            }
        }

        public static void CopyFile(string sourceFileName, string destFileName)
        {
            Console.Write(sourceFileName + " -> " + destFileName + "...");
            try
            {
                FileInfo fi = new FileInfo(sourceFileName);
                fi.CopyTo(destFileName, false);
                SetFileForEveryone(destFileName);

                Console.WriteLine("OK");
            }
            catch(Exception e)
            {
                Console.WriteLine("Fail: " + e.Message);
            }
        }

        public static void CopyDirectory(string sourceDirName, string destDirName, bool copySubDirs)
        {
            try
            {

                Console.Write(sourceDirName + " -> " + destDirName + "...");
                // Get the subdirectories for the specified directory.
                DirectoryInfo dir = new DirectoryInfo(sourceDirName);
                
                CreateDirectory(destDirName);

                if (!dir.Exists)
                {
                    Console.WriteLine("Directory not found");
                    return;
                }

                DirectoryInfo[] dirs = dir.GetDirectories();

                // Get the files in the directory and copy them to the new location.
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    string destFileName = Path.Combine(destDirName, file.Name);
                    file.CopyTo(destFileName, false);
                    SetFileForEveryone(destFileName);
                }

                Console.WriteLine("OK " + files.Count() + " file(s) copied.");

                // If copying subdirectories, copy them and their contents to new location.
                if (copySubDirs)
                {
                    foreach (DirectoryInfo subdir in dirs)
                    {
                        CopyDirectory(subdir.FullName, Path.Combine(destDirName, subdir.Name), copySubDirs);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Fail: " + e.Message);
            }
        }

        public static void SetDirectoryForEveryone(string directoryName) 
        {
            Directory.SetAccessControl(directoryName, SetAccessControl(Directory.GetAccessControl(directoryName)));
        }

        public static void SetFileForEveryone(string fileName)
        {
            File.SetAccessControl(fileName, SetAccessControl(File.GetAccessControl(fileName)));
        }

        private static T SetAccessControl<T>(T security) where T : FileSystemSecurity
        {
            security.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, AccessControlType.Allow));
            return security;
        }
    }
} 
"@

Add-Type -ReferencedAssemblies $Assem -TypeDefinition $Source -Language CSharp

[AutomotiveLighting.Tools.MoveUserDataHelper]::Work()

