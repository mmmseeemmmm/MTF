using AutomotiveLighting.MTFCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using PowerSupplyExtensionMethods;

namespace PowerSupply
{
    class LambdaTCP:LambdaBase
    {
        private string ipAdress;
        private int port;

        private static TcpClient client = null;
        private static NetworkStream stream = null;


        public LambdaTCP(string ipAdress, int port)
        {
            this.ipAdress = ipAdress;
            this.port = port;

        }


        public override void Init()
        {
            try
            {
                client = new TcpClient(this.ipAdress, this.port);
                this.connectionSucceed = true;
                stream = client.GetStream();
                
                //2s delay
                

                Write("*IDN?");
                identification = Read();
                maxPower = identification.GetPower();
                power = maxPower;
                //Volt = 0;
                //Current = 0;
                Output = false;
                SendCmd("OUTP:STAT OFF");

            }

            catch (Exception e)
            {
                this.connectionSucceed = false;
                throw new Exception("Communication failed: " + e.ToString());
            }

        }

        public override void Rst()
        {
            lock (PowerSupply.locker)
            {
                SendCmd("*RST");
                maxPower = identification.GetPower();
                power = maxPower;
            }
        }


        public  void Write(string cmd)
        {
            

            Byte[] data = System.Text.Encoding.ASCII.GetBytes(cmd);
          
            // Send the message to the connected TcpServer. 
            if (stream.CanRead)
            {
                stream.Write(data, 0, data.Length);
                System.Threading.Thread.Sleep(100);

            }
            else
            {
                throw new Exception("Command send failed");

            }
        }

        public string Read()
        {
            StringBuilder identification = new StringBuilder();


            //SendCmdTCP(query);

            if (stream.CanRead)
            {
                byte[] myReadBuffer = new byte[1024];

                int numberOfBytesRead = 0;

                // Incoming message may be larger than the buffer size. 
                do
                {
                    numberOfBytesRead = stream.Read(myReadBuffer, 0, myReadBuffer.Length);

                    identification.AppendFormat("{0}", Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));

                }
                while (stream.DataAvailable);


                return identification.ToString();

            }
            else
            {

                throw new Exception("Read Response Failed");
            }


        }


        public override double Volt
        {
            get
            {
                return SendQuery(":VOLT?").GetDouble();
            }

            set
            {

                if (current * value <= power)
                {

                    SendCmd(":VOLT " + value.FormatIntoString());
                    volt = Volt;
                }

                else
                {
                    throw new Exception("Power Limit Exceeded");
                }
                
            }
        }

        public override double Current
        {
            get
            {
                return SendQuery(":CURR?").GetDouble();
            }

            set
            {
                if (value * volt <= power)
                {

                    SendCmd(":CURR " + value.FormatIntoString());
                    current = Current;
                }
                else
                {
                    throw new Exception("Power Limit Exceeded");
                }
                
            }
        }


        public override bool Output
        {

            get
            {
                string s=SendQuery("OUTP:STAT?");
                if(s=="ON")
                {
                    return true;
                }
                else if (s == "OFF")
                {
                    return false;
                }
                else
                {
                    throw new Exception("Wrong response obtained");
                }
            }

            set
            {
                if (value)
                {
                    SendCmd("OUTP:STAT ON");
                }
                else 
                {
                    SendCmd("OUTP:STAT OFF");
                }
            }

        }
        
        
        


        private void SendCmd(string cmd)
        {
            if (this.connectionSucceed == true)
            {
                try
                {

                    if (cmd.Substring(cmd.Length - 1, 1) != "?") //Check if query is sent
                    {
                        Write("*OPC");
                        System.Threading.Thread.Sleep(100);
                        Write(cmd);
                        if (cmd == "*RST")
                        {
                            WaitAfterReset();

                        }
                        else
                        {
                            System.Threading.Thread.Sleep(100);
                        }

                        string s = string.Empty;
                        while (s != "1")
                        {
                            Write("*OPC?");
                            System.Threading.Thread.Sleep(100);
                            s = Read();
                            s = s.Trim('\n').Trim('\r');

                        }
                        Write("SYSTEM:ERROR?");
                        System.Threading.Thread.Sleep(100);
                        s = Read();

                        s = s.CheckLambdaErrorOccured();
                        if (!string.IsNullOrEmpty(s))
                        {
                            throw new Exception("LAMBDA Errors: " + s);
                        }

                    }

                    else
                    {
                        Write(cmd);
                        System.Threading.Thread.Sleep(100);

                    }

                }
                catch (Exception e)
                {
                    throw new Exception("Command send failed. " + e.Message);
                }

            }
            else
            {
                throw new Exception("Connection was not established");
            }

        }


        private string SendQuery(string query)
        {
            if (this.connectionSucceed == true)
            {
                Write(query);
                return Read().Trim('\n').Trim('\r');
            }
            else
            {
                throw new Exception("Connection was not established");
            }


        }




        public override void Dispose()
        {

            try
                {
                    if (stream != null && client != null)
                    {
                        /*Volt = 0;
                        Current = 0;*/
                        Output = false;
                        SendCmd("SYST:SET 0");
                    }
                }
                catch
                {

                }
                finally
                {
                    if (stream != null)
                    {
                        stream.Close();
                        stream = null;
                    }
                    if (client != null)
                    {
                        client.Close();
                        client = null;
                    }
                    
                }
        }

        ~LambdaTCP()
        {
            this.Dispose();
             
        }


        public override void VoltageRamp(double startVolt, double finVolt, double current, double dwell)
        {
            throw new NotImplementedException();
        }

        public override void CurrentRamp(double startCurrent, double finCurrent, double volt, double dwell)
        {
            throw new NotImplementedException();
        }

        public override void VoltageCurrentRamp(double startVolt, double finVolt, double startCurrent, double finCurrent, double dwell)
        {
            throw new NotImplementedException();
        }

        public override double MeasCurrent
        {
            get 
            {
                return SendQuery("MEAS:CURR?").GetDouble(); 
            }
        }
        public override double MeasVolt
        {
            get
            {
                return SendQuery("MEAS:VOLT?").GetDouble();
            }
        }
    }
}
