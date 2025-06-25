using Microsoft.Win32;
using Newtonsoft.Json;

namespace P3D_Scenario_Generator
{
    /// <summary>
    /// Stores information for a user selected aircraft variant; title, display name, cruisespeed, 
    /// thumbnail image full path, whether it has floats, and whether it has wheels equivalent
    /// </summary>
    public class AircraftVariant
    {
        /// <summary>
        /// The title of the aircraft variant as recorded in the relevant variant [fltsim.?] section of an aircraft.cfg file
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The user editable name of the aircraft variant for display purposes on General tab of form
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The cruise speed in knots of the aircraft variant as recorded in the aircraft.cfg file
        /// </summary>
        public string CruiseSpeed { get; set; }

        /// <summary>
        /// Full path including filename of selected thumbnail.jpg file or empty string
        /// </summary>
        public string ThumbnailImagePath { get; set; }

        /// <summary>
        /// Whether the aircraft has floats, used to exclude takeoff/landing for water runways if selected aircraft doesn't have floats
        /// </summary>
        public bool HasFloats { get; set; }

        /// <summary>
        /// Whether the aircraft has wheels/scrapes/skids/skis, used to exclude takeoff from land based (non water) runways if selected 
        /// aircraft doesn't have them. Note landing possible with straight floats.
        /// </summary>
        public bool HasWheelsOrEquiv { get; set; }
    }

    /// <summary>
    /// Methods for user selection of an aircraft variant for a scenario. The selected variants are stored <br />
    /// in <see cref="AircraftVariants"/> in alphabetical order by display name. Currently selected variant <br />
    /// is <see cref="CurrentAircraftVariantIndex"/>. Methods organised in three sections: <br />
    ///     1. Prompting user for an aircraft variant and reading the aircraft information from P3D files <br />
    ///     2. Adding and deleting aircraft variants from <see cref="AircraftVariants"/> and changing <see cref="AircraftVariants"/> <br />
    ///     3. Providing a list of display names for the form and a formatted string of the aircraft variant details
    /// </summary>
    internal class Aircraft
    {
        /// <summary>
        /// List of aircraft variants maintained by user with current selection shown on General tab of form.
        /// Sorted alphabetically by display name.
        /// </summary>
        internal static List<AircraftVariant> AircraftVariants = [];

        /// <summary>
        /// Currently selected aircraft variant displayed on General tab of form
        /// </summary>
        internal static int CurrentAircraftVariantIndex { get; private set; }

        #region Prompt user for and read a new aircraft variant from P3D files section

        /// <summary>
        /// Prompts the user to select an aircraft variant thumbnail image and collects the aircraft title, 
        /// cruise speed, whether it has wheels or equivalent, and whether it has floats, from the aircraft.cfg file.
        /// Display name is initialised to be the aircraft variant title but can be changed by user subsequently.
        /// Adds new variant to <see cref="AircraftVariants"/>, maintaining display name alphabetical sort.
        /// </summary>
        /// <param name="simulatorVersion">Which of version 4, 5, 6 etc. Prepar3D is being used.</param>
        /// <returns>True if new aircraft variant selected and added to <see cref="AircraftVariants"/></returns>
        internal static string ChooseAircraftVariant(string simulatorVersion)
        {
            AircraftVariant aircraftVariant = new();

            string thumbnailPath = GetThumbnail(simulatorVersion);

            if (thumbnailPath != "")
            {
                aircraftVariant.ThumbnailImagePath = thumbnailPath;
                aircraftVariant.Title = GetAircraftTitle(thumbnailPath);
                aircraftVariant.DisplayName = aircraftVariant.Title;
                aircraftVariant.CruiseSpeed = GetAircraftCruiseSpeed(thumbnailPath);
                aircraftVariant.HasFloats = GetAircraftFloatsStatus(thumbnailPath);
                aircraftVariant.HasWheelsOrEquiv = GetAircraftWheelsStatus(thumbnailPath);
                if (AddAircraftVariant(aircraftVariant))
                    return aircraftVariant.DisplayName;
            }
            return "";
        }

        /// <summary>
        /// Prompts user to select an aircraft thumbnail.jpg file 
        /// </summary>
        /// <param name="simulatorVersion">Which of version 4, 5, 6 etc. Prepar3D is being used.</param>
        /// <returns>Full path including filename of selected thumbnail.jpg file or empty string</returns>
        internal static string GetThumbnail(string simulatorVersion)
        {
            // Exit if we can't get the simulator program folder key
            RegistryKey key = GetSimProgramFolderKey(simulatorVersion);
            if (key == null)
                return "";

            // Prompt user to select an aircraft variant thumbnail image
            using OpenFileDialog openFileDialog1 = new()
            {
                Title = "Select an aircraft thumbnail image file from within a \"texture.X\" folder (X could be any string), NOT a \"texture\" folder",
                DefaultExt = "jpg",
                Filter = "JPG files (*.jpg)|*.jpg|All files (*.*)|*.*",
                FilterIndex = 1,
                InitialDirectory = $"{key.GetValue("SetupPath")}SimObjects\\Airplanes",
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
        /// The simulator program folder is accessed via a registry key and used to set the initial directory
        /// when prompting user for an aircraft variant thumbnail image.
        /// </summary>
        /// <param name="simulatorVersion">Will be "4", or "5", or "6" etc and is specified by user on application settings tab</param>
        /// <returns>The simulator program folder registry key or null</returns>
        internal static RegistryKey GetSimProgramFolderKey(string simulatorVersion)
        {
            string keyString = $"Software\\Lockheed Martin\\Prepar3D v{simulatorVersion}";
            RegistryKey key;
            key = Registry.LocalMachine.OpenSubKey(keyString);
            if (key == null)
            {
                MessageBox.Show($"Problem encountered referencing key value {keyString}, " +
                    "check selected Simulator Version on Settings tab", Constants.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return key;
            }
            return key;
        }

        /// <summary>
        /// Gets the aircraft title from the aircraft.cfg (or equivalent) file
        /// </summary>
        /// <param name="thumbnailPath">Path to the user selected aircraft variant thumbnail image</param>
        /// <returns>The aircraft variant title string or an empty string</returns>
        internal static string GetAircraftTitle(string thumbnailPath)
        {
            // Get texture value, used to find correct string in aircraft.cfg for retrieving aircraft variant title
            string textureValue = GetTextureValue(thumbnailPath);

            // Now get aircraft variant title, assumes "title" always comes before "texture" in each variant section
            string aircraftCFG = GetAircraftCFG(thumbnailPath);
            using StringReader reader = new(aircraftCFG);
            string currentLine;
            string currentTitle = "";
            while ((currentLine = reader.ReadLine()) != null)
            {
                currentLine = currentLine.Trim();
                // Store latest encountered title which may or may not be the right one
                if (currentLine.StartsWith("title="))
                {
                    currentTitle = currentLine["title=".Length..^0].Trim();
                }
                // If the texture string is right we know we have the right title string
                if (currentLine.StartsWith("texture="))
                {
                    string currentTexture = currentLine["texture=".Length..^0].Trim();
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
        internal static string GetAircraftCFG(string thumbnailPath)
        {
            string textureFolderPath = Path.GetDirectoryName(thumbnailPath);
            string aircraftFolderPath = Path.GetDirectoryName(textureFolderPath);
            if (File.Exists($"{aircraftFolderPath}\\aircraft.cfg"))
                return File.ReadAllText($"{aircraftFolderPath}\\aircraft.cfg");
            else if (File.Exists($"{aircraftFolderPath}\\sim.cfg"))
                return File.ReadAllText($"{aircraftFolderPath}\\sim.cfg");
            else
            {
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
        internal static string GetAircraftCruiseSpeed(string thumbnailPath)
        {
            string aircraftCFG = GetAircraftCFG(thumbnailPath);
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
                    return cruiseSpeedWithoutAnyComment;
                }
            }
            return "";
        }

        /// <summary>
        /// Gets whether the aircraft is equipped with floats from the aircraft.cfg file
        /// </summary>
        /// <param name="thumbnailPath">From this is extracted the aircraft folder which contains the aircraft.cfg file</param>
        /// <returns>True if the aircraft is equipped with floats</returns>
        internal static bool GetAircraftFloatsStatus(string thumbnailPath)
        {
            // https://www.prepar3d.com/SDKv5/sdk/simulation_objects/aircraft_configuration_files.html#contact_points
            // says that in the section [contact_points] a line starting "point.X = 4, etc." indicates floats
            // but I've seen ski variants of DHC-2 with both point.X = 16 (skis?) and point.X = 4 (float) in the 
            // aircraft.cfg so if both present assume wheel (or equivalent) based plane not float based.
            // Note: P3D learning centre has 3 = Skid and no mention of 16
            string aircraftCFG = GetAircraftCFG(thumbnailPath);
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
                    int contactPointClass = int.Parse(splitOnCommaSign[0]);
                    if (contactPointClass == 4)
                        hasFloats = true;
                    else if (contactPointClass == 3 || contactPointClass == 16)
                        hasSkis = true;
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
        internal static bool GetAircraftWheelsStatus(string thumbnailPath)
        {
            // https://www.prepar3d.com/SDKv5/sdk/simulation_objects/aircraft_configuration_files.html#contact_points
            // says that in the section [contact_points] a line starting "point.X = 1 or 2 or 3" indicates wheels/scrapes/skids
            // Note:  a value of 16 might indicate skis though not documented in P3D v5 learning centre
            string aircraftCFG = GetAircraftCFG(thumbnailPath);
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
                    int contactPointClass = int.Parse(splitOnCommaSign[0]);
                    if ((contactPointClass >= 1 && contactPointClass <= 3) || contactPointClass == 16)
                    {
                        return true;
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
        internal static bool AddAircraftVariant(AircraftVariant aircraftVariant)
        {
            // If list empty just add new variant
            if (AircraftVariants == null || AircraftVariants.Count == 0)
            {
                AircraftVariants = [aircraftVariant];
                CurrentAircraftVariantIndex = 0;
                return true;
            }

            // Check whether variant is already in list and add if it isn't, checking on title since
            // user may have aready added it and changed display name, make the newly added variant current selected
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
        internal static void ChangeCurrentAircraftVariantIndex(string displayName)
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
        internal static bool DeleteAircraftVariant(string displayName)
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
        internal static void UpdateAircraftVariantDisplayName(string displayName)
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
        internal static List<string> GetAircraftVariantDisplayNames()
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
        internal static string SetTextBoxGeneralAircraftValues()
        {
            string aircraftValues = "";

            if (AircraftVariants == null || AircraftVariants.Count == 0)
                return aircraftValues;

            aircraftValues = "Title = ";
            aircraftValues += AircraftVariants[CurrentAircraftVariantIndex].Title;
            aircraftValues += " \nDisplay Name = ";
            aircraftValues += AircraftVariants[CurrentAircraftVariantIndex].DisplayName;
            aircraftValues += " \nCruise Speed = ";
            aircraftValues += AircraftVariants[CurrentAircraftVariantIndex].CruiseSpeed;
            aircraftValues += " \nHas Wheels/Scrapes/Skis = ";
            aircraftValues += AircraftVariants[CurrentAircraftVariantIndex].HasWheelsOrEquiv.ToString();
            aircraftValues += " \nHas Floats = ";
            aircraftValues += AircraftVariants[CurrentAircraftVariantIndex].HasFloats.ToString();
            return aircraftValues;
        }

        #endregion

        #region Save and Load aircraft variants section

        /// <summary>
        /// Save <see cref="AircraftVariants"/> to file in JSON format
        /// </summary>
        internal static void SaveAircraftVariants()
        {
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            directory = Path.Combine(directory, AppDomain.CurrentDomain.FriendlyName);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            directory = Path.Combine(directory, "AircraftVariantsJSON.txt");

            JsonSerializer serializer = new()
            {
                Formatting = Formatting.Indented
            };
            using StreamWriter sw = new(directory);
            using JsonWriter writer = new JsonTextWriter(sw);
            serializer.Serialize(writer, AircraftVariants);
        }

        /// <summary>
        /// Load <see cref="AircraftVariants"/> from file stored in JSON format
        /// </summary>
        internal static void LoadAircraftVariants()
        {
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            directory = Path.Combine(directory, AppDomain.CurrentDomain.FriendlyName);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            directory = Path.Combine(directory, "AircraftVariantsJSON.txt");

            string input;
            if (File.Exists(directory))
            {
                input = File.ReadAllText(directory);
                AircraftVariants = JsonConvert.DeserializeObject<List<AircraftVariant>>(input);
            }
        }

        #endregion
    }
}
