using System.Runtime.CompilerServices;

namespace P3D_Scenario_Generator.Interfaces
{
    /// <summary>
    /// Defines the contract for an asynchronous logging service.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Asynchronously logs an error message with optional exception details.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="ex">The exception that occurred (optional).</param>
        /// <param name="callerName">The name of the calling method. This is automatically provided by the compiler.</param>
        /// <param name="callerFilePath">The full path to the source file that contains the caller. This is automatically provided by the compiler.</param>
        Task ErrorAsync(string message, Exception ex = null, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "");

        /// <summary>
        /// Asynchronously logs a warning message.
        /// </summary>
        /// <param name="message">The warning message.</param>
        /// <param name="callerName">The name of the calling method. This is automatically provided by the compiler.</param>
        /// <param name="callerFilePath">The full path to the source file that contains the caller. This is automatically provided by the compiler.</param>
        Task WarningAsync(string message, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "");

        /// <summary>
        /// Asynchronously logs an informational message.
        /// </summary>
        /// <param name="message">The informational message.</param>
        /// <param name="callerName">The name of the calling method. This is automatically provided by the compiler.</param>
        /// <param name="callerFilePath">The full path to the source file that contains the caller. This is automatically provided by the compiler.</param>
        Task InfoAsync(string message, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "");
    }
}
