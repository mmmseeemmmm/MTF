using MTFApp.UIHelpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MTFApp.UIControls.TagListBoxControl
{
    public delegate void SetItemsEventHandler(object sender, EventArgs e);

    public class TagListBox : Control
    {
        private bool canFocused;
        private bool allowAddNewItem = true;
        private bool allowUpdateItemsSource = true;
        private bool updateFromValue;

        public event SetItemsEventHandler SetItems;

        static TagListBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TagListBox), new FrameworkPropertyMetadata(typeof(TagListBox)));
        }

        public TagListBox()
        {
            SourceData = new ObservableCollection<TagListItem>();
            CreateNewItem();
        }

        #region Properties

        private TagListItem selectedItem;
        private bool isOnlyHorizontal;

        public TagListItem SelectedItem
        {
            get { return selectedItem; }
            set
            {
                if (this.selectedItem==null && value!=null)
                {
                    var newItem = SourceData.LastOrDefault(x => !x.IsClosed);
                    if (newItem!=null)
                    {
                        SourceData.Remove(newItem);
                    }
                }
                if (selectedItem!=null && !this.selectedItem.IsClosed && value!=this.selectedItem && value!=null)
                {
                    this.allowAddNewItem = false;
                    this.allowUpdateItemsSource = false;
                    this.selectedItem.IsClosed = true;
                    this.allowUpdateItemsSource = true;
                    this.allowAddNewItem = true;
                }
                if (value != null && value.IsClosed)
                {
                    allowUpdateItemsSource = false;
                    value.IsClosed = false;
                    allowUpdateItemsSource = true;
                    value.PropertyChanged += NewItem_PropertyChanged;
                }
                selectedItem = value;
            }
        }

        public int Count
        {
            get
            {
                return this.ItemSource == null ? 0 : this.ItemSource.Count;
            }
        }

        public bool CanFocused
        {
            get { return canFocused; }
        }

        public bool IsOnlyHorizontal
        {
            get { return isOnlyHorizontal; }
            set { isOnlyHorizontal = value; }
        }

        #endregion

        #region DependencyProperty

        #region ItemSource DependencyProperty

        public List<string> ItemSource
        {
            get { return (List<string>)GetValue(ItemSourceProperty); }
            set { SetValue(ItemSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemSourceProperty =
            DependencyProperty.Register("ItemSource", typeof(List<string>), typeof(TagListBox),
                new FrameworkPropertyMetadata() { BindsTwoWayByDefault = true, PropertyChangedCallback = PropertyChanged });

        private static void PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = d as TagListBox;
            if(!sender.allowUpdateItemsSource || sender.updateFromValue)
            {
                return;
            }
            if (e.NewValue is List<string>)
            {
                sender.SourceData = new ObservableCollection<TagListItem>();
                foreach (var item in e.NewValue as List<string>)
                {
                    sender.SourceData.Add(new TagListItem() { Value = item });
                }
                sender.CreateNewItem();
            }
        }
        #endregion

        #region SourceData DependencyProperty

        public ObservableCollection<TagListItem> SourceData
        {
            get { return (ObservableCollection<TagListItem>)GetValue(SourceDataProperty); }
            set { SetValue(SourceDataProperty, value); }
        }

        public static readonly DependencyProperty SourceDataProperty =
            DependencyProperty.Register("SourceData", typeof(ObservableCollection<TagListItem>), typeof(TagListBox), new PropertyMetadata(null));

        #endregion


        #region ItemBackground DependencyProperty

        public Brush ItemBackground
        {
            get { return (Brush)GetValue(ItemBackgroundProperty); }
            set { SetValue(ItemBackgroundProperty, value); }
        }

        public static readonly DependencyProperty ItemBackgroundProperty =
            DependencyProperty.Register("ItemBackground", typeof(Brush), typeof(TagListBox), new PropertyMetadata(null));

        #endregion


        #endregion

        #region Commands

        public ICommand RemoveItemCommand
        {
            get
            {
                return new Command(item =>
                {
                    SourceData.Remove(item as TagListItem);
                    UpdateItemSource();
                });
            }
        }

        #endregion

        #region Private methods

        private void CreateNewItem()
        {
            var newItem = new TagListItem() { Value = string.Empty, IsClosed = false };
            newItem.PropertyChanged += NewItem_PropertyChanged;
            SourceData.Add(newItem);
        }

        private void NewItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName== "Value")
            {
                this.updateFromValue = true;
                ItemSource = SourceData.Where(x => x.IsClosed || !string.IsNullOrEmpty(x.Value)).Select(x => x.Value).ToList();
                if (SetItems != null)
                {
                    SetItems(this, EventArgs.Empty);
                }
                this.updateFromValue = false;
            }
            else if (e.PropertyName == "IsClosed")
            {
                var lastItem = sender as TagListItem;
                lastItem.PropertyChanged -= NewItem_PropertyChanged;
                canFocused = true;
                if (allowAddNewItem)
                {
                    CreateNewItem();
                }
                UpdateItemSource();
            }
        }



        private void UpdateItemSource()
        {
            ItemSource = SourceData.Where(x => x.IsClosed || !string.IsNullOrEmpty(x.Value)).Select(x => x.Value).ToList();
            if (SetItems != null)
            {
                SetItems(this, EventArgs.Empty);
            }
        }


        #endregion
    }
}
