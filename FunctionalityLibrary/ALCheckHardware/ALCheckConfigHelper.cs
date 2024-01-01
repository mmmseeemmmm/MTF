using AutomotiveLighting.MTFCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ALCheckHardwareDriver
{
    class ALCheckConfigHelper : IParameterHelperClass
    {
        public List<MTFParameterDescriptor> GetParameterDescriptors()
        {
            List<string> coms = General.AvailableComPorts;

            List<ValueWithName> comPorts = new List<ValueWithName> { };

            foreach (string com in coms)
            {
                ValueWithName oneDevice = new ValueWithName();
                oneDevice.DisplayName = com;
                oneDevice.Value = com;
                comPorts.Add(oneDevice);
            }

            return new List<MTFParameterDescriptor> {
                new MTFParameterDescriptor { 
                    ParameterName = "comPort", 
                    IsEnabled = true, 
                    ControlType = MTFParameterControlType.ListBox, 
                    AllowedValues = comPorts.ToArray(),
                },
                new MTFParameterDescriptor{
                    ParameterName = "requestedPLCVersion",
                    IsEnabled = true,
                    ControlType = MTFParameterControlType.TextInput,
                    Value = "3297961669",
                },
            };

        }

        public void ParameterDescriptorChanged(ref List<MTFParameterDescriptor> currentParameterDescriptors)
        {
            //throw new NotImplementedException();
        }
    }
}
