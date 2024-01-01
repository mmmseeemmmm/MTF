using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace MTFClientServerCommon.Helpers
{
    public static class EnumHelper
    {
        public static string Description(this Enum eValue)
        {
            var nAttributes = eValue.GetType().GetField(eValue.ToString()).
                GetCustomAttributes(typeof(DescriptionAttribute), false);

            //no description attribute
            if (!nAttributes.Any())
            {
                CultureInfo.CurrentCulture.TextInfo.ToTitleCase(eValue.ToString());
            }

            return (nAttributes.First() as DescriptionAttribute).Description;
        }

        public static IEnumerable<EnumValueDescription> GetAllValuesAndDescriptions<TEnum>()
            where TEnum : struct, IConvertible, IComparable, IFormattable
        {
            if (!typeof(TEnum).IsEnum)
                throw new ArgumentException("TEnum must be an Enumeration type");

            return Enum.GetValues(typeof(TEnum)).Cast<Enum>().
                Select((e) => new EnumValueDescription() { Value = e, Description = e.Description() }).ToList();
        }

        public static IEnumerable<EnumValueDescription> GetAllValuesAndDescriptions<TEnum>(TEnum[] exceptions)
            where TEnum : struct, IConvertible, IComparable, IFormattable
        {
            if (!typeof(TEnum).IsEnum)
                throw new ArgumentException("TEnum must be an Enumeration type");

            foreach (var e in Enum.GetValues(typeof(TEnum)).Cast<TEnum>())
            {
                if (exceptions == null || !exceptions.Contains(e))
                {
                    yield return new EnumValueDescription() { Value = e, Description = (e as Enum).Description() };
                }
            }
        }
    }

    public class EnumValueDescription
    {
        public string Description { get; set; }
        public object Value { get; set; }

    }
}
