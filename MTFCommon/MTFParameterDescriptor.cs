using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace AutomotiveLighting.MTFCommon
{
    
    [KnownType(typeof(MTFDataTable))]
    [KnownType(typeof(MTFTabControl))]
    [KnownType(typeof(List<MTFTabControl>))]
    [KnownType(typeof(List<List<List<object>>>))]
    public class MTFParameterDescriptor : INotifyPropertyChanged, ICloneable
    {
        private bool isEnabled;
        public bool IsEnabled 
        {
            get => isEnabled;
            set { isEnabled = value; NotifyPropertyChanged(); }
        }

        private object val;
        public object Value
        {
            get => val;
            set { val = value; NotifyPropertyChanged(); }
        }

        private ValueWithName[] allowedValues;
        public ValueWithName[] AllowedValues
        {
            get => allowedValues;
            set { allowedValues = value; NotifyPropertyChanged(); }
        }

        public string ParameterName { get; set; }

        private string displayName;
        public string DisplayName
        {
            get => displayName;
            set { displayName = value; NotifyPropertyChanged(); }
        }

        private string description;
        public string Description 
        {
            get => description;
            set { description = value; NotifyPropertyChanged(); }
        }

        private MTFParameterControlType controlType;
        public MTFParameterControlType ControlType 
        {
            get => controlType;
            set { controlType = value; NotifyPropertyChanged(); }
        }

        private string typeName;
        public string TypeName
        {
            get => typeName;
            set { typeName = value; NotifyPropertyChanged(); }
        }

        public bool DontRaiseNotifyPropertyChanged { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (DontRaiseNotifyPropertyChanged)
            {
                return;
            }

            if (!string.IsNullOrEmpty(propertyName))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public object Clone()
        {
            return new MTFParameterDescriptor
            {
                Value = Value,
                ParameterName = ParameterName,
                IsEnabled = IsEnabled,
                DisplayName = DisplayName,
                Description = Description,
                ControlType = ControlType,
                AllowedValues = AllowedValues
            };
        }
    }

    public class ValueWithName
    {
        public string DisplayName { get; set; }
        public object Value { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is ValueWithName)
            {
                return ((ValueWithName)obj).DisplayName == DisplayName && ((ValueWithName)obj).Value.Equals(Value);
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public enum MTFParameterControlType
    {
        TextInput,
        ListBox,
        DataTable,
        TabControl,
    }
}
