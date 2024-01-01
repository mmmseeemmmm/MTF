using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using MTFApp.SequenceEditor;
using MTFApp.UIHelpers.DragAndDrop;
using MTFClientServerCommon;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MTFApp.UIControls.ActivityTargetControl
{
    /// <summary>
    /// Interaction logic for ActivityTarget.xaml
    /// </summary>
    public partial class ActivityTarget : UserControl, INotifyPropertyChanged
    {
        private Point? startPoint = null;
        private DragAndDrop dragAndDrop = DragAndDrop.Instance;
        private bool allowAllSubSequences;
        private bool allowActivity;
        private TouchHelper touch;
        readonly SettingsClass setting = StoreSettings.GetInstance.SettingsClass;

        public ActivityTarget()
        {
            InitializeComponent();
            Root.DataContext = this;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == DataContextProperty && e.NewValue != null)
            {
                NotifyPropertyChanged(nameof(ListActivities));
            }

            base.OnPropertyChanged(e);
        }

        public MTFSequenceActivity Activity
        {
            get => (MTFSequenceActivity)GetValue(ActivityProperty);
            set => SetValue(ActivityProperty, value);
        }

        public static readonly DependencyProperty ActivityProperty =
            DependencyProperty.Register("Activity", typeof(MTFSequenceActivity), typeof(ActivityTarget),
                new FrameworkPropertyMetadata {DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, BindsTwoWayByDefault = true});


        public bool IsVerticalControl
        {
            get => (bool)GetValue(IsVerticalControlProperty);
            set => SetValue(IsVerticalControlProperty, value);
        }

        public static readonly DependencyProperty IsVerticalControlProperty =
            DependencyProperty.Register("IsVerticalControl", typeof(bool), typeof(ActivityTarget),
                new FrameworkPropertyMetadata(false) {BindsTwoWayByDefault = false});

        public bool IsComboBoxVisible
        {
            get => (bool)GetValue(IsComboBoxVisibleProperty);
            set => SetValue(IsComboBoxVisibleProperty, value);
        }

        public static readonly DependencyProperty IsComboBoxVisibleProperty =
            DependencyProperty.Register("IsComboBoxVisible", typeof(bool), typeof(ActivityTarget),
                new FrameworkPropertyMetadata(false) {BindsTwoWayByDefault = false});

        public IEnumerable<MTFSequenceActivity> ListActivities
        {
            get { return ActivitiesByCall?.OrderBy(x => x.ActivityName); }
        }

        public IList<MTFSequenceActivity> ActivitiesByCall
        {
            private get { return (IList<MTFSequenceActivity>)GetValue(ActivitiesByCallProperty); }
            set { SetValue(ActivitiesByCallProperty, value); }
        }

        public static readonly DependencyProperty ActivitiesByCallProperty =
            DependencyProperty.Register("ActivitiesByCall", typeof(IList<MTFSequenceActivity>), typeof(ActivityTarget),
                new FrameworkPropertyMetadata {DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged});

        public bool AllowAllSubSequences
        {
            get => allowAllSubSequences;
            set => allowAllSubSequences = value;
        }

        public bool AllowActivity
        {
            get => allowActivity;
            set => allowActivity = value;
        }


        private void ActivityTarget_MouseMove(object sender, MouseEventArgs e)
        {
            if (startPoint == null || !setting.AllowDragDrop)
            {
                return;
            }

            Point mousePos = e.GetPosition(null);
            Vector diff = (Point)startPoint - mousePos;
            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                 Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                if (sender is FrameworkElement dragElement)
                {
                    DataObject dragData = new DataObject(AllowActivity ? DragAndDropTypes.GetActivityResult : DragAndDropTypes.GetActivityToCall,
                        new object());
                    var result = DragDrop.DoDragDrop(dragElement, dragData, DragDropEffects.All);
                    if (result != DragDropEffects.None)
                    {
                        if (AllowActivity)
                        {
                            if (dragData.GetData(DragAndDropTypes.GetActivityResult) is MTFSequenceActivity activity)
                            {
                                Activity = activity;
                            }
                        }
                        else
                        {
                            if (dragData.GetData(DragAndDropTypes.GetActivityToCall) is MTFSubSequenceActivity subActivity
                                && (allowAllSubSequences || subActivity.ExecutionType == ExecutionType.ExecuteByCall))
                            {
                                Activity = subActivity;
                            }
                        }
                    }
                }
            }
        }

        private void ActivityTarget_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (touch == null)
                {
                    touch = TouchHelper.Instance;
                }

                touch.SetAction(TouchUpdateAction, sender);
            }
            else
            {
                dragAndDrop.DisableDragAndDrop();
                startPoint = e.GetPosition(null);
            }
        }

        private void TouchUpdateAction(object param, object source)
        {
            if (AllowActivity)
            {
                if (param is MTFSequenceActivity activity)
                {
                    Activity = activity;
                }
            }
            else
            {
                if (param is MTFSubSequenceActivity subActivity &&
                    (allowAllSubSequences || subActivity.ExecutionType == ExecutionType.ExecuteByCall))
                {
                    Activity = subActivity;
                }
            }
        }

        private void ActivityTarget_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            startPoint = null;
        }

        private void RemoveActivityButtonClick(object sender, RoutedEventArgs e)
        {
            Activity = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}