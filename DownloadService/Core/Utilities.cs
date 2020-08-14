using System;
using System.IO;

namespace Devhus.DownloadService.Core
{
    public class Utilities
    {

        internal static Version GetCurrentVersion => System.Reflection.Assembly.GetExecutingAssembly()?.GetName().Version;

        /// <summary>
        /// Calculate the given file md5 hash and return it as string
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        internal static string GetFileMd5Hash(string filePath)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                using (var stream = new BufferedStream(File.OpenRead(filePath), 1200000))
                {
                    string ret = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
                    stream.Close();
                    return ret;
                }
            }
        }


        /// <summary>
        /// Convert bytes to human readable string
        /// </summary>
        /// <param name="byteCount"></param>
        /// <returns></returns>
        internal static string BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];

            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }
    }
}
