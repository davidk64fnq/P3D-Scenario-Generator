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
        Task ErrorAsync(string message, Exception ex = null);

        /// <summary>
        /// Asynchronously logs a warning message.
        /// </summary>
        /// <param name="message">The warning message.</param>
        Task WarningAsync(string message);

        /// <summary>
        /// Asynchronously logs an informational message.
        /// </summary>
        /// <param name="message">The informational message.</param>
        Task InfoAsync(string message);
    }
}
