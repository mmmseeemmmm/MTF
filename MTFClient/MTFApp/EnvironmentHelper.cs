using MTFClientServerCommon.MTFAccessControl;
using System;
using System.Linq;

namespace MTFApp
{
    static class EnvironmentHelper
    {
        private static bool? isProductionMode;
        public static bool IsProductionMode
        {
            get
            {
                if (isProductionMode != null)
                {
                    return (bool)isProductionMode;
                }

                return Environment.GetCommandLineArgs().Any(i => i.ToUpper() == "-PRODUCTIONMODE");
            }
            set
            {
                isProductionMode = value;
            }
        }

        public static AccessKey CurrentAccessKey { get; set; }

        public static string UserName
        {
            get { return CurrentAccessKey != null ? string.Format("{0} {1}", CurrentAccessKey.KeyOwnerFirstName, CurrentAccessKey.KeyOwnerLastName) : "Unknown user"; }
        }

        public static bool HasAccessKeyRole(string roleName)
        {
            return HasAccessKeyRole(roleName, true);
        }

        public static bool HasAccessKeyRole(string roleName, bool trueInDebug)
        {
#if DEBUG
            if (trueInDebug)
            {
                return true; 
            }
#endif
            return (CurrentAccessKey != null && CurrentAccessKey.HasRole(roleName));
        }
    }
}
