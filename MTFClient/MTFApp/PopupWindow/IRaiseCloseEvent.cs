using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFApp.PopupWindow
{
    public delegate void CloseEventHandler(object sender);
    public interface IRaiseCloseEvent
    {
        event CloseEventHandler Close;
    }
}
