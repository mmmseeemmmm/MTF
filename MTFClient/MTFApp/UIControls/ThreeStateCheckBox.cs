using System.Windows;
using System.Windows.Controls;

namespace MTFApp.UIControls
{
    public class ThreeStateCheckBox: CheckBox
    {
        protected override void OnClick()
        {
            switch (IsChecked)
            {
                case true:
                    IsChecked = false;
                    break;
                case null:
                    IsChecked = true;
                    break;
                case false:
                    IsChecked = null;
                    break;
            }
            RaiseEvent(new RoutedEventArgs(ClickEvent, this));
        }
    }
}
