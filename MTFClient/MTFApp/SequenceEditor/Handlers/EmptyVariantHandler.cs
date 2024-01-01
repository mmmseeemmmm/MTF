using MTFApp.SequenceEditor.Settings.SequenceVariant;
using MTFClientServerCommon;

namespace MTFApp.SequenceEditor.Handlers
{
    public class EmptyVariantHandler
    {
        public MTFDataTransferObject ParentObject { get; }
        public object ObjectToRemove { get; }


        public EmptyVariantChoices Choice { get; set; }

        public EmptyVariantHandler(MTFDataTransferObject parentObject, object objectToRemove)
        {
            ParentObject = parentObject;
            ObjectToRemove = objectToRemove;
        }
    }
}