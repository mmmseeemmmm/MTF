using System;
using System.Runtime.InteropServices;

namespace MTFClientServerCommon.MTFAccessControl.NamedPipe
{
    public static class NamedPipeHelper
    {
        public static string GetServerName(PipeEnvironment env)
        {
            switch (env)
            {
                case PipeEnvironment.MTF:
                    return "MTFLicencePipe";
                case PipeEnvironment.EOL:
                    return "EOLLicencePipe";
                default:
                    throw new ArgumentOutOfRangeException("env", env, null);
            }
        }

        public static byte[] GetBytes(PipeData data)
        {
            int size = Marshal.SizeOf(data);
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(data, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            return arr;
        }

        public static PipeData GetDataFromBytes(byte[] receiveByteArray)
        {
            PipeData str = new PipeData();
            int size = Marshal.SizeOf(str);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(receiveByteArray, 0, ptr, size);
            str = (PipeData)Marshal.PtrToStructure(ptr, str.GetType());
            Marshal.FreeHGlobal(ptr);
            return str;
        }
    }


}
