using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace MTFClientServerCommon.GraphicalView
{
    [Serializable]
    public class GraphicalViewInfo : MTFDataTransferObject
    {
        public GraphicalViewInfo()
            : base()
        {
            
        }

        public GraphicalViewInfo(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public string ImageFileName
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public string ViewName
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public ObservableCollection<GraphicalViewTestItem> TestItems
        {
            get => GetProperty<ObservableCollection<GraphicalViewTestItem>>();
            set => SetProperty(value);
        }

        public bool SaveToReport
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        public List<Guid> AssignedDuts
        {
            get => GetProperty<List<Guid>>();
            set => SetProperty(value);
        }

        public float ScreenDipX
        {
            get => GetProperty<float>();
            set => SetProperty(value);
        }

        public float ScreenDipY
        {
            get => GetProperty<float>();
            set => SetProperty(value);
        }

        public double Scale
        {
            get => GetProperty<double>();
            set => SetProperty(value);
        }

    }
}