using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.DbReporting.UiReportEntities;
using MTFCommon;

namespace MTFCore.ReportBuilder
{
    abstract class ReportDetailBuilder
    {
        private const string resourcesPath = "Resources";
        private const string alLogoImageName = "alLogo.png";
        private const string machineImageName = "machine.png";
        private const string userImageName = "user.png";
        private const string errorsImageName = "Errors.png";
        private const string messageImageName = "message.png";
        private const string variantImageName = "goldSample.png";
        private const string startTimeImageName = "startTime.png";
        private const string stopTimeImageName = "stopTime.png";
        private const string durationImageName = "duration.png";
        private const string goldSampleLifeImageName = "hourGlass.png";

        private SequenceReportDetail reportDetail;
        protected ReportDetailBuilder(SequenceReportDetail reportDetail)
        {
            this.reportDetail = reportDetail;
        }

        public Stream Build()
        {
            Init();
            
            AppendHeader(reportDetail);
            if (!string.IsNullOrEmpty(reportDetail.GraphicalViews))
            {
                AppendGraphicalViewImages(reportDetail.GraphicalViews.Split('|').Select(GetGraphicalViewImagePath).ToArray());
            }

            var errorImages = reportDetail.ValidationTables.SelectMany(t => t.Rows).Where(r => r.HasImage == true && r.Status == MTFValidationTableStatus.Nok).Select(r => GetImagePath(r.ActualValue)).ToArray();
            if (errorImages.Any())
            {
                AppendErrorImages(errorImages);
            }

            AppendTables(reportDetail.ValidationTables);

            if (reportDetail.Messages.Any())
            {
                AppendMessages(reportDetail.Messages);
            }

            if (reportDetail.Errors.Any())
            {
                AppendErrors(reportDetail.Errors);
            }

            return End();
        }

        protected Color AlYellowColor = Color.FromArgb(255,254,193,0);
        protected Color AlGreenColor = Color.FromArgb(255,134,222,117);
        protected Color AlLightGreenColor = Color.FromArgb(255,183,226,137);
        protected Color AlRedColor = Color.FromArgb(255,255,0,0);
        protected Color AlLightRedColor = Color.FromArgb(255,255,94,94);
        protected Color AlSilverColor = Color.FromArgb(255,201,201,201);
        protected Color AlLightSilverColor = Color.FromArgb(255, 221, 221, 221);

        protected  string GetImagePath(string path) => Path.Combine(BaseConstants.DataPath, BaseConstants.ReportImageBasePath, path);
        protected  string GetGraphicalViewImagePath(string path) => Path.Combine(BaseConstants.DataPath, BaseConstants.ReportGraphicalViewBasePath, path);

        protected string AlLogoFilePath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), resourcesPath, alLogoImageName);
        protected string MachineImagePath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), resourcesPath, machineImageName);
        protected string UserImagePath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), resourcesPath, userImageName);
        protected string ErrorsImagePath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), resourcesPath, errorsImageName);
        protected string MessageImagePath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), resourcesPath, messageImageName);
        protected string VariantImagePath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), resourcesPath, variantImageName);
        protected string StartTimeImagePath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), resourcesPath, startTimeImageName);
        protected string StopTimeImagePath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), resourcesPath, stopTimeImageName);
        protected string DurationImagePath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), resourcesPath, durationImageName);
        protected string GoldSampleLifeImagePath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), resourcesPath, goldSampleLifeImageName);

        protected abstract void Init();

        protected abstract void AppendHeader(SequenceReportDetail reportDetail);
        protected abstract void AppendTables(IEnumerable<SequenceReportValidationTableDetail> validationTable);
        protected abstract void AppendErrorImages(IList<string> images);
        protected abstract void AppendGraphicalViewImages(IList<string> images);
        protected abstract void AppendMessages(IList<SequenceReportMessageDetail> messages);
        protected abstract void AppendErrors(IList<SequenceReportErrorDetail> errors);

        protected abstract Stream End();

    }
}
