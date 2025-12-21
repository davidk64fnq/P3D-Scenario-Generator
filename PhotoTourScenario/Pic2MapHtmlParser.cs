using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.Services;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace P3D_Scenario_Generator.PhotoTourScenario
{
    /// <summary>
    /// Provides methods for parsing Pic2Map HTML documents to extract photo-related information.
    /// This class is a non-static implementation of IPic2MapHtmlParser, allowing for
    /// dependency injection of a logging and HTML parsing service.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the Pic2MapHtmlParser class.
    /// </remarks>
    /// <param name="logger">The logging service used to report errors.</param>
    /// <param name="htmlParser">The HTML parsing service used to select nodes and extract data.</param>
    /// <param name="httpRoutines">The HTTP routines service used to get HTML documents from a file.</param>
    public class Pic2MapHtmlParser(Logger logger, HttpRoutines httpRoutines, HtmlParser htmlParser)
    {
        private readonly Logger _log = logger;
        private readonly HttpRoutines _httpRoutines = httpRoutines; 
        private readonly HtmlParser _htmlParser = htmlParser;

        /// <inheritdoc/>
        /// <summary>
        /// Asynchronously extracts photo-related parameters from a local Pic2Map HTML file.
        /// This includes the photo URL, its unique ID, location string, latitude, and longitude.
        /// </summary>
        /// <param name="pic2mapHtmlSaveLocation">The full path to the local Pic2Map HTML file to parse.</param>
        /// <param name="photoLocation">The PhotoLocParams object to populate with the extracted data.</param>
        /// <returns>A <see cref="SetLegResult"/> indicating the outcome of the operation.</returns>
        public async Task<SetLegResult> ExtractPhotoParamsAsync(string pic2mapHtmlSaveLocation, PhotoLocParams photoLocation)
        {
            // CORRECTED: Use the injected _httpRoutines instance and the async method.
            HtmlDocument htmlDoc = await _httpRoutines.GetHtmlDocumentFromFileAsync(pic2mapHtmlSaveLocation);
            if (htmlDoc == null)
            {
                await _log.ErrorAsync($"Failed to get HTML document from file: {pic2mapHtmlSaveLocation}");
                return SetLegResult.HtmlParsingFailed;
            }

            // Extract photo URL from the HTML document
            string photoURLSelection = $"//meta[@property='og:image']";
            var (urlSuccess, photoURL) = await _htmlParser.SelectSingleNodeGetAttributeValueAsync(htmlDoc, photoURLSelection, "content");
            if (!urlSuccess)
            {
                await _log.ErrorAsync($"ExtractPhotoParamsAsync: Could not find photo URL in HTML document at {pic2mapHtmlSaveLocation}");
                return SetLegResult.HtmlParsingFailed;
            }
            photoLocation.photoURL = photoURL;
            photoLocation.legId = Path.GetFileNameWithoutExtension(photoLocation.photoURL);

            // Extract location from the HTML document
            string locationSelection = $"//meta[@name='Description']";
            var (locationSuccess, location) = await _htmlParser.SelectSingleNodeGetAttributeValueAsync(htmlDoc, locationSelection, "content");
            if (!locationSuccess)
            {
                await _log.ErrorAsync($"ExtractPhotoParamsAsync: Could not find location in HTML document at {pic2mapHtmlSaveLocation}");
                return SetLegResult.HtmlParsingFailed;
            }
            photoLocation.location = location;

            // Extract latitude from the HTML document
            string latitudeSelection = "//div[@id='gpsinformation']/following-sibling::ul[@class='details'][1]/li/div[@class='dbox']/span[@class='dtab' " +
                                       "and text()='Latitude:']/following-sibling::span[@class='dvalue']";
            var (latitudeSuccess, latitudeString) = await _htmlParser.SelectSingleNodeInnerTextAsync(htmlDoc, latitudeSelection);
            if (!latitudeSuccess)
            {
                await _log.ErrorAsync($"ExtractPhotoParamsAsync: Could not find latitude in HTML document at {pic2mapHtmlSaveLocation}");
                return SetLegResult.HtmlParsingFailed;
            }
            if (!double.TryParse(latitudeString, out photoLocation.latitude))
            {
                await _log.ErrorAsync($"Could not convert '{latitudeString}' to a double. Original string: '{latitudeSelection}'");
                return SetLegResult.HtmlParsingFailed;
            }

            // Extract longitude from the HTML document
            string longitudeSelection = "//div[@id='gpsinformation']/following-sibling::ul[@class='details'][1]/li/div[@class='dbox']/span[@class='dtab' " +
                                        "and text()='Longitude:']/following-sibling::span[@class='dvalue']";
            var (longitudeSuccess, longitudeString) = await _htmlParser.SelectSingleNodeInnerTextAsync(htmlDoc, longitudeSelection);
            if (!longitudeSuccess)
            {
                await _log.ErrorAsync($"ExtractPhotoParamsAsync: Could not find longitude in HTML document at {pic2mapHtmlSaveLocation}");
                return SetLegResult.HtmlParsingFailed;
            }
            if (!double.TryParse(longitudeString, out photoLocation.longitude))
            {
                await _log.ErrorAsync($"Could not convert '{longitudeString}' to a double. Original string: '{longitudeSelection}'");
                return SetLegResult.HtmlParsingFailed;
            }

            return SetLegResult.Success;
        }

        /// <inheritdoc/>
        /// <summary>
        /// Asynchronously extracts latitude and longitude for a candidate next photo from embedded JavaScript.
        /// </summary>
        /// <param name="htmlDoc">The HTML document containing the script with photo coordinate data.</param>
        /// <param name="id">The unique ID of the next photo candidate.</param>
        /// <returns>A tuple containing a <see cref="SetLegResult"/> and the extracted latitude and longitude.</returns>
        public async Task<(SetLegResult result, double latitude, double longitude)> ExtractNextPhotoCoordsFromJSAsync(HtmlDocument htmlDoc, string id)
        {
            double latitude = 0;
            double longitude = 0;

            string scriptSelection = $"//body[1]/script[1]";
            var (scriptSuccess, scriptString) = await _htmlParser.SelectSingleNodeInnerTextAsync(htmlDoc, scriptSelection);
            if (!scriptSuccess)
            {
                await _log.ErrorAsync($"ExtractNextPhotoCoordsFromJSAsync: Could not find script section with id '{id}'");
                return (SetLegResult.HtmlParsingFailed, latitude, longitude);
            }

            int idIndex = scriptString.IndexOf(id);
            if (idIndex == -1)
            {
                await _log.ErrorAsync($"Unable to locate next photo id '{id}' in html code section listing next photo candidates.");
                return (SetLegResult.HtmlParsingFailed, latitude, longitude);
            }

            // Truncate the script string by removing all characters from its beginning up to (but not including) the character at the position indicated by idIndex
            scriptString = scriptString[idIndex..];
            string[] words = scriptString.Split(',');
            if (words.Length < Constants.PhotoIdMinSegments)
            {
                await _log.ErrorAsync($"Photo id '{id}' in html code section listing has less than expected '{Constants.PhotoIdMinSegments}' comma separated segments.");
                return (SetLegResult.HtmlParsingFailed, latitude, longitude);
            }
            if (!double.TryParse(words[Constants.PhotoLatSegIndex], out latitude))
            {
                await _log.ErrorAsync($"Could not convert '{words[Constants.PhotoLatSegIndex]}' to a latitude double");
                return (SetLegResult.HtmlParsingFailed, latitude, longitude);
            }
            if (!double.TryParse(words[Constants.PhotoLonSegIndex], out longitude))
            {
                await _log.ErrorAsync($"Could not convert '{words[Constants.PhotoLonSegIndex]}' to a longitude double");
                return (SetLegResult.HtmlParsingFailed, latitude, longitude);
            }

            return (SetLegResult.Success, latitude, longitude);
        }

        /// <inheritdoc/>
        /// <summary>
        /// Asynchronously extracts the URL, distance, and coordinates for a nearby photo candidate from an HTML list.
        /// </summary>
        /// <param name="htmlDoc">The HTML document of the current photo's page.</param>
        /// <param name="index">The 1-based index of the list item for the candidate next photo.</param>
        /// <param name="curPhotoFileLocation">The full path to the local HTML file of the current photo.</param>
        /// <returns>A tuple containing a <see cref="SetLegResult"/>, the extracted distance, coordinates, and URL.</returns>
        public async Task<(SetLegResult result, double distance, double nextLat, double nextLon, string nextPhotoURL)> ExtractNextPhotoCoordsFromNearbyListAsync(HtmlDocument htmlDoc, int index, string curPhotoFileLocation)
        {
            double distance = 0;
            double nextLat = 0;
            double nextLon = 0;
            string nextPhotoURL = string.Empty;

            // Get the URL of candidate next photo
            string nextPhotoSelection = $"//li[{index}]/div[@class='dbox'][1]/a[1]";
            var (urlSuccess, photoURL) = await _htmlParser.SelectSingleNodeGetAttributeValueAsync(htmlDoc, nextPhotoSelection, "href");
            if (!urlSuccess)
            {
                await _log.ErrorAsync($"GetNextPhoto: Could not find photo URL in HTML document at {curPhotoFileLocation}");
                return (SetLegResult.HtmlParsingFailed, distance, nextLat, nextLon, nextPhotoURL);
            }
            nextPhotoURL = photoURL;

            // Get the distance to candidate next photo
            string nextDistSelection = $"//li[{index}]/div[@class='dbox'][1]/p[@class='undertitletext'][1]";
            var (distSuccess, nextDist) = await _htmlParser.SelectSingleNodeInnerTextAsync(htmlDoc, nextDistSelection);
            if (!distSuccess)
            {
                await _log.ErrorAsync($"GetNextPhoto: Could not find distance in HTML document at {curPhotoFileLocation}");
                return (SetLegResult.HtmlParsingFailed, distance, nextLat, nextLon, nextPhotoURL);
            }

            // Parse nextDist string to extract the distance value
            if (nextDist == null)
            {
                await _log.ErrorAsync("nextDist is null. Cannot extract distance.");
                return (SetLegResult.HtmlParsingFailed, distance, nextLat, nextLon, nextPhotoURL);
            }
            string[] words = nextDist.Split('/');
            if (words.Length < 2 || string.IsNullOrEmpty(words[1]))
            {
                await _log.ErrorAsync($"nextDist format is invalid. Expected at least two parts separated by '/'. Received: '{nextDist}'");
                return (SetLegResult.HtmlParsingFailed, distance, nextLat, nextLon, nextPhotoURL);
            }
            string potentialNumberString = words[1];
            const int suffixLength = 11;
            if (potentialNumberString.Length < suffixLength)
            {
                await _log.ErrorAsync($"The second part of nextDist ('{potentialNumberString}') is too short to extract the number. Expected length >= {suffixLength}. Full string: '{nextDist}'");
                return (SetLegResult.HtmlParsingFailed, distance, nextLat, nextLon, nextPhotoURL);
            }
            string numberString = potentialNumberString[..^suffixLength];
            if (!double.TryParse(numberString, out distance))
            {
                await _log.ErrorAsync($"Could not convert '{numberString}' to a double. Original string: '{nextDist}'");
                return (SetLegResult.HtmlParsingFailed, distance, nextLat, nextLon, nextPhotoURL);
            }

            // Get id of candidate next photo - used to check hasn't already been included in tour
            string id = Path.GetFileNameWithoutExtension(nextPhotoURL);
            if (id == null || id.Length > Constants.PhotoIdLengthChars || !id.All(c => char.IsLetter(c) && char.IsLower(c)))
            {
                await _log.ErrorAsync($"Next candidate photo id string '{id}' is not the expected 5 or 6 lowercase alphabetic characters format");
                return (SetLegResult.HtmlParsingFailed, distance, nextLat, nextLon, nextPhotoURL);
            }

            // Get candidate next photo coordinates and bearing
            var (coordsResult, lat, lon) = await ExtractNextPhotoCoordsFromJSAsync(htmlDoc, id);
            if (coordsResult != SetLegResult.Success)
            {
                await _log.ErrorAsync($"GetNextPhoto: Failed to extract coordinates for photo ID '{id}'.");
                return (SetLegResult.HtmlParsingFailed, distance, nextLat, nextLon, nextPhotoURL);
            }
            nextLat = lat;
            nextLon = lon;

            return (SetLegResult.Success, distance, nextLat, nextLon, nextPhotoURL);
        }
    }
}
