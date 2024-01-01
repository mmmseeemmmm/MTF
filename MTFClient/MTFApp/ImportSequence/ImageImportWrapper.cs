using MTFClientServerCommon.Import;

namespace MTFApp.ImportSequence
{
    public class ImageImportWrapper
    {
        private readonly ImportConflict importConflict;
        private readonly byte[] data;

        public ImageImportWrapper(ImportConflict importConflict, byte[] data)
        {
            this.importConflict = importConflict;
            this.data = data;
        }

        public ImportConflict Conflict
        {
            get { return importConflict; }
        }

        public byte[] Data
        {
            get { return data; }
        }
    }
}
