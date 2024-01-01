using System;
using System.Collections.Generic;
using System.Linq;

namespace AssemblyWorkStationCommon
{
    [Serializable]
    public class DataContainer
    {
        public object Data { get; set; }
        public string Identification { get; set; }
    }

    public static class DataContainerExtension
    {
        public static T GetDataByType<T>(this List<DataContainer> container, string type)
        {
            if (container != null)
            {
                var data = container.FirstOrDefault(x => x.Identification == type);
                if (data != null)
                {
                    return (T)data.Data;
                }
            }
            return default(T);
        }
    }
}
