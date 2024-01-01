using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.Helpers;

namespace MTFClientServerCommon
{
    public static class SystemLog
    {
        private const string logFileName = "SystemLog";
        private const string logFileExtension = "log";
        private const string indent = "    ";
        private static bool flushToFile = true;
        static StreamWriter logFile;
        private static readonly object fileLock = new object();

        public static string LogDirectory { get; set; }

        public static void LogMessage(string message) => LogMessage(message, false);

        public static void LogMessage(string message, string header) => LogMessage(message, header, false);

        public static void LogMessage(string message, string header, bool saveCallStack)
        {
            lock (fileLock)
            {
                StringBuilder sb = new StringBuilder();
                generateHeader(sb, header);
                sb.Append(": ").AppendLine(message);

                if (saveCallStack)
                {
                    writeCallStack(sb, 2);
                }

                flushMessage(sb.ToString());
            }
        }

        public static void LogMessage(string message, bool saveCallStack)
        {
            lock (fileLock)
            {
                StringBuilder sb = new StringBuilder();
                generateHeader(sb);
                sb.AppendLine().AppendLine(addIndent(message, 1)).AppendLine();

                if (saveCallStack)
                {
                    writeCallStack(sb, 2);
                }

                flushMessage(sb.ToString());
            }
        }

        public static void LogException(Exception e)
        {
            lock (fileLock)
            {
                StringBuilder sb = new StringBuilder();
                generateHeader(sb);

                sb.AppendLine();
                logExceptionMessage(e, sb, 1);
                sb.AppendLine();

                logExceptionStackTrace(e, sb, 1);
                sb.AppendLine();

                flushMessage(sb.ToString());
            }
        }

        private static void logExceptionMessage(Exception e, StringBuilder sb, int indentLevel)
        {
            if (!string.IsNullOrEmpty(e.Source))
            {
                sb.Append(addIndent($"<{e.Source}> ", indentLevel));
                writeMethod(e.TargetSite, sb, 0);
                sb.AppendLine();
            }
            sb.AppendLine(addIndent(e.Message, indentLevel)).AppendLine();

            if (e.InnerException != null)
            {
                logExceptionMessage(e.InnerException, sb, indentLevel + 1);
            }
        }

        private static void logExceptionStackTrace(Exception e, StringBuilder sb, int indentLevel)
        {
            if (e.StackTrace != null)
            {
                sb.AppendLine(addIndent(e.StackTrace, indentLevel));
                if (e.InnerException != null)
                {
                    logExceptionStackTrace(e.InnerException, sb, indentLevel + 1);
                }
            }
        }
        
        private static void openLogfile()
        {
            FileHelper.CreateDirectory(LogDirectory);

            string logFileNameBase = Path.Combine(LogDirectory, $"{logFileName} {DateTime.Now.ToString("yyyy-MM-dd-HH-mm")}");
            string fileName = logFileNameBase;
            int i = 1;
            while (File.Exists($"{fileName}.{logFileExtension}"))
            {
                fileName = logFileNameBase + "-" + i;
                i++;
            }
            logFile = new StreamWriter($"{fileName}.{logFileExtension}");
            FileHelper.SetFileForEveryone($"{fileName}.{logFileExtension}");
            StringBuilder fileHeader = new StringBuilder()
                .AppendLine($"{AppDomain.CurrentDomain.FriendlyName} version {Assembly.GetExecutingAssembly().GetName().Version}")
                .AppendLine($"Environment current directory: {Environment.CurrentDirectory}")
                .AppendLine($"System log directory: {LogDirectory}")
                .AppendLine();

            flushMessage(fileHeader.ToString());
        }
        
        private static void flushMessage(string message)
        {
            if (flushToFile)
            {
                if (logFile == null)
                {
                    openLogfile();
                }


                logFile.Write(message);
                logFile.Flush();
            }
        }

        private static void generateHeader(StringBuilder sb, string headerText)
        {
            sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Append(" ").Append(headerText);   
        }

        private static void generateHeader(StringBuilder sb)
        {
            StackFrame frame = new StackFrame(2);
            StringBuilder frameBuilder = new StringBuilder();
            writeFrame(frame, frameBuilder, 2);

            generateHeader(sb, frameBuilder.ToString());
        }

        private static void writeCallStack(StringBuilder sb, int indentLevel)
        {
            sb.AppendLine();
            sb.AppendLine(addIndent("CallStack:", 1));
            StackTrace trace = new StackTrace();
            int counter = 0;
            int skipCount = 2;
            foreach (var frame in trace.GetFrames())
            {
                if (counter > skipCount - 1)
                {
                    writeFrame(frame, sb, indentLevel);
                    sb.AppendLine();
                }
                counter++;
            }
        }

        private static void writeFrame(StackFrame frame, StringBuilder sb, int indentLevel)
        {
            if (frame != null)
            {
                writeMethod(frame.GetMethod(), sb, indentLevel);
            }
        }

        private static void writeMethod(MethodBase method, StringBuilder sb, int indentLevel)
        {
            if (method != null)
            {
                sb.Append(addIndent($"{(method.ReflectedType == null ? method.Name : method.ReflectedType.FullName)}.{method.Name}", indentLevel)).Append("(");
                bool first = true;
                foreach (var param in method.GetParameters())
                {
                    if (!first)
                    {
                        sb.Append(", ");
                    }
                    sb.Append(param.ParameterType.FullName).Append(" ").Append(param.Name);
                    first = false;
                }
                sb.Append(")");
            }
        }


        private static string addIndent(string message, int level)
        {
            string myIndent = string.Empty;
            for (int i = 0; i < level; i++)
            {
                myIndent = myIndent + indent;
            }
            return myIndent + message.Replace(Environment.NewLine, Environment.NewLine + myIndent); 
        }
    }
}
