using P3D_Scenario_Generator.Services;

namespace P3D_Scenario_Generator.Runways
{
    /// <summary>
    /// Manages the presentation and formatting of runway data for the user interface.
    /// This class is responsible for converting raw data objects into formatted strings
    /// and for parsing user-selected strings back into data components.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the RunwayUiManager.
    /// </remarks>
    /// <param name="searcher">An instance of RunwaySearcher to get raw runway data from.</param>
    /// <param name="logger">An instance of an asynchronous logging service.</param>
    /// <param name="cacheManager">An instance of a cache manager for serialization/deserialization.</param>
    public class RunwayUiManager(RunwaySearcher searcher, Logger logger, CacheManager cacheManager, FileOps fileOps)
    {
        private readonly RunwaySearcher _searcher = searcher ?? throw new ArgumentNullException(nameof(searcher));
        private readonly Logger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly CacheManager _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
        private readonly FileOps _fileOps = fileOps ?? throw new ArgumentNullException(nameof(fileOps));

        public RunwayUILists UILists { get; } = new RunwayUILists();

        // Path where user favourites will be stored.
        private readonly string _favouritesFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            AppDomain.CurrentDomain.FriendlyName,
            "LocationFavouritesJSON.txt"
        );

        public const string DefaultFavouriteName = "[All Locations]";

        /// <summary>
        /// User created location favourites built from combinations of Country/State/City strings in "runways.xml" file
        /// </summary>
        internal List<LocationFavourite> LocationFavourites { get; private set; } = [];

        /// <summary>
        /// Currently selected location favourite displayed on form
        /// </summary>
        internal int CurrentLocationFavouriteIndex { get; set; }

        #region Manage Location favourites region

        /// <summary>
        /// Loads the list of location favourites from a file stored in JSON format asynchronously.
        /// It first attempts to load a local user-created version. If the local file does not exist,
        /// or if it contains an empty list, it falls back to a non-empty embedded resource.
        /// </summary>
        /// <param name="progressReporter">Optional. Can be <see langword="null"/> if progress or error reporting to the UI is not required.</param>
        /// <returns>A task that returns <see langword="true"/> if location favourites were successfully loaded, <see langword="false"/> otherwise.</returns>
        internal async Task<bool> LoadLocationFavouritesAsync(IProgress<string> progressReporter = null)
        {
            // Initialize LocationFavourites to an empty list to prevent NullReferenceException on failure.
            LocationFavourites = [];
            bool needsFallback = false;

            try
            {
                // First, try to load from the local file using the asynchronous method.
                await _logger.InfoAsync($"Attempting to load location favourites from local file: {_favouritesFilePath}");
                var (success, data) = await _cacheManager.TryDeserializeFromFileAsync<List<LocationFavourite>>(_favouritesFilePath);

                if (success)
                {
                    LocationFavourites = data;
                }
                else
                {
                    // If the local file was not found or deserialization failed, trigger a fallback.
                    needsFallback = true;
                }

                // If the local file was loaded successfully but is empty, trigger a fallback.
                if (LocationFavourites == null || LocationFavourites.Count == 0)
                {
                    string warningMessage = "Local favourites file was empty or contained no valid data. Falling back to embedded resource.";
                    await _logger.WarningAsync(warningMessage);
                    progressReporter?.Report(warningMessage);
                    needsFallback = true;
                }
            }
            catch (Exception ex)
            {
                // Catch any other exceptions from the primary deserialize attempt.
                string errorMessage = $"An error occurred while processing local favourites file: {ex.Message}";
                await _logger.ErrorAsync(errorMessage, ex);
                progressReporter?.Report($"ERROR: {errorMessage}");
                needsFallback = true;
            }

            if (needsFallback)
            {
                try
                {
                    // Assuming FileOps and embedded resource logic are elsewhere.
                    (bool success, Stream resourceStream) = await _fileOps.TryGetResourceStreamAsync("Text.LocationFavouritesJSON.txt", progressReporter);
                    if (success)
                    {
                        using (resourceStream)
                        {
                            LocationFavourites = System.Text.Json.JsonSerializer.Deserialize<List<LocationFavourite>>(resourceStream);
                            await _logger.InfoAsync("Successfully loaded location favourites from embedded resource.");
                        }
                    }
                    else
                    {
                        // If loading fails completely, fall back to the protected default
                        LocationFavourites = [new LocationFavourite() { Name = DefaultFavouriteName }];
                        CurrentLocationFavouriteIndex = 0;
                    }
                }
                catch (Exception ex)
                {
                    string errorMessage = $"An unexpected error occurred during fallback to embedded resource: {ex.Message}";
                    await _logger.ErrorAsync(errorMessage, ex);
                    progressReporter?.Report($"ERROR: {errorMessage}");
                    LocationFavourites = [];
                    return false;
                }
            }

            if (LocationFavourites.Count > 0)
            {
                if (!LocationFavourites.Any(f => f.Name.Equals(DefaultFavouriteName, StringComparison.OrdinalIgnoreCase)))
                {
                    LocationFavourites.Insert(0, new LocationFavourite() { Name = DefaultFavouriteName });
                }

                await _logger.InfoAsync($"Successfully loaded {LocationFavourites.Count} location favourites.");
                progressReporter?.Report($"Location favourites loaded ({LocationFavourites.Count} entries).");
                return true;
            }
            else
            {
                string warningMessage = "All attempts to load a valid list of location favourites failed. The list will be empty.";
                await _logger.WarningAsync(warningMessage);
                progressReporter?.Report(warningMessage);
                LocationFavourites = [new LocationFavourite() { Name = DefaultFavouriteName }];
                CurrentLocationFavouriteIndex = 0;
                return false;
            }
        }

        /// <summary>
        /// Saves the current list of location favourites to a file in JSON format using CacheManager.
        /// The save operation is skipped if the list is empty to prevent overwriting a valid file.
        /// </summary>
        /// <param name="progressReporter">Optional. Can be <see langword="null"/> if progress or error reporting to the UI is not required.</param>
        internal async Task SaveLocationFavouritesAsync(IProgress<string> progressReporter)
        {
            if (LocationFavourites == null || LocationFavourites.Count == 0)
            {
                string warningMessage = "Location favourites list is empty. Save operation aborted to prevent data loss.";
                await _logger.WarningAsync(warningMessage);
                progressReporter?.Report(warningMessage);
                return;
            }
            try
            {
                bool success = await _cacheManager.TrySerializeToFileAsync(LocationFavourites, _favouritesFilePath);

                if (success)
                {
                    await _logger.InfoAsync($"Successfully saved {LocationFavourites.Count} location favourites to '{_favouritesFilePath}'.");
                    progressReporter?.Report($"Location favourites saved ({LocationFavourites.Count} entries).");
                }
                else
                {
                    await _logger.ErrorAsync("Failed to save location favourites.");
                    progressReporter?.Report("ERROR: Failed to save location favourites. See log for details.");
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Failed to save location favourites. Details: {ex.Message}", ex);
                progressReporter?.Report($"ERROR: Failed to save location favourites: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds a new location favourite to the list.
        /// </summary>
        /// <param name="name">The name for the new <see cref="LocationFavourite"/> to be added</param>
        internal void AddLocationFavourite(string name)
        {
            LocationFavourite deepCopyFav = new(LocationFavourites[CurrentLocationFavouriteIndex])
            {
                Name = name
            };
            LocationFavourites.Add(deepCopyFav);

            // Switch active index to the newly created favourite
            CurrentLocationFavouriteIndex = LocationFavourites.Count - 1;
        }

        /// <summary>
        /// Deletes a location favourite from the list and adjusts the current index.
        /// </summary>
        /// <param name="deleteLocationFavouriteName">The name of the <see cref="LocationFavourite"/> to be deleted</param>
        /// <returns>The name of the zero index <see cref="LocationFavourites"/> location favourite</returns>
        internal string DeleteLocationFavourite(string deleteLocationFavouriteName)
        {
            // Block deletion of the permanent default
            if (deleteLocationFavouriteName.Equals(DefaultFavouriteName, StringComparison.OrdinalIgnoreCase))
            {
                return LocationFavourites[CurrentLocationFavouriteIndex].Name;
            }

            if (LocationFavourites.Count > 1)
            {
                LocationFavourite deleteLocationFavourite = LocationFavourites.Find(favourite => favourite.Name == deleteLocationFavouriteName);
                if (deleteLocationFavourite != null)
                {
                    LocationFavourites.Remove(deleteLocationFavourite);
                    CurrentLocationFavouriteIndex = 0;
                }
            }
            return LocationFavourites[CurrentLocationFavouriteIndex].Name;
        }

        /// <summary>
        /// Updates the current location favourite with a new name if that name has not already been used.
        /// </summary>
        /// <param name="newLocationFavouriteName">The new location favourite name.</param>
        /// <returns>The replaced current location favourite name.</returns>
        internal string UpdateLocationFavouriteName(string newLocationFavouriteName)
        {
            string oldLocationFavouriteName = LocationFavourites[CurrentLocationFavouriteIndex].Name;

            // Block renaming of the permanent default
            if (oldLocationFavouriteName.Equals(DefaultFavouriteName, StringComparison.OrdinalIgnoreCase))
            {
                return oldLocationFavouriteName;
            }

            if (LocationFavourites.FindAll(favourite => favourite.Name == newLocationFavouriteName).Count == 0)
                LocationFavourites[CurrentLocationFavouriteIndex].Name = newLocationFavouriteName;

            return oldLocationFavouriteName;
        }

        /// <summary>
        /// Gets a sorted list of the location favourite names.
        /// </summary>
        /// <returns>Sorted list of the location favourite names.</returns>
        internal List<string> GetLocationFavouriteNames()
        {
            if (LocationFavourites.Count == 0) return [];

            // Capture the active item before sorting
            LocationFavourite currentSelected = GetCurrentLocationFavourite();

            // Sort the list
            LocationFavourites = [.. LocationFavourites.OrderBy(f => f.Name)];

            // Sync index back safely (default to 0 if IndexOf returns -1)
            int newIndex = LocationFavourites.IndexOf(currentSelected);
            CurrentLocationFavouriteIndex = Math.Max(0, newIndex);

            return [.. LocationFavourites.Select(f => f.Name)];
        }

        /// <summary>
        /// Reset <see cref="CurrentLocationFavouriteIndex"/> to the instance of <see cref="LocationFavourite"/> with newFavouriteName
        /// </summary>
        /// <param name="newFavouriteName">The name of the new instance to be set as <see cref="CurrentLocationFavouriteIndex"/></param>
        internal void ChangeCurrentLocationFavouriteIndex(string name)
        {
            int index = LocationFavourites.FindIndex(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            // Only update if the item actually exists.
            // While typing a new name, retain the current valid index.
            if (index != -1)
            {
                CurrentLocationFavouriteIndex = index;
            }
        }

        /// <summary>
        /// Get the <see cref="CurrentLocationFavouriteIndex"/> instance in <see cref="LocationFavourites"/>
        /// </summary>
        /// <returns>The <see cref="CurrentLocationFavouriteIndex"/> instance in <see cref="LocationFavourites"/></returns>
        internal LocationFavourite GetCurrentLocationFavourite()
        {
            // Safety fallback to prevent out-of-bounds crashes
            if (CurrentLocationFavouriteIndex < 0 || CurrentLocationFavouriteIndex >= LocationFavourites.Count)
            {
                CurrentLocationFavouriteIndex = 0;
            }
            return LocationFavourites[CurrentLocationFavouriteIndex];
        }

        /// <summary>
        /// Adds a filter string to one of Country/State/City for <see cref="CurrentLocationFavouriteIndex"/> in
        /// <see cref="LocationFavourites"/>.
        /// </summary>
        /// <param name="locationType">Which of Country/State/City to add to.</param>
        /// <param name="locationValue">The filter string to be added.</param>
        internal void AddFilterValueToLocationFavourite(string locationType, string locationValue)
        {
            // Do not allow adding country/state/city filters directly to the permanent global template
            if (LocationFavourites[CurrentLocationFavouriteIndex].Name.Equals(DefaultFavouriteName, StringComparison.OrdinalIgnoreCase) && locationValue != "None")
            {
                return;
            }

            if (locationValue == "None")
            {
                ClearLocationFavouriteList(locationType);
                AddToLocationFavouriteList(locationType, "None");
            }
            else
            {
                AddToLocationFavouriteList(locationType, locationValue);
                DeleteFromLocationFavouriteList(locationType, "None");
            }
        }

        /// <summary>
        /// Deletes filter string from one of Country/State/City for <see cref="CurrentLocationFavouriteIndex"/> in
        /// <see cref="LocationFavourites"/>.
        /// </summary>
        /// <param name="locationType">Which of Country/State/City to delete from.</param>
        /// <param name="locationValue">The filter string to be deleted.</param>
        internal void DeleteFilterValueFromLocationFavourite(string locationType, string locationValue)
        {
            if (locationValue == "None")
            {
                return;
            }
            else
            {
                DeleteFromLocationFavouriteList(locationType, locationValue);
            }
        }

        /// <summary>
        /// Gets a filter string to display in the Country/State/City fields on the General tab of form.
        /// </summary>
        /// <param name="locationType">Which of Country/State/City fields the display filter value is for.</param>
        /// <returns>The Country/State/City field display filter value.</returns>
        internal string GetLocationFavouriteDisplayFilterValue(string locationType)
        {
            if (locationType == "Country")
                return LocationFavourites[CurrentLocationFavouriteIndex].Countries[0];
            else if (locationType == "State")
                return LocationFavourites[CurrentLocationFavouriteIndex].States[0];
            else
                return LocationFavourites[CurrentLocationFavouriteIndex].Cities[0];
        }

        /// <summary>
        /// Combines the Country/State/City location filters into a single string for display using a tooltip.
        /// </summary>
        /// <returns>Country/State/City location filters combined into a single string.</returns>
        internal string SetTextBoxGeneralLocationFilters()
        {
            var current = GetCurrentLocationFavourite();
            return $"Countries = \"{SetTextBoxGeneralLocationFilter(current.Countries)}\" \n" +
                   $"States = \"{SetTextBoxGeneralLocationFilter(current.States)}\" \n" +
                   $"Cities = \"{SetTextBoxGeneralLocationFilter(current.Cities)}\"";
        }

        /// <summary>
        /// Adds a filter string to one of Country/State/City for <see cref="CurrentLocationFavouriteIndex"/> in
        /// <see cref="LocationFavourites"/>.
        /// </summary>
        /// <param name="locationType">Which of Country/State/City to add to.</param>
        /// <param name="locationValue">The filter string to be added.</param>
        private void AddToLocationFavouriteList(string locationType, string locationValue)
        {
            switch (locationType)
            {
                case "Country":
                    LocationFavourites[CurrentLocationFavouriteIndex].Countries.Add(locationValue);
                    LocationFavourites[CurrentLocationFavouriteIndex].Countries = [.. LocationFavourites[CurrentLocationFavouriteIndex].Countries.Distinct().OrderBy(c => c)];
                    break;
                case "State":
                    LocationFavourites[CurrentLocationFavouriteIndex].States.Add(locationValue);
                    LocationFavourites[CurrentLocationFavouriteIndex].States = [.. LocationFavourites[CurrentLocationFavouriteIndex].States.Distinct().OrderBy(s => s)];
                    break;
                default:
                    LocationFavourites[CurrentLocationFavouriteIndex].Cities.Add(locationValue);
                    LocationFavourites[CurrentLocationFavouriteIndex].Cities = [.. LocationFavourites[CurrentLocationFavouriteIndex].Cities.Distinct().OrderBy(c => c)];
                    break;
            }
        }

        /// <summary>
        /// Clears a filter string list for one of Country/State/City for <see cref="CurrentLocationFavouriteIndex"/> in
        /// <see cref="LocationFavourites"/>.
        /// </summary>
        /// <param name="locationType">Which Country/State/City filter string list to clear.</param>
        private void ClearLocationFavouriteList(string locationType)
        {
            switch (locationType)
            {
                case "Country":
                    LocationFavourites[CurrentLocationFavouriteIndex].Countries.Clear();
                    break;
                case "State":
                    LocationFavourites[CurrentLocationFavouriteIndex].States.Clear();
                    break;
                default:
                    LocationFavourites[CurrentLocationFavouriteIndex].Cities.Clear();
                    break;
            }
        }

        /// <summary>
        /// Deletes filter string from one of Country/State/City for <see cref="CurrentLocationFavouriteIndex"/> in
        /// <see cref="LocationFavourites"/>.
        /// </summary>
        /// <param name="locationType">Which of Country/State/City to delete from.</param>
        /// <param name="locationValue">The filter string to be deleted.</param>
        private void DeleteFromLocationFavouriteList(string locationType, string locationValue)
        {
            List<string> filterList = locationType switch
            {
                "Country" => LocationFavourites[CurrentLocationFavouriteIndex].Countries,
                "State" => LocationFavourites[CurrentLocationFavouriteIndex].States,
                _ => LocationFavourites[CurrentLocationFavouriteIndex].Cities,
            };
            filterList.Remove(locationValue);
            if (filterList.Count == 0)
                AddToLocationFavouriteList(locationType, "None");
        }

        /// <summary>
        /// Combines one of Country/State/City location filters into a single string for display.
        /// </summary>
        /// <param name="locationFilterStrings">The list of location filter strings to be combined.</param>
        /// <returns>One of Country/State/City location filters combined into a single string.</returns>
        private static string SetTextBoxGeneralLocationFilter(List<string> locationFilterStrings)
        {
            return string.Join(", ", locationFilterStrings);
        }

        #endregion

        /// <summary>
        /// Populates the UILists with data from the RunwaySearcher.
        /// </summary>
        public void PopulateUiLists()
        {
            UILists.States = _searcher.GetRunwayStates();
            UILists.Cities = _searcher.GetRunwayCities();
            UILists.Countries = _searcher.GetRunwayCountries();
            UILists.IcaoRunwayNumbers = GetIcaoRunwayNumbers();
        }

        /// <summary>
        /// Gets a list of ICAO IDs with their corresponding runway Numbers, formatting them
        /// as "ICAOId (Number)" or just "ICAOId" if the runway Number is empty.
        /// </summary>
        /// <returns>A list of formatted ICAO and runway Numbers.</returns>
        public List<string> GetIcaoRunwayNumbers()
        {
            // The logic for getting the runways is now in RunwaySearcher.
            return [.. _searcher.GetAllRunways().Select(RunwayUtils.FormatRunwayIcaoString)];
        }
    }
}
