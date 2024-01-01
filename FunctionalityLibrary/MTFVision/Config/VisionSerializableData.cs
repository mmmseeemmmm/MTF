using AutomotiveLighting.MTFCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MTFVision.Config
{
    public static class VisionSerializableData
    {
        public static IMTFSequenceRuntimeContext RuntimeContext { get; set; }

        public static void Save<T>(string UsedDataName, string key, T Data)
        {
            var data = RuntimeContext.GetTargetData<VisionConfigDictionary<T>>(UsedDataName); //load target data if teach mode
            if (data == null) //not teach mode
            {
                data=RuntimeContext.LoadData<VisionConfigDictionary<T>>(UsedDataName);
            }
            if(data==null) //no data was previously saved
            {
                data = new VisionConfigDictionary<T> { Dict = new Dictionary<string, T> () };
            }
            data.Dict[key] = Data;
            RuntimeContext.SaveData(UsedDataName, data);
        }

        public static T Load<T>(string UsedDataName, string Key)
        {
            var data= RuntimeContext.LoadData<VisionConfigDictionary<T>>(UsedDataName);
            if (data != null && data.Dict.ContainsKey(Key))
            {
                return data.Dict[Key];
            }
            return default(T);
        }
    }
}
