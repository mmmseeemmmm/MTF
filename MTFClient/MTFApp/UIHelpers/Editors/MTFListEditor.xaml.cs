using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using MTFApp.UIHelpers.DragAndDrop;
using MTFClientServerCommon.Helpers;
using MTFClientServerCommon;
using MTFClientServerCommon.Mathematics;

namespace MTFApp.UIHelpers.Editors
{
    /// <summary>
    /// Interaction logic for MTFListEditor.xaml
    /// </summary>
    public partial class MTFListEditor : MTFEditorBase
    {
        private bool handleEvent = true;
        private bool editValue = true;
        private bool editItems = true;
        private ObservableCollection<ListItemWrapper> items;
        private Command removeItemCommand = null;
        private Command moveUpItemCommand = null;
        private Command moveDownItemCommand = null;
        Point? targetStartPoint = null;
        private TouchHelper touch;

        public MTFListEditor()
        {
            InitializeComponent();
            root.DataContext = this;
            if (Value != null)
            {
                CreateNewCollectionForListEditor();
            }
            PropertyChanged += MTFListEditor_PropertyChanged;
            removeItemCommand = new Command(RemoveItem, () => items != null && items.Count > 1);
            moveUpItemCommand = new Command(MoveUpItem, () => selectedItem != null && items.IndexOf(selectedItem) > 0);
            moveDownItemCommand = new Command(MoveDownItem, () => selectedItem != null && items.IndexOf(selectedItem) < items.Count - 1);

            Unloaded += MTFListEditorUnloaded;
        }

        private void MTFListEditorUnloaded(object sender, RoutedEventArgs e)
        {
            Unload();
            PropertyChanged -= MTFListEditor_PropertyChanged;
        }

        #region TypeName dependency property
        public string TypeName
        {
            get => (string)GetValue(TypeNameProperty);
            set => SetValue(TypeNameProperty, value);
        }
        public static readonly DependencyProperty TypeNameProperty =
            DependencyProperty.Register("TypeName", typeof(string), typeof(MTFListEditor),
            new FrameworkPropertyMetadata { BindsTwoWayByDefault = false, PropertyChangedCallback = TypeNamePropertyChanged });

        private static void TypeNamePropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (source is MTFListEditor editor && editor.Value != null)
            {
                editor.FillItems(editor.Value);
            }
        }
        #endregion TypeName dependency property


        public Type ElementType
        {
            get
            {
                if (!string.IsNullOrEmpty(TypeName))
                {
                    var type = Type.GetType(TypeName);
                    if (type != null)
                    {
                        Type elType = null;
                        if (type.IsGenericType)
                        {
                            elType = type.GenericTypeArguments[0];
                        }
                        if (type.IsArray)
                        {
                            elType = type.GetElementType();
                        }
                        if (elType != null)
                        {
                            var typeInfo = new TypeInfo(elType.FullName);
                            if (typeInfo.IsCollection || typeInfo.IsSimpleType)
                            {
                                return elType;
                            }
                            if (typeInfo.IsUnknownType)
                            {
                                return null;
                            }
                            return elType;
                        }
                    }
                }
                return null;
            }
        }

        public string ElementTypeName => ElementType != null ? ElementType.FullName : GetElementTypeFromUnknownClass(TypeName);

        public TypeInfo TypeInfo => new TypeInfo(TypeName);

        public ObservableCollection<ListItemWrapper> Items => items;

        private ListItemWrapper selectedItem;

        public ListItemWrapper SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                NotifyPropertyChanged();
                RaiseCanExecuteOfUpDown();
            }
        }


        public bool IsActivityResult { get; set; }

        public ICommand RemoveItemCommand => removeItemCommand;

        public ICommand AddItemCommand => new Command(AddItem);

        public ICommand RemoveListCommand => new Command(RemoveList);

        public ICommand CreateListCommand => new Command(CreateList);

        public ICommand RemoveCommand => new Command(Remove);

        public ICommand MoveUpItemCommand => moveUpItemCommand;

        public ICommand MoveDownItemCommand => moveDownItemCommand;

        public Visibility InDesignerVisibility => (UIHelper.FindParent<MTFTermDesigner>(this) == null) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

        private void Remove()
        {
            IsActivityResult = false;
            NotifyPropertyChanged(nameof(IsActivityResult));
            Value = null;
        }



        private void MoveDownItem(object param)
        {
            int index = GetIndexByCommandParam(param);
            if (index != -1 && index + 1 < items.Count)
            {
                items.Move(index, index + 1);
                UpdateIndexes(index);
                AssignNewValue();
                RaiseCanExecuteOfUpDown();
            }
        }

        private void MoveUpItem(object param)
        {
            int index = GetIndexByCommandParam(param);
            if (index != -1 && index - 1 >= 0)
            {
                items.Move(index, index - 1);
                UpdateIndexes(index - 1);
                AssignNewValue();
                RaiseCanExecuteOfUpDown();
            }
        }

        private void RaiseCanExecuteOfUpDown()
        {
            moveDownItemCommand.RaiseCanExecuteChanged();
            moveUpItemCommand.RaiseCanExecuteChanged();
        }

        private void CreateNewCollectionForListEditor()
        {
            items = new ObservableCollection<ListItemWrapper>();
            this.items.CollectionChanged += items_CollectionChanged;
        }

        void MTFListEditor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Items) && editValue && handleEvent)
            {
                AssignNewValue();
                IsActivityResult = false;
                NotifyPropertyChanged(nameof(IsActivityResult));
            }
            removeItemCommand.RaiseCanExecuteChanged();
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

        void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(ListItemWrapper.Index))
            {
                AssignNewValue();
            }
        }

        protected override void OnPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == ValueProperty && !string.IsNullOrEmpty(TypeName))
            {
                if (e.NewValue != items)
                {
                    FillItems(e.NewValue);
                }
            }
        }

        private void FillItems(object newValue)
        {
            if (!editItems)
            {
                return;
            }
            if (newValue is TermWrapper wrapper)
            {
                newValue = wrapper.Value;
            }
            switch (newValue)
            {
                case ActivityResultTerm _:
                    IsActivityResult = true;
                    NotifyPropertyChanged(nameof(IsActivityResult));
                    return;
                case ICollection collection:
                {
                    int i = 0;
                    if (items == null)
                    {
                        CreateNewCollectionForListEditor();
                    }
                    foreach (var item in collection)
                    {
                        if (i < items.Count)
                        {
                            handleEvent = false;
                            items[i].Value = item;
                            items[i].TypeName = ElementTypeName;
                            handleEvent = true;
                        }
                        else
                        {
                            items.Add(new ListItemWrapper { Value = item, TypeName = ElementTypeName, Index = items.Count });
                        }
                        i++;
                    }
                    if (i < items.Count)
                    {
                        while (items.Count != i)
                        {
                            items.RemoveAt(i);
                        }
                    }

                    break;
                }

                default:
                    items = null;
                    break;
            }

            editValue = false;
            NotifyPropertyChanged(nameof(Items));
            editValue = true;
        }

        private object CreateNewItem()
        {
            if (ElementType != null)
            {
                if (ElementType == typeof(string))
                {
                    if (EditorMode == EditorModes.UseTerm)
                    {
                        return new ConstantTerm(typeof(string)) { Value = string.Empty };
                    }
                    return string.Empty;
                }
                else
                {
                    if (ElementType.IsAbstract && ElementType.FullName == typeof(Term).FullName)
                    {
                        return Activator.CreateInstance(typeof(EmptyTerm));
                    }
                    if (ElementType.IsArray)
                    {
                        var array = CreateArray(ElementType);
                        if (EditorMode == EditorModes.UseTerm)
                        {
                            return new TermWrapper() { Value = null, TypeName = array.GetType().FullName };
                        }
                        return array;
                    }
                    if (EditorMode == EditorModes.UseTerm)
                    {
                        if (ElementType.IsGenericType)
                        {
                            return new TermWrapper() { Value = null, TypeName = ElementType.FullName };
                        }
                        return new ConstantTerm(ElementType) { Value = Activator.CreateInstance(ElementType) };
                    }
                    return Activator.CreateInstance(ElementType);
                }
            }
            else
            {
                if (EditorMode == EditorModes.UseTerm)
                {
                    return new TermWrapper() { TypeName = ElementTypeName };
                }
                return null;
            }
        }

        private object CreateArray(Type elementType)
        {
            var innerType = elementType.GetElementType();
            if (innerType.IsArray)
            {
                var array = CreateArray(elementType.GetElementType());
                return Array.CreateInstance(array.GetType(), 0);
            }
            else
            {
                return Array.CreateInstance(innerType, 0);
            }
        }

        private Type GetElementTypeFromTypeName(string typeName)
        {
            Type elementType = null;
            if (EditorMode == EditorModes.UseTerm)
            {
                elementType = typeof(Term);
            }
            else
            {
                var type = Type.GetType(typeName);
                if (type != null)
                {
                    if (type.IsArray)
                    {
                        elementType = type.GetElementType();
                    }
                    if (type.IsGenericType)
                    {
                        elementType = type.GenericTypeArguments[0];
                    }
                }
                else
                {
                    elementType = typeof(GenericClassInstanceConfiguration);
                }
            }
            return elementType;
        }

        private void AssignNewValue()
        {
            if (handleEvent)
            {
                editItems = false;
                Value = UpdateValue();
                editItems = true;
            }
        }

        private object UpdateValue()
        {
            if (items == null)
            {
                return null;
            }
            Type elementType = GetElementTypeFromTypeName(TypeName);

            if (TypeInfo.IsArray)
            {
                var newValue = Array.CreateInstance(elementType, items.Count);
                for (int i = 0; i < items.Count; i++)
                {
                    var val = items[i].Value;
                    if (val != null && elementType.FullName == typeof(Term).FullName && !(val is Term))
                    {
                        val = new TermWrapper() { Value = val, TypeName = ElementTypeName };
                    }
                    newValue.SetValue(val, i);
                }
                return newValue;
            }
            if (TypeInfo.IsGenericType)
            {
                var newValue = Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
                foreach (var item in items)
                {
                    var val = item.Value;
                    if (val != null && elementType.FullName == typeof(Term).FullName && !(val is Term))
                    {
                        val = new TermWrapper() { Value = val, TypeName = ElementTypeName };
                    }
                    (newValue as IList).Add(val);
                }
                return newValue;
            }
            return null;
        }

        private int GetIndexByCommandParam(object param)
        {
            int index = -1;
            if (param is ListItemWrapper wrapper)
            {
                index = items.IndexOf(wrapper);
            }
            return index;
        }

        private void UpdateIndexes(int startIndex)
        {
            if (startIndex < items.Count)
            {
                for (int i = startIndex; i < items.Count; i++)
                {
                    items[i].Index = i;
                }
            }
        }

        private void RemoveItem(object param)
        {
            int index = GetIndexByCommandParam(param);
            if (index == -1)
            {
                index = items.Count - 1;
            }
            items.RemoveAt(index);
            UpdateIndexes(index);
            AssignNewValue();
        }

        private void AddItem(object param)
        {
            int index = GetIndexByCommandParam(param);
            var item = new ListItemWrapper() { Value = CreateNewItem(), TypeName = ElementTypeName };
            if (index != -1)
            {
                item.Index = index + 1;
                items.Insert(index + 1, item);
                UpdateIndexes(item.Index + 1);
            }
            else
            {
                item.Index = items.Count;
                items.Add(item);
            }
            selectedItem = item;
            AssignNewValue();
            NotifyPropertyChanged(nameof(SelectedItem));
        }

        private string GetElementTypeFromUnknownClass(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return null;
            }
            if (typeName.EndsWith("[]"))
            {
                int index = typeName.LastIndexOf("[]");
                return typeName.Substring(0, index);
            }
            else
            {
                int index1 = typeName.IndexOf("[[");
                typeName = typeName.Remove(0, index1 + 2);
                int index2 = typeName.LastIndexOf("]]");
                typeName = typeName.Remove(index2, typeName.Length - index2);
                var type = Type.GetType(typeName);
                if (type != null)
                {
                    return type.FullName;
                }
                if (!typeName.Contains("["))
                {
                    return typeName.Split(',')[0];
                }

                return typeName;
            }
        }

        private void RemoveList()
        {
            items = null;
            NotifyPropertyChanged(nameof(Items));
        }

        private void CreateList()
        {
            CreateNewCollectionForListEditor();
            AddItem(null);
            NotifyPropertyChanged(nameof(Items));
        }

        private void listBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            UIHelper.RaiseScrollEvent(sender, e);
        }

        private void target_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (targetStartPoint != null && Setting.AllowDragDrop)
            {
                Point mousePos = e.GetPosition(null);
                Vector diff = (Point)targetStartPoint - mousePos;
                if (e.LeftButton == MouseButtonState.Pressed &&
                    (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    var dragElement = sender as FrameworkElement;
                    DataObject dragData = new DataObject(DragAndDropTypes.GetActivityResult, new object());
                    var result = DragDrop.DoDragDrop(dragElement, dragData, DragDropEffects.All);
                    if (result != DragDropEffects.None)
                    {
                        var targetActivity = dragData.GetData(DragAndDropTypes.GetActivityResult);
                        var selectedActivity = dragData.GetData(DragAndDropTypes.SelectedActivity);
                        AssignActivityResult(targetActivity, selectedActivity);
                    }
                }
            }
        }

        private void target_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (touch == null)
                {
                    touch = TouchHelper.Instance;
                }
                touch.SetAction(AssignActivityResult, sender);
            }
            else
            {
                targetStartPoint = e.GetPosition(null); 
            }
        }

        private void target_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            targetStartPoint = null;
        }

        private void AssignActivityResult(object targetActivity, object selectedActivity)
        {
            if (targetActivity != null && targetActivity != selectedActivity)
            {
                Value = new ActivityResultTerm { Value = targetActivity as MTFSequenceActivity };
            }
            IsActivityResult = true;
            NotifyPropertyChanged(nameof(IsActivityResult));
        }

        public void Unload()
        {
            if (Items!=null)
            {
                Items.CollectionChanged -= items_CollectionChanged;
                foreach (var listItemWrapper in Items)
                {
                    listItemWrapper.PropertyChanged -= item_PropertyChanged;
                }
            }
        }
    }


    public class ListItemWrapper : NotifyPropertyBase
    {
        private object myVar;
        private string typeName;
        private int index;

        public object Value
        {
            get => myVar;
            set
            {
                //if (value != myVar)
                //{
                myVar = value;
                NotifyPropertyChanged();
                //}
            }
        }

        public string TypeName
        {
            get => typeName;
            set
            {
                typeName = value;
                NotifyPropertyChanged();
            }
        }


        public int Index
        {
            get => index;
            set
            {
                index = value;
                NotifyPropertyChanged();
            }
        }
    }
}
