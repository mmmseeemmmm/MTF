using System;
using System.Linq;
using System.Text;

namespace PLC.Helpers
{
    public delegate void OnDataEventHandler(object sender, byte[] data);

    public static class CommunicationBase
    {
        public static IPLCCommunication GetInstance(PlcEnums.PLCType plcType)
        {
            if (plcType == PlcEnums.PLCType.Bosch_UDP)
            {
                return new Bosch_UDP();
            }

            return null;
        }

        public static Signal RAWtoSignal(Signal signal, byte[] RawDataFromPLC, bool littleEndian)
        {
            Signal result = null;
            int bytePossition = signal.StartBit / 8;

            if (signal.DataType == PlcEnums.DataType.BOOL)
            {
                if ((bytePossition) >= RawDataFromPLC.Length)
                {
                    var errorMessage = "SignalsToRAW Error - bytePossition - " + bytePossition + " - " + signal.Name + " - " + PlcEnums.DataType.UINT16;
                    //Logging.AddTraceLine(errorMessage);
                    throw new Exception(errorMessage);
                }

                byte toGet = RawDataFromPLC[bytePossition];

                int bitPossition = signal.StartBit % 8;

                toGet = (byte)(((byte)(toGet >> bitPossition)) & 0x01);
                result = signal;
                result.Value = Convert.ToBoolean(toGet); // signal.Value;
            }

            else if (signal.DataType == PlcEnums.DataType.UINT16)
            {
                if ((bytePossition + (signal.DataLenght/8 - 1)) >= RawDataFromPLC.Length)
                {
                    var errorMessage = "SignalsToRAW Error - bytePossition - " + bytePossition + " - " + signal.Name + " - " + PlcEnums.DataType.UINT16;
                    //Logging.AddTraceLine(errorMessage);
                    throw new Exception(errorMessage);

                }

                byte[] intBytes = new byte[signal.DataLenght];
                if (littleEndian)
                {
                    intBytes[0] = RawDataFromPLC[bytePossition];
                    intBytes[1] = RawDataFromPLC[bytePossition + 1];
                }
                else
                {
                    intBytes[1] = RawDataFromPLC[bytePossition];
                    intBytes[0] = RawDataFromPLC[bytePossition + 1];
                }
                result = signal;
                result.Value = (UInt16)(BitConverter.ToUInt16(intBytes, 0)*signal.RatioToPhysValue);
            }
            else if (signal.DataType == PlcEnums.DataType.Byte)
            {
                if ((bytePossition) >= RawDataFromPLC.Length)
                {
                    var errorMessage = "SignalsToRAW Error - bytePossition - " + bytePossition + " - " + signal.Name + " - " + PlcEnums.DataType.UINT16;
                    //Logging.AddTraceLine(errorMessage);
                    throw new Exception(errorMessage);
                }

                result = signal;
                result.Value = (byte)(RawDataFromPLC[bytePossition]*signal.RatioToPhysValue);
            }
            else if (signal.DataType == PlcEnums.DataType.UInt32)
            {
                if ((bytePossition + (signal.DataLenght - 1)) >= RawDataFromPLC.Length)
                {
                    var errorMessage = "SignalsToRAW Error - bytePossition - " + bytePossition + " - " + signal.Name + " - " + PlcEnums.DataType.UINT16;
                    //Logging.AddTraceLine(errorMessage);
                    throw new Exception(errorMessage);
                }

                byte[] intBytes = new byte[signal.DataLenght];

                if (littleEndian)
                {
                    intBytes[0] = RawDataFromPLC[bytePossition];
                    intBytes[1] = RawDataFromPLC[bytePossition + 1];
                    intBytes[2] = RawDataFromPLC[bytePossition + 2];
                    intBytes[3] = RawDataFromPLC[bytePossition + 3];
                }
                else
                {
                    intBytes[3] = RawDataFromPLC[bytePossition];
                    intBytes[2] = RawDataFromPLC[bytePossition + 1];
                    intBytes[1] = RawDataFromPLC[bytePossition + 2];
                    intBytes[0] = RawDataFromPLC[bytePossition + 3];
                }
                result = signal;
                result.Value = (UInt32)(BitConverter.ToInt32(intBytes, 0)*signal.RatioToPhysValue);
            }
            else if (signal.DataType == PlcEnums.DataType.String)
            {
                if ((bytePossition + (signal.DataLenght - 1)) >= RawDataFromPLC.Length)
                {
                    var errorMessage = "SignalsToRAW Error - bytePossition - " + bytePossition + " - " + signal.Name + " - " + PlcEnums.DataType.String;
                    //Logging.AddTraceLine(errorMessage);
                    throw new Exception(errorMessage);
                }


                byte[] intBytes = new byte[signal.DataLenght];

                if (littleEndian)
                {
                    for (int i = 0; i < intBytes.Length; i++)
                    {
                        intBytes[i] = RawDataFromPLC[bytePossition + i];
                    }
                }
                else
                {
                    for (int i = 0; i < intBytes.Length; i++)
                    {
                        intBytes[intBytes.Length - 1 - i] = RawDataFromPLC[bytePossition + i];
                    }
                }
                result = signal;
                string strVal = Encoding.ASCII.GetString(intBytes);
                //cut no strings
                //strVal = new System.Text.RegularExpressions.Regex(@"[^a-zA-Z0-9\s]", (System.Text.RegularExpressions.RegexOptions)0).Replace(strVal, string.Empty);

                strVal = strVal.Trim('\0', '\n', '\r', '\t', ' ');
                if (strVal.Contains('\r'))
                {
                    strVal = strVal.Substring(0, strVal.IndexOf('\r'));
                }
                result.Value = strVal;
            }
            else
            {
                var errorMessage = "SignalsToRAW Error - " + signal.Name;
                //Logging.AddTraceLine(errorMessage);
                throw new Exception(errorMessage);
            }


            return result;
        }

        public static void SignalToRAW(Signal signal, ref byte[] RawDataToPLC, bool littleEndian)
        {
            int bitPossitionOffset = 0;
            bool done = false;
            for (int i = 0; i < RawDataToPLC.Length; i++)
            {
                //is this placed in this byte?
                if (signal.DataType == PlcEnums.DataType.BOOL && signal.StartBit <= bitPossitionOffset + 7)
                {
                    byte toSet = 1;
                    //toSet = (byte)(toSet | (Convert.ToByte(signal.Value)));
                    int bitPossition = signal.StartBit % 8;
                    toSet = (byte)(toSet << bitPossition);
                    if (Convert.ToBoolean(signal.Value))
                    {
                        RawDataToPLC[i] = (byte)(RawDataToPLC[i] | toSet);
                    }
                    else
                    {
                        RawDataToPLC[i] = (byte)(RawDataToPLC[i] & ~toSet);
                    }
                    done = true;
                    break;
                }
                if (signal.DataType == PlcEnums.DataType.UINT16 && bitPossitionOffset == signal.StartBit)
                {
                    byte[] intBytes = BitConverter.GetBytes(Convert.ToUInt16(signal.Value));
                    if (littleEndian)
                    {
                        RawDataToPLC[i] = intBytes[0];
                        RawDataToPLC[i + 1] = intBytes[1];
                    }
                    else
                    {
                        RawDataToPLC[i] = intBytes[1];
                        RawDataToPLC[i + 1] = intBytes[0];
                    }
                    done = true;
                    break;
                }
                if (signal.DataType == PlcEnums.DataType.Byte && bitPossitionOffset == signal.StartBit)
                {
                    RawDataToPLC[i] = Convert.ToByte(signal.Value);
                    done = true;
                    break;
                }

                if (signal.DataType == PlcEnums.DataType.UInt32 && bitPossitionOffset == signal.StartBit)
                {
                    byte[] intBytes = BitConverter.GetBytes(Convert.ToInt32(signal.Value));

                    if (littleEndian)
                    {
                        RawDataToPLC[i] = intBytes[0];
                        RawDataToPLC[i + 1] = intBytes[1];
                        RawDataToPLC[i + 2] = intBytes[2];
                        RawDataToPLC[i + 3] = intBytes[3];
                    }
                    else
                    {
                        RawDataToPLC[i] = intBytes[3];
                        RawDataToPLC[i + 1] = intBytes[2];
                        RawDataToPLC[i + 2] = intBytes[1];
                        RawDataToPLC[i + 3] = intBytes[0];
                    }

                    done = true;
                    break;
                }

                if (signal.DataType == PlcEnums.DataType.String && bitPossitionOffset == signal.StartBit)
                {

                    byte[] intBytes = Encoding.ASCII.GetBytes(signal.Value.ToString());

                    if (littleEndian)
                    {
                        for (int j = 0; j < intBytes.Length; j++)
                        {
                            RawDataToPLC[i+j] = intBytes[j];
                        }
                    }
                    else
                    {
                        for (int j = 0; j < intBytes.Length; j++)
                        {
                            RawDataToPLC[i + j] = intBytes[intBytes.Length-j];
                        }
                    }

                    done = true;
                    break;
                }
                if (signal.DataType == PlcEnums.DataType.ByteArray && bitPossitionOffset == signal.StartBit)
                {

                    byte[] intBytes = (byte[])signal.Value;

                    if (littleEndian)
                    {
                        for (int j = 0; j < intBytes.Length; j++)
                        {
                            RawDataToPLC[i + j] = intBytes[j];
                        }
                    }
                    else
                    {
                        for (int j = 0; j < intBytes.Length; j++)
                        {
                            RawDataToPLC[i + j] = intBytes[intBytes.Length - j];
                        }
                    }

                    done = true;
                    break;
                }

                bitPossitionOffset = bitPossitionOffset + 8;
            }
            if (!done)
            {
                var errorMessage = "SignalsToRAW Error - " + signal.Name;
                //Logging.AddTraceLine(errorMessage);
                throw new Exception(errorMessage);
            }
        }
    }

    public static class PlcEnums
    {
        public enum DataType { BOOL, Byte, UINT16, UInt32, String, ByteArray };

        public enum PLCType { Bosch_UDP };
    }
}
