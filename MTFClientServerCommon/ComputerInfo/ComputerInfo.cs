using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon.ComputerInfo
{
    public static class ComputerInfo
    {
        public static OSInfo GetOsInfo()
        {
            var wmi = new ManagementObjectSearcher("select * from Win32_OperatingSystem")
            .Get()
            .Cast<ManagementObject>()
            .First();

            return new OSInfo
            {
                Name = ((string)wmi["Caption"]).Trim(),
                Version = (string)wmi["Version"],
                Architecture = (string)wmi["OSArchitecture"],
                SerialNumber = (string)wmi["SerialNumber"],
                Build = (string)wmi["BuildNumber"],
            };
        }

        public static CPUInfo GetCpuInfo()
        {
            var cpu =
            new ManagementObjectSearcher("select * from Win32_Processor")
            .Get()
            .Cast<ManagementObject>()
            .First();

            return new CPUInfo
            {
                Id = (string)cpu["ProcessorId"],
                Name = (string)cpu["Name"],
                Description = (string)cpu["Caption"],
                Architecture = (UInt16)cpu["Architecture"],
                CPUSpeedMHz = (uint)cpu["MaxClockSpeed"],
                Cores = (uint)cpu["NumberOfCores"],
                Threads = (uint)cpu["NumberOfLogicalProcessors"],
            };
        }

        public static ApplicationInfo GetApplicationInfo()
        {
            return new ApplicationInfo
            {
                BaseDirectory = AppDomain.CurrentDomain.BaseDirectory,
                MemoryUsage = (float)System.Diagnostics.Process.GetCurrentProcess().WorkingSet64 / (1024 * 1024),
            };
        }

        public static void UpdateApplicationInfo(ApplicationInfo applicationInfo)
        {
            applicationInfo.MemoryUsage = (float)System.Diagnostics.Process.GetCurrentProcess().WorkingSet64 / (1024 * 1024);
        }
    }
}
