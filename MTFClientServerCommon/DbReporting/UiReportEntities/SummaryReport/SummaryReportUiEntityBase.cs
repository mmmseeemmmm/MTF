using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MTFClientServerCommon.DbReporting.UiReportEntities.SummaryReport
{
    [Serializable]
    public abstract class SummaryReportUiEntityBase : INotifyPropertyChanged, ICloneable
    {
        private bool isDeleted;

        public int Id { get; set; }

        public bool IsDeleted
        {
            get => isDeleted;
            set
            {
                isDeleted = value;
                NotifyPropertyChanged();
            }
        }

        [field: NonSerialized] public event PropertyChangedEventHandler PropertyChanged;

        public abstract object Clone();

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}