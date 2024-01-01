using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace MTFClientServerCommon
{
    [Serializable]
    public class GenericClassInfo : MTFDataTransferObject
    {
        public GenericClassInfo()
        {
        }

        public GenericClassInfo(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }

        public GenericClassInfo(Type classType)
        {
            AssemblyName = classType.Module.Name;
            RelativePath = Helpers.FileHelper.GetRelativePath(Constants.BaseConstants.AssembliesPath, classType.Module.FullyQualifiedName);
            var version = classType.Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
            AssemblyVersion = version != null ? version.Version : string.Empty;
            FullName = classType.FullName;

            SetName(classType);

            IsAbstract = classType.IsAbstract;
            IsSealed = classType.IsSealed;
            IsStatic = IsAbstract && IsSealed;
            IsGenericType = classType.IsGenericType;
            IsArray = classType.IsArray;

            FillConstrutors(classType);
            FillMethods(classType);
            FillProperites(classType);
        }

        protected virtual void SetName(Type classType)
        {
            Name = classType.Name;
        }

        protected virtual void FillConstrutors(Type classType)
        {
            Constructors = new MTFObservableCollection<GenericConstructorInfo>();
            classType.GetConstructors().ToList().ForEach(c => Constructors.Add(new GenericConstructorInfo(c)));
        }

        protected virtual void FillMethods(Type classType)
        {
            Methods = new MTFObservableCollection<GenericMethodInfo>();
            classType.GetMethods().ToList().ForEach(m => Methods.Add(new GenericMethodInfo(m)));
        }

        protected virtual void FillProperites(Type classType)
        {
            Properties = new MTFObservableCollection<GenericPropertyInfo>();
            classType.GetProperties().ToList().ForEach(p => Properties.Add(new GenericPropertyInfo(p)));
        }

        public string Name
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public string FullName
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public string FullPathName => System.IO.Path.Combine(Constants.BaseConstants.AssembliesPath, RelativePath);

        public string RelativePath
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public string AssemblyName
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public string AssemblyVersion
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public bool IsAbstract
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        public bool IsSealed
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        public bool IsStatic
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        public bool IsGenericType
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        public bool IsArray
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        public IList<GenericConstructorInfo> Constructors
        {
            get => GetProperty<IList<GenericConstructorInfo>>();
            set => SetProperty(value);
        }

        public IList<GenericMethodInfo> Methods
        {
            get => GetProperty<IList<GenericMethodInfo>>();
            set => SetProperty(value);
        }

        public IList<GenericPropertyInfo> Properties
        {
            get => GetProperty<IList<GenericPropertyInfo>>();
            set => SetProperty(value);
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}
