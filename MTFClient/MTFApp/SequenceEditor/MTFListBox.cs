using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MTFApp.UIHelpers;
using MTFClientServerCommon;

namespace MTFApp.SequenceEditor
{
    public class MTFListBox : ListBox
    {
        protected override void OnKeyDown(KeyEventArgs e)
        {
            e.Handled = true;

            if (SequenceEditor != null)
            {
                SequenceEditor.LastSelectedListBox = this;
            }
            if (e.Key == Key.Tab && Keyboard.Modifiers == ModifierKeys.Control)
            {
                SwitchSubSequenceCase(SelectedItem as MTFSubSequenceActivity, CaseDirection.Next);
                e.Handled = true;
                return;
            }
            if (e.Key == Key.Tab && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                SwitchSubSequenceCase(SelectedItem as MTFSubSequenceActivity, CaseDirection.Previous);
                e.Handled = true;
                return;
            }

            switch (e.Key)
            {
                case Key.Right:
                    PerformKeyRight();
                    break;
                case Key.Left:
                    PerformKeyLeft();
                    break;
                case Key.Up:
                    PerformKeyUp(e);
                    break;
                case Key.Down:
                    PerformKeyDown(e);
                    break;
                default:
                    e.Handled = false;
                    break;
            }
        }

        private void PerformKeyLeft()
        {
            var subSequence = SelectedItem as MTFSubSequenceActivity;
            if (subSequence != null)
            {
                if (subSequence.IsCollapsed)
                {
                    JumpUpToParent(this);
                }
                else
                {
                    subSequence.IsCollapsed = true;
                    SequenceEditor.LastSelectedItem = subSequence;
                }
            }
            else
            {
                JumpUpToParent(this);
            }
        }

        private void PerformKeyRight()
        {
            var subSequence = SelectedItem as MTFSubSequenceActivity;
            if (subSequence != null && subSequence.IsCollapsed)
            {
                subSequence.IsCollapsed = false;
            }
        }

        private void PerformKeyUp(KeyEventArgs e)
        {
            if (SelectedIndex == -1)
            {
                return;
            }
            if (SelectedIndex == 0)
            {
                JumpUpToParent(this);
                return;
            }
            var previousIndex = SelectedIndex - 1;
            if (previousIndex >= 0)
            {
                var activity = Items[previousIndex] as MTFSequenceActivity;
                var subSequence = activity as MTFSubSequenceActivity;
                if (subSequence != null)
                {
                    var previousItem = ItemContainerGenerator.ContainerFromIndex(previousIndex) as ListBoxItem;
                    if (previousItem != null)
                    {
                        if (JumpToLastItem(subSequence, previousItem))
                        {
                            return;
                        }
                    }

                }
                if (activity != null && SequenceEditor != null)
                {
                    if (Keyboard.Modifiers == ModifierKeys.Shift)
                    {
                        SequenceEditor.SelectionHelper.AddByKeyboard(activity, false);
                    }
                    else
                    {
                        SequenceEditor.SelectionHelper.Add(activity);
                    }
                    ScrollIntoView(activity);
                }
            }
        }

        private void PerformKeyDown(KeyEventArgs e)
        {
            if (SequenceEditor == null || SelectedItem == null)
            {
                return;
            }
            var subSequence = SelectedItem as MTFSubSequenceActivity;
            if (SelectedIndex == Items.Count - 1 &&
                (subSequence == null || subSequence.IsCollapsed || (subSequence.Activities != null && subSequence.Activities.Count == 0)))
            {
                if (SequenceEditor != null)
                {
                    JumpToNextInParent(this);
                }
                e.Handled = true;
                return;
            }
            if (subSequence != null && !subSequence.IsCollapsed && subSequence.IsActive && Keyboard.Modifiers!= ModifierKeys.Shift)
            {
                var activities = subSequence.ExecutionType == ExecutionType.ExecuteByCase ? GetActivitiesFromCurrentCase(subSequence) : subSequence.Activities;
                if (activities != null && activities.Count > 0)
                {
                    var firstActivity = activities.FirstOrDefault();
                    if (firstActivity != null)
                    {
                        SequenceEditor.SelectionHelper.Add(firstActivity);
                        var nextListBox = UIHelper.FindListBoxByActivity(firstActivity, this);
                        if (nextListBox != null)
                        {
                            nextListBox.Focus();
                        }
                    }
                }
                else
                {
                    GotoNextItem();
                }
            }
            else
            {
                GotoNextItem();
            }
        }


        private void GotoNextItem()
        {
            var nextIndex = SelectedIndex + 1;
            if (nextIndex < Items.Count)
            {
                var activity = Items[nextIndex] as MTFSequenceActivity;
                if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    SequenceEditor.SelectionHelper.AddByKeyboard(activity, true);
                }
                else
                {
                    SequenceEditor.SelectionHelper.Add(activity);
                }
                ScrollIntoView(SelectedItem);
            }
        }

        private void SwitchSubSequenceCase(MTFSubSequenceActivity subSequence, CaseDirection direction)
        {
            if (subSequence != null && subSequence.ExecutionType == ExecutionType.ExecuteByCase && subSequence.Cases != null && subSequence.Cases.Count > 1)
            {
                switch (direction)
                {
                    case CaseDirection.Next:
                        SwitchToNextCase(subSequence);
                        break;
                    case CaseDirection.Previous:
                        SwitchToPreviousCase(subSequence);
                        break;
                }
            }
        }

        private void SwitchToNextCase(MTFSubSequenceActivity subSequence)
        {
            var index = subSequence.ActualCaseIndex + 1;
            subSequence.ActualCaseIndex = index >= subSequence.Cases.Count ? 0 : index;
        }

        private void SwitchToPreviousCase(MTFSubSequenceActivity subSequence)
        {
            var index = subSequence.ActualCaseIndex - 1;
            subSequence.ActualCaseIndex = index < 0 ? subSequence.Cases.Count - 1 : index;
        }

        private IList<MTFSequenceActivity> GetActivitiesFromCurrentCase(MTFSubSequenceActivity subSequence)
        {
            if (subSequence.Cases != null && subSequence.Cases.Count > 0 && subSequence.ActualCaseIndex > -1 && subSequence.ActualCaseIndex < subSequence.Cases.Count)
            {
                var currentCase = subSequence.Cases[subSequence.ActualCaseIndex];
                if (currentCase != null)
                {
                    return currentCase.Activities;
                }
            }
            return null;
        }

        private bool JumpToLastItem(MTFSubSequenceActivity subSequence, ListBoxItem subSequenceContainer)
        {
            if (subSequence != null && !subSequence.IsCollapsed && SequenceEditor != null && subSequence.IsActive && Keyboard.Modifiers!= ModifierKeys.Shift)
            {
                var activities = subSequence.ExecutionType == ExecutionType.ExecuteByCase ? GetActivitiesFromCurrentCase(subSequence) : subSequence.Activities;
                if (activities != null && activities.Count > 0)
                {
                    ListBoxItem listBoxItem = null;
                    ListBox listBox = null;
                    var lastActivity = activities.Last();
                    if (subSequenceContainer != null)
                    {
                        listBox = UIHelper.FindChild<ListBox>(subSequenceContainer);
                        listBoxItem = listBox.ItemContainerGenerator.ContainerFromItem(lastActivity) as ListBoxItem;
                    }
                    var lastSubSequence = lastActivity as MTFSubSequenceActivity;
                    if (lastSubSequence != null && !lastSubSequence.IsCollapsed && lastSubSequence.Activities != null && lastSubSequence.Activities.Count > 0 && lastSubSequence.IsActive)
                    {
                        return JumpToLastItem(lastSubSequence, listBoxItem);
                    }
                    SequenceEditor.SelectionHelper.Add(lastActivity);
                    if (listBoxItem != null)
                    {
                        listBoxItem.Focus();
                        listBox.SelectedItem = listBoxItem.Content;
                    }
                    return true;
                }
            }
            return false;
        }

        private void JumpUpToParent(ListBox listBox)
        {
            var parentListBoxItem = UIHelper.FindParent<ListBoxItem>(listBox);
            if (parentListBoxItem != null)
            {
                var parentListBox = UIHelper.FindParent<ListBox>(parentListBoxItem) as MTFListBox;
                if (parentListBox != null && parentListBox.SequenceEditor != null)
                {
                    SelectListBoxItem(parentListBoxItem, parentListBox);
                }
            }
        }


        private void JumpToNextInParent(ListBox currentListBox)
        {
            var parentListBoxItem = UIHelper.FindParent<ListBoxItem>(currentListBox);
            if (parentListBoxItem != null)
            {
                var parentListBox = UIHelper.FindParent<ListBox>(parentListBoxItem);
                if (parentListBox != null)
                {
                    int index = parentListBox.ItemContainerGenerator.IndexFromContainer(parentListBoxItem);
                    if (index < parentListBox.Items.Count - 1)
                    {
                        var nextItem = parentListBox.ItemContainerGenerator.ContainerFromIndex(index + 1) as ListBoxItem;
                        if (nextItem != null)
                        {
                            SelectListBoxItem(nextItem, parentListBox);
                        }
                    }
                    else
                    {
                        JumpToNextInParent(parentListBox);
                    }
                }
            }
        }

        private void SelectListBoxItem(ListBoxItem item, ListBox listBox)
        {
            var activitiy = item.Content as MTFSequenceActivity;
            if (activitiy != null)
            {
                SequenceEditor.SelectionHelper.Add(activitiy);
                listBox.SelectedItem = activitiy;
                listBox.ScrollIntoView(listBox.SelectedItem);
                listBox.Focus();
            }
        }

        public SequenceEditorPresenter SequenceEditor
        {
            get { return (SequenceEditorPresenter)GetValue(SequenceEditorProperty); }
            set { SetValue(SequenceEditorProperty, value); }
        }

        public static readonly DependencyProperty SequenceEditorProperty = DependencyProperty.Register("SequenceEditor", typeof(SequenceEditorPresenter), typeof(MTFListBox), new FrameworkPropertyMetadata());
    }

    internal enum CaseDirection
    {
        Next,
        Previous
    }
}
