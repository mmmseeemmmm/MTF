using System.Collections.Generic;
using Microsoft.Win32;

namespace AutomotiveLighting.MTFCommon
{
    public static class General
    {
        public static List<string> AvailableComPorts
        {
            get { return GetComPorts(); }
        }
        static List<string> GetComPorts()
        {
            List<string> result = new List<string>();

            // Get COM-Ports from registry and add them to list
            RegistryKey regKeyDevicemap = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DEVICEMAP\SERIALCOMM");
            if (regKeyDevicemap != null)
            {
                string[] portNames = regKeyDevicemap.GetValueNames();
                foreach (string portName in portNames)
                {
                    result.Add((string)regKeyDevicemap.GetValue(portName));
                }
                regKeyDevicemap.Close();
            }
            return result;
        }
    }
}
