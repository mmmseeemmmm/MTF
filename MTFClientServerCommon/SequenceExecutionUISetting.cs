using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MTFClientServerCommon
{
    [Serializable]
    public class SequenceExecutionUISetting : MTFDataTransferObject
    {
        public SequenceExecutionUISetting()
        {
            AllowTableView = true;
            //AllowTimeView = true;
            AllowTreeView = true;
            AllowGraphicalView = true;

            setDefaultValues();
        }

        private void setDefaultValues()
        {
            TreeViewShowPixtureBox = true;
            TreeViewShowCurrentActivity = true;
            TableViewShowPixtureBox = true;
            TableViewShowCurrentActivity = true;
            GraphicalViewShowPixtureBox = true;
            GraphicalViewShowCurrentActivity = true;
        }

        public SequenceExecutionUISetting(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        public override string ObjectVersion
        {
            get { return "1.2.0"; }
        }

        protected override void VersionConvert(string fromVersion)
        {
            base.VersionConvert(fromVersion);

            if (fromVersion == "1.0.0")
            {
                AllowGraphicalView = true;
                fromVersion = "1.1.0";
            }

            if (fromVersion == "1.1.0")
            {
                setDefaultValues();
                fromVersion = "1.2.0";
            }
        }

        public bool AllowTreeView
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool TreeViewShowPixtureBox
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool TreeViewShowCurrentActivity
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        //public bool AllowTimeView
        //{
        //    get { return GetProperty<bool>(); }
        //    set { SetProperty(value); }
        //}

        public bool AllowTableView
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool TableViewShowPixtureBox
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool TableViewShowCurrentActivity
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool AllowGraphicalView
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool GraphicalViewShowPixtureBox
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool GraphicalViewShowCurrentActivity
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public IList<ClientUiIdentification> SelectedClientUis
        {
            get { return GetProperty<List<ClientUiIdentification>>(); }
            set { SetProperty(value); }
        }
    }

    [Serializable]
    public class ClientUiIdentification : MTFDataTransferObject
    {
        public ClientUiIdentification()
        {
            
        }

        public ClientUiIdentification(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            
        }
        public string AssemblyName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string TypeName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool IsActive
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }
    }
}
