using System;
using System.Collections.Generic;
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

namespace MTFApp.UIControls.DateTimePicker
{
    /// <summary>
    /// Interaction logic for DateTimePicker.xaml
    /// </summary>
    public partial class DateTimePicker : UserControl, INotifyPropertyChanged
    {
        public DateTimePicker()
        {
            InitializeComponent();
            Root.DataContext = this;
        }

        public DateTime SelectedDateTime
        {
            get => (DateTime)GetValue(SelectedDateTimeProperty);
            set => SetValue(SelectedDateTimeProperty, value);
        }

        public static readonly DependencyProperty SelectedDateTimeProperty =  
            DependencyProperty.Register("SelectedDateTime", typeof(DateTime), typeof(DateTimePicker), 
                new FrameworkPropertyMetadata {BindsTwoWayByDefault = true, PropertyChangedCallback = SelectedDateTimeChanged});

        private static void SelectedDateTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DateTimePicker dateTimePicker = d as DateTimePicker;
            if (dateTimePicker == null)
                return;

            dateTimePicker.OnPropertyChanged(nameof(DateTime));
            dateTimePicker.OnPropertyChanged(nameof(DateTimeHour));
            dateTimePicker.OnPropertyChanged(nameof(DateTimeMinute));
        }

        public DateTime DateTime
        {
            get => SelectedDateTime;
            set
            {
                SelectedDateTime = SetDateTimeDate(SelectedDateTime, value);
                OnPropertyChanged();
            }
        }

        public int DateTimeHour
        {
            get => SelectedDateTime.Hour;
            set
            {
                SelectedDateTime = SetDateTimeHour(SelectedDateTime, value < 0 ? 0 : value > 23 ? 23 : value);
                OnPropertyChanged();
            }
        }

        public int DateTimeMinute
        {
            get => SelectedDateTime.Minute;
            set
            {
                SelectedDateTime = SetDateTimeMinute(SelectedDateTime, value < 0 ? 0 : value > 59 ? 59 : value);
                OnPropertyChanged();
            }
        }

        private static DateTime SetDateTimeHour(DateTime dateTime, int hour) => new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, hour, dateTime.Minute, dateTime.Second);
        private static DateTime SetDateTimeMinute(DateTime dateTime, int minute) => new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, minute, dateTime.Second);
        private static DateTime SetDateTimeDate(DateTime dateTime, DateTime newDate) => new DateTime(newDate.Year, newDate.Month, newDate.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    

}
