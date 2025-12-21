using System.Text.RegularExpressions;

namespace P3D_Scenario_Generator.Services
{
    /// <summary>
    /// Manages the generation and updating of files (HTML, JavaScript, and CSS)
    /// necessary for the various scenario types within the simulation.
    /// </summary>
    public class AssetFileGenerator(Logger logger, FileOps fileOps, FormProgressReporter progressReporter)
    {
        private readonly Logger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly FileOps _fileOps = fileOps ?? throw new ArgumentNullException(nameof(fileOps));
        private readonly FormProgressReporter _progressReporter = progressReporter ?? throw new ArgumentNullException(nameof(progressReporter));

        /// <summary>
        /// Safely replaces the assignment value of a specific JavaScript variable using Regex,
        /// preserving its original declaration keyword (let, const, or var).
        /// </summary>
        /// <param name="jsContent">The original JavaScript file content.</param>
        /// <param name="varName">The exact name of the JavaScript variable (e.g., 'linesX').</param>
        /// <param name="rawValue">The raw string value to inject (e.g., a JSON array or a quoted string).</param>
        /// <returns>The modified JavaScript content.</returns>
        internal static string ReplaceJsVariable(string jsContent, string varName, string rawValue)
        {
            string pattern = $@"(^|\r?\n|\r)\s*(let|const|var)\s+{Regex.Escape(varName)}\s*[^;]*;";
            string replacement = $"$1$2 {varName} = {rawValue};";

            return Regex.Replace(jsContent, pattern, replacement, RegexOptions.Multiline);
        }

        /// <summary>
        /// Orchestrates the workflow of reading an embedded resource, applying string or regex 
        /// replacements, executing optional custom logic, and writing the result to a physical file.
        /// </summary>
        /// <param name="resourceName">The manifest resource name of the source asset.</param>
        /// <param name="fileName">The destination file name.</param>
        /// <param name="saveLocation">The directory path where the file should be created.</param>
        /// <param name="replacements">A dictionary where Keys are JS variable names and Values are the new assignments.</param>
        /// <param name="customLogic">An optional delegate for advanced content manipulation after standard replacements.</param>
        /// <returns>True if the asset was successfully read, processed, and written; otherwise, false.</returns>
        internal async Task<bool> WriteAssetFileAsync(
            string resourceName,
            string fileName,
            string saveLocation,
            Dictionary<string, string> replacements = null,
            Func<string, string> customLogic = null)
        {
            string outputPath = Path.Combine(saveLocation, fileName);

            (bool success, string content) = await _fileOps.TryReadAllTextFromResourceAsync(resourceName, _progressReporter);
            if (!success)
            {
                await _logger.ErrorAsync($"Resource missing: {resourceName}");
                return false;
            }

            if (replacements != null)
            {
                foreach (var kvp in replacements)
                {
                    content = ReplaceJsVariable(content, kvp.Key, kvp.Value);
                }
            }

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

        /// <summary>
        /// Streams a binary asset (e.g., an image) from embedded resources directly to a file on disk.
        /// </summary>
        /// <param name="resourceName">The manifest resource name of the image.</param>
        /// <param name="outputPath">The full destination file path including filename and extension.</param>
        /// <returns>True if the stream was successfully retrieved and copied to disk; otherwise, false.</returns>
        internal async Task<bool> CopyAssetImageAsync(string resourceName, string outputPath)
        {
            var (success, stream) = await _fileOps.TryGetResourceStreamAsync(resourceName, _progressReporter);
            if (!success) return false;

            using (stream)
            {
                return await _fileOps.TryCopyStreamToFileAsync(stream, outputPath, _progressReporter);
            }
        }
    }
}