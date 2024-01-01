using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace MTFApp.ReportViewer
{
    /// <summary>
    /// Interaction logic for ReportImageGrid.xaml
    /// </summary>
    public partial class ReportImageGrid : UserControl, INotifyPropertyChanged
    {
        public ReportImageGrid()
        {
            Images = new ObservableCollection<BitmapImage>();
            Images.CollectionChanged += ImagesOnCollectionChanged;
            InitializeComponent();
        }

        public ObservableCollection<BitmapImage> Images
        {
            get => (ObservableCollection<BitmapImage>)GetValue(ImagesProperty);
            set => SetValue(ImagesProperty, value);
        }

        public static readonly DependencyProperty ImagesProperty =
            DependencyProperty.Register("Images", typeof(ObservableCollection<BitmapImage>), typeof(ReportImageGrid), new PropertyMetadata(null));

        private void ImagesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Columns = Images.Count > 3 ? 3 : Images.Count;
            OnPropertyChanged(nameof(Columns));
        }

        public int Columns { get; private set; }

        private void UIElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var image = sender as Image;
            if (image == null)
                return;

            Window w = new Window
            {
                Width = Application.Current.MainWindow.Width,
                Height = Application.Current.MainWindow.Height,
                Owner = Application.Current.MainWindow,
                ShowInTaskbar = false,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                Background = Brushes.Black,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
            };

            w.MouseLeftButtonUp += (o, args) => w.Close();

            w.Show();

            w.Content = new Image
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Source = image.Source,
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
