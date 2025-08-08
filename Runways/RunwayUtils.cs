using System.Text.RegularExpressions;

namespace P3D_Scenario_Generator.Runways
{
    public static partial class RunwayUtils
    {
        // Use the GeneratedRegexAttribute to compile the regex at compile-time.
        // This method must be in a static partial class.
        [GeneratedRegex(@"^([A-Z0-9]{2,4})\s*(\((.*)\))?$")]
        private static partial Regex IcaoRunwayRegex();

        /// <summary>
        /// Parses a string formatted as "ICAOId (Number)" or "ICAOId" and returns the ICAO and runway Number.
        /// </summary>
        /// <param name="icaoRunwayString">The input string to parse.</param>
        /// <param name="icaoId">The extracted ICAO ID.</param>
        /// <param name="runwayNumber">The extracted runway Number (or an empty string if not present).</param>
        public static void ParseIcaoRunwayString(string icaoRunwayString, out string icaoId, out string runwayNumber)
        {
            // Initialize out parameters to empty strings to avoid nulls.
            icaoId = string.Empty;
            runwayNumber = string.Empty;

            // Use the generated Regex method.
            var match = IcaoRunwayRegex().Match(icaoRunwayString);

            if (match.Success)
            {
                // The first group (group 1) is the ICAO ID.
                icaoId = match.Groups[1].Value;

                // The third group (group 3) is the runway Number inside the parentheses.
                // It will be an empty string if there were no parentheses.
                if (match.Groups.Count > 3)
                {
                    runwayNumber = match.Groups[3].Value;
                }
            }
        }

        /// <summary>
        /// Formats a single RunwayParams object as an ICAO and runway Number string.
        /// </summary>
        /// <param name="runway">The RunwayParams object to format.</param>
        /// <returns>The formatted string, e.g., "ICAOId (Number)" or "ICAOId".</returns>
        public static string FormatRunwayIcaoString(RunwayParams runway)
        {
            if (runway == null)
            {
                return string.Empty;
            }

            // If the Runway Number is not null or empty, format as "ICAOId (Number)".
            if (!string.IsNullOrEmpty(runway.Number))
            {
                return $"{runway.IcaoId} ({runway.Number})";
            }
            // Otherwise, just return the ICAO ID.
            else
            {
                return runway.IcaoId;
            }
        }
    }
}
