using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MTFVision
{

    public static class ImageDatabase
    {
        static ImageDatabase()
        {
            BasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"data\vision\default");
            ImageDictionary = new Dictionary<string, string>();
        }   

        public static Dictionary<string, string> ImageDictionary { get; private set; }

        private static string basePath;
        public static string BasePath
        { 
            get
            {
                return basePath;
            }
            set
            {                
                basePath = value;
                if (Directory.Exists(basePath))
                {
                    loadImageDictionary();
                }
                else
                {
                    Directory.CreateDirectory(basePath);
                }
            }
        }
             
       
        public static void AddImage(string ActivityPath, string FileName)
        {
            //basePath = Path.GetDirectoryName(FileName);
            if (ImageDictionary.ContainsKey(ActivityPath))
            {
                ImageDictionary[ActivityPath] = FileName;
            }
            else
            {
                ImageDictionary.Add(ActivityPath, FileName);
            }            
            saveImageDictionary();
        }
        private static void saveImageDictionary()
        {
            using (StreamWriter file = File.CreateText(Path.Combine(basePath,"images.json")))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, ImageDictionary);
            }
        }
        private static void loadImageDictionary()
        {
            try
            {
                using (StreamReader file = File.OpenText(Path.Combine(basePath, "images.json")))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    Dictionary<string, string> ImageDictionary = (Dictionary<string, string>)serializer.Deserialize(file, typeof(Dictionary<string, string>));
                }
            }
            catch
            {
                
            }
        }

    }
}
