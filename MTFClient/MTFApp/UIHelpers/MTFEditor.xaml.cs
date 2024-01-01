using MTFApp.UIHelpers.Editors;
using MTFClientServerCommon;
using MTFClientServerCommon.Helpers;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using MTFApp.ServerService;
using MTFApp.ServerService.Components;
using MTFApp.UIHelpers.Converters;
using MTFClientServerCommon.Mathematics;

namespace MTFApp.UIHelpers
{
    /// <summary>
    /// Interaction logic for MTFEditor.xaml
    /// </summary>
    public partial class MTFEditor : UserControl
    {
        //private string baseTypeName;
        public delegate void ValueChangedEventHandler(object sender, PropertyChangedEventArgs e);
        public event ValueChangedEventHandler ValueChanged;


        public MTFEditor()
        {
            InitializeComponent();

            root.DataContext = this;
        }

        #region IsCollapsed dependency property
        public bool IsCollapsed
        {
            get { return (bool)GetValue(IsCollapsedProperty); }
            set { SetValue(IsCollapsedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsCollapsed.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsCollapsedProperty =
            DependencyProperty.Register("IsCollapsed", typeof(bool), typeof(MTFEditor),
            new FrameworkPropertyMetadata() { BindsTwoWayByDefault = false });


        #endregion IsCollapsed dependency property

        #region EditorMode Dependency Property
        public EditorModes EditorMode
        {
            get { return (EditorModes)GetValue(EditorModeProperty); }
            set { SetValue(EditorModeProperty, value); }
        }
        public static readonly DependencyProperty EditorModeProperty =
            DependencyProperty.Register("EditorMode", typeof(EditorModes), typeof(MTFEditor),
            new FrameworkPropertyMetadata() { DefaultValue = EditorModes.Standard, BindsTwoWayByDefault = false, PropertyChangedCallback = EditorModePropertyChanged });

        private static void EditorModePropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (source is MTFEditor editor && editor.TypeName != null)
            {
                editor.AssignEditor();
            }
        }
        #endregion EditorMode Dependency Property

        #region Value dependency property
        public object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(MTFEditor),
            new FrameworkPropertyMetadata { BindsTwoWayByDefault = true, PropertyChangedCallback = ValueChangedMethod });

        private static void ValueChangedMethod(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (source is MTFEditor editor)
            {
                editor.ValueChanged?.Invoke(editor, new PropertyChangedEventArgs(e.Property.Name));
            }
        }
        #endregion Value dependency property

        #region TypeName dependency property
        public string TypeName
        {
            get { return (string)GetValue(TypeNameProperty); }
            set { SetValue(TypeNameProperty, value); }
        }
        public static readonly DependencyProperty TypeNameProperty =
            DependencyProperty.Register("TypeName", typeof(string), typeof(MTFEditor),
            new PropertyMetadata(TypeNamePropertyChanged));

        private static void TypeNamePropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (source is MTFEditor editor)
            {
                editor.AssignEditor();
            }
        }
        #endregion TypeName dependency property

        #region ReadOnly dependency property
        public bool ReadOnly
        {
            get { return (bool)GetValue(ReadOnlyProperty); }
            set { SetValue(ReadOnlyProperty, value); }
        }
        public static readonly DependencyProperty ReadOnlyProperty =
            DependencyProperty.Register("ReadOnly", typeof(bool), typeof(MTFEditor),
            new FrameworkPropertyMetadata { DefaultValue = false, PropertyChangedCallback = ReadOnlyPropertyChanged, BindsTwoWayByDefault = false });

        private static void ReadOnlyPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion Value dependency property

        #region TargetType dependency property
        public string TargetType
        {
            get { return (string)GetValue(TargetTypeProperty); }
            set { SetValue(TargetTypeProperty, value); }
        }

        public static readonly DependencyProperty TargetTypeProperty =
            DependencyProperty.Register("TargetType", typeof(string), typeof(MTFEditor),
            new FrameworkPropertyMetadata() { BindsTwoWayByDefault = false });
        #endregion

        #region ParentSequence dependency property

        public MTFSequence ParentSequence
        {
            get { return (MTFSequence)GetValue(ParentSequenceProperty); }
            set { SetValue(ParentSequenceProperty, value); }
        }

        public static readonly DependencyProperty ParentSequenceProperty =
            DependencyProperty.Register("ParentSequence", typeof(MTFSequence), typeof(MTFEditor),
            new FrameworkPropertyMetadata { BindsTwoWayByDefault = false });

        #endregion

        #region ParentActivity dependency property

        public MTFSequenceActivity ParentActivity
        {
            get { return (MTFSequenceActivity)GetValue(ParentActivityProperty); }
            set { SetValue(ParentActivityProperty, value); }
        }

        public static readonly DependencyProperty ParentActivityProperty =
            DependencyProperty.Register("ParentActivity", typeof(MTFSequenceActivity), typeof(MTFEditor),
            new FrameworkPropertyMetadata { BindsTwoWayByDefault = false });

        #endregion

        #region ShowValueList dependency property

        public bool ShowValueList
        {
            get { return (bool)GetValue(ShowValueListProperty); }
            set { SetValue(ShowValueListProperty, value); }
        }

        public static readonly DependencyProperty ShowValueListProperty =
            DependencyProperty.Register("ShowValueList", typeof(bool), typeof(MTFEditor), new PropertyMetadata(false));

        #endregion


        private void AssignEditor()
        {
            mainContent.Content = GetEditor();
        }


        private UserControl GetEditor()
        {
            MTFEditorBase editor;
            TypeInfo typeInfo = new TypeInfo(TypeName);
            if (TypeName == "MTFApp.UIHelpers.TermConverter")
            {
                return null;
            }
            if (typeInfo.IsActivityTarget)
            {
                editor = new MTFActivityTargetEditor();
                editor.SetBinding(MTFActivityTargetEditor.ValueProperty, new Binding { Path = new PropertyPath(ValueProperty) });
                editor.SetBinding(MTFActivityTargetEditor.ReadOnlyProperty, new Binding { Path = new PropertyPath(ReadOnlyProperty) });
            }
            else if (typeInfo.IsSequenceVariantGroup)
            {
                editor = new MTFVariantEditor();
                editor.SetBinding(MTFVariantEditor.ValueProperty, new Binding { Path = new PropertyPath(ValueProperty) });
                editor.SetBinding(MTFVariantEditor.ReadOnlyProperty, new Binding { Path = new PropertyPath(ReadOnlyProperty) });
                editor.SetBinding(MTFVariantEditor.EditorModeProperty, new Binding { Path = new PropertyPath(EditorModeProperty) });
            }
            else if (typeInfo.IsCase)
            {
                editor = new MTFCaseEditor();
                editor.SetBinding(MTFCaseEditor.ValueProperty, new Binding { Path = new PropertyPath(ValueProperty) });
                editor.SetBinding(MTFCaseEditor.ReadOnlyProperty, new Binding { Path = new PropertyPath(ReadOnlyProperty) });
                editor.SetBinding(MTFCaseEditor.EditorModeProperty, new Binding { Path = new PropertyPath(EditorModeProperty) });
            }
            else if (TypeName == "TermDesigner")
            {
                editor = new MTFTermDesigner();
                editor.SetBinding(MTFTermDesigner.ValueProperty, new Binding { Path = new PropertyPath(ValueProperty) });
                editor.SetBinding(MTFTermDesigner.ReadOnlyProperty, new Binding { Path = new PropertyPath(ReadOnlyProperty) });
                editor.SetBinding(MTFTermDesigner.EditorModeProperty, new Binding { Path = new PropertyPath(EditorModeProperty) });
                editor.SetBinding(MTFTermDesigner.TargetTypeProperty, new Binding { Path = new PropertyPath(TargetTypeProperty) });
            }
            else if (typeInfo.IsValidationTableStatus)
            {
                editor = new MTFEnumEditor();
                editor.SetBinding(MTFEnumEditor.ValueProperty, new Binding { Path = new PropertyPath(ValueProperty), Converter = new TableResultConverter()});
                editor.SetBinding(MTFEnumEditor.ReadOnlyProperty, new Binding { Path = new PropertyPath(ReadOnlyProperty) });
                editor.SetBinding(MTFEnumEditor.EditorModeProperty, new Binding { Path = new PropertyPath(EditorModeProperty) });
            }
            else if (EditorMode == EditorModes.OnlyActivityResult)
            {
                editor = new MTFSimpleTermEditor();
                editor.SetBinding(MTFSimpleTermEditor.ValueProperty, new Binding { Path = new PropertyPath(ValueProperty) });
                editor.SetBinding(MTFSimpleTermEditor.ReadOnlyProperty, new Binding { Path = new PropertyPath(ReadOnlyProperty) });
                editor.SetBinding(MTFSimpleTermEditor.EditorModeProperty, new Binding { Path = new PropertyPath(EditorModeProperty) });
            }
            else if (typeInfo.IsValidationTable)
            {
                editor = new MTFValidationTableEditor();
                editor.SetBinding(MTFValidationTableEditor.TableDataProperty, new Binding
                {
                    Path = new PropertyPath(ValueProperty),
                    Converter = new ValidationTableFixConverter()
                });//TODO remove converter in binding and implement better solution
                editor.SetBinding(MTFValidationTableEditor.ReadOnlyProperty, new Binding { Path = new PropertyPath(ReadOnlyProperty) });
                editor.EditorMode = EditorModes.InitValidationTable;
            }
            else if (typeInfo.IsConstantTable)
            {
                editor = new MTFDataTableEditor();
                editor.SetBinding(MTFDataTableEditor.TableDataProperty, new Binding { Path = new PropertyPath(ValueProperty), Converter = new DataTableFixConverter()});
                editor.SetBinding(MTFDataTableEditor.ReadOnlyProperty, new Binding { Path = new PropertyPath(ReadOnlyProperty) });
                editor.EditorMode = EditorModes.InitValidationTable;
            }
            else if (typeInfo.IsChar)
            {
                editor = new MTFCharEditor();
                editor.SetBinding(MTFCharEditor.ValueProperty, new Binding { Path = new PropertyPath(ValueProperty) });
                editor.SetBinding(MTFCharEditor.ReadOnlyProperty, new Binding { Path = new PropertyPath(ReadOnlyProperty) });
                editor.SetBinding(MTFCharEditor.TypeNameProperty, new Binding { Path = new PropertyPath(TypeNameProperty) });
            }
            else if (typeInfo.IsString)
            {
                if (EditorMode == EditorModes.UseTerm || EditorMode == EditorModes.CheckOutputValue)
                {
                    editor = CreateTermEditor();
                }
                else
                {
                    editor = new MTFStringEditor();
                    editor.SetBinding(MTFStringEditor.ValueProperty, new Binding { Path = new PropertyPath(ValueProperty) });
                    editor.SetBinding(MTFStringEditor.ReadOnlyProperty, new Binding { Path = new PropertyPath(ReadOnlyProperty) });
                }
            }
            else if (typeInfo.IsBool)
            {
                if (EditorMode == EditorModes.UseTerm || EditorMode == EditorModes.CheckOutputValue)
                {
                    editor = CreateTermEditor();
                }
                else
                {
                    editor = new MTFBoolEditor();
                    editor.SetBinding(MTFBoolEditor.ValueProperty, new Binding { Path = new PropertyPath(ValueProperty) });
                    editor.SetBinding(MTFBoolEditor.ReadOnlyProperty, new Binding { Path = new PropertyPath(ReadOnlyProperty) });
                }
            }
            else if (typeInfo.IsNumeric)
            {
                if (EditorMode == EditorModes.UseTerm || EditorMode == EditorModes.CheckOutputValue)
                {
                    editor = CreateTermEditor();
                }
                else
                {
                    editor = new MTFNumericEditor();
                    editor.SetBinding(MTFNumericEditor.ValueProperty, new Binding { Path = new PropertyPath(ValueProperty) });
                    editor.SetBinding(MTFNumericEditor.ReadOnlyProperty, new Binding { Path = new PropertyPath(ReadOnlyProperty) });
                    editor.SetBinding(MTFNumericEditor.TypeNameProperty, new Binding { Path = new PropertyPath(TypeNameProperty) });
                }
            }
            else if (typeInfo.IsTerm)
            {
                if (EditorMode == EditorModes.HideTarget)
                {
                    //var parent = UIHelper.FindParent<MTFTermDesigner>(this);
                    editor = CreateTermEditor();
                }
                else
                {
                    editor = new MTFTermEditor();
                    editor.SetBinding(MTFTermEditor.ValueProperty, new Binding { Path = new PropertyPath(ValueProperty) });
                    editor.SetBinding(MTFTermEditor.ReadOnlyProperty, new Binding { Path = new PropertyPath(ReadOnlyProperty) });
                    editor.SetBinding(MTFTermEditor.EditorModeProperty, new Binding { Path = new PropertyPath(EditorModeProperty) });
                    (editor as MTFTermEditor).ResultTypeName = TypeName;
                }
            }
            else if (typeInfo.IsGenericType || typeInfo.IsArray)
            {
                editor = new MTFListEditor();
                editor.SetBinding(MTFListEditor.EditorModeProperty, new Binding { Path = new PropertyPath(EditorModeProperty) });
                editor.SetBinding(MTFListEditor.ValueProperty, new Binding { Path = new PropertyPath(ValueProperty) });
                editor.SetBinding(MTFListEditor.ReadOnlyProperty, new Binding { Path = new PropertyPath(ReadOnlyProperty) });
                editor.SetBinding(MTFListEditor.TypeNameProperty, new Binding { Path = new PropertyPath(TypeNameProperty) });
                editor.IsCollapsed = Settings.SettingsPresenter.MtfEditorIsCollapsed;
            }
            else if (typeInfo.IsImage)
            {
                editor = new MTFImageEditor();
                editor.SetBinding(MTFImageEditor.ValueProperty, new Binding { Path = new PropertyPath(ValueProperty) });
                editor.SetBinding(MTFImageEditor.ReadOnlyProperty, new Binding { Path = new PropertyPath(ReadOnlyProperty) });
            }
            else if (typeInfo.IsUnknownType || typeInfo.IsGenericClassInstanceConfiguration)
            {
                editor = new MTFObjectEditor();
                editor.SetBinding(MTFObjectEditor.EditorModeProperty, new Binding { Path = new PropertyPath(EditorModeProperty) });
                editor.SetBinding(MTFObjectEditor.ValueProperty, new Binding { Path = new PropertyPath(ValueProperty) });
                editor.SetBinding(MTFObjectEditor.ReadOnlyProperty, new Binding { Path = new PropertyPath(ReadOnlyProperty) });
                editor.SetBinding(MTFObjectEditor.TypeNameProperty, new Binding { Path = new PropertyPath(TypeNameProperty) });
                System.Threading.Tasks.Task.Run(() =>
                {
                    string typeName = string.Empty;
                    App.Current.Dispatcher.Invoke(() => (typeName = TypeName));
                    var ci = ServiceClientsContainer.Get<ComponentsClient>().GetClassInfo(typeName);
                    App.Current.Dispatcher.Invoke(() => (editor as MTFObjectEditor).ClassInfo = ci);
                });
                editor.IsCollapsed = Settings.SettingsPresenter.MtfEditorIsCollapsed;
            }
            else if (typeInfo.IsStringFormat)
            {
                editor = new MTFStringFormatEditor();
                editor.SetBinding(MTFStringFormatEditor.ValueProperty, new Binding { Path = new PropertyPath(ValueProperty) });
                editor.SetBinding(MTFStringFormatEditor.ReadOnlyProperty, new Binding { Path = new PropertyPath(ReadOnlyProperty) });
            }
            else if (typeInfo.IsListOperation)
            {
                editor = new MTFListOperationEditor();
                editor.SetBinding(MTFListOperationEditor.EditorModeProperty, new Binding { Path = new PropertyPath(EditorModeProperty) });
                editor.SetBinding(MTFListOperationEditor.ValueProperty, new Binding { Path = new PropertyPath(ValueProperty) });
                editor.SetBinding(MTFListOperationEditor.ReadOnlyProperty, new Binding { Path = new PropertyPath(ReadOnlyProperty) });
            }
            else
            {
                editor = new MTFNotDefinedEditor();
            }
            editor.SetBinding(MTFEditorBase.ParentSequenceProperty, new Binding { Path = new PropertyPath(ParentSequenceProperty) });
            editor.SetBinding(MTFEditorBase.ParentActivityProperty, new Binding { Path = new PropertyPath(ParentActivityProperty) });
            editor.SetBinding(MTFEditorBase.ShowValueListProperty, new Binding {Path = new PropertyPath(ShowValueListProperty)});
            return editor;
        }

        private MTFEditorBase CreateTermEditor()
        {
            MTFEditorBase editor;
            var c = new TermConverter(TypeName);
            var useSimple = UIHelper.FindParent<MTFTermDesigner>(this) == null;

            if (Value == null)
            {
                if (TypeName == typeof(Term).FullName)
                {
                    useSimple = false;
                }
                else
                {
                    var type = Type.GetType(TypeName);

                    Value = TermHelper.CreateConstantTermByType(type, TypeName == typeof(string).FullName ? string.Empty : Activator.CreateInstance(type));
                }
            }
            else
            {
                var valueType = Value.GetType();

                if (valueType.IsValueType || valueType == typeof(string))
                {
                    var type = Type.GetType(TypeName);

                    Value = TermHelper.CreateConstantTermByType(type, Value);
                }
                
            }

            if (useSimple)
            {
                editor = new MTFSimpleTermEditor();
                editor.SetBinding(MTFSimpleTermEditor.EditorModeProperty, new Binding { Path = new PropertyPath(EditorModeProperty) });
                editor.SetBinding(MTFSimpleTermEditor.ValueProperty, new Binding { Path = new PropertyPath(ValueProperty), Converter = c });
                editor.SetBinding(MTFSimpleTermEditor.ReadOnlyProperty, new Binding { Path = new PropertyPath(ReadOnlyProperty) });

                return editor;
            }
            else
            {
                editor = new MTFTermEditor();
                editor.SetBinding(MTFTermEditor.ValueProperty, new Binding { Path = new PropertyPath(ValueProperty), Converter = c });
                editor.SetBinding(MTFTermEditor.ReadOnlyProperty, new Binding { Path = new PropertyPath(ReadOnlyProperty) });
                editor.SetBinding(MTFTermEditor.EditorModeProperty, new Binding { Path = new PropertyPath(EditorModeProperty) });
                (editor as MTFTermEditor).ResultTypeName = TypeName;

                return editor;
            }
        }

        public bool IsInTermDesigner => UIHelper.FindParent<MTFTermDesigner>(this) != null;

    }



    public class TermConverter : IValueConverter
    {
        Type type;

        public TermConverter(string typeName)
        {
            type = Type.GetType(typeName);
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                if (type!=null)
                {
                    value = type == typeof(string) ? string.Empty : Activator.CreateInstance(type); 
                }
                else
                {
                    return null;
                }
            }
            if (!(value is MTFClientServerCommon.Mathematics.Term))
            {
                return MTFClientServerCommon.Mathematics.TermHelper.CreateConstantTermByType(type, value);
                //return new MTFClientServerCommon.Mathematics.ConstantTerm(type) { Value = value, TargetType = type.FullName };
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

}
