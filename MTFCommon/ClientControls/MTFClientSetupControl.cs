using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFCommon.ClientControls
{
    public class MTFClientSetupControl : MTFClientControlBase
    {
        public event OnCloseEventHandler OnClose;
        public delegate void OnCloseEventHandler(object sender);
        protected void Close()
        {
            if (OnClose != null)
            {
                OnClose(this);
            }
        }
    }
}
