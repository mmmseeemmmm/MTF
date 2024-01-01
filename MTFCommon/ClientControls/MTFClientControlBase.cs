using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MTFCommon.ClientControls
{
    public abstract class MTFClientControlBase : UserControl, IDisposable
    {
        public event OnSendDataEventHandler OnSendData;
        public delegate void OnSendDataEventHandler(object sender, object data, string dataName);

        public delegate bool HasUserRoleDelegate(string roleName);
        public HasUserRoleDelegate HasUserRole;

        public virtual void OnReceiveData(object data, string dataName)
        {
            
        }

        protected void SendData(object data, string dataName)
        {
            if (OnSendData != null)
            {
                OnSendData(this, data, dataName);
            }
        }

        public virtual void OnAccessKeyChanged(MTFAccessKey accessKey)
        {
        }

        public virtual void OnLanguageChanged(string language)
        {

        }

        public virtual void Dispose()
        {
        }

        public string AssemblyName { get; set; }
        public string TypeName { get; set; }
    }
}
