
namespace P3D_Scenario_Generator
{
    public static class Log
    {
        private static readonly string LogDirectory;
        private static readonly string LogFilePath;

        /// <summary>
        /// Initializes the Log class, determining the log file path within the application's data directory.
        /// It also ensures that the necessary log directory exists, creating it if it does not.
        /// </summary>
        static Log() // Static constructor to initialize readonly fields
        {
            LogDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                        AppDomain.CurrentDomain.FriendlyName);

            // Ensure the directory exists
            try
            {
                if (!Directory.Exists(LogDirectory))
                {
                    Directory.CreateDirectory(LogDirectory);
                }
            }
            catch (Exception ex)
            {
                // This is a critical failure: cannot even set up logging directory
                System.Diagnostics.Debug.WriteLine($"CRITICAL ERROR: Failed to create log directory {LogDirectory}. Logging will be impaired. Exception: {ex.Message}");
                // You might want to fall back to a temporary path or just swallow for release builds
                LogDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData); // Fallback
            }

            LogFilePath = Path.Combine(LogDirectory, "ErrorLog.txt");
        }

        /// <summary>
        /// Logs an error message with optional exception details to a file.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="ex">The exception that occurred (optional).</param>
        public static void Error(string message, Exception ex = null)
        {
            try
            {
                string logEntry = $"{DateTime.Now}: ERROR - {message}";
                if (ex != null)
                {
                    logEntry += $"\nException Type: {ex.GetType().Name}";
                    logEntry += $"\nMessage: {ex.Message}";
                    logEntry += $"\nStack Trace:\n{ex.StackTrace}";
                    if (ex.InnerException != null)
                    {
                        logEntry += $"\nInner Exception Type: {ex.InnerException.GetType().Name}";
                        logEntry += $"\nInner Message: {ex.InnerException.Message}";
                        logEntry += $"\nInner Stack Trace:\n{ex.InnerException.StackTrace}";
                    }
                }
                logEntry += "\n--------------------------------------------------\n";

                // Append to log file. Consider using a rolling log file if this could grow very large.
                File.AppendAllText(LogFilePath, logEntry);

                // For development/debugging, also output to debug console
                System.Diagnostics.Debug.WriteLine(logEntry);
            }
            catch (Exception logEx)
            {
                // Fallback for logging errors, prevent infinite loops
                System.Diagnostics.Debug.WriteLine($"Error writing to log file: {logEx.Message}");
            }
        }
    }
}
