using System.Windows;

namespace MTFApp.UIHelpers
{
    public static class FocusExtension
    {
        public static bool GetIsFocused(DependencyObject o)
        {
            return (bool)o.GetValue(IsFocusedProperty);
        }

        public static void SetIsFocusedProperty(DependencyObject o, bool value)
        {
            o.SetValue(IsFocusedProperty, value);
        }

        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.RegisterAttached("IsFocused", typeof(bool), typeof(FocusExtension), new UIPropertyMetadata(false, OnValueChanged));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = (UIElement)d;
            if ((bool)e.NewValue)
            {
                element.Focus();
            }
        }


    }
}
