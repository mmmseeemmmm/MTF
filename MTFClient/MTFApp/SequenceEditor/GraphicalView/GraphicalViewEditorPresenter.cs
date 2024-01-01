using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MTFApp.UIHelpers;
using MTFClientServerCommon;
using MTFClientServerCommon.GraphicalView;
using MTFClientServerCommon.Helpers;
using MTFClientServerCommon.MTFValidationTable;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace MTFApp.SequenceEditor.GraphicalView
{
    public class GraphicalViewEditorPresenter : NotifyPropertyBase
    {
        private readonly ObservableCollection<GraphicalViewWrapper> views = new ObservableCollection<GraphicalViewWrapper>();
        private ObservableCollection<GraphicalViewImg> imageCollection;
        private GraphicalViewWrapper selectedView;
        private bool enableSelectImg;
        private List<ValidationTableWrapper> validationTables = new List<ValidationTableWrapper>();
        private MTFSequence sequence;
        private double defaultPointSize = 30;
        private double defaultPointStroke = 1;
        private double scale = 1;
        private bool recalculateItems = false;
        private List<DutInfo> dutInfos;
        private readonly float screenDpiX;
        private readonly float screenDpiY;
        private readonly Command removeTableCmd;
        private readonly Command expandTableCmd;

        public GraphicalViewEditorPresenter()
        {
            removeTableCmd = new Command(RemoveTab);
            expandTableCmd = new Command(ExpandTable);
            views.Add(new GraphicalViewWrapper());
            GetImageResourcesAsync();

            using (var gr = Graphics.FromHwnd(IntPtr.Zero))
            {
                screenDpiX = gr.DpiX;
                screenDpiY = gr.DpiY;
            }
        }

        #region Commands

        public ICommand RemoveTabCommand => removeTableCmd;

        public ICommand ExpandTableCommand => expandTableCmd;



        #endregion

        #region Properties

        public ObservableCollection<GraphicalViewImg> ImageCollection
        {
            get => imageCollection;
            set => imageCollection = value;
        }

        public void SetImage(GraphicalViewImg img)
        {
            if (SelectedView != null && img != null)
            {
                SelectedView.Img = img;
            }
        }

        public double PointSize => defaultPointSize;
        public double PointSizeTransform => defaultPointSize*Scale;
        public double PointStrokeTransform => defaultPointStroke*Scale;

        public ObservableCollection<GraphicalViewWrapper> Views => views;

        public GraphicalViewWrapper SelectedView
        {
            get => selectedView;
            set
            {
                if (value != null)
                {
                    if (value.IsNew)
                    {
                        selectedView = null;
                    }
                    else
                    {
                        AssignImage(value);
                        CheckUsedTablesAsync(value);
                    }

                    value.ImageChanged += SelectedViewOnImageChanged;
                }

                selectedView = value;
                DutInfos = GetDutInfos(sequence);
                NotifyPropertyChanged();
            }
        }

        private void SelectedViewOnImageChanged(object sender, EventArgs eventArgs)
        {
            recalculateItems = true;
        }


        public bool EnableSelectImg
        {
            get => enableSelectImg;
            set
            {
                enableSelectImg = value;
                NotifyPropertyChanged();
            }
        }

        public List<DutInfo> DutInfos
        {
            get => dutInfos;
            set
            {
                dutInfos = value;
                NotifyPropertyChanged();
            }
        }

        public bool ShowDutSelection => DutInfos?.Count > 0;

        public List<ValidationTableWrapper> ValidationTables => validationTables;

        public ObservableCollection<GraphicalViewTestItem> TestItems => SelectedView?.GraphicalViewInfo?.TestItems;

        public double Scale
        {
            get => scale;
            private set
            {
                var newScale = value;
                if (double.IsNaN(newScale) || double.IsInfinity(newScale))
                {
                    newScale = 1;
                }
                scale = newScale;

                if (SelectedView?.GraphicalViewInfo != null)
                {
                    SelectedView.GraphicalViewInfo.ScreenDipX = screenDpiX;
                    SelectedView.GraphicalViewInfo.ScreenDipY = screenDpiY;
                    SelectedView.GraphicalViewInfo.Scale = newScale;
                }

                NotifyPropertyChanged();
                InvalidatePointScale();
            }
        }

        #endregion

        internal void CreateNewView(GraphicalViewWrapper value)
        {
            value.IsNew = false;
            value.GraphicalViewInfo.ViewName = $"{LanguageHelper.GetString("MainCommand_GraphicalView")} {views.Count}";
            Views.Add(new GraphicalViewWrapper());
            SelectedView = value;

            if (sequence != null)
            {
                if (sequence.GraphicalViewSetting == null)
                {
                    sequence.GraphicalViewSetting = new GraphicalViewSetting();
                }

                sequence.GraphicalViewSetting.AddView(value.GraphicalViewInfo);
            }
        }

        internal void SequenceChanged(MTFSequence mtfSequence)
        {
            sequence = mtfSequence;
            if (mtfSequence != null)
            {
                LoadViews();
                validationTables = GetTables(mtfSequence);
                CheckUsedTablesAsync(SelectedView);
            }
        }


        internal void AddPoint(Point point, GraphicalViewTableItemBase testItem, GraphicalViewTestItemType type)
        {
            if (testItem != null && SelectedView?.GraphicalViewInfo != null)
            {
                var info = SelectedView.GraphicalViewInfo;
                if (info.TestItems == null)
                {
                    info.TestItems = new ObservableCollection<GraphicalViewTestItem>();
                }

                info.TestItems.Add(new GraphicalViewTestItem
                                   {
                                       Position =
                                           new Point(point.X - PointSize / 2,
                                               point.Y - PointSize / 2),
                                       ValidationTableId = testItem.TableId,
                                       ValidationTableRowId = testItem.RowId,
                                       Alias = testItem.Alias,
                                       Type = type,
                                   });
                testItem.IsUsed = true;
            }
        }

        internal void SetScale(double newScale)
        {
            if (newScale != Scale)
            {
                Scale = newScale;
            }
        }

        internal void Move(GraphicalViewTestItem item, double horizontalChange, double verticalChange)
        {
            if (item != null)
            {
                item.Position = new Point(item.Position.X + horizontalChange, item.Position.Y + verticalChange);
            }
        }

        internal void Place(GraphicalViewTestItem item, Point position)
        {
            if (item != null)
            {
                var offset = PointSize * scale / 2;
                item.Position = new Point(position.X - offset, position.Y - offset);
            }
        }

        internal void RemoveTestItem(GraphicalViewTestItem graphicalViewTestItem)
        {
            if (graphicalViewTestItem != null && TestItems != null)
            {
                TestItems.Remove(graphicalViewTestItem);

                var table = validationTables?.FirstOrDefault(x => x.ValidationTable.Id == graphicalViewTestItem.ValidationTableId);
                if (table != null)
                {
                    if (graphicalViewTestItem.ValidationTableRowId == Guid.Empty)
                    {
                        table.IsUsed = TestItems.Any(x => x.ValidationTableId == table.ValidationTable.Id);
                    }
                    else
                    {
                        var row = table.Rows.FirstOrDefault(x => x.Row.Id == graphicalViewTestItem.ValidationTableRowId);
                        if (row != null)
                        {
                            row.IsUsed = TestItems.Any(x => x.ValidationTableRowId == row.Row.Id);
                        }
                    }
                }
            }
        }

        internal void ValidatePosition(GraphicalViewTestItem item, double maxX, double maxY)
        {
            if (item != null)
            {
                var pos = item.Position;
                double x = pos.X;
                double y = pos.Y;

                if (pos.X < 0)
                {
                    x = 0;
                }
                else if (pos.X > (maxX - PointSize * scale))
                {
                    x = maxX - PointSize * scale;
                }


                if (pos.Y < 0)
                {
                    y = 0;
                }
                else if (pos.Y > maxY - PointSize * scale)
                {
                    y = maxY - PointSize * scale;
                }

                if (x != pos.X || y != pos.Y)
                {
                    item.Position = new Point(x, y);
                }
            }
        }

        internal void RecalculateItems(Size previousSize, Size newSize)
        {
            if (recalculateItems)
            {
                if (TestItems != null)
                {
                    var scaleX = newSize.Width / previousSize.Width;
                    var scaleY = newSize.Height / previousSize.Height;

                    foreach (var testItem in TestItems)
                    {
                        testItem.Position = new Point(testItem.Position.X * scaleX, testItem.Position.Y * scaleY);
                    }
                }

                InvalidatePointScale();

                recalculateItems = false;
            }
        }

        private async void GetImageResourcesAsync()
        {
            ImageCollection = await GraphicalViewResourcesHelper.Instance.GetResources();
            NotifyPropertyChanged("ImageCollection");
            EnableSelectImg = true;
            AssignImage(SelectedView);
        }

        private void CheckUsedTablesAsync(GraphicalViewWrapper view)
        {
            Task.Run(() =>
                     {
                         if (validationTables != null && view != null)
                         {
                             var testItems = view.GraphicalViewInfo?.TestItems;
                             foreach (var tableWrapper in validationTables)
                             {
                                 if (testItems != null)
                                 {
                                     var wrapper = tableWrapper;
                                     var tableItems = testItems.Where(x => x.ValidationTableId == wrapper.ValidationTable.Id).ToList();

                                     tableWrapper.IsUsed = tableItems.Count > 0;

                                     foreach (var item in tableItems)
                                     {
                                         if (item.Alias != tableWrapper.ValidationTable.Name)
                                         {
                                             item.Alias = tableWrapper.ValidationTable.Name;
                                         }
                                     }
                                 }
                                 else
                                 {
                                     tableWrapper.IsUsed = false;
                                 }

                                 foreach (var rowWrapper in tableWrapper.Rows)
                                 {
                                     if (testItems != null)
                                     {
                                         var wrapper = rowWrapper;
                                         var rowItems = testItems.Where(x => x.ValidationTableRowId == wrapper.Row.Id).ToList();

                                         rowWrapper.IsUsed = rowItems.Count > 0;

                                         foreach (var item in rowItems)
                                         {
                                             if (item.Alias != rowWrapper.Row.Header)
                                             {
                                                 item.Alias = rowWrapper.Row.Header;
                                             }
                                         }
                                     }
                                     else
                                     {
                                         rowWrapper.IsUsed = false;
                                     }
                                 }
                             }
                         }
                     });
        }

        private void AssignImage(GraphicalViewWrapper graphicalViewWrapper)
        {
            if (graphicalViewWrapper != null && graphicalViewWrapper.Img == null && imageCollection != null &&
                graphicalViewWrapper.GraphicalViewInfo != null
                && !string.IsNullOrEmpty(graphicalViewWrapper.GraphicalViewInfo.ImageFileName))
            {
                var img = ImageCollection.FirstOrDefault(x => x.FileName == graphicalViewWrapper.GraphicalViewInfo.ImageFileName);
                if (img != null)
                {
                    graphicalViewWrapper.Img = img;
                }
            }
        }

        private void RemoveTab(object param)
        {
            if (param is GraphicalViewWrapper view && view.GraphicalViewInfo != null)
            {
                if (MTFMessageBox.ShowConfirmRemoveItem(view.GraphicalViewInfo.ViewName) == MTFMessageBoxResult.Yes)
                {
                    views.Remove(view);
                    SelectedView = null;

                    sequence?.GraphicalViewSetting?.RemoveView(view.GraphicalViewInfo);

                    NotifyPropertyChanged("SelectedTabIndex");
                }
            }
        }


        private List<DutInfo> GetDutInfos(MTFSequence mtfSequence)
        {
            DutInfos?.ForEach(x => x.PropertyChanged -= DutInfoPropertyChanged);

            return mtfSequence?.DeviceUnderTestInfos?.Select(x =>
                                                             {
                                                                 var dutInfo = new DutInfo
                                                                               {
                                                                                   Id = x.Id,
                                                                                   Name = x.Name,
                                                                                   IsSelected = SelectedView?.GraphicalViewInfo?.AssignedDuts
                                                                                                    ?.Contains(x.Id) ?? false,
                                                                               };
                                                                 dutInfo.PropertyChanged += DutInfoPropertyChanged;
                                                                 return dutInfo;
                                                             }).ToList();
        }

        private void DutInfoPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DutInfo.IsSelected) && SelectedView?.GraphicalViewInfo != null)
            {
                SelectedView.GraphicalViewInfo.AssignedDuts = DutInfos.Where(x => x.IsSelected).Select(x => x.Id).ToList();
            }
        }

        private List<ValidationTableWrapper> GetTables(MTFSequence sequence)
        {
            if (sequence == null)
            {
                return null;
            }

            var output = new List<ValidationTableWrapper>();
            if (sequence.MTFVariables != null)
            {
                output.AddRange(GetTables(sequence.MTFVariables, sequence.Name));
            }

            if (sequence.ExternalSubSequences != null)
            {
                foreach (var sequenceInfo in sequence.ExternalSubSequences)
                {
                    if (sequenceInfo.ExternalSequence?.MTFVariables != null)
                    {
                        output.AddRange(GetTables(sequenceInfo.ExternalSequence.MTFVariables, sequenceInfo.ExternalSequence.Name));
                    }
                }
            }

            return output;
        }

        private IEnumerable<ValidationTableWrapper> GetTables(IEnumerable<MTFVariable> variables, string sequenceName)
        {
            return variables.Where(x => x.HasValidationTable).Select(x => new ValidationTableWrapper
                                                                          {
                                                                              SequenceName = sequenceName,
                                                                              ValidationTable = (MTFValidationTable)x.Value,
                                                                              Rows = ((MTFValidationTable)x.Value).Rows
                                                                                  .Select(r => new ValidationRowWrapper(r,
                                                                                              ((MTFValidationTable)x.Value).Id)).ToList()
                                                                          });
        }

        private void LoadViews()
        {
            if (sequence.GraphicalViewSetting != null && sequence.GraphicalViewSetting.HasView)
            {
                Views.Clear();

                foreach (var info in sequence.GraphicalViewSetting.Views)
                {
                    Views.Add(new GraphicalViewWrapper
                              {
                                  GraphicalViewInfo = info,
                                  IsNew = false,
                              });
                }

                Views.Add(new GraphicalViewWrapper());

                if (Views.Count > 1)
                {
                    SelectedView = Views[0];
                }
            }
        }

        private void ExpandTable(object obj)
        {
            if (obj is ValidationTableWrapper table)
            {
                table.IsCollapsed = !table.IsCollapsed;
            }
        }

        private void InvalidatePointScale()
        {
            NotifyPropertyChanged(nameof(PointSizeTransform));
            NotifyPropertyChanged(nameof(PointStrokeTransform));
        }
    }
}