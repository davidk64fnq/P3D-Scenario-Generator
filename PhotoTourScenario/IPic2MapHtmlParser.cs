using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace P3D_Scenario_Generator.PhotoTourScenario
{
    /// <summary>
    /// Defines the contract for an asynchronous HTML parsing service specific to Pic2Map.
    /// </summary>
    public interface IPic2MapHtmlParser
    {
        /// <summary>
        /// Asynchronously extracts photo-related parameters from a local Pic2Map HTML file.
        /// </summary>
        Task<SetLegResult> ExtractPhotoParamsAsync(string pic2mapHtmlSaveLocation, PhotoLocParams photoLocation);

        /// <summary>
        /// Asynchronously extracts latitude and longitude for a candidate next photo from embedded JavaScript.
        /// </summary>
        Task<(SetLegResult result, double latitude, double longitude)> ExtractNextPhotoCoordsFromJSAsync(HtmlDocument htmlDoc, string id);

        /// <summary>
        /// Asynchronously extracts the URL, distance, and coordinates for a nearby photo candidate from an HTML list.
        /// </summary>
        Task<(SetLegResult result, double distance, double nextLat, double nextLon, string nextPhotoURL)> ExtractNextPhotoCoordsFromNearbyListAsync(HtmlDocument htmlDoc, int index, string curPhotoFileLocation);
    }
}
