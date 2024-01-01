using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace MTFClientServerCommon.MTFAccessControl
{
    public abstract class AccessKeyProvider : IDisposable
    {
        public delegate void NewAccessKeyHandler(object sender, AccessKey accessKey);
        public event NewAccessKeyHandler NewAccessKey;
        
        public delegate void OnErrorHandler(object sender, string message);
        public event OnErrorHandler OnError;

        public delegate bool ShowMessageDelegate(string header, string message, bool questionTypeMessage);
        public ShowMessageDelegate ShowMessage;

        public bool IsActive { get; set; }
        public string Name { get; set; }

        public abstract void Init();
        public abstract void Destroy();

        public virtual bool HasConfigControl { get { return false; } }

        public virtual UserControl ConfigControl { get { return null; } }

        public virtual bool OpenConfigControl { get; set; }

        public virtual bool CanReconnect
        {
            get { return false; }
        }

        public virtual void Reconnect()
        {
            Destroy();
            Init();
        }

        protected void SetAccessKey(AccessKey accessKey)
        {
            if (NewAccessKey != null)
            {
                NewAccessKey(this, accessKey);
            }
        }

        protected void RaiseError(string message)
        {
            if (OnError != null)
            {
                OnError(this, message);
            }
        }

        public List<AccessKeyProviderParameter> Parameters;

        public void Dispose()
        {
            Destroy();
        }
    }

    public class AccessKeyProviderParameter
    {
        public string Name { get; set; }
        public string Value { get; set; }        
    }
}
