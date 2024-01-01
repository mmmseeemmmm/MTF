using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTFCommon;
using MTFClientServerCommon;

namespace MTFCore
{
    class ActivityErrorsHandler
    {
        private Dictionary<MTFActivityError, GuidContainer> allVariables;
        private readonly Dictionary<Guid, List<MTFActivityError>> internalActivityErrors = new Dictionary<Guid, List<MTFActivityError>>();
        private readonly object activityErrorsLock = new object();
        private Guid DefaultDUTId = new Guid("80A9DF35-844F-45BA-9418-4590AB810586");
        private IEnumerable<DeviceUnderTestInfo> sequenceDuts;

        private List<MTFActivityError> activityErrors(Guid? dutId) => internalActivityErrors[dutId ?? DefaultDUTId];


        public bool HasError(Guid? dutId)
        {
            lock (activityErrorsLock)
            {
                return activityErrors(dutId).Count > 0;
            }
        }

        public void AddError(Guid? dutId, MTFActivityError error)
        {
            lock (activityErrorsLock)
            {
                activityErrors(dutId).Add(error);
            }
        }

        public void Clear(Guid? dutId)
        {
            lock (activityErrorsLock)
            {
                activityErrors(dutId).Clear();
            }
        }

        public List<MTFActivityError> GetErrors(Guid? dutId)
        {
            lock (activityErrorsLock)
            {
                return activityErrors(dutId).ToList();
            }
        }

        public void InitDuts(IEnumerable<DeviceUnderTestInfo> sequenceDeviceUnderTestInfos)
        {
            sequenceDuts = sequenceDeviceUnderTestInfos;
            internalActivityErrors.Clear();
            if (sequenceDeviceUnderTestInfos?.FirstOrDefault() != null)
            {
                DefaultDUTId = sequenceDeviceUnderTestInfos.FirstOrDefault().Id;
                foreach (var dut in sequenceDeviceUnderTestInfos)
                {
                    internalActivityErrors[dut.Id] = new List<MTFActivityError>();
                }
            }
            else
            {
                internalActivityErrors[DefaultDUTId] = new List<MTFActivityError>();
            }
        }

    }
}
