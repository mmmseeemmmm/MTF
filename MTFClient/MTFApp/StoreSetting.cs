using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Xml.Serialization;
using MTFClientServerCommon;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.Helpers;

namespace MTFApp
{
    class StoreSettings
    {
        private static StoreSettings instance;
        private SettingsClass settingsClass = new SettingsClass();

        public static StoreSettings GetInstance => instance = instance ?? new StoreSettings();

        public SettingsClass SettingsClass => settingsClass;

        private StoreSettings() => Load();

        private void Load()
        {
            XmlSerializer xs = new XmlSerializer(typeof(SettingsClass));

            if (!File.Exists(BaseConstants.ClientSettingsPath))
            {
                if (!File.Exists(BaseConstants.ClientBackupSettingsPath))
                {
                    settingsClass = CreateNew();
                }
                else
                {
                    settingsClass = LoadBackup(xs);
                }

                return;
            }

            try
            {
                using (var fs = new FileStream(BaseConstants.ClientSettingsPath, FileMode.OpenOrCreate))
                {
                    settingsClass = (SettingsClass)xs.Deserialize(fs);
                    try
                    {
                        File.Copy(BaseConstants.ClientSettingsPath, BaseConstants.ClientBackupSettingsPath, true);
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
                settingsClass = LoadBackup(xs);

            }
        }

        private static SettingsClass LoadBackup(XmlSerializer xs)
        {
            try
            {
                using (var fs = new FileStream(BaseConstants.ClientBackupSettingsPath, FileMode.OpenOrCreate))
                {
                    var setting =  (SettingsClass)xs.Deserialize(fs);
                    SystemLog.LogMessage("The backup of the setting has been used.");
                    return setting;
                }
            }
            catch (Exception ex)
            {
                SystemLog.LogMessage("Could not load the setting.");
                SystemLog.LogException(ex);
                return CreateNew();
            }
        }

        private static SettingsClass CreateNew()
        {
            var settingsClass = new SettingsClass();
            settingsClass.Connections = new List<ConnectionDialog.ConnectionContainer>();
            settingsClass.Connections.Add(new ConnectionDialog.ConnectionContainer
                                          {
                                              Alias = "localhost",
                                              Host = "localhost",
                                              Port = "2442",
                                          });
            return settingsClass;
        }

        public void Save() => Save(true);

        public void Save(bool setWindowLocation)
        {
            if (setWindowLocation && Application.Current.MainWindow!=null)
            {
                settingsClass.WindowLocation = new Point(Application.Current.MainWindow.Left, Application.Current.MainWindow.Top);
                settingsClass.WindowSize = new Size(Application.Current.MainWindow.Width, Application.Current.MainWindow.Height);
                settingsClass.WindowState = Application.Current.MainWindow.WindowState;
            }

            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(SettingsClass));
                FileHelper.CreateDirectory(Path.GetDirectoryName(BaseConstants.ClientSettingsPath));
                using (FileStream fs = new FileStream(BaseConstants.ClientSettingsPath, FileMode.Create))
                {
                    xs.Serialize(fs, settingsClass);
                }
                FileHelper.SetFileForEveryone(BaseConstants.ClientSettingsPath);
            }
            catch (Exception ex)
            {
                SystemLog.LogMessage("Could not save the setting.");
                SystemLog.LogException(ex);
            }
        }

    }
}