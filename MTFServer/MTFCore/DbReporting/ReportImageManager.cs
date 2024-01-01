using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutomotiveLighting.MTFCommon;
using MTFClientServerCommon;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.GraphicalView;
using MTFClientServerCommon.MTFValidationTable;

namespace MTFCore.DbReporting
{
    internal class ReportImageManager
    {
        private readonly DirectoryInfo defaultImageDir;
        private string actualPath;
        private const int MaxImagesInFolder = 10000;
        private int actualFolderImagesCount;

        public ReportImageManager(string imageRootFolder)
        {
            var defaultPath = Path.Combine(BaseConstants.DataPath, imageRootFolder);

            defaultImageDir = new DirectoryInfo(defaultPath);
            if (!defaultImageDir.Exists)
            {
                defaultImageDir.Create();
            }

            actualPath = GetActualPath();
        }

        private string GetActualPath()
        {
            var innerDirs = defaultImageDir.GetDirectories();

            if (innerDirs.Length > 0)
            {
                foreach (var innerDir in innerDirs.Reverse())
                {
                    var count = innerDir.GetFiles().Length;
                    if (count < MaxImagesInFolder)
                    {
                        actualFolderImagesCount = count;
                        return innerDir.FullName;
                    }
                }

                return CreateNewSubFolder(innerDirs.Length + 1);
            }

            return CreateNewSubFolder(0);
        }

        private string CreateNewSubFolder(int count)
        {
            var name = $"{count:D4}";
            return defaultImageDir.CreateSubdirectory(name).FullName;
        }

        public string SaveImage(MTFImage image, string fileName)
        {
            var actualDir = actualPath;
            var imgLongPath = Path.Combine(actualDir, fileName);

            Task.Run(() => Save(image, imgLongPath, fileName));

            return Path.Combine(Path.GetFileNameWithoutExtension(actualDir), fileName);
        }

        private void Save(MTFImage image, string imgLongPath, string fileName)
        {
            var isSaved = false;
            try
            {
                var ms = new MemoryStream(image.ImageData);
                using (var img = Image.FromStream(ms))
                {
                    img.Save(imgLongPath, ImageFormat.Jpeg);
                    ms.Dispose();
                }

                isSaved = true;
            }
            catch (Exception ex)
            {
                SystemLog.LogMessage($"Image {fileName} hasn't been saved");
                SystemLog.LogException(ex);
            }

            CheckSavingFolder(isSaved);
        }

        private void CheckSavingFolder(bool isSaved)
        {
            if (isSaved)
            {
                actualFolderImagesCount++;
                if (actualFolderImagesCount >= MaxImagesInFolder)
                {
                    actualPath = GetActualPath();
                }
            }
        }

        public string SaveImage(GraphicalViewInfo viewInfo, Dictionary<Guid, MTFValidationTable> validationTablesDict)
        {
            var actualDir = actualPath;
            var fileName = $"{Guid.NewGuid()}{BaseConstants.GraphicalViewScreenEx}";
            var imgLongPath = Path.Combine(actualDir, fileName);

            Task.Run(() => PerformSaveImage(viewInfo, validationTablesDict, imgLongPath));

            return Path.Combine(Path.GetFileNameWithoutExtension(actualDir), fileName);
        }

        private void PerformSaveImage(GraphicalViewInfo viewInfo, Dictionary<Guid, MTFValidationTable> validationTablesDict, string imgLongPath)
        {
            var info = GraphicalViewFileHelper.GetGraphicalViewInfo(viewInfo, validationTablesDict);
            if (info != null)
            {
                var isSaved = false;

                try
                {
                    info.Bitmap.Save(imgLongPath, ImageFormat.Png);
                    isSaved = true;
                }
                catch (Exception ex)
                {
                    SystemLog.LogMessage($"Graphical view {info.Name} hasn't been saved");
                    SystemLog.LogException(ex);
                }
                finally
                {
                    info.Dispose();
                }

                CheckSavingFolder(isSaved);
            }
        }
    }
}