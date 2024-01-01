using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using MTFClientServerCommon;
using MTFClientServerCommon.Mathematics;

namespace MTFApp.UIHelpers.Editors
{
    /// <summary>
    /// Interaction logic for MTFExecuteTermEditor.xaml
    /// </summary>
    public partial class MTFExecuteActivityTermEditor : MTFEditorBase, INotifyPropertyChanged
    {
        public event EventHandler<Type> MethodResultTypeChanged;

        private bool componentIsSelected;
        private MTFSequenceClassInfo selectedClassInfo;
        private List<ExecutableInfo> executables;
        private ExecutableInfo selectedMethod;
        private bool classIsLoaded;
        private bool termIsLoaded;
        private bool parametersAreAssigned;

        public MTFExecuteActivityTermEditor()
        {
            InitializeComponent();
            ExecuteActivityTermEditorRoot.DataContext = this;
        }


        public IList<MTFSequenceClassInfo> ClassInfos
        {
            get => (IList<MTFSequenceClassInfo>)GetValue(ClassInfosProperty);
            set => SetValue(ClassInfosProperty, value);
        }

        public static readonly DependencyProperty ClassInfosProperty =
            DependencyProperty.Register("ClassInfos", typeof(IList<MTFSequenceClassInfo>), typeof(MTFExecuteActivityTermEditor),
                new FrameworkPropertyMetadata(ClassInfosChangedCallback));

        private static void ClassInfosChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = (MTFExecuteActivityTermEditor)d;
            editor.OnClassInfosChanged(e.NewValue as IList<MTFSequenceClassInfo>);
        }

        private void OnClassInfosChanged(IList<MTFSequenceClassInfo> eNewValue)
        {
            classIsLoaded = true;
            UpdateValues();
        }


        public ExecuteActivityTerm Term
        {
            get => (ExecuteActivityTerm)GetValue(TermProperty);
            set => SetValue(TermProperty, value);
        }

        public static readonly DependencyProperty TermProperty =
            DependencyProperty.Register("Term", typeof(ExecuteActivityTerm), typeof(MTFExecuteActivityTermEditor),
                new FrameworkPropertyMetadata(TermChangedCallback));

        private static void TermChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = (MTFExecuteActivityTermEditor)d;
            editor.OnTermChanged(e.NewValue as ExecuteActivityTerm);
        }

        private void OnTermChanged(ExecuteActivityTerm newTerm)
        {
            if (newTerm != null)
            {
                termIsLoaded = true;
                UpdateValues();
            }
        }

        private void UpdateValues()
        {
            if (termIsLoaded && classIsLoaded)
            {
                SelectedClassInfo = ClassInfos?.FirstOrDefault(x => x.Id == Term.ClassInfoId);
                selectedMethod = Executables?.FirstOrDefault(x => x.Name == Term.MethodName);
                ParametersAreAssigned = true;
                NotifyPropertyChanged(nameof(selectedMethod));
            }
        }


        public bool ComponentIsSelected
        {
            get => componentIsSelected;
            set
            {
                componentIsSelected = value;
                NotifyPropertyChanged();
            }
        }

        public MTFSequenceClassInfo SelectedClassInfo
        {
            get => selectedClassInfo;
            set
            {
                selectedClassInfo = value;
                ComponentIsSelected = value != null;
                NotifyPropertyChanged();
                OnSelectionChanged(value);
            }
        }

        public List<ExecutableInfo> Executables
        {
            get => executables;
            set
            {
                executables = value;
                NotifyPropertyChanged();
            }
        }

        public ExecutableInfo SelectedMethod
        {
            get => selectedMethod;
            set
            {
                selectedMethod = value;
                AssignMethodValues(value);
            }
        }

        public bool ParametersAreAssigned
        {
            get => parametersAreAssigned;
            set
            {
                parametersAreAssigned = value;
                NotifyPropertyChanged();
            }
        }

        private void AssignMethodValues(ExecutableInfo value)
        {
            if (value != null && selectedClassInfo?.MTFClass != null)
            {
                switch (value.Type)
                {
                    case ExecutableType.Method:
                        AssignMethodValues(selectedClassInfo.MTFClass.Methods.FirstOrDefault(m => m.Name == value.Name));
                        break;
                    case ExecutableType.Property:
                        AssignMethodValues(selectedClassInfo.MTFClass.Properties.FirstOrDefault(m => m.Name == value.Name));
                        break;
                }

                ParametersAreAssigned = true;
            }
        }

        private void AssignMethodValues(MTFPropertyInfo property)
        {
            if (Term != null)
            {
                Term.MethodName = property.Name;
                Term.ClassInfo = selectedClassInfo;
                Term.ClassInfoId = selectedClassInfo.Id;
                AssignMethodType(property.Type);
                Term.MethodResultType = Type.GetType(property.Type);
                Term.MTFParameters = null;
            }
        }

        private void AssignMethodType(string typeName)
        {
            var type = Type.GetType(typeName);

            if (Term.MethodResultType != type)
            {
                Term.MethodResultType = type;
                MethodResultTypeChanged?.Invoke(this, type);
            }
        }

        private void AssignMethodValues(MTFMethodInfo method)
        {
            if (Term != null)
            {
                Term.MethodName = method.Name;
                Term.ClassInfo = selectedClassInfo;
                Term.ClassInfoId = selectedClassInfo.Id;
                AssignMethodType(method.ReturnType);
                Term.MTFParameters = method.Parameters.Select(x => new MTFParameterValue(x)).ToList();
            }
        }


        private void OnSelectionChanged(MTFSequenceClassInfo newValue)
        {
            if (newValue != null)
            {
                var methods = newValue.MTFClass.Methods.Where(x => x.ReturnType != "System.Void")
                    .Select(x => new ExecutableInfo
                    {
                        Name = x.Name,
                        DisplayName = x.DisplayName,
                        Type = ExecutableType.Method
                    })
                    .Union(newValue.MTFClass.Properties
                        .Where(x => x.CanRead).Select(x => new ExecutableInfo
                        {
                            Name = x.Name,
                            DisplayName = $"Get {x.DisplayName}",
                            Type = ExecutableType.Property
                        }))
                    .OrderBy(x => x.Name);
                Executables = methods.ToList();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ExecutableInfo
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public ExecutableType Type { get; set; }
    }

    public enum ExecutableType
    {
        Method,
        Property
    }
}