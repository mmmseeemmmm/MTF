using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using MTFApp.UIHelpers;
using MTFClientServerCommon;
using MTFClientServerCommon.Helpers;

namespace MTFApp.SequenceEditor
{
    /// <summary>
    /// Interaction logic for ServiceCommandSettings.xaml
    /// </summary>
    public partial class ServiceCommandSettings : UserControl, INotifyPropertyChanged
    {
        private bool canUpdateValue = true;
        private Command selectedServiceCommandMoveUpCommand;
        private Command selectedServiceCommandMoveDownCommand;

        public ServiceCommandSettings()
        {
            InitializeComponent();
            Root.DataContext = this;
            initializeCommands();
        }

        private void initializeCommands()
        {
            selectedServiceCommandMoveUpCommand = new Command(selectedServiceCommandMoveUp);
            selectedServiceCommandMoveDownCommand = new Command(selectedServiceCommandMoveDown);
        }

        public MTFServiceCommand ServiceCommand
        {
            get => (MTFServiceCommand)GetValue(ServiceCommandProperty);
            set => SetValue(ServiceCommandProperty, value);
        }

        public static readonly DependencyProperty ServiceCommandProperty =
            DependencyProperty.Register("ServiceCommand", typeof(MTFServiceCommand), typeof(ServiceCommandSettings), new FrameworkPropertyMetadata { PropertyChangedCallback = ServiceCommandChangedCallback });

        private static void ServiceCommandChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (dependencyObject is ServiceCommandSettings control)
            {
                if (dependencyPropertyChangedEventArgs.NewValue is MTFServiceCommand newServiceCommand)
                {
                    if (control.allServiceVariants != null)
                    {
                        UnregisterEvents(control.allServiceVariants, control);
                    }
                    control.allServiceVariants = GenerateServiceVariants(newServiceCommand, control);
                    control.NotifyPropertyChnaged("AllServiceVariants");
                }
            }
        }

        public MTFServiceDesignSetting ServiceDesignSetting
        {
            get => (MTFServiceDesignSetting)GetValue(ServiceDesignSettingProperty);
            set => SetValue(ServiceDesignSettingProperty, value);
        }

        public static readonly DependencyProperty ServiceDesignSettingProperty =
            DependencyProperty.Register("ServiceDesignSetting", typeof(MTFServiceDesignSetting), typeof(ServiceCommandSettings), 
            new FrameworkPropertyMetadata());


        private static void UnregisterEvents(IEnumerable<ServiceModeVariantWrapper> serviceModeVariantWrappers, ServiceCommandSettings control)
        {
            foreach (var serviceModeVariantWrapper in serviceModeVariantWrappers)
            {
                serviceModeVariantWrapper.PropertyChanged -= control.ServiceModeVariantWrapperChanged;
            }
        }

        private static IEnumerable<ServiceModeVariantWrapper> GenerateServiceVariants(MTFServiceCommand serviceCommand, ServiceCommandSettings control)
        {
            var variants = Enum.GetValues(typeof(MTFServiceModeVariants)).
                OfType<MTFServiceModeVariants>().Select(x =>
                                                        {
                                                            var wrapper = new ServiceModeVariantWrapper { ModeVariant = x };
                                                            wrapper.PropertyChanged += control.ServiceModeVariantWrapperChanged;
                                                            return wrapper;
                                                        }).ToList();
            if (serviceCommand != null && serviceCommand.UsedServiceVariants != null && serviceCommand.UsedServiceVariants.Count > 0)
            {
                control.canUpdateValue = false;
                foreach (var serviceModeVariantWrapper in variants)
                {
                    serviceModeVariantWrapper.IsUsed = serviceCommand.UsedServiceVariants.Any(x => x == serviceModeVariantWrapper.ModeVariant);
                }
                control.canUpdateValue = true;
            }
            return variants;
        }

        private void ServiceModeVariantWrapperChanged(object sender, PropertyChangedEventArgs e)
        {
            if (ServiceCommand != null && canUpdateValue)
            {
                ServiceCommand.UsedServiceVariants = allServiceVariants.Where(x => x.IsUsed).Select(x => x.ModeVariant).ToList();
            }
        }


        public MTFObservableCollection<MTFServiceCommand> AllServiceCommands
        {
            get { return (MTFObservableCollection<MTFServiceCommand>)GetValue(AllServiceCommandsProperty); }
            set { SetValue(AllServiceCommandsProperty, value); }
        }

        public static readonly DependencyProperty AllServiceCommandsProperty =
            DependencyProperty.Register("AllServiceCommands", typeof(MTFObservableCollection<MTFServiceCommand>), typeof(ServiceCommandSettings), new FrameworkPropertyMetadata { PropertyChangedCallback = ServiceCommandChangedCallback });



        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChnaged([CallerMemberName] string propertyName = null)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); 
            }
        }

        private void selectedServiceCommandMoveUp()
        {
            if (ServiceCommand != null)
            {
                var index = AllServiceCommands.IndexOf(ServiceCommand);
                if (index > 0)
                {
                    AllServiceCommands.Move(index, index - 1);
                }

            }
        }

        private void selectedServiceCommandMoveDown()
        {
            if (ServiceCommand != null)
            {
                var index = AllServiceCommands.IndexOf(ServiceCommand);
                if (index < AllServiceCommands.Count - 1)
                {
                    AllServiceCommands.Move(index, index + 1);
                }

            }
        }

        private IEnumerable<ServiceModeVariantWrapper> allServiceVariants;
        public IEnumerable<ServiceModeVariantWrapper> AllServiceVariants => allServiceVariants;

        public IEnumerable<EnumValueDescription> CommandTypes
        {
            get { return EnumHelper.GetAllValuesAndDescriptions<MTFServiceCommandType>(); }
        }

        public Command SelectedServiceCommandMoveUpCommand => selectedServiceCommandMoveUpCommand;
        public Command SelectedServiceCommandMoveDownCommand => selectedServiceCommandMoveDownCommand;
    }


    public class ServiceModeVariantWrapper : NotifyPropertyBase
    {
        private bool isUsed;

        public bool IsUsed
        {
            get => isUsed;
            set
            {
                isUsed = value;
                NotifyPropertyChanged();
            }
        }

        public string ModeVariantDesc => ModeVariant.Description();

        public MTFServiceModeVariants ModeVariant { get; set; }
    }
}
