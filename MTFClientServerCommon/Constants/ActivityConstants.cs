using System;

namespace MTFClientServerCommon.Constants
{
    public static class ActivityNameConstants
    {
        public const string SubSequence = "Activity_SubSequence_Key";
        public const string CallActivity = "Activity_Call_Key";
        public const string DynamicCallActivity = "Activity_CallDynamic_Key";
        public const string ActivityIndexerSeparator = " #";
        public const string CallWholeSequenceKey = "Activity_CallWholeSequence_Key";
        public static readonly Guid CallWholeSequenceId = Guid.Parse("8ec405ec-367c-4b15-9e56-e65814ee85ce");
        public const string ActivityPathSeparator = "->";

    }
}
