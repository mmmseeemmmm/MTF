using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using MTFClientServerCommon;

namespace MTFApp.UIHelpers.Editors
{
    /// <summary>
    /// Interaction logic for MTFCaseEditor.xaml
    /// </summary>
    public partial class MTFCaseEditor : MTFEditorBase
    {
        private bool editableName = true;
        public MTFCaseEditor()
        {
            InitializeComponent();
            Root.DataContext = this;
        }



        public string ElementTypeName
        {
            get { return (string)GetValue(ElementTypeNameProperty); }
            set { SetValue(ElementTypeNameProperty, value); }
        }

        public bool EditableName
        {
            get { return editableName; }
        }

        public static readonly DependencyProperty ElementTypeNameProperty =
            DependencyProperty.Register("ElementTypeName", typeof(string), typeof(MTFCaseEditor),
            new FrameworkPropertyMetadata { BindsTwoWayByDefault = false, PropertyChangedCallback = ElementTypeNameChanged });

        private static void ElementTypeNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = d as MTFCaseEditor;
            if (editor != null)
            {
                var variantType = typeof(List<SequenceVariantGroup>);
                var val = editor.Value as MTFCase;
                if (val != null)
                {
                    if (e.NewValue != null && val.Value == null)
                    {
                        var type = Type.GetType(e.NewValue.ToString());
                        if (type != null)
                        {
                            if (type == typeof(string))
                            {
                                val.Value = string.Empty;
                            }
                            else
                            {
                                try
                                {
                                    val.Value = Activator.CreateInstance(type);
                                }
                                catch (Exception)
                                {
                                    val.Value = null;
                                } 
                            }
                        }
                    }
                    editor.editableName = !Equals(e.NewValue, variantType.FullName) && !val.IsDefault;
                    editor.NotifyPropertyChanged("EditableName");
                }

            }

        }


        protected override void OnPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == ParentActivityProperty)
            {
                var subSequence = e.NewValue as MTFSubSequenceActivity;
                if (subSequence != null && subSequence.Term != null)
                {
                    var binding = new Binding
                                  {
                                      Source = ParentActivity,
                                      Mode = BindingMode.OneWay,
                                      UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                                      Path = new PropertyPath("Term.ResultType.FullName")
                                  };
                    BindingOperations.SetBinding(this, ElementTypeNameProperty, binding);
                }
            }
            base.OnPropertyChanged(source, e);
        }

        private void IsDefaultCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            var currentCase = this.Value as MTFCase;
            if (currentCase != null)
            {
                currentCase.Value = null;
                editableName = false;
                currentCase.Name = "default";
                NotifyPropertyChanged("EditableName");
                var subSequence = currentCase.Parent as MTFSubSequenceActivity;
                if (subSequence != null && subSequence.Cases != null)
                {
                    foreach (var mtfCase in subSequence.Cases)
                    {
                        if (mtfCase != currentCase)
                        {
                            mtfCase.IsDefault = false;
                        }
                    }
                }
            }
        }

        private void IsDefaultCheckBoxUnChecked(object sender, RoutedEventArgs e)
        {
            var actualCase = this.Value as MTFCase;
            if (actualCase != null)
            {
                if (actualCase.Value is SequenceVariant)
                {
                    actualCase.Name = actualCase.Value.ToString();
                    editableName = false;
                }
                else
                {
                    editableName = true;
                }
                NotifyPropertyChanged("EditableName");
            }
        }

        private void Editor_OnValueChanged(object sender, PropertyChangedEventArgs e)
        {
            var actualCase = this.Value as MTFCase;
            if (actualCase != null)
            {
                if (actualCase.IsDefault)
                {
                    editableName = false;
                    NotifyPropertyChanged("EditableName");
                }
                else
                {
                    if (actualCase.Value is SequenceVariant)
                    {
                        actualCase.Name = actualCase.Value.ToString();
                        if (editableName)
                        {
                            editableName = false;
                            NotifyPropertyChanged("EditableName");
                        }

                    }
                }
            }
        }
    }
}
