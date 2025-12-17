using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.Models;
using P3D_Scenario_Generator.Services;

namespace P3D_Scenario_Generator.Runways
{
    /// <summary>
    /// Manages and searches through a collection of runway data.
    /// This version uses a k-d tree for efficient nearest and nearby neighbor searches,
    /// applying filters dynamically during the search rather than on a pre-filtered subset.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the RunwaySearcher class with a pre-built k-d tree.
    /// The class now focuses solely on data retrieval and searching, with all UI-specific
    /// formatting logic moved to a presentation layer.
    /// </remarks>
    /// <param name="data">The complete runway data object containing all runways and the KD-tree root.</param>
    public class RunwaySearcher(RunwayData data, Logger log)
    {
        // Holds the complete list of all runways loaded from the data source.
        // We ensure _allRunways is never null.
        private readonly List<RunwayParams> _allRunways = data?.Runways ?? [];

        // We ensure _kdTreeRoot is never null to prevent NullReferenceExceptions.
        private readonly KDNode _kdTreeRoot = data?.RunwayTreeRoot ?? new KDNode();

        // The ILog interface for logging errors and other messages.
        private readonly Logger _log = log;

        // Using a thread-safe random number generator for robustness in a multi-threaded context.
        private static readonly Random _random = Random.Shared;

        /// <summary>
        /// Finds the nearest runway to a given point from the complete list,
        /// applying location filters dynamically using a k-d tree search.
        /// </summary>
        /// <param name="targetLat">The latitude of the target point.</param>
        /// <param name="targetLon">The longitude of the target point.</param>
        /// <param name="scenarioFormData">The DTO containing location filters.</param>
        /// <returns>The nearest runway that meets the filter criteria, or null if no match is found.</returns>
        public async Task<RunwayParams> FindNearestRunwayAsync(double targetLat, double targetLon, ScenarioFormData scenarioFormData)
        {
            try
            {
                RunwayParams best = null;
                double bestDistSq = double.MaxValue;
                FindNearestRecursive(_kdTreeRoot, targetLat, targetLon, runway => IsRunwayInFilteredLocation(runway, scenarioFormData), 0, ref best, ref bestDistSq);
                return best;
            }
            catch (Exception ex)
            {
                await _log.ErrorAsync($"An error occurred while finding the nearest runway to lat: {targetLat}, lon: {targetLon}", ex);
                return null;
            }
        }

        /// <summary>
        /// Finds a runway within a specified distance range from a target point,
        /// applying location filters dynamically using a k-d tree range search.
        /// The search returns a random result from the matching runways.
        /// </summary>
        /// <param name="targetLat">The latitude of the target point.</param>
        /// <param name="targetLon">The longitude of the target point.</param>
        /// <param name="minDist">The minimum distance the runway can be from the target point in nautical miles.</param>
        /// <param name="maxDist">The maximum distance the runway can be from the target point in nautical miles.</param>
        /// <param name="scenarioFormData">The DTO containing location filters.</param>
        /// <returns>A runway that meets the distance and filter criteria, or null if no match is found.</returns>
        public async Task<RunwayParams> FindNearbyRunwayAsync(double targetLat, double targetLon, double minDist, double maxDist, ScenarioFormData scenarioFormData)
        {
            try
            {
                List<RunwayParams> nearbyRunways = [];
                double minSq = minDist / Constants.NMInDegreeOfLatitude * minDist / Constants.NMInDegreeOfLatitude;
                double maxSq = maxDist / Constants.NMInDegreeOfLatitude * maxDist / Constants.NMInDegreeOfLatitude;
                FindInRangeRecursive(_kdTreeRoot, targetLat, targetLon, minSq, maxSq, runway => IsRunwayInFilteredLocation(runway, scenarioFormData), 0, nearbyRunways);

                if (nearbyRunways.Count == 0)
                {
                    return null;
                }

                int randomIndex = _random.Next(0, nearbyRunways.Count);
                return nearbyRunways[randomIndex];
            }
            catch (Exception ex)
            {
                await _log.ErrorAsync($"An error occurred while finding a nearby runway to lat: {targetLat}, lon: {targetLon}", ex);
                return null;
            }
        }

        /// <summary>
        /// Gets a single, randomly selected runway object from the complete list that meets the specified filter criteria.
        /// </summary>
        /// <param name="scenarioFormData">The DTO containing location filters to apply.</param>
        /// <returns>A randomly selected RunwayParams object that meets the filter criteria, or null if no matching runways are found.</returns>
        public async Task<RunwayParams> GetFilteredRandomRunwayAsync(ScenarioFormData scenarioFormData)
        {
            try
            {
                List<RunwayParams> filteredRunways = [.. _allRunways.Where(runway => IsRunwayInFilteredLocation(runway, scenarioFormData))];

                if (filteredRunways.Count == 0)
                {
                    await _log.InfoAsync("No runways found that match the specified filters.");
                    return null;
                }

                int randomIndex = _random.Next(0, filteredRunways.Count);
                return filteredRunways[randomIndex];
            }
            catch (Exception ex)
            {
                await _log.ErrorAsync($"An error occurred while getting a random filtered runway.", ex);
                return null;
            }
        }

        /// <summary>
        /// Returns the complete list of all runway data objects.
        /// The consumer of this data is responsible for any necessary formatting.
        /// </summary>
        /// <returns>The complete list of all runway data objects.</returns>
        public List<RunwayParams> GetAllRunways()
        {
            return _allRunways;
        }

        /// <summary>
        /// Finds a specific runway by its ICAO ID, runway ID and runway designator.
        /// </summary>
        /// <param name="icaoId">The ICAO ID of the airport.</param>
        /// <param name="runwayId">The ID of the runway (e.g., "14", "26").</param>
        /// <param name="runwayDesignator">The designator of the runway (e.g., "Left", "Centre").</param>
        /// <returns>The matching RunwayParams object, or null if not found.</returns>
        public RunwayParams GetRunwayByIcaoIdDesignator(string icaoId, string runwayId, string runwayDesignator)
        {
            if (string.IsNullOrEmpty(icaoId) || string.IsNullOrEmpty(runwayId) || _allRunways == null)
            {
                return null;
            }

            // Using LINQ's FirstOrDefault for a clean search.
            return _allRunways.FirstOrDefault(r =>
                r.IcaoId.Equals(icaoId, StringComparison.OrdinalIgnoreCase) &&
                r.Number.Equals(runwayId, StringComparison.OrdinalIgnoreCase) &&
                r.Designator.Equals(runwayDesignator, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets a single, randomly selected runway object from the complete list.
        /// </summary>
        public RunwayParams GetRandomRunway()
        {
            if (_allRunways == null || _allRunways.Count == 0)
            {
                return null;
            }

            int randomIndex = _random.Next(0, _allRunways.Count);
            return _allRunways[randomIndex];
        }

        /// <summary>
        /// Gets a single runway object by its index in the internal list.
        /// Performs a bounds check to prevent an IndexOutOfRangeException.
        /// </summary>
        /// <param name="index">The zero-based index of the runway to retrieve.</param>
        /// <returns>The RunwayParams object at the specified index, or null if the index is out of bounds.</returns>
        public async Task<RunwayParams> GetRunwayByIndexAsync(int index)
        {
            if (index >= 0 && index < _allRunways.Count)
            {
                // Use a LINQ query to find the first runway where the RunwaysIndex property matches.
                var result = _allRunways.FirstOrDefault(r => r.RunwaysIndex == index);

                if (result == null)
                {
                    await _log.WarningAsync($"Could not find runway with RunwaysIndex of {index}.");
                }

                return result; ;
            }

            await _log.WarningAsync($"Attempted to access runway at index {index}, which is out of bounds (list size: {_allRunways.Count}).");
            return null;
        }


        /// <summary>
        /// Gets a sorted list of the country strings from the full list of runways.
        /// </summary>
        /// <returns>Sorted list of the country strings in "runways.xml"</returns>
        public List<string> GetRunwayCountries()
        {
            List<string> countries = [.. _allRunways
                .Where(r => !string.IsNullOrEmpty(r.Country))
                .Select(r => r.Country)
                .Distinct()
                .OrderBy(c => c)];

            countries.Insert(0, "None");
            return countries;
        }

        /// <summary>
        /// Gets a sorted list of the state strings from the full list of runways.
        /// </summary>
        /// <returns>Sorted list of the state strings in "runways.xml"</returns>
        public List<string> GetRunwayStates()
        {
            List<string> states = [.. _allRunways
                .Where(r => !string.IsNullOrEmpty(r.State))
                .Select(r => r.State)
                .Distinct()
                .OrderBy(s => s)];

            states.Insert(0, "None");
            return states;
        }

        /// <summary>
        /// Gets a sorted list of the city strings from the full list of runways.
        /// </summary>
        /// <returns>Sorted list of the city strings in "runways.xml"</returns>
        public List<string> GetRunwayCities()
        {
            List<string> cities = [.. _allRunways
                .Where(r => !string.IsNullOrEmpty(r.City))
                .Select(r => r.City)
                .Distinct()
                .OrderBy(c => c)];

            cities.Insert(0, "None");
            return cities;
        }

        /// <summary>
        /// Helper method to check if a runway meets the location filter criteria.
        /// </summary>
        /// <param name="runway">The runway to check.</param>
        /// <param name="scenarioFormData">The ScenarioFormData object with filter lists.</param>
        /// <returns>True if the runway meets the criteria, false otherwise.</returns>
        private static bool IsRunwayInFilteredLocation(RunwayParams runway, ScenarioFormData scenarioFormData)
        {
            // A filter is considered a match if the list is null, empty,
            // or contains the special "None" value.
            bool countriesMatch = (scenarioFormData.LocationCountries == null ||
                                   scenarioFormData.LocationCountries.Count == 0 ||
                                   scenarioFormData.LocationCountries.Contains("None") ||
                                   scenarioFormData.LocationCountries.Contains(runway.Country));

            bool statesMatch = (scenarioFormData.LocationStates == null ||
                                 scenarioFormData.LocationStates.Count == 0 ||
                                 scenarioFormData.LocationStates.Contains("None") ||
                                 scenarioFormData.LocationStates.Contains(runway.State));

            bool citiesMatch = (scenarioFormData.LocationCities == null ||
                                 scenarioFormData.LocationCities.Count == 0 ||
                                 scenarioFormData.LocationCities.Contains("None") ||
                                 scenarioFormData.LocationCities.Contains(runway.City));

            return countriesMatch && statesMatch && citiesMatch;
        }

        /// <summary>
        /// Finds the nearest runway to a given point using a k-d tree.
        /// </summary>
        private static void FindNearestRecursive(KDNode node, double lat, double lon, Func<RunwayParams, bool> filter, int depth, ref RunwayParams best, ref double bestDistSq)
        {
            if (node == null)
            {
                return;
            }

            double currentDistSq = GetDistanceSq(node.Runway, lat, lon);

            if (currentDistSq < bestDistSq && filter(node.Runway))
            {
                bestDistSq = currentDistSq;
                best = node.Runway;
            }

            int axis = depth % 2;
            double axisDist = (axis == 0) ? (lat - node.Runway.AirportLat) : (lon - node.Runway.AirportLon);
            KDNode nearNode = (axisDist < 0) ? node.Left : node.Right;
            KDNode farNode = (axisDist < 0) ? node.Right : node.Left;

            FindNearestRecursive(nearNode, lat, lon, filter, depth + 1, ref best, ref bestDistSq);

            if (axisDist * axisDist < bestDistSq)
            {
                FindNearestRecursive(farNode, lat, lon, filter, depth + 1, ref best, ref bestDistSq);
            }
        }

        /// <summary>
        /// Finds all runways within a specified distance range using a k-d tree.
        /// </summary>
        private static void FindInRangeRecursive(KDNode node, double lat, double lon, double minSq, double maxSq, Func<RunwayParams, bool> filter, int depth, List<RunwayParams> results)
        {
            if (node == null)
            {
                return;
            }

            double currentDistSq = GetDistanceSq(node.Runway, lat, lon);

            if (currentDistSq >= minSq && currentDistSq <= maxSq && filter(node.Runway))
            {
                results.Add(node.Runway);
            }

            int axis = depth % 2;
            double axisDist = (axis == 0) ? (lat - node.Runway.AirportLat) : (lon - node.Runway.AirportLon);

            if (axisDist < 0)
            {
                FindInRangeRecursive(node.Left, lat, lon, minSq, maxSq, filter, depth + 1, results);
                if (axisDist * axisDist < maxSq)
                {
                    FindInRangeRecursive(node.Right, lat, lon, minSq, maxSq, filter, depth + 1, results);
                }
            }
            else
            {
                FindInRangeRecursive(node.Right, lat, lon, minSq, maxSq, filter, depth + 1, results);
                if (axisDist * axisDist < maxSq)
                {
                    FindInRangeRecursive(node.Left, lat, lon, minSq, maxSq, filter, depth + 1, results);
                }
            }
        }

        /// <summary>
        /// Calculates the squared Euclidean distance between a runway's coordinates and a target point.
        /// </summary>
        private static double GetDistanceSq(RunwayParams runway, double lat, double lon)
        {
            return Math.Pow(runway.AirportLat - lat, 2) + Math.Pow(runway.AirportLon - lon, 2);
        }
    }
}
