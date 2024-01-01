using System.Collections.Generic;

namespace AutomotiveLighting.MTFCommon
{
    public interface IParameterHelperClass
    {
        List<MTFParameterDescriptor> GetParameterDescriptors();
        void ParameterDescriptorChanged(ref List<MTFParameterDescriptor> currentParameterDescriptors);
    }
}
