using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MTFApp.UIHelpers.DragAndDrop
{
    public class DropInfo
    {
        public DropInfo(object sender, DragEventArgs e)
        {
            const double edgeHeight = 7;

            TargetCollection = sender as UIElement;

            if (sender is ItemsControl)
            {
                ItemsControl itemsControl = (ItemsControl)sender;
                UIElement item = itemsControl.GetItemContainerAt(e.GetPosition(itemsControl));
                if (item != null)
                {
                    ItemsControl itemParent = ItemsControl.ItemsControlFromItemContainer(item);
                    TargetItem = (item as ListBoxItem).Content;
                    InsertIndex = itemParent.ItemContainerGenerator.IndexFromContainer(item);
                    double yPosition = e.GetPosition(item).Y;
                    double itemHeight = item.RenderSize.Height;
                    if (yPosition > itemHeight / 2) InsertIndex++;
                    //TargetOnItem = (yPosition > itemHeight / 4 & yPosition < itemHeight * 3 / 4) ? true : false;
                    TargetOnItem = (yPosition > edgeHeight & yPosition < itemHeight-edgeHeight) ? true : false;
                }
                else
                {
                    InsertIndex = itemsControl.Items.Count;
                }
            }
        }
        public object TargetItem { get; private set; }
        public bool TargetOnItem { get; private set; }
        public UIElement TargetCollection { get; private set; }
        public DragDropEffects Effects { get; set; }
        public Type DropTargetAdorner { get; set; }
        public int InsertIndex { get; private set; }
    }
}
