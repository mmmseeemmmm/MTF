using MTFClientServerCommon;

namespace MTFApp.Managers.Sequence
{
    internal class SequenceManager: ManagerBase
    {
        private MTFSequence sequence;

        public MTFSequence LoadSequence(string sequenceFullPath)
        {
            sequence = MTFClient.GetMTFClient().LoadSequence(sequenceFullPath);

            return sequence;
        }

    }
}
