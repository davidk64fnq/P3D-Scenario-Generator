using P3D_Scenario_Generator.Models;
using P3D_Scenario_Generator.Services;
using System.Text;

namespace P3D_Scenario_Generator.CelestialScenario
{
    /// <summary>
    /// Handles the generation and management of simulator-specific files,
    /// primarily the "stars.dat" file, to ensure consistency between
    /// the celestial sextant display and the in-simulator view of stars.
    /// It provides functionality to backup the original "stars.dat" and
    /// replace it with a program-generated version containing relevant star data.
    /// </summary>
    public sealed class StarsDatFileGenerator(Logger logger, FileOps fileOps, IProgress<string> progressReporter)
    {
        // Guard clauses to validate the constructor parameters.
        private readonly Logger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly FileOps _fileOps = fileOps ?? throw new ArgumentNullException(nameof(fileOps));
        private readonly IProgress<string> _progressReporter = progressReporter ?? throw new ArgumentNullException(nameof(progressReporter));

        /// <summary>
        /// Creates a P3D Scenario Generator specific version of "stars.dat" if requested by user.
        /// </summary>
        /// <param name="formData">The scenario data containing the file paths.</param>
        /// <param name="starDataManager">The star data manager instance.</param>
        /// <returns><see langword="true"/> if all needed file operations complete successfully; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> CreateStarsDatAsync(ScenarioFormData formData, StarDataManager starDataManager)
        {
            string starsDatGGversionPath = Path.Combine(formData.P3DProgramData, "stars.dat.P3DscenarioGenerator");

            if (formData.UseCustomStarsDat == false)
            {
                _progressReporter.Report("INFO: User does not require creation of stars.dat.P3DscenarioGenerator file.");
                return false;
            }

            _progressReporter.Report("INFO: Preparing to create stars.dat.P3DscenarioGenerator file.");

            // Use StringBuilder for efficient string concatenation in a loop.
            StringBuilder starsDatContentBuilder = new();
            starsDatContentBuilder.AppendLine("[Star Settings]");
            starsDatContentBuilder.AppendLine($"Intensity=230");
            starsDatContentBuilder.AppendLine($"NumStars={starDataManager.NoStars}");
            starsDatContentBuilder.AppendLine("[Star Locations]");

            for (int index = 0; index < starDataManager.NoStars; index++)
            {
                starsDatContentBuilder.Append($"Star.{index} = {index + 1}");
                starsDatContentBuilder.Append($",{starDataManager.Stars[index].RaH}");
                starsDatContentBuilder.Append($",{starDataManager.Stars[index].RaM}");
                starsDatContentBuilder.Append($",{starDataManager.Stars[index].RaS}");
                starsDatContentBuilder.Append($",{starDataManager.Stars[index].DecD}");
                starsDatContentBuilder.Append($",{starDataManager.Stars[index].DecM}");
                starsDatContentBuilder.Append($",{starDataManager.Stars[index].DecS}");
                starsDatContentBuilder.Append($",{starDataManager.Stars[index].VisMag}\n");
            }

            if (!await _fileOps.TryWriteAllTextAsync(starsDatGGversionPath, starsDatContentBuilder.ToString(), _progressReporter))
            {
                await _logger.ErrorAsync($"Failed to write file to '{starsDatGGversionPath}'.");
                _progressReporter.Report($"ERROR: Failed to write file to '{starsDatGGversionPath}'.");
                return false;
            }

            _progressReporter.Report("INFO: Successfully created new 'stars.dat.P3DscenarioGenerator' file.");
            await _logger.InfoAsync("Successfully created new 'stars.dat.P3DscenarioGenerator' file.");

            return true;
        }
    }
}
