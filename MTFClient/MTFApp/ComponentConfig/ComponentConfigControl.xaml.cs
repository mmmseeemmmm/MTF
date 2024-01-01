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
using MTFClientServerCommon;
using System.Collections.ObjectModel;
using MTFApp.UIHelpers;

namespace MTFApp.ComponentConfig
{
    /// <summary>
    /// Interaction logic for ComponentConfigControl.xaml
    /// </summary>
    public partial class ComponentConfigControl : MTFUserControl
    {
        public ComponentConfigControl()
        {
            InitializeComponent();
        }

        private void ClassCategoriesSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeView t = sender as TreeView;
            ((ComponentConfigPresenter)t.DataContext).SelectedTreeViewItem = t.SelectedItem;
        }

        private void ListView_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            TreeView t = (TreeView)sender;

            var item = ItemsControl.ContainerFromElement(t, e.OriginalSource as DependencyObject) as TreeViewItem;
            if (item != null)
            {
                ((ComponentConfigPresenter)this.DataContext).SelectedMonsterClass = item.Header as MTFClassInfo;
            }
        }


        private void TextBlock_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            var textblock = sender as TextBlock;
            textblock.Foreground = textblock.Text == "OK" ? Brushes.Green : Brushes.Red;
        }

        private bool componentConfigurationNameInProgress = false;
        private void ComponentConfigurationName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (componentConfigurationNameInProgress)
            {
                return;
            }

            componentConfigurationNameInProgress = true;
            TextBox textBox = (TextBox)sender;
            if (((ComponentConfigPresenter)this.DataContext).SelectedComponentCfg is MTFClassInstanceConfiguration)
            {
                var caretIndex = textBox.CaretIndex;
                ((ComponentConfigPresenter)this.DataContext).UpdateConfigurationName((MTFClassInstanceConfiguration)((ComponentConfigPresenter)this.DataContext).SelectedComponentCfg);
                textBox.CaretIndex = caretIndex;
            }
            componentConfigurationNameInProgress = false;
        }

        private void UIElement_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            UIHelper.RaiseScrollEvent(sender, e);
        }
    }
}
