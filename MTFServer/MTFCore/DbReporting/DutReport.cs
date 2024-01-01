using System;
using System.Collections.Generic;
using System.Linq;
using AutomotiveLighting.MTFCommon;
using MTFClientServerCommon;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.DbEntities.DbEnums;
using MTFClientServerCommon.DbEntities.DbReportEntities;
using MTFClientServerCommon.DbReporting.Helpers;
using MTFClientServerCommon.GoldSamplePersist;
using MTFClientServerCommon.GraphicalView;
using MTFClientServerCommon.MTFTable;
using MTFClientServerCommon.MTFValidationTable;
using MTFCommon;
using MTFCore.DbReporting.Repositories;

namespace MTFCore.DbReporting
{
    class DutReport : IDisposable
    {
        private readonly Guid dutId;
        private readonly object errorLock = new object();
        private readonly object saveLock = new object();

        private readonly List<ReportError> errors = new List<ReportError>();
        private readonly Dictionary<Guid, int> savedTables = new Dictionary<Guid, int>();
        private readonly ReportImageManager imageManager = new ReportImageManager(BaseConstants.ReportImageBasePath);
        private readonly ReportImageManager graphicalViewManager = new ReportImageManager(BaseConstants.ReportGraphicalViewBasePath);
        private int logCount;
        private string userCycleName;
        private const string LogCyclePrefix = "Cycle";


        public DutReport(Guid dutId)
        {
            this.dutId = dutId;
            SequenceReport = new SequenceReport();
        }


        public SequenceReport SequenceReport { get; set; }

        public SequenceVariantStringValues CurrentVariant { get; private set; }

        public void SetVariant(SequenceVariantStringValues variantStrings)
        {
            SequenceReport.VariantVersion = variantStrings.Version != null
                ? new ReportSequenceVariant {Name = variantStrings.Version, Type = DbSequenceVariantType.Version}
                : null;

            SequenceReport.VariantLightDistribution = variantStrings.LightDistribution != null
                ? new ReportSequenceVariant
                  {Name = variantStrings.LightDistribution, Type = DbSequenceVariantType.LightDistribution}
                : null;

            SequenceReport.VariantMountingSide = variantStrings.MountingSide != null
                ? new ReportSequenceVariant
                  {Name = variantStrings.MountingSide, Type = DbSequenceVariantType.MountingSide}
                : null;

            SequenceReport.VariantGsDut = variantStrings.ProductionDut != null
                ? new ReportSequenceVariant {Name = variantStrings.ProductionDut, Type = DbSequenceVariantType.GsDut}
                : null;
        }

        public void SetStartTime()
        {
            lock (saveLock)
            {
                SequenceReport.StartTime = DateTime.Now;
            }
        }

        public void AddError(DateTime timeStamp, ErrorTypes errorType, string activityName, string msg)
        {
            lock (errorLock)
            {
                errors.Add(new ReportError
                {
                    ActivityName = activityName,
                    TimeStamp = timeStamp,
                    Message = msg,
                    ErrorType = EnumTransformHelper.TransformErrorType(errorType),
                });
            }
        }

        public void ClearErrors()
        {
            lock (errorLock)
            {
                errors.Clear();
            }
        }

        public void SaveMessage(DateTime timeStamp, string msg, MessageRepository repository)
        {
            lock (saveLock)
            {
                var reportMessage = new ReportMessage
                {
                    TimeStamp = timeStamp,
                    Message = msg,
                    SequenceReportId = SequenceReport.Id
                };
                SequenceReport.Messages.Add(reportMessage);
                repository.SaveMessage(reportMessage);
            }
        }

        public void SetSequenceVariant(SequenceVariantStringValues variantStrings)
        {
            SetVariant(variantStrings);
            CurrentVariant = variantStrings.Clone() as SequenceVariantStringValues;
        }

        public void SetSequenceStatus(string sequenceStatus)
        {
            lock (saveLock)
            {
                SequenceReport.SequenceStatus = ConvertStatusToBool(sequenceStatus);
            }
        }

        public void SaveTable(MTFValidationTable table, List<MTFValidationTableRow> rowsToLog,
            ValidationTableRepository repository)
        {
            lock (saveLock)
            {
                if (savedTables.ContainsKey(table.Id))
                {
                    var storedTableId = savedTables[table.Id];
                    var rows = GetRows(rowsToLog, storedTableId);
                    repository.SaveRows(rows);
                }
                else
                {
                    var tableData = new ReportValidationTable
                    {
                        SequenceReportId = SequenceReport.Id,
                        Name = table.Name,
                        ValidationMode = EnumTransformHelper.TransformValidationMode(table.ExecutionMode),
                        TableStatus = EnumTransformHelper.TransformTableStatus(table.Status),
                        ValidationTime = table.TimeOfFirstValidation,
                        Rows = GetRows(rowsToLog)
                    };

                    repository.SaveTable(tableData);
                    savedTables[table.Id] = tableData.Id;
                }
            }
        }

        public void CompleteReport(bool logHiddenRows, bool createNew, bool saveGraphicalView,
            GraphicalViewSetting sequenceGraphicalViewSetting,
            Dictionary<Guid, MTFValidationTable> validationTablesDict, SequenceReportRepository repository)
        {
            lock (saveLock)
            {
                var reportToSave = (SequenceReport)SequenceReport.Clone();
                reportToSave.StopTime = DateTime.Now;
                var currentCount = logCount;

                lock (errorLock)
                {
                    reportToSave.Errors = errors.Select(x => new ReportError
                    {
                        ActivityName = x.ActivityName,
                        Message = x.Message,
                        ErrorType = x.ErrorType,
                        TimeStamp = x.TimeStamp,
                    }).ToList();
                }

                if (createNew)
                {
                    InitSequenceReport(reportToSave.SequenceName, reportToSave.SequenceRunId, repository);
                }

                if (saveGraphicalView && sequenceGraphicalViewSetting != null && sequenceGraphicalViewSetting.HasView)
                {
                    var fileNames = sequenceGraphicalViewSetting.Views.Where(x => x.SaveToReport &&
                                                                                  (dutId == DutConstants.DefaultDutId ||
                                                                                   (x.AssignedDuts?.Contains(dutId) ?? false)))
                        .Select(viewInfo => graphicalViewManager.SaveImage(viewInfo, validationTablesDict)).ToList();

                    if (fileNames.Count > 0)
                    {
                        reportToSave.GraphicalViews = string.Join("|", fileNames);
                    }
                }

                repository.SaveReport(reportToSave, userCycleName ?? $"{LogCyclePrefix}{currentCount:D3}", logHiddenRows);
            }
        }

        public void InitSequenceReport(string sequenceName, int runId, SequenceReportRepository repository)
        {
            SequenceReport = new SequenceReport
            {
                SequenceName = sequenceName,
                SequenceRunId = runId,
            };


            SetStartTime();
            savedTables.Clear();
            logCount++;

            repository.CreateReport(SequenceReport);

            if (CurrentVariant != null)
            {
                SetVariant(CurrentVariant);
            }
        }


        private static bool? ConvertStatusToBool(string status)
        {
            switch (status)
            {
                case BaseConstants.ExecutionStatusOk:
                    return true;
                case BaseConstants.ExecutionStatusNok:
                    return false;
            }

            return null;
        }

        private List<ReportValidationTableRow> GetRows(List<MTFValidationTableRow> rows, int? tableId = null)
        {
            var dbRows = new List<ReportValidationTableRow>();

            if (rows != null)
            {
                foreach (var tableRow in rows.Where(x => x.Status != MTFValidationTableStatus.NotFilled))
                {
                    var row = new ReportValidationTableRow
                    {
                        NumberOfRepetition = (int)tableRow.NumberOfRepetition,
                        Status = EnumTransformHelper.TransformTableStatus(tableRow.Status),
                        GsPercentage = tableRow.GoldSamplePercentage,
                        TimeStamp = DateTime.Now,
                    };
                    AssignDbRowCells(tableRow, row);

                    if (tableId.HasValue)
                    {
                        row.ValidationTableId = tableId.Value;
                    }

                    dbRows.Add(row);
                }

                return dbRows;
            }

            return null;
        }

        private void AssignDbRowCells(MTFValidationTableRow row, ReportValidationTableRow reportRow)
        {
            if (row.RowVariants != null && row.RowVariants.Count > 0 && row.EvaluatedVariant > -1)
            {
                AssignDbRowCells(row.RowVariants[row.EvaluatedVariant].Items.ToList(), reportRow, row.GetActualValueCell());
            }
            else
            {
                AssignDbRowCells(row.Items.ToList(), reportRow, row.GetActualValueCell());
            }
        }

        private void AssignDbRowCells(List<MTFValidationTableCell> items, ReportValidationTableRow reportRow, MTFValidationTableCell actualValue)
        {
            foreach (var item in items)
            {
                switch (item.Type)
                {
                    case MTFTableColumnType.ActualValue:
                        reportRow.ActualValue = AssignActualValue(actualValue, reportRow);
                        break;

                    case MTFTableColumnType.Identification:
                        reportRow.Name = item.Value != null ? item.Value.ToString() : string.Empty;
                        break;
                    case MTFTableColumnType.Hidden:
                        reportRow.IsHidden = item.Value is bool b ? b : false;
                        break;
                    case MTFTableColumnType.Value:
                    case MTFTableColumnType.GoldSample:
                        if (item.IsSet)
                        {
                            AssignParameterValues(item, reportRow);
                        }

                        break;
                }
            }
        }

        private void AssignParameterValues(MTFValidationTableCell item, ReportValidationTableRow reportRow)
        {
            switch (item.Column.Header)
            {
                case ValidationTableConstants.ColumnMin:
                    reportRow.MinValue = item.GetValueAsString();
                    reportRow.MinStatus = item.Status;
                    break;
                case ValidationTableConstants.ColumnMax:
                    reportRow.MaxValue = item.GetValueAsString();
                    reportRow.MaxStatus = item.Status;
                    break;
                case ValidationTableConstants.ColumnRequired:
                    reportRow.RequiredValue = item.GetValueAsString();
                    reportRow.RequiredStatus = item.Status;
                    break;
                case ValidationTableConstants.ColumnProhibited:
                    reportRow.ProhibitedValue = item.GetValueAsString();
                    reportRow.ProhibitedStatus = item.Status;
                    break;
                case ValidationTableConstants.ColumnGs:
                    reportRow.GsValue = item.GetValueAsString();
                    reportRow.GsStatus = item.Status;
                    break;
            }
        }

        private string AssignActualValue(MTFValidationTableCell item, ReportValidationTableRow reportRow)
        {
            if (item.Value is MTFImage image)
            {
                reportRow.HasImage = true;
                string fileName = $"{Guid.NewGuid()}.jpg";
                return imageManager.SaveImage(image, fileName);
            }

            return item.GetValueAsString();
        }


        public void SetRemains(SequenceVariantInfo sequenceVariantInfo, GoldSampleSetting setting)
        {
            if (setting.GoldSampleValidationMode == GoldSampleValidationMode.Count)
            {
                lock (saveLock)
                {
                    SequenceReport.GsRemains = $"{sequenceVariantInfo.NonGoldSampleCount}/{setting.GoldSampleCount}";
                    SequenceReport.GsWarning = sequenceVariantInfo.GoldSampleExpired || sequenceVariantInfo.MissingGoldSample;
                }
            }
            else
            {
                lock (saveLock)
                {
                    SequenceReport.GsRemains =
                        $"{(int)sequenceVariantInfo.NonGoldSampleRemainsMinutes / 60}h{Math.Abs((int)sequenceVariantInfo.NonGoldSampleRemainsMinutes % 60):D2}m";
                    SequenceReport.GsWarning = sequenceVariantInfo.GoldSampleExpired || sequenceVariantInfo.MissingGoldSample;
                }
            }
        }

        public void CheckLastReport(SequenceReportRepository reportRepository)
        {
            bool isEmpty;
            lock (saveLock)
            {
                isEmpty = SequenceReport.IsEmpty && savedTables.Count == 0;
            }

            if (isEmpty)
            {
                RemoveReport(reportRepository);
            }
            else
            {
                SetCycleName("On stop");
                CompleteReport(false, false, false, null, null, reportRepository);
            }
        }

        private void RemoveReport(SequenceReportRepository reportRepository)
        {
            lock (saveLock)
            {
                reportRepository.RemoveReport(SequenceReport);
            }
        }

        public void SetCycleName(string cycleName)
        {
            userCycleName = cycleName;
        }

        public void Dispose()
        {
            lock (saveLock)
            {
                savedTables?.Clear();
            }
        }
    }
}