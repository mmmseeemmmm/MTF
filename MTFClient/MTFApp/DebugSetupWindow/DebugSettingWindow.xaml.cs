using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MTFApp.SequenceExecution.Helpers;
using MTFApp.UIHelpers;
using MTFClientServerCommon;

namespace MTFApp.DebugSetupWindow
{
    /// <summary>
    /// Interaction logic for BreakPointsWindow.xaml
    /// </summary>
    public sealed partial class DebugSettingWindow : DebugSetupBase
    {
        private readonly Action<MTFActivityVisualisationWrapper> changeBreakPointAction;
        
        private Command deleteAllPointsCommand;
        private Command deletePointCommand;

        public DebugSettingWindow(ObservableCollection<ExtendedModeActivitySetting> dataCollection, Action<MTFActivityVisualisationWrapper> setSelectedItem,
            Action<MTFActivityVisualisationWrapper> changeBreakPointAction)
            : base(dataCollection, setSelectedItem)
        {
            this.changeBreakPointAction = changeBreakPointAction;
            WindowSetting = StoreSettings.GetInstance.SettingsClass.DebugSetting;
            AdjustSize();

            InitializeComponent();
            Root.DataContext = this;

            InitCommands();
        }

        protected override void InitCommands()
        {
            base.InitCommands();
            
            deleteAllPointsCommand = new Command(DeleteAllPoints);
            deletePointCommand = new Command(DeleteSelectedPoint, () => SelectedItem != null);
        }

        public ICommand DeleteAllPointsCommand
        {
            get { return deleteAllPointsCommand; }
        }

        public ICommand DeletePointCommand
        {
            get { return deletePointCommand; }
        }

        private void DeletePoint(ExtendedModeActivitySetting activitySetting)
        {
            activitySetting.State = StateDebugSetup.None;
            if (activitySetting.ActivityWrapper != null)
            {
                activitySetting.ActivityWrapper.IsBreakPoint = StateDebugSetup.None;
            }
        }

        protected override void SelectionChanged()
        {
            base.SelectionChanged();
            deletePointCommand.RaiseCanExecuteChanged();
        }

        protected override void OnClosed(EventArgs e)
        {
            WindowSetting = StoreSettings.GetInstance.SettingsClass.DebugSetting;
            base.OnClosed(e);
        }

        private void DeleteSelectedPoint()
        {
            if (SelectedItem != null)
            {
                DeletePoint(SelectedItem);
                DataCollection.Remove(SelectedItem);
                SelectedItem = null;
            }
        }

        private void DeleteAllPoints()
        {
            if (DataCollection != null)
            {
                foreach (var activitySetting in DataCollection)
                {
                    DeletePoint(activitySetting);
                }
                DataCollection.Clear();
            }
        }

        protected override void EnableAllBreakPoints()
        {
            var collection = ShowFiltredItems ? FiltredItems : DataCollection;

            var debugState = collection.All(x => x.State == StateDebugSetup.Active) ? StateDebugSetup.Deactive : StateDebugSetup.Active;

            foreach (var activitySetting in collection)
            {
                ChangeState(activitySetting, debugState);
            }
        }

        private void BreakPointOnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var uiElement = (FrameworkElement)sender;
            var activitySetting = uiElement.Tag as ExtendedModeActivitySetting;
            if (activitySetting!=null)
            {
                switch (activitySetting.State)
                {
                    case StateDebugSetup.Active:
                        ChangeState(activitySetting, StateDebugSetup.Deactive);
                        break;
                    case StateDebugSetup.Deactive:
                        ChangeState(activitySetting, StateDebugSetup.Active);
                        break;
                }
            }
        }

        private void ChangeState(ExtendedModeActivitySetting activitySetting, StateDebugSetup state)
        {
            
            activitySetting.State = state;
            if (activitySetting.ActivityWrapper!=null)
            {
                activitySetting.ActivityWrapper.IsBreakPoint = state;
                changeBreakPointAction(activitySetting.ActivityWrapper);
            }
        }
    }
}
