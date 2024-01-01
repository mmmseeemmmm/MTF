using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon.Mathematics
{
    public static class TermHelper
    {
        public static void ForEachTerm<T>(this Term term, Action<T> func)
            where T : Term
        {
            if (term is T)
            {
                func((T)term);
            }

            if (term is UnaryTerm)
            {
                ForEachTerm<T>(((UnaryTerm)term).Value, func);
            }
            else if (term is BinaryTerm)
            {
                ForEachTerm<T>(((BinaryTerm)term).Value1, func);
                ForEachTerm<T>(((BinaryTerm)term).Value2, func);
            }
            else if (term is ListTerm)
            {
                ForEachTerm<T>(((ListTerm)term).Value1, func);
                if (((ListTerm)term).Value2 is Term)
                {
                    ForEachTerm<T>(((ListTerm)term).Value2 as Term, func);
                }
            }
            else if (term is TermWrapper)
            {
                if (((TermWrapper)term).Value is Term)
                {
                    ForEachTerm<T>((Term)((TermWrapper)term).Value, func);
                }
            }
            else if (term is StringFormatTerm)
            {
                var stringFormatTerm = term as StringFormatTerm;
                if (stringFormatTerm.Value.Parameters != null)
                {
                    foreach (var t in stringFormatTerm.Value.Parameters)
                    {
                        ForEachTerm<T>(t, func);
                    }
                }
            }
            else if (term is ListOperationTerm)
            {
                var listOperationTerm = term as ListOperationTerm;
                if (listOperationTerm.Value.Parameters != null)
                {
                    foreach (var t in listOperationTerm.Value.Parameters)
                    {
                        ForEachTerm<T>(t, func);
                    }
                }
            }
            else if (term is ActivityResultTerm && (term as ActivityResultTerm).PropertyPath != null)
            {
                foreach (var item in (term as ActivityResultTerm).PropertyPath)
                {
                    if (item is GenericPropertyIndexer && (item as GenericPropertyIndexer).Index is Term)
                    {
                        ForEachTerm<T>((item as GenericPropertyIndexer).Index as Term, func);
                    }
                }
            }
            else if (term is ValidationTableTerm)
            {
                if ((term as ValidationTableTerm).Rows != null)
                {
                    foreach (var item in (term as ValidationTableTerm).Rows)
                    {
                        ForEachTerm<T>(item.ActualValue, func);
                    }
                }
            }
        }

        public static Term GetParentTerm(this Term termTree, Term parent, Term term)
        {
            Term foundParent = null;
            if (term == termTree)
            {
                return parent;
            }
            switch (termTree)
            {
                case UnaryTerm unaryTerm:
                    foundParent = GetParentTerm(unaryTerm.Value, unaryTerm, term);
                    break;
                case BinaryTerm binaryTerm:
                    foundParent = GetParentTerm(binaryTerm.Value1, binaryTerm, term) ?? GetParentTerm(binaryTerm.Value2, binaryTerm, term);
                    break;
                case ListTerm listTerm:
                    foundParent = GetParentTerm(listTerm.Value1, listTerm, term);
                    break;
                case StringFormatTerm stringFormatTerm:
                {
                    if (stringFormatTerm.Value?.Parameters != null)
                    {
                        foreach (var item in stringFormatTerm.Value.Parameters)
                        {
                            foundParent = GetParentTerm(item, stringFormatTerm, term);
                            if (foundParent != null)
                            {
                                return foundParent;
                            }
                        }
                    }
                    break;
                }
                case ListOperationTerm listOperationTerm:
                {
                    if (listOperationTerm.Value?.Parameters != null)
                    {
                        foreach (var item in listOperationTerm.Value.Parameters)
                        {
                            foundParent = GetParentTerm(item, listOperationTerm, term);
                            if (foundParent != null)
                            {
                                return foundParent;
                            }
                        }
                    }
                    break;
                }
                case ValidationTableTerm validationTableTerm:
                {
                    if (validationTableTerm.Rows != null)
                    {
                        foreach (var item in validationTableTerm.Rows)
                        {
                            foundParent = GetParentTerm(item.ActualValue, validationTableTerm, term);
                            if (foundParent != null)
                            {
                                return foundParent;
                            }
                        }
                    }

                    break;
                }
                case ExecuteActivityTerm executeActivityTerm:
                    //executeActivityTerm.
                    if (executeActivityTerm.MTFParameters!=null)
                    {
                        foreach (var parameterValue in executeActivityTerm.MTFParameters)
                        {
                            if (parameterValue.Value is Term termParam)
                            {
                                var found = GetParentTerm(termParam, executeActivityTerm, term);
                                if (found !=null)
                                {
                                    return found;
                                }
                            }
                        }
                    }
                    break;
            }
            return foundParent;
        }

        public static bool HasChildrenOfType<T1, T2>(this BinaryTerm binaryTerm, out bool firstTypeIsValue1)
            where T1 : Term
            where T2 : Term
        {
            bool condition1 = (binaryTerm.Value1 is T1 && binaryTerm.Value2 is T2);
            bool condition2 = (binaryTerm.Value2 is T1 && binaryTerm.Value1 is T2);
            firstTypeIsValue1 = condition1;
            return condition1 || condition2;
        }

        public static bool HasTheSameChildrenGroup(this Term newTerm, Term oldTerm)
        {
            if (newTerm.ChildrenTermGroup == oldTerm.ChildrenTermGroup)
            {
                return true;
            }
            //else if ((oldTerm.ChildrenTermGroup & TermGroups.LogicalTerm) == TermGroups.LogicalTerm
            //    && (oldTerm.ChildrenTermGroup & TermGroups.LogicalTerm) == TermGroups.LogicalTerm)
            //{
            //    return true;
            //}
            //else if ((oldTerm.ChildrenTermGroup & TermGroups.NumberTerm) == TermGroups.NumberTerm
            //    && (oldTerm.ChildrenTermGroup & TermGroups.NumberTerm) == TermGroups.NumberTerm)
            //{
            //    return true;
            //}
            //else if ((oldTerm.ChildrenTermGroup & TermGroups.StringTerm) == TermGroups.StringTerm
            //    && (oldTerm.ChildrenTermGroup & TermGroups.StringTerm) == TermGroups.StringTerm)
            //{
            //    return true;
            //}
            return false;
        }

        public static ConstantTerm CreateConstantTermByType(string type, object value)
        {
            if (string.IsNullOrEmpty(type) || value == null)
            {
                return null;
            }

            return CreateConstantTermByType(Type.GetType(type), value);
        }

        public static ConstantTerm CreateConstantTermByType(Type type, object value)
        {
            if (type == null || ((value == null) && (Nullable.GetUnderlyingType(type) == null)))
            {
                return null;
            }

            return new ConstantTerm(type) { Value = value, TargetType = type.FullName };
        }

        public static void AssignChildrenOf(this Term newTerm, Term oldTerm)
        {
            var binaryTerm = newTerm as BinaryTerm;
            if (binaryTerm != null)
            {
                var oldBinaryTerm = oldTerm as BinaryTerm;
                if (oldBinaryTerm != null)
                {
                    binaryTerm.Value1 = oldBinaryTerm.Value1;
                    binaryTerm.Value2 = oldBinaryTerm.Value2;
                }
                else if (oldTerm is UnaryTerm)
                {
                    binaryTerm.Value1 = ((UnaryTerm)oldTerm).Value;
                    binaryTerm.Value2 = new EmptyTerm();
                }
                else if (oldTerm is ListTerm)
                {
                    binaryTerm.Value1 = ((ListTerm)oldTerm).Value1;
                    binaryTerm.Value2 = new EmptyTerm();
                }
                else
                {
                    if (binaryTerm.Value1 == null)
                    {
                        binaryTerm.Value1 = new EmptyTerm();
                    }
                    if (binaryTerm.Value2 == null)
                    {
                        binaryTerm.Value2 = new EmptyTerm();
                    }
                }
            }
            else if (newTerm is UnaryTerm)
            {
                if (oldTerm is BinaryTerm)
                {
                    ((UnaryTerm)newTerm).Value = ((BinaryTerm)oldTerm).Value1;
                }
                else if (oldTerm is UnaryTerm)
                {
                    ((UnaryTerm)newTerm).Value = ((UnaryTerm)oldTerm).Value;
                }
                else if (oldTerm is ListTerm)
                {
                    ((UnaryTerm)newTerm).Value = ((ListTerm)oldTerm).Value1;
                }
                else
                {
                    if (((UnaryTerm)newTerm).Value == null)
                    {
                        ((UnaryTerm)newTerm).Value = new EmptyTerm();
                    }
                }
            }
           else if (newTerm is ConstantTerm)
            {
                if (((ConstantTerm)newTerm).Value == null)
                {
                    ((ConstantTerm)newTerm).Value = string.Empty;
                }
            }
            else if (newTerm is ListTerm)
            {
                if (oldTerm is BinaryTerm)
                {
                    ((ListTerm)newTerm).Value1 = ((BinaryTerm)oldTerm).Value1;
                }
                else if (oldTerm is UnaryTerm)
                {
                    ((ListTerm)newTerm).Value1 = ((UnaryTerm)oldTerm).Value;
                }
                else if (oldTerm is ListTerm)
                {
                    ((ListTerm)newTerm).Value1 = ((ListTerm)oldTerm).Value1;
                }
                else
                {
                    if (((ListTerm)newTerm).Value1 == null)
                    {
                        ((ListTerm)newTerm).Value1 = new EmptyTerm();
                    }
                }
            }
        }

        public static void ChangeChildrenTerm(this Term parentTerm, Term newTerm, Term oldTerm)
        {
            if (parentTerm is BinaryTerm)
            {
                if ((parentTerm as BinaryTerm).Value1 == oldTerm)
                {
                    (parentTerm as BinaryTerm).Value1 = newTerm;
                }
                else if ((parentTerm as BinaryTerm).Value2 == oldTerm)
                {
                    (parentTerm as BinaryTerm).Value2 = newTerm;
                }
            }
            else if (parentTerm is UnaryTerm)
            {
                if ((parentTerm as UnaryTerm).Value == oldTerm)
                {
                    (parentTerm as UnaryTerm).Value = newTerm;
                }
            }
            else if (parentTerm is ListTerm)
            {
                if ((parentTerm as ListTerm).Value1 == oldTerm)
                {
                    (parentTerm as ListTerm).Value1 = newTerm;
                }
            }
        }

        public static bool Contains(this Term sourceTerm, Term termToFind)
        {
            bool returnValue = false;
            sourceTerm.ForEachTerm<Term>(t => { if (t == termToFind)returnValue = true; });
            return returnValue;
        }

        public static void RemoveTerm(this ValidationTableTerm tableTerm, Term termToRemove)
        {
            tableTerm.ReplaceTem(termToRemove, new EmptyTerm());
        }

        public static void ReplaceTem(this ValidationTableTerm tableTerm, Term termToRemove, Term termToReplace)
        {
            if (tableTerm.Rows != null)
            {
                var row = tableTerm.Rows.FirstOrDefault(x => x.ActualValue == termToRemove);
                if (row != null)
                {
                    row.ActualValue = termToReplace;
                }
            }
        }

        public static void RemoveTerm(this StringFormatTerm stringFormatTerm, Term termToRemove)
        {
            stringFormatTerm.ReplaceTerm(termToRemove, new EmptyTerm());
        }

        public static void RemoveTerm(this ListOperationTerm listOperationTerm, Term termToRemove)
        {
            listOperationTerm.ReplaceTerm(termToRemove, new EmptyTerm());
        }

        public static void ReplaceTerm(this StringFormatTerm stringFomratTerm, Term termToRemove, Term termToReplace)
        {
            if (stringFomratTerm.Value != null && stringFomratTerm.Value.Parameters != null)
            {
                var newList = stringFomratTerm.Value.Parameters.ToList();
                for (int i = 0; i < newList.Count; i++)
                {
                    if (newList[i] == termToRemove)
                    {
                        newList[i] = termToReplace;
                        stringFomratTerm.Value.Parameters = newList;
                        return;
                    }
                }
            }
        }

        public static void ReplaceTerm(this ListOperationTerm listOperationTerm, Term termToRemove, Term termToReplace)
        {
            if (listOperationTerm.Value != null && listOperationTerm.Value.Parameters != null)
            {
                var newList = listOperationTerm.Value.Parameters.ToList();
                for (int i = 0; i < newList.Count; i++)
                {
                    if (newList[i] == termToRemove)
                    {
                        newList[i] = termToReplace;
                        listOperationTerm.Value.Parameters = newList;
                        return;
                    }
                }
            }
        }

        public static void ChangeTerm(this StringFormatTerm stringFormat, Term termToChange, Term targetTerm, bool copy)
        {
            if (stringFormat!=null)
            {
                var sf = stringFormat.Value;
                if (sf!=null && sf.Parameters!=null)
                {
                    var parameters = sf.Parameters;
                    for (int i = 0; i < parameters.Count; i++)
                    {
                        if (parameters[i] == termToChange)
                        {
                            if (!copy)
                            {
                                parameters[i] = new EmptyTerm();
                            }
                        }
                        else if (parameters[i] == targetTerm)
                        {
                            if (copy)
                            {
                                var cloneTerm = (Term)termToChange.Clone();
                                cloneTerm.ReplaceIdentityObjectsNoCache(termToChange);
                                parameters[i] = cloneTerm;
                            }
                            else
                            {
                                parameters[i] = termToChange;
                            }
                        }
                    }
                    sf.Parameters = parameters.ToList();
                }
            }
        }
    }
}
