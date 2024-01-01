using System;
using MTFClientServerCommon.SequenceLocalization;

namespace MTFApp.EditorTest
{
    public class LocalizedTestPresenter
    {
        public LocalizedTestPresenter()
        {
            SequenceLocalizationHelper.Load();

        }

        private string key = "Msg_Header_Exit";


        public string Key
        {
            get { return key; }
            set
            {
                key = value;
                Console.WriteLine(value);
            }
        }
    }
}
