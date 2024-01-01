using System;
using System.Windows;
using System.Windows.Data;

namespace MTFApp.UIControls
{
    public class ToggleButton : System.Windows.Controls.Primitives.ToggleButton
    {
        public static readonly DependencyProperty IsCheckedPropertyBindingProperty =
            DependencyProperty.Register("IsCheckedPropertyBinding", typeof(string), typeof(ToggleButton),
                new FrameworkPropertyMetadata { BindsTwoWayByDefault = true, PropertyChangedCallback = PropertyChanged });

        private static void PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var toggleButton = d as ToggleButton;

            if (toggleButton == null)
            {
                return;
            }

            string propertyName = (string)e.NewValue;

            if (string.IsNullOrEmpty(propertyName))
            {
                return;
            }

            var myBinding = new Binding();
            myBinding.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(MainWindow), 1);
            myBinding.Path = new PropertyPath(String.Format("DataContext.MainContent.DataContext.{0}", propertyName)); // Property has to be in content presenter MainContent.
            myBinding.Mode = BindingMode.OneWay;
            myBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            BindingOperations.SetBinding(toggleButton, ToggleButton.IsCheckedProperty, myBinding);
        }

        public string IsCheckedPropertyBinding
        {
            get { return (string)GetValue(IsCheckedPropertyBindingProperty); }
            set { SetValue(IsCheckedPropertyBindingProperty, value); }
        }
    }
}
