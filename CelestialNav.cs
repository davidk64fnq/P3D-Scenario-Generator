using OfficeOpenXml;
using System.Text;

namespace P3D_Scenario_Generator
{
    public class Star(string s1, string s2, string s3, string s4, string s5, string s6, string s7, double d8, double d9, double d10, double d11, double d12, double d13, double d14)
    {
        public string constellation = s1;
        public string id = s2;
        public string connectedId = s3;
        public string starNumber = s4;
        public string starName = s5;
        public string wikiLink = s6;
        public string bayer = s7;
        public double raH = d8;
        public double raM = d9;
        public double raS = d10;
        public double decD = d11;
        public double decM = d12;
        public double decS = d13;
        public double visMag = d14;
    }

    class CelestialNav
    {
        private static readonly List<Star> stars = [];
        internal static List<string> navStarNames = [];
        internal static int noStars = 0;
        internal static double[,] ariesGHAd = new double[3, 24];
        internal static double[,] ariesGHAm = new double[3, 24];
        internal static double[] starsSHAd = new double[57];
        internal static double[] starsSHAm = new double[57];
        internal static double[] starsDECd = new double[57];
        internal static double[] starsDECm = new double[57];
        internal static double midairStartHdg;
        internal static double midairStartLat;
        internal static double midairStartLon;
        internal static double celestialImageNorth;
        internal static double celestialImageEast;
        internal static double celestialImageSouth;
        internal static double celestialImageWest;

        static internal bool SetCelestial()
        {
            Runway.destRwy = Runway.GetRandomRunway();
            SetCelestialStartLocation();
            if (GetAlmanacData() && InitStars() && CreateStarsDat() && SetCelestialSextantHTML() && SetCelestialSextantJS() && SetCelestialSextantCSS())
            {
                Common.SetOverviewImage();
                Common.SetLocationImage();
                return true;
            }
            return false;
        }

        static internal void SetCelestialMapEdges(double midairStartLat, double midairStartLon, double distance)
        {
            double dFinishLat = 0; 
            double dFinishLon = 0;
            double distFeet = distance * 1.1 * Con.feetInNM;
            MathRoutines.AdjCoords(midairStartLat, midairStartLon, 0, distFeet, ref celestialImageNorth, ref dFinishLon);
            MathRoutines.AdjCoords(midairStartLat, midairStartLon, 90, distFeet, ref dFinishLat, ref celestialImageEast);
            MathRoutines.AdjCoords(midairStartLat, midairStartLon, 180, distFeet, ref celestialImageSouth, ref dFinishLon);
            MathRoutines.AdjCoords(midairStartLat, midairStartLon, 270, distFeet, ref dFinishLat, ref celestialImageWest);
        }

        /// <summary>
        /// Finds OSM tile numbers and offsets for celestial scenario, comprising starting position in air and destination airport
        /// </summary>
        /// <param name="zoom">The zoom level to get OSM tiles at</param>
        /// <returns>The list of tiles</returns>
        static internal void SetCelestialOSMtiles(List<Tile> tiles, int zoom, int startItemIndex, int finishItemIndex)
        {
            tiles.Clear();
            if (startItemIndex == 0)
                tiles.Add(OSM.GetOSMtile(midairStartLon.ToString(), midairStartLat.ToString(), zoom));
            tiles.Add(OSM.GetOSMtile(Runway.destRwy.AirportLon.ToString(), Runway.destRwy.AirportLat.ToString(), zoom));
        }

        /// <summary>
        /// Using scenario date provided by user, obtain almanac data for three days, and extract Aries GHA degrees and minutes,
        /// and for the list of navigational stars SHA and Declination in degrees and minutes
        /// </summary>
        /// <returns>True if able to retrieve the almanac data from the web and everything on the page is still where expected!</returns>
        static internal bool GetAlmanacData()
        {
            string almanacData = DownloadAlmanac();
            if (almanacData == null) return false;

            if (!ExtractAriesGHA(almanacData)) return false;

            if (!ExtractStarData(almanacData)) return false;

            return true;
        }

        /// <summary>
        /// Download the almanac data for the three days centered on the selected scenario date
        /// </summary>
        /// <returns>The webpage outerHTML containing the alamanac data</returns>
        static internal string DownloadAlmanac()
        {
            DateTime startDate = new(Parameters.Year, Parameters.Month, Parameters.Day, Parameters.Hours, Parameters.Minutes, Parameters.Seconds, DateTimeKind.Local);
            startDate = startDate.AddDays(-1);
            string url = $"http://www.tecepe.com.br/scripts/AlmanacPagesISAPI.dll/pages?date={startDate.Month}%2F{startDate.Day}%2F{startDate.Year}";
            return HttpRoutines.GetWebString(url);
        }

        /// <summary>
        /// Extract the Aries GHA degrees and minutes data values for each hour of the three days
        /// </summary>
        /// <param name="almanacData">The raw data downloaded from the web</param>
        /// <returns>True if successful in extracting Aries GHA degrees and minutes</returns>
        static internal bool ExtractAriesGHA(string almanacData)
        {
            if (!almanacData.Contains("G.M.T", StringComparison.CurrentCulture)) return false;
            string ariesGHAdata = almanacData[almanacData.IndexOf("G.M.T")..];
            string[] hours = ariesGHAdata.Split("\n");                      // GHA data is one line per hour
            if (hours.Length < 85) return false;                            // Check sufficient lines to pick up the three days of data
            int day = 0, hour = 0;
            for (int line = 2; line < 85; line++)
            {
                if (hours[line].Length > 6 && hours[line][6] == '|')        // Uniquely identifies the data lines from formatting lines
                {
                    string[] pipes = hours[line].Split('|');                // pipes[0] will contain the hour, pipes[1] the GHA data for that hour
                    string[] spaces = pipes[0].Trim().Split(' ');           // spaces[^1] will contain the hour minus any day characters
                    if (spaces[^1] != hour.ToString()) return false;        // Check the data is arranged from hour 0 to hour 23, for the three days
                    spaces = pipes[1].Trim().Split(' ');                    // spaces[0] will contain the GHA degrees, spaces[1] the GHA minutes
                    int GHAdegrees = Convert.ToInt16(spaces[0]);
                    if (GHAdegrees < 0 || GHAdegrees > 360) return false;   // Check data is in degree range
                    ariesGHAd[day, hour] = GHAdegrees;                      // Store the degrees for current hour
                    double GHAminutes = Convert.ToDouble(spaces[1]);
                    if (GHAminutes < 0 || GHAminutes > 60) return false;    // Check data is in minutes range
                    ariesGHAm[day, hour++] = GHAminutes;                    // Store the minutes for current hour then increment the hour
                    if (hour == 24) { hour = 0; day++; }
                }
            }
            return true;
        }

        /// <summary>
        /// Extract degrees and minutes for the SHA and Declination of navigational stars
        /// </summary>
        /// <param name="almanacData">The raw data downloaded from the web</param>
        /// <returns>True if successful in extracting SHA and Declination of navigational stars</returns>
        static internal bool ExtractStarData(string almanacData)
        {
            int starIndex = 0;
            string[] starNames = { "Acamar", "Achernar", "Acrux", "Adhara", "Al Na-ir", "Aldebaran", "Alioth", "Alkaid", "Alnilam", "Alphard", "Alphecca",
                "Alpheratz", "Altair", "Ankaa", "Antares", "Arcturus", "Atria", "Avior", "Bellatrix", "Betelgeuse", "Canopus", "Capella", "Deneb",
                "Denebola", "Diphda", "Dubhe", "Elnath", "Eltanin", "Enif", "Fomalhaut", "Gacrux", "Gienah", "Hadar", "Hamal", "Kaus Austr.", "Kochab",
                "Markab", "Menkar", "Menkent", "Miaplacidus", "Mirfak", "Nunki", "Peacock", "Pollux", "Procyon", "Rasalhague", "Regulus", "Rigel",
                "Rigil Kent", "Sabik", "Schedar", "Shaula", "Sirius", "Spica", "Suhail", "Vega", "Zuben-ubi"};
            string[] almanacDataRows = almanacData.Split("\n");

            foreach (string starName in starNames)
            {
                string starDataLine = null;
                var matchingLines = almanacDataRows
                    .Select(line => new { Line = line, ExtractedName = ExtractStarName(line) })
                    .Where(item => item.ExtractedName != null && item.ExtractedName.Equals(starName, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                if (matchingLines.Count == 1)
                    starDataLine = matchingLines.Single().Line;
                else 
                    return false;
                string[] pipes = starDataLine.Split('|');
                // Star data occurs after last pipe symbol in line, and is whitespace separated ([12..] strips star name from front)
                string[] spaces = pipes[^1][12..].Split(" ", StringSplitOptions.RemoveEmptyEntries);   

                // Store SHA degrees and minutes for current star
                starsSHAd[starIndex] = Convert.ToDouble(spaces[0]);
                starsSHAm[starIndex] = Convert.ToDouble(spaces[1]);

                // Store SHA declination degrees and minutes, if degrees is < 10 then there is a space between the N or S character and the degrees number
                if (spaces.Length == 4)
                {
                    starsDECd[starIndex] = Convert.ToDouble(spaces[2][1..]);    // Exclude N or S character
                    if (spaces[2][0] == 'S')
                        starsDECd[starIndex] *= -1;                             // Store south declinations as negative values
                    starsDECm[starIndex++] = Convert.ToDouble(spaces[3]);
                }
                else if (spaces.Length == 5)
                {
                    starsDECd[starIndex] = Convert.ToDouble(spaces[3]);
                    if (spaces[2][0] == 'S')
                        starsDECd[starIndex] *= -1;                             // Store south declinations as negative values
                    starsDECm[starIndex++] = Convert.ToDouble(spaces[4]);
                }
                else
                    return false;
            }

            return true;
        }

        // Function to extract star name from a data line based on the new understanding
        static internal string ExtractStarName(string line)
        {
            // Find the last occurrence of "| "
            int lastPipeIndex = line.LastIndexOf("| ");

            if (lastPipeIndex != -1 && lastPipeIndex + 2 < line.Length)
            {
                // The star name starts 2 characters after the last "|", and is 12 characters long.
                int startIndex = lastPipeIndex + 2;
                int length = Math.Min(12, line.Length - startIndex); // Ensure we don't go out of bounds

                string potentialStarName = line.Substring(startIndex, length).Trim();

                // Optional: Validate if it looks like a star name (e.g., not just numbers or empty)
                if (!string.IsNullOrEmpty(potentialStarName) && !char.IsDigit(potentialStarName[0]))
                {
                    return potentialStarName;
                }
            }

            return null; // No valid star name found
        }

        /// <summary>
        /// Read in list of all stars from excel spreadsheet embedded resource, includes the creating a list of
        /// the navigational stars.
        /// </summary>
        /// <returns>True if the spreadsheet read in successfully</returns>
        static internal bool InitStars()
        {
            ExcelPackage.License.SetNonCommercialPersonal("David Kilpatrick");

            Stream stream = Form.GetResourceStream("Excel.CelestialNavStars.xlsx");
            using ExcelPackage package = new(stream);
            var worksheet = package.Workbook.Worksheets[0];
            int index = 2; // skip header row
            while (worksheet.Cells[index, 1].Value != null)
            {
                stars.Add(new Star(Convert.ToString(worksheet.Cells[index, 1].Value),
                    Convert.ToString(worksheet.Cells[index, 2].Value),
                    Convert.ToString(worksheet.Cells[index, 3].Value),
                    Convert.ToString(worksheet.Cells[index, 4].Value),
                    Convert.ToString(worksheet.Cells[index, 5].Value),
                    Convert.ToString(worksheet.Cells[index, 6].Value),
                    Convert.ToString(worksheet.Cells[index, 7].Value),
                    (double)worksheet.Cells[index, 8].Value,
                    (double)worksheet.Cells[index, 9].Value,
                    (double)worksheet.Cells[index, 10].Value,
                    (double)worksheet.Cells[index, 11].Value,
                    (double)worksheet.Cells[index, 12].Value,
                    (double)worksheet.Cells[index, 13].Value,
                    (double)worksheet.Cells[index, 14].Value
                    ));
                if (Convert.ToString(worksheet.Cells[index, 5].Value) != "")
                {
                    navStarNames.Add(worksheet.Cells[index, 5].Value.ToString());
                }
                noStars++;
                index++;
            }
            navStarNames.Sort();
            return true;
        }

        /// <summary>
        /// Creates P3D Scenario Generator specific version of "stars.dat" and backs up original if user agrees. If user doesn't agree
        /// a dummy copy of "stars.dat" is created so that user isn't asked again.
        /// </summary>
        /// <returns>True if all needed file operations complete successfully</returns>
        static internal bool CreateStarsDat()
        {
            string starsDatPath = Path.Combine(Parameters.SettingsP3DprogramData + Parameters.SettingsSimulatorVersion, "stars.dat");
            string starsDatBackupPath = Path.Combine(Parameters.SettingsP3DprogramData + Parameters.SettingsSimulatorVersion, "stars.dat.P3DscenarioGenerator.backup");

            string starsDatContent = $"[Star Settings]\nIntensity=230\nNumStars={noStars}\n[Star Locations]\n";

            // If the file "stars.dat.P3DscenarioGenerator.backup" exists then assume user has previously said yes to the prompt below
            // and there is no need to do it again as the replacement "stars.dat" doesn't change over time
            if (!File.Exists(starsDatBackupPath))
            {
                string message =    "To see the same stars out the window as are displayed in the celestial sextant, the program" +
                                    " needs to backup the existing stars.dat file (to stars.dat.P3DscenarioGenerator.backup) and replace" +
                                    " it with a program generated version. Press \"Yes\" to go ahead with backup and replacement, \"No\" to leave stars.dat as is";
                string title = "Confirm backup and replacement of stars.dat";
                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult result = MessageBox.Show(message, title, buttons, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Backup existing stars.dat if it exists
                    if (File.Exists(starsDatPath)) 
                    {
                        // Try to delete the old backup. If it fails, report and stop.
                        if (!FileOps.TryDeleteFile(starsDatBackupPath))
                        {
                            return false;
                        }

                        // Try to move the current stars.dat. If it fails, report and stop.
                        if (!FileOps.TryMoveFile(starsDatPath, starsDatBackupPath))
                        {
                            return false;
                        }
                    }

                    // Populate the content for the new stars.dat
                    for (int index = 0; index < noStars; index++)
                    {
                        starsDatContent += $"Star.{index} = {index + 1}";
                        starsDatContent += $",{stars[index].raH}";
                        starsDatContent += $",{stars[index].raM}";
                        starsDatContent += $",{stars[index].raS}";
                        starsDatContent += $",{stars[index].decD}";
                        starsDatContent += $",{stars[index].decM}";
                        starsDatContent += $",{stars[index].decS}";
                        starsDatContent += $",{stars[index].visMag}\n";
                    }

                    // Try to write the new stars.dat file. If it fails, report and stop.
                    if (!FileOps.TryWriteAllText(starsDatPath, starsDatContent))
                    {
                        return false;
                    }

                    MessageBox.Show("stars.dat successfully updated and backed up.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true; // Operation successful
                }
                else // Copy "stars.dat" to "stars.dat.P3DscenarioGenerator.backup" to prevent future prompting of user
                {
                    if (!FileOps.TryCopyFile(starsDatPath, starsDatBackupPath, false))
                    {
                        return false;
                    }
                }
            }
            return true;    
        }

        /// <summary>
        /// Retrieves a Star object from the internal 'stars' array at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the Star to retrieve.</param>
        /// <returns>The Star object located at the specified index.</returns>
        static internal Star GetStar(int index)
        {
            return stars[index];
        }

        /// <summary>
        /// Calculates the great-circle distance between two geographic points
        /// (midair starting latitude/longitude and destination latitude/longitude)
        /// using the CalcDistance method from MathRoutines.
        /// </summary>
        /// <returns>The calculated celestial distance in nautical miles.</returns>
        static internal double GetCelestialDistance()
        {
            return MathRoutines.CalcDistance(midairStartLat, midairStartLon, Runway.destRwy.AirportLat, Runway.destRwy.AirportLon);
        }

        // Consider using a static Random instance for performance and better randomness
        private static readonly Random _random = new();

        /// <summary>
        /// Sets a random celestial starting location and heading for a simulated aircraft.
        /// The start location is positioned within a specified distance range from the destination
        /// runway, using more accurate spherical geometry calculations for latitude/longitude.
        /// </summary>
        static internal void SetCelestialStartLocation()
        {
            // 1. Set a random heading between -180 and 180 degrees
            midairStartHdg = -180.0 + (_random.NextDouble() * 360.0); // Continuous double for heading

            // 2. Position plane randomly around destination within min/max distance
            // Using continuous random for angle and radius for better distribution

            // Generate a random angle in radians (0 to 2*PI)
            double randomAngleRad = _random.NextDouble() * 2 * Math.PI;

            // Generate a random radius (distance) within the specified range (in NM)
            double minDistanceNM = Parameters.CelestialMinDistance;
            double maxDistanceNM = Parameters.CelestialMaxDistance;
            double randomRadiusNM = minDistanceNM + (_random.NextDouble() * (maxDistanceNM - minDistanceNM));

            // Convert radius from nautical miles to degrees latitude for calculation reference
            // Assuming 1 nautical mile = 1 minute of arc (1/60th of a degree latitude)
            const double NAUTICAL_MILES_PER_DEGREE_LAT = 60.0;
            double randomRadiusDegreesLat = randomRadiusNM / NAUTICAL_MILES_PER_DEGREE_LAT;

            // More accurate latitude and longitude adjustments for spherical geometry
            // This is a simplified direct calculation. For very high accuracy over large distances,
            // use proper spherical trigonometry (e.g., haversine or Vincenty formula).
            // For short distances, this approximation is often acceptable.

            // Calculate latitude adjustment
            midairStartLat = Runway.destRwy.AirportLat + (randomRadiusDegreesLat * Math.Cos(randomAngleRad));

            // Calculate longitude adjustment, accounting for convergence of meridians
            // Longitude adjustment depends on latitude (cos(latitude))
            // This uses the destination latitude for simplicity, a more accurate method
            // would average the start/end latitudes or use iterative methods.
            double degreesLongitudePerDegreeLatitudeAtDest = 1.0 / Math.Cos(Runway.destRwy.AirportLat * Math.PI / 180.0);
            double randomRadiusDegreesLon = randomRadiusDegreesLat * degreesLongitudePerDegreeLatitudeAtDest;

            midairStartLon = Runway.destRwy.AirportLon + (randomRadiusDegreesLon * Math.Sin(randomAngleRad));

            // 3. Normalize Latitude and Longitude
            // Latitude normalization (-90 to +90)
            if (midairStartLat > 90)
            {
                midairStartLat = 180 - midairStartLat; // Go south from the pole
                midairStartLon += 180; // Flip longitude if crossing pole
            }
            else if (midairStartLat < -90)
            {
                midairStartLat = -180 - midairStartLat; // Go north from the pole
                midairStartLon += 180; // Flip longitude if crossing pole
            }
            // Ensure latitude is within -90 to 90 after crossing logic if it landed exactly on a pole
            if (midairStartLat > 90) midairStartLat = 90;
            if (midairStartLat < -90) midairStartLat = -90;


            // Longitude normalization (-180 to +180)
            midairStartLon = (midairStartLon + 180.0) % 360.0; // Wrap to 0 to 360
            if (midairStartLon < 0)
            {
                midairStartLon += 360.0; // Ensure positive if modulo resulted in negative for negative input
            }
            midairStartLon = midairStartLon - 180.0; // Shift to -180 to +180

            SetCelestialMapEdges(midairStartLat, midairStartLon, randomRadiusNM);
        }

        /// <summary>
        /// Populates an embedded HTML template (CelestialSextant.html) with dynamic data,
        /// specifically a list of navigational star names for a dropdown, and
        /// writes the modified HTML to the scenario folder.
        /// </summary>
        /// <returns>True if the HTML file is successfully generated and written; otherwise, false.</returns>
        static internal bool SetCelestialSextantHTML()
        {
            string htmlOutputPath = Path.Combine(Parameters.ImageFolder, "htmlCelestialSextant.html");

            try
            {
                // Read in CelestialSextant.html template using 'using' statements for proper disposal.
                string celestialHTML;
                using (Stream stream = Form.GetResourceStream("HTML.CelestialSextant.html"))
                using (StreamReader reader = new(stream))
                {
                    celestialHTML = reader.ReadToEnd();
                }

                // Create the list of star names for the HTML dropdown options.
                // Use String.Join for concise and efficient string building.
                // It automatically handles commas and adds the '<option>' tags.
                string starOptions = "<option>Select Star</option>" +
                                     string.Join("", navStarNames.Select(name => $"<option>{name}</option>"));

                // Replace the placeholder in the HTML.
                celestialHTML = celestialHTML.Replace("starOptionsX", starOptions);

                // Write the modified HTML to the scenario folder using the error-handling helper.
                return FileOps.TryWriteAllText(htmlOutputPath, celestialHTML);
            }
            catch (Exception ex)
            {
                // Catch any errors related to accessing the embedded resource itself,
                // or directory creation. File writing errors are handled by FileOperationsHelper.
                MessageBox.Show($"An unexpected error occurred while generating the Celestial Sextant HTML file: {ex.Message}",
                                "HTML Generation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Generates and updates JavaScript files (scriptsCelestialSextant.js and scriptsCelestialAstroCalcs.js)
        /// by reading templates from embedded resources, populating them with dynamic star data
        /// and other parameters, and writing the modified files to the specified image folder.
        /// Includes robust error handling for file operations.
        /// </summary>
        /// <returns>True if both JavaScript files are successfully generated and saved; otherwise, false.</returns>
        static internal bool SetCelestialSextantJS()
        {
            string saveLocation = Parameters.ImageFolder;
            string sextantJsOutputPath = Path.Combine(saveLocation, "scriptsCelestialSextant.js");
            string astroCalcsJsOutputPath = Path.Combine(saveLocation, "scriptsCelestialAstroCalcs.js");

            try
            {
                // --- Process CelestialSextant.js ---
                string celestialJS;
                using (Stream stream = Form.GetResourceStream("Javascript.scriptsCelestialSextant.js"))
                using (StreamReader reader = new(stream))
                {
                    celestialJS = reader.ReadToEnd();
                }

                // Build star data as a collection for LINQ processing
                IEnumerable<Star> allStars = Enumerable.Range(0, noStars).Select(GetStar);

                // Prepare all replacement values in a dictionary
                Dictionary<string, string> replacements = new()
                {
                    // Star Data Replacements
                    { "constellationX", string.Join(",", allStars.Select(s => $"\"{s.constellation}\"")) },
                    { "idX", string.Join(",", allStars.Select(s => $"\"{s.id}\"")) },
                    { "starNumberX", string.Join(",", allStars.Select(s => $"\"{s.starNumber}\"")) },
                    { "starNameX", string.Join(",", allStars.Select(s => $"\"{s.starName}\"")) },
                    { "bayerX", string.Join(",", allStars.Select(s => $"\"{s.bayer}\"")) },
                    { "raHX", string.Join(",", allStars.Select(s => s.raH.ToString())) },
                    { "raMX", string.Join(",", allStars.Select(s => s.raM.ToString())) },
                    { "raSX", string.Join(",", allStars.Select(s => s.raS.ToString())) },
                    { "decDX", string.Join(",", allStars.Select(s => s.decD.ToString())) },
                    { "decMX", string.Join(",", allStars.Select(s => s.decM.ToString())) },
                    { "decSX", string.Join(",", allStars.Select(s => s.decS.ToString())) },
                    { "visMagX", string.Join(",", allStars.Select(s => s.visMag.ToString())) },
                    { "linesX", string.Join(", ", allStars
                        .Where(s => !string.IsNullOrEmpty(s.connectedId))
                        .SelectMany(s => new[] { $"\"{s.id}\"", $"\"{s.connectedId}\"" }))
                    },

                    // Geographic Coordinates
                    { "destLatX", Runway.destRwy.AirportLat.ToString() },
                    { "destLonX", Runway.destRwy.AirportLon.ToString() },

                    // Aries GHA data (using LINQ for arrays/2D arrays)
                    { "ariesGHAdX", BuildNestedArrayString(ariesGHAd) },
                    { "ariesGHAmX", BuildNestedArrayString(ariesGHAm) },

                    // Star SHA data
                    { "starsSHAdX", string.Join(",", starsSHAd.Select(d => d.ToString())) },
                    { "starsSHAmX", string.Join(",", starsSHAm.Select(m => m.ToString())) },

                    // Star DEC data
                    { "starsDECdX", string.Join(",", starsDECd.Select(d => d.ToString())) },
                    { "starsDECmX", string.Join(",", starsDECm.Select(m => m.ToString())) },

                    // Nav Star Names
                    { "starNameListX", string.Join(",", navStarNames.Select(name => $"\"{name}\"")) },

                    // Date and Image Edge Parameters
                    { "startDateX", $"\"{Parameters.Month}/{Parameters.Day}/{Parameters.Year}\"" },
                    { "northEdgeX", celestialImageNorth.ToString() },
                    { "eastEdgeX", celestialImageEast.ToString() },
                    { "southEdgeX", celestialImageSouth.ToString() },
                    { "westEdgeX", celestialImageWest.ToString() }
                };

                // Apply all replacements
                foreach (var entry in replacements)
                {
                    celestialJS = celestialJS.Replace(entry.Key, entry.Value);
                }

                // Write the modified CelestialSextant.js file
                if (!FileOps.TryWriteAllText(sextantJsOutputPath, celestialJS))
                {
                    return false; // Error handled by helper, just exit
                }

                // --- Process CelestialAstroCalcs.js ---
                // Assuming this file doesn't need dynamic replacements and is just copied
                string astroCalcsJS;
                using (Stream stream = Form.GetResourceStream("Javascript.scriptsCelestialAstroCalcs.js"))
                using (StreamReader reader = new StreamReader(stream))
                {
                    astroCalcsJS = reader.ReadToEnd();
                }

                // Write the CelestialAstroCalcs.js file
                if (!FileOps.TryWriteAllText(astroCalcsJsOutputPath, astroCalcsJS))
                {
                    return false; // Error handled by helper, just exit
                }

                // Copy plotImage used in Plotting tab
                Stream plotStream = Form.GetResourceStream($"Images.plotImage.jpg");
                using (FileStream outputFileStream = new($"{Parameters.ImageFolder}\\plotImage.jpg", FileMode.Create))
                {
                    plotStream.CopyTo(outputFileStream);
                }
                plotStream.Dispose();

                return true;
            }
            catch (Exception ex)
            {
                // Catch any errors related to resource streams or initial directory creation
                MessageBox.Show($"An unexpected error occurred during JavaScript file generation: {ex.Message}",
                                "Error Generating Files", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Helper method to build a JavaScript-style 2D array string from a C# 2D array.
        /// </summary>
        private static string BuildNestedArrayString<T>(T[,] array)
        {
            StringBuilder sb = new();
            sb.Append('[');
            for (int i = 0; i < array.GetLength(0); i++)
            {
                sb.Append('[');
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    sb.Append(array[i, j]);
                    if (j < array.GetLength(1) - 1)
                    {
                        sb.Append(',');
                    }
                }
                sb.Append(']');
                if (i < array.GetLength(0) - 1)
                {
                    sb.Append(',');
                }
            }
            sb.Append(']');
            return sb.ToString();
        }

        /// <summary>
        /// Reads the Celestial Sextant CSS template from an embedded resource and writes it
        /// to a specified file location.
        /// </summary>
        /// <param name="saveLocation">The full path, including filename, where the CSS file should be saved.</param>
        static internal bool SetCelestialSextantCSS()
        {
            string signWritingCSS;
            string cssOutputPath = Path.Combine(Parameters.ImageFolder, "styleCelestialSextant.css");

            // Use 'using' statements to ensure streams are properly disposed, even if errors occur.
            // Also, incorporate FileOps for robust error handling.
            try
            {
                using (Stream stream = Form.GetResourceStream($"CSS.styleCelestialSextant.css"))
                using (StreamReader reader = new(stream))
                {
                    signWritingCSS = reader.ReadToEnd();
                }

                // Use the centralized file operation helper to write the file, with error handling.
                if (!FileOps.TryWriteAllText(cssOutputPath, signWritingCSS))
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                // Catch any errors related to accessing the embedded resource itself.
                MessageBox.Show($"An unexpected error occurred while processing the Celestial Sextant CSS: {ex.Message}",
                                "CSS File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}
