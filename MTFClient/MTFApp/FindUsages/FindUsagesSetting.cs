using System;
using System.Collections.Generic;
using System.Windows;
using MTFClientServerCommon;

namespace MTFApp.FindUsages
{
    public class FindUsagesSetting
    {
        public Window Owner { get; set; }
        public MTFSequence Sequence { get; set; }
        public Action<MTFSequenceActivity, MTFSequence> SelectionCallBack { get; set; }
        public Func<IEnumerable<MTFSequence>> GetAllSequences { get; set; }

        public FindUsagesSetting(Window owner, MTFSequence sequence, Action<MTFSequenceActivity, MTFSequence> selectionCallBack, Func<IEnumerable<MTFSequence>> getAllSequences)
        {
            Owner = owner;
            Sequence = sequence;
            SelectionCallBack = selectionCallBack;
            GetAllSequences = getAllSequences;
        }
    }
}
