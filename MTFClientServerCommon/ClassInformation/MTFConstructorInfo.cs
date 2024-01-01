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
    public class MTFConstructorInfo : GenericConstructorInfo
    {
        public MTFConstructorInfo()
            : base()
        {
        }
        
        public MTFConstructorInfo(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }

        public MTFConstructorInfo(ConstructorInfo constructorInfo)
            : base(constructorInfo)
        {
            Description = constructorInfo.GetCustomAttribute<MTFConstructorAttribute>().Description;
            ParameterHelperClassName = constructorInfo.GetCustomAttribute<MTFConstructorAttribute>().ParameterHelperClassName;

            foreach (var additionalInfo in constructorInfo.GetCustomAttributes<MTFAdditionalParameterInfoAttribute>())
            {
                var param = Parameters.FirstOrDefault(p => p.Name == additionalInfo.ParameterName);
                if (param != null)
                {
                    if (!string.IsNullOrEmpty(additionalInfo.DisplayName))
                    {
                        param.DisplayName = additionalInfo.DisplayName;
                    }
                    param.Description = additionalInfo.Description;
                    param.DefautlValue = additionalInfo.DefaultValue;
                    param.IsOptional = additionalInfo.IsOptional;
                    param.MinValue = additionalInfo.MinValue;
                    param.MaxValue = additionalInfo.MaxValue;
                }
            }

            foreach (var allowedParameterValue in constructorInfo.GetCustomAttributes<MTFAllowedParameterValueAttribute>())
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

        protected override void FillParameters(ConstructorInfo constructorInfo)
        {
            Parameters = new MTFObservableCollection<MTFParameterInfo>(constructorInfo.GetParameters().Select(param => new MTFParameterInfo(param)));            
        }

        public string Description
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string ParameterHelperClassName
        {
            get { return GetProperty<string>(); }
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
