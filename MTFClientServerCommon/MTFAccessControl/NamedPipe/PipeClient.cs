using System;
using System.IO.Pipes;

namespace MTFClientServerCommon.MTFAccessControl.NamedPipe
{
    public static class PipeClient
    {
        public static bool SendRequest(PipeEnvironment target)
        {
            var clientPipeStream = new NamedPipeClientStream(".", NamedPipeHelper.GetServerName(target));
            try
            {
                clientPipeStream.Connect(1000);
                byte[] buffer = NamedPipeHelper.GetBytes(new PipeData { Type = DataType.Request });
                clientPipeStream.BeginWrite(buffer, 0, buffer.Length, AsyncSend, clientPipeStream);
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public static bool SendMessage(PipeEnvironment target, PipeData data)
        {
            var clientPipeStream = new NamedPipeClientStream(".", NamedPipeHelper.GetServerName(target));
            try
            {
                clientPipeStream.Connect(1000);
                byte[] buffer = NamedPipeHelper.GetBytes(data);
                clientPipeStream.BeginWrite(buffer, 0, buffer.Length, AsyncSend, clientPipeStream);
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        private static void AsyncSend(IAsyncResult ar)
        {
            var pipeStream = (NamedPipeClientStream)ar.AsyncState;
            if (pipeStream != null)
            {
                try
                {
                    pipeStream.EndWrite(ar);
                    pipeStream.Flush();
                }
                catch (Exception ex)
                {
                    SystemLog.LogException(ex);
                }

                pipeStream.Close();
                pipeStream.Dispose();
            }
        }


    }
}
