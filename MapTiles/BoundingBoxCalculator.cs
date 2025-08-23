using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.Services;

namespace P3D_Scenario_Generator.MapTiles
{
    /// <summary>
    /// Provides methods for calculating, extending, and adjusting the bounding box of OpenStreetMap tiles.
    /// This class encapsulates the logic for determining the tile set that covers a given area,
    /// including handling edge conditions and meridian wrapping.
    /// </summary>
    public class BoundingBoxCalculator(Logger logger, IProgress<string> progressReporter)
    {
        private readonly Logger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IProgress<string> _progressReporter = progressReporter ?? throw new ArgumentNullException(nameof(progressReporter));

        /// <summary>
        /// Creates a bounding box of tiles needed at a given zoom level to include all tiles in a list.
        /// Each tile coordinate must be more than <c>Constants.BoundingBoxTrimMarginPixels</c> pixels
        /// from any edge of the bounding box.
        /// </summary>
        /// <param name="tiles">A list of OSM tile references and their associated coordinate.</param>
        /// <param name="zoom">The zoom level required for the bounding box.</param>
        /// <returns>A tuple containing a boolean indicating success and the resulting <see cref="BoundingBox"/>.</returns>
        public async Task<(bool success, BoundingBox boundingBox)> GetBoundingBoxAsync(List<Tile> tiles, int zoom)
        {
            try
            {
                // Validate input tiles.
                if (tiles is null || tiles.Count == 0)
                {
                    string message = "Input 'tiles' list is null or empty. Cannot compute bounding box.";
                    await _logger.ErrorAsync(message);
                    _progressReporter.Report($"ERROR: {message}");
                    return (false, null);
                }

                BoundingBox boundingBox = new()
                {
                    XAxis = [tiles[0].XIndex],
                    YAxis = [tiles[0].YIndex]
                };

                // Adjust boundingBox as needed to include remaining tiles.
                for (int tileNo = 1; tileNo < tiles.Count; tileNo++)
                {
                    await ExtendBoundingBoxAsync(tiles[tileNo], boundingBox, zoom);
                }

                // Add extra tiles if any tile coordinates are too close to a bounding box edge.
                await CheckBoundingBoxEdgesAsync(tiles, boundingBox, zoom);

                return (true, boundingBox);
            }
            catch (Exception ex)
            {
                // Log any unexpected exceptions during the bounding box calculation process.
                string errorMessage = $"An unexpected error occurred while calculating bounding box at zoom {zoom}.";
                await _logger.ErrorAsync(errorMessage, ex);
                _progressReporter.Report($"ERROR: {errorMessage}");
                return (false, null);
            }
        }

        #region Extension Methods

        /// <summary>
        /// Extends the bounding box if the new tile is not already included.
        /// </summary>
        /// <param name="newTile">The tile to be added to bounding box.</param>
        /// <param name="boundingBox">The bounding box to extend.</param>
        /// <param name="zoom">The zoom level of the bounding box.</param>
        public async Task ExtendBoundingBoxAsync(Tile newTile, BoundingBox boundingBox, int zoom)
        {
            ArgumentNullException.ThrowIfNull(newTile);

            ArgumentNullException.ThrowIfNull(boundingBox);

            // New tile is above BB, i.e., tileNo < boundingBox.YAxis[0].
            if (newTile.YIndex < boundingBox.YAxis[0])
            {
                await ExtendBoundingBoxNorthAsync(newTile, boundingBox);
            }
            // New tile is below BB, i.e., tileNo > boundingBox.YAxis[^1].
            else if (newTile.YIndex > boundingBox.YAxis[^1])
            {
                await ExtendBoundingBoxSouthAsync(newTile, boundingBox);
            }

            // New tile is right of BB, i.e., tileNo > boundingBox.XAxis[^1].
            if (newTile.XIndex > boundingBox.XAxis[^1])
            {
                await ExtendBoundingBoxEastAsync(newTile, boundingBox, zoom);
            }
            // New tile is left of BB, i.e., tileNo < boundingBox.XAxis[0].
            else if (newTile.XIndex < boundingBox.XAxis[0])
            {
                await ExtendBoundingBoxWestAsync(newTile, boundingBox, zoom);
            }
        }

        /// <summary>
        /// Extends the bounding box to the north.
        /// </summary>
        /// <param name="newTile">The tile to be added to bounding box.</param>
        /// <param name="boundingBox">The bounding box to extend.</param>
        private async Task ExtendBoundingBoxNorthAsync(Tile newTile, BoundingBox boundingBox)
        {
            try
            {
                for (int tileNo = boundingBox.YAxis[0] - 1; tileNo >= newTile.YIndex; tileNo--)
                {
                    boundingBox.YAxis.Insert(0, tileNo);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error extending bounding box north for tile {newTile.XIndex},{newTile.YIndex}.";
                await _logger.ErrorAsync(errorMessage, ex);
                _progressReporter.Report($"ERROR: {errorMessage}");
            }
        }

        /// <summary>
        /// Extends the bounding box to the south.
        /// </summary>
        /// <param name="newTile">The tile to be added to bounding box.</param>
        /// <param name="boundingBox">The bounding box to extend.</param>
        private async Task ExtendBoundingBoxSouthAsync(Tile newTile, BoundingBox boundingBox)
        {
            try
            {
                for (int tileNo = boundingBox.YAxis[^1] + 1; tileNo <= newTile.YIndex; tileNo++)
                {
                    boundingBox.YAxis.Add(tileNo);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error extending bounding box south for tile {newTile.XIndex},{newTile.YIndex}.";
                await _logger.ErrorAsync(errorMessage, ex);
                _progressReporter.Report($"ERROR: {errorMessage}");
            }
        }

        /// <summary>
        /// Extends the bounding box to the east, potentially crossing the anti-meridian.
        /// </summary>
        /// <param name="newTile">The tile to be added to bounding box.</param>
        /// <param name="boundingBox">The bounding box to extend.</param>
        /// <param name="zoom">The zoom level of the bounding box.</param>
        private async Task ExtendBoundingBoxEastAsync(Tile newTile, BoundingBox boundingBox, int zoom)
        {
            try
            {
                int distEast = newTile.XIndex - boundingBox.XAxis[^1];
                int distWest = boundingBox.XAxis[0] + (1 << zoom) - newTile.XIndex;
                if (distEast <= distWest)
                {
                    for (int tileNo = boundingBox.XAxis[^1] + 1; tileNo <= newTile.XIndex; tileNo++)
                    {
                        boundingBox.XAxis.Add(tileNo);
                    }
                }
                else
                {
                    // Handle meridian wrapping.
                    for (int tileNo = boundingBox.XAxis[0] - 1; tileNo >= 0; tileNo--)
                    {
                        boundingBox.XAxis.Insert(0, tileNo);
                    }
                    for (int tileNo = (1 << zoom) - 1; tileNo >= newTile.XIndex; tileNo--)
                    {
                        boundingBox.XAxis.Insert(0, tileNo);
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error extending bounding box east for tile {newTile.XIndex},{newTile.YIndex}.";
                await _logger.ErrorAsync(errorMessage, ex);
                _progressReporter.Report($"ERROR: {errorMessage}");
            }
        }

        /// <summary>
        /// Extends the bounding box to the west, potentially crossing the anti-meridian.
        /// </summary>
        /// <param name="newTile">The tile to be added to bounding box.</param>
        /// <param name="boundingBox">The bounding box to extend.</param>
        /// <param name="zoom">The zoom level of the bounding box.</param>
        private async Task ExtendBoundingBoxWestAsync(Tile newTile, BoundingBox boundingBox, int zoom)
        {
            try
            {
                int distWest = boundingBox.XAxis[0] - newTile.XIndex;
                int distEast = (1 << zoom) - boundingBox.XAxis[0] + newTile.XIndex;
                if (distWest <= distEast)
                {
                    for (int tileNo = boundingBox.XAxis[0] - 1; tileNo >= newTile.XIndex; tileNo--)
                    {
                        boundingBox.XAxis.Insert(0, tileNo);
                    }
                }
                else
                {
                    // Handle meridian wrapping.
                    for (int tileNo = boundingBox.XAxis[^1] + 1; tileNo < 1 << zoom; tileNo++)
                    {
                        boundingBox.XAxis.Add(tileNo);
                    }
                    for (int tileNo = 0; tileNo <= newTile.XIndex; tileNo++)
                    {
                        boundingBox.XAxis.Add(tileNo);
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error extending bounding box west for tile {newTile.XIndex},{newTile.YIndex}.";
                await _logger.ErrorAsync(errorMessage, ex);
                _progressReporter.Report($"ERROR: {errorMessage}");
            }
        }

        #endregion

        #region Edge Checking Methods

        /// <summary>
        /// Checks the list of tiles and, for those on an edge of the bounding box, determines if their coordinate offset
        /// is too close to the bounding box edge. The bounding box is extended if necessary.
        /// </summary>
        /// <param name="tiles">A list of OSM tile references and their associated coordinate.</param>
        /// <param name="boundingBox">The bounding box to be checked and potentially populated.</param>
        /// <param name="zoom">The zoom level required for the bounding box.</param>
        public async Task CheckBoundingBoxEdgesAsync(List<Tile> tiles, BoundingBox boundingBox, int zoom)
        {
            ArgumentNullException.ThrowIfNull(tiles);

            ArgumentNullException.ThrowIfNull(boundingBox);

            try
            {
                // This method iterates over all tiles and checks their position relative to the
                // bounding box edges. It then calls the relevant async method to extend the bounding box
                // if a tile is too close to an edge.
                foreach (var tile in tiles)
                {
                    // Check North edge of bounding box
                    if (tile.YIndex == boundingBox.YAxis[0])
                    {
                        await CheckBBedgesNorthAsync(tile, boundingBox);
                    }

                    // Check East edge of bounding box
                    if (tile.XIndex == boundingBox.XAxis[^1])
                    {
                        await CheckBBedgesEastAsync(tile, boundingBox, zoom);
                    }

                    // Check South edge of bounding box
                    if (tile.YIndex == boundingBox.YAxis[^1])
                    {
                        await CheckBBedgesSouthAsync(tile, boundingBox, zoom);
                    }

                    // Check West edge of bounding box
                    if (tile.XIndex == boundingBox.XAxis[0])
                    {
                        await CheckBBedgesWestAsync(tile, boundingBox, zoom);
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = "An unexpected error occurred while checking bounding box edges.";
                await _logger.ErrorAsync(errorMessage, ex);
                _progressReporter.Report($"ERROR: {errorMessage}");
            }
        }

        /// <summary>
        /// Extends the bounding box to the north by one tile if the coordinate offset is too close
        /// to the north edge and the bounding box is not already at the top of the world map.
        /// </summary>
        /// <param name="tileToCheck">The tile to be checked.</param>
        /// <param name="boundingBox">The bounding box to extend.</param>
        private async Task CheckBBedgesNorthAsync(Tile tileToCheck, BoundingBox boundingBox)
        {
            try
            {
                if (tileToCheck.YOffset < Constants.BoundingBoxTrimMarginPixels && tileToCheck.YIndex > 0)
                {
                    string message = $"Extending bounding box north from {boundingBox.YAxis[0]} due to tile at ({tileToCheck.XIndex},{tileToCheck.YIndex}).";
                    _progressReporter.Report($"INFO: {message}");
                    await _logger.InfoAsync(message);
                    boundingBox.YAxis.Insert(0, tileToCheck.YIndex - 1);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error checking north edge for tile {tileToCheck.XIndex},{tileToCheck.YIndex}.";
                await _logger.ErrorAsync(errorMessage, ex);
                _progressReporter.Report($"ERROR: {errorMessage}");
            }
        }

        /// <summary>
        /// Extends the bounding box to the east by one tile if the coordinate offset is too close
        /// to the east edge. This could involve crossing the anti-meridian line.
        /// </summary>
        /// <param name="tileToCheck">The tile to be checked.</param>
        /// <param name="boundingBox">The bounding box to extend.</param>
        /// <param name="zoom">The zoom level required for the bounding box.</param>
        private async Task CheckBBedgesEastAsync(Tile tileToCheck, BoundingBox boundingBox, int zoom)
        {
            try
            {
                if (tileToCheck.XOffset > Constants.TileSizePixels - Constants.BoundingBoxTrimMarginPixels)
                {
                    int newTileNo = MapTileCalculator.IncXtileNo(tileToCheck.XIndex, zoom);
                    if (newTileNo != boundingBox.XAxis[0]) // Avoid adding the same tile number if it's already at the west edge
                    {
                        string message = $"Extending bounding box east to {newTileNo} due to tile at ({tileToCheck.XIndex},{tileToCheck.YIndex}).";
                        _progressReporter.Report($"INFO: {message}");
                        await _logger.InfoAsync(message);
                        boundingBox.XAxis.Add(newTileNo);
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error checking east edge for tile {tileToCheck.XIndex},{tileToCheck.YIndex}.";
                await _logger.ErrorAsync(errorMessage, ex);
                _progressReporter.Report($"ERROR: {errorMessage}");
            }
        }

        /// <summary>
        /// Extends the bounding box to the south by one tile if the coordinate offset is too close
        /// to the south edge and the bounding box is not already at the bottom of the world map.
        /// </summary>
        /// <param name="tileToCheck">The tile to be checked.</param>
        /// <param name="boundingBox">The bounding box to extend.</param>
        /// <param name="zoom">The zoom level required for the bounding box.</param>
        private async Task CheckBBedgesSouthAsync(Tile tileToCheck, BoundingBox boundingBox, int zoom)
        {
            try
            {
                if (tileToCheck.YOffset > Constants.TileSizePixels - Constants.BoundingBoxTrimMarginPixels && tileToCheck.YIndex < (1 << zoom) - 1)
                {
                    string message = $"Extending bounding box south to {tileToCheck.YIndex + 1} due to tile at ({tileToCheck.XIndex},{tileToCheck.YIndex}).";
                    _progressReporter.Report($"INFO: {message}");
                    await _logger.InfoAsync(message);
                    boundingBox.YAxis.Add(tileToCheck.YIndex + 1);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error checking south edge for tile {tileToCheck.XIndex},{tileToCheck.YIndex}.";
                await _logger.ErrorAsync(errorMessage, ex);
                _progressReporter.Report($"ERROR: {errorMessage}");
            }
        }

        /// <summary>
        /// Extends the bounding box to the west by one tile if the coordinate offset is too close
        /// to the west edge. This could involve crossing the anti-meridian line.
        /// </summary>
        /// <param name="tileToCheck">The tile to be checked.</param>
        /// <param name="boundingBox">The bounding box to extend.</param>
        /// <param name="zoom">The zoom level required for the bounding box.</param>
        private async Task CheckBBedgesWestAsync(Tile tileToCheck, BoundingBox boundingBox, int zoom)
        {
            try
            {
                if (tileToCheck.XOffset < Constants.BoundingBoxTrimMarginPixels)
                {
                    int newTileNo = MapTileCalculator.DecXtileNo(tileToCheck.XIndex, zoom);
                    if (newTileNo != boundingBox.XAxis[^1])
                    {
                        string message = $"Extending bounding box west to {newTileNo} due to coordinate on tile " +
                            $"at ({tileToCheck.XIndex},{tileToCheck.YIndex}), zoom {zoom}.";
                        _progressReporter.Report($"INFO: {message}");
                        await _logger.InfoAsync(message);
                        boundingBox.XAxis.Insert(0, newTileNo);
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error checking west edge for tile {tileToCheck.XIndex},{tileToCheck.YIndex}, zoom {zoom}.";
                await _logger.ErrorAsync(errorMessage, ex);
                _progressReporter.Report($"ERROR: {errorMessage}");
            }
        }

        #endregion
    }
}
