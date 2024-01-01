using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace MTFApp.TermDesigner
{
    /// <summary>
    /// Interaction logic for TermDesignerWindow.xaml
    /// </summary>
    public partial class TermDesignerWindow : Window, INotifyPropertyChanged
    {
        private double originalWidth;
        private double originalHeight;

        public TermDesignerWindow()
        {

        }
        public TermDesignerWindow(MTFApp.UIHelpers.Editors.MTFTermDesigner termDesignerControl, UserControl parentControl)
        {
            this.DataContext = this;
            this.Owner = App.Current.MainWindow;
            InitializeComponent();
            this.Width = 1800;
            this.Height = 1000;
            this.root.Content = termDesignerControl;

            originalHeight = this.Height;
            originalWidth = this.Width;

            if (termDesignerControl is MTFApp.PopupWindow.IRaiseCloseEvent)
            {
                ((MTFApp.PopupWindow.IRaiseCloseEvent)termDesignerControl).Close += (s) =>
                {
                    this.Term = termDesignerControl.Term;
                    this.Close();
                };
            }
        }

        public MTFClientServerCommon.Mathematics.Term Term { get; private set; }

        private double scale;

        public double Scale
        {
            get => scale;
            set
            {
                scale = value;
                this.Height = originalHeight * value;
                this.Width = originalWidth * value;
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
