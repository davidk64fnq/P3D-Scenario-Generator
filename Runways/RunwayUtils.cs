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
        /// Parses a string formatted as "ICAOId (Number)" or "ICAOId (Number Designator)" or just "ICAOId"
        /// and returns the ICAO, runway number, and designator.
        /// </summary>
        /// <param name="icaoRunwayString">The input string to parse.</param>
        /// <param name="icaoId">The extracted ICAO ID.</param>
        /// <param name="runwayNumber">The extracted runway number (or an empty string if not present).</param>
        /// <param name="runwayDesignator">The extracted runway designator (e.g., "Left", "Right", "Centre"), or "None" by default.</param>
        public static void ParseIcaoRunwayString(string icaoRunwayString, out string icaoId, out string runwayNumber, out string runwayDesignator)
        {
            // Initialize out parameters to empty strings to avoid nulls.
            icaoId = string.Empty;
            runwayNumber = string.Empty;
            runwayDesignator = "None";

            // Use the generated Regex method.
            var match = IcaoRunwayRegex().Match(icaoRunwayString);

            if (match.Success)
            {
                // The first group (group 1) is the ICAO ID.
                icaoId = match.Groups[1].Value;

                // The third group (group 3) is the content inside the parentheses.
                // It can be either the number or the number and designator.
                if (match.Groups.Count > 3)
                {
                    var contentInParentheses = match.Groups[3].Value;
                    var parts = contentInParentheses.Split(' ');
                    runwayNumber = parts[0];

                    if (parts.Length > 1)
                    {
                        runwayDesignator = parts[1];
                    }
                }
            }
        }

        /// <summary>
        /// Formats a single RunwayParams object as an ICAO and runway string.
        /// </summary>
        /// <param name="runway">The RunwayParams object to format.</param>
        /// <returns>The formatted string, e.g., "ICAOId", "ICAOId (Number)", or "ICAOId (Number Designator)".</returns>
        public static string FormatRunwayIcaoString(RunwayParams runway)
        {
            if (runway == null)
            {
                return string.Empty;
            }

            // If the runway number is not null or empty, format as "ICAOId (Number)".
            if (!string.IsNullOrEmpty(runway.Number))
            {
                // Add the designator if it exists.
                if (!string.IsNullOrEmpty(runway.Designator) && runway.Designator != "None")
                {
                    return $"{runway.IcaoId} ({runway.Number} {runway.Designator})";
                }
                else
                {
                    return $"{runway.IcaoId} ({runway.Number})";
                }
            }
            // Otherwise, just return the ICAO ID.
            else
            {
                return runway.IcaoId;
            }
        }

    }
}
