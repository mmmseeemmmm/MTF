using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AutomotiveLighting.MTFCommon;
using MTFClientServerCommon;

namespace MTFApp.FindUsages
{
    public interface IFindActivityUsages
    {
        ObservableCollection<Tuple<MTFSequenceActivity, MTFSequence>> Items { get; }
        IEnumerable<Tuple<MTFSequenceActivity, MTFSequence>> FiltredItems { get; }
        IEnumerable<string> Types { get; }
        string SelectedType { get; set; }
        MTFIcons MTFIcon { get; set; }
    }
}
