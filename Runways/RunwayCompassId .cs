using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P3D_Scenario_Generator.Runways
{

    /// <summary>
    /// Represents a compass heading used as a special runway ID.
    /// </summary>
    /// <param name="Code">The unique code, a string from "37" to "52".</param>
    /// <param name="FullName">The full name, e.g., "Northwest-Southeast".</param>
    /// <param name="AbbrName">The abbreviated name, e.g., "NW-SE".</param>
    public record RunwayCompassId(string Code, string FullName, string AbbrName);


    /// <summary>
    /// A static utility class that provides a lookup map for the predefined runway compass IDs.
    /// </summary>
    internal static class RunwayCompassMap
    {
        private static readonly Dictionary<string, RunwayCompassId> _runwayCompassIds = new()
        {
            { "37", new("37", "North-South", "N-S") },
            { "38", new("38", "East-West", "E-W") },
            { "39", new("39", "Northwest-Southeast", "NW-SE") },
            { "40", new("40", "Southwest-Northeast", "SW-NE") },
            { "41", new("41", "South-North", "S-N") },
            { "42", new("42", "West-East", "W-E") },
            { "43", new("43", "Southeast-Northwest", "SE-NW") },
            { "44", new("44", "Northeast-Southwest", "NE-SW") },
            { "45", new("45", "North", "N") },
            { "46", new("46", "West", "W") },
            { "47", new("47", "Northwest", "NW") },
            { "48", new("48", "Southwest", "SW") },
            { "49", new("49", "South", "S") },
            { "50", new("50", "East", "E") },
            { "51", new("51", "Southeast", "SE") },
            { "52", new("52", "Northeast", "NE") },
        };

        /// <summary>
        /// Attempts to retrieve a RunwayCompassId from the map based on its code.
        /// </summary>
        /// <param name="code">The runway code to look up (e.g., "37").</param>
        /// <param name="compassId">When this method returns, contains the RunwayCompassId if the lookup was successful; otherwise, null.</param>
        /// <returns><c>true</c> if the compass ID was found; otherwise, <c>false</c>.</returns>
        public static bool TryGetCompassId(string code, out RunwayCompassId compassId)
        {
            return _runwayCompassIds.TryGetValue(code, out compassId);
        }
    }
}
