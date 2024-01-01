using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using AutomotiveLighting.MTFCommon;
using MTFClientServerCommon.Helpers;

namespace MTFClientServerCommon
{
    [Serializable]
    public class MTFErrorHandlingActivity : MTFSequenceActivity
    {
        public MTFErrorHandlingActivity()
            : base()
        {
            ErrorRenderActivityPathShort = true;
            ErrorRenderErroMessage = true;
            IncludeValidationTables = true;
        }

        public MTFErrorHandlingActivity(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        public override string ObjectVersion => "1.0.1";

        protected override void VersionConvert(string fromVersion)
        {
            base.VersionConvert(fromVersion);

            if (fromVersion == "0.0.0")
            {
                ErrorRenderActivityPathShort = true;
                ErrorRenderErroMessage = true;
                fromVersion = "1.0.0";
            }
            if (fromVersion == "1.0.0")
            {
                IncludeValidationTables= true;
                fromVersion = "1.0.1";
            }
        }

        public MTFStringFormat RaiseError
        {
            get => GetProperty<MTFStringFormat>();
            set => SetProperty(value);
        }

        public ErrorHandlingType ErrorHandlingType
        {
            get => GetProperty<ErrorHandlingType>();
            set
            {
                SetProperty(value);
                ActivityName = value.Description();

                if (value == ErrorHandlingType.GetErrorImages)
                {
                    ReturnType = typeof(MTFImage[]).FullName;
                }
                else if (value == ErrorHandlingType.GetErrors || value == ErrorHandlingType.GetInvalidValidationTables ||
                         value == ErrorHandlingType.GetLastErrorActivityName)
                {
                    ReturnType = typeof(string).FullName;
                }
                else
                {
                    ReturnType = typeof(bool).FullName;
                }
            }
        }

        public bool CleanErrorWindow
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        public override MTFIcons Icon => MTFIcons.ErrorHandling;

        public bool ErrorRenderTimeStamp
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        public bool ErrorRenderActivityName
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        public bool ErrorRenderActivityPathShort
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        public bool ErrorRenderActivityPathLong
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        public bool ErrorRenderErroMessage
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        public bool IncludeValidationTables
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

    }

    public enum ErrorHandlingType
    {
        [Description("Activity_ErrorHandling_Type_CheckErr")]
        CheckErrors,
        [Description("Activity_ErrorHandling_Type_CleanErr")]
        CleanErrors,
        [Description("Activity_ErrorHandling_Type_RaiseErr")]
        RaiseError,
        [Description("Activity_ErrorHandling_Type_GetErr")]
        GetErrors,
        [Description("Activity_ErrorHandling_Type_GetTables")]
        GetInvalidValidationTables,
        [Description("Activity_ErrorHandling_Type_GetLastActivityErr")]
        GetLastErrorActivityName,
        [Description("Activity_ErrorHandling_Type_ErrImg")]
        GetErrorImages,
    }
}
