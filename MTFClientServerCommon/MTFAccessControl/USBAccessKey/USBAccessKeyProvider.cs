using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsbDrive;

namespace MTFClientServerCommon.MTFAccessControl.USBAccessKey
{
    public class USBAccessKeyProvider : AccessKeyProvider
    {
        private USBAccessKeyHandler usbAccessKeyHandler;

        public USBAccessKeyProvider()
        {
            usbAccessKeyHandler = new USBAccessKeyHandler();
            usbAccessKeyHandler.UsbKeyStateChanged += usbAccessKeyHandler_UsbKeyStateChanged;

        }

        public override void Init()
        {
            Task.Run(() =>
            {
                var accessKey = usbAccessKeyHandler.CheckKeyPresence();
                if (accessKey != null)
                {
                    SetAccessKey(accessKey);
                }
            });
        }

        public override void Destroy()
        {
        }

        void usbAccessKeyHandler_UsbKeyStateChanged(object sender, AccessKey e)
        {
            if (e == null && ((USBAccessKeyHandler)sender).UsbStateChange == UsbStateChange.Added)
            {
                RaiseError("You inserted USB stick without MTF license. Please check your USB stick if contains license file.");
                return;
            }

            SetAccessKey(e);
        }
    }
}
