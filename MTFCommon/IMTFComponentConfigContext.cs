using System.Collections.Generic;

namespace AutomotiveLighting.MTFCommon
{
    public interface IMTFComponentConfigContext
    {
        string MessageBoxChoise(string header, string text, IList<string> items);
        string MessageBoxListBox(string header, string text, IList<string> items);
        string MessageBoxMultiChoise(string header, string text, IList<string> items);
    }
}
