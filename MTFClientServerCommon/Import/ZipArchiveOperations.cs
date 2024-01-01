using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.GraphicalView;
using MTFClientServerCommon.Helpers;

namespace MTFClientServerCommon.Import
{
    public static class ZipArchiveOperations
    {
        public static List<string> GetSequencesName(this ZipArchive zipArchive)
        {
            List<string> sequences = new List<string>();
            if (zipArchive.Entries.Count > 0)
            {
                foreach (var item in zipArchive.Entries)
                {
                    if (item.FullName.EndsWith(BaseConstants.SequenceExtension))
                    {
                        sequences.Add(item.FullName);
                    }
                }
            }
            return sequences;
        }

        public static List<ZipArchiveEntry> GetSequences(this ZipArchive zipArchive)
        {
            List<ZipArchiveEntry> sequences = new List<ZipArchiveEntry>();
            if (zipArchive.Entries.Count > 0)
            {
                foreach (var item in zipArchive.Entries)
                {
                    if (item.FullName.EndsWith(BaseConstants.SequenceExtension))
                    {
                        sequences.Add(item);
                    }
                }
            }
            return sequences;
        }

        public static List<ZipArchiveEntry> GetConfigurations(this ZipArchive zipArchive)
        {
            List<ZipArchiveEntry> configs = new List<ZipArchiveEntry>();
            if (zipArchive.Entries.Count > 0)
            {
                foreach (var item in zipArchive.Entries)
                {
                    if (item.FullName.StartsWith($"{ImportExportConstants.ComponentFolder}\\"))
                    {
                        configs.Add(item);
                    }
                }
            }
            return configs;
        }

        public static List<SequenceMetaData> GetMetaData(this ZipArchive zipArchive)
        {
            if (zipArchive.Entries.Count > 0)
            {
                var entry = zipArchive.Entries.FirstOrDefault(x => x.FullName.Contains(ImportExportConstants.MetaData));
                if (entry != null)
                {
                    using (var stream = entry.Open())
                    {
                        return DeserializeStream<List<SequenceMetaData>>(stream);
                    }
                }
            }
            return null;
        }

        public static List<GraphicalViewImg> GetImages(this ZipArchive zipArchive)
        {
            var images = new List<GraphicalViewImg>();
            if (zipArchive.Entries.Count > 0)
            {
                foreach (var item in zipArchive.Entries)
                {
                    if (item.FullName.StartsWith($"{ImportExportConstants.ImagesFolder}\\"))
                    {
                        using (var stream = item.Open())
                        {
                            BinaryFormatter bf = new BinaryFormatter();
                            if (bf.Deserialize(stream) is GraphicalViewImg img)
                            {
                                images.Add(img);
                            }
                        }
                    }
                }
            }
            return images;
        }

        public static List<ZipArchiveEntry> GetImagesAsZipEntry(this ZipArchive zipArchive)
        {
            var images = new List<ZipArchiveEntry>();
            if (zipArchive.Entries.Count > 0)
            {
                foreach (var item in zipArchive.Entries)
                {
                    if (item.FullName.StartsWith($"{ImportExportConstants.ImagesFolder}\\")
                        && item.FullName.EndsWith(BaseConstants.GraphicalViewImageExtension))
                    {
                        images.Add(item);
                    }
                }
            }
            return images;
        }

        public static Dictionary<string, List<ComponentConfigInfo>> GetConfigurationsName(this ZipArchive zipArchive)
        {
            var configs = new Dictionary<string, List<ComponentConfigInfo>>();
            if (zipArchive.Entries.Count > 0)
            {
                foreach (var item in zipArchive.Entries)
                {
                    if (item.FullName.StartsWith($"{ImportExportConstants.ComponentFolder}\\"))
                    {
                        var path = item.FullName.Split(new[] {"functionalityLibrary\\"},  StringSplitOptions.None);
                        List<ComponentConfigInfo> tmp = null;
                        using (var s = item.Open())
                        {
                            var formatter = new BinaryFormatter();
                            if (formatter.Deserialize(s) is List<MTFClassInstanceConfiguration> obj)
                            {
                                tmp = new List<ComponentConfigInfo>();
                                foreach (var cfg in obj)
                                {
                                    tmp.Add(new ComponentConfigInfo{ Id = cfg.Id, Name = cfg.Name});
                                }
                            }
                        }
                        configs.Add(path.Last(), tmp);
                    }
                }
            }
            return configs;
        }

        public static T DeserializeStream<T>(Stream stream) where T : class
        {
            var formatter = new BinaryFormatter() {Binder = new ImportSerializationBinder()};
            return formatter.Deserialize(stream) as T;
        }
    }
}