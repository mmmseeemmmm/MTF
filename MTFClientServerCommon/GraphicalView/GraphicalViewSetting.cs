using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MTFClientServerCommon.GraphicalView
{
    [Serializable]
    public class GraphicalViewSetting : MTFDataTransferObject
    {
        public GraphicalViewSetting()
            : base()
        {
        }

        public GraphicalViewSetting(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public bool HasView => Views != null && Views.Count > 0;

        public List<GraphicalViewInfo> Views
        {
            get => GetProperty<List<GraphicalViewInfo>>();
            set => SetProperty(value);
        }

        public void AddView(GraphicalViewInfo info)
        {
            if (Views == null)
            {
                Views = new List<GraphicalViewInfo>();
            }
            Views.Add(info);
        }

        public void RemoveView(GraphicalViewInfo info)
        {
            Views?.Remove(info);
        }
    }
}