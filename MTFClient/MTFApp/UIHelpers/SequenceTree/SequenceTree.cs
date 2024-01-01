using MTFClientServerCommon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace MTFApp.UIHelpers.SequenceTree
{
    public class SequenceTree : ListView
    {
        private ObservableCollection<SequenceTreeNode> visibleRows = new ObservableCollection<SequenceTreeNode>();

        public SequenceTree()
        {
            //this.VisibleRows = new ObservableCollection<SequenceTreeNode>();
            this.ItemsSource = this.visibleRows;
        }

        #region InputCollection dependency property


        public IList<MTFSequenceActivity> InputCollection
        {
            get { return (IList<MTFSequenceActivity>)GetValue(InputCollectionProperty); }
            set { SetValue(InputCollectionProperty, value); }
        }

        public static readonly DependencyProperty InputCollectionProperty =
            DependencyProperty.Register("InputCollection", typeof(IList<MTFSequenceActivity>), typeof(SequenceTree),
            new FrameworkPropertyMetadata() { BindsTwoWayByDefault = false, PropertyChangedCallback = InputCollectionChanged });

        private static void InputCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = d as SequenceTree;
            source.GenerateVisibleRows(e.NewValue as IList<MTFSequenceActivity>, null);
        }

        private void GenerateVisibleRows(IList<MTFSequenceActivity> activities, SequenceTreeNode parent)
        {
            if (activities != null)
            {
                foreach (var item in activities)
                {
                    var node = new SequenceTreeNode(item, parent, this);
                    if (parent != null)
                    {
                        parent.AddChild(node);
                    }
                    visibleRows.Add(node);
                    if (item is MTFSubSequenceActivity && !(item as MTFSubSequenceActivity).IsCollapsed)
                    {
                        Binding b = new Binding("IsCollapsed");
                        b.Source = item;
                        b.Mode = BindingMode.TwoWay;
                        BindingOperations.SetBinding(node, SequenceTreeNode.IsCollapsedProperty, b);
                        GenerateVisibleRows((item as MTFSubSequenceActivity).Activities, node);
                    }
                }
            }
        }

        public void RefreshVisibleRows(SequenceTreeNode node)
        {
            if (node.IsCollapsed)
            {
                HideChildren(node);
            }
            else
            {
                int index = visibleRows.IndexOf(node);
                ShowChildren(ref index, node);
            }
        }

        private void ShowChildren(ref int index, SequenceTreeNode node)
        {
            foreach (var child in node.Children)
            {
                visibleRows.Insert(++index, child);
                if (child.HasChildren && !child.IsCollapsed)
                {
                    ShowChildren(ref index, child);
                }
            }
        }

        private void HideChildren(SequenceTreeNode node)
        {
            int index = visibleRows.IndexOf(node);
            int currentLevel = node.Level;
            if (index != -1 && index < visibleRows.Count - 1)
            {
                var nextNode = visibleRows[index + 1];
                while (nextNode.Level > currentLevel)
                {
                    visibleRows.RemoveAt(index + 1);
                    nextNode = visibleRows[index + 1];
                }
            }
        }


        #endregion
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return base.IsItemItsOwnContainerOverride(item);
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            var x = base.GetContainerForItemOverride();
            return x;
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            if ((Keyboard.Modifiers == ModifierKeys.Control || Keyboard.Modifiers == ModifierKeys.Shift) && this.SelectedItems.Count > 1 && e.AddedItems.Count > 0)
            {
                var firstLevel = (this.SelectedItems[0] as SequenceTreeNode).Level;
                int newLevel;
                if (ContaintsDifferentLevel(this.SelectedItems, firstLevel, out newLevel))
                {
                    var newSelectedItem = GetSelectedItem(e.AddedItems, newLevel);
                    this.SelectedItem = newSelectedItem;
                }
            }
            base.OnSelectionChanged(e);
        }

        private object GetSelectedItem(IList list, int newLevel)
        {
            foreach (SequenceTreeNode item in list)
            {
                if (item!=null && item.Level == newLevel)
                {
                    return item;
                }
            }
            return null;
        }


        private bool ContaintsDifferentLevel(IEnumerable collection, int level, out int differentLevel)
        {
            differentLevel = -1;
            foreach (SequenceTreeNode item in collection)
            {
                if (item.Level != level)
                {
                    differentLevel = item.Level;
                    return true;
                }
            }
            return false;
        }

        //public ObservableCollection<SequenceTreeNode> VisibleRows
        //{
        //    get;
        //    private set;
        //}



    }

}
