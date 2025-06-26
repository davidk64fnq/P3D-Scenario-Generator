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
        internal static void GetWebDoc(string url, string saveFile)
        {
            try
            {
                HtmlAgilityPack.HtmlDocument htmlDoc = GetWebDoc(url);
                using FileStream newfs = new(saveFile, FileMode.Create);
                htmlDoc.Save(newfs, htmlDoc.Encoding);
            }
            catch (Exception ex)
            {
                Log.Error($"Encountered issues saving web document to \"{saveFile}\": {ex.Message}");
            }
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

            using (HttpClient client = new HttpClient())
            {
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
