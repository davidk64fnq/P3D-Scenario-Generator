// Interfaces/IHttpRoutines.cs
// This file contains the updated interface for HttpRoutines.

using HtmlAgilityPack;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace P3D_Scenario_Generator.Interfaces
{
    /// <summary>
    /// Defines the contract for a service that provides various HTTP-related routines.
    /// </summary>
    public interface IHttpRoutines
    {
        /// <summary>
        /// Asynchronously retrieves and parses an HTML document from the specified URL.
        /// </summary>
        /// <param name="url">The URL of the HTML document to retrieve.</param>
        /// <returns>An HtmlAgilityPack.HtmlDocument object if successful; otherwise, null.</returns>
        Task<HtmlDocument> GetWebDocAsync(string url);

        /// <summary>
        /// Asynchronously retrieves an HTML document from the specified URL and saves its content to a local file.
        /// </summary>
        /// <param name="url">The URL of the HTML document to retrieve.</param>
        /// <param name="saveFile">The full path where the HTML document will be saved.</param>
        /// <returns>True if the web document was successfully retrieved and saved; otherwise, false.</returns>
        Task<bool> GetWebDocAsync(string url, string saveFile);

        /// <summary>
        /// Asynchronously loads and parses an HTML document from a local file path.
        /// </summary>
        /// <param name="filePath">The full path to the local HTML file.</param>
        /// <returns>An HtmlAgilityPack.HtmlDocument object if successful; otherwise, null.</returns>
        Task<HtmlDocument> GetHtmlDocumentFromFileAsync(string filePath);

        /// <summary>
        /// Asynchronously validates an OSM tile server API key.
        /// </summary>
        /// <param name="apiKey">The API key string to be validated.</param>
        /// <returns>True if the API key is valid; otherwise, false.</returns>
        Task<bool> ValidateMapTileServerKeyAsync(string apiKey);

        /// <summary>
        /// Asynchronously downloads a file from a specified URL to a local path.
        /// </summary>
        /// <param name="url">The URL of the file to download.</param>
        /// <param name="saveFile">The full path where the downloaded file will be saved.</param>
        /// <returns>True if the file was downloaded and saved successfully; otherwise, false.</returns>
        Task<bool> DownloadBinaryFileAsync(string url, string saveFile);

        /// <summary>
        /// Safely gets the InnerText of a node found by an XPath expression.
        /// </summary>
        /// <param name="node">The parent HTML node to start the search from.</param>
        /// <param name="xpath">The XPath expression to select the target node.</param>
        /// <returns>The InnerText of the selected node, or null if the node is not found.</returns>
        string SelectSingleNodeInnerText(HtmlNode node, string xpath);
    }
}
