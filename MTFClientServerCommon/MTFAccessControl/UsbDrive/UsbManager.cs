using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;


namespace UsbDrive
{
    public enum UsbStateChange { Added, Removing, Removed }

    public class UsbManager : IDisposable
    {
        private delegate void GetDiskInformationDelegate(UsbKey disk);

        private bool isDisposed;
        ManagementEventWatcher insertWatcher;
        ManagementEventWatcher removeWatcher;

        //========================================================================================
        // Constructor
        //========================================================================================

        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// 
        /// 

        public static string GetDriveVidPid(string szDriveName)
        {
            string Result = string.Empty;
            string szSerialNumberDevice = null;

            ManagementObject oLogicalDisk = new ManagementObject("Win32_LogicalDisk.DeviceID='" + szDriveName.TrimEnd('\\') + "'");
            foreach (ManagementObject oDiskPartition in oLogicalDisk.GetRelated("Win32_DiskPartition"))
            {
                foreach (ManagementObject oDiskDrive in oDiskPartition.GetRelated("Win32_DiskDrive"))
                {
                    string szPNPDeviceID = oDiskDrive["PNPDeviceID"].ToString();
                    if (!szPNPDeviceID.StartsWith("USBSTOR"))
                        throw new Exception(szDriveName + " is not connected");

                    string[] aszToken = szPNPDeviceID.Split(new char[] { '\\', '&' });
                    szSerialNumberDevice = aszToken[aszToken.Length - 2];
                }
            }

            if (null != szSerialNumberDevice)
            {
                ManagementObjectSearcher oSearcher = new ManagementObjectSearcher(@"root\CIMV2", "Select * from Win32_USBHub");
                var drives= oSearcher.Get();
                string[] devices=new string[drives.Count];
                int i = 0;
                foreach (var item in drives)
                {
                    devices[i] = item["DeviceID"] as string;
                    i++;
                }
                    
                

                //ManagementObjectSearcher oSearcher = new ManagementObjectSearcher(@"root\CIMV2", "Select * From Win32_USBControllerDevice");
                foreach (ManagementObject oResult in oSearcher.Get())
                {
                    object oValue = oResult["DeviceID"];
                    if (oValue == null)
                        continue;

                    string szDeviceID = oValue.ToString();
                    string[] aszToken = szDeviceID.Split(new char[] { '\\' });
                    if (szSerialNumberDevice != aszToken[aszToken.Length - 1])
                        continue;

                    int nTemp = szDeviceID.IndexOf("VID_");
                    if (0 > nTemp)
                        continue;

                    nTemp += 4;
                     ushort.Parse(szDeviceID.Substring(nTemp, 4), System.Globalization.NumberStyles.AllowHexSpecifier);

                    nTemp += 4;
                    nTemp = szDeviceID.IndexOf("PID_", nTemp);
                    if (0 > nTemp)
                        continue;

                    nTemp += 4;
                    ushort.Parse(szDeviceID.Substring(nTemp, 4), System.Globalization.NumberStyles.AllowHexSpecifier);

                    Result = szDeviceID;
                    break;
                }
            }


            return Result;
        }

        public UsbManager()
        {
            this.isDisposed = false;

            WqlEventQuery insertQuery = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2");
            insertWatcher = new ManagementEventWatcher(insertQuery);
            insertWatcher.EventArrived += DeviceInsertedEvent;
            insertWatcher.Start();

            WqlEventQuery removeQuery = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 3");
            removeWatcher = new ManagementEventWatcher(removeQuery);
            removeWatcher.EventArrived += DeviceRemovedEvent;
            removeWatcher.Start();
        }

        private void DeviceInsertedEvent(object sender, EventArrivedEventArgs e)
        {
            var disks = GetAvailableDisks();

            SignalDeviceChange(UsbStateChange.Added, disks);
        }

        private void DeviceRemovedEvent(object sender, EventArrivedEventArgs e)
        {
            var disks = GetAvailableDisks();
            if (disks.Count > 0)
            {
                SignalDeviceChange(UsbStateChange.Removed, disks);
            }
            else
            {
                SignalDeviceChange(UsbStateChange.Removed, null);
            }
        }




        #region Lifecycle

        /// <summary>
        /// Destructor.
        /// </summary>

        ~UsbManager()
        {
            Dispose();
        }


        /// <summary>
        /// Must shutdown the driver window.
        /// </summary>

        public void Dispose()
        {
            if (!isDisposed)
            {
                insertWatcher.EventArrived -= DeviceInsertedEvent;
                insertWatcher.Stop();
                insertWatcher.Dispose();

                removeWatcher.EventArrived -= DeviceRemovedEvent;
                removeWatcher.Stop();
                removeWatcher.Dispose();
                //if (window != null)
                //{
                //    window.StateChanged -= new UsbStateChangedEventHandler(DoStateChanged);
                //    window.Dispose();
                //    window = null;
                //}

                isDisposed = true;

                GC.SuppressFinalize(this);
            }
        }

        #endregion Lifecycle


        //========================================================================================
        // Events/Properties
        //========================================================================================

        /// <summary>
        /// Add or remove a handler to listen for USB disk drive state changes.
        /// </summary>

        public event UsbStateChangedEventHandler StateChanged;


        private void SignalDeviceChange(UsbStateChange state, UsbKeysCollection disks)
        {
            if (StateChanged != null)
            {
                StateChanged(new UsbStateChangedEventArgs(state, disks));
            }
        }


        //========================================================================================
        // Methods
        //========================================================================================

        /// <summary>
        /// Gets a collection of all available USB disk drives currently mounted.
        /// </summary>
        /// <returns>
        /// A UsbDiskCollection containing the USB disk drives.
        /// </returns>

        public static UsbKeysCollection GetAvailableDisks()
        {
            try
            {
                UsbKeysCollection disks = new UsbKeysCollection();
                //Dictionary
                Dictionary<string, string> devices = new Dictionary<string, string>();


                foreach (ManagementObject device in new ManagementObjectSearcher(@"SELECT * FROM Win32_DiskDrive WHERE InterfaceType LIKE 'USB%'").Get())
                {
                    devices.Add((string)device.GetPropertyValue("DeviceID"), (string)device.GetPropertyValue("PNPDeviceID"));
                }
                
                // browse all USB WMI physical disks
                List<Task> tasks=new List<Task>();
                foreach (ManagementObject drive in
                    new ManagementObjectSearcher(
                        "select DeviceID, Model from Win32_DiskDrive where InterfaceType='USB'").Get())
                {
                    tasks.Add(Task.Factory.StartNew(() =>
                                                    {
                                                        // associate physical disks with partitions
                                                        ManagementObject partition = new ManagementObjectSearcher(String.Format(
                                                            "associators of {{Win32_DiskDrive.DeviceID='{0}'}} where AssocClass = Win32_DiskDriveToDiskPartition",
                                                            drive["DeviceID"])).First();

                                                        if (partition != null)
                                                        {
                                                            // associate partitions with logical disks (drive letter volumes)
                                                            ManagementObject logical = new ManagementObjectSearcher(String.Format(
                                                                "associators of {{Win32_DiskPartition.DeviceID='{0}'}} where AssocClass = Win32_LogicalDiskToPartition",
                                                                partition["DeviceID"])).First();

                                                            if (logical != null)
                                                            {
                                                                // finally find the logical disk entry to determine the volume name
                                                                var volumeName = logical["Name"].ToString();
                                                                UsbKey disk = new UsbKey(volumeName);
                                                                var volume  = DriveInfo.GetDrives().FirstOrDefault(x=>x.Name.StartsWith(volumeName));
                                                                if (volume != null)
                                                                {
                                                                    disk.Volume = volume.VolumeLabel;
                                                                }

                                                                //var hwID = (string)drive.GetPropertyValue("HardwareID");
                                                                disk.DeviceID = (string)drive.GetPropertyValue("DeviceID");
                                                                disk.DeviceID = devices[disk.DeviceID];

                                                                var hid = GetDriveVidPid(disk.DriveLetter);
                                                                if (!string.IsNullOrEmpty(hid))
                                                                {
                                                                    disk.HardwareID = hid;
                                                                    //throw new Exception("Usb is not supported!");
                                                                }

                                                                disks.Add(disk);

                                                            }
                                                        }
                                                    }));
                }
                try
                {
                    if (Task.WaitAll(tasks.ToArray(), 10000))
                    {
                        //When some of the mounted drives loads long time it is not possible to map USB with its Letter
                        return disks;
                    }                    
                }
                catch
                {
                    
                }
                return null;
                //return disks;
            }
            catch
            {
                return null;
            }
        }

        //private void getHID()
        //{
        //    ManagementObjectSearcher searcher1 = new ManagementObjectSearcher("Select * From Win32_PnPEntity where name = 'USB Mass Storage Device'");
        //    ManagementObjectCollection deviceCollection = searcher1.Get();

        //    foreach (var device in deviceCollection)
        //    {
        //        try
        //        {
        //            //Console.WriteLine("SerialNumber: {0}", device["SerialNumber"]);
        //            //var serialNr = device.GetPropertyValue("SerialNumber");
        //            string[] hardwareIDs = (string[])device.GetPropertyValue("HardwareID");
        //            Console.WriteLine((string)device.GetPropertyValue("DeviceID"));
        //            Console.WriteLine((string)device.GetPropertyValue("PNPDeviceID"));
        //            //string[] hardwareIDs = (string[])device.GetPropertyValue("HardwareID");
        //            if (hardwareIDs == null)
        //                continue;

        //            Console.WriteLine(hardwareIDs[0]);


        //            Console.WriteLine();
        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine(e);
        //        }
        //    }
        //}



    }
}
