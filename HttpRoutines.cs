using HtmlAgilityPack;
using System.Net;

namespace P3D_Scenario_Generator
{
    /// <summary>
    /// Provides utility methods for performing various HTTP-related routines,
    /// including retrieving and parsing HTML documents, and downloading general files.
    /// All operations in this class are performed synchronously.
    /// </summary>
    internal class HttpRoutines
    {
        /// <summary>
        /// Retrieves and parses an HTML document from the specified URL using HtmlAgilityPack.
        /// </summary>
        /// <param name="url">The URL of the HTML document to retrieve.</param>
        /// <returns>An HtmlAgilityPack.HtmlDocument object if successful; otherwise, null.</returns>
        internal static HtmlAgilityPack.HtmlDocument GetWebDoc(string url)
        {
            HtmlAgilityPack.HtmlDocument htmlDoc = null;
            try
            {
                HtmlWeb web = new();
                htmlDoc = web.Load(url);
            }
            catch (Exception ex)
            {
                Log.Error($"Encountered issues obtaining web document for the URL \"{url}\": { ex.Message}");
            }
            return htmlDoc;
        }

        /// <summary>
        /// Retrieves an HTML document from the specified URL and saves its content to a local file.
        /// This method re-uses the existing GetWebDoc(string url) to fetch the document.
        /// </summary>
        /// <param name="url">The URL of the HTML document to retrieve.</param>
        /// <param name="saveFile">The full path where the HTML document will be saved.</param>
        /// <returns>True if the web document was successfully retrieved and saved; otherwise, false.</returns>
        internal static bool GetWebDoc(string url, string saveFile) // Changed return type to bool
        {
            HtmlAgilityPack.HtmlDocument htmlDoc;
            try
            {
                // This line assumes a GetWebDoc(string url) method exists and is capable of
                // fetching the document. If it fails, it should ideally return null.
                htmlDoc = GetWebDoc(url); // Call to existing GetWebDoc(string url)
            }
            catch (Exception ex)
            {
                // Catch any exceptions during the web document retrieval itself (e.g., network issues)
                Log.Error($"HttpRoutines.GetWebDoc: An error occurred while retrieving web document from \"{url}\": {ex.Message}");
                return false; // Indicate failure
            }

            if (htmlDoc == null)
            {
                // This condition handles cases where GetWebDoc(string url) returns null on failure,
                // or if the document could not be parsed for some reason.
                Log.Error($"HttpRoutines.GetWebDoc: Failed to retrieve web document from \"{url}\", as returned document was null. Cannot save to \"{saveFile}\".");
                return false; // Indicate failure
            }

            try
            {
                // Save the HtmlDocument content to a MemoryStream first
                // Using MemoryStream ensures we handle HtmlAgilityPack's saving
                // before passing it to FileOps for file system writing.
                using MemoryStream ms = new();
                htmlDoc.Save(ms, htmlDoc.Encoding);
                ms.Position = 0; // Reset stream position to the beginning for reading

                // Now use the FileOps method to save the stream content to the file.
                // FileOps.TryCopyStreamToFile handles its own logging and message box display.
                if (!FileOps.TryCopyStreamToFile(ms, saveFile))
                {
                    // FileOps has already logged and potentially shown a message.
                    // We just need to return false to signal failure to the caller.
                    Log.Error($"HttpRoutines.GetWebDoc: Failed to save web document stream to '{saveFile}' using FileOps.TryCopyStreamToFile. Check FileOps logs for details.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                // This catch handles exceptions specifically from htmlDoc.Save(ms, ...)
                // or issues related to MemoryStream operations.
                Log.Error($"HttpRoutines.GetWebDoc: An unexpected error occurred while preparing or saving HTML document content to stream for \"{url}\" to \"{saveFile}\": {ex.Message}");
                return false;
            }

            return true; // All operations completed successfully
        }

        /// <summary>
        /// Loads and parses an HTML document from a local file path using HtmlAgilityPack.
        /// </summary>
        /// <param name="filePath">The full path to the local HTML file.</param>
        /// <returns>An HtmlAgilityPack.HtmlDocument object if successful; otherwise, null.</returns>
        internal static HtmlAgilityPack.HtmlDocument GetHtmlDocumentFromFile(string filePath)
        {
            // Ensure the file exists before attempting to load it.
            if (!File.Exists(filePath))
            {
                Log.Error($"GetHtmlDocumentFromFile: The specified file does not exist: \"{filePath}\"");
                return null;
            }

            HtmlAgilityPack.HtmlDocument htmlDoc = null;
            try
            {
                htmlDoc = new HtmlAgilityPack.HtmlDocument();
                htmlDoc.Load(filePath);
            }
            catch (Exception ex)
            {
                Log.Error($"GetHtmlDocumentFromFile: Error loading HTML document from \"{filePath}\": {ex.Message}");
            }
            return htmlDoc;
        }

        /// <summary>
        /// Validates the OSM tile server API key by making a test request and checking the HTTP status code synchronously.
        /// This method uses HttpClient, which provides direct access to HTTP status codes.
        /// </summary>
        /// <returns><see langword="true"/> if the API key is valid and a successful response (2xx status code) is received; otherwise, <see langword="false"/>.</returns>
        internal static bool ValidateMapTileServerKey()
        {
            // Use a minimal tile URL for validation, e.g., zoom 0, x 0, y 0
            string apiKey = Parameters.SettingsCacheServerAPIkey;       // Get the user-supplied key
            apiKey = apiKey.Replace("\"", "").Replace("'", "").Trim();  // Remove quotes and trim whitespace
            string url = $"{Constants.OSMtileServerURLprefix}/0/0/0.png?rapidapi-key={apiKey}";

            using HttpClient client = new();
            try
            {
                // Make the GET request synchronously.
                // HttpCompletionOption.ResponseHeadersRead ensures we only download headers, not the full image content,
                // as we only care about the HTTP status for validation.
                HttpResponseMessage response = client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).Result;

                // Check for success status codes (2xx)
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                {
                    // Specific handling for common API key errors
                    string errorMessage = $"The API key specified on Settings tab is not valid. (HTTP Status: {(int)response.StatusCode} {response.StatusCode})";
                    MessageBox.Show(errorMessage, "OSM tile server API key", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
                else
                {
                    // Handle other non-success status codes (e.g., 404 Not Found, 500 Internal Server Error)
                    string errorMessage = $"Server error while validating API key: {(int)response.StatusCode} {response.StatusCode}";
                    MessageBox.Show(errorMessage, "OSM tile server API key", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
            }
            catch (AggregateException ae)
            {
                // AggregateException wraps exceptions thrown from awaited tasks when .Result is called.
                // We need to unwrap them to get the actual cause.
                ae.Handle(innerEx =>
                {
                    if (innerEx is HttpRequestException httpEx)
                    {
                        // This catches network errors (DNS issues, connection refused) or
                        // HttpExceptions thrown if an internal check failed before a response was fully processed.
                        HttpStatusCode? statusCode = httpEx.StatusCode; // C# 6+ feature to get status from HttpRequestException

                        string errorMessage;
                        if (statusCode.HasValue)
                        {
                            errorMessage = $"The API key specified on Settings tab is not valid or server error: {(int)statusCode} {statusCode}";
                        }
                        else
                        {
                            errorMessage = $"A network connection error occurred while validating the API key: {httpEx.Message}";
                        }
                        MessageBox.Show(errorMessage, "OSM tile server API key", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Log.Error($"Network error during API key validation for URL \"{url}\": {httpEx.Message}");
                        return true; // Mark exception as handled
                    }
                    else if (innerEx is TaskCanceledException)
                    {
                        // This would indicate the request timed out or was explicitly cancelled
                        string errorMessage = $"API key validation request timed out or was cancelled for URL \"{url}\".";
                        MessageBox.Show(errorMessage, "OSM tile server API key", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Log.Error(errorMessage);
                        return true; // Mark handled
                    }
                    return false; // Let other types of exceptions propagate
                });
                return false; // Indicate failure due to an handled exception
            }
            catch (Exception ex) // Catch any other unexpected exceptions
            {
                string errorMessage = $"An unexpected error occurred during API key validation: {ex.Message}";
                MessageBox.Show(errorMessage, "OSM tile server API key", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log.Error($"Unexpected error during API key validation for URL \"{url}\": {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Downloads a file from a specified URL to a local path using HttpClient synchronously.
        /// This method will block the calling thread until the download is complete.
        /// </summary>
        /// <param name="url">The URL of the file to download.</param>
        /// <param name="saveFile">The full path where the downloaded file will be saved.</param>
        /// <returns><see langword="true"/> if the file was downloaded and saved successfully; otherwise, <see langword="false"/>.</returns>
        public static bool DownloadBinaryFile(string url, string saveFile)
        {
            try
            {
                using HttpClient client = new();
                using HttpResponseMessage response = client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).Result;
                response.EnsureSuccessStatusCode();

                using Stream contentStream = response.Content.ReadAsStreamAsync().Result;

                // Use the new FileOps.TryCopyStreamToFile method
                if (!FileOps.TryCopyStreamToFile(contentStream, saveFile))
                {
                    // TryCopyStreamToFile already handled logging and message box
                    return false;
                }

                return true;
            }
            catch (AggregateException ae)
            {
                ae.Handle(innerEx =>
                {
                    if (innerEx is HttpRequestException httpEx)
                    {
                        Log.Error($"HTTP error downloading file from URL \"{url}\": {httpEx.Message}");
                        return true;
                    }
                    else if (innerEx is TaskCanceledException)
                    {
                        Log.Error($"Download from URL \"{url}\" was cancelled.");
                        return true;
                    }
                    else
                    {
                        Log.Error($"An unexpected error occurred during HTTP request for \"{url}\": {innerEx.Message}");
                        return false;
                    }
                });
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"An unexpected error occurred while attempting to download from \"{url}\": {ex.Message}");
                return false;
            }
        }
    }
}
