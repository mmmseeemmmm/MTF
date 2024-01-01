using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MTFClientServerCommon;
using MTFClientServerCommon.Helpers;

namespace MTFApp.UIHelpers
{
    public static class UIHelper
    {
        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            if (child is Visual visual)
            {
                if (VisualTreeHelper.GetParent(visual) is Visual parentObject)
                {
                    var parent = parentObject as T;
                    return parent ?? FindParent<T>(parentObject);
                }
            }

            return null;
        }

        /// <summary>
        /// Find parent in Visual Tree
        /// </summary>
        /// <typeparam name="T">Type of object which should be found</typeparam>
        /// <param name="child">Object in Visual Tree from which searching is starting</param>
        /// <param name="maxRepeat">Maximum steps of searching by parent in visual tree</param>
        /// <returns>Null or found object of type T</returns>
        public static T FindParent<T>(DependencyObject child, int maxRepeat) where T : DependencyObject
        {
            if (child is Visual visual)
            {
                if (VisualTreeHelper.GetParent(visual) is Visual parentObject && maxRepeat >= 1)
                {
                    var parent = parentObject as T;
                    return parent ?? FindParent<T>(parentObject, maxRepeat - 1);
                }
            }

            return null;
        }


        public static T FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                var result = child as T ?? FindChild<T>(child);

                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        public static bool HasParentOfType<T>(DependencyObject child) where T : DependencyObject
        {
            return FindParent<T>(child) != null;
        }

        public static double GetNewScale(int whellDelta, double originalScale)
        {
            double step = 0.1;
            double newScale = originalScale;
            var zoomDirection = whellDelta > 0 ? 1 : -1;
            if ((originalScale > 0.2 || zoomDirection == 1) &&
                (originalScale < 3 || zoomDirection == -1))
            {
                newScale += step * zoomDirection;
            }
            return newScale;
        }

        public static object GetObjectDataFromPoint(ListBox source, Point point)
        {
            if (source.InputHitTest(point) is UIElement element)
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

        public static void RaiseScrollEvent(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            var captured = e.MouseDevice.Captured;
            if (captured is FrameworkElement element && element.Parent is System.Windows.Controls.Primitives.Popup)
            {
                e.Handled = false;
                return;
            }
            if (!e.Handled && !(e.MouseDevice.Captured is ComboBox))
            {
                e.Handled = true;
                var eventArg = new System.Windows.Input.MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                               {
                                   RoutedEvent = UIElement.MouseWheelEvent, Source = sender
                               };
                var parent = FindParent<UIElement>(sender as UIElement);
                parent?.RaiseEvent(eventArg);
            }
        }

        public static void AddSorted<T>(this IList<T> collection, T item) where T : MTFClientServerCommon.MTFVariable
        {
            var comparer = Comparer<T>.Default;
            int i = 0;
            while (i < collection.Count && string.Compare(collection[i].Name, item.Name) < 0)
            {
                i++;
            }
            collection.Insert(i, item);
        }

        public static int GetTargetIdOf<T>(this IList<T> collection, T item) where T : MTFClientServerCommon.MTFVariable
        {
            var comparer = Comparer<T>.Default;
            int i = 0;
            bool sameName = false;
            while (i < collection.Count && (string.Compare(collection[i].Name, item.Name) < 0 || collection[i].Name == item.Name))
            {
                if (collection[i].Name == item.Name)
                {
                    sameName = true;
                }
                i++;
            }
            if (sameName)
            {
                i--;
            }
            return i;
        }


        public static DependencyObject GetVisualAncestor(this DependencyObject d, Type type)
        {
            DependencyObject item = VisualTreeHelper.GetParent(d);

            while (item != null)
            {
                if (item.GetType() == type) return item;
                item = VisualTreeHelper.GetParent(item);
            }

            return null;
        }

        public static T GetVisualDescendent<T>(this DependencyObject d) where T : DependencyObject
        {
            return d.GetVisualDescendents<T>().FirstOrDefault();
        }

        public static IEnumerable<T> GetVisualDescendents<T>(this DependencyObject d) where T : DependencyObject
        {
            int childCount = VisualTreeHelper.GetChildrenCount(d);

            for (int n = 0; n < childCount; n++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(d, n);

                if (child is T dependencyObject)
                {
                    yield return dependencyObject;
                }

                foreach (T match in GetVisualDescendents<T>(child))
                {
                    yield return match;
                }
            }

            yield break;
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null)
            {
                yield return null;
            }
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                if (child is T children)
                {
                    yield return children;
                }

                foreach (var childOfChild in FindVisualChildren<T>(child))
                {
                    yield return childOfChild;
                }
            }
        }

        public static ListBox FindListBoxByActivity(MTFSequenceActivity activity, DependencyObject depObj)
        {
            IEnumerable<ListBox> collection = FindVisualChildren<ListBox>(depObj);
            foreach (var listBox in collection)
            {
                if (listBox == null)
                {
                    return null;
                }
                foreach (var item in listBox.Items)
                {
                    if (item == activity)
                    {
                        return listBox;
                    }
                }
            }
            return null;
        }

        public static T HandleWcfCallWithErrorMsg<T>(Func<T> wcfAction, string errorHeader) where T : class
        {
            try
            {
                return wcfAction();
            }
            catch (Exception ex)
            {
                var msg = $"{errorHeader}\n{ex.Message}";

                MTFMessageBox.Show(LanguageHelper.GetString("Msg_Header_Error"),
                    msg, MTFMessageBoxType.Error,
                    MTFMessageBoxButtons.Ok);
                return null;
            }
        }

        public static void InvokeOnDispatcher(Action a)
        {
            Application.Current.Dispatcher.Invoke(a);
        }

        public static T InvokeOnDispatcher<T>(Func<T> f)
        {
            return Application.Current.Dispatcher.Invoke(f);
        }

        public static async Task InvokeOnDispatcherAsyncAwaitable(Action a)
        {
            await Application.Current.Dispatcher.BeginInvoke(a);
        }

        public static void InvokeOnDispatcherAsync(Action a)
        {
            Application.Current.Dispatcher.BeginInvoke(a);
        }
    }
}