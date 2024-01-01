using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MTFApp.SequenceExecution.Helpers;
using MTFClientServerCommon;

namespace MTFApp.DebugSetupWindow
{
    /// <summary>
    /// Interaction logic for SetupSettingWindow.xaml
    /// </summary>
    public sealed partial class SetupSettingWindow : DebugSetupBase
    {
        private readonly Action<MTFActivityVisualisationWrapper> changeSetupPoint;

        public SetupSettingWindow(ObservableCollection<ExtendedModeActivitySetting> dataCollection, Action<MTFActivityVisualisationWrapper> setSelectedItem,
            Action<MTFActivityVisualisationWrapper> changeSetupPoint)
            : base(dataCollection, setSelectedItem)
        {
            this.changeSetupPoint = changeSetupPoint;
            WindowSetting = StoreSettings.GetInstance.SettingsClass.SetupSetting;
            AdjustSize();

            InitializeComponent();
            Root.DataContext = this;

            InitCommands();
        }


        protected override void EnableAllBreakPoints()
        {
            var collection = ShowFiltredItems ? FiltredItems : DataCollection;

            var setupState = collection.All(x => x.State == StateDebugSetup.Active) ? StateDebugSetup.None : StateDebugSetup.Active;

            foreach (var activitySetting in collection)
            {
                ChangeState(activitySetting, setupState);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            WindowSetting = StoreSettings.GetInstance.SettingsClass.SetupSetting;
            base.OnClosed(e);
        }

        private void SetupPointOnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var uiElement = (FrameworkElement)sender;
            var activitySetting = uiElement.Tag as ExtendedModeActivitySetting;
            if (activitySetting != null)
            {
                if (activitySetting.State == StateDebugSetup.Active)
                {
                    ChangeState(activitySetting, StateDebugSetup.None);
                }
                else 
                {
                    ChangeState(activitySetting, StateDebugSetup.Active);
                }
            }
        }

        private void ChangeState(ExtendedModeActivitySetting activitySetting, StateDebugSetup state)
        {
            activitySetting.State = state;
            if (activitySetting.ActivityWrapper != null)
            {
                activitySetting.ActivityWrapper.IsSetupMode = state;
                changeSetupPoint(activitySetting.ActivityWrapper);
            }
        }
    }
}
