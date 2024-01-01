using System;
using AutomotiveLighting.MTFCommon;
using MTFApp.UIHelpers;

namespace MTFApp.ExportSequence
{
    public class ComponentWrapper : NotifyPropertyBase
    {
        public Guid Guid { get; set; }
        public string Alias { get; set; }
        public MTFIcons Icon { get; set; }

        private bool export;

        public bool Export
        {
            get { return export; }
            set
            {
                export = value;
                NotifyPropertyChanged();
            }
        }
    }
}
