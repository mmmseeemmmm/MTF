using System;
using System.IO;

namespace Printer
{
    class PermanentCounter
    {
        private readonly string fileName;
        private readonly int stringLength;
        private int? currentValue = null;
        public PermanentCounter(string name, int stringLength)
        {
            fileName = string.Format("{0}.txt", name);
            this.stringLength = stringLength;
        }

        public int Value (DateTime date)
        {
            return MoveNext(date); 
        }

        public string StringValue(DateTime date)
        {
            return MoveNext(date).ToString(string.Format("D{0}", stringLength));
        }

        private int MoveNext(DateTime date)
        {
            int result;

            if (!File.Exists(fileName))
            {
                File.WriteAllText(fileName, "1");
            }

            //reset counter on midnight - if file is from diferent day then today
            var fileCreationDate = File.GetCreationTime(fileName);
            if (fileCreationDate.DayOfYear != date.DayOfYear || fileCreationDate.Year != date.Year)
            {
                File.Delete(fileName);
                File.WriteAllText(fileName, "1");
            }

            string counter = File.ReadAllText(fileName);

            try
            {
                result = int.Parse(counter);
            }
            catch
            {
                File.WriteAllText(fileName, "1");
                result = 1;
            }

            int newValue = result + 1;

            File.WriteAllText(fileName, newValue.ToString());

            return result;

        }
    }
}
