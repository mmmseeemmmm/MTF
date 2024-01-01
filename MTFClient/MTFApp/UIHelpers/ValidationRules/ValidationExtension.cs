using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace MTFApp.UIHelpers.ValidationRules
{
    public class ValidationExtension
    {
        public static readonly DependencyProperty UseValidationProperty = DependencyProperty.RegisterAttached("UseValidation",
            typeof(bool),
            typeof(ValidationExtension),
            new PropertyMetadata(false));

        public static bool GetUseValidation(DependencyObject obj)
        {
            return (bool)obj.GetValue(UseValidationProperty);
        }

        public static void SetUseValidation(DependencyObject obj, bool value)
        {
            obj.SetValue(UseValidationProperty, value);
        }


        public static void GetChildOfTypes(FrameworkElement parentElement, List<Type> foundedTypes, Action<FrameworkElement> action)
        {
            if (parentElement != null)
            {
                var count = VisualTreeHelper.GetChildrenCount(parentElement);
                for (int i = 0; i < count; i++)
                {
                    var element = VisualTreeHelper.GetChild(parentElement, i);

                    if (GetUseValidation(element))
                    {
                        if (foundedTypes.Contains(element.GetType()))
                        {
                            action(element as FrameworkElement);
                        }
                    }

                    GetChildOfTypes(element as FrameworkElement, foundedTypes, action);
                }
            }
        }
    }
}
