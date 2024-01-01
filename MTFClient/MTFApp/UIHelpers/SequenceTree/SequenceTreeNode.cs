using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MTFApp.UIHelpers.SequenceTree
{
    public class SequenceTreeNode : ListViewItem, INotifyPropertyChanged
    {
        private int index;
        private SequenceTree tree;
        private SequenceTreeNode parentNode;
        private ObservableCollection<SequenceTreeNode> children;


        public SequenceTreeNode(object content, SequenceTreeNode parent, SequenceTree tree)
        {
            this.ParentNode = parent;
            this.Content = content;
            this.tree = tree;
            this.children = new ObservableCollection<SequenceTreeNode>();
        }


        #region Properties

        public int Index
        {
            get { return index; }
        }

        public SequenceTreeNode ParentNode
        {
            get { return parentNode; }
            private set { parentNode = value; }
        }

        public bool HasChildren
        {
            get { return children.Count > 0; }
        }

        public bool IsExpandable
        {
            get
            {
                return HasChildren;
            }
        }

        public int Level
        {
            get
            {
                if (parentNode == null)
                {
                    return 0;
                }
                else
                {
                    return parentNode.Level + 1;
                }
            }
        }

        public ObservableCollection<SequenceTreeNode> Children
        {
            get { return children; }
        }

        public bool IsVisible
        {
            get
            {
                var node = parentNode;
                while (node!=null)
                {
                    if (node.IsCollapsed)
                    {
                        return false;
                    }
                    node = node.ParentNode;
                }
                return true;
            }
        }
        #endregion


        #region IsCollapsed dependency property


        public bool IsCollapsed
        {
            get { return (bool)GetValue(IsCollapsedProperty); }
            set { SetValue(IsCollapsedProperty, value); }
        }

        public static readonly DependencyProperty IsCollapsedProperty =
            DependencyProperty.Register("IsCollapsed", typeof(bool), typeof(SequenceTreeNode),
            new FrameworkPropertyMetadata() { BindsTwoWayByDefault = true, PropertyChangedCallback = IsCollapsedChanged });

        private static void IsCollapsedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as SequenceTreeNode).tree.RefreshVisibleRows(d as SequenceTreeNode);
        }


        #endregion


        
        public void AddChild(SequenceTreeNode child)
        {
            if (child!=null)
            {
                children.Add(child);
            }
        }









        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName]string name = null)
        {
            if (!string.IsNullOrEmpty(name))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
