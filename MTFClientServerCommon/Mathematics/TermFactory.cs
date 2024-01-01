using System;
using System.Collections.Generic;
using System.Linq;

namespace MTFClientServerCommon.Mathematics
{
    public static class TermFactory
    {
        private static List<Term> allTerms = new List<Term> { 
            new ConstantTerm(),
            new ActivityResultTerm(),
            new VariableTerm(),
            new ValidationTableTerm(),
            new ValidationTableResultTerm(),
            new ExecuteActivityTerm(),
            new EqualTerm(),
            new NotEqualTerm(),
            new AddTerm(),
            new SubtractTerm(),
            new MultiplyTerm(),
            new DivideTerm(),
            new GreaterThanTerm(),
            new LessThanTerm(),
            new GreaterOrEqualTerm(),
            new LessOrEqualTerm(),
            new StringFormatTerm(),
            new NotTerm(),
            new LogicalAndTerm(),
            new LogicalOrTerm(),
            new BitAndTerm(),
            new BitOrTerm(),
            new IsInListTerm(),
            new NotIsInListTerm(),
            new MinTerm(),
            new MaxTerm(),
            new SumTerm(),
            new AvgTerm(),
            new RangeTerm(),
            new CastTerm(),
            new IsNullTerm(),
            new ModuloTerm(),
            new AbsoluteValueTerm(),
            new PowerTerm(),
            new RootTerm(),
            new RoundTerm(),
            new RoundToValueTerm(),
            new RegExMatchTerm(),
            new RegExValueTerm()
        };

        public static List<string> TermSymbols
        {
            get
            {
                return allTerms.Select(t => t.Symbol).ToList();
            }
        }

        public static IEnumerable<string> GetTermSymbols(string typeName)
        {
            TermGroups termGourp = GetTermGroup(typeName);
            return allTerms.Where(t => (t.TermGroup & termGourp) == termGourp).Select(t => t.Symbol);
        }

        private static TermGroups GetTermGroup(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return TermGroups.None;
            }
            Type type = Type.GetType(typeName);
            if (type == null || type.FullName == typeof(object).FullName || type.IsGenericType || type.IsArray)
            {
                return TermGroups.ObjectTerm;
            }
            else
            {
                if (type == typeof(bool))
                {
                    return TermGroups.LogicalTerm;
                }
                else if (type == typeof(string))
                {
                    return TermGroups.StringTerm;
                }
                else if (type.IsValueType)
                {
                    return TermGroups.NumberTerm;
                }
                else
                {
                    return TermGroups.None;
                }
            }
        }

        public static Term GetTermBySymbol(string symbol)
        {
            Term term = allTerms.FirstOrDefault(t => t.Symbol == symbol);
            if (term == null)
            {
                throw new Exception(string.Format("Can't find term impolementation for symbol {0}.", symbol));
            }

            return Activator.CreateInstance(term.GetType()) as Term;
        }

        public static IEnumerable<string> GetTermSymbols(TermGroups childrenTermGroup)
        {
            IEnumerable<string> output = null;
            if ((childrenTermGroup & TermGroups.NumberTerm) == TermGroups.NumberTerm)
            {
                output = allTerms.Where(t => (t.TermGroup & TermGroups.NumberTerm) == TermGroups.NumberTerm).Select(t => t.Symbol);
            }
            if ((childrenTermGroup & TermGroups.LogicalTerm) == TermGroups.LogicalTerm)
            {
                var symbols = allTerms.Where(t => (t.TermGroup & TermGroups.LogicalTerm) == TermGroups.LogicalTerm).Select(t => t.Symbol);
                if (output != null)
                {
                    output = output.Union(symbols);
                }
                else
                {
                    output = symbols;
                }
            }
            if ((childrenTermGroup & TermGroups.StringTerm) == TermGroups.StringTerm)
            {
                var symbols = allTerms.Where(t => (t.TermGroup & TermGroups.StringTerm) == TermGroups.StringTerm).Select(t => t.Symbol);
                if (output != null)
                {
                    output = output.Union(symbols);
                }
                else
                {
                    output = symbols;
                }
            }
            if (output == null)
            {
                output = allTerms.Where(t => (t.TermGroup & TermGroups.None) == TermGroups.None).Select(t => t.Symbol);
            }
            return output;
        }

        public static IEnumerable<Term> GetTerms()
        {
            return allTerms;
        }

        public static Term CreateTerm(Type termType)
        {
            return Activator.CreateInstance(termType) as Term;
        }
    }

    public enum TermGroups
    {
        None,
        NumberTerm,
        LogicalTerm,
        ObjectTerm,
        StringTerm
    }
}
