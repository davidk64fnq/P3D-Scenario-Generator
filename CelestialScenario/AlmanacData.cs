using P3D_Scenario_Generator.ConstantsEnums;

namespace P3D_Scenario_Generator.CelestialScenario
{
    public record NavStarData(
        double SHADegrees,
        double SHAMinutes,
        double DECdegrees,
        double DECMinutes,
        string NavStarName
    );

    /// <summary>
    /// A class to store celestial almanac data, including Aries GHA and navigational star data.
    /// </summary>
    public class AlmanacData
    {
        /// <summary>
        /// The number of stars for which data extracted from web based almanac
        /// </summary>
        public const int NoStarsInAlmanacData = 57;

        /// <summary>
        /// Stores 3 days x 24 hours of Aries GHA degrees star data extracted from web based almanac
        /// </summary>
        public int[,] ariesGHAd = new int[Constants.AlmanacExtractDaysCount, Constants.HoursInADay];

        /// <summary>
        /// Stores 3 days x 24 hours of Aries GHA minutes star data extracted from web based almanac
        /// </summary>
        public double[,] ariesGHAm = new double[Constants.AlmanacExtractDaysCount, Constants.HoursInADay];

        /// <summary>
        /// Stores NoStarsInAlmanacData SHA degrees star data extracted from web based almanac
        /// </summary>
        public int[] starsSHAd = new int[NoStarsInAlmanacData];

        /// <summary>
        /// Stores NoStarsInAlmanacData SHA minutes star data extracted from web based almanac
        /// </summary>
        public double[] starsSHAm = new double[NoStarsInAlmanacData];

        /// <summary>
        /// Stores NoStarsInAlmanacData Declination degrees star data extracted from web based almanac
        /// </summary>
        public int[] starsDECd = new int[NoStarsInAlmanacData];

        /// <summary>
        /// Stores NoStarsInAlmanacData Declination minutes star data extracted from web based almanac
        /// </summary>
        public double[] starsDECm = new double[NoStarsInAlmanacData];

        /// <summary>
        /// Converts a multi-dimensional array ([,]) into a jagged array ([][]) 
        /// suitable for System.Text.Json serialization.
        /// </summary>
        private static T[][] Convert2DArrayToJagged<T>(T[,] array)
        {
            int rows = array.GetLength(0);
            int cols = array.GetLength(1);
            T[][] jaggedArray = new T[rows][];

            for (int i = 0; i < rows; i++)
            {
                T[] row = new T[cols];
                for (int j = 0; j < cols; j++)
                {
                    row[j] = array[i, j];
                }
                jaggedArray[i] = row;
            }
            return jaggedArray;
        }

        /// <summary>
        /// Gets 3 days x 24 hours of Aries GHA degrees star data (as a jagged array for JSON serialization).
        /// </summary>
        public int[][] AriesGhaDeg => Convert2DArrayToJagged(ariesGHAd);

        /// <summary>
        /// Gets 3 days x 24 hours of Aries GHA minutes star data (as a jagged array for JSON serialization).
        /// </summary>
        public double[][] AriesGhaMin => Convert2DArrayToJagged(ariesGHAm);
    }
}
