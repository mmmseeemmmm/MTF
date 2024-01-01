using MTFApp.UIHelpers;
using MTFApp.UIHelpers.DragAndDrop;
using MTFClientServerCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using MTFApp.MergeActivities;
using MTFApp.MTFWizard;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.Helpers;
using MTFClientServerCommon.Mathematics;
using MTFClientServerCommon.SequenceLocalization;


namespace MTFApp.SequenceEditor
{
    /// <summary>
    /// Interaction logic for SequenceEditorControl.xaml
    /// </summary>
    public partial class SequenceEditorControl : MTFUserControl
    {
        private void ListBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var b = (sender as ListBox).GetBindingExpression(ListBox.SelectedItemProperty);
            if (b != null)
            {
                b.UpdateSource();
            }
        }

        private readonly List<ListBox> selectedListBoxesTemp = new List<ListBox>();
        private readonly List<ListBox> movedListBoxes = new List<ListBox>();
        private Point? startPointLeftPanel;
        private object selectedLeftPanelItem;
        private SequenceEditorPresenter presenter;
        private bool allowDropCallList;
        private bool pasteWithKeyboard;
        private enum DragSource { None, MainListBox, CallListBox };
        private DragSource dragSource;
        private readonly DragAndDrop dragAndDrop;
        SettingsClass setting = StoreSettings.GetInstance.SettingsClass;
        private TouchHelper touch = TouchHelper.Instance;

        public SequenceEditorControl()
        {
            InitializeComponent();
            dragAndDrop = DragAndDrop.Instance;
        }

        #region UI helpers

        private void ScrollParentListBox(object sender, MouseWheelEventArgs e)
        {
            UIHelper.RaiseScrollEvent(sender, e);
        }

        private void AddButtonInLeftPanel_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                var menu = button.ContextMenu;
                if (menu != null)
                {
                    menu.PlacementTarget = button;
                    menu.IsOpen = true;
                }
            }
        }

        public object TargetForPopup
        {
            get
            {
                return TabControlSequence;
            }
        }

        private void PasteFromContextMenuOnListBox(object sender)
        {
            var menuItem = sender as MenuItem;
            if (menuItem != null && MTFClipboard.ContainsData<List<MTFSequenceActivity>>())
            {
                var parentMenu = menuItem.Parent as ContextMenu;
                if (parentMenu != null)
                {
                    var listBox = parentMenu.PlacementTarget as ListBox;
                    if (listBox != null)
                    {
                        var newData = MTFClipboard.GetData() as List<MTFSequenceActivity>;
                        if (newData != null)
                        {
                            IList<MTFSequenceActivity> targetCollection = null;
                            var targetIndex = 0;
                            switch (listBox.Name)
                            {
                                case "MainListBox":
                                    targetCollection = presenter.Sequence.MTFSequenceActivities;
                                    targetIndex = targetCollection.Count;
                                    break;
                                case "CallListBox":
                                    targetCollection = presenter.Sequence.ActivitiesByCall;
                                    targetIndex = targetCollection.Count;
                                    break;
                            }

                            CreateCopyOrMerge(targetCollection, targetIndex, null, false, newData);
                        }
                    }
                }
            }
        }

        private void MenuItem_PasteClick(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem != null && MTFClipboard.ContainsData<List<MTFSequenceActivity>>())
            {
                int index = -1;
                var parentMenu = menuItem.Parent as ContextMenu;
                if (parentMenu != null)
                {
                    MTFSequenceActivity targetActivity = null;
                    var listBoxItem = parentMenu.PlacementTarget as ListBoxItem;
                    if (listBoxItem != null)
                    {
                        targetActivity = listBoxItem.Content as MTFSequenceActivity;
                    }
                    else
                    {
                        var listBox = parentMenu.PlacementTarget as ListBox;
                        if (listBox != null)
                        {
                            targetActivity = listBox.SelectedItem as MTFSequenceActivity;
                            if (targetActivity != null)
                            {
                                index = listBox.Items.IndexOf(targetActivity);
                            }
                        }
                    }

                    var newData = MTFClipboard.GetData() as List<MTFSequenceActivity>;
                    if (newData != null)
                    {
                        var targetIsSubsequence = targetActivity is MTFSubSequenceActivity;
                        if (!targetIsSubsequence)
                        {
                            if (index == -1 && targetActivity != null)
                            {
                                var collection = GetCollectionByActivity(targetActivity);
                                if (collection != null)
                                {
                                    index = collection.IndexOf(targetActivity) + 1;
                                }
                            }
                        }
                        CreateCopyOrMerge(null, index, targetActivity, targetIsSubsequence, newData);
                    }
                }
            }
        }

        private void CreateCopyOrMerge(IList<MTFSequenceActivity> collection, int targetIndex, object targetItem,
            bool targetOnItem, List<MTFSequenceActivity> activities)
        {
            if (MTFClipboard.IsSameParent(presenter.Sequence))
            {
                CreateAndInsertCopy(collection, targetIndex, targetItem, targetOnItem, activities);
            }
            else
            {
                var mergedActivities = MergeActivities(activities, presenter.Sequence);
                InsertActivities(collection, targetIndex, targetItem, targetOnItem, mergedActivities);
            }
        }


        public ICommand RemoveActivityCommand
        {
            get { return new Command(RemoveActivityByButton); }
        }

        private void RemoveActivityByButton(object param)
        {
            DeleteSelectedItem(param as MTFSequenceActivity);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                presenter.PerformSave(presenter.Sequence, false);
                e.Handled = true;
            }
            //if (e.Key == Key.A && Keyboard.Modifiers == ModifierKeys.Control)
            //{
            //    //TODO CTRL+A
            //    AddGroupOfSelectedActivities(presenter.Sequence.MTFSequenceActivities.ToList());
            //}
        }

        private void ComboBox_selectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Fix due to a binding error
            //When the sequence is changed, LastSelectedItem is set to Activity's detailTemplate contains some ComboBox,
            //ItemsSource of this Combobox is changed too and SelectedValue raises binding of null value to source property
            //therefore UpdateSourceTrigger of binding SelectedValue is set to value Explicit and binding to source value is raised here.
            var combo = sender as ComboBox;
            if (combo != null && combo.Items.Count > 0 && combo.SelectedValue != null)
            {
                BindingExpression be = combo.GetBindingExpression(ComboBox.SelectedValueProperty);
                if (be != null)
                {
                    be.UpdateSource();
                }
            }
        }


        #endregion

        #region Helper methods

        public ICommand CopyFromContextMenu
        {
            get
            {
                return new Command(SaveToClipBoard);
            }
        }

        public ICommand PasteFromContextMenu
        {
            get
            {
                return new Command(param => MenuItem_PasteClick(param, null));
            }
        }

        public ICommand PasteFromContextMenuOnListBoxCommand
        {
            get { return new Command(PasteFromContextMenuOnListBox); }
        }

        public ICommand DeleteFromContextMenu
        {
            get
            {
                return new Command(DeleteSelectedItems);
            }
        }

        public void SetDataContext()
        {
            presenter = DataContext as SequenceEditorPresenter;
        }

        private int GetTargetIndex(ListBox listBox, DragEventArgs e)
        {
            if (dropTargetAdorner != null)
            {
                return dropTargetAdorner.DropInfo.InsertIndex;
            }
            MoveAdorner(listBox, e);
            return GetTargetIndex(listBox, e);
        }

        private void DeleteSelectedItems()
        {
            int count = presenter.SelectionHelper.Count;
            if (count > 0)
            {
                string text = count > 1 ? string.Format(LanguageHelper.GetString("Msg_Body_DeleteMoreAct"), count) :
                    string.Format(LanguageHelper.GetString("Msg_Body_DeleteAct"), presenter.SelectionHelper.FirstActivity().TranslateActivityName());

                var messageInfo = new MessageInfo { Text = text, Type = SequenceMessageType.Question, Buttons = MessageButtons.OkCancel };
                var msg = new MessageBoxControl.MessageBoxControl(messageInfo);
                PopupWindow.PopupWindow pw = new PopupWindow.PopupWindow(msg)
                                             {
                                                 CanClose = false,
                                                 Title = LanguageHelper.GetString("Msg_Header_DeleteAct")
                };
                pw.ShowDialog();
                if (msg.DialogResult.Result == MTFDialogResultEnum.Ok)
                {
                    presenter.SelectionHelper.PerformActionOnEachActivity(RemoveActivity);
                    presenter.SelectionHelper.Clear(true);
                }
            }
        }

        private void DeleteSelectedItem(MTFSequenceActivity activity)
        {
            if (activity != null)
            {
                var messageInfo = new MessageInfo
                {
                    Text = string.Format(LanguageHelper.GetString("Msg_Body_DeleteAct"), activity.TranslateActivityName()),
                    Type = SequenceMessageType.Question,
                    Buttons = MessageButtons.OkCancel
                };
                var msg = new MessageBoxControl.MessageBoxControl(messageInfo);
                PopupWindow.PopupWindow pw = new PopupWindow.PopupWindow(msg)
                {
                    CanClose = false,
                    Title = LanguageHelper.GetString("Msg_Header_DeleteAct")
                };
                pw.ShowDialog();
                if (msg.DialogResult.Result == MTFDialogResultEnum.Ok)
                {
                    RemoveActivity(activity);
                    presenter.SelectionHelper.Clear(true);
                }
            }
        }


        private void SaveToClipBoard()
        {

            List<MTFSequenceActivity> dataToClipboard = new List<MTFSequenceActivity>();
            presenter.SelectionHelper.PerformActionOnEachActivity(a => dataToClipboard.Add(a));
            MTFClipboard.SetData(dataToClipboard, presenter.Sequence);
            presenter.ClipboardIsChanged();
        }

        private void PasteFromClipboardByKeyboard(ListBox listBox)
        {
            var data = MTFClipboard.GetData<List<MTFSequenceActivity>>();
            if (data == null)
            {
                return;
            }

            IList<MTFSequenceActivity> targetCollection = null;
            int targetIndex = 0;
            MTFSequenceActivity targetItem = null;
            bool targetOnItem = false;

            if (presenter.SelectionHelper.Count > 0)
            {
                targetItem = presenter.SelectionHelper.LastActivity();
                var collection = GetCollectionByActivity(targetItem);
                if (collection != null)
                {
                    int index = collection.IndexOf(targetItem);
                    if (index == -1)
                    {
                        index = collection.Count - 1;
                    }
                    targetIndex = index + 1;
                    targetOnItem = targetItem is MTFSubSequenceActivity;
                }
            }
            else
            {
                switch (listBox.Name)
                {
                    case "MainListBox":
                        targetCollection = presenter.Sequence.MTFSequenceActivities;
                        targetIndex = targetCollection.Count;
                        break;
                    case "CallListBox":
                        targetCollection = presenter.Sequence.ActivitiesByCall;
                        targetIndex = targetCollection.Count;
                        break;
                }
            }

            CreateCopyOrMerge(targetCollection, targetIndex, targetItem, targetOnItem, data);
        }

        private List<MTFSequenceActivity> MergeActivities(List<MTFSequenceActivity> activities, MTFSequence sequence)
        {
            var copy = CreateCopyofActivities(activities);
            var mergedData = new MergeSharedData(copy);
            var controls = new List<MTFWizardUserControl>
                           {
                               new MergeComponents(sequence.MTFSequenceClassInfos, mergedData),
                               new MergeVariables(sequence.MTFVariables, mergedData),
                               new MergeActivitiesSumary(sequence, mergedData)
                           };
            var dialog = new MTFWizardWindow(controls) { Owner = Application.Current.MainWindow };
            dialog.ShowDialog();
            return dialog.Result == true ? copy : null;
        }

        private void ClearMovedListBoxes()
        {
            if (movedListBoxes.Count > 0)
            {
                foreach (var item in movedListBoxes)
                {
                    item.AllowDrop = true;
                    item.Opacity = 1;
                }
            }
        }

        private void executionType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combo = sender as ComboBox;
            if (combo.SelectedValue is ExecutionType)
            {
                var selectedItem = (ExecutionType)combo.SelectedValue;
                if (selectedItem == ExecutionType.ExecuteAlways || selectedItem == ExecutionType.ExecuteByCall || selectedItem == ExecutionType.ExecuteInParallel)
                {
                    var subSequence = combo.DataContext as MTFSubSequenceActivity;
                    if (subSequence != null)
                    {
                        if (!(subSequence.Term is EmptyTerm))
                        {
                            subSequence.Term = new EmptyTerm("System.Boolean");
                        }

                    }
                }
                if (e.AddedItems.Count > 0 && e.RemovedItems.Count > 0)
                {
                    var addItem = (ExecutionType)(e.AddedItems[0] as EnumValueDescription).Value;
                    var removeItem = (ExecutionType)(e.RemovedItems[0] as EnumValueDescription).Value;
                    if (addItem == ExecutionType.ExecuteByCall)
                    {
                        presenter.MoveToCallActivities(presenter.LastSelectedItem as MTFSubSequenceActivity);
                    }
                    else if (removeItem == ExecutionType.ExecuteByCall)
                    {
                        presenter.MoveFromCallActivities(presenter.LastSelectedItem as MTFSubSequenceActivity);
                    }
                    e.Handled = true;
                }
            }
        }

        #endregion

        #region Drag&Drop Adorner
        static DropTargetAdorner dropTargetAdorner;
        static DropTargetAdorner DropTargetAdorner
        {
            get { return dropTargetAdorner; }
            set
            {
                if (dropTargetAdorner != null)
                {
                    dropTargetAdorner.Detatch();
                }
                dropTargetAdorner = value;
            }
        }

        static DropTarget defaultDropHandler;
        public static DropTarget DefaultDropHandler
        {
            get
            {
                if (defaultDropHandler == null)
                {
                    defaultDropHandler = new DropTarget();
                }
                return defaultDropHandler;
            }
            set
            {
                defaultDropHandler = value;
            }
        }

        private void MoveAdorner(object sender, DragEventArgs e)
        {
            DropInfo dropInfo = new DropInfo(sender, e);
            DefaultDropHandler.DragOver(dropInfo, Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl));

            // Update the drag adorner.

            //If the target is an ItemsControl then update the drop target adorner.
            if (sender is ItemsControl)
            {
                UIElement adornedElement = ((ItemsControl)sender).GetVisualDescendent<ItemsPresenter>();
                if (dropInfo.DropTargetAdorner == null)
                {
                    DropTargetAdorner = null;
                }
                else if (!dropInfo.DropTargetAdorner.IsInstanceOfType(DropTargetAdorner))
                {
                    DropTargetAdorner = DropTargetAdorner.Create(dropInfo.DropTargetAdorner, adornedElement);
                }

                if (DropTargetAdorner != null)
                {

                    DropTargetAdorner.DropInfo = dropInfo;
                    DropTargetAdorner.InvalidateVisual();
                }
            }

            e.Effects = dropInfo.Effects;
            e.Effects = DragDropEffects.None;
            if (dropInfo.TargetItem != null &&
                dropInfo.TargetItem.GetType() == typeof(MTFSubSequenceActivity) &&
                dropInfo.TargetOnItem)
            {

                dropTargetAdorner.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                dropTargetAdorner.Visibility = System.Windows.Visibility.Visible;
            }


            //e.Handled = true;
        }

        private void DisposeAdorner()
        {
            if (dropTargetAdorner != null)
            {
                dropTargetAdorner.Detatch();
                dropTargetAdorner = null;
            }
        }
        #endregion

        #region Drag&Drop helpers



        private void Component_Drop(object sender, DragEventArgs e)
        {
            var fe = sender as FrameworkElement;
            if (fe != null)
            {
                var componentFromMainSequence = fe.Tag as MTFSequenceClassInfo;
                if (componentFromMainSequence != null && e.Data.GetDataPresent(DragAndDropTypes.SubComponent))
                {
                    var data = e.Data.GetData(DragAndDropTypes.SubComponent) as MTFSequenceClassInfo;
                    if (data != null)
                    {
                        presenter.SetMapping(componentFromMainSequence, data);
                    }
                }

            }

        }

        private void Component_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DragAndDropTypes.SubComponent))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        private void SubComponentsListBox_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DragAndDropTypes.SubComponent))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        private void SubComponentsListBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DragAndDropTypes.SubComponent))
            {
                var data = e.Data.GetData(DragAndDropTypes.SubComponent) as MTFSequenceClassInfo;
                if (data != null)
                {
                    presenter.RemoveMapping(data);
                }
            }
        }



        private bool MoveSelection(ListBox listBox, int targetIndex, object targetItem, bool targetOnItem, object dragData)
        {
            bool result = false;
            var itemsToMove = dragData as List<MTFSequenceActivity>;
            var subSequence = targetItem as MTFSubSequenceActivity;
            if (subSequence != null && targetOnItem)
            {
                var collection = presenter.GetCollectionFromSubsequence(subSequence);
                if (collection != null && itemsToMove != null && !itemsToMove.Contains(targetItem))
                {
                    if (subSequence.IsCollapsed)
                    {
                        subSequence.IsCollapsed = false;
                    }
                    int index = 0;
                    while (index < itemsToMove.Count)
                    {
                        var tmpActivity = itemsToMove[index];
                        RemoveActivity(itemsToMove[index]);
                        try
                        {
                            collection.Add(itemsToMove[index]);
                        }
                        catch (Exception ex)
                        {
                            presenter.Sequence.MTFSequenceActivities.Add(tmpActivity);
                            ShowChangePositionErrorMessage(tmpActivity.ActivityName, ex);
                        }
                        itemsToMove[index].AdjustName();
                        index++;
                    }
                    result = true;
                }
            }
            else
            {
                ChangePosition(listBox, targetIndex, itemsToMove);
                result = true;
            }
            if (itemsToMove != null)
            {
                presenter.LastSelectedItem = itemsToMove.LastOrDefault();
            }
            return result;
        }

        private void ShowChangePositionErrorMessage(string activityName, Exception ex)
        {
            SystemLog.LogException(ex);
            MTFMessageBox.Show("Sequence editor",
                                string.Format("Change position of activity {0} failed. This activity has been moved at the end of the sequence.{1}{1}Please send SystemLog from MTFClient!",
                                activityName, Environment.NewLine),
                                MTFMessageBoxType.Warning, MTFMessageBoxButtons.Ok);
        }

        private void ChangePosition(ListBox listBox, int targetIndex, List<MTFSequenceActivity> dragData)
        {
            int firstIndex = listBox.Items.IndexOf(dragData.First());
            int lastIndex = listBox.Items.IndexOf(dragData.Last());
            MTFSequenceActivity firtActivity = null;
            if (listBox.Items.Count > 0)
            {
                int i = 0;
                do
                {
                    if (i >= listBox.Items.Count)
                    {
                        return;
                    }
                    firtActivity = listBox.Items[i++] as MTFSequenceActivity;
                } while (dragData.Contains(firtActivity));

            }
            bool up = lastIndex > targetIndex;
            if (targetIndex >= firstIndex && targetIndex <= lastIndex)
            {
                return;
            }
            if (firstIndex != -1 && !up)
            {
                targetIndex--;
            }
            int index = 0;
            if (firstIndex == -1 && listBox.Name == "CallListBox")
            {
                while (index < dragData.Count)
                {
                    var tmpActivity = dragData[index];
                    RemoveActivity(dragData[index]);
                    try
                    {
                        if (targetIndex >= 0 && targetIndex < presenter.Sequence.ActivitiesByCall.Count)
                        {
                            presenter.Sequence.ActivitiesByCall.Insert(targetIndex, dragData[index]);
                        }
                        else
                        {
                            presenter.Sequence.ActivitiesByCall.Add(dragData[index]);
                        }
                    }
                    catch (Exception ex)
                    {
                        presenter.Sequence.MTFSequenceActivities.Add(tmpActivity);
                        ShowChangePositionErrorMessage(tmpActivity.ActivityName, ex);
                    }
                    dragData[index].AdjustName();
                    index++;
                    targetIndex++;
                }
            }
            else
            {
                while (index < dragData.Count)
                {
                    var tmpActivity = dragData[index];
                    RemoveActivity(dragData[index]);
                    try
                    {
                        InsertActivity(firtActivity, null, targetIndex, dragData[index], true);
                    }
                    catch (Exception ex)
                    {
                        presenter.Sequence.MTFSequenceActivities.Add(tmpActivity);
                        ShowChangePositionErrorMessage(tmpActivity.ActivityName, ex);
                    }
                    if (firstIndex == -1 || up)
                    {
                        targetIndex++;
                    }
                    index++;
                }
            }
        }

        private void RemoveActivity(object activity)
        {
            presenter.RemoveActivityFromSequence.Execute(activity);
        }

        private IList<MTFSequenceActivity> GetCollectionByActivity(MTFSequenceActivity activity)
        {
            var parentCollection = presenter.Activities;
            var collection = presenter.FindCollectionByParent(activity);
            if (collection == null)
            {
                collection = presenter.FindCollectionByActivity(
                parentCollection, activity);
            }
            if (collection == null)
            {
                parentCollection = presenter.Sequence.ActivitiesByCall;
                collection = presenter.FindCollectionByActivity(
                parentCollection, activity);
            }
            return collection;
        }

        private void InsertActivity(MTFSequenceActivity firstActivityInTargetCollection, IList<MTFSequenceActivity> collection, int indexInCollection,
            MTFSequenceActivity insertedActivity, bool setSelectedItem)
        {
            int index = indexInCollection;
            if (firstActivityInTargetCollection == null)
            {
                if (collection == null)
                {
                    collection = presenter.Activities;
                }
            }
            else
            {
                index = indexInCollection;
                collection = GetCollectionByActivity(firstActivityInTargetCollection);
                if (index == -1)
                {
                    index = collection.IndexOf(firstActivityInTargetCollection);
                }
            }
            if (collection != null)
            {
                if (ReferenceEquals(collection, presenter.Sequence.ActivitiesByCall) && !(insertedActivity is MTFSubSequenceActivity))
                {
                    if (pasteWithKeyboard)
                    {
                        MTFMessageBox.Show("MTF Warning",
                            "You cannot place activities to this area. Please place these activities into any SubSequence or use the top area.",
                            MTFMessageBoxType.Warning,
                            MTFMessageBoxButtons.Ok);
                        return;
                    }
                    throw new Exception("Activity can not be inserted between call subSequences.");
                }
                presenter.InsertActivityIntoCollection(collection, index, insertedActivity, setSelectedItem);
            }
            else
            {
                presenter.InsertActivityIntoCollection(presenter.Activities, insertedActivity, setSelectedItem);
            }
        }

        private void DragNewItemFromLeftPanel(ListBox sourceListBox, MouseEventArgs e, string dataType)
        {
            if (selectedLeftPanelItem == null || startPointLeftPanel == null || !setting.AllowDragDrop)
            {
                return;
            }
            Point mousePos = e.GetPosition(sourceListBox);
            Vector diff = startPointLeftPanel.Value - mousePos;
            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                // Get the dragged ListViewItem
                object data = selectedLeftPanelItem;
                if (data == null)
                {
                    return;
                }
                if (data is MTFVariable variable && variable.HasDataTable)
                {
                    return;
                }

                if (data is EnumValueDescription buildInCmd)
                {
                    var enumValue = (SequenceBaseCommands)buildInCmd.Value;
                    data = enumValue;
                    allowDropCallList = enumValue == SequenceBaseCommands.CreateSubSequence;
                }
                // Initialize the drag & drop operation
                DataObject dragData = new DataObject(dataType, data);
                DragDrop.DoDragDrop(sourceListBox, dragData, DragDropEffects.Move);
                DisposeAdorner();
                dragSource = DragSource.None;
                allowDropCallList = false;
                selectedListBoxesTemp.Clear();
                ReleaseLeftPanelDragHelpers();
                e.Handled = true;
            }
        }

        private void CreateAndInsertCopy(IList<MTFSequenceActivity> collection, int targetIndex, object targetItem,
            bool targetOnItem, List<MTFSequenceActivity> activities)
        {
            InsertActivities(collection, targetIndex, targetItem, targetOnItem, CreateCopyofActivities(activities));
        }

        private List<MTFSequenceActivity> CreateCopyofActivities(List<MTFSequenceActivity> activities)
        {
            if (activities != null)
            {
                List<MTFSequenceActivity> copiedData = new List<MTFSequenceActivity>();
                activities.ForEach(activitity =>
                {
                    var newItem = activitity.Clone() as MTFSequenceActivity;
                    if (newItem != null)
                    {
                        newItem.ReplaceIdentityObjectsNoCache(activitity);
                        copiedData.Add(newItem);
                    }
                });
                return copiedData;
            }
            return null;
        }


        private void InsertActivities(IList<MTFSequenceActivity> collection, int targetIndex, object targetItem,
            bool targetOnItem, List<MTFSequenceActivity> activities)
        {
            if (activities != null)
            {
                var activity = targetItem as MTFSubSequenceActivity;
                if (activity != null && targetOnItem && !pasteWithKeyboard)
                {
                    var item = activity;
                    var subSequenceCollection = presenter.GetCollectionFromSubsequence(item);
                    if (item.IsCollapsed)
                    {
                        item.IsCollapsed = false;
                    }
                    activities.ForEach(x => { InsertActivity(null, subSequenceCollection, subSequenceCollection.Count, x, false); });
                }
                else
                {
                    if (targetItem == null)
                    {
                        activities.ForEach(x => InsertActivity(null, collection, targetIndex++, x, false));
                    }
                    else
                    {
                        activities.ForEach(x => InsertActivity(targetItem as MTFSequenceActivity, collection, targetIndex++, x, false));
                    }
                }
                presenter.SelectionHelper.AddMore(activities);
            }
        }

        #endregion

        #region Commands as Activities
        private void CommandListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                EnumValueDescription selectedItem = (EnumValueDescription)((ListBox)sender).SelectedItem;
                var collection = presenter.Sequence.MTFSequenceActivities;
                var firstItem = collection.Count > 0 ? collection.First() : null;
                presenter.InsertNewCommandAsActivity((SequenceBaseCommands)selectedItem.Value, firstItem, collection.Count, null, false, false);
                presenter.SelectionHelper.Add(presenter.LastSelectedItem as MTFSequenceActivity);
            }
        }

        private void AssignLeftPanelDragHelpers(MouseButtonEventArgs e, object sender)
        {
            selectedLeftPanelItem = (e.MouseDevice.DirectlyOver as FrameworkElement)?.DataContext;
            startPointLeftPanel = e.GetPosition(sender as IInputElement);
        }

        private void ReleaseLeftPanelDragHelpers()
        {
            selectedLeftPanelItem = null;
            startPointLeftPanel = null;
        }

        private void CommandsListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AssignLeftPanelDragHelpers(e, sender);
            presenter.SelectionHelper.ClearAndInvalidate();
        }

        private void CommandsListBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dragAndDrop.DisableDragAndDrop();
            ReleaseLeftPanelDragHelpers();
        }

        private void CommandsListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            DragNewItemFromLeftPanel(sender as ListBox, e, DragAndDropTypes.NewCommand);
        }

        private void InsertNewCommandAsActivity(object data, int targetIndex, object targetItem, bool targetOnItem, ListBox listBox)
        {
            var firstItem = listBox.Items.Count > 0 ? listBox.Items[0] : null;
            presenter.InsertNewCommandAsActivity((SequenceBaseCommands)data, firstItem as MTFSequenceActivity, targetIndex, targetItem, targetOnItem, listBox.Name == "CallListBox");
        }
        #endregion

        #region Variables as Activities
        private void VariableListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                presenter.RemoveVariableFromSequence.Execute(((ListBox)sender).SelectedItem);
            }
            if (e.Key == Key.Enter)
            {
                var variable = ((ListBox)sender).SelectedItem as MTFVariable;
                if (variable == null || variable.HasDataTable)
                {
                    return;
                }
                presenter.AdoptVariableToSequenceActivity(((ListBox)sender).SelectedItem, -1, null, false);
                presenter.SelectionHelper.Add(presenter.LastSelectedItem as MTFSequenceActivity);
            }
        }

        private void VariableListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AssignLeftPanelDragHelpers(e, sender);
            presenter.SelectionHelper.ClearAndInvalidate();
            presenter.LastSelectedItem = UIHelper.GetObjectDataFromPoint(sender as ListBox, e.GetPosition(sender as ListBox));
        }

        private void VariableListBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dragAndDrop.DisableDragAndDrop();
            ReleaseLeftPanelDragHelpers();
        }

        private void VariableListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.OriginalSource.GetType() == typeof(System.Windows.Controls.Primitives.ScrollBar)
                || e.OriginalSource.GetType() == typeof(System.Windows.Controls.Primitives.Thumb)
                || e.OriginalSource.GetType() == typeof(System.Windows.Controls.Primitives.RepeatButton))
            {
                ReleaseLeftPanelDragHelpers();
                return;
            }
            
            DragNewItemFromLeftPanel(sender as ListBox, e, DragAndDropTypes.NewVariable);
        }


        private void VariableListBox_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            AssignLeftPanelDragHelpers(e, null);
            presenter.SelectionHelper.ClearAndInvalidate();
            var item = UIHelper.GetObjectDataFromPoint(sender as ListBox, e.GetPosition(sender as ListBox));
            if (item == null)
            {
                presenter.SelectedVariable = null;
            }
        }
        #endregion

        #region Components as Activities
        private void MTFSequenceClassInfoListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                presenter.RemoveClassFromSequence.Execute(((ListBox)sender).SelectedItem);
            }
            if (e.Key == Key.Enter)
            {
                var selectedItem = ((ListBox)sender).SelectedItem;
                if (selectedItem is SearchResultMethod || presenter.ComponentFromMainSequence(selectedItem as MTFSequenceClassInfo))
                {
                    presenter.AdoptComponentToSequenceActivity(((ListBox)sender).SelectedItem,
                        -1, null, false);
                    presenter.SelectionHelper.Add(presenter.LastSelectedItem as MTFSequenceActivity);
                }
            }
            e.Handled = true;
        }


        
        private void MTFSequenceClassInfoListBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            AssignLeftPanelDragHelpers(e, sender);
            presenter.SelectionHelper.ClearAndInvalidate();
            var item = UIHelper.GetObjectDataFromPoint(sender as ListBox, e.GetPosition(sender as ListBox));
            if (item != null)
            {
                presenter.SelectedSequenceClassInfo = item as MTFSequenceClassInfo;
            }
            selectedListBoxesTemp.Add(sender as ListBox);
        }

        private void MTFSequenceClassInfoListBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dragAndDrop.DisableDragAndDrop();
            ReleaseLeftPanelDragHelpers();
        }

        private void MTFSequenceClassInfoListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!ReferenceEquals(null, UIHelper.FindParent<System.Windows.Controls.Primitives.ScrollBar>((DependencyObject)e.OriginalSource)))
            {
                ReleaseLeftPanelDragHelpers();
                return;
            }
            if (startPointLeftPanel != null && selectedListBoxesTemp.Count > 0)
            {
                var sourceListBox = selectedListBoxesTemp.Last();
                DragNewItemFromLeftPanel(sourceListBox, e,
                    sourceListBox.Name == "SubComponentsListBox" ? DragAndDropTypes.SubComponent : DragAndDropTypes.SequenceClassInfo);
                e.Handled = false;
            }

        }

        #endregion

        private void ServiceCommandsListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            presenter.SelectionHelper.ClearAndInvalidate();
            presenter.LastSelectedItem = UIHelper.GetObjectDataFromPoint(sender as ListBox, e.GetPosition(sender as ListBox)); ;
        }

        private void CommandListBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                presenter.RemoveServiceCommandFromSequenceCommand.Execute(presenter.LastSelectedItem);
            }

        }

        private void UserCommandsListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            presenter.SelectionHelper.ClearAndInvalidate();
            presenter.LastSelectedItem = UIHelper.GetObjectDataFromPoint(sender as ListBox, e.GetPosition(sender as ListBox)); ;
        }

        private void UserListBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                presenter.RemoveUserCommandFromSequenceCommand.Execute(presenter.LastSelectedItem);
            }

        }

        //////////////////////////////NEW//////////////////////////////////////
        private int activityClickTimeStamp;

        private bool HandleTouch(MouseButtonEventArgs e)
        {
            var handled = false;

            var listBoxItem = UIHelper.FindParent<ListBoxItem>(e.MouseDevice.DirectlyOver as DependencyObject);
            if (listBoxItem != null)
            {
                var activity = listBoxItem.Content;
                var data = touch.DataObject;
                if (data != null)
                {
                    if (data.GetDataPresent(DragAndDropTypes.SetActivityResult))
                    {
                        if (data.GetData(DragAndDropTypes.SetActivityResult) is ActivityResultTerm term)
                        {
                            term.Value = activity as MTFSequenceActivity;
                            touch.Clear();
                            handled = true;
                        }
                    }
                    else if (data.GetDataPresent(DragAndDropTypes.SetActivity))
                    {
                        if (data.GetData(DragAndDropTypes.SetActivity) is MTFExecuteActivity term)
                        {
                            term.ActivityToCall = activity as MTFSubSequenceActivity;
                            touch.Clear();
                            handled = true;
                        }
                    }
                    else if (data.GetDataPresent(DragAndDropTypes.SetAction))
                    {
                        touch.InvokeAction(activity, presenter.LastSelectedItem);
                        touch.Clear();
                        handled = true;
                    }
                }
            }

            return handled;
        }

        private void ActivityListBoxPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (activityClickTimeStamp == e.Timestamp)
            {
                return;
            }
            activityClickTimeStamp = e.Timestamp;
            //touch.DisableEditorSelection = false;

            if (touch.DataObject != null)
            {
                touch.DisableEditorSelection = true;
                e.Handled = HandleTouch(e);
                return;
            }

            DisposeAdorner();
            if (!ReferenceEquals(null, UIHelper.FindParent<System.Windows.Controls.Primitives.ScrollBar>((DependencyObject)e.OriginalSource)))
            {
                return;
            }
            if (!ReferenceEquals(null, UIHelper.FindParent<ComboBoxItem>((DependencyObject)e.OriginalSource)))
            {
                return;
            }
            if (!ReferenceEquals(null, UIHelper.FindParent<TextBox>((DependencyObject)e.OriginalSource)))
            {
                return;
            }
            dragAndDrop.EnableDragAndDrop(e.GetPosition(sender as IInputElement));

            var listBoxItem = UIHelper.FindParent<ListBoxItem>(e.MouseDevice.DirectlyOver as DependencyObject);
            if (listBoxItem != null)
            {
                listBoxItem.Focus();
                var currentListBox = UIHelper.FindParent<ListBox>(listBoxItem);

                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    if (!presenter.SelectionHelper.IsSameSender(currentListBox))
                    {
                        presenter.SelectionHelper.ClearAndInvalidate();
                    }
                    presenter.SelectionHelper.AddWithCtrl(listBoxItem.Content as MTFSequenceActivity, currentListBox.Items.OfType<MTFSequenceActivity>().ToList());
                }
                else if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    if (!presenter.SelectionHelper.IsSameSender(currentListBox))
                    {
                        presenter.SelectionHelper.ClearAndInvalidate();
                    }
                    presenter.SelectionHelper.AddWithShift(currentListBox.Items, listBoxItem.Content as MTFSequenceActivity);
                }
                else
                {
                    var activity = listBoxItem.Content as MTFSequenceActivity;
                    if (activity != null)
                    {
                        presenter.SelectionHelper.Add(activity);
                    }
                }

                presenter.SelectionHelper.SetActualSender(currentListBox);
            }
            else
            {
                presenter.SelectionHelper.ClearAndInvalidate(true);
            }
            presenter.LastSelectedListBox = null;
            activityClickTimeStamp = e.Timestamp;
        }


        private void ActivityListBoxPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!dragAndDrop.IsEnableDragAndDrop || presenter.DisableDragDrop || !setting.AllowDragDrop)
            {
                return;
            }
            if (dragAndDrop.IsEnableDragAndDrop && e.LeftButton == MouseButtonState.Released)
            {
                dragAndDrop.DisableDragAndDrop();
                return;
            }
            if (Keyboard.IsKeyDown(Key.Escape))
            {
                dragAndDrop.DisableDragAndDrop();
                DisposeAdorner();
                return;
            }
            Point mousePos = e.GetPosition(sender as IInputElement);
            Vector diff = dragAndDrop.StartPoint - mousePos;
            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                if (presenter.SelectionHelper.Count > 0)
                {
                    if (!ReferenceEquals(null, UIHelper.FindParent<System.Windows.Controls.Primitives.ScrollBar>((DependencyObject)e.OriginalSource)))
                    {
                        dragAndDrop.DisableDragAndDrop();
                        return;
                    }
                    if (e.OriginalSource.GetType() == typeof(TextBox))
                    {
                        dragAndDrop.DisableDragAndDrop();
                        return;
                    }
                    DataObject dragData = new DataObject(DragAndDropTypes.MoveActivites, presenter.SelectionHelper.Selection);
                    presenter.SelectionHelper.PerformActionOnEachActivity(a =>
                                                                           {
                                                                               var subSequenceActivity = a as MTFSubSequenceActivity;
                                                                               if (subSequenceActivity != null)
                                                                               {
                                                                                   var subsequence = subSequenceActivity;
                                                                                   if (subsequence.Activities.Count > 0)
                                                                                   {
                                                                                       ListBox subSequenceListBox = UIHelper.FindListBoxByActivity(subsequence.Activities.First(), this);
                                                                                       if (subSequenceListBox != null)
                                                                                       {
                                                                                           movedListBoxes.Add(subSequenceListBox);
                                                                                           subSequenceListBox.Opacity = 0.5;
                                                                                           subSequenceListBox.AllowDrop = false;
                                                                                       }
                                                                                   }
                                                                               }
                                                                           });
                    allowDropCallList = presenter.SelectionHelper.AllIsSubsequences();
                    DragDrop.DoDragDrop((ListBox)sender, dragData, DragDropEffects.All);
                    dragSource = DragSource.None;
                    ClearMovedListBoxes();
                    DisposeAdorner();
                }
            }
        }

        private void ActivityListBoxOnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Delete:
                    {
                        DeleteSelectedItems();
                        e.Handled = true;
                        return;
                    }
                case Key.C:
                    {
                        if (Keyboard.Modifiers == ModifierKeys.Control)
                        {
                            SaveToClipBoard();
                            e.Handled = true;
                        }
                        return;
                    }
                case Key.V:
                    {
                        if (Keyboard.Modifiers == ModifierKeys.Control)
                        {
                            pasteWithKeyboard = true;
                            PasteFromClipboardByKeyboard(sender as ListBox);
                            pasteWithKeyboard = false;
                            e.Handled = true;
                        }
                        return;
                    }
                case Key.F12:
                    {
                        JumpToActivity(e);
                        return;
                    }
                case Key.F2:
                    {
                        var listBox = sender as ListBox;
                        if (listBox != null)
                        {
                            var listBoxItem = listBox.ItemContainerGenerator.ContainerFromItem(listBox.SelectedItem) as ListBoxItem;
                            if (listBoxItem != null)
                            {
                                var textBox = listBoxItem.GetVisualDescendent<TextBox>();
                                if (textBox != null)
                                {
                                    textBox.Focus();
                                    textBox.SelectAll();
                                }
                            }
                        }
                        e.Handled = true;
                        return;
                    }
            }
            e.Handled = false;
        }

        private void ActivityListBoxDrop(object sender, DragEventArgs e)
        {
            var listBox = (ListBox)sender;
            var targetPoint = e.GetPosition(listBox);
            var data = UIHelper.GetObjectDataFromPoint(sender as ListBox, targetPoint);
            var isCopy = dropTargetAdorner != null && (dropTargetAdorner.DropInfo.Effects == DragDropEffects.Copy);


            if (e.Data.GetDataPresent(DragAndDropTypes.GetActivityResult))
            {
                if (data != null)
                {
                    e.Data.SetData(DragAndDropTypes.GetActivityResult, data);
                    e.Data.SetData(DragAndDropTypes.SelectedActivity, presenter.LastSelectedItem);
                }
                e.Handled = true;
            }
            else if (e.Data.GetDataPresent(DragAndDropTypes.GetActivityToCall))
            {
                var activity = e.Data.GetData(DragAndDropTypes.GetActivityToCall);
                if (data != null && data != activity && data is MTFSubSequenceActivity)
                {
                    e.Data.SetData(DragAndDropTypes.GetActivityToCall, data);
                }
                e.Handled = true;
            }
            else
            {
                presenter.SelectionHelper.SetActualSender(listBox);
                int targetIndex = GetTargetIndex(listBox, e);
                var targetItem = dropTargetAdorner == null ? null : dropTargetAdorner.DropInfo.TargetItem;
                var targetOnItem = dropTargetAdorner != null && dropTargetAdorner.DropInfo.TargetOnItem;

                if (isCopy)
                {
                    MakeCopy(listBox, e, targetIndex, targetItem, targetOnItem);
                }
                else
                {
                    AddNewDroppedItem(listBox, e, targetIndex, targetItem, targetOnItem);
                    e.Handled = true;
                    listBox.Focus();
                }

                DisposeAdorner();
            }
        }

        private void MakeCopy(ListBox listBox, DragEventArgs e, int targetIndex, object targetItem, bool targetOnItem)
        {
            var activitiesToCopy = e.Data.GetData(DragAndDropTypes.MoveActivites) as List<MTFSequenceActivity>;
            if (listBox.Items.Count > 0)
            {
                switch (listBox.Name)
                {
                    case "CallListBox":
                        CreateAndInsertCopy(presenter.Sequence.ActivitiesByCall, targetIndex, targetItem, targetOnItem, activitiesToCopy);
                        break;
                    case "MainListBox":
                        CreateAndInsertCopy(presenter.Sequence.MTFSequenceActivities, targetIndex, targetItem, targetOnItem, activitiesToCopy);
                        break;
                    default:
                        CreateAndInsertCopy(null, targetIndex, targetItem, targetOnItem, activitiesToCopy);
                        break;
                }
                presenter.VerifyExecutionType(listBox.Items, targetItem, targetOnItem);
                e.Handled = true;
            }
            else
            {
                switch (listBox.Name)
                {
                    case "CallListBox":
                        CreateAndInsertCopy(presenter.Sequence.ActivitiesByCall, targetIndex, targetItem, targetOnItem, activitiesToCopy);
                        e.Handled = true;
                        break;
                    case "MainListBox":
                        CreateAndInsertCopy(presenter.Sequence.MTFSequenceActivities, targetIndex, targetItem, targetOnItem, activitiesToCopy);
                        e.Handled = true;
                        break;
                    default:
                        e.Handled = false;
                        break;
                }
                presenter.VerifyExecutionType(listBox.Items, targetItem, targetOnItem);
            }
        }

        private void AddNewDroppedItem(ListBox listBox, DragEventArgs e, int targetIndex, object targetItem, bool targetOnItem)
        {
            bool changeSelection = false;

            if (e.Data.GetDataPresent(DragAndDropTypes.MoveActivites))
            {
                var dragData = e.Data.GetData(DragAndDropTypes.MoveActivites);
                if (MoveSelection(listBox, targetIndex, targetItem, targetOnItem, dragData))
                {
                    presenter.VerifyExecutionType(listBox.Items, targetItem, targetOnItem);
                }
            }
            else if (e.Data.GetDataPresent(DragAndDropTypes.NewCommand))
            {
                InsertNewCommandAsActivity(e.Data.GetData(DragAndDropTypes.NewCommand), targetIndex, targetItem, targetOnItem, listBox);
                changeSelection = true;
            }
            else if (e.Data.GetDataPresent(DragAndDropTypes.SequenceClassInfo))
            {
                object sequenceClassInfo = e.Data.GetData(DragAndDropTypes.SequenceClassInfo);
                presenter.AdoptComponentToSequenceActivity(sequenceClassInfo, targetIndex, targetItem, targetOnItem);
                changeSelection = true;
            }
            else if (e.Data.GetDataPresent(DragAndDropTypes.NewVariable))
            {
                var variable = e.Data.GetData(DragAndDropTypes.NewVariable) as MTFVariable;
                presenter.AdoptVariableToSequenceActivity(variable, targetIndex, targetItem, targetOnItem);
                changeSelection = true;
            }

            if (changeSelection)
            {
                presenter.SelectionHelper.Add(presenter.LastSelectedItem as MTFSequenceActivity);
            }
        }

        private void ActivityListBoxPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var listBoxItem = UIHelper.FindParent<ListBoxItem>(e.MouseDevice.DirectlyOver as DependencyObject);
            if (listBoxItem != null)
            {
                var activity = listBoxItem.Content as MTFSequenceActivity;
                presenter.IsSelectedItem = activity != null;

                if (!presenter.SelectionHelper.MoreSelectedActivities || !presenter.SelectionHelper.Contains(activity))
                {
                    presenter.SelectionHelper.Add(activity);
                }
            }
            else
            {
                presenter.IsSelectedItem = false;
            }
        }

        private void ActivityListBoxDragOver(object sender, DragEventArgs e)
        {
            SwitchCaseByDragAndDropOver(e);
            var listBox = sender as ListBox;
            if (dragSource == DragSource.CallListBox)
            {
                DisposeAdorner();
            }
            dragSource = DragSource.MainListBox;
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Data.GetDataPresent(DragAndDropTypes.MoveActivites))
            {
                var o = UIHelper.GetObjectDataFromPoint(sender as ListBox, e.GetPosition(listBox));

                var data = e.Data.GetData(DragAndDropTypes.MoveActivites) as List<MTFSequenceActivity>;
                if (data != null && data.Contains(o))
                {
                    e.Effects = DragDropEffects.None;
                    e.Handled = true;
                    return;
                }
            }
            if (!listBox.AllowDrop)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }
            if (e.Data.GetDataPresent(DragAndDropTypes.SequenceClassInfo) ||
                e.Data.GetDataPresent(DragAndDropTypes.MoveActivites) ||
                e.Data.GetDataPresent(DragAndDropTypes.NewCommand) ||
                e.Data.GetDataPresent(DragAndDropTypes.NewVariable))
            {
                MoveAdorner(sender, e);
            }
            else if (!e.Data.GetDataPresent(DragAndDropTypes.GetActivityResult) && !e.Data.GetDataPresent(DragAndDropTypes.GetActivityToCall))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        private void CallListBoxDragOver(object sender, DragEventArgs e)
        {
            SwitchCaseByDragAndDropOver(e);
            MoveAdorner(sender, e);
            var listBox = sender as ListBox;
            if (dragSource == DragSource.MainListBox)
            {
                DisposeAdorner();
            }
            dragSource = DragSource.CallListBox;

            var o = UIHelper.GetObjectDataFromPoint(sender as ListBox, e.GetPosition(listBox));

            if (allowDropCallList || o != null)
            {
                if (dropTargetAdorner != null)
                {
                    var targetOnItem = dropTargetAdorner.DropInfo.TargetOnItem;
                    if ((dropTargetAdorner.DropInfo.TargetItem != null && !targetOnItem) && !allowDropCallList)
                    {
                        e.Effects = DragDropEffects.None;
                        e.Handled = true;
                        return;
                    }
                }
                MoveAdorner(sender, e);
            }
            else if (!e.Data.GetDataPresent(DragAndDropTypes.GetActivityResult) && !e.Data.GetDataPresent(DragAndDropTypes.GetActivityToCall))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        private void JumpToActivity(KeyEventArgs e)
        {
            var executeActivity = presenter.LastSelectedItem as MTFExecuteActivity;
            if (executeActivity != null)
            {
                switch (executeActivity.Type)
                {
                    case ExecuteActyvityTypes.Local:
                        JumpToActivityFromExecuteLocal(executeActivity, e);
                        break;
                    case ExecuteActyvityTypes.External:
                        JumpToActivityFromExecuteExternal(executeActivity, e);
                        break;
                    case ExecuteActyvityTypes.Dynamic:
                        //Do nothing
                        break;
                }
            }
        }

        private void JumpToActivityFromExecuteLocal(MTFExecuteActivity executeActivity, KeyEventArgs e)
        {
            if (executeActivity.ActivityToCall != null)
            {
                presenter.SelectionHelper.Add(executeActivity.ActivityToCall);
                presenter.LastSelectedItem = executeActivity.ActivityToCall;
                if (executeActivity.ActivityToCall != null)
                {
                    executeActivity.ActivityToCall.InvalidateVisual();
                }
                var listBox = UIHelper.FindListBoxByActivity(presenter.LastSelectedItem as MTFSequenceActivity, this);
                if (listBox != null)
                {
                    listBox.ScrollIntoView(presenter.LastSelectedItem);
                    listBox.Focus();
                }
                e.Handled = true;
            }
        }

        private void JumpToActivityFromExecuteExternal(MTFExecuteActivity externalActivity, KeyEventArgs e)
        {
            if (externalActivity.ExternalCall != null && !string.IsNullOrEmpty(externalActivity.ExternalCall.ExternalSequenceToCall))
            {
                Application.Current.Dispatcher.Invoke(() => presenter.ShowSequence(externalActivity.ExternalCall.ExternalSequenceToCall));
                if (externalActivity.ExternalCall.InnerSubSequenceByCallId != Guid.Empty &&
                    externalActivity.ExternalCall.InnerSubSequenceByCallId != ActivityNameConstants.CallWholeSequenceId)
                {

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        presenter.SelectionHelper.ClearAndInvalidate();
                        presenter.SelectionHelper.JumpToExternalMethod(presenter.Sequence, externalActivity.ExternalCall.InnerSubSequenceByCallId);
                        var listBox =
                            UIHelper.FindListBoxByActivity(presenter.LastSelectedItem as MTFSequenceActivity,
                                this);
                        if (listBox != null)
                        {
                            listBox.ScrollIntoView(presenter.LastSelectedItem);
                            listBox.Focus();
                        }
                    }, DispatcherPriority.ApplicationIdle);
                }
                else
                {
                    presenter.LastSelectedItem = null;
                    presenter.SelectionHelper.ClearAndInvalidate();
                }
            }
            e.Handled = true;
        }

        private void SwitchCaseByDragAndDropOver(DragEventArgs e)
        {
            if (e.Effects != DragDropEffects.None)
            {
                var tabItem = UIHelper.FindParent<TabItem>(e.OriginalSource as DependencyObject, 6);
                if (tabItem != null)
                {
                    var mtfCase = tabItem.Content as MTFCase;
                    if (mtfCase != null)
                    {
                        var subSequence = mtfCase.Parent as MTFSubSequenceActivity;
                        if (subSequence != null)
                        {
                            var tabControl = UIHelper.FindParent<TabControl>(tabItem, 3);
                            if (tabControl != null)
                            {
                                for (int i = 0; i < tabControl.Items.Count; i++)
                                {
                                    if (ReferenceEquals(tabControl.Items[i], tabItem.Content))
                                    {
                                        subSequence.ActualCaseIndex = i;
                                    }
                                }
                            }
                        }

                    }
                }
            }
        }

        private void ActivityListBoxPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (presenter.SelectionHelper.Count > 1 && (Keyboard.Modifiers != ModifierKeys.Control && Keyboard.Modifiers != ModifierKeys.Shift))
            {
                var listBoxItem = UIHelper.FindParent<ListBoxItem>(e.MouseDevice.DirectlyOver as DependencyObject);
                if (listBoxItem != null)
                {
                    var activity = listBoxItem.Content as MTFSequenceActivity;
                    if (activity != null)
                    {
                        presenter.SelectionHelper.ClearAndInvalidate(false);
                        presenter.SelectionHelper.Add(activity);
                    }
                }
            }
        }
    }

}
