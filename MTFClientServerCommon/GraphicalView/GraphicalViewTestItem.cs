using System;
using System.Runtime.Serialization;
using System.Windows;

namespace MTFClientServerCommon.GraphicalView
{
    [Serializable]
    public class GraphicalViewTestItem : MTFDataTransferObject
    {
        public GraphicalViewTestItem()
            : base()
        {
        }

        public GraphicalViewTestItem(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public Point Position
        {
            get => GetProperty<Point>();
            set => SetProperty(value);
        }

        public Guid ValidationTableId
        {
            get => GetProperty<Guid>();
            set => SetProperty(value);
        }

        public Guid ValidationTableRowId
        {
            get => GetProperty<Guid>();
            set => SetProperty(value);
        }

        public string Alias
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public GraphicalViewTestItemType Type
        {
            get => GetProperty<GraphicalViewTestItemType>();
            set => SetProperty(value);
        }
    }
}