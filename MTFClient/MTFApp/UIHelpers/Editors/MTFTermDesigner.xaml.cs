using MTFApp.UIHelpers.DragAndDrop;
using MTFClientServerCommon;
using MTFClientServerCommon.Mathematics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MTFApp.UIHelpers.Editors
{
    /// <summary>
    /// Interaction logic for MTFTermDesigner.xaml
    /// </summary>
    public partial class MTFTermDesigner : MTFEditorBase
    {
        private Point? startPoint = null;
        //private bool updateValue = false;
        private Term selectedTerm = null;
        private TouchHelper touch = TouchHelper.Instance;
        private Brush blackBrush;
        private Brush redBrush;
        private bool allowPaste = false;
        private bool allowSaveAfterClose = true;

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.C)
                {
                    CopyTerm();
                    e.Handled = true;
                }
                if (e.Key == Key.V)
                {
                    PasteTerm();
                    e.Handled = true;
                }
            }
        }

        private void PasteTerm()
        {
            if (MTFClipboard.ContainsData<Term>())
            {
                var data = MTFClipboard.GetData<Term>();
                var cloneData = data.Clone() as Term;
                if (cloneData != null)
                {
                    cloneData.ReplaceIdentityObjectsNoCache(data);
                    var tableTerm = term as ValidationTableTerm;
                    if (tableTerm != null && tableTerm.Rows != null)
                    {
                        var row = tableTerm.Rows.FirstOrDefault(x => x.IsSelected);
                        if (row != null && row.ActualValue is EmptyTerm)
                        {
                            row.ActualValue = cloneData;
                        }
                    }
                    else
                    {
                        if (term is EmptyTerm)
                        {
                            term = cloneData;
                            NotifyPropertyChanged("Term");
                        }
                    }
                }

            }
        }

        private void CopyTerm()
        {
            Term workingTerm = null;
            var tableTerm = term as ValidationTableTerm;
            if (tableTerm != null && tableTerm.Rows != null)
            {
                var row = tableTerm.Rows.FirstOrDefault(x => x.IsSelected);
                if (row != null)
                {
                    workingTerm = row.ActualValue;
                }
            }
            if (workingTerm == null)
            {
                workingTerm = term;
            }

            if (workingTerm != null)
            {
                MTFClipboard.SetData(workingTerm);
            }
        }



        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property.Name == "ParentOfTerm")
            {
                if (e.NewValue == null && e.OldValue!=null)
                {
                    SaveAfterClose(e.OldValue);
                }
            }
            else if (e.Property == IsOpenProperty)
            {
                KeyboardHandler.Instance.SetControl(this);
                if ((bool)e.NewValue)
                {
                    allowSaveAfterClose = true;
                }
                else
                {
                    KeyboardHandler.Instance.RemoveControl();
                    if (allowSaveAfterClose)
                    {
                        CloseDesigner_Click(null, null);
                    }
                }
            }
            base.OnPropertyChanged(e);
        }

        public MTFTermDesigner()
        {
            InitializeComponent();
            TermDesignerRoot.DataContext = this;
            blackBrush = (Brush)App.Current.FindResource("ALBlackBrush");
            redBrush = (Brush)App.Current.FindResource("BaseRedBrush");
            //this.PropertyChanged += MTFTermDesigner_PropertyChanged;
            //Binding b = new Binding();
            //b.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(SequenceEditor.SequenceEditorControl), 1);
            //b.Path = new PropertyPath("DataContext.LastSelectedItem");
            //BindingOperations.SetBinding(this, SelectedActivityProperty, b);
        }

        //void MTFTermDesigner_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    if (Value != Term && updateValue)
        //    {
        //        //Value = Term;
        //    }
        //    //NotifyPropertyChanged("Value");
        //}

        protected override void OnPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is Term)
            {
                term = e.NewValue as Term;
                //term.PropertyChanged += term_PropertyChanged;
            }
            NotifyPropertyChanged("Term");
        }


        //private void term_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        private void MTFEditor_ValueChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (touch != null)
            {
                touch.Clear();
            }
            //if (e.PropertyName.Contains("MTFMethodName"))
            //{
            //    term.PropertyChanged -= term_PropertyChanged;
            //    term = new EmptyTerm();
            //}
            //else if (term != null && !e.PropertyName.Contains("Term"))
            //{
            //    if (sender is BinaryTerm)
            //    {
            //        ValidateTermsInBinaryTerm(sender as BinaryTerm);
            //    }
            //    else if (sender is IsInListTerm)
            //    {

            //        ValidateTermsInIsInListTerm(sender as IsInListTerm);
            //    }
            //}
            //NotifyPropertyChanged("Term");
        }


        #region SelectedActivity dependendy property
        public object SelectedActivity
        {
            get { return (object)GetValue(SelectedActivityProperty); }
            set { SetValue(SelectedActivityProperty, value); }
        }

        public static readonly DependencyProperty SelectedActivityProperty =
            DependencyProperty.Register("SelectedActivity", typeof(object), typeof(MTFTermDesigner),
            new FrameworkPropertyMetadata() { BindsTwoWayByDefault = false, PropertyChangedCallback = SelectedActivityChanged });

        private static void SelectedActivityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //throw new NotImplementedException();
        }
        #endregion

        #region TargetType dependency property
        public string TargetType
        {
            get { return (string)GetValue(TargetTypeProperty); }
            set { SetValue(TargetTypeProperty, value); }
        }

        public static readonly DependencyProperty TargetTypeProperty =
            DependencyProperty.Register("TargetType", typeof(string), typeof(MTFTermDesigner),
            new FrameworkPropertyMetadata() { BindsTwoWayByDefault = false });
        #endregion


        #region ParentOfTerm dependency property
        public object ParentOfTerm
        {
            get { return (object)GetValue(ParentOfTermProperty); }
            set { SetValue(ParentOfTermProperty, value); }
        }

        public static readonly DependencyProperty ParentOfTermProperty =
            DependencyProperty.Register("ParentOfTerm", typeof(object), typeof(MTFTermDesigner),
            new FrameworkPropertyMetadata() { BindsTwoWayByDefault = false });
        #endregion


        #region TermPropertyName dependency property
        public string TermPropertyName
        {
            get { return (string)GetValue(TermPropertyNameProperty); }
            set { SetValue(TermPropertyNameProperty, value); }
        }

        public static readonly DependencyProperty TermPropertyNameProperty =
            DependencyProperty.Register("TermPropertyName", typeof(string), typeof(MTFTermDesigner),
            new FrameworkPropertyMetadata() { BindsTwoWayByDefault = false });
        #endregion

        #region Visibility dependency property
        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register("IsOpen", typeof(bool), typeof(MTFTermDesigner),
            new FrameworkPropertyMetadata() { BindsTwoWayByDefault = true, PropertyChangedCallback = IsOpenChanged });

        private static void IsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //throw new NotImplementedException();
        }
        #endregion

        private Term term;

        public Term Term
        {
            get { return term; }
            set
            {
                term = value;
                NotifyPropertyChanged();
            }
        }

        private double scale = 1;

        public double Scale
        {
            get { return scale; }
            set
            {
                scale = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand ZoomInCommand
        {
            get
            {
                return new Command(() => Scale = UIHelpers.UIHelper.GetNewScale(1, Scale));
            }
        }

        public ICommand ZoomOutCommand
        {
            get
            {
                return new Command(() => Scale = UIHelpers.UIHelper.GetNewScale(-1, Scale));
            }
        }

        public ICommand ActualSizeCommand
        {
            get
            {
                return new Command(() => Scale = 1);
            }
        }

        public ICommand FitSizeCommand
        {
            get
            {
                return new Command(() => Scale = Math.Min(zoomPaletteFixed.ViewportHeight / zoomPaletteResizable.ActualHeight,
                    zoomPaletteFixed.ViewportWidth / zoomPaletteResizable.ActualWidth));
            }
        }

        public ICommand CopyToClipboardCommand
        {
            get
            {
                return new Command(() =>
                {
                    if (touch.DataObject != null && touch.DataObject.GetDataPresent("Term"))
                    {
                        MTFClipboard.SetData(touch.DataObject.GetData("Term"));
                    }
                    else if (!(term is EmptyTerm))
                    {
                        MTFClipboard.SetData(term);
                    }
                });
            }
        }

        public ICommand PasteFromClipboardCommand
        {
            get
            {
                return new Command(() =>
                {
                    if (MTFClipboard.ContainsData<Term>())
                    {
                        var tmp = MTFClipboard.GetData();
                        if (tmp is Term && term is EmptyTerm)
                        {
                            term = (tmp as Term).Clone() as Term;
                            term.ReplaceIdentityObjectsNoCache(tmp as MTFDataTransferObject);
                            NotifyPropertyChanged("Term");
                        }
                        else
                        {
                            AllowPaste = true;
                        }
                    }
                });
            }
        }


        private void ToolBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!ReferenceEquals(null, UIHelper.FindParent<System.Windows.Controls.Primitives.ScrollBar>((DependencyObject)e.OriginalSource)))
            {
                startPoint = null;
                return;
            }
            startPoint = e.GetPosition(null);
            selectedTerm = UIHelpers.UIHelper.GetObjectDataFromPoint(sender as ListBox, e.GetPosition(sender as ListBox)) as Term;
        }

        private void ToolBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (startPoint == null || !Setting.AllowDragDrop)
            {
                return;
            }
            Point mousePos = e.GetPosition(null);
            Vector diff = (Point)startPoint - mousePos;
            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                if (selectedTerm != null)
                {
                    var dragData = new DataObject("NewTerm", TermFactory.CreateTerm(selectedTerm.GetType()));
                    if (this.TargetType != null)
                    {
                        dragData.SetData("TargetType", this.TargetType);
                    }
                    DragDrop.DoDragDrop(sender as ListBox, dragData, DragDropEffects.All);
                }
            }
        }

        private void ResetTerm_Click(object sender, RoutedEventArgs e)
        {
            Term = new EmptyTerm();
            NotifyPropertyChanged("Term");
        }

        private void ToolBox_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            //e.UseDefaultCursors = false;
            //Mouse.SetCursor(Cursors.Hand);
        }

        private void Trash_MouseLeave(object sender, MouseEventArgs e)
        {
            UserControl userControl = sender as UserControl;
            if (touch.DataObject != null && touch.DataObject.GetDataPresent("Term"))
            {
                userControl.Foreground = blackBrush;
            }
            e.Handled = true;
        }

        private void Trash_MouseEnter(object sender, MouseEventArgs e)
        {
            UserControl userControl = sender as UserControl;
            if (touch.DataObject != null && touch.DataObject.GetDataPresent("Term"))
            {
                userControl.Foreground = redBrush;
            }
            e.Handled = true;
        }

        private void Trash_DragOver(object sender, DragEventArgs e)
        {
            UserControl userControl = sender as UserControl;
            if (e.Data.GetDataPresent("Term"))
            {
                userControl.Foreground = redBrush;
            }
            else
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        private void Trash_DragLeave(object sender, DragEventArgs e)
        {
            UserControl userControl = sender as UserControl;
            userControl.Foreground = blackBrush;

        }

        private void Trash_Drop(object sender, DragEventArgs e)
        {
            UserControl userControl = sender as UserControl;
            userControl.Foreground = blackBrush;
            RemoveTerm(e.Data);
        }

        private void Trash_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            RemoveTerm(touch.DataObject);
            UserControl userControl = sender as UserControl;
            userControl.Foreground = blackBrush;
            touch.Clear();
        }

        private void RemoveTerm(IDataObject data)
        {
            if (data != null && data.GetDataPresent("Term"))
            {
                var termToRemove = data.GetData("Term") as Term;
                RemoveTerm(termToRemove);
            }
        }

        private void RemoveTerm(Term termToRemove)
        {
            var parentTerm = term.GetParentTerm(term, termToRemove);
            if (parentTerm == termToRemove)
            {
                term = new EmptyTerm();
            }
            else if (parentTerm is BinaryTerm)
            {
                var tmp = parentTerm as BinaryTerm;
                if (tmp.Value1 == termToRemove)
                {
                    tmp.Value1 = new EmptyTerm();
                }
                else if (tmp.Value2 == termToRemove)
                {
                    tmp.Value2 = new EmptyTerm();
                }
            }
            else if (parentTerm is UnaryTerm)
            {
                (parentTerm as UnaryTerm).Value = new EmptyTerm();
            }
            else if (parentTerm is ListTerm)
            {
                (parentTerm as ListTerm).Value1 = new EmptyTerm();
            }
            else if (parentTerm is ValidationTableTerm)
            {
                (parentTerm as ValidationTableTerm).RemoveTerm(termToRemove);
            }
            else if (parentTerm is StringFormatTerm)
            {
                (parentTerm as StringFormatTerm).RemoveTerm(termToRemove);
            }
            else if (parentTerm is ListOperationTerm)
            {
                (parentTerm as ListOperationTerm).RemoveTerm(termToRemove);
            }
            NotifyPropertyChanged("Term");
        }

        private void target_Drop(object sender, DragEventArgs e)
        {
            var data = (sender as FrameworkElement).Tag;
            if (data is MTFSequenceActivity)
            {
                if (e.Data.GetDataPresent(DragAndDropTypes.SetActivityResult))
                {
                    if ((data != SelectedActivity || EditorMode == EditorModes.CheckOutputValue) && !(data is MTFSubSequenceActivity))
                    {
                        e.Data.SetData(DragAndDropTypes.SetActivityResult, data);
                    }
                    e.Handled = true;
                }
                else if (e.Data.GetDataPresent(DragAndDropTypes.GetActivityResult))
                {
                    if ((data != SelectedActivity || EditorMode == EditorModes.CheckOutputValue) && !(data is MTFSubSequenceActivity))
                    {
                        e.Data.SetData(DragAndDropTypes.GetActivityResult, data);
                    }
                    e.Handled = true;
                }
            }
            if (data is MTFVariable)
            {
                if (e.Data.GetDataPresent(DragAndDropTypes.SetVariable))
                {
                    e.Data.SetData(DragAndDropTypes.SetVariable, data);
                    e.Handled = true;
                }
            }

        }


        private void Sequence_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var activity = (sender as FrameworkElement).Tag;
            var data = touch.DataObject;
            if (data != null)
            {
                if (data.GetDataPresent(DragAndDropTypes.SetActivityResult))
                {
                    var term = data.GetData(DragAndDropTypes.SetActivityResult) as ActivityResultTerm;
                    if (term != null)
                    {
                        term.Value = activity as MTFSequenceActivity;
                        touch.Clear();
                    }
                }
                else if (data.GetDataPresent(DragAndDropTypes.SetVariable))
                {
                    var term = data.GetData(DragAndDropTypes.SetVariable) as VariableTerm;
                    if (term != null)
                    {
                        term.MTFVariable = activity as MTFVariable;
                        touch.Clear();
                    }
                }
            }
        }

        private void ListBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            var targetPoint = e.GetPosition(sender as ListBox);
            var data = UIHelper.GetObjectDataFromPoint(sender as ListBox, targetPoint);
        }

        private void SaveAfterClose(object parent)
        {
            allowSaveAfterClose = false;
            if (parent != null && !string.IsNullOrEmpty(TermPropertyName))
            {
                var prop = parent.GetType().GetProperty(TermPropertyName);
                if (prop != null)
                {
                    var value = prop.GetValue(parent);
                    if (value is IList)
                    {
                        var index = (value as IList).IndexOf(Value);
                        if (index != -1)
                        {
                            (value as IList)[index] = term;
                        }
                        //prop.SetValue(ParentOfTerm, value);
                    }
                    else
                    {
                        prop.SetValue(parent, term);
                    }
                }
                else
                {
                    if (parent is IEnumerable<ListItemWrapper>)
                    {
                        var item = (parent as IEnumerable<ListItemWrapper>).FirstOrDefault(x => x.Value == this.Value);
                        if (item != null)
                        {
                            item.Value = term;
                        }
                    }
                }
            }
            this.IsOpen = false;
            NotifyPropertyChanged("IsOpen");
        }

        private void CloseDesigner_Click(object sender, RoutedEventArgs e)
        {
            //updateValue = true;
            //NotifyPropertyChanged("Term");
            //updateValue = false;
            //this.Term
            SaveAfterClose(ParentOfTerm);
        }

        private void Control_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                this.Scale = UIHelper.GetNewScale(e.Delta, this.Scale);
            }
            else if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new System.Windows.Input.MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                UIElement parent = UIHelper.FindParent<UIElement>(sender as UIElement);
                if (parent != null)
                {
                    parent.RaiseEvent(eventArg);
                }
            }
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                Control_PreviewMouseWheel(this, e);
                e.Handled = true;
            }
        }

        private void ToolBox_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        public object TouchItem
        {
            get
            {
                return touch.DataObject;
            }
        }

        private void ToolBox_PreviewMouseLeftDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var newTerm = TermFactory.CreateTerm(selectedTerm.GetType());
                if (term is EmptyTerm)
                {
                    if (newTerm is BinaryTerm)
                    {
                        if (EditorMode == EditorModes.CheckOutputValue)
                        {
                            (newTerm as BinaryTerm).Value1 = new ActivityResultTerm() { Value = SelectedActivity as MTFSequenceActivity };
                        }
                        else
                        {
                            (newTerm as BinaryTerm).Value1 = new EmptyTerm();
                        }
                        (newTerm as BinaryTerm).Value2 = new EmptyTerm();
                    }
                    else if (newTerm is UnaryTerm)
                    {
                        (newTerm as UnaryTerm).Value = new EmptyTerm();
                    }
                    else if (newTerm is ListTerm)
                    {
                        (newTerm as ListTerm).Value1 = new EmptyTerm();
                    }
                    else if (newTerm is ConstantTerm)
                    {
                        (newTerm as ConstantTerm).TargetType = typeof(string).FullName;
                        (newTerm as ConstantTerm).Value = string.Empty;
                    }
                    else if (newTerm is ListOperationTerm)
                    {
                        if (EditorMode == EditorModes.CheckOutputValue)
                        {
                            (newTerm as ListOperationTerm).Value.Parameters = new ObservableCollection<Term>();
                            (newTerm as ListOperationTerm).Value.Parameters.Add(new ActivityResultTerm() { Value = SelectedActivity as MTFSequenceActivity });
                        }
                    }
                    Term = newTerm;
                }
                else
                {
                    touch.SetItem("NewTerm", newTerm, sender);
                    NotifyPropertyChanged("TouchItem");
                }
            }
        }



        public bool AllowPaste
        {
            get { return allowPaste; }
            set
            {
                allowPaste = value;
                if (touch.DataObject != null && touch.DataObject.GetDataPresent("Term"))
                {
                    if (value)
                    {
                        touch.DataObject.SetData("PasteCopy", true);
                    }
                    else
                    {
                        touch.DataObject.SetData("PasteCopy", false);
                    }

                }
                NotifyPropertyChanged();
            }
        }



        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (allowPaste)
            {
                var emptyTerm = e.OriginalSource as Term;
                if (emptyTerm is EmptyTerm)
                {
                    var parent = term.GetParentTerm(term, emptyTerm);
                    if (parent != null && MTFClipboard.ContainsData<Term>())
                    {
                        var tmp = MTFClipboard.GetData();
                        if (tmp is Term)
                        {
                            var t = (tmp as Term).Clone() as Term;
                            t.ReplaceIdentityObjectsNoCache(tmp as MTFDataTransferObject);
                            if (emptyTerm == term)
                            {
                                term = t;
                            }
                            else
                            {
                                parent.ChangeChildrenTerm(t, emptyTerm);
                            }
                            NotifyPropertyChanged("Term");
                        }

                    }
                }
                AllowPaste = false;
            }
            e.Handled = true;
        }

        //protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        //{
            //if (!(e.OriginalSource is Border) && !(e.OriginalSource is Image))
            //{
            //    AllowDrop = false;
            //    touch.Clear();
            //}
        //}

    }
}
