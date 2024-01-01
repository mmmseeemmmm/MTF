using System;
using System.IO;
using System.IO.Pipes;
using System.Text;

namespace MTFClientServerCommon.MTFAccessControl.NamedPipe
{
    public class PipeServer
    {
        public event EventHandler<EventArgs> Connected;
        private readonly NamedPipeServerStream serverPipeStream;
        public event EventHandler<PipeEventArgs> DataReceived;

        public PipeServer(PipeEnvironment env)
        {
            serverPipeStream = new NamedPipeServerStream(
                NamedPipeHelper.GetServerName(env),
                PipeDirection.InOut,
                NamedPipeServerStream.MaxAllowedServerInstances,
                PipeTransmissionMode.Message,
                PipeOptions.Asynchronous);
            serverPipeStream.BeginWaitForConnection(PipeConnectedCallBack, null);
        }

        private void PipeConnectedCallBack(IAsyncResult ar)
        {
            serverPipeStream.EndWaitForConnection(ar);
            if (Connected != null)
            {
                Connected(this, EventArgs.Empty);
            }

            StreamReader reader = new StreamReader(serverPipeStream);
            var message = reader.ReadToEnd();
            byte[] bytes = Encoding.ASCII.GetBytes(message);
            
            var data = NamedPipeHelper.GetDataFromBytes(bytes);
            if (DataReceived!=null)
            {
                DataReceived(this, new PipeEventArgs(data));
            }
        }
        
    }

    public enum PipeEnvironment
    {
        MTF,
        EOL,
    }
}
