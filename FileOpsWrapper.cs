using P3D_Scenario_Generator.Interfaces;
using System.Reflection;

namespace P3D_Scenario_Generator
{
    internal class FileOpsWrapper : IFileOps
    {
        #region Helper Methods

        /// <summary>
        /// Gets the full path to the application's local data directory.
        /// The directory is created if it does not already exist.
        /// </summary>
        /// <returns>The full path to the application data directory.</returns>
        public string GetApplicationDataDirectory()
        {
            string appName = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().GetName().Name);
            string dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appName);

            if (!Directory.Exists(dataDirectory))
            {
                Directory.CreateDirectory(dataDirectory);
            }

            return dataDirectory;
        }

        #endregion
    }
}
