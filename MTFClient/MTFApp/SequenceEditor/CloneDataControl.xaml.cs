using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using MTFApp.PopupWindow;
using MTFClientServerCommon;

namespace MTFApp.SequenceEditor
{
    /// <summary>
    /// Interaction logic for CloneDataControl.xaml
    /// </summary>
    public partial class CloneDataControl : UserControl, IRaiseCloseEvent, IReturnsDialogResult, INotifyPropertyChanged
    {
        private readonly ClassInfoData originalData;
        private readonly string dataPackageName;
        private readonly MTFSequenceClassInfo selectedClassInfo;
        private readonly MTFSequence selectedSequence;
        private readonly ObservableCollection<ClassInfoData> dataVariants = new ObservableCollection<ClassInfoData>();
        private bool removeOriginal;
        private bool showError;
        private string errorMsg;

        public CloneDataControl(ClassInfoData originalData, string dataPackageName, MTFSequenceClassInfo selectedClassInfo, MTFSequence selectedSequence)
        {
            this.originalData = originalData;
            this.dataPackageName = dataPackageName;
            this.selectedClassInfo = selectedClassInfo;
            this.selectedSequence = selectedSequence;
            InitializeComponent();
            Root.DataContext = this;
        }

        public ClassInfoData OriginalData => originalData;

        public MTFSequence Sequence => selectedSequence;

        public ObservableCollection<ClassInfoData> DataVariants => dataVariants;

        public bool RemoveOriginal
        {
            get => removeOriginal;
            set
            {
                removeOriginal = value;
                NotifyPropertyChanged();
            }
        }

        public bool ShowError
        {
            get => showError;
            set
            {
                showError = value;
                NotifyPropertyChanged();
            }
        }

        public string ErrorMsg
        {
            get => errorMsg;
            set
            {
                errorMsg = value;
                NotifyPropertyChanged();
            }
        }


        private void AddEmptyClick(object sender, RoutedEventArgs e)
        {
            dataVariants.Add(ClassInfoData(originalData.Data));
        }

        private void AddOriginalClick(object sender, RoutedEventArgs e)
        {
            if (originalData != null && originalData.SequenceVariant != null)
            {
                dataVariants.Add(ClassInfoData(originalData.Data, originalData.SequenceVariant));
            }
        }

        private void CloneClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var original = button.CommandParameter as ClassInfoData;
            if (original != null && original.SequenceVariant != null && originalData != null)
            {
                var newData = ClassInfoData(originalData.Data, original.SequenceVariant);
                var index = dataVariants.IndexOf(original);
                if (index != -1)
                {
                    dataVariants.Insert(index + 1, newData);
                }
                else
                {
                    dataVariants.Add(newData);
                }
            }
        }

        private ClassInfoData ClassInfoData(byte[] originalData, SequenceVariant variant = null)
        {
            var cid = new ClassInfoData
            {
                Data = originalData,
                LastModifiedTime = DateTime.Now,
                SequenceVariant = variant == null ? new SequenceVariant() : variant.Clone() as SequenceVariant,
            };
            return cid;
        }

        private void OkClick(object sender, RoutedEventArgs e)
        {
            if (!FindDuplicity())
            {
                Save(sender);
            }
        }

        private bool FindDuplicity()
        {
            string duplicity = string.Empty;
            ShowError = false;
            var hashSet = new HashSet<string>();
            foreach (var classInfoData in DataVariants)
            {
                var variant = classInfoData.SequenceVariant != null ? classInfoData.SequenceVariant.ToString() : string.Empty;
                if (!hashSet.Add(variant))
                {
                    duplicity = variant;
                    ShowError = true;
                    break;
                }
            }
            if (!RemoveOriginal)
            {
                var variant = originalData.SequenceVariant != null ? originalData.SequenceVariant.ToString() : string.Empty;
                if (!hashSet.Add(variant))
                {
                    duplicity = variant;
                    ShowError = true;
                }
            }
            if (selectedClassInfo != null && !string.IsNullOrEmpty(dataPackageName) && selectedClassInfo.Data != null)
            {
                var package = selectedClassInfo.Data.FirstOrDefault(x => x.Key == dataPackageName);
                if (package.Value != null)
                {
                    foreach (var classInfoData in package.Value)
                    {
                        if (classInfoData != originalData)
                        {
                            var variant = classInfoData.SequenceVariant != null ? classInfoData.SequenceVariant.ToString() : string.Empty;
                            if (!hashSet.Add(variant))
                            {
                                duplicity = variant;
                                ShowError = true;
                                break;
                            }
                        }
                    }
                }
            }
            if (ShowError)
            {
                ErrorMsg = string.Format("There is a duplicity in your variants: {0}.{1}Please check it!", duplicity, Environment.NewLine);
            }
            return ShowError;
        }

        private void Save(object sender)
        {
            if (selectedClassInfo != null && !string.IsNullOrEmpty(dataPackageName) && selectedClassInfo.Data != null)
            {
                var package = selectedClassInfo.Data.FirstOrDefault(x => x.Key == dataPackageName);
                if (package.Value != null)
                {
                    foreach (var dataVariant in dataVariants)
                    {
                        package.Value.Add(dataVariant);
                    }
                    if (RemoveOriginal)
                    {
                        package.Value.Remove(originalData);
                    }
                    DialogResult = new MTFDialogResult() { Result = MTFDialogResultEnum.Ok };
                }
            }
            CloseWindow(sender);
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = new MTFDialogResult { Result = MTFDialogResultEnum.Cancel };
            CloseWindow(sender);
        }

        private void RemoveClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var item = button.CommandParameter as ClassInfoData;
            dataVariants.Remove(item);
        }

        private void CloseWindow(object sender)
        {
            Close?.Invoke(sender);
        }

        public event CloseEventHandler Close;
        public MTFDialogResult DialogResult { get; private set; }




        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); 
            }
        }


    }
}
