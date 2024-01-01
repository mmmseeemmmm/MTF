using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MTFApp.UIHelpers;
using MTFClientServerCommon;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.Helpers;

namespace MTFApp.SequenceEditor.Settings
{
    /// <summary>
    /// Interaction logic for ExternalSequenceSettings.xaml
    /// </summary>
    public partial class ExternalSequenceSettings : UserControl, INotifyPropertyChanged
    {
        private List<ExternalSequenceWrapper> externalSequences;
        private MTFSequence currentSequence;


        public ExternalSequenceSettings()
        {
            InitializeComponent();
            Root.DataContext = this;
        }

        private void ListBox_OnPreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            UIHelpers.UIHelper.RaiseScrollEvent(sender, e);
        }


        public MTFSequence Sequence
        {
            get => (MTFSequence)GetValue(SequenceProperty);
            set => SetValue(SequenceProperty, value);
        }

        public static readonly DependencyProperty SequenceProperty =
            DependencyProperty.Register("Sequence", typeof(MTFSequence), typeof(ExternalSequenceSettings),
                new FrameworkPropertyMetadata { PropertyChangedCallback = SequenceChanged, BindsTwoWayByDefault = false });

        private static void SequenceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (ExternalSequenceSettings)d;
            var s = e.NewValue as MTFSequence;
            if (s != null)
            {
                sender.currentSequence = s;
                if (sender.currentSequence.ExternalSubSequences != null)
                {
                    sender.ExternalSequences = sender.currentSequence.ExternalSubSequences.Select(
                                x => new ExternalSequenceWrapper { Name = x.ExternalSequence.Name, IsEnabled = x.IsEnabled }).ToList();
                }
            }
        }

        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register("IsOpen", typeof(bool), typeof(ExternalSequenceSettings),
                new FrameworkPropertyMetadata { PropertyChangedCallback = IsOpenChanged, BindsTwoWayByDefault = false });


        private static void IsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (ExternalSequenceSettings)d;
            if (!(bool)e.NewValue)
            {
                sender.UpdateSetting();
            }
        }

        private void UpdateSetting()
        {
            if (currentSequence != null && currentSequence.ExternalSubSequences != null && ExternalSequences != null)
            {
                ChangeNames();
                UpdateEnaleStatus();
            }
        }

        private void UpdateEnaleStatus()
        {
            foreach (var externalSequenceWrapper in ExternalSequences)
            {
                var externalSequenceInfo = currentSequence.ExternalSubSequences.FirstOrDefault(x => x.ExternalSequence.Name == externalSequenceWrapper.Name);
                if (externalSequenceInfo!=null && externalSequenceInfo.IsEnabled!=externalSequenceWrapper.IsEnabled)
                {
                    externalSequenceInfo.IsEnabled = externalSequenceWrapper.IsEnabled;
                }
            }
        }

        private void ChangeNames()
        {
            var listNewName = ExternalSequences.Where(x => x.Rename && !string.IsNullOrEmpty(x.NewName)).ToList();
            if (listNewName.Count > 1)
            {
                if (listNewName.GroupBy(x => x.NewName).Any(y => y.Count() > 1))
                {
                    MTFMessageBox.Show("MTF error",
                        "More new names have the same name. Please choose different names.",
                        MTFMessageBoxType.Error, MTFMessageBoxButtons.Ok);
                    return;
                }
            }
            foreach (var externalSequenceWrapper in listNewName)
            {
                if (!FileHelper.IsCorrectFileName(externalSequenceWrapper.NewName))
                {
                    MTFMessageBox.Show("MTF error",
                        $"Sequence name {externalSequenceWrapper.NewName} cannot be used because this name is reserved by the Window operating system. Please choose different name.",
                        MTFMessageBoxType.Error, MTFMessageBoxButtons.Ok);
                    return;
                }
            }

            var allowRename = true;
            foreach (var externalSequenceWrapper in listNewName)
            {
                var externalSequenceInfo = currentSequence.ExternalSubSequences.FirstOrDefault(x => x.ExternalSequence.Name == externalSequenceWrapper.Name);
                if (externalSequenceInfo != null)
                {
                    string dir = string.Empty;
                    try
                    {
                        dir = Path.GetDirectoryName(externalSequenceInfo.ExternalSequence.FullPath);
                        var serverItems = MTFClient.GetMTFClient().GetSequencesInfo(BaseConstants.SequenceBasePath, dir);
                        if (serverItems != null)
                        {
                            allowRename = !serverItems.Any(
                                    x =>
                                        x.Type == MTFDialogItemTypes.File &&
                                        x.Name == string.Format("{0}{1}", externalSequenceWrapper.NewName, BaseConstants.SequenceExtension));
                        }
                    }
                    catch (System.Exception)
                    {
                        allowRename = false;
                    }
                    if (allowRename)
                    {
                        var originalValue = externalSequenceWrapper;
                        currentSequence.ForEachActivity<MTFExecuteActivity>(x =>
                        {
                            if (x.Type == ExecuteActyvityTypes.External)
                            {
                                if (x.ExternalCall != null && x.ExternalCall.ExternalSequenceToCall == originalValue.Name)
                                {
                                    x.ExternalCall.ExternalSequenceToCall = originalValue.NewName;
                                }
                                if (x.ActivityName.Contains(originalValue.Name))
                                {
                                    x.ActivityName = x.ActivityName.Replace(originalValue.Name, originalValue.NewName);
                                }
                            }
                        });
                        externalSequenceInfo.ExternalSequence.Name = externalSequenceWrapper.NewName;
                        externalSequenceInfo.ExternalSequence.FullPath = Path.Combine(dir, string.Format("{0}{1}", externalSequenceWrapper.NewName, BaseConstants.SequenceExtension));
                    }
                    else
                    {
                        MTFMessageBox.Show("MTF error",
                            string.Format("Sequence name {0} cannot be used because this name already exists. Please choose different name.", externalSequenceWrapper.NewName),
                            MTFMessageBoxType.Error, MTFMessageBoxButtons.Ok);
                        return;
                    }
                }
            }
        }

        public List<ExternalSequenceWrapper> ExternalSequences
        {
            get => externalSequences;
            set
            {
                externalSequences = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand IntegrityCheckCommand => new Command(IntegrityCheck);

        private void IntegrityCheck()
        {
            var control = new IntegrityCheckControl(Sequence, Sequence.ExternalSubSequences.Select(x=>x.ExternalSequence).ToList());
            var popup = new PopupWindow.PopupWindow(control, true);
            popup.Show();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); 
            }
        }
    }


    public class ExternalSequenceWrapper : NotifyPropertyBase
    {
        private string name;
        private bool isEnabled;
        private bool rename;

        public string NewName { get; set; }
        
        public string Name
        {
            get => name;
            set
            {
                name = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                isEnabled = value;
                NotifyPropertyChanged();
            }
        }

        public bool Rename
        {
            get => rename;
            set
            {
                rename = value;
                NotifyPropertyChanged();
            }
        }
    }
}