using MTFApp.UIHelpers;
using MTFClientServerCommon;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using AutomotiveLighting.MTFCommon;
using MTFApp.Managers;
using MTFApp.Managers.Components;
using MTFApp.ServerService;
using MTFApp.ServerService.Components;
using MTFClientServerCommon.Helpers;

namespace MTFApp.ComponentConfig
{
    class ComponentConfigPresenter : PresenterBase, IMainCommands
    {
        private Command saveSettingsCommand;
        private Command removeAllSettingsCommand;
        private Command copyAsNewConfigCommand;
        private Command testCommand;
        private Command loadValueListCommand;

        private MTFClassInfo selectedMonsterClass;
        private List<MTFClassInstanceConfiguration> LocalClassInstancesCfg;
        private MTFClassCategory selectedCategory;
        object selectedComponentCfg = null;
        private string createInstanceInfo;

        private readonly ComponentsClient componentsClient;

        public ComponentConfigPresenter()
        {
            componentsClient = ServiceClientsContainer.Get<ComponentsClient>();
            saveSettingsCommand = new Command(SaveConfiguration, () => SelectedMonsterClass != null)
                                  {
                                      Icon = MTFIcons.SaveFile,
                                      Name = "MainCommand_Save",
                                      KeyShortucuts = new[] { new CommandShortcut { Key = Key.S, Modifier = ModifierKeys.Control } }
                                  };
            removeAllSettingsCommand = new Command(RemoveAllInstance, () => selectedMonsterClass != null)
                                       {
                                           Icon = MTFIcons.RemoveTab,
                                           Name = "MainCommand_RemoveAll"
                                       };
            copyAsNewConfigCommand = new Command(() => CopyConfiguration(selectedComponentCfg), () => selectedComponentCfg is MTFClassInstanceConfiguration)
                                     {
                                         Icon = MTFIcons.Copy,
                                         Name = "MainCommand_CopyCfg"
                                     };
            testCommand = new Command(TestConstructor, () => selectedComponentCfg is MTFClassInstanceConfiguration)
                          {
                              Icon = MTFIcons.Test,
                              Name = "MainCommand_Test"
                          };
            loadValueListCommand = new Command(LoadValueLists, () => selectedComponentCfg is MTFClassInstanceConfiguration)
                                   {
                                       Icon = MTFIcons.TermIsInList,
                                       Name = "MainCommand_LoadList"
                                   };
            componentsClient.ComponentConfigOnQuestion += MTFClient_ComponentConfigOnQuestion;
        }

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
            mainCommands = new List<Command>
            {
                saveSettingsCommand,
                removeAllSettingsCommand,
                copyAsNewConfigCommand,
                testCommand,
                loadValueListCommand,
            };
        }

        private void updateButtons()
        {
            foreach (var button in mainCommands)
            {
                button.RaiseCanExecuteChanged();
            }
        }

        string MTFClient_ComponentConfigOnQuestion(string header, string text, SequenceMessageType messageType, SequenceMessageDisplayType displayType, IList<string> items)
        {
            PopupWindow.PopupWindow pw = null;

            var messageInfo = new MessageInfo
                              {
                                  Text = text,
                                  Type = messageType,
                                  ChoiceDisplayType = displayType,
                                  ButtonValues = items.ToList(),
                                  Buttons = MessageButtons.Ok
                              };
            Application.Current.Dispatcher.Invoke(() =>
            {
                pw = new PopupWindow.PopupWindow(new MessageBoxControl.MessageBoxControl(messageInfo)) { Title = header };
                pw.ShowDialog();
            });

            return pw.MTFDialogResult == null ? string.Empty : pw.MTFDialogResult.TextResult;
        }

        public ObservableCollection<MTFClassCategory> MTFClassCategories => ManagersContainer.Get<ComponentsManager>().MTFClassCategories;

        public MTFClassInfo SelectedMonsterClass
        {
            get { return selectedMonsterClass; }
            set
            {
                selectedMonsterClass = value;
                if (selectedMonsterClass != null)
                {
                    System.Threading.Tasks.Task.Run(() =>
                    {
                        LocalClassInstancesCfg = componentsClient.ClassInstanceConfigurations(selectedMonsterClass);

                        System.Windows.Application.Current.Dispatcher.Invoke(() => NotifyPropertyChanged("ConfiguredComponents"));
                        selectedComponentCfg = selectedMonsterClass;
                        System.Windows.Application.Current.Dispatcher.Invoke(() => NotifyPropertyChanged("SelectedComponentCfg"));
                        createInstanceInfo = string.Empty;
                        System.Windows.Application.Current.Dispatcher.Invoke(() => NotifyPropertyChanged("CreateInstanceInfo"));
                    });
                }
                updateButtons();
            }
        }

        public string CreateInstanceInfo
        {
            get { return createInstanceInfo; }
            set
            {
                createInstanceInfo = value;
                NotifyPropertyChanged();
            }
        }


        public MTFClassCategory SelectedCategory
        {
            get { return selectedCategory; }
            set
            {
                selectedCategory = value;
                //LocalClassInstancesCfg = MTFClient.ClassInstanceConfigurations(selectedMonsterClass);
                NotifyPropertyChanged("ConfiguredComponents");
                selectedComponentCfg = selectedCategory;
                NotifyPropertyChanged("SelectedComponentCfg");
                createInstanceInfo = string.Empty;
                NotifyPropertyChanged("CreateInstanceInfo");
            }
        }


        public object SelectedTreeViewItem
        {
            get { return selectedMonsterClass; }
            set
            {
                if (value is MTFClassInfo)
                {
                    SelectedCategory = null;
                    SelectedMonsterClass = value as MTFClassInfo;
                }
                if (value is MTFClassCategory)
                {
                    SelectedMonsterClass = null;
                    SelectedCategory = value as MTFClassCategory;
                }
            }
        }

        public ICommand CreateNewCommand
        {
            get { return new Command(CreateNewInstance); }
        }

        public ICommand RemoveInstanceCommand
        {
            get { return new Command(RemoveInstance); }
        }

        public ICommand RemoveAllInstanceCommand
        {
            get { return new Command(RemoveAllInstance); }
        }

        public ICommand SaveConfigurationCommand
        {
            get { return new Command(SaveConfiguration); }
        }

        public ICommand CopyConfigurationCommand
        {
            get { return new Command(CopyConfiguration); }
        }

        public ICommand TestConstructorCommand
        {
            get { return new Command(TestConstructor); }
        }

        public ICommand LoadValueListsCommand
        {
            get { return new Command(LoadValueLists); }
        }

        public object SelectedComponentCfg
        {
            get
            {
                return selectedComponentCfg;
            }
            set
            {
                updateParamsByParameterHelper();
                selectedComponentCfg = value;
                createInstanceInfo = string.Empty;
                prepareParameterHelper(selectedComponentCfg as MTFClassInstanceConfiguration);
                fillHelperByClassInfo(selectedComponentCfg as MTFClassInstanceConfiguration);

                NotifyPropertyChanged("CreateInstanceInfo");
                updateButtons();
            }
        }

        private List<MTFParameterDescriptor> parameterHelper;
        public List<MTFParameterDescriptor> ParameterHelper
        {
            get { return parameterHelper; }
            set
            {
                if (parameterHelper != null)
                {
                    foreach (var paramHelper in parameterHelper)
                    {
                        paramHelper.PropertyChanged -= paramHelper_PropertyChanged;
                    }
                }
                parameterHelper = value;
                if (parameterHelper != null)
                {
                    foreach (var paramHelper in parameterHelper)
                    {
                        paramHelper.PropertyChanged += paramHelper_PropertyChanged;
                    }
                }

                NotifyPropertyChanged("ParameterHelper");
            }
        }

        private List<MTFParameterDescriptor> GetParameterHelperClone()
        {
            var parameterHelperClone = new List<MTFParameterDescriptor>(parameterHelper.Count());
            foreach (MTFParameterDescriptor parameterDescriptor in parameterHelper)
            {
                parameterHelperClone.Add(parameterDescriptor.Clone() as MTFParameterDescriptor);
            }

            return parameterHelperClone;
        }

        private void updateParamsByParameterHelper()
        {
            if (parameterHelper == null)
            {
                return;
            }

            foreach (var paramDescription in parameterHelper)
            {
                //parameter control listbox and text input is handled automatically -> no handling here
                if (paramDescription.ControlType == MTFParameterControlType.TabControl || paramDescription.ControlType == MTFParameterControlType.DataTable)
                {
                    //var tabControls = paramDescription.Value as List<MTFTabControl>;
                    var instanceConfig = SelectedComponentCfg as MTFClassInstanceConfiguration;

                    if (instanceConfig != null)
                    {
                        var paramValue = instanceConfig.ParameterValues.FirstOrDefault(p => p.Name == paramDescription.ParameterName);
                        paramValue.Value = paramDescription.Value;
                    }
                }
            }
        }

        private bool paramHelperPropertyChanging = false;
        void paramHelper_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (paramHelperPropertyChanging)
            {
                return;
            }
            paramHelperPropertyChanging = true;

            var instanceConfig = SelectedComponentCfg as MTFClassInstanceConfiguration;
            var paramDescriptior = sender as MTFParameterDescriptor;
            if (instanceConfig == null || paramDescriptior == null)
            {
                return;
            }

            var param = instanceConfig.ParameterValues.FirstOrDefault(p => p.Name == paramDescriptior.ParameterName);
            if (param != null)
            {
                param.Value = paramDescriptior.Value;
            }

            //find correct constructor by parameters
            var classInstanceConfiguration = selectedComponentCfg as MTFClassInstanceConfiguration;
            if (classInstanceConfiguration == null)
            {
                return;
            }
            var constructor = getConstructor(classInstanceConfiguration.ClassInfo, classInstanceConfiguration.ParameterValues);
            if (constructor != null && !string.IsNullOrEmpty(constructor.ParameterHelperClassName))
            {
                var parameterHelperClone = new List<MTFParameterDescriptor>(parameterHelper.Count());
                for (int i = 0; i < parameterHelper.Count; i++)
                {
                    parameterHelperClone.Add(parameterHelper[i].Clone() as MTFParameterDescriptor);
                }

                if (!fillHelperByClassInfoInProgress)
                {
                    componentsClient.ParameterDescriptionChanged(constructor.ParameterHelperClassName, ref parameterHelperClone, classInstanceConfiguration.ClassInfo.FullPathName);
                }

                foreach (var paramHelper in parameterHelperClone)
                {
                    updateParamHelper(parameterHelper.FirstOrDefault(p => p.ParameterName == paramHelper.ParameterName), paramHelper);
                }
            }
            paramHelperPropertyChanging = false;
        }

        private void updateParamHelper(MTFParameterDescriptor oldParam, MTFParameterDescriptor newData)
        {
            if (oldParam == null)
            {
                return;
            }

            oldParam.Value = newData.Value;
            oldParam.IsEnabled = newData.IsEnabled;
            oldParam.DisplayName = newData.DisplayName;
            oldParam.Description = newData.Description;
            oldParam.ControlType = newData.ControlType;

            bool isEqual = true;

            if (oldParam.AllowedValues != null && newData.AllowedValues != null)
            {
                if (oldParam.AllowedValues.Count() == newData.AllowedValues.Count())
                {
                    for (int i = 0; i < oldParam.AllowedValues.Count(); i++)
                    {
                        isEqual = isEqual && oldParam.AllowedValues[i].Equals(newData.AllowedValues[i]);
                    }
                }
                else
                {
                    isEqual = false;
                }
            }
            else
            {
                isEqual = false;
            }

            if (!isEqual)
            {
                oldParam.AllowedValues = newData.AllowedValues;
            }
        }

        private void prepareParameterHelper(MTFClassInstanceConfiguration classInstanceConfiguration)
        {
            if (classInstanceConfiguration == null)
            {
                ParameterHelper = null;
                return;
            }

            //find correct constructor by parameters
            var constructor = getConstructor(classInstanceConfiguration.ClassInfo, classInstanceConfiguration.ParameterValues);
            if (constructor != null && !string.IsNullOrEmpty(constructor.ParameterHelperClassName))
            {
                //get parameter helper form server
                ParameterHelper = componentsClient.GetParameterDescription(constructor.ParameterHelperClassName, classInstanceConfiguration.ClassInfo.FullPathName);
                parameterHelper.ForEach(h => h.DontRaiseNotifyPropertyChanged = true);
                foreach (var descriptor in parameterHelper)
                {
                    var param = constructor.Parameters.FirstOrDefault(p => p.Name == descriptor.ParameterName);
                    if (param != null)
                    {
                        updateDescriptorByParameter(descriptor, param);
                    }
                }
                //replace empty display name with parameter name
                foreach (var paramDescriptor in ParameterHelper.Where(pd => string.IsNullOrEmpty(pd.DisplayName)))
                {
                    paramDescriptor.DisplayName = paramDescriptor.ParameterName;
                }
                parameterHelper.ForEach(h => h.DontRaiseNotifyPropertyChanged = false);
            }
            else
            {
                ParameterHelper = null;
            }
        }

        private void updateDescriptorByParameter(MTFParameterDescriptor descriptor, MTFParameterInfo parameterInfo)
        {
            if (string.IsNullOrEmpty(descriptor.Description))
            {
                descriptor.Description = parameterInfo.Description;
            }
            if (string.IsNullOrEmpty(descriptor.TypeName))
            {
                descriptor.TypeName = parameterInfo.TypeName;
            }
            if (string.IsNullOrEmpty(descriptor.DisplayName))
            {
                descriptor.DisplayName = parameterInfo.DisplayName;
            }
        }

        private bool fillHelperByClassInfoInProgress;
        private void fillHelperByClassInfo(MTFClassInstanceConfiguration classInstanceConfiguration)
        {
            if (classInstanceConfiguration == null || ParameterHelper == null)
            {
                return;
            }

            fillHelperByClassInfoInProgress = true;
            foreach (var paramHelper in ParameterHelper)
            {
                var classParam = classInstanceConfiguration.ParameterValues.FirstOrDefault(p => p.Name == paramHelper.ParameterName);
                if (classParam != null)
                {
                    paramHelper.Value = classParam.Value;
                }

            }

            fillHelperByClassInfoInProgress = false;
            ////find correct constructor by parameters
            var constructor = getConstructor(classInstanceConfiguration.ClassInfo, classInstanceConfiguration.ParameterValues);

            if (UseLegacyParameterHelperHandling)
            {
                var parameterHelperClone = new List<MTFParameterDescriptor>(parameterHelper.Count());
                for (int i = 0; i < parameterHelper.Count; i++)
                {
                    parameterHelperClone.Add(parameterHelper[i].Clone() as MTFParameterDescriptor);
                }
                componentsClient.ParameterDescriptionChanged(constructor.ParameterHelperClassName, ref parameterHelperClone, classInstanceConfiguration.ClassInfo.FullPathName);
            }
            else
            {
                componentsClient.ParameterDescriptionChanged(constructor.ParameterHelperClassName, ref parameterHelper, classInstanceConfiguration.ClassInfo.FullPathName);
                ParameterHelper = parameterHelper;
            }
        }

        //used because of Joerg's ALVision driver -> new parameter halper handling is not working with this driver. 
        //Information about using this legacy mode is shown in component configuration -> remove binding when isn't needed
        public bool UseLegacyParameterHelperHandling
        {
            get
            {
                return selectedMonsterClass != null && selectedMonsterClass.AssemblyName == "ALVisionToMTFWrapperClient.dll";
            }
        }

        private void fillClassInfoByHelper(MTFClassInstanceConfiguration classInstanceConfiguration)
        {
            if (classInstanceConfiguration == null || ParameterHelper == null)
            {
                return;
            }

            foreach (var paramHelper in ParameterHelper)
            {
                var classParam = classInstanceConfiguration.ParameterValues.FirstOrDefault(p => p.Name == paramHelper.ParameterName);
                if (classParam != null)
                {
                    classParam.Value = paramHelper.Value;
                }

            }
        }

        private MTFConstructorInfo getConstructor(MTFClassInfo classInfo, IEnumerable<MTFParameterValue> parameters)
        {
            foreach (MTFConstructorInfo constuctor in classInfo.Constructors.Where(c => c.Parameters.Count == parameters.Count()))
            {
                int i = 0;
                bool areEqual = true;
                foreach (var param in parameters)
                {
                    areEqual = areEqual && constuctor.Parameters[i].TypeName == param.TypeName && constuctor.Parameters[i].Name == param.Name;
                    i++;
                }
                if (areEqual)
                {
                    return constuctor;
                }
            }

            return null;
        }

        public IList<object> ConfiguredComponents
        {
            get
            {
                var configuredComponents = new List<object>();
                if (selectedCategory != null)
                {
                    configuredComponents.Add(selectedCategory);
                    return configuredComponents;
                }
                if (selectedMonsterClass == null)
                {
                    return null;
                }
                configuredComponents.Add(selectedMonsterClass);
                //LocalClassInstancesCfg = MTFClient.ClassInstanceConfigurations(selectedMonsterClass);
                configuredComponents.AddRange(LocalClassInstancesCfg);

                return configuredComponents;
            }
        }


        private void TestConstructor()
        {
            createInstanceInfo = componentsClient.TestClassInstance(selectedComponentCfg as MTFClassInstanceConfiguration);
            NotifyPropertyChanged("CreateInstanceInfo");
            if (createInstanceInfo == "OK")
            {
                MTFMessageBox.Show(LanguageHelper.GetString("Msg_Header_InstanceTest"),
                    LanguageHelper.GetString("Msg_Body_InstanceOk"), MTFMessageBoxType.Info, MTFMessageBoxButtons.Ok);
            }
            else
            {
                MTFMessageBox.Show(LanguageHelper.GetString("Msg_Header_InstanceTest"), createInstanceInfo, MTFMessageBoxType.Error,
                    MTFMessageBoxButtons.Ok);
            }
        }

        private void LoadValueLists()
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    MTFClassInstanceConfiguration classInstanceCfg =
                        selectedComponentCfg as MTFClassInstanceConfiguration;
                    classInstanceCfg.ValueLists = componentsClient.LoadValueLists(classInstanceCfg);
                }
                catch (Exception e)
                {
                    MTFMessageBox.Show(LanguageHelper.GetString("Msg_Header_Error"), e.Message, MTFMessageBoxType.Error, MTFMessageBoxButtons.Ok);
                }
            });
        }

        private void CreateNewInstance(object parameter)
        {
            MTFConstructorInfo constructorInfo = parameter as MTFConstructorInfo;
            if (constructorInfo == null)
            {
                return;
            }

            var newComponentCfg =
                new MTFClassInstanceConfiguration(
                    string.Format("{0} {1}", LanguageHelper.GetString("BaseString_New"), selectedMonsterClass.Name),
                    selectedMonsterClass, constructorInfo);
            UpdateConfigurationName(newComponentCfg);
            LocalClassInstancesCfg.Add(newComponentCfg);

            prepareParameterHelper(newComponentCfg);
            fillClassInfoByHelper(newComponentCfg);

            NotifyPropertyChanged("ConfiguredComponents");
            selectedComponentCfg = newComponentCfg;
            NotifyPropertyChanged("SelectedComponentCfg");

            updateButtons();
        }

        private void RemoveInstance(object param)
        {
            var item = ((MTFClassInstanceConfiguration)param);
#if !DEBUG
            var result = MTFMessageBox.Show(LanguageHelper.GetString("Msg_Header_RemoveComponentConf"),
                $"{LanguageHelper.GetString("Msg_Body_RemoveComponentConf")}{Environment.NewLine}{Environment.NewLine}{item.Name}",
                MTFMessageBoxType.Question, MTFMessageBoxButtons.YesNo);
            if (result == MTFMessageBoxResult.No)
            {
                return;
            }
#endif
            LocalClassInstancesCfg.Remove(LocalClassInstancesCfg.FirstOrDefault(i => i.Id == item.Id));
            NotifyPropertyChanged("ConfiguredComponents");
            selectedComponentCfg = ConfiguredComponents[0];
            NotifyPropertyChanged("SelectedComponentCfg");
        }

        private void RemoveAllInstance()
        {
#if !DEBUG
            var result = MTFMessageBox.Show(LanguageHelper.GetString("Msg_Header_RemoveComponentConf"),
                LanguageHelper.GetString("Msg_Body_RemoveAllComponentConf"),
                MTFMessageBoxType.Question, MTFMessageBoxButtons.YesNo);
            if (result == MTFMessageBoxResult.No)
            {
                return;
            }
#endif
            LocalClassInstancesCfg.Clear();
            NotifyPropertyChanged("ConfiguredComponents");
            selectedComponentCfg = ConfiguredComponents[0];
            NotifyPropertyChanged("SelectedComponentCfg");
        }

        private void SaveConfiguration()
        {
            var editorPresenter = GetEditorPresenter();
            if (editorPresenter != null)
            {
                editorPresenter.InvalidateClassInstanceConfigurations();
            }

            updateParamsByParameterHelper();
            if (LocalClassInstancesCfg != null)
            {
                try
                {
                    componentsClient.SaveClassInstanceConfiguration(LocalClassInstancesCfg, selectedMonsterClass);
                }
                catch (Exception ex)
                {
                    MTFMessageBox.Show(LanguageHelper.GetString("Msg_Header_Error"), ex.Message, MTFMessageBoxType.Error, MTFMessageBoxButtons.Ok);
                }
            }
        }

        private void CopyConfiguration(object param)
        {
            var newComponentCfg = (param as MTFDataTransferObject).Clone() as MTFClassInstanceConfiguration;
            UpdateConfigurationName(newComponentCfg);
            LocalClassInstancesCfg.Add(newComponentCfg);
            selectedComponentCfg = newComponentCfg;
            NotifyPropertyChanged("ConfiguredComponents");
            NotifyPropertyChanged("SelectedComponentCfg");
        }

        public void UpdateConfigurationName(MTFClassInstanceConfiguration classInstanceConfiguration)
        {
            classInstanceConfiguration.AdjustName(LocalClassInstancesCfg);
        }

        private SequenceEditor.SequenceEditorPresenter GetEditorPresenter()
        {
            return ((MainWindowPresenter)Application.Current.MainWindow.DataContext).SequenceEditorPresenter;
        }

        public override void Activated()
        {
            base.Activated();

            if (selectedMonsterClass != null)
            {
                LocalClassInstancesCfg = componentsClient.ClassInstanceConfigurations(selectedMonsterClass);
            }
            NotifyPropertyChanged("ConfiguredComponents");
        }
    }
}
