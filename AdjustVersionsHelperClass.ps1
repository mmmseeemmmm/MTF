$Assem = (
"System"
)
$Source = @"

using System;
using System.Globalization;
using System.Linq;
using System.IO;

namespace AutomotiveLighting.Tools 
{ 
    public static class VersionHelper
    {
        static readonly string VersionFileNameSuffix = @"Properties\AssemblyInfo.cs";
        static readonly string[] ProjectsToAdjust = { 
          "MTFCommon", 
          "MTFClientServerCommon",
          "MTFClient\\MTFApp",
          "MTFServer\\MTFServer",
          "MTFServer\\MTFCore",
          "MTFAccessControl\\USB\\UsbAccessRequest",
        };

        static readonly string AssemblyVersionTextPrefix = "[assembly: AssemblyVersion(\"";
        static readonly string AssemblyFileVersionTextPrefix = "[assembly: AssemblyFileVersion(\"";
        const int MaxVersion = 65534;
                  
        public static void Adjust()
        {
          Adjust(Environment.CurrentDirectory);
        }
        
        public static void Adjust(string basePath)
        {
            var currentVersion = GetCurrentVersion(basePath);
            var newVersion = currentVersion.Substring(0, currentVersion.LastIndexOf(".") + 1) + LastVersionNumber();
            SetNewVersion(basePath, newVersion);
        }
        
        public static string GetCurrentVersion()
        {
            return GetCurrentVersion(Environment.CurrentDirectory);
        }

        public static string GetCurrentVersion(string basePath)
        {
            string[] versionFile = File.ReadAllLines(Path.Combine(basePath, ProjectsToAdjust[0], VersionFileNameSuffix));
            string assemblyVersionLine = versionFile.First(l => l.StartsWith(AssemblyVersionTextPrefix));

            return assemblyVersionLine.Substring(AssemblyVersionTextPrefix.Length, assemblyVersionLine.LastIndexOf("\"") - AssemblyVersionTextPrefix.Length);
        }
        
        public static void SetNewVersion(string basePath, string newVersion)
        {
            ValidateVersion(newVersion);
            foreach (var projectToAdjust in ProjectsToAdjust)
            {
                Console.WriteLine(projectToAdjust);
                string[] versionFile = File.ReadAllLines(Path.Combine(basePath, projectToAdjust, VersionFileNameSuffix));

                string assemblyVersionLine = versionFile.First(l => l.StartsWith(AssemblyVersionTextPrefix));
                string newAssemblyVersionLine = assemblyVersionLine.Substring(0, AssemblyVersionTextPrefix.Length) + newVersion +
                    assemblyVersionLine.Substring(assemblyVersionLine.LastIndexOf("\""), assemblyVersionLine.Length - assemblyVersionLine.LastIndexOf("\""));
                versionFile[Array.IndexOf(versionFile, assemblyVersionLine)] = newAssemblyVersionLine;

                string assemblyFileVersionLine = versionFile.First(l => l.StartsWith(AssemblyFileVersionTextPrefix));
                string newAssemblyFileVersionLine = assemblyFileVersionLine.Substring(0, AssemblyFileVersionTextPrefix.Length) + newVersion +
                    assemblyFileVersionLine.Substring(assemblyFileVersionLine.LastIndexOf("\""), assemblyFileVersionLine.Length - assemblyFileVersionLine.LastIndexOf("\""));
                versionFile[Array.IndexOf(versionFile, assemblyFileVersionLine)] = newAssemblyFileVersionLine;

                File.WriteAllLines(Path.Combine(basePath, projectToAdjust, VersionFileNameSuffix), versionFile);
            }
        }
        
        private static void ValidateVersion(string version)
        {
            Version v = new Version(version);
            if (v.Revision < 0)
            {
                throw new Exception("Project version must have revision number!");
            }
            if (v.Revision > MaxVersion)
            {
                throw new Exception (string.Format("Revision must be smaller than {0}!", MaxVersion));
            }
            if (v.Build > MaxVersion)
            {
                throw new Exception(string.Format("Build must be smaller than {0}!", MaxVersion));
            }
            if (v.Minor > MaxVersion)
            {
                throw new Exception(string.Format("Minor must be smaller than {0}!", MaxVersion));
            }
            if (v.Major > MaxVersion)
            {
                throw new Exception(string.Format("Major must be smaller than {0}!", MaxVersion));
            }
        }
        
        private static string LastVersionNumber()
        {
            return ((DateTime.UtcNow - new DateTime(DateTime.UtcNow.Year, 1, 1)).TotalMinutes / 10).ToString("F0", CultureInfo.InvariantCulture);
        }
    }
} 
"@

Add-Type -ReferencedAssemblies $Assem -TypeDefinition $Source -Language CSharp