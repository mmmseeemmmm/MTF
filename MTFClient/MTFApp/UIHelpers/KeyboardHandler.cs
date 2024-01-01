using System.Windows;
using System.Windows.Controls;

namespace MTFApp.UIHelpers
{
    class KeyboardHandler
    {
        private static KeyboardHandler instance = new KeyboardHandler();
        private UserControl handledControl;
        private RoutedEvent lastRoutedEvent;

        private KeyboardHandler()
        {

        }

        public static KeyboardHandler Instance
        {
            get { return instance ?? new KeyboardHandler(); }
        }


        public void SetControl(UserControl userControl)
        {
            handledControl = userControl;
        }

        public void RemoveControl()
        {
            handledControl = null;
        }

        public void RaiseEvent(RoutedEventArgs e)
        {
            if (lastRoutedEvent != e.RoutedEvent)
            {
                lastRoutedEvent = e.RoutedEvent;
                if (handledControl != null)
                {
                    handledControl.RaiseEvent(e);
                }
            }
            else
            {
                lastRoutedEvent = null;
            }
        }
    }
}
