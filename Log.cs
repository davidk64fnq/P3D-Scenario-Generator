using System.Diagnostics;
using System.IO;
using System;

namespace P3D_Scenario_Generator
{
    /// <summary>
    /// Provides a static, application-wide logging mechanism to write informational, warning, and error messages to separate files.
    /// </summary>
    public static class Log
    {
        private static readonly string LogDirectory;
        private static readonly string LogFilePath;
        private static readonly string InfoLogFilePath;
        private static readonly string WarningLogFilePath;

        // A helper method to get the prefix for logging, including the calling method's name.
        private static string GetLogPrefix()
        {
            // Skip 2 frames: one for this method, one for the public Error/Warning/Info method.
            var stackFrame = new StackTrace().GetFrame(2);
            var method = stackFrame?.GetMethod();

            if (method != null)
            {
                string className = method.DeclaringType?.Name ?? "UnknownClass";
                string methodName = method.Name ?? "UnknownMethod";
                return $"{className}.{methodName}: ";
            }

            return "UnknownLocation: ";
        }

        /// <summary>
        /// Initializes the Log class, determining the log file paths within the application's data directory.
        /// It ensures the necessary log directory exists, creates it if it does not, and clears previous log files on startup.
        /// If the primary log directory cannot be created, it attempts to fall back to a local application data directory.
        /// </summary>
        static Log()
        {
            // Primary log directory path: C:\Users\<user>\AppData\Roaming\<AppName>
            string appName = Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName);
            LogDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appName);

            try
            {
                if (!Directory.Exists(LogDirectory))
                {
                    Directory.CreateDirectory(LogDirectory);
                }
            }
            catch (Exception ex)
            {
                // Critical failure: cannot set up primary logging directory. Fallback to a local app data path.
                Debug.WriteLine($"CRITICAL ERROR: Failed to create log directory at '{LogDirectory}'. Logging will use a fallback directory. Exception: {ex.Message}");

                // Fallback path: C:\Users\<user>\AppData\Local\<AppName>\Logs_Fallback
                LogDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName, "Logs_Fallback");

                try
                {
                    if (!Directory.Exists(LogDirectory))
                    {
                        Directory.CreateDirectory(LogDirectory);
                    }
                }
                catch (Exception fallbackEx)
                {
                    Debug.WriteLine($"CRITICAL ERROR: Failed to create fallback log directory at '{LogDirectory}'. Logging is seriously impaired. Exception: {fallbackEx.Message}");
                }
            }

            LogFilePath = Path.Combine(LogDirectory, "ErrorLog.txt");
            InfoLogFilePath = Path.Combine(LogDirectory, "InfoLog.txt");
            WarningLogFilePath = Path.Combine(LogDirectory, "WarningLog.txt");

            // Clear log files on application startup to ensure a fresh log for each session.
            ClearLogFile(LogFilePath);
            ClearLogFile(InfoLogFilePath);
            ClearLogFile(WarningLogFilePath);
        }

        /// <summary>
        /// Logs an error message with optional exception details to the error log file.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="ex">The exception that occurred (optional).</param>
        public static void Error(string message, Exception ex = null)
        {
            string prefix = GetLogPrefix();
            string logEntry = $"{DateTime.Now}: ERROR - {prefix}{message}";
            if (ex != null)
            {
                logEntry += $"\nException Type: {ex.GetType().Name}\nMessage: {ex.Message}\nStack Trace:\n{ex.StackTrace}";
                if (ex.InnerException != null)
                {
                    logEntry += $"\nInner Exception Type: {ex.InnerException.GetType().Name}\nInner Message: {ex.InnerException.Message}\nInner Stack Trace:\n{ex.InnerException.StackTrace}";
                }
            }
            WriteLogEntry(LogFilePath, logEntry);
        }

        /// <summary>
        /// Logs a warning message to the warning log file.
        /// </summary>
        /// <param name="message">The warning message.</param>
        public static void Warning(string message)
        {
            string prefix = GetLogPrefix();
            string logEntry = $"{DateTime.Now}: WARNING - {prefix}{message}";
            WriteLogEntry(WarningLogFilePath, logEntry);
        }

        /// <summary>
        /// Logs an informational message to the info log file.
        /// </summary>
        /// <param name="message">The informational message.</param>
        public static void Info(string message)
        {
            string prefix = GetLogPrefix();
            string logEntry = $"{DateTime.Now}: INFO - {prefix}{message}";
            WriteLogEntry(InfoLogFilePath, logEntry);
        }

        /// <summary>
        /// Clears the contents of a specified log file.
        /// </summary>
        /// <param name="filePath">The full path to the log file to clear.</param>
        private static void ClearLogFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.WriteAllText(filePath, string.Empty);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error clearing log file '{filePath}': {ex.Message}");
            }
        }

        /// <summary>
        /// Writes a formatted log entry to the specified file and to the debug output.
        /// </summary>
        /// <param name="filePath">The full path to the log file.</param>
        /// <param name="logEntry">The log message to write.</param>
        private static void WriteLogEntry(string filePath, string logEntry)
        {
            try
            {
                File.AppendAllText(filePath, logEntry + "\n--------------------------------------------------\n");
                Debug.WriteLine(logEntry);
            }
            catch (Exception logEx)
            {
                Debug.WriteLine($"Error writing to log file '{filePath}': {logEx.Message}");
            }
        }
    }
}