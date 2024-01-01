using System;
using System.Linq.Expressions;

namespace MTFApp.UIHelpers
{
    public static class PropertyInfoHelper
    {
        public static string GetPropertyName<T>(Expression<Func<T>> property)
        {
            var memberExpression = property.Body as MemberExpression;

            if (memberExpression != null)
            {
                return memberExpression.Member.Name;
            }

            return null;
        }
    }
}
