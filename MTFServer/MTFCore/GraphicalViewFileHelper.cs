using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using MTFClientServerCommon;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.GraphicalView;
using MTFClientServerCommon.Helpers;
using MTFClientServerCommon.MTFValidationTable;
using MTFCommon;

namespace MTFCore
{
    public static class GraphicalViewFileHelper
    {
        static readonly Brush notFillBrush = new SolidBrush(Color.FromArgb(255, 201, 201, 201));
        static readonly Brush okBrush = new SolidBrush(Color.FromArgb(255, 183, 226, 137));
        static readonly Brush nokBrush = new SolidBrush(Color.FromArgb(255, 255, 94, 94));
        static readonly Brush gsFail = new SolidBrush(Color.FromArgb(255, 254, 193, 0));
        static readonly string labelFontName = "Avenir";
        static readonly int defaultLabelSize = 10;
        const int TestItemSize = 20;
        const int TextOffset = 25;
        private static readonly object GetGraphicalViewInfoLock = new object();


        public static List<GraphicalViewImg> LoadAllGraphicalViewImages()
        {
            return LoadGraphicalViewImages(null);
        }

        public static List<GraphicalViewImg> LoadGraphicalViewImages(IEnumerable<string> fileNames)
        {
            var destination = Path.Combine(BaseConstants.DataPath, BaseConstants.GraphicalViewSources);
            var output = new List<GraphicalViewImg>();
            var all = fileNames == null;

            var di = new DirectoryInfo(destination);

            if (di.Exists)
            {
                var files = di.GetFiles($"*{BaseConstants.GraphicalViewImageExtension}");
                foreach (var fileInfo in files)
                {
                    if (!all)
                    {
                        if (!fileNames.Contains(Path.GetFileNameWithoutExtension(fileInfo.Name)))
                        {
                            continue;
                        }
                    }

                    try
                    {
                        var data = FileOperation.GetData<GraphicalViewImg>(fileInfo.FullName);
                        if (data != null)
                        {
                            output.Add(data);
                        }
                    }
                    catch (Exception ex)
                    {
                        SystemLog.LogMessage(ex.Message);
                    }
                }
            }

            return output;
        }

        public static List<GraphicalViewImg> SaveGraficalViewImages(List<GraphicalViewImg> images)
        {
            if (images != null)
            {
                var destination = Path.Combine(BaseConstants.DataPath, BaseConstants.GraphicalViewSources);

                FileHelper.CreateDirectory(destination);

                foreach (var img in images)
                {
                    var name = GetUniqueGuidName(destination, BaseConstants.GraphicalViewImageExtension);
                    img.FileName = Path.GetFileNameWithoutExtension(name);
                    FileOperation.SaveData(img, name);
                }
            }

            return images;
        }


        private static string GetUniqueGuidName(string destination, string extension)
        {
            var uniqueName = GetGuidName(destination, extension);
            while (File.Exists(uniqueName))
            {
                uniqueName = GetGuidName(destination, extension);
            }

            return uniqueName;
        }

        private static string GetGuidName(string destination, string extension)
        {
            return $"{Path.Combine(destination, Guid.NewGuid().ToString())}{extension}";
        }

        public static IEnumerable<string> GetGraphicalViewImageNames()
        {
            var destination = Path.Combine(BaseConstants.DataPath, BaseConstants.GraphicalViewSources);

            var di = new DirectoryInfo(destination);

            if (di.Exists)
            {
                var files = di.GetFiles($"*{BaseConstants.GraphicalViewImageExtension}");
                return files.Select(x => Path.GetFileNameWithoutExtension(x.Name));
            }

            return null;
        }


        public static BitmapInfo GetGraphicalViewInfo(GraphicalViewInfo view, Dictionary<Guid, MTFValidationTable> validationTables)
        {
            lock (GetGraphicalViewInfoLock)
            {
                var fi = new FileInfo(
                    $"{Path.Combine(BaseConstants.DataPath, BaseConstants.GraphicalViewSources, view.ImageFileName)}{BaseConstants.GraphicalViewImageExtension}");
                if (fi.Exists)
                {
                    var imgData = FileOperation.GetData<GraphicalViewImg>(fi.FullName);

                    if (imgData?.Data != null)
                    {
                        using (var ms = new MemoryStream(imgData.Data))
                        {
                            using (var b = new Bitmap(ms))
                            {
                                using (var gr = Graphics.FromImage(b))
                                {
                                    var dpiScaleX = gr.DpiX / view.ScreenDipX;
                                    var dpiScaleY = gr.DpiY / view.ScreenDipY;
                                    var ratio = view.Scale * dpiScaleX;
                                    var size = (int) (TestItemSize * ratio);
                                    var textOffset = (int) (TextOffset * ratio);
                                    var labelFont = new Font(labelFontName, (float) (defaultLabelSize * view.Scale));

                                    gr.SmoothingMode = SmoothingMode.HighQuality;
                                    if (validationTables?.Count > 0)
                                    {
                                        foreach (var testItem in view.TestItems)
                                        {
                                            if (validationTables.ContainsKey(testItem.ValidationTableId))
                                            {
                                                var table = validationTables[testItem.ValidationTableId];
                                                if (table != null)
                                                {
                                                    var rect = new Rectangle((int)(testItem.Position.X * dpiScaleX),
                                                        (int)(testItem.Position.Y * dpiScaleY), size, size);

                                                    if (testItem.ValidationTableRowId != Guid.Empty)
                                                    {
                                                        var row = table.Rows?.FirstOrDefault(r => r.Id == testItem.ValidationTableRowId);
                                                        if (row!=null)
                                                        {
                                                            gr.FillEllipse(GetColorByStatus(row.Status), rect);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        gr.FillEllipse(GetColorByStatus(table.Status), rect);
                                                    }

                                                    gr.DrawEllipse(Pens.Black, rect);

                                                    var fontSize = gr.MeasureString(testItem.Alias, labelFont);
                                                    var x = rect.X + textOffset;
                                                    var y = GetY(rect.Y, size, fontSize.Height);
                                                    gr.FillRectangle(Brushes.White, x, y, fontSize.Width,
                                                        fontSize.Height);
                                                    gr.DrawString(testItem.Alias, labelFont,
                                                        Brushes.Black, new PointF(x, y));
                                                }
                                            }
                                        }
                                    }

                                    return new BitmapInfo(new Bitmap(b), view.ViewName);
                                }
                            }
                        }
                    }
                }

                return null;
            }
        }

        private static int GetY(int yposition, int testItemHeight, float fontHeight)
        {
            return (int)(yposition + (testItemHeight - fontHeight) / 2);
        }

        private static Brush GetColorByStatus(MTFValidationTableStatus status)
        {
            switch (status)
            {
                case MTFValidationTableStatus.NotFilled:
                    return notFillBrush;
                case MTFValidationTableStatus.Ok:
                    return okBrush;
                case MTFValidationTableStatus.Nok:
                    return nokBrush;
                case MTFValidationTableStatus.GSFail:
                    return gsFail;
                default:
                    return notFillBrush;
            }
        }
    }
}