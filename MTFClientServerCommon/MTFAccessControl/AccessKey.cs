using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UsbDrive;

namespace MTFClientServerCommon.MTFAccessControl
{

    [Serializable]
    public class AccessKey
    {
        public string KeyOwnerFirstName { get; private set; }
        public string KeyOwnerLastName { get; private set; }
        public string KeyCreatorName { get; private set; }        
        public DateTime Expiration { get; private set; }
        public List<AccessMachine> Machines { get; private set; }        
        public string AccessKeyType { get; private set; }
        public string VersionSW { get; private set; }
        public string UsbID { get; private set; }
        public string HwId { get; private set; }
        public byte[] Signature { get; set; }

        public AccessKey(string KeyOwnerFirstName, string KeyOwnerLastName, string KeyCreatorName, DateTime Expiration, List<AccessMachine> Machines, string AccessKeyType, string VersionSW, string UsbID, string HwID)
        {
            this.KeyOwnerFirstName = KeyOwnerFirstName;
            this.KeyOwnerLastName = KeyOwnerLastName;
            this.KeyCreatorName = KeyCreatorName;
            this.Expiration = Expiration;
            this.Machines = Machines;            
            this.AccessKeyType = AccessKeyType;
            this.VersionSW = VersionSW;
            this.UsbID = UsbID;
            this.HwId = HwID;
        }

        public bool IsDateValid
        {
            get { return DateTime.Now <= Expiration; }
        }

        [NonSerialized]
        private bool? isUsbIdValid ;
        public bool IsUsbIdValid
        {
            get
            {
                //if access key type isn't usb -> key is valid from usb point of view
                if (AccessKeyType != "USB")
                {
                    return true;
                }

                if (isUsbIdValid != null)
                {
                    return (bool)isUsbIdValid;
                }

                if (UsbID == "Key-Generated-By-Application-159a-adfe-eghz-547ydf-8jk-58rtuj-vluyj59")
                {
                    return true;
                }

                using (var usbManager = new UsbManager())
                {
                    isUsbIdValid = UsbManager.GetAvailableDisks().Any(usb => (usb.DeviceID == this.UsbID) || (usb.HardwareID == this.HwId));
                    return isUsbIdValid.Value;
                }
            }
            set
            {
                isUsbIdValid = value;
            }
        }

        public bool IsMachineValid
        {
            get
            {
                if (Machines == null || Machines.Count == 0)
                {
                    return true;
                }

                if (Machines != null && Machines.Count == 1 && string.Equals(Machines[0].MachineName, "default", StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }

                return Machines.Any(m => m.MachineName == Environment.MachineName);
            }
        }

        public bool IsValid
        {
            get { return IsDateValid && IsUsbIdValid && IsMachineValid; }
        }

        public bool HasRole(string roleName)
        {
            if (Machines == null || Machines.Count == 0)
            {
                return false;
            }
            var currentMachine = Machines.FirstOrDefault(m => m.MachineName == Environment.MachineName);
            if (currentMachine != null && currentMachine.Roles.Any(r => string.Equals(r.Role, roleName, StringComparison.CurrentCultureIgnoreCase)))
            {
                return true;
            }

            var allMachines = Machines.FirstOrDefault(m => string.Equals(m.MachineName, "default", StringComparison.CurrentCultureIgnoreCase));
            if (allMachines != null && allMachines.Roles.Any(r => string.Equals(r.Role, roleName, StringComparison.CurrentCultureIgnoreCase)))
            {
                return true;
            }

            return false;
        }

        public string GetTesxtForSign()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(KeyOwnerFirstName).
                Append(KeyOwnerLastName).
                Append(KeyCreatorName).
                Append(Expiration.ToString("M/d/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture)).
                Append(AccessKeyType).
                Append(VersionSW).
                Append(UsbID).
                Append(HwId);
            if (Machines != null)
            {
                sb.Append("Machines:");
                foreach (var machine in Machines)
                {
                    sb.Append(machine.MachineName);
                    if (machine.Roles != null)
                    {
                        sb.Append("Roles:");
                        foreach (var role in machine.Roles)
                        {
                            sb.Append(role.Role);
                        }
                    }
                    if (machine.Sequences != null)
                    {
                        sb.Append("Seqences");
                        foreach (var seq in machine.Sequences)
                        {
                            sb.Append(seq.Sequence);
                        }
                    }
                }
            }

            return sb.ToString();
        }
    }


}
