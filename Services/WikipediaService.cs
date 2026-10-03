using System.Globalization;
using System.Text.Json;

namespace P3D_Scenario_Generator.Services
{
    /// <summary>
    /// Provides shared methods for querying Wikipedia's MediaWiki API for geo-referenced articles,
    /// article abstracts, and thumbnails.
    /// </summary>
    /// <param name="logger">The application logging service.</param>
    /// <param name="httpRoutines">The HTTP routines service.</param>
    internal class WikipediaService(Logger logger, HttpRoutines httpRoutines)
    {
        private readonly Logger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly HttpRoutines _httpRoutines = httpRoutines ?? throw new ArgumentNullException(nameof(httpRoutines));

        private const string MediaWikiApiUrl = "https://en.wikipedia.org/w/api.php";

        /// <summary>
        /// Searches Wikipedia for geo-referenced articles within a given radius of a latitude and longitude.
        /// </summary>
        /// <param name="latitude">Latitude in decimal degrees.</param>
        /// <param name="longitude">Longitude in decimal degrees.</param>
        /// <param name="radiusMeters">Search radius in meters (10m to 10,000m).</param>
        /// <param name="limit">Maximum number of results to return (1 to 50).</param>
        /// <returns>A list of matching <see cref="WikiPoi"/> items, sorted closest first.</returns>
        internal async Task<List<WikiPoi>> GetNearbyPoisAsync(double latitude, double longitude, int radiusMeters = 5000, int limit = 5)
        {
            List<WikiPoi> results = [];
            radiusMeters = Math.Clamp(radiusMeters, 10, 10000);
            limit = Math.Clamp(limit, 1, 50);

            string latStr = latitude.ToString("F6", CultureInfo.InvariantCulture);
            string lonStr = longitude.ToString("F6", CultureInfo.InvariantCulture);

            string url = $"{MediaWikiApiUrl}?action=query&list=geosearch&gscoord={latStr}|{lonStr}&gsradius={radiusMeters}&gslimit={limit}&format=json";

            var (success, json) = await _httpRoutines.GetStringFromWebAsync(url);
            if (!success || string.IsNullOrWhiteSpace(json))
            {
                return results;
            }

            try
            {
                var parsed = JsonSerializer.Deserialize<WikiApiResponse>(json);
                if (parsed?.Query?.GeoSearch != null)
                {
                    results.AddRange(parsed.Query.GeoSearch);
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"WikipediaService.GetNearbyPoisAsync: Failed to deserialize Wikipedia response for ({latStr}, {lonStr}). Details: {ex.Message}");
            }

            return results;
        }

        /// <summary>
        /// Enriches a collection of <see cref="WikiPoi"/> objects with brief text extracts and thumbnail image URLs.
        /// </summary>
        /// <param name="pois">The list of POIs to populate.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        internal async Task PopulateSummariesAndThumbnailsAsync(List<WikiPoi> pois)
        {
            if (pois == null || pois.Count == 0) return;

            var pageIds = pois.Select(p => p.PageId).Take(20);
            string pageIdParam = string.Join("|", pageIds);

            string url = $"{MediaWikiApiUrl}?action=query&prop=extracts|pageimages&exintro=1&explaintext=1&exchars=250&pithumbsize=400&pageids={pageIdParam}&format=json";

            var (success, json) = await _httpRoutines.GetStringFromWebAsync(url);
            if (!success || string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            try
            {
                var parsed = JsonSerializer.Deserialize<WikiApiResponse>(json);
                if (parsed?.Query?.Pages != null)
                {
                    foreach (var poi in pois)
                    {
                        string key = poi.PageId.ToString(CultureInfo.InvariantCulture);
                        if (parsed.Query.Pages.TryGetValue(key, out var detail) && detail != null)
                        {
                            poi.Extract = detail.Extract ?? string.Empty;
                            poi.ThumbnailUrl = detail.Thumbnail?.Source;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"WikipediaService.PopulateSummariesAndThumbnailsAsync: Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Finds the closest landmark within the specified radius and formats an informative display subtitle.
        /// </summary>
        /// <param name="latitude">Latitude in decimal degrees.</param>
        /// <param name="longitude">Longitude in decimal degrees.</param>
        /// <param name="radiusMeters">Search radius in meters (defaults to 4,000m).</param>
        /// <returns>A tuple containing the article title and formatted distance subtitle (or empty strings if none found).</returns>
        internal async Task<(string Title, string Subtitle)> GetNearestPoiSubtitleAsync(double latitude, double longitude, int radiusMeters = 4000)
        {
            var pois = await GetNearbyPoisAsync(latitude, longitude, radiusMeters, limit: 1);
            if (pois.Count == 0)
            {
                return (string.Empty, string.Empty);
            }

            WikiPoi nearest = pois[0];
            double distKm = nearest.DistanceMeters / 1000.0;

            // If within 500m, treat as directly at the landmark; otherwise show distance
            string subtitle = distKm < 0.5
                ? $"At {nearest.Title}"
                : $"Near {nearest.Title} ({distKm:F1} km / {nearest.DistanceNM:F1} NM away)";

            return (nearest.Title, subtitle);
        }
    }
}