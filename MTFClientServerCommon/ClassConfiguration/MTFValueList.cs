using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon
{
    [Serializable]
    public class MTFValueList : MTFDataTransferObject
    {
        public MTFValueList()
            : base()
        {
        }
        public MTFValueList(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public MTFValueList(string name, string subListSeparator, IList<Tuple<string, object>> items)
        {
            this.Name = name;
            this.SubListSeparator = subListSeparator;
            this.Items = new List<MTFValueListItem>();
            foreach (var item in items)
            {
                Items.Add(new MTFValueListItem { DisplayName= item.Item1, Value = item.Item2 });
            }
        }

        public string Name
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string SubListSeparator
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public IList<MTFValueListItem> Items
        {
            get { return GetProperty<IList<MTFValueListItem>>(); }
            set { SetProperty(value); }
        }

        public override string ToString()
        {
            return this.Name;
        }
    }

    [Serializable]
    public class MTFValueListItem: ISemradList
    {
        public string DisplayName { get; set; }
        public object Value { get; set; }
    }
}
