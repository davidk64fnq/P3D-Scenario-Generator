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
