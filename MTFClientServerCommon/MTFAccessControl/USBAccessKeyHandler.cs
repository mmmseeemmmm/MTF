using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MTFClientServerCommon.MTFAccessControl.UsbDrive;
using UsbDrive;

namespace MTFClientServerCommon.MTFAccessControl
{
    public class USBAccessKeyHandler : IDisposable
    {
        public event EventHandler<AccessKey> UsbKeyStateChanged;
        private UsbManager usbManager = new UsbManager();
        private const string GeneratedKeyName = "MTFLicense.dat";

        public USBAccessKeyHandler()
        {
            usbManager.StateChanged += OnUsbStateChangedEventHandler;
        }

        public AccessKey CheckKeyPresence()
        {
            if (usbManager != null)
            {

                var disks = UsbManager.GetAvailableDisks();
                return findKey(disks);

            }
            return null;
        }

        public UsbStateChange UsbStateChange { get; set; }

        protected void OnUsbStateChangedEventHandler(UsbStateChangedEventArgs usbState)
        {
            UsbStateChange = usbState.State;
            EventHandler<AccessKey> handler = UsbKeyStateChanged;
            if (handler != null)
            {
                handler(this, findKey(usbState.Disks));
            }
        }

        public static async Task<bool> DestroyLicence(AccessKey loggedUser)
        {
            var allDisks = await Task.Run(() => UsbManager.GetAvailableDisks());
            if (allDisks != null)
            {
                foreach (var disk in allDisks)
                {
                    var file = Path.Combine(string.Format("{0}{1}", disk.DriveLetter, Path.DirectorySeparatorChar), GeneratedKeyName);
                    if (File.Exists(file))
                    {
                        var license = await Task.Run(() => AccessKeySerializer.DecryptAccessKeyFromFile(file));
                        if (license != null && license.UsbID == loggedUser.UsbID && license.HwId == loggedUser.HwId)
                        {
                            try
                            {
                                File.Delete(file);
                                return true;
                            }
                            catch (Exception ex)
                            {
                                SystemLog.LogException(ex);
                            }
                            break;
                        }
                    }
                }
            }
            return false;
        }


        private AccessKey findKey(UsbKeysCollection disks)
        {
            if (disks != null)
            {
                List<AccessKey> accessKeys = new List<AccessKey>();
                bool usbKeyValid = false;
                var tokenSource = new CancellationTokenSource();
                var token = tokenSource.Token;
                Task[] tasks = new Task[disks.Count];
                for (int i = 0; i < disks.Count; i++)
                {
                    tasks[i] = Task.Factory.StartNew(
                        (o) =>
                        {
                            var j = (int)o;
                            var file = Path.Combine(disks[j].DriveLetter + "\\", GeneratedKeyName);
                            if (File.Exists(file))
                            {
                                AccessKey accessKey = AccessKeySerializer.DecryptAccessKeyFromFile(file);
                                if (accessKey != null)
                                {
                                    accessKeys.Add(accessKey);
                                    if ((disks[j].DeviceID == accessKey.UsbID) || (disks[j].HardwareID == accessKey.HwId))
                                    {
                                        usbKeyValid = true;
                                        accessKey.IsUsbIdValid = true;
                                        if (tokenSource != null)
                                        {
                                            tokenSource.Cancel();
                                        }
                                        //return accessKey;
                                    }
                                }
                            }

                        }, i, token);

                }
                try
                {
                    Task.WaitAll(tasks, 5000, token);
                }
                catch
                {
                    //Cancel exception
                }
                finally
                {
                    lock (tokenSource)
                    {
                        tokenSource.Dispose();
                        tokenSource = null;
                    }

                }
                if (accessKeys.Count > 0)
                {
                    return accessKeys[0];
                }
            }

            return null;
        }

        public void Dispose()
        {
            usbManager.Dispose();
        }
    }
}
