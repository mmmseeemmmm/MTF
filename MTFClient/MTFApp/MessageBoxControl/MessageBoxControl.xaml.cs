using MTFApp.PopupWindow;
using MTFClientServerCommon;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using MTFApp.UIHelpers;
using MTFClientServerCommon.Helpers;

namespace MTFApp.MessageBoxControl
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class MessageBoxControl : UserControl, IReturnsDialogResult, IRaiseCloseEvent, INotifyPropertyChanged
    {
        private readonly MessageInfo messageInfo;
        private Command cancelCommand;
        private Command trueCommand;
        private Command falseCommand;
        private MTFDialogResult dialogResult;
        private string text;
        private List<ICommand> buttonCommands;

        public MessageBoxControl(MessageInfo messageInfo)
        {
            this.messageInfo = messageInfo;
            InitializeComponent();
            Root.DataContext = this;
            FillButtonValues();
            GenerateCommands();
            text = messageInfo.Text;
            if (!messageInfo.IsFullScreen)
            {
                Width = 500;
                Height = 200;
            }
        }

        private void GenerateCommands()
        {
            switch (messageInfo.Buttons)
            {
                case MessageButtons.None:
                    break;
                case MessageButtons.Ok:
                    cancelCommand = new Command(Cancel) { Name = LanguageHelper.GetString("Buttons_Ok"), Focus = true };
                    break;
                case MessageButtons.Cancel:
                    cancelCommand = new Command(Cancel) { Name = LanguageHelper.GetString("Buttons_Cancel"), Focus = true };
                    break;
                case MessageButtons.YesNo:
                    trueCommand = new Command(Accept) { Name = LanguageHelper.GetString("Buttons_Yes"), Focus = true };
                    falseCommand = new Command(Decline) { Name = LanguageHelper.GetString("Buttons_No") };
                    break;
                case MessageButtons.OkCancel:
                    trueCommand = new Command(Accept) { Name = LanguageHelper.GetString("Buttons_Ok"), Focus = true };
                    falseCommand = new Command(Decline) { Name = LanguageHelper.GetString("Buttons_Cancel") };
                    break;
                case MessageButtons.YesNoCancel:
                    trueCommand = new Command(Accept) { Name = LanguageHelper.GetString("Buttons_Yes"), Focus = true };
                    falseCommand = new Command(Decline) { Name = LanguageHelper.GetString("Buttons_No") };
                    cancelCommand = new Command(Cancel) { Name = LanguageHelper.GetString("Buttons_Cancel") };
                    break;
                default:
                    cancelCommand = new Command(Cancel) { Name = LanguageHelper.GetString("Buttons_Cancel"), Focus = true };
                    break;
            }

        }

        private void Decline()
        {
            SetDialogResult(messageInfo.Buttons == MessageButtons.OkCancel ? MTFDialogResultEnum.Cancel : MTFDialogResultEnum.No);
            CloseWindow();
        }

        private void Accept()
        {
            SetDialogResult(messageInfo.Buttons == MessageButtons.OkCancel ? MTFDialogResultEnum.Ok : MTFDialogResultEnum.Yes);
            CloseWindow();
        }

        private void Cancel()
        {
            SetDialogResult(messageInfo.Buttons == MessageButtons.Ok ? MTFDialogResultEnum.Ok : MTFDialogResultEnum.Cancel);
            CloseWindow();
        }

        private void FillButtonValues()
        {
            if (messageInfo.ButtonValues != null && messageInfo.Type == SequenceMessageType.Choice)
            {
                if (messageInfo.ChoiceDisplayType == SequenceMessageDisplayType.ToggleButtons)
                {
                    GenerateToggleValues();
                }
                else if (messageInfo.ChoiceDisplayType == SequenceMessageDisplayType.Buttons)
                {
                    GenerateButtonCommands();
                }
                else
                {
                    if (messageInfo.ButtonValues.Any())
                    {
                        SelectedValue = MessageInfo.ButtonValues.First();
                    }
                }

            }
        }

        private void GenerateButtonCommands()
        {
            var first = true;
            buttonCommands = new List<ICommand>();
            foreach (var buttonValue in messageInfo.ButtonValues)
            {
                buttonCommands.Add(new Command(ButtonChoiceSelection) { Name = buttonValue, Focus = first });
                if (first)
                {
                    first = false;
                }
            }
        }

        private void GenerateToggleValues()
        {
            MultipleChoiceValues = new List<MultipleChoiceItem>(messageInfo.ButtonValues.Count);
            foreach (var buttonValue in messageInfo.ButtonValues)
            {
                MultipleChoiceValues.Add(new MultipleChoiceItem { Name = buttonValue });
            }
        }

        public void UpdateText(string text)
        {
            this.text = text;
            NotifyPropertyChanged("Text");
        }

        public IList<MultipleChoiceItem> MultipleChoiceValues { private set; get; }


        private void ButtonChoiceSelection(object param)
        {
            dialogResult = new MTFDialogResult { Result = MTFDialogResultEnum.TextResult, TextResult = param as string };
            CloseWindow();
        }


        public string InputString { get; set; }


        public string SelectedValue { get; set; }



        private void SetDialogResult(MTFDialogResultEnum mtfDialogResultEnum)
        {
            dialogResult = new MTFDialogResult();

            if (messageInfo.Type == SequenceMessageType.Choice)
            {
                dialogResult.Result = MTFDialogResultEnum.TextResult;
                dialogResult.TextResult = string.Empty;

                if (mtfDialogResultEnum == MTFDialogResultEnum.Ok || mtfDialogResultEnum == MTFDialogResultEnum.Yes)
                {
                    if (messageInfo.ChoiceDisplayType == SequenceMessageDisplayType.ComboBox)
                    {
                        dialogResult.TextResult = SelectedValue;
                    }
                    else if (messageInfo.ChoiceDisplayType == SequenceMessageDisplayType.ToggleButtons)
                    {
                        dialogResult.TextResult = string.Join(";", MultipleChoiceValues.Where(i => i.Checked).Select(i => i.Name).ToArray());
                    }
                }
            }
            else if (messageInfo.Type == SequenceMessageType.Input)
            {
                dialogResult.Result = MTFDialogResultEnum.TextResult;
                dialogResult.TextResult = mtfDialogResultEnum == MTFDialogResultEnum.Yes || mtfDialogResultEnum == MTFDialogResultEnum.Ok ? InputString : string.Empty;
            }
            else
            {
                dialogResult.Result = mtfDialogResultEnum;
            }
        }

        private void CloseWindow()
        {
            if (Close != null)
            {
                Close(this);
            }
        }


        public MTFDialogResult DialogResult => dialogResult;

        public MessageInfo MessageInfo => messageInfo;

        public ICommand CancelCommand => cancelCommand;

        public ICommand TrueCommand => trueCommand;

        public ICommand FalseCommand => falseCommand;

        public string Text => text;

        public List<ICommand> ButtonCommands => buttonCommands;

        public event CloseEventHandler Close;


        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Accept();
            }
        }
    }

    public class MultipleChoiceItem
    {
        public string Name { get; set; }
        public bool Checked { get; set; }
    }
}
