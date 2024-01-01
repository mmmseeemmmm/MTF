using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualBasic.FileIO;
using MTFClientServerCommon;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.Helpers;

namespace MTFCore
{
    public static class FileOperation
    {
        public static string CreateNewFolder(string path, string newDirName, bool useDefaultMtfDirectory)
        {
            var targetDirectory = useDefaultMtfDirectory ? Path.Combine(BaseConstants.DataPath, BaseConstants.SequenceBasePath, path) : path;
            var parentDirectory = new DirectoryInfo(targetDirectory);
            if (parentDirectory.Exists)
            {
                bool repeat;
                int i = 1;
                var subDirs = parentDirectory.GetDirectories();
                do
                {
                    repeat = false;
                    if (subDirs.Any(x => x.Name == newDirName))
                    {
                        repeat = true;
                        newDirName = $"{newDirName} ({i++})";
                    }
                }
                while (repeat);
            }
            parentDirectory.CreateSubdirectory(newDirName);
            return newDirName;
        }

        public static void CreateDirectory(string dirName) => FileHelper.CreateDirectory(Path.Combine(BaseConstants.DataPath, dirName));

        public static string RenameItem(string newName, string oldFullName, string root, bool useDefaultMtfDirectory)
        {
            if (string.IsNullOrEmpty(oldFullName))
            {
                return "Bad old name.";
            }

            if (newName.Replace(" ", string.Empty) == string.Empty)
            {
                return "Enter the name.";
            }

            if (BaseConstants.IllegalFileNameCharacters.Any(newName.Contains))
            {
                return "Illegal character in name";
            }

            bool isDirectory = false;
            var fileExtension = Path.GetExtension(oldFullName);
            var oldName = Path.GetFileName(oldFullName);
            if (!string.IsNullOrEmpty(fileExtension) && !newName.EndsWith(fileExtension))
            {
                newName = $"{newName}{fileExtension}";
            }
            string completeOldPath = useDefaultMtfDirectory ? Path.Combine(BaseConstants.DataPath, root, oldFullName) : oldFullName;
            DirectoryInfo parentDirectory = Directory.GetParent(completeOldPath);
            if (Directory.Exists(completeOldPath))
            {
                isDirectory = true;
            }
            string newFullPath = Path.Combine(parentDirectory.FullName, newName);
            if (!Directory.Exists(newFullPath) && !File.Exists(newFullPath))
            {
                string oldPath = Path.Combine(parentDirectory.FullName, oldName);
                try
                {
                    if (isDirectory)
                    {
                        Directory.Move(oldPath, newFullPath);
                    }
                    else
                    {
                        File.Move(oldPath, newFullPath);
                    }
                    return string.Empty;
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
            else
            {
                if (newName == oldName)
                {
                    return string.Empty;
                }
                return "Directory or file already exists.";
            }
        }

        public static void RemoveFile(string name, string root, bool useDefaultMtfDirectory)
        {
            var path = useDefaultMtfDirectory ? Path.Combine(BaseConstants.DataPath, root, name) : name;
            if (File.Exists(path))
            {
                try
                {
                    FileSystem.DeleteFile(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                }
                catch (Exception ex)
                {
                    SystemLog.LogException(ex);
                }
            }
        }

        public static void RemoveDirectory(string name, string root, bool useDefaultMtfDirectory)
        {
            var path = useDefaultMtfDirectory ? Path.Combine(BaseConstants.DataPath, root, name) : name;
            if (Directory.Exists(path))
            {
                try
                {
                    FileSystem.DeleteDirectory(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                }
                catch (Exception ex)
                {
                    SystemLog.LogException(ex);
                }
            }
        }

        public static bool ExistFile(string fullName) => File.Exists(Path.Combine(BaseConstants.DataPath, fullName));

        public static bool ExistDirectory(string fullName) => Directory.Exists(Path.Combine(BaseConstants.DataPath, fullName));

        public static List<MTFPersistDataInfo> GetAllServerItems(string path, bool getFiles)
        {
            var getDrives = string.IsNullOrEmpty(path);
            var items = new List<MTFPersistDataInfo>();

            if (getDrives)
            {
                try
                {
                    return DriveInfo.GetDrives().Where(x => x.IsReady).Select(x => new MTFPersistDataInfo
                    {
                        Name = x.Name,
                        Type = GetServerItemType(x.DriveType)
                    }).ToList();
                }
                catch (Exception)
                {
                    return items;
                }
            }


            var directory = new DirectoryInfo(path);

            if (directory.Exists)
            {
                try
                {
                    items.AddRange(directory.GetDirectories().Select(x => new MTFPersistDataInfo { Name = x.Name, Type = MTFDialogItemTypes.Folder }));
                }
                catch (Exception)
                {
                    // ignored
                }
                if (getFiles)
                {
                    try
                    {
                        items.AddRange(directory.GetFiles().Select(x => new MTFPersistDataInfo { Name = x.Name, Type = MTFDialogItemTypes.File }));
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }

            return items;
        }

        private static MTFDialogItemTypes GetServerItemType(DriveType driveType)
        {
            switch (driveType)
            {
                case DriveType.Removable:
                    return MTFDialogItemTypes.RemovableDrive;
                case DriveType.Fixed:
                    return MTFDialogItemTypes.LocalDrive;
                case DriveType.Network:
                    return MTFDialogItemTypes.NetworkDrive;
                case DriveType.CDRom:
                    return MTFDialogItemTypes.CD;
                default:
                    return MTFDialogItemTypes.File;
            }
        }

        public static string GetServerFullDirectoryPath(string relativePath)
        {
            var path = Path.Combine(BaseConstants.DataPath, relativePath);
            var di = new DirectoryInfo(path);
            return di.Exists ? di.FullName : null;
        }

        public static void SaveData(object data, string fullPath)
        {
            var formatter = new BinaryFormatter();

            using (var fs = new FileStream(fullPath, FileMode.Create))
            {
                formatter.Serialize(fs, data);
            }

            FileHelper.SetFileForEveryone(fullPath);
        }

        public static T GetData<T>(string fullPath) where T : class
        {
            var formatter = new BinaryFormatter();

            using (var fs = new FileStream(fullPath, FileMode.Open))
            {
                return formatter.Deserialize(fs) as T;
            }
        }
    }
}
