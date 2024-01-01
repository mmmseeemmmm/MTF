using System;
using System.IO;
using System.Linq;
using System.ServiceModel;
using AL.Utils.Logging;
using ALStdHostCommon;
using ALStdWrapperNamespace;
using ALStdWrapperNamespace.Drivers;
using DriverType = ALStdHostCommon.DriverType;

namespace ALStdHost
{
    class StdHost : IStdHost
    {
        private ALStd alStdInstance;
        private VideoModuleDriver videoModuleDriver;
        public void CreateInstance(string alStdAssembly, string workingDirectory)
        {
            
            var dir = !string.IsNullOrEmpty(workingDirectory) ? Path.GetFullPath(workingDirectory) : String.Empty;
            Log.LogMessage(string.Format("Creating AlStd instance, alStdAssembly= {0} workingDirectory = {1}" , alStdAssembly, dir), true);
            try
            {
                alStdInstance = new ALStd(alStdAssembly, dir);
            }
            catch (Exception e)
            {
                Log.LogMessage("Creating of instance crashed", true);
                Log.LogMessage(e, true);

                throw new FaultException(e.Message);
            }
        }

        public void GetVideoModuleDriver()
        {
            try
            {
                Log.LogMessage(string.Format("Getting video module driver."), true);
                videoModuleDriver = (VideoModuleDriver)alStdInstance.Drivers.FirstOrDefault(d => d.DriverInfo.DriverType == ALStdWrapperNamespace.Drivers.DriverType.Vmd);
            }
            catch (Exception e)
            {
                Log.LogMessage("Creating of instance crashed", true);
                Log.LogMessage(e, true);

                throw new FaultException(e.Message);
            }
        }

        public byte VideoModuleDriverGetNamedValue(string parentName, string parameterName)
        {
            Log.LogMessage(string.Format("VideoModuleDriverGetNamedValue, parentName= {0} parameterName = {1}", parentName, parameterName), true);
            try
            {
                return (byte)videoModuleDriver.GetNamedValue(parentName, parameterName);
            }
            catch (Exception e)
            {
                Log.LogMessage("VideoModuleDriverGetNamedValue crashed", true);
                Log.LogMessage(e, true);

                throw new FaultException(e.Message);
            }
        }        
        
        public void VideoModuleDriverSetNamedValue(string parentName, string parameterName, byte value)
        {
            Log.LogMessage(string.Format("VideoModuleDriverSetNamedValue, parentName= {0} parameterName = {1} value = {2}", parentName, parameterName, value), true);
            try
            {
                videoModuleDriver.SetNamedValue(parentName, parameterName, value);
            }
            catch (Exception e)
            {
                Log.LogMessage("VideoModuleDriverSetNamedValue crashed", true);
                Log.LogMessage(e, true);

                throw new FaultException(e.Message);
            }
        }

        public void VideoModuleDriverSetPicture(ALStdHostCommon.VmdPictures pictureName, ALStdHostCommon.HsvlMode hsvlMode)
        {
            Log.LogMessage(string.Format("VideoModuleDriverSetPicture, pictureName= {0} hsvlMode = {1}", pictureName, hsvlMode), true);
            try
            {
                videoModuleDriver.SetPicture(ConvertVmdPicture(pictureName), ConvertHsvlMode(hsvlMode) );
            }
            catch (Exception e)
            {
                Log.LogMessage("VideoModuleDriverSetPicture crashed", true);
                Log.LogMessage(e, true);

                throw new FaultException(e.Message);
            }
        }

        public void DestroyInstance()
        {
            Log.LogMessage("Entering DestroyInstance");

            try
            {
                alStdInstance.Dispose();
            }
            catch (Exception ex)
            {
                Log.LogMessage("Exception during DestroyInstance - " + ex.Message);
            }

            Log.LogMessage("Leaving DestroyInstance");
        }

        public string Ping()
        {
            return "OK";
        }

        private ALStdWrapperNamespace.Drivers.DriverType ConvertDriverType(DriverType driverType)
        {
            switch (driverType)
            {
                case DriverType.Flmd: return ALStdWrapperNamespace.Drivers.DriverType.Flmd;
                case DriverType.Vmd: return ALStdWrapperNamespace.Drivers.DriverType.Vmd;
                default: return ALStdWrapperNamespace.Drivers.DriverType.NotSet;
            }
        }

        private ALStdWrapperNamespace.Drivers.VmdPictures ConvertVmdPicture(ALStdHostCommon.VmdPictures picture)
        {
            switch (picture)
            {
                case ALStdHostCommon.VmdPictures.AL: return ALStdWrapperNamespace.Drivers.VmdPictures.AL;
                case ALStdHostCommon.VmdPictures.White: return ALStdWrapperNamespace.Drivers.VmdPictures.White;
                case ALStdHostCommon.VmdPictures.Black: return ALStdWrapperNamespace.Drivers.VmdPictures.Black;
                case ALStdHostCommon.VmdPictures.BlackWhite: return ALStdWrapperNamespace.Drivers.VmdPictures.BlackWhite;
                case ALStdHostCommon.VmdPictures.WhiteBlack: return ALStdWrapperNamespace.Drivers.VmdPictures.WhiteBlack;
                case ALStdHostCommon.VmdPictures.LowBeam: return ALStdWrapperNamespace.Drivers.VmdPictures.LowBeam;
                case ALStdHostCommon.VmdPictures.Chessboard01: return ALStdWrapperNamespace.Drivers.VmdPictures.Chessboard01;
                case ALStdHostCommon.VmdPictures.Chessboard02: return ALStdWrapperNamespace.Drivers.VmdPictures.Chessboard02;
                case ALStdHostCommon.VmdPictures.Chessboard03: return ALStdWrapperNamespace.Drivers.VmdPictures.Chessboard03;
                case ALStdHostCommon.VmdPictures.Error: return ALStdWrapperNamespace.Drivers.VmdPictures.Error;
                case ALStdHostCommon.VmdPictures.Edge: return ALStdWrapperNamespace.Drivers.VmdPictures.Edge;
                case ALStdHostCommon.VmdPictures.NotFound: return ALStdWrapperNamespace.Drivers.VmdPictures.NotFound;
                case ALStdHostCommon.VmdPictures.ShadeOfGreys: return ALStdWrapperNamespace.Drivers.VmdPictures.ShadeOfGreys;
                case ALStdHostCommon.VmdPictures.DlpAdjustmentRhs: return ALStdWrapperNamespace.Drivers.VmdPictures.DlpAdjustmentRhs;
                case ALStdHostCommon.VmdPictures.DlpAdjustmentLhs: return ALStdWrapperNamespace.Drivers.VmdPictures.DlpAdjustmentLhs;
                default: return ALStdWrapperNamespace.Drivers.VmdPictures.None;
            }
        }

        private ALStdWrapperNamespace.Drivers.HsvlMode ConvertHsvlMode(ALStdHostCommon.HsvlMode mode)
        {
            switch (mode)
            {
                case ALStdHostCommon.HsvlMode.Crc6Bit: return ALStdWrapperNamespace.Drivers.HsvlMode.Crc6Bit;
                default: return ALStdWrapperNamespace.Drivers.HsvlMode.Par1Bit;
            }

        }
    }
}
