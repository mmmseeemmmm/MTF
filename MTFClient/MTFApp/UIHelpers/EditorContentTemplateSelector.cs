using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MTFApp.UIHelpers
{
    public class EditorContentTemplateSelector: DataTemplateSelector
    {
        public string TrueValueTemplateName { get; set; }
        public string FalseValueTemplateName { get; set; }

        public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container)
        {
            if (item==null)
            {
                return null;
            }
            FrameworkElement element = container as FrameworkElement;
            if ((bool)item)
            {
                return element.FindResource(TrueValueTemplateName) as DataTemplate;
            }
            else
            {
                return element.FindResource(FalseValueTemplateName) as DataTemplate;
            }
        }
    }
}
