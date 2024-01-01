using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MTFClientServerCommon;

namespace MTFApp.FindUsages
{
    /// <summary>
    /// Interaction logic for FindActivityControl.xaml
    /// </summary>
    public partial class FindActivityControl : UserControl
    {
        public FindActivityControl()
        {
            InitializeComponent();
        }

        private void ItemsListBox_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox != null)
            {
                var tupple = listBox.SelectedItem as Tuple<MTFSequenceActivity, MTFSequence>;
                if (tupple != null)
                {
                    var findUsageBase = DataContext as FindUsagesBase;
                    if (findUsageBase!=null)
                    {
                        findUsageBase.Select(tupple.Item1, tupple.Item2);
                    }
                }
            }
        }

        private void ItemsListBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F12)
            {
                ItemsListBox_OnMouseDoubleClick(sender, null);
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            var findUsageBase = DataContext as FindUsagesBase;
            if (findUsageBase != null)
            {
                findUsageBase.MinimizeButtonClick(sender, e);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            var findUsageBase = DataContext as FindUsagesBase;
            if (findUsageBase != null)
            {
                findUsageBase.CloseButtonClick(sender, e);
            }
        }
    }
}
