using MTFClientServerCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.Helpers;

namespace MTFCore
{
    public class MTFPersistToXml : IMTFPersist
    {
        public List<T> LoadDataList<T>(string fileName) where T : MTFPersist
        {
            string fullFileName = Path.Combine(BaseConstants.DataPath, fileName);

            if (!File.Exists(fullFileName))
            {
                return new List<T>();
            }

            XmlSerializer s = new XmlSerializer(typeof(List<T>));
            StreamReader reader = new StreamReader(fullFileName);
            List<T> data;
            bool delete = false;
            try
            {
                data = s.Deserialize(reader) as List<T>;
            }
            catch (Exception)
            {
                delete = true;
                return new List<T>();
            }
            finally
            {
                reader.Close();
                reader.Dispose();
                if (delete)
                {
                    File.Delete(fullFileName);
                }
            }

            data.ForEach(item => { item.IsNew = false; item.IsModified = false; });

            return data as List<T>;
        }

        public T LoadData<T>(string fileName) where T : MTFPersist
        {
            string fullFileName = Path.Combine(BaseConstants.DataPath, fileName);
            
            if (!File.Exists(fullFileName))
            {
                return Activator.CreateInstance<T>();
            }

            XmlSerializer s = new XmlSerializer(typeof(T));
            StreamReader reader = new StreamReader(fullFileName);
            T data;
            bool delete = false;
            try
            {
                data = s.Deserialize(reader) as T;
            }
            catch (Exception)
            {
                delete = true;
                return Activator.CreateInstance<T>();
            }
            finally
            {
                reader.Close();
                reader.Dispose();
                if (delete)
                {
                    File.Delete(fullFileName);
                }
            }
            data.IsNew = false;
            data.IsModified = false;

            return data as T;
        }

        public void SaveData<T>(List<T> objectToPersist, string fileName) where T : MTFPersist
        {
            string fullFileName = Path.Combine(BaseConstants.DataPath, fileName);

            if (objectToPersist == null || objectToPersist.Count < 1)
            {
                return;
            }
            HashSet<Type> extraType = new HashSet<Type>();
            if (typeof(T) == typeof(MTFClassInstanceConfiguration))
            {
                foreach (var item in objectToPersist)
                {
                    foreach (MTFParameterValue item2 in (item as MTFClassInstanceConfiguration).ParameterValues)
                    {

                        extraType.Add(Type.GetType(item2.TypeName));
                    }
                }
                XmlSerializer s = new XmlSerializer(typeof(List<T>), extraType.ToArray());
                string name = Path.GetFileName(fullFileName);
                string path = fullFileName.Remove(fullFileName.Length - name.Length);

                FileHelper.CreateDirectory(path);

                StreamWriter writer = new StreamWriter(fullFileName);
                List<T> g = objectToPersist.Where(item => !item.IsDeleted).ToList<T>();
                s.Serialize(writer, g);

                writer.Close();
                writer.Dispose();

                FileHelper.SetFileForEveryone(fullFileName);
            }
        }

        public void SaveData<T>(T objectToPersist, string fileName) where T : MTFPersist
        {
            string fullFileName = Path.Combine(BaseConstants.DataPath, fileName);

            if (objectToPersist == null)
            {
                return;
            }

            if (objectToPersist.IsDeleted)
            {
                File.Delete(fullFileName);
                return;
            }
            

            //if (objectToPersist.IsModified || objectToPersist.IsNew)
            //{
            XmlSerializer s = new XmlSerializer(typeof(T));
            string name = Path.GetFileName(fullFileName);
            string path = fullFileName.Remove(fullFileName.Length - name.Length);

            FileHelper.CreateDirectory(path);

            StreamWriter writer = new StreamWriter(fullFileName);
            s.Serialize(writer, objectToPersist);

            writer.Close();
            writer.Dispose();

            FileHelper.SetFileForEveryone(fullFileName);
            //}
        }

        

        public List<MTFPersistDataInfo> GetPersistInfo(string path)
        {
            string fullPath = Path.Combine(BaseConstants.DataPath, path);

            if (!Directory.Exists(fullPath))
            {
                return null;
            }

            List<MTFPersistDataInfo> persistDataInfos = new List<MTFPersistDataInfo>();
            foreach (string dir in Directory.GetDirectories(fullPath))
            {
                persistDataInfos.Add(new MTFPersistDataInfo { Name = dir.Remove(0, fullPath.Length + 1), Type = MTFDialogItemTypes.Folder});
            }

            foreach (string file in Directory.GetFiles(fullPath))
            {
                persistDataInfos.Add(new MTFPersistDataInfo { Name = file.Remove(0, fullPath.Length + 1), Type = MTFDialogItemTypes.File});
            }

            return persistDataInfos;
        } 
    }
}
