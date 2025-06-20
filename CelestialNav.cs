using OfficeOpenXml;
using System.Text;

namespace P3D_Scenario_Generator
{
    /// <summary>
    /// Represents a celestial star with various astronomical properties.
    /// </summary>
    /// <param name="s1">The constellation the star belongs to.</param>
    /// <param name="s2">A unique identifier for the star.</param>
    /// <param name="s3">An identifier for a connected star, if applicable.</param>
    /// <param name="s4">The star's number designation.</param>
    /// <param name="s5">The common name of the star.</param>
    /// <param name="s6">A link to its Wikipedia page or other relevant information.</param>
    /// <param name="s7">The Bayer designation of the star.</param>
    /// <param name="d8">The Right Ascension in hours.</param>
    /// <param name="d9">The Right Ascension in minutes.</param>
    /// <param name="d10">The Right Ascension in seconds.</param>
    /// <param name="d11">The Declination in degrees.</param>
    /// <param name="d12">The Declination in arcminutes.</param>
    /// <param name="d13">The Declination in arcseconds.</param>
    /// <param name="d14">The visual magnitude of the star.</param>
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

    /// <summary>
    /// Manages all aspects of celestial navigation for the simulation, including loading star data,
    /// retrieving almanac information, calculating celestial positions, and generating
    /// dynamic web content (HTML, JavaScript, CSS) for a celestial sextant display.
    /// It also handles the creation and backup of the simulator's stars.dat file.
    /// </summary>
    class CelestialNav
    {
        private static readonly List<Star> stars = [];
        internal static List<string> navStarNames = [];
        internal static int noStars = 0;
        internal static int[,] ariesGHAd = new int[Con.NumberOfDaysToExtract, Con.HoursPerDay];
        internal static double[,] ariesGHAm = new double[Con.NumberOfDaysToExtract, Con.HoursPerDay];
        internal static int[] starsSHAd = new int[57];
        internal static double[] starsSHAm = new double[57];
        internal static int[] starsDECd = new int[57];
        internal static double[] starsDECm = new double[57];
        internal static double midairStartHdg;
        internal static double midairStartLat;
        internal static double midairStartLon;
        internal static double celestialImageNorth;
        internal static double celestialImageEast;
        internal static double celestialImageSouth;
        internal static double celestialImageWest;

        private static readonly Random _random = new();

        /// <summary>
        /// Initializes the celestial navigation system for a new scenario.
        /// This method sets a random destination runway, determines the celestial start location,
        /// and then attempts to load almanac data, initialize star data, create star data files,
        /// and set up the necessary HTML, JavaScript, and CSS for the celestial sextant display.
        /// If all these steps are successful, it also updates the overview and location images.
        /// </summary>
        /// <returns>True if all celestial setup operations complete successfully; otherwise, false.</returns>
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

        /// <summary>
        /// Calculates and sets the geographical boundaries (North, East, South, West) for a celestial map image.
        /// These boundaries define the extent of the map based on a starting point and a specified distance.
        /// </summary>
        /// <param name="midairStartLat">The starting latitude (in degrees) of the center of the celestial map.</param>
        /// <param name="midairStartLon">The starting longitude (in degrees) of the center of the celestial map.</param>
        /// <param name="distance">The radial distance (in nautical miles) from the center to the edges of the map.</param>
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

            string result = HttpRoutines.GetWebDoc(url).DocumentNode.OuterHtml; 
            if (result == null)
            {
                Log.Error($"Failed to download almanac data from URL: {url}");
            }
            return result;
        }

        /// <summary>
        /// Extract the Aries GHA degrees and minutes data values for each hour of the three days
        /// </summary>
        /// <param name="almanacData">The raw data downloaded from the web</param>
        /// <returns>True if successful in extracting Aries GHA degrees and minutes</returns>
        static internal bool ExtractAriesGHA(string almanacData)
        {
            const int DegsAndMinsSegmentIndex = 1;              // The segment in pipes string array containing GHA degrees and minutes values
            const int Expected_Space_Separated_Segments = 2;    // Expecting 2 space separated segments being GHA degrees and minutes values
            const int StarGHAdegreesIndex = 0;                  // Index in spaces string array of GHA degrees value
            const int StarGHAminutesIndex = 1;                  // Index in spaces string array of GHA minutes value
            int day = 0, hour = 0;
            if (GetAriesGHAdataBlock(almanacData, out string[] hourDataLines))
            {
                for (int lineNo = 0; lineNo < hourDataLines.Length; lineNo++)
                {
                    if (ValidAriesGHAdataline(hourDataLines[lineNo], hour))
                    {
                        string[] pipes = hourDataLines[lineNo].Split('|');
                        string[] spaces = pipes[DegsAndMinsSegmentIndex].Trim().Split(' ');     // pipes[1] contains the Aries GHA data
                        if (spaces.Length < Expected_Space_Separated_Segments)                  // Check if degrees and minutes exist
                        {
                            Log.Error($"Malformed GHA data (degrees/minutes) in Aries GHA line {lineNo}: '{hourDataLines[lineNo]}'");
                            return false;
                        }

                        if (!TryParseDegrees(spaces[StarGHAdegreesIndex], "Aries GHA", out ariesGHAd[day, hour]))
                            return false;

                        if (!TryParseMinutes(spaces[StarGHAminutesIndex], "Aries GHA", out ariesGHAm[day, hour++]))
                            return false;

                        if (hour == Con.HoursPerDay) { hour = 0; day++; }
                    }
                }
                if (hour == 0 && day == Con.NumberOfDaysToExtract)
                    return true;
                else
                {
                    Log.Error($"Unable to extract {Con.NumberOfDaysToExtract} days of {Con.HoursPerDay} hours Aries GHA degrees and minutes data from almanac data.");
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Extracts the specific block of Aries GHA data lines from the raw almanac data.
        /// This block is identified by its header line containing "ARIES", "VENUS", and "MARS",
        /// and ends before the next major data block identified by "SUN", "MOON", and "STARS".
        /// Performs checks to ensure both header lines are present and the Aries GHA block
        /// contains at least the expected number of data rows (3 days * 24 hours).
        /// </summary>
        /// <param name="almanacData">The complete raw almanac data as a single string.</param>
        /// <param name="hourDataLines">
        /// When this method returns, contains an array of strings representing
        /// only the lines within the identified Aries GHA data block if successful; otherwise, an empty array.
        /// </param>
        /// <returns>True if the Aries GHA data block was successfully identified and extracted; otherwise, false.</returns>
        static internal bool GetAriesGHAdataBlock(string almanacData, out string[] hourDataLines)
        {
            // Almanac data has two blocks of data covering 3 days with one line per each of 24 hours labelled 0 to 23
            // The Aries GHA data is in the first block which has a header row with the strings "ARIES", "VENUS", and "MARS" in it
            // The second block has a header row with the strings "SUN", "MOON", and "STARS" in it

            hourDataLines = almanacData.Split("\n");

            // Check Aries GHA data block header line present
            if (!HeaderLinePresent(hourDataLines, Con.AriesKeyword, Con.VenusKeyword, Con.MarsKeyword, out int firstBlockHeaderIndex))
            {
                return false;
            }

            // Check second non Aries GHA data block header line present
            if (!HeaderLinePresent(hourDataLines, Con.SunKeyword, Con.MoonKeyword, Con.StarsKeyword, out int secondBlockHeaderIndex))
            {
                return false;
            }

            // Check there is atleast 72 rows of data in first block being 3 days x 24 hours per day
            if (firstBlockHeaderIndex + (Con.NumberOfDaysToExtract * Con.HoursPerDay) < secondBlockHeaderIndex)
            {
                hourDataLines = hourDataLines[firstBlockHeaderIndex..secondBlockHeaderIndex];
                return true;
            }
            else
            {
                Log.Error($"There is less than 72 ({Con.NumberOfDaysToExtract} days x {Con.HoursPerDay} hours) rows of data in the Aries GHA block " +
                    "within the downloaded almanac data");
                return false;
            }
        }

        /// <summary>
        /// Checks if a header line containing all three specified keywords (case-sensitive) is present
        /// within the provided array of data lines.
        /// </summary>
        /// <param name="hourDataLines">The array of strings to search within.</param>
        /// <param name="keyword1">The first keyword to search for.</param>
        /// <param name="keyword2">The second keyword to search for.</param>
        /// <param name="keyword3">The third keyword to search for.</param>
        /// <param name="headerLineIndex">
        /// When this method returns, contains the zero-based index of the first line
        /// that contains all three keywords if found; otherwise, returns -1.
        /// </param>
        /// <returns>True if a matching header line is found; otherwise, false.</returns>
        static internal bool HeaderLinePresent(string[] hourDataLines, string keyword1, string keyword2, string keyword3, out int headerLineIndex)
        {
            var matchingLineInfo = hourDataLines
                .Select((line, index) => new { Line = line, Index = index })
                .FirstOrDefault(item =>
                    item.Line.Contains(keyword1) &&
                    item.Line.Contains(keyword2) &&
                    item.Line.Contains(keyword3)
                );
            if (matchingLineInfo != null)
            {
                headerLineIndex = matchingLineInfo.Index;
                return true;
            }
            else
            {
                headerLineIndex = matchingLineInfo.Index;
                Log.Error($"Header line containing '{keyword1}', '{keyword2}', and '{keyword3}' not found in almanac data.");
                return false;
            }
        }

        /// <summary>
        /// Validates if a given string line is a well-formed Aries GHA data line
        /// by checking its pipe-separated structure and if the embedded hour matches the expected next hour.
        /// </summary>
        /// <param name="potentialHourDataLine">The string line from the almanac data to validate.</param>
        /// <param name="nextHour">The expected hour value (0-23) that the data line should contain.</param>
        /// <returns>
        /// True if the line contains at least two pipe-separated parts, and the last
        /// space-delimited value in the first part matches the <paramref name="nextHour"/>;
        /// otherwise, false. No error is logged if validation fails, as non-data lines are expected.
        /// </returns>
        static internal bool ValidAriesGHAdataline(string potentialHourDataLine, int nextHour)
        {
            const int Expected_Min_Pipe_Separated_Parts = 2;
            const int HourSegmentIndex = 0;

            // Check whether it contains atleast one pipe symbol
            string[] pipes = potentialHourDataLine.Split('|');
            if (pipes.Length >= Expected_Min_Pipe_Separated_Parts)
            {
                // Check whether last space delimited data in pipes[HourSegmentIndex] matches nextHour
                string[] spaces = pipes[HourSegmentIndex].Trim().Split(' ');
                if (spaces.Length > 0 && spaces[^1] == nextHour.ToString())
                {
                    return true;
                }
            }
            // No error logging as it's expected some lines contain no data
            return false;
        }

        /// <summary>
        /// Attempts to parse a string into an integer representing degrees and validates its range (0 to 360, inclusive).
        /// Logs an error if parsing fails or if the value is out of the valid range.
        /// </summary>
        /// <param name="degreesStringIn">The string containing the degrees value to parse.</param>
        /// <param name="degreesName">A descriptive name for the degrees value (e.g., "Aries GHA") used in error messages.</param>
        /// <param name="degreesIntOut">When this method returns, contains the parsed integer value if successful; otherwise, 0.</param>
        /// <returns>True if the string was successfully parsed into a valid degree value; otherwise, false.</returns>
        static internal bool TryParseDegrees(string degreesStringIn, string degreesName, out int degreesIntOut)
        {
            if (!int.TryParse(degreesStringIn, out degreesIntOut))
            {
                Log.Error($"Failed to parse {degreesName} degrees in string: '{degreesStringIn}'");
                return false;
            }
            if (degreesIntOut < 0 || degreesIntOut > Con.MaxDegrees)
            {
                Log.Error($"{degreesName} degrees out of range in string: {degreesStringIn}");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Attempts to parse a string into a double representing minutes and validates its range (0 to 60, inclusive).
        /// Logs an error if parsing fails or if the value is out of the valid range.
        /// </summary>
        /// <param name="minutesStringIn">The string containing the minutes value to parse.</param>
        /// <param name="minutesName">A descriptive name for the minutes value (e.g., "Aries GHA") used in error messages.</param>
        /// <param name="minutesDoubleOut">When this method returns, contains the parsed double value if successful; otherwise, 0.</param>
        /// <returns>True if the string was successfully parsed into a valid minute value; otherwise, false.</returns>
        static internal bool TryParseMinutes(string minutesStringIn, string minutesName, out double minutesDoubleOut)
        {
            if (!double.TryParse(minutesStringIn, out minutesDoubleOut))
            {
                Log.Error($"Failed to parse {minutesName} minutes in string: '{minutesStringIn}'");
                return false;
            }
            if (minutesDoubleOut < 0 || minutesDoubleOut > Con.MaxMinutes)
            {
                Log.Error($"{minutesName} minutes out of range in string: {minutesStringIn}");
                return false;
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
                if (!GetStarDataLine(almanacDataRows, starName, out string starDataLine))
                {
                    Log.Error($"Unable to locate exactly one line of data for star name \"{starName}\" in almanac data.");
                    return false;
                }

                if (!GetStarDataValues(starDataLine, out string[] starDataValues))
                {
                    Log.Error($"Unable to locate the star data values for star name \"{starName}\" in almanac data.");
                    return false;
                }

                if (!TryParseStarDataValues(starDataValues, starIndex, starName))
                {
                    Log.Error($"Unable to parse the star data values for star name \"{starName}\" in almanac data.");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Attempts to find a single data line for a specified star within an array of almanac data rows.
        /// It extracts a potential star name from each line and performs a case-insensitive comparison.
        /// </summary>
        /// <param name="almanacDataRows">An array of strings, where each string represents a row of almanac data.</param>
        /// <param name="starName">The name of the star to search for (e.g., "POLLUX", "SIRIUS").</param>
        /// <param name="starDataLine">
        /// When this method returns, contains the found star's data line if exactly one match is found;
        /// otherwise, it is set to null.
        /// </param>
        /// <returns>
        /// True if exactly one line containing the specified star's data is found;
        /// False if no lines are found, or if multiple lines for the same star are found (indicating ambiguity).
        /// </returns>
        static internal bool GetStarDataLine(string[] almanacDataRows, string starName, out string starDataLine)
        {
            var matchingLines = almanacDataRows
                .Select(line => new { Line = line, ExtractedName = ExtractStarName(line) })
                .Where(item => item.ExtractedName != null && item.ExtractedName.Equals(starName, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (matchingLines.Count == 1)
            {
                starDataLine = matchingLines.Single().Line;
                return true;
            }
            else
            {
                starDataLine = null;
                return false;
            }
        }

        /// <summary>
        /// Extracts the star data related values from a star data line.
        /// </summary>
        /// <param name="starDataLine">The raw string line containing star data.</param>
        /// <param name="starDataValues">SHA and Declination, degrees and minutes for star.</param>
        /// <returns>True if the line is valid and values are extracted; otherwise, false.</returns>
        static internal bool GetStarDataValues(string starDataLine, out string[] starDataValues)
        {
            const int ExpectedPipeParts = 4; // Expecting 3 pipes, resulting in 4 parts
            const int LastPipeSegmentMinLength = 12; // Minimum length for last segment before stripping
            const int DataSubstringStartIndex = 12; // Index to start extracting data from last segment
            const int ExpectedMinSpaceParts = 4;
            const int ExpectedMaxSpaceParts = 5;

            starDataValues = default; // Initialize out parameter

            string[] pipes = starDataLine.Split('|');
            if (pipes.Length != ExpectedPipeParts)
            {
                Log.Error($"Malformed star data line '{starDataLine}'. Expected exactly {ExpectedPipeParts - 1} pipe symbols, found {pipes.Length - 1}.");
                return false;
            }

            string lastPipeSegment = pipes[^1]; // Use a meaningful name for clarity
            if (lastPipeSegment.Length < LastPipeSegmentMinLength)
            {
                Log.Error($"Malformed star data line '{starDataLine}'. Last data segment '{lastPipeSegment}' is too short (expected at least {LastPipeSegmentMinLength} chars).");
                return false;
            }

            // Star data occurs after the initial part of the last pipe segment
            string dataSubstring = lastPipeSegment[DataSubstringStartIndex..];
            string[] spaces = dataSubstring.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            if (spaces.Length < ExpectedMinSpaceParts || spaces.Length > ExpectedMaxSpaceParts)
            {
                Log.Error($"Malformed star data line '{starDataLine}'. Expected {ExpectedMinSpaceParts}-{ExpectedMaxSpaceParts} space-separated values for SHA/Declination data, found {spaces.Length}. Data: '{dataSubstring}'.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Parses and stores the Right Ascension (SHA) and Declination (DEC) values for a specific star
        /// from an array of space-separated data strings. It handles variations in the format
        /// of the declination data (4 or 5 parts depending on leading sign/space).
        /// </summary>
        /// <param name="starDataValues">
        /// An array of strings containing the SHA degrees, SHA minutes, and Declination values.
        /// Expected to have 4 or 5 elements.
        /// </param>
        /// <param name="starIndex">The array index at which to store the parsed star data (e.g., 0 for Acamar, 1 for Achernar).</param>
        /// <param name="starName">The name of the star for context in error messages (e.g., "Acamar").</param>
        /// <returns>True if all SHA and Declination values are successfully parsed and validated; otherwise, false.</returns>
        static internal bool TryParseStarDataValues(string[] starDataValues, int starIndex, string starName)
        {
            const int StarSHAdegreesIndex = 0; // Index in starDataValues of SHA degrees value
            const int StarSHAminutesIndex = 1; // Index in starDataValues of SHA minutes value
            const int ExpectedMinSpaceParts = 4;
            const int ExpectedMaxSpaceParts = 5;

            // Store SHA degrees and minutes for current star
            if (!TryParseDegrees(starDataValues[StarSHAdegreesIndex], $"{starName} SHA", out starsSHAd[starIndex]))
                return false;

            if (!TryParseMinutes(starDataValues[StarSHAminutesIndex], $"{starName} SHA", out starsSHAm[starIndex]))
                return false;

            // Store SHA declination degrees and minutes, if degrees is < 10 then there is a space between the N or S character and the degrees number
            if (starDataValues.Length == ExpectedMinSpaceParts)
            {
                const int starDECsignIndex = 2;         // Index in starDataValues of where declination sign (N or S) is stored
                const int starDECdegreesIndex = 2;      // Index in starDataValues of Declination degrees value
                const int starDECminutesIndex = 3;      // Index in starDataValues of Declination minutes value
                if (!TryParseDegrees(starDataValues[starDECdegreesIndex][1..], $"{starName} Dec", out starsDECd[starIndex])) // Exclude N or S character
                    return false; 
                if (starDataValues[starDECsignIndex][0] == 'S')
                    starsDECd[starIndex] *= -1;         // Store south declinations as negative values
                if (!TryParseMinutes(starDataValues[starDECminutesIndex], $"{starName} Dec", out starsDECm[starIndex++]))
                    return false;
            }
            else if (starDataValues.Length == ExpectedMaxSpaceParts)
            {
                const int starDECsignIndex = 2;         // Index in starDataValues of where declination sign (N or S) is stored
                const int starDECdegreesIndex = 3;      // Index in starDataValues of Declination degrees value
                const int starDECminutesIndex = 4;      // Index in starDataValues of Declination minutes value
                if (!TryParseDegrees(starDataValues[starDECdegreesIndex], $"{starName} Dec", out starsDECd[starIndex])) // Exclude N or S character
                    return false;
                if (starDataValues[starDECsignIndex][0] == 'S')
                    starsDECd[starIndex] *= -1;         // Store south declinations as negative values
                if (!TryParseMinutes(starDataValues[starDECminutesIndex], $"{starName} Dec", out starsDECm[starIndex++]))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Extracts a potential star name from a given string line.
        /// The star name is expected to be located after the last occurrence of "| " and be up to 12 characters long.
        /// It performs basic validation to ensure the extracted string is not empty, all numeric, or out of bounds.
        /// </summary>
        /// <param name="line">The input string line from which to extract the star name.</param>
        /// <returns>The extracted star name if found and valid; otherwise, returns null.</returns>
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

                // Validate if it looks like a star name (e.g., not just numbers or empty)
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
                Log.Error($"An error occurred while generating the Celestial Sextant HTML file: {ex.Message}");
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
                Log.Error($"An unexpected error occurred during JavaScript file generation: {ex.Message}");
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
                Log.Error($"An unexpected error occurred while processing the Celestial Sextant CSS: {ex.Message}");
                return false;
            }
        }
    }
}
