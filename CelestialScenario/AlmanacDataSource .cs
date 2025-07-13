using P3D_Scenario_Generator.ConstantsEnums;

namespace P3D_Scenario_Generator.CelestialScenario
{
    /// <summary>
    /// Manages the acquisition, parsing, and storage of celestial almanac data from a web-based source.
    /// This includes extracting Aries GHA (Greenwich Hour Angle) and navigational star
    /// SHA (Sidereal Hour Angle) and Declination values for use in celestial navigation calculations.
    /// </summary>
    internal class AlmanacDataSource
    {
        /// <summary>
        /// Stores 3 days x 24 hours of Aries GHA degrees star data extracted from web based almanac
        /// </summary>
        internal static int[,] ariesGHAd = new int[Constants.AlmanacExtractDaysCount, Constants.HoursInADay];

        /// <summary>
        /// Stores 3 days x 24 hours of Aries GHA minutes star data extracted from web based almanac
        /// </summary>
        internal static double[,] ariesGHAm = new double[Constants.AlmanacExtractDaysCount, Constants.HoursInADay];

        /// <summary>
        /// The number of stars for which data extracted from web based almanac
        /// </summary>
        internal const int NoStarsInAlmanacData = 57;

        /// <summary>
        /// Stores NoStarsInAlmanacData SHA degrees star data extracted from web based almanac
        /// </summary>
        internal static int[] starsSHAd = new int[NoStarsInAlmanacData];

        /// <summary>
        /// Stores NoStarsInAlmanacData SHA minutes star data extracted from web based almanac
        /// </summary>
        internal static double[] starsSHAm = new double[NoStarsInAlmanacData];

        /// <summary>
        /// Stores NoStarsInAlmanacData Declination degrees star data extracted from web based almanac
        /// </summary>
        internal static int[] starsDECd = new int[NoStarsInAlmanacData];

        /// <summary>
        /// Stores NoStarsInAlmanacData Declination minutes star data extracted from web based almanac
        /// </summary>
        internal static double[] starsDECm = new double[NoStarsInAlmanacData];

        /// <summary>
        /// Using scenario date provided by user, obtain almanac data for three days, and extract Aries GHA degrees and minutes,
        /// and for the list of navigational stars SHA and Declination in degrees and minutes
        /// </summary>
        /// <returns>True if able to retrieve the almanac data from the web and everything on the page is still where expected!</returns>
        internal static bool GetAlmanacData(ScenarioFormData formData)
        {
            string almanacData = DownloadAlmanac(formData);
            if (almanacData == null) return false;

            if (!ExtractAriesGHA(almanacData)) return false;

            if (!ExtractStarData(almanacData)) return false;

            return true;
        }

        /// <summary>
        /// Download the almanac data for the three days centered on the selected scenario date
        /// </summary>
        /// <returns>The webpage outerHTML containing the alamanac data</returns>
        internal static string DownloadAlmanac(ScenarioFormData formData)
        {
            DateTime startDate = new(formData.DatePickerValue.Year, formData.DatePickerValue.Month, formData.DatePickerValue.Day,
                formData.TimePickerValue.Hour, formData.TimePickerValue.Minute, formData.TimePickerValue.Second, DateTimeKind.Local);
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

                        if (!ParsingHelpers.TryParseDegrees(spaces[StarGHAdegreesIndex], "Aries GHA", out ariesGHAd[day, hour]))
                            return false;

                        if (!ParsingHelpers.TryParseMinutes(spaces[StarGHAminutesIndex], "Aries GHA", out ariesGHAm[day, hour++]))
                            return false;

                        if (hour == Constants.HoursInADay) { hour = 0; day++; }
                    }
                }
                if (hour == 0 && day == Constants.AlmanacExtractDaysCount)
                    return true;
                else
                {
                    Log.Error($"Unable to extract {Constants.AlmanacExtractDaysCount} days of {Constants.HoursInADay} hours Aries GHA degrees and minutes data from almanac data.");
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Extract degrees and minutes for the SHA and Declination of navigational stars
        /// </summary>
        /// <param name="almanacData">The raw data downloaded from the web</param>
        /// <returns>True if successful in extracting SHA and Declination of navigational stars</returns>
        static internal bool ExtractStarData(string almanacData)
        {
            int starIndex = 0;
            string[] starNames = [ "Acamar", "Achernar", "Acrux", "Adhara", "Al Na-ir", "Aldebaran", "Alioth", "Alkaid", "Alnilam", "Alphard", "Alphecca",
                "Alpheratz", "Altair", "Ankaa", "Antares", "Arcturus", "Atria", "Avior", "Bellatrix", "Betelgeuse", "Canopus", "Capella", "Deneb",
                "Denebola", "Diphda", "Dubhe", "Elnath", "Eltanin", "Enif", "Fomalhaut", "Gacrux", "Gienah", "Hadar", "Hamal", "Kaus Austr.", "Kochab",
                "Markab", "Menkar", "Menkent", "Miaplacidus", "Mirfak", "Nunki", "Peacock", "Pollux", "Procyon", "Rasalhague", "Regulus", "Rigel",
                "Rigil Kent", "Sabik", "Schedar", "Shaula", "Sirius", "Spica", "Suhail", "Vega", "Zuben-ubi"];

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
                else
                {
                    starIndex++;
                }
            }

            return true;
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
            if (!HeaderLinePresent(hourDataLines, Constants.FirstGhaBlockKeywords, out int firstBlockHeaderIndex))
            {
                return false;
            }

            // Check second non Aries GHA data block header line present
            if (!HeaderLinePresent(hourDataLines, Constants.SecondGhaBlockKeywords, out int secondBlockHeaderIndex))
            {
                return false;
            }

            // Check there is atleast 72 rows of data in first block being 3 days x 24 hours per day
            if (firstBlockHeaderIndex + Constants.AlmanacExtractDaysCount * Constants.HoursInADay < secondBlockHeaderIndex)
            {
                hourDataLines = hourDataLines[firstBlockHeaderIndex..secondBlockHeaderIndex];
                return true;
            }
            else
            {
                Log.Error($"There is less than 72 ({Constants.AlmanacExtractDaysCount} days x {Constants.HoursInADay} hours) rows of data in the Aries GHA block " +
                    "within the downloaded almanac data");
                return false;
            }
        }

        /// <summary>
        /// Checks if a header line containing all specified keywords (case-sensitive) is present
        /// within the provided array of data lines.
        /// </summary>
        /// <param name="hourDataLines">The array of strings to search within.</param>
        /// <param name="keywords">The collection of keywords to search for.</param>
        /// <param name="headerLineIndex">
        /// When this method returns, contains the zero-based index of the first line
        /// that contains all keywords if found; otherwise, returns -1.
        /// </param>
        /// <returns>True if a matching header line is found; otherwise, false.</returns>
        static internal bool HeaderLinePresent(string[] hourDataLines, IEnumerable<string> keywords, out int headerLineIndex)
        {
            var matchingLineInfo = hourDataLines
                .Select((line, index) => new { Line = line, Index = index })
                .FirstOrDefault(item =>
                    // Check if the current line contains ALL keywords in the 'keywords' collection
                    keywords.All(keyword => item.Line.Contains(keyword))
                );

            if (matchingLineInfo != null)
            {
                headerLineIndex = matchingLineInfo.Index;
                return true;
            }
            else
            {
                headerLineIndex = -1;
                // Updated logging to display all keywords from the collection
                Log.Error($"Header line containing '{string.Join("', '", keywords)}' not found in almanac data.");
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
            const int ExpectedMinPipeParts = 2; // Expecting 3 pipes, resulting in 4 parts for all but Pollux which is 1 pipe and 2 parts
            const int LastPipeSegmentMinLength = 12; // Minimum length for last segment before stripping
            const int DataSubstringStartIndex = 12; // Index to start extracting data from last segment
            const int ExpectedMinSpaceParts = 4;
            const int ExpectedMaxSpaceParts = 5;

            starDataValues = default; // Initialize out parameter

            string[] pipes = starDataLine.Split('|');
            if (pipes.Length < ExpectedMinPipeParts)
            {
                Log.Error($"Malformed star data line '{starDataLine}'. Expected a minimum of {ExpectedMinPipeParts - 1} pipe symbols, found {pipes.Length - 1}.");
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

            starDataValues = spaces;
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
            if (!ParsingHelpers.TryParseDegrees(starDataValues[StarSHAdegreesIndex], $"{starName} SHA", out starsSHAd[starIndex]))
                return false;

            if (!ParsingHelpers.TryParseMinutes(starDataValues[StarSHAminutesIndex], $"{starName} SHA", out starsSHAm[starIndex]))
                return false;

            // Store SHA declination degrees and minutes, if degrees is < 10 then there is a space between the N or S character and the degrees number
            if (starDataValues.Length == ExpectedMinSpaceParts)
            {
                const int starDECsignIndex = 2;         // Index in starDataValues of where declination sign (N or S) is stored
                const int starDECdegreesIndex = 2;      // Index in starDataValues of Declination degrees value
                const int starDECminutesIndex = 3;      // Index in starDataValues of Declination minutes value
                if (!ParsingHelpers.TryParseDegrees(starDataValues[starDECdegreesIndex][1..], $"{starName} Dec", out starsDECd[starIndex])) // Exclude N or S character
                    return false;
                if (starDataValues[starDECsignIndex][0] == 'S')
                    starsDECd[starIndex] *= -1;         // Store south declinations as negative values
                if (!ParsingHelpers.TryParseMinutes(starDataValues[starDECminutesIndex], $"{starName} Dec", out starsDECm[starIndex]))
                    return false;
            }
            else if (starDataValues.Length == ExpectedMaxSpaceParts)
            {
                const int starDECsignIndex = 2;         // Index in starDataValues of where declination sign (N or S) is stored
                const int starDECdegreesIndex = 3;      // Index in starDataValues of Declination degrees value
                const int starDECminutesIndex = 4;      // Index in starDataValues of Declination minutes value
                if (!ParsingHelpers.TryParseDegrees(starDataValues[starDECdegreesIndex], $"{starName} Dec", out starsDECd[starIndex])) // Exclude N or S character
                    return false;
                if (starDataValues[starDECsignIndex][0] == 'S')
                    starsDECd[starIndex] *= -1;         // Store south declinations as negative values
                if (!ParsingHelpers.TryParseMinutes(starDataValues[starDECminutesIndex], $"{starName} Dec", out starsDECm[starIndex]))
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
    }
}
