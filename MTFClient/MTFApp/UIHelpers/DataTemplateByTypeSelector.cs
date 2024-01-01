using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MTFApp.UIHelpers
{
    public class DataTemplateByTypeSelector : DataTemplateSelector
    {
        public string DataTemplateNamePrefix { get; set; }
        public string DataTemplateNameSufix { get; set; }

        public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container)
        {
            if (item == null)
            {
                return null;
            }
            string typename = item.GetType().Name;
            FrameworkElement element = container as FrameworkElement;
            if (typename == typeof(string).Name)
            {
                string value = item.ToString();
                if (value.Contains(" "))
                {
                    value = value.Remove(value.IndexOf(" "));
                }
                return element.TryFindResource(DataTemplateNamePrefix + value + DataTemplateNameSufix) as DataTemplate;
            }
            if (item is MTFClientServerCommon.MTFActivityResult)
            {
                var result = item as MTFClientServerCommon.MTFActivityResult;
                return element.FindResource(DataTemplateNamePrefix + typename + DataTemplateNameSufix) as DataTemplate;
            }
            return element.FindResource(DataTemplateNamePrefix + typename + DataTemplateNameSufix) as DataTemplate;
        }
    }
}
