using AutomotiveLighting.MTFCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NationalInstruments.NI4882;
using PowerSupplyExtensionMethods;

namespace PowerSupply
{
    class SyskonGPIB:SyskonBase
    {

        private int boardNumber;
        private byte adress;

        private Device device = null;


        public SyskonGPIB(int boardNumber, byte adress)
        {
            this.boardNumber = boardNumber;
            this.adress = adress;
        }

 
        public override void Init()
        {
            try
            {
                device = new Device(this.boardNumber, this.adress);
                this.connectionSucceed = true;
                device.SetEndOnWrite = true;
                device.EndOfStringCharacter = 0xA;
                //device.
                //device.SynchronizeCallbacks = true;

                this.identification = DoInit();
                 
                

            }
            catch (Exception e)
            {
                this.connectionSucceed = false;
                throw new Exception("Communication failed: " + e.ToString());
            }
        }

        public override void Rst()
        {
            SendCmd("*RST");
            
        }

        public override void Write(string command)
        {
            device.Write(command);
            System.Threading.Thread.Sleep(100);
        }

        public override string Read()
        {
            return device.ReadString().Trim('\n');
        }
       


        public override void Dispose()
        {
            
            if (device != null)
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
                    device.Dispose();
                    device = null;
                }
            }
        }

        ~SyskonGPIB()
        {
            this.Dispose();
             
        }




    }
}
