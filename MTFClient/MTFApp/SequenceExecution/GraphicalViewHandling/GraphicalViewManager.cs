using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MTFApp.SequenceExecution.TableHandling;
using MTFApp.UIHelpers;
using MTFClientServerCommon;
using MTFClientServerCommon.GraphicalView;
using MTFClientServerCommon.MTFValidationTable;

namespace MTFApp.SequenceExecution.GraphicalViewHandling
{
    class GraphicalViewManager : NotifyPropertyBase
    {
        private readonly MTFClient client;
        private readonly Dictionary<Guid, List<ExecutionGraphicalViewWrapper>> views = new Dictionary<Guid, List<ExecutionGraphicalViewWrapper>>();
        private List<GraphicalViewImg> images = new List<GraphicalViewImg>();
        private ExecutionGraphicalViewWrapper currentView;
        private Guid defaultDutId = new Guid("2DD8CDB3-DCED-4564-814C-313177277E7A");
        private List<Guid> dutIds;

        public GraphicalViewManager(MTFClient client)
        {
            this.client = client;
        }

        private List<ExecutionGraphicalViewWrapper> Views(Guid? dutId) => views[dutId ?? defaultDutId];

        internal void Init(MTFSequence sequence, TableManager tableManager)
        {
            Clear();
            if (sequence.DeviceUnderTestInfos != null && sequence.DeviceUnderTestInfos.Count > 0)
            {
                dutIds = sequence.DeviceUnderTestInfos.Select(d => d.Id).ToList();
                defaultDutId = sequence.DeviceUnderTestInfos[0].Id;
                foreach (var dut in sequence.DeviceUnderTestInfos)
                {
                    views[dut.Id] = new List<ExecutionGraphicalViewWrapper>();
                }
            }
            else
            {
                dutIds = new List<Guid> { defaultDutId };
                views[defaultDutId] = new List<ExecutionGraphicalViewWrapper>();
            }


            if (sequence != null && sequence.GraphicalViewSetting != null && sequence.GraphicalViewSetting.HasView)
            {
                LoadImages(sequence.GraphicalViewSetting.Views);
                CreateViews(sequence.GraphicalViewSetting.Views, tableManager);
            }
        }

        public ExecutionGraphicalViewWrapper GetGraphicalView(Guid? graphicalViewId, Guid? dutId) =>
            graphicalViewId == null
                ? Views(dutId).FirstOrDefault()
                : Views(dutId).FirstOrDefault(x => x.Id.Equals(graphicalViewId));
        public ExecutionGraphicalViewWrapper GetFirstGraphicalView(Guid? dutId) => Views(dutId).FirstOrDefault();

        

        private void LoadImages(List<GraphicalViewInfo> graphicalViewInfos)
        {
            images = client.LoadGraphicalViewImages(graphicalViewInfos.Select(x => x.ImageFileName));
        }

        private void CreateViews(List<GraphicalViewInfo> graphicalViewInfos, TableManager tableManager)
        {
            foreach (var dutId in dutIds)
            {
                foreach (var viewInfo in graphicalViewInfos)
                {
                    var view = new ExecutionGraphicalViewWrapper
                    {
                        Id = viewInfo.Id,
                        ViewName = viewInfo.ViewName,
                        ImgData = AssignImage(viewInfo.ImageFileName),
                        TestItems = AssignTestedItems(viewInfo.TestItems, tableManager.ExistingTables(dutId))
                    };
                    Views(dutId).Add(view);
                }
            }
        }

        private List<ExecutionGraphicalViewTestedItem> AssignTestedItems(ObservableCollection<GraphicalViewTestItem> source, List<ExecutionValidTable> tables)
        {
            return source?.Select(x => AssignTestItem(x, tables)).Where(x => x.ValidationTable != null).ToList();
        }

        private ExecutionGraphicalViewTestedItem AssignTestItem(GraphicalViewTestItem graphicalViewTestItem, IList<ExecutionValidTable> tables)
        {
            var item = new ExecutionGraphicalViewTestedItem
                       {
                           Position = graphicalViewTestItem.Position,
                           Alias = graphicalViewTestItem.Alias,
                           ValidationTable = tables.FirstOrDefault(t => t.Id == graphicalViewTestItem.ValidationTableId),
                       };

            if (graphicalViewTestItem.ValidationTableRowId == Guid.Empty)
            {
                item.ExecutionItem = item.ValidationTable;
            }
            else
            {
                item.AssignRow(graphicalViewTestItem.ValidationTableRowId);
            }

            return item;
        }

        private byte[] AssignImage(string imageFileName)
        {
            if (images != null)
            {
                var img = images.FirstOrDefault(x => x.FileName == imageFileName);
                return img != null ? img.Data : null;
            }
            return null;
        }
        
        
        public void Clear()
        {
            views.Clear();
        }
    }
}