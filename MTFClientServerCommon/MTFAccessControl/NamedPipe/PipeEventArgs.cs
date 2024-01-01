using System;
using System.Runtime.InteropServices;

namespace MTFClientServerCommon.MTFAccessControl.NamedPipe
{
    public class PipeEventArgs : EventArgs
    {
        public PipeData Data { get; private set; }

        public PipeEventArgs(PipeData data)
        {
            Data = data;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PipeData
    {
        public DataType Type { get; set; }
        public bool Data { get; set; }
    }

    public enum DataType : byte
    {
        Request,
        CanDestroy,
        ReleaseLicense,
    }
}
