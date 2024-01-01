using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using AutomotiveLighting.MTFCommon;

namespace ProductionCounter
{
    [MTFClass(Name = "Production counter", Icon = MTFIcons.ProductionCounter)]
    [MTFClassCategory("Report")]
    public class ProductionCounter
    {
        private string fileName = "dataFile.txt";
        private string filePath;
        private DataClass dataClass;
        private readonly XmlSerializer xmlSerializer;


        [MTFConstructor]
        public ProductionCounter()
        {
            xmlSerializer = new XmlSerializer(typeof(DataClass));
            InitData();
        }

        private void InitData()
        {
            var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            filePath = location != null ? Path.Combine(location, fileName) : Path.Combine(Environment.CurrentDirectory, filePath);
            dataClass = File.Exists(filePath) ? LoadData() : new DataClass();
        }

        private DataClass LoadData()
        {
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open))
                {
                    return (DataClass)xmlSerializer.Deserialize(fs);
                }
            }
            catch (Exception)
            {
                return new DataClass();
            }
        }

        private void Save()
        {
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Create))
                {
                    xmlSerializer.Serialize(fs, dataClass);
                }
            }
            catch (Exception)
            {
                //return new DataClass();
            }
        }


        [MTFProperty(Name = "Actual variant")]
        public string ActualVariant
        {
            get { return dataClass.ActualVariant; }
            set
            {
                dataClass.ActualVariant = value;
                if (dataClass.Data.All(x => x.Variant != value))
                {
                    dataClass.Data.Add(new DataItem {Variant = value, Count = 0});
                }
                Save();
            }
        }

        [MTFProperty(Name = "Target countdown")]
        public int TargetCountdown
        {
            get { return dataClass.TargetCountdown; }
            set
            {
                dataClass.TargetCountdown = value;
                Save();
            }
        }

        [MTFMethod(DisplayName = "Set file name")]
        public void SetFileName(string fileName)
        {
            this.fileName = fileName.EndsWith(".txt") ? fileName : string.Format("{0}.txt", fileName);
            InitData();
        }


        [MTFMethod(DisplayName = "Decrement target countdown")]
        public int DecrementTargetCountdown()
        {
            var now = DateTime.Now;
            CheckDay(now);
            if (!string.IsNullOrEmpty(ActualVariant))
            {
                dataClass.TargetCountdown--;

                var item = dataClass.Data.FirstOrDefault(x => x.Variant == ActualVariant);
                if (item != null)
                {
                    item.Count++;
                }
                else
                {
                    throw new Exception(string.Format("Variant {0} is not exists.", ActualVariant));
                }

                Save();

                return dataClass.TargetCountdown;
            }

            throw new Exception("Variant is not set.");
        }

        [MTFMethod(DisplayName = "Get variant counter")]
        public int GetVariantCounter(string variant)
        {
            CheckDay(DateTime.Now);
            var item = dataClass.Data.FirstOrDefault(x => x.Variant == variant);
            if (item != null)
            {
                return item.Count;
            }
            throw new Exception(string.Format("Variant {0} is not exists.", variant));
        }

        //[MTFMethod(DisplayName = "Get and increment variant counter")]
        //public int GetAndIncrementVariantCounter(string variant)
        //{
        //    var item = dataClass.Data.FirstOrDefault(x => x.Variant == variant);
        //    if (item != null)
        //    {
        //        var value = item.Count;
        //        item.Count++;
        //        Save();

        //        return value;
        //    }
        //    throw new Exception(string.Format("Variant {0} is not exists", variant));
        //}

        [MTFMethod(DisplayName = "Get serial number")]
        public SerialNumber GetSerialNumber(string variant)
        {
            var now = DateTime.Now;
            CheckDay(now);
            var item = dataClass.Data.FirstOrDefault(x => x.Variant == variant);
            if (item != null)
            {
                var serialNumber = new SerialNumber();
                serialNumber.Date = now.ToString("ddMMyy");
                serialNumber.Time = now.ToString("HHmmss");
                serialNumber.RunningNumber = (item.Count + 1).ToString("D4");

                return serialNumber;
            }

            throw new Exception(string.Format("Variant {0} is not exists.", variant));
        }

        private void CheckDay(DateTime actualDate)
        {
            if (dataClass.LastDate.Date != actualDate.Date)
            {
                dataClass.Data.ForEach(x => x.Count = 0);
                dataClass.LastDate = DateTime.Now;
                Save();
            }
        }
    }
}