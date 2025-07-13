using P3D_Scenario_Generator.ConstantsEnums;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace P3D_Scenario_Generator.PhotoTourScenario
{
    /// <summary>
    /// Provides static methods for parsing Pic2Map HTML documents to extract photo-related information.
    /// This includes extracting main photo parameters like URL, ID, location, and coordinates,
    /// as well as details for nearby photo candidates.
    /// </summary>
    internal class Pic2MapHtmlParser
    {
        /// <summary>
        /// Extracts various photo-related parameters from a local Pic2Map HTML file
        /// and populates the provided <see cref="PhotoLocParams"/> object.
        /// This includes the photo URL, its unique ID, location string, latitude, and longitude.
        /// </summary>
        /// <param name="pic2mapHtmlSaveLocation">The full path to the local Pic2Map HTML file to parse.</param>
        /// <param name="photoLocation">The <see cref="PhotoLocParams"/> object to populate with the extracted data.</param>
        /// <returns>
        /// <see cref="SetLegResult.Success"/> if all parameters are successfully extracted and parsed.
        /// <see cref="SetLegResult.HtmlParsingFailed"/> if the HTML document cannot be loaded or any required data
        /// (photo URL, location, latitude, longitude) cannot be found or parsed from the document.
        /// </returns>
        static internal SetLegResult ExtractPhotoParams(string pic2mapHtmlSaveLocation, PhotoLocParams photoLocation)
        {
            // Get html document from the saved file
            HtmlDocument htmlDoc = HttpRoutines.GetHtmlDocumentFromFile(pic2mapHtmlSaveLocation);
            if (htmlDoc == null)
            {
                return SetLegResult.HtmlParsingFailed;
            }

            // Extract photo URL from the HTML document
            string photoURLSelection = $"//meta[@property='og:image']";
            if (!HtmlParser.SelectSingleNodeGetAttributeValue(htmlDoc, photoURLSelection, "content", out photoLocation.photoURL))
            {
                Log.Error($"ExtractPhotoParams: Could not find photo URL in HTML document at {pic2mapHtmlSaveLocation}");
                return SetLegResult.HtmlParsingFailed;
            }
            photoLocation.legId = Path.GetFileNameWithoutExtension(photoLocation.photoURL);

            // Extract location from the HTML document
            string locationSelection = $"//meta[@name='Description']";
            if (!HtmlParser.SelectSingleNodeGetAttributeValue(htmlDoc, locationSelection, "content", out photoLocation.location))
            {
                Log.Error($"ExtractPhotoParams: Could not find location in HTML document at {pic2mapHtmlSaveLocation}");
                return SetLegResult.HtmlParsingFailed;
            }

            // Extract latitude from the HTML document
            string latitudeSelection = "//div[@id='gpsinformation']/following-sibling::ul[@class='details'][1]/li/div[@class='dbox']/span[@class='dtab' " +
                "and text()='Latitude:']/following-sibling::span[@class='dvalue']"; ;
            if (!HtmlParser.SelectSingleNodeInnerText(htmlDoc, latitudeSelection, out string latitudeString))
            {
                Log.Error($"ExtractPhotoParams: Could not find latitude in HTML document at {pic2mapHtmlSaveLocation}");
                return SetLegResult.HtmlParsingFailed;
            }
            if (!double.TryParse(latitudeString, out photoLocation.latitude))
            {
                Log.Error($"Could not convert '{latitudeString}' to a double. Original string: '{latitudeSelection}'");
                return SetLegResult.HtmlParsingFailed;
            }

            // Extract longitude from the HTML document
            string longitudeSelection = "//div[@id='gpsinformation']/following-sibling::ul[@class='details'][1]/li/div[@class='dbox']/span[@class='dtab' " +
                "and text()='Longitude:']/following-sibling::span[@class='dvalue']"; ;
            if (!HtmlParser.SelectSingleNodeInnerText(htmlDoc, longitudeSelection, out string longitudeString))
            {
                Log.Error($"ExtractPhotoParams: Could not find longitude in HTML document at {pic2mapHtmlSaveLocation}");
                return SetLegResult.HtmlParsingFailed;
            }
            if (!double.TryParse(longitudeString, out photoLocation.longitude))
            {
                Log.Error($"Could not convert '{longitudeString}' to a double. Original string: '{longitudeSelection}'");
                return SetLegResult.HtmlParsingFailed;
            }

            return SetLegResult.Success;
        }

        /// <summary>
        /// Extracts the latitude and longitude coordinates for a candidate next photo
        /// from an embedded JavaScript string within the provided HTML document.
        /// The method locates the relevant data using the photo's unique ID.
        /// </summary>
        /// <param name="htmlDoc">The HTML document containing the script with photo coordinate data.</param>
        /// <param name="id">The unique ID string of the next photo candidate, used to locate its data in the script.</param>
        /// <param name="latitude">An output parameter that will hold the extracted latitude if successful.</param>
        /// <param name="longitude">An output parameter that will hold the extracted longitude if successful.</param>
        /// <returns>
        /// <see cref="SetLegResult.Success"/> if both latitude and longitude are successfully extracted and parsed.
        /// <see cref="SetLegResult.HtmlParsingFailed"/> if the script cannot be found, the photo ID is not located
        /// within the script, the script format is unexpected (e.g., insufficient segments), or coordinate values cannot be parsed to doubles.
        /// </returns>
        static internal SetLegResult ExtractNextPhotoCoordsFromJS(HtmlDocument htmlDoc, string id, ref double latitude, ref double longitude)
        {
            string scriptSelection = $"//body[1]/script[1]";
            if (!HtmlParser.SelectSingleNodeInnerText(htmlDoc, scriptSelection, out string scriptString))
            {
                Log.Error($"ExtractNextPhotoCoords: Could not find latitude in HTML document with id '{id}'");
                return SetLegResult.HtmlParsingFailed;
            }

            int idIndex = scriptString.IndexOf(id);
            if (idIndex == -1)
            {
                Log.Error($"Unable to locate next photo id '{id}' in html code section listing next photo candidates.");
                return SetLegResult.HtmlParsingFailed;
            }

            // Truncate the script string by removing all characters from its beginning up to (but not including) the character at the position indicated by idIndex
            scriptString = scriptString[idIndex..];
            string[] words = scriptString.Split(',');
            if (words.Length < Constants.PhotoIdMinSegments)
            {
                Log.Error($"Photo id '{id}' in html code section listing has less than expected '{Constants.PhotoIdMinSegments}' comma separated segments.");
                return SetLegResult.HtmlParsingFailed;
            }
            if (!double.TryParse(words[Constants.PhotoLatSegIndex], out latitude))
            {
                Log.Error($"Could not convert '{words[Constants.PhotoLatSegIndex]}' to a latitude double");
                return SetLegResult.HtmlParsingFailed;
            }
            if (!double.TryParse(words[Constants.PhotoLonSegIndex], out longitude))
            {
                Log.Error($"Could not convert '{words[Constants.PhotoLonSegIndex]}' to a longitude double");
                return SetLegResult.HtmlParsingFailed;
            }

            return SetLegResult.Success;
        }

        /// <summary>
        /// Extracts the URL, distance, and geographical coordinates (latitude and longitude)
        /// for a candidate "next photo" from a specific entry within the "nearby photos" list
        /// found in a Pic2Map HTML document. This method also derives and validates the photo's unique ID.
        /// </summary>
        /// <param name="htmlDoc">The HTML document of the current photo's page, containing the list of nearby photos.</param>
        /// <param name="index">The 1-based index of the specific list item representing the candidate next photo.</param>
        /// <param name="curPhotoFileLocation">The full path to the local Pic2Map HTML file of the current photo (used for logging context).</param>
        /// <param name="distance">An output parameter that will hold the parsed distance to the candidate next photo.</param>
        /// <param name="nextLat">An output parameter that will hold the extracted latitude of the candidate next photo.</param>
        /// <param name="nextLon">An output parameter that will hold the extracted longitude of the candidate next photo.</param>
        /// <param name="nextPhotoURL">An output parameter that will hold the extracted URL of the candidate next photo.</param>
        /// <returns>
        /// <see cref="SetLegResult.Success"/> if all required photo parameters are successfully extracted and parsed.
        /// <see cref="SetLegResult.HtmlParsingFailed"/> if any HTML element cannot be found, if extracted strings
        /// are in an unexpected format, or if coordinate/distance values cannot be parsed correctly.
        /// </returns>
        static internal SetLegResult ExtractNextPhotoCoordsFromNearbyList(HtmlDocument htmlDoc, int index, string curPhotoFileLocation,
            ref double distance, ref double nextLat, ref double nextLon, ref string nextPhotoURL)
        {
            // Get the URL of candidate next photo
            string nextPhotoSelection = $"//li[{index}]/div[@class='dbox'][1]/a[1]";
            if (!HtmlParser.SelectSingleNodeGetAttributeValue(htmlDoc, nextPhotoSelection, "href", out nextPhotoURL))
            {
                Log.Error($"GetNextPhoto: Could not find photo URL in HTML document at {curPhotoFileLocation}");
                return SetLegResult.HtmlParsingFailed;
            }

            // Get the distance to candidate next photo
            string nextDistSelection = $"//li[{index}]/div[@class='dbox'][1]/p[@class='undertitletext'][1]";
            if (!HtmlParser.SelectSingleNodeInnerText(htmlDoc, nextDistSelection, out string nextDist))
            {
                Log.Error($"GetNextPhoto: Could not find latitude in HTML document at {curPhotoFileLocation}");
                return SetLegResult.HtmlParsingFailed;
            }

            // Parse nextDist string to extract the distance value
            if (nextDist == null)
            {
                Log.Error("nextDist is null. Cannot extract distance.");
                return SetLegResult.HtmlParsingFailed;
            }
            string[] words = nextDist.Split('/'); if (words.Length < 2 || string.IsNullOrEmpty(words[1]))
            {
                Log.Error($"nextDist format is invalid. Expected at least two parts separated by '/'. Received: '{nextDist}'");
                return SetLegResult.HtmlParsingFailed;
            }
            string potentialNumberString = words[1];
            const int suffixLength = 11;
            if (potentialNumberString.Length < suffixLength)
            {
                Log.Error($"The second part of nextDist ('{potentialNumberString}') is too short to extract the number. Expected length >= {suffixLength}. Full string: '{nextDist}'");
                return SetLegResult.HtmlParsingFailed;
            }
            string numberString = potentialNumberString[..^suffixLength];
            if (!double.TryParse(numberString, out distance))
            {
                Log.Error($"Could not convert '{numberString}' to a double. Original string: '{nextDist}'");
                return SetLegResult.HtmlParsingFailed;
            }

            // Get id of candidate next photo - used to check hasn't already been included in tour
            string id = Path.GetFileNameWithoutExtension(nextPhotoURL);
            if (id == null || id.Length > Constants.PhotoIdLength || !id.All(c => char.IsLetter(c) && char.IsLower(c)))
            {
                Log.Error($"Next candidate photo id string '{id}' is not the expected 5 or 6 lowercase alphabetic characters format");
                return SetLegResult.HtmlParsingFailed;
            }

            // Get candidate next photo coordinates and bearing
            if (ExtractNextPhotoCoordsFromJS(htmlDoc, id, ref nextLat, ref nextLon) != SetLegResult.Success)
            {
                Log.Error($"GetNextPhoto: Failed to extract coordinates for photo ID '{id}'.");
                return SetLegResult.HtmlParsingFailed;
            }

            return SetLegResult.Success;
        }
    }
}
