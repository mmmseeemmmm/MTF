using System;
using System.Collections.Generic;

namespace MTFApp.UIHelpers.Editors
{
    /// <summary>
    /// Interaction logic for MTFListOperationEditor.xaml
    /// </summary>
    public partial class MTFListOperationEditor : MTFEditorBase
    {
        public MTFListOperationEditor()
        {
            InitializeComponent();
            root.DataContext = this;
        }

        public string ListType
        {
            get
            {
                return typeof(List<MTFClientServerCommon.Mathematics.Term>).FullName;
            }
        }
    }
}
