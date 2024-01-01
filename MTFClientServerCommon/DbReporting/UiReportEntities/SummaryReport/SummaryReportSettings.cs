using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace MTFClientServerCommon.DbReporting.UiReportEntities.SummaryReport
{
    [Serializable]
    public class SummaryReportSettings : ICloneable
    {
        public SummaryReportSettings()
        {
            Filter = new ReportFilter{Last24Hours = true};
            Panels = new ObservableCollection<PanelBase>();
            Id = -1;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public int Index { get; set; }
        public bool IsPinned { get; set; }
        public ReportFilter Filter { get; set; }
        public bool CanModifyFilterInView { get; set; }
        public ObservableCollection<PanelBase> Panels { get; set; }
        public object Clone()
        {
            return new SummaryReportSettings
            {
                Name = Name,
                Title = Title,
                Index = Index,
                IsPinned = IsPinned,
                Filter = Filter.Clone() as ReportFilter,
                CanModifyFilterInView = CanModifyFilterInView,
                Panels = new ObservableCollection<PanelBase>(Panels.Select(p => p.Clone() as PanelBase)),
            };
        }
    }
}
