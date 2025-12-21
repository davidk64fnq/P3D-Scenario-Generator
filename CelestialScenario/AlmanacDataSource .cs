using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.Models;
using P3D_Scenario_Generator.Services;

namespace P3D_Scenario_Generator.CelestialScenario
{

    /// <summary>
    /// Manages the acquisition, parsing, and storage of celestial almanac data from a web-based source.
    /// This includes extracting Aries GHA (Greenwich Hour Angle) and navigational star
    /// SHA (Sidereal Hour Angle) and Declination values for use in celestial navigation calculations.
    /// </summary>
    internal class AlmanacDataSource(
        Logger logger,
        FormProgressReporter progressReporter,
        HttpRoutines httpRoutines,
        AlmanacData almanacData,
        ParsingHelpers parsingHelpers) 
    {
        private readonly Logger _logger = logger;
        private readonly FormProgressReporter _progressReporter = progressReporter;
        private readonly HttpRoutines _httpRoutines = httpRoutines;
        private readonly AlmanacData _almanacData = almanacData;
        private readonly ParsingHelpers _parsingHelpers = parsingHelpers;

        /// <summary>
        /// Using scenario date provided by user, obtain almanac data for three days, and extract Aries GHA degrees and minutes,
        /// and for the list of navigational stars SHA and Declination in degrees and minutes.
        /// </summary>
        /// <param name="formData">The scenario form data.</param>
        /// <returns><see langword="true"/> if the almanac data was retrieved and parsed successfully; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> GetAlmanacDataAsync(ScenarioFormData formData)
        {
            string message = "Starting almanac data retrieval...";
            await _logger.InfoAsync(message);
            _progressReporter.Report($"INFO: {message}");

            string almanacDataHtml = await DownloadAlmanacAsync(formData);

            if (almanacDataHtml is null)
            {
                return false;
            }

            if (!await ExtractAriesGHAAsync(almanacDataHtml))
            {
                return false;
            }

            if (!await ExtractStarDataAsync(almanacDataHtml))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Download the almanac data for the three days centered on the selected scenario date
        /// </summary>
        /// <param name="formData">The scenario form data, used to calculate the date range.</param>
        /// <returns>The webpage outerHTML containing the almanac data, or <see langword="null"/> if the download fails.</returns>
        internal async Task<string> DownloadAlmanacAsync(ScenarioFormData formData)
        {
            DateTime startDate = new(formData.DatePickerValue.Year, formData.DatePickerValue.Month, formData.DatePickerValue.Day,
                formData.TimePickerValue.Hour, formData.TimePickerValue.Minute, formData.TimePickerValue.Second, DateTimeKind.Local);
            startDate = startDate.AddDays(-1);
            string url = $"http://www.tecepe.com.br/scripts/AlmanacPagesISAPI.dll/pages?date={startDate.Month}%2F{startDate.Day}%2F{startDate.Year}";

            var htmlDoc = await _httpRoutines.GetWebDocAsync(url);

            if (htmlDoc is null)
            {
                await _logger.ErrorAsync($"Failed to download almanac data from URL: {url}");
                return null;
            }

            // Access OuterHtml only if the document is not null.
            return htmlDoc.DocumentNode.OuterHtml;
        }


        /// <summary>
        /// Extract the Aries GHA degrees and minutes data values for each hour of the three days
        /// </summary>
        /// <param name="almanacDataHtml">The raw data downloaded from the web</param>
        /// <returns><see langword="true"/> if successful in extracting Aries GHA degrees and minutes; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> ExtractAriesGHAAsync(string almanacDataHtml)
        {
            const int DegsAndMinsSegmentIndex = 1;
            const int Expected_Space_Separated_Segments = 2;
            const int StarGHAdegreesIndex = 0;
            const int StarGHAminutesIndex = 1;
            int day = 0, hour = 0;
            bool success;
            var hourDataLines = await GetAriesGHAdataBlockAsync(almanacDataHtml);
            if (hourDataLines != null)
            {
                for (int lineNo = 0; lineNo < hourDataLines.Length; lineNo++)
                {
                    if (ValidAriesGHAdataline(hourDataLines[lineNo], hour))
                    {
                        string[] pipes = hourDataLines[lineNo].Split('|');
                        string[] spaces = pipes[DegsAndMinsSegmentIndex].Trim().Split(' ');
                        if (spaces.Length < Expected_Space_Separated_Segments)
                        {
                            await _logger.ErrorAsync($"Malformed GHA data (degrees/minutes) in Aries GHA line {lineNo}: '{hourDataLines[lineNo]}'");
                            return false;
                        }

                        (success, _almanacData.ariesGHAd[day, hour]) = await _parsingHelpers.TryParseDegreesAsync(spaces[StarGHAdegreesIndex], "Aries GHA");
                        if (!success)
                            return false;

                        (success, _almanacData.ariesGHAm[day, hour++]) = await _parsingHelpers.TryParseMinutesAsync(spaces[StarGHAminutesIndex], "Aries GHA");
                        if (!success)
                            return false;

                        if (hour == Constants.HoursInADay) { hour = 0; day++; }
                    }
                }
                if (hour == 0 && day == Constants.AlmanacExtractDaysCount)
                    return true;
                else
                {
                    await _logger.ErrorAsync($"Unable to extract {Constants.AlmanacExtractDaysCount} days of {Constants.HoursInADay} hours Aries GHA degrees and minutes data from almanac data.");
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Extract degrees and minutes for the SHA and Declination of navigational stars
        /// </summary>
        /// <param name="almanacDataHtml">The raw data downloaded from the web</param>
        /// <returns><see langword="true"/> if successful in extracting SHA and Declination of navigational stars; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> ExtractStarDataAsync(string almanacDataHtml)
        {
            int starIndex = 0;
            string[] starNames = [ "Acamar", "Achernar", "Acrux", "Adhara", "Al Na-ir", "Aldebaran", "Alioth", "Alkaid", "Alnilam", "Alphard", "Alphecca",
                "Alpheratz", "Altair", "Ankaa", "Antares", "Arcturus", "Atria", "Avior", "Bellatrix", "Betelgeuse", "Canopus", "Capella", "Deneb",
                "Denebola", "Diphda", "Dubhe", "Elnath", "Eltanin", "Enif", "Fomalhaut", "Gacrux", "Gienah", "Hadar", "Hamal", "Kaus Austr.", "Kochab",
                "Markab", "Menkar", "Menkent", "Miaplacidus", "Mirfak", "Nunki", "Peacock", "Pollux", "Procyon", "Rasalhague", "Regulus", "Rigel",
                "Rigil Kent", "Sabik", "Schedar", "Shaula", "Sirius", "Spica", "Suhail", "Vega", "Zuben-ubi"];

            string[] almanacDataRows = almanacDataHtml.Split("\n");
            foreach (string starName in starNames)
            {
                if (!GetStarDataLine(almanacDataRows, starName, out string starDataLine))
                {
                    await _logger.ErrorAsync($"Unable to locate exactly one line of data for star name \"{starName}\" in almanac data.");
                    return false;
                }

                string[] starDataValues = await GetStarDataValuesAsync(starDataLine);
                if (starDataValues is null)
                {
                    await _logger.ErrorAsync($"Unable to locate the star data values for star name \"{starName}\" in almanac data.");
                    return false;
                }

                if (!await TryParseStarDataValues(starDataValues, starIndex, starName))
                {
                    await _logger.ErrorAsync($"Unable to parse the star data values for star name \"{starName}\" in almanac data.");
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
        /// <param name="almanacDataHtml">The complete raw almanac data as a single string.</param>
        /// <returns>An array of strings containing the Aries GHA data block, or <see langword="null"/> if the block cannot be identified.</returns>
        internal async Task<string[]> GetAriesGHAdataBlockAsync(string almanacDataHtml)
        {
            bool success;

            string[] hourDataLines = almanacDataHtml.Split("\n");

            // Check Aries GHA data block header line present
            (success, int firstBlockHeaderIndex) = await HeaderLinePresentAsync(hourDataLines, Constants.FirstGhaBlockKeywords);
            if (!success)
            {
                return null;
            }

            // Check second non Aries GHA data block header line present
            (success, int secondBlockHeaderIndex) = await HeaderLinePresentAsync(hourDataLines, Constants.SecondGhaBlockKeywords);
            if (!success)
            {
                return null;
            }

            // Check there is atleast 72 rows of data in first block being 3 days x 24 hours per day
            if (firstBlockHeaderIndex + Constants.AlmanacExtractDaysCount * Constants.HoursInADay < secondBlockHeaderIndex)
            {
                hourDataLines = hourDataLines[firstBlockHeaderIndex..secondBlockHeaderIndex];
                return hourDataLines;
            }
            else
            {
                await _logger.ErrorAsync($"There is less than 72 ({Constants.AlmanacExtractDaysCount} days x {Constants.HoursInADay} hours) rows of data in the Aries GHA block " +
                    "within the downloaded almanac data");
                return null;
            }
        }

        /// <summary>
        /// Checks if a header line containing all specified keywords (case-sensitive) is present
        /// within the provided array of data lines.
        /// </summary>
        /// <param name="hourDataLines">The array of strings to search within.</param>
        /// <param name="keywords">The collection of keywords to search for.</param>
        /// <returns>
        /// A value tuple indicating success and the zero-based index of the found header line.
        /// Returns <see langword="true"/> and the index if a match is found; otherwise, returns <see langword="false"/> and -1.
        /// </returns>
        internal async Task<(bool success, int headerLineIndex)> HeaderLinePresentAsync(string[] hourDataLines, IEnumerable<string> keywords)
        {
            int headerLineIndex;

            var matchingLineInfo = hourDataLines
                .Select((line, index) => new { Line = line, Index = index })
                .FirstOrDefault(item =>
                    // Check if the current line contains ALL keywords in the 'keywords' collection
                    keywords.All(keyword => item.Line.Contains(keyword))
                );

            if (matchingLineInfo is not null)
            {
                headerLineIndex = matchingLineInfo.Index;
                return (true, headerLineIndex);
            }
            else
            {
                headerLineIndex = -1;
                // Updated logging to display all keywords from the collection
                await _logger.ErrorAsync($"Header line containing '{string.Join("', '", keywords)}' not found in almanac data.");
                return (false, headerLineIndex);
            }
        }

        /// <summary>
        /// Validates if a given string line is a well-formed Aries GHA data line
        /// by checking its pipe-separated structure and if the embedded hour matches the expected next hour.
        /// </summary>
        /// <param name="potentialHourDataLine">The string line from the almanac data to validate.</param>
        /// <param name="nextHour">The expected hour value (0-23) that the data line should contain.</param>
        /// <returns>
        /// <see langword="true"/> if the line contains at least two pipe-separated parts, and the last
        /// space-delimited value in the first part matches the <paramref name="nextHour"/>;
        /// otherwise, <see langword="false"/>. No error is logged if validation fails, as non-data lines are expected.
        /// </returns>
        static internal bool ValidAriesGHAdataline(string potentialHourDataLine, int nextHour)
        {
            const int Expected_Min_Pipe_Separated_Parts = 2;
            const int HourSegmentIndex = 0;

            string[] pipes = potentialHourDataLine.Split('|');
            if (pipes.Length >= Expected_Min_Pipe_Separated_Parts)
            {
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
        /// otherwise, it is set to <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if exactly one line containing the specified star's data is found;
        /// <see langword="false"/> if no lines are found, or if multiple lines for the same star are found (indicating ambiguity).
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
        /// <returns>An array of strings containing SHA and Declination values, or <see langword="null"/> if the line is malformed.</returns>
        internal async Task<string[]> GetStarDataValuesAsync(string starDataLine)
        {
            const int ExpectedMinPipeParts = 2; // Expecting 3 pipes, resulting in 4 parts for all but Pollux which is 1 pipe and 2 parts
            const int LastPipeSegmentMinLength = 12; // Minimum length for last segment before stripping
            const int DataSubstringStartIndex = 12; // Index to start extracting data from last segment
            const int ExpectedMinSpaceParts = 4;
            const int ExpectedMaxSpaceParts = 5;

            string[] pipes = starDataLine.Split('|');
            if (pipes.Length < ExpectedMinPipeParts)
            {
                await _logger.ErrorAsync($"Malformed star data line '{starDataLine}'. Expected a minimum of {ExpectedMinPipeParts - 1} pipe symbols, found {pipes.Length - 1}.");
                return null;
            }

            string lastPipeSegment = pipes[^1];
            if (lastPipeSegment.Length < LastPipeSegmentMinLength)
            {
                await _logger.ErrorAsync($"Malformed star data line '{starDataLine}'. Last data segment '{lastPipeSegment}' is too short (expected at least {LastPipeSegmentMinLength} chars).");
                return null;
            }

            // Star data occurs after the initial part of the last pipe segment
            string dataSubstring = lastPipeSegment[DataSubstringStartIndex..];
            string[] spaces = dataSubstring.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            if (spaces.Length < ExpectedMinSpaceParts || spaces.Length > ExpectedMaxSpaceParts)
            {
                await _logger.ErrorAsync($"Malformed star data line '{starDataLine}'. Expected {ExpectedMinSpaceParts}-{ExpectedMaxSpaceParts} space-separated values for SHA/Declination data, found {spaces.Length}. Data: '{dataSubstring}'.");
                return null;
            }

            return spaces;
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
        /// <returns><see langword="true"/> if all SHA and Declination values are successfully parsed and validated; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> TryParseStarDataValues(string[] starDataValues, int starIndex, string starName)
        {
            const int StarSHAdegreesIndex = 0;
            const int StarSHAminutesIndex = 1;
            const int ExpectedMinSpaceParts = 4;
            const int ExpectedMaxSpaceParts = 5;
            bool success;

            // Store SHA degrees and minutes for current star
            (success, _almanacData.starsSHAd[starIndex]) = await _parsingHelpers.TryParseDegreesAsync(starDataValues[StarSHAdegreesIndex], $"{starName} SHA");
            if (!success)
                return false;

            (success, _almanacData.starsSHAm[starIndex]) = await _parsingHelpers.TryParseMinutesAsync(starDataValues[StarSHAminutesIndex], $"{starName} SHA");
            if (!success)
                return false;

            // Store SHA declination degrees and minutes, if degrees is < 10 then there is a space between the N or S character and the degrees number
            if (starDataValues.Length == ExpectedMinSpaceParts)
            {
                const int starDECsignIndex = 2;
                const int starDECdegreesIndex = 2;
                const int starDECminutesIndex = 3;

                (success, _almanacData.starsDECd[starIndex]) = await _parsingHelpers.TryParseDegreesAsync(starDataValues[starDECdegreesIndex][1..], $"{starName} Dec"); // Exclude N or S character
                if (!success) 
                    return false;
                if (starDataValues[starDECsignIndex][0] == 'S')
                    _almanacData.starsDECd[starIndex] *= -1;
                (success, _almanacData.starsDECm[starIndex]) = await _parsingHelpers.TryParseMinutesAsync(starDataValues[starDECminutesIndex], $"{starName} Dec");
                if (!success)
                    return false;
            }
            else if (starDataValues.Length == ExpectedMaxSpaceParts)
            {
                const int starDECsignIndex = 2;
                const int starDECdegreesIndex = 3;
                const int starDECminutesIndex = 4;

                (success, _almanacData.starsDECd[starIndex]) = await _parsingHelpers.TryParseDegreesAsync(starDataValues[starDECdegreesIndex], $"{starName} Dec"); // Exclude N or S character
                if (!success) 
                    return false;
                if (starDataValues[starDECsignIndex][0] == 'S')
                    _almanacData.starsDECd[starIndex] *= -1;
                (success, _almanacData.starsDECm[starIndex]) = await _parsingHelpers.TryParseMinutesAsync(starDataValues[starDECminutesIndex], $"{starName} Dec");
                if (!success)
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
        /// <returns>The extracted star name if found and valid; otherwise, returns <see langword="null"/>.</returns>
        static internal string ExtractStarName(string line)
        {
            // Find the last occurrence of "| "
            int lastPipeIndex = line.LastIndexOf("| ");

            if (lastPipeIndex != -1 && lastPipeIndex + 2 < line.Length)
            {
                int startIndex = lastPipeIndex + 2;
                int length = Math.Min(12, line.Length - startIndex);

                string potentialStarName = line[startIndex..(startIndex + length)].Trim();

                // Validate if it looks like a star name (e.g., not just numbers or empty)
                if (!string.IsNullOrEmpty(potentialStarName) && !char.IsDigit(potentialStarName[0]))
                {
                    return potentialStarName;
                }
            }
            return null;
        }
    }
}
