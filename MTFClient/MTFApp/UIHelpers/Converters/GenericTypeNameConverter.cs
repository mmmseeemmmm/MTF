using System;
using System.Text;
using System.Windows.Data;
using MTFClientServerCommon.Helpers;

namespace MTFApp.UIHelpers.Converters
{
    class GenericTypeNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            const string arraySymbol = "[]";
            const string collection = "System.Collection";

            var output = new StringBuilder();
            string @string = value.ToString();

            if (@string.Contains(collection))
            {
                int count = @string.Split(new [] { arraySymbol }, StringSplitOptions.None).Length - 1;

                if (count > 0)
                {
                    @string = @string.Replace(arraySymbol, string.Empty);
                }

                string[] splittedString = @string.Split('[');

                for (int i = splittedString.Length - 1; i >= 0; i--)
                {
                    if (!string.IsNullOrEmpty(splittedString[i]))
                    {
                        if (!splittedString[i].Contains(collection))
                        {
                            output.Append(splittedString[i].Split(',', ']')[0]);
                        }
                        else
                        {
                            output.Append(arraySymbol);
                        }
                    }
                }

                for (int i = 0; i < count; i++)
                {
                    output.Append(arraySymbol);
                }

                string outputString = output.ToString();

                return outputString.Substring(outputString.LastIndexOf('.') + 1);
            }

            if (@string.StartsWith("System.Nullable`1[["))
            {
                var typeInfo = new TypeInfo(@string);

                if (typeInfo.IsNullableSimpleType)
                {
                    var innerTypeName = typeInfo.InnerTypeInfo.FullName;

                    return $"Nullable<{innerTypeName.Substring(innerTypeName.LastIndexOf('.') + 1)}>";
                }
            }

            return @string.Substring(@string.LastIndexOf('.') + 1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
