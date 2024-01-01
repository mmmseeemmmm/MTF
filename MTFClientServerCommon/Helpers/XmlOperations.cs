using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using MTFClientServerCommon.Constants;

namespace MTFClientServerCommon.Helpers
{
    public static class XmlOperations
    {
        public static T LoadXmlData<T>(string fileName) where T : class, new()
        {
            T data = null;
            try
            {
                var xs = new XmlSerializer(typeof(T));
                if (File.Exists(fileName))
                {
                    using (var fs = new FileStream(fileName, FileMode.OpenOrCreate))
                    {
                        data = fs.Length == 0 ? new T() : xs.Deserialize(fs) as T;
                        fs.Close();
                    }
                }
            }
            catch
            {
                data = new T();
            }
            return data ?? new T();
        }

        public static ServerSettings LoadServerSetting()
        {
            ServerSettings settings;
            XmlSerializer xs = new XmlSerializer(typeof(ServerSettings));

            if (!File.Exists(BaseConstants.ServerSettingsPath))
            {
                if (!File.Exists(BaseConstants.ServerBackupSettingsPath))
                {
                    settings = new ServerSettings();
                }
                else
                {
                    settings = LoadBackup(xs);
                }

                return settings;
            }

            try
            {
                using (var fs = new FileStream(BaseConstants.ServerSettingsPath, FileMode.OpenOrCreate))
                {
                    settings = (ServerSettings)xs.Deserialize(fs);
                    try
                    {
                        File.Copy(BaseConstants.ServerSettingsPath, BaseConstants.ServerBackupSettingsPath, true);
                    }
                    catch (Exception ex)
                    {
                        SystemLog.LogMessage("Could not create a backup of the setting.");
                        SystemLog.LogException(ex);
                    }
                }
            }
            catch (Exception)
            {
                settings = LoadBackup(xs);

            }
            return settings;
        }

        private static ServerSettings LoadBackup(XmlSerializer xs)
        {
            try
            {
                using (var fs = new FileStream(BaseConstants.ServerBackupSettingsPath, FileMode.OpenOrCreate))
                {
                    var setting = (ServerSettings)xs.Deserialize(fs);
                    SystemLog.LogMessage("The backup of the setting has been used.");
                    return setting;
                }
            }
            catch (Exception ex)
            {
                SystemLog.LogMessage("Could not load the setting.");
                SystemLog.LogException(ex);
                return new ServerSettings();
            }
        }

        public static void SaveXmlData<T>(string fileName, T data)
        {
            try
            {
                var xs = new XmlSerializer(typeof(T));
                FileHelper.CreateDirectory(Path.GetDirectoryName(fileName));
                using (var writeFileStream = XmlWriter.Create(fileName))
                {
                    xs.Serialize(writeFileStream, data);
                }
                FileHelper.SetFileForEveryone(fileName);
            }
            catch (Exception ex)
            {
                SystemLog.LogMessage("Could not save xml data: " + fileName);
                SystemLog.LogException(ex);
            }
        }
    }
}
