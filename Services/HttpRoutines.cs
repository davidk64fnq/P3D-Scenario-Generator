using HtmlAgilityPack;
using P3D_Scenario_Generator.ConstantsEnums;
using System.Net;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace P3D_Scenario_Generator.Services
{
    /// <summary>
    /// Provides utility methods for performing various HTTP-related routines.
    /// This is a non-static class that uses dependency injection for its IFileOps and ILogger dependencies.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the HttpRoutines class.
    /// </remarks>
    /// <param name="fileOps">The file operations service.</param>
    /// <param name="logger">The logging service.</param>
    public class HttpRoutines(FileOps fileOps, Logger logger) 
    {
        private readonly FileOps _fileOps = fileOps;
        private readonly Logger _logger = logger;
        private readonly HttpClient _httpClient = new();

        /// <summary>
        /// Asynchronously retrieves and parses an HTML document from the specified URL using HtmlAgilityPack.
        /// </summary>
        /// <param name="url">The URL of the HTML document to retrieve.</param>
        /// <returns>An HtmlAgilityPack.HtmlDocument object if successful; otherwise, null.</returns>
        public async Task<HtmlDocument> GetWebDocAsync(string url)
        {
            HtmlDocument htmlDoc = null;
            try
            {
                HtmlWeb web = new();
                htmlDoc = await web.LoadFromWebAsync(url);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Encountered issues obtaining web document for the URL \"{url}\".", ex);
            }
            return htmlDoc;
        }

        /// <summary>
        /// Asynchronously retrieves an HTML document from the specified URL and saves its content to a local file.
        /// </summary>
        /// <param name="url">The URL of the HTML document to retrieve.</param>
        /// <param name="saveFile">The full path where the HTML document will be saved.</param>
        /// <returns>True if the web document was successfully retrieved and saved; otherwise, false.</returns>
        public async Task<bool> GetWebDocAsync(string url, string saveFile)
        {
            HtmlDocument htmlDoc;
            try
            {
                htmlDoc = await GetWebDocAsync(url);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"An error occurred while retrieving web document from \"{url}\".", ex);
                return false;
            }

            if (htmlDoc == null)
            {
                await _logger.ErrorAsync($"Failed to retrieve web document from \"{url}\", as returned document was null. Cannot save to \"{saveFile}\".");
                return false;
            }

            try
            {
                using MemoryStream ms = new();
                htmlDoc.Save(ms, htmlDoc.Encoding);
                ms.Position = 0;

                // Use the injected _fileOps instance and its async method
                if (!await _fileOps.TryCopyStreamToFileAsync(ms, saveFile, null))
                {
                    await _logger.ErrorAsync($"Failed to save web document stream to '{saveFile}' using IFileOps.TryCopyStreamToFileAsync.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"An unexpected error occurred while preparing or saving HTML document content for \"{url}\" to \"{saveFile}\".", ex);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Asynchronously loads and parses an HTML document from a local file path using HtmlAgilityPack.
        /// </summary>
        /// <param name="filePath">The full path to the local HTML file.</param>
        /// <returns>An HtmlAgilityPack.HtmlDocument object if successful; otherwise, null.</returns>
        public async Task<HtmlDocument> GetHtmlDocumentFromFileAsync(string filePath)
        {
            // Use the injected _fileOps to check for file existence
            if (!FileOps.FileExists(filePath))
            {
                await _logger.ErrorAsync($"GetHtmlDocumentFromFileAsync: The specified file does not exist: \"{filePath}\"");
                return null;
            }

            HtmlDocument htmlDoc = new();
            try
            {
                // Corrected approach: read the file content as a string first, then load into HtmlDocument
                var (success, content) = await _fileOps.TryReadAllTextAsync(filePath, null);
                if (success)
                {
                    htmlDoc.LoadHtml(content);
                }
                else
                {
                    await _logger.ErrorAsync($"GetHtmlDocumentFromFileAsync: Failed to read content from file: \"{filePath}\"");
                    return null;
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"GetHtmlDocumentFromFileAsync: Error loading HTML document from \"{filePath}\".", ex);
                return null;
            }
            return htmlDoc;
        }

        /// <summary>
        /// Asynchronously validates the OSM tile server API key by making a test request and checking the HTTP status code.
        /// </summary>
        /// <param name="apiKey">The API key string to be validated.</param>
        /// <returns><see langword="true"/> if the API key is valid and a successful response (2xx status code) is received; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> ValidateMapTileServerKeyAsync(string apiKey)
        {
            apiKey = apiKey.Replace("\"", "").Replace("'", "").Trim();
            string url = $"{Constants.OSMtileServerURLprefix}/0/0/0.png?rapidapi-key={apiKey}";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                {
                    string errorMessage = $"The API key specified on Settings tab is not valid. (HTTP Status: {(int)response.StatusCode} {response.StatusCode})";
                    await _logger.ErrorAsync(errorMessage);
                    return false;
                }
                else
                {
                    string errorMessage = $"Server error while validating API key: {(int)response.StatusCode} {response.StatusCode}";
                    await _logger.ErrorAsync(errorMessage);
                    return false;
                }
            }
            catch (HttpRequestException httpEx)
            {
                HttpStatusCode? statusCode = httpEx.StatusCode;

                string errorMessage;
                if (statusCode.HasValue)
                {
                    errorMessage = $"The API key specified on Settings tab is not valid or server error: {(int)statusCode} {statusCode}";
                }
                else
                {
                    errorMessage = $"A network connection error occurred while validating the API key: {httpEx.Message}";
                }
                await _logger.ErrorAsync(errorMessage, httpEx);
                return false;
            }
            catch (TaskCanceledException ex)
            {
                string errorMessage = $"API key validation request timed out or was cancelled for URL \"{url}\".";
                await _logger.ErrorAsync(errorMessage, ex);
                return false;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Unexpected error during API key validation for URL \"{url}\".", ex);
                return false;
            }
        }

        /// <summary>
        /// Asynchronously downloads a file from a specified URL to a local path using HttpClient.
        /// </summary>
        /// <param name="url">The URL of the file to download.</param>
        /// <param name="saveFile">The full path where the downloaded file will be saved.</param>
        /// <returns><see langword="true"/> if the file was downloaded and saved successfully; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> DownloadBinaryFileAsync(string url, string saveFile)
        {
            try
            {
                using HttpResponseMessage response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                using Stream contentStream = await response.Content.ReadAsStreamAsync();

                // Use the injected _fileOps and its async method
                if (!await _fileOps.TryCopyStreamToFileAsync(contentStream, saveFile, null))
                {
                    await _logger.ErrorAsync($"Download of binary file failed to save using IFileOps.TryCopyStreamToFileAsync from URL \"{url}\" to path \"{saveFile}\"");
                    return false;
                }
                return true;
            }
            catch (HttpRequestException httpEx)
            {
                await _logger.ErrorAsync($"HTTP error downloading file from URL \"{url}\".", httpEx);
                return false;
            }
            catch (TaskCanceledException ex)
            {
                await _logger.ErrorAsync($"Download from URL \"{url}\" was cancelled.", ex);
                return false;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"An unexpected error occurred while attempting to download from \"{url}\".", ex);
                return false;
            }
        }

        /// <summary>
        /// Safely gets the InnerText of a node found by an XPath expression.
        /// This method prevents NullReferenceExceptions by returning null if the node is not found.
        /// </summary>
        /// <param name="node">The parent HTML node to start the search from.</param>
        /// <param name="xpath">The XPath expression to select the target node.</param>
        /// <returns>The InnerText of the selected node, or null if the node is not found.</returns>
        public static string SelectSingleNodeInnerText(HtmlNode node, string xpath)
        {
            if (node == null)
            {
                return null;
            }

            HtmlNode selectedNode = node.SelectSingleNode(xpath);

            if (selectedNode != null)
            {
                return selectedNode.InnerText;
            }

            return null;
        }
    }
}
