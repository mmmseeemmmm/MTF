using AL.Utils.Logging;

namespace PLC.Helpers
{
    public static class Logging
    {
        private static string prefixFile = "PLCDriver";

        public static void AddTraceLine(string message, bool includeTimestamp = true)
        {
            Log.LogMessage(message, prefixFile, includeTimestamp);
        }
    }
}
