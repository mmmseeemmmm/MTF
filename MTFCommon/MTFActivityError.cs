using System;

namespace MTFCommon
{
    [Serializable]
    public class MTFActivityError
    {
        public DateTime TimeStamp { get; set; }
        public string ActivityName { get; set; }
        public string ActivityPathLong { get; set; }
        public string ActivityPathShort { get; set; }
        public string ErrorMessage { get; set; }

        
        /// <summary>
        /// Formats activity error by format string.
        /// </summary>
        /// <param name="formatString">Use placeholders for format: {TimeStamp}, {ActivityName}, {ActivityPathShort}, {ActivityPathLong}, {ErrorMessage}.</param>
        /// <returns>Returns formated activity error by fomrat string.</returns>
        public string FormatActivityError(string formatString)
        {
            return string.IsNullOrEmpty(formatString)
                ? string.Empty
                : formatString.Replace("{TimeStamp}", TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"))
                    .Replace("{ActivityName}", ActivityName)
                    .Replace("{ActivityPathShort}", ActivityPathShort)
                    .Replace("{ActivityPathLong}", ActivityPathLong)
                    .Replace("{ErrorMessage}", ErrorMessage);
        }
    }
}
