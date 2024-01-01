namespace MTFClientServerCommon.MTFAccessControl
{
    public static class AccessRoleNameConvertor
    {
        /// <summary>
        /// Converts role name to LocTextKey for translation
        /// </summary>
        /// <remarks>
        /// sequenceEditor -> AccessRole_sequenceEditor";
        /// componentConfiguration -> AccessRole_componentConfiguration";
        /// teach -> AccessRole_teach";
        /// mtfSetting -> AccessRole_mtfSetting";
        /// debug -> AccessRole_debug";
        /// resultViewer -> AccessRole_resultViewer";
        /// noDestroy -> AccessRole_noDestroy";
        /// service -> AccessRole_service";
        /// hideCommands -> AccessRole_hideCommands";
        /// serviceReadOnly -> AccessRole_serviceReadOnly";
        /// </remarks>
        /// <param name="roleName">AccessRole name given from USB flash drive</param>
        /// <returns></returns>
        public static string ConvertAccessName(string roleName)
        {
            switch (roleName)
            {
                case "sequenceEditor":
                    return "AccessRole_sequenceEditor";
                case "componentConfiguration":
                    return "AccessRole_componentConfiguration";
                case "teach":
                    return "AccessRole_teach";
                case "mtfSetting":
                    return "AccessRole_mtfSetting";
                case "debug":
                    return "AccessRole_debug";
                case "resultViewer":
                    return "AccessRole_resultViewer";
                case "noDestroy":
                    return "AccessRole_noDestroy";
                case "service":
                    return "AccessRole_service";
                case "hideCommands":
                    return "AccessRole_hideCommands";
                case "serviceReadOnly":
                    return "AccessRole_serviceReadOnly";
            }
            return roleName;
        }
    }
}
