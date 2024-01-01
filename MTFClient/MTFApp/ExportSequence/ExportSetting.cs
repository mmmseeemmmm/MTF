using System;
using MTFApp.UIHelpers;

namespace MTFApp.ExportSequence
{
    public class ExportSetting : NotifyPropertyBase
    {
        private string alias;
        private bool export;

        public Guid Guid { get; set; }
        public string Name { get; set; }

        public bool Export
        {
            get { return export; }
            set
            {
                export = value;
                NotifyPropertyChanged();
            }
        }

        public string Alias
        {
            get { return alias ?? Name; }
            set
            {
                alias = value;
                NotifyPropertyChanged();
            }
        }

        public ExportSetting(Guid guid, string name, bool export)
        {
            this.Guid = guid;
            this.Name = name;
            this.export = export;
        }
    }
}