namespace P3D_Scenario_Generator.Runways
{
    internal class RunwaySearcher(RunwayData data)
    {
        private readonly RunwayData _data = data;
        private static readonly Random _random = new();

        /// <summary>
        /// Builds a list of strings in the format "Airport ICAO (Runway Id)" e.g. "LFGO (14L)"
        /// </summary>
        /// <returns>The list of formatted runway strings</returns>
        public List<string> GetICAOids()
        {
            return [.. _data.Runways.Select(FormatRunwayString)];
        }

        /// <summary>
        /// Finds the index of a runway based on its formatted string representation.
        /// </summary>
        /// <param name="runwayString">The formatted string of the runway to search for.</param>
        /// <returns>The zero-based index of the matching runway, or -1 if no match is found.</returns>
        public int FindRunwayIndexByString(string runwayString)
        {
            if (string.IsNullOrEmpty(runwayString) || _data.Runways == null)
            {
                return -1;
            }

            for (int i = 0; i < _data.Runways.Count; i++)
            {
                if (FormatRunwayString(_data.Runways[i]).Equals(runwayString, StringComparison.Ordinal))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Generates and returns a single, randomly selected formatted runway string.
        /// </summary>
        public string GetRandomICAOid()
        {
            if (_data.Runways == null || _data.Runways.Count == 0)
            {
                return string.Empty;
            }

            int randomIndex = _random.Next(0, _data.Runways.Count);
            return FormatRunwayString(_data.Runways[randomIndex]);
        }

        /// <summary>
        /// Get a sorted list of the country strings in "runways.xml"
        /// </summary>
        /// <returns>Sorted list of the country strings in "runways.xml"</returns>
        public List<string> GetRunwayCountries()
        {
            List<string> countries = [.. _data.Runways
                .Where(r => !string.IsNullOrEmpty(r.Country))
                .Select(r => r.Country)
                .Distinct()
                .OrderBy(c => c)];

            countries.Insert(0, "None");
            return countries;
        }

        /// <summary>
        /// Get a sorted list of the state strings in "runways.xml"
        /// </summary>
        /// <returns>Sorted list of the state strings in "runways.xml"</returns>
        public List<string> GetRunwayStates()
        {
            List<string> states = [.. _data.Runways
                .Where(r => !string.IsNullOrEmpty(r.State))
                .Select(r => r.State)
                .Distinct()
                .OrderBy(s => s)];

            states.Insert(0, "None");
            return states;
        }

        /// <summary>
        /// Get a sorted list of the city strings in "runways.xml"
        /// </summary>
        /// <returns>Sorted list of the city strings in "runways.xml"</returns>
        public List<string> GetRunwayCities()
        {
            // Use LINQ to get distinct, non-empty cities, sort them, and convert to a list.
            List<string> cities = [.. _data.Runways
                .Where(r => !string.IsNullOrEmpty(r.City))
                .Select(r => r.City)
                .Distinct()
                .OrderBy(c => c)];

            cities.Insert(0, "None");
            return cities;
        }

        /// <summary>
        /// Helper method to format a runway into its string representation.
        /// </summary>
        /// <param name="runway">The runway to format.</param>
        /// <returns>The formatted runway string.</returns>
        private static string FormatRunwayString(RunwayParams runway)
        {
            if (int.TryParse(runway.Number, out int runwayNumber) && runwayNumber <= 36)
            {
                return $"{runway.IcaoId} ({runway.Id})";
            }
            else
            {
                string runwayId = $"{runway.IcaoId} ({runway.Number})";
                if (!string.IsNullOrEmpty(runway.Designator) && runway.Designator != "None")
                {
                    runwayId = $"{runwayId}[{runway.Designator[..1]}]";
                }
                return runwayId;
            }
        }
    }
}
