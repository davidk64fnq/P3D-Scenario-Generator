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
        /// The cruise speed in knots of the aircraft variant as recorded in the aircraft.cfg file
        /// </summary>
        public string CruiseSpeed { get; set; }

        /// <summary>
        /// Full path including filename of selected thumbnail.jpg file or empty string
        /// </summary>
        public string ThumbnailImagePath { get; set; }
    }

    /// <summary>
    /// Methods for user selection of an aircraft variant for a scenario
    /// </summary>
    internal class Aircraft
    {
        /// <summary>
        /// Prompts the user to select an aircraft variant thumbnail image and collects the aircraft title and 
        /// cruise speed from the aircraft.cfg file
        /// </summary>
        /// <returns>An instance of <see cref="AircraftVariant"/> containing the aircraft variant information</returns>
        static internal AircraftVariant ChooseAircraftVariant(string simulatorVersion)
        {
            AircraftVariant aircraftVariant = new();

            string thumbnailPath = GetThumbnail(simulatorVersion);
            if (thumbnailPath != "")
            {
                aircraftVariant.ThumbnailImagePath = thumbnailPath;
                aircraftVariant.Title = GetAircraftTitle(thumbnailPath);
                aircraftVariant.CruiseSpeed = GetAircraftCruiseSpeed(thumbnailPath);
            }
            return aircraftVariant;
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
