namespace P3D_Scenario_Generator
{
    /// <summary>
    /// Provides methods for calculating, extending, and adjusting the bounding box of OpenStreetMap tiles.
    /// This class encapsulates the logic for determining the tile set that covers a given area,
    /// including handling edge conditions and meridian wrapping.
    /// </summary>
    public static class BoundingBoxCalculator
    {
        /// <summary>
        /// Creates bounding box of tiles needed at given zoom level to include all tiles in list. Each tile
        /// coordinate must be more than boundingBoxTrimMargin pixels from any edge of the bounding box.
        /// </summary>
        /// <param name="tiles">A list of OSM tile references and their associated coordinate</param>
        /// <param name="zoom">The zoom level required for the bounding box</param>
        /// <param name="boundingBox">The output bounding box to be populated.</param>
        /// <returns>True if the bounding box was successfully created and populated, false otherwise.</returns>
        public static bool GetBoundingBox(List<Tile> tiles, int zoom, out BoundingBox boundingBox)
        {
            boundingBox = null; // Initialize out parameter to null as per out parameter rules

            try
            {
                // Input validation
                if (tiles == null || tiles.Count == 0)
                {
                    Log.Error("BoundingBoxCalculator.GetBoundingBox: Input 'tiles' list is null or empty. Cannot compute bounding box.");
                    return false;
                }

                boundingBox = new BoundingBox();
                boundingBox.XAxis = [tiles[0].XIndex];
                boundingBox.YAxis = [tiles[0].YIndex];

                // Adjust boundingBox as needed to include remaining tiles
                for (int tileNo = 1; tileNo < tiles.Count; tileNo++)
                {
                    ExtendBoundingBox(tiles[tileNo], boundingBox, zoom);
                }

                // Add extra tiles if any tile coordinates are too close to bounding box edge
                CheckBoundingBoxEdges(tiles, boundingBox, zoom);

                return true;
            }
            catch (Exception ex)
            {
                // Log any unexpected exceptions during the bounding box calculation process.
                Log.Error($"BoundingBoxCalculator.GetBoundingBox: An unexpected error occurred while calculating bounding box at zoom {zoom}. Exception: {ex.Message}", ex);
                boundingBox = null; // Explicitly set to null on failure
                return false;
            }
        }

        #region Adding tiles to bounding box region

        /// <summary>
        /// Extends bounding box if newTile is not already included.
        /// </summary>
        /// <param name="newTile">The tile to be added to bounding box</param>
        /// <param name="boundingBox">The bounding box is two lists of tile numbers, one for x axis the other y axis</param>
        /// <param name="zoom">The zoom level of the bounding box</param>
        internal static void ExtendBoundingBox(Tile newTile, BoundingBox boundingBox, int zoom)
        {
            // The calls below are internal and assume that boundingBox is not null
            // and the lists within are initialized, as checked by GetBoundingBox.

            // New tile is above BB i.e. tileNo < boundingBox[yAxis][0]
            if (newTile.YIndex < boundingBox.YAxis[0])
            {
                ExtendBoundingBoxNorth(newTile, boundingBox);
            }

            // New tile is below BB i.e. tileNo > boundingBox[yAxis][^1]
            if (newTile.YIndex > boundingBox.YAxis[^1])
            {
                ExtendBoundingBoxSouth(newTile, boundingBox);
            }

            // New tile is right of BB i.e. tileNo > boundingBox[xAxis][^1], determine whether to move righthand
            // side of bounding box further to the right (usual case) or lefthand side further to the left (across meridian)
            if (newTile.XIndex > boundingBox.XAxis[^1])
            {
                ExtendBoundingBoxEast(newTile, boundingBox, zoom);
            }

            // New tile is left of BB i.e. tileNo < boundingBox[xAxis][0], determine whether to move lefthand
            // side of bounding box further to the left (usual case) or righthand side further to the right (across meridian)
            if (newTile.XIndex < boundingBox.XAxis[0])
            {
                ExtendBoundingBoxWest(newTile, boundingBox, zoom);
            }
        }

        /// <summary>
        /// New tile is above BB i.e. tileNo less than boundingBox[yAxis][0]
        /// </summary>
        /// <param name="newTile">The tile to be added to bounding box</param>
        /// <param name="boundingBox">The bounding box is two lists of tile numbers, one for x axis the other y axis</param>
        internal static void ExtendBoundingBoxNorth(Tile newTile, BoundingBox boundingBox)
        {
            try
            {
                for (int tileNo = boundingBox.YAxis[0] - 1; tileNo >= newTile.YIndex; tileNo--)
                {
                    if (!boundingBox.YAxis.Contains(tileNo)) // Prevent adding duplicates
                    {
                        boundingBox.YAxis.Insert(0, tileNo);
                    }
                }
                // Sort after additions to maintain order, crucial if later logic depends on sorted list.
                // Could be optimized by only sorting once at the end of ExtendBoundingBox if many inserts happen.
                boundingBox.YAxis.Sort();
            }
            catch (Exception ex)
            {
                Log.Error($"ExtendBoundingBoxNorth: Error extending bounding box north for tile {newTile.XIndex},{newTile.YIndex}. Exception: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// New tile is below BB i.e. tileNo > boundingBox[yAxis][^1]
        /// </summary>
        /// <param name="newTile">The tile to be added to bounding box</param>
        /// <param name="boundingBox">The bounding box is two lists of tile numbers, one for x axis the other y axis</param>
        internal static void ExtendBoundingBoxSouth(Tile newTile, BoundingBox boundingBox)
        {
            try
            {
                for (int tileNo = boundingBox.YAxis[^1] + 1; tileNo <= newTile.YIndex; tileNo++)
                {
                    if (!boundingBox.YAxis.Contains(tileNo)) // Prevent adding duplicates
                    {
                        boundingBox.YAxis.Add(tileNo);
                    }
                }
                boundingBox.YAxis.Sort();
            }
            catch (Exception ex)
            {
                Log.Error($"ExtendBoundingBoxSouth: Error extending bounding box south for tile {newTile.XIndex},{newTile.YIndex}. Exception: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// New tile is right of BB i.e. tileNo > boundingBox[xAxis][^1], determine whether to move righthand
        /// side of bounding box further to the right (usual case) or lefthand side further to the left (across meridian)
        /// </summary>
        /// <param name="newTile">The tile to be added to bounding box</param>
        /// <param name="boundingBox">The bounding box is two lists of tile numbers, one for x axis the other y axis</param>
        /// <param name="zoom">The zoom level of the bounding box</param>
        internal static void ExtendBoundingBoxEast(Tile newTile, BoundingBox boundingBox, int zoom)
        {
            try
            {
                int distEast, distWest;

                distEast = newTile.XIndex - boundingBox.XAxis[^1];
                distWest = boundingBox.XAxis[0] + (1 << zoom) - newTile.XIndex;
                if (distEast <= distWest)
                {
                    for (int tileNo = boundingBox.XAxis[^1] + 1; tileNo <= newTile.XIndex; tileNo++)
                    {
                        if (!boundingBox.XAxis.Contains(tileNo)) // Prevent adding duplicates
                        {
                            boundingBox.XAxis.Add(tileNo);
                        }
                    }
                }
                else
                {
                    for (int tileNo = boundingBox.XAxis[0] - 1; tileNo >= 0; tileNo--)
                    {
                        if (!boundingBox.XAxis.Contains(tileNo)) // Prevent adding duplicates
                        {
                            boundingBox.XAxis.Insert(0, tileNo);
                        }
                    }
                    for (int tileNo = (1 << zoom) - 1; tileNo >= newTile.XIndex; tileNo--)
                    {
                        if (!boundingBox.XAxis.Contains(tileNo)) // Prevent adding duplicates
                        {
                            boundingBox.XAxis.Insert(0, tileNo);
                        }
                    }
                }
                boundingBox.XAxis.Sort();
            }
            catch (Exception ex)
            {
                Log.Error($"ExtendBoundingBoxEast: Error extending bounding box east for tile {newTile.XIndex},{newTile.YIndex}. Exception: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// New tile is left of BB i.e. tileNo less than boundingBox[xAxis][0], determine whether to move lefthand
        /// side of bounding box further to the left (usual case) or righthand side further to the right (across meridian)
        /// </summary>
        /// <param name="newTile">The tile to be added to bounding box</param>
        /// <param name="boundingBox">The bounding box is two lists of tile numbers, one for x axis the other y axis</param>
        /// <param name="zoom">The zoom level of the bounding box</param>
        internal static void ExtendBoundingBoxWest(Tile newTile, BoundingBox boundingBox, int zoom)
        {
            try
            {
                int distEast, distWest;

                distWest = boundingBox.XAxis[0] - newTile.XIndex;
                distEast = (1 << zoom) - boundingBox.XAxis[0] + newTile.XIndex;
                if (distWest <= distEast)
                {
                    for (int tileNo = boundingBox.XAxis[0] - 1; tileNo >= newTile.XIndex; tileNo--)
                    {
                        if (!boundingBox.XAxis.Contains(tileNo)) // Prevent adding duplicates
                        {
                            boundingBox.XAxis.Insert(0, tileNo);
                        }
                    }
                }
                else
                {
                    for (int tileNo = boundingBox.XAxis[^1] + 1; tileNo < (1 << zoom); tileNo++)
                    {
                        if (!boundingBox.XAxis.Contains(tileNo)) // Prevent adding duplicates
                        {
                            boundingBox.XAxis.Add(tileNo);
                        }
                    }
                    for (int tileNo = 0; tileNo <= newTile.XIndex; tileNo++)
                    {
                        if (!boundingBox.XAxis.Contains(tileNo)) // Prevent adding duplicates
                        {
                            boundingBox.XAxis.Add(tileNo);
                        }
                    }
                }
                boundingBox.XAxis.Sort();
            }
            catch (Exception ex)
            {
                Log.Error($"ExtendBoundingBoxWest: Error extending bounding box west for tile {newTile.XIndex},{newTile.YIndex}. Exception: {ex.Message}", ex);
            }
        }

        #endregion

        #region Checking bounding box edges region

        /// <summary>
        /// Go through list of tiles and for those tiles that are on an edge of the bounding box check that the offset values
        /// of tile coordinate are not too close to the bounding box edge. Extend bounding box if necessary.
        /// </summary>
        /// <param name="tiles">A list of OSM tile references and their associated coordinate</param>
        /// <param name="boundingBox">The bounding box to be populated</param>
        /// <param name="zoom">The zoom level required for the bounding box</param>
        internal static void CheckBoundingBoxEdges(List<Tile> tiles, BoundingBox boundingBox, int zoom)
        {
            try
            {
                for (int tileNo = 0; tileNo < tiles.Count; tileNo++)
                {
                    // Check North edge of bounding box
                    if (tiles[tileNo].YIndex == boundingBox.YAxis[0])
                    {
                        CheckBBedgesNorth(tiles, boundingBox, tileNo);
                    }

                    // Check East edge of bounding box
                    if (tiles[tileNo].XIndex == boundingBox.XAxis[^1])
                    {
                        CheckBBedgesEast(tiles, boundingBox, zoom, tileNo);
                    }

                    // Check South edge of bounding box
                    if (tiles[tileNo].YIndex == boundingBox.YAxis[^1])
                    {
                        CheckBBedgesSouth(tiles, boundingBox, zoom, tileNo);
                    }

                    // Check West edge of bounding box
                    if (tiles[tileNo].XIndex == boundingBox.XAxis[0])
                    {
                        CheckBBedgesWest(tiles, boundingBox, zoom, tileNo);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"CheckBoundingBoxEdges: An unexpected error occurred while checking bounding box edges. Exception: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// If the bounding box is not already at top of world map then extend it north by one if coordinate offset is
        /// too close to north edge of bounding box.
        /// </summary>
        /// <param name="tiles">A list of OSM tile references and their associated coordinate</param>
        /// <param name="boundingBox">The bounding box to be populated</param>
        /// <param name="tileIndexToCheck">The index of the tile in the 'tiles' list to be checked.</param>
        internal static void CheckBBedgesNorth(List<Tile> tiles, BoundingBox boundingBox, int tileIndexToCheck)
        {
            try
            {
                if ((tiles[tileIndexToCheck].YOffset < Constants.boundingBoxTrimMargin) && (tiles[tileIndexToCheck].YIndex > 0))
                {
                    if (!boundingBox.YAxis.Contains(tiles[tileIndexToCheck].YIndex - 1))
                    {
                        boundingBox.YAxis.Insert(0, tiles[tileIndexToCheck].YIndex - 1);
                        boundingBox.YAxis.Sort();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"CheckBBedgesNorth: Error checking north edge for tile index {tileIndexToCheck}. Exception: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Extend bounding box east if coordinate offset is too close to the east edge. This could involve
        /// crossing the meridian line.
        /// </summary>
        /// <param name="tiles">A list of OSM tile references and their associated coordinate</param>
        /// <param name="boundingBox">The bounding box to be populated</param>
        /// <param name="zoom">The zoom level required for the bounding box</param>
        /// <param name="tileIndexToCheck">The index of the tile in the 'tiles' list to be checked</param>
        internal static void CheckBBedgesEast(List<Tile> tiles, BoundingBox boundingBox, int zoom, int tileIndexToCheck)
        {
            try
            {
                int newTileNo;

                if (tiles[tileIndexToCheck].XOffset > Constants.tileSize - Constants.boundingBoxTrimMargin)
                {
                    newTileNo = MapTileCalculator.IncXtileNo(tiles[tileIndexToCheck].XIndex, zoom);
                    if (!boundingBox.XAxis.Contains(newTileNo)) // Removed `&& newTileNo != boundingBox.XAxis[0]` as Contains implies this
                    {
                        boundingBox.XAxis.Add(newTileNo);
                        boundingBox.XAxis.Sort();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"CheckBBedgesEast: Error checking east edge for tile index {tileIndexToCheck}. Exception: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// If the bounding box is not already at bottom of world map then extend it south by one if coordinate offset is
        /// too close to south edge of bounding box.
        /// </summary>
        /// <param name="tiles">A list of OSM tile references and their associated coordinate</param>
        /// <param name="boundingBox">The bounding box to be populated</param>
        /// <param name="zoom">The zoom level required for the bounding box</param>
        /// <param name="tileIndexToCheck">The index of the tile in the 'tiles' list to be checked</param>
        internal static void CheckBBedgesSouth(List<Tile> tiles, BoundingBox boundingBox, int zoom, int tileIndexToCheck)
        {
            try
            {
                if ((tiles[tileIndexToCheck].YOffset > Constants.tileSize - Constants.boundingBoxTrimMargin) && (tiles[tileIndexToCheck].YIndex < (1 << zoom) - 1))
                {
                    if (!boundingBox.YAxis.Contains(tiles[tileIndexToCheck].YIndex + 1))
                    {
                        boundingBox.YAxis.Add(tiles[tileIndexToCheck].YIndex + 1);
                        boundingBox.YAxis.Sort();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"CheckBBedgesSouth: Error checking south edge for tile index {tileIndexToCheck}. Exception: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Extend bounding box west if coordinate offset is too close to the west edge. This could involve
        /// crossing the meridian line.
        /// </summary>
        /// <param name="tiles">A list of OSM tile references and their associated coordinate</param>
        /// <param name="boundingBox">The bounding box to be populated</param>
        /// <param name="zoom">The zoom level required for the bounding box</param>
        /// <param name="tileIndexToCheck">The index of the tile in the 'tiles' list to be checked</param>
        internal static void CheckBBedgesWest(List<Tile> tiles, BoundingBox boundingBox, int zoom, int tileIndexToCheck)
        {
            try
            {
                int newTileNo;

                if (tiles[tileIndexToCheck].XOffset < Constants.boundingBoxTrimMargin)
                {
                    newTileNo = MapTileCalculator.DecXtileNo(tiles[tileIndexToCheck].XIndex, zoom);
                    if (!boundingBox.XAxis.Contains(newTileNo)) // Removed `&& newTileNo != boundingBox.XAxis[^1]` as Contains implies this
                    {
                        boundingBox.XAxis.Insert(0, newTileNo);
                        boundingBox.XAxis.Sort();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"CheckBBedgesWest: Error checking west edge for tile index {tileIndexToCheck}. Exception: {ex.Message}", ex);
            }
        }

        #endregion
    }
}
