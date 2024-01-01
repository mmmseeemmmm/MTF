using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using MTFApp.UIHelpers;
using MTFClientServerCommon;

namespace MTFApp.SequenceExecution.Helpers
{
    public class MessageBoxHandler
    {
        PopupWindow.PopupWindow currentBlockingPopup;
        private bool pwClosedByEvent;
        private readonly Dictionary<Guid, PopupWindow.PopupWindow> modelessDialogs = new Dictionary<Guid, PopupWindow.PopupWindow>();
        private readonly object noBlockingMsgLock = new object();

        public void Init()
        {
            currentBlockingPopup?.Close();
            modelessDialogs?.Clear();
        }

        private void ClosePopup(Guid id)
        {
            if (modelessDialogs[id] != null)
            {
                modelessDialogs[id].CanClose = true;
                modelessDialogs[id].Close();
            }
        }


        private void CloseCurrentBlockedPopup()
        {
            if (currentBlockingPopup == null)
            {
                return;
            }
            pwClosedByEvent = true;
            UIHelper.InvokeOnDispatcher(() =>
                               {
                                   if (currentBlockingPopup != null)
                                   {
                                       currentBlockingPopup.CanClose = true;
                                       currentBlockingPopup.Close();
                                   }
                               });
        }

        public void ClosePopups(List<Guid> executingActivityPath)
        {
            if (executingActivityPath != null && executingActivityPath.Count > 0)
            {
                lock (noBlockingMsgLock)
                {
                    if (modelessDialogs.ContainsKey(executingActivityPath.Last()))
                    {
                        var id = executingActivityPath.Last();
                        UIHelper.InvokeOnDispatcher(() => { ClosePopup(id); });
                        modelessDialogs.Remove(id);
                    }
                }
            }
            CloseCurrentBlockedPopup();
        }


        public void HandleMessage(MessageInfo messageInfo, Action<MTFDialogResult, List<Guid>> sendResult)
        {
            if (messageInfo.Type == SequenceMessageType.NoBlockingMessage)
            {
                UIHelper.InvokeOnDispatcher(() => HandleNoBlockingMessage(messageInfo, sendResult));
            }
            else
            {
                UIHelper.InvokeOnDispatcher(() => HandleBlockingMessage(messageInfo, sendResult));
            }
        }

        private void HandleBlockingMessage(MessageInfo messageInfo, Action<MTFDialogResult, List<Guid>> sendResult)
        {
            var messageBody = new MessageBoxControl.MessageBoxControl(messageInfo);
            currentBlockingPopup = new PopupWindow.PopupWindow(messageBody)
                                   {
                                       WindowState = messageInfo.IsFullScreen?WindowState.Maximized : WindowState.Normal,
                                       CanClose = messageInfo.Buttons == MessageButtons.None,
                                       Title = messageInfo.Header,
                                   };
            currentBlockingPopup.ShowDialog();

            if (!pwClosedByEvent && currentBlockingPopup != null)
            {
                sendResult(currentBlockingPopup.MTFDialogResult, messageInfo.ActivityPath);
                currentBlockingPopup = null;
            }
            pwClosedByEvent = false;
        }

        private void HandleNoBlockingMessage(MessageInfo messageInfo, Action<MTFDialogResult, List<Guid>> sendResult)
        {
            if (messageInfo.ActivityPath != null && messageInfo.ActivityPath.Count > 0)
            {
                var messageBody = new MessageBoxControl.MessageBoxControl(messageInfo);
                var activityId = messageInfo.ActivityPath.Last();
                lock (noBlockingMsgLock)
                {
                    if (modelessDialogs.ContainsKey(activityId))
                    {
                        modelessDialogs[activityId].Title = messageInfo.Header;
                        var body = modelessDialogs[activityId].GetContent();
                        if (body != null)
                        {
                            body.UpdateText(messageInfo.Text);
                        }
                    }
                    else
                    {
                        var noBlockPopup = new PopupWindow.PopupWindow(sendResult, messageInfo.ActivityPath, messageBody, false)
                                           {
                                               WindowState = messageInfo.IsFullScreen ? WindowState.Maximized : WindowState.Normal,
                                               CanClose = messageInfo.Buttons == MessageButtons.None,
                                               Title = messageInfo.Header
                                           };
                        UIHelper.InvokeOnDispatcher(() => noBlockPopup.Show());
                        modelessDialogs[activityId] = noBlockPopup;
                    }
                }
            }
        }
    }
}