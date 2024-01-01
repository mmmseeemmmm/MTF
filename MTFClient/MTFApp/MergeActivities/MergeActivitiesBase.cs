using MTFApp.MTFWizard;

namespace MTFApp.MergeActivities
{
    public abstract class MergeActivitiesBase : MTFWizardUserControl
    {
        public override string WizardType
        {
            get { return "Merge activities"; }
        }
    }
}
