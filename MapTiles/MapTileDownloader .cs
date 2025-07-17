using P3D_Scenario_Generator.ConstantsEnums;

namespace P3D_Scenario_Generator.MapTiles
{
    /// <summary>
    /// Provides static methods for downloading and managing OpenStreetMap (OSM) tiles.
    /// It handles fetching individual tiles and collections of tiles (rows or columns)
    /// from a configured server, leveraging a local cache to optimize retrieval.
    /// </summary>
    /// <remarks>
    /// This class acts as the primary interface for acquiring OSM image data.
    /// It abstracts away the details of URL construction, API key integration,
    /// and cache management. All tile requests are first directed to a local cache;
    /// if a tile is not found there, it is downloaded and then stored in the cache
    /// for future use.
    /// </remarks>
    internal class MapTileDownloader
    {

        /// <summary>
        /// Orchestrates the retrieval of a single OpenStreetMap (OSM) tile.
        /// It attempts to fetch the tile from a local cache first; if not found, it downloads the tile
        /// from the configured server using the user provided API key. The retrieved tile is then stored
        /// at the specified local path.
        /// </summary>
        /// <param name="xTileNo">The X (East/West) coordinate of the required tile at the specified zoom level, corresponding to the OSM tiling scheme.</param>
        /// <param name="yTileNo">The Y (North/South) coordinate of the required tile at the specified zoom level, corresponding to the OSM tiling scheme.</param>
        /// <param name="zoom">The specific zoom level for which the OSM tile is required.</param>
        /// <param name="fullPath">The full local path and filename including extension where the OSM tile will be stored after retrieval.</param>
        /// <returns><see langword="true"/> if the OSM tile was successfully retrieved (either from cache or by download) and saved;
        /// otherwise, <see langword="false"/> if any error occurred during the process (errors are logged by underlying methods).</returns>
        static internal bool DownloadOSMtile(int xTileNo, int yTileNo, int zoom, string fullPath, ScenarioFormData formData)
        {
            // Construct the full URL for the OSM tile based on configured server URL, tile coordinates, zoom, and API key.
            string url = $"{Constants.OSMtileServerURLprefix}/{zoom}/{xTileNo}/{yTileNo}.png?rapidapi-key={formData.CacheServerAPIkey}";

            // Delegate the actual retrieval (from cache or download) and saving to the Cache class.
            // The key is constructed using zoom, xTileNo, and yTileNo for cache lookup.
            // Errors are handled and logged by the Cache.GetOrCopyOSMtile method and its dependencies.
            return Cache.GetOrCopyOSMtile($"{zoom}-{xTileNo}-{yTileNo}.png", url, fullPath);
        }

        /// <summary>
        /// Downloads a column of OpenStreetMap (OSM) tiles using server and API key specified by user settings.
        /// It first checks whether the OSM tiles are in the tile cache. If not, they are downloaded.
        /// All tiles are stored at the specified path given by filename.
        /// </summary>
        /// <param name="xTileNo">East/West OSM reference number for the required tile column at specified zoom.</param>
        /// <param name="columnId">Used as part of the filename to uniquely identify the tile column.</param>
        /// <param name="boundingBox">The bounding box containing the y-axis tile numbers, used to determine the height of the column of tiles to be downloaded.</param>
        /// <param name="zoom">Required zoom level for the OSM tiles to be downloaded.</param>
        /// <param name="fullPathNoExt">Base path and filename where the individual OSM tiles will be stored.
        /// Each tile's filename will be suffixed with its columnId and rowId (e.g., "basefilename_xIndex_yIndex.png").</param>
        /// <returns><see langword="true"/> if all tiles in the column were successfully downloaded or retrieved from cache;
        /// otherwise, <see langword="false"/> if any tile operation failed.</returns>
        static internal bool DownloadOSMtileColumn(int xTileNo, int columnId, BoundingBox boundingBox, int zoom, string fullPathNoExt, ScenarioFormData formData)
        {
            // Iterate through each y-axis tile number in the bounding box
            for (int yIndex = 0; yIndex < boundingBox.YAxis.Count; yIndex++)
            {
                // Construct the unique filename for the current tile
                string tileFilename = $"{fullPathNoExt}_{columnId}_{yIndex}.png";

                // Attempt to download or copy the individual OSM tile.
                // If DownloadOSMtile returns false (indicating a failure),
                // we immediately return false for the entire column download.
                if (!DownloadOSMtile(xTileNo, boundingBox.YAxis[yIndex], zoom, tileFilename, formData))
                {
                    return false; // An individual tile failed to download/copy, so the column download fails.
                }
            }

            // If the loop completes, all individual tiles were successfully downloaded or copied.
            return true;
        }

        /// <summary>
        /// Downloads a row of OpenStreetMap (OSM) tiles using server and API key specified by user settings.
        /// It first checks whether the OSM tiles are in the tile cache. If not, they are downloaded.
        /// All tiles are stored at the specified path given by filename.
        /// </summary>
        /// <param name="yTileNo">North/South reference number for the required tile row at specified zoom.</param>
        /// <param name="rowId">Used as part of the filename to uniquely identify the tile row.</param>
        /// <param name="boundingBox">The bounding box containing the x-axis tile numbers, used to determine the width of the row of tiles to be downloaded.</param>
        /// <param name="zoom">Required zoom level for the OSM tiles to be downloaded.</param>
        /// <param name="fullPathNoExt">Base path and filename where the individual OSM tiles will be stored.
        /// Each tile's filename will be suffixed with its columnId and rowId (e.g., "basefilename_xIndex_yIndex.png").</param>
        /// <returns><see langword="true"/> if all tiles in the row were successfully downloaded or retrieved from cache;
        /// otherwise, <see langword="false"/> if any tile operation failed.</returns>
        static internal bool DownloadOSMtileRow(int yTileNo, int rowId, BoundingBox boundingBox, int zoom, string fullPathNoExt, ScenarioFormData formData)
        {
            // Iterate through each x-axis tile number in the bounding box
            for (int xIndex = 0; xIndex < boundingBox.XAxis.Count; xIndex++)
            {
                // Construct the unique filename for the current tile
                string tileFilename = $"{fullPathNoExt}_{xIndex}_{rowId}.png";

                // Attempt to download or copy the individual OSM tile.
                // If DownloadOSMtile returns false (indicating a failure),
                // we immediately return false for the entire row download.
                if (!DownloadOSMtile(boundingBox.XAxis[xIndex], yTileNo, zoom, tileFilename, formData))
                {
                    return false; // An individual tile failed to download/copy, so the row download fails.
                }
            }

            // If the loop completes, all individual tiles were successfully downloaded or copied.
            return true;
        }
    }
}
