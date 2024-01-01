using System;

namespace MTFClientServerCommon.Import
{
    [Serializable]
    public class SequenceMetaData
    {
        private string storedName;
        private string originalName;
        private string path;
        private string mainSequenceName;
        private string pathForImport;
        private Version mtfVersion;
        private Guid id;

        public string StoredName
        {
            get => storedName;
            set => storedName = value;
        }

        public string OriginalName
        {
            get => originalName;
            set => originalName = value;
        }


        public string Path
        {
            get => path;
            set => path = value;
        }


        public string MainSequenceName
        {
            get => mainSequenceName;
            set => mainSequenceName = value;
        }


        public string PathForImport
        {
            get => pathForImport;
            set => pathForImport = value;
        }


        public Version MTFVersion
        {
            get => mtfVersion;
            set => mtfVersion = value;
        }

        public Guid Id
        {
            get => id;
            set => id = value;
        }
    }
}