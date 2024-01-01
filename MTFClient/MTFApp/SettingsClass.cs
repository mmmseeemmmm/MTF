using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using MTFClientServerCommon;

namespace MTFApp
{
    public delegate void LanguageChangedEventHandler(object sender, LanguageChangeEventArgs e);

    public class LanguageChangeEventArgs : EventArgs
    {
        public string Language { get; set; }
    }

    [Serializable]
    public class SettingsClass
    {
        public SettingsClass() { }

        private Point windowLocation = new Point(Application.Current.MainWindow.Left, Application.Current.MainWindow.Top);
        private Size windowSize = new Size(Application.Current.MainWindow.Width, Application.Current.MainWindow.Height);
        private WindowState windowState = Application.Current.MainWindow.WindowState;
        private List<Size> connectionDialog = new List<Size>();
        private List<ControlInfo> controlInfos = new List<ControlInfo>();
        private List<ConnectionDialog.ConnectionContainer> connections = new List<ConnectionDialog.ConnectionContainer>();
        private WindowSetting findUsagesSetting = new WindowSetting();
        private WindowSetting debugSetting = new WindowSetting();
        private WindowSetting setupSetting = new WindowSetting();
        private static ConnectionDialog.ConnectionContainer selectedConnection;
        private double scale = 1.0;
        private double dialogScale = 1.0;
        private string language = "en-US";
        private bool showTreeWithFullImage;
        private bool collapsedParametersInEditor;
        private bool allowDragDrop = true;

        private List<AccessKeyProviderSettings> accessKeyProviderSettingses;

        [field: NonSerialized]
        public event LanguageChangedEventHandler OnLanguageChanged;

        public double AppScale
        {
            get { return scale; }
            set { scale = value; }
        }

        public double DialogScale
        {
            get { return dialogScale; }
            set { dialogScale = value; }
        }


        public Point WindowLocation
        {
            get { return windowLocation; }
            set { windowLocation = value; }
        }

        private int selectedConnectionIndex;

        public int SelectedConnectionIndex
        {
            get { return selectedConnectionIndex; }
            set { selectedConnectionIndex = value; }
        }


        public Size WindowSize
        {
            get { return windowSize; }
            set { windowSize = value; }
        }

        public WindowState WindowState
        {
            get { return windowState; }
            set { windowState = value; }
        }

        public List<ConnectionDialog.ConnectionContainer> Connections
        {
            get { return connections; }
            set { connections = value; }
        }


        public List<Size> ConnectionDialog
        {
            get { return connectionDialog; }
            set { connectionDialog = value; }
        }


        public bool CollapsedParametersInEditor
        {
            get { return collapsedParametersInEditor; }
            set { collapsedParametersInEditor = value; }
        }

        public bool AllowDragDrop
        {
            get { return allowDragDrop; }
            set { allowDragDrop = value; }
        }

        public bool StartMTFServer
        {
            get;
            set;
        }

        public bool StopMTFServer
        {
            get;
            set;
        }

        public string MTFServerPath
        {
            get;
            set;
        }

        public bool ConnectToServerAutomatically
        {
            get;
            set;
        }

        public int ConnectionDelay
        {
            get;
            set;
        }

        public Controls OpenControlOnStart
        {
            get;
            set;
        }

        public bool HideMainMenu
        {
            get;
            set;
        }

        public bool AutoHideMainMenu
        {
            get;
            set;
        }

        public SequenceExecutionViewType SequenceExecutionViewType
        {
            get;
            set;
        }

        public int StartSequenceAfterStartDelay
        {
            get;
            set;
        }

        public bool IsTableCollapsed
        {
            get;
            set;
        }

        public List<ControlInfo> ControlInfos
        {
            get { return controlInfos; }
            set { controlInfos = value; }
        }

        public static ConnectionDialog.ConnectionContainer SelectedConnection
        {
            get { return selectedConnection; }
            set { selectedConnection = value; }
        }

        public bool AllowOpenSequenceInSequenceExecution
        {
            get;
            set;
        }

        public bool AllowStartSequenceInSequenceExecution
        {
            get;
            set;
        }

        public bool AllowStopSequenceInSequenceExecution
        {
            get;
            set;
        }

        public bool AllowPauseSequenceInSequenceExecution
        {
            get;
            set;
        }

        public bool HideAllCommandsInSequenceExecution
        {
            get;
            set;
        }

        public bool BackupEnabled
        {
            get;
            set;
        }

        public int BackupPeriod
        {
            get;
            set;
        }

        public int DeleteBackupPeriod
        {
            get;
            set;
        }

        public bool MoreClients { get; set; }

        public string Language
        {
            get { return language; }
            set
            {
                language = value;
                if (OnLanguageChanged != null)
                {
                    OnLanguageChanged(this, new LanguageChangeEventArgs { Language = value });
                }
            }
        }

        public WindowSetting FindUsagesSetting
        {
            get { return findUsagesSetting; }
            set { findUsagesSetting = value; }
        }

        public WindowSetting DebugSetting
        {
            get { return debugSetting; }
            set { debugSetting = value; }
        }

        public WindowSetting SetupSetting
        {
            get { return setupSetting; }
            set { setupSetting = value; }
        }

        public bool ShowTreeWithFullImage
        {
            get { return showTreeWithFullImage; }
            set { showTreeWithFullImage = value; }
        }

        public List<AccessKeyProviderSettings> AccessKeyProviderSettingses
        {
            get { return accessKeyProviderSettingses; }
            set { accessKeyProviderSettingses = value; }
        }

        public static Size AdjustSize(Size value, Point windowLocation)
        {
            Size windowSize = new Size();
            if ((value.Height + windowLocation.Y) > SystemParameters.VirtualScreenHeight)
            {
                windowSize.Height = Math.Min(SystemParameters.VirtualScreenHeight, value.Height);
            }
            else
            {
                windowSize.Height = value.Height;
            }

            if ((value.Width + windowLocation.X) > SystemParameters.VirtualScreenWidth)
            {
                windowSize.Width = Math.Min(SystemParameters.VirtualScreenWidth, value.Width);
            }
            else
            {
                windowSize.Width = value.Width;
            }
            return windowSize;
        }

        public static Point AdjustLocation(Point value, double minWidth, double minHeight)
        {
            Point windowLocation = new Point();
            if (value.X <= SystemParameters.VirtualScreenWidth)
            {
                windowLocation.X = Math.Max(SystemParameters.VirtualScreenLeft, value.X);
            }
            else if (value.X > SystemParameters.VirtualScreenWidth)
            {
                windowLocation.X = SystemParameters.VirtualScreenWidth - minWidth;
            }

            if (value.Y <= SystemParameters.VirtualScreenHeight)
            {
                windowLocation.Y = Math.Max(SystemParameters.VirtualScreenTop, value.Y);
            }
            else if (value.Y > SystemParameters.VirtualScreenHeight)
            {
                windowLocation.Y = SystemParameters.VirtualScreenHeight - minHeight;
            }
            return windowLocation;
        }
    }

    public class ControlInfo
    {
        public string Name;
        public List<PropertyInfo> Values;
    }

    public class PropertyInfo
    {
        public string Name;
        public string Value;
    }

    public class WindowSetting
    {
        private Point location;
        private Size size;
        private bool isEmpty = true;

        public Point Location
        {
            get { return location; }
            set
            {
                location = value;
                isEmpty = false;
            }
        }

        public Size Size
        {
            get { return size; }
            set
            {
                size = value;
                isEmpty = false;
            }
        }

        public bool IsEmpty
        {
            get { return isEmpty; }
        }
    }

    public class AccessKeyProviderSettings : ICloneable
    {
        private bool isActive;
        private string typeName;
        private string name;
        private bool hasConfigControl;

        private List<AccessKeyProviderParameter> parameters;

        public string TypeName
        {
            get { return typeName; }
            set { typeName = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public bool IsActive
        {
            get { return isActive; }
            set { isActive = value; }
        }

        public bool HasConfigControl
        {
            get { return hasConfigControl; }
            set { hasConfigControl = value; }
        }

        public List<AccessKeyProviderParameter> Parameters
        {
            get { return parameters; }
            set { parameters = value; }
        }

        public object Clone()
        {
            return new AccessKeyProviderSettings
            {
                Name = Name,
                TypeName = TypeName,
                IsActive = IsActive,
                HasConfigControl = HasConfigControl,
                Parameters = Parameters == null ? null : Parameters.Select(p => p.Clone() as AccessKeyProviderParameter).ToList(),
            };
        }
    }

    public class AccessKeyProviderParameter : ICloneable
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public object Clone()
        {
            return new AccessKeyProviderParameter
            {
                Name = Name,
                Value = Value,
            };
        }
    }

    public enum Controls
    {
        [Description("Enum_None")]
        None,
        [Description("MainMenu_ComponentConfiguration")]
        ComponentsConfiguration,
        [Description("MainMenu_Editor")]
        SequenceEditor,
        [Description("MainMenu_Execution")]
        SequenceExecution,
        [Description("MainMenu_Setting")]
        Settings
    }

    
}
