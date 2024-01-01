using System;
using System.Linq;
using System.Runtime.Serialization;

namespace MTFClientServerCommon.GoldSamplePersist
{
    [Serializable]
    public class SequenceVariantInfo : MTFDataTransferObject
    {
        private bool isCurrent;

        public SequenceVariantInfo()
            : base()
        {

        }

        public SequenceVariantInfo(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
        public SequenceVariant SequenceVariant
        {
            get { return GetProperty<SequenceVariant>(); }
            set { SetProperty(value); }
        }

        public DateTime GoldSampleDate
        {
            get { return GetProperty<DateTime>(); }
            set { SetProperty(value); }
        }

        public double NonGoldSampleCount
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public double NonGoldSampleRemainsMinutes
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public double NonGoldSampleShiftRemainsMinutes
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public double ElapsedMinutes
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public bool GoldSampleExpired
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsGoldSample
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool MissingGoldSample
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool AllowMoreGS
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsCurrent
        {
            get { return isCurrent; }
            set
            {
                isCurrent = value;
                NotifyPropertyChanged();
            }
        }


        public void Reset(GoldSampleSetting setting, DateTime now)
        {
            GoldSampleDate = now;
            GoldSampleExpired = false;
            MissingGoldSample = false;
            NonGoldSampleCount = 0;

            if (setting.GoldSampleValidationMode == GoldSampleValidationMode.Time)
            {
                NonGoldSampleRemainsMinutes = setting.GoldSampleMinutes;
            }
            else if (setting.GoldSampleValidationMode == GoldSampleValidationMode.Shift)
            {
                if (setting.GoldSampleShifts != null)
                {
                    var shifts = setting.GetSortedShift();
                    if (shifts.Count > 0)
                    {
                        var shift =
                            shifts.FirstOrDefault(
                                x => now.Hour < x.ShiftBeginningHour || (now.Hour == x.ShiftBeginningHour && now.Minute < x.ShiftBeginningMinute));
                        if (shift == null)
                        {
                            shift = shifts.First();
                            shift.ShiftBeginningHour += 24;
                        }
                        NonGoldSampleRemainsMinutes = (shift.ShiftBeginningHour - now.Hour) * 60 + shift.ShiftBeginningMinute - now.Minute;
                    }
                    else
                    {
                        NonGoldSampleRemainsMinutes = 0;
                    }
                    NonGoldSampleShiftRemainsMinutes = NonGoldSampleRemainsMinutes;
                }
            }
        }


        public void Increase(GoldSampleSetting setting, DateTime? lastTime, DateTime now)
        {
            switch (setting.GoldSampleValidationMode)
            {
                case GoldSampleValidationMode.Count:
                    NonGoldSampleCount++;
                    break;
                case GoldSampleValidationMode.Time:
                    ElapsedMinutes = lastTime.HasValue ? (now - lastTime.Value).TotalMinutes : 0;
                    NonGoldSampleRemainsMinutes = setting.GoldSampleMinutes - (now - GoldSampleDate).TotalMinutes;
                    break;
                case GoldSampleValidationMode.Shift:
                    ElapsedMinutes = lastTime.HasValue ? (now - lastTime.Value).TotalMinutes : 0;
                    NonGoldSampleRemainsMinutes = NonGoldSampleShiftRemainsMinutes - (now - GoldSampleDate).TotalMinutes;
                    break;
            }
        }


        public bool IsValid(GoldSampleSetting setting)
        {
            switch (setting.GoldSampleValidationMode)
            {
                case GoldSampleValidationMode.Count:
                    return NonGoldSampleCount <= setting.GoldSampleCount;
                case GoldSampleValidationMode.Time:
                case GoldSampleValidationMode.Shift:
                    return NonGoldSampleRemainsMinutes > 0;
            }
            return true;
        }
    }
}
