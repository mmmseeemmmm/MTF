using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AutomotiveLighting.MTFCommon
{
    [Serializable]
    [KnownType(typeof(List<MTFTabControl>))]
    [KnownType(typeof(List<MTFDataTable>))]
    [KnownType(typeof(MTFDataTable))]
    public class MTFTabControl
    {
        private string header;

        public string Header
        {
            get { return header; }
            set { header = value; }
        }
        private List<object> content;

        public List<object> Content
        {
            get { return content; }
            set { content = value; }
        }
    }
}
