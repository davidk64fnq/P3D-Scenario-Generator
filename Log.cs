using System.Diagnostics; // Required for System.Diagnostics.Debug.WriteLine

namespace P3D_Scenario_Generator
{
    public static class Log
    {
        private static readonly string LogDirectory;
        private static readonly string LogFilePath;     // Path for error logs
        private static readonly string InfoLogFilePath; // Path for info logs
        private static readonly string WarningLogFilePath; // NEW: Path for warning logs

        /// <summary>
        /// Initializes the Log class, determining the log file paths within the application's data directory.
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
                Debug.WriteLine($"CRITICAL ERROR: Failed to create log directory {LogDirectory}. Logging will be impaired. Exception: {ex.Message}");
                // You might want to fall back to a temporary path or just swallow for release builds
                LogDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                             AppDomain.CurrentDomain.FriendlyName + "_Logs_Fallback"); // Fallback to a temp path for logging
                try
                {
                    if (!Directory.Exists(LogDirectory))
                    {
                        Directory.CreateDirectory(LogDirectory);
                    }
                }
                catch (Exception fallbackEx)
                {
                    Debug.WriteLine($"CRITICAL ERROR: Failed to create fallback log directory {LogDirectory}. Logging seriously impaired. Exception: {fallbackEx.Message}");
                }
            }

            LogFilePath = Path.Combine(LogDirectory, "ErrorLog.txt");
            InfoLogFilePath = Path.Combine(LogDirectory, "InfoLog.txt");
            WarningLogFilePath = Path.Combine(LogDirectory, "WarningLog.txt"); // NEW: Initialize new warning log path
        }

        /// <summary>
        /// Logs an error message with optional exception details to the error log file.
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

                File.AppendAllText(LogFilePath, logEntry);
                Debug.WriteLine(logEntry);
            }
            catch (Exception logEx)
            {
                Debug.WriteLine($"Error writing to ErrorLog.txt: {logEx.Message}");
            }
        }

        /// <summary>
        /// Logs a warning message to the warning log file.
        /// </summary>
        /// <param name="message">The warning message.</param>
        public static void Warning(string message) // NEW: Warning method
        {
            try
            {
                string logEntry = $"{DateTime.Now}: WARNING - {message}\n";

                File.AppendAllText(WarningLogFilePath, logEntry); // Append to warning log file
                Debug.WriteLine(logEntry);
            }
            catch (Exception logEx)
            {
                Debug.WriteLine($"Error writing to WarningLog.txt: {logEx.Message}");
            }
        }

        /// <summary>
        /// Logs an informational message to the info log file.
        /// </summary>
        /// <param name="message">The informational message.</param>
        public static void Info(string message)
        {
            try
            {
                string logEntry = $"{DateTime.Now}: INFO - {message}\n";

                File.AppendAllText(InfoLogFilePath, logEntry);
                Debug.WriteLine(logEntry);
            }
            catch (Exception logEx)
            {
                Debug.WriteLine($"Error writing to InfoLog.txt: {logEx.Message}");
            }
        }
    }
}