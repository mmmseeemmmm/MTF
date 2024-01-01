using AutomotiveLighting.MTFCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using PowerSupplyExtensionMethods;

namespace PowerSupply
{
    
    class SyskonSerial : SyskonBase
    {
        private int timeOut = 1000;
        protected string comPort;
        protected int baudRate=-1;
        private System.IO.Ports.SerialPort serialPort = null;

        
        public SyskonSerial(string comPort, int baudRate)
        {
            this.comPort = comPort;
            this.baudRate = baudRate;
            

        }
        
        public override void Init()
        {
            if ((comPort != null) && (comPort != string.Empty) && (baudRate != -1))
            {

                InitSerialPort(this.comPort, this.baudRate);
                this.connectionSucceed = true;

                identification = DoInit();

            }
            else
            {
                throw new Exception("Communication parameters are not set");
            }
        }

        public override void Rst()    
        {
            SendCmd("*RST");
            
        }

        private void InitSerialPort(string comPort, int baudRate)
        {
           
            try
            {
                this.serialPort = SerialCommunication.GetSerialPort(this.comPort, this.baudRate);//new SerialPort();
                //this.serialPort.PortName = comPort;
                //this.serialPort.BaudRate = baudRate;
                this.serialPort.WriteTimeout = this.timeOut;
                this.serialPort.ReadTimeout = this.timeOut;
                //this.serialAdress = "ADR " + Convert.ToString(serialAdress);
                this.serialPort.NewLine = "\r";
                //this.serialPort.Open();


            }
            catch (Exception e)
            {
                throw new Exception("Serial port was not initialized:" + e.ToString());

            }
      
            
        }


        public override void Dispose()
        {
            
            if (serialPort != null) 
            {

                try
                {
                    DoClose();
                }
                catch
                {

                }
                finally
                {
                    SerialCommunication.CloseSerailPort(this.comPort);
                    //serialPort.Close();
                    serialPort = null;
                }
            }

        }


        ~SyskonSerial()
        {
            this.Dispose();
             
        }
                


        public override void Write(string command)
        {
            serialPort.WriteLine(command);
            System.Threading.Thread.Sleep(100);
        }

        public override string Read()
        {
            return serialPort.ReadLine();
        }
    }
}
