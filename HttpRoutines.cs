using HtmlAgilityPack;

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
        /// Downloads a file from a specified URL to a local path using HttpClient synchronously.
        /// This method will block the calling thread until the download is complete.
        /// </summary>
        /// <param name="url">The URL of the file to download.</param>
        /// <param name="saveFile">The full path where the downloaded file will be saved.</param>
        public static void DownloadBinaryFile(string url, string saveFile)
        {
            try
            {
                // It's generally recommended to reuse HttpClient instances for performance.
                // In a small, self-contained method like this, 'using' is acceptable.
                using (HttpClient client = new())
                {
                    // Send a GET request and get the response synchronously.
                    // .Result waits for the async operation to complete.
                    using (HttpResponseMessage response = client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).Result)
                    {
                        // Throw an exception if the HTTP response status is not a success code (2xx).
                        response.EnsureSuccessStatusCode();

                        // Ensure the directory exists before writing the file
                        var directory = Path.GetDirectoryName(saveFile);
                        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        // Get the content as a stream synchronously.
                        // .Result waits for the async operation to complete.
                        using (Stream contentStream = response.Content.ReadAsStreamAsync().Result)
                        {
                            // Create a file stream to write the downloaded content.
                            using (FileStream fileStream = new(saveFile, FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                // Copy the content stream to the file stream synchronously.
                                // While CopyToAsync is available, CopyTo is the synchronous equivalent.
                                contentStream.CopyTo(fileStream);
                            }
                        }
                    }
                }
            }
            catch (AggregateException ae)
            {
                // When using .Result on a Task, exceptions are wrapped in an AggregateException.
                // You should unwrap them to get to the inner exception.
                ae.Handle(innerEx =>
                {
                    if (innerEx is HttpRequestException httpEx)
                    {
                        Log.Error($"HTTP error downloading file from URL \"{url}\": {httpEx.Message}");
                        return true; // Mark as handled
                    }
                    else if (innerEx is TaskCanceledException)
                    {
                        Log.Error($"Download from URL \"{url}\" was cancelled.");
                        return true; // Mark as handled
                    }
                    else
                    {
                        // For any other unhandled exception within the AggregateException
                        Log.Error($"An unexpected error occurred while downloading \"{url}\": {innerEx.Message}");
                        return true; // Mark as handled
                    }
                });
            }
            catch (Exception ex)
            {
                // Catches any other general exceptions not wrapped by AggregateException
                Log.Error($"An unexpected error occurred while downloading \"{url}\": {ex.Message}");
            }
        }

    }
}
