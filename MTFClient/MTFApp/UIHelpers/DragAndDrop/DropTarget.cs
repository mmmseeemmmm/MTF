using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MTFApp.UIHelpers.DragAndDrop
{
    public class DropTarget
    {
        public void DragOver(DropInfo dropInfo, bool ctrlPress)
        {
            //if (dropInfo.TargetItem is MTFClientServerCommon.MTFSubSequenceActivity)
            //{
            //    //dropInfo.Effects = DragDropEffects.None;
            //}
            //else
            //{
                dropInfo.Effects = ctrlPress ? DragDropEffects.Copy : DragDropEffects.Move;
            //}
            dropInfo.DropTargetAdorner = Insert;
        }

        
        public static Type Insert
        {
            get { return typeof(DropTargetInsertionAdorner); }
        }

    }
}
