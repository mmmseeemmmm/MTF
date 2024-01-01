using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ALBusServer
{
    class Restbus3Wrapper : ALRestbusVCF.ICommunicationObject
    {
        private ALRestbus.ALRestbus3Net restbus3ComObj;
        public Restbus3Wrapper(string deviceName, string busType, string busChannel, string netCfgFile, string busNodes, string customCfgFile, string linScheduleTable, string linMasterNode, string clusterName, bool continueOnError)
        {
            restbus3ComObj = ALRestbus.ALRestbus3Net.VectorInit(Restbus3BusType(busType),
                deviceName,
                Restbus3HwChannel(busChannel),
                netCfgFile, Restbus3DllPath(), Restbus3FibexDll(), busNodes,
                linScheduleTable, clusterName, customCfgFile);            

            restbus3ComObj.Start();
        }

        public void Dispose()
        {
            restbus3ComObj.Stop();
        }

        public void Start()
        {
            restbus3ComObj.TransmitControl(true);
        }

        public void Stop()
        {
            restbus3ComObj.TransmitControl(false);
        }

        public void ActivateTable(string scheduleTableName)
        {
            restbus3ComObj.ActivateTable(scheduleTableName);
        }

        public void SetSignal(string frameName, string signalName, string signalValue)
        {
            restbus3ComObj.SetSignal(frameName, signalName, signalValue);
        }

        public void GetSignal(string frameName, string signalName, ref string signalValue, ref string signalUnit)
        {
            restbus3ComObj.GetSignal(frameName, signalName, ref signalValue, ref signalUnit);
        }

        public void SendFrameOnce(uint frameId, byte[] data, uint dataLength)
        {
            restbus3ComObj.SendFrameOnce(frameId, data.Select(d => (int)d).ToList());
        }

        public void SendFrameOnce(uint frameId, byte[] data, uint dataLength, uint flag)
        {
            throw new NotImplementedException();
        }

        public void ReceiveFrameOnce(uint frameId, byte[] responseData, ref uint responseDataLength, uint timeout)
        {
            throw new NotImplementedException();
        }

        public void ReceiveFrameOnce(uint frameId, byte[] responseData, ref uint responseDataLength, uint timeout, uint flag)
        {
            throw new NotImplementedException();
        }

        public void ReceiveFrameOnce(uint frameId, uint responseFrameId, byte[] data, uint dataLengt, byte[] responseData,
            ref uint responseDataLength, uint timeout)
        {
            throw new NotImplementedException();
        }

        public void ReceiveFrameOnce(uint frameId, uint responseFrameId, byte[] data, uint dataLengt, byte[] responseData,
            ref uint responseDataLength, uint timeout, uint flag)
        {
            throw new NotImplementedException();
        }

        public void ReceiveFrameOnce(uint frameId, uint responseFrameId, byte[] data, ref byte[] responseData, uint timeout)
        {
            throw new NotImplementedException();
        }

        public void SetGlobalVariable(string variableName, int variableValue)
        {
            restbus3ComObj.SetVariable(variableName, variableValue.ToString());
        }

        public void GetGlobalVariable(string variableName, ref byte[] variableValueBuffer)
        {
            throw new NotImplementedException();
        }

        public void GetGlobalVariableString(string variableName, ref string variableValue)
        {
            restbus3ComObj.GetVariable(variableName, ref variableValue);
        }

        public string GetStdOut()
        {
            throw new NotImplementedException();
        }

        public List<string> GetOutputSignals()
        {
            throw new NotImplementedException();
        }

        public List<string> GetInputSignals()
        {
            throw new NotImplementedException();
        }

        public bool IsBusCommunication()
        {
            return true;
        }

        public bool IsHSXReady()
        {
            return true;
        }

        public void SendCANFDFrameCyclic(uint frameId, List<List<int>> data, uint timeOutFrame, uint timeOutSubList)
        {
            restbus3ComObj.SendCANFDFrameCyclic(frameId, data, timeOutFrame, timeOutSubList);
        }

        public void SendCANFDFrameCyclic(uint frameId, List<int> data, uint timeOutFrame)
        {
            restbus3ComObj.SendCANFDFrameCyclic(frameId, data, timeOutFrame);
        }

        public void SendFrameCyclic(uint frameId, List<List<int>> data, uint timeOutFrame, uint timeOutSubList)
        {
            restbus3ComObj.SendFrameCyclic(frameId, data, timeOutFrame, timeOutSubList);
        }

        public void SendFrameCyclic(uint frameId, List<int> data, uint timeOutFrame)
        {
            restbus3ComObj.SendFrameCyclic(frameId, data, timeOutFrame);
        }

        public void SetGlobalProjectVariable(string variableName, string value)
        {
            restbus3ComObj.setGlobalProjectVariable(variableName, value);
        }

        public string GetGlobalProjectVariable(string variableName)
        {
            string value = string.Empty;
            restbus3ComObj.getGlobalProjectVariable(variableName, ref value);
            return value;
        }

        private UInt16 Restbus3BusType(string busType)
        {
            if (busType.ToUpper() == "CAN")
            {
                return 1;
            }
            if (busType.ToUpper() == "LIN")
            {
                return 2;
            }

            throw new Exception(string.Format("Unknow bus type {0}", busType));
        }

        //private UInt16 Restbus3HwType()
        //{
        //    return 57;
        //}

        //private UInt16 Restbus3HwIndex()
        //{
        //    return 0;
        //}

        private UInt16 Restbus3HwChannel(string channel)
        {
            UInt16 channelNumber = 0;
            if (!UInt16.TryParse(channel, out channelNumber))
            {
                throw new Exception(string.Format("Incorect channel {0}. Channel must be number!", channel));
            }

            if (channelNumber == 0)
            {
                throw new Exception("Channel number must be grater than 0.");
            }

            return (UInt16)(UInt16.Parse(channel) - 1);
        }

        private string Restbus3DllPath()
        {
            return "ALRestbus3.dll";
        }

        private string Restbus3FibexDll()
        {
            return "ImportFibex.dll";
        }
    }
}
