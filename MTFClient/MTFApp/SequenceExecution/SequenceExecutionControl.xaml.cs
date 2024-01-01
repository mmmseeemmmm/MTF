using MTFApp.UIHelpers;
using System.Windows.Controls;
using System.Windows.Input;

namespace MTFApp.SequenceExecution
{
    /// <summary>
    /// Interaction logic for SequenceExecution.xaml
    /// </summary>
    public partial class SequenceExecutionControl : MTFUserControl
    {
        public SequenceExecutionControl()
        {
            InitializeComponent();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            listBox.ScrollIntoView(listBox.SelectedItem);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox t = sender as TextBox;
            if (t == null)
            {
                return;
            }

            t.ScrollToEnd();
        }
    }
}
