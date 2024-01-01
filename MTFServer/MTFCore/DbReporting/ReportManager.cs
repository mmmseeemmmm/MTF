using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MTFClientServerCommon;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.DbEntities.DbReportEntities;
using MTFClientServerCommon.GraphicalView;
using MTFClientServerCommon.MTFValidationTable;
using MTFCommon;
using MTFCore.DbReporting.Repositories;

namespace MTFCore.DbReporting
{
    class ReportManager : IDisposable
    {
        private readonly string sequenceName;
        private readonly IList<RoundingRule> roundingRules;
        private readonly Dictionary<Guid, DutReport> sequenceReports = new Dictionary<Guid, DutReport>();
        private ReportSequenceRun sequenceRun = new ReportSequenceRun();
        private bool canLog;

        private SequenceReportRepository ReportRepository { get; }
        private MessageRepository MessageRepository { get; }
        private ValidationTableRepository ValidationTableRepository { get; }

        public ReportManager(string sequenceName, IList<RoundingRule> roundingRules, ServerSettings serverSettings)
        {
            this.sequenceName = sequenceName;
            this.roundingRules = roundingRules;
            var setting = serverSettings ?? new ServerSettings();
            canLog = setting.AllowXmlLogging;

            if (canLog)
            {
                ReportRepository = new SequenceReportRepository();
                MessageRepository = new MessageRepository();
                ValidationTableRepository = new ValidationTableRepository();
            }
        }

        #region Public

        public void CompleteReport(bool logHiddenRows, bool createNew, bool saveGraphicalView,
            GraphicalViewSetting sequenceGraphicalViewSetting, Dictionary<Guid, MTFValidationTable> validationTablesDict, Guid? dut)
        {
            if (!canLog)
            {
                return;
            }

            GetDutReport(dut).CompleteReport(logHiddenRows, createNew, saveGraphicalView, sequenceGraphicalViewSetting, validationTablesDict, ReportRepository);
        }

        public DutReport GetDutReport(Guid? id)
        {
            return id.HasValue ? sequenceReports[id.Value] : sequenceReports.First().Value;
        }


        public void SetStartTime(Guid? dut)
        {
            if (!canLog)
            {
                return;
            }

            GetDutReport(dut).SetStartTime();
        }

        public void SaveMessageAsync(DateTime timeStamp, string msg, Guid? dut)
        {
            if (!canLog)
            {
                return;
            }

            Task.Run(() => { GetDutReport(dut).SaveMessage(timeStamp, msg, MessageRepository); });
        }

        public void AddError(DateTime timeStamp, ErrorTypes errorType, string activityName, string msg, Guid? dut)
        {
            if (!canLog)
            {
                return;
            }

            GetDutReport(dut).AddError(timeStamp, errorType, activityName, msg);
        }

        public void AddError(ErrorTypes errorType, string activityName, string msg, Guid? dut)
        {
            if (!canLog)
            {
                return;
            }

            AddError(DateTime.Now, errorType, activityName, msg, dut);
        }

        public void ClearErrors(Guid? dut)
        {
            if (!canLog)
            {
                return;
            }

            GetDutReport(dut).ClearErrors();
        }

        public void SaveTableAsync(MTFValidationTable table, List<MTFValidationTableRow> rowsToLog, Guid? dut)
        {
            if (!canLog || !table.SaveToLog)
            {
                return;
            }

            Task.Run(() => { GetDutReport(dut).SaveTable(table, rowsToLog, ValidationTableRepository); });
        }

        public void CreateLastReport()
        {
            if (!canLog)
            {
                return;
            }

            foreach (var dutReport in sequenceReports.Values)
            {
                dutReport.CheckLastReport(ReportRepository);
            }

            CompleteRun();
        }

        public void SetSequenceStatus(string sequenceStatus, Guid? dut)
        {
            if (!canLog)
            {
                return;
            }

            GetDutReport(dut).SetSequenceStatus(sequenceStatus);
        }

        public void SetSequenceVariant(SequenceVariant variant, Guid? dut)
        {
            if (!canLog)
            {
                return;
            }

            if (variant != null)
            {
                GetDutReport(dut).SetSequenceVariant(variant.GetStringValues());
            }
        }


        public void SetRemains(MTFClientServerCommon.GoldSamplePersist.SequenceVariantInfo sequenceVariantInfo, GoldSampleSetting setting, Guid? dut)
        {
            if (!canLog || sequenceVariantInfo == null)
            {
                return;
            }

            GetDutReport(dut).SetRemains(sequenceVariantInfo, setting);
        }

        public void SetCycleName(string cycleName, Guid? dut)
        {
            if (!canLog)
            {
                return;
            }

            GetDutReport(dut).SetCycleName(cycleName);
        }

        public static void InitializeDatabase()
        {
            using (var db = new DbReportingContext())
            {
                db.Database.Migrate();
            }
        }

        public void InitializeReporting(IEnumerable<Guid> duts)
        {
            if (!canLog)
            {
                return;
            }

            sequenceReports.Clear();

            try
            {
                InitSequenceRun();

                if (duts!=null && duts.Any())
                {
                    foreach (var dut in duts)
                    {
                        sequenceReports[dut] = new DutReport(dut);
                        InitSequenceReport(dut);
                    }
                }
                else
                {
                    sequenceReports[DutConstants.DefaultDutId] = new DutReport(DutConstants.DefaultDutId);
                    InitSequenceReport(DutConstants.DefaultDutId);
                }

                
            }
            catch (Exception)
            {
                canLog = false;
                throw;
            }
        }

        #endregion


        #region Private

        private void InitSequenceRun()
        {
            sequenceRun = new ReportSequenceRun
            {
                SequenceName = sequenceName,
                Machine = Environment.MachineName,
                WinUser = Environment.UserName,
                RoundingRules = GetRoundingRules(),
            };
            ReportRepository.CreateSequenceRun(sequenceRun);
        }

        private void InitSequenceReport(Guid dut)
        {
            GetDutReport(dut).InitSequenceReport(sequenceName, sequenceRun.Id, ReportRepository);
        }

       

        private List<ReportRoundingRules> GetRoundingRules()
        {
            return roundingRules?.Select(x => new ReportRoundingRules { Min = x.Min, Max = x.Max, Digits = x.Digits }).ToList();
        }

        private void CompleteRun()
        {
            ReportRepository.SaveSequenceRun(sequenceRun.Id, DateTime.Now);
        }

        #endregion


        public void Dispose()
        {
            foreach (var dutReport in sequenceReports.Values)
            {
                dutReport.Dispose();
            }
        }
    }
}