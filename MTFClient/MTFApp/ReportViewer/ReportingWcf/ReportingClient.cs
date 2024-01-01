using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using MTFClientServerCommon;
using MTFClientServerCommon.DbReporting;
using MTFClientServerCommon.DbReporting.UiReportEntities;
using MTFClientServerCommon.DbReporting.UiReportEntities.SummaryReport;
using MTFClientServerCommon.Helpers;

namespace MTFApp.ReportViewer.ReportingWcf
{
    class ReportingClient
    {
        private readonly string ip;
        private readonly string port;
        private IDbReportingService reportingService;
        private static ReportingClient reportingClient;
        private static object connectionLock = new object();

        public static ReportingClient GetReportingClient()
        {
            lock (connectionLock)
            {
                if (reportingClient == null)
                {
                    reportingClient = new ReportingClient(SettingsClass.SelectedConnection.Host, SettingsClass.SelectedConnection.Port);
                }

                return reportingClient;
            }
        }

        private ReportingClient(string ip, string port)
        {
            this.ip = ip;
            this.port = port;
            Connect();
        }


        private void Connect()
        {
            var myBinding = new NetTcpBinding
            {
                ReceiveTimeout = new TimeSpan(0, 20, 0),
                SendTimeout = new TimeSpan(0, 20, 0),
                MaxReceivedMessageSize = int.MaxValue,
                MaxBufferSize = int.MaxValue,
                Security = { Mode = SecurityMode.None }
            };

            var myEndpoint = new EndpointAddress("net.tcp://" + ip + ":" + port + "/MTFReporting/");
            var myChannelFactory = new ChannelFactory<IDbReportingService>(myBinding, myEndpoint);

            try
            {
                reportingService = myChannelFactory.CreateChannel();
                reportingService.TestConnection();
            }
            catch (Exception e)
            {
                throw new Exception(
                    string.Format("{0}{1}{1}{2}", LanguageHelper.GetString("MTF_Connection_CantConnect"), Environment.NewLine, e.Message),
                    e);
            }
        }


        public Task<List<string>> GetSequenceNames()
        {
            return CallWithReconnect(reportingService.GetSequenceNames);
        }

        public Task<int> GetReportsCount(ReportFilter filter)
        {
            return CallWithReconnect(() => reportingService.GetReportsCount(filter));
        }

        public Task<IEnumerable<SequenceReportPreview>> GetReports(ReportFilter filter)
        {
            return CallWithReconnect(() => reportingService.GetReports(filter, null));
        }

        public Task<IEnumerable<SummaryReportData>> GetReportsCountsPerTimeSlice(ReportFilter filter, int timeSliceInMinutes)
        {
            return CallWithReconnect(() => reportingService.GetReportsCountsPerTimeSlice(filter, timeSliceInMinutes));
        }

        public Task<IEnumerable<double>> GetTableSummaryData(ReportFilter filter, string tableName, string rowName, string columnName)
        {
            return CallWithReconnect(() => reportingService.GetTableSummaryData(filter, tableName, rowName, columnName));
        }

        public Task<SequenceReportDetail> GetDetail(int id)
        {
            return CallWithReconnect(() => reportingService.GetDetail(id));
        }

        public Task<BitmapImage> GetReportImage(string path)
        {
                return Task.Run(() =>
                {
                    using (var dataStream = CallWithReconnect(() => reportingService.GetReportImageData(path)))
                    {
                        return StreamToImage(dataStream);
                    }
                });
        }

        public Task<BitmapImage> GetGraphicalUIImage(string path)
        {
            return Task.Run(() =>
            {
                using (var dataStream = CallWithReconnect(() => reportingService.GetGraphicalUIImageData(path)))
                {
                    return StreamToImage(dataStream);
                }
            });
        }

        public Stream GetPdfReport(int id)
        {
            return CallWithReconnect(() => reportingService.GetPdfReport(id));
        }

        private static BitmapImage StreamToImage(Stream stream)
        {
            using (var memStream = new MemoryStream(ReadFully(stream)))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = memStream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
        }

        private static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        private T CallWithReconnect<T>(Func<T> func)
        {
            try
            {
                return func();
            }
            catch (FaultException)
            {
                throw;
            }
            catch (CommunicationException ex)
            {
                SystemLog.LogMessage(ex.Message);
                Connect();
                return func();
            }
        }


        public Task<List<SummaryReportSettings>> GetSummaryReports()
        {
            return CallWithReconnect(reportingService.GetSummaryReports);
        }

        public Task<List<SummaryReportSettings>> SaveSummarySettings(List<SummaryReportSettings> data)
        {
            return CallWithReconnect(() => reportingService.SaveSummarySetting(data));
        }

        public Task RemoveSummarySettings(List<SummaryReportSettings> data)
        {
            return CallWithReconnect(() => reportingService.RemoveSummarySetting(data));
        }
    }
}
