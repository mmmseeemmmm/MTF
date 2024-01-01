using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using AutomotiveLighting.MTFCommon.Types;

namespace General
{
    //Implemented by Joerg, dispose method taken from MSDN
    class CSVLogger : IDisposable
    {
        private StreamWriter sw;
        private bool disposed = false;

        public string Delimiter { get; set; }

        public CSVLogger(string FileName, int mode, List<String> header, string Delimeter = "; ")
        {
            this.Delimiter = Delimeter;

            //Check, if file exists, if yes, we don't write a header.
            bool ShouldWriteHeader = !File.Exists(FileName);
            if (mode == 1)
            {
                ShouldWriteHeader = true;
                //We may have opened the file already...
                if (sw != null) sw.Close();
            }
            if (header == null) ShouldWriteHeader = false;
            sw = new StreamWriter(FileName, mode == 0); //Append if neccessary (mode=0=append)
            sw.AutoFlush = true; //Elsewise, we have no date if the seuqnce is aborted.

            if (ShouldWriteHeader)
            {
                String s = "Opening file at: " + MTFDateTime.Now().ToString() + Delimeter;
                //Collect data for string
                foreach (var item in header)
                    s = s + item + this.Delimiter;

                sw.WriteLine(s.Substring(0, s.Length - Delimeter.Length));
            }

        }

        ~CSVLogger()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);               
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                try
                {
                    Close();
                    sw = null;
                    GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                    GC.Collect(99, GCCollectionMode.Forced, true);
                }
                catch (Exception)
                { }
            }

            disposed = true;
        }

        public void Close()
        {
            sw.Flush();
            sw.Close();
            sw = null;
        }

        public bool Write(MTFDateTime date, List<string> data)
        {
            try
            {
                //Sanity check
                if (data == null) return true;

                //Create string
                String s = "";

                //Add date into first place
                if (date == null)
                    s = s + MTFDateTime.Now().ToString() + Delimiter;
                else
                    s = s + date.ToString() + Delimiter;

                //Collect data for string
                foreach (var item in data)
                    s = s + item + Delimiter;

                sw.WriteLine(s.Substring(0, s.Length - Delimiter.Length));
            }
            catch (Exception)
            {
                //Don't complain about logging errors!
                return false;
            }
            return true;
        }
    }
}
