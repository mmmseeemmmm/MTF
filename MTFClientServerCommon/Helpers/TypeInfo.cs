using MTFClientServerCommon.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutomotiveLighting.MTFCommon;
using MTFClientServerCommon.MTFTable;
using MTFCommon.Helpers;

namespace MTFClientServerCommon.Helpers
{
    public class TypeInfo
    {
        private Type type;
        private string typeName;

        public TypeInfo InnerTypeInfo
        {
            get;
            set;
        }

        public bool IsNullableSimpleType 
        {
            get
            {
                if (this.InnerTypeInfo == null)
                {
                    return false;
                }

                if (!this.InnerTypeInfo.IsSimpleType)
                {
                    return false;
                }

                return true;
            }
        }

        public TypeInfo(string typeName)
        {
            //correct common library version to currently used - mtf common should be usable in all version
            if (typeName != null && typeName.Contains("MTFCommon, Version="))
            {
                int mtfCommonIndex = typeName.IndexOf("MTFCommon, Version=") + 19;
                typeName = typeName.Remove(mtfCommonIndex, typeName.IndexOf(",", mtfCommonIndex) - mtfCommonIndex).Insert(mtfCommonIndex, MTFCommonVersion);
            }
            this.typeName = typeName;
            if (!string.IsNullOrEmpty(typeName))
            {
                type = Type.GetType(typeName);
                
                if (type!=null)
                {
                    var knownClass = type.GetCustomAttributes(true);
                    if (knownClass.Any(x=>x.GetType().FullName == typeof(AutomotiveLighting.MTFCommon.MTFKnownClassAttribute).FullName))
                    {
                        type = null;
                    }
                }

                if (type != null)
                {
                    if (this.IsGenericType && this.IsNullable && !this.IsCollection && !this.IsArray && (this.type.GenericTypeArguments.Length == 1))
                    {
                        this.InnerTypeInfo = new TypeInfo(this.type.GenericTypeArguments.First().FullName);
                    }
                }
            }
        }

        private static string mtfCommonVerison;
        public static string MTFCommonVersion
        {
            get
            {
                if (string.IsNullOrEmpty(mtfCommonVerison))
                {
                    var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "MTFCommon");
                    if (asm == null)
                    {
                        return string.Empty;
                    }
                    mtfCommonVerison = asm.GetName().Version.ToString();
                }

                return mtfCommonVerison;
            }
        }

        public Type Type => type;

        public string FullName
        {
            get
            {
                if (type == null)
                {
                    return typeName;
                }

                return type.FullName;
            }
        }


        public bool IsCase
        {
            get
            {
                if (type == null)
                {
                    return false;
                }
                return type == typeof(MTFCase);
            }
        }

        public bool IsSequenceVariantGroup
        {
            get
            {
                if (type == null)
                {
                    return false;
                }
                return type == typeof(List<SequenceVariantGroup>);
            }
        }

        public bool IsNumeric
        {
            get
            {
                if (type == null)
                {
                    return false;
                }

                if (this.IsNullable)
                {
                    if (this.IsNullableSimpleType)
                    {
                        return this.InnerTypeInfo.IsNumeric;
                    }
                }

                return type.IsNumericType();
            }
        }

        public bool IsTerm
        {
            get
            {
                if (type == null)
                {
                    return false;
                }

                return type == typeof(Term) || type.IsSubclassOf(typeof(Term));
            }
        }

        public bool IsStringFormat
        {
            get
            {
                if (type == null)
                {
                    return false;
                }

                return typeName == typeof(MTFStringFormat).FullName;
            }
        }


        public bool IsValidationTableStatus => typeName == typeof(MTFCommon.MTFValidationTableStatus).FullName;

        public bool IsValidationTable => typeName == typeof(MTFValidationTable.MTFValidationTable).FullName;

        public bool IsConstantTable => typeName == typeof(MTFConstantTable).FullName;

        public bool IsGenericClassInstanceConfiguration
        {
            get
            {
                if (type == null)
                {
                    return false;
                }

                return type == typeof(GenericClassInstanceConfiguration);
            }
        }

        public bool IsSimpleTerm
        {
            get
            {
                if (type == null)
                {
                    return false;
                }
                return type == typeof(ConstantTerm)
                    || type == typeof(VariableTerm)
                    || type == typeof(ActivityResultTerm)
                    || type == typeof(EmptyTerm);
            }
        }

        public bool IsBool
        {
            get
            {
                if (type == null)
                {
                    return false;
                }

                if (IsNullable)
                {
                    return ElementTypeName.Split(',')[0] == typeof(bool).FullName;
                }

                return type == typeof(bool);
            }
        }

        public bool IsChar
        {
            get
            {
                if (type == null)
                {
                    return false;
                }

                if (IsNullable)
                {
                    return ElementTypeName.Split(',')[0] == typeof(char).FullName;
                }

                return type == typeof(char);
            }
        }

        public bool IsString
        {
            get
            {
                if (type == null)
                {
                    return false;
                }

                if (IsNullable)
                {
                    return ElementTypeName.Split(',')[0] == typeof(string).FullName;
                }

                return type == typeof(string);
            }
        }

        public bool IsArray
        {
            get
            {
                if (type == null)
                {
                    if (string.IsNullOrEmpty(FullName))
                    {
                        return false;
                    }

                    return FullName.EndsWith("[]");
                }

                return type.IsArray;
            }
        }

        public bool IsSimpleType
        {
            get
            {
                if (IsValidationTableStatus)
                {
                    return true;
                }

                if (type == null)
                {
                    return false;
                }

                if (this.IsNullable)
                {
                    return false;
                }

                return IsNumeric || IsBool || IsString || IsChar;
            }
        }

        public bool IsArrayOfSimpleType
        {
            get
            {
                if (!IsArray)
                {
                    return false;
                }

                var elementTypeName = ElementTypeName;
                if (string.IsNullOrEmpty(elementTypeName))
                {
                    return false;
                }
                TypeInfo ti = new TypeInfo(elementTypeName);

                return ti.IsSimpleType;
            }
        }

        public bool IsCollection => IsArray || (IsGenericType && !IsNullable);

        public bool IsNullable => FullName.StartsWith("System.Nullable`1[[");

        public bool IsGenericType
        {
            get
            {
                if (type == null)
                {
                    if (string.IsNullOrEmpty(FullName))
                    {
                        return false;
                    }

                    return FullName.Contains("System.Collections.Generic");
                }

                return type.IsGenericType;
            }
        }

        public bool IsImage
        {
            get
            {
                if (type == null)
                {
                    if (string.IsNullOrEmpty(FullName))
                    {
                        return false;
                    }
                    return FullName.Contains(typeof(MTFImage).FullName);
                }
                return false;
            }
        }

        public bool IsActivityTarget => type == null && (!string.IsNullOrEmpty(FullName) && FullName.Contains(typeof(MTFActivity).FullName));


        public bool IsUnknownType => ((Type == null && !string.IsNullOrEmpty(typeName)) || (Type == typeof(object)) || Type == typeof(Stream)) && !IsValidationTableStatus;

        public bool IsListOperation
        {
            get
            {
                if (type == null)
                {
                    return false;
                }
                return type == typeof(MTFListOperation);
            }
        }

        public string ElementTypeName
        {
            get
            {
                if (string.IsNullOrEmpty(typeName))
                {
                    return string.Empty;
                }

                if (typeName.EndsWith("[]"))
                {
                    int index = typeName.LastIndexOf("[]");
                
                    return typeName.Substring(0, index);
                }

                if(this.typeName.Contains("[["))
                {
                    int index1 = typeName.IndexOf("[[");
                    string subTypeName = typeName.Remove(0, index1 + 2);
                    int index2 = subTypeName.LastIndexOf("]]");

                    return subTypeName.Remove(index2, subTypeName.Length - index2);
                }

                if (this.typeName.Contains("["))
                {
                    int index1 = this.typeName.IndexOf("[");
                    string subTypeName = typeName.Remove(0, index1 + 1);
                    int index2 = subTypeName.LastIndexOf("]");

                    return subTypeName.Remove(index2, subTypeName.Length - index2);
                }

                return string.Empty;
            }
        }
    }
}
