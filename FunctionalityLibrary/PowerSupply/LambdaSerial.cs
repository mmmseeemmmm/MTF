using AutomotiveLighting.MTFCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO.Ports;
using PowerSupplyExtensionMethods;

namespace PowerSupply
{
    class LambdaSerial:LambdaBase
    {
        private int timeOut = 10000;
        private string comPort;
        private int baudRate=-1;
        private int adress=-1;
        private string serialAdress = string.Empty;



        private System.IO.Ports.SerialPort serialPort = null;

        
        public LambdaSerial(string comPort, int baudRate, int serialAdress)
        {
            this.comPort = comPort;
            this.baudRate = baudRate;
            this.adress = serialAdress;

        }


        public override void Init()
        {
            if ((comPort != null) && (comPort != string.Empty) && (adress != -1) && (baudRate != -1))
            {
                //InitSerialPort(this.comPort, this.baudRate, this.adress);
                try
                {
                    serialPort = SerialCommunication.GetSerialPort(this.comPort, this.baudRate);
                    serialPort.WriteTimeout = this.timeOut;
                    serialPort.ReadTimeout = this.timeOut;
                    connectionSucceed = true;



                    this.serialAdress = "ADR " + Convert.ToString(adress);


                    Write(serialAdress);
                    if (Read() != "OK")
                    {
                        throw new Exception("Serial adress was not accepted");
                    }
                    //Volt = 0;
                    //Current = 0;
                    Output = false;
                    //30s delay
                    identification = SendQuery("IDN?");
                    maxPower = identification.GetPower();
                    power = maxPower;
                }
                catch(Exception e)
                {
                    connectionSucceed = false;
                    throw new Exception("Communication failed: "+e.ToString());
                }

                //identification = Read();

            }
            else
            {
                throw new Exception("Communication parameters are not set");
            }
        }

        public override void Rst()
        {
            lock (PowerSupply.locker)
            {
                SendCmd("RST");
                maxPower = identification.GetPower();
                power = maxPower;
            }
        }

        //private void InitSerialPort(string comPort, int baudRate, int serialAdress)
        //{
           
        //    try
        //    {
        //        this.serialPort = new SerialPort();
        //        this.serialPort.PortName = comPort;
        //        this.serialPort.BaudRate = baudRate;
        //        this.serialPort.WriteTimeout = this.timeOut;
        //        this.serialPort.ReadTimeout = this.timeOut;
        //        this.serialAdress = "ADR " + Convert.ToString(serialAdress);
        //        this.serialPort.NewLine = "\r";
        //        this.serialPort.Open();


        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception("Serial port was not initialized: " + e.ToString());

        //    }
      
            
        //}

        public override double Volt
        {
            get
            {
                return SendQuery("PV?").GetDouble();
            }

            set
            {
                if (current * value <= power)
                {
                    SendCmd("PV " + value.FormatIntoString());
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
                return SendQuery("PC?").GetDouble();
            }

            set
            {
                if (value * volt <= power)
                {
                    
                    SendCmd("PC " + value.FormatIntoString());
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
                string s = SendQuery("OUT?");
                if (s == "ON")
                {
                    return true;
                }
                else if (s == "OFF")
                {
                    return false;
                }
                else
                {
                    throw new Exception("Obtained wrong response string");
                }
            }

            set
            {

                if (value)
                {
                    SendCmd("OUT 1");
                }
                else
                {
                    SendCmd("OUT 0");
                }
               
            }

        }

        private void SendCmd(string cmd)
        {
            if (this.connectionSucceed == true)
            {
                string s = string.Empty;
                try
                {
                        //Console.WriteLine(cmd);
                    Write(serialAdress);
                    s = Read();
                    //Console.WriteLine(s);
                    if (s != "OK")
                    {
                        ParseError(s);
                    }

                    Write(cmd);
                    if (cmd == "RST")
                    {
                        WaitAfterReset();
                    }

                    else
                    {
                    
                    }
                    s = Read();
                    //Console.WriteLine(s);
                    if (s != "OK")
                    {
                        ParseError(s);
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

        private string ParseError(string s)
        {
            if (s == "E01")
            {
                throw new Exception("Voltage over acceptable rande");
            }
            else if (s == "E02")
            {
                throw new Exception("Programming output voltave below UVL setting");
            }
            else if (s == "E04")
            {
                throw new Exception("OVP is programmed below acceptable range");
            }
            else if (s == "E06")
            {
                throw new Exception("UVL is proggammed above the programmed output voltage");

            }
            else if (s == "E07")
            {
                throw new Exception("Programming the output to on during a fault shut down");
            }
            else if (s == "C01")
            {
                throw new Exception("Illegal command or query");
            }
            else if (s == "C02")
            {
                throw new Exception("Missing parameter");
            }
            else if (s == "C03")
            {
                throw new Exception("Illegal parameter");
            }
            else if (s == "C04")
            {
                throw new Exception("Checksum error");
            }
            else if (s == "C05")
            {
                throw new Exception("Setting out of range");
            }
            else
            {
            }
            return s;
        
        }

        private string SendQuery(string query)
        {
            if (this.connectionSucceed == true)
            {
                string s = string.Empty;
                try
                {
                    Write(serialAdress);
                    s = Read();
                }
                catch(Exception e)
                {
                    connectionSucceed = false;
                    throw new Exception("Device communication failed"+e.ToString());
                }
                if (s == "OK")
                {
                    Write(query);

                    s = Read();

                    return ParseError(s);
                }
                else
                {
                    throw new Exception("Adress was not accepted");
                }
            }
            else
            {
                throw new Exception("Connection was not established");
            }


        }


        public  void Write(string command)
        {

            serialPort.WriteLine(command);
            Thread.Sleep(100);
            
        }

        public  string Read()
        {
            
                return serialPort.ReadLine();
                
        }
        


        public override void Dispose()
        {

            if (serialPort != null)
            {
                try
                {
                    /*Volt = 0;
                    Current = 0;*/
                    Output = false;
                    SendCmd("RMT 0");
                }
                catch
                {

                }
                finally
                {
                    SerialCommunication.CloseSerailPort(this.comPort);
                }
            }
        }

        ~LambdaSerial()
        {
            this.Dispose(); 
        }




        public override void VoltageRamp(double startVolt, double finVolt, double current, double dwell)
        {
            //string aaa;
            double minTime = 0.1;
            double dU = 0.01;
            startVolt = Math.Round(startVolt, 2);
            finVolt = Math.Round(finVolt, 2);
            double range=Math.Abs(finVolt - startVolt);
            int steps = (int)(range/dU);
            double dT = dwell / steps;
            if (dT < minTime)
            {
                dT = minTime;
                steps = (int)(dwell/dT);
                dU = (finVolt - startVolt) *dT / dwell;
                
            }
            else 
            {
                dU = (finVolt - startVolt) / steps;
            }

            Current = current;
            Output = true;
            
            for (int i = 0; i < steps; i++)
            {
                double myvolt = startVolt + dU * (i + 1);
                Write("PV " + myvolt.FormatIntoString());
                //Console.WriteLine("PV " + myvolt.FormatIntoString());
                //Volt=startVolt + dU * (i + 1);
                System.Threading.Thread.Sleep((int)(dT*1000));
            
            }
            if (serialPort.ReadExisting().ErrorFromRampOccured())
            {
                throw new Exception("Error during ramp generation occured");
            }
            serialPort.DiscardInBuffer();
                
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
                return SendQuery("MC?").GetDouble();
            }
        }

        public override double MeasVolt
        {
            get
            {
                return SendQuery("MV?").GetDouble();
            }
        }
    }
}
