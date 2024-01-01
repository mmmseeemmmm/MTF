using MTFClientServerCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFApp.PopupWindow
{
    public interface IReturnsDialogResult
    {
        MTFDialogResult DialogResult
        {
            get;
        }
    }
}
