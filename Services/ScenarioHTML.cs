using P3D_Scenario_Generator.Models;

namespace P3D_Scenario_Generator.Services
{
    /// <summary>
    /// Handles the generation and copying of HTML and associated resource files for a scenario.
    /// </summary>
    /// <param name="logger">The logger for writing log messages.</param>
    /// <param name="fileOps">The file operations service for reading and writing files.</param>
    /// <param name="progressReporter">The progress reporter for UI updates.</param>
    internal class ScenarioHTML(Logger logger, FileOps fileOps, FormProgressReporter progressReporter)
    {
        private readonly Logger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly FileOps _fileOps = fileOps ?? throw new ArgumentNullException(nameof(fileOps));
        private readonly FormProgressReporter _progressReporter = progressReporter ?? throw new ArgumentNullException(nameof(progressReporter));

        /// <summary>
        /// A struct to hold data for the mission brief HTML template.
        /// </summary>
        internal struct MissionBrief
        {
            internal string title;
            internal string h1;
            internal string h2Location;
            internal string h2Difficulty;
            internal string h2Duration;
            internal string h2Aircraft;
            internal string pBriefing;
            internal string liObjective;
            internal string h2Tips;
        }

        /// <summary>
        /// Generates all necessary HTML files and copies supporting assets for a scenario.
        /// </summary>
        /// <param name="formData">The scenario form data, which includes paths and titles.</param>
        /// <param name="overview">The scenario overview data used to populate the HTML templates.</param>
        /// <returns><see langword="true"/> if all files were generated and copied successfully; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> GenerateHTMLfilesAsync(ScenarioFormData formData, Overview overview)
        {
            ArgumentNullException.ThrowIfNull(formData);
            ArgumentNullException.ThrowIfNull(overview);

            string overviewHtml = await GenerateOverviewHtmlAsync(overview);
            if (!await _fileOps.TryWriteAllTextAsync($"{formData.ScenarioFolder}\\Overview.htm", overviewHtml, _progressReporter))
            {
                string message = $"Failed to create Overview HTML file at {formData.ScenarioFolder}\\Overview.htm";
                await _logger.ErrorAsync(message);
                _progressReporter.Report($"ERROR: {message}");
                return false;
            }
            await _logger.InfoAsync($"Overview HTML file created successfully at {formData.ScenarioFolder}\\Overview.htm");
            _progressReporter.Report("INFO: Overview HTML file created.");

            string missionBriefHtml = await GenerateMissionBriefHtmlAsync(overview);
            if (!await _fileOps.TryWriteAllTextAsync($"{formData.ScenarioFolder}\\{formData.ScenarioTitle}.htm", missionBriefHtml, _progressReporter))
            {
                string message = $"Failed to create Mission Brief HTML file at {formData.ScenarioFolder}\\{formData.ScenarioTitle}.htm";
                await _logger.ErrorAsync(message);
                _progressReporter.Report($"ERROR: {message}");
                return false;
            }
            await _logger.InfoAsync($"Mission Brief HTML file created successfully at {formData.ScenarioFolder}\\{formData.ScenarioTitle}.htm");
            _progressReporter.Report("INFO: Mission Brief HTML file created.");

            return await CopyFilesAsync(formData);
        }

        /// <summary>
        /// Generates the HTML string for the scenario overview page.
        /// </summary>
        /// <param name="overview">The overview data to use for populating the template.</param>
        /// <returns>A task that returns the populated HTML string, or an empty string on failure.</returns>
        private async Task<string> GenerateOverviewHtmlAsync(Overview overview)
        {
            const string resourceName = "HTML.OverviewSource.htm";
            string message = "Loading and populating overview HTML template.";
            await _logger.InfoAsync(message);
            _progressReporter.Report($"INFO: {message}");

            var (success, overviewHtml) = await _fileOps.TryReadAllTextFromResourceAsync(resourceName, _progressReporter);
            if (!success || overviewHtml == null)
            {
                message = $"Failed to get HTML template from resource '{resourceName}'. HTML generation failed.";
                await _logger.ErrorAsync(message);
                _progressReporter.Report($"ERROR: {message}");
                return string.Empty;
            }

            overviewHtml = overviewHtml.Replace("overviewParams.title", overview.Title ?? string.Empty);
            overviewHtml = overviewHtml.Replace("overviewParams.h1", overview.Heading1 ?? string.Empty);
            overviewHtml = overviewHtml.Replace("overviewParams.h2Location", overview.Location ?? string.Empty);
            overviewHtml = overviewHtml.Replace("overviewParams.pDifficulty", overview.Difficulty ?? string.Empty);
            overviewHtml = overviewHtml.Replace("overviewParams.pDuration", overview.Duration ?? string.Empty);
            overviewHtml = overviewHtml.Replace("overviewParams.h2Aircraft", overview.Aircraft ?? string.Empty);
            overviewHtml = overviewHtml.Replace("overviewParams.pBriefing", overview.Briefing ?? string.Empty);
            overviewHtml = overviewHtml.Replace("overviewParams.liObjective", overview.Objective ?? string.Empty);
            overviewHtml = overviewHtml.Replace("overviewParams.liTips", overview.Tips ?? string.Empty);

            await _logger.InfoAsync("Successfully populated overview HTML template.");
            _progressReporter.Report("INFO: Overview HTML populated.");
            return overviewHtml;
        }

        /// <summary>
        /// Generates the HTML string for the mission brief page.
        /// </summary>
        /// <param name="overview">The overview data used to generate the mission brief.</param>
        /// <returns>A task that returns the populated HTML string, or an empty string on failure.</returns>
        private async Task<string> GenerateMissionBriefHtmlAsync(Overview overview)
        {
            const string resourceName = "HTML.MissionBriefSource.htm";
            string message = "Loading and populating mission brief HTML template.";
            await _logger.InfoAsync(message);
            _progressReporter.Report($"INFO: {message}");

            var (success, missionBriefHtml) = await _fileOps.TryReadAllTextFromResourceAsync(resourceName, _progressReporter);
            if (!success || missionBriefHtml == null)
            {
                message = $"Failed to get HTML template from resource '{resourceName}'. HTML generation failed.";
                await _logger.ErrorAsync(message);
                _progressReporter.Report($"ERROR: {message}");
                return string.Empty;
            }

            MissionBrief missionBrief = SetMissionBriefStruct(overview);

            missionBriefHtml = missionBriefHtml.Replace("missionBriefParams.title", missionBrief.title);
            missionBriefHtml = missionBriefHtml.Replace("missionBriefParams.h1", missionBrief.h1);
            missionBriefHtml = missionBriefHtml.Replace("missionBriefParams.h2Location", missionBrief.h2Location);
            missionBriefHtml = missionBriefHtml.Replace("missionBriefParams.h2Difficulty", missionBrief.h2Difficulty);
            missionBriefHtml = missionBriefHtml.Replace("missionBriefParams.h2Duration", missionBrief.h2Duration);
            missionBriefHtml = missionBriefHtml.Replace("missionBriefParams.h2Aircraft", missionBrief.h2Aircraft);
            missionBriefHtml = missionBriefHtml.Replace("missionBriefParams.pBriefing", missionBrief.pBriefing);
            missionBriefHtml = missionBriefHtml.Replace("missionBriefParams.liObjective", missionBrief.liObjective);
            missionBriefHtml = missionBriefHtml.Replace("missionBriefParams.h2Tips", missionBrief.h2Tips);

            await _logger.InfoAsync("Successfully populated mission brief HTML template.");
            _progressReporter.Report("INFO: Mission brief HTML populated.");
            return missionBriefHtml;
        }

        /// <summary>
        /// Creates a <see cref="MissionBrief"/> struct from an <see cref="Overview"/> object.
        /// </summary>
        /// <param name="overview">The overview data to convert.</param>
        /// <returns>A new <see cref="MissionBrief"/> struct.</returns>
        private static MissionBrief SetMissionBriefStruct(Overview overview)
        {
            return new MissionBrief
            {
                title = overview.Title ?? string.Empty,
                h1 = overview.Title ?? string.Empty,
                h2Location = overview.Location ?? string.Empty,
                h2Difficulty = overview.Difficulty ?? string.Empty,
                h2Duration = overview.Duration ?? string.Empty,
                h2Aircraft = overview.Aircraft ?? string.Empty,
                pBriefing = overview.Briefing ?? string.Empty,
                liObjective = overview.Objective ?? string.Empty,
                h2Tips = overview.Tips ?? string.Empty
            };
        }

        /// <summary>
        /// Copies all necessary supporting files (images, styles, sounds) for the scenario.
        /// </summary>
        /// <param name="formData">The scenario form data containing file paths.</param>
        /// <returns><see langword="true"/> if all files were copied successfully; otherwise, <see langword="false"/>.</returns>
        private async Task<bool> CopyFilesAsync(ScenarioFormData formData)
        {
            string message = "Starting to copy supporting scenario files.";
            await _logger.InfoAsync(message);
            _progressReporter.Report($"INFO: {message}");

            string aircraftImageSource = formData.AircraftImagePath;
            string aircraftImageDest = Path.Combine(formData.ScenarioImageFolder, "Overview_01.jpg");

            if (FileOps.FileExists(aircraftImageSource))
            {
                if (!await _fileOps.TryCopyFileAsync(aircraftImageSource, aircraftImageDest, _progressReporter, true))
                {
                    message = $"Failed to copy aircraft image from {aircraftImageSource} to {aircraftImageDest}.";
                    await _logger.ErrorAsync(message);
                    _progressReporter.Report($"ERROR: {message}");
                    return false;
                }
                await _logger.InfoAsync($"Successfully copied aircraft image from {aircraftImageSource} to {aircraftImageDest}.");
                _progressReporter.Report("INFO: Aircraft image copied.");
            }
            else
            {
                if (!await _fileOps.CopyResourceFileAsync("Images.thumbnail.jpg", aircraftImageDest, _progressReporter))
                {
                    message = "Failed to copy default aircraft image from resources.";
                    await _logger.ErrorAsync(message);
                    _progressReporter.Report($"ERROR: {message}");
                    return false;
                }
                await _logger.InfoAsync($"Successfully copied default aircraft image from resource to {aircraftImageDest}.");
                _progressReporter.Report("INFO: Default aircraft image copied.");
            }

            string soundDirectoryPath = Path.Combine(formData.ScenarioFolder, "sound");
            if (!await _fileOps.TryCreateDirectoryAsync(soundDirectoryPath, _progressReporter))
            {
                return false;
            }

            if (!await _fileOps.CopyResourceFileAsync("CSS.style_kneeboard.css", Path.Combine(formData.ScenarioFolder, "style_kneeboard.css"), _progressReporter) ||
                !await _fileOps.CopyResourceFileAsync("CSS.style_load_flight.css", Path.Combine(formData.ScenarioFolder, "style_load_flight.css"), _progressReporter) ||
                !await _fileOps.CopyResourceFileAsync("Sounds.ThruHoop.wav", Path.Combine(soundDirectoryPath, "ThruHoop.wav"), _progressReporter) ||
                !await _fileOps.CopyResourceFileAsync("Images.aircraft.png", Path.Combine(formData.ScenarioImageFolder, "aircraft.png"), _progressReporter) ||
                !await _fileOps.CopyResourceFileAsync("Images.header.png", Path.Combine(formData.ScenarioImageFolder, "header.png"), _progressReporter))
            {
                return false;
            }

            message = "All files copied successfully.";
            await _logger.InfoAsync(message);
            _progressReporter.Report($"INFO: {message}");
            return true;
        }

        /// <summary>
        /// Extracts the numeric duration from the overview duration string.
        /// </summary>
        /// <param name="overview">The overview data containing the duration string.</param>
        /// <returns>The duration as an integer, or 0 if parsing fails.</returns>
        internal static int GetDuration(Overview overview)
        {
            if (overview?.Duration != null)
            {
                string[] words = overview.Duration.Split(' ');
                if (words.Length > 0 && int.TryParse(words[0], out int duration))
                {
                    return duration;
                }
            }

            return 0;
        }
    }
}