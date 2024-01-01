using System.Collections.Generic;
using System.ServiceModel;

namespace MTFCommon
{
    [ServiceContract]
    public interface IMTFExtendedCoreService
    {
        [OperationContract]
        List<MTFFileInfo> GetSequenceFileInfos(string path);

        [OperationContract]
        MTFSequenceVariant GetSequenceVariant(string sequenceFullName);
    }
}
