using MTFClientServerCommon;
using MTFClientServerCommon.Mathematics;
using System;

using System.Linq;
using System.Windows;
using System.Windows.Input;
using MTFApp.UIHelpers.DragAndDrop;

namespace MTFApp.UIHelpers.Editors
{
    /// <summary>
    /// Interaction logic for MTFObjectEditor.xaml
    /// </summary>
    public partial class MTFObjectEditor : MTFEditorBase
    {
        private Command createCommand = null;
        Point? targetStartPoint = null;
        private object objectValue;
        private bool updateValue = true;
        private bool updateObjectValue = true;
        private TouchHelper touch;

        public MTFObjectEditor()
        {
            InitializeComponent();

            root.DataContext = this;
            this.PropertyChanged += MTFObjectEditor_PropertyChanged;
            createCommand = new Command(Create, () => classInfo != null && HasParameterLessConstructor);
        }

        void MTFObjectEditor_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ObjectValue" && updateValue)
            {
                updateObjectValue = false;
                if (EditorMode == EditorModes.UseTerm)
                {
                    Value = new TermWrapper() { Value = ObjectValue, TypeName = TypeName };
                }
                else
                {
                    Value = ObjectValue;
                }
                updateObjectValue = true;
            };
        }


        #region TypeName dependency property

        public string TypeName
        {
            get { return (string)GetValue(TypeNameProperty); }
            set { SetValue(TypeNameProperty, value); }
        }

        public static readonly DependencyProperty TypeNameProperty =
            DependencyProperty.Register("TypeName", typeof(string), typeof(MTFObjectEditor),
            new FrameworkPropertyMetadata { BindsTwoWayByDefault = false });

        #endregion TypeName dependency property

        private GenericClassInfo classInfo;

        public GenericClassInfo ClassInfo
        {
            get { return classInfo; }
            set
            {
                classInfo = value;
                createCommand.RaiseCanExecuteChanged();
            }
        }



        public object ObjectValue
        {
            get { return objectValue; }
            set
            {
                objectValue = value;
                NotifyPropertyChanged();
            }
        }


        protected override void OnPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == ValueProperty && updateObjectValue)
            {
                updateValue = false;
                if (e.NewValue is TermWrapper)
                {
                    ObjectValue = (e.NewValue as TermWrapper).Value;
                }
                else
                {
                    ObjectValue = e.NewValue;
                }
                updateValue = true; 
            }
        }

        public bool HasParameterLessConstructor
        {
            get
            {
                if (classInfo == null)
                {
                    return true;
                }
                bool parameterLessConstructor = false;
                foreach (var item in classInfo.Constructors)
                {
                    if (item is GenericConstructorInfo && (item as GenericConstructorInfo).Parameters.Count == 0)
                    {
                        parameterLessConstructor = true;
                    }
                }
                return parameterLessConstructor;
            }
        }

        public ICommand CreateCommand
        {
            get { return createCommand; }
        }

        public ICommand RemoveCommand
        {
            get { return new Command(Remove); }
        }

        private void Create()
        {
            ObjectValue = new GenericClassInstanceConfiguration(classInfo);
            //check if some property use Live semrad's list and prepair data
            foreach (var prop in ((GenericClassInstanceConfiguration)ObjectValue).PropertyValues.Where(p => !string.IsNullOrEmpty(p.ValueListName)))
            {
                prop.AllowedValues = getListValues(prop);
            }
        }

        private MTFObservableCollection<MTFAllowedValue> getListValues(GenericParameterValue property)
        {
            //property.Parent
            var activity = property.GetParent<MTFSequenceActivity>();
            if (activity == null)
            {
                return null;
            }

            if (activity.ClassInfo != null && activity.ClassInfo.MTFClassInstanceConfiguration != null && activity.ClassInfo.MTFClassInstanceConfiguration.ValueLists != null)
            {
                MTFValueList valueList = activity.ClassInfo.MTFClassInstanceConfiguration.ValueLists.FirstOrDefault(vl => vl.Name == property.ValueListName);
                var listValues = new MTFObservableCollection<MTFAllowedValue>();
                foreach (var val in valueList.Items)
                {
                    listValues.Add(new MTFAllowedValue { DisplayName = val.DisplayName, Value = val.Value });
                }

                return listValues;
            }

            return null;
        }

        private void Remove()
        {
            if (MTFMessageBox.Show("Remove object", "Do you want remove object?", MTFMessageBoxType.Question, MTFMessageBoxButtons.YesNo) != MTFMessageBoxResult.Yes)
            {
                return;
            }

            Value = null;
        }

        private void target_PreviewMouseMove(object sender, MouseEventArgs e)
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

        private void target_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (touch == null)
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

        private void AssignActivityResult(object targetActivity, object selectedActivity)
        {
            if (targetActivity != null && targetActivity != selectedActivity)
            {
                ObjectValue = new ActivityResultTerm() { Value = targetActivity as MTFSequenceActivity };
            }
        }

        private void listBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            UIHelper.RaiseScrollEvent(sender, e);
        }
    }
}
