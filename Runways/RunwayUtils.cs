using System.Text.RegularExpressions;

namespace P3D_Scenario_Generator.Runways
{
    /// <summary>
    /// Provides utility methods for parsing and formatting runway and airport identifiers.
    /// </summary>
    internal static partial class RunwayUtils
    {
        [GeneratedRegex(@"^([A-Z0-9]{2,4})\s*(\((.*)\))?$")]
        private static partial Regex IcaoRunwayRegex();

        /// <summary>
        /// Parses an ICAO runway composite string (e.g., "KSEA (16L)", "EGLL (27R)", or "KJFK")
        /// into its component ICAO ID, runway number, and runway designator.
        /// </summary>
        /// <param name="icaoRunwayString">The input string to parse.</param>
        /// <param name="icaoId">When this method returns, contains the extracted ICAO ID, or an empty string.</param>
        /// <param name="runwayNumber">When this method returns, contains the extracted runway number, or an empty string.</param>
        /// <param name="runwayDesignator">When this method returns, contains the extracted runway designator (e.g. "Left", "Right"), or "None".</param>
        internal static void ParseIcaoRunwayString(string icaoRunwayString, out string icaoId, out string runwayNumber, out string runwayDesignator)
        {
            icaoId = string.Empty;
            runwayNumber = string.Empty;
            runwayDesignator = "None";

            var match = IcaoRunwayRegex().Match(icaoRunwayString);

            if (match.Success)
            {
                icaoId = match.Groups[1].Value;

                if (match.Groups.Count > 3)
                {
                    string contentInParentheses = match.Groups[3].Value;
                    string[] parts = contentInParentheses.Split(' ');
                    runwayNumber = parts[0];

                    if (parts.Length > 1)
                    {
                        runwayDesignator = parts[1];
                    }
                }
            }
        }

        /// <summary>
        /// Formats a <see cref="RunwayParams"/> object into a standard ICAO runway composite string.
        /// </summary>
        /// <param name="runway">The runway parameters object to format.</param>
        /// <returns>A formatted string such as "ICAOId", "ICAOId (Number)", or "ICAOId (Number Designator)".</returns>
        internal static string FormatRunwayIcaoString(RunwayParams runway)
        {
            if (runway is null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(runway.Number))
            {
                if (!string.IsNullOrEmpty(runway.Designator) && runway.Designator != "None")
                {
                    return $"{runway.IcaoId} ({runway.Number} {runway.Designator})";
                }

                return $"{runway.IcaoId} ({runway.Number})";
            }

            return runway.IcaoId;
        }
    }
}