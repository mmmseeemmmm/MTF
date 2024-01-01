using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using AutomotiveLighting.MTFCommon;
using Microsoft.CSharp;
using MTFCommon;
using MTFCommon.Helpers;

namespace DataCollectionDriver
{
    public class DataCollectionALCZControl
    {
        private System.IO.StreamWriter sw = null;
        string openedFile = string.Empty;
        private List<CategoryOfReport> dataToWrite = new List<CategoryOfReport>();

        public void CollectData(string fileName, List<EntryOfReport> categoryOfReport)
        {
            string directory = AppDomain.CurrentDomain.BaseDirectory + @"\data\logs\";

            string fullPath = createFinalFullPath(directory, fileName, "txt");
            openFileForStream(fullPath);

            foreach (var line in categoryOfReport)
            {
                if (line.Value == MTFValidationTableStatus.NotFilled.ToString() || line.Value == null)
                {
                    line.Value = "NA";
                }
                if (line.Value == MTFValidationTableStatus.Ok.ToString() || line.Value == "True")
                {
                    line.Value = "1";
                }
                if (line.Value == MTFValidationTableStatus.Nok.ToString() || line.Value == "False")
                {
                    line.Value = "0";
                }
                line.Value = line.Value.Replace(Environment.NewLine, " - "); //Errors on one line separated by '-'
                preparaDataToWrite(line);
            }
        }

        public void CollectDataByTemplate(string fileName, string templateFile, IMTFSequenceRuntimeContext runtimeContext)
        {
            string directory = AppDomain.CurrentDomain.BaseDirectory + @"\data\logs\";

            string fullPath = createFinalFullPath(directory, fileName, "txt");
            openFileForStream(fullPath);

            var template = File.OpenText(templateFile);
            CategoryOfReport category = null;
            List<CategoryOfReport> categories = new List<CategoryOfReport>();
            int templateLineNumber = 0;
            while (!template.EndOfStream)
            {
                var templateLine = template.ReadLine();
                templateLineNumber++;

                try
                {
                    if (!string.IsNullOrEmpty(templateLine) && templateLine.StartsWith("["))
                    {
                        category = new CategoryOfReport { CategoryName = templateLine.Substring(1, templateLine.Length - 2), EntriesOfCategory = new List<EntryOfReport>() };
                        categories.Add(category);
                    }
                    else if (!string.IsNullOrEmpty(templateLine) && templateLine.Contains("="))
                    {
                        var parameterName = templateLine.Substring(0, templateLine.IndexOf("="));
                        var parameterValue = templateLine.Substring(templateLine.IndexOf("=") + 1);

                        var val = getParameterValue(parameterValue, runtimeContext);
                        if (val != null)
                        {
                            category.EntriesOfCategory.Add(new EntryOfReport { Category = category.CategoryName, Key = parameterName.Trim(' '), Value = val });
                        }
                    }

                }
                catch (Exception e)
                {
                    throw new Exception(string.Format("Error by processing template line {0}: {1} occured. Inner error: {2}", templateLineNumber, templateLine, e.Message), e);
                }
            }

            dataToWrite.AddRange(categories);
        }

        public void CollectErrorImages(string directory, string fileName, string controlCharacter, int startNumber, MTFImage[] errorImages)
        {
            if (errorImages == null || errorImages.Length == 0)
            {
                return;
            }

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            foreach (var errorImage in errorImages)
            {
                var fullPath = createFinalFullPath(directory, !string.IsNullOrEmpty(controlCharacter)
                    ? fileName.Replace(controlCharacter, startNumber.ToString()) : fileName, "jp2");

                using (var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
                {
                    stream.Write(errorImage.ImageData, 0, errorImage.ImageData.Length);
                }

                startNumber++;
            }
        }

        public void SaveFileToDestinationFolder(string basePath, string fileName)
        {
            writeDataToLocalFile();

            string destFullPath = createFinalFullPath(basePath, fileName, "txt");

            if (Path.GetFileName(destFullPath) != Path.GetFileName(openedFile))
            {
                throw new Exception("This file is not exist.");
            }

            if (sw != null)
            {
                sw.Close();
                sw.Dispose();
                sw = null;
            }

            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }
            var access = File.GetAccessControl(openedFile);
            access.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, AccessControlType.Allow));
            File.SetAccessControl(openedFile, access);
            File.Move(openedFile, destFullPath);
        }

        public ushort MoveNext()
        {
            UInt16 result;

            string fileName = "DataCollectionCounter.txt";
            if (!File.Exists(fileName))
            {
                File.WriteAllText(fileName, "1");
            }

            string counter = File.ReadAllText(fileName);

            try
            {
                result = UInt16.Parse(counter);
            }
            catch
            {
                File.WriteAllText(fileName, "1");
                result = 1;
            }

            UInt16 newValue = (UInt16)(result + (UInt16)1);

            File.WriteAllText(fileName, newValue.ToString());

            return result;
        }

        private string getParameterValue(string parameter, IMTFSequenceRuntimeContext runtimeContext)
        {
            var trimedParam = parameter.Trim(' ');
            var inner = getInnerStrings(trimedParam, '{', '}');
            bool allNull = inner.Length > 0; ;
            foreach (var p in inner)
            {
                try
                {
                    var val = ProcessOperator(p, runtimeContext);
                    if (val != null)
                    {
                        allNull = false;
                    }
                    trimedParam = trimedParam.Replace("{" + p + "}", val);
                }
                catch (Exception e)
                {
                    throw new Exception(string.Format("Get parameter(parameter name {0}) value failed: {1}", p, e.Message), e);
                }
            }

            return allNull ? null : trimedParam;
        }

        private CompilerResults compileCode(IEnumerable<string> parameters, string code)
        {
            CSharpCodeProvider c = new CSharpCodeProvider();
            ICodeCompiler icc = c.CreateCompiler();
            CompilerParameters cp = new CompilerParameters();

            cp.ReferencedAssemblies.Add("system.dll");
            cp.ReferencedAssemblies.Add("System.Core.dll");
            cp.ReferencedAssemblies.Add("system.xml.dll");
            cp.ReferencedAssemblies.Add("system.xml.Linq.dll");
            cp.ReferencedAssemblies.Add("system.data.dll");
            cp.ReferencedAssemblies.Add("MTFCommon.dll");

            cp.CompilerOptions = "/t:library";
            cp.GenerateInMemory = true;

            StringBuilder sb = new StringBuilder("");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Xml;");
            sb.AppendLine("using System.Data;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using System.Text;");
            sb.AppendLine("using System.Collections.Generic;");

            sb.AppendLine("namespace CodeExecuter{");
            sb.AppendLine("public static class Executer{");


            sb.Append("public static object Run(");
            bool firstParam = true;
            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    if (!firstParam)
                    {
                        sb.Append(", ");
                    }
                    sb.Append(p);
                    firstParam = false;
                }
            }
            sb.AppendLine("){");

            var headerLines = Regex.Matches(sb.ToString(), Environment.NewLine).Count;

            sb.AppendLine(code);
            sb.AppendLine("}");
            sb.AppendLine("}");
            sb.AppendLine("}");

            CompilerResults cr = icc.CompileAssemblyFromSource(cp, sb.ToString());
            if (cr.Errors.HasErrors)
            {
                StringBuilder errorMessageBuilder = new StringBuilder().AppendFormat("Code compilation failed with {0} errors", cr.Errors.Count).AppendLine();
                foreach (CompilerError error in cr.Errors)
                {
                    errorMessageBuilder.AppendFormat("ERROR: {0} on line {1}, column {2}", error.ErrorText, error.Line - headerLines, error.Column);
                }

                throw new Exception(errorMessageBuilder.ToString());
            }

            return cr;
        }

        private object executeCompiledCode(IList<string> parameters, CompilerResults cr)
        {
            try
            {
                List<object> retypedParameters = new List<object>();
                var codeParameters = cr.CompiledAssembly.GetType("CodeExecuter.Executer").GetMethod("Run").GetParameters();
                for (var i = 0; i < codeParameters.Length; i++)
                {
                    retypedParameters.Add(TypeHelper.ConvertValue(parameters[i], codeParameters[i].ParameterType));
                }

                return cr.CompiledAssembly.GetType("CodeExecuter.Executer").GetMethod("Run").Invoke(null, retypedParameters.ToArray());
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    throw new Exception("Code execution failed with error: " + e.InnerException.Message, e.InnerException);
                }

                throw new Exception("Code execution failed with error: " + e.Message, e);
            }
        }

        private IEnumerable<string> prepaireFuncParameters(string funcHeader)
        {
            var innerStrings = getInnerStrings(funcHeader, '(', ')');
            if (innerStrings.Length == 0)
            {
                return null;
            }
            var parameters = getInnerStrings(funcHeader, '(', ')')[0].Split(',');

            return parameters.Select(p => p.Substring(0, p.IndexOf("="))).ToList();
        }

        private IList<string> prepaireFuncValues(string funcHeader, IMTFSequenceRuntimeContext runtimeContext)
        {
            List<string> values = new List<string>();
            var trimedParam = funcHeader.Trim(' ');
            var inner = getInnerStrings(trimedParam, '{', '}');
            foreach (var p in inner)
            {
                try
                {
                    values.Add(ProcessOperator(p, runtimeContext));
                }
                catch (Exception e)
                {
                    throw new Exception(string.Format("Get parameter(parameter name {0}) value failed: {1}", p, e.Message), e);
                }
            }

            return values;
        }

        private string ProcessOperator(string oper, IMTFSequenceRuntimeContext runtimeContext)
        {
            if (oper.StartsWith("("))
            {
                int indexOfFuncSeparator = oper.IndexOf("=>");
                if (indexOfFuncSeparator < 0)
                {
                    return null;
                }

                string funcHeader = oper.Substring(0, indexOfFuncSeparator);
                string funcBody = oper.Substring(indexOfFuncSeparator + 2);

                var compiledCode = compileCode(prepaireFuncParameters(funcHeader), funcBody);
                var codeResult = executeCompiledCode(prepaireFuncValues(funcHeader, runtimeContext), compiledCode);

                return codeResult != null ? codeResult.ToString() : null;
            }
            if (oper.Contains("{"))
            {
                var innerOperator = oper.Substring(0, oper.IndexOf("{"));
                if (innerOperator.Contains("=="))
                {
                    var innerOper = innerOperator.Substring(0, innerOperator.IndexOf("=="));
                    var innerValue = innerOperator.Substring(innerOperator.IndexOf("==") + 2);
                    var val = GetTableValue(innerOper, runtimeContext);
                    if (string.IsNullOrEmpty(val))
                    {
                        return null;
                    }

                    var parameters = getInnerStrings(oper.Substring(oper.IndexOf("{")), '{', '}');

                    if (val == innerValue && parameters.Length >= 1)
                    {
                        return getParameterValue(parameters[0], runtimeContext);
                    }

                    if (val != innerValue && parameters.Length >= 2)
                    {
                        return getParameterValue(parameters[1], runtimeContext);
                    }

                    return null;
                }
            }

            return GetTableValue(oper, runtimeContext);
        }

        private string[] getInnerStrings(string s, char start, char end)
        {

            int startCount = 0;
            List<string> output = new List<string>();
            string tmp = string.Empty;
            foreach (var c in s)
            {
                if (c == end)
                {
                    startCount--;
                }
                if (startCount > 0)
                {
                    tmp += c;
                }
                if (c == start)
                {
                    startCount++;
                }
                if (startCount == 0 && !string.IsNullOrEmpty(tmp))
                {
                    output.Add(tmp);
                    tmp = string.Empty;
                }
            }
            return output.ToArray();
        }

        private string GetTableValue(string parameter, IMTFSequenceRuntimeContext runtimeContext)
        {
            var formatString = getFormatString(parameter);
            string[] splitedParam = getSplitedPramas(parameter);

            if (splitedParam.Length == 2 && splitedParam[1] == "TableStatus")
            {
                try
                {
                    return MTFValidationTableStatusToString(runtimeContext.GetValidationTableStatus(splitedParam[0]));
                }
                catch
                {
                    return null;
                }
            }
            if (splitedParam.Length == 2 && splitedParam[1] == "TableErrorText")
            {
                try
                {
                    var tableErrorText = runtimeContext.GetValidationTableErrorText(splitedParam[0]);
                    return string.IsNullOrEmpty(tableErrorText) ? null : tableErrorText.TrimEnd(Environment.NewLine.ToCharArray());
                }
                catch
                {
                    return null;
                }
            }

            if (splitedParam.Length == 3)
            {
                return ParameterValueFromTable(splitedParam[0], splitedParam[1], splitedParam[2], runtimeContext, formatString);
            }

            return null;
        }

        private string getFormatString(string param)
        {
            var splitFormat = param.Split(',');
            if (splitFormat.Length == 2)
            {
                return splitFormat[1].Trim(' ');
            }
            return string.Empty;
        }

        private string[] getSplitedPramas(string parameter)
        {
            var split1 = parameter.Split(',');
            if (split1.Length == 0)
            {
                return new string[0];
            }
            return split1[0].Trim(' ').Split('.');
        }

        private string ParameterValueFromTable(string tableName, string rowName, string colName, IMTFSequenceRuntimeContext runtimeContext, string format)
        {
            if (colName == "RowStatus")
            {
                try
                {
                    return MTFValidationTableStatusToString(runtimeContext.GetValidationTableRowStatus(tableName, rowName));
                }
                catch
                {
                    return null;
                }
            }
            if (colName == "RowErrorText")
            {
                try
                {
                    var rowErrorText = runtimeContext.GetValidationTableRowErrorText(tableName, rowName);
                    return string.IsNullOrEmpty(rowErrorText) ? null : rowErrorText.TrimEnd(Environment.NewLine.ToCharArray());
                }
                catch
                {
                    return null;
                }
            }
            //column name is null => get data from constant dable
            if (colName == "ConstantValue")
            {
                try
                {
                    var constant = runtimeContext.GetFromConstantTable(tableName, rowName);
                    return constant == null ? null : constant.ToString();
                }
                catch (Exception)
                {
                    return null;
                }
            }
            try
            {
                if (runtimeContext.GetValidationTableRowStatus(tableName, rowName) != MTFValidationTableStatus.NotFilled)
                {
                    var tableRow = runtimeContext.GetFromValidationTable(tableName, rowName);
                    if (tableRow.Any(r => r.Name == colName))
                    {
                        var col = tableRow.FirstOrDefault(r => r.Name == colName);
                        if (col == null || col.Value == null)
                        {
                            return null;
                        }
                        if (col.Value is ICollection<string>)
                        {
                            return string.Join("; ", (ICollection<string>)col.Value);
                        }

                        if (col.Value is IFormattable)
                        {
                            //Invariant culture because of "." as decimal separator is required
                            return ((IFormattable)col.Value).ToString(format, CultureInfo.InvariantCulture);
                        }

                        return col.Value.ToString();
                    }
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        private string MTFValidationTableStatusToString(MTFValidationTableStatus status)
        {
            switch (status)
            {
                case MTFValidationTableStatus.Ok: return "1";
                case MTFValidationTableStatus.Nok: return "0";
                case MTFValidationTableStatus.GSFail: return "1";
                default: return null;
            }
        }

        private void writeDataToLocalFile()
        {
            if (dataToWrite == null || dataToWrite.Count == 0)
            {
                throw new Exception("No data to write!");
            }
            var lastCategory = dataToWrite.Last();

            foreach (var category in dataToWrite)
            {
                if (category.EntriesOfCategory.Count > 0)
                {
                    var lastEntry = category.EntriesOfCategory.Last();
                    sw.WriteLine("[" + category.CategoryName + "]");
                    foreach (var entry in category.EntriesOfCategory)
                    {
                        if (entry == lastEntry && category == lastCategory)
                        {
                            sw.Write(entry.Key + "=" + entry.Value);
                        }
                        else
                        {
                            sw.WriteLine(entry.Key + "=" + entry.Value);
                        }
                    }
                    if (category != lastCategory)
                    {
                        sw.WriteLine("");
                    }
                }
            }
            sw.Flush();
            dataToWrite = new List<CategoryOfReport>();
        }

        private void preparaDataToWrite(EntryOfReport line)
        {

            if (dataToWrite.Any(d => d.CategoryName == line.Category))
            {
                dataToWrite.Where(i => i.CategoryName == line.Category).FirstOrDefault().EntriesOfCategory.Add(line);
            }
            else
            {
                dataToWrite.Add(new CategoryOfReport
                {
                    CategoryName = line.Category,
                    EntriesOfCategory = new List<EntryOfReport>
                    {
                        new EntryOfReport
                        {
                            Key = line.Key,
                            Value = line.Value
                        }
                    }
                });
            }
        }

        private void openFileForStream(string fullpath)
        {

            if (openedFile != fullpath)
            {
                if (sw != null)
                {
                    sw.Close();
                    sw.Dispose();
                }

                string directory = Path.GetDirectoryName(fullpath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                sw = new StreamWriter(fullpath, false, Encoding.UTF8);
                openedFile = fullpath;
            }
        }

        private string createFinalFullPath(string basePath, string fileName, string extension)
        {
            if (String.IsNullOrEmpty(basePath) || String.IsNullOrEmpty(fileName) || String.IsNullOrEmpty(extension))
            {
                throw new Exception("Invalid file name.");
            }
            string fullPath = Path.Combine(basePath, fileName);

            fullPath = Path.ChangeExtension(fullPath, extension);

            return fullPath;
        }

        internal void Close()
        {
            if (sw != null)
            {
                sw.Close();
                sw.Dispose();
                sw = null;
            }
        }

    }
}
