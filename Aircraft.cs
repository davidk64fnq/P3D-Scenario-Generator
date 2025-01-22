using Microsoft.Win32;

namespace P3D_Scenario_Generator
{
    /// <summary>
    /// Stores information for a user selected aircraft variant, title, cruisespeed, and thumbnail image full path
    /// </summary>
    public class AircraftVariant
    {
        /// <summary>
        /// The title of the aircraft variant as recorded in the relevant variant [fltsim.?] section of aircraft.cfg file
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
        /// Whether the aircraft has wheels/scrapes/skids, used to exclude takeoff from land based (non water) runways if selected 
        /// aircraft doesn't have them
        /// </summary>
        public bool HasWheelsOrEquiv { get; set; }
    }

    /// <summary>
    /// Methods for user selection of an aircraft variant for a scenario
    /// </summary>
    internal class Aircraft
    {
        /// <summary>
        /// List of aircraft variants maintained by user with current selection shown on General tab of form
        /// </summary>
        internal static List<AircraftVariant> AircraftVariants {  get; set; }

        /// <summary>
        /// Currently selected aircraft variant displayed on General tab of form
        /// </summary>
        internal static int CurrentAircraftVariantIndex;

        /// <summary>
        /// Prompts the user to select an aircraft variant thumbnail image and collects the aircraft title and 
        /// cruise speed from the aircraft.cfg file
        /// </summary>
        /// <returns>True if new aircraft variant selected and added to <see cref="AircraftVariants"/></returns>
        static internal bool ChooseAircraftVariant(string simulatorVersion)
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
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Adds aircraft variant to <see cref="AircraftVariants"/> ensuring no duplicates
        /// </summary>
        /// <param name="aircraftVariant">The aircraft variant to be added</param>
        /// <returns>True if new aircraft variant added to <see cref="AircraftVariants"/></returns>
        static internal bool AddAircraftVariant(AircraftVariant aircraftVariant)
        {
            // If list empty just add new variant
            if (AircraftVariants == null || AircraftVariants.Count == 0)
            {
                AircraftVariants = [aircraftVariant];
                CurrentAircraftVariantIndex = 0;
                return true;
            }

            // Check whether variant is already in list and add if it isn't
            int variantIndex = AircraftVariants.FindIndex(aircraft => aircraft.Title == aircraftVariant.Title);
            if (variantIndex == -1)
            {
                AircraftVariants.Add(aircraftVariant);
                CurrentAircraftVariantIndex = AircraftVariants.Count - 1;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Get a sorted list of the aircraft variant display names
        /// </summary>
        /// <returns>Sorted list of the aircraft variant display names</returns>
        static internal List<string> GetAircraftVariantDisplayNames()
        {
            List<string> names = [];

            for (int i = 0; i < AircraftVariants.Count; i++)
            {
                names.Add(AircraftVariants[i].DisplayName);
            }
            names.Sort();
            return names;
        }

        /// <summary>
        /// Reset <see cref="CurrentAircraftVariantIndex"/> to the instance of <see cref="AircraftVariant"/>
        /// with newFavouriteAircraft
        /// </summary>
        /// <param name="newFavouriteAircraft">The name of the new instance to be set as <see cref="CurrentAircraftVariantIndex"/></param>
        static internal void ChangeCurrentAircraftVariantIndex(string newFavouriteAircraft)
        {
            AircraftVariant aircraftVariant = AircraftVariants.Find(aircraft => aircraft.DisplayName == newFavouriteAircraft);
            CurrentAircraftVariantIndex = AircraftVariants.IndexOf(aircraftVariant);
        }

        /// <summary>
        /// Gets the aircraft title from the aircraft.cfg file
        /// </summary>
        /// <returns>The aircraft variant title string or an empty string</returns>
        static internal string GetAircraftTitle(string thumbnailPath)
        {
            // Texture value used to find correct string in aircraft.cfg for retrieving aircraft variant title
            string textureValue = GetTextureValue(thumbnailPath);

            // Now get aircraft variant title, assumes "title" always comes before "texture" in each variant section
            string textureFolderPath = Path.GetDirectoryName(thumbnailPath);
            string aircraftFolderPath = Path.GetDirectoryName(textureFolderPath);
            string aircraftCFG = File.ReadAllText($"{aircraftFolderPath}\\aircraft.cfg");
            using StringReader reader = new(aircraftCFG);
            string currentLine;
            string currentTitle = "";
            while ((currentLine = reader.ReadLine()) != null)
            {
                currentLine = currentLine.Trim();
                if (currentLine.StartsWith("title="))
                {
                    currentTitle = currentLine["title=".Length..^0].Trim();
                }
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
        /// Each variant [fltsim.?] section in aircraft.cfg has a line "texture=X". This method retrieves the X
        /// for the user selected variant from the fullpath of the thumbnail image
        /// </summary>
        /// <param name="thumbnailPath">From this the texture value is extracted, it includes the thumbnail filename</param>
        /// <returns>The texture value</returns>
        static internal string GetTextureValue(string thumbnailPath)
        {
            // Get full path of texture folder that contains user selected aircraft variant thumbnail image
            string textureFolderPath = Path.GetDirectoryName(thumbnailPath);

            // Get the texture folder
            string textureFolder = Path.GetFileName(textureFolderPath);

            // Split on '.' on assumption all P3D texture folders contain the period character i.e. "texture.X"
            string[] strings = textureFolder.Split('.');

            // 2nd string is the X in "texture=X" within relevant section of aircraft.cfg
            return strings[1];
        }

        /// <summary>
        /// Gets the aircraft cruise speed from the aircraft.cfg file
        /// </summary>
        /// <param name="thumbnailPath">From this is extracted the aircraft folder which contains the aircraft.cfg file</param>
        /// <returns>The aircraft variant cruise speed string or an empty string</returns>
        static internal string GetAircraftCruiseSpeed(string thumbnailPath)
        {
            string textureFolderPath = Path.GetDirectoryName(thumbnailPath);
            string aircraftFolderPath = Path.GetDirectoryName(textureFolderPath);
            string aircraftCFG = File.ReadAllText($"{aircraftFolderPath}\\aircraft.cfg");
            using StringReader reader = new(aircraftCFG);
            string currentLine;
            while ((currentLine = reader.ReadLine()) != null)
            {
                currentLine = currentLine.Trim();
                if (currentLine.StartsWith("cruise_speed"))
                {
                    string[] words1 = currentLine.Split("=");
                    string[] words2 = words1[1].Trim().Split(null); // Remove any comment at end of line?
                    return words2[0].Trim();
                }
            }
            return "";
        }

        /// <summary>
        /// Gets whether the aircraft is equipped with floats from the aircraft.cfg file
        /// </summary>
        /// <param name="thumbnailPath">From this is extracted the aircraft folder which contains the aircraft.cfg file</param>
        /// <returns>True if the aircraft is equipped with floats</returns>
        static internal bool GetAircraftFloatsStatus(string thumbnailPath)
        {
            // https://www.prepar3d.com/SDKv5/sdk/simulation_objects/aircraft_configuration_files.html#contact_points
            // says that in the section [contact_points] a line starting "point.X = 4, etc." indicates floats
            string textureFolderPath = Path.GetDirectoryName(thumbnailPath);
            string aircraftFolderPath = Path.GetDirectoryName(textureFolderPath);
            string aircraftCFG = File.ReadAllText($"{aircraftFolderPath}\\aircraft.cfg");
            using StringReader reader = new(aircraftCFG);
            string currentLine;
            while ((currentLine = reader.ReadLine()) != null)
            {
                currentLine = currentLine.Trim();
                if (currentLine.StartsWith("point."))
                {
                    string[] words1 = currentLine.Split("=");
                    string[] words2 = words1[1].Trim().Split(',');
                    int contactPointClass = int.Parse(words2[0]);
                    if (contactPointClass == 4)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Gets whether the aircraft is equipped with wheels/scrapes/skids from the aircraft.cfg file
        /// </summary>
        /// <param name="thumbnailPath">From this is extracted the aircraft folder which contains the aircraft.cfg file</param>
        /// <returns>True if the aircraft is equipped with wheels/scrapes/skids</returns>
        static internal bool GetAircraftWheelsStatus(string thumbnailPath)
        {
            // https://www.prepar3d.com/SDKv5/sdk/simulation_objects/aircraft_configuration_files.html#contact_points
            // says that in the section [contact_points] a line starting "point.X = 1 or 2 or 3, etc." indicates wheels/scrapes/skids
            string textureFolderPath = Path.GetDirectoryName(thumbnailPath);
            string aircraftFolderPath = Path.GetDirectoryName(textureFolderPath);
            string aircraftCFG = File.ReadAllText($"{aircraftFolderPath}\\aircraft.cfg");
            using StringReader reader = new(aircraftCFG);
            string currentLine;
            while ((currentLine = reader.ReadLine()) != null)
            {
                currentLine = currentLine.Trim();
                if (currentLine.StartsWith("point."))
                {
                    string[] words1 = currentLine.Split("=");
                    string[] words2 = words1[1].Trim().Split(',');
                    int contactPointClass = int.Parse(words2[0]);
                    if (contactPointClass >= 1 && contactPointClass <= 3)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Prompts user to select an aircraft thumbnail.jpg file 
        /// </summary>
        /// <returns>Full path including filename of selected thumbnail.jpg file or empty string</returns>
        static internal string GetThumbnail(string simulatorVersion)
        {
            RegistryKey key = GetSimProgramFolderKey(simulatorVersion);
            if (key == null)
                return "";
            using OpenFileDialog openFileDialog1 = new()
            {
                Title = "Aircraft thumbnail image file location",
                DefaultExt = "jpg",
                Filter = "JPG files (*.jpg)|*.jpg|All files (*.*)|*.*",
                FilterIndex = 1,
                InitialDirectory = $"{key.GetValue("SetupPath")}SimObjects\\Airplanes",
                RestoreDirectory = false
            };
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string thumbnailPath = openFileDialog1.FileName;
                // Check user has selected a file "thumbnail.jpg"
                if (!System.IO.Path.GetFileName(thumbnailPath).Equals("thumbnail.jpg", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Please select a \"thumbnail.jpg\" file (case insensitive)", Con.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return "";
                }

                // Check whether user has selected AI aircraft
                string textureFolderPath = Path.GetDirectoryName(thumbnailPath);
                string aircraftFolderPath = Path.GetDirectoryName(textureFolderPath);
                if (Directory.GetDirectories(aircraftFolderPath, "panel*").Length == 0)
                {
                    MessageBox.Show($"This is an AI aircraft, there is no panel folder in {aircraftFolderPath}", 
                        Con.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        /// <returns>The simulator program folder registry key</returns>
        static internal RegistryKey GetSimProgramFolderKey(string simulatorVersion)
        {
            string keyString = $"Software\\Lockheed Martin\\Prepar3D v{simulatorVersion}";
            RegistryKey key;
            key = Registry.LocalMachine.OpenSubKey(keyString);
            if (key == null)
            {
                MessageBox.Show($"Problem encountered referencing key value {keyString}, " +
                    "check selected Simulator Version on Settings tab", Con.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return key;
            }
            return key;
        }
    }
}
