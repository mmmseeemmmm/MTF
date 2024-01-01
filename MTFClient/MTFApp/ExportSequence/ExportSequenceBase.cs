using MTFClientServerCommon.Helpers;

namespace MTFApp.ExportSequence
{
    public abstract class ExportSequenceBase: MTFWizard.MTFWizardUserControl
    {
        protected ExportSharedData SharedData { get; private set; }

        protected ExportSequenceBase(ExportSharedData data)
        {
            SharedData = data;
        }

        public override string WizardType
        {
            get { return LanguageHelper.GetString("Mtf_Export_Title"); }
        }
    }
}
