using System;
using System.Windows;
using System.Windows.Input;

namespace MTFApp.UIHelpers.LongTask
{
    static class LongTask
    {
        private static PopupWindow.PopupWindow popupWindow;
        public static void Do(Action func, string windowText)
        {
            var popup = Application.Current.Dispatcher.Invoke(() => createPopup(windowText));
            try
            {
                ShowPopup(popup);
                func();
            }
            finally
            {
                Finish(popup);
            }
        }

        private static PopupWindow.PopupWindow createPopup(string text)
        {
            System.Windows.Controls.UserControl uc = new System.Windows.Controls.UserControl();
            uc.Background = uc.FindResource("ALYellowBrush") as System.Windows.Media.Brush;
            uc.Width = 320;
            uc.Height = 180;
            uc.Content = new System.Windows.Controls.TextBlock
            {
                Text = text,
                FontSize = 16,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            var pw = new PopupWindow.PopupWindow(uc, false);
            pw.CanClose = false;

            return pw;
        }

        private static void ShowPopup(PopupWindow.PopupWindow popup)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                popup.Show();
                Mouse.OverrideCursor = Cursors.Wait;
            });
        }

        private static void Finish(PopupWindow.PopupWindow popup)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    Mouse.OverrideCursor = null;
                    if (popup != null)
                    {
                        popup.CanClose = true;
                        popup.Close();
                    }
                });
        }
    }
}
