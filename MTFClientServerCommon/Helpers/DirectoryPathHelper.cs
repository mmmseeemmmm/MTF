using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon.Helpers
{
    public static class DirectoryPathHelper
    {
        public static string GetFullPathFromRelative(string parentPath, string relativePath)
        {
            if (!string.IsNullOrEmpty(parentPath))
            {
                var absoluteDir = Path.GetDirectoryName(parentPath);
                if (!string.IsNullOrEmpty(absoluteDir))
                {
                    var folders = absoluteDir.Split(Path.DirectorySeparatorChar);
                    var relativeDir = Path.GetDirectoryName(relativePath);
                    if (!string.IsNullOrEmpty(relativePath))
                    {
                        var relativeFolders = relativeDir.Split(Path.DirectorySeparatorChar);
                        var countUp = relativeFolders.Count(x => x == "..");
                        var path = new StringBuilder();
                        for (int i = 0; i < folders.Length - countUp; i++)
                        {
                            path.Append(folders[i]).Append(Path.DirectorySeparatorChar);
                        }
                        for (int i = countUp; i < relativeFolders.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(relativeFolders[i]))
                            {
                                path.Append(relativeFolders[i]).Append(Path.DirectorySeparatorChar); 
                            }
                        }
                        path.Append(Path.GetFileName(relativePath));
                        return path.ToString();
                    }
                } 
            }
            return relativePath;
        }

        public static string GetRelativePath(string directoryPath, string subSequencePath)
        {
            var folders = directoryPath.Split(Path.DirectorySeparatorChar);
            var subSeqDirPath = Path.GetDirectoryName(subSequencePath).Split(Path.DirectorySeparatorChar);
            int count = 0;
            bool sameValue = true;
            do
            {
                if (folders[count] != subSeqDirPath[count])
                {
                    sameValue = false;
                }
                else
                {
                    count++;
                }
            } while (sameValue && subSeqDirPath.Length > count && folders.Length > count);
            var relativePath = new StringBuilder();
            for (int i = 0; i < folders.Length - count; i++)
            {
                if (!string.IsNullOrEmpty(folders[i]))
                {
                    relativePath.Append("..");
                    relativePath.Append(Path.DirectorySeparatorChar); 
                }
            }
            for (int i = count; i < subSeqDirPath.Length; i++)
            {
                if (!string.IsNullOrEmpty(subSeqDirPath[i]))
                {
                    relativePath.Append(subSeqDirPath[i]);
                    relativePath.Append(Path.DirectorySeparatorChar);
                }
            }
            relativePath.Append(Path.GetFileName(subSequencePath));
            return relativePath.ToString();
        }
    }
}
