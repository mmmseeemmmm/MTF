using System.Collections.Generic;
using System.IO.Ports;
using AutomotiveLighting.MTFCommon;

namespace ModuleCheckBoxComponent
{
    public class ModuleCheckBoxHelper : IParameterHelperClass
    {
        private List<string> ports;

        public ModuleCheckBoxHelper()
        {
            this.refreshPorts();
        }

        public List<MTFParameterDescriptor> GetParameterDescriptors()
        {
            return new List<MTFParameterDescriptor>
                   {
                       new MTFParameterDescriptor
                       {
                           ParameterName = "portName",
                           ControlType = MTFParameterControlType.ListBox,
                           Value = null,
                           IsEnabled = true
                       }
                   };
        }

        public void ParameterDescriptorChanged(ref List<MTFParameterDescriptor> currentParameterDescriptors)
        {
            currentParameterDescriptors[0].Value = ports;
        }

        private void refreshPorts()
        {
            this.ports = new List<string>();
            this.ports.Add("USB");
            foreach (var element in SerialPort.GetPortNames())
            {
                this.ports.Add(element);
            }
        }
    }
}
