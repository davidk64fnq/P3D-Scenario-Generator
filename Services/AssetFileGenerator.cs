using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.Models;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace P3D_Scenario_Generator.Services
{
    /// <summary>
    /// Manages the generation and updating of files (HTML, JavaScript, and CSS)
    /// necessary for the various scenario types within the simulation.
    /// </summary>
    /// <param name="logger">The application logger instance.</param>
    /// <param name="fileOps">The centralized file operations service.</param>
    /// <param name="progressReporter">The UI progress reporter.</param>
    internal class AssetFileGenerator(Logger logger, FileOps fileOps, FormProgressReporter progressReporter)
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
        /// Replaces the value of an object literal property (e.g., azTrueDeg: 0,)
        /// preserving original formatting and trailing commas.
        /// </summary>
        /// <param name="jsContent">The original JavaScript file content.</param>
        /// <param name="propName">The property name within the object literal.</param>
        /// <param name="rawValue">The replacement raw string or number value.</param>
        /// <returns>The modified JavaScript content.</returns>
        internal static string ReplaceJsObjectProperty(string jsContent, string propName, string rawValue)
        {
            string pattern = $@"(^|\r?\n|\r)(\s*){Regex.Escape(propName)}\s*:\s*[^,\r\n]+([,\r\n])";
            string replacement = $"$1$2{propName}: {rawValue}$3";

            return Regex.Replace(jsContent, pattern, replacement, RegexOptions.Multiline);
        }

        /// <summary>
        /// Replaces a nested object literal property (e.g., position: { latitude: 0, longitude: 0 })
        /// preserving indentation and trailing syntax.
        /// </summary>
        /// <param name="jsContent">The original JavaScript file content.</param>
        /// <param name="propName">The property name for the nested object.</param>
        /// <param name="rawJson">The replacement JSON or object literal string.</param>
        /// <returns>The modified JavaScript content.</returns>
        internal static string ReplaceJsObjectBlock(string jsContent, string propName, string rawJson)
        {
            string pattern = $@"(^|\r?\n|\r)(\s*){Regex.Escape(propName)}\s*:\s*\{{[\s\S]*?\}}([,\r\n])";
            string replacement = $"$1$2{propName}: {rawJson}$3";

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
        /// <returns><see langword="true"/> if the asset was successfully read, processed, and written; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> WriteAssetFileAsync(
            string resourceName,
            string fileName,
            string saveLocation,
            Dictionary<string, string>? replacements = null,
            Func<string, string>? customLogic = null)
        {
            string outputPath = Path.Combine(saveLocation, fileName);

            (bool success, string? content) = await _fileOps.TryReadAllTextFromResourceAsync(resourceName, _progressReporter);
            if (!success || content == null)
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
        /// <returns><see langword="true"/> if the stream was successfully retrieved and copied to disk; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> CopyAssetImageAsync(string resourceName, string outputPath)
        {
            var (success, stream) = await _fileOps.TryGetResourceStreamAsync(resourceName, _progressReporter);
            if (!success || stream is null) return false;

            using (stream)
            {
                return await _fileOps.TryCopyStreamToFileAsync(stream, outputPath, _progressReporter);
            }
        }

        /// <summary>
        /// Generates the client-side moving map JavaScript file for the scenario.
        /// </summary>
        /// <param name="count">The number of map tiles or waypoint coordinates.</param>
        /// <param name="formData">The scenario form configuration data.</param>
        /// <returns><see langword="true"/> if the script was generated successfully; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> GenerateMovingMapScriptAsync(int count, ScenarioFormData formData)
        {
            ArgumentNullException.ThrowIfNull(formData);

            var mapData = formData.OSMmapData.Take(count - 1).ToList();

            // Extract lat/lon waypoint arrays per leg from MapData.Items
            var legCoords = mapData
                .ConvertAll(m => m.Items.Select(c => new
                {
                    lat = c.Latitude.ToDouble(),
                    lon = c.Longitude.ToDouble()
                }))
;

            var replacements = new Dictionary<string, string>
            {
                { "mapNorthX",  $"[{string.Join(", ", mapData.Select(m => m.North.ToDouble().ToString(CultureInfo.InvariantCulture)))}]" },
                { "mapEastX",   $"[{string.Join(", ", mapData.Select(m => m.East.ToDouble().ToString(CultureInfo.InvariantCulture)))}]"  },
                { "mapSouthX",  $"[{string.Join(", ", mapData.Select(m => m.South.ToDouble().ToString(CultureInfo.InvariantCulture)))}]" },
                { "mapWestX",   $"[{string.Join(", ", mapData.Select(m => m.West.ToDouble().ToString(CultureInfo.InvariantCulture)))}]"  },
                { "legCoordsX", JsonSerializer.Serialize(legCoords) }
            };

            if (formData.MapWindowSize == MapWindowSizeOption.Size512)
            {
                replacements.Add("imagePixelsX", "[512, 1024, 2048]");
                replacements.Add("viewPortWidthX", "512");
                replacements.Add("viewPortHeightX", "512");
                replacements.Add("zoom1FilenameSuffixX", "1");
                replacements.Add("zoom2FilenameSuffixX", "2");
                replacements.Add("zoom3FilenameSuffixX", "3");
            }
            else
            {
                replacements.Add("imagePixelsX", "[1024, 2048, 4096]");
                replacements.Add("viewPortWidthX", "1024");
                replacements.Add("viewPortHeightX", "1024");
                replacements.Add("zoom1FilenameSuffixX", "2");
                replacements.Add("zoom2FilenameSuffixX", "3");
                replacements.Add("zoom3FilenameSuffixX", "4");
            }

            return await WriteAssetFileAsync(
                "Javascript.scriptsMovingMap.js",
                "scriptsMovingMap.js",
                formData.ScenarioImageFolder,
                replacements
            );
        }
    }
}