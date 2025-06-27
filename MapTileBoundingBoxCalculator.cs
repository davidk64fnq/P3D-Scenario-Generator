namespace P3D_Scenario_Generator
{
    /// <summary>
    /// Provides methods for calculating, extending, and adjusting the bounding box of OpenStreetMap tiles.
    /// This class encapsulates the logic for determining the tile set that covers a given area,
    /// including handling edge conditions and meridian wrapping.
    /// </summary>
    public static class MapTileBoundingBoxCalculator // New class, made static as all its methods are static
    {
        /// <summary>
        /// Creates bounding box of tiles needed at given zoom level to include all tiles in list. Each tile
        /// coordinate must be more than boundingBoxTrimMargin pixels from any edge of the bounding box.
        /// </summary>
        /// <param name="tiles">A list of OSM tile references and their associated coordinate</param>
        /// <param name="boundingBox">The bounding box to be populated</param>
        /// <param name="zoom">The zoom level required for the bounding box</param>
        public static BoundingBox GetBoundingBox(List<Tile> tiles, int zoom)
        {
            // Initialise boundingBox to the first tile
            BoundingBox boundingBox = new();
            // Ensure there's at least one tile to initialize with
            if (tiles == null || tiles.Count == 0)
            {
                // Handle this case as appropriate for your application,
                // perhaps by throwing an exception or returning an empty bounding box.
                // For now, let's assume `tiles` will always have at least one element
                // when this method is called from `GetOptimalZoomLevel`.
                throw new ArgumentException("The 'tiles' list cannot be null or empty when calculating a bounding box.");
            }

            List<int> xAxis = [tiles[0].XIndex];
            boundingBox.XAxis = xAxis;
            List<int> yAxis = [tiles[0].YIndex];
            boundingBox.YAxis = yAxis;

            // Adjust boundingBox as needed to include remaining tiles
            for (int tileNo = 1; tileNo < tiles.Count; tileNo++)
            {
                ExtendBoundingBox(tiles[tileNo], boundingBox, zoom);
            }

            // Add extra tiles if any tile coordinates are too close to bounding box edge
            CheckBoundingBoxEdges(tiles, boundingBox, zoom);

            return boundingBox;
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
            // Insert extra tile No's at beginning of yAxis list
            for (int tileNo = boundingBox.YAxis[0] - 1; tileNo >= newTile.YIndex; tileNo--)
            {
                boundingBox.YAxis.Insert(0, tileNo);
            }
        }

        /// <summary>
        /// New tile is below BB i.e. tileNo > boundingBox[yAxis][^1]
        /// </summary>
        /// <param name="newTile">The tile to be added to bounding box</param>
        /// <param name="boundingBox">The bounding box is two lists of tile numbers, one for x axis the other y axis</param>
        internal static void ExtendBoundingBoxSouth(Tile newTile, BoundingBox boundingBox)
        {
            // Append extra tileNo's at end of yAxis list
            for (int tileNo = boundingBox.YAxis[^1] + 1; tileNo <= newTile.YIndex; tileNo++)
            {
                boundingBox.YAxis.Add(tileNo);
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
            int distEast, distWest;

            distEast = newTile.XIndex - boundingBox.XAxis[^1];
            distWest = boundingBox.XAxis[0] + (1 << zoom) - newTile.XIndex; // Use bit shift for Math.Pow(2, zoom) for int operations
            if (distEast <= distWest)
            {
                // Append extra tileNo's at end of xAxis list
                for (int tileNo = boundingBox.XAxis[^1] + 1; tileNo <= newTile.XIndex; tileNo++)
                {
                    boundingBox.XAxis.Add(tileNo);
                }
            }
            else
            {
                // Insert extra tileNo's at beginning of xAxis list, handling wrap-around
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

        /// <summary>
        /// New tile is left of BB i.e. tileNo less than boundingBox[xAxis][0], determine whether to move lefthand
        /// side of bounding box further to the left (usual case) or righthand side further to the right (across meridian)
        /// </summary>
        /// <param name="newTile">The tile to be added to bounding box</param>
        /// <param name="boundingBox">The bounding box is two lists of tile numbers, one for x axis the other y axis</param>
        /// <param name="zoom">The zoom level of the bounding box</param>
        internal static void ExtendBoundingBoxWest(Tile newTile, BoundingBox boundingBox, int zoom)
        {
            int distEast, distWest;

            distWest = boundingBox.XAxis[0] - newTile.XIndex;
            distEast = (1 << zoom) - boundingBox.XAxis[0] + newTile.XIndex; // Use bit shift for Math.Pow(2, zoom) for int operations
            if (distWest <= distEast)
            {
                // Insert extra tileNo's at front of xAxis list
                for (int tileNo = boundingBox.XAxis[0] - 1; tileNo >= newTile.XIndex; tileNo--)
                {
                    boundingBox.XAxis.Insert(0, tileNo);
                }
            }
            else
            {
                // Append extra tileNo's at end of xAxis list, handling wrap-around
                for (int tileNo = boundingBox.XAxis[^1] + 1; tileNo < (1 << zoom); tileNo++)
                {
                    boundingBox.XAxis.Add(tileNo);
                }
                for (int tileNo = 0; tileNo <= newTile.XIndex; tileNo++)
                {
                    boundingBox.XAxis.Add(tileNo);
                }
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

        /// <summary>
        /// If the bounding box is not already at top of world map then extend it north by one if coordinate offset is
        /// too close to north edge of bounding box.
        /// </summary>
        /// <param name="tiles">A list of OSM tile references and their associated coordinate</param>
        /// <param name="boundingBox">The bounding box to be populated</param>
        /// <param name="zoom">The zoom level required for the bounding box</param>
        /// <param name="tileIndexToCheck">The index of the tile in the 'tiles' list to be checked.</param>
        internal static void CheckBBedgesNorth(List<Tile> tiles, BoundingBox boundingBox, int tileIndexToCheck)
        {
            // Using tileIndexToCheck to access the specific tile from the list
            if ((tiles[tileIndexToCheck].YOffset < Constants.boundingBoxTrimMargin) && (tiles[tileIndexToCheck].YIndex > 0))
            {
                // Only extend if the new tile index is not already present to avoid duplicates and infinite loops
                if (!boundingBox.YAxis.Contains(tiles[tileIndexToCheck].YIndex - 1))
                {
                    boundingBox.YAxis.Insert(0, tiles[tileIndexToCheck].YIndex - 1); // Only extend as far as yTile = 0
                    boundingBox.YAxis.Sort(); // Ensure the YAxis remains sorted
                }
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
            int newTileNo;

            // Using tileIndexToCheck to access the specific tile from the list
            if (tiles[tileIndexToCheck].XOffset > Constants.tileSize - Constants.boundingBoxTrimMargin)
            {
                newTileNo = MapTileCalculator.IncXtileNo(tiles[tileIndexToCheck].XIndex, zoom);
                // Only extend if the new tile index is not already present and not wrapping to the beginning of the x-axis list if it's already full range.
                if (!boundingBox.XAxis.Contains(newTileNo) && newTileNo != boundingBox.XAxis[0])
                {
                    boundingBox.XAxis.Add(newTileNo); // Only extend if not already using all tiles available in x axis
                    boundingBox.XAxis.Sort(); // Ensure the XAxis remains sorted
                }
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
            // Using tileIndexToCheck to access the specific tile from the list
            if ((tiles[tileIndexToCheck].YOffset > Constants.tileSize - Constants.boundingBoxTrimMargin) && (tiles[tileIndexToCheck].YIndex < (1 << zoom) - 1))
            {
                // Only extend if the new tile index is not already present
                if (!boundingBox.YAxis.Contains(tiles[tileIndexToCheck].YIndex + 1))
                {
                    boundingBox.YAxis.Add(tiles[tileIndexToCheck].YIndex + 1); // Only extend as far as yTile = Math.Pow(2, zoom) - 1
                    boundingBox.YAxis.Sort(); // Ensure the YAxis remains sorted
                }
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
            int newTileNo;

            // Using tileIndexToCheck to access the specific tile from the list
            if (tiles[tileIndexToCheck].XOffset < Constants.boundingBoxTrimMargin)
            {
                newTileNo = MapTileCalculator.DecXtileNo(tiles[tileIndexToCheck].XIndex, zoom);
                // Only extend if the new tile index is not already present and not wrapping to the end of the x-axis list if it's already full range.
                if (!boundingBox.XAxis.Contains(newTileNo) && newTileNo != boundingBox.XAxis[^1])
                {
                    boundingBox.XAxis.Insert(0, newTileNo); // Only extend if not already using all tiles available in x axis
                    boundingBox.XAxis.Sort(); // Ensure the XAxis remains sorted
                }
            }
        }

        #endregion
    }
}
