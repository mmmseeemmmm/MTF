using MTFClientServerCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.Import;

namespace MTFCore
{
    public static class ImportHelper
    {
        public static bool ImportSequences(Stream stream, MTFImportSetting setting)
        {
            try
            {
                using (ZipArchive zip = new ZipArchive(stream, ZipArchiveMode.Read))
                {
                    ImportSequences(zip.GetSequences(), setting);
                    ImportComponentConfigs(zip.GetConfigurations(), setting);
                    ImportImages(zip.GetImagesAsZipEntry(), setting.ImagesSetting);
                }
                return true;
            }
            catch (Exception ex)
            {
                SystemLog.LogException(ex);
                return false;
            }
        }

        private static void ImportImages(List<ZipArchiveEntry> imageEntries, List<ImportConflict> setting)
        {
            var importedItems = setting.Where(x => x.EnableImport && (!x.IsConflict || x.OverrideOriginal)).ToList();
            if (imageEntries != null)
            {
                foreach (var entry in imageEntries)
                {
                    var fileName = Path.GetFileNameWithoutExtension(entry.Name);
                    var imgSetting = importedItems.FirstOrDefault(x => x.FileName == fileName);
                    if (imgSetting != null)
                    {
                        entry.ExtractToFile(Path.Combine(BaseConstants.DataPath, BaseConstants.GraphicalViewSources, entry.Name), true);
                    }
                }
            }
        }

        private static void ImportComponentConfigs(List<ZipArchiveEntry> configurations, MTFImportSetting setting)
        {
            var importedConfigurations = ExtractConfigurations(configurations);
            var savedConfigurations =
                LoadSavedConfigurations(setting.ClassInstances.Where(x => x.EnableImport).Select(x => x.FullFileName).Distinct().ToList());
            MergeConfigurations(importedConfigurations, savedConfigurations, setting);
            SaveData(savedConfigurations);
        }

        private static void SaveData(List<MTFClassInstanceConfiguration> savedConfigurations)
        {
            Dictionary<string, List<MTFClassInstanceConfiguration>> data = SeparateByFileName(savedConfigurations);
            foreach (var item in data.Values)
            {
                if (item.Count > 0)
                {
                    Core.Persist.SaveData<MTFClassInstanceConfiguration>(item, Core.ClassInstanceConfigPersistPath(item.First().ClassInfo));
                }
            }
        }

        private static Dictionary<string, List<MTFClassInstanceConfiguration>> SeparateByFileName(
            List<MTFClassInstanceConfiguration> savedConfigurations)
        {
            var output = new Dictionary<string, List<MTFClassInstanceConfiguration>>();
            foreach (var item in savedConfigurations)
            {
                if (output.ContainsKey(item.ClassInfo.FullName))
                {
                    output[item.ClassInfo.FullName].Add(item);
                }
                else
                {
                    output.Add(item.ClassInfo.FullName, new List<MTFClassInstanceConfiguration>() {item});
                }
            }
            return output;
        }

        private static void MergeConfigurations(List<MTFClassInstanceConfiguration> importedConfigurations,
            List<MTFClassInstanceConfiguration> savedConfigurations, MTFImportSetting setting)
        {
            foreach (var importConfig in importedConfigurations)
            {
                var fileName = Core.ClassInstanceConfigPersistPath(importConfig.ClassInfo);
                var currentSetting =
                    setting.ClassInstances.FirstOrDefault(x => fileName.EndsWith(x.FullFileName) && importConfig.Name == x.SubName);
                if (currentSetting != null)
                {
                    if (currentSetting.EnableImport)
                    {
                        var saveConfig = savedConfigurations.Where(x => x.ClassInfo.FullName == importConfig.ClassInfo.FullName && importConfig.Id == x.Id).ToList();
                        if (!currentSetting.IsConflict)
                        {
                            if (saveConfig.Count > 0)
                            {
                                throw new Exception("Bad setting! ClassInstanceConfiguration was changed during import");
                            }
                            else
                            {
                                savedConfigurations.Add(importConfig);
                            }
                        }
                        else
                        {
                            if (currentSetting.OverrideOriginal)
                            {
                                if (saveConfig.Count > 0)
                                {
                                    var index = savedConfigurations.IndexOf(saveConfig[0]);
                                    if (index != -1)
                                    {
                                        savedConfigurations[index] = importConfig;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception("Setting is not found");
                }
            }
        }

        private static List<MTFClassInstanceConfiguration> LoadSavedConfigurations(List<string> fileNames)
        {
            var outputList = new List<MTFClassInstanceConfiguration>();
            foreach (var name in fileNames)
            {
                outputList.AddRange(Core.Persist.LoadDataList<MTFClassInstanceConfiguration>(Path.Combine(BaseConstants.ClassInstanceConfigBasePath, name)));
            }
            return outputList;
        }

        private static List<MTFClassInstanceConfiguration> ExtractConfigurations(List<ZipArchiveEntry> zipArchiveEntries)
        {
            List<MTFClassInstanceConfiguration> importedConfigurations = new List<MTFClassInstanceConfiguration>();
            foreach (var item in zipArchiveEntries)
            {
                using (Stream s = item.Open())
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    var obj = formatter.Deserialize(s) as List<MTFClassInstanceConfiguration>;
                    if (obj != null)
                    {
                        importedConfigurations.AddRange(obj);
                    }
                }
            }
            return importedConfigurations;
        }

        private static void ImportSequences(IEnumerable<ZipArchiveEntry> sequences, MTFImportSetting setting)
        {
            foreach (var item in sequences)
            {
                var currentSetting = setting.Sequences.FirstOrDefault(x => x.FullFileName == item.FullName);
                if (currentSetting != null)
                {
                    if (currentSetting.EnableImport)
                    {
                        if (!currentSetting.IsConflict || currentSetting.OverrideOriginal)
                        {
                            var dirName = Path.Combine(BaseConstants.SequenceBasePath, Path.GetDirectoryName(currentSetting.StorePath));
                            FileOperation.CreateDirectory(dirName);
                            item.ExtractToFile(GetSequenceFullName(currentSetting.StorePath), true);
                        }
                    }
                }
                else
                {
                    throw new Exception("Setting is not found");
                }
            }
        }

        private static string GetSequenceFullName(string fileName)
        {
            return Path.Combine(BaseConstants.DataPath, BaseConstants.SequenceBasePath, fileName);
        }
    }
}