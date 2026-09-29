using P3D_Scenario_Generator.Services;

namespace P3D_Scenario_Generator.Runways
{
    /// <summary>
    /// Manages the presentation and formatting of runway data for the user interface.
    /// This class is responsible for converting raw data objects into formatted strings
    /// and for parsing user-selected strings back into data components.
    /// </summary>
    /// <param name="searcher">An instance of <see cref="RunwaySearcher"/> to get raw runway data from.</param>
    /// <param name="logger">The application logger instance.</param>
    /// <param name="cacheManager">The cache management service for serialization and deserialization.</param>
    /// <param name="fileOps">The centralized file operations service.</param>
    internal class RunwayUiManager(RunwaySearcher searcher, Logger logger, CacheManager cacheManager, FileOps fileOps)
    {
        private readonly RunwaySearcher _searcher = searcher ?? throw new ArgumentNullException(nameof(searcher));
        private readonly Logger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly CacheManager _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
        private readonly FileOps _fileOps = fileOps ?? throw new ArgumentNullException(nameof(fileOps));

        internal RunwayUILists UILists { get; } = new RunwayUILists();

        // Path where user favourites will be stored.
        private readonly string _favouritesFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            AppDomain.CurrentDomain.FriendlyName,
            "LocationFavouritesJSON.txt"
        );

        internal const string DefaultFavouriteName = "[All Locations]";

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
        internal async Task<bool> LoadLocationFavouritesAsync(IProgress<string>? progressReporter = null)
        {
            LocationFavourites = [];
            bool needsFallback = false;

            try
            {
                await _logger.InfoAsync($"Attempting to load location favourites from local file: {_favouritesFilePath}");
                var (success, data) = await _cacheManager.TryDeserializeFromFileAsync<List<LocationFavourite>>(_favouritesFilePath);

                if (success && data?.Count > 0)
                {
                    LocationFavourites = data;
                }
                else
                {
                    const string warningMessage = "Local favourites file was missing, empty, or invalid. Falling back to embedded resource.";
                    await _logger.WarningAsync(warningMessage);
                    progressReporter?.Report(warningMessage);
                    needsFallback = true;
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"An error occurred while processing local favourites file: {ex.Message}";
                await _logger.ErrorAsync(errorMessage, ex);
                progressReporter?.Report($"ERROR: {errorMessage}");
                needsFallback = true;
            }

            if (needsFallback)
            {
                var (resSuccess, resData) = await _fileOps.TryDeserializeJsonFromResourceAsync<List<LocationFavourite>>("Text.LocationFavouritesJSON.txt", null, progressReporter);

                if (resSuccess && resData?.Count > 0)
                {
                    LocationFavourites = resData;
                    await _logger.InfoAsync("Successfully loaded location favourites from embedded resource.");
                }
                else
                {
                    LocationFavourites = [new LocationFavourite() { Name = DefaultFavouriteName }];
                    CurrentLocationFavouriteIndex = 0;
                }
            }

            if (LocationFavourites?.Count > 0)
            {
                if (!LocationFavourites.Any(f => string.Equals(f?.Name, DefaultFavouriteName, StringComparison.OrdinalIgnoreCase)))
                {
                    LocationFavourites.Insert(0, new LocationFavourite() { Name = DefaultFavouriteName });
                }

                await _logger.InfoAsync($"Successfully loaded {LocationFavourites.Count} location favourites.");
                progressReporter?.Report($"Location favourites loaded ({LocationFavourites.Count} entries).");
                return true;
            }
            else
            {
                const string warningMessage = "All attempts to load a valid list of location favourites failed. Default item inserted.";
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
        internal async Task SaveLocationFavouritesAsync(IProgress<string>? progressReporter = null)
        {
            if (LocationFavourites == null || LocationFavourites.Count == 0)
            {
                const string warningMessage = "Location favourites list is empty. Save operation aborted to prevent data loss.";
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
        /// <param name="name">The name for the new <see cref="LocationFavourite"/> to be added.</param>
        internal void AddLocationFavourite(string name)
        {
            LocationFavourite deepCopyFav = new(LocationFavourites[CurrentLocationFavouriteIndex])
            {
                Name = name
            };
            LocationFavourites.Add(deepCopyFav);

            CurrentLocationFavouriteIndex = LocationFavourites.Count - 1;
        }

        /// <summary>
        /// Deletes a location favourite from the list and adjusts the current index.
        /// </summary>
        /// <param name="deleteLocationFavouriteName">The name of the <see cref="LocationFavourite"/> to be deleted.</param>
        /// <returns>The name of the zero index <see cref="LocationFavourites"/> location favourite.</returns>
        internal string DeleteLocationFavourite(string deleteLocationFavouriteName)
        {
            if (deleteLocationFavouriteName.Equals(DefaultFavouriteName, StringComparison.OrdinalIgnoreCase))
            {
                return LocationFavourites[CurrentLocationFavouriteIndex].Name;
            }

            if (LocationFavourites.Count > 1)
            {
                LocationFavourite? deleteLocationFavourite = LocationFavourites.Find(favourite => string.Equals(favourite.Name, deleteLocationFavouriteName, StringComparison.OrdinalIgnoreCase));
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

            LocationFavourite currentSelected = GetCurrentLocationFavourite();

            LocationFavourites = [.. LocationFavourites.OrderBy(f => f.Name)];

            int newIndex = LocationFavourites.IndexOf(currentSelected);
            CurrentLocationFavouriteIndex = Math.Max(0, newIndex);

            return [.. LocationFavourites.Select(f => f.Name)];
        }

        /// <summary>
        /// Reset <see cref="CurrentLocationFavouriteIndex"/> to the instance of <see cref="LocationFavourite"/> with name.
        /// </summary>
        /// <param name="name">The name of the instance to set as active.</param>
        internal void ChangeCurrentLocationFavouriteIndex(string name)
        {
            int index = LocationFavourites.FindIndex(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (index != -1)
            {
                CurrentLocationFavouriteIndex = index;
            }
        }

        /// <summary>
        /// Get the <see cref="CurrentLocationFavouriteIndex"/> instance in <see cref="LocationFavourites"/>
        /// </summary>
        /// <returns>The <see cref="CurrentLocationFavouriteIndex"/> instance in <see cref="LocationFavourites"/>.</returns>
        internal LocationFavourite GetCurrentLocationFavourite()
        {
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

            DeleteFromLocationFavouriteList(locationType, locationValue);
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
                    LocationFavourites[CurrentLocationFavouriteIndex].Countries = [.. LocationFavourites[CurrentLocationFavouriteIndex].Countries.Distinct().Order()];
                    break;
                case "State":
                    LocationFavourites[CurrentLocationFavouriteIndex].States.Add(locationValue);
                    LocationFavourites[CurrentLocationFavouriteIndex].States = [.. LocationFavourites[CurrentLocationFavouriteIndex].States.Distinct().Order()];
                    break;
                default:
                    LocationFavourites[CurrentLocationFavouriteIndex].Cities.Add(locationValue);
                    LocationFavourites[CurrentLocationFavouriteIndex].Cities = [.. LocationFavourites[CurrentLocationFavouriteIndex].Cities.Distinct().Order()];
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
        internal void PopulateUiLists()
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
        internal List<string> GetIcaoRunwayNumbers()
        {
            return [.. _searcher.GetAllRunways().Select(RunwayUtils.FormatRunwayIcaoString)];
        }
    }
}