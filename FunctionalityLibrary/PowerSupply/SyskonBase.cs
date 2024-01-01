using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerSupplyExtensionMethods;
using AutomotiveLighting.MTFCommon;

namespace PowerSupply
{
    public abstract class SyskonBase : PowerSupplyParent
    {
        /*public string comport;
        public int baudrate;
        public byte syskonadress;
        public int boardNumber;*/
        
        private object lockerLocal = new object();
        public abstract string Read();
        public abstract void Write(string cmd);
        //public MTFPercentProgressEventHandler InitProgress { get; set; }

        


        public override string Idn
        {
            get { return identification; }
        }

        public override double Volt
        {
            get
            {
                return SendQuery("USET?").GetDouble();
            }

            set
            {
                SendCmd("USET " + value.FormatIntoString());
            }
        }

        public override double Current
        {
            get
            {
                return SendQuery("ISET?").GetDouble();
            }

            set
            {
                SendCmd("ISET " + value.FormatIntoString());
            }
        }

        public override double Power
        {
            get
            {
                return SendQuery("PSET?").GetDouble();
            }

            set
            {
                SendCmd("PSET " + value.FormatIntoString());
            }
        }


        public override bool Output
        {

            get
            {
                string s = SendQuery("OUTP?");
                if (s == "OUTPUT ON")
                {
                    return true;
                }
                else if (s == "OUTPUT OFF")
                {
                    return false;
                }
                else
                {
                    throw new Exception("Received wrong string");
                }
            }

            set
            {
                if (value)
                {
                    SendCmd("OUTP ON");
                }
                else
                {
                    SendCmd("OUTP OFF");
                }
            }

        }

        protected virtual string DoInit()
        {
            //Current = 0;
            //Volt = 0;
            Output = false;
            return SendQuery("*IDN?");
        }

        protected virtual void DoClose()
        {
            /*Volt = 0;
            Current = 0;*/
            Output = false;
            Write("GTL");
        }

        public override void VoltageRamp(double startVolt, double finVolt, double current, double dwell)
        {

            SendCmd("STORE 1," + startVolt.FormatIntoString() + "," + current.FormatIntoString() + ",0,NF");
            SendCmd("STORE 2," + finVolt.FormatIntoString() + "," + current.FormatIntoString() + "," + dwell.FormatIntoString() + ",RU");
            SendCmd("START_STOP 1,2");
            SendCmd("REPETITION 1");
            SendCmd("SEQ GO");
            while (SendQuery("SEQ?").Contains("RUN"))
            {
                System.Threading.Thread.Sleep(500);
            }
            SendCmd("SEQ OFF");


        }

        public override void CurrentRamp(double startCurrent, double finCurrent, double volt, double dwell)
        {
            throw new NotImplementedException();
        }

        public override void VoltageCurrentRamp(double startVolt, double finVolt, double startCurrent, double finCurrent, double dwell)
        {
            throw new NotImplementedException();
        }


        int nrOfTries = 0;
        protected string SendCmd(string cmd)
        {
            
            if (this.connectionSucceed == true)
            {
                try
                {

                    if (cmd.Substring(cmd.Length - 1, 1) != "?") //Check if query is sent
                    {
                        
                        if (cmd == "*RST")
                        {
                            lock (lockerLocal)
                            {
                                //Write("*OPC");
                                //System.Threading.Thread.Sleep(100);
                                Write(cmd);


                                System.Threading.Thread.Sleep(30);
                                //for (int i = 0; i < 60; i++) //60
                                //{
                                //    if (stop)
                                //    {
                                //        InitProgress(100);
                                //        break;
                                //    }
                                //    System.Threading.Thread.Sleep(500);
                                //    if (InitProgress != null)
                                //    {
                                //        InitProgress(i * 100 / 60);
                                //    }
                                //}
                            }

                        }
                        else
                        {
                            //Write("*OPC");
                            //System.Threading.Thread.Sleep(100);
                            Write(cmd);
                           // System.Threading.Thread.Sleep(100);
                        }

                        string s = string.Empty;
                        while (s != "1")
                        {
                            Write("*OPC?");
                            //System.Threading.Thread.Sleep(100);
                            s = Read();


                        }
                        CheckErrors();

                    }

                    else
                    {
                        Write(cmd);
                        System.Threading.Thread.Sleep(100);

                    }
                    nrOfTries = 0;
                    return "OK";
                }
                catch (Exception e)
                {
                    if ((e.Message.Contains("Command error") || e.HResult == -2146233083) && nrOfTries < 10)
                    {
                        nrOfTries++;
                        Console.WriteLine(cmd +"Nr of tries: " +nrOfTries);
                        //mTFWriteOutput(cmd + "Nr of tries: " + nrOfTries.ToString());
                        System.Threading.Thread.Sleep(200);
                        return SendCmd(cmd);
                        
                    }
                    
                    else
                    {
                        nrOfTries = 0;
                        throw new Exception("Command send failed." + e.Message + ", CMD Repetitions:" + nrOfTries);
                    }

                }

            }
            else return "Connection was not established";

        }

        int queryNrTries = 0;
        protected string SendQuery(string query)
        {
            try
            {
                string indata = SendCmd(query);
                if (indata == "OK")
                {
                    indata = string.Empty;
                    indata = Read().Trim().Replace("  ", " ");
                    queryNrTries = 0;
                    return indata;
                }
                else
                {
                    
                    return indata;
                }
            }
            catch (Exception e)
            {
                
                if (e.HResult == -2146233083 && queryNrTries<10)
                {
                    queryNrTries++;
                    Console.WriteLine(query + "Nr of tries: " + queryNrTries);
                    //mTFWriteOutput(query + "Nr of tries: " + queryNrTries.ToString());
                    System.Threading.Thread.Sleep(200);
                    return SendQuery(query);
                }
                else
                {
                    queryNrTries = 0;
                    throw new Exception(e.ToString() + "Query Repetitions:" + queryNrTries);
                }
            }
        }

        public virtual void CheckErrors()
        {
            string s;
            Write("ERROR?");
            //System.Threading.Thread.Sleep(100);
            s = Read();
           // System.Threading.Thread.Sleep(100);


            s = s.CheckSyskonErrorOccured();
            if (!string.IsNullOrEmpty(s))
            {
                throw new Exception("Syskon Errors" + s);
            }
        }
             

        public override double MeasCurrent
        {
            get 
            {
                return SendQuery("IOUT?").GetDouble();
            }
        }
        public override double MeasVolt
        {
            get
            {
                return SendQuery("UOUT?").GetDouble();
            }
        }

       
    }
}
