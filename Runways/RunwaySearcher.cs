using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.Models;
using P3D_Scenario_Generator.Services;
using P3D_Scenario_Generator.Utilities;

namespace P3D_Scenario_Generator.Runways
{
    /// <summary>
    /// Manages and searches through a collection of runway data.
    /// Uses a k-d tree for efficient nearest and nearby neighbor searches,
    /// applying filters dynamically during the search rather than on a pre-filtered subset.
    /// </summary>
    /// <param name="data">The complete runway data object containing all runways and the KD-tree root.</param>
    /// <param name="log">The logging service.</param>
    internal class RunwaySearcher(RunwayData data, Logger log)
    {
        private readonly Logger _log = log ?? throw new ArgumentNullException(nameof(log));
        private readonly RunwayData _data = data ?? throw new ArgumentNullException(nameof(data));

        private List<RunwayParams> AllRunways => _data.Runways ?? [];
        private KDNode? KdTreeRoot => _data.RunwayTreeRoot;

        private static readonly Random _random = Random.Shared;

        /// <summary>
        /// Searches runways matching a search query while respecting active location filters and aircraft capabilities.
        /// </summary>
        /// <param name="searchText">The raw text entered into the search box.</param>
        /// <param name="scenarioFormData">The current form configuration payload.</param>
        /// <returns>A list of matching <see cref="RunwayParams"/> objects.</returns>
        internal List<RunwayParams> SearchRunways(string searchText, ScenarioFormData scenarioFormData)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return [];
            }

            string[] searchTokens = searchText.Split([' ', '(', ')', '-', ','], StringSplitOptions.RemoveEmptyEntries);

            if (searchTokens.Length == 0)
            {
                return [];
            }

            return [.. AllRunways
                .Where(runway => IsRunwayInFilteredLocation(runway, scenarioFormData))
                .Where(runway =>
                {
                    string displayString = $"{runway.IcaoId} ({runway.Number}{runway.Designator})";
                    return searchTokens.All(token => displayString.Contains(token, StringComparison.OrdinalIgnoreCase));
                })];
        }

        /// <summary>
        /// Returns all runways that meet active location filters and aircraft surface capability criteria.
        /// </summary>
        /// <param name="scenarioFormData">The current form configuration payload containing location and aircraft settings.</param>
        /// <returns>A list of filtered <see cref="RunwayParams"/> objects.</returns>
        internal List<RunwayParams> GetFilteredRunways(ScenarioFormData scenarioFormData)
        {
            return [.. AllRunways.Where(runway => IsRunwayInFilteredLocation(runway, scenarioFormData))];
        }

        /// <summary>
        /// Finds the nearest runway to a given point from the complete list,
        /// applying location filters dynamically using a k-d tree search.
        /// </summary>
        /// <param name="targetLat">The latitude of the target point.</param>
        /// <param name="targetLon">The longitude of the target point.</param>
        /// <param name="scenarioFormData">The DTO containing location filters.</param>
        /// <returns>The nearest runway that meets the filter criteria, or <see langword="null"/> if no match is found.</returns>
        internal async Task<RunwayParams?> FindNearestRunwayAsync(double targetLat, double targetLon, ScenarioFormData scenarioFormData)
        {
            try
            {
                RunwayParams? best = null;
                double bestDistSq = double.MaxValue;
                FindNearestRecursive(KdTreeRoot, targetLat, targetLon, runway => IsRunwayInFilteredLocation(runway, scenarioFormData), 0, ref best, ref bestDistSq);
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
        /// <returns>A runway that meets the distance and filter criteria, or <see langword="null"/> if no match is found.</returns>
        internal async Task<RunwayParams?> FindNearbyRunwayAsync(double targetLat, double targetLon, double minDist, double maxDist, ScenarioFormData scenarioFormData)
        {
            try
            {
                List<RunwayParams> nearbyRunways = [];
                double minSq = minDist / Constants.NMInDegreeOfLatitude * minDist / Constants.NMInDegreeOfLatitude;
                double maxSq = maxDist / Constants.NMInDegreeOfLatitude * maxDist / Constants.NMInDegreeOfLatitude;
                FindInRangeRecursive(KdTreeRoot, targetLat, targetLon, minSq, maxSq, runway => IsRunwayInFilteredLocation(runway, scenarioFormData), 0, nearbyRunways);

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
        /// <returns>A randomly selected <see cref="RunwayParams"/> object that meets the filter criteria, or <see langword="null"/> if no matching runways are found.</returns>
        internal async Task<RunwayParams?> GetFilteredRandomRunwayAsync(ScenarioFormData scenarioFormData)
        {
            try
            {
                List<RunwayParams> filteredRunways = [.. AllRunways.Where(runway => IsRunwayInFilteredLocation(runway, scenarioFormData))];

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
                await _log.ErrorAsync("An error occurred while getting a random filtered runway.", ex);
                return null;
            }
        }

        /// <summary>
        /// Returns the complete list of all runway data objects.
        /// </summary>
        /// <returns>The complete list of all runway data objects.</returns>
        internal List<RunwayParams> GetAllRunways()
        {
            return AllRunways;
        }

        /// <summary>
        /// Finds a specific runway by its ICAO ID, runway ID, and runway designator.
        /// </summary>
        /// <param name="icaoId">The ICAO ID of the airport.</param>
        /// <param name="runwayId">The ID of the runway (e.g., "14", "26").</param>
        /// <param name="runwayDesignator">The designator of the runway (e.g., "Left", "Centre").</param>
        /// <returns>The matching <see cref="RunwayParams"/> object, or <see langword="null"/> if not found.</returns>
        internal RunwayParams? GetRunwayByIcaoIdDesignator(string icaoId, string runwayId, string runwayDesignator)
        {
            if (string.IsNullOrEmpty(icaoId) || string.IsNullOrEmpty(runwayId) || AllRunways == null)
            {
                return null;
            }

            return AllRunways.FirstOrDefault(r =>
                r.IcaoId.Equals(icaoId, StringComparison.OrdinalIgnoreCase) &&
                r.Number.Equals(runwayId, StringComparison.OrdinalIgnoreCase) &&
                r.Designator.Equals(runwayDesignator, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets a single runway object by its index in the internal list.
        /// </summary>
        /// <param name="index">The zero-based index of the runway to retrieve.</param>
        /// <returns>The <see cref="RunwayParams"/> object at the specified index, or <see langword="null"/> if the index is out of bounds.</returns>
        internal async Task<RunwayParams?> GetRunwayByIndexAsync(int index)
        {
            if (index >= 0 && index < AllRunways.Count)
            {
                var result = AllRunways.FirstOrDefault(r => r.RunwaysIndex == index);

                if (result == null)
                {
                    await _log.WarningAsync($"Could not find runway with RunwaysIndex of {index}.");
                }

                return result;
            }

            await _log.WarningAsync($"Attempted to access runway at index {index}, which is out of bounds (list size: {AllRunways.Count}).");
            return null;
        }

        /// <summary>
        /// Gets a sorted list of the unique country strings from the full list of runways.
        /// </summary>
        /// <returns>A sorted list of unique country names with "None" as the first entry.</returns>
        internal List<string> GetRunwayCountries()
        {
            List<string> countries = [.. AllRunways
                .Where(r => !string.IsNullOrEmpty(r.Country))
                .Select(r => r.Country)
                .Distinct()
                .Order()];

            countries.Insert(0, "None");
            return countries;
        }

        /// <summary>
        /// Gets a sorted list of the unique state strings from the full list of runways.
        /// </summary>
        /// <returns>A sorted list of unique state names with "None" as the first entry.</returns>
        internal List<string> GetRunwayStates()
        {
            List<string> states = [.. AllRunways
                .Where(r => !string.IsNullOrEmpty(r.State))
                .Select(r => r.State)
                .Distinct()
                .Order()];

            states.Insert(0, "None");
            return states;
        }

        /// <summary>
        /// Gets a sorted list of the unique city strings from the full list of runways.
        /// </summary>
        /// <returns>A sorted list of unique city names with "None" as the first entry.</returns>
        internal List<string> GetRunwayCities()
        {
            List<string> cities = [.. AllRunways
                .Where(r => !string.IsNullOrEmpty(r.City))
                .Select(r => r.City)
                .Distinct()
                .Order()];

            cities.Insert(0, "None");
            return cities;
        }

        /// <summary>
        /// Checks whether a runway meets location, surface capability, and night lighting criteria.
        /// </summary>
        /// <param name="runway">The runway to check.</param>
        /// <param name="scenarioFormData">The ScenarioFormData object with filter settings.</param>
        /// <returns><see langword="true"/> if the runway meets the criteria; otherwise, <see langword="false"/>.</returns>
        internal static bool IsRunwayInFilteredLocation(RunwayParams runway, ScenarioFormData scenarioFormData)
        {
            var aircraft = scenarioFormData?.SelectedAircraft;
            if (aircraft != null)
            {
                bool isWater = runway.IsWaterRunway;
                if (isWater && !aircraft.HasFloats)
                {
                    return false;
                }
                if (!isWater && !aircraft.HasWheelsOrEquiv)
                {
                    return false;
                }
            }

            if (scenarioFormData?.ScenarioType == ScenarioTypes.Celestial)
            {
                if (!runway.HasLights)
                {
                    return false;
                }
            }

            bool hasCountryFilter = scenarioFormData?.LocationCountries?.Count > 0 && !scenarioFormData.LocationCountries.Contains("None");
            bool hasStateFilter = scenarioFormData?.LocationStates?.Count > 0 && !scenarioFormData.LocationStates.Contains("None");
            bool hasCityFilter = scenarioFormData?.LocationCities?.Count > 0 && !scenarioFormData.LocationCities.Contains("None");

            if (!hasCountryFilter && !hasStateFilter && !hasCityFilter)
            {
                return true;
            }

            bool countryMatches = hasCountryFilter && scenarioFormData!.LocationCountries.Contains(runway.Country);
            bool stateMatches = hasStateFilter && scenarioFormData!.LocationStates.Contains(runway.State);
            bool cityMatches = hasCityFilter && scenarioFormData!.LocationCities.Contains(runway.City);

            return countryMatches || stateMatches || cityMatches;
        }

        private static void FindNearestRecursive(KDNode? node, double lat, double lon, Func<RunwayParams, bool> filter, int depth, ref RunwayParams? best, ref double bestDistSq)
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
            KDNode? nearNode = (axisDist < 0) ? node.Left : node.Right;
            KDNode? farNode = (axisDist < 0) ? node.Right : node.Left;

            FindNearestRecursive(nearNode, lat, lon, filter, depth + 1, ref best, ref bestDistSq);

            if (axisDist * axisDist < bestDistSq)
            {
                FindNearestRecursive(farNode, lat, lon, filter, depth + 1, ref best, ref bestDistSq);
            }
        }

        private static void FindInRangeRecursive(KDNode? node, double lat, double lon, double minSq, double maxSq, Func<RunwayParams, bool> filter, int depth, List<RunwayParams> results)
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

        private static double GetDistanceSq(RunwayParams runway, double lat, double lon)
        {
            return Math.Pow(runway.AirportLat - lat, 2) + Math.Pow(runway.AirportLon - lon, 2);
        }

        /// <summary>
        /// Finds a random pair of runways (Departure and Destination) that meet the location filters,
        /// aircraft capabilities, and are separated by a distance within the specified min and max bounds.
        /// </summary>
        /// <param name="minDistanceNM">Minimum distance between the airports in nautical miles.</param>
        /// <param name="maxDistanceNM">Maximum distance between the airports in nautical miles.</param>
        /// <param name="scenarioFormData">The current form configuration payload for filtering.</param>
        /// <returns>A tuple containing the Departure and Destination runways, or <see langword="null"/> if no valid pair is found.</returns>
        internal async Task<(RunwayParams Departure, RunwayParams Destination)?> GetRandomRunwayPairAsync(double minDistanceNM, double maxDistanceNM, ScenarioFormData scenarioFormData)
        {
            try
            {
                List<RunwayParams> validStartRunways = GetFilteredRunways(scenarioFormData);

                if (validStartRunways.Count < 2)
                {
                    await _log.WarningAsync("Not enough filtered runways to form a departure/destination pair.");
                    return null;
                }

                int n = validStartRunways.Count;
                while (n > 1)
                {
                    n--;
                    int k = _random.Next(n + 1);
                    (validStartRunways[k], validStartRunways[n]) = (validStartRunways[n], validStartRunways[k]);
                }

                foreach (var departureRunway in validStartRunways)
                {
                    RunwayParams? destinationRunway = await FindNearbyRunwayAsync(
                        departureRunway.AirportLat,
                        departureRunway.AirportLon,
                        minDistanceNM,
                        maxDistanceNM,
                        scenarioFormData);

                    if (destinationRunway?.IcaoId.Equals(departureRunway.IcaoId, StringComparison.OrdinalIgnoreCase) == false)
                    {
                        return (departureRunway, destinationRunway);
                    }
                }

                await _log.WarningAsync($"Could not find any runway pair within {minDistanceNM} to {maxDistanceNM} NM matching the current filters.");
                return null;
            }
            catch (Exception ex)
            {
                await _log.ErrorAsync("An error occurred while finding a random runway pair.", ex);
                return null;
            }
        }
    }
}