using P3D_Scenario_Generator.Models;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace P3D_Scenario_Generator.Services
{
    /// <summary>
    /// Provides an asynchronous logging service that writes log entries to separate files based on severity.
    /// Uses asynchronous file I/O to avoid blocking the calling thread.
    /// Direct file I/O is retained here to prevent circular dependencies with <see cref="FileOps"/>.
    /// </summary>
    internal class Logger
    {
        private readonly string _logDirectory;
        private readonly string _errorLogFilePath;
        private readonly string _infoLogFilePath;
        private readonly string _warningLogFilePath;
        private readonly ScenarioFormData? _formData;

        /// <summary>
        /// Gets or sets a value indicating whether to include the date in the log entry.
        /// </summary>
        internal bool IncludeDate { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to include the time in the log entry.
        /// </summary>
        internal bool IncludeTime { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to include the log level (e.g., "INFO - ") in the log entry.
        /// </summary>
        internal bool IncludeLevel { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class with default settings.
        /// Ensures the log directory exists and clears previous session log files on startup.
        /// </summary>
        internal Logger() : this(false, false, false, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class with custom settings.
        /// </summary>
        /// <param name="includeDate">A boolean to control whether to include the date.</param>
        /// <param name="includeTime">A boolean to control whether to include the time.</param>
        /// <param name="includeLevel">A boolean to control whether to include the log level.</param>
        /// <param name="formData">The optional <see cref="ScenarioFormData"/> instance containing base paths for path sanitization.</param>
        internal Logger(bool includeDate, bool includeTime, bool includeLevel, ScenarioFormData? formData = null)
        {
            IncludeDate = includeDate;
            IncludeTime = includeTime;
            IncludeLevel = includeLevel;
            _formData = formData;

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
                Debug.WriteLine($"CRITICAL ERROR: Failed to create log directory at '{_logDirectory}'. Logging will use a fallback directory. Exception: {ex.Message}");

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

            ClearLogFile(_errorLogFilePath);
            ClearLogFile(_infoLogFilePath);
            ClearLogFile(_warningLogFilePath);
        }

        /// <summary>
        /// Asynchronously writes an error log entry to the error log file.
        /// </summary>
        /// <param name="message">The diagnostic error message to log.</param>
        /// <param name="ex">Optional exception details to include.</param>
        /// <param name="callerName">The caller method name automatically populated by the compiler.</param>
        /// <param name="callerFilePath">The caller source file path automatically populated by the compiler.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        internal async Task ErrorAsync(string message, Exception? ex = null, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "")
        {
            string prefix = GetLogPrefix(callerName, callerFilePath);
            string timestamp = GetTimestamp();
            string logEntry = GetLogEntry("ERROR", prefix, timestamp, message);

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

        /// <summary>
        /// Asynchronously writes a warning log entry to the warning log file.
        /// </summary>
        /// <param name="message">The diagnostic warning message to log.</param>
        /// <param name="callerName">The caller method name automatically populated by the compiler.</param>
        /// <param name="callerFilePath">The caller source file path automatically populated by the compiler.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        internal async Task WarningAsync(string message, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "")
        {
            string prefix = GetLogPrefix(callerName, callerFilePath);
            string timestamp = GetTimestamp();
            string logEntry = GetLogEntry("WARNING", prefix, timestamp, message);
            await WriteLogEntryAsync(_warningLogFilePath, logEntry);
        }

        /// <summary>
        /// Asynchronously writes an informational log entry to the info log file.
        /// </summary>
        /// <param name="message">The diagnostic informational message to log.</param>
        /// <param name="callerName">The caller method name automatically populated by the compiler.</param>
        /// <param name="callerFilePath">The caller source file path automatically populated by the compiler.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        internal async Task InfoAsync(string message, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "")
        {
            string prefix = GetLogPrefix(callerName, callerFilePath);
            string timestamp = GetTimestamp();
            string logEntry = GetLogEntry("INFO", prefix, timestamp, message);
            await WriteLogEntryAsync(_infoLogFilePath, logEntry);
        }

        private static string GetLogPrefix(string callerName, string callerFilePath)
        {
            string className = Path.GetFileNameWithoutExtension(callerFilePath);

            if (callerName.Contains('<') && callerName.Contains('>'))
            {
                int startIndex = callerName.IndexOf('<') + 1;
                int endIndex = callerName.IndexOf('>');
                callerName = callerName[startIndex..endIndex];
            }

            return $"{className}.{callerName}: ";
        }

        private string GetLogEntry(string level, string prefix, string timestamp, string message)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(timestamp))
            {
                sb.Append(timestamp);
            }
            if (IncludeLevel)
            {
                if (sb.Length > 0)
                {
                    sb.Append(' ');
                }
                sb.Append($"- {level} - ");
            }
            else if (sb.Length > 0)
            {
                sb.Append(' ');
            }
            sb.Append($"{prefix}{ProcessPath(message)}");

            return sb.ToString().TrimStart(' ', '-');
        }

        private string ProcessPath(string message)
        {
            if (_formData == null ||
                _formData.P3DProgramInstall == null ||
                _formData.P3DProgramData == null ||
                _formData.ScenarioFolderBase == null ||
                _formData.ScenarioFolder == null ||
                _formData.ScenarioImageFolder == null ||
                _formData.TempScenarioDirectory == null)
            {
                return message;
            }

            var pathMap = new List<(string Path, string Name)>
            {
                (_formData.P3DProgramInstall, "P3DProgramInstall"),
                (_formData.P3DProgramData, "P3DProgramData"),
                (_formData.ScenarioFolderBase, "ScenarioFolderBase"),
                (_formData.ScenarioFolder, "ScenarioFolder"),
                (_formData.ScenarioImageFolder, "ScenarioImageFolder"),
                (_formData.TempScenarioDirectory, "TempScenarioDirectory"),
                (FileOps.GetApplicationDataDirectory(), "P3DSGProgramData")
            };

            pathMap.Sort((a, b) => b.Path.Length.CompareTo(a.Path.Length));

            foreach (var entry in pathMap)
            {
                int index = message.IndexOf(entry.Path, StringComparison.OrdinalIgnoreCase);

                if (index >= 0)
                {
                    return string.Concat(message.AsSpan(0, index), entry.Name, message.AsSpan(index + entry.Path.Length));
                }
            }

            return message;
        }

        private string GetTimestamp()
        {
            var parts = new List<string>();
            if (IncludeDate)
            {
                parts.Add(DateTime.Now.ToShortDateString());
            }
            if (IncludeTime)
            {
                parts.Add(DateTime.Now.ToLongTimeString());
            }
            return string.Join(" ", parts);
        }

        private static void ClearLogFile(string filePath)
        {
            try
            {
                if (FileOps.FileExists(filePath))
                {
                    File.WriteAllText(filePath, string.Empty);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error clearing log file '{filePath}': {ex.Message}");
            }
        }

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