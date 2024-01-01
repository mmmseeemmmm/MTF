using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AutomotiveLighting.MTFCommon;
using MTFClientServerCommon;
using MTFClientServerCommon.Mathematics;
using MTFCommon.Helpers;
using MTFActivityResult = MTFClientServerCommon.MTFActivityResult;


namespace MTFCore.Managers.Components
{
    class Invoker
    {
        private ComponentsHandler componentsHandler;
        public Invoker(ComponentsHandler componentsHandler)
        {
            this.componentsHandler = componentsHandler;
        }

        public object CreateInstance(MTFClassInstanceConfiguration classInstanceConfiguration)
        {
            if (!componentsHandler.IsAssemblyLoaded(classInstanceConfiguration.ClassInfo.AssemblyName))
            {
                try
                {
                    componentsHandler.AddAssemblyFromFile(classInstanceConfiguration.ClassInfo.FullPathName);
                }
                catch (Exception e)
                {
                    throw new Exception($"Class {classInstanceConfiguration.ClassInfo.FullName} from assembly {classInstanceConfiguration.ClassInfo.AssemblyName} can not be loaded. Inner error is: {e.Message}");
                }
            }

            var asm = componentsHandler.GetAssembly(classInstanceConfiguration.ClassInfo.AssemblyName);
            object[] parameters = CastParameters(classInstanceConfiguration.ParameterValues, null, null);

            try
            {
                return asm.CreateInstance(classInstanceConfiguration.ClassInfo.FullName, false, BindingFlags.CreateInstance, null, parameters, null, null);
            }
            catch (Exception e)
            {
                string message = $"Constructor of class type {classInstanceConfiguration.ClassInfo.Name} in assembly {classInstanceConfiguration.ClassInfo.AssemblyName} crashed with error: ";
                if (e.InnerException != null)
                {
                    throw new Exception(message + e.InnerException.Message, e.InnerException);
                }

                throw new Exception(message + e.Message, e);
            }
        }

        public object CreateInstance(string typeName, GenericClassInstanceConfiguration classInstanceConfiguration, Func<Term, MTFSequenceActivity, MTFActivityResult, ScopeData, object> evaluateTerm, ScopeData scope)
        {
            var classType = GetType(typeName);
            var classInstance = Activator.CreateInstance(classType);

            foreach (var property in classInstanceConfiguration.PropertyValues)
            {
                if (property.Value is Term term)
                {
                    if (evaluateTerm == null)
                    {
                        throw new Exception("Evaluation of property isn't possible. Callback function for Term evaluation is null");
                    }
                    var classProperty = classType.GetProperty(property.Name);
                    if (classProperty == null)
                    {
                        throw new Exception($"Property {property.Name} is not found in class {typeName}");
                    }
                    else if (classProperty.SetMethod == null)
                    {
                        throw new Exception($"Property {property.Name} in class {typeName} can't be set. Check if set method is public.");
                    }
                    classProperty.SetValue(classInstance, Cast(
                        new MTFSimpleParameterValue(property.TypeName, evaluateTerm(term, null, null, scope)), null, null, evaluateTerm, scope));
                }
                else
                {
                    var classProperty = classType.GetProperty(property.Name);
                    if (classProperty == null)
                    {
                        throw new Exception($"Property {property.Name} is not found in class {typeName}");
                    }
                    else if (classProperty.SetMethod == null)
                    {
                        throw new Exception($"Property {property.Name} in class {typeName} can't be set. Check if set method is public.");
                    }
                    if (property.Value == null)
                    {
                        classProperty.SetValue(classInstance, null);
                    }
                    else
                    {
                        classProperty.SetValue(classInstance, Cast(property, null, null, evaluateTerm, scope));
                    }
                }

            }

            return classInstance;
        }

        public IParameterHelperClass CreateHelperClassInstance(string helperClassName, string assemblyFullName)
        {
            var helperClassType = GetType(helperClassName, assemblyFullName);
            if (helperClassType == null)
            {
                return null;
            }

            var parameterLessConstructor = helperClassType.GetConstructor(new Type[] { });
            if (parameterLessConstructor == null)
            {
                return null;
            }

            return Activator.CreateInstance(helperClassType) as IParameterHelperClass;
        }

        public Type GetType(MTFClassInfo classInfo)
        {
            if (classInfo == null)
            {
                return null;
            }

            if (!componentsHandler.IsAssemblyLoaded(classInfo.AssemblyName))
            {
                componentsHandler.AddAssemblyFromFile(classInfo.FullPathName);
            }

            return componentsHandler.GetAssembly(classInfo.AssemblyName).GetType(classInfo.FullName);
        }

        private Type GetType(string typeName)
        {
            MTFClientServerCommon.Helpers.TypeInfo typeInfo = new MTFClientServerCommon.Helpers.TypeInfo(typeName);
            return typeInfo.Type != null ? typeInfo.Type : Type.GetType(typeName, AssemblyResolver, TypeResolver);
        }

        private Type GetType(string typeName, string fullAssemblyName)
        {
            var asmName = Path.GetFileName(fullAssemblyName);
            if (!componentsHandler.IsAssemblyLoaded(asmName))
            {
                componentsHandler.AddAssemblyFromFile(fullAssemblyName);
            }

            return componentsHandler.GetAssembly(asmName).GetType(typeName);
        }

        private Assembly AssemblyResolver(AssemblyName asmName)
        {
            if (componentsHandler.IsAssemblyLoaded(asmName.Name + ".dll"))
            {
                return componentsHandler.GetAssembly(asmName.Name + ".dll");
            }

            return Assembly.Load(asmName);
        }

        private Type TypeResolver(Assembly assembly, string typeName, bool caseSensitive)
        {
            MTFClientServerCommon.Helpers.TypeInfo typeInfo = new MTFClientServerCommon.Helpers.TypeInfo(typeName);
            if (!typeInfo.IsUnknownType)
            {
                return Type.GetType(typeName);
            }

            var knownClass = componentsHandler.AvailableClasses.FirstOrDefault(c => c.FullName == typeName);
            if (knownClass == null)
            {
                if (assembly != null)
                {
                    var type = assembly.GetType(typeName);
                    if (type != null)
                    {
                        return type;
                    }
                }

                throw new Exception($"{typeName} is not found in loaded classes.");
            }
            if (!componentsHandler.IsAssemblyLoaded(knownClass.AssemblyName))
            {
                throw new Exception($"Assembly {knownClass.AssemblyName} is not found.");
            }

            return componentsHandler.GetAssembly(knownClass.AssemblyName).GetType(knownClass.FullName);
        }

        public object[] CastParameters(IEnumerable<IParameterValue> values, MTFSequenceActivity currentActivity, MTFActivityResult activityResult)
        {
            if (values == null)
            {
                return null;
            }

            object[] parameters = new object[values.Count()];
            int i = 0;
            foreach (IParameterValue val in values)
            {
                parameters[i] = Cast(val, currentActivity, activityResult);
                i++;
            }

            return parameters;
        }

        public object Cast(IParameterValue value, MTFSequenceActivity currentActivity, MTFActivityResult activityResult)
        {
            return Cast(value, currentActivity, activityResult, null, null);
        }

        public object Cast(IParameterValue value, MTFSequenceActivity currentActivity, MTFActivityResult activityResult,
            Func<Term, MTFSequenceActivity, MTFActivityResult, ScopeData, object> evaluateTerm, ScopeData scope)
        {
            object inputData = value.Value;
            if (value.Value is Term)
            {
                if (evaluateTerm != null)
                {
                    inputData = evaluateTerm((Term)value.Value, currentActivity, activityResult, scope);
                }
                else
                {
                    inputData = ((Term)value.Value).Evaluate();
                }
            }

            //if demanded type is same as type of value, return it
            if (inputData == null || value.TypeName == inputData.GetType().FullName || value.TypeName == inputData.GetType().AssemblyQualifiedName)
            {
                return inputData;
            }

            MTFClientServerCommon.Helpers.TypeInfo typeInfo = new MTFClientServerCommon.Helpers.TypeInfo(value.TypeName);

            //handle standard properties;
            if (typeInfo.IsSimpleType)
            {
                return TypeHelper.ConvertValue(inputData, typeInfo.Type);
            }
            else if (typeInfo.IsNullableSimpleType)
            {
                return TypeHelper.ConvertValue(inputData, typeInfo.InnerTypeInfo.Type);
            }
            else if (typeInfo.IsCollection)
            {
                if (!(inputData is ICollection))
                {
                    throw new Exception($"Can not convert value of type {inputData.GetType().FullName} to collection of type {typeInfo.FullName}");
                }

                if (typeInfo.IsArray)
                {
                    Array array = Array.CreateInstance(GetType(typeInfo.ElementTypeName), ((ICollection)inputData).Count);
                    int i = 0;
                    foreach (object item in (ICollection)inputData)
                    {
                        array.SetValue(Cast(new MTFSimpleParameterValue(typeInfo.ElementTypeName, item), currentActivity, activityResult, evaluateTerm, scope), i);
                        i++;
                    }

                    return array;
                }
                else if (typeInfo.IsGenericType)
                {
                    IList list = (System.Collections.IList)typeof(List<>)
                        .MakeGenericType(GetType(typeInfo.ElementTypeName))
                        .GetConstructor(Type.EmptyTypes)
                        .Invoke(null);
                    foreach (object item in (ICollection)inputData)
                    {
                        list.Add(Cast(new MTFSimpleParameterValue(typeInfo.ElementTypeName, item), currentActivity, activityResult, evaluateTerm, scope));
                    }

                    return list;
                }

                return null;
            }
            //if type is unknown, try create it as MTF known class, or try convert it to output type
            else if (typeInfo.IsUnknownType)
            {
                if ((inputData is GenericClassInstanceConfiguration))
                {
                    return CreateInstance(typeInfo.FullName, (GenericClassInstanceConfiguration)inputData, evaluateTerm, scope);
                }
                else
                {
                    return TypeHelper.ConvertValue(inputData, GetType(typeInfo.FullName));
                }

            }

            throw new Exception($"Value {value.TypeName} can not be converted to type {value.Value} ");
        }
    }
}
