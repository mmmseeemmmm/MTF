// ==========================================================================
// AUTHOR       : Jascha Breithaupt (jascha.breithaupt@al-lighting.com)
// CREATE DATE  : 19 Feb 2019
// PURPOSE      : Extension for file and directory handling in MTF
// SPECIAL NOTES: needs DotNetZip.dll to work properly
// ==========================================================================
// Change History:
// 
// 2019-08-02 (Jascha Breithaupt): CHANGED ImportContent(): added parameter "DeleteContainingFilesFolders" -> deletes folder with all its content
//===========================================================================

using AutomotiveLighting.MTFCommon;
//using ArtisanCode.SimpleAesEncryption;
using Ionic.Zip;
using System;
using System.Collections.Generic;
//using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
//using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace MTFFileManager
{
    [MTFClass(Name = "File Manager",
              Description = "Description:\n"
                          + "======================================\n"
                          + "File and Directory operations.\n"
                          + "\n"
                          + "Functions:\n"
                          + "======================================\n"
                          + "- Compress File\n"
                          + "- Copy And Rename File\n"
                          + "- Copy File\n"
                          + "- Delete File\n"
                          + "- Move File\n"
                          + "- Rename File\n"
                          + "- Compress Directory\n"
                          + "- Create Directory\n"
                          + "- Delete Directory\n"
                          + "- Move Directory\n"
                          + "- Decrypt File\n"
                          + "- Encrypt File\n"
                          + "\n"
                          + "Changelog:\n"
                          + "======================================\n"
                          + "1.0.0.3 (02 Aug 2019)\n"
                          + "" + "- changed Delete Directory\n"
                          + "\t-> " + "added parameter \"DeleteContainingFilesFolders\"\n"
                          + "1.0.0.2 (03 Mär 2019)\n"
                          + "" + "- changed File Operations to File Manager\n"
                          + "1.0.0.1 (25 Feb 2019)\n"
                          + "" + "- added check of TargetPath\n"
                          + "1.0.0.0 (19 Feb 2019)\n"
                          + "" + "- initial release\n",
              Icon = MTFIcons.NewFile)]//TODO
    [MTFClassCategory("File Manager")]
    public class FileOperations
    {
        // *************************************************************************************************************************************
        // VARIABLES AND CONSTS
        // *************************************************************************************************************************************
        public const string _descSourceFilePath = "Path of source/origin file";
        public const string _descSourceFileName = "Name (with extension) of source/origin file";
        public const string _descTargetFilePath = "Path of target file";
        public const string _descTargetFileName = "Name (with extension) of target file";
        public const string _descSourceTargetFileName = "Name (with extension) of source and target file";
        public const string _descAppendTargetFileName = "String (TargetFileName) will be appended to SourceFileName";
        public const string _descOverride = "File will be overwritten if already exists";
        public const string _descMakeVersion = "File will be versionised if already exists";
        public const string _descMakeReadOnly = "Target will be read-only";
        public const string _descDeleteOrigin = "Delete source file(s)";

        public bool _zipLibAvailable = true;

        //!!!WARNING: BE AWARE THAT THIS IS NOT REALLY SAFE (placing key in text)!!!
        public RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
        public const string rjdlKey = "=Hx5DPzQdcoBjPdPZyUqQgmvggwY4BgV";
        // *************************************************************************************************************************************
        // CONSTRUCTORS
        // *************************************************************************************************************************************
        [MTFConstructor]
        public FileOperations()
        {
            //try
            //{
            //    var _path = Path.GetFullPath(@".\DotNetZip.dll");
            //    var DLL = Assembly.LoadFile(_path);

            //    foreach (Type type in DLL.GetExportedTypes())
            //    {                  
            //        dynamic c = Activator.CreateInstance(type);
            //        c.Output(@"Hello");
            //    }
            //}
            //catch (Exception)
            //{}
            //find out if assembly is available
            //NOTE: this seems not to work with MTF!
            //var assemblies = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
            //                  from type in assembly.GetTypes()
            //                  where type.FullName == ("Ionic.Zip.ZipFile")
            //                  select type);

            ////set flag
            //if (assemblies.Count() != 0) { _zipLibAvailable = true; }//TODO: true
            //else { _zipLibAvailable = false; }
        }

        // *************************************************************************************************************************************
        // METHODS
        // *************************************************************************************************************************************
        // *************************************************************************************************************************************
        // -- FILE
        // *************************************************************************************************************************************
        #region file
        [MTFMethod(Description = "Compresses file.",
                   DisplayName = "0 - Compress File")]
        [MTFAdditionalParameterInfo(ParameterName = "SourceFilePath", Description = _descSourceFilePath)]
        [MTFAdditionalParameterInfo(ParameterName = "SourceFileName", Description = _descSourceFileName)]
        [MTFAdditionalParameterInfo(ParameterName = "OutputFormat", Description = "Output format of compressed file")]
        [MTFAllowedParameterValue("OutputFormat", "ZIP", "ZIP")]
        [MTFAdditionalParameterInfo(ParameterName = "Override", Description = _descOverride)]
        [MTFAdditionalParameterInfo(ParameterName = "MakeVersion", Description = _descMakeVersion)]
        [MTFAdditionalParameterInfo(ParameterName = "MakeReadOnly", Description = _descMakeReadOnly)]
        [MTFAdditionalParameterInfo(ParameterName = "DeleteOrigin", Description = _descDeleteOrigin)]
        public string[] FileCompress(string SourceFilePath, string SourceFileName, string OutputFormat, bool Override = false, bool MakeVersion = false, bool MakeReadOnly = false, bool SetPassword = false, string Password = "", bool DeleteOrigin = false)
        {
            CheckPathHasExtension(SourceFilePath);

            string sourceFile = System.IO.Path.Combine(SourceFilePath, SourceFileName);
            string targetFile = System.IO.Path.Combine(SourceFilePath, SourceFileName + ".zip");

            //sanity check
            CheckOverrideMakeVersion(Override, MakeVersion);
            CheckFileExists(sourceFile);
            if (!Override && File.Exists(targetFile) && !MakeVersion) { throw new Exception("TargetFileName (" + SourceFileName + ".zip" + ") does already exist in TargetPath (" + SourceFilePath + ")! If you want to override check Override parameter!"); }

            //version file
            if (MakeVersion) { targetFile = GetNextFileVersionName(targetFile); }

            if (_zipLibAvailable)
            {
                Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile();

                //encrypt
                //zip.Encryption = EncryptionAlgorithm.WinZipAes256; //TODO           

                //set password
                if (SetPassword) { zip.Password = Password; }

                //create zip
                zip.AddFile(System.IO.Path.GetFullPath(sourceFile), "");

                //save file
                zip.Save(targetFile);
            }

            else
            {
                //create zip file
                MakeZipFile(sourceFile, targetFile);
            }

            //readonly
            if (MakeReadOnly) { SetFileReadOnlyAttribute(targetFile, true); }

            //delete origin
            if (DeleteOrigin)
            { FileDelete(SourceFilePath, SourceFileName); }

            //return filepath ([0]), filename ([1]) and full path ([2])
            return new string[3] { Path.GetDirectoryName(targetFile), Path.GetFileName(targetFile), Path.GetFullPath(targetFile) };
        }
        //--------------------------------------------------------------------------------------------------------------------------------------
        [MTFMethod(Description = "\n",
                   DisplayName = "0 - Copy And Rename File")]
        [MTFAdditionalParameterInfo(ParameterName = "SourceFilePath", Description = _descSourceFilePath)]
        [MTFAdditionalParameterInfo(ParameterName = "SourceFileName", Description = _descSourceFileName)]
        [MTFAdditionalParameterInfo(ParameterName = "TargetPath", Description = _descTargetFilePath)]
        [MTFAdditionalParameterInfo(ParameterName = "TargetFileName", Description = _descTargetFileName)]
        [MTFAdditionalParameterInfo(ParameterName = "AppendTargetFileName", Description = _descAppendTargetFileName)]
        [MTFAdditionalParameterInfo(ParameterName = "Override", Description = _descOverride)]
        [MTFAdditionalParameterInfo(ParameterName = "MakeVersion", Description = _descMakeVersion)]
        public string[] FileCopyAndRename(string SourceFilePath, string SourceFileName, string TargetPath, string TargetFileName, bool AppendTargetFileName = false, bool Override = false, bool MakeVersion = false)
        {
            //FileCopy(FilePath, FileName, TargetPath, Override);
            //FileRename(TargetPath, FileName, TargetFileName, Override);

            CheckPathHasExtension(SourceFilePath);
            CheckPathHasExtension(TargetPath);

            string sourceFile = System.IO.Path.Combine(SourceFilePath, SourceFileName);
            string targetFile = "";
            if (AppendTargetFileName) { targetFile = System.IO.Path.Combine(TargetPath, Path.GetFileNameWithoutExtension(SourceFileName) + TargetFileName + Path.GetExtension(SourceFileName)); }
            else { targetFile = System.IO.Path.Combine(TargetPath, TargetFileName); }

            //sanity check
            CheckOverrideMakeVersion(Override, MakeVersion);
            CheckFileExists(sourceFile);
            if (File.Exists(targetFile) && !Override) throw new Exception("TargetFileName (" + TargetFileName + ") does already exist in TargetPath (" + TargetPath + ")! If you want to override check Override parameter!");

            //version file
            if (MakeVersion) { targetFile = GetNextFileVersionName(targetFile); }

            //copy file
            File.Copy(sourceFile, targetFile, Override);

            //return filepath ([0]), filename ([1]) and full path ([2])
            return new string[3] { Path.GetDirectoryName(targetFile), Path.GetFileName(targetFile), Path.GetFullPath(targetFile) };
        }
        //--------------------------------------------------------------------------------------------------------------------------------------
        [MTFMethod(Description = "Copies source file to target path.\n(Creates directory if not exists)\n",
                   DisplayName = "0 - Copy File")]
        [MTFAdditionalParameterInfo(ParameterName = "SourceFilePath", Description = _descSourceFilePath)]
        [MTFAdditionalParameterInfo(ParameterName = "SourceTargetFileName", Description = _descSourceTargetFileName)]
        [MTFAdditionalParameterInfo(ParameterName = "TargetPath", Description = _descTargetFilePath)]
        [MTFAdditionalParameterInfo(ParameterName = "Override", Description = _descOverride)]
        [MTFAdditionalParameterInfo(ParameterName = "MakeVersion", Description = _descMakeVersion)]
        [MTFAdditionalParameterInfo(ParameterName = "MakeReadOnly", Description = _descMakeReadOnly)]
        public string[] FileCopy(string SourceFilePath, string SourceTargetFileName, string TargetPath, bool Override = false, bool MakeVersion = false, bool MakeReadOnly = false)
        {
            CheckPathHasExtension(SourceFilePath);
            CheckPathHasExtension(TargetPath);

            string sourceFile = System.IO.Path.Combine(SourceFilePath, SourceTargetFileName);
            string targetFile = System.IO.Path.Combine(TargetPath, SourceTargetFileName);

            //sanity check
            CheckOverrideMakeVersion(Override, MakeVersion);
            CheckFileExists(sourceFile);
            if (File.Exists(targetFile) && !Override && !MakeVersion)
            { throw new Exception("SourceFileName (" + SourceTargetFileName + ") does already exist in TargetPath (" + TargetPath + ")! If you want to override check Override parameter!"); }

            //create directory if not exists
            CheckDirectoryExists(Path.GetDirectoryName(targetFile), true);

            //version file
            if (MakeVersion) { targetFile = GetNextFileVersionName(targetFile); }

            //copy file
            if (sourceFile != targetFile) { File.Copy(sourceFile, targetFile, Override); }

            //readonly
            if (MakeReadOnly) { SetFileReadOnlyAttribute(targetFile, true); }

            //return filepath ([0]), filename ([1]) and full path ([2])
            return new string[3] { Path.GetDirectoryName(targetFile), Path.GetFileName(targetFile), Path.GetFullPath(targetFile) };
        }
        //--------------------------------------------------------------------------------------------------------------------------------------
        [MTFMethod(Description = "\n",
                   DisplayName = "0 - Delete File")]
        [MTFAdditionalParameterInfo(ParameterName = "FilePath", Description = "Path of file that should be deleted")]
        [MTFAdditionalParameterInfo(ParameterName = "FileName", Description = "Name (and extension) of file that should be deleted")]
        public bool FileDelete(string FilePath, string FileName)
        {
            CheckPathHasExtension(FilePath);

            string sourceFile = System.IO.Path.Combine(FilePath, FileName);
            //sanity check
            CheckFileExists(sourceFile);

            //delete file
            File.Delete(sourceFile);

            //return
            if (File.Exists(sourceFile)) { return false; }
            else { return true; }
        }
        //--------------------------------------------------------------------------------------------------------------------------------------
        [MTFMethod(Description = "\n",
                   DisplayName = "0 - Move File")]
        [MTFAdditionalParameterInfo(ParameterName = "SourceFilePath", Description = _descSourceFilePath)]
        [MTFAdditionalParameterInfo(ParameterName = "SourceTargetFileName", Description = _descSourceTargetFileName)]
        [MTFAdditionalParameterInfo(ParameterName = "TargetPath", Description = _descTargetFilePath)]
        [MTFAdditionalParameterInfo(ParameterName = "Override", Description = _descOverride)]
        [MTFAdditionalParameterInfo(ParameterName = "MakeVersion", Description = _descMakeVersion)]
        [MTFAdditionalParameterInfo(ParameterName = "MakeReadOnly", Description = _descMakeReadOnly)]
        public bool FileMove(string SourceFilePath, string SourceTargetFileName, string TargetPath, bool Override = false, bool MakeVersion = false, bool MakeReadOnly = false)
        {
            CheckPathHasExtension(SourceFilePath);
            CheckPathHasExtension(TargetPath);

            string sourceFile = System.IO.Path.Combine(SourceFilePath, SourceTargetFileName);
            string targetFile = System.IO.Path.Combine(TargetPath, SourceTargetFileName);

            //sanity check
            CheckOverrideMakeVersion(Override, MakeVersion);
            CheckFileExists(sourceFile);
            if (File.Exists(targetFile) && !Override && !MakeVersion) throw new Exception("TargetPath file does already exist! If you want to override check Override parameter!");

            //version file
            if (MakeVersion) { targetFile = GetNextFileVersionName(targetFile); }

            //move file
            File.Move(sourceFile, targetFile);

            //readonly
            if (MakeReadOnly) { SetFileReadOnlyAttribute(targetFile, true); }

            //check if move was successful
            if (!File.Exists(sourceFile) && File.Exists(targetFile)) { return true; }
            else { return false; }
        }
        //--------------------------------------------------------------------------------------------------------------------------------------
        [MTFMethod(Description = "\n",
                   DisplayName = "0 - Rename File")]
        [MTFAdditionalParameterInfo(ParameterName = "", Description = "\n")]
        [MTFAdditionalParameterInfo(ParameterName = "SourceFilePath", Description = _descSourceFilePath)]
        [MTFAdditionalParameterInfo(ParameterName = "SourceFileName", Description = _descSourceFileName)]
        [MTFAdditionalParameterInfo(ParameterName = "TargetFileName", Description = _descTargetFileName)]
        [MTFAdditionalParameterInfo(ParameterName = "AppendTargetFileName", Description = _descAppendTargetFileName)]
        [MTFAdditionalParameterInfo(ParameterName = "Override", Description = _descOverride)]
        [MTFAdditionalParameterInfo(ParameterName = "MakeVersion", Description = _descMakeVersion)]
        [MTFAdditionalParameterInfo(ParameterName = "MakeReadOnly", Description = _descMakeReadOnly)]
        [MTFAdditionalParameterInfo(ParameterName = "KeepSourceFile", Description = "Source file will be not deleted")]
        public string[] FileRename(string SourceFilePath, string SourceFileName, string TargetFileName, bool AppendTargetFileName = false, bool Override = false, bool MakeVersion = false, bool MakeReadOnly = false, bool KeepSourceFile = false)
        {
            CheckPathHasExtension(SourceFilePath);

            string sourceFile = System.IO.Path.Combine(SourceFilePath, SourceFileName);
            string targetFile = "";
            if (AppendTargetFileName) { targetFile = System.IO.Path.Combine(SourceFilePath, Path.GetFileNameWithoutExtension(SourceFileName) + TargetFileName + Path.GetExtension(SourceFileName)); }
            else { targetFile = System.IO.Path.Combine(SourceFilePath, TargetFileName); }

            //sanity check
            CheckOverrideMakeVersion(Override, MakeVersion);
            CheckFileExists(sourceFile);
            if (File.Exists(targetFile) && !Override && !MakeVersion) throw new Exception("NewFileName (" + TargetFileName + ") does already exist! If you want to override check Override parameter!");

            //version file
            if (MakeVersion) { targetFile = GetNextFileVersionName(targetFile); }

            if (KeepSourceFile)
            {
                FileCopyAndRename(SourceFilePath, SourceFileName, Path.GetDirectoryName(targetFile), Path.GetFileName(targetFile), false, Override, MakeVersion);
            }
            else
            {
                //move file
                File.Move(sourceFile, targetFile);
            }

            //readonly
            if (MakeReadOnly) { SetFileReadOnlyAttribute(targetFile, true); }

            //return filepath ([0]), filename ([1]) and full path ([2])
            return new string[3] { Path.GetDirectoryName(targetFile), Path.GetFileName(targetFile), Path.GetFullPath(targetFile) };
        }
        #endregion

        // *************************************************************************************************************************************
        // -- DIRECTORY
        // *************************************************************************************************************************************
        #region directory
        [MTFMethod(Description = "\n",
                   DisplayName = "1 - Delete Directory")]
        [MTFAdditionalParameterInfo(ParameterName = "SourcePath", Description = "Directory to delete")]
        [MTFAdditionalParameterInfo(ParameterName = "DeleteContainingFilesFolders", Description = "Delete all containing files and folders")]
        public bool DirectoryDelete(string SourcePath, bool DeleteContainingFilesFolders = false)
        {
            //sanity check
            CheckPathHasExtension(SourcePath);
            CheckDirectoryExists(SourcePath);

            //delete file(s)/folder(s)
            if (DeleteContainingFilesFolders)
            { Directory.Delete(SourcePath, true); }
            else
            { Directory.Delete(SourcePath); }

            //wait 100ms
            System.Threading.Thread.Sleep(100);

            //return
            if (Directory.Exists(SourcePath)) { return false; }
            else { return true; }
        }
        //--------------------------------------------------------------------------------------------------------------------------------------
        [MTFMethod(Description = "\n",
                  DisplayName = "1 - Compress Directory")]
        [MTFAdditionalParameterInfo(ParameterName = "", Description = "\n")]
        [MTFAllowedParameterValue("OutputFormat", "ZIP", "ZIP")]
        [MTFAdditionalParameterInfo(ParameterName = "Override", Description = _descOverride)]
        [MTFAdditionalParameterInfo(ParameterName = "MakeVersion", Description = _descMakeVersion)]
        [MTFAdditionalParameterInfo(ParameterName = "DeleteOrigin", Description = _descDeleteOrigin)]
        public string[] DirectoryCompress(string SourcePath, string OutputFormat, bool Override = false, bool MakeVersion = false, bool SetPassword = false, string Password = "", bool DeleteOrigin = false)
        {
            CheckPathHasExtension(SourcePath);

            var targetFile = Path.GetFullPath(SourcePath) + ".zip";
            //sanity check
            CheckOverrideMakeVersion(Override, MakeVersion);
            CheckDirectoryExists(SourcePath);
            if (!Override && !MakeVersion) { CheckFileExists(SourcePath + ".zip"); }

            if (MakeVersion) { targetFile = GetNextFileVersionName(targetFile); }

            if (_zipLibAvailable)
            {
                Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile();
                zip.AddDirectory(SourcePath);

                //set password
                if (SetPassword) { zip.Password = Password; }

                zip.Save(targetFile);
            }
            else
            {
                MakeZipDirectory(SourcePath, targetFile);
            }

            //delete origin
            if (DeleteOrigin)
            { DirectoryDelete(SourcePath); }

            //return filepath ([0]), filename ([1]) and full path ([2])
            return new string[3] { Path.GetDirectoryName(targetFile), Path.GetFileName(targetFile), Path.GetFullPath(targetFile) };
        }
        //--------------------------------------------------------------------------------------------------------------------------------------
        [MTFMethod(Description = "\n",
                  DisplayName = "1 - Create Directory")]
        [MTFAdditionalParameterInfo(ParameterName = "", Description = "\n")]
        [MTFAdditionalParameterInfo(ParameterName = "TargetPath", Description = "Path where directory should be created")]
        [MTFAdditionalParameterInfo(ParameterName = "MakeReadOnly", Description = _descMakeReadOnly)]
        public string DirectoryCreate(string TargetPath, string TargetName, bool MakeReadOnly = false)//, bool MakeVersion = false)
        {
            CheckPathHasExtension(TargetPath);
            var targetDirectory = Path.Combine(TargetPath, TargetName);
            //sanity check
            CheckDirectoryExists(TargetPath);

            //create directory
            Directory.CreateDirectory(targetDirectory);

            //readonly
            if (MakeReadOnly) { SetDirectoryReadOnlyAttribute(targetDirectory, true); }

            //return 
            return Path.GetFullPath(targetDirectory);
        }
        //--------------------------------------------------------------------------------------------------------------------------------------
        [MTFMethod(Description = "\n",
                   DisplayName = "1 - Move Directory")]
        [MTFAdditionalParameterInfo(ParameterName = "SourcePath", Description = "Origin path of directory")]
        [MTFAdditionalParameterInfo(ParameterName = "TargetPath", Description = "Path to move directory to")]
        [MTFAdditionalParameterInfo(ParameterName = "Override", Description = _descOverride)]
        public string DirectoryMove(string SourcePath, string TargetPath, bool Override = false)
        {
            CheckPathHasExtension(SourcePath);
            CheckPathHasExtension(TargetPath);

            //sanity check
            CheckDirectoryExists(SourcePath);
            if (Directory.Exists(TargetPath) && !Override) throw new Exception("TargetPath does already exist! If you want to override check Override parameter!");

            //move file
            Directory.Move(SourcePath, TargetPath);

            if (!CheckDirectoryExists(SourcePath) && CheckDirectoryExists(TargetPath))
            //return filepath ([0]) and filename ([1])
            { return TargetPath; }
            else
            { return "Move Directory operation failed!"; }
        }
        #endregion

        // *************************************************************************************************************************************
        // -- ENCRYPTION
        // *************************************************************************************************************************************
        #region encryption

        [MTFMethod(Description = "\n",
                   DisplayName = "2 - Decrypt File")]
        [MTFAdditionalParameterInfo(ParameterName = "FilePath", Description = "Path of file")]
        [MTFAdditionalParameterInfo(ParameterName = "FileName", Description = "Name of file")]
        [MTFAdditionalParameterInfo(ParameterName = "DeleteOrigin", Description = "Delete encrypted file (.enc) after decryption")]
        public string[] FileDecrypt(string FilePath, string FileName, bool DeleteOrigin = false)
        {
            CheckPathHasExtension(FilePath);

            string sourceFile = Path.Combine(FilePath, FileName);
            string targetFile = sourceFile.Replace(".enc", "");

            //sanity check
            CheckFileExists(sourceFile);

            //decrypt
            DecryptFile(sourceFile);

            //delete origin
            if (DeleteOrigin) { FileDelete(Path.GetDirectoryName(sourceFile), Path.GetFileName(sourceFile)); }

            //return filepath ([0]), filename ([1]) and full path ([2])
            return new string[3] { Path.GetDirectoryName(targetFile), Path.GetFileName(targetFile), Path.GetFullPath(targetFile) };
        }

        [MTFMethod(Description = "\n",
                   DisplayName = "2 - Encrypt File")]
        [MTFAdditionalParameterInfo(ParameterName = "FilePath", Description = "Path of file")]
        [MTFAdditionalParameterInfo(ParameterName = "FileName", Description = "Name of file")]
        [MTFAdditionalParameterInfo(ParameterName = "DeleteOrigin", Description = "Delete origin/source file after encryption")]
        public string[] FileEncrypt(string FilePath, string FileName, bool DeleteOrigin = false)
        {
            CheckPathHasExtension(FilePath);

            string sourceFile = Path.Combine(FilePath, FileName);
            string targetFile = Path.Combine(sourceFile + ".enc");

            //sanity check
            CheckFileExists(sourceFile);

            //encrypt
            EncryptFile(sourceFile);

            //delete origin
            if (DeleteOrigin) { FileDelete(Path.GetDirectoryName(sourceFile), Path.GetFileName(sourceFile)); }

            //return filepath ([0]), filename ([1]) and full path ([2])
            return new string[3] { Path.GetDirectoryName(targetFile), Path.GetFileName(targetFile), Path.GetFullPath(targetFile) };
        }
        #region algorhythm
        //https://docs.microsoft.com/de-de/dotnet/standard/security/walkthrough-creating-a-cryptographic-application      

        internal void EncryptFile(string inFile)
        {

            // Create instance of Rijndael for
            // symetric encryption of the data.
            RijndaelManaged rjndl = new RijndaelManaged();
            rjndl.KeySize = 256;
            rjndl.BlockSize = 256;
            rjndl.Mode = CipherMode.CBC;
            var test = Encoding.ASCII.GetBytes(rjdlKey);
            rjndl.Key = Encoding.ASCII.GetBytes(rjdlKey);
            ICryptoTransform transform = rjndl.CreateEncryptor();

            // Use RSACryptoServiceProvider to
            // enrypt the Rijndael key.
            // rsa is previously instantiated: 
            //    rsa = new RSACryptoServiceProvider(cspp);
            byte[] keyEncrypted = rsa.Encrypt(rjndl.Key, false);

            // Create byte arrays to contain
            // the length values of the key and IV.
            byte[] LenK = new byte[4];
            byte[] LenIV = new byte[4];

            int lKey = keyEncrypted.Length;
            LenK = BitConverter.GetBytes(lKey);
            int lIV = rjndl.IV.Length;
            LenIV = BitConverter.GetBytes(lIV);

            // Write the following to the FileStream
            // for the encrypted file (outFs):
            // - length of the key
            // - length of the IV
            // - ecrypted key
            // - the IV
            // - the encrypted cipher content

            // Change the file's extension to ".enc"
            string outFile = Path.Combine(Path.GetDirectoryName(inFile), Path.GetFileName(inFile) + ".enc");//inFile.Substring(startFileName, inFile.LastIndexOf(".") - startFileName) + ".enc";

            using (FileStream outFs = new FileStream(outFile, FileMode.Create))
            {

                outFs.Write(LenK, 0, 4);
                outFs.Write(LenIV, 0, 4);
                outFs.Write(keyEncrypted, 0, lKey);
                outFs.Write(rjndl.IV, 0, lIV);

                // Now write the cipher text using
                // a CryptoStream for encrypting.
                using (CryptoStream outStreamEncrypted = new CryptoStream(outFs, transform, CryptoStreamMode.Write))
                {

                    // By encrypting a chunk at
                    // a time, you can save memory
                    // and accommodate large files.
                    int count = 0;
                    int offset = 0;

                    // blockSizeBytes can be any arbitrary size.
                    int blockSizeBytes = rjndl.BlockSize / 8;
                    byte[] data = new byte[blockSizeBytes];
                    int bytesRead = 0;

                    using (FileStream inFs = new FileStream(inFile, FileMode.Open))
                    {
                        do
                        {
                            count = inFs.Read(data, 0, blockSizeBytes);
                            offset += count;
                            outStreamEncrypted.Write(data, 0, count);
                            bytesRead += blockSizeBytes;
                        }
                        while (count > 0);
                        inFs.Close();
                    }
                    outStreamEncrypted.FlushFinalBlock();
                    outStreamEncrypted.Close();
                }
                outFs.Close();
            }

        }

        internal void DecryptFile(string inFile)
        {

            // Create instance of Rijndael for
            // symetric decryption of the data.
            RijndaelManaged rjndl = new RijndaelManaged();
            rjndl.KeySize = 256;
            rjndl.BlockSize = 256;
            rjndl.Mode = CipherMode.CBC;
            rjndl.Key = Encoding.ASCII.GetBytes(rjdlKey);

            // Create byte arrays to get the length of
            // the encrypted key and IV.
            // These values were stored as 4 bytes each
            // at the beginning of the encrypted package.
            byte[] LenK = new byte[4];
            byte[] LenIV = new byte[4];

            // Consruct the file name for the decrypted file.
            string outFile = inFile.Substring(0, inFile.LastIndexOf("."));

            // Use FileStream objects to read the encrypted
            // file (inFs) and save the decrypted file (outFs).
            using (FileStream inFs = new FileStream(inFile, FileMode.Open))
            {

                inFs.Seek(0, SeekOrigin.Begin);
                inFs.Seek(0, SeekOrigin.Begin);
                inFs.Read(LenK, 0, 3);
                inFs.Seek(4, SeekOrigin.Begin);
                inFs.Read(LenIV, 0, 3);

                // Convert the lengths to integer values.
                int lenK = BitConverter.ToInt32(LenK, 0);
                int lenIV = BitConverter.ToInt32(LenIV, 0);

                // Determine the start postition of
                // the ciphter text (startC)
                // and its length(lenC).
                int startC = lenK + lenIV + 8;
                int lenC = (int)inFs.Length - startC;

                // Create the byte arrays for
                // the encrypted Rijndael key,
                // the IV, and the cipher text.
                byte[] KeyEncrypted = new byte[lenK];
                byte[] IV = new byte[lenIV];

                // Extract the key and IV
                // starting from index 8
                // after the length values.
                inFs.Seek(8, SeekOrigin.Begin);
                inFs.Read(KeyEncrypted, 0, lenK);
                inFs.Seek(8 + lenK, SeekOrigin.Begin);
                inFs.Read(IV, 0, lenIV);
                //Directory.CreateDirectory(DecrFolder);
                // Use RSACryptoServiceProvider
                // to decrypt the Rijndael key.
                byte[] KeyDecrypted = rsa.Decrypt(KeyEncrypted, false);

                // Decrypt the key.
                ICryptoTransform transform = rjndl.CreateDecryptor(KeyDecrypted, IV);

                // Decrypt the cipher text from
                // from the FileSteam of the encrypted
                // file (inFs) into the FileStream
                // for the decrypted file (outFs).
                using (FileStream outFs = new FileStream(outFile, FileMode.Create))
                {

                    int count = 0;
                    int offset = 0;

                    // blockSizeBytes can be any arbitrary size.
                    int blockSizeBytes = rjndl.BlockSize / 8;
                    byte[] data = new byte[blockSizeBytes];


                    // By decrypting a chunk a time,
                    // you can save memory and
                    // accommodate large files.

                    // Start at the beginning
                    // of the cipher text.
                    inFs.Seek(startC, SeekOrigin.Begin);
                    using (CryptoStream outStreamDecrypted = new CryptoStream(outFs, transform, CryptoStreamMode.Write))
                    {
                        do
                        {
                            count = inFs.Read(data, 0, blockSizeBytes);
                            offset += count;
                            outStreamDecrypted.Write(data, 0, count);

                        }
                        while (count > 0);

                        outStreamDecrypted.FlushFinalBlock();
                        outStreamDecrypted.Close();
                    }
                    outFs.Close();
                }
                inFs.Close();
            }

        }
        #endregion
        #endregion
        // *************************************************************************************************************************************
        // INTERNAL
        // *************************************************************************************************************************************
        #region internal
        internal void CheckOverrideMakeVersion(bool Override, bool MakeVersion)
        {
            if (Override && MakeVersion) throw new Exception("Override and MakeVersion Parameter can not be activated at same time! Please check your settings!");
        }
        internal bool CheckFileExists(string FilePathAndName)
        {
            if (!File.Exists(FilePathAndName)) throw new Exception("File (" + FilePathAndName + ") does not exist!");
            return true;
        }
        //--------------------------------------------------------------------------------------------------------------------------------------
        internal bool CheckDirectoryExists(string DirectoryPath, bool CreateDirectory = false)
        {
            string _dirPath = Path.GetFullPath(DirectoryPath);
            if (CreateDirectory)
            {
                if (!Directory.Exists(_dirPath)) { Directory.CreateDirectory(_dirPath); }
            }
            if (!Directory.Exists(_dirPath)) throw new Exception("Directory (" + Path.GetDirectoryName(_dirPath) + ") does not exist!");
            return true;
        }
        //--------------------------------------------------------------------------------------------------------------------------------------
        internal void CheckPathHasExtension(string TargetPath)
        {
            if (Path.HasExtension(TargetPath)) throw new Exception("Path (" + TargetPath + ") is not valid!");
        }
        //--------------------------------------------------------------------------------------------------------------------------------------
        internal int GetNextFileVersion(string FilePathName)
        {
            int nextVersion = 0;

            if (File.Exists(FilePathName))
            {
                string[] _files = Directory.GetFiles(Path.GetDirectoryName(FilePathName));
                string _fileName = Path.GetFileNameWithoutExtension(FilePathName);
                string _fileExtension = Path.GetExtension(FilePathName);
                string _filePath = Path.GetDirectoryName(FilePathName);

                //find versions of file
                for (int i = 0; i < _files.Count(); i++)
                {
                    if (_files[i].Contains(_fileName + "_"))
                    {
                        bool isVersion;
                        int curVer;
                        //clean string
                        string numVer = _files[i].Replace(_fileName, string.Empty);
                        numVer = numVer.Replace(_fileExtension, string.Empty);
                        numVer = numVer.Replace(_filePath, string.Empty);
                        numVer = numVer.Replace("_", string.Empty);
                        numVer = numVer.Replace("\\", string.Empty);
                        //check if it is a version of file
                        isVersion = int.TryParse(numVer, out curVer);
                        if (isVersion && nextVersion <= curVer) { nextVersion = curVer + 1; }
                    }
                }
                if (nextVersion == 0) { nextVersion = 1; }
            }
            return nextVersion;
        }
        //--------------------------------------------------------------------------------------------------------------------------------------
        internal string GetNextFileVersionName(string FullPathFileName)
        {
            int nextVersion = GetNextFileVersion(FullPathFileName);
            string nextVersionFileName = Path.GetFullPath(FullPathFileName);

            if (nextVersion != 0)
            {
                string newFileName = Path.GetFileNameWithoutExtension(FullPathFileName) + "_" + nextVersion.ToString("D3") + Path.GetExtension(FullPathFileName);
                nextVersionFileName = Path.Combine(Path.GetDirectoryName(FullPathFileName), newFileName);
            }

            return nextVersionFileName;
        }
        //--------------------------------------------------------------------------------------------------------------------------------------
        internal static void SetFileReadOnlyAttribute(string FullPathFileName, bool readOnly)
        {
            FileInfo filePath = new FileInfo(FullPathFileName);
            FileAttributes attribute;
            //if (readOnly)
            //    attribute = filePath.Attributes | FileAttributes.ReadOnly;
            //else
            //    attribute = (FileAttributes)(filePath.Attributes - FileAttributes.ReadOnly);

            attribute = filePath.Attributes ^ FileAttributes.ReadOnly;

            File.SetAttributes(filePath.FullName, attribute);
        }
        //--------------------------------------------------------------------------------------------------------------------------------------
        internal static void SetDirectoryReadOnlyAttribute(string FullPathName, bool readOnly)
        {
            DirectoryInfo dirPath = new DirectoryInfo(FullPathName);
            dirPath.Attributes |= FileAttributes.ReadOnly;
        }
        #region internal zip
        //use this functions if Ionic.Zip is not available
        internal void MakeZipFile(string SourceFile, string TargetFileName)
        {
            if (Path.GetExtension(TargetFileName) == ".zip")
            {
                using (ZipArchive zip = System.IO.Compression.ZipFile.Open(Path.GetFullPath(TargetFileName), ZipArchiveMode.Update))
                {
                    zip.CreateEntryFromFile(Path.GetFullPath(SourceFile), Path.GetFileNameWithoutExtension(TargetFileName));
                    zip.Dispose();
                }
            }
        }

        internal void MakeZipDirectory(string SourceDirectory, string TargetFile)
        {
            if (Path.GetExtension(TargetFile) == ".zip")
            {
                using (ZipArchive zip = System.IO.Compression.ZipFile.Open(TargetFile, ZipArchiveMode.Update))
                {
                    foreach (var item in Directory.GetFiles(SourceDirectory))
                    {
                        zip.CreateEntryFromFile(item, Path.GetFileName(item), CompressionLevel.Optimal);
                    }

                }
            }
        }
        #endregion
        #endregion
    }
}
