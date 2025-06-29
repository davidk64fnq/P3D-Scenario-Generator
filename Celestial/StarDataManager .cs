using OfficeOpenXml;

namespace P3D_Scenario_Generator.Celestial
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
        /// Read in list of all stars from excel spreadsheet embedded resource, includes the creating a list of
        /// the navigational stars.
        /// </summary>
        /// <returns>True if the spreadsheet read in successfully</returns>
        static internal bool InitStars()
        {
            ExcelPackage.License.SetNonCommercialPersonal("David Kilpatrick");

            stars.Clear();
            navStarNames.Clear();
            noStars = 0;

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
