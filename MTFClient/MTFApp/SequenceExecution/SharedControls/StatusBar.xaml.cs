using System.Windows.Controls;

namespace MTFApp.SequenceExecution.SharedControls
{
    /// <summary>
    /// Interaction logic for StatusBar.xaml
    /// </summary>
    public partial class StatusBar : UserControl
    {
        public StatusBar()
        {
            InitializeComponent();
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
