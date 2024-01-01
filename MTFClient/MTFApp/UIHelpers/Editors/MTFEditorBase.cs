using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using MTFClientServerCommon;

namespace MTFApp.UIHelpers.Editors
{
    public class MTFEditorBase : MTFUserControl, INotifyPropertyChanged
    {
        private bool isCollapsed = true;
        protected readonly SettingsClass Setting = StoreSettings.GetInstance.SettingsClass;

        public bool IsCollapsed
        {
            get => isCollapsed;
            set
            {
                isCollapsed = value;
                NotifyPropertyChanged();
            }
        }

        #region EditorMode Dependency Property
        public EditorModes EditorMode
        {
            get => (EditorModes)GetValue(EditorModeProperty);
            set => SetValue(EditorModeProperty, value);
        }
        public static readonly DependencyProperty EditorModeProperty =
            DependencyProperty.Register("EditorMode", typeof(EditorModes), typeof(MTFEditorBase),
            new FrameworkPropertyMetadata() { DefaultValue = EditorModes.Standard, BindsTwoWayByDefault = false });
        #endregion EditorMode Dependency Property

        #region ReadOnly dependency property
        public bool ReadOnly
        {
            get => (bool)GetValue(ReadOnlyProperty);
            set => SetValue(ReadOnlyProperty, value);
        }
        public static readonly DependencyProperty ReadOnlyProperty =
            DependencyProperty.Register("ReadOnly", typeof(bool), typeof(MTFEditorBase),
            new PropertyMetadata(ReadOnlyPropertyChanged));

        private static void ReadOnlyPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (source is MTFEditorBase editor)
            {
                editor.OnReadOnlyPropertyChanged(source, e);
            }
        }
        #endregion ReadOnly dependency property

        #region Value dependency property
        public object Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(MTFEditorBase),
            new FrameworkPropertyMetadata { BindsTwoWayByDefault = true, PropertyChangedCallback = ValuePropertyChanged });

        private static void ValuePropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (source is MTFEditorBase editor)
            {
                editor.OnPropertyChanged(source, e);
            }
        }
        #endregion Value dependency property

        #region ParentSequence dependency property

        public MTFSequence ParentSequence
        {
            get => (MTFSequence)GetValue(ParentSequenceProperty);
            set => SetValue(ParentSequenceProperty, value);
        }

        public static readonly DependencyProperty ParentSequenceProperty =
            DependencyProperty.Register("ParentSequence", typeof(MTFSequence), typeof(MTFEditorBase),
            new FrameworkPropertyMetadata { BindsTwoWayByDefault = false, PropertyChangedCallback = ParentSequenceChanged});

        private static void ParentSequenceChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (source is MTFEditorBase editor)
            {
                editor.OnPropertyChanged(source, e);
            }
        }

        #endregion

        #region ParentActivity dependency property

        public MTFSequenceActivity ParentActivity
        {
            get => (MTFSequenceActivity)GetValue(ParentActivityProperty);
            set => SetValue(ParentActivityProperty, value);
        }

        public static readonly DependencyProperty ParentActivityProperty =
            DependencyProperty.Register("ParentActivity", typeof(MTFSequenceActivity), typeof(MTFEditorBase),
            new FrameworkPropertyMetadata { BindsTwoWayByDefault = false, PropertyChangedCallback = ParentActivityChanged});

        private static void ParentActivityChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (source is MTFEditorBase editor)
            {
                editor.OnPropertyChanged(source, e);
            }
        }

        #endregion


        public bool ShowValueList
        {
            get { return (bool)GetValue(ShowValueListProperty); }
            set { SetValue(ShowValueListProperty, value); }
        }

        public static readonly DependencyProperty ShowValueListProperty =
            DependencyProperty.Register("ShowValueList", typeof(bool), typeof(MTFEditorBase), new PropertyMetadata(false));


        public System.Windows.Input.ICommand ChangeCollapsedStateCommand
        {
            get
            {
                return new Command(param =>
                {
                    (param as MTFEditorBase).IsCollapsed = !(param as MTFEditorBase).IsCollapsed;
                    NotifyPropertyChanged(nameof(IsCollapsed));
                });
            }
        }

        protected virtual void OnReadOnlyPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            IsCollapsed = (bool)e.NewValue;
        }

        protected virtual void OnPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
