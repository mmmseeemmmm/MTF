using MTFClientServerCommon.Constants;

namespace MTFClientServerCommon.Helpers
{
    public static class ValidationTableHelper
    {
        public static string SetDisplayKey(string header)
        {
            switch (header)
            {
                case ValidationTableConstants.ColumnMin:
                    return  ValidationTableConstants.ColumnMinKey;
                case ValidationTableConstants.ColumnMax:
                    return  ValidationTableConstants.ColumnMaxKey;
                case ValidationTableConstants.ColumnRequired:
                    return  ValidationTableConstants.ColumnRequiredKey;
                case ValidationTableConstants.ColumnProhibited:
                    return  ValidationTableConstants.ColumnProhibitedKey;
                case ValidationTableConstants.ColumnActual:
                    return  ValidationTableConstants.ColumnActualKey;
                case ValidationTableConstants.ColumnName:
                    return  ValidationTableConstants.ColumnNameKey;
                case ValidationTableConstants.ColumnHidden:
                    return  ValidationTableConstants.ColumnHiddenKey;
                case ValidationTableConstants.ColumnGs:
                    return  ValidationTableConstants.ColumnGsKey;
            }
            return null;
        }
    }
}
