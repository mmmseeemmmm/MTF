using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AL_Dat_Client;
using AutomotiveLighting.MTFCommon;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using MTFCommon.Helpers;

//using MTFBusCommunication.Structures;

namespace BMWCoding
{
    [MTFClass(Name = "BMW Coding", Icon = MTFIcons.AL)]
    [MTFClassCategory("Communication")]
    public class BMWCoding
    {
        private Database projectDatabase;
        private readonly string libPath = "mtfLibs\\BMWCoding";
        //private readonly string codingDataSubPath = "CodingData";
        //private readonly string downloadFileSubPath = "Download";
        //private readonly string eSysArgs = "";
        private readonly string codingDataStorage;
        private readonly string fileXMLname = "DatabaseXMLs";
        

        public IMTFSequenceRuntimeContext RuntimeContext;

        [MTFConstructor]
        [MTFAdditionalParameterInfo(ParameterName = "codingDataPath", Description = "Path where are stored coding data packages")]
        public BMWCoding(string CodingDataPath)
        {
            if (!Directory.Exists(CodingDataPath))
            {
                Directory.CreateDirectory(CodingDataPath);
            }

            if (!Directory.Exists(CodingDataPath + "\\" + fileXMLname))
            {
                Directory.CreateDirectory(CodingDataPath + "\\" + fileXMLname);
            }

            this.projectDatabase = new Database();
            this.projectDatabase.workspace_directory = CodingDataPath + "\\" + fileXMLname;

            this.codingDataStorage = CodingDataPath;
        }

        //private string 

        //private string CodingDataPath
        //{
        //    get { return Path.Combine(Environment.CurrentDirectory, libPath, codingDataSubPath); }
        //}        

        //private string DownloadFilePath
        //{
        //    get { return Path.Combine(Environment.CurrentDirectory, libPath, downloadFileSubPath); }
        //}

        [MTFMethod]
        public MTFHeadlampData[] GetAllECUs()
        {
            return this.projectDatabase.GetAllECUs().Select(MTFHeadlampData.GetHeadlampData).ToArray();
        }

        [MTFMethod]
        public MTFHeadlampData[] GetProjectData(string projectName)
        {
            return this.projectDatabase.GetProjectData(projectName).Select(MTFHeadlampData.GetHeadlampData).ToArray();
        }

        [MTFMethod]
        public MTFHeadlampData GetECUData(string AlPartNr, string AlChangeIndex)
        {
            return MTFHeadlampData.GetHeadlampData(this.projectDatabase.GetECUData(AlPartNr, AlChangeIndex));
        }

        [MTFMethod]
        public MTFHeadlampData GetHeadlampData(string AlPartNr, string AlChangeIndex)
        {
            return MTFHeadlampData.GetHeadlampData(this.projectDatabase.GetHeadlampData(AlPartNr, AlChangeIndex));
        }

        [MTFProperty(Description = "Default value is true, if TRUE - download data from server and update local data, if FALSE - use local xml files with data.")]
        public bool UpdateLocal
        {
            get { return this.projectDatabase.update_local; }
            set { this.projectDatabase.update_local = value; }
        }

        [MTFProperty]
        public string LogicalLink
        {
            set { LogicalLink = value; }
        }

        [MTFMethod]
        public void DownloadAndUnpackCodingData(string DataPath, string CdZipName)
        {
            ProgressNotificationIndeterminate("Downloadig file", true);

            string destFile = Path.Combine(codingDataStorage, CdZipName);

            if (!Directory.Exists(codingDataStorage))
            {
                Directory.CreateDirectory(codingDataStorage);
            }

            if (!File.Exists(destFile))
            {
                DownloadFile(Path.Combine(DataPath, CdZipName), destFile);

                ProgressNotificationIndeterminate("Unpacking coding data", true);

                unzipFile(destFile, codingDataStorage);

                ProgressNotificationIndeterminate("Run AdaptBMWcoding", true);

                runAdaptBmwCoding(destFile);
            }

            ProgressNotificationIndeterminate(string.Empty, false);
        }

        [MTFMethod (Description = "Upload bootloader.")]
        [MTFAdditionalParameterInfo(ParameterName = "E_Sys_parameter", Description = "'-s' - start E-Sys server for coding\nno parameter - E-Sys server will close after coding")]
        [MTFAdditionalParameterInfo(ParameterName = "codingExpirationTimeInSeconds", DefaultValue = "120", Description = "Set value in seconds\nThrow exception when coding time expire")]
        public void CodeBTLD(string CDFileName, string Istep, string E_Sys_parameter, string codingExpirationTimeInSeconds)
        {
            string codingFileName = "Cod_BTLD_" + CDFileName;
            string fileDir = Directory.GetDirectories(this.codingDataStorage).First(o => o.EndsWith(Istep));

            try
            {
                ProgressNotificationIndeterminate("Coding ECU", true);

                using (Process exeProcess = new Process())
                {
                    bool isCoding = true;
                    StringBuilder standardOutputBuilder = new StringBuilder();
                    exeProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                    exeProcess.StartInfo.CreateNoWindow = true;
                    exeProcess.StartInfo.FileName = Path.Combine(fileDir, codingFileName);

                    exeProcess.StartInfo.WorkingDirectory = fileDir;
                    exeProcess.StartInfo.UseShellExecute = false;
                    exeProcess.StartInfo.RedirectStandardOutput = true;
                    //exeProcess.StartInfo.Arguments = "-s";
                    exeProcess.StartInfo.Arguments = E_Sys_parameter;
                    
                    exeProcess.OutputDataReceived += (sender, args) =>
                    {
                        if (args.Data != null)
                        {
                            standardOutputBuilder.Append(args.Data);
                            if (args.Data.Contains("Log File generated, ExitCode:"))
                            {
                                isCoding = false;
                            }
                        }
                    };

                    exeProcess.Start();
                    exeProcess.BeginOutputReadLine();

                    int codingExpTime = 0;
                    Int32.TryParse(codingExpirationTimeInSeconds, out codingExpTime);

                    int i = 0;
                    while (isCoding && i < codingExpTime)
                    {
                        if (i < codingExpTime)
                        {
                            Thread.Sleep(1000);
                            i++;
                        }
                        if (i == (codingExpTime - 1))
                        {
                            exeProcess.Kill();
                            throw new TimeoutException("Coding timeout " + codingExpTime + "(s) expired");
                            
                        }
                    }
                    
                    codingErrorHandling(standardOutputBuilder);
                    
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ProgressNotificationIndeterminate(string.Empty, false);
            }
        }

        [MTFMethod(Description = "Upload application data.")]
        [MTFAdditionalParameterInfo(ParameterName = "E_Sys_parameter", Description = "'-s' - start E-Sys server for coding\nno parameter - E-Sys server will close after coding")]
        [MTFAdditionalParameterInfo(ParameterName = "codingExpirationTimeInSeconds", DefaultValue = "120", Description = "Set value in seconds\nThrow exception when coding time expire")]
        public void CodeSWFL(string CDFileName, string Istep, string E_Sys_parameter, string codingExpirationTimeInSeconds)
        {
            string codingFileName = "Cod_SWFL_" + CDFileName;
            string fileDir = Directory.GetDirectories(this.codingDataStorage).First(o => o.EndsWith(Istep));

            try
            {
                ProgressNotificationIndeterminate("Coding ECU", true);

                using (Process exeProcess = new Process())
                {
                    bool isCoding = true;
                    StringBuilder standardOutputBuilder = new StringBuilder();
                    exeProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                    exeProcess.StartInfo.CreateNoWindow = true;
                    exeProcess.StartInfo.FileName = Path.Combine(fileDir, codingFileName);
                    exeProcess.StartInfo.WorkingDirectory = fileDir;
                    exeProcess.StartInfo.UseShellExecute = false;
                    exeProcess.StartInfo.RedirectStandardOutput = true;
                    //exeProcess.StartInfo.Arguments = "-s";
                    exeProcess.StartInfo.Arguments = E_Sys_parameter;
                    exeProcess.OutputDataReceived += (sender, args) =>
                    {
                        if (args.Data != null)
                        {
                            standardOutputBuilder.Append(args.Data);
                            if (args.Data.Contains("Log File generated, ExitCode:"))
                            {
                                isCoding = false;
                            }
                        }
                    };

                    exeProcess.Start();
                    exeProcess.BeginOutputReadLine();

                    int codingExpTime = 0;
                    Int32.TryParse(codingExpirationTimeInSeconds, out codingExpTime);

                    int i = 0;
                    while (isCoding && i < codingExpTime)
                    {
                        if (i < codingExpTime)
                        {
                            Thread.Sleep(1000);
                            i++;
                        }
                        if (i == (codingExpTime - 1))
                        {
                            exeProcess.Kill();
                            throw new TimeoutException("Coding timeout " + codingExpTime + "(s) expired");
                        }
                    }
                    
                    codingErrorHandling(standardOutputBuilder);
                    
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ProgressNotificationIndeterminate(string.Empty, false);
            }
        }

        [MTFMethod(Description = "Upload data for Laser satellite.")]
        [MTFAdditionalParameterInfo(ParameterName = "E_Sys_parameter", Description = "'-s' - start E-Sys server for coding\nno parameter - E-Sys server will close after coding")]
        [MTFAdditionalParameterInfo(ParameterName = "codingExpirationTimeInSeconds", DefaultValue = "120", Description = "Set value in seconds\nThrow exception when coding time expire")]
        public void CodeSWFK(string CDFileName, string Istep, string E_Sys_parameter, string codingExpirationTimeInSeconds)
        {
            string codingFileName = "Cod_SWFK_" + CDFileName;
            string fileDir = Directory.GetDirectories(this.codingDataStorage).First(o => o.EndsWith(Istep));

            try
            {
                ProgressNotificationIndeterminate("Coding ECU", true);

                using (Process exeProcess = new Process())
                {
                    bool isCoding = true;
                    StringBuilder standardOutputBuilder = new StringBuilder();
                    exeProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                    exeProcess.StartInfo.CreateNoWindow = true;
                    exeProcess.StartInfo.FileName = Path.Combine(fileDir, codingFileName);
                    exeProcess.StartInfo.WorkingDirectory = fileDir;
                    exeProcess.StartInfo.UseShellExecute = false;
                    exeProcess.StartInfo.RedirectStandardOutput = true;
                    //exeProcess.StartInfo.Arguments = "-s";
                    exeProcess.StartInfo.Arguments = E_Sys_parameter;
                    exeProcess.OutputDataReceived += (sender, args) =>
                    {
                        if (args.Data != null)
                        {
                            standardOutputBuilder.Append(args.Data);
                            if (args.Data.Contains("Log File generated, ExitCode:"))
                            {
                                isCoding = false;
                            }
                        }
                    };

                    exeProcess.Start();
                    exeProcess.BeginOutputReadLine();

                    int codingExpTime = 0;
                    Int32.TryParse(codingExpirationTimeInSeconds, out codingExpTime);

                    int i = 0;
                    while (isCoding && i < codingExpTime)
                    {
                        if (i < codingExpTime)
                        {
                            Thread.Sleep(1000);
                            i++;
                        }
                        if (i == (codingExpTime - 1))
                        {
                            exeProcess.Kill();
                            throw new TimeoutException("Coding timeout " + codingExpTime + "(s) expired");
                        }
                    }


                    codingErrorHandling(standardOutputBuilder);
                    
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ProgressNotificationIndeterminate(string.Empty, false);
            }
        }

        [MTFMethod(Description = "Upload data for laser satellite and coding data in one step.")]
        [MTFAdditionalParameterInfo(ParameterName = "E_Sys_parameter", Description = "'-s' - start E-Sys server for coding\nno parameter - E-Sys server will close after coding")]
        [MTFAdditionalParameterInfo(ParameterName = "codingExpirationTimeInSeconds", DefaultValue = "120", Description = "Set value in seconds\nThrow exception when coding time expire")]
        public void CodeSWFK_CAFD(string CDFileName, string Istep, string E_Sys_parameter, string codingExpirationTimeInSeconds)
        {
            string codingFileName = "Cod_SWFK_CAFD_" + CDFileName;
            string fileDir = Directory.GetDirectories(this.codingDataStorage).First(o => o.EndsWith(Istep));

            try
            {
                ProgressNotificationIndeterminate("Coding ECU", true);

                using (Process exeProcess = new Process())
                {
                    bool isCoding = true;
                    StringBuilder standardOutputBuilder = new StringBuilder();
                    exeProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                    exeProcess.StartInfo.CreateNoWindow = true;
                    exeProcess.StartInfo.FileName = Path.Combine(fileDir, codingFileName);
                    exeProcess.StartInfo.WorkingDirectory = fileDir;
                    exeProcess.StartInfo.UseShellExecute = false;
                    exeProcess.StartInfo.RedirectStandardOutput = true;
                    //exeProcess.StartInfo.Arguments = "-s";
                    exeProcess.StartInfo.Arguments = E_Sys_parameter;
                    exeProcess.OutputDataReceived += (sender, args) =>
                    {
                        if (args.Data != null)
                        {
                            standardOutputBuilder.Append(args.Data);
                            if (args.Data.Contains("Log File generated, ExitCode:"))
                            {
                                isCoding = false;
                            }
                        }
                    };

                    exeProcess.Start();
                    exeProcess.BeginOutputReadLine();

                    int codingExpTime = 0;
                    Int32.TryParse(codingExpirationTimeInSeconds, out codingExpTime);

                    int i = 0;
                    while (isCoding && i < codingExpTime)
                    {
                        if (i < codingExpTime)
                        {
                            Thread.Sleep(1000);
                            i++;
                        }
                        if (i == (codingExpTime - 1))
                        {
                            exeProcess.Kill();
                            throw new TimeoutException("Coding timeout " + codingExpTime + "(s) expired");
                        }
                    }

                    codingErrorHandling(standardOutputBuilder);

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ProgressNotificationIndeterminate(string.Empty, false);
            }
        }

        [MTFMethod(Description = "Upload coding data.")]
        [MTFAdditionalParameterInfo(ParameterName = "E_Sys_parameter", Description = "'-s' - start E-Sys server for coding\nno parameter - E-Sys server will close after coding")]
        [MTFAdditionalParameterInfo(ParameterName = "codingExpirationTimeInSeconds", DefaultValue = "60", Description = "Set value in seconds\nThrow exception when coding time expire")]
        public void CodeCAFD(string CDFileName, string Istep, string E_Sys_parameter, string codingExpirationTimeInSeconds)
        {
            string codingFileName = "Cod_CAFD_" + CDFileName;
            string fileDir = Directory.GetDirectories(this.codingDataStorage).First(o => o.EndsWith(Istep));

            try
            {
                ProgressNotificationIndeterminate("Coding ECU", true);
                
                //using (Process exeProcess = Process.Start(psi))
                using (Process exeProcess = new Process())
                {
                    bool isCoding = true;
                    StringBuilder standardOutputBuilder = new StringBuilder();
                    exeProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                    exeProcess.StartInfo.CreateNoWindow = true;
                    exeProcess.StartInfo.FileName = Path.Combine(fileDir, codingFileName);
                    exeProcess.StartInfo.WorkingDirectory = fileDir;
                    exeProcess.StartInfo.UseShellExecute = false;
                    exeProcess.StartInfo.RedirectStandardOutput = true;
                    //exeProcess.StartInfo.Arguments = "-s";
                    exeProcess.StartInfo.Arguments = E_Sys_parameter;
                    exeProcess.OutputDataReceived += (sender, args) =>
                                                     {
                                                         if (args.Data != null)
                                                         {
                                                             standardOutputBuilder.Append(args.Data);
                                                             if (args.Data.Contains("Log File generated, ExitCode:"))
                                                             {
                                                                 isCoding = false;
                                                             }
                                                         }
                                                     };

                    exeProcess.Start();
                    exeProcess.BeginOutputReadLine();

                    int codingExpTime = 0;
                    Int32.TryParse(codingExpirationTimeInSeconds, out codingExpTime);

                    int i = 0;

                    while (isCoding && i < codingExpTime)
                    {
                        if (i < codingExpTime)
                        {
                            Thread.Sleep(1000);
                            i++;
                        }
                        if (i == (codingExpTime - 1))
                        {
                            exeProcess.Kill();
                            throw new TimeoutException("Coding timeout " + codingExpTime + "(s) expired");
                        }
                    }
                    
                    codingErrorHandling(standardOutputBuilder);

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ProgressNotificationIndeterminate(string.Empty, false);
            }
        }

        

        [MTFMethod(Description = "Upload bootloader, application software ,(laser satellite data) and coding data in one step.")]
        [MTFAdditionalParameterInfo(ParameterName = "E_Sys_parameter", Description = "'-s' - start E-Sys server for coding\nno parameter - E-Sys server will close after coding")]
        [MTFAdditionalParameterInfo(ParameterName = "codingExpirationTimeInSeconds", DefaultValue = "180", Description = "Set value in seconds\nThrow exception when coding time expire")]
        public void CodeALL(string CDFileName, string Istep, string E_Sys_parameter, string codingExpirationTimeInSeconds)
        {
            string codingFileName = "Cod_All_" + CDFileName;
            string fileDir = Directory.GetDirectories(this.codingDataStorage).First(o => o.EndsWith(Istep));

            try
            {
                ProgressNotificationIndeterminate("Coding ECU", true);

                using (Process exeProcess = new Process())
                {
                    bool isCoding = true;
                    StringBuilder standardOutputBuilder = new StringBuilder();
                    exeProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                    exeProcess.StartInfo.CreateNoWindow = true;
                    exeProcess.StartInfo.FileName = Path.Combine(fileDir, codingFileName);
                    exeProcess.StartInfo.WorkingDirectory = fileDir;
                    exeProcess.StartInfo.UseShellExecute = false;
                    exeProcess.StartInfo.RedirectStandardOutput = true;
                    //exeProcess.StartInfo.Arguments = "-s";
                    exeProcess.StartInfo.Arguments = E_Sys_parameter;
                    exeProcess.OutputDataReceived += (sender, args) =>
                    {
                        if (args.Data != null)
                        {
                            standardOutputBuilder.Append(args.Data);
                            if (args.Data.Contains("Log File generated, ExitCode:"))
                            {
                                isCoding = false;
                            }
                        }
                    };

                    exeProcess.Start();
                    exeProcess.BeginOutputReadLine();

                    int codingExpTime = 0;
                    Int32.TryParse(codingExpirationTimeInSeconds, out codingExpTime);

                    int i = 0;
                    while (isCoding && i < codingExpTime)
                    {
                        if (i < codingExpTime)
                        {
                            Thread.Sleep(1000);
                            i++;
                        }
                        if (i == (codingExpTime - 1))
                        {
                            exeProcess.Kill();
                            throw new TimeoutException("Coding timeout " + codingExpTime + "(s) expired");
                        }
                    }
                   
                    codingErrorHandling(standardOutputBuilder);
                    
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ProgressNotificationIndeterminate(string.Empty, false);
            }
        }

        [MTFMethod(Description = "Upload shipment data.")]
        [MTFAdditionalParameterInfo(ParameterName = "E_Sys_parameter", Description = "'-sp' - prepare E-Sys server for uploading shipment data\n" +
                                                                                     "'-sr' - run E-Sys server for uploading shipment data\n" +
                                                                                     "no parameter - E-Sys server will close after shipment coding")]
        [MTFAdditionalParameterInfo(ParameterName = "codingExpirationTimeInSeconds", DefaultValue = "60", Description = "Set value in seconds\nThrow exception when coding time expire")]
        public void CodeSHIP(string CDFileName, string Istep, string E_Sys_parameter, string codingExpirationTimeInSeconds)
        {
            string codingFileName = "Ship_" + CDFileName;
            string fileDir = Directory.GetDirectories(this.codingDataStorage).First(o => o.EndsWith(Istep));

            try
            {
                ProgressNotificationIndeterminate("Coding ECU", true);

                using (Process exeProcess = new Process())
                {
                    bool isCoding = true;
                    StringBuilder standardOutputBuilder = new StringBuilder();
                    exeProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                    exeProcess.StartInfo.CreateNoWindow = true;
                    exeProcess.StartInfo.FileName = Path.Combine(fileDir, codingFileName);
                    exeProcess.StartInfo.WorkingDirectory = fileDir;
                    exeProcess.StartInfo.UseShellExecute = false;
                    exeProcess.StartInfo.RedirectStandardOutput = true;
                    //exeProcess.StartInfo.Arguments = "-s";
                    exeProcess.StartInfo.Arguments = E_Sys_parameter;
                    exeProcess.OutputDataReceived += (sender, args) =>
                    {
                        if (args.Data != null)
                        {
                            standardOutputBuilder.Append(args.Data);
                            if (args.Data.Contains("Log File generated, ExitCode:"))
                            {
                                isCoding = false;
                            }
                        }
                    };

                    exeProcess.Start();
                    exeProcess.BeginOutputReadLine();

                    int codingExpTime = 0;
                    Int32.TryParse(codingExpirationTimeInSeconds, out codingExpTime);

                    int i = 0;
                    while (isCoding && i < codingExpTime)
                    {
                        if (i < codingExpTime)
                        {
                            Thread.Sleep(1000);
                            i++;
                        }
                        if (i == (codingExpTime - 1))
                        {
                            exeProcess.Kill();
                            throw new TimeoutException("Coding timeout " + codingExpTime + "(s) expired");
                        }
                    }

                    codingErrorHandling(standardOutputBuilder);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ProgressNotificationIndeterminate(string.Empty, false);
            }
        }

        private void ProgressNotificationIndeterminate(string text, bool isStarted)
        {
            if (RuntimeContext != null)
            {
                RuntimeContext.ProgressNotificationIndeterminate(text, isStarted);
            }
        }

        private string getPathToUnzipFile(string pathToDataFile)
        {
            string[] path = pathToDataFile.Split('.');
            return path[0];
        }

        private void runAdaptBmwCoding(string pathToZipFile)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.FileName = "AdaptBMWcoding.exe";
            //psi.WorkingDirectory = getPathToUnzipFile(pathToZipFile);
            //Char delimiter = '.';
            //String[] substrings = pathToZipFile.Split(delimiter);

            psi.WorkingDirectory = getPathToUnzipFile(pathToZipFile);
            psi.CreateNoWindow = false;

            try
            {
                using (Process exeProcess = Process.Start(psi))
                {
                    exeProcess.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void unzipFile(string pathToArchive, string outFolder)
        {
            ZipFile zipFile = null;

            try
            {
                FileStream fs = File.OpenRead(pathToArchive);
                zipFile = new ZipFile(fs);

                foreach (ZipEntry zipEntry in zipFile)
                {
                    if (!zipEntry.IsFile)
                    {
                        continue;
                    }

                    String entryFileName = zipEntry.Name;
                    byte[] buffer = new byte[4096];
                    Stream zipStream = zipFile.GetInputStream(zipEntry);

                    string fullZipToPath = Path.Combine(outFolder, entryFileName);
                    string directoryName = Path.GetDirectoryName(fullZipToPath);

                    if (directoryName.Length > 0)
                    {
                        Directory.CreateDirectory(directoryName);
                    }

                    using (FileStream streamWriter = File.Create(fullZipToPath))
                    {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                }
            }
            finally 
            {
                if (zipFile != null)
                {
                    zipFile.IsStreamOwner = true;
                    zipFile.Close();
                }
            }
        }

        private void DownloadFile(string source, string destination)
        {
            var sourceDirectory = Path.GetDirectoryName(source).Trim();
            var username = "data_reader";
            var password = "Storage01";

            var command = "NET USE " + sourceDirectory + " /delete";
            ExecuteCommand(command);

            command = "NET USE " + sourceDirectory + " /user:" + username + " " + password;
            ExecuteCommand(command);

            command = " copy \"" + source + "\"  \"" + destination + "\"";
            ExecuteCommand(command);

            command = "NET USE " + sourceDirectory + " /delete";
            ExecuteCommand(command);
        }

        private int ExecuteCommand(string command)
        {
            var processInfo = new ProcessStartInfo("cmd.exe", "/C " + command)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = Environment.CurrentDirectory,
            };

            var process = Process.Start(processInfo);
            process.WaitForExit();
            var exitCode = process.ExitCode;
            process.Close();

            return exitCode;
        }

        private void codingErrorHandling(StringBuilder outputText)
        {
            var standardOutput = outputText.ToString();
            int startIndex = standardOutput.LastIndexOf("Process E-Sys.bat Exit Code:");
            string errorCode = standardOutput.Substring(startIndex, 33);

            if (!errorCode.Contains("Process E-Sys.bat Exit Code: 0"))
            {
                throw new Exception("Coding error: " + standardOutput);
            }
        }
    }
}
