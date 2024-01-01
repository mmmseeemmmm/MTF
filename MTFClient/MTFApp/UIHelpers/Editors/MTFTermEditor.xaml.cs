using MTFApp.UIHelpers.DragAndDrop;
using MTFClientServerCommon;
using MTFClientServerCommon.Mathematics;
using MTFClientServerCommon.MTFValidationTable;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace MTFApp.UIHelpers.Editors
{
    /// <summary>
    /// Interaction logic for MTFTermEditor.xaml
    /// </summary>
    public partial class MTFTermEditor : MTFEditorBase
    {
        private Point? startPointForTarget = null;
        private Point? termStartPoint = null;
        private DataObject dragData = null;
        private bool updateValue = true;
        private bool dontValidate = false;
        //private Visibility showErrorInTarget = Visibility.Collapsed;
        private TouchHelper touch = TouchHelper.Instance;

        public MTFTermEditor()
        {
            InitializeComponent();
            root.DataContext = this;
            this.PropertyChanged += MTFTermEditor_PropertyChanged;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                if (touch.DataObject != null && touch.DataObject.GetDataPresent("Term"))
                {
                    var termToRemove = touch.DataObject.GetData("Term") as Term;
                    if (termToRemove == term)
                    {
                        term = new EmptyTerm();
                    }
                    else
                    {
                        var parentTerm = term.GetParentTerm(term, termToRemove);
                        if (parentTerm != null)
                        {
                            parentTerm.ChangeChildrenTerm(new EmptyTerm(), termToRemove);
                        }
                    }
                    touch.Clear();
                    NotifyPropertyChanged("Term");
                }
            }
            else
            {
                base.OnPreviewKeyDown(e);
            }
        }

        void MTFTermEditor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Term")
            {
                if (Value != Term || updateValue)
                {
                    Value = Term;
                }
            }
            if (touch != null)
            {
                touch.Clear();
            }
        }

        #region ResultTypeName Dependency Property
        public string ResultTypeName
        {
            get { return (string)GetValue(ResultTypeNameProperty); }
            set { SetValue(ResultTypeNameProperty, value); }
        }
        public static readonly DependencyProperty ResultTypeNameProperty =
            DependencyProperty.Register("ResultTypeName", typeof(string), typeof(MTFTermEditor),
            new FrameworkPropertyMetadata() { BindsTwoWayByDefault = false });
        #endregion ResultTypeName Dependency Property


        protected override void OnPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            updateValue = false;
            if (e.NewValue is Term)
            {
                term = e.NewValue as Term;
                //term.PropertyChanged += term_PropertyChanged;//
            }
            NotifyPropertyChanged("Term");
            updateValue = true;
        }

        //void term_PropertyChanged(object sender, PropertyChangedEventArgs e)
        private void MTFEditor_ValueChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Contains("MTFMethodName"))
            {
                //term.PropertyChanged -= term_PropertyChanged;//
                term = new EmptyTerm();
            }
            else if (term != null && !e.PropertyName.Contains("Term"))
            {
                //if (sender is BinaryTerm)
                //{
                //    ValidateTermsInBinaryTerm(sender as BinaryTerm);
                //}
                //else if (sender is IsInListTerm)
                //{

                //    ValidateTermsInIsInListTerm(sender as IsInListTerm);
                //}
                var propertyNames = e.PropertyName.Split('.');
                if (propertyNames.Length > 1)
                {
                    var termToValidate = GetTermByPropertyPath(term, propertyNames, 0);
                    if (!(termToValidate is BinaryTerm) && !(termToValidate is ListTerm))
                    {
                        termToValidate = term.GetParentTerm(term, termToValidate);
                    }
                    if (termToValidate is BinaryTerm)
                    {
                        ValidateTermsInBinaryTerm(termToValidate as BinaryTerm);
                    }
                    else if (termToValidate is ListTerm)
                    {
                        ValidateTermsInListTerm(termToValidate as ListTerm);
                    }
                }

            }
            NotifyPropertyChanged("Term");
        }

        private Term GetTermByPropertyPath(Term actualTerm, string[] propertyNames, int index)
        {
            if (index < propertyNames.Length - 1)
            {
                var prop = actualTerm.GetType().GetProperty(propertyNames[index]);
                if (prop != null)
                {
                    var val = actualTerm.GetType().GetProperty(propertyNames[index]).GetValue(actualTerm);
                    if (val is Term)
                    {
                        var t = GetTermByPropertyPath(val as Term, propertyNames, index + 1);
                        if (t != null)
                        {
                            return t;
                        }
                    }
                }
            }
            return actualTerm;
        }


        private void ValidateTermsInListTerm(ListTerm listTerm)
        {
            var parent = term.GetParentTerm(term, listTerm);
            if (parent != null)
            {
                ListTerm newTerm = null;
                if (listTerm is IsInListTerm)
                {
                    newTerm = new IsInListTerm();
                }
                else if (listTerm is NotIsInListTerm)
                {
                    newTerm = new NotIsInListTerm();
                }
                if (newTerm != null)
                {
                    newTerm.Value1 = listTerm.Value1;
                    if (listTerm.Value2 != null && listTerm.Value2.GetType().GetElementType() == listTerm.Value1.ResultType)
                    {
                        newTerm.Value2 = listTerm.Value2;
                    }

                    AssignNewTerm(term, listTerm, newTerm);
                }
            }
        }

        private void ValidateTermsInBinaryTerm(BinaryTerm binaryTerm)
        {
            if (dontValidate)
            {
                return;
            }
            bool firstTypeIsValue1;
            if (binaryTerm.HasChildrenOfType<Term, ConstantTerm>(out firstTypeIsValue1)
                )
            {
                if (binaryTerm.Value1.ResultType != binaryTerm.Value2.ResultType && binaryTerm.Value1.ResultType != null && binaryTerm.Value2.ResultType != null)
                {
                    if (firstTypeIsValue1)//Value1 si VariableTerm; Value2 is ConstantTerm
                    {
                        if (!binaryTerm.Value1.ResultType.IsAbstract)
                        {
                            dontValidate = true;
                            binaryTerm.Value2 = UpdateConstantTerm((binaryTerm.Value2 as ConstantTerm).Value, binaryTerm.Value1.ResultType);
                            dontValidate = false;
                        }
                    }
                    else //Value1 si ConstantTerm; Value2 is VariableTerm
                    {
                        if (!binaryTerm.Value2.ResultType.IsAbstract)
                        {
                            dontValidate = true;
                            binaryTerm.Value1 = UpdateConstantTerm((binaryTerm.Value1 as ConstantTerm).Value, binaryTerm.Value2.ResultType);
                            dontValidate = false;
                        }
                    }
                }
            }
            //NotifyPropertyChanged("Term");
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



        //public Visibility ShowErrorInTarget
        //{
        //    get { return showErrorInTarget; }
        //    set
        //    {
        //        showErrorInTarget = value;
        //        NotifyPropertyChanged();
        //    }
        //}


        private void AssignNewTerm(Term parentTerm, Term oldTerm, Term newTerm)
        {
            if (parentTerm == oldTerm)
            {
                term = newTerm;
            }
            else if (parentTerm is UnaryTerm)
            {
                (parentTerm as UnaryTerm).Value = newTerm;
            }
            else if (parentTerm is BinaryTerm)
            {
                BinaryTerm binaryTerm = parentTerm as BinaryTerm;
                if (binaryTerm.Value1 == oldTerm)
                {
                    binaryTerm.Value1 = newTerm;
                    CheckConstantTerm(binaryTerm, true);
                }
                else
                {
                    binaryTerm.Value2 = newTerm;
                    CheckConstantTerm(binaryTerm, false);
                }
            }
            else if (parentTerm is ListTerm)
            {
                ListTerm newList = null;
                if (parentTerm is IsInListTerm)
                {
                    newList = new IsInListTerm() { Value1 = newTerm, Value2 = null };
                }
                else
                {
                    newList = new NotIsInListTerm() { Value1 = newTerm, Value2 = null };
                }
                var old = parentTerm;
                var parent = term.GetParentTerm(term, old);
                AssignNewTerm(parent, old, newList);

            }

            NotifyPropertyChanged("Term");
        }

        private void CheckConstantTerm(BinaryTerm binaryTerm, bool newIsFirst)
        {
            if (newIsFirst)
            {
                if (binaryTerm.Value2 is ConstantTerm && binaryTerm.Value1.ResultType != binaryTerm.Value2.ResultType && binaryTerm.Value1.ResultType != null)
                {
                    binaryTerm.Value2 = UpdateConstantTerm((binaryTerm.Value2 as ConstantTerm).Value, binaryTerm.Value1.ResultType);
                }
            }
            else
            {
                if (binaryTerm.Value1 is ConstantTerm && binaryTerm.Value1.ResultType != binaryTerm.Value2.ResultType && binaryTerm.Value2.ResultType != null)
                {
                    binaryTerm.Value1 = UpdateConstantTerm((binaryTerm.Value1 as ConstantTerm).Value, binaryTerm.Value2.ResultType);
                }
            }
        }

        private Term UpdateConstantTerm(object originalValue, Type targetType)
        {
            if (MTFClientServerCommon.Helpers.TryParseValueHelper.TryParse(originalValue, targetType))
            {
                return new ConstantTerm(targetType) { Value = originalValue };
            }
            return CreateConstantTerm(targetType);
        }

        private void ValidateNewTermAsActivityResultTerm(ActivityResultTerm activityResultTerm)
        {
            if (EditorMode == EditorModes.CheckOutputValue)
            {
                var parent = UIHelper.FindParent<MTFEditor>(this);
                if (parent != null)
                {
                    var activity = parent.DataContext as MTFSequenceActivity;
                    if (activity != null)
                    {
                        activityResultTerm.Value = activity;
                    }
                    else if (parent.DataContext is MTFTermDesigner && ((MTFTermDesigner)parent.DataContext).SelectedActivity is MTFSequenceActivity)
                    {
                        activityResultTerm.Value = ((MTFTermDesigner)parent.DataContext).SelectedActivity as MTFSequenceActivity;
                    }
                    else
                    {
                        var termDesigner = UIHelper.FindParent<MTFTermDesigner>(parent);
                        if (termDesigner != null)
                        {
                            activityResultTerm.Value = termDesigner.SelectedActivity as MTFSequenceActivity;
                        }
                    }
                }
            }

        }

        private void ValidateNewTermAsListOperationTerm(ListOperationTerm listOperationTerm)
        {
            if (EditorMode == EditorModes.CheckOutputValue)
            {
                var parent = UIHelper.FindParent<MTFEditor>(this);
                if (parent != null)
                {
                    listOperationTerm.Value.Parameters = new ObservableCollection<Term>();
                    if (parent.DataContext is MTFSequenceActivity)
                    {
                        listOperationTerm.Value.Parameters.Add(new ActivityResultTerm()
                        {
                            Value = (MTFSequenceActivity)parent.DataContext
                        });
                    }
                    else if (parent.DataContext is MTFTermDesigner && ((MTFTermDesigner)parent.DataContext).SelectedActivity is MTFSequenceActivity)
                    {
                        listOperationTerm.Value.Parameters.Add(new ActivityResultTerm()
                        {
                            Value = ((MTFTermDesigner)parent.DataContext).SelectedActivity as MTFSequenceActivity
                        });
                    }
                    else
                    {
                        var termDesigner = UIHelper.FindParent<MTFTermDesigner>(parent);
                        if (termDesigner != null)
                            listOperationTerm.Value.Parameters.Add(new ActivityResultTerm()
                            {
                                Value = termDesigner.SelectedActivity as MTFSequenceActivity
                            });
                    }
                }
            }
        }

        private ConstantTerm CreateConstantTerm(Term parentTerm, Term oldTerm)
        {
            if (Value == oldTerm)
            {
                parentTerm.TargetType = ResultTypeName;
            }
            if (parentTerm is BinaryTerm)
            {
                BinaryTerm binaryTerm = parentTerm as BinaryTerm;
                if (binaryTerm.Value1 == oldTerm)
                {
                    return CreateConstantTerm(binaryTerm.Value2.ResultType, oldTerm);
                }
                else
                {
                    return CreateConstantTerm(binaryTerm.Value1.ResultType, oldTerm);
                }
            }
            if (parentTerm is UnaryNumberTerm)
            {
                return CreateConstantTerm(typeof(double));
            }

            return CreateConstantTerm(parentTerm.TargetType);
        }

        private ConstantTerm CreateConstantTerm(Type type, Term oldTerm)
        {
            if (type == null)
            {
                if (string.IsNullOrEmpty(oldTerm.TargetType))
                {
                    string t = GetTargetTypeFromGroup(oldTerm.TargetGroup);
                    if (string.IsNullOrEmpty(t))
                    {
                        t = typeof(string).FullName;
                    }
                    return CreateConstantTerm(Type.GetType(t));
                }
                else
                {
                    return CreateConstantTerm(Type.GetType(oldTerm.TargetType));
                }
            }
            return CreateConstantTerm(type);
        }

        private ConstantTerm CreateConstantTerm(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                typeName = "System.String";
            }
            var type = Type.GetType(typeName);
            return CreateConstantTerm(type);
        }

        private ConstantTerm CreateConstantTerm(Type type)
        {
            if (type == null)
            {
                type = Type.GetType(ResultTypeName);
            }
            else if (type == typeof(MTFClientServerCommon.MTFValidationTable.MTFValidationTable) || type.FullName.StartsWith("System.Nullable`1[["))
            {
                type = typeof(string);
            }
            if (type == typeof(string) || type == null || type == typeof(void) || type.IsGenericType || type.IsArray)
            {
                return new ConstantTerm(type) { Value = string.Empty };
            }
            return new ConstantTerm(type) { Value = Activator.CreateInstance(type), TargetType = type.FullName };
        }

        private void ValidateNewTermAsIsInList(ListTerm newTerm, Term oldTerm)
        {
            if (oldTerm is BinaryTerm)
            {
                newTerm.Value1 = (oldTerm as BinaryTerm).Value1;
            }
            else if (oldTerm is UnaryTerm)
            {
                newTerm.Value1 = (oldTerm as UnaryTerm).Value;
            }
            else if (oldTerm is ListTerm)
            {
                newTerm.Value1 = (oldTerm as ListTerm).Value1;
            }
            else if (EditorMode == EditorModes.CheckOutputValue)
            {
                var parent = UIHelper.FindParent<MTFEditor>(this);
                if (parent != null && parent.DataContext is MTFSequenceActivity)
                {
                    newTerm.Value1 = new ActivityResultTerm() { Value = parent.DataContext as MTFSequenceActivity };
                }
                else if (parent.DataContext is MTFTermDesigner && (parent.DataContext as MTFTermDesigner).SelectedActivity is MTFSequenceActivity)
                {
                    newTerm.Value1 = new ActivityResultTerm() { Value = (parent.DataContext as MTFTermDesigner).SelectedActivity as MTFSequenceActivity };
                }
            }
            else
            {
                newTerm.Value1 = CreateDefaultTerm(newTerm);
            }
        }

        private void ValidateNewTermAsUnary(UnaryTerm newTerm, Term oldTerm)
        {
            if (oldTerm is BinaryTerm)
            {
                newTerm.Value = (oldTerm as BinaryTerm).Value1;
            }
            else if (oldTerm is UnaryTerm)
            {
                newTerm.Value = (oldTerm as UnaryTerm).Value;
            }
            else if (oldTerm is ListTerm)
            {
                newTerm.Value = (oldTerm as ListTerm).Value1;
            }
            else
            {
                newTerm.Value = CreateDefaultTerm(newTerm);
            }
        }

        private void ValidateNewTermAsBinary(BinaryTerm newTerm, Term oldTerm)
        {
            if (oldTerm is BinaryTerm)
            {
                newTerm.Value1 = (oldTerm as BinaryTerm).Value1;
                newTerm.Value2 = (oldTerm as BinaryTerm).Value2;
            }
            else if (oldTerm is UnaryTerm)
            {
                newTerm.Value1 = (oldTerm as UnaryTerm).Value;
                newTerm.Value2 = CreateDefaultTerm(newTerm);
            }
            else if (oldTerm is ListTerm)
            {
                newTerm.Value1 = (oldTerm as ListTerm).Value1;
                newTerm.Value2 = CreateDefaultTerm(newTerm);
            }
            else
            {
                if (EditorMode == EditorModes.CheckOutputValue)
                {
                    var parent = UIHelper.FindParent<MTFEditor>(this);
                    if (parent != null && (parent.DataContext is MTFSequenceActivity))
                    {
                        newTerm.Value1 = new ActivityResultTerm() { Value = parent.DataContext as MTFSequenceActivity };
                        newTerm.Value2 = CreateConstantTerm(newTerm.Value1.ResultType);
                    }
                    else if (parent.DataContext is MTFTermDesigner && (parent.DataContext as MTFTermDesigner).SelectedActivity is MTFSequenceActivity)
                    {
                        newTerm.Value1 = new ActivityResultTerm() { Value = (parent.DataContext as MTFTermDesigner).SelectedActivity as MTFSequenceActivity };
                        newTerm.Value2 = CreateConstantTerm(newTerm.Value1.ResultType);
                    }
                    else
                    {
                        var termDesigner = UIHelper.FindParent<MTFTermDesigner>(parent);
                        if (termDesigner != null)
                        {
                            newTerm.Value1 = new ActivityResultTerm() { Value = termDesigner.SelectedActivity as MTFSequenceActivity };
                            newTerm.Value2 = CreateConstantTerm(newTerm.Value1.ResultType);
                        }
                    }
                }
                if (newTerm.Value1 == null)
                {
                    newTerm.Value1 = CreateDefaultTerm(newTerm);
                    newTerm.Value2 = CreateDefaultTerm(newTerm);
                }
            }
        }

        private Term CreateDefaultTerm(Term newTerm)
        {
            var targetType = newTerm.TargetType == "System.Boolean" ? GetTargetTypeFromGroup(newTerm.ChildrenTermGroup) : newTerm.TargetType;
            return new EmptyTerm() { TargetGroup = newTerm.ChildrenTermGroup, TargetType = targetType };
        }

        private string GetTargetTypeFromGroup(TermGroups termGroups)
        {
            if ((termGroups & TermGroups.StringTerm) == TermGroups.StringTerm)
            {
                return "System.String";
            }
            if ((termGroups & TermGroups.NumberTerm) == TermGroups.NumberTerm)
            {
                return "System.Double";
            }
            if ((termGroups & TermGroups.LogicalTerm) == TermGroups.LogicalTerm)
            {
                return "System.Boolean";
            }

            return null;
        }

        private void target_MouseMove(object sender, MouseEventArgs e)
        {
            if (startPointForTarget == null || !Setting.AllowDragDrop)
            {
                return;
            }
            Point mousePos = e.GetPosition(null);
            Vector diff = (Point)startPointForTarget - mousePos;
            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                var dragElement = sender as FrameworkElement;
                var oldTerm = dragElement.Tag;
                DataObject dragData = null;
                if (oldTerm is ActivityResultTerm)
                {
                    dragData = new DataObject(DragAndDropTypes.SetActivityResult, new Object());
                }
                else if (oldTerm is VariableTerm)
                {
                    dragData = new DataObject("SetVariable", new Object());
                }
                // Initialize the drag & drop operation

                if (dragData != null)
                {
                    var result = DragDrop.DoDragDrop(dragElement, dragData, DragDropEffects.All);
                    if (result != DragDropEffects.None)
                    {
                        if (oldTerm is ActivityResultTerm)
                        {
                            var activity = dragData.GetData(DragAndDropTypes.SetActivityResult) as MTFClientServerCommon.MTFSequenceActivity;
                            if (activity != null)
                            {
                                (oldTerm as ActivityResultTerm).Value = activity;
                                var parentTerm = term.GetParentTerm(term, oldTerm as Term);
                                if (parentTerm is BinaryTerm)
                                {
                                    ValidateBinaryTerm(parentTerm as BinaryTerm);
                                }
                                else if (parentTerm is ListTerm)
                                {
                                    (parentTerm as ListTerm).ValidateList();
                                }
                            }
                            else
                            {
                                (oldTerm as ActivityResultTerm).Value = null;
                            }

                        }
                        else if (oldTerm is VariableTerm)
                        {
                            var variable = dragData.GetData(DragAndDropTypes.SetVariable) as MTFVariable;
                            if (variable != null)
                            {
                                (oldTerm as VariableTerm).MTFVariable = variable;
                            }
                        }
                    }
                }
            }
        }

        private void target_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var data = (sender as FrameworkElement).Tag;
                if (data is VariableTerm)
                {
                    touch.SetItem(DragAndDropTypes.SetVariable, data, sender);
                }
                else if (data is ActivityResultTerm)
                {
                    touch.SetItem(DragAndDropTypes.SetActivityResult, data, sender);
                }

            }
            else
            {
                startPointForTarget = e.GetPosition(null);
            }
        }

        private void target_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            startPointForTarget = null;
        }




        private void EmptyTerm_Drop(object sender, DragEventArgs e)
        {
            ReplaceEmptyTerm(sender, e.Data);
        }

        private Term ValidateTerm(Term termToValidate, Term oldTerm, Term parentTerm)
        {
            Term newTerm = termToValidate;
            if (oldTerm is EmptyTerm)
            {
                newTerm.TargetType = oldTerm.TargetType;
            }
            newTerm.TargetGroup = oldTerm.TargetGroup;

            if (newTerm is BinaryTerm)
            {
                ValidateNewTermAsBinary(newTerm as BinaryTerm, oldTerm);
            }
            else if (newTerm is UnaryTerm)
            {
                ValidateNewTermAsUnary(newTerm as UnaryTerm, oldTerm);
            }
            else if (newTerm is ConstantTerm)
            {
                newTerm = CreateConstantTerm(parentTerm, oldTerm);
            }
            else if (newTerm is ListTerm)
            {
                ValidateNewTermAsIsInList(newTerm as ListTerm, oldTerm);
            }
            else if (newTerm is ActivityResultTerm)
            {
                ValidateNewTermAsActivityResultTerm(newTerm as ActivityResultTerm);
            }
            else if (newTerm is ListOperationTerm)
            {
                ValidateNewTermAsListOperationTerm(newTerm as ListOperationTerm);
            }
            return newTerm;
        }

        private void Term_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var data = (sender as FrameworkElement).Tag;
            if (data != null)
            {
                dragData = new DataObject("Term", data);
                if (e.ClickCount == 2)
                {
                    touch.SetItem(dragData, sender);
                }
                else
                {
                    if (touch.DataObject != null)
                    {
                        ReplaceTerm(sender, touch.DataObject);
                    }
                    else
                    {
                        termStartPoint = e.GetPosition(null);
                    }
                }
            }
            else
            {
                dragData = null;
            }
        }


        private void Term_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            termStartPoint = null;
        }

        private void Term_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (termStartPoint == null || !Setting.AllowDragDrop)
            {
                return;
            }
            Point mousePos = e.GetPosition(null);
            Vector diff = (Point)termStartPoint - mousePos;
            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                if (dragData != null)
                {
                    DragDrop.DoDragDrop(sender as FrameworkElement, dragData, DragDropEffects.All);
                    dragData = null;
                }
            }
        }

        private void ParentEmptyTerm_Drop(object sender, DragEventArgs e)
        {
            ReplaceParentEmptyTerm(sender, e.Data);
        }

        private void EmptyTerm_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("NewTerm") && !e.Data.GetDataPresent("Term"))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        private void EmptyTerm_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ReplaceEmptyTerm(sender, touch.DataObject);
        }

        private void ReplaceEmptyTerm(object sender, IDataObject data)
        {
            if (sender is FrameworkElement fe)
            {
                Term targetTerm = fe.Tag as Term;
                if (data != null && data.GetDataPresent("NewTerm"))
                {
                    Term t = (data.GetData("NewTerm") as Term).Clone() as Term;
                    if (t is ValidationTableTerm && UIHelper.HasParentOfType<MTFValidationTableEditor>(sender as FrameworkElement))
                    {
                        return;
                    }
                    Term parentTerm = term.GetParentTerm(term, targetTerm);
                    if (parentTerm.Parent is MTFClientServerCommon.MTFListOperation)
                    {
                        this.ResultTypeName = typeof(int).ToString();
                    }
                    else if (parentTerm.Parent is ExtendedRow row)
                    {
                        this.ResultTypeName = row.GetExpectedTypeName();
                    }
                    else if (parentTerm == targetTerm && parentTerm is EmptyTerm && t is ConstantTerm && data.GetDataPresent("TargetType"))
                    {
                        this.ResultTypeName = data.GetData("TargetType") as string;
                    }

                    Term newTerm = ValidateTerm(t, targetTerm, parentTerm);
                    AssignNewTerm(parentTerm, targetTerm, newTerm);
                }
                else if (data != null && data.GetDataPresent("Term"))
                {
                    if (!data.GetDataPresent("PasteCopy") || (bool)data.GetData("PasteCopy") == false)
                    {
                        var sourceTerm = data.GetData("Term") as Term;
                        if (!sourceTerm.Contains(targetTerm))
                        {
                            var copy = Keyboard.Modifiers == ModifierKeys.Control;
                            if (term is EmptyTerm emptyTerm && emptyTerm.GetParent<StringFormatTerm>() != null)
                            {
                                var sft = emptyTerm.GetParent<StringFormatTerm>();
                                if (!copy)
                                {
                                    var parent = sourceTerm.GetParent<Term>();
                                    parent?.ChangeChildrenTerm(new EmptyTerm(), sourceTerm);

                                }
                                sft.ChangeTerm(sourceTerm, targetTerm, copy);
                            }
                            else
                            {
                                if (copy)
                                {
                                    var cloneTerm = (Term)sourceTerm.Clone();
                                    cloneTerm.ReplaceIdentityObjectsNoCache(sourceTerm);
                                    ChangeTermForOtherTerm(cloneTerm, targetTerm, false);
                                }
                                else
                                {
                                    ChangeTermForOtherTerm(new EmptyTerm(), sourceTerm, false);
                                    ChangeTermForOtherTerm(sourceTerm, targetTerm, false);
                                }
                            }
                        }
                    }
                    touch.Clear();
                }
            }
        }

        private void ReplaceParentEmptyTerm(object sender, IDataObject data)
        {
            if (data != null && data.GetDataPresent("NewTerm"))
            {
                var newTerm = (data.GetData("NewTerm") as Term).Clone() as Term;
                if (newTerm is ValidationTableTerm && UIHelper.HasParentOfType<MTFValidationTableEditor>(sender as FrameworkElement))
                {
                    return;
                }
                switch (newTerm)
                {
                    case BinaryTerm binaryTerm:
                        binaryTerm.Value1 = term;
                        binaryTerm.Value2 = new EmptyTerm();
                        term = binaryTerm;
                        NotifyPropertyChanged("Term");
                        break;
                    case UnaryTerm unaryTerm:
                        unaryTerm.Value = term;
                        term = unaryTerm;
                        NotifyPropertyChanged("Term");
                        break;
                    case ListTerm listTerm:
                        listTerm.Value1 = term;
                        term = listTerm;
                        NotifyPropertyChanged("Term");
                        break;
                }
            }
        }

        private void ParentEmptyTerm_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ReplaceParentEmptyTerm(sender, touch.DataObject);
        }

        private void Term_Drop(object sender, DragEventArgs e)
        {
            ReplaceTerm(sender, e.Data);
        }

        private void ReplaceTerm(object sender, IDataObject data)
        {
            FrameworkElement b = sender as FrameworkElement;
            Term oldTerm = b.Tag as Term;
            if (data.GetDataPresent("NewTerm"))
            {
                var t = data.GetData("NewTerm") as Term;
                if (t is ValidationTableTerm && UIHelper.HasParentOfType<MTFValidationTableEditor>(sender as FrameworkElement))
                {
                    return;
                }

                Term parentTerm = term.GetParentTerm(term, oldTerm);
                if (parentTerm.Parent is MTFClientServerCommon.MTFListOperation)
                {
                    this.ResultTypeName = typeof(Int32).ToString();
                }

                else if (oldTerm == term && t is ConstantTerm && data.GetDataPresent("TargetType"))
                {
                    var type = Type.GetType(data.GetData("TargetType") as string);
                    if (type != null && (MTFCommon.Helpers.TypeHelper.IsNumericType(type) || type == typeof(bool)))
                    {
                        this.ResultTypeName = type.FullName;
                        (t as ConstantTerm).Value = Activator.CreateInstance(type);
                    }
                }
                //var parentTerm = term.GetParentTerm(term, oldTerm);
                var newTerm = ValidateTerm(t, oldTerm, parentTerm);
                //AssignNewTerm(parentTerm, oldTerm, newTerm);
                ChangeTermForOtherTerm(newTerm, oldTerm, true);
            }
            else if (data.GetDataPresent("Term"))
            {
                var newTerm = data.GetData("Term") as Term;
                if (!newTerm.Contains(oldTerm))
                {
                    ChangeTermForOtherTerm(newTerm, oldTerm, false);
                }
            }
        }

        private void ChangeTermForOtherTerm(Term newTerm, Term targetTerm, bool isNewTerm)
        {
            Term targetParent = term.GetParentTerm(term, targetTerm);
            Term sourceParent = term.GetParentTerm(term, newTerm);
            if (sourceParent != null)
            {
                sourceParent.ChangeChildrenTerm(new EmptyTerm(), newTerm);
            }
            if (targetParent == null)
            {
                var sft = targetTerm.GetParent<StringFormatTerm>();
                if (sft != null)
                {
                    sft.ChangeTerm(targetTerm, newTerm, false);
                    return;
                }
            }
            if (targetTerm == term)
            {
                if (isNewTerm)
                {
                    newTerm.AssignChildrenOf(term);
                }
                term = newTerm;
            }
            else
            {
                if (isNewTerm)
                {
                    newTerm.AssignChildrenOf(targetTerm);
                }
                targetParent.ChangeChildrenTerm(newTerm, targetTerm);
            }
            NotifyPropertyChanged("Term");
        }

        private void EmptyTerm_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var eventArg = new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, e.ChangedButton);
            eventArg.RoutedEvent = UIElement.MouseLeftButtonDownEvent;
            eventArg.Source = (sender as Border).Tag;
            UIElement parent = UIHelper.FindParent<MTFTermDesigner>(sender as UIElement);
            if (parent != null)
            {
                parent.RaiseEvent(eventArg);
            }
        }

        private void ComboBox_selectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Fix due to a binding error
            //When the sequence is changed, LastSelectedItem is set to Activity's detailTemplate contains some ComboBox,
            //ItemsSource of this Combobox is changed too and SelectedValue raises binding of null value to source property
            //therefore UpdateSourceTrigger of binding SelectedValue is set to value Explicit and binding to source value is raised here.
            var combo = sender as ComboBox;
            if (combo != null && combo.Items.Count > 0 && combo.SelectedValue != null)
            {
                BindingExpression be = combo.GetBindingExpression(ComboBox.SelectedValueProperty);
                if (be != null)
                {
                    be.UpdateSource();
                }
            }


            var currentTerm = combo.Tag as Term;
            if (currentTerm is ValidationTableTerm)
            {
                var addItem = e.AddedItems.Count > 0 ? e.AddedItems[0] as MTFValidationTable : null;
                var removeItem = e.RemovedItems.Count > 0 ? e.RemovedItems[0] as MTFValidationTable : null;
                (currentTerm as ValidationTableTerm).RegistrRowChangedEvent(removeItem, addItem);
            }
            var parentTerm = term.GetParentTerm(term, currentTerm);
            if (parentTerm is BinaryTerm)
            {
                ValidateBinaryTerm(parentTerm as BinaryTerm);
            }
            else if (parentTerm is ListTerm)
            {
                (parentTerm as ListTerm).ValidateList();
            }
        }



        private void ActivityResultTerm_PathChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is MTFActivityResultEditor editor)
            {
                if (editor.DataContext is Term actResTerm)
                {
                    var parentTerm = term.GetParentTerm(term, actResTerm);
                    switch (parentTerm)
                    {
                        case BinaryTerm binaryTerm:
                            ValidateBinaryTerm(binaryTerm);
                            break;
                        case ListTerm listTerm:
                            listTerm.ValidateList();
                            break;
                    }
                }
            }
        }

        private void ValidateBinaryTerm(BinaryTerm binaryTerm)
        {
            if (binaryTerm.Value2 is ConstantTerm value2)
            {
                binaryTerm.Value2 = GetConstantTermByActivityResult(value2, binaryTerm.Value1);
            }
            else if (binaryTerm.Value1 is ConstantTerm value1)
            {
                binaryTerm.Value1 = GetConstantTermByActivityResult(value1, binaryTerm.Value2);
            }
        }

        private Term GetConstantTermByActivityResult(ConstantTerm constantTerm, Term t)
        {
            if (t != null && !(t is EmptyTerm) && constantTerm.ResultType != t.ResultType)
            {
                return CreateConstantTerm(t.ResultType);
            }

            return constantTerm;
        }

        private void Term_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("NewTerm") && !e.Data.GetDataPresent("Term"))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        private void MTFExecuteActivityTermEditor_OnMethodResultTypeChanged(object sender, Type type)
        {
            if (sender is MTFExecuteActivityTermEditor editor)
            {
                if (editor.Term?.Parent is BinaryTerm binaryTerm)
                {
                    ValidateBinaryTerm(binaryTerm);
                }
            }
        }
    }
}
