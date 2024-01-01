using System;
using MTFClientServerCommon;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;
using Microsoft.Win32;
using MTFApp.MTFWizard;
using MTFApp.UIHelpers;
using MTFApp.UIHelpers.LongTask;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.Helpers;
using MTFClientServerCommon.Import;
using Version = MTFClientServerCommon.Version;

namespace MTFApp.ExportSequence
{
    public static class ExportHelper
    {
        private const string ClassInstanceConfigBasePath = "functionalityLibrary";

        public static void ExportSequence(MTFSequence mainSequence)
        {
            var sharedData = new ExportSharedData(mainSequence);
            var controls = new List<MTFWizardUserControl>
                           {
                               new ExportPreview(sharedData),
                               new ExportSequences(sharedData),
                               new ExportClassInstances(sharedData),
                               new ExportImages(sharedData),
                               new ExportSummary(sharedData),
                           };

            var exportDialog = new MTFWizardWindow(controls) {Owner = Application.Current.MainWindow};
            exportDialog.ShowDialog();

            if (exportDialog.Result == true)
            {
                var dialog = new SaveFileDialog {Filter = string.Format("{0} (*.zip)|*.zip", LanguageHelper.GetString("Mtf_FileDialog_ZipFile"))};

                if (dialog.ShowDialog() == true)
                {
                    var path = dialog.FileName;
                    try
                    {
                        LongTask.Do(() =>
                                    {
                                        Stream s = CreateZipFile(sharedData);
                                        using (var fs = new FileStream(path, FileMode.Create))
                                        {
                                            s.CopyTo(fs);
                                            s.Dispose();
                                        }
                                    }, LanguageHelper.GetString("Mtf_Export_Exporting"));
                    }
                    catch (Exception ex)
                    {
                        MTFMessageBox.Show(LanguageHelper.GetString("Msg_Header_Error"), ex.Message, MTFMessageBoxType.Error,
                            MTFMessageBoxButtons.Ok);
                    }
                }
            }
        }

        public static Stream CreateZipFile(ExportSharedData sharedData)
        {
            List<SequenceMetaData> metaData = new List<SequenceMetaData>();
            Stream ms = new MemoryStream();
            List<string> entriesPath = new List<string>();
            using (ZipArchive zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
            {
                CreateEntriesFromSequences(zip, string.Empty, new[] {sharedData.MainSequence}, null, sharedData.SequencesToExport, string.Empty,
                    metaData, entriesPath);
                CreateEntryFromMetaData(zip, metaData);
                var configurations = SumarizeInstances(new[] {sharedData.MainSequence}, sharedData.ConfigsToExport);
                CreateEntriesFromComponnetsConfig(zip, configurations);
                CreateEntriesFromImages(zip, sharedData.ImagesToExport);
            }
            ms.Position = 0;
            return ms;
        }

        private static void CreateEntriesFromImages(ZipArchive zip, IList<ImageExportWrapper> images)
        {
            var imagesToExport = images.Where(x => x.ExportSetting.Export);
            foreach (var exportWrapper in imagesToExport)
            {
                var entry =
                    zip.CreateEntry(string.Format("{0}{1}", Path.Combine(ImportExportConstants.ImagesFolder, exportWrapper.ExportSetting.Name),
                        BaseConstants.GraphicalViewImageExtension));
                using (var entryStream = entry.Open())
                {
                    Stream entryContent = CreateStream(exportWrapper.Img);
                    if (entryContent != null)
                    {
                        entryContent.Position = 0;
                        entryContent.CopyTo(entryStream);
                        entryContent.Dispose();
                    }
                }
            }
        }

        private static void CreateEntryFromMetaData(ZipArchive zip, List<SequenceMetaData> metaData)
        {
            var entry = zip.CreateEntry(ImportExportConstants.MetaData);
            using (var entryStream = entry.Open())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(entryStream, metaData);
            }
        }

        private static void CreateEntriesFromComponnetsConfig(ZipArchive zip,
            Dictionary<string, List<MTFClassInstanceConfiguration>> configurations)
        {
            foreach (var config in configurations.Values)
            {
                if (config.Count > 0)
                {
                    var classInfo = config.First().ClassInfo;
                    if (classInfo != null)
                    {
                        string name = Path.Combine(ImportExportConstants.ComponentFolder, ClassInstanceConfigPersistPath(classInfo));
                        var entry = zip.CreateEntry(name);
                        using (var entryStream = entry.Open())
                        {
                            Stream entryContent = CreateStreamFromSequence<MTFClassInstanceConfiguration>(config);
                            if (entryContent != null)
                            {
                                entryContent.Position = 0;
                                entryContent.CopyTo(entryStream);
                                entryContent.Dispose();
                            }
                        }
                    }
                }
            }
        }

        private static string ClassInstanceConfigPersistPath(MTFClassInfo classInfo)
        {
            return Path.Combine(ClassInstanceConfigBasePath,
                classInfo.AssemblyName,
                classInfo.FullName + ".xml");
        }


        private static void CreateEntriesFromSequences(ZipArchive zip, string mainSequenceName, IEnumerable<MTFSequence> sequences,
            IEnumerable<string> relativePaths, IEnumerable<ExportSetting> settings, string folder, List<SequenceMetaData> metaData,
            List<string> entriesPath)
        {
            var mtfVersion = new Version(Assembly.GetExecutingAssembly().GetName().Version);
            foreach (var sequence in sequences)
            {
                string relativePath = sequence.FullName;
                var setting = settings.FirstOrDefault(x => x.Guid == sequence.Id);
                if (setting != null && setting.Export)
                {
                    var entryOriginalName = Path.Combine(folder, sequence.FullName);
                    var entryStoredName = GenerateName(entryOriginalName, entriesPath);
                    var entry = zip.CreateEntry(entryStoredName);
                    entriesPath.Add(entryStoredName);

                    if (relativePaths != null)
                    {
                        relativePath = relativePaths.FirstOrDefault(x => x.EndsWith(sequence.FullName));
                    }
                    metaData.Add(new SequenceMetaData()
                    {
                        Id = sequence.Id,
                        StoredName = entryStoredName,
                        OriginalName = entryOriginalName,
                        Path = relativePath,
                        MainSequenceName = mainSequenceName,
                        MTFVersion = mtfVersion,
                    });
                    sequence.MTFVersion = mtfVersion;
                    using (var entryStream = entry.Open())
                    {
                        Stream entryContent = CreateStreamFromSequence<MTFSequence>(sequence);
                        entryContent.Position = 0;
                        entryContent.CopyTo(entryStream);
                        entryContent.Dispose();
                    }
                }
                if (sequence.ExternalSubSequences != null && sequence.ExternalSubSequencesPath != null)
                {
                    CreateEntriesFromSequences(zip, relativePath, sequence.ExternalSubSequences.Select(x => x.ExternalSequence),
                        sequence.ExternalSubSequencesPath.Select(x => x.Key), settings, ImportExportConstants.SubSequencesFolder, metaData,
                        entriesPath);
                }
            }
        }

        private static string GenerateName(string originalName, List<string> entriesPath)
        {
            int i = 1;
            string name = originalName;
            var fileName = Path.GetFileNameWithoutExtension(originalName);
            var extension = Path.GetExtension(originalName);
            var directory = Path.GetDirectoryName(originalName);
            while (entriesPath.Any(x => x.EndsWith(name)))
            {
                var sb = new StringBuilder();
                sb.Append(fileName).Append('_').Append(i).Append(extension);
                name = Path.Combine(directory, sb.ToString());
                i++;
            }
            return name;
        }

        private static Stream CreateStreamFromSequence<T>(object objectToPersist) where T : MTFPersist
        {
            if (objectToPersist is T || objectToPersist is IEnumerable<T>)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream ms = new MemoryStream();
                formatter.Serialize(ms, objectToPersist);
                return ms;
            }
            return null;
        }

        private static Stream CreateStream(object data)
        {
            if (data != null)
            {
                var type = data.GetType();
                if (type.IsSerializable)
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    MemoryStream ms = new MemoryStream();
                    formatter.Serialize(ms, data);
                    return ms;
                }
            }
            return null;
        }

        private static Dictionary<string, List<MTFClassInstanceConfiguration>> SumarizeInstances(IEnumerable<MTFSequence> sequences,
            IEnumerable<ExportSetting> settings)
        {
            var dict = new Dictionary<string, List<MTFClassInstanceConfiguration>>();
            foreach (var sequence in sequences)
            {
                foreach (var item in sequence.MTFSequenceClassInfos.Select(x => x.MTFClassInstanceConfiguration).Where(x => x != null))
                {
                    var currentSetting = settings.FirstOrDefault(x => x.Guid == item.Id);
                    if (currentSetting != null && currentSetting.Export)
                    {
                        if (dict.ContainsKey(item.ClassInfo.FullName))
                        {
                            if (dict[item.ClassInfo.FullName].All(x => x.ClassInfo.Id != item.ClassInfo.Id))
                            {
                                dict[item.ClassInfo.FullName].Add(item);
                            }
                        }
                        else
                        {
                            dict.Add(item.ClassInfo.FullName, new List<MTFClassInstanceConfiguration> {item});
                        }
                    }
                }
            }
            return dict;
        }
    }
}