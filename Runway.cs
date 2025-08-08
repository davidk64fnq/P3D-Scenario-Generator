using P3D_Scenario_Generator.Runways;

namespace P3D_Scenario_Generator
{

    /// <summary>
    /// Stores the Country/State/City location filter values for a location favourite
    /// </summary>
    public class LocationFavourite()
    {
        /// <summary>
        /// The name of the favourite
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The list of valid country strings for this favourite 
        /// </summary>
        public List<string> Countries { get; set; } = [];

        /// <summary>
        /// The list of valid state strings for this favourite 
        /// </summary>
        public List<string> States { get; set; } = [];

        /// <summary>
        /// The list of valid city strings for this favourite 
        /// </summary>
        public List<string> Cities { get; set; } = [];

        // Copy constructor 
        public LocationFavourite(LocationFavourite original) : this() // Calls the primary constructor for initialization
        {
            this.Name = original.Name;
            this.Countries = original.Countries?.ToList() ?? [];
            this.States = original.States?.ToList() ?? [];
            this.Cities = original.Cities?.ToList() ?? [];
        }
    }

    internal class Runway
    {
        /// <summary>
        /// The list of runways loaded from "runways.xml" file on application startup
        /// </summary>
        internal static List<RunwayParams> Runways { get; set; }

        /// <summary>
        /// The scenario start runway
        /// </summary>
        internal static RunwayParams startRwy = new();

        /// <summary>
        /// The scenario destination runway
        /// </summary>
        internal static RunwayParams destRwy = new();

        /// <summary>
        /// User created location favourites built from combinations of Country/State/City strings in "runways.xml" file
        /// </summary>
        internal static List<LocationFavourite> LocationFavourites = [];

        /// <summary>
        /// Currently selected location favourite displayed on form
        /// </summary>
        internal static int CurrentLocationFavouriteIndex;

        #region Manage Location favourites region

        /// <summary>
        /// Adds filter string to one of Country/State/City for <see cref="CurrentLocationFavouriteIndex"/> in 
        /// <see cref="LocationFavourites"/>
        /// </summary>
        /// <param name="locationType">Which of Country/State/City to add to</param>
        /// <param name="locationValue">The filter string to be added</param>
        static internal void AddFilterValueToLocationFavourite(string locationType, string locationValue)
        {
            if (locationValue == "None")
            {
                // Clear existing filter strings and add "None"
                ClearLocationFavouriteList(locationType);
                AddToLocationFavouriteList(locationType, "None");
            }
            else
            {
                // Add filter string
                AddToLocationFavouriteList(locationType, locationValue);
                // Handle case where filter list was "None"
                DeleteFromLocationFavouriteList(locationType, "None");
            }
        }

        /// <summary>
        /// Adds filter string to one of Country/State/City for <see cref="CurrentLocationFavouriteIndex"/> in 
        /// <see cref="LocationFavourites"/>
        /// </summary>
        /// <param name="locationType">Which of Country/State/City to add to</param>
        /// <param name="locationValue">The filter string to be added</param>
        static internal void AddToLocationFavouriteList(string locationType, string locationValue)
        {
            if (locationType == "Country")
            {
                LocationFavourites[CurrentLocationFavouriteIndex].Countries.Add(locationValue);
                LocationFavourites[CurrentLocationFavouriteIndex].Countries = [.. LocationFavourites[CurrentLocationFavouriteIndex].Countries.Distinct()];
                LocationFavourites[CurrentLocationFavouriteIndex].Countries.Sort();
            }
            else if (locationType == "State")
            {
                LocationFavourites[CurrentLocationFavouriteIndex].States.Add(locationValue);
                LocationFavourites[CurrentLocationFavouriteIndex].States = [.. LocationFavourites[CurrentLocationFavouriteIndex].States.Distinct()];
                LocationFavourites[CurrentLocationFavouriteIndex].States.Sort();
            }
            else
            {
                LocationFavourites[CurrentLocationFavouriteIndex].Cities.Add(locationValue);
                LocationFavourites[CurrentLocationFavouriteIndex].Cities = [.. LocationFavourites[CurrentLocationFavouriteIndex].Cities.Distinct()];
                LocationFavourites[CurrentLocationFavouriteIndex].Cities.Sort();
            }
        }

        /// <summary>
        /// Clears filter string list for one of Country/State/City for <see cref="CurrentLocationFavouriteIndex"/> in 
        /// <see cref="LocationFavourites"/>
        /// </summary>
        /// <param name="locationType">Which Country/State/City filter string list to clear </param>
        static internal void ClearLocationFavouriteList(string locationType)
        {
            if (locationType == "Country")
                LocationFavourites[CurrentLocationFavouriteIndex].Countries.Clear();
            else if (locationType == "State")
                LocationFavourites[CurrentLocationFavouriteIndex].States.Clear();
            else
                LocationFavourites[CurrentLocationFavouriteIndex].Cities.Clear();
        }

        /// <summary>
        /// Deletes filter string from one of Country/State/City for <see cref="CurrentLocationFavouriteIndex"/> in 
        /// <see cref="LocationFavourites"/>
        /// </summary>
        /// <param name="locationType">Which of Country/State/City to delete from</param>
        /// <param name="locationValue">The filter string to be deleted</param>
        static internal void DeleteFromLocationFavouriteList(string locationType, string locationValue)
        {
            if (locationType == "Country")
            {
                LocationFavourites[CurrentLocationFavouriteIndex].Countries.Remove(locationValue);
                if (LocationFavourites[CurrentLocationFavouriteIndex].Countries.Count == 0)
                    AddToLocationFavouriteList(locationType, "None");
            }
            else if (locationType == "State")
            {
                LocationFavourites[CurrentLocationFavouriteIndex].States.Remove(locationValue);
                if (LocationFavourites[CurrentLocationFavouriteIndex].States.Count == 0)
                    AddToLocationFavouriteList(locationType, "None");
            }
            else
            {
                LocationFavourites[CurrentLocationFavouriteIndex].Cities.Remove(locationValue);
                if (LocationFavourites[CurrentLocationFavouriteIndex].Cities.Count == 0)
                    AddToLocationFavouriteList(locationType, "None");
            }
        }

        /// <summary>
        ///  Gets a filter string to display in the Country/State/City fields on the General tab of form.
        ///  The current favourite may include more than one filter value for one or more of these fields.
        ///  Displays the first of the filter values as the list is maintained sorted.
        /// </summary>
        /// <param name="locationType">Which of Country/State/City fields the display filter value is for</param>
        /// <returns>The Country/State/City field display filter value</returns>
        static internal string GetLocationFavouriteDisplayFilterValue(string locationType)
        {
            if (locationType == "Country")
                return LocationFavourites[CurrentLocationFavouriteIndex].Countries[0];
            else if (locationType == "State")
                return LocationFavourites[CurrentLocationFavouriteIndex].States[0];
            else
                return LocationFavourites[CurrentLocationFavouriteIndex].Cities[0];
        }

        /// <summary>
        /// Get a sorted list of the location favourite names
        /// </summary>
        /// <returns>Sorted list of the location favourite names</returns>
        static internal List<string> GetLocationFavouriteNames()
        {
            List<string> locationFavouriteNames = [];

            for (int i = 0; i < LocationFavourites.Count; i++)
            {
                locationFavouriteNames.Add(LocationFavourites[i].Name);
            }
            locationFavouriteNames.Sort();
            return locationFavouriteNames;
        }

        /// <summary>
        /// Deletes filter string from one of Country/State/City for <see cref="CurrentLocationFavouriteIndex"/> in 
        /// <see cref="LocationFavourites"/>
        /// </summary>
        /// <param name="locationType">Which of Country/State/City to delete from</param>
        /// <param name="locationValue">The filter string to be deleted</param>
        static internal void DeleteFilterValueFromLocationFavourite(string locationType, string locationValue)
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
        /// Combines the Country/State/City location filters into a single string for display using
        /// a tooltip with MouseHover event over TextBoxGeneralLocationFilters
        /// </summary>
        /// <returns>Country/State/City location filters combined into a single string</returns>
        static internal string SetTextBoxGeneralLocationFilters()
        {
            string filters;

            filters = "Countries = \"";
            filters += SetTextBoxGeneralLocationFilter(LocationFavourites[CurrentLocationFavouriteIndex].Countries);
            filters += "\" \nStates = \"";
            filters += SetTextBoxGeneralLocationFilter(LocationFavourites[CurrentLocationFavouriteIndex].States);
            filters += "\" \nCities = \"";
            filters += SetTextBoxGeneralLocationFilter(LocationFavourites[CurrentLocationFavouriteIndex].Cities);
            filters += "\"";
            return filters;
        }

        /// <summary>
        /// Updates current location favourite with new name if that name has not already been used
        /// </summary>
        /// <param name="newLocationFavouriteName">The new location favourite name</param>
        /// <returns>The replaced current location favourite name</returns>
        static internal string UpdateLocationFavouriteName(string newLocationFavouriteName)
        {
            // Save name of current location favourite before changing
            string oldLocationFavouriteName = LocationFavourites[CurrentLocationFavouriteIndex].Name;

            // Make sure new name is not already in use and then change in current location favourite
            if (LocationFavourites.FindAll(favourite => favourite.Name == newLocationFavouriteName).Count == 0)
                LocationFavourites[CurrentLocationFavouriteIndex].Name = newLocationFavouriteName;

            // Return old favourite name so a new location favourite of that name can be created
            return oldLocationFavouriteName;
        }

        /// <summary>
        /// Add a location favourite to end of <see cref="LocationFavourites"/>, no need to
        /// adjust <see cref="CurrentLocationFavouriteIndex"/> as it is unaffected by adding to
        /// end of the list unless it's the first favourite to be added in which case set
        /// <see cref="CurrentLocationFavouriteIndex"/> to zero.
        /// </summary>
        /// <param name="name">The name for the new <see cref="LocationFavourite"/> to be added</param>
        static internal void AddLocationFavourite(string name)
        {
            // Create a deep copy using the extension method
            LocationFavourite deepCopyFav = new(LocationFavourites[CurrentLocationFavouriteIndex])
            {
                Name = name
            };
            LocationFavourites.Add(deepCopyFav);
            if (LocationFavourites.Count == 1)
                CurrentLocationFavouriteIndex = 0;
        }

        /// <summary>
        /// Delete a location favourite from <see cref="LocationFavourites"/> and adjust 
        /// <see cref="CurrentLocationFavouriteIndex"/> to zero
        /// </summary>
        /// <param name="deleteLocationFavouriteName">The name of the  <see cref="LocationFavourite"/> to be deleted</param>
        /// <returns>The name of the zero index <see cref="LocationFavourites"/> location favourite</returns>
        static internal string DeleteLocationFavourite(string deleteLocationFavouriteName)
        {
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
        /// Combines one of Country/State/City location filters into a single string for display using
        /// a tooltip with MouseHover event over TextBoxGeneralLocationFilters
        /// </summary>
        /// <param name="locationFilterStrings">The list of location filter strings to be combined</param>
        /// <returns>One of Country/State/City location filters combined into a single string</returns>
        static private string SetTextBoxGeneralLocationFilter(List<string> locationFilterStrings)
        {
            string filters = "";

            foreach (string filterString in locationFilterStrings)
                filters += $"{filterString}, ";
            filters = filters.Trim();
            filters = filters.Trim(',');

            return filters;
        }

        /// <summary>
        /// Get the <see cref="CurrentLocationFavouriteIndex"/> instance in <see cref="LocationFavourites"/>
        /// </summary>
        /// <returns>The <see cref="CurrentLocationFavouriteIndex"/> instance in <see cref="LocationFavourites"/></returns>
        static internal LocationFavourite GetCurrentLocationFavourite()
        {
            return LocationFavourites[CurrentLocationFavouriteIndex];
        }

        /// <summary>
        /// Reset <see cref="CurrentLocationFavouriteIndex"/> to the instance of <see cref="LocationFavourite"/>
        /// with newFavouriteName
        /// </summary>
        /// <param name="newFavouriteName">The name of the new instance to be set as <see cref="CurrentLocationFavouriteIndex"/></param>
        static internal void ChangeCurrentLocationFavouriteIndex(string newFavouriteName)
        {
            LocationFavourite locationFavourite = LocationFavourites.Find(favourite => favourite.Name == newFavouriteName);
            CurrentLocationFavouriteIndex = LocationFavourites.IndexOf(locationFavourite);
        }

        /// <summary>
        /// Saves the current list of location favourites to a file in JSON format using CacheManager.
        /// The save operation is skipped if the list is empty to prevent overwriting a valid file.
        /// </summary>
        /// <param name="progressReporter">Optional. Can be <see langword="null"/> if progress or error reporting to the UI is not required.</param>
        internal static void SaveLocationFavourites(IProgress<string> progressReporter = null)
        {
            // Do not save if the list is empty to prevent overwriting a valid file with an empty one.
            if (LocationFavourites == null || LocationFavourites.Count == 0)
            {
                string warningMessage = "Location favourites list is empty. Save operation aborted to prevent data loss.";
                Log.Warning(warningMessage);
                progressReporter?.Report(warningMessage);
                return;
            }

            string appDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                                   AppDomain.CurrentDomain.FriendlyName);
            string filePath = Path.Combine(appDataDirectory, "LocationFavouritesJSON.txt");

            try
            {
                CacheManager.SerializeToFile(LocationFavourites, filePath);
                Log.Info($"Successfully saved {LocationFavourites.Count} location favourites to '{filePath}'.");
                progressReporter?.Report($"Location favourites saved ({LocationFavourites.Count} entries).");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to save location favourites. Details: {ex.Message}", ex);
                progressReporter?.Report($"ERROR: Failed to save location favourites: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads the list of location favourites from a file stored in JSON format.
        /// It first attempts to load a local user-created version. If the local file does not exist,
        /// or if it contains an empty list, it falls back to a non-empty embedded resource.
        /// </summary>
        /// </summary>
        /// <param name="progressReporter">Optional. Can be <see langword="null"/> if progress or error reporting to the UI is not required.</param>
        /// <returns><see langword="true"/> if location favourites were successfully loaded, <see langword="false"/> otherwise.</returns>
        internal static bool LoadLocationFavourites(IProgress<string> progressReporter = null)
        {
            // Initialize LocationFavourites to an empty list to prevent NullReferenceException on failure.
            LocationFavourites = [];
            string appDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                                   AppDomain.CurrentDomain.FriendlyName);
            string filePath = Path.Combine(appDataDirectory, "LocationFavouritesJSON.txt");

            // Flag to determine if we need to fall back to the embedded resource.
            bool needsFallback = false;

            try
            {
                // First, try to load from the local file using CacheManager.
                Log.Info($"Attempting to load location favourites from local file: {filePath}");
                LocationFavourites = CacheManager.DeserializeFromFile<List<LocationFavourite>>(filePath);

                // If the local file was loaded successfully but is empty, trigger a fallback.
                if (LocationFavourites == null || LocationFavourites.Count == 0)
                {
                    string warningMessage = "Local favourites file was empty or contained no valid data. Falling back to embedded resource.";
                    Log.Warning(warningMessage);
                    progressReporter?.Report(warningMessage);
                    needsFallback = true;
                }
            }
            catch (FileNotFoundException)
            {
                // If the local file is not found, automatically fall back to the embedded resource.
                Log.Warning("Local favourites file not found. Falling back to embedded resource.");
                needsFallback = true;
            }
            catch (Exception ex)
            {
                // Catch any other exceptions (e.g., JsonException) from the primary deserialize attempt.
                string errorMessage = $"An error occurred while processing local favourites file: {ex.Message}";
                Log.Error(errorMessage, ex);
                progressReporter?.Report($"ERROR: {errorMessage}");
                needsFallback = true;
            }

            if (needsFallback)
            {
                try
                {
                    if (FileOps.TryGetResourceStream("Text.LocationFavouritesJSON.txt", progressReporter, out Stream resourceStream))
                    {
                        using (resourceStream)
                        {
                            LocationFavourites = System.Text.Json.JsonSerializer.Deserialize<List<LocationFavourite>>(resourceStream);
                            Log.Info("Successfully loaded location favourites from embedded resource.");
                        }
                    }
                    else
                    {
                        // FileOps.TryGetResourceStream already reported the error.
                        LocationFavourites = []; // Ensure an empty list on final failure.
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    // Catch any errors during embedded resource deserialization.
                    string errorMessage = $"An unexpected error occurred during fallback to embedded resource: {ex.Message}";
                    Log.Error(errorMessage, ex);
                    progressReporter?.Report($"ERROR: {errorMessage}");
                    LocationFavourites = [];
                    return false;
                }
            }

            // Final check to report the outcome.
            if (LocationFavourites.Count > 0)
            {
                Log.Info($"Successfully loaded {LocationFavourites.Count} location favourites.");
                progressReporter?.Report($"Location favourites loaded ({LocationFavourites.Count} entries).");
                return true;
            }
            else
            {
                // The list is still empty after all attempts.
                string warningMessage = "All attempts to load a valid list of location favourites failed. The list will be empty.";
                Log.Warning(warningMessage);
                progressReporter?.Report(warningMessage);
                return false;
            }
        }

        #endregion
    }
}