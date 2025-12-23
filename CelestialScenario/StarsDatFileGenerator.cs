using P3D_Scenario_Generator.Models;
using P3D_Scenario_Generator.Services;
using System.Text;

namespace P3D_Scenario_Generator.CelestialScenario
{
    /// <summary>
    /// Handles the generation and management of simulator-specific files, primarily the "stars.dat" file, to ensure consistency between
    /// the celestial sextant display and the in-simulator view of stars.
    /// </summary>
    public sealed class StarsDatFileGenerator(Logger logger, FileOps fileOps, FormProgressReporter progressReporter)
    {
        private readonly Logger _logger = logger;
        private readonly FileOps _fileOps = fileOps;
        private readonly FormProgressReporter _progressReporter = progressReporter;

        /// <summary>
        /// Creates a P3D Scenario Generator specific version of "stars.dat" if requested by user.
        /// </summary>
        /// <param name="formData">The scenario data containing the file paths.</param>
        /// <param name="starDataManager">The star data manager instance.</param>
        /// <returns><see langword="true"/> if all needed file operations complete successfully; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> CreateStarsDatAsync(ScenarioFormData formData, StarDataManager starDataManager)
        {
            _progressReporter.Report("INFO: Preparing to create stars.dat.P3DscenarioGenerator file.");

            // Use LINQ and string.Join for a cleaner, modern approach to the content block
            var starLines = starDataManager.Stars
                .Take(starDataManager.NoStars)
                .Select((s, i) => $"Star.{i} = {i + 1},{s.RaH},{s.RaM},{s.RaS},{s.DecD},{s.DecM},{s.DecS},{s.VisMag}");

            StringBuilder sb = new();
            sb.AppendLine("[Star Settings]");
            sb.AppendLine("Intensity=230");
            sb.AppendLine($"NumStars={starDataManager.NoStars}");
            sb.AppendLine("[Star Locations]");
            sb.Append(string.Join("\n", starLines));

            string outputPath = Path.Combine(formData.P3DProgramData, "stars.dat.P3DscenarioGenerator");

            if (!await _fileOps.TryWriteAllTextAsync(outputPath, sb.ToString(), _progressReporter))
            {
                await _logger.ErrorAsync($"Failed to write stars.dat to: {outputPath}");
                return false;
            }

            await _logger.InfoAsync("Successfully generated stars.dat.P3DscenarioGenerator");
            return true;
        }
    }
}
