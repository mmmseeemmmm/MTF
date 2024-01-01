using System;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;

namespace MTFApp.UIHelpers.DragAndDrop
{
    public class TouchHelper
    {
        private static TouchHelper instance;
        private DataObject dataObject;
        private object sourceControl;
        private Brush blackBrush;
        private Brush redBrush;
        private Action<object, object> action;

        public object SourceControl
        {
            get { return sourceControl; }
            set { sourceControl = value; }
        }


        private TouchHelper()
        {
            blackBrush = (Brush)App.Current.FindResource("ALBlackBrush");
            redBrush = (Brush)App.Current.FindResource("BaseRedBrush");
        }

        public static TouchHelper Instance
        {
            get
            {
                return instance = instance ?? new TouchHelper();
            }
        }

        public void SetItem(DataObject dataObject, object source)
        {
            this.dataObject = dataObject;
            SetSourceControl(source);
        }

        public void SetItem(string type, object data, object source)
        {
            this.dataObject = new DataObject(type, data);
            SetSourceControl(source);
        }

        public void SetItem(string type, object data, object source, ITouchHelper parent)
        {
            this.dataObject = new DataObject(type, data);
            SetSourceControl(source, parent);
        }

        public void SetAction(Action<object, object> action, object source)
        {
            this.dataObject = new DataObject(DragAndDropTypes.SetAction, new object());
            this.action = action;
            SetSourceControl(source);
        }

        public void InvokeAction(object param, object source)
        {
            if (action!=null)
            {
                action(param, source);
            }
        }

        public void Clear()
        {
            UnselectSourceControl();
            this.dataObject = null;
            this.sourceControl = null;
            this.action = null;
            DisableEditorSelection = false;
        }

        private void SetSourceControl(object newSource)
        {
            UnselectSourceControl();

            var userControl = newSource as UserControl;
            if (userControl!=null)
            {
                userControl.Foreground = Brushes.Red;
            }
            else
            {
                var border = newSource as Border;
                if (border!=null)
                {
                    border.BorderThickness = new Thickness(3);
                }
            }
            
            this.sourceControl = newSource;
        }

        private void SetSourceControl(object newSource, ITouchHelper parent)
        {
            parent.SourceElement = newSource;
            parent.Select();
            this.sourceControl = parent;
        }

        private void UnselectSourceControl()
        {
            var tHelper = sourceControl as ITouchHelper;
            if (tHelper != null)
            {
                tHelper.Unselect();
            }
            else
            {
                var userControl = sourceControl as UserControl;
                if (userControl != null)
                {
                    userControl.Foreground = blackBrush;
                }
                else
                {
                    var border = sourceControl as Border;
                    if (border != null)
                    {
                        border.BorderThickness = new Thickness(0);
                    }
                } 
            }
        }

        public bool DisableEditorSelection { get; set; }


        public DataObject DataObject
        {
            get
            {
                return dataObject;
            }
        }

        public override string ToString()
        {
            if (dataObject!=null)
            {
                return dataObject.ToString();
            }
            return "Null";
        }
    }
}
