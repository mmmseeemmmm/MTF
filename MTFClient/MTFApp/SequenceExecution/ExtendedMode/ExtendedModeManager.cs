using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using MTFApp.DebugSetupWindow;
using MTFApp.SequenceExecution.Helpers;
using MTFApp.UIHelpers;
using MTFClientServerCommon;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.Helpers;

namespace MTFApp.SequenceExecution.ExtendedMode
{
    public class ExtendedModeManager
    {
        private readonly Action<MTFActivityVisualisationWrapper> selectItemAction;
        private readonly Action<MTFActivityVisualisationWrapper> changeBreakPointAction;
        private readonly Action<MTFActivityVisualisationWrapper> changeSetupPointAction;
        private readonly ObservableCollection<ExtendedModeActivitySetting> breakPoints = new ObservableCollection<ExtendedModeActivitySetting>();
        private readonly ObservableCollection<ExtendedModeActivitySetting> setupModes = new ObservableCollection<ExtendedModeActivitySetting>();
        private List<ExtendedModeActivitySetting> dynamicSetupModes = new List<ExtendedModeActivitySetting>();
        private SettingDebugAndSetupMode breakPointData = new SettingDebugAndSetupMode();
        private SettingDebugAndSetupMode setupData = new SettingDebugAndSetupMode();
        private DebugSetupBase debugWindow;
        private DebugSetupBase setupWindow;

        public ExtendedModeManager(ExtendedModeActions actions)
        {
            selectItemAction = actions.SelectItemAction;
            changeBreakPointAction = actions.ChangeBreakPointAction;
            changeSetupPointAction = actions.ChangeSetupPointAction;
        }

        public ObservableCollection<ExtendedModeActivitySetting> BreakPoints => breakPoints;

        public ObservableCollection<ExtendedModeActivitySetting> SetupModes => setupModes;

        public bool EnableDebug { get; private set; }
        public bool EnableSetup { get; private set; }

        public SettingDebugAndSetupMode BreakPointData => breakPointData;

        public SettingDebugAndSetupMode SetupData => setupData;

        public void ChangePointState(MTFActivityVisualisationWrapper activityWrapper, ExtendedModeTypes mode)
        {
            switch (mode)
            {
                case ExtendedModeTypes.Debug:
                    ChangeBreakPoint(activityWrapper);
                    break;
                case ExtendedModeTypes.Setup:
                    ChangeSetupPoint(activityWrapper);
                    break;

            }
        }

        public void ChangeBreakPoint(MTFActivityVisualisationWrapper activityWrapper)
        {
            var point = breakPoints.FirstOrDefault(x => GuidHelper.CompareGuidPath(x.GuidPath, activityWrapper.GuidPath));
            if (point != null)
            {
                if (activityWrapper.IsBreakPoint == StateDebugSetup.None)
                {
                    breakPoints.Remove(point);
                }
                else
                {
                    point.State = activityWrapper.IsBreakPoint;
                }
            }
            else
            {
                if (activityWrapper.IsBreakPoint != StateDebugSetup.None)
                {
                    var newPoint = new ExtendedModeActivitySetting(activityWrapper) { State = activityWrapper.IsBreakPoint };
                    breakPoints.Add(newPoint);
                }
            }
        }

        public void AddBreakPoint(ExtendedModeActivitySetting breakPoint) => breakPoints.Add(breakPoint);

        public void ChangeSetupPoint(MTFActivityVisualisationWrapper activityWrapper)
        {
            var point = setupModes.FirstOrDefault(x => GuidHelper.CompareGuidPath(x.GuidPath, activityWrapper.GuidPath));
            if (point != null)
            {
                point.State = activityWrapper.IsSetupMode;
            }
        }

        public void AddSetupPoint(MTFActivityVisualisationWrapper activityWrapper)
        {
            var newItem = new ExtendedModeActivitySetting(activityWrapper);
            UIHelper.InvokeOnDispatcher(() => setupModes.Add(newItem));
            if (activityWrapper.IsDynamicActivity && dynamicSetupModes!=null)
            {
                var data = dynamicSetupModes.FirstOrDefault(x => GuidHelper.CompareGuidPath(x.GuidPath, activityWrapper.GuidPath));
                if (data!=null)
                {
                    newItem.State = data.State;
                    activityWrapper.IsSetupMode = data.State;
                }
            }
        }


        public void Clear()
        {
            setupModes.Clear();
            breakPoints.Clear();
        }

        public void Init(string sequenceName)
        {
            breakPoints.Clear();
            LoadBreakPoints(sequenceName);
            LoadSetupMode(sequenceName);
        }

        private void LoadBreakPoints(string sequenceName)
        {
            var path = Path.Combine(BaseConstants.BreakPointsLocation, $"{sequenceName}{BaseConstants.BreakPointsExtension}");

            breakPointData = XmlOperations.LoadXmlData<SettingDebugAndSetupMode>(path);
            if (breakPointData.Active)
            {
                EnableDebug = true;
                SwitchDebugWindow(true);
            }
        }

        private void LoadSetupMode(string sequenceName)
        {
            var path = Path.Combine(BaseConstants.SetupModeLocation, $"{sequenceName}{BaseConstants.SetupModeExtension}");

            var data = XmlOperations.LoadXmlData<SettingDebugAndSetupMode>(path);

            if (!data.IsEmpty)
            {
                foreach (var activitySetting in setupModes)
                {
                    var loadedData = data.ListActivities.FirstOrDefault(x => GuidHelper.CompareGuidPath(activitySetting.GuidPath, x.GuidPath));
                    if (loadedData!=null)
                    {
                        activitySetting.State = loadedData.State;
                        activitySetting.ActivityWrapper.IsSetupMode = loadedData.State;
                    }
                }

                dynamicSetupModes = data.ListActivities.Where(x => x.IsDynamic).ToList();
            }

            if (data.Active)
            {
                EnableSetup = true;
                SwitchSetupWindow(true);
            }

        }

        public void SaveData(Guid sequenceId, string sequenceName, ExtendedModeTypes mode)
        {
            switch (mode)
            {
                case ExtendedModeTypes.Debug:
                    SavePoints(sequenceId, sequenceName, breakPointData, breakPoints, BaseConstants.BreakPointsLocation, BaseConstants.BreakPointsExtension);
                    break;
                case ExtendedModeTypes.Setup:
                    SavePoints(sequenceId, sequenceName, setupData, setupModes, BaseConstants.SetupModeLocation, BaseConstants.SetupModeExtension);
                    break;
            }
        }

        public void SwichtExtendedMode(bool state, ExtendedModeTypes mode)
        {
            ChangeWindowState(state, mode);
            switch (mode)
            {
                case ExtendedModeTypes.Debug:
                    breakPointData.Active = state;
                    break;
                case ExtendedModeTypes.Setup:
                    setupData.Active = state;
                    break;
            }
        }

        private void ChangeWindowState(bool state, ExtendedModeTypes mode)
        {
            switch (mode)
            {
                case ExtendedModeTypes.Debug:
                    SwitchDebugWindow(state);
                    break;
                case ExtendedModeTypes.Setup:
                    SwitchSetupWindow(state);
                    break;
            }
        }

        private void SwitchDebugWindow(bool state)
        {
            UIHelper.InvokeOnDispatcher(() =>
                                        {
                                            if (state)
                                            {
                                                if (debugWindow == null)
                                                {
                                                    debugWindow = new DebugSettingWindow(breakPoints, selectItemAction, changeBreakPointAction);
                                                }

                                                debugWindow.Show();
                                            }
                                            else
                                            {
                                                if (debugWindow != null)
                                                {
                                                    debugWindow.Close();
                                                    debugWindow = null;
                                                }
                                            }
                                        });
        }

        private void SwitchSetupWindow(bool state)
        {
            UIHelper.InvokeOnDispatcher(() =>
                                        {
                                            if (state)
                                            {
                                                if (setupWindow == null)
                                                {
                                                    setupWindow = new SetupSettingWindow(setupModes, selectItemAction, changeSetupPointAction);
                                                }

                                                setupWindow.Show();
                                            }
                                            else
                                            {
                                                if (setupWindow != null)
                                                {
                                                    setupWindow.Close();
                                                    setupWindow = null;
                                                }
                                            }
                                        });
        }

        private void SavePoints(Guid sequenceId, string sequenceName, SettingDebugAndSetupMode data, IEnumerable<ExtendedModeActivitySetting> pointCollection, string location, string extension)
        {
            data.SequenceId = sequenceId;
            data.ListActivities = pointCollection.ToList();
            if (!data.IsEmpty)
            {
                var path = Path.Combine(location, $"{sequenceName}{extension}");
                try
                {
                    FileHelper.CreateDirectory(location);
                    XmlOperations.SaveXmlData(path, data);
                }
                catch (Exception ex)
                {
                    MTFMessageBox.Show(LanguageHelper.GetString("Msg_Header_Error"), ex.Message, MTFMessageBoxType.Error, MTFMessageBoxButtons.Ok);
                }
            }
        }
    }
}
