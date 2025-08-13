using P3D_Scenario_Generator.ConstantsEnums;

namespace P3D_Scenario_Generator.CelestialScenario
{
    /// <summary>
    /// A class to store celestial almanac data, including Aries GHA and navigational star data.
    /// This separates data storage from the data acquisition and parsing logic.
    /// </summary>
    internal class AlmanacData
    {
        /// <summary>
        /// The number of stars for which data extracted from web based almanac
        /// </summary>
        internal const int NoStarsInAlmanacData = 57;

        /// <summary>
        /// Stores 3 days x 24 hours of Aries GHA degrees star data extracted from web based almanac
        /// </summary>
        internal int[,] ariesGHAd = new int[Constants.AlmanacExtractDaysCount, Constants.HoursInADay];

        /// <summary>
        /// Stores 3 days x 24 hours of Aries GHA minutes star data extracted from web based almanac
        /// </summary>
        internal double[,] ariesGHAm = new double[Constants.AlmanacExtractDaysCount, Constants.HoursInADay];

        /// <summary>
        /// Stores NoStarsInAlmanacData SHA degrees star data extracted from web based almanac
        /// </summary>
        internal int[] starsSHAd = new int[NoStarsInAlmanacData];

        /// <summary>
        /// Stores NoStarsInAlmanacData SHA minutes star data extracted from web based almanac
        /// </summary>
        internal double[] starsSHAm = new double[NoStarsInAlmanacData];

        /// <summary>
        /// Stores NoStarsInAlmanacData Declination degrees star data extracted from web based almanac
        /// </summary>
        internal int[] starsDECd = new int[NoStarsInAlmanacData];

        /// <summary>
        /// Stores NoStarsInAlmanacData Declination minutes star data extracted from web based almanac
        /// </summary>
        internal double[] starsDECm = new double[NoStarsInAlmanacData];
    }
}
