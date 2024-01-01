using MTFClientServerCommon.Mathematics;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MTFApp.UIHelpers.DragAndDrop;

namespace MTFApp.UIHelpers.Editors
{
    /// <summary>
    /// Interaction logic for MTFSimpleTermEditor.xaml
    /// </summary>
    public partial class MTFSimpleTermEditor : MTFEditorBase
    {
        private bool updateValue = true;
        private Point? targetStartPoint = null;
        private TouchHelper touch;

        public MTFSimpleTermEditor()
        {
            InitializeComponent();
            root.DataContext = this;
            this.PropertyChanged += MTFSimpleTermEditor_PropertyChanged;
            //var relativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(SequenceEditor.SequenceEditorControl), 1);
            //Binding b = new Binding("DataContext.TermPropertyName");
            //b.RelativeSource = relativeSource;
            //b.Mode = BindingMode.OneWayToSource;
            //BindingOperations.SetBinding(this, TermPropertyNameProperty, b);
        }

        #region TermPropertyName
        public string TermPropertyName
        {
            get { return (string)GetValue(TermPropertyNameProperty); }
            set { SetValue(TermPropertyNameProperty, value); }
        }

        public static readonly DependencyProperty TermPropertyNameProperty =
            DependencyProperty.Register("TermPropertyName", typeof(string), typeof(MTFSimpleTermEditor),
            new FrameworkPropertyMetadata() { BindsTwoWayByDefault = true });
        #endregion

        void MTFSimpleTermEditor_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (Value != Term || updateValue || Term is ConstantTerm)
            {
                Value = Term;
            }
        }

        protected override void OnPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == ValueProperty)
            {
                updateValue = false;
                if (e.NewValue is Term)
                {
                    term = e.NewValue as Term;
                    //term.PropertyChanged += term_PropertyChanged;
                }
                NotifyPropertyChanged("Term");
                updateValue = true; 
            }
        }

        //private void term_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName.Contains("MTFMethodName"))
        //    {
        //        //term.PropertyChanged -= term_PropertyChanged;
        //        term = new EmptyTerm();
        //    }
        //    NotifyPropertyChanged("Term");
        //}

        private void MTFEditor_ValueChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Contains("MTFMethodName"))
            {
                //term.PropertyChanged -= term_PropertyChanged;
                term = new EmptyTerm();
            }
            NotifyPropertyChanged("Term");
        }

        private Term term;

        public Term Term
        {
            get { return term; }
            set
            {
                term = value;
                //NotifyPropertyChanged();
            }
        }

        private void target_MouseMove(object sender, MouseEventArgs e)
        {
            if (targetStartPoint != null && Setting.AllowDragDrop)
            {
                Point mousePos = e.GetPosition(null);
                Vector diff = (Point)targetStartPoint - mousePos;
                if (e.LeftButton == MouseButtonState.Pressed &&
                    (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    var dragElement = sender as FrameworkElement;
                    DataObject dragData = new DataObject(DragAndDropTypes.GetActivityResult, new object());
                    var result = DragDrop.DoDragDrop(dragElement, dragData, DragDropEffects.All);
                    if (result != DragDropEffects.None)
                    {
                        var targetActivity = dragData.GetData(DragAndDropTypes.GetActivityResult);
                        var selectedActivity = dragData.GetData(DragAndDropTypes.SelectedActivity);
                        AssignActivityResult(targetActivity, selectedActivity);
                    }
                }
            }
        }

        private void AssignActivityResult(object targetActivity, object selectedActivity)
        {
            if (targetActivity != null && targetActivity != selectedActivity)
            {
                term = new ActivityResultTerm { Value = targetActivity as MTFClientServerCommon.MTFSequenceActivity };
                NotifyPropertyChanged("Term");
            }
        }

        private void target_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount==2)
            {
                if (touch==null)
                {
                    touch = TouchHelper.Instance;
                }
                touch.SetAction(AssignActivityResult, sender);
            }
            else
            {
                targetStartPoint = e.GetPosition(null); 
            }
        }

        private void target_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            targetStartPoint = null;
        }

        private void RemoveTerm_Click(object sender, RoutedEventArgs e)
        {
            term = new EmptyTerm();
            NotifyPropertyChanged("Term");
        }

        private void OpenDesigner_Click(object sender, RoutedEventArgs e)
        {
            var b = sender as Button;
            if (b != null)
            {
                TermPropertyName = b.Tag as string;
            }
        }
    }
}
