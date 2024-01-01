using MTFClientServerCommon.Helpers;
using System.Collections.Generic;

namespace MTFApp.UIHelpers.Editors
{
    /// <summary>
    /// Interaction logic for MTFEnumEditor.xaml
    /// </summary>
    public partial class MTFEnumEditor : MTFEditorBase
    {
        public MTFEnumEditor()
        {
            InitializeComponent();

            root.DataContext = this;
        }

        public IEnumerable<EnumValueDescription> TableStatuses
        {
            get { return EnumHelper.GetAllValuesAndDescriptions<MTFCommon.MTFValidationTableStatus>(); }
        }
    }
}
