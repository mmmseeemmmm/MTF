using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using AutomotiveLighting.MTFCommon;

namespace MTFClientServerCommon.Mathematics
{
    [Serializable]
    public class ExecuteActivityTerm : Term, ISequenceClassInfo
    {
        public ExecuteActivityTerm()
        {
        }

        public ExecuteActivityTerm(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override object Evaluate()
        {
            return Method.Invoke(Instance, EvaluatedParameters);
        }

        [MTFPersistIdOnly]
        public MTFSequenceClassInfo ClassInfo
        {
            get => GetProperty<MTFSequenceClassInfo>();
            set => SetProperty(value);
        }

        public Guid ClassInfoId
        {
            get => GetProperty<Guid>();
            set => SetProperty(value);
        }

        public string MethodName
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public IList<MTFParameterValue> MTFParameters
        {
            get => GetProperty<IList<MTFParameterValue>>();
            set
            {
                SetProperty(value);
                NotifyPropertyChanged(nameof(HasParameters));
            }
        }

        public bool HasParameters => MTFParameters != null && MTFParameters.Count > 0;

        public Type MethodResultType
        {
            get => GetProperty<Type>();
            set => SetProperty(value);
        }

        public object[] EvaluatedParameters { get; set; }

        public object Instance { get; set; }

        public MethodInfo Method { get; set; }

        public override Type ResultType => MethodResultType;
        public override string Symbol => "Exec";
        public override TermGroups TermGroup => TermGroups.None | TermGroups.ObjectTerm;
        public override TermGroups ChildrenTermGroup => TermGroups.None;
        public override MTFIcons Icon => MTFIcons.Execute;
        public override string Label => "Execute";

        public override string ToString()
        {
            return $"Execute({MethodName})";
        }
    }
}