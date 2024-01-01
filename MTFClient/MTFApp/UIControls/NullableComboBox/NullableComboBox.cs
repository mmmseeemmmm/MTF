using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using MTFApp.UIHelpers;

namespace MTFApp.UIControls.NullableComboBox
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:PMP.UserControls.NullableComboBox"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:PMP.UserControls.NullableComboBox;assembly=PMP.UserControls.NullableComboBox"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:NullableComboBox/>
    ///
    /// </summary>
    public class NullableComboBox : ComboBox, INotifyPropertyChanged
    {
        private bool canRemove;

        static NullableComboBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NullableComboBox), new FrameworkPropertyMetadata(typeof(NullableComboBox)));
        }

        public ICommand RemoveSelection
        {
            get
            {
                return new Command(() =>
                                   {
                                       canRemove = true;
                                       SelectedItem = null;
                                       canRemove = false;
                                   });
            }
        }

        public Visibility VisibleRemoveButton
        {
            get { return SelectedItem == null ? Visibility.Collapsed : Visibility.Visible; }
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            var be = GetBindingExpression(SelectedValueProperty);
            if (be != null && be.ParentBinding != null && be.ParentBinding.UpdateSourceTrigger == UpdateSourceTrigger.Explicit)
            {
                if (SelectedValue != null || canRemove)
                {
                    be.UpdateSource();
                }
                
            }
            NotifyPropertyChanged("VisibleRemoveButton");
            base.OnSelectionChanged(e);
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
}
