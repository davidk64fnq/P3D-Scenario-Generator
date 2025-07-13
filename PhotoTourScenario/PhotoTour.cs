using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.MapTiles;

namespace P3D_Scenario_Generator.PhotoTourScenario
{
    /// <summary>
    /// Defines the possible outcomes or results when setting or processing a leg (segment)
    /// within the photo tour generation process.
    /// </summary>
    public enum SetLegResult
    {
        /// <summary>
        /// Indicates that the operation or process completed successfully without any issues.
        /// </summary>
        Success,
        /// <summary>
        /// Indicates an error occurred during a file system operation, such as reading from or writing to a file.
        /// </summary>
        FileOperationError,
        /// <summary>
        /// Indicates that a web download attempt failed, possibly due to network issues,
        /// unreachable server, or invalid URL.
        /// </summary>
        WebDownloadFailed,
        /// <summary>
        /// Indicates that an error occurred while parsing an HTML document.
        /// This could be due to malformed HTML or an inability to locate expected elements.
        /// </summary>
        HtmlParsingFailed,
        /// <summary>
        /// Indicates that an expected airport could not be found based on the provided criteria.
        /// </summary>
        NoAirportFound,
        /// <summary>
        /// Indicates that a candidate for the next photo in the sequence could not be located or identified.
        /// </summary>
        NoNextPhotoFound,
        /// <summary>
        /// Indicates an unexpected logical error or an inconsistency in the program's flow.
        /// </summary>
        LogicError
    }

    /// <summary>
    /// Stores information pertaining to a photo location in the photo tour, also used for start and destination airports
    /// </summary>
    public class PhotoLocParams
    {
        /// <summary>
        /// URL of photo this leg travels to
        /// </summary>
        public string photoURL;

        /// <summary>
        /// Unique id string used by pic2map for each photo
        /// </summary>
        public string legId;

        /// <summary>
        /// Only used for start and destination airport instances
        /// </summary>
        public string airportICAO;

        /// <summary>
        /// Only used for start and destination airport instances
        /// </summary>
        public int airportIndex;

        /// <summary>
        /// Used to filter on location string for starting photo in tour
        /// </summary>
        public string location;

        /// <summary>
        /// Distance from this instance location to next location in photo tour
        /// </summary>
        public double forwardDist;

        /// <summary>
        /// Latitude for this instance location
        /// </summary>
        public double latitude;

        /// <summary>
        /// Longitude for this instance location
        /// </summary>
        public double longitude;

        /// <summary>
        /// Bearing to get from this instance location to next location in photo tour
        /// </summary>
        public double forwardBearing; 
    }

    /// <summary>
    /// Orchestrates the generation and management of a dynamic photo tour.
    /// This includes finding a sequence of geolocated photos and associated airports,
    /// managing their data, creating visual map representations of the tour legs,
    /// and handling the downloading and resizing of tour photos.
    /// </summary>
    internal class PhotoTour
    {
        /// <summary>
        /// List of geolocated random photos webscraped from pic2map plus start and finish airports
        /// </summary>
        internal static List<PhotoLocParams> PhotoLocations { get; private set; }

        /// <summary>
        /// Lat/Lon boundaries for each OSM montage leg image
        /// </summary>
        internal static List<MapEdges> PhotoTourLegMapEdges { get; private set; } 

        /// <summary>
        /// Includes start and finish airports
        /// </summary>
        internal static int PhotoCount { get; private set; }

        /// <summary>
        /// Populate PhotoLocations plus set airport(s) and create OSM images
        /// </summary>
        static internal bool SetPhotoTour(ScenarioFormData formData)
        {
            // First, try to generate the random photo tour
            if (!SetRandomPhotoTour(formData)) 
            {
                Log.Error("Failed to generate a random photo tour.");
                return false; 
            }

            bool drawRoute = false;
            if (!MapTileImageMaker.CreateOverviewImage(PhotoTourUtilities.SetOverviewCoords(PhotoLocations), drawRoute, formData))
            {
                Log.Error("Failed to create overview image during photo tour setup.");
                return false;
            }

            if (!MapTileImageMaker.CreateLocationImage(PhotoTourUtilities.SetLocationCoords(), formData))
            {
                Log.Error("Failed to create location image during photo tour setup.");
                return false;
            }

            PhotoTourLegMapEdges = [];
            drawRoute = true;
            for (int index = 0; index < PhotoLocations.Count - 1; index++)
            {
                int legNo = index + 1; 
                if (!MapTileImageMaker.SetLegRouteImages(PhotoTourUtilities.SetRouteCoords(PhotoLocations, index), PhotoTourLegMapEdges, legNo, drawRoute, formData))
                {
                    Log.Error($"Failed to create route image for leg {index} during photo tour setup.");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Creates the photo tour by finding a random pic2map photo page with nearby airport, then
        /// looking for a series of photo pages within bearing and distance constraints, then that
        /// a finish airport is the required distance range from the last photo location.
        /// </summary>
        /// <returns>True if a complete photo tour was successfully created; otherwise, false.</returns>
        static internal bool SetRandomPhotoTour(ScenarioFormData formData)
        {
            // Define max attempts to prevent infinite loops if tour generation is consistently difficult
            int maxOverallAttempts = formData.PhotoTourMaxSearchAttempts;
            int currentOverallAttempt = 0;
            bool tourSuccessfullyFormed = false;

            // Clear PhotoLocations at the start of the entire process, as it will be populated per attempt
            PhotoLocations = [];

            while (!tourSuccessfullyFormed && currentOverallAttempt < maxOverallAttempts)
            {
                currentOverallAttempt++;
                Log.Info($"SetRandomPhotoTour: Attempting to generate photo tour (Attempt {currentOverallAttempt}/{maxOverallAttempts}).");
                PhotoLocations.Clear(); // Clear for each new attempt to find a tour

                // 1. Try to find the first leg (starting airport and first photo)
                SetLegResult firstLegResult = SetFirstLeg(formData);
                if (firstLegResult == SetLegResult.NoAirportFound)
                {
                    continue; // Skip to next overall attempt
                }
                else if (firstLegResult != SetLegResult.Success)
                {
                    Log.Error($"SetRandomPhotoTour: Failed while attempting to set first leg.");
                    return false;
                }

                // 2. Try to add subsequent photos up to the maximum number of legs
                SetLegResult nextLegResult = SetLegResult.Success;
                while (PhotoLocations.Count < formData.PhotoTourMaxNoLegs && nextLegResult != SetLegResult.NoNextPhotoFound)
                {
                    nextLegResult = SetNextLeg(formData);
                    if (nextLegResult != SetLegResult.NoNextPhotoFound && nextLegResult != SetLegResult.Success)
                    {
                        Log.Error($"SetRandomPhotoTour: Failed while attempting to set subsequent leg.");
                        return false;
                    }
                }

                // 3. If candidate route has enough legs, try to locate a destination airport
                if (PhotoLocations.Count >= formData.PhotoTourMinNoLegs)
                {
                    SetLegResult lastLegResult = SetLastLeg(formData);
                    if (lastLegResult == SetLegResult.NoAirportFound)
                    {
                        continue; // Skip to next overall attempt
                    }
                    else if (firstLegResult != SetLegResult.Success)
                    {
                        Log.Error($"SetRandomPhotoTour: Failed while attempting to set last leg.");
                        return false;
                    }
                    else
                    {
                        tourSuccessfullyFormed = true; // A complete tour was found
                        Log.Info($"SetRandomPhotoTour: Successfully generated a photo tour after {currentOverallAttempt} attempts.");
                    }
                }
            }

            if (!tourSuccessfullyFormed)
            {
                Log.Error($"SetRandomPhotoTour: Failed to generate a complete photo tour after {maxOverallAttempts} attempts.");
                // Ensure PhotoLocations is empty or in a known failed state if no tour was formed
                PhotoLocations.Clear();
            }
            return tourSuccessfullyFormed;
        }

        /// <summary>
        /// Downloads a random photo page from pic2map site and tries to find nearby airport within
        /// the required distance range. If found adds the starting airport and first photo to the photo tour.
        /// </summary>
        /// <returns>True if first leg created</returns>
        static internal SetLegResult SetFirstLeg(ScenarioFormData formData)
        {
            PhotoLocParams airportLocation;
            string pic2mapHtmlSaveLocation = $"{formData.ScenarioImageFolder}\\random_pic2map.html";

            // Clear last attempt
            PhotoLocations = [];

            // Get starting random photo page
            if (!FileOps.TryDeleteFile(pic2mapHtmlSaveLocation))
            {
                Log.Error($"SetFirstLeg: Failed to delete previous random photo file at '{pic2mapHtmlSaveLocation}'.");
                return SetLegResult.FileOperationError;
            }

            HttpRoutines.GetWebDoc("https://www.pic2map.com/random.php", pic2mapHtmlSaveLocation);
            if (!File.Exists(pic2mapHtmlSaveLocation)) // Indirect check for download success
            {
                Log.Error($"SetFirstLeg: Web document was not saved to '{pic2mapHtmlSaveLocation}'. HttpRoutines.GetWebDoc likely failed.");
                return SetLegResult.WebDownloadFailed;
            }

            PhotoLocParams photoLocation = new();
            if (Pic2MapHtmlParser.ExtractPhotoParams(pic2mapHtmlSaveLocation, photoLocation) != SetLegResult.Success)
            {
                Log.Error($"SetFirstLeg: Failed to extract valid photo parameters from '{pic2mapHtmlSaveLocation}'.");
                return SetLegResult.HtmlParsingFailed;
            }


            // Find nearby airport to starting random photo
            airportLocation = GetNearbyAirport(photoLocation.latitude, photoLocation.longitude, formData.PhotoTourMinLegDist, formData.PhotoTourMaxLegDist);
            if (airportLocation == null)
            {
                return SetLegResult.NoAirportFound;
            }
            formData.RunwayIndex = airportLocation.airportIndex;
            Runway.startRwy = Runway.Runways[formData.RunwayIndex];
            airportLocation.forwardBearing = MathRoutines.GetReciprocalHeading(airportLocation.forwardBearing);
            PhotoLocations.Add(airportLocation);
            PhotoLocations.Add(photoLocation);
            return SetLegResult.Success;
        }

        /// <summary>
        /// Calls GetNearbyAirport method in Runway class to look for an airport within required distance
        /// from a photo location. Populates an instance of PhotoLocParams with the airport information
        /// </summary>
        /// <param name="queryLat">The photo location latitude</param>
        /// <param name="queryLon">The photo location longitude</param>
        /// <param name="minDist">Minimum required distance between photo location and airport</param>
        /// <param name="maxDist">Maximum required distance between photo location and airport</param>
        /// <returns></returns>
        static internal PhotoLocParams GetNearbyAirport(double queryLat, double queryLon, double minDist, double maxDist)
        {
            PhotoLocParams photoLocationParams = new();
            RunwayParams nearbyAirport = Runway.GetNearbyRunway(queryLat, queryLon, minDist, maxDist);
            if (nearbyAirport == null)
                return null;
            photoLocationParams.legId = nearbyAirport.IcaoId; // So that field isn't null when searching photo tour for photo locations included
            photoLocationParams.airportICAO = nearbyAirport.IcaoId;
            photoLocationParams.airportIndex = nearbyAirport.RunwaysIndex;
            photoLocationParams.forwardDist = MathRoutines.CalcDistance(queryLat, queryLon, nearbyAirport.AirportLat, nearbyAirport.AirportLon);
            photoLocationParams.latitude = nearbyAirport.AirportLat;
            photoLocationParams.longitude = nearbyAirport.AirportLon;
            photoLocationParams.forwardBearing = MathRoutines.CalcBearing(queryLat, queryLon, nearbyAirport.AirportLat, nearbyAirport.AirportLon);
            photoLocationParams.location = nearbyAirport.State + nearbyAirport.City + nearbyAirport.Country;
            return photoLocationParams;
        }
        
        /// <summary>
        /// Attempts to find and add the next suitable photo location to the photo tour.
        /// It first determines a candidate photo from the current photo's web page that meets
        /// distance and bearing constraints and has not been previously included in the tour.
        /// If a suitable photo is found, its details are downloaded, parsed, and added to the tour.
        /// </summary>
        /// <returns>
        /// <see cref="SetLegResult.Success"/> if a suitable next photo is found and successfully added.
        /// <see cref="SetLegResult.LogicError"/> if the <see cref="PhotoLocations"/> list is empty,
        /// indicating an invalid state for determining the next leg.
        /// <see cref="SetLegResult.NoNextPhotoFound"/> if no qualifying photo is identified within
        /// the constraints or the search loop finishes without a suitable candidate.
        /// <see cref="SetLegResult.HtmlParsingFailed"/> if there are issues extracting or parsing data
        /// from the HTML document for either the current or the next photo.
        /// <see cref="SetLegResult.FileOperationError"/> if there's a problem deleting temporary files.
        /// <see cref="SetLegResult.WebDownloadFailed"/> if the next photo's web page cannot be downloaded.
        /// </returns>
        static internal SetLegResult SetNextLeg(ScenarioFormData formData) 
        {
            double distance = 9999;
            double bearing = 0;
            string pic2mapHtmlSaveLocation = $"{formData.ScenarioImageFolder}\\random_pic2map.html"; // Re-using for the next photo page
            string photoURL = "";

            // Ensure there's a previous photo location to work from
            if (PhotoLocations == null || PhotoLocations.Count == 0)
            {
                Log.Error("SetNextLeg: PhotoLocations list is empty. Cannot determine next leg.");
                return SetLegResult.LogicError; 
            }

            // Get next nearest unselected photo URL, distance, and bearing (that meets constraints)
            SetLegResult getNextPhotoResult = GetNextPhoto(pic2mapHtmlSaveLocation, ref distance, ref bearing, ref photoURL, formData);
            if (getNextPhotoResult == SetLegResult.NoNextPhotoFound)
            {
                return SetLegResult.NoNextPhotoFound;
            }
            if (getNextPhotoResult == SetLegResult.HtmlParsingFailed)
            {
                Log.Warning("SetNextLeg: Encountered HTML parsing error attempting to find a suitable next photo that meets distance/bearing constraints or is not already in the tour.");
                return SetLegResult.HtmlParsingFailed;
            }

            // Add forward distance and bearing for this next nearest unselected photo to last selected photo location
            // This assumes PhotoLocations[^1] exists and is valid, checked by PhotoLocations.Count check above.
            PhotoLocations[^1].forwardDist = distance;
            PhotoLocations[^1].forwardBearing = bearing;

            // Extract next nearest unselected photo location parameters
            if (!FileOps.TryDeleteFile(pic2mapHtmlSaveLocation))
            {
                Log.Error($"SetNextLeg: Failed to delete previous random photo file at '{pic2mapHtmlSaveLocation}'. Cannot proceed.");
                return SetLegResult.FileOperationError;
            }

            // HttpRoutines.GetWebDoc now returns bool
            if (!HttpRoutines.GetWebDoc(photoURL, pic2mapHtmlSaveLocation))
            {
                Log.Error($"SetNextLeg: Failed to download web document from '{photoURL}' to '{pic2mapHtmlSaveLocation}'. Check HttpRoutines logs for details.");
                return SetLegResult.WebDownloadFailed;
            }

            if (!File.Exists(pic2mapHtmlSaveLocation)) // Defensive check, though HttpRoutines.GetWebDoc should cover this
            {
                Log.Error($"SetNextLeg: Downloaded web document was not found at '{pic2mapHtmlSaveLocation}' after HttpRoutines.GetWebDoc call.");
                return SetLegResult.WebDownloadFailed; 
            }

            PhotoLocParams photoLocation = new();
            if (Pic2MapHtmlParser.ExtractPhotoParams(pic2mapHtmlSaveLocation, photoLocation) != SetLegResult.Success)
            {
                Log.Error($"SetNextLeg: Failed to extract valid photo parameters from '{pic2mapHtmlSaveLocation}'. HTML parsing issue?");
                return SetLegResult.HtmlParsingFailed;
            }

            PhotoLocations.Add(photoLocation);
            return SetLegResult.Success;
        }

        /// <summary>
        /// Attempts to locate a suitable next photo from the current photo's web page.
        /// It iterates through nearby photo candidates, leveraging a helper method to parse their
        /// URLs, distances, and coordinates from the HTML document.
        /// The method then applies tour-specific distance and bearing constraints, and ensures
        /// the candidate photo has not been previously included in the tour.
        /// </summary>
        /// <param name="curPhotoFileLocation">The full path to the local Pic2Map HTML file of the current photo.</param>
        /// <param name="distance">An output parameter that will hold the distance to the found next photo if successful.</param>
        /// <param name="bearing">An output parameter that will hold the bearing to the found next photo if successful.</param>
        /// <param name="photoURL">An output parameter that will hold the URL of the found next photo if successful.</param>
        /// <returns>
        /// <see cref="SetLegResult.Success"/> if a suitable next photo is found that meets all criteria.
        /// <see cref="SetLegResult.NoNextPhotoFound"/> if no qualifying photo is identified within the constraints
        /// or if all candidates have been exhausted.
        /// <see cref="SetLegResult.HtmlParsingFailed"/> if there are issues retrieving the HTML document
        /// or if the helper method encounters errors extracting or parsing data from the HTML document for a candidate photo.
        /// </returns>
        static private SetLegResult GetNextPhoto(string curPhotoFileLocation, ref double distance, ref double bearing, ref string photoURL, ScenarioFormData formData)
        {
            var htmlDoc = HttpRoutines.GetHtmlDocumentFromFile(curPhotoFileLocation);
            if (htmlDoc == null)
            {
                return SetLegResult.HtmlParsingFailed;
            }

            int index = 1;
            double nextLat = 0;
            double nextLon = 0;
            while (index <= Constants.PhotoMaxNearby)
            {
                string nextPhotoURL = "";
                if (Pic2MapHtmlParser.ExtractNextPhotoCoordsFromNearbyList(htmlDoc, index, curPhotoFileLocation, 
                    ref distance, ref nextLat, ref nextLon, ref nextPhotoURL) != SetLegResult.Success)
                {
                    Log.Error($"GetNextPhoto: Failed to extract next photo coordinates or URL for index {index} from HTML document at {curPhotoFileLocation}");
                    return SetLegResult.HtmlParsingFailed;
                }

                // Calculate candidate next photo bearing change from current photo location
                bearing = MathRoutines.CalcBearing(PhotoLocations[^1].latitude, PhotoLocations[^1].longitude, nextLat, nextLon);
                int headingChange = MathRoutines.CalcHeadingChange(PhotoLocations[^2].forwardBearing, bearing);

                // Does candidate next photo satisfy distance and bearing constraints and has not already been included
                if (distance <= formData.PhotoTourMaxLegDist && distance >= formData.PhotoTourMinLegDist &&
                    Math.Abs(headingChange) < formData.PhotoTourMaxBearingChange &&
                    PhotoLocations.FindIndex(leg => nextPhotoURL.Contains(leg.legId)) == -1)
                {
                    photoURL = nextPhotoURL; // Set the output parameter to the found photo URL
                    return SetLegResult.Success;
                }
                index++;
            }
            return SetLegResult.NoNextPhotoFound;
        }

        /// <summary>
        /// Tries to find a nearby airport to the last photo location within the required distance range.
        /// If found adds the airport to the photo tour.
        /// </summary>
        /// <returns>SetLegResult indicating success or type of failure.</returns>
        static internal SetLegResult SetLastLeg(ScenarioFormData formData) // Changed return type to SetLegResult
        {
            PhotoLocParams airportLocation;

            // Ensure there's at least one photo location to find an airport for
            if (PhotoLocations == null || PhotoLocations.Count == 0)
            {
                Log.Error("SetLastLeg: PhotoLocations list is empty. Cannot determine last leg (destination airport).");
                return SetLegResult.LogicError; // Add this specific enum value
            }

            // Find nearby airport to last photo
            airportLocation = GetNearbyAirport(PhotoLocations[^1].latitude, PhotoLocations[^1].longitude,
                formData.PhotoTourMinLegDist, formData.PhotoTourMaxLegDist);

            // Clean up the temporary pic2map HTML file.
            // Even if this fails, we might still have a valid tour, but log the issue.
            string tempHtmlFile = $"{formData.ScenarioImageFolder}\\random_pic2map.html";
            if (!FileOps.TryDeleteFile(tempHtmlFile))
            {
                Log.Warning($"SetLastLeg: Failed to delete temporary HTML file at '{tempHtmlFile}'. This is not critical for tour generation but should be investigated.");
                // We don't return false here if only the file deletion failed, as the core task might still succeed.
            }

            if (airportLocation != null)
            {
                Runway.destRwy = Runway.Runways[airportLocation.airportIndex];
                PhotoLocations.Add(airportLocation);
                PhotoCount = PhotoLocations.Count;
                PhotoTourUtilities.GetPhotos(PhotoLocations, formData); // Call GetPhotos only when the tour is fully established
                return SetLegResult.Success;
            }
            else
            {
                return SetLegResult.NoAirportFound; 
            }
        }
    }
}
