

namespace P3D_Scenario_Generator
{
    internal class Cache
    {
        static internal void GetOrCopyWebDoc(string key, string url, string saveFile)
        {
            string cachePath = "";
            if (DoesKeyExist(key, ref cachePath))
            {
                File.Copy(cachePath, saveFile, true);
            }
            else
            {
                HttpRoutines.GetWebDoc(url, saveFile);
                if (File.Exists(saveFile)) {
                    Properties.Settings.Default.CacheMonthlyTotal += 1;
                    File.Copy(saveFile, cachePath, true);
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

        static internal void CheckCacheMonth()
        {
            string curCacheMonth = DateTime.Now.Month.ToString() + "-" + DateTime.Now.Year.ToString();
            if (Properties.Settings.Default.CacheMonth != curCacheMonth)
            {
                Properties.Settings.Default.CacheMonth = curCacheMonth;
                Properties.Settings.Default.CacheMonthlyTotal = 0;
            }
        }
    }
}
