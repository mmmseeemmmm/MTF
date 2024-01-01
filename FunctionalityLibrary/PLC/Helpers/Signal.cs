using System;

namespace PLC.Helpers
{
    public class Signal
    {
        public int DataLenght;
        public int StartBit;
        public string Name;
        public object Value;
        public PlcEnums.DataType DataType;
        public double RatioToPhysValue;

        public Signal(int startBit, string name, PlcEnums.DataType type)
        {
            this.StartBit = startBit;
            this.Name = name;
            if (type == PlcEnums.DataType.BOOL)
            {
                this.DataLenght = 1;
                this.DataType = PlcEnums.DataType.BOOL;
                this.Value = false;
            }
            else if (type == PlcEnums.DataType.UINT16)
            {
                this.DataLenght = 16;
                this.DataType = PlcEnums.DataType.UINT16;
                this.Value = 0;
                this.RatioToPhysValue = 1;
            }
            else if (type == PlcEnums.DataType.Byte)
            {
                this.DataLenght = 8;
                this.DataType = PlcEnums.DataType.Byte;
                this.Value = 0;
                this.RatioToPhysValue = 1;
            }
            else if (type == PlcEnums.DataType.UInt32)
            {
                this.DataLenght = 32;
                this.DataType = PlcEnums.DataType.UInt32;
                this.Value = 0;
                this.RatioToPhysValue = 1;
            }
            else if (type == PlcEnums.DataType.String)
            {
                var errorMessage = "For String please use this constructor - Signal(int startBit, string name, PlcEnums.DataType type, int stringLenght)";
                //Logging.AddTraceLine(errorMessage);
                throw new Exception(errorMessage);
            }
            else
            {
                var errorMessage = name + " - Defined data type is not supported - " + type.GetType().ToString();
                //Logging.AddTraceLine(errorMessage);
                throw new Exception(errorMessage);
            }
        }
        public Signal(int startBit, string name, PlcEnums.DataType type, int Lenght)
        {
            this.StartBit = startBit;
            this.Name = name;
            if (type == PlcEnums.DataType.String)
            {
                this.DataLenght = Lenght;
                this.DataType = PlcEnums.DataType.String;
                this.Value = string.Empty;
                this.RatioToPhysValue = 0;
            }
            else if (type == PlcEnums.DataType.ByteArray)
            {
                this.DataLenght = Lenght;
                this.DataType = PlcEnums.DataType.ByteArray;
                this.Value = 0;
                this.RatioToPhysValue = 0;
            }
            else
            {
                var errorMessage = name + " - Defined data type is not supported - " + type.GetType().ToString();
                //Logging.AddTraceLine(errorMessage);
                throw new Exception(errorMessage);
            }
        }

        public Signal(int startBit, string name, PlcEnums.DataType type, double ratioToPhysValue)
        {
            this.StartBit = startBit;
            this.Name = name;
            if (type == PlcEnums.DataType.BOOL)
            {
                this.DataLenght = 1;
                this.DataType = PlcEnums.DataType.BOOL;
                this.Value = false;
            }
            else if (type == PlcEnums.DataType.UINT16)
            {
                this.DataLenght = 16;
                this.DataType = PlcEnums.DataType.UINT16;
                this.Value = 0;
                this.RatioToPhysValue = ratioToPhysValue;
            }
            else if (type == PlcEnums.DataType.Byte)
            {
                this.DataLenght = 8;
                this.DataType = PlcEnums.DataType.Byte;
                this.Value = 0;
                this.RatioToPhysValue = ratioToPhysValue;
            }
            else if (type == PlcEnums.DataType.UInt32)
            {
                this.DataLenght = 32;
                this.DataType = PlcEnums.DataType.UInt32;
                this.Value = 0;
                this.RatioToPhysValue = ratioToPhysValue;
            }
            else if (type == PlcEnums.DataType.String)
            {
                var errorMessage = "For String please use this constructor - Signal(int startBit, string name, PlcEnums.DataType type, int stringLenght)";
                //Logging.AddTraceLine(errorMessage);
                throw new Exception(errorMessage);
            }
            else
            {
                var errorMessage = name + " - Defined data type is not supported - " + type.GetType().ToString();
                //Logging.AddTraceLine(errorMessage);
                throw new Exception(errorMessage);
            }
        }

    }
}
