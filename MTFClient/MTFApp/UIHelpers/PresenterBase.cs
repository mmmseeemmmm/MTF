using MTFClientServerCommon;
using System;

namespace MTFApp.UIHelpers
{
    public class PresenterBase : NotifyPropertyBase
    {
        private readonly Guid id = Guid.NewGuid();

        public static bool ShowDebugInfo => true;


        protected MTFClient MTFClient
        {
            get
            {
                var client = MTFClient.GetMTFClient();
                if (client != null)
                {
                    return client;
                }

                if (SettingsClass.SelectedConnection == null || string.IsNullOrEmpty(SettingsClass.SelectedConnection.Host) ||
                    string.IsNullOrEmpty(SettingsClass.SelectedConnection.Port))
                {
                    return null;
                }

                return MTFClient.GetMTFClient(SettingsClass.SelectedConnection.Host, SettingsClass.SelectedConnection.Port);
            }
        }

        public Guid Id => id;

        public static readonly Guid DebugLoggingEnabledId = new Guid("098FADDE-F707-4F06-8210-812B448F276A");

        protected void AddDebugLoggingEnabledWarning()
        {
            Warning.Add(new WarningMessage
                        {
                            Id = DebugLoggingEnabledId,
                            Message =
                                "Debug logging is enabled - sequence execution may be slow!\nYou can switch it off in dialog Settings -> Logging settings -> Allow debug logging"
                        });
        }

        protected void RemoveDebugLoggingEnabled()
        {
            Warning.Remove(DebugLoggingEnabledId);
        }

        public virtual void Activated()
        {
        }
    }
}