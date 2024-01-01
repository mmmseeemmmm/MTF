using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MTFApp.UIHelpers;

namespace MTFApp.SequenceExecution.SharedControls
{
    /// <summary>
    /// Interaction logic for VariablesWatch.xaml
    /// </summary>
    public partial class VariablesWatchControl : UserControl
    {
        private MTFEditor lastEditor;
        bool allowSave = true;

        public VariablesWatchControl()
        {
            InitializeComponent();
        }


        private void Eye_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var control = sender as UserControl;
            if (control != null)
            {
                var item = control.Tag as VariablesWatch;
                if (item != null)
                {
                    item.IsInWatch = !item.IsInWatch;
                }
            }
        }

        private void AllEye_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var dataContext = DataContext as SequenceExecutionPresenter;
            if (dataContext != null)
            {
                dataContext.SetAllInWatch();
            }
        }



        //private void ShowButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    var dataContext = DataContext as SequenceExecutionPresenter;
        //    if (dataContext != null)
        //    {
        //        dataContext.ShowVariablesCommand.Execute(null);
        //    }
        //    allowSave = false;
        //    if (lastEditor != null)
        //    {
        //        SetReadOnlyEditor(lastEditor);
        //        lastEditor = null;
        //    }
        //    allowSave = true;
        //}


        //private void Title_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    //ShowButton.RaiseEvent(e);
        //}

        private void EyeComlexType_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var control = sender as UserControl;
            if (control != null)
            {
                var item = control.Tag as VariablesWatch;
                if (item != null)
                {
                    item.IsExpanded = !item.IsExpanded;
                    var dataContext = DataContext as SequenceExecutionPresenter;
                    if (dataContext != null)
                    {
                        dataContext.UpdateTablesWatchPreviewCommand.Execute(item);
                    }
                }
            }
        }

        private void MTFEditor_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var editor = sender as MTFEditor;
            if (lastEditor != null && editor != lastEditor)
            {
                allowSave = false;
                SetReadOnlyEditor(lastEditor);
                lastEditor = null;
                allowSave = true;
            }
            SetWriteableEditor(editor);
            lastEditor = editor;
        }


        private void SetWriteableEditor(MTFEditor editor)
        {
            if (editor != null)
            {
                editor.ReadOnly = false;
            }
        }

        private void SetReadOnlyEditor(MTFEditor editor)
        {

            if (editor != null)
            {
                var dataContext = DataContext as SequenceExecutionPresenter;
                if (dataContext != null)
                {
                    var watch = editor.Tag as VariablesWatch;
                    if (watch != null)
                    {

                        watch.Value = editor.Value;
                        dataContext.UpdateVariableFromDebug(watch.Variable, editor.Value);
                    }
                    editor.ReadOnly = true;
                }
            }
        }


        private void MTFEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                SetReadOnlyEditor(sender as MTFEditor);
            }
            if (e.Key == Key.Enter)
            {
                var dataContext = DataContext as SequenceExecutionPresenter;
                if (dataContext == null)
                {
                    return;
                }
                var editor = sender as MTFEditor;
                if (editor == null)
                {
                    return;
                }
                var watch = editor.Tag as VariablesWatch;
                if (watch != null)
                {
                    allowSave = false;
                    watch.Value = editor.Value;
                    dataContext.UpdateVariableFromDebug(watch.Variable, editor.Value);
                    editor.ReadOnly = true;
                    lastEditor = null;
                    allowSave = true;
                }
            }
        }

        private void VariablesSetting_Click(object sender, RoutedEventArgs e)
        {
            if (lastEditor != null)
            {
                SetReadOnlyEditor(lastEditor);
                lastEditor = null;
            }
        }


        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property.Name == "IsKeyboardFocusWithin" && allowSave)
            {
                if (!(bool)e.NewValue)
                {
                    if (lastEditor != null)
                    {
                        SetReadOnlyEditor(lastEditor);
                        lastEditor = null;
                    }
                }
            }
            base.OnPropertyChanged(e);
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (lastEditor != null)
            {
                var textBox = UIHelper.FindParent<TextBox>(e.MouseDevice.DirectlyOver as DependencyObject);
                if (textBox == null)
                {
                    SetReadOnlyEditor(lastEditor);
                    lastEditor = null;
                }
            }
            base.OnPreviewMouseDown(e);
        }
    }
}