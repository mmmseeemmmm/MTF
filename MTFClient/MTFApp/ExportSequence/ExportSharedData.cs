using MTFClientServerCommon;
using System.Collections.Generic;
using System.Linq;
using MTFApp.SequenceEditor.GraphicalView;
using MTFClientServerCommon.GraphicalView;

namespace MTFApp.ExportSequence
{
    public class ExportSharedData
    {
        private readonly GraphicalViewResourcesHelper imageResourcesManager = GraphicalViewResourcesHelper.Instance;
        private IList<GraphicalViewImg> images;
        private bool imagesAssigned;
        private readonly object imagesLoadLock = new object();

        public MTFSequence MainSequence { get; private set; }

        public IList<ExportSetting> SequencesToExport { get; set; }

        public IList<ExportSetting> ConfigsToExport { get; set; }

        public IList<ImageExportWrapper> ImagesToExport { get; set; }


        public ExportSharedData(MTFSequence mainSequence)
        {
            SequencesToExport = new List<ExportSetting>();
            ImagesToExport = new List<ImageExportWrapper>();
            MainSequence = mainSequence;

            LoadImagesAsync();

            FillSequencesSetting(new[] {mainSequence}, SequencesToExport);
            ConfigsToExport = FillConfigsToExport(new[] {mainSequence});
            FillImages(new[] {mainSequence}, ImagesToExport);
        }

        private async void LoadImagesAsync()
        {
            images = await imageResourcesManager.GetResources();
            AssignImageNames();
        }

        private void AssignImageNames()
        {
            lock (imagesLoadLock)
            {
                if (!imagesAssigned && ImagesToExport.Count > 0 && images != null)
                {
                    imagesAssigned = true;
                    foreach (var item in ImagesToExport)
                    {
                        if (item.ExportSetting != null && !item.IsAssigned)
                        {
                            item.Img = images.FirstOrDefault(x => x.FileName == item.ExportSetting.Name);
                            if (item.Img != null)
                            {
                                item.ExportSetting.Alias = item.Img.Name;
                            }
                        }
                    }
                }
            }
        }

        private void FillImages(IEnumerable<MTFSequence> sequences, ICollection<ImageExportWrapper> exportList)
        {
            foreach (var item in sequences)
            {
                if (item.GraphicalViewSetting != null && item.GraphicalViewSetting.HasView)
                {
                    foreach (var viewInfo in item.GraphicalViewSetting.Views)
                    {
                        if (!string.IsNullOrEmpty(viewInfo.ImageFileName) && exportList.All(x => x.ExportSetting.Name != viewInfo.ImageFileName))
                        {
                            exportList.Add(new ImageExportWrapper {ExportSetting = new ExportSetting(viewInfo.Id, viewInfo.ImageFileName, true)});
                        }
                    }
                }

                if (item.ExternalSubSequences != null)
                {
                    FillImages(item.ExternalSubSequences.Select(x => x.ExternalSequence), exportList);
                }
            }
            AssignImageNames();
        }

        private IList<ExportSetting> FillConfigsToExport(IEnumerable<MTFSequence> sequences)
        {
            List<ExportSetting> tmp = new List<ExportSetting>();
            foreach (var sequence in sequences)
            {
                foreach (var classInfo in sequence.MTFSequenceClassInfos)
                {
                    if (classInfo.MTFClassInstanceConfiguration != null)
                    {
                        tmp.Add(new ExportSetting(classInfo.MTFClassInstanceConfiguration.Id, classInfo.Alias, true));
                    }
                }
            }
            return tmp;
        }

        private void FillSequencesSetting(IEnumerable<MTFSequence> sequences, ICollection<ExportSetting> exportList)
        {
            foreach (var item in sequences)
            {
                exportList.Add(new ExportSetting(item.Id, item.Name, true));
                if (item.ExternalSubSequences != null)
                {
                    FillSequencesSetting(item.ExternalSubSequences.Select(x => x.ExternalSequence), exportList);
                }
            }
        }
    }
}