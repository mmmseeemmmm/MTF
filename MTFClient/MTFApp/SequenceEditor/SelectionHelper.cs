using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MTFClientServerCommon;

namespace MTFApp.SequenceEditor
{
    public class SelectionHelper
    {
        private readonly SequenceEditorPresenter editorPresenter;
        private readonly List<MTFSequenceActivity> selectedActivities = new List<MTFSequenceActivity>();
        private object actualSender = null;


        public SelectionHelper(SequenceEditorPresenter editorPresenter)
        {
            this.editorPresenter = editorPresenter;
        }

        public List<MTFSequenceActivity> Selection
        {
            get
            {
                return selectedActivities;
            }
        }


        public void Add(MTFSequenceActivity activity)
        {
            if (activity != null)
            {
                if (!selectedActivities.Contains(activity))
                {
                    ClearAndInvalidate();
                    selectedActivities.Add(activity);
                    activity.InvalidateVisual();
                    editorPresenter.LastSelectedItem = activity;
                }
            }
            else
            {
                SystemLog.LogMessage("Null activity has been added", true);
            }
        }

        public void AddMore(List<MTFSequenceActivity> activities)
        {
            ClearAndInvalidate();
            if (activities != null && activities.Count > 0)
            {
                foreach (var activity in activities)
                {
                    if (activity != null)
                    {
                        selectedActivities.Add(activity);
                        activity.InvalidateVisual();
                    }
                    else
                    {
                        SystemLog.LogMessage("Null activity has been added", true);
                    }
                }
                editorPresenter.LastSelectedItem = activities.Last();
            }
        }


        public void AddByKeyboard(MTFSequenceActivity activity, bool downDirection)
        {
            if (selectedActivities.Count > 0)
            {
                if (selectedActivities.Contains(activity))
                {
                    var activityToRemive = downDirection ? selectedActivities.First() : selectedActivities.Last();
                    selectedActivities.Remove(activityToRemive);
                    activityToRemive.InvalidateVisual();
                }
                else
                {
                    if (downDirection)
                    {
                        selectedActivities.Add(activity);
                    }
                    else
                    {
                        selectedActivities.Insert(0, activity);
                    }
                }
            }
            else
            {
                selectedActivities.Add(activity);
            }
            activity.InvalidateVisual();
            editorPresenter.LastSelectedItem = activity;
        }

        public void AddWithCtrl(MTFSequenceActivity activity, IList<MTFSequenceActivity> itemCollection)
        {
            if (activity != null)
            {
                if (selectedActivities.Contains(activity))
                {
                    selectedActivities.Remove(activity);
                }
                else
                {
                    AddSorted(activity, itemCollection);
                }
                activity.InvalidateVisual();
                editorPresenter.LastSelectedItem = activity;
            }
        }

        private void AddSorted(MTFSequenceActivity activity, IList<MTFSequenceActivity> itemCollection)
        {
            if (selectedActivities.Count > 0)
            {
                var activityPosition = itemCollection.IndexOf(activity);

                if (activityPosition != -1)
                {
                    for (int i = 0; i < selectedActivities.Count; i++)
                    {
                        var currentIndex = itemCollection.IndexOf(selectedActivities[i]);
                        if (currentIndex > activityPosition)
                        {
                            selectedActivities.Insert(i, activity);
                            return;
                        }
                    }
                }
            }

            selectedActivities.Add(activity);
        }

        public void AddWithShift(IList collection, MTFSequenceActivity lastActivity)
        {
            if (collection != null && collection.Count > 0)
            {
                var firstSelectedActivity = selectedActivities.FirstOrDefault();
                if (firstSelectedActivity != null)
                {
                    var index1 = collection.IndexOf(firstSelectedActivity);
                    var index2 = collection.IndexOf(lastActivity);
                    if (index1 != -1 && index2 != -1)
                    {
                        var oldActivities = selectedActivities.ToList();
                        selectedActivities.Clear();
                        if (index1 > index2)
                        {
                            for (int i = index2; i < index1 + 1; i++)
                            {
                                AddNextItem(collection[i]);
                            }
                        }
                        else
                        {
                            for (int i = index1; i < index2 + 1; i++)
                            {
                                AddNextItem(collection[i]);
                            }
                        }
                        foreach (var activity in selectedActivities.Union(oldActivities))
                        {
                            activity.InvalidateVisual();
                        }

                    }
                    else
                    {
                        Add(lastActivity);
                    }
                }
                else
                {
                    Add(lastActivity);
                }
            }
        }

        private void AddNextItem(object item)
        {
            var activity = item as MTFSequenceActivity;
            if (activity != null)
            {
                selectedActivities.Add(activity);
            }
        }

        public void ClearAndInvalidate()
        {
            ClearAndInvalidate(false);
        }

        public void ClearAndInvalidate(bool nullAsSelected)
        {
            var oldActivities = selectedActivities.ToList();
            selectedActivities.Clear();
            oldActivities.ForEach(a => a.InvalidateVisual());
            if (nullAsSelected)
            {
                editorPresenter.LastSelectedItem = null;
            }
        }

        public void Clear()
        {
            Clear(false);
        }

        public void Clear(bool nullAsSelected)
        {
            selectedActivities.Clear();
            if (nullAsSelected)
            {
                editorPresenter.LastSelectedItem = null;
            }
        }

        public void PerformActionOnEachActivity(Action<MTFSequenceActivity> action)
        {
            selectedActivities.ForEach(action);
        }

        public bool AllIsSubsequences()
        {
            return selectedActivities.All(a => a is MTFSubSequenceActivity);
        }


        public void JumpToExternalMethod(MTFSequence sequence, Guid subSequenceId)
        {
            if (sequence != null && sequence.ActivitiesByCall != null)
            {
                var subSequence = sequence.ActivitiesByCall.FirstOrDefault(x => x.Id == subSequenceId);
                if (subSequence != null)
                {
                    Add(subSequence);
                }
            }
        }

        public int Count
        {
            get { return selectedActivities.Count; }
        }

        public MTFSequenceActivity FirstActivity()
        {
            return selectedActivities.First();
        }

        public MTFSequenceActivity LastActivity()
        {
            return selectedActivities.Last();
        }

        public bool MoreSelectedActivities
        {
            get { return selectedActivities.Count > 1; }
        }

        public bool Contains(MTFSequenceActivity activity)
        {
            return selectedActivities.Contains(activity);
        }

        public void SetActualSender(object sender)
        {
            actualSender = sender;
        }

        public bool IsSameSender(object sender)
        {
            return ReferenceEquals(actualSender, sender);
        }




    }
}
