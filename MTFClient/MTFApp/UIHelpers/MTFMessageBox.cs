using System;
using System.Windows;
using MTFClientServerCommon;
using MTFClientServerCommon.Helpers;

namespace MTFApp.UIHelpers
{
    static class MTFMessageBox
    {
        public static MTFMessageBoxResult Show(string header, string message, MTFMessageBoxType type, MTFMessageBoxButtons buttons)
        {
            return Show(header, message, type, buttons, null, true);
        }

        public static MTFMessageBoxResult Show(string header, string message, MTFMessageBoxType type, MTFMessageBoxButtons buttons, Window owner)
        {
            return Show(header, message, type, buttons, owner, true);
        }

        public static MTFMessageBoxResult Show(string header, string message, MTFMessageBoxType type, MTFMessageBoxButtons buttons, Window owner, bool isBlockingDialog)
        {
            PopupWindow.PopupWindow pw = null;

            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                                                      {
                                                          var messageInfo = new MessageInfo
                                                                            {
                                                                                Text = message,
                                                                                Type = GetMessageType(type),
                                                                                Buttons = GetButtonType(buttons),
                                                                            };
                                                          pw = new PopupWindow.PopupWindow(new MessageBoxControl.MessageBoxControl(messageInfo),
                                                              true, owner, isBlockingDialog)
                                                               {
                                                                   CanClose = false,
                                                                   Title = header
                                                               };
                                                          pw.ShowDialog();
                                                      });
            }


            if (pw != null && pw.MTFDialogResult != null)
            {
                switch (pw.MTFDialogResult.Result)
                {
                    case MTFDialogResultEnum.Cancel:
                        return MTFMessageBoxResult.Cancel;
                    case MTFDialogResultEnum.No:
                        return MTFMessageBoxResult.No;
                    case MTFDialogResultEnum.Ok:
                        return MTFMessageBoxResult.Ok;
                    case MTFDialogResultEnum.Yes:
                        return MTFMessageBoxResult.Yes;
                    default:
                        return MTFMessageBoxResult.Cancel;
                }
            }
            return MTFMessageBoxResult.Cancel;
        }

        public static void ShowNonBlockingMessage(string header, string message, MTFMessageBoxType type, MTFMessageBoxButtons buttons, Action closeAction)
        {
            PopupWindow.PopupWindow pw = null;

            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var messageInfo = new MessageInfo
                    {
                        Text = message,
                        Type = GetMessageType(type),
                        Buttons = GetButtonType(buttons),
                    };
                    pw = new PopupWindow.PopupWindow(new MessageBoxControl.MessageBoxControl(messageInfo),
                        true, null, false, closeAction)
                    {
                        CanClose = false,
                        Title = header
                    };
                    pw.Show();
                });
            }
        }

        public static MTFMessageBoxResult ShowConfirmRemoveItem(string itemName)
        {
            return Show(LanguageHelper.GetString("Msg_Header_DeleteItem"),
                string.Format(LanguageHelper.GetString("Msg_Body_DeleteItem"), itemName), MTFMessageBoxType.Question,
                MTFMessageBoxButtons.YesNo);
        }

        private static SequenceMessageType GetMessageType(MTFMessageBoxType type)
        {
            switch (type)
            {
                case MTFMessageBoxType.Info:
                    return SequenceMessageType.Info;
                case MTFMessageBoxType.Warning:
                    return SequenceMessageType.Warning;
                case MTFMessageBoxType.Error:
                    return SequenceMessageType.Error;
                case MTFMessageBoxType.Question:
                    return SequenceMessageType.Question;
                case MTFMessageBoxType.ImportantQuestion:
                    return SequenceMessageType.ImportantQuestion;
                default:
                    return SequenceMessageType.Info;
            }
        }

        private static MessageButtons GetButtonType(MTFMessageBoxButtons buttons)
        {
            switch (buttons)
            {
                case MTFMessageBoxButtons.YesNo:
                    return MessageButtons.YesNo;
                case MTFMessageBoxButtons.OkCancel:
                    return MessageButtons.OkCancel;
                case MTFMessageBoxButtons.Ok:
                    return MessageButtons.Ok;
                case MTFMessageBoxButtons.YesNoCancel:
                    return MessageButtons.YesNoCancel;
                default:
                    return MessageButtons.Cancel;
            }
        }
    }

    enum MTFMessageBoxType
    {
        Info,
        Warning,
        Error,
        Question,
        ImportantQuestion,
    }

    enum MTFMessageBoxButtons
    {
        YesNo,
        OkCancel,
        Ok,
        YesNoCancel,
    }

    enum MTFMessageBoxResult
    {
        Ok,
        Cancel,
        Yes,
        No,
    }
}