namespace MTFApp.UIHelpers
{
    /// <summary>
    /// Clipboard for any data
    /// </summary>
    public static class MTFClipboard
    {
        private static object clipboardData;
        private static object clipboardDataParent;
        static MTFClipboard()
        {
            //clipboardData = new List<object>();
        }

        /// <summary>
        /// Copy data to clipboard
        /// </summary>
        /// <param name="data">Copied data</param>
        public static void SetData(object data)
        {
            clipboardData = data;
        }

        /// <summary>
        /// Copy data to clipboard
        /// </summary>
        /// <param name="data">Copied data</param>
        /// <param name="dataParent">Parent of data</param>
        public static void SetData(object data, object dataParent)
        {
            clipboardData = data;
            clipboardDataParent = dataParent;
        }

        public static bool IsSameParent(object parent)
        {
            return Equals(clipboardDataParent, parent);
        }

        /// <summary>
        /// Get data from clipboard
        /// </summary>
        /// <returns>Getting data</returns>
        public static object GetData()
        {
            return clipboardData;
        }

        public static T GetData<T>()
        {
            if (ContainsData<T>())
            {
                return (T)clipboardData;
            }
            return default(T);
        }

        /// <summary>
        /// Equals data in clipboard with type T
        /// </summary>
        /// <typeparam name="T">Testing Type</typeparam>
        /// <returns>True if data is type of T</returns>
        public static bool ContainsData<T>()
        {
            return clipboardData is T;
        }

        public static bool IsEmpty()
        {
            return clipboardData == null;
        }
    }
}
