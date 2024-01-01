using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace MTFClientServerCommon
{
    [Serializable]
    public abstract class MTFPersist : MTFDataTransferObject
    {

        public MTFPersist()
            : base()
        {
            IsNew = true;
        }

        public MTFPersist(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }

        public bool IsNew
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsDeleted
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public void Delete()
        {
            IsDeleted = true;
        }

        public DateTime LastPersistTime
        {
            get { return GetProperty<DateTime>(); }
            set { SetProperty(value); }
        }

        public Guid LastPersistId
        {
            get { return GetProperty<Guid>(); }
            set { SetProperty(value); }
        }

        protected override void SetProperty(object value, [CallerMemberName] string propertyName = null)
        {
            base.SetProperty(value, propertyName);
            if (propertyName != "IsModified" && propertyName != "IsNew")
            {
                IsModified = true;
            }
        }

        protected override void ChildMemberChanged(string memberName, string memberPropertyName)
        {
            base.ChildMemberChanged(memberName, memberPropertyName);
            this.IsModified = true;
        }
    }
}
