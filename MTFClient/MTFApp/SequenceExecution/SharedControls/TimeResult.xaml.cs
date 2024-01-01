using System;
using System.Windows;
using System.Windows.Controls;

namespace MTFApp.SequenceExecution.SharedControls
{
    /// <summary>
    /// Interaction logic for TimeResult.xaml
    /// </summary>
    public partial class TimeResult : UserControl
    {
        public TimeResult()
        {
            InitializeComponent();
            Root.DataContext = this;
        }



        public DateTime StartTime
        {
            get { return (DateTime)GetValue(StartTimeProperty); }
            set { SetValue(StartTimeProperty, value); }
        }

        public static readonly DependencyProperty StartTimeProperty =
            DependencyProperty.Register("StartTime", typeof(DateTime), typeof(TimeResult), 
            new FrameworkPropertyMetadata{BindsTwoWayByDefault = false});



        public double Duration
        {
            get { return (double)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }

        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register("Duration", typeof(double), typeof(TimeResult), new FrameworkPropertyMetadata { BindsTwoWayByDefault = false });




        public double TimeStamp
        {
            get { return (double)GetValue(TimeStampProperty); }
            set { SetValue(TimeStampProperty, value); }
        }

        public static readonly DependencyProperty TimeStampProperty =
            DependencyProperty.Register("TimeStamp", typeof(double), typeof(TimeResult), new FrameworkPropertyMetadata { BindsTwoWayByDefault = false });


        public bool UseTimeStamp { get; set; }
        
    }
}
