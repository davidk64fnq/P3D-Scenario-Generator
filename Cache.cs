

using ImageMagick;
using Microsoft.Office.Interop.Excel;

namespace P3D_Scenario_Generator
{
    internal class Cache
    {
        static internal void GetOrCopyOSMtile(string key, string url, string saveFile)
        {
            string cachePath = "";
            if (DoesKeyExist(key, ref cachePath))
            {
                File.Copy(cachePath, saveFile, true);
            }
            else
            {
                HttpRoutines.GetWebImage(url, saveFile);
                if (File.Exists(saveFile)) {
                    Properties.Settings.Default.TextBoxSettingsCacheDailyTotal += 1;
                    Properties.Settings.Default.Save();
                    File.Copy(saveFile, cachePath, true);
                }
                else
                {
                    // generate blank tile file so application doesn't fall over
                    using var image = new MagickImage(new MagickColor("#ffffff"), 256, 256);
                    image.Write(saveFile);
                }
            }
        }

        static internal bool DoesKeyExist(string key, ref string cachePath)
        {
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            directory = Path.Combine(directory, AppDomain.CurrentDomain.FriendlyName);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            directory = Path.Combine(directory, key.Split('-')[0]); // Check zoom directory exists
            cachePath = Path.Combine(directory, key);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            else if (File.Exists(cachePath))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// User setting SettingsCacheDailyTotal is reset to zero each day. Allows user to track number of OSM tiles downloaded
        /// for the day for the current server / API key pair. Some servers have a daily limit. Also updates user setting SettingsCacheUsage.
        /// </summary>
        static internal void CheckCache()
        {
            string curCacheDate = DateTime.Now.Date.ToString();
            if (Properties.Settings.Default.TextBoxSettingsCacheDate != curCacheDate)
            {
                Properties.Settings.Default.TextBoxSettingsCacheDate = curCacheDate;
                Properties.Settings.Default.TextBoxSettingsCacheDailyTotal = 0;
                Properties.Settings.Default.Save();
            }
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            directory = Path.Combine(directory, AppDomain.CurrentDomain.FriendlyName);
            long cacheUsage = Directory.EnumerateFiles($"{directory}", "*", SearchOption.AllDirectories).Sum(fileInfo => new FileInfo(fileInfo).Length);
            Properties.Settings.Default.TextBoxSettingsCacheUsage = FormatBytes(cacheUsage);
            Properties.Settings.Default.Save();
        }

        private static string FormatBytes(long bytes)
        {
            string[] Suffix = ["B", "KB", "MB", "GB", "TB"];
            int i;
            double dblSByte = bytes;
            for (i = 0; i < Suffix.Length && bytes >= 1024; i++, bytes /= 1024)
            {
                dblSByte = bytes / 1024.0;
            }

            return String.Format("{0:0.##} {1}", dblSByte, Suffix[i]);
        }
    }
}
