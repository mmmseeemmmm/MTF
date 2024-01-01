using System;
using System.IO;

namespace MTFCommon
{
    [Serializable]
    public class MTFFileInfo
    {
        private bool isDirectory;
        private string fullName;

        public string Name
        {
            get
            {
                if (!string.IsNullOrEmpty(FullName))
                {
                    return IsDirectory ? Path.GetFileName(FullName) : Path.GetFileNameWithoutExtension(FullName);
                }
                return null;
            }
        }

        public bool IsDirectory
        {
            get { return isDirectory; }
            set { isDirectory = value; }
        }

        public string FullName
        {
            get { return fullName; }
            set { fullName = value; }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
