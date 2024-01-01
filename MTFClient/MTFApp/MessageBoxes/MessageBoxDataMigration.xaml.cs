using MTFClientServerCommon;
using MTFClientServerCommon.Helpers;
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
using System.Windows.Shapes;

namespace MTFApp.MessageBoxes
{
    /// <summary>
    /// Interaction logic for MessageBoxDataMigration.xaml
    /// </summary>
    public partial class MessageBoxDataMigration : Window
    {
        public MessageBoxDataMigration(string previousMTFVersion)
        {
            InitializeComponent();
            message.Text = string.Format(LanguageHelper.GetString("Mtf_Data_Migration_Message"), previousMTFVersion);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        public DataMigrationType Result { get; private set; }

        private void ButtonCopy_Click(object sender, RoutedEventArgs e)
        {
            Result = DataMigrationType.Copy;
            Close();
        }

        private void ButtonMove_Click(object sender, RoutedEventArgs e)
        {
            Result = DataMigrationType.Move;
            Close();
        }

        private void ButtonDoNothing_Click(object sender, RoutedEventArgs e)
        {
            Result = DataMigrationType.DoNothing;
            Close();
        }
    }
}
