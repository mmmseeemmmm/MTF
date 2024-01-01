using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using AutomotiveLighting.MTFCommon;

namespace ClimaChamberDriver
{
    //[MTFClass(Name = "Clima Chamber helper class", Description = "BaudRate = 19200, ASCI")]
    //[MTFClassCategory("Clima chambers")]
    internal class ClimaHelper
    {
        [MTFMethod(Description = "Returns list of available Clima Chambers")]
        public List<ClimaChamber> GetDevices()
        {
            return GetDevices();
        }

        [MTFMethod(Description = "Returns list of available Clima Chambers, com ports for ignoring could be defined")]
        public List<ClimaChamber> GetDevices(List<string> doNotUseComs)
        {
            List<string> coms = General.AvailableComPorts;
            List<ClimaChamber> result = null;
            
            foreach (var item in coms)
            {
                if (!(doNotUseComs != null && doNotUseComs.Contains(item)))
                {
                    ClimaChamber clima = null;
                    try
                    {
                        //clima = new ClimaChamber(item);
                        result.Add(clima);
                    }
                    catch { }
                }
            }
            return result;
        }     
            
    }
}
