using MTFClientServerCommon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using MTFApp.ServerService;
using MTFApp.ServerService.Components;

namespace MTFApp.UIHelpers
{
    /// <summary>
    /// Interaction logic for ComplexTypeViewer.xaml
    /// </summary>
    public partial class ComplexTypeViewer : UserControl, INotifyPropertyChanged
    {
        private bool isComplexType;
        private List<MTFKnownClassPropertyDescription> values;
        private bool isCollapsed = true;
        private bool generate;
        private Dictionary<string, GenericClassInfo> cache = new Dictionary<string, GenericClassInfo>();
        private HashSet<string> propertyPath = new HashSet<string>();

        public ComplexTypeViewer()
        {
            InitializeComponent();
            this.root.DataContext = this;
            isComplexType = false;
        }



        public string TypeName
        {
            get { return (string)GetValue(TypeNameProperty); }
            set { SetValue(TypeNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TypeName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TypeNameProperty =
            DependencyProperty.Register("TypeName", typeof(string), typeof(ComplexTypeViewer),
            new FrameworkPropertyMetadata() { BindsTwoWayByDefault = false, PropertyChangedCallback = PropertyChangedCallback });

        private static void PropertyChangedCallback(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (source is ComplexTypeViewer && e.NewValue is string)
            {
                (source as ComplexTypeViewer).IsCollapsed = true;
                (source as ComplexTypeViewer).generate = true;
                (source as ComplexTypeViewer).IsComplexType = Type.GetType(e.NewValue.ToString()) == null;
            }
        }

        public bool IsComplexType
        {
            get { return isComplexType; }
            set
            {
                isComplexType = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsCollapsed
        {
            get { return isCollapsed; }
            set
            {
                isCollapsed = value;
                NotifyPropertyChanged();
                if (!value && generate)
                {
                    Generate();
                    generate = false;
                }
            }
        }

        public ICommand IsCollapsedChanged
        {
            get
            {
                return new Command(() => IsCollapsed = !isCollapsed);
            }
        }


        public List<MTFKnownClassPropertyDescription> Values
        {
            get { return values; }
            set
            {
                values = value;
                NotifyPropertyChanged();
            }
        }


        private void Generate()
        {
            GenericClassInfo classInfo = ServiceClientsContainer.Get<ComponentsClient>().GetClassInfo(TypeName);
            propertyPath.Add(TypeName);
            if (classInfo != null && classInfo.Properties != null)
            {
                Values = GenerateValues(classInfo.Properties);
            }
        }

        private List<MTFKnownClassPropertyDescription> GenerateValues(IEnumerable<GenericPropertyInfo> properties)
        {
            var output = new List<MTFKnownClassPropertyDescription>();
            foreach (var item in properties)
            {
                var tmp = new MTFKnownClassPropertyDescription()
                {
                    Name = item.Name,
                    Type = item.Type
                };
                if (Type.GetType(item.Type) == null && item.Type!=TypeName)
                {
                    if (!cache.ContainsKey(item.Type))
                    {
                        GenericClassInfo classInfo = ServiceClientsContainer.Get<ComponentsClient>().GetClassInfo(item.Type);
                        cache.Add(item.Type, classInfo);
                        if (classInfo != null && classInfo.Properties != null && propertyPath.Add(classInfo.FullName))
                        {
                            tmp.SubParameters = GenerateValues(classInfo.Properties);
                            propertyPath.Remove(classInfo.FullName);
                        }
                    }
                    else
                    {
                        var classInfo = cache[item.Type];
                        if (classInfo!=null && propertyPath.Add(classInfo.FullName))
                        {
                            tmp.SubParameters = GenerateValues(classInfo.Properties);
                            propertyPath.Remove(classInfo.FullName);
                        }
                    }
                    
                }
                output.Add(tmp);
            }
            return output;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void ListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            UIHelper.RaiseScrollEvent(sender, e);
        }
    }

    public class MTFKnownClassPropertyDescription
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public List<MTFKnownClassPropertyDescription> SubParameters { get; set; }

        public override string ToString()
        {
            return Name + "(" + Type + ")";
        }
    }
}
