using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;

namespace MTFClientServerCommon.Helpers
{
    public static class FileHelper
    {
        private static List<string> listProhibitedName = new List<string>
                                                         {
                                                             "CON", "PRN", "AUX", "NUL",
                                                             "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
                                                             "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
                                                         };

        public static bool IsCorrectFileName(string name)
        {
            return !listProhibitedName.Exists(x => x.Equals(name.ToUpper()));
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

        public static void CreateEmptyFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                return;
            }
 
            CreateDirectory(Path.GetDirectoryName(fileName));
            File.Create(fileName);
            SetFileForEveryone(fileName);
        }

        public static void SetFileForEveryone(string fileName) => File.SetAccessControl(fileName, SetAccessControl(File.GetAccessControl(fileName)));

        public static void SetDirectoryForEveryone(string directoryName) => Directory.SetAccessControl(directoryName, SetAccessControl(Directory.GetAccessControl(directoryName)));

        public static void MoveDirectory(string sourceDirName, string destDirName) => Directory.Move(sourceDirName, destDirName);

        public static void CopyDirectory(string sourceDirName, string destDirName) => CopyDirectory(sourceDirName, destDirName, true);

        public static void CopyDirectory(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(sourceDirName);
            }

            CreateDirectory(destDirName);

            DirectoryInfo[] dirs = dir.GetDirectories();

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string destFileName = Path.Combine(destDirName, file.Name);
                file.CopyTo(destFileName, false);
                SetFileForEveryone(destFileName);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    CopyDirectory(subdir.FullName, Path.Combine(destDirName, subdir.Name), copySubDirs);
                }
            }
        }

        public static string GetRelativePath(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(fromPath))
            {
                throw new ArgumentNullException("fromPath");
            }

            if (string.IsNullOrEmpty(toPath))
            {
                throw new ArgumentNullException("toPath");
            }

            Uri fromUri = new Uri(AppendDirectorySeparatorChar(fromPath));
            Uri toUri = new Uri(AppendDirectorySeparatorChar(toPath));

            if (fromUri.Scheme != toUri.Scheme)
            {
                return toPath;
            }

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (string.Equals(toUri.Scheme, Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }

        private static T SetAccessControl<T>(T security) where T : FileSystemSecurity
        {
            security.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, AccessControlType.Allow));
            return security;
        }

        private static string AppendDirectorySeparatorChar(string path)
        {
            // Append a slash only if the path is a directory and does not have a slash.
            if (!Path.HasExtension(path) &&
                !path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                return path + Path.DirectorySeparatorChar;
            }

            return path;
        }
    }
}
