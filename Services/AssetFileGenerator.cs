using System.Text.RegularExpressions;

namespace P3D_Scenario_Generator.Services
{
    /// <summary>
    /// Manages the generation and updating of files (HTML, JavaScript, and CSS)
    /// necessary for the various scenario types within the simulation.
    /// </summary>
    public class AssetFileGenerator(Logger logger, FileOps fileOps, IProgress<string> progressReporter)
    {
        // Guard clauses to validate the constructor parameters.
        private readonly Logger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly FileOps _fileOps = fileOps ?? throw new ArgumentNullException(nameof(fileOps));
        private readonly IProgress<string> _progressReporter = progressReporter ?? throw new ArgumentNullException(nameof(progressReporter));

        /// <summary>
        /// Safely replaces the assignment value of a specific JavaScript variable using Regex,
        /// preserving its original declaration keyword (let, const, or var).
        /// </summary>
        /// <param name="jsContent">The original JavaScript file content.</param>
        /// <param name="varName">The exact name of the JavaScript variable (e.g., 'linesX').</param>
        /// <param name="rawValue">The raw string value to inject (e.g., a JSON array or a quoted string).</param>
        /// <returns>The modified JavaScript content.</returns>
        private static string ReplaceJsVariable(string jsContent, string varName, string rawValue)
        {
            // Capture Group 1: The declaration keyword (let, const, or var)
            // Non-capture Group: Ensures the match starts on a newline or file start boundary
            // The pattern captures the declaration, variable name, and the assignment operator (=),
            // and then matches everything up to the semicolon, which is necessary to include.

            string pattern = $@"(^|\r?\n|\r)\s*(let|const|var)\s+{Regex.Escape(varName)}\s*[^;]*;";

            // The replacement reconstructs the line:
            // $1: The boundary (\n or start of file)
            // $2: The original declaration keyword (let/const/var)
            // The new assignment statement is formed using the provided rawValue.
            string replacement = $"$1$2 {varName} = {rawValue};";

            // Use RegexOptions.Multiline to handle ^ (start of line) and ignore comments/JSDoc above the declaration.
            return Regex.Replace(jsContent, pattern, replacement, RegexOptions.Multiline);
        }

        /// <summary>
        /// General purpose helper to Load, Process (optional), and Write text assets (JS/CSS).
        /// </summary>
        private async Task<bool> WriteAssetFileAsync(
            string resourceName,
            string fileName,
            string saveLocation,
            Dictionary<string, string> replacements = null,
            Func<string, string> customLogic = null)
        {
            string outputPath = Path.Combine(saveLocation, fileName);

            // Using your existing TryReadAllTextFromResourceAsync for simplicity
            (bool success, string content) = await _fileOps.TryReadAllTextFromResourceAsync(resourceName, _progressReporter);
            if (!success)
            {
                await _logger.ErrorAsync($"Resource missing: {resourceName}");
                return false;
            }

            // Apply standard replacements
            if (replacements != null)
            {
                foreach (var kvp in replacements)
                {
                    content = ReplaceJsVariable(content, kvp.Key, kvp.Value);
                }
            }

            // Apply logic like SetCelestialMapEdges
            if (customLogic != null)
            {
                content = customLogic(content);
            }

            if (!await _fileOps.TryWriteAllTextAsync(outputPath, content, _progressReporter))
            {
                await _logger.ErrorAsync($"Failed to write asset: {fileName}");
                return false;
            }

            await _logger.InfoAsync($"Successfully generated: {fileName}");
            return true;
        }

        private async Task<bool> CopyAssetImageAsync(string resourceName, string outputPath)
        {
            // Uses your existing FileOps method
            var (success, stream) = await _fileOps.TryGetResourceStreamAsync(resourceName, _progressReporter);
            if (!success) return false;

            using (stream)
            {
                // Uses your existing FileOps method
                return await _fileOps.TryCopyStreamToFileAsync(stream, outputPath, _progressReporter);
            }
        }
    }
}
