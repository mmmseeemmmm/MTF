using System;
using System.Threading;
using System.Windows.Threading;
using MTFClientServerCommon.MTFAccessControl.NamedPipe;

namespace MTFApp.UIHelpers
{
    public class LicenseDestroyManager
    {
        private int lastTimeStamp;
        private readonly DispatcherTimer timer = new DispatcherTimer();
        private const int MaxMinutesInactivity = 120;
        //private const int NoticeInActivity = 110;
        private bool responseReached;
        private bool canDestroyFromRemote;
        private bool popupIsOpen;

        public EventHandler<EventArgs> OnRemove;

        public LicenseDestroyManager()
        {
            timer.Interval = new TimeSpan(0, 0, 10, 0, 0);
            timer.Tick += TimerTick;
            timer.Start();
            lastTimeStamp = Environment.TickCount;
            CreateServer();
        }

        private void PipeSeverDataReceived(object sender, PipeEventArgs e)
        {
            if (e.Data.Type == DataType.Request)
            {
                PipeClient.SendMessage(PipeEnvironment.EOL, new PipeData {Data = true, Type = DataType.CanDestroy});
            }
            else
            {
                responseReached = true;
                canDestroyFromRemote = e.Data.Data;
            }
        }

        private void PipeSeverConnected(object sender, EventArgs e)
        {
            CreateServer();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            if (EnvironmentHelper.CurrentAccessKey != null && !EnvironmentHelper.HasAccessKeyRole("noDestroy"))
            {
                var inactivity = GetInActivity(Environment.TickCount);
                //if (!popupIsOpen && inactivity.Minutes >= NoticeInActivity && inactivity.Minutes < MaxMinutesInactivity)
                //{
                //    MTFMessageBox.ShowNonBlockingMessage(LanguageHelper.GetString("Msg_Header_Inactivity"),
                //        string.Format(LanguageHelper.GetString("Msg_Body_Inactivity"), MaxMinutesInactivity / 60,
                //            MaxMinutesInactivity - inactivity.Minutes),
                //        MTFMessageBoxType.Warning,
                //        MTFMessageBoxButtons.Ok, OnPopupClose);
                //    popupIsOpen = true;
                //}
                if (inactivity.Minutes > MaxMinutesInactivity)
                {
                    responseReached = false;
                    canDestroyFromRemote = false;
                    bool success = PipeClient.SendRequest(PipeEnvironment.EOL);
                    if (success)
                    {
                        int count = 0;
                        while (!responseReached && count < 10)
                        {
                            Thread.Sleep(500);
                            count++;
                        }
                    }
                    if (!responseReached || canDestroyFromRemote)
                    {
                        DestroyLicense();
                    }
                }
            }
        }

        //private void OnPopupClose()
        //{
        //    popupIsOpen = false;
        //}

        private void CreateServer()
        {
            var pipeSever = new PipeServer(PipeEnvironment.MTF);
            pipeSever.Connected += PipeSeverConnected;
            pipeSever.DataReceived += PipeSeverDataReceived;
        }

        private async void DestroyLicense()
        {
            var accessKey = EnvironmentHelper.CurrentAccessKey;
            if (accessKey != null)
            {
                var result = await MTFClientServerCommon.MTFAccessControl.USBAccessKeyHandler.DestroyLicence(accessKey);
                if (result && OnRemove != null)
                {
                    PipeClient.SendMessage(PipeEnvironment.EOL, new PipeData {Data = true, Type = DataType.ReleaseLicense});
                    OnRemove(this, EventArgs.Empty);
                }
            }
        }

        private TimeSpan GetInActivity(int tickCount)
        {
            int diff;
            if (tickCount < lastTimeStamp)
            {
                diff = (int.MaxValue - lastTimeStamp) + Math.Abs(int.MinValue - tickCount);
            }
            else
            {
                diff = tickCount - lastTimeStamp;
            }
            return new TimeSpan(0, 0, 0, 0, diff);
        }

        public void Set(int timestamp)
        {
            lastTimeStamp = timestamp;
        }
    }
}