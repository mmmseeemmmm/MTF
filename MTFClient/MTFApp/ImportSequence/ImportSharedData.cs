using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using MTFClientServerCommon.GraphicalView;
using MTFClientServerCommon.Helpers;
using MTFClientServerCommon.Import;
using Version = MTFClientServerCommon.Version;

namespace MTFApp.ImportSequence
{
    public class ImportSharedData
    {
        private readonly List<SequencesInZip> sequences = new List<SequencesInZip>();
        private readonly Dictionary<string, List<ComponentConfigInfo>> configurations = new Dictionary<string, List<ComponentConfigInfo>>();
        private readonly Version originalMtfVersion;
        private readonly string fileName;
        private readonly ZipArchive zipArchive;
        private readonly List<SequenceMetaData> metaData;
        private readonly List<GraphicalViewImg> images;
        private readonly MTFImportSetting importSetting = new MTFImportSetting();
        private string sequenceDestinationPath = string.Empty;

        public ImportSharedData(FileStream fileStream)
        {
            zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Read, true);
            if (zipArchive != null)
            {
                fileName = fileStream.Name;
                metaData = zipArchive.GetMetaData();
                if (metaData != null)
                {
                    sequences = GetSequencesFromMetaData(metaData);
                    configurations = zipArchive.GetConfigurationsName();
                    images = zipArchive.GetImages();
                    FillSharedData();

                    if (sequences != null && sequences.Count > 0)
                    {
                        originalMtfVersion = sequences.First().MTFVersion;
                    }
                }
                else
                {
                    throw new Exception(LanguageHelper.GetString("Mtf_Import_PreviewErr"));
                }
            }
            zipArchive.Dispose();
        }

        public string SequenceDestinationPath
        {
            get => sequenceDestinationPath;
            set => sequenceDestinationPath = value;
        }

        public List<SequenceMetaData> MetaData => metaData;

        public Version OriginalMtfVersion => originalMtfVersion;

        public string FileName => fileName;

        public List<SequencesInZip> Sequences => sequences;

        public List<GraphicalViewImg> Images => images;

        public MTFImportSetting ImportSetting => importSetting;

        public Dictionary<string, List<ComponentConfigInfo>> Configurations => configurations;

        private List<SequencesInZip> GetSequencesFromMetaData(List<SequenceMetaData> data)
        {
            return data.Select(item => new SequencesInZip
                                       {
                                           SequenceFullName = Path.GetFileName(item.OriginalName),
                                           ParentFullName = Path.GetFileName(item.MainSequenceName),
                                           SequenceStoredName = item.StoredName,
                                           MTFVersion = item.MTFVersion,
                                       }).ToList();
        }


        private void FillSharedData()
        {
            if (sequences != null && sequences.Any())
            {
                foreach (var item in sequences)
                {
                    importSetting.Sequences.Add(new ImportConflict(item.Id, item.SequenceStoredName, false, false));
                }
            }

            if (configurations != null && configurations.Any())
            {
                foreach (var file in configurations.Keys)
                {
                    foreach (var item in configurations[file])
                    {
                        importSetting.ClassInstances.Add(new ImportConflict(item.Id, file, item.Name, false, false));
                    }
                }
            }
        }
    }
}