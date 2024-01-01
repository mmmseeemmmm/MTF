using MTFClientServerCommon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MTFApp.ServerService;
using MTFApp.ServerService.Components;

namespace MTFApp.UIHelpers.Editors
{
    /// <summary>
    /// Interaction logic for MTFActivityResultEditor.xaml
    /// </summary>
    public partial class MTFActivityResultEditor : MTFEditorBase
    {
        bool load;
        bool isEdited;
        private bool fillItemsLock;
        private bool typeNameItemsLock;

        public MTFActivityResultEditor()
        {
            InitializeComponent();

            root.DataContext = this;
            this.PropertyChanged += MTFActivityResultEditor_PropertyChanged;
            items = new ObservableCollection<ActivityResultEditorItemWrapper>();
            items.CollectionChanged += items_CollectionChanged;
        }

        void items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (INotifyPropertyChanged item in e.NewItems)
                {
                    item.PropertyChanged += item_PropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (INotifyPropertyChanged item in e.OldItems)
                {
                    item.PropertyChanged -= item_PropertyChanged;
                }
            }
        }

        private void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var selectedItem = sender as ActivityResultEditorItemWrapper;
            int index = items.IndexOf(selectedItem);
            if (items.Count > index + 1)
            {
                while (index + 1 < items.Count)
                {
                    items.RemoveAt(index + 1);
                }
            }
            var newItem = selectedItem.SelectedItem;
            if (newItem is GenericPropertyInfo)
            {
                var propertyItem = newItem as GenericPropertyInfo;
                if (propertyItem.Type.Contains("System.Collections.Generic") || propertyItem.Type.EndsWith("[]"))
                {
                    string typeName = null;
                    Type type = Type.GetType(propertyItem.Type);
                    if (type != null)
                    {
                        typeName = type.IsGenericType ? type.GenericTypeArguments[0].FullName : type.GetElementType().FullName;
                    }
                    else
                    {
                        var classInfo = ServiceClientsContainer.Get<ComponentsClient>().GetClassInfo(propertyItem.Type);
                        if (classInfo != null)
                        {
                            typeName = classInfo.FullName;
                        }
                        else
                        {
                            type = Type.GetType(propertyItem.AssemblyQualifiedName);
                            if (type != null)
                            {
                                typeName = type.IsGenericType ? type.GenericTypeArguments[0].FullName : type.GetElementType().FullName;
                            }
                        }
                    }

                    CreateIndexerItem(new GenericPropertyInfo() { Type = typeName, Name = propertyItem.Name });
                }
                else
                {
                    var typeInfo = new MTFClientServerCommon.Helpers.TypeInfo(propertyItem.Type);
                    if (typeInfo.Type == null)
                    {
                        GenericClassInfo gci = ServiceClientsContainer.Get<ComponentsClient>().GetClassInfo(propertyItem.Type);
                        if (gci != null)
                        {
                            CreateGenericPropertyItem(gci.Properties);
                        }
                    }
                }
            }
            NotifyPropertyChanged("Items");
        }

        void MTFActivityResultEditor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Items" && (!load || ((Path == null || Path.Count == 0) && Items != null)) && !fillItemsLock && !typeNameItemsLock)
            {
                var newPath = new List<GenericPropertyInfo>();
                foreach (var item in Items)
                {
                    if (item.SelectedItem is GenericPropertyInfo)
                    {
                        newPath.Add((GenericPropertyInfo)item.SelectedItem);
                    }
                }
                this.load = false;
                this.isEdited = true;
                Path = newPath;
                this.isEdited = false;
            }

        }

        #region TermPropertyName
        public string TermPropertyName
        {
            get => (string)GetValue(TermPropertyNameProperty);
            set => SetValue(TermPropertyNameProperty, value);
        }

        public static readonly DependencyProperty TermPropertyNameProperty =
            DependencyProperty.Register("TermPropertyName", typeof(string), typeof(MTFActivityResultEditor),
            new FrameworkPropertyMetadata() { BindsTwoWayByDefault = true });
        #endregion

        #region Path dependency property
        public List<GenericPropertyInfo> Path
        {
            get => (List<GenericPropertyInfo>)GetValue(PathProperty);
            set => SetValue(PathProperty, value);
        }

        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register("Path", typeof(List<GenericPropertyInfo>), typeof(MTFActivityResultEditor),
            new PropertyMetadata(PathPropertyChanged));

        private static void PathPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            MTFActivityResultEditor editor = (MTFActivityResultEditor)source;
            if (editor.fillItemsLock && editor.typeNameItemsLock)
            {
                return;
            }
            if (editor.load && editor.Path != null)
            {
                editor.FillItems();
                //editor.isEdited = false;
            }
            else
            {
                if (!editor.isEdited)
                {
                    editor.FillItems();
                    //editor.isEdited = false;
                }

            }
        }
        #endregion Path dependency property


        #region TypeName dependency property
        public string TypeName
        {
            get => (string)GetValue(TypeNameProperty);
            set => SetValue(TypeNameProperty, value);
        }
        public static readonly DependencyProperty TypeNameProperty =
            DependencyProperty.Register("TypeName", typeof(string), typeof(MTFActivityResultEditor),
            new PropertyMetadata(TypeNamePropertyChanged));

        private static void TypeNamePropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            MTFActivityResultEditor editor = (MTFActivityResultEditor)source;
            editor.typeNameItemsLock = true;
            if (!string.IsNullOrEmpty(editor.TypeName))
            {
                var typeInfo = new MTFClientServerCommon.Helpers.TypeInfo(editor.TypeName);
                editor.Items.Clear();
                if (typeInfo.IsUnknownType)
                {
                    editor.load = true;
                    //GenericClassInfo gci = null;
                    //var typeName = editor.TypeName;
                    editor.SetItemAsync(editor.TypeName);
                    //Task.Run(() => gci = MTFClient.GetMTFClient().GetClassInfo(typeName)).Wait();
                    //if (gci != null)
                    //{
                    //    if (editor.TypeName.Contains("System.Collections.Generic") || editor.TypeName.EndsWith("[]"))
                    //    {
                    //        editor.CreateIndexerItem(new GenericPropertyInfo { Type = gci.FullName });
                    //    }
                    //    else
                    //    {
                    //        editor.CreateGenericPropertyItem(gci.Properties);
                    //    }
                    //}
                }
                else if (typeInfo.IsCollection)
                {
                    editor.load = true;
                    editor.Items.Clear();
                    var type = Type.GetType(editor.TypeName);
                    var elementType = type.IsGenericType ? type.GenericTypeArguments[0] : type.GetElementType();
                    if (elementType != null)
                    {
                        editor.CreateIndexerItem(new GenericPropertyInfo() { Type = elementType.FullName });
                    }
                }
            }
            editor.typeNameItemsLock = false;
        }

        private async void SetItemAsync(string typeName)
        {
            typeNameItemsLock = true;
            GenericClassInfo gci = null;
            await Task.Run(() => gci = ServiceClientsContainer.Get<ComponentsClient>().GetClassInfo(typeName));
            if (gci != null)
            {
                if (!fillItemsLock && items.Count==0)
                {
                    if (TypeName.Contains("System.Collections.Generic") || TypeName.EndsWith("[]"))
                    {
                        CreateIndexerItem(new GenericPropertyInfo { Type = gci.FullName });
                    }
                    else
                    {
                        CreateGenericPropertyItem(gci.Properties);
                    } 
                }
            }
            typeNameItemsLock = false;
        }

        #endregion TypeName dependency property

        public Visibility InDesignerVisibility
        {
            get
            {
                if (UIHelper.FindParent<MTFTermDesigner>(this) == null)
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }


        private async void FillItems()
        {
            fillItemsLock = true;
            load = true;
            items.Clear();
            if (Path != null && Path.Count > 0)
            {
                var typeName = TypeName;
                GenericClassInfo gci = null;
                await Task.Run(() => gci = ServiceClientsContainer.Get<ComponentsClient>().GetClassInfo(typeName));
                var indexer = Path[0] as GenericPropertyIndexer;
                if (indexer != null)
                {
                    CreateIndexerItem(new GenericPropertyInfo() { Type = indexer.Type });
                    var selectedItem = items.Last().GenericPropertyItems[1];
                    (selectedItem as GenericPropertyIndexer).Index = indexer.Index;
                    items.Last().SelectedItem = selectedItem;
                }
                else
                {
                    if (gci == null)
                    {
                        return;
                    }
                    CreateGenericPropertyItem(gci.Properties);
                    AssignSelectedItem(Path[0]);
                }
                for (int i = 0; i < Path.Count - 1; i++)
                {
                    AssignSelectedItem(Path[i + 1]);
                }
            }
            load = false;
            NotifyPropertyChanged("Items");
            isEdited = false;
            fillItemsLock = false;
        }

        private void AssignSelectedItem(GenericPropertyInfo genericPropertyInfo)
        {
            var selectedItem = items.Last().GenericPropertyItems.FirstOrDefault(x => x is GenericPropertyInfo && ((GenericPropertyInfo)x).Name == genericPropertyInfo.Name);
            if (selectedItem is GenericPropertyIndexer)
            {
                (selectedItem as GenericPropertyIndexer).Index = (genericPropertyInfo as GenericPropertyIndexer).Index;
            }
            items.Last().SelectedItem = selectedItem;
        }

        private void CreateGenericPropertyItem(IEnumerable collection)
        {
            var item = new ActivityResultEditorItemWrapper();
            foreach (GenericPropertyInfo x in collection)
            {
                item.GenericPropertyItems.Add(x);
            }
            item.GenericPropertyItems.Add(string.Empty);
            this.items.Add(item);
        }

        private void CreateIndexerItem(GenericPropertyInfo genericPropertyInfo)
        {
            var item = new ActivityResultEditorItemWrapper();
            item.GenericPropertyItems.Add(string.Empty);
            var indexer = new GenericPropertyIndexer(genericPropertyInfo);
            item.GenericPropertyItems.Add(indexer);
            this.items.Add(item);
        }

        private void MTFEditor_ValueChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged("Items");
        }

        private ObservableCollection<ActivityResultEditorItemWrapper> items;

        public ObservableCollection<ActivityResultEditorItemWrapper> Items
        {
            get => items;
            set => items = value;
        }
    }

    public class ActivityResultEditorItemWrapper : NotifyPropertyBase
    {
        public ActivityResultEditorItemWrapper()
        {
            GenericPropertyItems = new List<object>();
        }

        public List<object> GenericPropertyItems { get; set; }
        private object selectedItem;

        public object SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                NotifyPropertyChanged();
            }
        }
    }
}
