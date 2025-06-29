namespace P3D_Scenario_Generator
{
    /// <summary>
    /// Provides helper methods for parsing and validating numerical string inputs,
    /// specifically for degrees and minutes values, with integrated error logging.
    /// </summary>
    internal class ParsingHelpers
    {
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
            if (degreesIntOut < 0 || degreesIntOut > Constants.MaxDegrees)
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
            if (minutesDoubleOut < 0 || minutesDoubleOut > Constants.MaxMinutes)
            {
                Log.Error($"{minutesName} minutes out of range in string: {minutesStringIn}");
                return false;
            }
            return true;
        }
    }
}
