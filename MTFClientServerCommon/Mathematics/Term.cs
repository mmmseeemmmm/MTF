using AutomotiveLighting.MTFCommon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MTFClientServerCommon.Mathematics
{
    [XmlInclude(typeof(ActivityResultTerm))]
    [XmlInclude(typeof(AddTerm))]
    [XmlInclude(typeof(ConstantTerm))]
    [XmlInclude(typeof(EmptyTerm))]
    [XmlInclude(typeof(EqualTerm))]
    [XmlInclude(typeof(GreaterOrEqualTerm))]
    [XmlInclude(typeof(GreaterThanTerm))]
    [XmlInclude(typeof(LessOrEqualTerm))]
    [XmlInclude(typeof(LessThanTerm))]
    [XmlInclude(typeof(NotEqualTerm))]
    [XmlInclude(typeof(NotTerm))]
    [XmlInclude(typeof(IsInListTerm))]
    [XmlInclude(typeof(StringFormatTerm))]
    public abstract class Term : MTFDataTransferObject
    {
        public Term()
            : base()
        {
        }

        public Term(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public Term(string targetTypeName)
        {
            this.TargetType = targetTypeName;
        }

        public override string ObjectVersion
        {
            //version 1.0.1 - activity result term indexer is stored without property name, other terms are without changes
            get { return "1.0.1"; }
        }

        protected override void VersionConvert(string fromVersion)
        {
            base.VersionConvert(fromVersion);
        }

        public abstract object Evaluate();
        public abstract Type ResultType { get; }
        public abstract string Symbol { get; }
        public abstract TermGroups TermGroup { get; }
        public abstract TermGroups ChildrenTermGroup { get; }
        public abstract MTFIcons Icon { get; }
        public abstract string Label { get; }
        public IEnumerable<string> TermSymbols
        {
            get
            {
                return TermFactory.GetTermSymbols(TermGroups.None);
            }
        }

        public IEnumerable<Term> TermCollection
        {
            get
            {
                return TermFactory.GetTerms();
            }
                
        }

        public string TargetType
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public TermGroups TargetGroup
        {
            get { return GetProperty<TermGroups>(); }
            set { SetProperty(value); }
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public virtual string ToStringAsSubterm()
        {
            return string.Format("({0})", ToString());
        }

    }
}
