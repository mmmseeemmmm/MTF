using MTFApp.UIHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MTFApp.EditorTest
{
    /// <summary>
    /// Interaction logic for EditorTestControl.xaml
    /// </summary>
    public partial class EditorTestControl : MTFUserControl
    {
        public EditorTestControl()
        {
            InitializeComponent();
        }

        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            var targetPoint = e.GetPosition(sender as ListBox);
            var data = GetObjectDataFromPoint(sender as ListBox, targetPoint);
            TextBlock textBlock = (data as ListBoxItem).Content as TextBlock;

            if (e.Data.GetDataPresent("ActivityResult"))
            {
                var activity = new MTFClientServerCommon.MTFSequenceActivity()
                {
                    ActivityName = textBlock.Text,
                    MTFMethodDisplayName = textBlock.Tag.ToString(),
                    MTFMethodName = textBlock.Tag.ToString(),
                    ReturnType = textBlock.Tag.ToString()
                };
                e.Data.SetData("ActivityResult", activity);
                e.Handled = true;
            }
        }




        private object GetObjectDataFromPoint(ListBox source, Point point)
        {
            UIElement element = source.InputHitTest(point) as UIElement;
            if (element != null)
            {
                //get the object from the element
                object data = DependencyProperty.UnsetValue;
                while (data == DependencyProperty.UnsetValue)
                {
                    // try to get the object value for the corresponding element
                    data = source.ItemContainerGenerator.ItemFromContainer(element);

                    //get the parent and we will iterate again
                    if (data == DependencyProperty.UnsetValue)
                        element = VisualTreeHelper.GetParent(element) as UIElement;

                    //if we reach the actual listbox then we must break to avoid an infinite loop
                    if (element == source)
                        return null;
                }

                //return the data that we fetched only if it is not Unset value, 
                //which would mean that we did not find the data
                if (data != DependencyProperty.UnsetValue)
                    return data;
            }

            return null;
        }
    }
}
