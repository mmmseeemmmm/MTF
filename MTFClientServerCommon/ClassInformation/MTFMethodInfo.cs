using AutomotiveLighting.MTFCommon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace MTFClientServerCommon
{
    [Serializable]
    public class MTFMethodInfo : GenericMethodInfo
    {
        public MTFMethodInfo()
        {
        }

        public MTFMethodInfo(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }

        public MTFMethodInfo(MethodInfo methodInfo)
            : base(methodInfo)
        {
            var monsterMethodAttribute = methodInfo.GetCustomAttribute<MTFMethodAttribute>();
            Description = monsterMethodAttribute.Description;
            DisplayName = string.IsNullOrEmpty(monsterMethodAttribute.DisplayName) ? Name : monsterMethodAttribute.DisplayName;
            
            foreach (var additionalInfo in methodInfo.GetCustomAttributes<MTFAdditionalParameterInfoAttribute>())
            {
                var param = Parameters.FirstOrDefault(p => p.Name == additionalInfo.ParameterName);
                if (param != null)
                {
                    if (!string.IsNullOrEmpty(additionalInfo.DisplayName))
                    {
                        param.DisplayName =  additionalInfo.DisplayName;
                    }
                    param.Description = additionalInfo.Description;
                    param.SetupMethodName = additionalInfo.SetupMethodName;
                    param.DefautlValue = additionalInfo.DefaultValue;
                    param.ValueListName = additionalInfo.ValueListName;
                    param.ValueListLevel = additionalInfo.ValueListLevel;
                    param.ValueListParentName = additionalInfo.ValueListParentName;
                    param.DataNameExtension = additionalInfo.DataNameExtension;
                    param.IsOptional = additionalInfo.IsOptional;
                    param.MinValue = additionalInfo.MinValue;
                    param.MaxValue = additionalInfo.MaxValue;
                }
            }

            SetupModeSupport = monsterMethodAttribute.SetupModeAvailable || Parameters.Any(p => !string.IsNullOrEmpty(p.SetupMethodName));
            if (monsterMethodAttribute.UsedDataNames != null)
            {
                UsedDataNames = new ObservableCollection<string>(monsterMethodAttribute.UsedDataNames);
            }

            foreach (var allowedParameterValue in methodInfo.GetCustomAttributes<MTFAllowedParameterValueAttribute>())
            {
                var param = Parameters.FirstOrDefault(p => p.Name == allowedParameterValue.ParameterName);
                if (param != null)
                {
                    if (param.AllowedValues == null)
                    {
                        param.AllowedValues = new MTFObservableCollection<MTFAllowedValue>();
                    }
                    param.AllowedValues.Add(new MTFAllowedValue { DisplayName = allowedParameterValue.ParameterValueDisplayText, Value = allowedParameterValue.ParameterValue });
                }                
            }

            //order allowed parameters
            foreach (var param in Parameters)
            {
                if (param.AllowedValues != null)
                {
                    var paramClone = param.AllowedValues.Copy() as MTFObservableCollection<MTFAllowedValue>;
                    param.AllowedValues = new MTFObservableCollection<MTFAllowedValue>();
                    foreach (var p in paramClone.OrderBy(item => item.Value))
                    {
                        param.AllowedValues.Add(p.Copy());
                    }
                }
            }
        }

        protected override void FillParameters(MethodInfo methodInfo)
        {
            Parameters = new MTFObservableCollection<MTFParameterInfo>(methodInfo.GetParameters().Select(param => new MTFParameterInfo(param)));
        }

        public string DisplayName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string Description
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool SetupModeSupport
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public IList<string> UsedDataNames
        {
            get { return GetProperty<IList<string>>(); }
            set { SetProperty(value); }
        }

        new public IList<MTFParameterInfo> Parameters
        {
            get { return GetProperty<IList<MTFParameterInfo>>(); }
            set { SetProperty(value); }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Name).Append("(");
            bool firstParam = true;
            foreach (var p in Parameters)
            {
                if (!firstParam)
                {
                    sb.Append(", ");
                }
                else
                {
                    firstParam = false;
                }

                sb.Append(p);
            }
            sb.Append(")");

            return sb.ToString();
        }
    }
}
