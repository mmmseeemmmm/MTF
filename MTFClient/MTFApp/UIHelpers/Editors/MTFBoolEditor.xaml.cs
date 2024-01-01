using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MTFApp.UIHelpers.Editors
{
    /// <summary>
    /// Interaction logic for MTFBoolEditor.xaml
    /// </summary>
    public partial class MTFBoolEditor : MTFEditorBase
    {
        public MTFBoolEditor()
        {
            InitializeComponent();

            root.DataContext = this;
        }
    }
}
