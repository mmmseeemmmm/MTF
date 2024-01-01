//#define UseNewList

using Microsoft.Win32;
using MTFApp.ExportSequence;
using MTFApp.ImportSequence;
using MTFApp.MergeSequences;
using MTFApp.UIHelpers;
using MTFClientServerCommon;
using MTFClientServerCommon.Helpers;
using MTFClientServerCommon.Mathematics;
using MTFClientServerCommon.MTFTable;
using MTFClientServerCommon.MTFValidationTable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using AutomotiveLighting.MTFCommon;
using MTFApp.FindUsages;
using MTFApp.Managers;
using MTFApp.Managers.Components;
using MTFApp.OpenSaveSequencesDialog;
using MTFApp.SequenceEditor.Handlers;
using MTFApp.SequenceExecution;
using MTFApp.ServerService;
using MTFApp.ServerService.Components;
using MTFApp.UIHelpers.DragAndDrop;
using MTFApp.UIHelpers.Editors;
using MTFApp.UIHelpers.LongTask;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.GoldSamplePersist;
using MTFClientServerCommon.GraphicalView;
using Version = MTFClientServerCommon.Version;

namespace MTFApp.SequenceEditor
{
    public class SequenceEditorPresenter : PresenterBase, IMainCommands, IHeaderText
    {
        private MTFSequenceClassInfo selectedSequenceClassInfo;
        private MTFVariable selectedVariable;
        private MTFServiceCommand selectedServiceCommand;
        private MTFUserCommand selectedUserCommand;
        private object lastSelectedItem;
        private MTFSequenceActivity selectedSequenceActivity;
        private readonly Command removeActivityFromSequenceCommand;
        private readonly Command removeClassFromSequenceCommand;
        private readonly Command removeVariableFromSequenceCommand;
        private readonly Command removeServiceCommandFromSequenceCommand;
        private readonly Command removeUserCommandFromSequenceCommand;
        private readonly Command copyVariableCommand;
        private readonly Command pasteVariableCommand;
        private TouchHelper touch = TouchHelper.Instance;
        //private Command addAvailableComponentsCommand = null;
        private Command cleanSearchTextCommand;
        //private bool addAvailableComponentsButtonEnable = true;
        private MTFSequence sequence;
        private MTFSequence mainSequence;
        private readonly ObservableCollection<MTFSequence> mtfProject = new ObservableCollection<MTFSequence>();
        private string sequenceFullName = string.Empty;
        private List<MTFClassInfo> availableClasses;
        //private readonly List<MTFSequenceActivity> selectedActivities = new List<MTFSequenceActivity>();
        private readonly SelectionHelper selectionHelper;
        private readonly ComponentDataHandler componentDataHandler;
        private bool cancelSaving = false;
        private bool disableDragDrop = false;
        private bool enableSearch = false;
        private readonly DispatcherTimer searchTimer = new DispatcherTimer();
        private bool showTermDesigner;
        private SequenceVariant actualVariant;
        private bool sequenceWasModified;
        private bool allowEditSequence = true;
        private FindUsagesBase findUsagesWindow;
        private bool isFindUsagesWindowOpened;
        private bool isFindUsagesCreated;
        private ListBox lastSelectedListBox;
        private SettingsClass mtfSetting;
        private bool isDifferentControlOpened;
        private EditorViewMode editorViewMode = EditorViewMode.None;
        private bool reloadSequence;
        private bool reloadSequenceAfterStart = false;
        private readonly ActivityNavigationHandler navigationHandler = new ActivityNavigationHandler();

        private Command createNewSequenceCommand;
        private Command openSequenceDialogCommand;
        private Command addNewSequenceCommand;
        private Command addExistSequenceCommand;
        private Command saveSequenceCommand;
        private Command saveSequenceAsCommand;
        private Command saveAllCommand;
        //private Command mergeCommand;
        private Command importCommand;
        private Command exportCommand;
        //private Command setVariantCommand;
        private ToggleCommand sequenceSettingsCommand;
        private Command openFromAutoBackupCommand;
        private ToggleCommand findUsagesCommand;
        private Command backCommand;
        private Command graphicalViewCommand;

        private readonly ComponentsClient componentsClient;

        private readonly DispatcherTimer EachMinuteTimer = new DispatcherTimer();

        public bool VisibleNewList
        {
            get
            {
#if UseNewList
    return true;            
#else
                return false;
#endif
            }
        }



        private readonly List<Type> variableCategories = new List<Type>()
                                                         {
                                                             typeof(int),
                                                             typeof(bool),
                                                             typeof(string),
                                                             typeof(float),
                                                             typeof(double),
                                                             typeof(long),
                                                             typeof(ushort),
                                                             typeof(MTFValidationTable),
                                                             typeof(MTFConstantTable)
                                                         };

        private readonly List<string> variableTypeToChange = new List<string>()
                                                             {
                                                                 typeof(int).FullName,
                                                                 typeof(bool).FullName,
                                                                 typeof(string).FullName,
                                                                 typeof(float).FullName,
                                                                 typeof(double).FullName,
                                                                 typeof(long).FullName,
                                                                 typeof(ushort).FullName
                                                             };

        public string OpenedSequenceFileName => mainSequence.FullPath;

        public ObservableCollection<MTFSequence> MTFProject => mtfProject;

        private MainWindowPresenter MainWindowPresenter => (MainWindowPresenter)Application.Current.MainWindow.DataContext;

        public SequenceEditorPresenter()
        {
            componentsClient = ServiceClientsContainer.Get<ComponentsClient>();
            LoadSettingAsync();
            createCommands();
            searchTimer.Interval = new TimeSpan(0, 0, 1);
            searchTimer.Tick += (s, e) =>
                                {
                                    if (enableSearch)
                                    {
                                        Search(searchText);
                                        searchTimer.Stop();
                                    }
                                };

            EachMinuteTimer.Interval = new TimeSpan(0, 1, 0);
            EachMinuteTimer.Tick += (s, a) => EachMinuteTask();
            EachMinuteTimer.Start();

            CreateNewSequence(true);
            FillCommands();

            removeClassFromSequenceCommand = new Command(removeClassFromSequence,
                () => SelectedSequenceClassInfo != null);

            removeVariableFromSequenceCommand = new Command(removeVariableFromSequence,
                () => SelectedVariable != null);

            copyVariableCommand = new Command(copyVariable, () => SelectedVariable != null);

            pasteVariableCommand = new Command(pasteVariable, () => !MTFClipboard.IsEmpty());

            removeServiceCommandFromSequenceCommand = new Command(RemoveServiceCommandFromSequence,
                () => SelectedServiceCommand != null);

            removeUserCommandFromSequenceCommand = new Command(RemoveUserCommandFromSequence,
                () => SelectedUserCommand != null);

            removeActivityFromSequenceCommand = new Command(removeActivityFromSequence);

            //addAvailableComponentsCommand = new Command(addAvailableComponents, () => addAvailableComponentsButtonEnable);
            cleanSearchTextCommand = new Command(() => { SearchText = string.Empty; }, () => !string.IsNullOrEmpty(searchText));

            var sequenceExecutionPresenter = ((MainWindowPresenter)App.Current.MainWindow.DataContext).SequenceExecutionPresenter;

            LoadAvailableClassesAsync(sequenceExecutionPresenter);

            if (sequenceExecutionPresenter != null && !string.IsNullOrEmpty(sequenceExecutionPresenter.OpenedSequence))
            {
                reloadSequenceAfterStart = true;
            }

            selectionHelper = new SelectionHelper(this);
            componentDataHandler = new ComponentDataHandler(this);
            //OpenSequenceAutomaticaly();
        }

        private async void LoadSettingAsync()
        {
            await Task.Run(() => mtfSetting = StoreSettings.GetInstance.SettingsClass);
        }

        public bool MtfEditorIsCollapsed
        {
            get { return mtfSetting != null && mtfSetting.CollapsedParametersInEditor; }
        }

        private async void OpenSequenceAutomaticaly()
        {
            await Task.Run(() => System.Threading.Thread.Sleep(500));
            //openSequence("ExecuteTerm.sequence", false);
            //ImportSequence();
            //OpenGraphicalView();
            //openSequence("Develop\\ExecuteActitiy\\Test0.sequence", false);
            // openSequence("Develop\\ExecuteActitiy\\Transform0.sequence", false);
            //openSequence("Develop\\ExecuteActitiy\\Vymazat0.sequence", false);
        }

        private int backupCounter;
        private bool previousBackupFailed = false;

        private void EachMinuteTask()
        {
            if (!MainWindowPresenter.IsSequenceEditorActive)
            {
                return;
            }

            if (mtfSetting != null && mtfSetting.BackupEnabled)
            {
                backupCounter++;
                if (previousBackupFailed || backupCounter >= StoreSettings.GetInstance.SettingsClass.BackupPeriod)
                {
                    backupCounter = 0;
                    previousBackupFailed = false;
                    CreateBackup();
                    CleanOldBackups();
                }
            }
        }

        private void createCommands()
        {
            createNewSequenceCommand = new Command(() => CreateNewSequence(true), () => !IsDifferentControlOpened && AllowEditSequence) { Name = "MainCommand_New", Icon = MTFIcons.NewFile, KeyShortucuts = new[] { new CommandShortcut { Key = Key.N, Modifier = ModifierKeys.Control } } };
            openSequenceDialogCommand = new Command(() => OpenSequenceDialog(false), () => !IsDifferentControlOpened) { Name = "MainCommand_Open", Icon = MTFIcons.OpenFile, KeyShortucuts = new[] { new CommandShortcut { Key = Key.O, Modifier = ModifierKeys.Control } } };
            addNewSequenceCommand = new Command(() => CreateNewSequence(false), () => !IsDifferentControlOpened && AllowEditSequence) { Name = "MainCommand_AddNew", Icon = MTFIcons.AddNew };
            addExistSequenceCommand = new Command(() => OpenSequenceDialog(true), () => !IsDifferentControlOpened && AllowEditSequence) { Name = "MainCommand_AddExist", Icon = MTFIcons.AddExisting };
            saveSequenceCommand = new Command(() => PerformSave(sequence, false), () => AllowEditSequence) { Name = "MainCommand_Save", Icon = MTFIcons.SaveFile, KeyShortucuts = new[] { new CommandShortcut { Key = Key.S, Modifier = ModifierKeys.Control } } };
            saveSequenceAsCommand = new Command(() => PerformSave(sequence, true), () => !IsDifferentControlOpened && AllowEditSequence) { Name = "MainCommand_SaveAs", Icon = MTFIcons.SaveFileAs };
            saveAllCommand = new Command(() => SaveAll(), () => !IsDifferentControlOpened && AllowEditSequence) { Name = "MainCommand_SaveAll", Icon = MTFIcons.SaveAll, KeyShortucuts = new[] { new CommandShortcut { Key = Key.S, Modifier = (ModifierKeys.Control | ModifierKeys.Shift) } } };
            //mergeCommand = new Command(() => MergeSequence(sequence), () => !IsSettingsOpened && AllowEditSequence) { Name = "Merge", Icon = MTFIcons.Merge };
            importCommand = new Command(ImportSequence, () => !IsDifferentControlOpened && AllowEditSequence) { Name = "MainCommand_Import", Icon = MTFIcons.Import };
            exportCommand = new Command(ExportSequence, () => !IsDifferentControlOpened && AllowEditSequence) { Name = "MainCommand_Export", Icon = MTFIcons.Export };
            //setVariantCommand = new Command(() => SetActualVariant(), () => !IsSettingsOpened && AllowEditSequence) { Name = "MainCommand_SetVariant", Icon = MTFIcons.Variant };
            sequenceSettingsCommand = new ToggleCommand(OpenSetting, () => !IsDifferentControlOpened && AllowEditSequence, PropertyInfoHelper.GetPropertyName(() => IsSettingsOpened)) { Name = "MainCommand_Settings", Icon = MTFIcons.Settings };
            graphicalViewCommand = new ToggleCommand(OpenGraphicalView, () => !IsDifferentControlOpened && AllowEditSequence, PropertyInfoHelper.GetPropertyName(() => IsGraphicalViewOpened)) { Name = "MainCommand_GraphicalView", Icon = MTFIcons.GraphicalView };
            findUsagesCommand = new ToggleCommand(toggleIsFindUsagesOpened, () => AllowEditSequence, PropertyInfoHelper.GetPropertyName(() => IsFindUsagesWindowOpened)) { Name = "MainCommand_FindUsages", Icon = MTFIcons.Settings };
            backCommand = new Command(SwichDifferentControl, () => AllowEditSequence) { Name = "MainCommand_Back", Icon = MTFIcons.Back };
            openFromAutoBackupCommand = new Command(OpenBackup, () => !IsDifferentControlOpened && AllowEditSequence) { Name = "MainCommand_OpenBackup", Icon = MTFIcons.OpenFile };

        }


        private async void LoadAvailableClassesAsync(SequenceExecution.SequenceExecutionPresenter sequenceExecutionPresenter)
        {
            await Task.Run(() => availableClasses = new List<MTFClassInfo>(ServiceClientsContainer.Get<ComponentsClient>().AvailableMonsterClasses()));
            enableSearch = true;
        }

        private List<Command> mainCommands;

        IEnumerable<Command> IMainCommands.Commands()
        {
            GenerateMainCommands();
            return mainCommands;
        }

        private void GenerateMainCommands()
        {
            mainCommands = new List<Command>()
                           {
                               createNewSequenceCommand,
                               openSequenceDialogCommand,
                               addNewSequenceCommand,
                               addExistSequenceCommand,
                               saveSequenceCommand,
                               saveSequenceAsCommand,
                               saveAllCommand,
                               //mergeCommand,
                               importCommand,
                               exportCommand,
                               //setVariantCommand,
//#if DEBUG            
//                  new Command(() => showDebugInfo()) { Name = "Debug Info", Icon = AutomotiveLighting.MTFCommon.MTFIcons.Bug},
//#endif
                           };
            if (IsMainSequence)
            {
                mainCommands.Add(sequenceSettingsCommand);
                mainCommands.Add(graphicalViewCommand);
            }
            mainCommands.Add(openFromAutoBackupCommand);
            if (isFindUsagesCreated)
            {
                mainCommands.Add(findUsagesCommand);
            }
            if (IsDifferentControlOpened)
            {
                mainCommands.Add(backCommand);
            }
        }

        private void SwichDifferentControl()
        {
            IsDifferentControlOpened = !IsDifferentControlOpened;
            GenerateMainCommands();
            mainCommands.ForEach(c => c.RaiseCanExecuteChanged());
            EditorViewMode = EditorViewMode.None;
            NotifyPropertyChanged("Commands");
        }

        private void OpenSetting()
        {
            LoadAllSequences();
            SwichDifferentControl();
            EditorViewMode = EditorViewMode.Setting;
        }

        public bool IsSettingsOpened => EditorViewMode == EditorViewMode.Setting;

        private void OpenGraphicalView()
        {
            LoadAllSequences();
            SwichDifferentControl();
            EditorViewMode = EditorViewMode.GraphicalView;
        }

        public bool IsGraphicalViewOpened => EditorViewMode == EditorViewMode.GraphicalView;

        public bool IsDifferentControlOpened
        {
            get => isDifferentControlOpened;
            set
            {
                isDifferentControlOpened = value;
                NotifyPropertyChanged();
            }
        }

        public EditorViewMode EditorViewMode
        {
            get => editorViewMode;
            set
            {
                editorViewMode = value;
                NotifyPropertyChanged();
            }
        }




        public ICommand AddRoundingRuleCommand
        {
            get { return new Command(addRoundingRule); }
        }

        public ICommand RemoveRoundingRuleCommand
        {
            get { return new Command(removeRoundingRule); }
        }

        private void addRoundingRule()
        {
            if (MainSequence.RoundingRules == null)
            {
                MainSequence.RoundingRules = new MTFObservableCollection<RoundingRule>();
            }

            MainSequence.RoundingRules.Add(new RoundingRule());
        }

        private void removeRoundingRule(object param)
        {
            var roundingRule = param as RoundingRule;
            if (roundingRule == null)
            {
                return;
            }

            MainSequence.RoundingRules.Remove(roundingRule);
        }

        private void showDebugInfo()
        {
            Mouse.SetCursor(Cursors.Wait);
            StringBuilder sb = new StringBuilder();
            Task.Run(() =>
                     {
                         sb.Append("Terms: ").Append(sequence.GetObjectCount<Term>()).AppendLine();
                         sb.Append("Activities: ").Append(sequence.GetObjectCount<MTFSequenceActivity>()).AppendLine();
                         sb.Append("DTOs: ").Append(sequence.GetObjectCount<MTFDataTransferObject>()).AppendLine();
                     }).Wait();
            Mouse.SetCursor(Cursors.Arrow);

            MTFMessageBox.Show("Debug Info", sb.ToString(), MTFMessageBoxType.Info, MTFMessageBoxButtons.Ok);
        }

        public string HeaderText
        {
            get
            {
                if (mtfProject.Count > 0)
                {
                    return mtfProject.First().Name;
                }
                return string.Empty;
            }
        }

        public ICommand AddServiceCommandCommand
        {
            get { return new Command(addServiceCommand); }
        }

        public ICommand AddUserCommandCommand
        {
            get { return new Command(addUserCommand); }
        }

        private void CreateNewSequence(bool createMainSequence)
        {
#if !DEBUG
            if (AcceptSave())
            {
                return;
            }
#endif
            if (createMainSequence)
            {
                mtfProject.Clear();
                mainSequence = null;
                sequenceWasModified = false;
            }
            sequence = new MTFSequence();
            sequence.PropertyChanged += sequence_PropertyChanged;
            sequence.Name = GenerateNewName();
            sequence.MTFSequenceClassInfos = new MTFObservableCollection<MTFSequenceClassInfo>();
            sequence.MTFSequenceActivities = new MTFObservableCollection<MTFSequenceActivity>();
            sequence.ActivitiesByCall = new MTFObservableCollection<MTFSequenceActivity>();
            sequence.MTFVariables = new MTFObservableCollection<MTFVariable>();
            sequence.ExternalSubSequences = new List<MTFExternalSequenceInfo>();
            sequence.ComponetsMapping = new Dictionary<Guid, Guid>();
            sequence.IsLoad = true;
            sequence.ParentSequence = mainSequence;
            LastSelectedItem = null;
            sequenceFullName = string.Empty;
            mtfProject.Add(sequence);
            mainSequence = mtfProject.First();
            if (!createMainSequence)
            {
                if (mainSequence.ExternalSubSequences == null)
                {
                    mainSequence.ExternalSubSequences = new List<MTFExternalSequenceInfo>();
                }
                mainSequence.ExternalSubSequences.Add(new MTFExternalSequenceInfo(sequence));
                mainSequence.IsModified = true;
            }
            ShowTermDesigner = false;
            NotifyPropertyChanged("Sequence");
            NotifyPropertyChanged("HeaderText");
            NotifyPropertyChanged("DisplaySubComponents");
            NotifyPropertyChanged("Commands");
            NotifyPropertyChanged("MainSequence");
        }

        private bool AcceptSave()
        {
            if (IsModified && !IsNew)
            {
                var result = MTFMessageBox.Show("Save sequence", "Sequence is not saved.\n\nDo you want to save sequence?",
                    MTFMessageBoxType.Question, MTFMessageBoxButtons.YesNoCancel);
                if (result == MTFMessageBoxResult.Cancel)
                {
                    return true;
                }
                if (result == MTFMessageBoxResult.Yes)
                {
                    SaveAll();
                }
            }
            return false;
        }

        public bool IsNew
        {
            get
            {
                return mtfProject != null && mtfProject.Count == 1 && Sequence.ActivitiesByCall.Count == 0
                       && Sequence.MTFSequenceActivities.Count == 0 && Sequence.MTFSequenceClassInfos.Count == 0
                       && Sequence.MTFVariables.Count == 0;
            }
        }


        public bool DisableDragDrop
        {
            get { return disableDragDrop; }
            set { disableDragDrop = value; }
        }


        private string GenerateNewName()
        {
            string baseName = "NewSequence";
            if (mtfProject.Count == 0)
            {
                return baseName;
            }

            var sequencesWithBaseName = mtfProject.Where(x => x.Name.StartsWith(baseName));
            int i = 1;
            string newName = baseName;
            while (sequencesWithBaseName.Any(x => x.Name == newName))
            {
                newName = baseName + "_" + i++;
            }
            return newName;
        }

        void sequence_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "MTFSequenceClassInfos.Alias")
            {
                var mtfSequenceClassInfo = lastSelectedItem as MTFSequenceClassInfo;
                if (mtfSequenceClassInfo != null)
                {
                    mtfSequenceClassInfo.AdjustName(sequence.MTFSequenceClassInfos);
                }
            }
            else if (e.PropertyName.Contains("MTFVariables.Name"))
            {
                var variable = lastSelectedItem as MTFVariable;
                if (variable != null)
                {
                    variable.AdjustName(sequence.MTFVariables);
                    var tmp = variable;
                    int sourceId = sequence.MTFVariables.IndexOf(tmp);
                    int targetId = sequence.MTFVariables.GetTargetIdOf(tmp);
                    if (sourceId != targetId)
                    {
                        sequence.MTFVariables.Move(sourceId, targetId);
                    }
                }
            }
            else if (e.PropertyName.Contains("ActivityName"))
            {
                var activity = lastSelectedItem as MTFSequenceActivity;
                if (activity != null)
                {
                    activity.UniqueIndexer = 0;
                    activity.AdjustName();
                }
            }
            else if (e.PropertyName == "ServiceCommands.Name")
            {
                var command = lastSelectedItem as MTFServiceCommand;
                if (command != null)
                {
                    command.AdjustName(sequence.ServiceCommands);
                }
            }
        }

        public List<Type> VariableCategories
        {
            get { return variableCategories; }
        }

        public List<string> VariableTypeToChange
        {
            get { return variableTypeToChange; }
        }

        public IList<MTFClassCategory> ClassCategories => ManagersContainer.Get<ComponentsManager>().MTFClassCategories;


        public MTFVariable SelectedVariable
        {
            get { return selectedVariable; }
            set
            {
                selectedVariable = value;
                LastSelectedItem = null;
                removeVariableFromSequenceCommand.RaiseCanExecuteChanged();
                copyVariableCommand.RaiseCanExecuteChanged();
                pasteVariableCommand.RaiseCanExecuteChanged();
                LastSelectedItem = value;
            }
        }

        private bool isSelectedMainSequenceComponent;

        public bool IsSelectedMainSequenceComponent
        {
            get { return isSelectedMainSequenceComponent; }
            set
            {
                isSelectedMainSequenceComponent = value;
                NotifyPropertyChanged();
            }
        }

        public bool ComponentFromMainSequence(MTFSequenceClassInfo classInfo)
        {
            return mainSequence.MTFSequenceClassInfos.Contains(classInfo);
        }

        public MTFSequenceClassInfo SelectedSequenceClassInfo
        {
            get { return selectedSequenceClassInfo; }
            set
            {
                DataHandler.SetSelectedSequenceClassInfo(value);
                if (IsMainSequence)
                {
                    IsSelectedMainSequenceComponent = value != null && sequence != null && ComponentFromMainSequence(value);
                    if (IsSelectedMainSequenceComponent)
                    {
                        DataHandler.FillSequenceDataAsync(value);
                    }
                }
                else
                {
                    IsSelectedMainSequenceComponent = true;
                }
                var tmp1 = selectedSequenceClassInfo;
                MTFClassInstanceConfiguration tmp = null;
                if (tmp1 != null)
                {
                    tmp = selectedSequenceClassInfo.MTFClassInstanceConfiguration;
                }
                selectedSequenceClassInfo = value;

                LastSelectedItem = null;
                removeClassFromSequenceCommand.RaiseCanExecuteChanged();
                NotifyPropertyChanged("ClassInstanceConfiurations");
                if (tmp1 != null)
                {
                    tmp1.MTFClassInstanceConfiguration = tmp;
                }


                LastSelectedItem = value;

                NotifyPropertyChanged("MappedComponent");
            }
        }

        private List<MTFClassInstanceConfiguration> classInstanceConfiurations;

        public List<MTFClassInstanceConfiguration> ClassInstanceConfiurations
        {
            get
            {
                if (selectedSequenceClassInfo == null)
                {
                    return null;
                }

                classInstanceConfiurations = componentsClient.ClassInstanceConfigurations(selectedSequenceClassInfo.MTFClass);

                if (selectedSequenceClassInfo.MTFClassInstanceConfiguration != null)
                {
                    var selected = classInstanceConfiurations.
                        FirstOrDefault(cfg => cfg.Id == selectedSequenceClassInfo.MTFClassInstanceConfiguration.Id);

                    selectedSequenceClassInfo.SetMTFClassInstanceConfigurationWithoutModification(selected);
                    //selectedSequenceClassInfo.MTFClassInstanceConfiguration = selected;
                }

                return classInstanceConfiurations;
            }
        }

        public MTFSequenceClassInfo MappedComponent
        {
            get
            {
                if (selectedSequenceClassInfo != null && mainSequence.ComponetsMapping.ContainsKey(selectedSequenceClassInfo.Id))
                {
                    return
                        mainSequence.MTFSequenceClassInfos.FirstOrDefault(
                            x => x.Id == mainSequence.ComponetsMapping[selectedSequenceClassInfo.Id]);
                }
                return null;
            }
            set { SetMapping(value, selectedSequenceClassInfo); }
        }

        public void SetMapping(MTFSequenceClassInfo componentFromMainSequence, MTFSequenceClassInfo subComponent)
        {
            if (componentFromMainSequence != null && subComponent != null)
            {
                if (subComponent.IsMapped && mainSequence.ComponetsMapping.ContainsKey(subComponent.Id))
                {
                    var componentInMainSequence =
                        mainSequence.MTFSequenceClassInfos.FirstOrDefault(x => x.Id == mainSequence.ComponetsMapping[subComponent.Id]);
                    if (componentInMainSequence != null && componentInMainSequence.SubComponents != null)
                    {
                        componentInMainSequence.SubComponents.Remove(subComponent);
                    }
                }
                mainSequence.ComponetsMapping[subComponent.Id] = componentFromMainSequence.Id;
                if (componentFromMainSequence.SubComponents == null)
                {
                    componentFromMainSequence.SubComponents = new ObservableCollection<MTFSequenceClassInfo>();
                }

                componentFromMainSequence.SubComponents.Add(subComponent);
                subComponent.IsMapped = true;
                NotifyPropertyChanged("SubSequenceComponents");
                NotifyPropertyChanged("MTFSequenceClassInfos");
                NotifyPropertyChanged("MappedComponent");
            }
        }

        public void RemoveMapping(MTFSequenceClassInfo subComponent)
        {
            if (subComponent != null)
            {
                if (subComponent.IsMapped && mainSequence.ComponetsMapping.ContainsKey(subComponent.Id))
                {
                    var componentInMainSequence =
                        mainSequence.MTFSequenceClassInfos.FirstOrDefault(x => x.Id == mainSequence.ComponetsMapping[subComponent.Id]);
                    if (componentInMainSequence != null && componentInMainSequence.SubComponents != null)
                    {
                        componentInMainSequence.SubComponents.Remove(subComponent);
                    }
                }
                mainSequence.ComponetsMapping.Remove(subComponent.Id);
                subComponent.IsMapped = false;
                NotifyPropertyChanged("SubSequenceComponents");
                NotifyPropertyChanged("MTFSequenceClassInfos");
                NotifyPropertyChanged("MappedComponent");
            }
        }



        public IEnumerable<MTFSequence> AvailableSequences
        {
            get { return mtfProject.Where(x => x != mainSequence); }
        }



        public List<MTFSequenceClassInfo> MappedComponents
        {
            get
            {
                if (selectedSequenceClassInfo == null || mainSequence == null || mainSequence.ComponetsMapping == null)
                {
                    return null;
                }

                var classInfoIds = mainSequence.ComponetsMapping.Where(x => x.Value == selectedSequenceClassInfo.Id);
                List<MTFSequenceClassInfo> outputList = new List<MTFSequenceClassInfo>();
                foreach (var classInfoId in classInfoIds)
                {
                    var classInfo = SubSequenceComponents.FirstOrDefault(c => c.Id == classInfoId.Key);
                    if (classInfo != null)
                    {
                        outputList.Add(classInfo);
                    }
                }

                return outputList;
            }
        }

        public object SelectedClassExecutable
        {
            get
            {
                if (selectedSequenceActivity == null)
                {
                    return null;
                }
                NotifyPropertyChanged("LastSelectedItem");
                //if (selectedSequenceActivity.MTFMethodName == null)
                //{
                //if (selectedActivities.Count > 0)
                //{
                //    return selectedActivities.Last().MTFMethodName;
                //}
                //}
                return selectedSequenceActivity.MTFMethodName;
            }
            set
            {
                //adopt executable parameters to sequence Activity
                var mtfMethod = value as MTFMethodInfo;
                if (mtfMethod != null)
                {
                    selectedSequenceActivity.MTFMethodName = mtfMethod.Name;
                    selectedSequenceActivity.MTFMethodDisplayName = mtfMethod.DisplayName;
                    selectedSequenceActivity.MTFMethodDescription = mtfMethod.Description;
                    selectedSequenceActivity.SetupModeSupport = mtfMethod.SetupModeSupport;
                    selectedSequenceActivity.UsedDataNames = mtfMethod.UsedDataNames;
                    selectedSequenceActivity.ReturnType = mtfMethod.ReturnType;
                    selectedSequenceActivity.MTFParameters = new MTFObservableCollection<MTFParameterValue>();
                    foreach (MTFParameterInfo paramInfo in mtfMethod.Parameters)
                    {
                        selectedSequenceActivity.MTFParameters.Add(new MTFParameterValue(paramInfo));
                    }
                }
                else
                {
                    var mtfProperty = value as MTFPropertyInfo;
                    if (mtfProperty == null)
                    {
                        return;
                    }
                    selectedSequenceActivity.MTFMethodName = mtfProperty.Name;
                    selectedSequenceActivity.MTFMethodDisplayName = mtfProperty.DisplayName;
                    selectedSequenceActivity.MTFMethodDescription = mtfProperty.Description;
                    selectedSequenceActivity.ReturnType = mtfProperty.Name.EndsWith(".Set") ? null : mtfProperty.Type;
                    if (selectedSequenceActivity.MTFMethodName.EndsWith(".Get"))
                    {
                        selectedSequenceActivity.MTFParameters = null;
                    }
                    else
                    {
                        selectedSequenceActivity.MTFParameters = new MTFObservableCollection<MTFParameterValue>
                                                                 {
                                                                     new MTFParameterValue(
                                                                         mtfProperty)
                                                                 };
                    }
                }
                NotifyPropertyChanged("LastSelectedItem");
            }
        }

        public IEnumerable<EnumValueDescription> ConstructionTypes
        {
            get { return EnumHelper.GetAllValuesAndDescriptions<MTFClassConstructionType>(); }
        }

        public object LastSelectedItem
        {
            get { return lastSelectedItem; }
            set
            {
                if (!touch.DisableEditorSelection)
                {
                    lastSelectedItem = value;
                    selectedInitRow = null;
                    NotifyPropertyChanged("SelectedInitRow");
                    CheckSequenceHandlingActivity(value);
                    if (lastSelectedItem != null)
                    {
                        selectedSequenceActivity = value as MTFSequenceActivity;
                    }
                    NotifyPropertyChanged();
                }
            }
        }

        private void CheckSequenceHandlingActivity(object value)
        {
            var activity = value as MTFSequenceHandlingActivity;
            if (activity != null)
            {
                if (activity.SequenceHandlingType == SequenceHandlingType.ClearValidTables ||
                    activity.SequenceHandlingType == SequenceHandlingType.SaveReportAndCleanErrors)
                {
                    CheckValidationTablesForClear(activity);
                }
                if (activity.SequenceHandlingType == SequenceHandlingType.ChangeCommandsStatus)
                {
                    CheckCommandsStatus(activity);
                }

                if (activity.SequenceHandlingType == SequenceHandlingType.ChangeUserCommandsStatus)
                {
                    CheckUserCommandsStatus(activity);
                }
            }
        }

        private void CheckCommandsStatus(MTFSequenceHandlingActivity activity)
        {
            var dictionary = new Dictionary<Guid, bool>();
            var nullCommands = new List<ServiceCommandsSetting>();

            if (sequence.ServiceCommands == null)
            {
                return;
            }
            if (activity.CommandsSetting == null)
            {
                activity.CommandsSetting = new List<ServiceCommandsSetting>();
            }

            foreach (var serviceCommandsSetting in activity.CommandsSetting)
            {
                if (serviceCommandsSetting.ServiceCommand != null)
                {
                    dictionary.Add(serviceCommandsSetting.ServiceCommand.Id, false);
                }
                else
                {
                    nullCommands.Add(serviceCommandsSetting);
                }
            }
            foreach (var serviceCommand in sequence.ServiceCommands)
            {
                if (dictionary.ContainsKey(serviceCommand.Id))
                {
                    dictionary[serviceCommand.Id] = true;
                }
                else
                {
                    activity.CommandsSetting.Add(new ServiceCommandsSetting { ServiceCommand = serviceCommand });
                }
            }
            foreach (var source in dictionary.Where(x => !x.Value))
            {
                var itemToDelete = activity.CommandsSetting.FirstOrDefault(x => x.ServiceCommand != null && x.ServiceCommand.Id == source.Key);
                if (itemToDelete != null)
                {
                    activity.CommandsSetting.Remove(itemToDelete);
                }
            }

            foreach (var nullCommand in nullCommands)
            {
                activity.CommandsSetting.Remove(nullCommand);
            }
        }

        private void CheckUserCommandsStatus(MTFSequenceHandlingActivity activity)
        {
            if (sequence.UserCommands == null)
            {
                return;
            }

            if (activity.UserCommandsSetting == null)
            {
                activity.UserCommandsSetting = new List<UserCommandsState>();
            }

            var newUserCommandSettings = sequence.UserCommands.Where(c => c.Type == MTFUserCommandType.Button || c.Type == MTFUserCommandType.ToggleButton).Select(c => new UserCommandsState {UserCommand = c}).ToList();
            if (activity.UserCommandsSetting != null)
            {
                foreach (var oldSetting in activity.UserCommandsSetting)
                {
                    var newSetting =
                        newUserCommandSettings.FirstOrDefault(s => s.UserCommand.Id == oldSetting.UserCommand.Id);
                    if (newSetting != null)
                    {
                        newSetting.IsChecked = oldSetting.IsChecked;
                        newSetting.IsEnabled = oldSetting.IsEnabled;
                    }
                }
            }

            activity.UserCommandsSetting.Clear();
            newUserCommandSettings.ForEach(s => activity.UserCommandsSetting.Add(s));
        }

        private void CheckValidationTablesForClear(MTFSequenceHandlingActivity sequenceHandlingActivity)
        {
            if (sequenceHandlingActivity.TablesSetting == null)
            {
                sequenceHandlingActivity.TablesSetting = new List<TableSetting>();
            }
            var dictionary = new Dictionary<Guid, bool>();
            foreach (var item in sequenceHandlingActivity.TablesSetting)
            {
                if (item.ValidationTable != null)
                {
                    dictionary.Add(item.ValidationTable.Id, false);
                }
            }

            foreach (var table in ValidationTables)
            {
                if (dictionary.ContainsKey(table.Id))
                {
                    dictionary[table.Id] = true;
                }
                else
                {
                    sequenceHandlingActivity.TablesSetting.Add(new TableSetting() { ValidationTable = table });
                }
            }
            foreach (var item in dictionary.Where(x => !x.Value))
            {
                var itemToDelete = sequenceHandlingActivity.TablesSetting.FirstOrDefault(x => x.ValidationTable.Id == item.Key);
                if (itemToDelete != null)
                {
                    sequenceHandlingActivity.TablesSetting.Remove(itemToDelete);
                }
            }
        }


        public IEnumerable<MTFSequenceClassInfo> SubSequenceComponents
        {
            get
            {
                foreach (var item in mtfProject)
                {
                    if (item != mainSequence)
                    {
                        if (item.MTFSequenceClassInfos != null)
                        {
                            foreach (var clasInfo in item.MTFSequenceClassInfos)
                            {
                                if (clasInfo.MTFClass != null && clasInfo.MTFClass.IsStatic)
                                {
                                    continue;
                                }
                                if (!clasInfo.IsMapped)
                                {
                                    yield return clasInfo;
                                }
                            }
                        }
                    }
                }
            }
        }

        public bool DisplaySubComponents
        {
            get { return sequence == mainSequence && sequence.ExternalSubSequences != null && sequence.ExternalSubSequences.Count > 0; }
        }

        public bool IsMainSequence
        {
            get { return mainSequence == sequence; }
        }


        public ObservableCollection<EnumValueDescription> BuildInCommands { get; private set; }

        public void ClipboardIsChanged()
        {
            NotifyPropertyChanged("IsFilledMTFClipboard");
        }

        public bool IsFilledMTFClipboard
        {
            get { return !MTFClipboard.IsEmpty(); }
        }

        private bool isSelectedItem;

        public bool IsSelectedItem
        {
            get { return isSelectedItem; }
            set
            {
                isSelectedItem = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsModified
        {
            get { return mtfProject.Any(x => x.IsModified); }
        }

        public bool SequenceWasModified
        {
            get { return sequenceWasModified || IsModified; }
        }

        public Array MTFColors
        {
            get { return Enum.GetValues(typeof(AutomotiveLighting.MTFCommon.MTFColors)); }
        }

        public IEnumerable<EnumValueDescription> ErrorBehaviours
        {
            get { return EnumHelper.GetAllValuesAndDescriptions<MTFErrorBehavior>(); }
        }

        public IEnumerable<EnumValueDescription> ExecutionTypes
        {
            get { return EnumHelper.GetAllValuesAndDescriptions<ExecutionType>(); }
        }

        public IEnumerable<EnumValueDescription> SequenceHandlingTypes
        {
            get { return EnumHelper.GetAllValuesAndDescriptions<SequenceHandlingType>(); }
        }

        public IEnumerable<EnumValueDescription> ErrorHandlingTypes
        {
            get { return EnumHelper.GetAllValuesAndDescriptions<ErrorHandlingType>(); }
        }

        public IEnumerable<EnumValueDescription> ExecuteActivityTypes
        {
            get { return EnumHelper.GetAllValuesAndDescriptions<ExecuteActyvityTypes>(); }
        }

        public IEnumerable<EnumValueDescription> DynamicActivityTypes
        {
            get { return EnumHelper.GetAllValuesAndDescriptions<DynamicActivityTypes>(); }
        }

        public IEnumerable<EnumValueDescription> LoggingTypes
        {
            get { return EnumHelper.GetAllValuesAndDescriptions<LoggingType>(); }
        }

        public IEnumerable<EnumValueDescription> SequenceMessageTypes
        {
            get { return EnumHelper.GetAllValuesAndDescriptions<SequenceMessageType>(); }
        }

        public IEnumerable<EnumValueDescription> SequenceMessageDisplayTypes
        {
            get { return EnumHelper.GetAllValuesAndDescriptions<SequenceMessageDisplayType>(); }
        }

        public IEnumerable<EnumValueDescription> ValidationTableExecutionModes
        {
            get { return EnumHelper.GetAllValuesAndDescriptions<MTFValidationTableExecutionMode>(); }
        }

        public IEnumerable<EnumValueDescription> ServiceExecutionBehaviors
        {
            get { return EnumHelper.GetAllValuesAndDescriptions<ServiceExecutionBehavior>(); }
        }

        public IEnumerable<EnumValueDescription> ExecutionViewTypes
        {
            get { return EnumHelper.GetAllValuesAndDescriptions(new[] { SequenceExecutionViewType.Service, SequenceExecutionViewType.TimeView, }); }
        }

        public IEnumerable<GraphicalViewInfo> GraphicalViews
        {
            get
            {
                if (sequence != null && sequence.GraphicalViewSetting != null && sequence.GraphicalViewSetting.HasView)
                {
                    return sequence.GraphicalViewSetting.Views;
                }
                return null;
            }
        }

        public MTFSequence Sequence
        {
            get { return sequence; }
            set
            {
                selectionHelper.Clear(true);
                if (value != null)
                {
                    if (value.IsLoad)
                    {
                        sequence = null;
                        //fix due to correct binding in comboBoxes when IsAsync == true
                        int index = mtfProject.IndexOf(value);
                        if (index != -1)
                        {
                            mtfProject[index] = value;
                        }
                        //endfix
                        sequence = value;
                    }
                    else
                    {
                        sequence = null;
                        var tmp = LoadSequence(value.FullPath);
                        ReplaceLoadSequence(tmp, value);
                        sequence = tmp;
                    }

                    NotifyPropertyChanged();
                }
                else
                {
                    sequence = null;
                }
                if (value == mainSequence)
                {
                    NotifyPropertyChanged("SubSequenceComponents");
                }
                NotifyPropertyChanged("DisplaySubComponents");
                NotifyPropertyChanged("Commands");
            }
        }


        public MTFSequence MainSequence
        {
            get { return mainSequence; }
        }

        public void ReplaceLoadSequence(MTFSequence newSequence, MTFSequence oldSequence)
        {
            int index = mtfProject.IndexOf(oldSequence);
            if (index != -1)
            {
                mtfProject[index] = newSequence;
            }

            var extrnalInfo = mainSequence.ExternalSubSequences.FirstOrDefault(x => x.ExternalSequence == oldSequence);
            if (extrnalInfo != null)
            {
                extrnalInfo.ExternalSequence = newSequence;
            }
            GenerateComponentsMapping();
        }

        public MTFSequence LoadForExecuteActivity(MTFSequence dontLoadSequence)
        {
            MTFSequence loadedSequence = null;
            LongTask.Do(() =>
                        {
                            loadedSequence = LoadSequence(dontLoadSequence.FullPath);
                            ReplaceLoadSequence(loadedSequence, dontLoadSequence);
                        }, LanguageHelper.GetString("Mtf_LongTask_LoadingSequence"));
            return loadedSequence;
        }


        public IList<MTFSequenceActivity> Activities
        {
            get { return sequence.MTFSequenceActivities; }
        }

        public ICommand AddVariableToSequence
        {
            get { return new Command(addVariableToSequence); }
        }

        public ICommand RemoveVariableFromSequence
        {
            get { return removeVariableFromSequenceCommand; }
        }

        public ICommand RemoveServiceCommandFromSequenceCommand
        {
            get { return removeServiceCommandFromSequenceCommand; }
        }

        public ICommand RemoveUserCommandFromSequenceCommand
        {
            get { return removeUserCommandFromSequenceCommand; }
        }

        //public ICommand AddAvailableComponents
        //{
        //    get { return addAvailableComponentsCommand; }
        //}

        public ICommand CleanSearchText
        {
            get { return cleanSearchTextCommand; }
        }

        public ICommand ShowTermDesignerCommand
        {
            get
            {
                return new Command((param) =>
                                   {
                                       if (!ShowTermDesigner)
                                       {
                                           ShowTermDesigner = true;
                                           AssignParameterFromCommand(param);
                                           NotifyPropertyChanged(nameof(SelectedTerm));
                                       }

                                   });
            }
        }

        private void AssignParameterFromCommand(object param)
        {
            if (param is Array array && array.Length >= 4)
            {
                TermPropertyName = array.GetValue(0) as string;

                var val = array.GetValue(1);
                if (val is EditorModes modes)
                {
                    EditorMode = modes;
                }
                else
                {
                    EditorMode = (EditorModes)Enum.Parse(typeof(EditorModes), (string)val);
                }

                SelectedTermTargetType = array.GetValue(2) as string;

                SelectedTerm = array.GetValue(3) as Term ?? new ConstantTerm(typeof(string)) { Value = array.GetValue(3) };

                if (array.Length == 5)
                {
                    ParentOfTerm = array.GetValue(4);
                    NotifyPropertyChanged(nameof(ParentOfTerm));
                }
            }
        }

        public void SelectActivityById(List<Guid> idPath, bool navigateFromExecution)
        {
            if (idPath != null && idPath.Count > 1)
            {
                LoadAllSequences();

                var currentSequence = mtfProject.FirstOrDefault(x => x.Id == idPath[0]);
                if (currentSequence != null)
                {
                    if (currentSequence != this.Sequence)
                    {
                        this.Sequence = currentSequence;
                    }

                    var activityOnFirstLevel = currentSequence.MTFSequenceActivities.FirstOrDefault(x => x.Id == idPath[1]) ??
                                               currentSequence.ActivitiesByCall.FirstOrDefault(x => x.Id == idPath[1]);

                    if (activityOnFirstLevel != null)
                    {
                        if (idPath.Count > 2)
                        {
                            MTFSequenceActivity activityToSelect = activityOnFirstLevel;

                            for (int i = 2; i < idPath.Count; i++)
                            {
                                if (activityToSelect is MTFSubSequenceActivity subSequence)
                                {
                                    activityToSelect = subSequence.Activities.FirstOrDefault(x => x.Id == idPath[i]);
                                    if (activityToSelect == null && subSequence.Cases != null)
                                    {
                                        var caseToSelect = subSequence.Cases.FirstOrDefault(x => x.Id == idPath[i]);
                                        if (caseToSelect?.Activities != null)
                                        {
                                            i++;
                                            activityToSelect = caseToSelect.Activities.FirstOrDefault(x => x.Id == idPath[i]);
                                        }
                                    }
                                }
                                else
                                {
                                    break;
                                }

                            }
                            if (activityToSelect != null)
                            {
                                MakeSelection(activityToSelect);
                                return;
                            }
                        }
                        else
                        {
                            MakeSelection(activityOnFirstLevel);
                            return;
                        }
                    }

                }
            }

            if (navigateFromExecution)
            {
                navigationHandler.TargetGuidPath = idPath;
            }
            else
            {
                MTFMessageBox.Show("Sequence Editor", "Activity could not be found in current opened sequence.", MTFMessageBoxType.Error,
                    MTFMessageBoxButtons.Ok);
            }

        }

        private void MakeSelection(MTFSequenceActivity activity)
        {
            activity.ExpandParentSubSequence();
            selectionHelper.Add(activity);
        }


        public ICommand NavigateToActivityCommand => new Command(NavigateToActivity);

        private void NavigateToActivity(object param)
        {
            NavigateToActivity(param as MTFSequenceActivity, false);
        }

        public void NavigateToActivity(MTFSequenceActivity activity, bool navigateFromExecution)
        {
            bool navigate = false;

            var parentSequence = activity?.GetActivityParentOfType<MTFSequence>();
            if (parentSequence != null)
            {
                var tmpSequence = mtfProject.FirstOrDefault(x => x.Name == parentSequence.Name);
                MTFSequence loadedSequence = null;
                if (tmpSequence == this.Sequence)
                {
                    loadedSequence = tmpSequence;
                }
                else if (tmpSequence != null)
                {
                    if (!tmpSequence.IsLoad)
                    {
                        LongTask.Do(() =>
                                    {
                                        loadedSequence = LoadSequence(tmpSequence.FullPath);
                                        ReplaceLoadSequence(loadedSequence, tmpSequence);
                                    }, LanguageHelper.GetString("Mtf_LongTask_OpeningSeq"));
                    }
                    else
                    {
                        loadedSequence = tmpSequence;
                    }
                    Sequence = loadedSequence;
                    NotifyPropertyChanged(nameof(Sequence));
                }
                if (loadedSequence != null)
                {
                    MTFSequenceActivity activityToSelect = null;
                    loadedSequence.ForEachActivity(act =>
                                                   {
                                                       if (activity.Id == act.Id)
                                                       {
                                                           activityToSelect = act;
                                                       }
                                                   });
                    if (activityToSelect != null)
                    {
                        MakeSelection(activityToSelect);
                    }


                    navigate = true;
                }
            }
            if (!navigate)
            {
                if (navigateFromExecution)
                {
                    navigationHandler.TargetActivity = activity;
                }
                else
                {
                    MTFMessageBox.Show("Sequence Editor", "Activity could not be found in current opened sequence.", MTFMessageBoxType.Error,
                        MTFMessageBoxButtons.Ok);
                }
            }
        }

        public ICommand RemoveExternalSubSequenceCommand => new Command(RemoveExternalSubSequence);

        private void RemoveExternalSubSequence(object param)
        {
            var removedSequence = param as MTFSequence;
            if (removedSequence != mainSequence && removedSequence != null)
            {
                var result = MTFMessageBox.Show("Delete external SubSequence",
                    "Are you sure you want to delete SubSequence: " + removedSequence.Name + " from Sequence " + mainSequence.Name + "?",
                     MTFMessageBoxType.Question, MTFMessageBoxButtons.OkCancel);
                if (result == MTFMessageBoxResult.Ok)
                {
                    sequence = mainSequence;
                    NotifyPropertyChanged("Sequence");
                    mtfProject.Remove(removedSequence);
                    mainSequence.ExternalSubSequences.Remove(mainSequence.ExternalSubSequences.FirstOrDefault(x => x.ExternalSequence == removedSequence));
                    NotifyPropertyChanged("SubSequenceComponents");
                    NotifyPropertyChanged("DisplaySubComponents");
                    NotifyPropertyChanged("Commands");
                    mainSequence.IsModified = true;
                    RemoveSubComponentsFromMainSequence(removedSequence.MTFSequenceClassInfos);
                }
            }
        }

        private void RemoveSubComponentsFromMainSequence(IList<MTFSequenceClassInfo> classInfos)
        {
            if (classInfos != null)
            {
                foreach (var classInfo in classInfos)
                {
                    mainSequence.ComponetsMapping.Remove(classInfo.Id);
                    foreach (var item in mainSequence.MTFSequenceClassInfos)
                    {
                        if (item.SubComponents != null)
                        {
                            item.SubComponents.Remove(classInfo);
                        }
                    }
                }
            }
        }

        private string searchText;
        public string SearchText
        {
            get
            {
                return searchText;
            }
            set
            {
                searchText = value;
                NotifyPropertyChanged();
                cleanSearchTextCommand.RaiseCanExecuteChanged();
                searchTimer.Stop();
                if (!string.IsNullOrEmpty(searchText))
                {
                    searchTimer.Start();
                }
                else
                {
                    if (SearchResults != null)
                    {
                        SearchResults.Clear();
                    }
                }
            }
        }


        public List<SearchResult> SearchResults
        {
            get;
            set;
        }

        private void Search(string text)
        {
            List<SearchResult> results = new List<SearchResult>();
            if (sequence != null)
            {
                foreach (var classInfo in sequence.MTFSequenceClassInfos)
                {
                    var result = SearchInMTFClass(classInfo.MTFClass, text, classInfo.Alias);
                    if (result != null)
                    {
                        results.Add(result);
                    }
                }
            }
            foreach (var classInfo in availableClasses)
            {
                var result = SearchInMTFClass(classInfo, text, string.Empty);
                if (result != null)
                {
                    result.IsClassNOTInSequence = true;
                    results.Add(result);
                }
            }

            SearchResults = results;
            NotifyPropertyChanged("SearchResults");
        }

        private SearchResult SearchInMTFClass(MTFClassInfo classInfo, string text, string classAlias)
        {
            List<SearchResultMethod> methods = new List<SearchResultMethod>();
            foreach (var all in classInfo.AllExecutables)
            {
                if (all is MTFMethodInfo)
                {
                    var m = all as MTFMethodInfo;
                    if (cotainsString(m.Name, text) || cotainsString(m.DisplayName, text) || cotainsString(m.Description, text))
                    {
                        ObservableCollection<MTFParameterValue> parameters = new ObservableCollection<MTFParameterValue>();
                        foreach (MTFParameterInfo item in m.Parameters)
                        {
                            parameters.Add(new MTFParameterValue(item));
                        }
                        methods.Add(new SearchResultMethod
                        {
                            Name = m.Name,
                            DisplayName = m.DisplayName,
                            Description = m.Description,
                            MTFClassInfo = classInfo,
                            ResultType = m.ReturnType,
                            MTFParameters = parameters,
                            ClassAlias = classAlias,
                            SetupModeSupport = m.SetupModeSupport,
                            UsedDataNames = m.UsedDataNames,
                        });
                    }
                }
                else if (all is MTFPropertyInfo)
                {
                    var p = all as MTFPropertyInfo;
                    if (cotainsString(p.Name, text) || cotainsString(p.DisplayName, text) || cotainsString(p.Description, text))
                    {
                        ObservableCollection<MTFParameterValue> parameters = null;
                        if (!p.Name.EndsWith(".Get"))
                        {
                            parameters = new ObservableCollection<MTFParameterValue> { new MTFParameterValue(p) };
                        }
                        methods.Add(new SearchResultMethod
                        {
                            Name = p.Name,
                            DisplayName = p.DisplayName,
                            Description = p.Description,
                            MTFClassInfo = classInfo,
                            ResultType = p.Name.EndsWith(".Set") ? null : p.Type,
                            MTFParameters = parameters
                        });
                    }
                }
            }



            if (methods.Count > 0)
            {
                return new SearchResult
                {
                    ClassInfo = classInfo,
                    Alias = classAlias,
                    Methods = methods,
                };
            }

            return null;
        }

        private bool cotainsString(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
            {
                return false;
            }
            return s1.ToUpper().Contains(s2.ToUpper());
        }

        //private void addAvailableComponents(object param)
        //{
        //    var currentSequence = this.Sequence;
        //    addAvailableComponentsButtonEnable = false;
        //    Task.Run(() =>
        //    {
        //        MTFClient.AvailableMonsterClasses.ForEach(x =>
        //        {
        //            if (!currentSequence.MTFSequenceClassInfos.Any(c => c.MTFClass.Name == x.Name))
        //            {
        //                if (MTFClient.ClassInstanceConfigurations(x).Count > 0 || x.IsStatic)
        //                {
        //                    App.Current.Dispatcher.Invoke(() => addClassToSequence(x, currentSequence));
        //                }
        //            }
        //        });
        //        App.Current.Dispatcher.Invoke(() => addAvailableComponentsButtonEnable = true);
        //    });


        //}


        public ICommand AddClassToSequence
        {
            get { return new Command(param => addClassToSequence(param, this.sequence)); }
        }

        public ICommand SwitchClassType
        {
            get { return new Command(param => switchClassType(param, this.sequence)); }
        }

        public ICommand RemoveClassFromSequence
        {
            get { return removeClassFromSequenceCommand; }
        }

        public ICommand RemoveActivityFromSequence
        {
            get { return removeActivityFromSequenceCommand; }
        }

        public ICommand ChangeCollapsedStateCommand
        {
            get
            {
                return new Command(param =>
                                   {
                                       var subSequenceActivity = param as MTFSubSequenceActivity;
                                       if (subSequenceActivity != null)
                                       {
                                           subSequenceActivity.IsCollapsed = !subSequenceActivity.IsCollapsed;
                                           if (actualVariant != null)
                                           {
                                               SequenceVariantHelper.SwitchCases(subSequenceActivity, actualVariant);
                                           }
                                       }
                                   });
            }
        }

        public ICommand RemoveCaseCommand
        {
            get { return new Command(RemoveCase); }
        }

        private void RemoveCase(object obj)
        {
            var mtfCase = obj as MTFCase;
            if (mtfCase != null)
            {
                var result = MTFMessageBox.Show("Remove case",
                    string.Format("Do you really want to remove case {0} and all included activities?", mtfCase.Name), MTFMessageBoxType.Question,
                    MTFMessageBoxButtons.YesNo);
                if (result == MTFMessageBoxResult.Yes)
                {
                    var subSequence = mtfCase.Parent as MTFSubSequenceActivity;
                    if (subSequence != null)
                    {
                        var tmp = subSequence.Cases.ToList();
                        tmp.Remove(mtfCase);
                        subSequence.Cases = tmp;
                        subSequence.ActualCaseIndex = 0;
                    }
                }
            }
        }

        public ICommand CopyVariableCommand => copyVariableCommand;

        public ICommand PasteVariableCommand => pasteVariableCommand;

        public ICommand SubSequenceVariantSwitchCommand
        {
            get { return new Command(SubSequenceVariantSwitch); }
        }

        private void SubSequenceVariantSwitch()
        {
            var subSequence = selectedSequenceActivity as MTFSubSequenceActivity;
            if (subSequence != null)
            {
                subSequence.Term = subSequence.VariantSwitch
                    ? (Term)new TermWrapper { Value = sequence.VariantGroups }
                    : new EmptyTerm("System.Boolean");
            }
        }

        private void copyVariable(object obj)
        {
            MTFClipboard.SetData(SelectedVariable, SelectedVariable.Parent);
        }

        private void pasteVariable(object obj)
        {
            if (MTFClipboard.IsEmpty())
            {
                return;
            }

            if (obj is SequenceEditorPresenter data)
            {
                if (MTFClipboard.IsSameParent(data.sequence))
                {
                    AddCopyOfVariable(MTFClipboard.GetData());
                }
                else
                {
                    if (MTFClipboard.GetData() is MTFVariable variable)
                    {
                        var sameVariable = data.sequence.MTFVariables.FirstOrDefault(x => x.Name == variable.Name);
                        if (sameVariable != null)
                        {
                            if (MTFMessageBox.Show("Pasting Variable",
                                    $"The variable {variable.Name} is exist in this sequence.{Environment.NewLine}Do you want to replace this variable?",
                                    MTFMessageBoxType.Question, MTFMessageBoxButtons.YesNo) == MTFMessageBoxResult.Yes)
                            {
                                if (variable.HasValidationTable)
                                {
                                    replaceValidationTable(variable, sameVariable);
                                }
                                else if (variable.HasConstantTable)
                                {
                                    replaceConstantTable(variable, sameVariable);
                                }
                                else
                                {
                                    sameVariable.TypeName = variable.TypeName;
                                    sameVariable.Value = variable.Value;
                                }
                            }
                            else
                            {
                                AddCopyOfVariable(variable);
                            }
                        }
                        else
                        {
                            AddCopyOfVariable(variable, false);
                        }
                    }
                }
            }
        }

        private void replaceValidationTable(MTFVariable variable, MTFVariable sameVariable)
        {
            if (!(variable.Value is MTFValidationTable mainValidationTable) || !(sameVariable.Value is MTFValidationTable externalValidationTable))
            {
                return;
            }

            // add columns
            foreach (var column in mainValidationTable.Columns.Where(x => x.Type == MTFTableColumnType.Value))
            {
                if (externalValidationTable.Columns.FirstOrDefault(x => x.Header.Equals(column.Header)) == null)
                {
                    externalValidationTable.AddColumn(column.Clone() as MTFValidationTableColumn);
                }
            }

            // add rows
            foreach (var row in mainValidationTable.Rows)
            {
                var externalRow = externalValidationTable.Rows.FirstOrDefault(x => x.Header.Equals(row.Header));
                if (externalRow == null)
                {
                    externalValidationTable.AddRow(row.Header);
                    externalRow = externalValidationTable.Rows.Last();
                }
                foreach (var item in row.Items.Where(x => x.Type == MTFTableColumnType.Value).ToList())
                {
                    var externalItem = externalRow.Items.FirstOrDefault(x => x.Column.Header == item.Column.Header);
                    if (externalItem != null)
                    {
                        externalItem.Value = item.Value;
                    }
                }

                if (row.AllowNokGs)
                {
                    externalRow.AllowNokGs = true;
                    externalRow.NokVariantSelector = row.NokVariantSelector;
                }
                else
                {
                    externalRow.AllowNokGs = false;
                    externalRow.NokVariantSelector = null;
                }

                if (row.RowVariants != null && row.RowVariants.Count > 0)
                {
                    foreach (var rowVariant in row.RowVariants)
                    {
                        var sameRowVariant = externalRow.RowVariants?.FirstOrDefault(x => x.SequenceVariant.Equals(rowVariant.SequenceVariant));
                        if (sameRowVariant != null)
                        {
                            foreach (var externalItem in sameRowVariant.Items)
                            {
                                var mainItem = rowVariant.Items.FirstOrDefault(x => x.Header.Equals(externalItem.Header));
                                if (mainItem != null)
                                {
                                    externalItem.Value = mainItem.Value;
                                }
                            }
                        }
                        else
	                    {
                            externalRow.AddVariant();
                            var lastExternalRow = externalRow.RowVariants.Last();
                            lastExternalRow.SequenceVariant = rowVariant.SequenceVariant;
                            foreach (var externalItem in lastExternalRow.Items)
                            {
                                var mainItem = rowVariant.Items.FirstOrDefault(x => x.Header.Equals(externalItem.Header));
                                if (mainItem != null)
                                {
                                    externalItem.Value = mainItem.Value;
                                }
                            } 
                        }
                    }
                }
                else
                {
                    externalRow.RowVariants = null;
                }
            }
            externalValidationTable.UseGoldSample = mainValidationTable.UseGoldSample;
        }

        private void replaceConstantTable(MTFVariable variable, MTFVariable sameVariable)
        {
            if (!(variable.Value is MTFConstantTable mainConstantTable) || !(sameVariable.Value is MTFConstantTable externalConstantTable))
            {
                return;
            }

            // add columns
            foreach (var column in mainConstantTable.Columns)
            {
                if (externalConstantTable.Columns.FirstOrDefault(x => x.Header.Equals(column.Header)) == null)
                {
                    externalConstantTable.AddColumn(column.Clone() as MTFTableColumn);
                }
            }

            // add rows
            foreach (var row in mainConstantTable.Rows)
            {
                var externalRow = externalConstantTable.Rows.FirstOrDefault(x => x.Header.Equals(row.Header));
                if (externalRow == null)
                {
                    externalConstantTable.AddRow();
                    externalRow = externalConstantTable.Rows.Last();
                }
                foreach (var item in row.Items)
                {
                    var externalItem = externalRow.Items.FirstOrDefault(x => x.Column.Header == item.Column.Header);
                    if (externalItem != null)
                    {
                        externalItem.Value = item.Value;
                    }
                }

                if (row.RowVariants != null && row.RowVariants.Count > 0)
                {
                    foreach (var rowVariant in row.RowVariants)
                    {
                        var sameRowVariant = externalRow.RowVariants?.FirstOrDefault(x => x.SequenceVariant.Equals(rowVariant.SequenceVariant));
                        if (sameRowVariant != null)
                        {
                            sameRowVariant.Items.First(x => x.Column.Header.Equals("Value")).Value = rowVariant.Items.First(x => x.Column.Header.Equals("Value")).Value;
                        }
                        else
                        {
                            externalRow.AddVariant();
                            var lastExternalRow =  externalRow.RowVariants.Last();
                            lastExternalRow.SequenceVariant = rowVariant.SequenceVariant;
                            lastExternalRow.Items.First(x => x.Column.Header.Equals("Value")).Value = rowVariant.Items.First(x => x.Column.Header.Equals("Value")).Value;
                        }
                    }
                }
                else
                {
                    externalRow.RowVariants = null;
                }
            }
        }

        
        private void AddCopyOfVariable(object param, bool adjustName = true)
        {
            if (param is ContextMenu menu)
            {
                var listBox = menu.PlacementTarget as ListBox;
                if (listBox?.SelectedItem is MTFVariable variable)
                {
                    if (variable.Clone() is MTFVariable newVariable)
                    {
                        if (adjustName)
                        {
                            newVariable.AdjustName(sequence.MTFVariables);
                        }
                        sequence.MTFVariables.AddSorted(newVariable);
                        if (newVariable.HasTable)
                        {
                            NotifyPropertyChanged(nameof(ValidationTables));
                            NotifyPropertyChanged(nameof(Tables));
                        }
                    }
                }
            }
        }


        private bool isComponentActive = true;
        public bool IsComponentActive
        {
            get { return isComponentActive; }
            set
            {
                if (value)
                {
                    isComponentActive = true;
                    isVariableActive = false;
                    isServiceCommandsActive = false;
                    isUserCommandsActive = false;
                    NotifyPropertyChanged("IsUserCommandsActive");
                    NotifyPropertyChanged("IsVariableActive");
                    NotifyPropertyChanged("IsServiceCommandsActive");
                    NotifyPropertyChanged();
                }
            }
        }

        private bool isVariableActive;
        public bool IsVariableActive
        {
            get { return isVariableActive; }
            set
            {
                if (value)
                {
                    isVariableActive = value;
                    isComponentActive = false;
                    isServiceCommandsActive = false;
                    isUserCommandsActive = false;
                    NotifyPropertyChanged("IsUserCommandsActive");
                    NotifyPropertyChanged("IsComponentActive");
                    NotifyPropertyChanged("IsServiceCommandsActive");
                    NotifyPropertyChanged();
                }
            }
        }

        private bool isServiceCommandsActive;
        public bool IsServiceCommandsActive
        {
            get { return isServiceCommandsActive; }
            set
            {
                if (value)
                {
                    isServiceCommandsActive = value;
                    isComponentActive = false;
                    isVariableActive = false;
                    isUserCommandsActive = false;
                    NotifyPropertyChanged("IsUserCommandsActive");
                    NotifyPropertyChanged("IsVariableActive");
                    NotifyPropertyChanged("IsComponentActive");
                    NotifyPropertyChanged();
                }
            }
        }

        private bool isUserCommandsActive;
        public bool IsUserCommandsActive
        {
            get { return isUserCommandsActive; }
            set
            {
                if (value)
                {
                    isUserCommandsActive = value;
                    isServiceCommandsActive = false;
                    isComponentActive = false;
                    isVariableActive = false;
                    NotifyPropertyChanged("IsVariableActive");
                    NotifyPropertyChanged("IsComponentActive");
                    NotifyPropertyChanged("IsServiceCommandsActive");
                    NotifyPropertyChanged();
                }
            }
        }

        private void addServiceCommand()
        {
            if (sequence.ServiceCommands == null)
            {
                sequence.ServiceCommands = new MTFObservableCollection<MTFServiceCommand>();
            }
            var command = new MTFServiceCommand { Name = "New Service Command", UsedServiceVariants = new List<MTFServiceModeVariants> { MTFServiceModeVariants.ServiceMode, MTFServiceModeVariants.Teach } };
            command.AdjustName(sequence.ServiceCommands);
            sequence.ServiceCommands.Add(command);
        }

        private void addUserCommand()
        {
            if (sequence.UserCommands == null)
            {
                sequence.UserCommands = new MTFObservableCollection<MTFUserCommand>();
            }
            var command = new MTFUserCommand{ Name = "New User Command"};
            command.AdjustName(sequence.UserCommands);
            sequence.UserCommands.Add(command);
        }

        public MTFServiceCommand SelectedServiceCommand
        {
            get { return selectedServiceCommand; }
            set
            {
                selectedServiceCommand = value;
                LastSelectedItem = value;
            }
        }

        public MTFUserCommand SelectedUserCommand
        {
            get { return selectedUserCommand; }
            set
            {
                selectedUserCommand = value;
                LastSelectedItem = value;
            }
        }

        public object ParentOfTerm { get; set; }

        private Term selectedTerm;

        public Term SelectedTerm
        {
            get { return selectedTerm; }
            set
            {
                selectedTerm = value;
                if (value != null)
                {
                    ParentOfTerm = value.Parent;
                    NotifyPropertyChanged("ParentOfTerm");
                }
                NotifyPropertyChanged();
            }
        }

        private string termPropertyName;

        public string TermPropertyName
        {
            get { return termPropertyName; }
            set
            {
                termPropertyName = value;
                NotifyPropertyChanged();
            }
        }

        private string selectedTermTargetType;

        public string SelectedTermTargetType
        {
            get { return selectedTermTargetType; }
            set
            {
                selectedTermTargetType = value;
                NotifyPropertyChanged();
            }
        }

        private UIHelpers.Editors.EditorModes editorMode;

        public UIHelpers.Editors.EditorModes EditorMode
        {
            get { return editorMode; }
            set
            {
                editorMode = value;
                NotifyPropertyChanged();
            }
        }


        public bool ShowTermDesigner
        {
            get { return showTermDesigner; }
            set
            {
                showTermDesigner = value;
                NotifyPropertyChanged();
                EditorIsVisible = !value;
                if (!value)
                {
                    SelectedTerm = null;
                }
            }
        }

        private bool editorIsVisible = true;

        public bool EditorIsVisible
        {
            get => editorIsVisible;
            set
            {
                editorIsVisible = value;
                NotifyPropertyChanged();
            }
        }

        private void addVariableToSequence(object param)
        {
            Type variableType = param as Type;
            if (variableType != null)
            {
                if (sequence.MTFVariables == null)
                {
                    sequence.MTFVariables = new MTFObservableCollection<MTFVariable>();
                    NotifyPropertyChanged("Sequence.MTFVariables");
                }
                var variable = new MTFVariable()
                {
                    Name = "New " + variableType.Name + " Variable",
                    Id = Guid.NewGuid(),
                    TypeName = variableType.FullName,
                    Value = variableType == typeof(string) ? string.Empty : Activator.CreateInstance(variableType),
                };
                variable.AdjustName(sequence.MTFVariables);
                sequence.MTFVariables.AddSorted(variable);
                if (variable.HasTable)
                {
                    NotifyPropertyChanged("ValidationTables");
                    NotifyPropertyChanged("Tables");
                }
            }
        }


        private void removeVariableFromSequence(object obj)
        {
            sequence.MTFVariables.Remove(selectedVariable);
            selectedVariable = null;
            NotifyPropertyChanged("ValidationTables");
            NotifyPropertyChanged("Tables");
        }

        private void RemoveServiceCommandFromSequence(object obj)
        {
            if (sequence.ServiceCommands != null)
            {
                sequence.ServiceCommands.Remove(selectedServiceCommand);
                SelectedServiceCommand = null;
            }
        }

        private void RemoveUserCommandFromSequence(object obj)
        {
            if (sequence.UserCommands != null)
            {
                sequence.UserCommands.Remove(selectedUserCommand);
                SelectedUserCommand = null;
            }
        }


        private void addClassToSequence(object param, MTFSequence currentSequence)
        {
            MTFClassInfo mtfClassInfo = param as MTFClassInfo;
            if (mtfClassInfo == null)
            {
                return;
            }

            if (currentSequence.MTFSequenceClassInfos == null)
            {
                currentSequence.MTFSequenceClassInfos = new MTFObservableCollection<MTFSequenceClassInfo>();
                NotifyPropertyChanged("Sequence.MTFSequenceClassInfos");
            }

            if (mtfClassInfo.IsStatic)
            {
                if (currentSequence.MTFSequenceClassInfos.Any(x => x.MTFClass != null && x.MTFClass.IsStatic &&
                                                                   x.MTFClass.FullName == mtfClassInfo.FullName))
                {
                    return;
                }
            }


            var seqClassInfo = GetSequenceClassInfoFromMTFClassInfo(mtfClassInfo);
            seqClassInfo.AdjustName(currentSequence.MTFSequenceClassInfos);
            currentSequence.MTFSequenceClassInfos.Add(seqClassInfo);
        }

        private void switchClassType(object param, MTFSequence currentSequence)
        {
            var newClassInfo = param as MTFClassInfo;
            if (newClassInfo == null)
            {
                return;
            }

            if (selectedSequenceClassInfo != null)
            {
                selectedSequenceClassInfo.MTFClassInstanceConfiguration = null;
                var oldClassInfo = selectedSequenceClassInfo.MTFClass;
                selectedSequenceClassInfo.MTFClass = newClassInfo;
                NotifyPropertyChanged(nameof(ClassInstanceConfiurations));
                currentSequence.ForEachActivity(a =>
                {
                    if (a?.ClassInfo?.MTFClass == oldClassInfo)
                    {
                        a.ClassInfo.MTFClass = newClassInfo;
                    }
                });
                NotifyPropertyChanged(nameof(Sequence));

                UpdateActivitiesParams(currentSequence);
            }
        }

        private void removeClassFromSequence()
        {
            bool delete = true;
            if (selectedSequenceClassInfo != null)
            {
                if (CheckClassInfoInSequence(selectedSequenceClassInfo, sequence))
                {
                    if (MTFMessageBox.Show("Delete Component",
                        string.Format("Do you really want to delete {0}?{1}This component is used in sequence.",
                        selectedSequenceClassInfo.Alias, Environment.NewLine),
                         MTFMessageBoxType.Question, MTFMessageBoxButtons.YesNo) == MTFMessageBoxResult.No)
                    {
                        delete = false;
                    }
                }
                if (delete)
                {
                    RemoveMapping(selectedSequenceClassInfo);
                    PerformRemoveSequenceClassInfo(sequence, selectedSequenceClassInfo);
                    selectedSequenceClassInfo = null;
                    NotifyPropertyChanged("MTFSequenceClassInfos");
                    NotifyPropertyChanged("SubSequenceComponents");
                }

            }
        }

        private bool CheckClassInfoInSequence(MTFSequenceClassInfo selectedSequenceClassInfo, MTFSequence sequence)
        {
            return sequence.Any(x => x.ClassInfo == selectedSequenceClassInfo);
        }

        private void PerformRemoveSequenceClassInfo(MTFSequence currentSequence, MTFSequenceClassInfo classInfo)
        {
            if (classInfo != null)
            {
                if (classInfo.SubComponents != null && classInfo.SubComponents.Count > 0)
                {
                    foreach (var item in classInfo.SubComponents)
                    {
                        currentSequence.ComponetsMapping.Remove(item.Id);
                        item.IsMapped = false;
                    }
                    classInfo.SubComponents.Clear();
                }
            }
            currentSequence.MTFSequenceClassInfos.Remove(classInfo);

        }

        private MTFSequenceClassInfo GetSequenceClassInfoFromMTFClassInfo(MTFClassInfo mtfClassInfo)
        {
            var sequenceClassInfo = new MTFSequenceClassInfo
            {
                MTFClass = (MTFClassInfo)mtfClassInfo.Copy(),
                Alias = mtfClassInfo.Name
            };
            var classInstanceConfigs = componentsClient.ClassInstanceConfigurations(mtfClassInfo);
            if (classInstanceConfigs != null && classInstanceConfigs.Count == 1)
            {
                sequenceClassInfo.MTFClassInstanceConfiguration = classInstanceConfigs[0];
            }
            return sequenceClassInfo;
        }

        public void AdoptComponentToSequenceActivity(object component, int index, object targetItem, bool targetOnItem)
        {
            bool startSearch = false;
            if (component == null)
            {
                return;
            }

            MTFSequenceClassInfo sequenceClassInfo;
            if (component is SearchResultMethod)
            {
                startSearch = true;
                var compClassInfo = ((SearchResultMethod)component).MTFClassInfo;
                sequenceClassInfo = sequence.MTFSequenceClassInfos.FirstOrDefault(x => x.MTFClass.FullName == compClassInfo.FullName && x.Alias == ((SearchResultMethod)component).ClassAlias);
                if (sequenceClassInfo == null)
                {
                    sequenceClassInfo = GetSequenceClassInfoFromMTFClassInfo(compClassInfo);
                    sequenceClassInfo.AdjustName(sequence.MTFSequenceClassInfos);
                    sequence.MTFSequenceClassInfos.Add(sequenceClassInfo);
                    var result = SearchResults.FirstOrDefault(x => x.ClassInfo == ((SearchResultMethod)component).MTFClassInfo);
                    if (result != null)
                    {
                        result.IsClassNOTInSequence = false;
                    }
                }
                SearchText = string.Empty;
            }
            else if (component is SearchResult)
            {
                startSearch = true;
                var compClassInfo = ((SearchResult)component).ClassInfo;
                sequenceClassInfo = sequence.MTFSequenceClassInfos.FirstOrDefault(x => x.MTFClass.FullName == compClassInfo.FullName && x.Alias == ((SearchResult)component).Alias);
                if (sequenceClassInfo == null)
                {
                    sequenceClassInfo = GetSequenceClassInfoFromMTFClassInfo(compClassInfo);
                    sequenceClassInfo.AdjustName(sequence.MTFSequenceClassInfos);
                    sequence.MTFSequenceClassInfos.Add(sequenceClassInfo);
                    ((SearchResult)component).IsClassNOTInSequence = false;
                }
                SearchText = string.Empty;
            }
            else
            {
                sequenceClassInfo = component as MTFSequenceClassInfo;
            }
            if (sequenceClassInfo == null)
            {
                sequenceClassInfo = SelectedSequenceClassInfo;
            }

            if (Sequence.MTFSequenceActivities == null)
            {
                Sequence.MTFSequenceActivities = new MTFObservableCollection<MTFSequenceActivity>();
                NotifyPropertyChanged("Sequence");
            }
            MTFSequenceActivity newActivity = new MTFSequenceActivity()
            {
                //MTFClassAlias = sequenceClassInfo.Alias,
                ClassInfo = sequenceClassInfo,
                IsActive = true,
                ActivityName = sequenceClassInfo.Alias,
                Term = new EmptyTerm("System.Boolean"),
                OnError = MTFErrorBehavior.HandledByParent,
                OnCheckOutputFailed = MTFErrorBehavior.HandledByParent,
            };
            if (component is SearchResultMethod)
            {
                newActivity.MTFMethodName = ((SearchResultMethod)component).Name;
                newActivity.MTFMethodDisplayName = ((SearchResultMethod)component).DisplayName;
                newActivity.MTFMethodDescription = ((SearchResultMethod)component).Description;
                newActivity.SetupModeSupport = ((SearchResultMethod)component).SetupModeSupport;
                newActivity.UsedDataNames = ((SearchResultMethod)component).UsedDataNames;
                newActivity.ReturnType = ((SearchResultMethod)component).ResultType;
                newActivity.MTFParameters = ((SearchResultMethod)component).MTFParameters;
                NotifyPropertyChanged("SelectedClassExecutable");
            }
            PutActivityOnTheRightPlace(newActivity, index, targetItem, targetOnItem);
            if (startSearch && !string.IsNullOrEmpty(searchText))
            {
                Search(searchText);
            }
        }

        public void AdoptVariableToSequenceActivity(object variable, int index, object targetItem, bool targetOnItem)
        {
            var mtfVariable = variable as MTFVariable;
            if (mtfVariable != null)
            {
                MTFSequenceActivity activity;
                if (mtfVariable.HasValidationTable)
                {
                    var table = (MTFValidationTable)mtfVariable.Value;
                    activity = new MTFFillValidationTableActivity
                    {
                        IsActive = true,
                        ActivityName = string.Format("Fill {0}", table.Name),
                        Term = new ValidationTableTerm
                        {
                            ValidationTable = table
                        },
                        OnCheckOutputFailed = MTFErrorBehavior.HandledByParent,
                    };
                }
                else
                {
                    var value = GetDefaultValueByType(mtfVariable.TypeName);
                    activity = new MTFVariableActivity
                    {
                        IsActive = true,
                        ActivityName = string.Format("Set {0}", mtfVariable.Name),
                        Variable = mtfVariable,
                        Value = value
                    };
                }
                PutActivityOnTheRightPlace(activity, index, targetItem, targetOnItem);
            }
        }

        private Term GetDefaultValueByType(string variableTypeName)
        {
            if (!string.IsNullOrEmpty(variableTypeName))
            {
                var type = Type.GetType(variableTypeName);
                if (type != null)
                {
                    return new ConstantTerm(type) { Value = type == typeof(string) ? string.Empty : Activator.CreateInstance(type) };
                }
            }
            return new EmptyTerm();
        }


        public void PutActivityOnTheRightPlace(MTFSequenceActivity activity, int index, object targetItem, bool targetOnItem)
        {
            var subSequence = targetItem as MTFSubSequenceActivity;
            if (subSequence != null && targetOnItem)
            {
                var collection = GetCollectionFromSubsequence(subSequence);
                if (collection != null)
                {
                    if (subSequence.IsCollapsed)
                    {
                        subSequence.IsCollapsed = false;
                    }
                    collection.Add(activity);
                    activity.AdjustName();
                }
            }
            else
            {
                var collection = sequence.MTFSequenceActivities;
                if (targetItem is MTFSequenceActivity)
                {
                    collection = FindCollectionByParent(targetItem as MTFSequenceActivity);
                    if (collection == null)
                    {
                        collection = FindCollectionByActivity(Sequence.MTFSequenceActivities,
                        targetItem as MTFSequenceActivity);
                    }
                    if (collection == null)
                    {
                        collection = FindCollectionByActivity(Sequence.ActivitiesByCall,
                        targetItem as MTFSequenceActivity);
                    }
                }
                if (index < 0 || index >= collection.Count)
                {
                    collection.Add(activity);
                }
                else
                {
                    collection.Insert(index, activity);
                }

                activity.AdjustName();
                selectedSequenceActivity = activity;
            }
            LastSelectedItem = activity;
        }

        private void removeActivityFromSequence(object param)
        {
            MTFSequenceActivity sequenceActivity = param as MTFSequenceActivity;
            if (sequenceActivity != null)
            {
                if (!Sequence.MTFSequenceActivities.Remove(sequenceActivity))
                {
                    var collection = FindCollectionByParent(sequenceActivity);
                    if (collection != null)
                    {
                        collection.Remove(sequenceActivity);
                    }
                    else
                    {
                        collection = FindCollectionByActivity(Sequence.MTFSequenceActivities, sequenceActivity);
                        if (collection != null)
                        {
                            collection.Remove(sequenceActivity);
                        }
                        else
                        {
                            collection = FindCollectionByActivity(sequence.ActivitiesByCall, sequenceActivity);
                            if (collection != null)
                            {
                                collection.Remove(sequenceActivity);
                            }
                        }
                    }
                }
            }
        }

        public IList<MTFSequenceActivity> FindCollectionByParent(MTFSequenceActivity sequenceActivity)
        {
            if (sequenceActivity != null && sequenceActivity.Parent != null)
            {
                var subSequence = sequenceActivity.Parent as MTFSubSequenceActivity;
                if (subSequence != null)
                {
                    return subSequence.Activities;
                }
                var mtfCase = sequenceActivity.Parent as MTFCase;
                if (mtfCase != null)
                {
                    return mtfCase.Activities;
                }
            }
            return null;
        }

        public IList<MTFSequenceActivity> FindCollectionByActivity(IList<MTFSequenceActivity> collection, MTFSequenceActivity sequenceActivity)
        {
            if (collection == null)
            {
                collection = Activities;
            }
            IList<MTFSequenceActivity> outputCollection = null;
            if (sequenceActivity == null)
            {
                return collection;
            }
            if (collection.Contains(sequenceActivity))
            {
                return collection;
            }

            foreach (var item in collection)
            {
                if (item is MTFSubSequenceActivity)
                {
                    outputCollection = FindCollectionByActivity(((MTFSubSequenceActivity)item).Activities, sequenceActivity);
                }
                if (outputCollection != null)
                {
                    return outputCollection;
                }
            }
            return outputCollection;
        }


        private void FillCommands()
        {
            if (BuildInCommands == null)
            {
                BuildInCommands = new ObservableCollection<EnumValueDescription>();
            }
            foreach (var val in EnumHelper.GetAllValuesAndDescriptions<SequenceBaseCommands>())
            {
                BuildInCommands.Add(val);
            }
        }

        //private void MergeSequence(MTFSequence currentSequence)
        //{
        //    disableDragDrop = true;
        //    if (currentSequence == null)
        //    {
        //        return;
        //    }
        //    OpenSequenceAndMerge(currentSequence);
        //    disableDragDrop = false;
        //}

        //private void OpenSequenceAndMerge(MTFSequence currentSequence)
        //{
        //    bool loadSequence = false;
        //    var openDialog = new OpenSaveSequenceDialogContainer(OpenSaveSequencesDialog.DialogTypeEnum.OpenDialog, Constants.SequenceBasePath, new List<string>() { Constants.SequenceExtension },
        //        true, false);
        //    PopupWindow.PopupWindow pw = new PopupWindow.PopupWindow(openDialog) { Title = "Open Sequence for merging" };
        //    MTFSequence newSequence = null;
        //    if (pw.ShowDialog() == true)
        //    {
        //        LongTask.Do(() =>
        //        {
        //            var result = ((OpenSaveSequencesDialogPresenter)openDialog.DataContext).SelectedItem;
        //            newSequence = LoadSequenceFromServer(result.FullName);
        //            if (newSequence != null)
        //            {
        //                loadSequence = true;
        //            }
        //        }, "Loading sequence ...");

        //        var sharedData = new MergeSharedData(currentSequence, newSequence);
        //        List<MTFWizardUserControl> controls = new List<MTFWizardUserControl>()
        //            {
        //                new MergePreview(sharedData),
        //                new MergeComponents(sharedData),
        //                new MergeVariables(sharedData),
        //                new MergeSummary(sharedData),
        //            };

        //        var mergeDialog = new MTFWizardWindow(controls);
        //        mergeDialog.Owner = Application.Current.MainWindow;
        //        mergeDialog.ShowDialog();
        //        if (mergeDialog.Result == true)
        //        {
        //            MergeTwoSequences(currentSequence, newSequence, sharedData);
        //        }
        //        else
        //        {
        //            return;
        //        }

        //        if (!loadSequence)
        //        {
        //            MTFMessageBox.Show("Sequence error", "Error during loading sequence.", MTFMessageBoxType.Error, MTFMessageBoxButtons.Ok);
        //        }
        //    }
        //}



        private void MergeTwoSequences(MTFSequence currentSequence, MTFSequence newSequence, MergeSharedData sharedData)
        {
            if (currentSequence.MTFSequenceActivities != null)
            {
                MergeHelper.MergeActivities(currentSequence.MTFSequenceActivities, newSequence.MTFSequenceActivities, sharedData);
            }
            else
            {
                currentSequence.MTFSequenceActivities = newSequence.MTFSequenceActivities;
            }

            if (currentSequence.ActivitiesByCall != null)
            {
                MergeHelper.MergeActivities(currentSequence.ActivitiesByCall, newSequence.ActivitiesByCall, sharedData);
            }
            else
            {
                currentSequence.ActivitiesByCall = newSequence.ActivitiesByCall;
            }



            if (currentSequence.MTFSequenceClassInfos != null)
            {
                MergeHelper.MergeVariableOrComponent(currentSequence.MTFSequenceClassInfos, newSequence.MTFSequenceClassInfos, sharedData.MergeComponentsSetting);
            }
            else
            {
                currentSequence.MTFSequenceClassInfos = newSequence.MTFSequenceClassInfos;
            }

            if (currentSequence.MTFVariables != null)
            {
                MergeHelper.MergeVariableOrComponent(currentSequence.MTFVariables, newSequence.MTFVariables, sharedData.MergeVariablesSetting);
            }
            else
            {
                currentSequence.MTFVariables = newSequence.MTFVariables;
            }
        }

        private HashSet<Guid> GenerateIds(MTFSequence currentSequence)
        {
            HashSet<Guid> idCollection = new HashSet<Guid>();
            currentSequence.ForEachActivity(x => idCollection.Add(x.Id));
            foreach (var item in currentSequence.MTFSequenceClassInfos)
            {
                idCollection.Add(item.Id);
            }
            foreach (var item in currentSequence.MTFVariables)
            {
                idCollection.Add(item.Id);
            }
            return idCollection;
        }


        private async void ImportSequence()
        {
            var sequenceToOpen = ImportHelper.ImportSequence(MTFClient);
            if (sequenceToOpen != null)
            {
                await openSequence(sequenceToOpen, false);
                GenerateComponentsMapping();
            }
        }

        private void ExportSequence()
        {
            LoadAllSequences();
            ExportHelper.ExportSequence(mainSequence);
        }

        private void LoadAllSequences()
        {
            if (mtfProject.All(x => x.IsLoad))
            {
                return;
            }
            LongTask.Do(() =>
                        {
                            List<KeyValuePair<MTFSequence, MTFSequence>> tmpList = new List<KeyValuePair<MTFSequence, MTFSequence>>();
                            foreach (var item in mtfProject)
                            {
                                if (item != mainSequence)
                                {
                                    if (!item.IsLoad)
                                    {
                                        tmpList.Add(new KeyValuePair<MTFSequence, MTFSequence>(item, LoadSequence(item.FullPath, false)));
                                    }
                                    else
                                    {
                                        LoadInnerSequence(item);
                                    }
                                }
                            }

                            foreach (var item in tmpList)
                            {
                                LoadInnerSequence(item.Value);
                                UIHelper.InvokeOnDispatcher(()=>ReplaceLoadSequence(item.Value, item.Key));
                            }
                        }, LanguageHelper.GetString("Mtf_LongTask_LoadingAllSequences"));
        }

        private void LoadInnerSequence(MTFSequence sequence)
        {
            if (sequence.ExternalSubSequencesPath != null &&
                        (sequence.ExternalSubSequences == null || sequence.ExternalSubSequencesPath.Count > sequence.ExternalSubSequences.Count))
            {
                sequence.ExternalSubSequences = new List<MTFExternalSequenceInfo>();
                foreach (var sequencePath in sequence.ExternalSubSequencesPath)
                {
                    sequence.ExternalSubSequences.Add(new MTFExternalSequenceInfo(LoadSequence(DirectoryPathHelper.GetFullPathFromRelative(sequence.FullPath, sequencePath.Key)), sequencePath.Value));
                }
            }
        }

        private async void OpenSequenceDialog(bool addToProject)
        {
#if !DEBUG
            if (AcceptSave())
            {
                return;
            }
#endif
            var openDialog = new OpenSaveSequenceDialogContainer(DialogTypeEnum.OpenDialog, BaseConstants.SequenceBasePath, new List<string>() { BaseConstants.SequenceExtension }, true, false);
            PopupWindow.PopupWindow pw = new PopupWindow.PopupWindow(openDialog);
            pw.Title = LanguageHelper.GetString("Mtf_OpenDialog_OpenSeq");
            if (pw.ShowDialog() == true)
            {
                selectionHelper.Clear(true);
                var result = ((OpenSaveSequencesDialogPresenter)openDialog.DataContext).SelectedItem;
                await openSequence(result.FullName, addToProject);
                ShowTermDesigner = false;
                NotifyPropertyChanged(nameof(HeaderText));
                NotifyPropertyChanged(nameof(DisplaySubComponents));
                NotifyPropertyChanged("Commands");
                NotifyPropertyChanged(nameof(ValidationTables));
                NotifyPropertyChanged(nameof(Tables));
                RefreshAllowEditSequence(GetExecutionPresenter());
            }
        }

        private void GenerateComponentsMapping()
        {
            var isModified = mainSequence.IsModified;
            var subcomponents = new List<MTFSequenceClassInfo>();
            if (mainSequence.ComponetsMapping != null && mainSequence.ComponetsMapping.Count > 0)
            {
                foreach (var item in mtfProject)
                {
                    if (item.MTFSequenceClassInfos != null)
                    {
                        subcomponents.AddRange(item.MTFSequenceClassInfos);
                    }
                }
                foreach (var item in mainSequence.MTFSequenceClassInfos)
                {
                    item.SubComponents?.Clear();
                }
                foreach (var currentMapping in mainSequence.ComponetsMapping)
                {
                    var component = mainSequence.MTFSequenceClassInfos.FirstOrDefault(x => x.Id == currentMapping.Value);
                    if (component != null)
                    {
                        if (component.SubComponents == null)
                        {
                            component.SubComponents = new ObservableCollection<MTFSequenceClassInfo>();
                        }
                        var subComponent = subcomponents.FirstOrDefault(x => x.Id == currentMapping.Key);
                        if (subComponent != null)
                        {
                            subComponent.IsMapped = true;
                            component.SubComponents.Add(subComponent);
                        }
                    }
                }
            }
            mainSequence.IsModified = isModified;
            NotifyPropertyChanged(nameof(SubSequenceComponents));
        }

        public async Task<bool> SaveAll()
        {
            string userName = EnvironmentHelper.UserName;
            if (!CheckIfPossibleToSave())
            {
                return true;
            }
            List<MTFSequence> modifiedSequences = new List<MTFSequence>();
            ShowTermDesigner = false;
            cancelSaving = false;
            bool isSave = false;
            foreach (var item in mtfProject.Reverse())
            {
                sequence = item;
                NotifyPropertyChanged("Sequence");
                if (item.IsModified)
                {
                    isSave = true;
                    modifiedSequences.Add(item);
                    await SaveSequence(item, false, userName);
                }
            }
            if (!isSave)
            {
                modifiedSequences.Add(mainSequence);
                await SaveSequence(mainSequence, false, userName);
            }

            sendEmailAfterModification(modifiedSequences);

            return !cancelSaving;
        }

        public async void PerformSave(MTFSequence currentSequence, bool saveAs)
        {
            string userName = EnvironmentHelper.UserName;
            if (!CheckIfPossibleToSave())
            {
                return;
            }
            if (IsDifferentControlOpened)
            {
                SwichDifferentControl();
            }
            ShowTermDesigner = false;
            cancelSaving = false;
            await SaveSequence(currentSequence, saveAs, userName);

            sendEmailAfterModification(new[] { currentSequence });
        }

        private void sendEmailAfterModification(IEnumerable<MTFSequence> modifiedSequences)
        {
            if (!mainSequence.SendEmailAfterChanged)
            {
                return;
            }

            Task.Run(() =>
                EmailHelper.SendEmail(mainSequence.SmtpServer, mainSequence.MailTo, mainSequence.EmailSubject, EmailHelper.GenerateSequenceChangedEmail(modifiedSequences)));
        }

        public async Task SaveSequence(MTFSequence currentSequence, bool saveAs, string userName)
        {
            if (currentSequence == null || cancelSaving)
            {
                return;
            }
            if (currentSequence == mainSequence)
            {
                var dontSavedSequences = mtfProject.Where(x => x.IsModified && x != mainSequence);
                foreach (var item in dontSavedSequences)
                {
                    ChangeSequenceAndRefreshUI(item);
                    await SaveSequence(item, false, userName);
                }
                if (mainSequence.ExternalSubSequences != null && !string.IsNullOrEmpty(mainSequence.FullPath) && !cancelSaving)
                {
                    mainSequence.ExternalSubSequencesPath = mainSequence.ExternalSubSequences.ToDictionary(k => DirectoryPathHelper.GetRelativePath(Path.GetDirectoryName(mainSequence.FullPath), k.ExternalSequence.FullPath), v => v.IsEnabled);
                }
                else
                {
                    if ((mainSequence.ExternalSubSequences == null || mainSequence.ExternalSubSequences.Count == 0) && mainSequence.ComponetsMapping != null)
                    {
                        mainSequence.ComponetsMapping.Clear();
                    }
                }
            }
            if (!cancelSaving)
            {
                if (currentSequence.IsLoad)
                {
                    ChangeSequenceAndRefreshUI(currentSequence);
                    if (currentSequence.IsNew || saveAs)
                    {
                        SaveAsSequenceDialog(currentSequence, saveAs);
                    }
                    else
                    {
                        await Task.Run(() => PerformSaveSequence(currentSequence, false, userName));
                    }
                }
                else
                {
                    currentSequence.IsModified = false;
                }
            }
        }

        private MTFSequence LoadSequenceBackup(string fullFileName)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream reader = new FileStream(fullFileName, FileMode.Open);
            MTFSequence sequence;
            try
            {
                sequence = formatter.Deserialize(reader) as MTFSequence;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                reader.Close();
                reader.Dispose();
            }

            if (sequence != null)
            {
                sequence.ReplaceIdentityObjects();
            }

            return sequence;
        }

        private void OpenBackup()
        {
            OpenFileDialog f = new OpenFileDialog
            {
                InitialDirectory = Path.Combine(Environment.CurrentDirectory, SequenceBackupDirectoryName),
            };
            if (f.ShowDialog() == true)
            {
                LongTask.Do(() =>
                {
                    var tmp = LoadSequenceBackup(f.FileName);
                    var originSequence = mtfProject.FirstOrDefault(s => s.Id == tmp.Id);

                    if (originSequence != null)
                    {
                        tmp.ExternalSubSequences = originSequence.ExternalSubSequences;
                        tmp.FullPath = originSequence.FullPath;
                        tmp.IsLoad = true;

                        mtfProject[mtfProject.IndexOf(originSequence)] = tmp;
                        mainSequence = mtfProject.First();
                        mainSequence.ParentSequence = null;
                        Sequence = tmp;

                        NotifyPropertyChanged("Sequence");
                        NotifyPropertyChanged("HeaderText");
                        NotifyPropertyChanged("MainSequence");

                        GenerateComponentsMapping();
                    }
                }, LanguageHelper.GetString("Mtf_LongTask_LoadingSequence"));
            }
        }

        private const string SequenceBackupDirectoryName = "SequenceBackup";
        private void CleanOldBackups()
        {
            if (mtfSetting != null)
            {
                var backupsDir = Path.Combine(Environment.CurrentDirectory, SequenceBackupDirectoryName);
                foreach (var dir in Directory.GetDirectories(backupsDir))
                {
                    foreach (var backup in Directory.GetDirectories(dir))
                    {
                        if ((DateTime.Now - Directory.GetCreationTime(backup)) > new TimeSpan(0, mtfSetting.DeleteBackupPeriod, 0))
                        {
                            try
                            {
                                Directory.Delete(backup, true);
                            }
                            catch
                            {
                                //do notihg, just delete it in next turn
                            }
                        }
                    }

                    if (IsDirectoryEmpty(dir))
                    {
                        Directory.Delete(dir);
                    }
                }
            }
        }

        private bool IsDirectoryEmpty(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }

        private void CreateBackup()
        {
            if (!mtfProject.Any(s => s.IsModified))
            {
                return;
            }

            Task.Factory.StartNew(() =>
            {
                try
                {
                    var now = DateTime.Now;
                    string directoryName = Path.Combine(Environment.CurrentDirectory, SequenceBackupDirectoryName, MainSequence.Name, string.Format("{0}-{1:D2}-{2:D2}-{3:D2}-{4:D2}-{5:D2}", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second));
                    FileHelper.CreateDirectory(directoryName);
                    foreach (var mtfSequence in mtfProject)
                    {
                        if (mtfSequence.IsModified)
                        {
                            StoreSequenceBackup(mtfSequence, directoryName);
                        }
                    }
                }
                catch
                {
                    //backup failed, try it next time
                    previousBackupFailed = true;
                }
            });
        }

        private void StoreSequenceBackup(MTFSequence sequence, string directoryName)
        {
            string fullFileName = Path.Combine(directoryName, sequence.FullName);

            BinaryFormatter formatter = new BinaryFormatter();
            string name = Path.GetFileName(fullFileName);
            string path = fullFileName.Remove(fullFileName.Length - name.Length);

            FileHelper.CreateDirectory(path);

            FileStream writer;
            if (File.Exists(fullFileName))
            {
                writer = new FileStream(fullFileName, FileMode.Truncate);
            }
            else
            {
                writer = new FileStream(fullFileName, FileMode.CreateNew);
            }
            formatter.Serialize(writer, sequence);

            writer.Close();
            writer.Dispose();

            FileHelper.SetFileForEveryone(fullFileName);
        }


        private bool CheckIfPossibleToSave()
        {
            return CanSave() || MTFMessageBox.Show("Save sequence", "Sequence has been modified in executor.\n\nDo you really want to save sequence?",
                MTFMessageBoxType.ImportantQuestion, MTFMessageBoxButtons.YesNo) == MTFMessageBoxResult.Yes;
        }

        public bool CanSave()
        {
            var executionPresenter = GetExecutionPresenter();
            return executionPresenter == null || executionPresenter.OpenedSequence != OpenedSequenceFileName || !executionPresenter.SequenceWasModified;
        }

        private void ChangeSequenceAndRefreshUI(MTFSequence seq)
        {
            if (sequence != seq)
            {
                sequence = seq;
                NotifyPropertyChanged("Sequence");
                NotifyPropertyChanged("DisplaySubComponents");
                NotifyPropertyChanged("Commands");
            }
        }

        private void SaveAsSequenceDialog(MTFSequence currentSequence, bool saveAs)
        {
            var userName = EnvironmentHelper.UserName;
            var saveDialog = new OpenSaveSequenceDialogContainer(DialogTypeEnum.SaveDialog, BaseConstants.SequenceBasePath, new List<string>() { BaseConstants.SequenceExtension }, true, false);
            PopupWindow.PopupWindow pw = new PopupWindow.PopupWindow(saveDialog);
            pw.Title = "Save Sequence";
            bool saveAsNewFile = false;
            if (pw.ShowDialog() == true)
            {
                var result = ((OpenSaveSequencesDialogPresenter)saveDialog.DataContext).SelectedItem;
                if (mtfProject.Any(x => x.FullPath == result.FullName) && currentSequence.FullPath != result.FullName)
                {
                    MTFMessageBox.Show("Save Sequence Error", "You cannot override an open sequence.", MTFMessageBoxType.Error, MTFMessageBoxButtons.Ok);
                }
                else
                {
                    var replaceGuids = false;
                    MTFSequence newSequence = null;
                    if (currentSequence == mainSequence || currentSequence.IsNew)
                    {
                        newSequence = currentSequence;
                    }
                    else
                    {
                        saveAsNewFile = true;
                        newSequence = currentSequence.ShallowCopy() as MTFSequence;
                        //Hack: because parent is changed to newSequence
                        //currentSequence.MTFSequenceClassInfos = null;
                        //currentSequence.MTFSequenceClassInfos = newSequence.MTFSequenceClassInfos;
                        //endHack
                    }

                    newSequence.Name = result.Name;
                    newSequence.FullPath = result.FullName;
                    sequenceFullName = result.FullName;
                    if (newSequence == mainSequence)
                    {
                        newSequence.ExternalSubSequencesPath = GenerateRelativePathOfExternalSubSequences(currentSequence);
                        NotifyPropertyChanged("HeaderText");
                        if (saveAs)
                        {
                            newSequence.ReplaceGuids();
                        }
                    }
                    else
                    {
                        if (saveAs)
                        {
                            replaceGuids = true;
                        }
                    }
                    PerformSaveSequence(newSequence, replaceGuids, userName);
                    if (saveAsNewFile)
                    {
                        MTFMessageBox.Show("Sequence is saved", "Copy of this sequence was saved as new file:\n" + newSequence.FullPath, MTFMessageBoxType.Info, MTFMessageBoxButtons.Ok);
                    }
                }
            }
            else
            {
                cancelSaving = true;
            }
        }

        private Dictionary<string, bool> GenerateRelativePathOfExternalSubSequences(MTFSequence currentSequence)
        {
            var pathOfExternalSubSequences = new Dictionary<string, bool>();
            var dirPath = Path.GetDirectoryName(currentSequence.FullPath);
            if (!string.IsNullOrEmpty(dirPath))
            {
                if (currentSequence.ExternalSubSequences != null)
                {
                    foreach (var item in currentSequence.ExternalSubSequences)
                    {
                        pathOfExternalSubSequences.Add(DirectoryPathHelper.GetRelativePath(dirPath, item.ExternalSequence.FullPath), item.IsEnabled);
                    }
                }
                return pathOfExternalSubSequences;
            }
            else
            {
                return currentSequence.ExternalSubSequencesPath;
            }
        }

        public bool AllowEditSequence
        {
            get { return allowEditSequence; }
            set
            {
                allowEditSequence = value;
                NotifyPropertyChanged();
            }
        }

        public void InvalidateSequence()
        {
            reloadSequence = true;
        }

        private bool reloadClassInstanceConfigurations;
        public void InvalidateClassInstanceConfigurations()
        {
            reloadClassInstanceConfigurations = true;
        }

        public override async void Activated()
        {
            base.Activated();

            if (sequence != null && sequence.IsModified && reloadSequence)
            {
                var result = MTFMessageBox.Show("Sequence changed", "Sequence was changed by execution, but there are some changes in editor. Do you want to reload sequence and delete your changes?", MTFMessageBoxType.Question, MTFMessageBoxButtons.YesNo);
                if (result == MTFMessageBoxResult.Yes)
                {
                    var openedSequenceInExecution = MainWindowPresenter.SequenceExecutionPresenter.OpenedSequence;
                    if (openedSequenceInExecution != null)
                    {
                        await openSequence(openedSequenceInExecution, false, false);
                    }
                }
                return;
            }
            var executionPresenter = GetExecutionPresenter();
            if (reloadSequence || reloadSequenceAfterStart)
            {
                if (executionPresenter != null && !string.IsNullOrEmpty(executionPresenter.OpenedSequence))
                {
                    await openSequence(executionPresenter.OpenedSequence, false, false);
                    executionPresenter.NotifySequenceIsActual();
                }
            }
            else if (reloadClassInstanceConfigurations && mainSequence?.MTFSequenceClassInfos != null)
            {
                ReloadClassInstanceConfigurations();
            }
            RefreshAllowEditSequence(executionPresenter);

            if (navigationHandler.NavigateAfterActivated)
            {
                switch (navigationHandler.NavigationMode)
                {
                    case NavigationMode.Activity:
                        NavigateToActivity(navigationHandler.TargetActivity, false);
                        navigationHandler.TargetActivity = null;
                        break;
                    case NavigationMode.GuidPath:
                        SelectActivityById(navigationHandler.TargetGuidPath, false);
                        navigationHandler.TargetGuidPath = null;
                        break;
                }
            }
            reloadSequenceAfterStart = false;
        }

        //reload is needed if live semrad's list is modified in component config
        private void ReloadClassInstanceConfigurations()
        {
            reloadClassInstanceConfigurations = false;
            foreach (var classInfo in mainSequence.MTFSequenceClassInfos.Where(ci => ci.MTFClassInstanceConfiguration != null))
            {
                var newClassInstanceConfiguration = componentsClient.ClassInstanceConfigurations(classInfo.MTFClass).FirstOrDefault(cic => cic.Name == classInfo.MTFClassInstanceConfiguration.Name);
                if (newClassInstanceConfiguration != null)
                {
                    classInfo.MTFClassInstanceConfiguration = newClassInstanceConfiguration;
                }
            }
            selectionHelper.ClearAndInvalidate(true);
        }

        private void RefreshAllowEditSequence(SequenceExecutionPresenter executionPresenter)
        {
            AllowEditSequence = executionPresenter == null || executionPresenter.SequenceIsNotRunning || executionPresenter.OpenedSequence != OpenedSequenceFileName;
        }

        private void PerformSaveSequence(MTFSequence currentSequence, bool replaceGuids, string userName)
        {
            LongTask.Do(() =>
                        {
                            GoldSampleHelper.GetNewGsDataFileName(currentSequence);
                            try
                            {
                                sequenceWasModified = true;
                                MTFClient.SaveSequence(currentSequence, currentSequence.FullPath, replaceGuids, userName);
                            }
                            catch (Exception ex)
                            {
                                MTFMessageBox.Show("MTF Error", ex.Message, MTFMessageBoxType.Error, MTFMessageBoxButtons.Ok);
                            }
                            currentSequence.IsNew = false;
                            currentSequence.IsModified = false;

                            InvalidateSequenceInExecution();
                        },
                LanguageHelper.GetString("Mtf_LongTask_SavingSeq"));
        }

        private void InvalidateSequenceInExecution()
        {
            var sequenceExecutionPresenter = GetExecutionPresenter();
            if (sequenceExecutionPresenter != null &&
                (sequenceExecutionPresenter.ExecutionState == MTFSequenceExecutionState.None ||
                sequenceExecutionPresenter.ExecutionState == MTFSequenceExecutionState.Stopped ||
                sequenceExecutionPresenter.ExecutionState == MTFSequenceExecutionState.Finished ||
                sequenceExecutionPresenter.ExecutionState == MTFSequenceExecutionState.Aborted))
            {
                sequenceExecutionPresenter.InvalidateSequence();
            }
        }

        public void NotifySequenceIsActual()
        {
            sequenceWasModified = false;
        }



        private SequenceExecutionPresenter GetExecutionPresenter()
        {
            return UIHelper.InvokeOnDispatcher(() => MainWindowPresenter.SequenceExecutionPresenter);
        }

        private bool UpdateActivitiesParams(MTFSequence sequence)
        {
            bool modified = false;
            var removeVersionReg = new Regex(@"Version=[\d|.]*");
            sequence.ForEachActivity(activity =>
            {
                if (activity?.ClassInfo?.MTFClass != null && !string.IsNullOrEmpty(activity.MTFMethodName))
                {
                    var method = activity.ClassInfo.MTFClass.Methods.FirstOrDefault(m => m.Name == activity.MTFMethodName);
                    if (method != null)
                    {
                        IEnumerable<MTFParameterValue> paramToDel = activity.MTFParameters.Where(p => !method.Parameters.Any(mp => mp.Name == p.Name && removeVersionReg.Replace(mp.TypeName, string.Empty) == removeVersionReg.Replace(p.TypeName, string.Empty))).ToArray();
                        foreach (var p in paramToDel)
                        {
                            modified = true;
                            activity.MTFParameters.Remove(p);
                            Warning.Add(new WarningMessage { Message = $"{activity.ActivityName} : ({p.TypeName}){p.Name} removed"});
                        }

                        IEnumerable<MTFParameterInfo> paramToAdd = method.Parameters.Where(mp => !activity.MTFParameters.Any(p => p.Name == mp.Name && removeVersionReg.Replace(mp.TypeName, string.Empty) == removeVersionReg.Replace(p.TypeName, string.Empty)));
                        foreach (var mp in paramToAdd)
                        {
                            modified = true;
                            activity.MTFParameters.Insert(method.Parameters.IndexOf(mp), new MTFParameterValue(mp));
                            Warning.Add(new WarningMessage { Message = $"{activity.ActivityName} : ({mp.TypeName}){mp.Name} added"});
                        }

                        bool isOrderCorrect = true;
                        for (int i = 0; i < activity.MTFParameters.Count; i++)
                        {
                            if (activity.MTFParameters[i].Name != method.Parameters[i].Name)
                            {
                                isOrderCorrect = false;
                            }
                        }
                        if (!isOrderCorrect)
                        {
                            modified = true;
                            Warning.Add(new WarningMessage { Message = $"{activity.ActivityName} : parameters order changed"});
                            //sort activity params by method param order
                            IList<MTFParameterValue> newParams = new List<MTFParameterValue>();
                            foreach (var p in method.Parameters)
                            {
                                newParams.Add(activity.MTFParameters.First(ap => ap.Name == p.Name));
                            }
                            activity.MTFParameters = newParams;
                        }

                        //fix setup mode support
                        if (activity.SetupModeSupport != method.SetupModeSupport)
                        {
                            if (activity.SetupModeSupport)
                            {
                                Warning.Add(new WarningMessage { Message = $"{activity.ActivityName} : Setup mode support available for this activity"});
                            }
                            else
                            {
                                Warning.Add(new WarningMessage { Message = $"{activity.ActivityName} : Setup mode support isn't available on this activity"});
                            }

                            activity.SetupModeSupport = method.SetupModeSupport;
                        }

                        //fix used data names
                        IList<string> usedDataToAdd = new List<string>();
                        IList<string> usedDataToRemove = new List<string>();
                        if (activity.UsedDataNames != null && method.UsedDataNames == null)
                        {
                            usedDataToRemove = activity.UsedDataNames.ToList();
                        }
                        if (activity.UsedDataNames == null && method.UsedDataNames != null)
                        {
                            usedDataToAdd = method.UsedDataNames.ToList();
                        }

                        if (method.UsedDataNames != null && activity.UsedDataNames != null)
                        {
                            usedDataToRemove = activity.UsedDataNames.Where(d => !method.UsedDataNames.Contains(d)).ToList();
                            usedDataToAdd = method.UsedDataNames.Where(d => !activity.UsedDataNames.Contains(d)).ToList();
                        }

                        foreach (var dataName in usedDataToRemove)
                        {
                            modified = true;
                            activity.UsedDataNames.Remove(dataName);
                            Warning.Add(new WarningMessage { Message = $"{activity.ActivityName} : removed used data {dataName} from activity"});
                        }
                        foreach (var dataName in usedDataToAdd)
                        {
                            modified = true;
                            if (activity.UsedDataNames == null)
                            {
                                activity.UsedDataNames = new List<string>();
                            }
                            activity.UsedDataNames.Add(dataName);
                            Warning.Add(new WarningMessage { Message = $"{activity.ActivityName} : added used data {dataName} from activity"});
                        }
                    }
                    else if (activity.MTFMethodName.EndsWith(".Set") || activity.MTFMethodName.EndsWith(".Get"))
                    {
                        var propertyName = activity.MTFMethodName.Remove(activity.MTFMethodName.IndexOf('.'));
                        var property = activity.ClassInfo.MTFClass.Properties.FirstOrDefault(m => m.Name == propertyName);
                        if (property != null)
                        {
                            if (!property.CanRead && activity.MTFMethodName.EndsWith(".Get"))
                            {
                                Warning.Add(new WarningMessage { Message = $"{activity.ActivityName} : property {activity.MTFMethodName} in component {activity.ClassInfo?.MTFClass?.Name} isn't readable " });
                            }
                            if (!property.CanWrite && activity.MTFMethodName.EndsWith(".Set"))
                            {
                                Warning.Add(new WarningMessage { Message = $"{activity.ActivityName} : property {activity.MTFMethodName} in component {activity.ClassInfo?.MTFClass?.Name} isn't writable " });
                            }

                        }
                        else
                        {
                            Warning.Add(new WarningMessage { Message = $"{activity.ActivityName} : property {activity.MTFMethodName} not found in component {activity.ClassInfo?.MTFClass?.Name} " });
                        }
                    }
                    else
                    {
                        Warning.Add(new WarningMessage { Message = $"{activity.ActivityName} : method {activity.MTFMethodName} not found in component {activity.ClassInfo?.MTFClass?.Name} "});
                    }
                }
            });

            return modified;
        }

        public MTFSequence LoadSequence(string sequenceFullPath)
        {
            return LoadSequence(sequenceFullPath, true);
        }

        public MTFSequence LoadSequence(string sequenceFullPath, bool invalidateSequenceInExecution)
        {
            reloadSequence = false;
            var newSequence = MTFClient.LoadSequence(sequenceFullPath);

            //UpdateSequenceClassInfos(newSequence);
            if (newSequence != null)
            {
                if (newSequence.MTFVersion != null)
                {
                    var currentVersion = new Version(Assembly.GetExecutingAssembly().GetName().Version);
                    if (newSequence.MTFVersion > currentVersion)
                    {
                        var msg =
                            "Sequence cannot be loaded because it was created in the higher version of MTF than you have.\nPlease update your MTF and try it again!";
                        MTFMessageBox.Show(LanguageHelper.GetString("Msg_Header_Error"), msg, MTFMessageBoxType.Error,
                            MTFMessageBoxButtons.Ok);
#if !DEBUG
                        return null; 
#endif
                    }
                }

                bool modified = false;
                try
                {
                    modified = UpdateActivitiesParams(newSequence);
                }
                catch (Exception e)
                {
                    MTFMessageBox.Show("Error", "Error during update activities parameters: " + e, MTFMessageBoxType.Error, MTFMessageBoxButtons.Ok);
                }

                newSequence.ParentSequence = mainSequence;
                newSequence.PropertyChanged += sequence_PropertyChanged;
                sequenceFullName = sequenceFullPath;
                newSequence.FullPath = sequenceFullName;
                newSequence.Name = Path.GetFileNameWithoutExtension(sequenceFullPath);
                newSequence.IsLoad = true;
                newSequence.IsModified = modified;
            }
            else
            {
                newSequence = new MTFSequence
                {
                    Name = Path.GetFileNameWithoutExtension(sequenceFullPath),
                    FullPath = sequenceFullPath,
                    IsModified = false
                };
                MTFMessageBox.Show("Error", "The sequence " + sequenceFullPath + " could not be found.", MTFMessageBoxType.Error, MTFMessageBoxButtons.Ok);
            }

            if (invalidateSequenceInExecution)
            {
                InvalidateSequenceInExecution();
            }

            return newSequence;
        }

        private async Task openSequence(string sequenceName, bool addToProject)
        {
            await openSequence(sequenceName, addToProject, true);
        }

        private async Task openSequence(string sequenceName, bool addToProject, bool invalidateSequenceInEditor)
        {
            await Task.Run(() => PerformOpenSequence(sequenceName, addToProject, invalidateSequenceInEditor));
        }

        private void PerformOpenSequence(string sequenceName, bool addToProject, bool invalidateSequenceInEditor)
        {
            bool allowAdd = true;
            bool errorDuringLoad = false;
            var badSequences = new List<string>();

            LongTask.Do(() =>
            {
                var tmp = LoadSequence(sequenceName, invalidateSequenceInEditor);
                if (tmp == null)
                {
                    return;
                }
                if (!addToProject)
                {
                    UIHelper.InvokeOnDispatcher(mtfProject.Clear);
                    CloseFindUsageWindow();
                    sequenceWasModified = false;
                }
                else
                {
                    allowAdd = mtfProject.All(x => x.FullPath != tmp.FullPath);
                }

                if (allowAdd)
                {
                    sequence = tmp;
                    NotifyPropertyChanged(nameof(Sequence));
                    NotifyPropertyChanged(nameof(HeaderText));
                    UIHelper.InvokeOnDispatcher(() => mtfProject.Add(sequence));
                    mainSequence = mtfProject.First();
                    mainSequence.ParentSequence = null;
                    if (sequence.ComponetsMapping == null)
                    {
                        sequence.ComponetsMapping = new Dictionary<Guid, Guid>();
                    }
                    if (sequence == mainSequence && sequence.ExternalSubSequencesPath?.Count > 0)
                    {
                        mainSequence.ExternalSubSequences = new List<MTFExternalSequenceInfo>();

                        foreach (var item in sequence.ExternalSubSequencesPath)
                        {
                            var s = new MTFSequence
                            {
                                IsNew = false,
                                Name = Path.GetFileNameWithoutExtension(item.Key),
                                FullPath = DirectoryPathHelper.GetFullPathFromRelative(sequence.FullPath, item.Key),
                            };
                            var classInfos = MTFClient.LoadSequenceClassInfo(s.FullPath);
                            if (classInfos != null)
                            {
                                s.MTFSequenceClassInfos = classInfos;
                                s.IsModified = false;
                                UIHelper.InvokeOnDispatcher(() => mtfProject.Add(s));
                                mainSequence.ExternalSubSequences.Add(new MTFExternalSequenceInfo(s, item.Value));
                            }
                            else
                            {
                                badSequences.Add(s.FullPath);
                                errorDuringLoad = true;
                            }

                        }
                    }
                    if (addToProject)
                    {
                        mainSequence.IsModified = true;
                        if (mainSequence.ExternalSubSequences == null)
                        {
                            mainSequence.ExternalSubSequences = new List<MTFExternalSequenceInfo>();
                        }
                        mainSequence.ExternalSubSequences.Add(new MTFExternalSequenceInfo(sequence));
                    }
                    NotifyPropertyChanged("Commands");
                    NotifyPropertyChanged(nameof(MainSequence));
                    //sequence.IsModified = false;
                }
            }, LanguageHelper.GetString("Mtf_LongTask_LoadingSequence"));
            if (!allowAdd)
            {
                MTFMessageBox.Show("Add Sequence Error", "This SubSequence already exists.", MTFMessageBoxType.Error, MTFMessageBoxButtons.Ok);
            }
            if (errorDuringLoad)
            {
                MTFMessageBox.Show("Load Error",
                    $"These sequences were not found:{Environment.NewLine}{string.Join(Environment.NewLine, badSequences)}",
                    MTFMessageBoxType.Error, MTFMessageBoxButtons.Ok);
            }

            if (invalidateSequenceInEditor)
            {
                GetExecutionPresenter()?.InvalidateSequence();
            }
            GenerateComponentsMapping();

            var currentVersion = new Version(1, 8, 2, 0);
            if (sequence.MTFVersion == null || sequence.MTFVersion < currentVersion)
            {
                LoadAllSequences();
                VersionConvertHelper.ConvertCallActivities(MTFProject);
            }
        }



        public void InsertNewCommandAsActivity(SequenceBaseCommands command, MTFSequenceActivity firstActivityInCollection,
            int targetIndex, object targetItem, bool targetOnItem, bool insertToCallListBox)
        {
            MTFSequenceActivity actvity = GetActivityFromCommand(command);
            if (actvity == null)
            {
                SystemLog.LogMessage("Null activity has been added.", true);
                return;
            }
            bool acceptInsertToCallListBox = false;
            if (actvity is MTFSubSequenceActivity)
            {
                acceptInsertToCallListBox = insertToCallListBox;
            }
            var subsequence = targetItem as MTFSubSequenceActivity;
            if (subsequence != null && targetOnItem)
            {
                var collection = GetCollectionFromSubsequence(subsequence);
                if (collection != null)
                {
                    if (subsequence.IsCollapsed)
                    {
                        subsequence.IsCollapsed = false;
                    }
                    collection.Add(actvity);
                }
                else
                {
                    return;
                }
            }
            else
            {
                FindCollectionAndInsert(firstActivityInCollection, actvity, targetIndex, acceptInsertToCallListBox);
            }
            actvity.AdjustName();
            //LastSelectedItem = null;
            LastSelectedItem = actvity;
        }


        public IList<MTFSequenceActivity> GetCollectionFromSubsequence(MTFSubSequenceActivity subSequence)
        {
            if (subSequence.ExecutionType == ExecutionType.ExecuteByCase)
            {
                if (subSequence.Cases != null && subSequence.Cases.Count > 0)
                {
                    if (subSequence.ActualCaseIndex >= 0 && subSequence.ActualCaseIndex < subSequence.Cases.Count)
                    {
                        return subSequence.Cases[subSequence.ActualCaseIndex].Activities;
                    }
                    return subSequence.Cases[0].Activities;
                }
            }
            else
            {
                return subSequence.Activities;
            }
            return null;
        }

        private MTFSequenceActivity GetActivityFromCommand(SequenceBaseCommands command)
        {
            MTFSequenceActivity activity = null;
            if (command == SequenceBaseCommands.CreateSubSequence)
            {
                activity = CreateSubSequenceActivity();
            }
            else if (command == SequenceBaseCommands.ExecuteSubSequence)
            {
                activity = CreateExecuteActivity();
            }
            else if (command == SequenceBaseCommands.SequenceHandling)
            {
                activity = CreateSequenceHandlingActivity();
            }
            else if (command == SequenceBaseCommands.ErrorHandling)
            {
                activity = CreateErrorHandlingActivity();
            }
            else if (command == SequenceBaseCommands.ShowMessage)
            {
                activity = CreateShowMessageActivity();
            }
            return activity;
        }

        private MTFSequenceActivity CreateSubSequenceActivity()
        {
            return new MTFSubSequenceActivity()
                {
                    MTFClassAlias = String.Empty,
                    Activities = new MTFObservableCollection<MTFSequenceActivity>(),
                    IsActive = true,
                    ActivityName = ActivityNameConstants.SubSequence,
                    MTFMethodName = string.Empty,
                    MTFMethodDisplayName = string.Empty,
                    SetupModeSupport = false,
                    OnError = MTFErrorBehavior.HandledByParent,
                    OnCheckOutputFailed = MTFErrorBehavior.HandledByParent,
                    Term = new EmptyTerm("System.Boolean"),
                    ActivityOnCatch = null
                };
        }



        private MTFSequenceActivity CreateShowMessageActivity()
        {
            return new MTFSequenceMessageActivity
            {
                MTFClassAlias = string.Empty,
                IsActive = true,
                ActivityName = "ShowMessage",
                MTFMethodName = string.Empty,
                MTFMethodDisplayName = string.Empty,
                SetupModeSupport = false,
                MessageType = SequenceMessageType.Info,
                Header = new MTFStringFormat() { Text = string.Empty },
                Message = new MTFStringFormat() { Text = string.Empty },
                Buttons = MessageActivityButtons.Ok,
                Term = new EmptyTerm(),
                Image = new EmptyTerm(),
                OnCheckOutputFailed = MTFErrorBehavior.HandledByParent,
            };
        }

        private MTFSequenceActivity CreateErrorHandlingActivity()
        {
            return new MTFErrorHandlingActivity
            {
                MTFClassAlias = string.Empty,
                IsActive = true,
                ActivityName = "ErrorHandling",
                MTFMethodName = string.Empty,
                MTFMethodDisplayName = string.Empty,
                SetupModeSupport = false,
                ReturnType = typeof(bool).FullName,
                RaiseError = new MTFStringFormat(),
                Term = new EmptyTerm(typeof(bool).FullName),
                ErrorHandlingType = ErrorHandlingType.CheckErrors,
                OnError = MTFErrorBehavior.HandledByParent,
                OnCheckOutputFailed = MTFErrorBehavior.HandledByParent,
            };

        }

        private MTFSequenceActivity CreateSequenceHandlingActivity()
        {
            return new MTFSequenceHandlingActivity
            {
                MTFClassAlias = string.Empty,
                IsActive = true,
                ActivityName = "SequenceHandling",
                MTFMethodName = string.Empty,
                MTFMethodDisplayName = string.Empty,
                SetupModeSupport = false,
                ReturnType = typeof(bool).FullName,
                Term = new EmptyTerm(),
                SequenceHandlingType = SequenceHandlingType.SaveReportAndCleanErrors,
                StatusLines = null,
                Logs = null,
                ClearAllTables = true,
                //ClearTimeView = true,
                TablesSetting = FillTablesSetting(),
                CommandsSetting = FillCommandsSetting(),
                UserCommandsSetting = FillUserCommandsSettings(),
                SaveToTxt = true,
                RestartTimer = true,
                SetStatus = true,
                IncludeValidationTables = true,
                CleanErrorMemory = true,
                CleanErrorWindow = false,
                CleanTables = false,
                LogMessage = new MTFStringFormat(),
                LogCycleName = new MTFStringFormat(),
            };

        }


        private MTFSequenceActivity CreateExecuteActivity()
        {
            return new MTFExecuteActivity()
            {
                MTFClassAlias = "ExecuteSubSequence",
                IsActive = true,
                ActivityName = "CallSubSequence",
                MTFMethodName = string.Empty,
                MTFMethodDisplayName = string.Empty,
                SetupModeSupport = false,
                ActivityToCall = null,
                DynamicMethod = new MTFStringFormat(),
                DynamicActivityType = MTFClientServerCommon.DynamicActivityTypes.Load,
                DynamicSequence = new MTFStringFormat(),
                Type = ExecuteActyvityTypes.Local,
            };
        }


        private IList<TableSetting> FillTablesSetting()
        {
            return ValidationTables.Select(table => new TableSetting() { ValidationTable = table }).ToList();
        }

        private IList<ServiceCommandsSetting> FillCommandsSetting()
        {
            return Sequence.ServiceCommands != null ? Sequence.ServiceCommands.Select(command => new ServiceCommandsSetting { ServiceCommand = command }).ToList() : null;
        }

        private IList<UserCommandsState> FillUserCommandsSettings()
        {
            return Sequence.UserCommands != null ? Sequence.UserCommands.Where(c => c.Type == MTFUserCommandType.Button || c.Type == MTFUserCommandType.ToggleButton)
                .Select(command => new UserCommandsState { UserCommand = command }).ToList() : null;
        }

        private void FindCollectionAndInsert(MTFSequenceActivity firstActivityInCollection, MTFSequenceActivity activity, int targetIndex, bool callListBox)
        {
            if (firstActivityInCollection != null && firstActivityInCollection.Parent != null)
            {
                var subSequence = firstActivityInCollection.Parent as MTFSubSequenceActivity;
                if (subSequence != null)
                {
                    if (subSequence.Activities != null)
                    {
                        if (targetIndex >= 0 && targetIndex < subSequence.Activities.Count)
                        {
                            subSequence.Activities.Insert(targetIndex, activity);
                        }
                        else
                        {
                            subSequence.Activities.Add(activity);
                        }
                    }
                    return;
                }
                var mtfCase = firstActivityInCollection.Parent as MTFCase;
                if (mtfCase != null)
                {
                    if (mtfCase.Activities != null)
                    {
                        if (targetIndex >= 0 && targetIndex < mtfCase.Activities.Count)
                        {
                            mtfCase.Activities.Insert(targetIndex, activity);
                        }
                        else
                        {
                            mtfCase.Activities.Add(activity);
                        }
                    }
                    return;
                }
            }
            if (callListBox)
            {
                var subSequenceActivity = activity as MTFSubSequenceActivity;
                if (subSequenceActivity != null)
                {
                    subSequenceActivity.ExecutionType = ExecutionType.ExecuteByCall;
                }
                sequence.ActivitiesByCall.Insert(targetIndex, activity);
            }
            else
            {
                var collection = FindCollectionByActivity(Sequence.MTFSequenceActivities, firstActivityInCollection) ??
                                 FindCollectionByActivity(Sequence.ActivitiesByCall, firstActivityInCollection);
                collection.Insert(targetIndex, activity);
            }
        }

        public void InsertActivityIntoCollection(IList<MTFSequenceActivity> collection,
            int indexInCollection, MTFSequenceActivity insertedActivity, bool setSelectedItem)
        {
            if (insertedActivity == null)
            {
                SystemLog.LogMessage("Null activity has been added.", true);
                return;
            }
            if (indexInCollection == -1 || indexInCollection > collection.Count)
            {
                indexInCollection = collection.Count;
            }
            collection.Insert(indexInCollection, insertedActivity);
            insertedActivity.AdjustName();

            if (setSelectedItem)
            {
                LastSelectedItem = insertedActivity;
            }
        }

        public void InsertActivityIntoCollection(IList<MTFSequenceActivity> collection, MTFSequenceActivity insertedActivity, bool setSelectedItem)
        {
            InsertActivityIntoCollection(collection, insertedActivity, false, setSelectedItem);
        }

        public void InsertActivityIntoCollection(IList<MTFSequenceActivity> collection, MTFSequenceActivity insertedActivity, bool replaceIdentityObjects, bool setSelectedItem)
        {
            if (insertedActivity == null)
            {
                SystemLog.LogMessage("Null activity has been added.", true);
                return;
            }
            collection.Add(insertedActivity);
            insertedActivity.AdjustName();
            if (replaceIdentityObjects)
            {
                sequence.ReplaceIdentityObjects();
            }
            LastSelectedItem = insertedActivity;
        }



        public void MoveToCallActivities(MTFSubSequenceActivity subSequence)
        {
            var collection = FindCollectionByActivity(sequence.MTFSequenceActivities, subSequence) ??
                             FindCollectionByActivity(sequence.ActivitiesByCall, subSequence);
            if (collection != null && collection.Contains(subSequence))
            {
                collection.Remove(subSequence);
                sequence.ActivitiesByCall.Add(subSequence);
            }
            LastSelectedItem = subSequence;
        }

        public void MoveFromCallActivities(MTFSubSequenceActivity subSequence)
        {
            var collection = FindCollectionByActivity(sequence.ActivitiesByCall, subSequence);
            if (Equals(collection, sequence.ActivitiesByCall) && collection.Contains(subSequence))
            {
                collection.Remove(subSequence);
                sequence.MTFSequenceActivities.Add(subSequence);
            }
            LastSelectedItem = subSequence;
        }


        public void VerifyExecutionType(ItemCollection itemCollection, object targetItem, bool targetOnItem)
        {
            if (itemCollection.Count > 0)
            {
                var collection = FindCollectionByParent(itemCollection[0] as MTFSequenceActivity);
                if (collection == null)
                {
                    collection = FindCollectionByActivity(sequence.ActivitiesByCall, itemCollection[0] as MTFSequenceActivity);
                }
                if (collection == null)
                {
                    collection = FindCollectionByActivity(sequence.MTFSequenceActivities, itemCollection[0] as MTFSequenceActivity);
                }
                if (collection.Equals(Sequence.ActivitiesByCall) && !targetOnItem)
                {
                    for (int i = 0; i < collection.Count; i++)
                    {
                        var subSequenceActivity = collection[i] as MTFSubSequenceActivity;
                        if (subSequenceActivity != null)
                        {
                            subSequenceActivity.ExecutionType = ExecutionType.ExecuteByCall;
                        }
                    }
                }
                else
                {
                    if (targetOnItem && targetItem is MTFSubSequenceActivity)
                    {
                        SetDefaultExecutionType((targetItem as MTFSubSequenceActivity).Activities);
                    }
                    else
                    {
                        SetDefaultExecutionType(collection);
                    }

                }
            }
        }

        public ICommand SelectDestinationPathCommand
        {
            get { return new Command(SelectDestinationPath); }
        }

        private void SelectDestinationPath()
        {
            var folderDialog = new OpenSaveSequenceDialogContainer(DialogTypeEnum.FileSystemDialog, string.Empty, BaseConstants.ImageExtensions, false, true);
            var popup = new PopupWindow.PopupWindow(folderDialog) { Title = "Select image" };

            if (popup.ShowDialog() == true)
            {
                ((MTFSequenceMessageActivity)selectedSequenceActivity).PathToImage = ((OpenSaveSequencesDialogPresenter)folderDialog.DataContext).SelectedItem.FullName;
            }
        }

        ExecutionType defaultType = ExecutionType.ExecuteAlways;

        private void SetDefaultExecutionType(IEnumerable collection)
        {
            foreach (var item in collection)
            {
                if (item is MTFSubSequenceActivity && (item as MTFSubSequenceActivity).ExecutionType == ExecutionType.ExecuteByCall)
                {
                    (item as MTFSubSequenceActivity).ExecutionType = defaultType;
                }
            }
        }

        public void ShowSequence(string sequenceName)
        {
            var s = mtfProject.FirstOrDefault(x => x.Name == sequenceName);
            if (s != null)
            {
                Sequence = s;
            }
        }


        public IEnumerable<MTFValidationTable> ValidationTables
        {
            get
            {
                if (Sequence != null && Sequence.MTFVariables != null)
                {
                    return Sequence.MTFVariables.Where(x => x.TypeName == typeof(MTFValidationTable).FullName).Select(x => x.Value as MTFValidationTable);
                }
                return null;
            }
        }

        public IEnumerable<IMTFTable> Tables
        {
            get
            {
                if (Sequence != null && Sequence.MTFVariables != null)
                {
                    return
                        Sequence.MTFVariables.Where(x => x.HasTable).Select(x => x.Value as IMTFTable);
                }
                return null;
            }
        }

        private object selectedInitRow;

        public object SelectedInitRow
        {
            get { return selectedInitRow; }
            set
            {
                selectedInitRow = value;
                NotifyPropertyChanged();
            }
        }

        //private void SetActualVariant()
        //{
        //    if (sequence == null)
        //    {
        //        return;
        //    }
        //    var variantDialog = new SetVariantDialog(sequence, actualVariant);
        //    var dialog = new PopupWindow.PopupWindow(variantDialog) { Title = "Set sequence variant" };
        //    dialog.ShowDialog();
        //    if (dialog.MTFDialogResult != null && dialog.MTFDialogResult.Result == MTFDialogResultEnum.Ok)
        //    {
        //        actualVariant = variantDialog.SequenceVariant;
        //        UpdateVariant(actualVariant);
        //    }
        //}

        //private void UpdateVariant(SequenceVariant variant)
        //{
        //    SequenceVariantHelper.SwitchSequence(sequence, variant);
        //}




        public bool IsFindUsagesWindowOpened
        {
            get { return isFindUsagesWindowOpened; }
            set
            {
                isFindUsagesWindowOpened = value;
                NotifyPropertyChanged();
            }
        }

        private void toggleIsFindUsagesOpened(object obj)
        {
            if (findUsagesWindow != null)
            {
                if (IsFindUsagesWindowOpened)
                {
                    findUsagesWindow.Hide();
                }
                else
                {
                    findUsagesWindow.Show();
                }
            }
            IsFindUsagesWindowOpened = !IsFindUsagesWindowOpened;
            mainCommands.ForEach(c => c.RaiseCanExecuteChanged());
        }

        public ICommand CreateFindUsagesCommand
        {
            get { return new Command(CreateFindUsages); }
        }

        private void CreateFindUsages(object param)
        {
            var variable = param as MTFVariable;
            CloseFindUsageWindow();
            var findUsagesSetting = new FindUsagesSetting(Application.Current.MainWindow, Sequence, SelectActivityFromFindUsages, GetExternalSequences);
            if (variable != null)
            {
                findUsagesWindow = new FindVariableUsagesWindow(findUsagesSetting, variable);
                OpenFindUsagesWindow(findUsagesWindow);
                return;
            }
            var classInfo = param as MTFSequenceClassInfo;
            if (classInfo != null)
            {
                findUsagesWindow = new FindComponentUsagesWindow(findUsagesSetting, classInfo, mainSequence.ComponetsMapping.Where(x => x.Value == classInfo.Id).Select(x => x.Key));
                OpenFindUsagesWindow(findUsagesWindow);
                return;
            }
            var generalCommand = param as EnumValueDescription;
            if (generalCommand != null)
            {
                var generalCommandActivity = GetActivityFromCommand((SequenceBaseCommands)generalCommand.Value);
                if (generalCommandActivity != null)
                {
                    findUsagesWindow = new FindMtfHandlingUsagesWindow(findUsagesSetting, generalCommandActivity.GetType(), generalCommandActivity.Icon);
                    OpenFindUsagesWindow(findUsagesWindow);
                }
                return;
            }

        }

        private void OpenFindUsagesWindow(FindUsagesBase window)
        {
            window.Closed += findUsagesWindow_Closed;
            window.OnHide += findUsagesWindow_OnHide;
            window.Show();
            isFindUsagesCreated = true;
            IsFindUsagesWindowOpened = true;
            GenerateMainCommands();
            mainCommands.ForEach(c => c.RaiseCanExecuteChanged());
            NotifyPropertyChanged("Commands");
        }

        public ICommand CreateFindUsagesFromActivityCommand
        {
            get { return new Command(CreateFindUsagesFromActivity); }
        }

        public ListBox LastSelectedListBox
        {
            get { return lastSelectedListBox; }
            set { lastSelectedListBox = value; }
        }

        public SelectionHelper SelectionHelper
        {
            get { return selectionHelper; }
        }

        public ComponentDataHandler DataHandler
        {
            get { return componentDataHandler; }
        }

        private void CreateFindUsagesFromActivity(object param)
        {
            var subSequence = param as MTFSubSequenceActivity;
            var executeActivity = param as MTFExecuteActivity;
            //var externalActivity = param as MTFExternalActivity;
            var activity = param as MTFSequenceActivity;
            CloseFindUsageWindow();
            var findUsagesSetting = new FindUsagesSetting(Application.Current.MainWindow, Sequence, SelectActivityFromFindUsages, GetExternalSequences);

            if (subSequence != null)
            {
                findUsagesWindow = new FindSubSequenceUsagesWindow(findUsagesSetting, subSequence, SelectCommandFromFindUsages);
                OpenFindUsagesWindow(findUsagesWindow);
            }
            else if (executeActivity != null)
            {
                var callSubSequence = executeActivity.ActivityToCall;
                if (callSubSequence != null)
                {
                    findUsagesWindow = new FindSubSequenceUsagesWindow(findUsagesSetting, callSubSequence, SelectCommandFromFindUsages);
                    OpenFindUsagesWindow(findUsagesWindow);
                }
            }
            //else if (externalActivity != null)
            //{

            //}
            else if (activity != null && activity.ClassInfo != null)
            {
                findUsagesWindow = new FindComponentUsagesWindow(findUsagesSetting, activity.ClassInfo,
                    mainSequence.ComponetsMapping.Where(x => x.Value == activity.ClassInfo.Id).Select(x => x.Key), activity.MTFMethodDisplayName);
                OpenFindUsagesWindow(findUsagesWindow);
            }
        }

        private IEnumerable<MTFSequence> GetExternalSequences()
        {
            LoadAllSequences();
            return MTFProject;
        }

        void findUsagesWindow_OnHide(object sender, EventArgs e)
        {
            IsFindUsagesWindowOpened = false;
            mainCommands.ForEach(c => c.RaiseCanExecuteChanged());
        }

        void findUsagesWindow_Closed(object sender, EventArgs e)
        {
            CloseFindUsageWindow();
        }

        private void CloseFindUsageWindow()
        {
            if (findUsagesWindow != null)
            {
                findUsagesWindow.Closed -= findUsagesWindow_Closed;
                findUsagesWindow.OnHide -= findUsagesWindow_OnHide;
                UIHelper.InvokeOnDispatcher(findUsagesWindow.Close);
                findUsagesWindow = null;
                isFindUsagesCreated = false;
                GenerateMainCommands();
                NotifyPropertyChanged("Commands");
            }

        }

        private void SelectActivityFromFindUsages(MTFSequenceActivity activity, MTFSequence currentSequence)
        {
            if (currentSequence != null && currentSequence != Sequence)
            {
                Sequence = currentSequence;
            }
            activity.ExpandParentSubSequence();
            selectionHelper.Add(activity);
            //TODO nefunguje
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                new Action(
                    () =>
                    {
                        var listBox = UIHelper.FindListBoxByActivity(activity, Application.Current.MainWindow);
                        if (listBox != null)
                        {
                            listBox.ScrollIntoView(activity);
                        }
                    }
                    ));

        }

        private void SelectCommandFromFindUsages(MTFServiceCommand command, MTFSequence currentSequence)
        {
            if (currentSequence != null && currentSequence != Sequence)
            {
                Sequence = currentSequence;
            }
            selectionHelper.ClearAndInvalidate();
            LastSelectedItem = command;
            IsServiceCommandsActive = true;
        }

        public IEnumerable<MTFUserCommand> Indicators => Sequence?.UserCommands?.Where(c => c.Type == MTFUserCommandType.IndicatorGrayGreen || c.Type == MTFUserCommandType.IndicatorRedGreen || c.Type == MTFUserCommandType.IndicatorGrayRed);

    }

    public enum SequenceBaseCommands
    {
        [Description("Activity_BuildIn_SubSeq")]
        CreateSubSequence,
        [Description("Activity_BuildIn_Call")]
        ExecuteSubSequence,
        [Description("Activity_BuildIn_SeqHandling")]
        SequenceHandling,
        [Description("Activity_BuildIn_ErrHandling")]
        ErrorHandling,
        [Description("Activity_BuildIn_ShowMsg")]
        ShowMessage,
    }

    public class SearchResult : NotifyPropertyBase
    {
        public MTFClassInfo ClassInfo
        {
            get;
            set;
        }

        private bool isClassNOTInSequence;

        public bool IsClassNOTInSequence
        {
            get { return isClassNOTInSequence; }
            set
            {
                isClassNOTInSequence = value;
                NotifyPropertyChanged();
            }
        }

        public List<SearchResultMethod> Methods
        {
            get;
            set;
        }

        public string Alias
        {
            get;
            set;
        }
    }

    public class SearchResultMethod
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string ResultType { get; set; }
        public string ClassAlias { get; set; }
        public bool SetupModeSupport { get; set; }
        public IList<string> UsedDataNames { get; set; }
        public MTFClassInfo MTFClassInfo { get; set; }
        public ObservableCollection<MTFParameterValue> MTFParameters { get; set; }
    }

}
