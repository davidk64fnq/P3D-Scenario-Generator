using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.Legacy;

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
            if (degreesIntOut < 0 || degreesIntOut > Constants.DegreesInACircle)
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
            if (minutesDoubleOut < 0 || minutesDoubleOut > Constants.MinutesInAnHour)
            {
                Log.Error($"{minutesName} minutes out of range in string: {minutesStringIn}");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Attempts to parse a string into a double and validates its range (inclusive).
        /// Returns an error message if parsing fails or if the value is out of the valid range.
        /// </summary>
        /// <param name="valueStringIn">The string containing the double value to parse.</param>
        /// <param name="valueName">A descriptive name for the value (e.g., "Circuit Upwind Leg") used in error messages.</param>
        /// <param name="minValue">The minimum allowed value (inclusive).</param>
        /// <param name="maxValue">The maximum allowed value (inclusive).</param>
        /// <param name="doubleOut">When this method returns, contains the parsed double value if successful; otherwise, 0.</param>
        /// <param name="errorMessage">When this method returns, contains an error message if parsing fails or the value is out of range; otherwise, null or empty.</param>
        /// <param name="units">Optional: A string representing the units of the value (e.g., "miles", "knots").</param>
        /// <returns>True if the string was successfully parsed into a double and is within the specified range; otherwise, false.</returns>
        // Modified method with 'units' parameter
        internal static bool TryParseDouble(
            string valueStringIn,
            string valueName,
            double minValue,
            double maxValue,
            out double doubleOut,
            out string errorMessage,
            string units = "") 
        {
            errorMessage = null; // Initialize error message

            if (!double.TryParse(valueStringIn, out doubleOut))
            {
                errorMessage = $"Invalid format for '{valueName}'. Please enter a valid number.";
                return false;
            }

            // Append units to the error message if provided
            string unitSuffix = string.IsNullOrWhiteSpace(units) ? "" : $" {units}";

            if (doubleOut < minValue || doubleOut > maxValue)
            {
                errorMessage = $"{valueName} ({doubleOut}{unitSuffix}) is out of range. Please enter a value between {minValue}{unitSuffix} and {maxValue}{unitSuffix}.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Attempts to parse a string into an integer and validates its range (inclusive).
        /// Returns an error message if parsing fails or if the value is out of the valid range.
        /// </summary>
        /// <param name="valueStringIn">The string containing the integer value to parse.</param>
        /// <param name="valueName">A descriptive name for the value (e.g., "Altitude") used in error messages.</param>
        /// <param name="minValue">The minimum allowed value (inclusive).</param>
        /// <param name="maxValue">The maximum allowed value (inclusive).</param>
        /// <param name="intOut">When this method returns, contains the parsed integer value if successful; otherwise, 0.</param>
        /// <param name="errorMessage">When this method returns, contains an error message if parsing fails or the value is out of range; otherwise, null or empty.</param>
        /// <param name="units">Optional: A string representing the units of the value (e.g., "feet", "meters").</param>
        /// <returns>True if the string was successfully parsed into an integer and is within the specified range; otherwise, false.</returns>
        internal static bool TryParseInteger(
            string valueStringIn,
            string valueName,
            int minValue,
            int maxValue,
            out int intOut,
            out string errorMessage,
            string units = "")
        {
            errorMessage = null; // Initialize error message

            if (!int.TryParse(valueStringIn, out intOut))
            {
                errorMessage = $"Invalid format for '{valueName}'. Please enter a valid whole number.";
                return false;
            }

            // Append units to the error message if provided
            string unitSuffix = string.IsNullOrWhiteSpace(units) ? "" : $" {units}";

            if (intOut < minValue || intOut > maxValue)
            {
                errorMessage = $"{valueName} ({intOut}{unitSuffix}) is out of range. Please enter a value between {minValue}{unitSuffix} and {maxValue}{unitSuffix}.";
                return false;
            }

            return true;
        }
    }
}
