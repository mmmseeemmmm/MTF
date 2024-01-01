using MTFClientServerCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using MTFClientServerCommon.Constants;
using System.ServiceModel;
using MTFClientServerCommon.Helpers;

namespace MTFCore
{
    public class MTFPersistToBin
    {
        private const string saveMsg = "File was not saved correctly. Rollback has been executed. Please check a file destination.";

        public List<T> LoadDataList<T>(string fileName) where T : MTFPersist
        {
            string fullFileName = Path.Combine(BaseConstants.DataPath, fileName);

            if (!File.Exists(fullFileName))
            {
                return new List<T>();
            }

            BinaryFormatter formatter = new BinaryFormatter();

            FileStream reader = new FileStream(fullFileName, FileMode.Open);
            List<T> data;
            try
            {
                data = formatter.Deserialize(reader) as List<T>;
            }
            catch (Exception ex)
            {
                SystemLog.LogException(ex);
                return new List<T>();
            }
            finally
            {
                reader.Close();
                reader.Dispose();
            }

            data.ForEach(item => { item.IsNew = false; item.IsModified = false; });

            return data;
        }

        public Stream GetFileStream(string fileName)
        {
            string fullFileName = Path.Combine(BaseConstants.DataPath, fileName);

            if (!File.Exists(fullFileName))
            {
                throw new FaultException($"Could not find the sequnece: {fileName}");
            }

            return new FileStream(fullFileName, System.IO.FileMode.Open);
        }

        public T LoadData<T>(string fileName) where T : MTFPersist
        {
            string fullFileName = Path.Combine(BaseConstants.DataPath, fileName);

            if (!File.Exists(fullFileName))
            {
                return null;// Activator.CreateInstance<T>();
            }

            BinaryFormatter formatter = new BinaryFormatter();
            FileStream reader = new FileStream(fullFileName, FileMode.Open);
            T data;
            try
            {
                data = formatter.Deserialize(reader) as T;
            }
            catch (Exception ex)
            {
                SystemLog.LogException(ex);
                return Activator.CreateInstance<T>();
            }
            finally
            {
                reader.Close();
                reader.Dispose();
            }
            data.IsNew = false;
            data.IsModified = false;

            return data;
        }

        public void SaveData<T>(List<T> objectToPersist, string fileName) where T : MTFPersist
        {
            string backupFileName = null;
            string fullFileName = null;

            try
            {
                fullFileName = Path.Combine(BaseConstants.DataPath, fileName);

                if (objectToPersist == null || objectToPersist.Count < 1)
                {
                    //nothign to save -> delete file.
                    SystemLog.LogMessage($"List of objects to persist is empty. File to delete: {fullFileName}", true);
                    throw new Exception($"Saving was not done correctly because object to persist is empty. ({fullFileName})");
                }

                backupFileName = CreateBackupFile(fullFileName);

                BinaryFormatter formatter = new BinaryFormatter();
                string name = Path.GetFileName(fullFileName);
                string path = fullFileName.Remove(fullFileName.Length - name.Length);

                FileHelper.CreateDirectory(path);

                using (FileStream writer = new FileStream(fullFileName, System.IO.FileMode.OpenOrCreate))
                {
                    formatter.Serialize(writer, objectToPersist);
                    writer.Close();
                }

                FileHelper.SetFileForEveryone(fullFileName);

                RemoveBackupFile(backupFileName);
            }
            catch (Exception ex)
            {
                SystemLog.LogException(ex);
                FileRollBack(backupFileName, fullFileName);
                throw new System.ServiceModel.FaultException($"{saveMsg}{Environment.NewLine}{ex.Message}");
            }
        }


        public void SaveData<T>(T objectToPersist, string fileName) where T : MTFPersist
        {
            string backupFileName = null;
            string fullFileName = null;

            try
            {
                fullFileName = Path.Combine(BaseConstants.DataPath, fileName);

                if (objectToPersist == null)
                {
                    return;
                }

                if (objectToPersist.IsDeleted)
                {
                    SystemLog.LogMessage($"Object to persist is set as deleted. File to delete: {fullFileName}, Type of object: {objectToPersist.GetType()}", true);
                    throw new Exception($"Saving was not done correctly because object to persist is set as deleted. ({fullFileName})");
                }

                backupFileName = CreateBackupFile(fullFileName);

                BinaryFormatter formatter = new BinaryFormatter();
                string name = Path.GetFileName(fullFileName);
                string path = fullFileName.Remove(fullFileName.Length - name.Length);

                FileHelper.CreateDirectory(path);

                FileStream writer;
                if (File.Exists(fullFileName))
                {
                    writer = new FileStream(fullFileName, FileMode.Truncate);
                }
                else
                {
                    writer = new FileStream(fullFileName, FileMode.CreateNew);
                }
                formatter.Serialize(writer, objectToPersist);

                writer.Close();
                writer.Dispose();

                FileHelper.SetFileForEveryone(fullFileName);

                RemoveBackupFile(backupFileName);
            }
            catch (Exception ex)
            {
                SystemLog.LogException(ex);
                FileRollBack(backupFileName, fullFileName);
                throw new System.ServiceModel.FaultException($"{saveMsg}{Environment.NewLine}{ex.Message}");
            }
        }

        private string CreateBackupFile(string fullFileName)
        {
            var fi = new FileInfo(fullFileName);
            if (fi.Exists && fi.Length > 0)
            {
                var backupName = RenameFileWithUniqueName(fullFileName, "backup");
                if (!string.IsNullOrEmpty(backupName))
                {
                    File.Move(fullFileName, backupName);
                }
                return backupName;
            }
            return null;
        }

        private void FileRollBack(string backupFileName, string fullFileName)
        {
            bool originalFileIsReady = false;
            try
            {
                if (File.Exists(fullFileName))
                {
                    var newName = RenameFileWithUniqueName(fullFileName, "NEW");
                    try
                    {
                        MoveFileAndChangeDate(fullFileName, newName);
                        originalFileIsReady = true;
                    }
                    catch (Exception)
                    {
                        originalFileIsReady = false;
                    }
                }
                if (File.Exists(backupFileName))
                {

                    if (originalFileIsReady)
                    {
                        File.Move(backupFileName, fullFileName);
                    }
                    else
                    {
                        var newName = RenameFileWithUniqueName(fullFileName, "RESTORED");
                        MoveFileAndChangeDate(backupFileName, newName);
                    }
                }
            }
            catch (Exception ex)
            {
                SystemLog.LogException(ex);
            }
        }

        private void RemoveBackupFile(string backupFileName)
        {
            if (File.Exists(backupFileName))
            {
                File.Delete(backupFileName);
            }
        }


        private string RenameFileWithUniqueName(string fullFileName, string postfix)
        {
            if (File.Exists(fullFileName))
            {
                var extension = Path.GetExtension(fullFileName) ?? string.Empty;
                var nameWithoutExtension = fullFileName.Replace(extension, string.Empty);
                var backupName = $"{nameWithoutExtension}.{postfix}{extension}";
                int i = 1;
                while (File.Exists(backupName))
                {
                    backupName = $"{nameWithoutExtension}.{postfix}{i++}{extension}";
                }
                return backupName;
            }
            return null;
        }

        private void MoveFileAndChangeDate(string sourcePath, string destPath)
        {
            File.Move(sourcePath, destPath);
            File.SetCreationTime(destPath, DateTime.Now);
            File.SetLastWriteTime(destPath, DateTime.Now);
        }

        public List<MTFPersistDataInfo> GetPersistInfo(string path)
        {
            string fullPath = Path.Combine(BaseConstants.DataPath, path);

            List<MTFPersistDataInfo> persistDataInfos = new List<MTFPersistDataInfo>();
            foreach (string dir in Directory.GetDirectories(fullPath))
            {
                persistDataInfos.Add(new MTFPersistDataInfo { Name = dir.Remove(0, fullPath.Length + 1), Type = MTFDialogItemTypes.Folder });
            }

            foreach (string file in Directory.GetFiles(fullPath))
            {
                persistDataInfos.Add(new MTFPersistDataInfo { Name = file.Remove(0, fullPath.Length + 1), Type = MTFDialogItemTypes.File });
            }

            return persistDataInfos;
        }

        public MTFSequence LoadSequenceProject(string sequenceFullName)
        {
            var s = LoadData<MTFSequence>(sequenceFullName);
            if (s != null)
            {
                s.FullPath = sequenceFullName;
                if (s.ExternalSubSequencesPath != null && s.ExternalSubSequencesPath.Count > 0)
                {
                    s.ExternalSubSequences = new List<MTFExternalSequenceInfo>();
                    foreach (var item in s.ExternalSubSequencesPath)
                    {
                        s.ExternalSubSequences.Add(new MTFExternalSequenceInfo(LoadSequenceProject(DirectoryPathHelper.GetFullPathFromRelative(sequenceFullName, item.Key)), item.Value));
                    }
                }
            }
            return s;
        }

    }
}
