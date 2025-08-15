using ImageMagick;
using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.Interfaces;

namespace P3D_Scenario_Generator.MapTiles
{
    /// <summary>
    /// Provides static methods for assembling OpenStreetMap (OSM) tiles into larger images.
    /// This class handles the process of combining individual tile images into vertical columns,
    /// horizontal rows, and ultimately a complete grid image, while also managing temporary files.
    /// </summary>
    public class MapTileMontager(ILogger logger, FormProgressReporter progressReporter, IFileOps fileOps, IHttpRoutines httpRoutines)
    {
        private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly FormProgressReporter _progressReporter = progressReporter ?? throw new ArgumentNullException(nameof(progressReporter));
        private readonly IFileOps _fileOps = fileOps ?? throw new ArgumentNullException(nameof(fileOps));
        private readonly IHttpRoutines _httpRoutines = httpRoutines ?? throw new ArgumentNullException(nameof(httpRoutines));
        private readonly MapTileDownloader _mapTileDownloader = new(fileOps, httpRoutines, progressReporter);

        /// <summary>
        /// Montages a column of individual tile images into a single vertical image.
        /// This method reads individual tile images from the specified folder, combines them,
        /// and writes the resulting montage to a new image file.
        /// </summary>
        /// <param name="yCount">The number of tiles in the column (height of the montage in tiles).</param>
        /// <param name="columnID">The X-index of the column, used for naming the input and output files.</param>
        /// <param name="fullPathNoExt">The base path and fullPathNoExt prefix for the input individual tiles and the output montaged column image.</param>
        /// <returns><see langword="true"/> if the tiles were successfully montaged into a column image; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> MontageTilesToColumnAsync(int yCount, int columnID, string fullPathNoExt)
        {
            try
            {
                // MagickImageCollection will hold the individual tile images to be montaged.
                using var images = new MagickImageCollection();

                // Setup for the montage operation.
                // Geometry defines the size of each individual image within the montage.
                // TileGeometry defines the layout of the montage (1 column by yCount rows).
                var settings = new MontageSettings
                {
                    Geometry = new MagickGeometry($"{Constants.TileSizePixels}x{Constants.TileSizePixels}"),
                    TileGeometry = new MagickGeometry($"1x{yCount}"),
                };

                // Load each individual tile image into the collection.
                for (int yIndex = 0; yIndex < yCount; yIndex++)
                {
                    string tilePath = $"{fullPathNoExt}_{columnID}_{yIndex}.png";
                    if (!_fileOps.FileExists(tilePath))
                    {
                        await _logger.ErrorAsync($"Required tile image not found: {tilePath}");
                        return false; // Fail if a source tile is missing
                    }
                    images.Add(new MagickImage(tilePath));
                }

                // Perform the montage operation. The result is a single MagickImage.
                using var result = images.Montage(settings);

                // Write the resulting montaged image to the specified output file.
                string outputPath = $"{fullPathNoExt}_{columnID}.png";

                // Ensure destination directory exists before writing
                var directory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                result.Write(outputPath);

                // If execution reaches here, the operation was successful.
                return true;
            }
            catch (MagickErrorException mex)
            {
                // Catch specific Magick.NET exceptions
                await _logger.ErrorAsync($"Magick.NET error during column montage for '{fullPathNoExt}_{columnID}': {mex.Message}", mex);
                return false;
            }
            catch (FileNotFoundException fex)
            {
                // Catch if a specific tile file was not found during loading
                await _logger.ErrorAsync($"File not found error during column montage: {fex.FileName}. Details: {fex.Message}", fex);
                return false;
            }
            catch (IOException ioex)
            {
                // Catch general I/O errors (e.g., file locked, disk full)
                await _logger.ErrorAsync($"I/O error during column montage for '{fullPathNoExt}_{columnID}': {ioex.Message}", ioex);
                return false;
            }
            catch (Exception ex)
            {
                // Catch any other unexpected exceptions
                await _logger.ErrorAsync($"An unexpected error occurred during column montage for '{fullPathNoExt}_{columnID}': {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Montages a row of individual tile images into a single horizontal image.
        /// This method reads individual tile images from the specified folder, combines them,
        /// and writes the resulting montage to a new image file.
        /// </summary>
        /// <param name="xCount">The number of tiles in the row (width of the montage in tiles).</param>
        /// <param name="rowId">The Y-index of the row, used for naming the input and output files.</param>
        /// <param name="fullPathNoExt">The base path and fullPathNoExt prefix for the input individual tiles and the output montaged row image.</param>
        /// <returns><see langword="true"/> if the tiles were successfully montaged into a row image; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> MontageTilesToRowAsync(int xCount, int rowId, string fullPathNoExt)
        {
            try
            {
                // MagickImageCollection will hold the individual tile images to be montaged.
                using var images = new MagickImageCollection();

                // Setup for the montage operation.
                // Geometry defines the size of each individual image within the montage.
                // TileGeometry defines the layout of the montage (xCount columns by 1 row).
                var settings = new MontageSettings
                {
                    Geometry = new MagickGeometry($"{Constants.TileSizePixels}x{Constants.TileSizePixels}"),
                    TileGeometry = new MagickGeometry($"{xCount}x1"),
                };

                // Load each individual tile image into the collection.
                for (int xIndex = 0; xIndex < xCount; xIndex++)
                {
                    string tilePath = $"{fullPathNoExt}_{xIndex}_{rowId}.png";
                    if (!_fileOps.FileExists(tilePath))
                    {
                        await _logger.ErrorAsync($"Required tile image not found: {tilePath}");
                        return false; // Fail if a source tile is missing
                    }
                    images.Add(new MagickImage(tilePath));
                }

                // Perform the montage operation. The result is a single MagickImage.
                using var result = images.Montage(settings);

                // Write the resulting montaged image to the specified output file.
                string outputPath = $"{fullPathNoExt}_{rowId}.png";

                // Ensure destination directory exists before writing
                var directory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                result.Write(outputPath);

                // If execution reaches here, the operation was successful.
                return true;
            }
            catch (MagickErrorException mex)
            {
                // Catch specific Magick.NET exceptions
                await _logger.ErrorAsync($"Magick.NET error during row montage for '{fullPathNoExt}_{rowId}': {mex.Message}", mex);
                return false;
            }
            catch (FileNotFoundException fex)
            {
                // Catch if a specific tile file was not found during loading
                await _logger.ErrorAsync($"File not found error during row montage: {fex.FileName}. Details: {fex.Message}", fex);
                return false;
            }
            catch (IOException ioex)
            {
                // Catch general I/O errors (e.g., file locked, disk full)
                await _logger.ErrorAsync($"I/O error during row montage for '{fullPathNoExt}_{rowId}': {ioex.Message}", ioex);
                return false;
            }
            catch (Exception ex)
            {
                // Catch any other unexpected exceptions
                await _logger.ErrorAsync($"An unexpected error occurred during row montage for '{fullPathNoExt}_{rowId}': {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Montages a row of individual column images into a single final image.
        /// This method reads pre-montaged column images from the specified folder, combines them
        /// horizontally, and writes the resulting complete image to a new file.
        /// </summary>
        /// <param name="xCount">The number of columns to montage (width of the montage in columns).</param>
        /// <param name="yCount">The height of each individual column image in tiles, used for geometry calculation.</param>
        /// <param name="fullPathNoExt">The base path and fullPathNoExt prefix for the input individual column images and the output final montaged image.</param>
        /// <returns><see langword="true"/> if the columns were successfully montaged into a single image; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> MontageColumnsAsync(int xCount, int yCount, string fullPathNoExt)
        {
            try
            {
                // MagickImageCollection will hold the individual column images to be montaged.
                using var images = new MagickImageCollection();

                // Setup for the montage operation.
                // Geometry defines the size of each individual column image (Con.TileSizePixels width, yCount * Con.TileSizePixels height).
                // TileGeometry defines the layout of the montage (xCount columns by 1 row).
                var settings = new MontageSettings
                {
                    Geometry = new MagickGeometry($"{Constants.TileSizePixels}x{Constants.TileSizePixels * yCount}"),
                    TileGeometry = new MagickGeometry($"{xCount}x1"),
                };

                // Load each individual column image into the collection.
                for (int xIndex = 0; xIndex < xCount; xIndex++)
                {
                    string columnPath = $"{fullPathNoExt}_{xIndex}.png";
                    if (!_fileOps.FileExists(columnPath))
                    {
                        await _logger.ErrorAsync($"Required column image not found: {columnPath}");
                        return false; // Fail if a source column image is missing
                    }
                    images.Add(new MagickImage(columnPath));
                }

                // Perform the montage operation. The result is a single MagickImage.
                using var result = images.Montage(settings);

                // Write the resulting montaged image to the specified output file.
                string outputPath = $"{fullPathNoExt}.png";

                // Ensure destination directory exists before writing (though likely covered by previous steps)
                var directory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                result.Write(outputPath);

                // If execution reaches here, the operation was successful.
                return true;
            }
            catch (MagickErrorException mex)
            {
                // Catch specific Magick.NET exceptions
                await _logger.ErrorAsync($"Magick.NET error during full image montage for '{fullPathNoExt}': {mex.Message}", mex);
                return false;
            }
            catch (FileNotFoundException fex)
            {
                // Catch if a specific column file was not found during loading
                await _logger.ErrorAsync($"File not found error during full image montage: {fex.FileName}. Details: {fex.Message}", fex);
                return false;
            }
            catch (IOException ioex)
            {
                // Catch general I/O errors (e.g., file locked, disk full)
                await _logger.ErrorAsync($"I/O error during full image montage for '{fullPathNoExt}': {ioex.Message}", ioex);
                return false;
            }
            catch (Exception ex)
            {
                // Catch any other unexpected exceptions
                await _logger.ErrorAsync($"An unexpected error occurred during full image montage for '{fullPathNoExt}': {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Montages a column of individual row images into a single final image.
        /// This method reads pre-montaged row images from the specified folder, combines them
        /// vertically, and writes the resulting complete image to a new file.
        /// </summary>
        /// <param name="xCount">The width of each individual row image in tiles, used for geometry calculation.</param>
        /// <param name="yCount">The number of rows to montage (height of the montage in rows).</param>
        /// <param name="fullPathNoExt">The base path and fullPathNoExt prefix for the input individual row images and the output final montaged image.</param>
        /// <returns><see langword="true"/> if the rows were successfully montaged into a single image; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> MontageRowsAsync(int xCount, int yCount, string fullPathNoExt)
        {
            try
            {
                // MagickImageCollection will hold the individual column images to be montaged.
                using var images = new MagickImageCollection();

                // Setup for the montage operation.
                // Geometry defines the size of each individual row image (xCount * Con.TileSizePixels width, Con.TileSizePixels height).
                // TileGeometry defines the layout of the montage (1 column by yCount rows).
                var settings = new MontageSettings
                {
                    Geometry = new MagickGeometry($"{Constants.TileSizePixels * xCount}x{Constants.TileSizePixels}"),
                    TileGeometry = new MagickGeometry($"1x{yCount}"),
                };

                // Load each individual row image into the collection.
                for (int yIndex = 0; yIndex < yCount; yIndex++)
                {
                    string rowPath = $"{fullPathNoExt}_{yIndex}.png";
                    if (!_fileOps.FileExists(rowPath))
                    {
                        await _logger.ErrorAsync($"Required row image not found: {rowPath}");
                        return false; // Fail if a source row image is missing
                    }
                    images.Add(new MagickImage(rowPath));
                }

                // Perform the montage operation. The result is a single MagickImage.
                using var result = images.Montage(settings);

                // Write the resulting montaged image to the specified output file.
                string outputPath = $"{fullPathNoExt}.png";

                // Ensure destination directory exists before writing 
                var directory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                result.Write(outputPath);

                // If execution reaches here, the operation was successful.
                return true;
            }
            catch (MagickErrorException mex)
            {
                // Catch specific Magick.NET exceptions
                await _logger.ErrorAsync($"Magick.NET error during full image montage for '{fullPathNoExt}': {mex.Message}", mex);
                return false;
            }
            catch (FileNotFoundException fex)
            {
                // Catch if a specific row file was not found during loading
                await _logger.ErrorAsync($"File not found error during full image montage: {fex.FileName}. Details: {fex.Message}", fex);
                return false;
            }
            catch (IOException ioex)
            {
                // Catch general I/O errors (e.g., file locked, disk full)
                await _logger.ErrorAsync($"I/O error during full image montage for '{fullPathNoExt}': {ioex.Message}", ioex);
                return false;
            }
            catch (Exception ex)
            {
                // Catch any other unexpected exceptions
                await _logger.ErrorAsync($"An unexpected error occurred during full image montage for '{fullPathNoExt}': {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Orchestrates the entire process of downloading a grid of OpenStreetMap (OSM) tiles,
        /// montaging them into columns, then montaging those columns into a single complete image,
        /// and finally cleaning up temporary files. This is the main entry point for generating
        /// a complete map image from a bounding box.
        /// </summary>
        /// <param name="boundingBox">Defines the rectangular area (set of X and Y tile coordinates)
        /// for which OSM tiles need to be processed.</param>
        /// <param name="zoom">The specific zoom level for all tiles in the montage.</param>
        /// <param name="fullPathNoExt">The base path and fullPathNoExt prefix used for all intermediate and final image files.</param>
        /// <returns><see langword="true"/> if the entire montage process (downloading, montaging, and cleanup)
        /// completes successfully; otherwise, <see langword="false"/> if any step fails (errors are logged by
        /// underlying methods).</returns>
        public async Task<bool> MontageTilesAsync(BoundingBox boundingBox, int zoom, string fullPathNoExt, ScenarioFormData formData)
        {
            // Step 1: Download individual tiles column by column and montage them into vertical strips.
            // This loop processes each column (columnID) within the specified bounding box.
            for (int xIndex = 0; xIndex < boundingBox.XAxis.Count; xIndex++)
            {
                // Download all individual tiles for the current column.
                // If the download of any tile in the column fails, the entire process fails.
                if (!await _mapTileDownloader.DownloadOSMtileColumnAsync(boundingBox.XAxis[xIndex], xIndex, boundingBox, zoom, fullPathNoExt, formData))
                {
                    await _logger.ErrorAsync($"Failed to download OSM tile column for xIndex {xIndex}.");
                    return false; // Propagate failure from column download.
                }

                // Montage the individual tiles (downloaded in the previous step) into a single vertical column image.
                // If the montage of any column fails, the entire process fails.
                if (!await MontageTilesToColumnAsync(boundingBox.YAxis.Count, xIndex, fullPathNoExt))
                {
                    await _logger.ErrorAsync($"Failed to montage tiles to column for xIndex {xIndex}.");
                    return false; // Propagate failure from column montage.
                }
            }

            // Step 2: Montage the generated column strips horizontally to form the final complete image.
            // This combines all the vertical strips into one large map image.
            if (!await MontageColumnsAsync(boundingBox.XAxis.Count, boundingBox.YAxis.Count, fullPathNoExt))
            {
                await _logger.ErrorAsync($"Failed to montage columns into final image.");
                return false; // Propagate failure from final montage.
            }

            // Step 3: Delete all temporary individual tile and column strip files.
            // Although cleanup is typically a post-process, if it fails, it's still considered
            // a failure of the overall operation to ensure a clean state (or at least report an issue).
            if (!await _fileOps.TryDeleteTempOSMfilesAsync(fullPathNoExt, null)) 
            {
                await _logger.ErrorAsync($"Failed to delete temporary OSM files.");
                return false; // Propagate failure from temporary file deletion.
            }

            // If all steps completed without returning false, the entire process was successful.
            return true;
        }
    }
}
