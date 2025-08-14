using P3D_Scenario_Generator.ConstantsEnums;

namespace P3D_Scenario_Generator.CelestialScenario
{
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
    }
}
