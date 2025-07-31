using OfficeOpenXml; 


namespace P3D_Scenario_Generator.CelestialScenario
{
    /// <summary>
    /// Manages the loading, storage, and retrieval of star data from an embedded Excel resource.
    /// It populates a list of all stars, identifies and organizes navigational stars,
    /// and provides methods to access individual star properties.
    /// </summary>
    internal class StarDataManager
    {
        /// <summary>
        /// Read in from embedded Excel resource and then written to "stars.dat"
        /// </summary>
        internal static readonly List<Star> stars = [];

        /// <summary>
        /// Read in from embedded Excel resource and then written to scenario html and javascript files
        /// </summary>
        internal static List<string> navStarNames = [];

        /// <summary>
        /// Stores the number of stars read in from embedded Excel resource
        /// </summary>
        internal static int noStars = 0;

        /// <summary>
        /// Reads in the list of all stars from an embedded Excel spreadsheet resource
        /// ("Excel.CelestialNavStars.xlsx") and populates the application's star data,
        /// including creating a list of navigational star names.
        /// </summary>
        /// <param name="progressReporter">Optional IProgress<string> for reporting progress or errors to the UI.</param>
        /// <returns>True if the spreadsheet is read in and processed successfully; otherwise, false.</returns>
        static internal bool InitStars(IProgress<string> progressReporter = null)
        {
            Log.Info("Starting initialization of star data from embedded Excel resource.");
            progressReporter?.Report("Initializing star data...");

            // Set EPPlus license for non-commercial use
            ExcelPackage.License.SetNonCommercialPersonal("David Kilpatrick");

            // Clear existing data before loading new data
            // Assuming 'stars' is a static List<Star> and 'navStarNames' is a static List<string>
            // and 'noStars' is a static int within StarDataManager.
            StarDataManager.stars.Clear();
            StarDataManager.navStarNames.Clear();
            StarDataManager.noStars = 0;

            string resourceName = "Excel.CelestialNavStars.xlsx";

            // Attempt to get the embedded resource stream using FileOps
            if (!FileOps.TryGetResourceStream(resourceName, progressReporter, out Stream stream))
            {
                Log.Error($"Failed to load embedded resource: '{resourceName}'. Star data initialization failed.");
                progressReporter?.Report($"ERROR: Failed to load '{resourceName}'.");
                return false; // Error already logged by FileOps.TryGetResourceStream
            }

            try
            {
                using (stream) // Ensure the stream is disposed after use by ExcelPackage
                using (ExcelPackage package = new(stream))
                {
                    // Assuming the relevant data is in the first worksheet (index 0)
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    if (worksheet == null)
                    {
                        Log.Error($"Worksheet not found in '{resourceName}'. Star data initialization failed.");
                        progressReporter?.Report("ERROR: Star data worksheet not found in Excel file.");
                        return false;
                    }

                    int index = 2; // Start from the second row (skip header row)
                                   // Continue as long as the first cell of the current row has a value
                    while (worksheet.Cells[index, 1].Value != null)
                    {
                        try
                        {
                            // Read values directly from Excel cells based on your confirmed mapping:
                            string constellation = Convert.ToString(worksheet.Cells[index, 1].Value);
                            string id = Convert.ToString(worksheet.Cells[index, 2].Value);
                            string connectedId = Convert.ToString(worksheet.Cells[index, 3].Value);
                            string starNumber = Convert.ToString(worksheet.Cells[index, 4].Value);
                            string starName = Convert.ToString(worksheet.Cells[index, 5].Value);
                            string wikiLink = Convert.ToString(worksheet.Cells[index, 6].Value);
                            string bayer = Convert.ToString(worksheet.Cells[index, 7].Value);
                            double raH = (double)worksheet.Cells[index, 8].Value;
                            double raM = (double)worksheet.Cells[index, 9].Value;
                            double raS = (double)worksheet.Cells[index, 10].Value;
                            double decD = (double)worksheet.Cells[index, 11].Value;
                            double decM = (double)worksheet.Cells[index, 12].Value;
                            double decS = (double)worksheet.Cells[index, 13].Value;
                            double visMag = (double)worksheet.Cells[index, 14].Value;

                            // Create a new Star object and add it to the list
                            StarDataManager.stars.Add(new Star(
                                constellation, id, connectedId, starNumber, starName,
                                wikiLink, bayer,
                                raH, raM, raS, decD, decM, decS, visMag
                            ));

                            // Add to navigational star names if Bayer value is present
                            if (!string.IsNullOrEmpty(bayer))
                            {
                                StarDataManager.navStarNames.Add(bayer);
                            }

                            StarDataManager.noStars++;
                        }
                        catch (Exception rowEx)
                        {
                            // Log an error for specific row parsing failures but try to continue
                            string errorMessage = $"Error parsing star data at row {index} in '{resourceName}'. Details: {rowEx.Message}";
                            Log.Warning(errorMessage);
                            progressReporter?.Report($"WARNING: {errorMessage}");
                        }
                        index++;
                    }
                }

                // Sort navigational star names after all stars have been processed
                StarDataManager.navStarNames.Sort();
                Log.Info($"Successfully initialized {StarDataManager.noStars} stars from '{resourceName}'.");
                progressReporter?.Report($"Star data loaded: {StarDataManager.noStars} stars.");
                return true;
            }
            catch (InvalidCastException ex)
            {
                // Catches errors when converting cell values to expected types (e.g., string to double)
                Log.Error($"Data type conversion error while reading '{resourceName}'. Check Excel data format. Details: {ex.Message}", ex);
                progressReporter?.Report($"ERROR: Data format issue in star data. See log for details.");
                return false;
            }
            catch (Exception ex)
            {
                // Catch any other unexpected errors during Excel package processing or data handling
                Log.Error($"An unexpected error occurred while processing '{resourceName}' for star data. Details: {ex.Message}", ex);
                progressReporter?.Report($"ERROR: Unexpected error during star data loading. See log for details.");
                return false;
            }
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
    }
}
