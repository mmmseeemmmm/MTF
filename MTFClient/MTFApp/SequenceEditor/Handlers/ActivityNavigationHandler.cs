using System;
using System.Collections.Generic;
using MTFClientServerCommon;

namespace MTFApp.SequenceEditor.Handlers
{
    class ActivityNavigationHandler
    {
        private MTFSequenceActivity targetActivity;
        private List<Guid> targetGuidPath;
        public bool NavigateAfterActivated { get; private set; }

        public MTFSequenceActivity TargetActivity
        {
            get => targetActivity;
            set
            {
                targetActivity = value;
                if (value != null)
                {
                    NavigateAfterActivated = true;
                    NavigationMode = NavigationMode.Activity;
                }
                else
                {
                    NavigateAfterActivated = false;
                    NavigationMode = NavigationMode.None;
                }
            }
        }

        public List<Guid> TargetGuidPath
        {
            get => targetGuidPath;
            set
            {
                targetGuidPath = value;
                if (value != null)
                {
                    NavigateAfterActivated = true;
                    NavigationMode = NavigationMode.GuidPath;
                }
                else
                {
                    NavigateAfterActivated = false;
                    NavigationMode = NavigationMode.None;
                }
            }
        }

        public NavigationMode NavigationMode { get; private set; }
    }

    enum NavigationMode
    {
        None,
        Activity,
        GuidPath
    }
}