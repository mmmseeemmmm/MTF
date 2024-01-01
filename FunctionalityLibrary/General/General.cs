using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using AutomotiveLighting.MTFCommon;
using AutomotiveLighting.MTFCommon.Types;
using Microsoft.CSharp;
using System.Globalization;
using System.IO;
using System.Runtime;
using General;


namespace GeneralDriver
{
    [MTFClass(Name = "General", Description = "Driver for General functions like counters, timers etc.", Icon = MTFIcons.General, ThreadSafeLevel = ThreadSafeLevel.Instance)]
    [MTFClassCategory("Execution Control")]
    //[MTFClassCategory("MTF Testing")]
    public class General : ICanStop, IDisposable
    {
        public IMTFSequenceRuntimeContext runtimeContext;

        #region private fields, ctor
        private Stopwatch[] timers = new Stopwatch[5];

        [MTFConstructor]
        public General()
        {
            for (int i = 0; i < 5; i++)
            {
                timers[i] = new Stopwatch();
            }
        }

        #endregion

        #region timer methods

        [MTFMethod]
        [MTFAllowedParameterValue("timer", "Timer 0", "0")]
        [MTFAllowedParameterValue("timer", "Timer 1", "1")]
        [MTFAllowedParameterValue("timer", "Timer 2", "2")]
        [MTFAllowedParameterValue("timer", "Timer 3", "3")]
        [MTFAllowedParameterValue("timer", "Timer 4", "4")]
        public void StartTimer(int timer)
        {
            timers[timer].Start();
        }

        [MTFMethod]
        [MTFAllowedParameterValue("timer", "Timer 0", "0")]
        [MTFAllowedParameterValue("timer", "Timer 1", "1")]
        [MTFAllowedParameterValue("timer", "Timer 2", "2")]
        [MTFAllowedParameterValue("timer", "Timer 3", "3")]
        [MTFAllowedParameterValue("timer", "Timer 4", "4")]
        public void RestartTimer(int timer)
        {
            timers[timer].Restart();
        }

        [MTFMethod]
        [MTFAllowedParameterValue("timer", "Timer 0", "0")]
        [MTFAllowedParameterValue("timer", "Timer 1", "1")]
        [MTFAllowedParameterValue("timer", "Timer 2", "2")]
        [MTFAllowedParameterValue("timer", "Timer 3", "3")]
        [MTFAllowedParameterValue("timer", "Timer 4", "4")]
        public void StopTimer(int timer)
        {
            timers[timer].Stop();
        }

        [MTFMethod]
        [MTFAllowedParameterValue("timer", "Timer 0", "0")]
        [MTFAllowedParameterValue("timer", "Timer 1", "1")]
        [MTFAllowedParameterValue("timer", "Timer 2", "2")]
        [MTFAllowedParameterValue("timer", "Timer 3", "3")]
        [MTFAllowedParameterValue("timer", "Timer 4", "4")]
        public long GetAbsoluteMiliseconds(int timer)
        {
            return timers[timer].ElapsedMilliseconds;
        }

        [MTFMethod]
        [MTFAllowedParameterValue("timer", "Timer 0", "0")]
        [MTFAllowedParameterValue("timer", "Timer 1", "1")]
        [MTFAllowedParameterValue("timer", "Timer 2", "2")]
        [MTFAllowedParameterValue("timer", "Timer 3", "3")]
        [MTFAllowedParameterValue("timer", "Timer 4", "4")]
        public long GetAbsoluteSeconds(int timer)
        {
            return timers[timer].ElapsedMilliseconds / 1000;
        }

        [MTFMethod]
        [MTFAllowedParameterValue("timer", "Timer 0", "0")]
        [MTFAllowedParameterValue("timer", "Timer 1", "1")]
        [MTFAllowedParameterValue("timer", "Timer 2", "2")]
        [MTFAllowedParameterValue("timer", "Timer 3", "3")]
        [MTFAllowedParameterValue("timer", "Timer 4", "4")]
        public void WakeUpInAbsoluteMiliseconds(long miliseconds, int timer, bool raiseExceptionWhenIsLate)
        {
            if (!timers[timer].IsRunning)
            {
                throw new Exception("Timer " + timer + " has not been started yet.");
            }

            if (timers[timer].ElapsedMilliseconds > miliseconds)
            {
                if (raiseExceptionWhenIsLate)
                {
                    throw new Exception("WakeUpIn " + miliseconds + " is impossible because now is already " + timers[timer].ElapsedMilliseconds);
                }
                return;
            }
            LongWaiting(miliseconds, timers[timer]);

        }

        [MTFMethod]
        [MTFAllowedParameterValue("timer", "Timer 0", "0")]
        [MTFAllowedParameterValue("timer", "Timer 1", "1")]
        [MTFAllowedParameterValue("timer", "Timer 2", "2")]
        [MTFAllowedParameterValue("timer", "Timer 3", "3")]
        [MTFAllowedParameterValue("timer", "Timer 4", "4")]
        public void WakeUpInAbsoluteTime(int hours, int minutes, float seconds, int timer, bool raiseExceptionWhenIsLate)
        {
            long miliseconds = hours * 3600000 + minutes * 60000 + (long)(seconds * 1000);
            WakeUpInAbsoluteMiliseconds(miliseconds, timer, raiseExceptionWhenIsLate);
        }

        [MTFMethod]
        public void Delay(long milisecondsToWait)
        {

            Stopwatch timer = Stopwatch.StartNew();
            if (milisecondsToWait < 20)
            {
                BusyWaiting(milisecondsToWait, timer);
                return;
            }
            LongWaiting(milisecondsToWait, timer);
        }

        #endregion

        #region private timer methods

        private void BusyWaiting(long finalTime, Stopwatch timer)
        {
            while (timer.ElapsedMilliseconds < finalTime) ;
        }

        private void ShortWaiting(long finalTime, Stopwatch timer)
        {
            long milisecondsToEnd = finalTime - timer.ElapsedMilliseconds;

            if (milisecondsToEnd > 20)
            {
                Thread.Sleep((int)(milisecondsToEnd - 20));
            }

            BusyWaiting(finalTime, timer);
        }

        private static string milisecondsToHumanReadable(long time)
        {
            TimeSpan ts = TimeSpan.FromMilliseconds(time);
            return string.Join(" ", string.Format("{0:D2}d:{1:D2}h:{2:D2}m:{3:D2}s:", ts.Days, ts.Hours, ts.Minutes, ts.Seconds)
                .Split(':')
                .SkipWhile(s => System.Text.RegularExpressions.Regex.Match(s, @"00\w").Success)
                .ToArray());
        }

        private void LongWaiting(long finalTime, Stopwatch timer)
        {

            int report;
            long milisecondsToWait = finalTime - timer.ElapsedMilliseconds;
            long milisecondsToEnd = milisecondsToWait;

            while (milisecondsToEnd > 200 && !stop)
            {
                report = 100 - (int)(100 * milisecondsToEnd / milisecondsToWait);
                runtimeContext.ProgressNotification(report, milisecondsToHumanReadable(milisecondsToEnd));
                Thread.Sleep(150);
                milisecondsToEnd = finalTime - timer.ElapsedMilliseconds;
            }
            if (stop)
            {
                return;
            }

            runtimeContext.ProgressNotification(100);
            ShortWaiting(finalTime, timer);

        }

        private long getUptime()
        {
            var result = (long)TimeSpan.FromMilliseconds(Environment.TickCount).TotalMilliseconds;
            if (result < 0)
            {
                return int.MaxValue + Math.Abs(result);
            }
            return result;
        }
       
        #endregion

        #region other helper methods

        [MTFMethod]
        public bool StringContain(string value, List<string> toFind)
        {
            if (value != null)
            {
                for (int i = 0; i < toFind.Count; i++)
                {
                    if (value.Contains(toFind[i]))
                        return true;
                }
            }
            return false;
        }

        #region methods by Jascha
        [MTFMethod(Description = "Get sub array of given byte array.\n"
                                    + "If Amount is set to '0' then all following elements are returned.",
                   DisplayName = "Get Sub Array Of Byte Array")]
        [MTFAdditionalParameterInfo(ParameterName = "Array", Description = "Array of bytes to get sub array from")]
        [MTFAdditionalParameterInfo(ParameterName = "Begin", Description = "Position to begin taking bytes")]
        [MTFAdditionalParameterInfo(ParameterName = "Amount", Description = "Amount how many bytes should be taken\n"
                                                                            + "0: all following elements are taken")]
        public byte[] GetSubArrayOfByteArray(byte[] Array, int Begin, int Amount)
        {
            //sanity check
            if (Begin < 0) throw new Exception("Begin must be >= 0!");
            if (Begin > Array.Count()) throw new Exception("Begin (" + Begin + ") must be smaller than the size of the Array (" + Array.Count() + ")!");
            if (Begin + Amount >= Array.Count()) throw new Exception("Begin (" + Begin + ") and Amount (" + Amount + ") must be smaller than the size of the Array (" + Array.Count() + ")!");

            int _amount = Amount;
            //take all elements from _begin to end
            if (Amount == 0) _amount = Array.Count();

            //return
            return Array.Skip(Begin).Take(_amount).ToArray();
        }

        // -------------------------------------------------------------------------------------------------------------------------------------
        [MTFMethod(Description = "Concatenates given strings to one single string.",
                   DisplayName = "Concatenate Strings")]
        [MTFAdditionalParameterInfo(ParameterName = "Strings", Description = "Strings that should be concatenated")]
        [MTFAdditionalParameterInfo(ParameterName = "Separator", Description = "Separator between strings in output string")]
        public string ConcatStrings(string[] Strings, string Separator)
        {
            //sanity check
            if (Strings.Count() <= 1) throw new Exception("There are not enough Strings (" + Strings.Count() + ") to concatenate!");

            string _concatenatedString = "";
            //concatenate strings with separator
            for (int i = 0; i < Strings.Count(); i++)
            {
                _concatenatedString += Strings[i];
                //set separator except for last one
                if (i < Strings.Count() - 1)
                { _concatenatedString += Separator; }
            }

            //return
            return _concatenatedString;
        }

        // -------------------------------------------------------------------------------------------------------------------------------------
        [MTFMethod(Description = "Reflection of .NET interface of method \"Substring\"",
            DisplayName = "Get Sub String .NET")]
        [MTFAdditionalParameterInfo(ParameterName = "InputString", Description = "Retrieves a substring from this instance.")]
        [MTFAdditionalParameterInfo(ParameterName = "StartIndex", Description = "The zero-based starting character position of a substring in this instance.")]
        [MTFAdditionalParameterInfo(ParameterName = "Length", Description = "The number of characters in the substring.\nIf set to \"0\", the string is returned from StartIndex to the end of input string.", DefaultValue = "0")]
        public string GetSubStringNET(string inputString, int startIndex, int length)
        {
            //sanity check
            if (string.IsNullOrEmpty(inputString)) throw new Exception("Input string can not be empty!");

            //return desired substring
            return length == 0 ? inputString.Substring(startIndex) : inputString.Substring(startIndex, length);
        }


        // -------------------------------------------------------------------------------------------------------------------------------------
        [MTFMethod(Description = "Compresses blanks in string to defined number of blanks.\n",
                   DisplayName = "Compress Blanks")]
        [MTFAdditionalParameterInfo(ParameterName = "String", Description = "String in which the blanks are manipulated")]
        [MTFAdditionalParameterInfo(ParameterName = "NumBlanks", Description = "Number of target blanks between words")]
        public string CompressBlanks(string String, int NumBlanks)
        {
            if (NumBlanks == 0)
            { return new string(String.ToCharArray().Where(c => !Char.IsWhiteSpace(c)).ToArray()); }
            else
            {
                string _blanks = "";
                for (int i = 0; i < NumBlanks; i++) { _blanks += " "; }
                return Regex.Replace(String, " {1,}", _blanks).Trim();
            }
        }
        #endregion methods by Jascha

        [MTFMethod]
        public bool ByteArrayContain(List<byte> ByteToFind, byte[] ByteArray)
        {
            if (ByteArray.Length > 0)
            {
                foreach (var item in ByteToFind)
                {
                    for (int i = 0; i < ByteArray.Length; i++)
                    {
                        if (ByteArray[i] == item)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        [MTFMethod]
        public string TimeAfter(int hours, int minutes, int seconds)
        {
            System.DateTime finishTime = System.DateTime.Now.AddHours(hours).AddMinutes(minutes).AddSeconds(seconds);
            return finishTime.ToString("hh:mm tt", System.Globalization.DateTimeFormatInfo.InvariantInfo);
        }

        [MTFMethod]
        public int ExecuteProcess(string fullPath, List<String> args)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = fullPath;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            if (args != null)
            {
                startInfo.Arguments = string.Join(" ", args);
            }

            using (Process exeProcess = Process.Start(startInfo))
            {
                exeProcess.WaitForExit();
                return exeProcess.ExitCode;
            }
        }

        [MTFMethod]
        public MTFDateTime GetDateTimeNow()
        {
            return MTFDateTime.Now();
        }

        [MTFMethod]
        public int GetProductionWeek()
        {
            // ISO 8601
            Calendar calendar = CultureInfo.InvariantCulture.Calendar;
            DateTime time = DateTime.Now;
            DayOfWeek day = calendar.GetDayOfWeek(time);

            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            return calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        [MTFMethod]
        public void WakeUpInSpecifiedUpTime(double seconds)
        {

           double milisecondsToWait = seconds * 1000 - getUptime();

            if (milisecondsToWait > 0)
            {
                Stopwatch timer = Stopwatch.StartNew();

                LongWaiting((long)milisecondsToWait, timer);
            }
        }

        [MTFMethod]
        [MTFAdditionalParameterInfo(ParameterName = "number", DisplayName = "Input Number")]
        [MTFAdditionalParameterInfo(ParameterName = "minimalNumberOfCharacters", DisplayName = "Minimal Number Of Characters", DefaultValue = "1")]
        public string GetHexString(int number, int minimalNumberOfCharacters)
        {
            if (minimalNumberOfCharacters >= 1)
            {
                return number.ToString("X" + minimalNumberOfCharacters);
            }
            else
            {
                return number.ToString("X");
            }
        }

        //[MTFMethod]
        [MTFMethod(Description = "This method returns relative time from 2 timestamps in cDAQ timestamps", DisplayName = "Get relative time")]
        public double GetRelativeFromCdaq(MTFDateTime baseTime, MTFDateTime actualTime)
        {
            return MTFDateTime.GetRelativeTime(baseTime, actualTime);
        }

        [MTFMethod]
        public void AddToTable(string tableName, string rowPrefix, IEnumerable<object> array)
        {
            int i = 0;
            List<ValidationRowContainer> validationRows = new List<ValidationRowContainer>();
            foreach (var o in array)
            {
                validationRows.Add(new ValidationRowContainer
                {
                    RowName = string.Format("{0} {1}", rowPrefix, i),
                    Value = o,
                });
                i++;
            }
            runtimeContext.AddRangeToValidationTable(tableName, validationRows);
        }

        [MTFMethod]
        public string ReadSystemVariable(string variableName)
        {
            return Environment.GetEnvironmentVariable(variableName);
        }

        [MTFMethod]
        public void WriteSystemVariable(string variableName, string value)
        {
            Environment.SetEnvironmentVariable(variableName, value);
        }

        private Dictionary<string, CompilerResults> compiledCode = new Dictionary<string, CompilerResults>();
        [MTFMethod]
        public void CompileCode(List<ExecuteCodeParameter> parameters, string name, string sourceCode)
        {
            compiledCode[name] = compileCode(parameters, sourceCode);
        }

        [MTFMethod]
        public void CompileCodes(List<SourceCode> sourceCodes)
        {
            foreach (var sourceCode in sourceCodes)
            {
                try
                {
                    compiledCode[sourceCode.Name] = compileCode(sourceCode.Parameters, sourceCode.SourceCodeText);
                }
                catch (Exception e)
                {
                    throw new Exception(string.Format("Error in compiling {0}. Compile error: {1}", sourceCode.Name, e.Message));
                }
            }
        }

        [MTFMethod]
        public object ExecuteCompiledCode(string name, List<ExecuteCodeParameter> parameters)
        {
            if (!compiledCode.ContainsKey(name))
            {
                throw new Exception(string.Format("Code with name {0} isn't compiled", name));
            }

            return executeCompiledCode(parameters, compiledCode[name]);
        }

        [MTFMethod]
        public object ExecuteCode(List<ExecuteCodeParameter> parameters, string code)
        {
            var cr = compileCode(parameters, code);
            var result = executeCompiledCode(parameters, cr);
            SetRuntimeContext(cr, null);

            return result;
        }

        private void SetRuntimeContext(CompilerResults cr, IMTFSequenceRuntimeContext value)
        {
            cr.CompiledAssembly.GetType("CodeExecuter.Executer").GetProperty("RuntimeContext").SetMethod.Invoke(null, new object[] { value });
        }

        private object executeCompiledCode(List<ExecuteCodeParameter> parameters, CompilerResults cr)
        {
            List<object> param = new List<object>();
            if (parameters != null)
            {
                param.AddRange(parameters.Select(p => p.TypeName == typeof(object).FullName ? p.ObjectValue : MTFCommon.Helpers.TypeHelper.ConvertValue(p.Value, Type.GetType(p.TypeName))));
            }
            else
            {
                param = null;
            }

            try
            {
                return cr.CompiledAssembly.GetType("CodeExecuter.Executer")
                    .GetMethod("Run")
                    .Invoke(null, param != null ? param.ToArray() : null);
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

        private CompilerResults compileCode(List<ExecuteCodeParameter> parameters, string code)
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
            sb.AppendLine("public static AutomotiveLighting.MTFCommon.IMTFSequenceRuntimeContext RuntimeContext { get; set; }");


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
                    sb.Append(string.Format("{0} {1}", p.TypeName, p.Name));
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

            SetRuntimeContext(cr, runtimeContext);

            return cr;
        }

        [MTFMethod(DisplayName = "Execute code with string result")]
        public string ExecuteCodeWithStringResult(List<ExecuteCodeParameter> parameters, string code)
        {
            var result = this.ExecuteCode(parameters, code);

            return Convert.ToString(result);
        }
        #endregion

        #region Joergs' methods

        [MTFMethod(DisplayName = "File Manager", Description = "Copies, Moves or deletes a given file.")]
        [MTFAllowedParameterValue("operation", "Copy", 0)]
        [MTFAllowedParameterValue("operation", "Move", 1)]
        [MTFAllowedParameterValue("operation", "Delete [Ignores Target]", 2)]
        [MTFAllowedParameterValue("operation", "Move to timestamped version [Ignores Target]", 3)]
        public string Filemanager(int operation, string Source, string Target)
        {
            try
            {
                switch (operation)
                {
                    case 0: //Copy
                        File.Copy(Source, Target);
                        return Target;
                    case 1: //Move
                        File.Move(Source, Target);
                        return Target;
                    case 2: //delete
                        File.Delete(Source);
                        return Source + " was deleted.";
                    case 3: //Move to filestamped version
                        Target = Path.GetFullPath(Source) + "\\" + Path.GetFileNameWithoutExtension(Source) + DateTime.Now.ToString("-yyyy-MM-dd-HHmm") + Path.GetExtension(Source);
                        File.Move(Source, Target);
                        return Target;
                    default:
                        return "Not implemented";
                }
            }
            catch (Exception e)
            {
                return "Error: " + e.Message;
            }
        }

        private Dictionary<string, CSVLogger> CSVLoggers = new Dictionary<string, CSVLogger>();

        [MTFMethod(DisplayName = "CSV Logging start", Description = "Starts Logging to a file")]
        [MTFAllowedParameterValue("Mode", "Append", 0)]
        [MTFAllowedParameterValue("Mode", "Overwrite", 1)]
        [MTFAdditionalParameterInfo(ParameterName = "Delimeter", DefaultValue = ";")]
        public string CSVLoggingStart(String FileName, List<String> Header, int Mode, string Delimeter)
        {
            //If we have the same logger in the directory, we kill it
            if (CSVLoggers.ContainsKey(FileName))
            {
                CSVLoggers.Remove(FileName);
                GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                GC.Collect();
            }

            //Adding new logger to dictionary
            CSVLoggers.Add(FileName, new CSVLogger(FileName, Mode, Header));
            return FileName;
        }

        [MTFMethod(DisplayName = "CSV Logging stop", Description = "Stops Logging to a file. True if the file is opened.")]
        public bool CSVLoggingStop(String FileName)
        {
            try
            {
                CSVLoggers[FileName].Close();
                CSVLoggers[FileName].Dispose();
                CSVLoggers.Remove(FileName);
                //GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                //GC.Collect();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        [MTFMethod(DisplayName = "CSV Logging log", Description = "Logs a list of objects to the file. If an optinal data is not given, the acual date is taken.")]
        public bool CSVLoggingLog(String FileName, List<String> data, MTFDateTime OptionalDate)
        {
            try
            {
                CSVLoggers[FileName].Write(OptionalDate, data);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private void CleanCSVLogging()
        {
            if (CSVLoggers == null)
            {
                return;
            }

            foreach (var csvLogger in CSVLoggers.Values)
            {
                csvLogger.Dispose();
            }
        }
        #endregion Joergs' methods

        private bool stop = false;
        public bool Stop
        {
            set { stop = value; }
        }

        public void Dispose()
        {
            CleanCSVLogging();

            if (compiledCode != null)
            {
                foreach (var cr in compiledCode.Values)
                {
                    if (cr != null)
                    {
                        SetRuntimeContext(cr, null);
                    }
                }
            }
        }
    }

    [MTFKnownClass]
    public class ExecuteCodeParameter
    {
        public string Name { get; set; }

        [MTFAllowedPropertyValue("Int32", "System.Int32")]
        [MTFAllowedPropertyValue("String", "System.String")]
        [MTFAllowedPropertyValue("Decimal", "System.Decimal")]
        [MTFAllowedPropertyValue("Object", "System.Object")]
        [MTFAllowedPropertyValue("Double", "System.Double")]
        [MTFAllowedPropertyValue("Boolean", "System.Boolean")]
        public string TypeName { get; set; }

        public string Value { get; set; }

        public object ObjectValue { get; set; }
    }

    [MTFKnownClass]
    public class SourceCode
    {
        public List<ExecuteCodeParameter> Parameters { get; set; }
        public string Name { get; set; }
        public string SourceCodeText { get; set; }
    }
}
