using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.Models;

namespace P3D_Scenario_Generator.Services
{
    /// <summary>
    /// Methods for user selection of an aircraft variant for a scenario. The selected variants are stored
    /// in <see cref="AircraftVariants"/> in alphabetical order by display name.
    /// </summary>
    /// <param name="log">The logging service instance.</param>
    /// <param name="cacheManager">The cache management service instance.</param>
    internal class Aircraft(Logger log, CacheManager cacheManager)
    {
        private readonly Logger _log = log ?? throw new ArgumentNullException(nameof(log));
        private readonly CacheManager _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));

        /// <summary>
        /// Gets the list of aircraft variants maintained by the user, sorted alphabetically by display name.
        /// </summary>
        internal List<AircraftVariant> AircraftVariants { get; private set; } = [];

        /// <summary>
        /// Gets the index of the currently selected aircraft variant displayed on the form.
        /// </summary>
        internal int CurrentAircraftVariantIndex { get; private set; } = -1;

        #region Prompt user for and read a new aircraft variant from P3D files section

        /// <summary>
        /// Prompts the user to select an aircraft variant thumbnail image and collects the aircraft title,
        /// cruise speed, whether it has wheels or equivalent, and whether it has floats, from the aircraft.cfg file.
        /// </summary>
        /// <param name="formData">The scenario form data containing paths like the P3D install directory.</param>
        /// <returns>The display name of the new aircraft variant, or an empty string if cancelled or invalid.</returns>
        internal async Task<string> ChooseAircraftVariantAsync(ScenarioFormData formData)
        {
            AircraftVariant aircraftVariant = new();

            string selectedPath = await GetThumbnailAsync(formData);

            if (!string.IsNullOrEmpty(selectedPath))
            {
                aircraftVariant.ThumbnailImagePath = ResolveValidThumbnailPath(selectedPath);
                aircraftVariant.Title = await GetAircraftTitleAsync(selectedPath);
                aircraftVariant.DisplayName = aircraftVariant.Title;
                aircraftVariant.CruiseSpeed = await GetAircraftCruiseSpeedAsync(selectedPath);
                aircraftVariant.HasFloats = await GetAircraftFloatsStatusAsync(selectedPath);
                aircraftVariant.HasWheelsOrEquiv = await GetAircraftWheelsStatusAsync(selectedPath);

                if (AddAircraftVariant(aircraftVariant))
                {
                    return aircraftVariant.DisplayName;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Ensures the assigned thumbnail path points to an image matching standard P3D thumbnail dimensions (2:1 ratio).
        /// If a non-image file or invalid dimensions are selected, scans the folder for a valid thumbnail,
        /// falling back to default thumbnail if none match.
        /// </summary>
        private static string ResolveValidThumbnailPath(string selectedPath)
        {
            if (FileOps.FileExists(selectedPath) && IsValidThumbnailDimensions(selectedPath))
            {
                return selectedPath;
            }

            string textureFolderPath = Path.GetDirectoryName(selectedPath)!;
            string standardThumbnailPath = Path.Combine(textureFolderPath, "thumbnail.jpg");

            if (FileOps.FileExists(standardThumbnailPath) && IsValidThumbnailDimensions(standardThumbnailPath))
            {
                return standardThumbnailPath;
            }

            string[] candidateImages = [.. Directory.GetFiles(textureFolderPath, "*.*")
                .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))];

            foreach (string imagePath in candidateImages)
            {
                if (IsValidThumbnailDimensions(imagePath))
                {
                    return imagePath;
                }
            }

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Images", "thumbnail.jpg");
        }

        /// <summary>
        /// Verifies if an image file matches standard Prepar3D thumbnail dimensions (2:1 aspect ratio).
        /// </summary>
        private static bool IsValidThumbnailDimensions(string filePath)
        {
            if (!FileOps.FileExists(filePath))
            {
                return false;
            }

            try
            {
                byte[] imageBytes = FileOps.ReadAllBytes(filePath);
                using var ms = new MemoryStream(imageBytes);
                using var img = System.Drawing.Image.FromStream(ms, useEmbeddedColorManagement: false, validateImageData: false);

                double aspectRatio = (double)img.Width / img.Height;
                bool hasCorrectRatio = Math.Abs(aspectRatio - 2.0) < 0.15;
                bool isReasonableSize = img.Width <= 1024 && img.Height <= 512;

                return hasCorrectRatio && isReasonableSize;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Prompts the user to select an aircraft thumbnail or image file from within a texture folder.
        /// </summary>
        /// <param name="formData">The scenario form data containing paths like the P3D install directory.</param>
        /// <returns>Full path of the selected image file, or an empty string if cancelled or invalid.</returns>
        internal async Task<string> GetThumbnailAsync(ScenarioFormData formData)
        {
            using OpenFileDialog openFileDialog = new()
            {
                Title = "Select a thumbnail image or file from within a \"texture.X\" folder",
                DefaultExt = "jpg",
                Filter = "Image Files (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp|All files (*.*)|*.*",
                FilterIndex = 1,
                InitialDirectory = Path.Combine(formData.P3DProgramInstall, "SimObjects"),
                RestoreDirectory = false
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string thumbnailPath = openFileDialog.FileName;

                if (string.IsNullOrEmpty(GetTextureValue(thumbnailPath)))
                {
                    const string formatError = "Not a valid variant. Please select an image file from within a texture folder name containing a \".\" (e.g., texture.1)";
                    await _log.WarningAsync(formatError);
                    MessageBox.Show(formatError, Constants.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return string.Empty;
                }

                string textureFolderPath = Path.GetDirectoryName(thumbnailPath)!;
                string aircraftFolderPath = Path.GetDirectoryName(textureFolderPath)!;

                if (Directory.GetDirectories(aircraftFolderPath, "panel*").Length == 0)
                {
                    string aiError = $"This is an AI aircraft; there is no panel folder in {aircraftFolderPath}";
                    await _log.WarningAsync(aiError);
                    MessageBox.Show(aiError, Constants.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return string.Empty;
                }

                return openFileDialog.FileName;
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the aircraft title from the aircraft.cfg (or equivalent) file.
        /// </summary>
        /// <param name="thumbnailPath">Path to the user-selected aircraft variant thumbnail image.</param>
        /// <returns>The aircraft variant title string, or an empty string if not found.</returns>
        internal async Task<string> GetAircraftTitleAsync(string thumbnailPath)
        {
            string textureValue = GetTextureValue(thumbnailPath);
            string aircraftCfg = await GetAircraftCFGAsync(thumbnailPath);

            using StringReader reader = new(aircraftCfg);
            string? currentLine;
            string currentTitle = string.Empty;

            while ((currentLine = reader.ReadLine()) != null)
            {
                if (!TryExtractCfgKey(currentLine, out string key))
                {
                    continue;
                }

                int equalsIndex = currentLine.IndexOf('=');
                string rawValue = currentLine[(equalsIndex + 1)..];

                switch (key)
                {
                    case "title":
                        currentTitle = SanitizeCfgValue(rawValue);
                        break;

                    case "texture":
                        string currentTexture = SanitizeCfgValue(rawValue);
                        if (currentTexture.Equals(textureValue, StringComparison.OrdinalIgnoreCase))
                        {
                            return currentTitle;
                        }
                        break;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Extracts the key name from a raw configuration line, ignoring whitespace.
        /// </summary>
        private static bool TryExtractCfgKey(string line, out string key)
        {
            key = string.Empty;
            string trimmedLine = line.Trim();

            if (string.IsNullOrEmpty(trimmedLine) ||
                trimmedLine.StartsWith('[') ||
                trimmedLine.StartsWith(';') ||
                trimmedLine.StartsWith("//"))
            {
                return false;
            }

            int equalsIndex = trimmedLine.IndexOf('=');
            if (equalsIndex == -1)
            {
                return false;
            }

            key = trimmedLine[..equalsIndex].Trim().ToLowerInvariant();
            return true;
        }

        /// <summary>
        /// Strips inline comments and removes surrounding whitespace or quotes from a raw configuration value.
        /// </summary>
        private static string SanitizeCfgValue(string rawValue)
        {
            int commentIndex = rawValue.IndexOfAny([';', '/']);
            if (commentIndex != -1)
            {
                rawValue = rawValue[..commentIndex];
            }

            return rawValue.Trim().Trim('"');
        }

        /// <summary>
        /// Reads the aircraft.cfg file associated with the selected aircraft variant into a string.
        /// </summary>
        /// <param name="thumbnailPath">Path to the user-selected aircraft variant thumbnail image.</param>
        /// <returns>Contents of the aircraft.cfg (or sim.cfg) file, or an empty string if missing.</returns>
        internal async Task<string> GetAircraftCFGAsync(string thumbnailPath)
        {
            string? textureFolderPath = Path.GetDirectoryName(thumbnailPath);
            string? aircraftFolderPath = Path.GetDirectoryName(textureFolderPath);

            if (string.IsNullOrEmpty(aircraftFolderPath))
            {
                await _log.ErrorAsync($"Unable to resolve aircraft folder from path '{thumbnailPath}'.");
                return string.Empty;
            }

            string aircraftCfg = Path.Combine(aircraftFolderPath, "aircraft.cfg");
            string simCfg = Path.Combine(aircraftFolderPath, "sim.cfg");

            if (FileOps.FileExists(aircraftCfg))
            {
                return FileOps.ReadAllText(aircraftCfg);
            }

            if (FileOps.FileExists(simCfg))
            {
                return FileOps.ReadAllText(simCfg);
            }

            await _log.ErrorAsync($"Unable to locate aircraft.cfg or sim.cfg for selected aircraft variant at '{aircraftFolderPath}'.");
            MessageBox.Show("Unable to locate aircraft.cfg or sim.cfg for selected aircraft variant. Rename equivalent file and advise developer.",
                Constants.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return string.Empty;
        }

        /// <summary>
        /// Extracts the texture subfolder identifier from the thumbnail image path.
        /// </summary>
        /// <param name="thumbnailPath">Path containing the thumbnail filename and texture folder name.</param>
        /// <returns>The texture suffix value (e.g., "1" for "texture.1"), or empty string.</returns>
        internal static string GetTextureValue(string thumbnailPath)
        {
            string? textureFolderPath = Path.GetDirectoryName(thumbnailPath);
            if (string.IsNullOrEmpty(textureFolderPath))
            {
                return string.Empty;
            }

            string? textureFolder = Path.GetFileName(textureFolderPath);
            if (string.IsNullOrEmpty(textureFolder))
            {
                return string.Empty;
            }

            string[] splitOnPeriod = textureFolder.Split('.');
            return splitOnPeriod.Length <= 1 ? string.Empty : splitOnPeriod[1];
        }

        /// <summary>
        /// Gets the aircraft cruise speed from the aircraft.cfg file.
        /// </summary>
        /// <param name="thumbnailPath">Path to the user-selected aircraft variant thumbnail image.</param>
        /// <returns>The cruise speed in knots, or 0.0 if not found or invalid.</returns>
        internal async Task<double> GetAircraftCruiseSpeedAsync(string thumbnailPath)
        {
            string aircraftCfg = await GetAircraftCFGAsync(thumbnailPath);
            using StringReader reader = new(aircraftCfg);
            string? currentLine;

            while ((currentLine = reader.ReadLine()) != null)
            {
                if (TryExtractCfgKey(currentLine, out string key) && key == "cruise_speed")
                {
                    int equalsIndex = currentLine.IndexOf('=');
                    string rawValue = currentLine[(equalsIndex + 1)..];
                    string cleanValue = SanitizeCfgValue(rawValue);

                    const double minCruiseSpeed = 0.0;
                    const double maxCruiseSpeed = Constants.PlausibleMaxCruiseSpeedKnots;

                    if (ParsingHelpers.TryParseDouble(
                        cleanValue,
                        "Aircraft cruise speed",
                        minCruiseSpeed,
                        maxCruiseSpeed,
                        out double cruiseSpeedOut,
                        out _,
                        "knots"))
                    {
                        return cruiseSpeedOut;
                    }

                    return 0.0;
                }
            }

            return 0.0;
        }

        /// <summary>
        /// Evaluates whether the aircraft is equipped with floats based on contact points in aircraft.cfg.
        /// </summary>
        /// <param name="thumbnailPath">Path to the user-selected aircraft variant thumbnail image.</param>
        /// <returns><see langword="true"/> if equipped with floats and not skis; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> GetAircraftFloatsStatusAsync(string thumbnailPath)
        {
            string aircraftCfg = await GetAircraftCFGAsync(thumbnailPath);
            using StringReader reader = new(aircraftCfg);
            string? currentLine;
            bool hasFloats = false;
            bool hasSkis = false;

            while ((currentLine = reader.ReadLine()) != null)
            {
                currentLine = currentLine.Trim();

                if (currentLine.StartsWith(';') || currentLine.StartsWith("//"))
                {
                    continue;
                }

                if (currentLine.StartsWith("point.", StringComparison.OrdinalIgnoreCase))
                {
                    int equalsIndex = currentLine.IndexOf('=');
                    if (equalsIndex == -1)
                    {
                        continue;
                    }

                    string rhs = currentLine[(equalsIndex + 1)..].Trim().Trim('"');
                    int commentIndex = rhs.IndexOfAny([';', '/']);
                    if (commentIndex != -1)
                    {
                        rhs = rhs[..commentIndex].Trim();
                    }

                    string[] parts = rhs.Split(',');
                    if (parts.Length > 0 && int.TryParse(parts[0].Trim(), out int contactPointClass))
                    {
                        if (contactPointClass == 4)
                        {
                            hasFloats = true;
                        }
                        else if (contactPointClass == 3 || contactPointClass == 16)
                        {
                            hasSkis = true;
                        }
                    }
                }
            }

            return hasFloats && !hasSkis;
        }

        /// <summary>
        /// Evaluates whether the aircraft is equipped with wheels, scrapes, skids, or skis from aircraft.cfg.
        /// </summary>
        /// <param name="thumbnailPath">Path to the user-selected aircraft variant thumbnail image.</param>
        /// <returns><see langword="true"/> if landing gear / skids are present; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> GetAircraftWheelsStatusAsync(string thumbnailPath)
        {
            string aircraftCfg = await GetAircraftCFGAsync(thumbnailPath);
            using StringReader reader = new(aircraftCfg);
            string? currentLine;

            while ((currentLine = reader.ReadLine()) != null)
            {
                currentLine = currentLine.Trim();

                if (currentLine.StartsWith(';') || currentLine.StartsWith("//"))
                {
                    continue;
                }

                if (currentLine.StartsWith("point.", StringComparison.OrdinalIgnoreCase))
                {
                    int equalsIndex = currentLine.IndexOf('=');
                    if (equalsIndex == -1)
                    {
                        continue;
                    }

                    string rhs = currentLine[(equalsIndex + 1)..].Trim().Trim('"');
                    int commentIndex = rhs.IndexOfAny([';', '/']);
                    if (commentIndex != -1)
                    {
                        rhs = rhs[..commentIndex].Trim();
                    }

                    string[] parts = rhs.Split(',');
                    if (parts.Length > 0 && int.TryParse(parts[0].Trim(), out int contactPointClass))
                    {
                        if ((contactPointClass >= 1 && contactPointClass <= 3) || contactPointClass == 16)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Adds an aircraft variant to <see cref="AircraftVariants"/>, maintaining alphabetical order by display name.
        /// </summary>
        /// <param name="aircraftVariant">The aircraft variant to add.</param>
        /// <returns><see langword="true"/> if added; <see langword="false"/> if a variant with the same title already exists.</returns>
        internal bool AddAircraftVariant(AircraftVariant aircraftVariant)
        {
            if (AircraftVariants == null || AircraftVariants.Count == 0)
            {
                AircraftVariants = [aircraftVariant];
                CurrentAircraftVariantIndex = 0;
                return true;
            }

            int variantIndex = AircraftVariants.FindIndex(aircraft => aircraft.Title == aircraftVariant.Title);
            if (variantIndex == -1)
            {
                AircraftVariants.Add(aircraftVariant);
                AircraftVariants.Sort((x, y) => string.Compare(x.DisplayName, y.DisplayName, StringComparison.OrdinalIgnoreCase));
                ChangeCurrentAircraftVariantIndex(aircraftVariant.DisplayName);
                return true;
            }

            return false;
        }

        #endregion

        #region Manage list of aircraft variants section

        /// <summary>
        /// Resets <see cref="CurrentAircraftVariantIndex"/> to the variant matching <paramref name="displayName"/>.
        /// </summary>
        /// <param name="displayName">The display name of the variant to set as active.</param>
        internal void ChangeCurrentAircraftVariantIndex(string displayName)
        {
            AircraftVariant? aircraftVariant = AircraftVariants.Find(aircraft => aircraft.DisplayName == displayName);
            if (aircraftVariant != null)
            {
                CurrentAircraftVariantIndex = AircraftVariants.IndexOf(aircraftVariant);
            }
        }

        /// <summary>
        /// Deletes the aircraft variant matching <paramref name="displayName"/> from <see cref="AircraftVariants"/>.
        /// </summary>
        /// <param name="displayName">The display name of the variant to delete.</param>
        /// <returns><see langword="true"/> if removed; otherwise, <see langword="false"/>.</returns>
        internal bool DeleteAircraftVariant(string displayName)
        {
            if (AircraftVariants?.Count > 0)
            {
                AircraftVariant? aircraftVariant = AircraftVariants.Find(aircraft => aircraft.DisplayName == displayName);
                if (aircraftVariant != null)
                {
                    AircraftVariants.Remove(aircraftVariant);
                    CurrentAircraftVariantIndex--;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Updates the display name of the currently selected aircraft variant and re-sorts the list.
        /// </summary>
        /// <param name="displayName">The new unique display name.</param>
        internal void UpdateAircraftVariantDisplayName(string displayName)
        {
            if (AircraftVariants?.FindAll(aircraft => aircraft.DisplayName == displayName).Count == 0)
            {
                AircraftVariants[CurrentAircraftVariantIndex].DisplayName = displayName;
                AircraftVariants.Sort((x, y) => string.Compare(x.DisplayName, y.DisplayName, StringComparison.OrdinalIgnoreCase));
                ChangeCurrentAircraftVariantIndex(displayName);
            }
        }

        #endregion

        #region Provide aircraft variant display information section

        /// <summary>
        /// Gets an alphabetically sorted list of the aircraft variant display names.
        /// </summary>
        /// <returns>A list of display names.</returns>
        internal List<string> GetAircraftVariantDisplayNames()
        {
            if (AircraftVariants == null)
            {
                return [];
            }

            return [.. AircraftVariants.Select(v => v.DisplayName)];
        }

        /// <summary>
        /// Builds a formatted multi-line string of the currently selected aircraft variant.
        /// </summary>
        /// <returns>Formatted string containing title, display name, speed, and gear capability.</returns>
        internal string SetTextBoxGeneralAircraftValues()
        {
            if (AircraftVariants == null || AircraftVariants.Count == 0 ||
                CurrentAircraftVariantIndex < 0 || CurrentAircraftVariantIndex >= AircraftVariants.Count)
            {
                return string.Empty;
            }

            AircraftVariant currentVariant = AircraftVariants[CurrentAircraftVariantIndex];

            return $"Title = {currentVariant.Title}\n" +
                   $"Display Name = {currentVariant.DisplayName}\n" +
                   $"Cruise Speed = {currentVariant.CruiseSpeed}\n" +
                   $"Has Wheels/Scrapes/Skis = {currentVariant.HasWheelsOrEquiv}\n" +
                   $"Has Floats = {currentVariant.HasFloats}";
        }

        /// <summary>
        /// Retrieves the currently selected aircraft variant.
        /// </summary>
        /// <returns>The active <see cref="AircraftVariant"/>, or <see langword="null"/> if none is selected.</returns>
        internal async Task<AircraftVariant?> GetCurrentVariantAsync()
        {
            if (AircraftVariants == null || CurrentAircraftVariantIndex < 0 || CurrentAircraftVariantIndex >= AircraftVariants.Count)
            {
                await _log.WarningAsync("Attempted to retrieve current aircraft variant with no variants loaded or an invalid index.");
                return null;
            }

            return AircraftVariants[CurrentAircraftVariantIndex];
        }

        #endregion

        #region Save and Load aircraft variants section

        /// <summary>
        /// Saves the list of aircraft variants to the AppData JSON cache.
        /// </summary>
        /// <param name="progressReporter">Optional progress reporter for UI status updates.</param>
        internal async Task SaveAircraftVariantsAsync(IProgress<string>? progressReporter = null)
        {
            if (AircraftVariants == null || AircraftVariants.Count == 0)
            {
                const string warningMessage = "Aircraft variants list is empty. Save operation aborted to prevent data loss.";
                await _log.WarningAsync(warningMessage);
                progressReporter?.Report(warningMessage);
                return;
            }

            string appDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                                   AppDomain.CurrentDomain.FriendlyName);
            string filePath = Path.Combine(appDataDirectory, "AircraftVariantsJSON.txt");

            try
            {
                bool success = await _cacheManager.TrySerializeToFileAsync(AircraftVariants, filePath);

                if (success)
                {
                    await _log.InfoAsync($"Successfully saved {AircraftVariants.Count} aircraft variants to '{filePath}'.");
                    progressReporter?.Report($"Aircraft variants saved ({AircraftVariants.Count} entries).");
                }
                else
                {
                    await _log.ErrorAsync("Failed to save aircraft variants.");
                    progressReporter?.Report("ERROR: Failed to save aircraft variants. See log for details.");
                }
            }
            catch (Exception ex)
            {
                await _log.ErrorAsync($"An unexpected error occurred while saving aircraft variants. Details: {ex.Message}", ex);
                progressReporter?.Report($"ERROR: An unexpected error occurred while saving aircraft variants: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads the list of aircraft variants from the AppData JSON cache.
        /// </summary>
        /// <param name="progressReporter">Optional progress reporter for UI status updates.</param>
        /// <returns><see langword="true"/> if loaded successfully; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> LoadAircraftVariantsAsync(IProgress<string>? progressReporter = null)
        {
            AircraftVariants = [];

            string appDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                                   AppDomain.CurrentDomain.FriendlyName);
            string filePath = Path.Combine(appDataDirectory, "AircraftVariantsJSON.txt");

            await _log.InfoAsync($"Attempting to load aircraft variants from local file: {filePath}");

            var (success, loadedVariants) = await _cacheManager.TryDeserializeFromFileAsync<List<AircraftVariant>>(filePath);

            if (success && loadedVariants?.Count > 0)
            {
                AircraftVariants = loadedVariants;
                CurrentAircraftVariantIndex = 0;
                await _log.InfoAsync($"Successfully loaded {AircraftVariants.Count} aircraft variants from local file.");
                progressReporter?.Report($"Aircraft variants loaded ({AircraftVariants.Count} entries).");
                return true;
            }

            const string warningMessage = "Aircraft variants file was missing, empty, or contained no valid data. Initializing with an empty list.";
            await _log.WarningAsync(warningMessage);
            progressReporter?.Report(warningMessage);
            return false;
        }

        #endregion
    }
}