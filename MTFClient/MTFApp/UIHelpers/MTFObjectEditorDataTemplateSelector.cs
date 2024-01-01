using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MTFApp.UIHelpers
{
    class MTFObjectEditorDataTemplateSelector : DataTemplateSelector
    {
        public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;
            if (item == null)
            {
                return element.FindResource("NullDataTemplate") as DataTemplate;
            }
            string typename = item.GetType().Name;
            switch (typename)
            {
                case "ActivityResultTerm":
                case "GenericClassInstanceConfiguration":
                    return element.FindResource(typename) as DataTemplate;
                default:
                    return element.FindResource("UnknownType") as DataTemplate;
            }
        }
    }
}
