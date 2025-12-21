using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.Models;

namespace P3D_Scenario_Generator.Services
{
    /// <summary>
    /// Methods for user selection of an aircraft variant for a scenario. The selected variants are stored <br />
    /// in <see cref="AircraftVariants"/> in alphabetical order by display name. Currently selected variant <br />
    /// is <see cref="CurrentAircraftVariantIndex"/>. Methods organised in three sections: <br />
    ///     1. Prompting user for an aircraft variant and reading the aircraft information from P3D files <br />
    ///     2. Adding and deleting aircraft variants from <see cref="AircraftVariants"/> and changing <see cref="AircraftVariants"/> <br />
    ///     3. Providing a list of display names for the form and a formatted string of the aircraft variant details
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the Aircraft class.
    /// This constructor uses dependency injection to provide the necessary services.
    /// </remarks>
    /// <param name="log">The logging service instance.</param>
    /// <param name="cacheManager">The cache management service instance.</param>
    /// <param name="fileOps">The file operations service instance.</param>
    internal class Aircraft(Logger log, CacheManager cacheManager)
    {
        private readonly Logger _log = log;
        private readonly CacheManager _cacheManager = cacheManager;

        /// <summary>
        /// List of aircraft variants maintained by user with current selection shown on General tab of form.
        /// Sorted alphabetically by display name.
        /// </summary>
        internal List<AircraftVariant> AircraftVariants { get; private set; } = [];

        /// <summary>
        /// Currently selected aircraft variant displayed on General tab of form
        /// </summary>
        internal int CurrentAircraftVariantIndex { get; private set; } = -1;

        #region Prompt user for and read a new aircraft variant from P3D files section

        /// <summary>
        /// Prompts the user to select an aircraft variant thumbnail image and collects the aircraft title, 
        /// cruise speed, whether it has wheels or equivalent, and whether it has floats, from the aircraft.cfg file.
        /// Display name is initialised to be the aircraft variant title but can be changed by user subsequently.
        /// Adds new variant to <see cref="AircraftVariants"/>, maintaining display name alphabetical sort.
        /// </summary>
        /// <param name="formData">The scenario form data containing paths like the P3D install directory.</param>
        /// <returns>The display name of the new aircraft variant, or an empty string if none was selected or an error occurred.</returns>
        internal async Task<string> ChooseAircraftVariantAsync(ScenarioFormData formData)
        {
            AircraftVariant aircraftVariant = new();

            string thumbnailPath = GetThumbnail(formData);

            if (thumbnailPath != "")
            {
                aircraftVariant.ThumbnailImagePath = thumbnailPath;
                aircraftVariant.Title = await GetAircraftTitleAsync(thumbnailPath);
                aircraftVariant.DisplayName = aircraftVariant.Title;
                aircraftVariant.CruiseSpeed = await GetAircraftCruiseSpeedAsync(thumbnailPath);
                aircraftVariant.HasFloats = await GetAircraftFloatsStatusAsync(thumbnailPath);
                aircraftVariant.HasWheelsOrEquiv = await GetAircraftWheelsStatusAsync(thumbnailPath);

                if (AddAircraftVariant(aircraftVariant))
                {
                    return aircraftVariant.DisplayName;
                }
            }
            return "";
        }

        /// <summary>
        /// Prompts user to select an aircraft thumbnail.jpg file 
        /// </summary>
        /// <param name="formData">The scenario form data containing paths like the P3D install directory.</param>
        /// <returns>Full path including filename of selected thumbnail.jpg file or empty string</returns>
        internal static string GetThumbnail(ScenarioFormData formData)
        {
            // Prompt user to select an aircraft variant thumbnail image
            using OpenFileDialog openFileDialog1 = new()
            {
                Title = "Select an aircraft thumbnail image file from within a \"texture.X\" folder (X could be any string), NOT a \"texture\" folder",
                DefaultExt = "jpg",
                Filter = "JPG files (*.jpg)|*.jpg|All files (*.*)|*.*",
                FilterIndex = 1,
                InitialDirectory = Path.Combine(formData.P3DProgramInstall, "SimObjects\\Airplanes"),
                RestoreDirectory = false
            };

            // Check that the user has selected a non AI aircraft variant thumbnail image
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string thumbnailPath = openFileDialog1.FileName;
                // Check user has selected a file "thumbnail.jpg"
                if (!Path.GetFileName(thumbnailPath).Equals("thumbnail.jpg", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Please select a \"thumbnail.jpg\" file (case insensitive)", Constants.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return "";
                }

                // Check the user hasn't selected from a "texture" folder instead of a "texture.X" folder where X is the 
                // name of the aircraft variant texture
                if (GetTextureValue(thumbnailPath) == "")
                {
                    MessageBox.Show($"Not a valid variant, please select from a texture folder containing a \".\"",
                        Constants.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return "";
                }

                // Check whether user has selected AI aircraft
                string textureFolderPath = Path.GetDirectoryName(thumbnailPath);
                string aircraftFolderPath = Path.GetDirectoryName(textureFolderPath);
                if (Directory.GetDirectories(aircraftFolderPath, "panel*").Length == 0)
                {
                    MessageBox.Show($"This is an AI aircraft, there is no panel folder in {aircraftFolderPath}",
                        Constants.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return "";
                }

                return openFileDialog1.FileName;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Gets the aircraft title from the aircraft.cfg (or equivalent) file
        /// </summary>
        /// <param name="thumbnailPath">Path to the user selected aircraft variant thumbnail image</param>
        /// <returns>The aircraft variant title string or an empty string</returns>
        internal async Task<string> GetAircraftTitleAsync(string thumbnailPath)
        {
            // Get texture value, used to find correct string in aircraft.cfg for retrieving aircraft variant title
            string textureValue = GetTextureValue(thumbnailPath);

            // Now get aircraft variant title, assumes "title" always comes before "texture" in each variant section
            string aircraftCFG = await GetAircraftCFGAsync(thumbnailPath);
            using StringReader reader = new(aircraftCFG);
            string currentLine;
            string currentTitle = "";
            while ((currentLine = reader.ReadLine()) != null)
            {
                currentLine = currentLine.Trim();
                // Store latest encountered title which may or may not be the right one
                if (currentLine.StartsWith("title="))
                {
                    currentTitle = currentLine["title=".Length..].Trim();
                }
                // If the texture string is right we know we have the right title string
                if (currentLine.StartsWith("texture="))
                {
                    string currentTexture = currentLine["texture=".Length..].Trim();
                    if (currentTexture == textureValue)
                        return currentTitle;
                }
            }
            return "";
        }

        /// <summary>
        /// Looks for the aircraft.cfg file associated with the user selected aircraft variant and reads
        /// it into a text string. Will try to read sim.cfg as an alternative else returns an empty string
        /// and advises user
        /// </summary>
        /// <param name="thumbnailPath">Path to the user selected aircraft variant thumbnail image</param>
        /// <returns>Text string containing contents of aircraft.cfg file (or equivalent) otherwise empty string</returns>
        internal async Task<string> GetAircraftCFGAsync(string thumbnailPath)
        {
            string textureFolderPath = Path.GetDirectoryName(thumbnailPath);
            string aircraftFolderPath = Path.GetDirectoryName(textureFolderPath);

            if (FileOps.FileExists($"{aircraftFolderPath}\\aircraft.cfg"))
            {
                return FileOps.ReadAllText($"{aircraftFolderPath}\\aircraft.cfg");
            }
            else if (FileOps.FileExists($"{aircraftFolderPath}\\sim.cfg"))
            {
                return FileOps.ReadAllText($"{aircraftFolderPath}\\sim.cfg");
            }
            else
            {
                await _log.ErrorAsync($"Unable to locate aircraft.cfg or sim.cfg for selected aircraft variant at '{aircraftFolderPath}'.");
                MessageBox.Show($"Unable to locate aircraft.cfg or sim.cfg for selected aircraft variant, " +
                    "rename equivalent file and advise developer", Constants.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "";
            }
        }

        /// <summary>
        /// Each variant [fltsim.?] section in aircraft.cfg has a line "texture=X". This method retrieves the X
        /// for the user selected variant from the fullpath of the thumbnail image
        /// </summary>
        /// <param name="thumbnailPath">From this the texture value is extracted, it includes the thumbnail filename</param>
        /// <returns>The texture value</returns>
        internal static string GetTextureValue(string thumbnailPath)
        {
            // Get full path of texture folder that contains user selected aircraft variant thumbnail image
            string textureFolderPath = Path.GetDirectoryName(thumbnailPath);

            // Get the texture folder
            string textureFolder = Path.GetFileName(textureFolderPath);

            // Split on '.' on assumption all P3D texture folders contain the period character i.e. "texture.X"
            string[] splitOnPeriod = textureFolder.Split('.');

            // 2nd string is the X in "texture=X" within relevant section of aircraft.cfg
            if (splitOnPeriod.Length == 1)
                return "";
            else
                return splitOnPeriod[1];
        }

        /// <summary>
        /// Gets the aircraft cruise speed from the aircraft.cfg file
        /// </summary>
        /// <param name="thumbnailPath">From this is extracted the aircraft folder which contains the aircraft.cfg file</param>
        /// <returns>The aircraft variant cruise speed string or an empty string</returns>
        internal async Task<double> GetAircraftCruiseSpeedAsync(string thumbnailPath)
        {
            string aircraftCFG = await GetAircraftCFGAsync(thumbnailPath);
            using StringReader reader = new(aircraftCFG);
            string currentLine;
            while ((currentLine = reader.ReadLine()) != null)
            {
                currentLine = currentLine.Trim();
                if (currentLine.StartsWith("cruise_speed"))
                {
                    string[] splitOnEqualsSign = currentLine.Split("=");
                    string cruiseSpeed = splitOnEqualsSign[1].Trim();
                    string[] splitOnCommentIndicator = cruiseSpeed.Split("//"); // Remove any comment at end of line
                    string cruiseSpeedWithoutAnyComment = splitOnCommentIndicator[0].Trim();
                    const double minCruiseSpeed = 0.0;
                    const double maxCruiseSpeed = Constants.PlausibleMaxCruiseSpeedKnots;
                    if (ParsingHelpers.TryParseDouble(
                        cruiseSpeedWithoutAnyComment,
                        "Aircraft cruise speed",
                        minCruiseSpeed,
                        maxCruiseSpeed,
                        out double cruiseSpeedOut,
                        out string validationMessage,
                        "knots"))
                    {
                        return cruiseSpeedOut;
                    }
                    else
                    {
                        return 0.0;
                    }
                }
            }
            return 0.0;
        }

        /// <summary>
        /// Gets whether the aircraft is equipped with floats from the aircraft.cfg file
        /// </summary>
        /// <param name="thumbnailPath">From this is extracted the aircraft folder which contains the aircraft.cfg file</param>
        /// <returns>True if the aircraft is equipped with floats</returns>
        internal async Task<bool> GetAircraftFloatsStatusAsync(string thumbnailPath)
        {
            string aircraftCFG = await GetAircraftCFGAsync(thumbnailPath);
            using StringReader reader = new(aircraftCFG);
            string currentLine;
            bool hasFloats = false, hasSkis = false;
            while ((currentLine = reader.ReadLine()) != null)
            {
                currentLine = currentLine.Trim();
                if (currentLine.StartsWith("point."))
                {
                    string[] splitOnEqualsSign = currentLine.Split("=");
                    string rhsEqualSign = splitOnEqualsSign[1].Trim();
                    string[] splitOnCommaSign = rhsEqualSign.Split(',');
                    if (int.TryParse(splitOnCommaSign[0], out int contactPointClass))
                    {
                        if (contactPointClass == 4)
                            hasFloats = true;
                        else if (contactPointClass == 3 || contactPointClass == 16)
                            hasSkis = true;
                    }
                }
            }
            if (hasFloats && !hasSkis)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Gets whether the aircraft is equipped with wheels/scrapes/skids from the aircraft.cfg file
        /// </summary>
        /// <param name="thumbnailPath">From this is extracted the aircraft folder which contains the aircraft.cfg file</param>
        /// <returns>True if the aircraft is equipped with wheels/scrapes/skids/skis</returns>
        internal async Task<bool> GetAircraftWheelsStatusAsync(string thumbnailPath)
        {
            string aircraftCFG = await GetAircraftCFGAsync(thumbnailPath);
            using StringReader reader = new(aircraftCFG);
            string currentLine;
            while ((currentLine = reader.ReadLine()) != null)
            {
                currentLine = currentLine.Trim();
                if (currentLine.StartsWith("point."))
                {
                    string[] splitOnEqualsSign = currentLine.Split("=");
                    string rhsEqualsSign = splitOnEqualsSign[1].Trim();
                    string[] splitOnCommaSign = rhsEqualsSign.Split(',');
                    if (int.TryParse(splitOnCommaSign[0], out int contactPointClass))
                    {
                        if (contactPointClass >= 1 && contactPointClass <= 3 || contactPointClass == 16)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Adds aircraft variant to <see cref="AircraftVariants"/> ensuring no duplicates, maintains list
        /// in alphabetical order on display name, adjusts <see cref="CurrentAircraftVariantIndex"/>
        /// </summary>
        /// <param name="aircraftVariant">The aircraft variant to be added</param>
        /// <returns>True if new aircraft variant added to <see cref="AircraftVariants"/></returns>
        internal bool AddAircraftVariant(AircraftVariant aircraftVariant)
        {
            // If list empty just add new variant
            if (AircraftVariants == null || AircraftVariants.Count == 0)
            {
                AircraftVariants = [aircraftVariant];
                CurrentAircraftVariantIndex = 0;
                return true;
            }

            // Check whether variant is already in list and add if it isn't, checking on title since
            // user may have already added it and changed display name, make the newly added variant current selected
            int variantIndex = AircraftVariants.FindIndex(aircraft => aircraft.Title == aircraftVariant.Title);
            if (variantIndex == -1)
            {
                AircraftVariants.Add(aircraftVariant);
                AircraftVariants.Sort((x, y) => x.DisplayName.CompareTo(y.DisplayName));
                ChangeCurrentAircraftVariantIndex(aircraftVariant.DisplayName);
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Manage list of aircraft variants section

        /// <summary>
        /// Reset <see cref="CurrentAircraftVariantIndex"/> to the instance of <see cref="AircraftVariant"/>
        /// with displayName. If displayName not found in <see cref="AircraftVariants"/> then does nothing.
        /// </summary>
        /// <param name="displayName">The display name of the new instance to be set as <see cref="CurrentAircraftVariantIndex"/></param>
        internal void ChangeCurrentAircraftVariantIndex(string displayName)
        {
            AircraftVariant aircraftVariant = AircraftVariants.Find(aircraft => aircraft.DisplayName == displayName);
            if (aircraftVariant != null)
                CurrentAircraftVariantIndex = AircraftVariants.IndexOf(aircraftVariant);
        }

        /// <summary>
        /// Deletes the aircraft variant with displayName from <see cref="AircraftVariants"/> and reduces
        /// <see cref="CurrentAircraftVariantIndex"/> by 1, i.e. the prior item in alphabetical list of display names
        /// </summary>
        /// <param name="displayName">The display name of the instance to be deleted from <see cref="AircraftVariants"/></param>
        /// <returns>True if aircraft variant with displayName deleted, false otherwise</returns>
        internal bool DeleteAircraftVariant(string displayName)
        {
            if (AircraftVariants != null && AircraftVariants.Count > 0)
            {
                AircraftVariant aircraftVariant = AircraftVariants.Find(aircraft => aircraft.DisplayName == displayName);
                if (aircraftVariant != null)
                {
                    // Do the deletion
                    AircraftVariants.Remove(aircraftVariant);

                    // Adjust the selected aircraft variant
                    CurrentAircraftVariantIndex--;

                    return true;
                }
                else
                    return false;
            }
            return false;
        }

        /// <summary>
        /// Changes the display name of <see cref="CurrentAircraftVariantIndex"/> aircraft variant in
        /// <see cref="AircraftVariants"/> to displayName. Resorts <see cref="AircraftVariants"/> and
        /// updates <see cref="CurrentAircraftVariantIndex"/>. If <see cref="AircraftVariants"/> is null
        /// or empty then does nothing.
        /// </summary>
        /// <param name="displayName">The new display name</param>
        internal void UpdateAircraftVariantDisplayName(string displayName)
        {
            // Make sure new name is not already in use and then change in current aircraft variant
            if (AircraftVariants != null && AircraftVariants.FindAll(aircraft => aircraft.DisplayName == displayName).Count == 0)
            {
                AircraftVariants[CurrentAircraftVariantIndex].DisplayName = displayName;
                AircraftVariants.Sort((x, y) => x.DisplayName.CompareTo(y.DisplayName));
                ChangeCurrentAircraftVariantIndex(displayName);
            }
        }

        #endregion

        #region Provide aircraft variant display information section

        /// <summary>
        /// Get a sorted list of the aircraft variant display names
        /// </summary>
        /// <returns>Alphabetically sorted list of the aircraft variant display names</returns>
        internal List<string> GetAircraftVariantDisplayNames()
        {
            List<string> names = [];

            if (AircraftVariants == null)
                return names;

            for (int i = 0; i < AircraftVariants.Count; i++)
                names.Add(AircraftVariants[i].DisplayName);
            return names;
        }

        /// <summary>
        /// Builds a formatted string showing the <see cref="CurrentAircraftVariantIndex"/> aircraft variant in
        /// <see cref="AircraftVariants"/>.
        /// </summary>
        /// <returns>Formatted string showing title, display name, cruise speed, whether aircraft has wheels (equivalent) and/or floats</returns>
        internal string SetTextBoxGeneralAircraftValues()
        {
            string aircraftValues = "";

            if (AircraftVariants == null || AircraftVariants.Count == 0 || CurrentAircraftVariantIndex < 0 || CurrentAircraftVariantIndex >= AircraftVariants.Count)
                return aircraftValues;

            AircraftVariant currentVariant = AircraftVariants[CurrentAircraftVariantIndex];

            aircraftValues = "Title = ";
            aircraftValues += currentVariant.Title;
            aircraftValues += " \nDisplay Name = ";
            aircraftValues += currentVariant.DisplayName;
            aircraftValues += " \nCruise Speed = ";
            aircraftValues += currentVariant.CruiseSpeed;
            aircraftValues += " \nHas Wheels/Scrapes/Skis = ";
            aircraftValues += currentVariant.HasWheelsOrEquiv.ToString();
            aircraftValues += " \nHas Floats = ";
            aircraftValues += currentVariant.HasFloats.ToString();
            return aircraftValues;
        }

        /// <summary>
        /// Retrieves the currently selected aircraft variant.
        /// Handles cases where no variant is selected or the index is invalid.
        /// </summary>
        /// <returns>The selected AircraftVariant, or null if no valid variant is currently selected.</returns>
        public async Task<AircraftVariant> GetCurrentVariantAsync()
        {
            if (AircraftVariants == null || CurrentAircraftVariantIndex < 0 || CurrentAircraftVariantIndex >= AircraftVariants.Count)
            {
                await _log.WarningAsync("Attempted to retrieve current aircraft variant with no variants loaded or an invalid index.");
                return null; // No valid aircraft selected or available
            }
            return AircraftVariants[CurrentAircraftVariantIndex];
        }

        #endregion

        #region Save and Load aircraft variants section

        /// <summary>
        /// Saves the current list of aircraft variants to a file in JSON format using CacheManagerAsync.
        /// The save operation is skipped if the list of variants is empty to prevent overwriting
        /// a valid file with an empty list.
        /// </summary>
        /// <param name="progressReporter">Optional. Can be <see langword="null"/> if progress or error reporting to the UI is not required.</param>
        internal async Task SaveAircraftVariantsAsync(IProgress<string> progressReporter)
        {
            // Do not save if the list is empty to prevent overwriting a valid file with an empty one.
            if (AircraftVariants == null || AircraftVariants.Count == 0)
            {
                string warningMessage = "Aircraft variants list is empty. Save operation aborted to prevent data loss.";
                await _log.WarningAsync(warningMessage).ConfigureAwait(false);
                progressReporter?.Report(warningMessage);
                return;
            }

            string appDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                                   AppDomain.CurrentDomain.FriendlyName);
            string filePath = Path.Combine(appDataDirectory, "AircraftVariantsJSON.txt");

            // Use a try/catch block to handle exceptions from the asynchronous operation.
            // The asynchronous call is awaited, which allows the calling thread to remain responsive.
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
                    // The CacheManagerAsync logs the specific error, so we can provide a general message here.
                    await _log.ErrorAsync("Failed to save aircraft variants.").ConfigureAwait(false);
                    progressReporter?.Report("ERROR: Failed to save aircraft variants. See log for details.");
                }
            }
            catch (Exception ex)
            {
                // This catch block will handle any exceptions not caught within the CacheManagerAsync class.
                await _log.ErrorAsync($"An unexpected error occurred while saving aircraft variants. Details: {ex.Message}", ex);
                progressReporter?.Report($"ERROR: An unexpected error occurred while saving aircraft variants: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads the list of aircraft variants from a file stored in JSON format using CacheManagerAsync.
        /// It first attempts to load a local user-created version. If the local file is not found,
        /// it falls back to an embedded resource.
        /// </summary>
        /// <param name="progressReporter">Optional. Can be <see langword="null"/> if progress or error reporting to the UI is not required.</param>
        /// <returns>A <see cref="Task{TResult}"/> that returns <see langword="true"/> if aircraft variants were successfully loaded, <see langword="false"/> otherwise.</returns>
        internal async Task<bool> LoadAircraftVariantsAsync(IProgress<string> progressReporter = null)
        {
            AircraftVariants = [];

            string appDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                                   AppDomain.CurrentDomain.FriendlyName);
            string filePath = Path.Combine(appDataDirectory, "AircraftVariantsJSON.txt");

            // First, try to load from the local file using the asynchronous CacheManager.
            await _log.InfoAsync($"Attempting to load aircraft variants from local file: {filePath}");

            // We await the deserialization, which returns a tuple with a success boolean and the data.
            var (success, loadedVariants) = await _cacheManager.TryDeserializeFromFileAsync<List<AircraftVariant>>(filePath);

            if (success)
            {
                AircraftVariants = loadedVariants;
                await _log.InfoAsync($"Successfully loaded {AircraftVariants.Count} aircraft variants from local file.");
                progressReporter?.Report($"Aircraft variants loaded ({AircraftVariants.Count} entries).");
                return true;
            }
            else
            {
                AircraftVariants = [];
                string warningMessage = "Aircraft variants file was empty or contained no valid data. Initializing with an empty list.";
                await _log.WarningAsync(warningMessage);
                progressReporter?.Report(warningMessage);
                return false;
            }
        }

        #endregion
    }
}
