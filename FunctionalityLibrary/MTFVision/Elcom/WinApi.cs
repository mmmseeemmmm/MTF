using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MTFVision
{
    public static class WinApi
    {
        public static IDictionary<IntPtr, string> GetOpenWindows()
        {
            IntPtr shellWindow = GetShellWindow();
            Dictionary<IntPtr, string> windows = new Dictionary<IntPtr, string>();

            EnumWindows(delegate(IntPtr hWnd, int lParam)
            {
                if (hWnd == shellWindow) return true;
                if (!IsWindowVisible(hWnd)) return true;

                int length = GetWindowTextLength(hWnd);
                if (length == 0) return true;

                StringBuilder builder = new StringBuilder(length);
                GetWindowText(hWnd, builder, length + 1);

                windows[hWnd] = builder.ToString();
                return true;

            }, 0);

            return windows;
        }

        public static void SetWindowToForeground(string prefix, string sufix, int timeoutMs)
        {
            Task.Run(() =>
            {
                IEnumerable<KeyValuePair<IntPtr, string>> hwnds = null;
                int i = 0;
                while (hwnds == null || hwnds.Count() == 0)
                {
                    if (i > 5)
                    {
                        return;
                    }
                    System.Threading.Thread.Sleep(timeoutMs / 5);
                    var openedWindows = GetOpenWindows();
                    hwnds = openedWindows.Where(f => f.Value.StartsWith(prefix) && f.Value.EndsWith(sufix));
                    i++;
                }
                foreach (var valuePair in hwnds)
                {
                    SetWindowToForeground(valuePair.Key);
                    System.Threading.Thread.Sleep(100);
                }
            });
        }

        public static void SetWindowToForeground(IntPtr hwnd)
        {
            //SetWindowPos(hwnd, new IntPtr(-1), 0, 0, 0, 0, 0x1 | 0x2); //this method set windows to most foreground nothing will ever go infront of this window
            SetForegroundWindow(hwnd);
        }

        private delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetShellWindow();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32")]
        private static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwnd2, int x, int y, int cx, int cy, int flags);

    }
}
