using OfficeOpenXml;
using P3D_Scenario_Generator.Interfaces;

namespace P3D_Scenario_Generator.CelestialScenario
{
    /// <summary>
    /// Manages the loading, storage, and retrieval of star data from an embedded Excel resource.
    /// It populates a list of all stars, identifies and organizes navigational stars,
    /// and provides methods to access individual star properties.
    /// </summary>
    public sealed class StarDataManager(ILogger logger, IFileOps fileOps, IProgress<string> progressReporter)
    {
        // Guard clauses to validate the constructor parameters.
        private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IFileOps _fileOps = fileOps ?? throw new ArgumentNullException(nameof(fileOps));
        private readonly IProgress<string> _progressReporter = progressReporter ?? throw new ArgumentNullException(nameof(progressReporter));

        /// <summary>
        /// Read in from embedded Excel resource and then written to "stars.dat"
        /// </summary>
        private readonly List<Star> _stars = [];

        /// <summary>
        /// Read in from embedded Excel resource and then written to scenario html and javascript files
        /// </summary>
        private readonly List<string> _navStarNames = [];

        /// <summary>
        /// Stores the number of stars read in from embedded Excel resource
        /// </summary>
        private int _noStars = 0;

        /// <summary>
        /// A read-only list of all stars.
        /// </summary>
        public IReadOnlyList<Star> Stars => _stars.AsReadOnly();

        /// <summary>
        /// A read-only list of navigational star names.
        /// </summary>
        public IReadOnlyList<string> NavStarNames => _navStarNames.AsReadOnly();

        /// <summary>
        /// The number of stars loaded.
        /// </summary>
        public int NoStars => _noStars;

        /// <summary>
        /// Initializes the star data from an embedded Excel resource.
        /// </summary>
        /// <returns><see langword="true"/> if the star data was successfully initialized; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> InitStarsAsync()
        {
            await _logger.InfoAsync("Starting initialization of star data from embedded Excel resource.");
            _progressReporter.Report("Initializing star data...");

            // Set EPPlus license for non-commercial use.
            // This should ideally be called once at application startup.
            ExcelPackage.License.SetNonCommercialPersonal("David Kilpatrick");

            _stars.Clear();
            _navStarNames.Clear();
            _noStars = 0;

            string resourceName = "Excel.CelestialNavStars.xlsx";

            // Attempt to get the embedded resource stream using FileOps
            (bool success, Stream stream) = await _fileOps.TryGetResourceStreamAsync(resourceName, _progressReporter);
            if (!success)
            {
                await _logger.ErrorAsync($"Failed to load embedded resource: '{resourceName}'. Star data initialization failed.");
                _progressReporter.Report($"ERROR: Failed to load '{resourceName}'.");
                return false; // Error already logged by FileOps.TryGetResourceStream
            }

            try
            {
                using (stream)
                using (ExcelPackage package = new(stream))
                {
                    // Assuming the relevant data is in the first worksheet (index 0)
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    if (worksheet == null)
                    {
                        await _logger.ErrorAsync($"Worksheet not found in '{resourceName}'. Star data initialization failed.");
                        _progressReporter.Report("ERROR: Star data worksheet not found in Excel file.");
                        return false;
                    }

                    int index = 2; // Start from the second row (skip header row)
                    while (worksheet.Cells[index, 1].Value != null)
                    {
                        try
                        {
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
                            _stars.Add(new Star(
                                constellation, id, connectedId, starNumber, starName,
                                wikiLink, bayer,
                                raH, raM, raS, decD, decM, decS, visMag
                            ));

                            // Add to navigational star names if Bayer value is present
                            if (!string.IsNullOrEmpty(bayer))
                            {
                                _navStarNames.Add(bayer);
                            }

                            _noStars++;
                        }
                        catch (Exception rowEx)
                        {
                            string errorMessage = $"Error parsing star data at row {index} in '{resourceName}'. Details: {rowEx.Message}";
                            await _logger.WarningAsync(errorMessage);
                            _progressReporter.Report($"WARNING: {errorMessage}");
                        }
                        index++;
                    }
                }

                // Sort navigational star names after all stars have been processed
                _navStarNames.Sort();
                await _logger.InfoAsync($"Successfully initialized {_noStars} stars from '{resourceName}'.");
                _progressReporter.Report($"Star data loaded: {_noStars} stars.");
                return true;
            }
            catch (InvalidCastException ex)
            {
                await _logger.ErrorAsync($"Data type conversion error while reading '{resourceName}'. Check Excel data format. Details: {ex.Message}", ex);
                _progressReporter.Report("ERROR: Data format issue in star data. See log for details.");
                return false;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"An unexpected error occurred while processing '{resourceName}' for star data. Details: {ex.Message}", ex);
                _progressReporter.Report("ERROR: Unexpected error during star data loading. See log for details.");
                return false;
            }
        }

        /// <summary>
        /// Retrieves a Star object from the internal 'stars' array at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the Star to retrieve.</param>
        /// <returns>The Star object located at the specified index.</returns>
        public Star GetStar(int index)
        {
            if (index < 0 || index >= _stars.Count)
            {
                // You might want to handle this with an exception or a different return type.
                // Throwing an exception is a common approach for out-of-bounds access.
                throw new IndexOutOfRangeException($"Index {index} is out of bounds for the stars collection.");
            }
            return _stars[index];
        }
    }
}
