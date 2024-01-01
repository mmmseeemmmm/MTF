using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MTFApp.UIHelpers;
using MTFClientServerCommon;
using MTFClientServerCommon.Helpers;

namespace MTFApp.SequenceEditor.Settings
{
    /// <summary>
    /// Interaction logic for GoldSampleSettings.xaml
    /// </summary>
    public partial class GoldSampleSettings : UserControl, INotifyPropertyChanged
    {
        private readonly Command resetGoldSampleCommand;
        private readonly Command addNewStartOfShiftCommand;
        private readonly Command removeStartOfShiftCommand;
        private bool settingIsReady;
        private bool fileIsLoad;
        public GoldSampleSettings()
        {
            InitializeComponent();
            this.Root.DataContext = this;
            resetGoldSampleCommand = new Command(ResetGoldSample, () => FileInfo != null && FileInfo.Exists);
            addNewStartOfShiftCommand = new Command(addNewStartOfShift);
            removeStartOfShiftCommand = new Command(removeStartOfShift);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == IsVisibleProperty && (bool)e.NewValue)
            {
                LoadFileInfoAsync();
            }
        }

        private async void LoadFileInfoAsync()
        {
            if (!settingIsReady || fileIsLoad)
            {
                return;
            }
            var setting = Setting;
            if (setting != null && !string.IsNullOrEmpty(setting.GoldSampleDataFile))
            {
                await Task.Run(() => fileInfo = MTFClient.GetMTFClient().GetGoldSampleDataFileInfo(setting.GoldSampleDataFile));
            }
            else
            {
                fileInfo = null;
            }
            NotifyPropertyChnaged("FileInfo");
            NotifyPropertyChnaged("FileSize");
            resetGoldSampleCommand.RaiseCanExecuteChanged();

        }

        public string FileSize
        {
            get
            {
                if (FileInfo != null && FileInfo.Exists)
                {
                    return string.Format("{0} kB", Math.Round((double)FileInfo.Length / 1024, 1));
                }
                return null;
            }

        }

        public GoldSampleSetting Setting
        {
            get => (GoldSampleSetting)GetValue(SettingProperty);
            set => SetValue(SettingProperty, value);
        }

        public static readonly DependencyProperty SettingProperty =
            DependencyProperty.Register("Setting", typeof(GoldSampleSetting), typeof(GoldSampleSettings),
            new FrameworkPropertyMetadata { PropertyChangedCallback = SettingChangedCallBack });

        private static void SettingChangedCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (GoldSampleSettings)d;
            control.settingIsReady = true;
            control.LoadFileInfoAsync();
        }

        public IEnumerable<EnumValueDescription> SampleValidationModes => EnumHelper.GetAllValuesAndDescriptions<GoldSampleValidationMode>();


        public ICommand ResetGoldSampleCommand => resetGoldSampleCommand;

        public ICommand AddNewStartOfShift => addNewStartOfShiftCommand;

        public ICommand RemoveStartOfShift => removeStartOfShiftCommand;

        private FileInfo fileInfo;

        public FileInfo FileInfo
        {
            get => fileInfo;
            set
            {
                fileInfo = value;
                NotifyPropertyChnaged();
            }
        }

        private void ResetGoldSample()
        {
            if (Setting != null && !string.IsNullOrEmpty(Setting.GoldSampleDataFile))
            {
#if !DEBUG
                var result = MTFMessageBox.Show("Clear stored data.", "Do you really want to clear gold sample stored data?",
                    MTFMessageBoxType.Question, MTFMessageBoxButtons.YesNo);
                if (result == MTFMessageBoxResult.No)
                {
                    return;
                }
#endif
                MTFClient.GetMTFClient().RemoveGoldSampleData(Setting.GoldSampleDataFile);
                LoadFileInfoAsync();
            }
        }

        private void addNewStartOfShift(object param)
        {
            if (Setting.GoldSampleShifts == null)
            {
                Setting.GoldSampleShifts = new MTFObservableCollection<GoldSampleShift>();
            }

            Setting.GoldSampleShifts.Add(new GoldSampleShift());
        }

        private void removeStartOfShift(object param)
        {
            var shift = param as GoldSampleShift;
            if (shift == null)
            {
                return;
            }

            Setting.GoldSampleShifts.Remove(shift);
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChnaged([CallerMemberName] string propertyName = null)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); 
            }
        }
    }
}
