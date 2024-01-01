using MTFClientServerCommon.DbEntities.DbEnums;
using MTFClientServerCommon.MTFValidationTable;
using MTFCommon;

namespace MTFClientServerCommon.DbReporting.Helpers
{
    public static class EnumTransformHelper
    {
        public static DbErrorTypes TransformErrorType(ErrorTypes errorType)
        {
            switch (errorType)
            {
                case ErrorTypes.CheckOutputValue:
                    return DbErrorTypes.CheckOutputValue;
                case ErrorTypes.ComponentError:
                    return DbErrorTypes.ComponentError;
                case ErrorTypes.SequenceError:
                    return DbErrorTypes.SequenceError;
                default:
                    return DbErrorTypes.SequenceError;
            }
        }

        public static ErrorTypes TransformErrorType(DbErrorTypes errorType)
        {
            switch (errorType)
            {
                case DbErrorTypes.CheckOutputValue:
                    return ErrorTypes.CheckOutputValue;
                case DbErrorTypes.ComponentError:
                    return ErrorTypes.ComponentError;
                case DbErrorTypes.SequenceError:
                    return ErrorTypes.SequenceError;
                default:
                    return ErrorTypes.SequenceError;
            }
        }

        public static DbReportValidationTableStatus TransformTableStatus(MTFValidationTableStatus tableStatus)
        {
            switch (tableStatus)
            {
                case MTFValidationTableStatus.NotFilled:
                    return DbReportValidationTableStatus.NotFilled;
                case MTFValidationTableStatus.Ok:
                    return DbReportValidationTableStatus.Ok;
                case MTFValidationTableStatus.Nok:
                    return DbReportValidationTableStatus.Nok;
                case MTFValidationTableStatus.GSFail:
                    return DbReportValidationTableStatus.GsFail;
                default:
                    return DbReportValidationTableStatus.NotFilled;
            }
        }

        public static MTFValidationTableStatus TransformTableStatus(DbReportValidationTableStatus tableStatus)
        {
            switch (tableStatus)
            {
                case DbReportValidationTableStatus.NotFilled:
                    return MTFValidationTableStatus.NotFilled;
                case DbReportValidationTableStatus.Ok:
                    return MTFValidationTableStatus.Ok;
                case DbReportValidationTableStatus.Nok:
                    return MTFValidationTableStatus.Nok;
                case DbReportValidationTableStatus.GsFail:
                    return MTFValidationTableStatus.GSFail;
                default:
                    return MTFValidationTableStatus.NotFilled;
            }
        }

        public static DbReportTableValidationMode TransformValidationMode(MTFValidationTableExecutionMode tableExecutionMode)
        {
            switch (tableExecutionMode)
            {
                case MTFValidationTableExecutionMode.AllRows:
                    return DbReportTableValidationMode.AllRows;
                case MTFValidationTableExecutionMode.OnlySet:
                    return DbReportTableValidationMode.OnlySetRows;
                default:
                    return DbReportTableValidationMode.OnlySetRows;
            }
        }


        public static MTFValidationTableExecutionMode TransformValidationMode(DbReportTableValidationMode tableExecutionMode)
        {
            switch (tableExecutionMode)
            {
                case DbReportTableValidationMode.AllRows:
                    return MTFValidationTableExecutionMode.AllRows;
                case DbReportTableValidationMode.OnlySetRows:
                    return MTFValidationTableExecutionMode.OnlySet;
                default:
                    return MTFValidationTableExecutionMode.OnlySet;
            }
        }
    }
}