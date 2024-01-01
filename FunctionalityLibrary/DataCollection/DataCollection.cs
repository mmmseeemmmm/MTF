using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomotiveLighting.MTFCommon;
using System.IO;

namespace DataCollectionDriver
{
    [MTFClass(Name = "Data Collection", Icon = MTFIcons.DataCollection, Description = "Driver for Data Collection")]
    [MTFClassCategory("Report")]
    public class DataCollection : IDisposable
    {
        private string basePath;

        private DataCollectionALCZControl dataCollectionControl;
        private string templatePath;

        public IMTFSequenceRuntimeContext RuntimeContext;

        [MTFConstructor]
        public DataCollection(string basePath, string templatePath)
        {
            this.basePath = basePath;
            this.templatePath = templatePath;
            dataCollectionControl = new DataCollectionALCZControl();
        }

        [MTFMethod]
        public void CollectData(string fileName, List<EntryOfReport> entriesOfReport)
        {
            dataCollectionControl.CollectData(fileName, entriesOfReport);
        }

        [MTFMethod]
        public void CollectErrorImages(string fileName, string controlCharacter, int startNumber,  MTFImage[] errorImages)
        {
            dataCollectionControl.CollectErrorImages(basePath, fileName, controlCharacter, startNumber, errorImages);
        }

        [MTFMethod]
        public void SaveFileToDestinationFolder(string fileName)
        {
            dataCollectionControl.SaveFileToDestinationFolder(this.basePath, fileName);
        }

        [MTFMethod]
        public void CollectByTemplate(string fileName)
        {
            var versions = RuntimeContext.SequenceVariantValue("Version");
            if (versions == null)
            {
                throw new Exception("Sequence variant is not set.");
            }

            string templateName = Path.Combine(templatePath, string.Format("{0}.template",versions.First()));
            if (!File.Exists(templateName))
            {
                throw new Exception(string.Format("Template for version {0} not found (file {1} doesn't exists)", versions.First(), templateName));
            }

            dataCollectionControl.CollectDataByTemplate(fileName, templateName, RuntimeContext);
        }

        [MTFMethod]
        public UInt16 MoveNext()
        {
            return dataCollectionControl.MoveNext();
        }

        public void Dispose()
        {
            if (dataCollectionControl != null)
            {
                dataCollectionControl.Close();
            }
            GC.SuppressFinalize(this);
        }

        ~DataCollection()
        {
            this.Dispose();
        }


    }
}
