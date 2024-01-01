using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ALStdHostCommon;

namespace ALStdComponent
{
    public class ALStdControl : IDisposable
    {
        private bool disposed;
        private IStdHost stdHost;

        public ALStdControl()
        {
            //AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            var alBusServers = Process.GetProcesses().Where(p => p.ProcessName.StartsWith("ALStdHost"));
            foreach (var server in alBusServers)
            {
                server.Kill();
            }

            ALStdHostHelper.StartALStdHostServer(@"mtfLibs\ALStdComponent\ALStdHost");
            stdHost = ALStdHostHelper.Connect();
            stdHost.CreateInstance("PrototypeAssembly", "");
            stdHost.GetVideoModuleDriver();
        }

        //System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        //{
        //    return null;
        //}

        public void SetPicture(VmdPictures vmdPicture, HsvlMode hsvlMode)
        {
            if (stdHost.VideoModuleDriverGetNamedValue("Power_Supply", "CTRL_Enable") == 0)
            {
                stdHost.VideoModuleDriverSetNamedValue("Power_Supply", "CTRL_Enable", 1);
                System.Threading.Thread.Sleep(1000);
            }

            stdHost.VideoModuleDriverSetPicture(vmdPicture, hsvlMode);
        }

        public void Stop()
        {
            stdHost.VideoModuleDriverSetNamedValue("Serializer_1", "CTRL_Enable", 0);
            stdHost.VideoModuleDriverSetNamedValue("Power_Supply", "CTRL_Enable", 0);
        }

        public void Dispose()
        {
            this.Stop();
            this.Dispose(true);



            GC.SuppressFinalize(this);
        }

        ~ALStdControl()
        {
            this.Dispose(false);
        }

        protected void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                // Managed code
            }

            // Unmanaged code
            stdHost.DestroyInstance();
            ALStdHostHelper.StopALStdHostServer();
        }
    }
}
