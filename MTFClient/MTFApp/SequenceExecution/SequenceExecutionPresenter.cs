using MTFApp.UIHelpers;
using MTFClientServerCommon;
using AutomotiveLighting.MTFCommon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MTFClientServerCommon.Helpers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using MTFApp.OpenSaveSequencesDialog;
using MTFApp.SequenceExecution.ActivityProgress;
using MTFApp.SequenceExecution.ExtendedMode;
using MTFApp.SequenceExecution.GraphicalViewHandling;
using MTFApp.SequenceExecution.Helpers;
using MTFApp.SequenceExecution.ImageHandling;
using MTFApp.SequenceExecution.MainViews;
using MTFApp.SequenceExecution.SharedControls;
using MTFApp.SequenceExecution.TableHandling;
using MTFApp.UIControls.CommandGrid;
using MTFApp.UIControls.UserCommands;
using MTFApp.UIHelpers.LongTask;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.GoldSamplePersist;
using MTFClientServerCommon.MTFTable;
using MTFClientServerCommon.MTFValidationTable;
using MTFCommon.ClientControls;
using MTFActivityResult = MTFClientServerCommon.MTFActivityResult;

namespace MTFApp.SequenceExecution
{
    class SequenceExecutionPresenter : PresenterBase, IMainCommands, IHeaderText, IAccess, IControlStatus
    {
        private MTFSequence sequence;
        private readonly Command openSequenceDialogCommand;
        private readonly Command startSequenceCommand;
        private readonly Command stopSequenceCommand;
        private readonly Command pauseSequenceCommand;
        private readonly Command toggleSetupModeCommand;
        private readonly Command graphicalViewCommand;
        private readonly Command tableViewCommand;
        private readonly Command treeViewCommand;
        private readonly Command debugCommand;
        private readonly Command serviceCommand;
        private readonly Command stepIntoCommand;
        private readonly Command stepOverCommand;
        private readonly Command stepOutCommand;
        private readonly Command teachingCommand;
        private readonly Command collapseAllCommand;
        private readonly Command expandAllCommand;
        private readonly Command backCommand;
        private readonly Command saveCommand;
        private readonly Command pictureBoxCommand;
        private readonly Command jumpToErrorActivityCommand;
        private readonly Command exportCommand;
        private bool isCollapseAllActivated;
        private bool sequenceIsModified;
        private bool sequenceWasModified;
        private bool firstChangeView = true;
        private bool waitingForTreeResults;
        private bool waitingForTableViewResults;
        private ControlStatus controlStatus;

        private string openedSequence = string.Empty;
        private MTFSequenceExecutionState sequenceState;
        private MTFSequenceActivity executingActivity;
        private List<string> dontLoadedSequence;

        private HashSet<string> sequenceNameList = new HashSet<string>();
        private readonly object progressEventsLock = new object();

        private readonly MessageBoxHandler msgHandler = new MessageBoxHandler();
        private readonly ImageHandler imageHandler;
        private readonly TreeViewManager treeViewManager;
        private readonly SettingsClass settings;
        private readonly GraphicalViewManager graphicalViewManager;
        private readonly TableManager tableManager = new TableManager();

        private ObservableCollection<SequenceVariantInfo> goldSampleVariants = new MTFObservableCollection<SequenceVariantInfo>();

        public SequenceExecutionPresenter()
        {
            imageHandler = new ImageHandler(SwitchImageDetailMode);
            graphicalViewManager = new GraphicalViewManager(MTFClient);
            treeViewManager = new TreeViewManager(new ExtendedModeActions
            {
                SelectItemAction = SetSelectedItem,
                ChangeBreakPointAction = ChangeBreakPoint,
                ChangeSetupPointAction = ChangeSetupPoint,
            });
            RuntimePartVisible = Visibility.Collapsed;
            try
            {
                openSequenceDialogCommand = new Command(openSequenceDialog, () => ExecutionState == MTFSequenceExecutionState.None || ExecutionState == MTFSequenceExecutionState.Stopped || ExecutionState == MTFSequenceExecutionState.Aborted || ExecutionState == MTFSequenceExecutionState.Finished)
                {
                    Name = "MainCommand_Open",
                    Icon = MTFIcons.OpenFile,
                    KeyShortucuts = new[] { new CommandShortcut { Key = Key.O, Modifier = ModifierKeys.Control } }
                };
                startSequenceCommand = new Command(startSequence, () => sequence != null && (ExecutionState == MTFSequenceExecutionState.None || ExecutionState == MTFSequenceExecutionState.Stopped || ExecutionState == MTFSequenceExecutionState.Pause || ExecutionState == MTFSequenceExecutionState.Aborted || ExecutionState == MTFSequenceExecutionState.Finished))
                {
                    Name = "MainCommand_Start",
                    Icon = MTFIcons.StartSequence,
                    KeyShortucuts = new[] { new CommandShortcut { Key = Key.F5, Modifier = ModifierKeys.None } }
                };
                stopSequenceCommand = new Command(stopSequence, () => ExecutionState == MTFSequenceExecutionState.Executing || ExecutionState == MTFSequenceExecutionState.Pause)
                {
                    Name = "MainCommand_Stop",
                    Icon = MTFIcons.StopSequence,
                    KeyShortucuts = new[] { new CommandShortcut { Key = Key.F5, Modifier = ModifierKeys.Shift } }
                };
                pauseSequenceCommand = new Command(pauseSequence, () => ExecutionState == MTFSequenceExecutionState.Executing) { Name = "MainCommand_Pause", Icon = MTFIcons.PauseSequence };
                toggleSetupModeCommand = new ToggleCommand(toggleSetupMode, () => sequence != null && treeViewManager.ExistSetupMode, PropertyInfoHelper.GetPropertyName(() => IsSetupModeActive))
                {
                    Name = "MainCommand_Setup",
                    Icon = MTFIcons.Tune,
                    KeyShortucuts = new[] { new CommandShortcut { Key = Key.S, Modifier = ModifierKeys.Alt } }
                };
                treeViewCommand = new ToggleCommand(ShowTreeView, PropertyInfoHelper.GetPropertyName(() => IsShowTreeActivated)) { Name = "Enum_ExecutionType_Tree", Icon = MTFIcons.TreeView };
                //timeViewCommand = new ToggleCommand(ShowTimeView, () => !IsDebugEnabled, PropertyInfoHelper.GetPropertyName(() => IsShowTimeActivated)) { Name = "Enum_ExecutionType_Time", Icon = MTFIcons.TimeView };
                tableViewCommand = new ToggleCommand(ShowTableView, () => !IsDebugEnabled, PropertyInfoHelper.GetPropertyName(() => IsShowTableActivated)) { Name = "Enum_ExecutionType_Table", Icon = MTFIcons.TableView };
                graphicalViewCommand = new ToggleCommand(ShowGraphicalView, () => !IsDebugEnabled, PropertyInfoHelper.GetPropertyName(() => IsShowGraphicalViewActivated)) { Name = "Enum_ExecutionType_Graphical", Icon = MTFIcons.GraphicalView };
                debugCommand = new ToggleCommand(debugToggle, () => sequence != null && IsShowTreeActivated, PropertyInfoHelper.GetPropertyName(() => IsDebugEnabled))
                {
                    Name = "MainCommand_Debug",
                    Icon = MTFIcons.Bug,
                    KeyShortucuts = new[] { new CommandShortcut { Key = Key.D, Modifier = ModifierKeys.Alt } }
                };
                collapseAllCommand = new Command(() => CollapseAllTables(true)) { Name = "MainCommand_CollapseAll", Icon = MTFIcons.CollapseAll };
                expandAllCommand = new Command(() => CollapseAllTables(false)) { Name = "MainCommand_ExpandAll", Icon = MTFIcons.ExpandAll };
                serviceCommand = new ToggleCommand(ShowServceView, () => sequence != null && !IsServiceActivated && !IsDebugEnabled && !IsTeachingActivated && sequence.ServiceCommands != null && sequence.ServiceCommands.Count > 0, PropertyInfoHelper.GetPropertyName(() => IsServiceActivated)) { Name = "Enum_ExecutionType_Service", Icon = MTFIcons.Service };
                teachingCommand = new ToggleCommand(ShowTeachingView, () => sequence != null && !IsTeachingActivated && !IsDebugEnabled && !IsServiceActivated && sequence.ServiceCommands != null && sequence.ServiceCommands.Count > 0, PropertyInfoHelper.GetPropertyName(() => IsTeachingActivated)) { Name = "Enum_ExecutionType_Teach", Icon = MTFIcons.Teaching };
                backCommand = new Command(Back, () => IsTeachingActivated || IsServiceActivated) { Name = "MainCommand_Back", Icon = MTFIcons.Back };
                saveCommand = new Command(SaveSequence, () => sequenceIsModified) { Name = "MainCommand_Save", Icon = MTFIcons.SaveFile };
                stepIntoCommand = new Command(() => { MTFClient.DebugStepInto(); }, () => ExecutionState == MTFSequenceExecutionState.Pause)
                {
                    Name = "MainCommand_Debud_StepInto",
                    Icon = MTFIcons.StepInto,
                    KeyShortucuts = new[] { new CommandShortcut { Key = Key.F11, Modifier = ModifierKeys.None } }
                };
                stepOverCommand = new Command(() => { MTFClient.DebugStepOver(); }, () => ExecutionState == MTFSequenceExecutionState.Pause)
                {
                    Name = "MainCommand_Debud_StepOver",
                    Icon = MTFIcons.StepOver,
                    KeyShortucuts = new[] { new CommandShortcut { Key = Key.F10, Modifier = ModifierKeys.None } }
                };
                stepOutCommand = new Command(() => { MTFClient.DebugStepOut(); }, () => ExecutionState == MTFSequenceExecutionState.Pause)
                {
                    Name = "MainCommand_Debud_StepOut",
                    Icon = MTFIcons.StepOut,
                    KeyShortucuts = new[] { new CommandShortcut { Key = Key.F11, Modifier = ModifierKeys.Shift } }
                };
                pictureBoxCommand = new ToggleCommand(showPictureBox, PropertyInfoHelper.GetPropertyName(() => IsShowPicture)) { Name = "MainCommand_PictureBox", Icon = MTFIcons.Photo };

                jumpToErrorActivityCommand = new Command(JumpToErrorActivity, AllowJumpToActivity);

                exportCommand = new Command(ExportSequence,
                                    () => sequenceState == MTFSequenceExecutionState.Finished ||
                                          sequenceState == MTFSequenceExecutionState.Stopped ||
                                          sequenceState == MTFSequenceExecutionState.Aborted)
                                {
                                    Name = "MainCommand_Export",
                                    Icon = MTFIcons.Export
                                };

                settings = StoreSettings.GetInstance.SettingsClass;
                settings.OnLanguageChanged += LanguageChanged;
                ActivityProgress = new ObservableCollection<ActivityProgressBase>();
                ErrorMessages = new ObservableCollection<StatusMessage>();

                GetSequenceExecutingState();

                MTFClient.SequenceExecutionError += MTFClient_SequenceExecutionError;
                MTFClient.SequenceExecutionActivityChanged += MTFClient_SequenceExecutionActivityChanged;
                MTFClient.SequenceExecutionStateChanged += MTFClient_SequenceExecutionStateChanged;
                MTFClient.SequenceExecutionActivityPercentProgress += MTFClient_SequenceExecutionActivityPercentProgress;
                MTFClient.SequenceExecutionActivityStringProgress += MTFClient_SequenceExecutionActivityStringProgress;
                MTFClient.SequenceExecutionActivityImageProgress += MTFClient_SequenceExecutionActivityImageProgress;
                MTFClient.SequenceExecutionNewActivityResult += MTFClient_SequenceExecutionNewActivityResult;
                MTFClient.SequenceExecutionTreeResults += MTFClient_SequenceExecutionTreeResults;
                MTFClient.SequenceExecutionTableViewResults += MTFClient_SequenceExecutionTableViewResults;
                MTFClient.SequenceExecutionNewValidateRows += MTFClient_SequenceExecutionNewValidateRows;
                MTFClient.SequenceExecutionRepeatSubSequence += MTFClient_SequenceExecutionRepeatSubSequence;
                MTFClient.SequenceExecutionOnSequenceStatusMessage += MTFClient_SequenceExecutionOnSequenceStatusMessage;
                MTFClient.SequenceExecutionShowMessage += MTFClient_SequenceExecutionShowMessage;
                MTFClient.SequenceExecutionFinished += MTFClient_SequenceExecutionFinished;
                MTFClient.SequenceExecutionCloseMessage += MTFClient_SequenceExecutionCloseMessage;
                MTFClient.SequenceExecutionOnClearValidationTables += MTFClient_SequenceExecutionOnClearValidationTables;
                MTFClient.SequenceExecutionShowSetupVariantSelection += MTFClient_SequenceExecutionShowSetupVariantSelection;
                MTFClient.SequenceExecutionSequenceVariantChanged += MTFClient_SequenceExecutionSequenceVariantChanged;
                MTFClient.ServiceCommandsStateChanged += MtfClientServiceCommandsStateChanged;
                MTFClient.SequenceExecutionLoadGoldSamples += MTFClient_SequenceExecutionLoadGoldSamples;
                MTFClient.SequenceExecutionAllowSaveExecutedSequence += MTFClient_SequenceExecutionAllowSaveExecutedSequence;
                MTFClient.SequenceExecutionOnUIControlReceiveData += MTFClientOnSequenceExecutionOnUiControlReceiveData;
                MTFClient.SequenceExecutionDynamicUnloadSequence += MTFClient_SequenceExecutionDynamicUnloadSequence;
                MTFClient.SequenceExecutionDynamicLoadSequence += MTFClient_SequenceExecutionDynamicLoadSequence;
                MTFClient.SequenceExecutionDynamicExecuteSequence += MTFClient_SequenceExecutionDynamicExecuteSequence;
                MTFClient.SequenceExecutionOpenClientSetupControl += MTFClientOnSequenceExecutionOpenClientSetupControl;
                MTFClient.OnUserCommandsStatusChanged += MTFClientOnOnUserCommandsStatusChanged;
                MTFClient.OnUserIndicatorValueChanged += MTFClientOnUserIndicatorValueChanged;
                MTFClient.OnViewChanged += MtfClientOnOnViewChanged;


                //test if is some sequence running on server and load it
                OpenExecutingSequenceAsync();
                detailMode = ExecutionDetailModes.Table;

                switch (settings.SequenceExecutionViewType)
                {
                    case SequenceExecutionViewType.TableView:
                        ViewMode = ExecutionViewMode.Table;
                        break;
                    case SequenceExecutionViewType.TimeView:
                        ViewMode = ExecutionViewMode.Table;
                        break;
                    case SequenceExecutionViewType.TreeView:
                        ViewMode = ExecutionViewMode.Tree;
                        break;
                    case SequenceExecutionViewType.GraphicalView:
                        ViewMode = ExecutionViewMode.GraphicalView;
                        break;
                }
                previousViewMode = ViewMode;


                //OpenSequenceManualy();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

        }

        private void MTFClientOnUserIndicatorValueChanged(Guid indicatorId, bool value)
        {
            var commandWrapper = userCommands.FirstOrDefault(c => c.Command.Id == indicatorId);
            if (commandWrapper == null)
            {
                return;
            }

            commandWrapper.IndicatorValue = value;
        }

        private void MTFClientOnOnUserCommandsStatusChanged(IEnumerable<UserCommandsState> commandsSettings)
        {
            sequenceUserCommandsStates = commandsSettings;
            foreach (var command in sequenceUserCommandsStates)
            {
                var userCommandId = ((MTFIdentityObject)command.InternalProperties["UserCommand"]).Id;
                command.UserCommand = userCommands.FirstOrDefault(c => c.Command.Id == userCommandId)?.Command;
            }

            RefreshUserCommandsAccessibility();
        }

        private void showPictureBox()
        {
            IsShowPicture = !IsShowPicture;
            SwitchImageDetailMode(IsShowPicture);
        }

        private void LanguageChanged(object sender, LanguageChangeEventArgs e)
        {
            NotifyPropertyChanged("HeaderText");
            ClientUiHelper.OnLanguageChanged(e.Language);
        }

        private Command clientSetupCommand = null;
        private void MTFClientOnSequenceExecutionOpenClientSetupControl(OpenClientSetupControlArgs args)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    var setupUiControl =
                        ClientUiHelper.GetControl(args.AssemblyName, args.TypeName) as MTFClientSetupControl;
                    var attr = setupUiControl.GetType().GetCustomAttribute<MTFClientControlSetupAttribute>();

                    clientSetupCommand = new ToggleCommand(() =>
                                                           {
                                                               CurrentUiControl = setupUiControl;
                                                               ViewMode = ExecutionViewMode.ClientUI;
                                                           }, "")
                    {
                        Icon = attr?.Icon ?? MTFIcons.AL,
                        Name = attr?.Name
                    };
                    var oldControl = CurrentUiControl;
                    var oldViewMode = ViewMode;

                    setupUiControl.OnClose += sender =>
                    {
                        CurrentUiControl = oldControl;
                        ViewMode = oldViewMode;
                        ClientUiHelper.RemoveFromCache(args.AssemblyName, args.TypeName);
                        MTFClient.SetupControlClosed(args);
                        clientSetupCommand = null;
                        RefreshCommands();
                        RefreshViewModes();
                    };

                    CurrentUiControl = setupUiControl;
                    if (ViewMode != ExecutionViewMode.ClientUI)
                    {
                        ViewMode = ExecutionViewMode.ClientUI;
                    }

                    RefreshCommands();
                    RefreshViewModes();
                }
                catch (Exception e)
                {
                    ErrorMessages.Add(new StatusMessage { Type = StatusMessage.MessageType.Error, TimeStamp = DateTime.Now, Text = $"Client UI failed during opening control with error: {e.Message}" });
                    MTFClient.SetupControlClosed(args);
                }

                //ClientUiHelper.RemoveFromCache(args.AssemblyName, args.TypeName);
                //MTFClient.SetupControlClosed(args);
            });
        }

        private async void OpenSequenceManualy()
        {
            await Task.Run(() => Thread.Sleep(500));
            // openSequence("BugFixing\\fexa\\RAM_DT\\New\\RamDt_Main.sequence");
            //openSequence("Stritez\\Test4 - paralel.sequence");
            //openSequence("00.sequence");
        }

        public void SaveSequence()
        {
            var userName = EnvironmentHelper.UserName;
            if (CheckIfPossibleToSave() || MTFMessageBox.Show(LanguageHelper.GetString("Msg_Header_SaveSequence"),
                string.Format("{0}\n\n{1}", LanguageHelper.GetString("Msg_Body_EditorModified"), LanguageHelper.GetString("Msg_Body_SaveSequence")),
                    MTFMessageBoxType.ImportantQuestion, MTFMessageBoxButtons.YesNo) == MTFMessageBoxResult.Yes)
            {
                sequenceWasModified = true;
                AllowSave(false);
                sequence.IncreaseRevision();
                NotifyPropertyChanged("HeaderText");

                MTFClient.SaveExecutedSequence(userName);
                sendEmailAfterModification();

                ReloadSequenceInEditor();
            }
        }

        private void ExportSequence()
        {
            try
            {
                var dialog = new SaveFileDialog { Filter = "Binary file (*.bin)|*.bin" };
                if (dialog.ShowDialog() == true)
                {
                    LongTask.Do(() => { this.SaveActivityResultsToFile(MTFClient.GetMTFActivityResult(), dialog.FileName); }, "Exporting ...");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Export sequence failed. Internal error: " + ex.Message);
            }
        }

        private void SaveActivityResultsToFile(List<MTFActivityResult> listResults, string fileName)
        {
            var listActivityResults = new List<MTFActivityResultWrapper>();
            foreach (var result in listResults)
            {
                listActivityResults.Add(new MTFActivityResultWrapper
                {
                    ActivityId = result.ActivityId,
                    ActivityIndexer = result.ActivityIndexer,
                    ActivityIdPath = result.ActivityIdPath,
                    ActivityResult = result.ActivityResult,
                    ActivityResultTypeName = result.ActivityResultTypeName,
                    ElapsedMs = result.ElapsedMs,
                    ActivityName = result.ActivityName,
                    Status = result.Status,
                    NumberOfRepetition = result.NumberOfRepetition,
                    ExceptionMessage = result.ExceptionMessage,
                    TimestampMs = result.TimestampMs,
                    MTFParameters = result.MTFParameters
                });

                if (result is MTFVariableActivityResult variableResult)
                {
                    var lastActivityResult = listActivityResults.Last();
                    lastActivityResult.VariableName = variableResult.VariableName;
                    lastActivityResult.VariableValue = variableResult.Value;
                    lastActivityResult.VariableTypeName = variableResult.VariableTypeName;
                }
            }

            var formatter = new BinaryFormatter();
            using (var writer = new FileStream(fileName, FileMode.OpenOrCreate))
            {
                formatter.Serialize(writer, listActivityResults);
                writer.Close();
            }
        }

        private void sendEmailAfterModification()
        {
            if (!sequence.SendEmailAfterChanged)
            {
                return;
            }

            List<MTFSequence> modifiedSequences = new List<MTFSequence>();
            modifiedSequences.Add(sequence);
            modifiedSequences.AddRange(sequence.ExternalSubSequences.Select(s => s.ExternalSequence));

            Task.Run(() =>
                EmailHelper.SendEmail(sequence.SmtpServer, sequence.MailTo, sequence.EmailSubject, EmailHelper.GenerateSequenceChangedEmail(modifiedSequences)));
        }

        private bool CheckIfPossibleToSave()
        {
            var editorPresenter = GetEditorPresenter();
            return editorPresenter == null || editorPresenter.OpenedSequenceFileName != openedSequence || !editorPresenter.SequenceWasModified;
        }

        private bool CheckIfPossibleToExecuted()
        {
            var editorPresenter = GetEditorPresenter();
            return editorPresenter == null || editorPresenter.OpenedSequenceFileName == null || editorPresenter.OpenedSequenceFileName != openedSequence || !editorPresenter.IsModified;
        }

        private void AllowSave(bool state)
        {
            sequenceIsModified = state;
            saveCommand.RaiseCanExecuteChanged();
        }

        public bool SequenceWasModified
        {
            get { return sequenceIsModified || sequenceWasModified; }
        }

        public bool SequenceIsModified
        {
            get { return sequenceIsModified; }
        }

        public void NotifySequenceIsActual()
        {
            sequenceWasModified = false;
        }

        private void CollapseAllTables(bool collapse)
        {
            tableManager.CollapseAllTables(collapse, GetExecutionStatus());
            RefreshCommands();
        }


        public ICommand StartSequenceCommand
        {
            get { return startSequenceCommand; }
        }

        public ICommand StepIntoCommand
        {
            get { return stepIntoCommand; }
        }

        public ICommand StepOverCommand
        {
            get { return stepOverCommand; }
        }

        public ICommand StepOutCommand
        {
            get { return stepOutCommand; }
        }

        private void InitUi()
        {
            MessagesCount = 0;
            NotifyPropertyChanged("HasError");
            ErrorMessages.Clear();
            ShowError = false;
            ClearStatus();
            imageHandler.Reset();
            SequenceResult = null;
            NotifyPropertyChanged("SequenceResult");
            SelectedItem = null;
            tableManager.ClearRowsInValidationTables();
            ActivityProgress.Clear();
            //NotifyPropertyChanged("ActivityProgress");
            ClearSequenceVariants();
            ServiceTables.Clear();
            GoldSampleVariants.Clear();
            ControlStatus = ControlStatus.None;
            msgHandler.Init();
        }

        private void debugToggle()
        {
            treeViewManager.SaveExtendedModeData(Sequence.Name, Sequence.Id, ExtendedModeTypes.Debug);

            IsDebugEnabled = !IsDebugEnabled;

            treeViewManager.SwitchExtendedMode(IsDebugEnabled, ExtendedModeTypes.Debug);

            if (ExecutionState == MTFSequenceExecutionState.Executing || ExecutionState == MTFSequenceExecutionState.ExecutionPreparation || ExecutionState == MTFSequenceExecutionState.Pause)
            {
                MTFClient.SetIsDebugMode(IsDebugEnabled);
            }
            if (ExecutionState == MTFSequenceExecutionState.Pause)
            {
                ChangeTableEditMode(IsDebugEnabled);
            }
            RefreshCommands();
        }

        private void ChangeTableEditMode(bool editable)
        {
            foreach (var executionValidTable in tableManager.ValidationTables(null))
            {
                executionValidTable.IsEditable = editable && Sequence.ServiceDesignSetting.AllowEditTables;
            }
        }

        private bool isDebugEnabled = false;
        public bool IsDebugEnabled
        {
            get { return isDebugEnabled; }
            set
            {
                isDebugEnabled = value;
                treeViewManager.ExecutionPointer.IsActive = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("IsPausedInDebug");
                if (value)
                {
                    RegisterAllSequenceExecutionEventListener();
                    currentMarkedItems.MarkItems();
                }
                else
                {
                    RemoveAllSequenceExecutionEventListener();
                    ChangeExecutionEventsListeners(viewMode);
                    currentMarkedItems.UnmarkItems();
                }
                //timeViewCommand.RaiseCanExecuteChanged();
                tableViewCommand.RaiseCanExecuteChanged();
            }
        }

        private Command setupModeOnActivityChangedCommand;
        public Command SetupModeOnActivityChangedCommand
        {
            get
            {
                if (setupModeOnActivityChangedCommand == null)
                {
                    setupModeOnActivityChangedCommand = new Command(SetupModeOnActivityChanged);
                }

                return setupModeOnActivityChangedCommand;
            }
        }

        private void SetupModeOnActivityChanged(object param)
        {
            var visualisationWrapper = param as MTFActivityVisualisationWrapper;
            if (visualisationWrapper == null)
            {
                return;
            }
            treeViewManager.ChangePointState(visualisationWrapper, ExtendedMode.ExtendedModeTypes.Setup);
            ChangeSetupPoint(visualisationWrapper);
        }

        private void ChangeSetupPoint(MTFActivityVisualisationWrapper activityWrapper)
        {
            if (ExecutionState == MTFSequenceExecutionState.Executing || ExecutionState == MTFSequenceExecutionState.ExecutionPreparation ||
                ExecutionState == MTFSequenceExecutionState.Pause)
            {
                if (activityWrapper.IsSetupMode == StateDebugSetup.Active)
                {
                    MTFClient.AddSetupActivityPath(activityWrapper.GuidPath);
                }
                else
                {
                    MTFClient.RemoveSetupActivityPath(activityWrapper.GuidPath);
                }
            }
        }

        private Command breakpintOnActivityChangedCommand;
        public Command BreakPointOnActivityChangedCommand
        {
            get { return breakpintOnActivityChangedCommand ?? (breakpintOnActivityChangedCommand = new Command(BreakPointOnActivityChanged)); }
        }

        private void BreakPointOnActivityChanged(object param)
        {
            var visualisationWrapper = param as MTFActivityVisualisationWrapper;
            if (visualisationWrapper == null)
            {
                return;
            }
            treeViewManager.ChangePointState(visualisationWrapper, ExtendedModeTypes.Debug);
            ChangeBreakPoint(visualisationWrapper);
        }

        private void ChangeBreakPoint(MTFActivityVisualisationWrapper activityWrapper)
        {
            if (ExecutionState == MTFSequenceExecutionState.Executing || ExecutionState == MTFSequenceExecutionState.ExecutionPreparation ||
                ExecutionState == MTFSequenceExecutionState.Pause)
            {
                if (activityWrapper.IsBreakPoint == StateDebugSetup.Active)
                {
                    MTFClient.AddBreakPointActivityPath(activityWrapper.GuidPath);
                }
                else
                {
                    MTFClient.RemoveBreakPointActivityPath(activityWrapper.GuidPath);
                }
            }
        }

        public void SetExecutionPosition(MTFActivityVisualisationWrapper activity)
        {
            treeViewManager.ExecutionPointer.Change(activity);
            MTFClient.SetNewExecutionPointer(activity.GuidPath.ToArray());
        }

        void MTFClient_SequenceExecutionAllowSaveExecutedSequence(bool state)
        {
            Application.Current.Dispatcher.Invoke(() => AllowSave(state));
        }


        void MTFClient_SequenceExecutionLoadGoldSamples(List<SequenceVariantInfo> goldSampleList)
        {
            if (goldSampleList != null)
            {
                GoldSampleVariants = new ObservableCollection<SequenceVariantInfo>(goldSampleList);
            }
        }

        private void MTFClient_SequenceExecutionOnClearValidationTables(bool clearAllTables, List<Guid> tablesForClearing, Guid? dutId)
        {
            tableManager.ClearTables(clearAllTables, tablesForClearing, dutId);
        }

        private SequenceVariant lastSaveVariant;
        private void MTFClient_SequenceExecutionShowSetupVariantSelection(string activityName, Dictionary<string, IEnumerable<SequenceVariant>> dataVariants, Dictionary<string, string> extendetUsedDataNames)
        {
            SetupVariantControl setupVariantControl = null;
            MTFDialogResult setupVariantWindowResult = null;
            Application.Current.Dispatcher.Invoke(() =>
            {
                setupVariantControl = new SetupVariantControl
                {
                    ActivityName = activityName,
                    Sequence = sequence,
                    DataVariants = dataVariants.Select(d => new DataVariantWrapper
                    {
                        DataName = d.Key,
                        DisplayDataName = extendetUsedDataNames != null && extendetUsedDataNames.ContainsKey(d.Key) ? string.Format("{0} - {1}", d.Key, extendetUsedDataNames[d.Key]) : d.Key,
                        UsedDataVariants = d.Value,
                        //SaveVariant = lastSaveVariant == null ? null : lastSaveVariant.Copy() as SequenceVariant,
                        SelectedUsedVariant = lastSaveVariant != null && d.Value != null && d.Value.Any(v => v.Match(lastSaveVariant)) ? lastSaveVariant.Copy() as SequenceVariant : null,
                    }).ToList(),
                };

                PopupWindow.PopupWindow window = new PopupWindow.PopupWindow(setupVariantControl, true) { CanClose = false };
                window.ShowDialog();
                setupVariantWindowResult = window.MTFDialogResult;
            });

            if (setupVariantWindowResult != null && setupVariantWindowResult.Result == MTFDialogResultEnum.Ok)
            {
                var res = setupVariantControl.DataVariants.Select(d =>
                    new SetupVariantSelectionResult
                    {
                        DataName = d.DataName,
                        SaveVariant = d.SaveVariant,
                        UseVariant = d.SelectedUsedVariant
                    }).ToArray();

                lastSaveVariant = res.Last().SaveVariant;
                MTFClient.SetupVariantSelectionResult(res);
            }
            else
            {
                MTFClient.SetupVariantSelectionResult(null);
            }
        }

        private void GetSequenceExecutingState()
        {
            Task.Run(() => sequenceState = MTFClient.GetSequenceExecutingState());
        }

        private async void OpenExecutingSequenceAsync()
        {
            await Task.Run(() => openedSequence = MTFClient.GetExecutingSequenceName());

            if (!CheckIfPossibleToExecuted())
            {
                return;
            }

            if (!string.IsNullOrEmpty(openedSequence))
            {
                openSequence(openedSequence);
                await Task.Run(() => MTFClient_SequenceExecutionActivityChanged(MTFClient.GetExecutingActivityPath()));
            }

            else
            {
                ServerSettings serverSettings = null;
                await Task.Run(() => serverSettings = MTFClient.GetServerSettings());
                if (serverSettings != null && serverSettings.StartSequenceOnServerStart)
                {
                    var defaultSequenceName = serverSettings.SequenceName;
                    openedSequence = defaultSequenceName;
                    tableManager.ClearExistingTables();
                    treeViewManager.Clear();
                    SequenceVariables = new List<VariablesWatch>();
                    sequenceNameList.Clear();
                    Sequence = LoadSequence(defaultSequenceName, sequenceNameList);

                    if (dontLoadedSequence != null)
                    {
                        NotifyDoNotLoadedSequences();
                        return;
                    }

                    InitUi();
                    RefreshCommands();
                    tableManager.Init(Sequence, settings.IsTableCollapsed);
                    graphicalViewManager.Init(Sequence, tableManager);
                    InitDuts();
                    initializeClientContols();
                    OpenFirstActivClientUI();
                    AllowSave(false);
                    InitDebug();
                    List<string> emptyCallActivites = new List<string>();

                    try
                    {
                        MTFSequenceHelper.TransformActivitiesToExecution(Sequence, Sequence.MTFSequenceActivities,
                                        x => treeViewManager.AddToSource(x), true, 0, new List<Guid>() { sequence.Id }, emptyCallActivites);
                        treeViewManager.FillTree();
                    }
                    catch (Exception ex)
                    {
                        Sequence = null;
                        MTFMessageBox.Show(LanguageHelper.GetString("Msg_Header_Error"), ex.Message, MTFMessageBoxType.Error, MTFMessageBoxButtons.Ok);
                        return;
                    }

                    FillUserCommands();
                    InitExtendedMode();

                    if (emptyCallActivites.Count > 0)
                    {
                        ShowEmptyExecuteSubSequences(emptyCallActivites);
                    }
                    NotifyPropertyChanged("ExecutionCollection");
                    NotifyPropertyChanged("HeaderText");
                    toggleSetupModeCommand.RaiseCanExecuteChanged();

                    if (settings.StartSequenceAfterStartDelay > 0)
                    {
                        LongTask.Do(
                            () =>
                                Thread.Sleep(new TimeSpan(0, 0, settings.StartSequenceAfterStartDelay)),
                                string.Format(LanguageHelper.GetString("Execution_StartDelay"),
                                settings.StartSequenceAfterStartDelay));
                    }
                    if (settings.SequenceExecutionViewType == SequenceExecutionViewType.Service &&
                        (sequence != null && sequence.ServiceCommands != null && sequence.ServiceCommands.Count > 0))
                    {
                        ShowServceView();
                    }
                    else
                    {
                        startSequence();
                    }
                }
                else if (Application.Current.MainWindow.DataContext != null)
                {
                    var sequenceEditorPresenter = GetEditorPresenter();
                    if (sequenceEditorPresenter != null &&
                        !string.IsNullOrEmpty(sequenceEditorPresenter.OpenedSequenceFileName))
                    {
                        openedSequence = sequenceEditorPresenter.OpenedSequenceFileName;
                        if (CheckIfPossibleToExecuted())
                        {
                            await Task.Run(() => openSequence(sequenceEditorPresenter.OpenedSequenceFileName, false));
                            sequenceEditorPresenter.NotifySequenceIsActual();
                        }

                    }
                }

                if (ExecutionState != MTFSequenceExecutionState.None)
                {
                    if (viewMode == ExecutionViewMode.Tree)
                    {
                        MTFClient.ResultReuqest(ResultRequestTypes.TreeResults);
                    }
                }
            }

        }

        private void ShowTableView()
        {
            if (ViewMode != ExecutionViewMode.Table)
            {
                ViewMode = ExecutionViewMode.Table;
            }

            RefreshViewModes();
        }

        public bool IsShowTableActivated
        {
            get
            {
                return ViewMode == ExecutionViewMode.Table;
            }
        }

        private void ShowGraphicalView()
        {
            if (ViewMode != ExecutionViewMode.GraphicalView)
            {
                ViewMode = ExecutionViewMode.GraphicalView;
            }

            RefreshViewModes();
        }

        public bool IsShowGraphicalViewActivated
        {
            get
            {
                return ViewMode == ExecutionViewMode.GraphicalView;
            }
        }

        private void ShowTreeView()
        {
            if (ViewMode != ExecutionViewMode.Tree)
            {
                ViewMode = ExecutionViewMode.Tree;
            }
            UIHelper.InvokeOnDispatcher(debugCommand.RaiseCanExecuteChanged);
            RefreshViewModes();
        }

        public bool IsShowTreeActivated
        {
            get
            {
                return ViewMode == ExecutionViewMode.Tree;
            }
        }

        private void ShowServceView()
        {
            if (!CheckIfPossibleToExecuted())
            {
                ShowStartErrorMessage();
                return;
            }
            if (!isService || ViewMode != ExecutionViewMode.Service)
            {
                ViewMode = ExecutionViewMode.Service;
                isService = true;
                MTFClient.SetIsServiceMode(true);
                if (ExecutionState == MTFSequenceExecutionState.None ||
                    ExecutionState == MTFSequenceExecutionState.Aborted ||
                    ExecutionState == MTFSequenceExecutionState.Pause ||
                    ExecutionState == MTFSequenceExecutionState.Finished ||
                    ExecutionState == MTFSequenceExecutionState.Stopped)
                {
                    startSequence();
                }
                AllowedServicecommands = null;
                GenerateServiceCommands();

                RefreshViewModes();
            }
        }

        private void ShowTeachingView()
        {
            if (!CheckIfPossibleToExecuted())
            {
                ShowStartErrorMessage();
                return;
            }
            if (!isTeaching || ViewMode != ExecutionViewMode.Service)
            {
                ViewMode = ExecutionViewMode.Service;
                isTeaching = true;
                MTFClient.SetIsTeachingMode(true);
                if (ExecutionState == MTFSequenceExecutionState.None ||
                    ExecutionState == MTFSequenceExecutionState.Aborted ||
                    ExecutionState == MTFSequenceExecutionState.Pause ||
                    ExecutionState == MTFSequenceExecutionState.Finished ||
                    ExecutionState == MTFSequenceExecutionState.Stopped)
                {
                    startSequence();
                }
                AllowedServicecommands = null;
                GenerateServiceCommands();
                RefreshViewModes();
            }
        }

        private void ShowStartErrorMessage()
        {
            MTFMessageBox.Show(LanguageHelper.GetString("Msg_Header_Error"), LanguageHelper.GetString("Msg_Body_SequenceNotStartModifiedEditor"),
                    MTFMessageBoxType.Info, MTFMessageBoxButtons.Ok);
        }

        private bool isService;
        public bool IsServiceActivated
        {
            get
            {
                return isService && ViewMode == ExecutionViewMode.Service;
            }
        }

        private bool isTeaching;
        public bool IsTeachingActivated
        {
            get
            {
                return isTeaching && ViewMode == ExecutionViewMode.Service;
            }
        }

        public bool IsShowPicture { get; private set; }

        private void Back()
        {
            switch (previousViewMode)
            {
                case ExecutionViewMode.Tree:
                    ShowTreeView();
                    break;
                case ExecutionViewMode.Table:
                    ShowTableView();
                    break;
                case ExecutionViewMode.Time:
                    //ShowTimeView();
                    ShowTableView();
                    break;
                case ExecutionViewMode.GraphicalView:
                    ShowGraphicalView();
                    break;
            }
        }

        public IEnumerable<ICommand> ExecuteServiceCommandCommands
        {
            get { return null; }
        }

        private void executeServiceCommand(object param)
        {
            MTFServiceCommand serviceCommand = param as MTFServiceCommand;
            if (serviceCommand == null)
            {
                return;
            }

            AllowedServicecommands = null;
            MTFClient.ExecuteServiceCommand(serviceCommand.Id);
        }

        private ExecutionViewMode previousViewMode;
        private bool askChangeFromService;

        private ExecutionViewMode viewMode;
        public ExecutionViewMode ViewMode
        {
            get { return viewMode; }
            set
            {
                if (DetailMode == ExecutionDetailModes.Image)
                {
                    DetailMode = ExecutionDetailModes.Table;
                }
                if (viewMode == ExecutionViewMode.Service)
                {
                    if (askChangeFromService && MTFMessageBox.Show(LanguageHelper.GetString("Msg_Header_LeaveService"),
                        isService
                            ? LanguageHelper.GetString("Msg_Body_LeaveService")
                            : LanguageHelper.GetString("Msg_Body_LeaveTeach"), MTFMessageBoxType.Question,
                        MTFMessageBoxButtons.YesNo) != MTFMessageBoxResult.Yes)
                    {
                        return;
                    }
                    if (isService)
                    {
                        MTFClient.SetIsServiceMode(false);
                        isService = false;
                    }
                    if (isTeaching)
                    {
                        MTFClient.SetIsTeachingMode(false);
                        isTeaching = false;
                    }
                    AllowedServicecommands = null;
                }
                previousViewMode = viewMode;

                Task.Run(() =>
                {
                    if (!IsDebugEnabled)
                    {
                        ChangeExecutionEventsListeners(previousViewMode, value);
                    }

                });

                viewMode = value;
                switch (viewMode)
                {
                    case ExecutionViewMode.Service:
                        askChangeFromService = true;
                        break;
                    case ExecutionViewMode.Tree:
                        TreeResultsRequest();
                        break;
                    case ExecutionViewMode.Table:
                    case ExecutionViewMode.GraphicalView:
                        TableViewResultsRequest();
                        break;
                }
                NotifyPropertyChanged();
            }
        }

        private void TreeResultsRequest()
        {
            if (ExecutionState == MTFSequenceExecutionState.None || waitingForTreeResults)
                return;

            waitingForTreeResults = true;
            MTFClient.ResultReuqest(ResultRequestTypes.TreeResults);
        }

        private void TableViewResultsRequest()
        {
            if (ExecutionState == MTFSequenceExecutionState.None || waitingForTableViewResults)
                return;
         
            waitingForTableViewResults = true;
            MTFClient.ResultReuqest(ResultRequestTypes.TableResults);
        }

        private void ChangeExecutionEventsListeners(ExecutionViewMode oldMode, ExecutionViewMode newMode)
        {
            var eventToRemove = oldMode == ExecutionViewMode.Tree || oldMode == ExecutionViewMode.ClientUI
                ? SequenceExecutionEventType.Tree
                : SequenceExecutionEventType.Table;

            var eventToAdd = newMode == ExecutionViewMode.Tree || newMode == ExecutionViewMode.ClientUI
                ? SequenceExecutionEventType.Tree
                : SequenceExecutionEventType.Table;

            if (eventToAdd != eventToRemove || firstChangeView)
            {
                MTFClient.ChangeExecutionEventListener(new[] { eventToAdd }, new[] { eventToRemove });
            }
            if (firstChangeView)
            {
                firstChangeView = false;
            }
        }

        private void ChangeExecutionEventsListeners(ExecutionViewMode newMode)
        {
            var eventToAdd = newMode == ExecutionViewMode.Tree || newMode == ExecutionViewMode.ClientUI
                ? SequenceExecutionEventType.Tree
                : SequenceExecutionEventType.Table;

            MTFClient.ChangeExecutionEventListener(new[] { eventToAdd }, null);
        }

        private void RegisterAllSequenceExecutionEventListener()
        {
            MTFClient.ChangeExecutionEventListener(new[] { SequenceExecutionEventType.Tree, SequenceExecutionEventType.Table, }, null);
        }

        private void RemoveAllSequenceExecutionEventListener()
        {
            MTFClient.ChangeExecutionEventListener(null, new[] { SequenceExecutionEventType.Tree, SequenceExecutionEventType.Table, });
        }

        public ICommand SetTableDetailCommand
        {
            get { return new Command(() => DetailMode = ExecutionDetailModes.Table); }
        }

        private void SwitchImageDetailMode(bool switchOn)
        {
            DetailMode = switchOn ? (DetailMode == ExecutionDetailModes.Image ? ExecutionDetailModes.Table : ExecutionDetailModes.Image) : ExecutionDetailModes.Table;
            if (!switchOn)
            {
                IsShowPicture = false;
                NotifyPropertyChanged("IsShowPicture");
            }
        }

        public ICommand SetTableImageDetailCommand
        {
            get { return new Command(SetTableImageDetail); }
        }

        private void SetTableImageDetail(object param)
        {
            var mtfImage = param as MTFImage;
            if (mtfImage != null)
            {
                imageHandler.ShowTableImages(new List<byte[]> { mtfImage.ImageData });
            }
        }


        private ExecutionDetailModes detailMode;

        public ExecutionDetailModes DetailMode
        {
            get { return detailMode; }
            set
            {
                detailMode = value;
                NotifyPropertyChanged();
            }
        }

        private List<Guid> warningMessages = new List<Guid>();
        private MTFSequence LoadSequence(string sequenceName, HashSet<string> sequenceNames)
        {
            if (!sequenceNames.Add(sequenceName))
            {
                MTFMessageBox.Show(LanguageHelper.GetString("Msg_Header_Error"),
                    string.Format(LanguageHelper.GetString("Msg_Body_RecursiveOpen"), sequenceName),
                    MTFMessageBoxType.Error, MTFMessageBoxButtons.Ok);
                return null;
            }
            foreach (var m in warningMessages)
            {
                Warning.Remove(m);
            }
            warningMessages.Clear();

            MTFSequence sequence = null;

            try
            {
                sequence = MTFClient.LoadSequence(sequenceName);
            }
            catch (Exception ex)
            {
                MTFMessageBox.Show(LanguageHelper.GetString("Msg_Header_Error"), ex.Message, MTFMessageBoxType.Error,
                    MTFMessageBoxButtons.Ok);
            }

            if (sequence == null)
            {
                return null;
            }

            if (sequence.MTFVersion != null)
            {
                var currentVersion = new MTFClientServerCommon.Version(Assembly.GetExecutingAssembly().GetName().Version);
                if (sequence.MTFVersion > currentVersion)
                {
                    var msg = string.Format("{0}\n{1}",
                        LanguageHelper.GetString("Msg_Body_CouldNotLoad"),
                        LanguageHelper.GetString("Msg_Body_UpdateMTF"));
                    MTFMessageBox.Show(LanguageHelper.GetString("Msg_Header_Error"), msg, MTFMessageBoxType.Error,
                        MTFMessageBoxButtons.Ok);
#if !DEBUG
                    return null; 
#endif
                }
            }

            FillValidationTablesAndVariables(sequence);
            if (sequence.ExternalSubSequencesPath != null && sequence.ExternalSubSequencesPath.Count > 0)
            {
                sequence.ExternalSubSequences = new List<MTFExternalSequenceInfo>();
                foreach (var item in sequence.ExternalSubSequencesPath)
                {
                    var tmp = LoadSequence(DirectoryPathHelper.GetFullPathFromRelative(sequenceName, item.Key), sequenceNames);
                    if (tmp != null)
                    {
                        sequence.ExternalSubSequences.Add(new MTFExternalSequenceInfo(tmp, item.Value));
                        tmp.ParentSequence = sequence;
                    }
                    else
                    {
                        AddDontLoadSequence(item.Key);
                    }
                }
            }

            if (sequence.MTFSequenceClassInfos != null)
            {
                foreach (var c in sequence.MTFSequenceClassInfos.Where(c => !c.IsEnabled))
                {
                    WarningMessage message = new WarningMessage { Message = string.Format(LanguageHelper.GetString("Mtf_WarningMsg_ComponentDisabled"), c.Alias) };
                    warningMessages.Add(message.Id);
                    Warning.Add(message);
                }
            }
            return sequence;
        }


        private void FillValidationTablesAndVariables(MTFSequence sequence)
        {
            if (sequence.MTFVariables != null)
            {
                foreach (var variable in sequence.MTFVariables)
                {
                    if (!(variable.Value is MTFConstantTable))
                    {
                        SequenceVariables.Add(new VariablesWatch(variable));
                    }
                    var table = variable.Value as MTFValidationTable;
                    tableManager.AddExistingTable(table, variable.Id, variable.DependsOnDut);
                }
            }
        }

        private void AddDontLoadSequence(string sequenceName)
        {
            if (dontLoadedSequence == null)
            {
                dontLoadedSequence = new List<string>();
            }
            dontLoadedSequence.Add(sequenceName);
        }

        public MTFSequenceExecutionState ExecutionState => sequenceState;

        private List<Command> mainCommands = null;

        public IEnumerable<Command> Commands()
        {
            if (mainCommands == null)
            {
                generateCommands();
            }

            return mainCommands;
        }

        private void generateCommands()
        {
            if (EnvironmentHelper.HasAccessKeyRole("hideCommands", false))
            {
                mainCommands = null;
                return;
            }
            if (EnvironmentHelper.IsProductionMode)
            {
                if (settings.HideAllCommandsInSequenceExecution)
                {
                    mainCommands = null;
                    return;
                }

                mainCommands = new List<Command>();
                if (settings.AllowOpenSequenceInSequenceExecution)
                {
                    mainCommands.Add(openSequenceDialogCommand);

                }
                if (settings.AllowStartSequenceInSequenceExecution)
                {
                    mainCommands.Add(startSequenceCommand);
                }
                if (settings.AllowStopSequenceInSequenceExecution)
                {
                    mainCommands.Add(stopSequenceCommand);
                }
                if (settings.AllowPauseSequenceInSequenceExecution)
                {
                    mainCommands.Add(pauseSequenceCommand);
                }

                AddBaseViews(mainCommands);
                if (IsShowTableActivated || IsServiceActivated || IsTeachingActivated)
                {
                    mainCommands.Add(tableManager.IsCollapseAllActivated ? expandAllCommand : collapseAllCommand);
                }
            }
            else
            {
                mainCommands = new List<Command>
                               {
                                   openSequenceDialogCommand,
                                   startSequenceCommand,
                                   stopSequenceCommand,
                                   pauseSequenceCommand,
                                   toggleSetupModeCommand,
                               };
                AddBaseViews(mainCommands);
                if (IsShowTableActivated || IsServiceActivated || IsTeachingActivated)
                {
                    mainCommands.Add(tableManager.IsCollapseAllActivated ? expandAllCommand : collapseAllCommand);
                }
                if (EnvironmentHelper.HasAccessKeyRole("service"))
                {
                    mainCommands.Add(serviceCommand);
                }
                if (EnvironmentHelper.HasAccessKeyRole("teach"))
                {
                    mainCommands.Add(teachingCommand);
                }
                if (EnvironmentHelper.HasAccessKeyRole("debug"))
                {
                    mainCommands.Add(debugCommand);

                    if (IsDebugEnabled)
                    {
                        mainCommands.Add(stepIntoCommand);
                        mainCommands.Add(stepOverCommand);
                        mainCommands.Add(stepOutCommand);
                    }
                }
                if (IsServiceActivated || IsTeachingActivated)
                {
                    mainCommands.Add(backCommand);
                }

                if (EnvironmentHelper.HasAccessKeyRole("service") || EnvironmentHelper.HasAccessKeyRole("teach") || EnvironmentHelper.HasAccessKeyRole("debug"))
                {
                    mainCommands.Add(saveCommand);
                }

                if (EnvironmentHelper.UserName.Equals("Super User"))
                {
                    mainCommands.Add(exportCommand);
                }
            }

            if (Sequence != null)
            {
                addClientCorntrolsCommands(Sequence.GetAvailableClientControls());
            }

            if (clientSetupCommand != null)
            {
                mainCommands.Add(clientSetupCommand);
            }

            if (sequence != null && 
                (!sequence.SequenceExecutionUiSetting.TreeViewShowPixtureBox && ViewMode == ExecutionViewMode.Tree
                || !sequence.SequenceExecutionUiSetting.TableViewShowPixtureBox && ViewMode == ExecutionViewMode.Table
                || !sequence.SequenceExecutionUiSetting.GraphicalViewShowPixtureBox && ViewMode == ExecutionViewMode.GraphicalView))
            {
                mainCommands.Add(pictureBoxCommand);
            }
        }

        private void AddBaseViews(List<Command> commands)
        {
            if (Sequence != null)
            {
                if (Sequence.SequenceExecutionUiSetting.AllowTreeView)
                {
                    commands.Add(treeViewCommand);
                }
                if (Sequence.SequenceExecutionUiSetting.AllowTableView)
                {
                    commands.Add(tableViewCommand);
                }
                //if (Sequence.SequenceExecutionUiSetting.AllowTimeView)
                //{
                //    commands.Add(timeViewCommand);
                //}
                if (Sequence.SequenceExecutionUiSetting.AllowGraphicalView)
                {
                    if (Sequence != null && Sequence.GraphicalViewSetting != null && Sequence.GraphicalViewSetting.HasView)
                    {
                        commands.Add(graphicalViewCommand);
                    }
                }
                
            }
        }



        private void addClientCorntrolsCommands(IEnumerable<ClientContolInfo> clientControls)
        {
            var setting = Sequence.SequenceExecutionUiSetting.SelectedClientUis ?? new List<ClientUiIdentification>();
            foreach (var clientControl in clientControls)
            {
                var control = clientControl;
                if (setting.Any(x => x.AssemblyName == control.AssemblyName && x.TypeName == control.TypeName))
                {
                    var clientUiCommand = new Command(() => OpenClientUI(control.AssemblyName, control.TypeName)) { Name = clientControl.Name, Icon = clientControl.Icon };
                    mainCommands.Add(clientUiCommand);
                }
            }
        }

        private void OpenClientUI(string assemblyName, string typeName)
        {
            CurrentUiControl = ClientUiHelper.GetControl(assemblyName, typeName);
            if (ViewMode != ExecutionViewMode.ClientUI)
            {
                ViewMode = ExecutionViewMode.ClientUI;
            }

            RefreshViewModes();
        }

        private void OpenFirstActivClientUI()
        {
            if (settings.SequenceExecutionViewType == SequenceExecutionViewType.ClientUi && Sequence != null &&
                Sequence.SequenceExecutionUiSetting != null)
            {
                var clientUis = Sequence.SequenceExecutionUiSetting.SelectedClientUis;
                if (clientUis != null)
                {
                    var activeUi = clientUis.FirstOrDefault(x => x.IsActive);
                    if (activeUi != null)
                    {
                        OpenClientUI(activeUi.AssemblyName, activeUi.TypeName);
                    }
                    else
                    {
                        ViewMode = ExecutionViewMode.Tree;
                    }
                }
            }
        }

        private void MTFClientOnSequenceExecutionOnUiControlReceiveData(byte[] data, ClientUIDataInfo info)
        {
            ClientUiHelper.ReceiveData(data, info);
        }

        private Control currentUIControl;

        public Control CurrentUiControl
        {
            get { return currentUIControl; }
            set
            {
                currentUIControl = value;
                NotifyPropertyChanged();
            }
        }

        public string OpenedSequence
        {
            get { return openedSequence; }
        }

        public string HeaderText
        {
            get { return sequence == null ? string.Empty : string.Format(LanguageHelper.GetString("Execution_Header_Version"), sequence.Name, sequence.SequenceVersion); }
        }

        void MTFClient_SequenceExecutionRepeatSubSequence(Guid[] executingActivityPath)
        {
            if (ViewMode == ExecutionViewMode.Tree)
            {
                treeViewManager.RepeatSubSequence(executingActivityPath); 
            }
        }

        void MTFClient_SequenceExecutionOnSequenceStatusMessage(string line1, string line2, string line3, StatusLinesFontSize fontSize, Guid? dutId)
        {
            var dutPresenter = GetDut(dutId);

            dutPresenter.Line1 = line1;
            dutPresenter.Line2 = line2;
            dutPresenter.Line3 = line3;
            dutPresenter.LinesFontSize = fontSize;
        }

        void MTFClient_SequenceExecutionShowMessage(MessageInfo messageInfo)
        {
            msgHandler.HandleMessage(messageInfo, MTFClient.SendMessageBoxResult);
        }

        void MTFClient_SequenceExecutionCloseMessage(List<Guid> executingActivityPath)
        {
            msgHandler.ClosePopups(executingActivityPath);
        }

        private readonly object errorMessageLock = new object();

        public ICommand JumpToErrorActivityCommand => jumpToErrorActivityCommand;
    

        private void JumpToErrorActivity(object param)
        {
            var currentApp = Application.Current.MainWindow;
            var mainPresenter = (MainWindowPresenter)currentApp?.DataContext;
            if (mainPresenter != null)
            {
                if (param is StatusMessage msg)
                {
                    mainPresenter.JumpToActivityCommand.Execute(msg.ActivityPath);
                }
            }
        }

        private bool AllowJumpToActivity()
        {
            return !(ExecutionState == MTFSequenceExecutionState.Pause
                   || ExecutionState == MTFSequenceExecutionState.DebugGoToNewPosition
                   || ExecutionState == MTFSequenceExecutionState.DebugGoToTopPosition
                   || ExecutionState == MTFSequenceExecutionState.Executing
                   || ExecutionState == MTFSequenceExecutionState.ExecutionPreparation
                   || ExecutionState == MTFSequenceExecutionState.Stopping
                   || ExecutionState == MTFSequenceExecutionState.Aborting
                   || ExecutionState == MTFSequenceExecutionState.AborSubSequence);
        }

        private int messagesCount;

        public int MessagesCount
        {
            get { return messagesCount; }
            set
            {
                messagesCount = value;
                NotifyPropertyChanged();
            }
        }

        public bool HasMessages
        {
            get { return MessagesCount > 0; }
        }


        void MTFClient_SequenceExecutionError(StatusMessage errorMessage)
        {
            ClientUiHelper.OnErrorMessage(errorMessage);

            App.Current.Dispatcher.Invoke(() =>
                                          {
                                              lock (errorMessageLock)
                                              {
                                                  addMessageToStatusBox(errorMessage);
                                              }
                                          });
        }

        private void addMessageToStatusBox(StatusMessage message)
        {
            MessagesCount++;
            if (ErrorMessages.Count >= 100)
            {
                ErrorMessages.RemoveAt(0);
                messagesCount = ErrorMessages.Count;
            }
            ErrorMessages.Add(message);
            checkControlStatus();

            NotifyPropertyChanged("HasMessages");
            if (popErrorAutomaticaly && ControlStatus == ControlStatus.Error)
            {
                ShowError = true;
            }
        }

        private void checkControlStatus()
        {
            if (ErrorMessages.Any(em => em.Type == StatusMessage.MessageType.Error))
            {
                ControlStatus = ControlStatus.Error;
                return;
            }
            if (ErrorMessages.Any(em => em.Type == StatusMessage.MessageType.Warning))
            {
                ControlStatus = ControlStatus.Warning;
                return;
            }

            ControlStatus = ControlStatus.None;
        }

        void MTFClient_SequenceExecutionFinished(MTFSequenceResult sequenceResult)
        {
            if (ViewMode == ExecutionViewMode.Service)
            {
                askChangeFromService = false;
                ViewMode = previousViewMode;
                RefreshViewModes();
            }

            treeViewManager.ExecutionPointer.Hide();

            //if (newState == MTFSequenceExecutionState.Stopped || newState == MTFSequenceExecutionState.Aborted || newState == MTFSequenceExecutionState.Finished)
            {
                NotifyPropertyChanged("ActivityHeaderParameters");
                NotifyPropertyChanged("ActivityResult");
                SequenceResult = sequenceResult;
                NotifyPropertyChanged("SequenceResult");
            }
            ChangeTableEditMode(false);
        }

        private void ReloadSequenceInEditor()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var sequenceEdirorPresenter = GetEditorPresenter();
                if (sequenceEdirorPresenter != null && sequenceEdirorPresenter.OpenedSequenceFileName == openedSequence)
                {
                    //sequencaWasModified = false;
                    sequenceEdirorPresenter.InvalidateSequence();
                }
            });
        }

        private SequenceEditor.SequenceEditorPresenter GetEditorPresenter()
        {
            return UIHelper.InvokeOnDispatcher(() => ((MainWindowPresenter)Application.Current.MainWindow.DataContext).SequenceEditorPresenter);
        }


        private PopupWindow.PopupWindow sequenceEndWindow;

        void MTFClient_SequenceExecutionStateChanged(MTFSequenceExecutionState newState)
        {

            if (newState == MTFSequenceExecutionState.Aborted || newState == MTFSequenceExecutionState.Finished || newState == MTFSequenceExecutionState.Stopped)
            {
                treeViewManager.Stop();
                UIHelper.InvokeOnDispatcher(ActivityProgress.Clear);
                ResetUserCommands();
            }
            ClientUiHelper.OnExecutionStateChanged(newState);
            if (sequenceEndWindow != null)
            {
                UIHelper.InvokeOnDispatcher(() =>
                                   {
                                       if (sequenceEndWindow != null)
                                       {
                                           sequenceEndWindow.CanClose = true;
                                           sequenceEndWindow.Close();
                                       }
                                   });
            }

            if (IsDebugEnabled && newState == MTFSequenceExecutionState.Executing)
            {
                currentMarkedItems.ClearAndUnmark();
            }

            NotifyPropertyChanged("Activity");
            sequenceState = newState;

            UIHelper.InvokeOnDispatcher(jumpToErrorActivityCommand.RaiseCanExecuteChanged);

            NotifyPropertyChanged("IsPausedInDebug");

            RuntimePartVisible = ExecutionState == MTFSequenceExecutionState.Stopped || ExecutionState == MTFSequenceExecutionState.None ||
                                 ExecutionState == MTFSequenceExecutionState.Aborted || ExecutionState == MTFSequenceExecutionState.Finished ||
                                 executingActivity == null
                ? Visibility.Collapsed
                : Visibility.Visible;

            if (newState == MTFSequenceExecutionState.Stopped)
            {
                executingActivity = null;
            }

            if (newState == MTFSequenceExecutionState.CriticalAbort)
            {
                sequence = null;
            }

            UIHelper.InvokeOnDispatcher(() =>
                               {
                                   startSequenceCommand.RaiseCanExecuteChanged();
                                   stopSequenceCommand.RaiseCanExecuteChanged();
                                   pauseSequenceCommand.RaiseCanExecuteChanged();
                                   openSequenceDialogCommand.RaiseCanExecuteChanged();
                                   toggleSetupModeCommand.RaiseCanExecuteChanged();
                                   stepIntoCommand.RaiseCanExecuteChanged();
                                   stepOutCommand.RaiseCanExecuteChanged();
                                   stepOverCommand.RaiseCanExecuteChanged();
                                   exportCommand.RaiseCanExecuteChanged();
                               });

            if (newState == MTFSequenceExecutionState.Stopped)
            {
                UIHelper.InvokeOnDispatcher(() =>
                                   {
                                       var messageInfo = new MessageInfo
                                                         {
                                                             Text = LanguageHelper.GetString("Msg_Body_SeqStop"),
                                                             Type = SequenceMessageType.Info,
                                                             Buttons = MessageButtons.Ok
                                                         };
                                       sequenceEndWindow =
                                           new PopupWindow.PopupWindow(new MessageBoxControl.MessageBoxControl(messageInfo)) { Title = LanguageHelper.GetString("Msg_Header_Info") };
                                       sequenceEndWindow.ShowDialog();
                                   });
            }
            if (newState == MTFSequenceExecutionState.Aborted)
            {
                string errorMessage = string.Empty;
                if (ErrorMessages != null && ErrorMessages.Count > 0)
                {
                    errorMessage = ErrorMessages.Last().Text;
                }
                UIHelper.InvokeOnDispatcher(() =>
                                   {
                                       var messageInfo = new MessageInfo
                                                         {
                                                             Text = LanguageHelper.GetString("Msg_Body_SeqAbort"),
                                                             Type = SequenceMessageType.Error,
                                                             Buttons = MessageButtons.Ok
                                                         };
                                       sequenceEndWindow =
                                           new PopupWindow.PopupWindow(
                                               new MessageBoxControl.MessageBoxControl(messageInfo)) { Title = LanguageHelper.GetString("Msg_Header_Error") };
                                       sequenceEndWindow.ShowDialog();
                                   });
            }
            if (newState == MTFSequenceExecutionState.Finished)
            {
                UIHelper.InvokeOnDispatcher(() =>
                                   {
                                       var messageInfo = new MessageInfo
                                                         {
                                                             Text = LanguageHelper.GetString("Msg_Body_SeqFinish"),
                                                             Type = SequenceMessageType.Info,
                                                             Buttons = MessageButtons.Ok
                                                         };
                                       sequenceEndWindow =
                                           new PopupWindow.PopupWindow(
                                               new MessageBoxControl.MessageBoxControl(messageInfo))
                                           { Title = LanguageHelper.GetString("Msg_Header_Info") };
                                       sequenceEndWindow.ShowDialog();
                                   });
            }
            if (IsDebugEnabled)
            {
                ChangeTableEditMode(ExecutionState == MTFSequenceExecutionState.Pause);
            }
        }

        private Visibility runtimePartVisible;

        public Visibility RuntimePartVisible
        {
            get { return runtimePartVisible; }
            set
            {
                runtimePartVisible = value;
                NotifyPropertyChanged();
            }
        }

        void MTFClient_SequenceExecutionActivityStringProgress(ActivityStringProgressChangedEventArgs e)
        {
            lock (progressEventsLock)
            {
                if (e.Command == ActivityStringProgressCommand.ShowMessage)
                {
                    App.Current.Dispatcher.Invoke(() =>
                                                  {
                                                      lock (errorMessageLock)
                                                      {
                                                          addMessageToStatusBox(e.Message);
                                                      }
                                                  });
                }
                else if (e.Command == ActivityStringProgressCommand.CleanErrorWindow)
                {
                    App.Current.Dispatcher.Invoke(() =>
                                                  {
                                                      lock (errorMessageLock)
                                                      {
                                                          MessagesCount = 0;
                                                          NotifyPropertyChanged("HasMessages");
                                                          ErrorMessages.Clear();
                                                          ShowError = false;
                                                          ControlStatus = ControlStatus.None;
                                                      }
                                                  });
                }
            }
        }

        void MTFClient_SequenceExecutionActivityPercentProgress(ActivityPercentProgressChangedEventArgs e)
        {
            lock (progressEventsLock)
            {
                if (ActivityProgress.Count > 0)
                {
                    var percentProgress = ActivityProgress.First() as ActivityProgressBar;
                    if (percentProgress != null)
                    {
                        percentProgress.Percent = e.Percent;
                        percentProgress.Text = e.Text;
                        percentProgress.IsIndeterminate = e.IsStarted;
                    }
                }
            }
        }

        private void SetSequenceVariant(SequenceVariantInfo sequenceVariantInfo, Guid? dutId)
        {
            GetDut(dutId).SequenceVariant = sequenceVariantInfo.SequenceVariant;
        }

        private void ClearSequenceVariants()
        {
            if (DutPresenters == null)
                return;
            foreach (var dutPresenter in DutPresenters)
            {
                dutPresenter.SequenceVariant = null;
            }
        }

        private SequenceVariantInfo lastUseVariantInfo = null;

        private readonly object sequenceVariantLock = new object();

        private ControlStatus prevControlStatus;
        void MTFClient_SequenceExecutionSequenceVariantChanged(SequenceVariantInfo sequenceVariantInfo, Guid? dutId)
        {
            ClientUiHelper.OnVariantChanged(sequenceVariantInfo);

            lock (sequenceVariantLock)
            {
                if (sequenceVariantInfo != null)
                {
                    SetSequenceVariant(sequenceVariantInfo, dutId);

                    //if is goldsample in, change staus
                    if (sequenceVariantInfo.IsGoldSample)
                    {
                        prevControlStatus = ControlStatus;
                        ControlStatus = ControlStatus.GoldSample;
                    }
                    else
                    {
                        ControlStatus = prevControlStatus;
                    }

                    if (sequenceVariantInfo.MissingGoldSample)
                    {
                        var emptyGoldSample = goldSampleVariants.FirstOrDefault(x => x.MissingGoldSample);
                        if (emptyGoldSample != null)
                        {
                            if (!sequenceVariantInfo.AllowMoreGS)
                            {
                                UIHelper.InvokeOnDispatcher(() =>
                                {
                                    goldSampleVariants.Clear();
                                    goldSampleVariants.Add(emptyGoldSample);
                                });
                            }
                            else
                            {
                                MoveToFront(emptyGoldSample);
                            }
                        }
                        else
                        {
                            if (!sequenceVariantInfo.AllowMoreGS)
                            {
                                UIHelper.InvokeOnDispatcher(() => goldSampleVariants.Clear());
                            }
                            SetCurrentGS(sequenceVariantInfo);
                            UIHelper.InvokeOnDispatcher(() => goldSampleVariants.Insert(0, sequenceVariantInfo));
                        }
                        return;
                    }

                    var bestGoldSample = sequenceVariantInfo.SequenceVariant.GetBestGoldSample(goldSampleVariants.Select(x => x.SequenceVariant), sequence.GoldSampleSetting.GoldSampleSelector);
                    if (bestGoldSample != null)
                    {
                        var bestGoldSampleInfo = goldSampleVariants.FirstOrDefault(x => Equals(x.SequenceVariant, bestGoldSample));

                        if (bestGoldSampleInfo != null)
                        {
                            if (bestGoldSampleInfo.MissingGoldSample)
                            {
                                bestGoldSampleInfo.MissingGoldSample = false;
                                bestGoldSampleInfo.GoldSampleDate = sequenceVariantInfo.GoldSampleDate;
                                bestGoldSampleInfo.NonGoldSampleCount = sequenceVariantInfo.NonGoldSampleCount;
                                bestGoldSampleInfo.NonGoldSampleRemainsMinutes = sequenceVariantInfo.NonGoldSampleRemainsMinutes;
                                bestGoldSampleInfo.SequenceVariant = sequenceVariantInfo.SequenceVariant;
                            }
                            bestGoldSampleInfo.GoldSampleExpired = sequenceVariantInfo.GoldSampleExpired;
                            MoveToFront(bestGoldSampleInfo);
                            if (sequence.GoldSampleSetting.GoldSampleValidationMode == GoldSampleValidationMode.Time
                                || sequence.GoldSampleSetting.GoldSampleValidationMode == GoldSampleValidationMode.Shift)
                            {
                                bestGoldSampleInfo.NonGoldSampleRemainsMinutes = sequenceVariantInfo.NonGoldSampleRemainsMinutes;
                                foreach (var goldSampleVariant in goldSampleVariants)
                                {
                                    if (goldSampleVariant != bestGoldSampleInfo)
                                    {
                                        goldSampleVariant.NonGoldSampleRemainsMinutes -= sequenceVariantInfo.ElapsedMinutes;
                                    }
                                }
                            }
                            else
                            {
                                bestGoldSampleInfo.NonGoldSampleCount = sequenceVariantInfo.NonGoldSampleCount;
                            }
                            if (!sequenceVariantInfo.AllowMoreGS)
                            {
                                UIHelper.InvokeOnDispatcher(() =>
                                {
                                    goldSampleVariants.Clear();
                                    goldSampleVariants.Add(bestGoldSampleInfo);
                                });
                            }
                        }

                    }
                    else
                    {
                        if (!sequenceVariantInfo.AllowMoreGS)
                        {
                            UIHelper.InvokeOnDispatcher(() => goldSampleVariants.Clear());
                        }
                        SetCurrentGS(sequenceVariantInfo);
                        UIHelper.InvokeOnDispatcher(() => goldSampleVariants.Insert(0, sequenceVariantInfo));
                    }
                }
            }
        }

        private void MoveToFront(SequenceVariantInfo currentGs)
        {
            var oldIndex = goldSampleVariants.IndexOf(currentGs);
            if (!Equals(lastUseVariantInfo, currentGs))
            {
                SetCurrentGS(currentGs);
                if (oldIndex != 0)
                {
                    Application.Current.Dispatcher.Invoke(() => goldSampleVariants.Move(oldIndex, 0));
                }
            }
        }

        public void SetCurrentGS(SequenceVariantInfo currentGs)
        {
            currentGs.IsCurrent = true;
            if (lastUseVariantInfo != null)
            {
                lastUseVariantInfo.IsCurrent = false;
            }
            lastUseVariantInfo = currentGs;
        }

        private void GenerateServiceCommands()
        {
            if (sequence.ServiceCommands != null)
            {
                //allowedServicecommands = sequence.ServiceCommands.Select(x => x.Id);
                MTFServiceModeVariants serviceModeVariant = IsServiceActivated ? MTFServiceModeVariants.ServiceMode : MTFServiceModeVariants.Teach;
                serviceCommands = new ObservableCollection<MTFServiceCommandWrapper>(
                    sequence.ServiceCommands.
                    Where(c => c.UsedServiceVariants != null && c.UsedServiceVariants.Contains(serviceModeVariant)).
                    Select(c => new MTFServiceCommandWrapper(c) { ExecutedCommand = new Command(executeServiceCommand, () => isServiceCommandEnabled(c.Id)) })
                    );
            }
        }

        private ObservableCollection<MTFServiceCommandWrapper> serviceCommands;

        public ObservableCollection<MTFServiceCommandWrapper> ServiceCommands
        {
            get { return serviceCommands; }
            set { serviceCommands = value; }
        }

        private bool isServiceCommandEnabled(Guid commandId)
        {
            return allowedServicecommands != null && allowedServicecommands.Contains(commandId);
        }

        private IEnumerable<Guid> allowedServicecommands;

        public IEnumerable<Guid> AllowedServicecommands
        {
            set
            {
                allowedServicecommands = value;
                if (ServiceCommands != null)
                {
                    foreach (var serviceCommand in ServiceCommands)
                    {
                        Application.Current.Dispatcher.Invoke(() => serviceCommand.ExecutedCommand.RaiseCanExecuteChanged());
                    }
                }
            }
            get { return allowedServicecommands; }
        }

        private void MtfClientServiceCommandsStateChanged(IEnumerable<Guid> allowedCommands, IEnumerable<Guid> checkedCommands)
        {
            AllowedServicecommands = allowedCommands;
            if (checkedCommands != null)
            {
                foreach (var commandWrapper in ServiceCommands)
                {
                    commandWrapper.IsChecked = checkedCommands.Contains(commandWrapper.Command.Id);
                }
            }
        }

        void MTFClient_SequenceExecutionActivityImageProgress(ActivityImageProgressChangedEventArgs e)
        {
            lock (progressEventsLock)
            {
                imageHandler.AddToBuffer(e.ImageData, e.ExecutionPath != null ? e.ExecutionPath.ToArray() : null);
            }
        }

        private MTFSequenceActivity getActivityByPath(MTFSequenceActivity rootActivity, Guid[] activityPath)
        {
            if (rootActivity.Id == activityPath[0] && activityPath.Length == 1)
            {
                return rootActivity;
            }

            if (rootActivity is MTFSubSequenceActivity && activityPath.Length > 1 && ((MTFSubSequenceActivity)rootActivity).Activities != null)
            {
                var activity = ((MTFSubSequenceActivity)rootActivity).Activities.FirstOrDefault(a => a.Id == activityPath[1]);
                if (activity != null)
                {
                    return getActivityByPath(activity, activityPath.Except(new[] { activityPath[0] }).ToArray());
                }
            }
            return null;
        }

        private MTFSequenceActivity getActivityInServcieCommand(Guid[] executingActivityPath)
        {
            if (Sequence.ServiceCommands != null)
            {
                var command = Sequence.ServiceCommands.FirstOrDefault(c => c.PrepairActivity != null && c.PrepairActivity.Id == executingActivityPath[0]);
                if (command != null)
                {
                    return getActivityByPath(command.PrepairActivity, executingActivityPath);
                }

                command = Sequence.ServiceCommands.FirstOrDefault(c => c.ExecuteActivity != null && c.ExecuteActivity.Id == executingActivityPath[0]);
                if (command != null)
                {
                    return getActivityByPath(command.ExecuteActivity, executingActivityPath);
                }

                command = Sequence.ServiceCommands.FirstOrDefault(c => c.CleaunupActivity != null && c.CleaunupActivity.Id == executingActivityPath[0]);
                if (command != null)
                {
                    return getActivityByPath(command.CleaunupActivity, executingActivityPath);
                }
            }

            return null;
        }

        private readonly object executionActivityChangedLock = new object();

        void MTFClient_SequenceExecutionActivityChanged(Guid[] executingActivityPath)
        {
            lock (executionActivityChangedLock)
            {
                if (treeViewManager.IsEmpty)
                {
                    return;
                }

                if (isDebugEnabled)
                {
                    treeViewManager.ExecutionPointer.Hide();
                }



                //optimalization - try find activity in service commands
                var itemToSelect = treeViewManager.FindInSourceWithIndexes(executingActivityPath);
                if (itemToSelect == null && Sequence.ServiceCommands != null)
                {

                    var activity = getActivityInServcieCommand(executingActivityPath);
                    if (activity != null)
                    {
                        itemToSelect = new MTFActivityVisualisationWrapper { Activity = activity };
                    }
                }

                // try find activity by id - last chance to find something
                if (itemToSelect == null)
                {
                    var activity = Sequence.GetActivity(executingActivityPath.Last());
                    if (activity == null)
                    {
                        return;
                    }

                    itemToSelect = new MTFActivityVisualisationWrapper { Activity = activity };
                }

                treeViewManager.ExecutionPointer.Change(itemToSelect);

                executingActivity = itemToSelect.Activity;
                if (treeViewManager.ActivityDetailChangeAtRuntime || ViewMode == ExecutionViewMode.Table || ViewMode == ExecutionViewMode.Time)
                {
                    SelectedItem = itemToSelect;
                }

                ClientUiHelper.OnActivityChanged(executingActivity);

                if (executingActivity is MTFCallActivityBase)
                {
                    if (executingActivity is MTFSubSequenceActivity && (executingActivity as MTFSubSequenceActivity).ExecutionType == ExecutionType.ExecuteInParallel)
                    {
                        RuntimePartVisible = Visibility.Collapsed;
                    }
                    if (ViewMode == ExecutionViewMode.Tree && treeViewManager.ActivityDetailChangeAtRuntime)
                    {
                        treeViewManager.ChangeTree(itemToSelect, true);
                    }
                }
                else if (!executingActivity.GetType().IsSubclassOf(typeof(MTFSequenceActivity)))
                {
                    //var method = sequence.MTFSequenceClassInfos.FirstOrDefault(ci => ci.Alias == executingActivity.ClassInfo.Alias)
                    //                .MTFClass.Methods.FirstOrDefault(m => m.Name == executingActivity.MTFMethodName);
                    var method = executingActivity.ClassInfo.MTFClass.Methods.FirstOrDefault(m => m.Name == executingActivity.MTFMethodName);

                    if (method != null)
                    {
                        FillProgressEvents();
                    }
                    else
                    {
                        //var property = sequence.MTFSequenceClassInfos.FirstOrDefault(ci => ci.Alias == executingActivity.MTFClassAlias)
                        //            .MTFClass.Properties.FirstOrDefault(p => p.Name == executingActivity.MTFMethodName.Split('.')[0]);
                        var property = executingActivity.ClassInfo.MTFClass.Properties.FirstOrDefault(p => p.Name == executingActivity.MTFMethodName.Split('.')[0]);
                        if (property != null)
                        {
                            FillProgressEvents();
                        }
                    }
                }
            }
        }

        private void MtfClientOnOnViewChanged(SequenceExecutionViewType view, Guid? graphicalViewId, Guid? dutId)
        {
            Task.Run(() =>
            {
                if (ViewMode != ExecutionViewMode.Service)
                {
                    switch (view)
                    {
                        case SequenceExecutionViewType.TreeView:
                            ShowTreeView();
                            break;
                        case SequenceExecutionViewType.TableView:
                            ShowTableView();
                            break;
                        case SequenceExecutionViewType.TimeView:
                            //ShowTimeView();
                            ShowTableView();
                            break;
                        case SequenceExecutionViewType.Service:
                            break;
                        case SequenceExecutionViewType.ClientUi:
                            SwitchToFirstClientUI();
                            break;
                        case SequenceExecutionViewType.GraphicalView:
                            ShowGraphicalView();
                            if (graphicalViewId != null)
                            {
                                GetDut(dutId).GraphicalView = GraphicalViewManager.GetGraphicalView(graphicalViewId, dutId);
                            }
                            break;
                    }
                }
            });
        }

        private void SwitchToFirstClientUI()
        {
            if (Sequence != null)
            {
                var clientUis = Sequence.SequenceExecutionUiSetting.SelectedClientUis;
                if (clientUis != null)
                {
                    var activeUi = clientUis.FirstOrDefault();
                    if (activeUi != null)
                    {
                        OpenClientUI(activeUi.AssemblyName, activeUi.TypeName);
                    }
                    else
                    {
                        ViewMode = ExecutionViewMode.Tree;
                    }
                }
            }
        }

        public ICommand ChangeCollapsedStateCommand
        {
            get
            {
                return new Command(param =>
                                   {
                                       var subSequenceListBoxItem = UIHelper.FindParent<ListBoxItem>(param as Button);
                                       var item = subSequenceListBoxItem.Content as MTFActivityVisualisationWrapper;
                                       treeViewManager.Start();
                                       treeViewManager.ChangeTree(subSequenceListBoxItem.Content as MTFActivityVisualisationWrapper, item.IsCollapsed);
                                   });
            }
        }

        public bool IsProductionMode
        {
            get { return EnvironmentHelper.IsProductionMode; }
        }

        public Visibility NoProductionMode
        {
            get { return EnvironmentHelper.IsProductionMode ? Visibility.Visible : Visibility.Collapsed; }
        }

        public void RefreshMenuVisibility()
        {
            generateCommands();
            if (EnvironmentHelper.IsProductionMode)
            {
                IsSetupModeActive = false;
            }

            NotifyPropertyChanged("IsProductionMode");
            NotifyPropertyChanged("NoProductionMode");
        }


        void MTFClient_SequenceExecutionNewValidateRows(List<MTFValidationTableRowResult> rows, bool activityWillBeRepeated, Guid? dutId)
        {
            tableManager.NewValidateRows(rows, activityWillBeRepeated, ViewMode, GetExecutionStatus(), currentMarkedItems, dutId);

            if (IsDebugEnabled)
            {
                var table = tableManager.GetValidationTableForWatch(rows);
                if (table != null)
                {
                    SetTableInWatchAsync(table);
                }
            }
        }

        private async void SetTableInWatchAsync(ExecutionValidTable currentTable)
        {
            VariablesWatch currentVariable = null;
            await Task.Run(() => currentVariable = SequenceVariables.FirstOrDefault(x => x.Variable != null && x.Variable.Value is MTFValidationTable && (x.Variable.Value as MTFValidationTable).Id == currentTable.Id));
            if (currentVariable != null && currentVariable.IsInWatch)
            {
                currentVariable.IsChanged = true;
                currentMarkedItems.AddItem(currentVariable);
            }
        }

        private ExecutionStatus GetExecutionStatus()
        {
            return new ExecutionStatus { IsDebugActivated = IsDebugEnabled, IsTeachActivated = IsTeachingActivated, IsServiceActivated = IsServiceActivated };
        }

        void MTFClient_SequenceExecutionDynamicLoadSequence(MTFSequence dynamicSequence, Dictionary<Guid, MTFExternalSequenceInfo> externalSequences)
        {
            treeViewManager.LoadDynamic(dynamicSequence, externalSequences);
            if (dynamicSequence != null && dynamicSequence.MTFVariables != null)
            {
                FillValidationTablesAndVariables(dynamicSequence);
            }
        }

        void MTFClient_SequenceExecutionDynamicUnloadSequence(Guid sequenceId)
        {
            treeViewManager.DynamicUnload(sequenceId);
            CleanDynamicVariables(sequenceId);
        }

        private void CleanDynamicVariables(Guid sequenceId)
        {
            var dynamicVariables = treeViewManager.GetDynamicVariablesBySequence(sequenceId);
            if (dynamicVariables != null)
            {
                foreach (var variableId in dynamicVariables)
                {
                    if (SequenceVariables != null)
                    {
                        var sv = SequenceVariables.FirstOrDefault(x => x.Variable.Id == variableId);
                        if (sv != null)
                        {
                            SequenceVariables.Remove(sv);
                        }
                        tableManager.RemoveDynamicTables(variableId);
                    }
                }
            }
        }

        void MTFClient_SequenceExecutionDynamicExecuteSequence(Guid sequenceId, Guid subSequenceId, bool callSubSequence, Guid[] activityIdPath)
        {
            try
            {
                treeViewManager.InsertDynamicActivities(sequenceId, subSequenceId, callSubSequence, activityIdPath);
            }
            catch (Exception ex)
            {
                MTFMessageBox.Show(LanguageHelper.GetString("Msg_Header_Error"), ex.Message, MTFMessageBoxType.Error, MTFMessageBoxButtons.Ok);
                MTFClient.StopSequence();
            }
        }











        void MTFClient_SequenceExecutionTreeResults(MTFActivityResult[] results)
        {
            if (results != null)
            {
                allowRefreshWatch = false;
                foreach (var result in results)
                {
                    NewLoadedActivityResult(result);
                }
                allowRefreshWatch = true;
            }
            waitingForTreeResults = false;
        }


        void MTFClient_SequenceExecutionTableViewResults(MTFValidationTableRowResult[] results)
        {
            if (results != null)
            {
                foreach (IEnumerable<MTFValidationTableRowResult> data in results.GroupBy(g => new {g.TableId, g.DutId}))
                {
                    tableManager.UpdateLocalValidationTable(data.First().TableId, data, GetExecutionStatus(), currentMarkedItems, data.First().DutId);
                }
            }
            waitingForTableViewResults = false;
        }

        void MTFClient_SequenceExecutionNewActivityResult(MTFActivityResult result)
        {
            if (treeViewManager.IsEmpty)
            {
                return;
            }

            ClientUiHelper.OnNewActivityResult(result);

            treeViewManager.AssignNewActivityResultInRuntime(result);

            //var image = result.ActivityResult as MTFImage;
            //if (image != null)
            //{
            //    imageHandler.AddToBuffer(image.ImageData, result.ActivityIdPath);
            //}

            if (IsDebugEnabled && allowRefreshWatch)
            {
                var variableResult = result as MTFVariableActivityResult;
                if (variableResult != null && VariablesInWatch != null && VariablesInWatch.Count > 0)
                {
                    SetVariableValueInWatch(variableResult.VariableId, variableResult.Value);
                }
            }
        }

        private void NewLoadedActivityResult(MTFActivityResult result)
        {
            if (treeViewManager.IsEmpty)
            {
                return;
            }

            ClientUiHelper.OnNewActivityResult(result);

            var item = treeViewManager.FindInSourceWithIndexes(result.ActivityIdPath);
            if (item != null)
            {
                item.Status = result.Status;
                item.Result = result;
                result.Parent = item.Activity;

                //var image = result.ActivityResult as MTFImage;
                //if (image != null)
                //{
                //    imageHandler.AddLoadedResultToBuffer(image.ImageData, result.ActivityIdPath);
                //}
            }
        }

        public object ActivityHeader
        {
            get
            {
                if (selectedItem == null)
                {
                    return null;
                }
                else
                {
                    return selectedItem.Activity;
                }
            }
        }

        public object ActivityHeaderParameters
        {
            get
            {
                if (selectedItem == null)
                {
                    return null;
                }
                else
                {
                    var subSequenceActivity = selectedItem.Activity as MTFSubSequenceActivity;
                    if (subSequenceActivity != null && subSequenceActivity.ExecutionType == ExecutionType.ExecuteInParallel)
                    {
                        treeViewManager.GenerateParallelActivities(selectedItem);
                    }
                    if (selectedItem.Result == null)
                    {
                        return selectedItem.Activity;
                    }
                    else
                    {
                        return selectedItem.Result;
                    }
                }
            }
        }


        public object ActivityResult
        {
            get
            {
                if (selectedItem == null)
                {
                    return null;
                }
                else
                {
                    return selectedItem.Result;
                }
            }
        }

        public ObservableCollection<StatusMessage> ErrorMessages { get; set; }

        private bool showError;
        private bool popErrorAutomaticaly = true; //!EnvironmentHelper.IsProductionMode;

        public bool ShowError
        {
            get { return showError; }
            set
            {
                showError = value;
                //if (!value)
                //{
                //    popErrorAutomaticaly = false;
                //}
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<ActivityProgressBase> ActivityProgress { get; set; }

        public MTFSequenceResult SequenceResult { get; set; }

        public MTFSequence Sequence
        {
            get { return sequence; }
            set
            {
                sequence = value;
                NotifyPropertyChanged();
                UIHelper.InvokeOnDispatcher(() =>
                                   {
                                       startSequenceCommand.RaiseCanExecuteChanged();
                                       toggleSetupModeCommand.RaiseCanExecuteChanged();
                                       debugCommand.RaiseCanExecuteChanged();
                                       serviceCommand.RaiseCanExecuteChanged();
                                       teachingCommand.RaiseCanExecuteChanged();
                                   });
            }
        }

        public ObservableCollection<ExecutionValidTable> ServiceTables
        {
            get { return tableManager.ServiceTables; }
        }

        //public ExecutionGlobalTable GlobalValidationTable
        //{
        //    get { return tableManager.GlobalValidationTable; }
        //}


        private MTFActivityVisualisationWrapper selectedItem;

        public MTFActivityVisualisationWrapper SelectedItem
        {
            get { return selectedItem; }
            set
            {
                selectedItem = value;
                NotifyPropertyChanged();
                if (selectedItem == null || executingActivity == null)
                {
                    RuntimePartVisible = Visibility.Collapsed;
                }
                else
                {
                    RuntimePartVisible = executingActivity.Id == selectedItem.Activity.Id && ExecutionState != MTFSequenceExecutionState.Finished ? Visibility.Visible : Visibility.Collapsed;
                }
                NotifyPropertyChanged("ActivityHeader");
                NotifyPropertyChanged("ActivityHeaderParameters");
                NotifyPropertyChanged("ActivityResult");
                if (selectedItem != null && selectedItem.Result != null)
                {
                    var currentResult = selectedItem.Result;
                    imageHandler.SetImagByResult(currentResult);
                }
            }
        }

        private void SetSelectedItem(MTFActivityVisualisationWrapper activity)
        {
            treeViewManager.ShowActivityInTree(activity);
            SelectedItem = activity;
        }

        #region Variables Watch

        private int variablesOpenButtonScaleTarget = 1;
        private bool showVariablesSetting;
        private bool showVar;
        private bool allowRefreshWatch = true;
        private MarkedDebugItems currentMarkedItems = new MarkedDebugItems();
        private ObservableCollection<ExecutionValidTable> tablesInWatch = new ObservableCollection<ExecutionValidTable>();
        private List<VariablesWatch> variablesInWatch;

        private void InitDebug()
        {
            tablesInWatch.Clear();
            currentMarkedItems.ClearAndUnmark();
            AllInWatch = false;
            if (SequenceVariables != null)
            {
                VariablesInWatch = SequenceVariables.Where(x => x.IsInWatch).ToList();
            }
        }

        public List<VariablesWatch> SequenceVariables { get; set; }

        public List<VariablesWatch> VariablesInWatch
        {
            get => variablesInWatch;
            set
            {
                variablesInWatch = value;
                NotifyPropertyChanged();
            }
        }

        public bool VariablesIsVisible
        {
            get { return showVar; }
            set
            {
                showVar = value;
                NotifyPropertyChanged();
            }
        }


        public bool VariablesSettingIsVisible
        {
            get { return showVariablesSetting; }
            set
            {
                showVariablesSetting = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand GoToVariablesWatchSettingCommand
        {
            get { return new Command(() => VariablesSettingIsVisible = true); }
        }


        public ICommand SaveVariablesWatchSettingCommand
        {
            get { return new Command(SaveVariablesWatchSetting); }
        }

        private void SaveVariablesWatchSetting()
        {
            if (VariablesInWatch != null)
            {
                VariablesInWatch.Clear();
                foreach (var item in SequenceVariables)
                {
                    if (item.IsInWatch)
                    {
                        VariablesInWatch.Add(item);
                    }
                    else
                    {
                        item.IsExpanded = false;
                        UpdateTablesWatchPreview(item);
                    }
                }
                var results = MTFClient.GetActualVariableValues(VariablesInWatch.Select(x => x.Variable.Id));
                if (results != null)
                {
                    foreach (var res in results)
                    {
                        SetVariableValueInWatchWithoutChangeFlag(res.Key, res.Value);
                    }
                }
                if (VariablesInWatch.Count > 0)
                {
                    VariablesSettingIsVisible = false;
                }
            }
            else
            {
                VariablesSettingIsVisible = false;
            }
        }

        public ICommand ShowVariablesCommand
        {
            get { return new Command(ShowVariables); }
        }

        private void ShowVariables()
        {
            if (SequenceVariables != null && SequenceVariables.Count > 0)
            {
                VariablesIsVisible = !VariablesIsVisible;
                VariablesOpenButtonScaleTarget *= -1;
                if (!VariablesSettingIsVisible && (VariablesInWatch == null || VariablesInWatch.Count < 1))
                {
                    VariablesSettingIsVisible = true;
                }
            }
        }


        public int VariablesOpenButtonScaleTarget
        {
            get { return variablesOpenButtonScaleTarget; }
            set
            {
                variablesOpenButtonScaleTarget = value;
                NotifyPropertyChanged();
            }
        }

        private void SetVariableValueInWatchWithoutChangeFlag(Guid variableId, object value)
        {
            var variableInWatch = VariablesInWatch.FirstOrDefault(x => x.Variable.Id == variableId);
            if (variableInWatch != null)
            {
                variableInWatch.Value = value;
            }
        }

        private void SetVariableValueInWatch(Guid variableId, object value)
        {
            var variableInWatch = VariablesInWatch.FirstOrDefault(x => x.Variable.Id == variableId);
            if (variableInWatch != null)
            {
                variableInWatch.IsChanged = true;
                variableInWatch.Value = value;
                currentMarkedItems.AddItem(variableInWatch);
            }
        }

        public bool AllInWatch { get; set; }

        public void SetAllInWatch()
        {
            if (SequenceVariables != null)
            {
                if (AllInWatch)
                {
                    if (VariablesInWatch != null)
                    {
                        VariablesInWatch.Clear();
                    }
                    SequenceVariables.ForEach(x => x.IsInWatch = false);
                    AllInWatch = false;
                }
                else
                {
                    SequenceVariables.ForEach(x => x.IsInWatch = true);
                    VariablesInWatch = SequenceVariables.ToList();
                    AllInWatch = true;
                }
                NotifyPropertyChanged("AllInWatch");
            }
        }


        public ObservableCollection<ExecutionValidTable> TablesInWatch
        {
            get { return tablesInWatch; }
            set { tablesInWatch = value; }
        }

        public ICommand UpdateTablesWatchPreviewCommand
        {
            get { return new Command(UpdateTablesWatchPreview); }
        }

        private void UpdateTablesWatchPreview(object param)
        {
            var tableInWatch = param as VariablesWatch;
            if (tableInWatch != null && tableInWatch.Variable != null && tableInWatch.Variable.Value is MTFValidationTable)
            {
                var table = tableManager.ExistingTables(null).FirstOrDefault(x => x.Id == ((MTFValidationTable)tableInWatch.Variable.Value).Id);
                if (table != null)
                {
                    if (tableInWatch.IsExpanded)
                    {
                        TablesInWatch.Add(table);
                    }
                    else
                    {
                        TablesInWatch.Remove(table);
                    }
                }
            }
        }

        public void UpdateVariableFromDebug(MTFVariable variable, object value)
        {
            if (variable != null)
            {
                MTFClient.SetNewVariableValue(variable.Id, value);
            }
        }

        #endregion

        public ObservableCollection<SequenceVariantInfo> GoldSampleVariants
        {
            get { return goldSampleVariants; }
            set
            {
                goldSampleVariants = value;
                NotifyPropertyChanged();
            }
        }

        private void openSequenceDialog()
        {
#if !DEBUG
            if (sequenceIsModified)
            {
                var result = MTFMessageBox.Show("Save sequence", "Sequence is not saved.\n\nDo you want to save sequence?",
                    MTFMessageBoxType.Question, MTFMessageBoxButtons.YesNoCancel);
                if (result == MTFMessageBoxResult.Yes)
                {
                    SaveSequence();
                }
            }
#endif
            var openControl = new OpenSaveSequenceDialogContainer(DialogTypeEnum.OpenDialog, BaseConstants.SequenceBasePath, new List<string>() { BaseConstants.SequenceExtension }, true, false);
            PopupWindow.PopupWindow pw = new PopupWindow.PopupWindow(openControl);
            pw.Title = LanguageHelper.GetString("Mtf_OpenDialog_OpenSeq");
            if (pw.ShowDialog() == true)
            {
                sequenceWasModified = false;
                var result = ((OpenSaveSequencesDialogPresenter)openControl.DataContext).SelectedItem;
                openSequence(result.FullName);
                NotifyPropertyChanged("ViewMode");
            }
        }

        private bool reloadSequence = false;

        public void InvalidateSequence()
        {
            reloadSequence = true;
        }

        public override void Activated()
        {
            base.Activated();

            if (reloadSequence && SequenceIsNotRunning)
            {
                var editorPresenter = GetEditorPresenter();
                if (editorPresenter != null && !string.IsNullOrEmpty(editorPresenter.OpenedSequenceFileName))
                {
                    openSequence(editorPresenter.OpenedSequenceFileName, false);
                    editorPresenter.NotifySequenceIsActual();
                }
            }
            NotifyPropertyChanged("ShowTreeWithFullImage");
        }

        private void openSequence(string sequenceName)
        {
            openSequence(sequenceName, true);
        }

        private void openSequence(string sequenceName, bool invalidateSequenceInEditor)
        {
            treeViewManager.SwitchExtendedMode(false, ExtendedModeTypes.Debug);
            treeViewManager.SwitchExtendedMode(false, ExtendedModeTypes.Setup);
            sequenceState = MTFSequenceExecutionState.None;

            openedSequence = sequenceName;

            if (!CheckIfPossibleToExecuted())
            {
                MTFMessageBox.Show(LanguageHelper.GetString("Msg_Header_Error"), LanguageHelper.GetString("Msg_Body_SequenceNotStartModifiedEditor"), MTFMessageBoxType.Info, MTFMessageBoxButtons.Ok);
                return;
            }

            LongTask.Do(() =>
                        {
                            if (Sequence != null)
                            {
                                treeViewManager.SaveExtendedModeData(Sequence.Name, Sequence.Id);
                            }

                            reloadSequence = false;
                            UIHelper.InvokeOnDispatcher(InitUi);
                            UIHelper.InvokeOnDispatcher(()=>AllowSave(false));
                            UIHelper.InvokeOnDispatcher(treeViewManager.Clear);
                            SequenceVariables = new List<VariablesWatch>();
                            List<string> emptyCallActivites = new List<string>();
                            tableManager.ClearExistingTables();
                            sequenceNameList.Clear();
                            Sequence = LoadSequence(sequenceName, sequenceNameList);
                            if (Sequence == null)
                            {
                                return;
                            }

                            var currentVersion = new MTFClientServerCommon.Version(1, 8, 2, 0);

                            if (sequence.MTFVersion == null || sequence.MTFVersion < currentVersion)
                            {
                                if (sequence.ExternalSubSequences != null)
                                {
                                    var list = new List<MTFSequence> { sequence };
                                    list.AddRange(sequence.ExternalSubSequences.Select(x => x.ExternalSequence));
                                    VersionConvertHelper.ConvertCallActivities(list);
                                }
                            }

                            RefreshCommands();
                            tableManager.Init(Sequence, settings.IsTableCollapsed);
                            graphicalViewManager.Init(Sequence, tableManager);
                            InitDuts();
                            UIHelper.InvokeOnDispatcher(initializeClientContols);
                            UIHelper.InvokeOnDispatcher(OpenFirstActivClientUI);
                            UIHelper.InvokeOnDispatcher(InitDebug);
                            NotifyPropertyChanged("IsGSPanelEnabled");

                            try
                            {
                                MTFSequenceHelper.TransformActivitiesToExecution(Sequence, Sequence.MTFSequenceActivities, x => treeViewManager.AddToSource(x), true, 0, new List<Guid>() { sequence.Id }, emptyCallActivites);
                                treeViewManager.FillTree();
                            }
                            catch (Exception ex)
                            {
                                Sequence = null;
                                MTFMessageBox.Show(LanguageHelper.GetString("Msg_Header_Error"), ex.Message, MTFMessageBoxType.Error, MTFMessageBoxButtons.Ok);
                                return;
                            }
                            if (emptyCallActivites.Count > 0)
                            {
                                ShowEmptyExecuteSubSequences(emptyCallActivites);
                            }
                            NotifyPropertyChanged("HeaderText");
                            UIHelper.InvokeOnDispatcher(toggleSetupModeCommand.RaiseCanExecuteChanged);
                            IsSetupModeActive = false;

                            if (invalidateSequenceInEditor)
                            {
                                var sequenceEdirorPresenter = GetEditorPresenter();
                                if (sequenceEdirorPresenter != null && sequenceEdirorPresenter.OpenedSequenceFileName == openedSequence)
                                {
                                    sequenceEdirorPresenter.InvalidateSequence();
                                }
                            }

                            UIHelper.InvokeOnDispatcher(InitExtendedMode);
                            FillUserCommands();
                        }, LanguageHelper.GetString("Mtf_LongTask_OpeningSeq"));
            if (dontLoadedSequence != null)
            {
                NotifyDoNotLoadedSequences();
            }
        }

        private void NotifyDoNotLoadedSequences()
        {
            StringBuilder msg = new StringBuilder();
            msg.Append(LanguageHelper.GetString("Msg_Body_UnableLoadExetnal")).Append(Environment.NewLine);
            dontLoadedSequence.ForEach(x => { msg.Append(x).Append(Environment.NewLine); });
            MTFMessageBox.Show(LanguageHelper.GetString("Msg_Header_Error"), msg.ToString(), MTFMessageBoxType.Error, MTFMessageBoxButtons.Ok);
            dontLoadedSequence = null;
            sequence = null;
            MTFClient_SequenceExecutionStateChanged(MTFSequenceExecutionState.None);
        }

        private void InitExtendedMode()
        {
            treeViewManager.InitExtendedMode(Sequence.Name, Sequence.Id);
            if (treeViewManager.EnableSetup)
            {
                ActivateSetupMode(true);
            }
            if (treeViewManager.EnableDebug)
            {
                ActivateDebugMode(true);
            }
        }

        private void ActivateDebugMode(bool value)
        {
            IsDebugEnabled = value;
            if (ExecutionState == MTFSequenceExecutionState.Executing || ExecutionState == MTFSequenceExecutionState.ExecutionPreparation || ExecutionState == MTFSequenceExecutionState.Pause)
            {
                MTFClient.SetIsDebugMode(IsDebugEnabled);
            }
            if (ExecutionState == MTFSequenceExecutionState.Pause)
            {
                ChangeTableEditMode(IsDebugEnabled);
            }
            RefreshCommands();
        }

        private void ActivateSetupMode(bool value)
        {
            IsSetupModeActive = value;
            if (ExecutionState == MTFSequenceExecutionState.Executing || ExecutionState == MTFSequenceExecutionState.ExecutionPreparation || ExecutionState == MTFSequenceExecutionState.Pause)
            {
                MTFClient.SetIsSetupMode(IsSetupModeActive);
            }
            NotifyPropertyChanged("IsSetupModeActive");
        }


        private void initializeClientContols()
        {
            if (Sequence == null || Sequence.SequenceExecutionUiSetting == null || Sequence.SequenceExecutionUiSetting.SelectedClientUis == null)
            {
                return;
            }
            ClientUiHelper.InitCache(Sequence.SequenceExecutionUiSetting.SelectedClientUis.Select(i => new ClientContolInfo { AssemblyName = i.AssemblyName, TypeName = i.TypeName }));

            if (CurrentUiControl != null && viewMode == ExecutionViewMode.ClientUI)
            {
                var type = CurrentUiControl.GetType();
                CurrentUiControl = ClientUiHelper.GetControl(type.Module.Name, type.FullName);
            }
        }

        private void FillProgressEvents()
        {
            lock (progressEventsLock)
            {
                ActivityProgress = new ObservableCollection<ActivityProgressBase>
                                   {
                                       new ActivityProgressBar
                                       {
                                           Title = LanguageHelper.GetString("Execution_ActivityProgress"), Percent = 0
                                       }
                                   };
            }
            NotifyPropertyChanged("ActivityProgress");
        }

        private void startSequence()
        {
            if (!CheckIfPossibleToExecuted())
            {
                MTFMessageBox.Show(LanguageHelper.GetString("Msg_Header_Error"), LanguageHelper.GetString("Msg_Body_SequenceNotStartModifiedEditor"), MTFMessageBoxType.Info, MTFMessageBoxButtons.Ok);
                return;
            }
            var previousState = ExecutionState;
            if (ExecutionState != MTFSequenceExecutionState.Pause)
            {
                InitUi();
                treeViewManager.ClearResults();
                executingActivity = null;
                SelectedItem = null;
            }
            treeViewManager.Start();
            MTFClient_SequenceExecutionStateChanged(MTFSequenceExecutionState.ExecutionPreparation);
            MTFClient.StartSequence(openedSequence, IsSetupModeActive, treeViewManager.GetActiveSetupModes(), IsDebugEnabled, treeViewManager.GetActiveBreakPoints(), IsServiceActivated, IsTeachingActivated);
            if (previousState != MTFSequenceExecutionState.Pause)
            {
                if (IsDebugEnabled && VariablesInWatch != null)
                {
                    foreach (var watch in VariablesInWatch)
                    {
                        if (watch != null && watch.IsInWatch && watch.Variable != null)
                        {
                            SetVariableValueInWatchWithoutChangeFlag(watch.Variable.Id, watch.Variable.Value);
                        }
                    }
                }
            }
        }

        private void stopSequence()
        {
            Task.Run(() => { MTFClient.StopSequence(); });

            if (IsServiceActivated || IsTeachingActivated)
            {
                askChangeFromService = false;
                ViewMode = previousViewMode;
                askChangeFromService = true;
                AllowedServicecommands = null;
                RefreshViewModes();
            }
        }

        private void pauseSequence()
        {
            Task.Run(() => { MTFClient.PauseSequence(); });
        }

        private bool isSetupModeActive;

        public bool IsSetupModeActive
        {
            get { return isSetupModeActive; }
            set
            {
                isSetupModeActive = value;
                NotifyPropertyChanged();
            }
        }

        public bool ShowTreeWithFullImage
        {
            get { return settings.ShowTreeWithFullImage; }
        }

        public bool IsGSPanelEnabled
        {
            get { return Sequence != null && Sequence.ServiceDesignSetting != null && Sequence.ServiceDesignSetting.AllowGSPanel; }
        }

        private void toggleSetupMode()
        {
            treeViewManager.SaveExtendedModeData(Sequence.Name, Sequence.Id, ExtendedModeTypes.Setup);

            IsSetupModeActive = !IsSetupModeActive;

            treeViewManager.SwitchExtendedMode(IsSetupModeActive, ExtendedModeTypes.Setup);


            if (ExecutionState == MTFSequenceExecutionState.Executing || ExecutionState == MTFSequenceExecutionState.ExecutionPreparation || ExecutionState == MTFSequenceExecutionState.Pause)
            {
                MTFClient.SetIsSetupMode(IsSetupModeActive);
            }

            NotifyPropertyChanged("IsSetupModeActive");
        }

        private void ClearStatus()
        {
            if (DutPresenters == null)
                return;
            foreach (var dutPresenter in DutPresenters)
            {
                dutPresenter.ClearStatus();
            }
        }

        private void ShowEmptyExecuteSubSequences(List<string> list)
        {
            StringBuilder sb = new StringBuilder();
            list.ForEach(x => sb.Append(x).Append(Environment.NewLine));
            sb.Append(Environment.NewLine).Append(LanguageHelper.GetString("Msg_Body_MissingRef"));
            Task.Run(() => MTFMessageBox.Show(LanguageHelper.GetString("Msg_Header_MissingRef"), sb.ToString(), MTFMessageBoxType.Warning, MTFMessageBoxButtons.Ok));
        }

        public bool IsPausedInDebug
        {
            get { return IsDebugEnabled && (ExecutionState == MTFSequenceExecutionState.Pause || ExecutionState == MTFSequenceExecutionState.DebugGoToNewPosition || ExecutionState == MTFSequenceExecutionState.DebugGoToTopPosition); }
        }

        
        public void AccesskeyChanged(MTFClientServerCommon.MTFAccessControl.AccessKey accessKey)
        {
            ClientUiHelper.OnAccessKeyChanged(accessKey);

            IsSetupModeActive = false;

            if (IsDebugEnabled)
            {
                Task.Run(() => IsDebugEnabled = false);
            }

            this.RefreshCommands();
            RefreshUserCommandsAccessibility();
        }

        private void RefreshCommands()
        {
            generateCommands();

            NotifyPropertyChanged("Commands");
        }

        private void RefreshViewModes()
        {
            NotifyPropertyChanged("IsShowTreeActivated");
            NotifyPropertyChanged("IsShowTableActivated");
            //NotifyPropertyChanged("IsShowTimeActivated");
            NotifyPropertyChanged("IsShowGraphicalViewActivated");
            NotifyPropertyChanged("IsServiceActivated");
            NotifyPropertyChanged("IsTeachingActivated");
            RefreshCommands();
            IsShowPicture = false;

            if (DetailMode == ExecutionDetailModes.Image)
            {
                SwitchImageDetailMode(false);
            }
        }

        public bool SequenceIsNotRunning
        {
            get { return ExecutionState == MTFSequenceExecutionState.Finished || ExecutionState == MTFSequenceExecutionState.Aborted || ExecutionState == MTFSequenceExecutionState.None || ExecutionState == MTFSequenceExecutionState.Stopped; }
        }


        public ControlStatus ControlStatus
        {
            get { return controlStatus; }
            set
            {
                controlStatus = value;
                NotifyPropertyChanged();
            }
        }

        public ImageHandler ImageHandler
        {
            get { return imageHandler; }
        }

        public TreeViewManager TreeViewManager
        {
            get { return treeViewManager; }
        }

        public GraphicalViewManager GraphicalViewManager
        {
            get { return graphicalViewManager; }
        }

        public ICommand OpenTableCommand
        {
            get { return new Command(OpenTable); }
        }

        public void OpenAllNokImages(ExecutionValidTable executionValidTable)
        {
            if (executionValidTable != null)
            {
                var images = executionValidTable.GetAllNokImages();

                OpenSelectedTable(executionValidTable);

                if (images != null && images.Count > 0)
                {
                    ImageHandler.ShowTableImages(images.Select(x => x.ImageData).ToList());
                }
            }
        }

        private void OpenTable(object param)
        {
            var item = param as ExecutionGraphicalViewTestedItem;
            if (item != null && item.ValidationTable != null)
            {
                ShowTableView();
                OpenSelectedTable(item.ValidationTable);
            }
        }

        private void OpenSelectedTable(ExecutionValidTable executionValidTable)
        {
            if (executionValidTable != null)
            {
                tableManager.OpenExecutionTable(executionValidTable);
                SwitchImageDetailMode(false);
            }
        }

        public void CloseExtendedMode()
        {
            if (Sequence != null)
            {
                treeViewManager.SaveExtendedModeData(Sequence.Name, Sequence.Id);
            }
            treeViewManager.SwitchExtendedMode(false, ExtendedModeTypes.Debug);
            treeViewManager.SwitchExtendedMode(false, ExtendedModeTypes.Setup);
        }

        private IEnumerable<MTFUserCommandWrapper> userCommands;
        public IEnumerable<MTFUserCommandWrapper> UserCommands
        {
            get => userCommands;
            set
            {
                userCommands = value; 
                NotifyPropertyChanged();
            }
        }

        private void FillUserCommands()
        {
            UserCommands = Sequence?.UserCommands?.Select(c => new MTFUserCommandWrapper {Command = c}).ToArray();
        }

        private void ResetUserCommands()
        {
            if (UserCommands == null)
            {
                return;
            }
            foreach (var userCommand in UserCommands)
            {
                if (userCommand.Command.Type == MTFUserCommandType.Button || userCommand.Command.Type == MTFUserCommandType.ToggleButton)
                {
                    userCommand.IsEnabled = false;
                    userCommand.IsChecked = false;
                }
                userCommand.IndicatorValue = false;
            }
        }

        private IEnumerable<UserCommandsState> sequenceUserCommandsStates;

        private void RefreshUserCommandsAccessibility()
        {
            if (userCommands == null)
                return;

            var isExecuting = sequenceUserCommandsStates?.Any(c => c.IsExecuting) ?? false;
            foreach (var userCommand in UserCommands.Where(c => c.Command.Type == MTFUserCommandType.Button || c.Command.Type == MTFUserCommandType.ToggleButton))
            {
                var sequenceCommandSettings = sequenceUserCommandsStates?.FirstOrDefault(sc => sc.UserCommand.Id == userCommand.Command.Id);
                //evaluate access rights
                bool accessGranted = false;
                switch (userCommand.Command.AccessRole)
                {
                    case MTFUserCommandAccessRole.EveryOne: accessGranted = true;
                        break;
                    case MTFUserCommandAccessRole.LoggedUser: accessGranted = !EnvironmentHelper.IsProductionMode;
                        break;
                    case MTFUserCommandAccessRole.ServiceRole: accessGranted = EnvironmentHelper.HasAccessKeyRole("Service");
                        break;
                }

                if (sequenceCommandSettings != null)
                {
                    if (userCommand.Command.Type == MTFUserCommandType.ToggleButton)
                    {
                        userCommand.IsChecked = sequenceCommandSettings.IsChecked == true;
                    }

                    userCommand.IsEnabled = !isExecuting && accessGranted && sequenceCommandSettings.IsEnabled == true;
                }
            }
        }

        private void InitDuts()
        {
            DutPresenters = sequence.DeviceUnderTestInfos == null || sequence.DeviceUnderTestInfos.Count == 0
                ? new List<DutPresenter>  {new DutPresenter {DeviceUnderTest = null, ValidationTables = tableManager.ValidationTables(null), GraphicalView = graphicalViewManager.GetFirstGraphicalView(null)}}
                : sequence.DeviceUnderTestInfos.Select(d => new DutPresenter
                {
                    DeviceUnderTest = d, ValidationTables = tableManager.ValidationTables(d.Id),
                    GraphicalView = graphicalViewManager.GetGraphicalView(d.StartupGraphicalView?.Id, d.Id)
                }).ToList();

            DefaultDutPresenter = DutPresenters.First();
            NotifyPropertyChanged("DutRows");
            NotifyPropertyChanged("DutColumns");
            NotifyPropertyChanged("DutPresenters");
            NotifyPropertyChanged("DutsMoreThanOne");
        }

        public int DutColumns =>
            sequence?.DeviceUnderTestInfos == null || sequence.DeviceUnderTestInfos.Count == 0
                ? 0 : Convert.ToInt32(Math.Ceiling(Math.Sqrt(sequence.DeviceUnderTestInfos.Count)));
        public int DutRows =>
            sequence?.DeviceUnderTestInfos == null || sequence.DeviceUnderTestInfos.Count == 0
                ? 1 : Convert.ToInt32(Math.Ceiling((decimal)sequence.DeviceUnderTestInfos.Count / DutColumns));

        public IList<DutPresenter> DutPresenters { get; private set; }

        public bool DutsMoreThanOne => sequence?.DeviceUnderTestInfos != null && sequence.DeviceUnderTestInfos.Count != 0 && sequence.DeviceUnderTestInfos.Count > 1;

        private DutPresenter GetDut(Guid? dutId)
        {
            if (dutId == null)
            {
                return DutPresenters.First();
            }

            var dut = DutPresenters.FirstOrDefault(d => d.DeviceUnderTest.Id == dutId);
            return dut ?? GetDut(null);
        }

        private DutPresenter defaultDutPresenter;

        public DutPresenter DefaultDutPresenter
        {
            get => defaultDutPresenter;
            set
            {
                defaultDutPresenter = value; 
                NotifyPropertyChanged();
            }
        }
    }
}
