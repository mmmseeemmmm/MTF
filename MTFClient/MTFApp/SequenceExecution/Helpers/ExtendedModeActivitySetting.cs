using System;
using System.Collections.Generic;
using MTFApp.UIHelpers;
using MTFClientServerCommon;
using MTFClientServerCommon.Helpers;

namespace MTFApp.SequenceExecution.Helpers
{
    [Serializable]
    public class ExtendedModeActivitySetting : NotifyPropertyBase
    {
        private MTFActivityVisualisationWrapper activityWrapper;
        private List<Guid> guidPath;
        private StateDebugSetup state;
        private bool isDynamic;
        private string activityName;
        private bool isLoaded;
        private double priority;


        public ExtendedModeActivitySetting()
        {

        }

        public ExtendedModeActivitySetting(MTFActivityVisualisationWrapper activityWrapper)
        {
            this.activityWrapper = activityWrapper;
            guidPath = activityWrapper.GuidPath;
            isDynamic = activityWrapper.IsDynamicActivity;
            activityName = activityWrapper.Activity.ActivityName;
            isLoaded = true;
        }

        public List<Guid> GuidPath
        {
            get { return guidPath; }
            set { guidPath = value; }
        }

        public StateDebugSetup State
        {
            get { return state; }
            set
            {
                state = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsDynamic
        {
            get { return isDynamic; }
            set
            {
                isDynamic = value;
                NotifyPropertyChanged();
            }
        }

        public string ActivityName
        {
            get { return activityName; }
            set { activityName = value; }
        }

        public bool IsLoaded
        {
            get { return isLoaded; }
        }

        public double Priority
        {
            get { return priority; }
            set { priority = value; }
        }

        public MTFActivityVisualisationWrapper ActivityWrapper
        {
            get { return activityWrapper; }
        }

        public void AssignActivity(MTFActivityVisualisationWrapper activity)
        {
            isLoaded = true;
            activityWrapper = activity;
            NotifyPropertyChanged("IsLoaded");
            NotifyPropertyChanged("SequenceName");
            NotifyPropertyChanged("Path");
        }

        public string SequenceName
        {
            get
            {
                if (activityWrapper != null)
                {
                    var parent = activityWrapper.Activity.GetActivityParentOfType<MTFSequence>();
                    if (parent != null)
                    {
                        return parent.FullName;
                    }
                }
                return null;
            }
        }

        public string Path
        {
            get
            {
                return activityWrapper != null
                    ? string.Format("{0}{1}", activityWrapper.Activity.GetActivityPath().Replace("\\", "/"), ActivityName)
                    : null;
            }
        }
    }
}
