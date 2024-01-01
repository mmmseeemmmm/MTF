using System;
using MTFApp.MTFWizard;
using MTFClientServerCommon.Helpers;

namespace MTFApp.ImportSequence
{
    public abstract class ImportSequenceBase: MTFWizardUserControl, IDisposable
    {
        protected readonly ImportSharedData SharedData;

        protected ImportSequenceBase(ImportSharedData sharedData)
        {
            SharedData = sharedData;
        }
        public override string WizardType
        {
            get { return LanguageHelper.GetString("Mtf_Import_Title"); }
        }

        public virtual void Dispose()
        {
            
        }
    }
}
