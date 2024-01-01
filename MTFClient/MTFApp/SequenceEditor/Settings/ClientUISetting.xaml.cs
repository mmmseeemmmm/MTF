using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MTFApp.UIHelpers;
using MTFClientServerCommon;

namespace MTFApp.SequenceEditor.Settings
{
    /// <summary>
    /// Interaction logic for ClientUISetting.xaml
    /// </summary>
    public partial class ClientUISetting : UserControl, INotifyPropertyChanged
    {
        public ClientUISetting()
        {
            InitializeComponent();
            Root.DataContext = this;
        }

        public IEnumerable<ClientContolInfoWrapper> ClientUis { get; set; }

        public SequenceExecutionUISetting UiSetting { get; set; }

        public MTFSequence Sequence
        {
            get => (MTFSequence)GetValue(SequenceProperty);
            set => SetValue(SequenceProperty, value);
        }



        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register("IsOpen", typeof(bool), typeof(ClientUISetting),
            new FrameworkPropertyMetadata { PropertyChangedCallback = IsOpenChanged, BindsTwoWayByDefault = false });

        private static void IsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var uiSetting = (ClientUISetting)d;
            if (!(bool)e.NewValue)
            {
                uiSetting.UpdateSetting();
            }
        }


        public static readonly DependencyProperty SequenceProperty =
            DependencyProperty.Register("Sequence", typeof(MTFSequence), typeof(ClientUISetting),
            new FrameworkPropertyMetadata { PropertyChangedCallback = SequenceChanged });

        private static void SequenceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var uiSetting = (ClientUISetting)d;
            uiSetting.GenerateSetting();
        }

        private void GenerateSetting()
        {
            if (Sequence == null)
            {
                return;
            }
            var availableControls = Sequence.GetAvailableClientControls();
            if (Sequence.SequenceExecutionUiSetting == null)
            {
                Sequence.SequenceExecutionUiSetting = new SequenceExecutionUISetting();
            }
            UiSetting = Sequence.SequenceExecutionUiSetting;
            NotifyPropertyChanged("UiSetting");
            var originalSelectedUis = Sequence.SequenceExecutionUiSetting.SelectedClientUis ?? new List<ClientUiIdentification>();

            ClientUis =
                availableControls.Select(x =>
                {
                    var original =
                        originalSelectedUis.FirstOrDefault(
                            o => o.AssemblyName == x.AssemblyName && o.TypeName == x.TypeName);
                    var wrapper = new ClientContolInfoWrapper
                    {
                        ClientControl = x,
                        IsSelected = original != null,
                        IsActive = original != null && original.IsActive
                    };
                    wrapper.PropertyChanged += SettingChanged;
                    return wrapper;
                }).ToList();
            NotifyPropertyChanged("ClientUis");
        }

        private void UpdateSetting()
        {
            Sequence.SequenceExecutionUiSetting.SelectedClientUis =
                ClientUis.Where(x => x.IsSelected)
                    .Select(
                        x =>
                            new ClientUiIdentification
                            {
                                AssemblyName = x.ClientControl.AssemblyName,
                                TypeName = x.ClientControl.TypeName,
                                IsActive = x.IsActive
                            })
                    .ToList();
        }

        void SettingChanged(object sender, PropertyChangedEventArgs e)
        {
            var clientWrapper = (ClientContolInfoWrapper)sender;
            if (e.PropertyName == ClientContolInfoWrapper.IsActivePropertyName && clientWrapper.IsActive)
            {
                if (ClientUis!=null)
                {
                    foreach (var clientUi in ClientUis)
                    {
                        if (clientUi!=clientWrapper)
                        {
                            clientUi.IsActive = false;
                        }
                    }
                }
            }
            if (e.PropertyName == ClientContolInfoWrapper.IsSelectedPropertyName && !clientWrapper.IsSelected && clientWrapper.IsActive)
            {
                clientWrapper.IsActive = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); 
            }
        }

        private void ListBox_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            UIHelper.RaiseScrollEvent(sender, e);
        }
    }


    public class ClientContolInfoWrapper : NotifyPropertyBase
    {
        public const string IsActivePropertyName = "IsActive";
        public const string IsSelectedPropertyName = "IsSelected";
        private bool isSelected;
        private bool isActive;

        public ClientContolInfo ClientControl { get; set; }

        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsActive
        {
            get => isActive;
            set
            {
                isActive = value;
                NotifyPropertyChanged();
            }
        }
    }


}
