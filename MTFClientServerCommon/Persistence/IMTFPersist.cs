using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon
{
    [ServiceContract]
    public interface IMTFPersist
    {
        [OperationContract]
        List<T> LoadDataList<T>(string fileName) where T : MTFPersist;

        [OperationContract]
        void SaveData<T>(List<T> objectToPersist, string fileName) where T : MTFPersist;
    }
}
