using P3D_Scenario_Generator.Interfaces;
using System.Diagnostics;

namespace P3D_Scenario_Generator.Services
{
    /// <summary>
    /// Provides an asynchronous logging service that writes log entries to separate files based on severity.
    /// This implementation uses asynchronous file I/O to avoid blocking the calling thread.
    /// </summary>
    public class Logger : ILogger
    {
        private readonly string _logDirectory;
        private readonly string _errorLogFilePath;
        private readonly string _infoLogFilePath;
        private readonly string _warningLogFilePath;

        /// <summary>
        /// Initializes a new instance of the Logger class.
        /// It ensures the necessary log directory exists and clears previous log files on startup.
        /// </summary>
        public Logger()
        {
            // Primary log directory path: C:\Users\<user>\AppData\Roaming\<AppName>
            string appName = Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName);
            _logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appName);

            try
            {
                if (!Directory.Exists(_logDirectory))
                {
                    Directory.CreateDirectory(_logDirectory);
                }
            }
            catch (Exception ex)
            {
                // Critical failure: cannot set up primary logging directory. Fallback to a local app data path.
                Debug.WriteLine($"CRITICAL ERROR: Failed to create log directory at '{_logDirectory}'. Logging will use a fallback directory. Exception: {ex.Message}");

                // Fallback path: C:\Users\<user>\AppData\Local\<AppName>\Logs_Fallback
                _logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName, "Logs_Fallback");

                try
                {
                    if (!Directory.Exists(_logDirectory))
                    {
                        Directory.CreateDirectory(_logDirectory);
                    }
                }
                catch (Exception fallbackEx)
                {
                    Debug.WriteLine($"CRITICAL ERROR: Failed to create fallback log directory at '{_logDirectory}'. Logging is seriously impaired. Exception: {fallbackEx.Message}");
                }
            }

            _errorLogFilePath = Path.Combine(_logDirectory, "ErrorLog.txt");
            _infoLogFilePath = Path.Combine(_logDirectory, "InfoLog.txt");
            _warningLogFilePath = Path.Combine(_logDirectory, "WarningLog.txt");

            // Clear log files on application startup to ensure a fresh log for each session.
            ClearLogFile(_errorLogFilePath);
            ClearLogFile(_infoLogFilePath);
            ClearLogFile(_warningLogFilePath);
        }

        /// <inheritdoc/>
        public async Task ErrorAsync(string message, Exception ex = null)
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
            await WriteLogEntryAsync(_errorLogFilePath, logEntry);
        }

        /// <inheritdoc/>
        public async Task WarningAsync(string message)
        {
            string prefix = GetLogPrefix();
            string logEntry = $"{DateTime.Now}: WARNING - {prefix}{message}";
            await WriteLogEntryAsync(_warningLogFilePath, logEntry);
        }

        /// <inheritdoc/>
        public async Task InfoAsync(string message)
        {
            string prefix = GetLogPrefix();
            string logEntry = $"{DateTime.Now}: INFO - {prefix}{message}";
            await WriteLogEntryAsync(_infoLogFilePath, logEntry);
        }

        /// <summary>
        /// A helper method to get the prefix for logging, including the calling method's name.
        /// </summary>
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
        /// Asynchronously writes a formatted log entry to the specified file and to the debug output.
        /// </summary>
        /// <param name="filePath">The full path to the log file.</param>
        /// <param name="logEntry">The log message to write.</param>
        private static async Task WriteLogEntryAsync(string filePath, string logEntry)
        {
            try
            {
                await File.AppendAllTextAsync(filePath, logEntry + "\n--------------------------------------------------\n");
                Debug.WriteLine(logEntry);
            }
            catch (Exception logEx)
            {
                Debug.WriteLine($"Error writing to log file '{filePath}': {logEx.Message}");
            }
        }
    }
}
