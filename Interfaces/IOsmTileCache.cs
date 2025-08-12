namespace P3D_Scenario_Generator.Interfaces
{
    public interface IOsmTileCache
    {
        /// <summary>
        /// Retrieves an OpenStreetMap (OSM) tile, either from a local cache or by downloading it,
        /// and saves it to a specified file path. It also manages a daily download count.
        /// </summary>
        /// <param name="key">A unique identifier for the tile, typically in the format "zoom-xTileNo-yTileNo.png",
        /// used to check for its existence in the local cache.</param>
        /// <param name="url">The URL from which the OSM tile should be downloaded if not found in the cache.
        /// This URL includes the cache server address, tile coordinates, and API key.</param>
        /// <param name="saveFile">The full path and filename where the retrieved (copied or downloaded)
        /// OSM tile should be saved. Example: `formData.ScenarioImageFolder\\filename`.</param>
        /// <returns>Returns <see langword="true"/> if the tile was successfully retrieved (copied from cache or downloaded) and saved;
        /// otherwise, <see langword="false"/> if an error occurred during file operations or download.</returns>
        Task<bool> GetOrCopyOSMtile(string key, string url, string saveFile);

        /// <summary>
        /// Determines if a specific cached OpenStreetMap (OSM) tile file, identified by a key, exists
        /// within the application's dedicated cache directory structure. This method also ensures
        /// that the necessary cache directories (application root and zoom-level subdirectory) are created if they do not exist.
        /// </summary>
        /// <param name="key">The unique identifier for the OSM tile. This key typically follows the format "zoom-xTileNo-yTileNo.png",
        /// where the 'zoom' component is used to create a dedicated subdirectory within the cache.</param>
        /// <param name="cachePath">A reference parameter. Upon return, this will contain the full,
        /// calculated file path where the tile *would be* or *is* cached, regardless of whether it exists.</param>
        /// <returns>Returns <see langword="true"/> if the tile file identified by the <paramref name="key"/> exists in the cache;
        /// otherwise, <see langword="false"/>.</returns>
        bool DoesKeyExist(string key, ref string cachePath);
    }
}
