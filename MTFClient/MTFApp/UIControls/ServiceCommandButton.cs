using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using MTFClientServerCommon;
using System.Windows.Media;

namespace MTFApp.UIControls
{
    public class ServiceCommandButton : Button, INotifyPropertyChanged
    {

        public MTFServiceCommandIcon Icon
        {
            get { return (MTFServiceCommandIcon)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(MTFServiceCommandIcon), typeof(ServiceCommandButton), new FrameworkPropertyMetadata());


        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(ServiceCommandButton), new PropertyMetadata(string.Empty));



        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(ServiceCommandButton), new FrameworkPropertyMetadata());


        public Brush CheckedBrush
        {
            get { return (Brush)GetValue(CheckedBrushProperty); }
            set { SetValue(CheckedBrushProperty, value); }
        }

        public static readonly DependencyProperty CheckedBrushProperty =
            DependencyProperty.Register("CheckedBrush", typeof(Brush), typeof(ServiceCommandButton), new FrameworkPropertyMetadata());


        public bool ShowLabel
        {
            get { return (bool)GetValue(ShowLabelProperty); }
            set { SetValue(ShowLabelProperty, value); }
        }

        public static readonly DependencyProperty ShowLabelProperty =
            DependencyProperty.Register("ShowLabel", typeof(bool), typeof(ServiceCommandButton), new FrameworkPropertyMetadata());


        public Brush UnCheckedBrush
        {
            get { return (Brush)GetValue(UnCheckedBrushProperty); }
            set { SetValue(UnCheckedBrushProperty, value); }
        }

        public static readonly DependencyProperty UnCheckedBrushProperty =
            DependencyProperty.Register("UnCheckedBrush", typeof(Brush), typeof(ServiceCommandButton), new FrameworkPropertyMetadata());


        public bool IsHorizontal { get; private set; }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == ActualWidthProperty)
            {
                IsHorizontal = ActualHeight * 2 < ActualWidth;
                NotifyPropertyChanged("IsHorizontal");
            }
            base.OnPropertyChanged(e);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
