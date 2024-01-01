using System;
using System.Drawing;

namespace MTFClientServerCommon.GraphicalView
{
    public class BitmapInfo : IDisposable
    {
        private readonly Bitmap b;
        private readonly string name;

        public BitmapInfo(Bitmap b, string name)
        {
            this.b = b;
            this.name = name;
        }

        public Bitmap Bitmap => b;

        public string Name => name;

        public void Dispose()
        {
            b?.Dispose();
        }
    }
}
