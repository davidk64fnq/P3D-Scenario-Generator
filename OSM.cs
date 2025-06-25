using CoordinateSharp;

namespace P3D_Scenario_Generator
{
    /// <summary> 
    /// The bounding box is two lists of tile numbers, one for x axis the other y axis.
    /// The tile numbers will usually be consecutive within the bounds 0 .. (2 to exp zoom - 1). However it's 
    /// possible for tiles grouped across the meridian to have a sequence of x axis tile numbers that goes up 
    /// to (2 to exp zoom - 1) and then continues from 0
    /// </summary>
    public class BoundingBox
    {
        public List<int> xAxis;        // List of OSM xTile references
        public List<int> yAxis;        // List of OSM yTile references
    }

    /// <summary>
    /// An OSM tile is defined by two numbers for a given zoom level. Origin of coordinate system
    /// is North-West corner of world map which is 0,0. Bottom right is (2 power zoom) - 1, (2 power zoom - 1)
    /// First value is horizontal index xIndex, second value is vertical index yIndex. xOffset and yOffset are
    /// for the coordinate of interest within the tile.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="Tile"/> class.
    /// </remarks>
    /// <param name="xIndex">The horizontal index (X-coordinate) of the OSM tile.</param>
    /// <param name="yIndex">The vertical index (Y-coordinate) of the OSM tile.</param>
    /// <param name="xOffset">The X-offset of the coordinate within the tile (in pixels).</param>
    /// <param name="yOffset">The Y-offset of the coordinate within the tile (in pixels).</param>
    public class Tile(int xIndex, int yIndex, int xOffset, int yOffset)
    {
        public int XIndex = xIndex;
        public int YIndex = yIndex;
        public int XOffset = xOffset;
        public int YOffset = yOffset;
    }

    /// <summary>
    /// Stores latitude and longitude boundaries of OSM image depicting coordinates, used by HTML Javascript moving map code
    /// </summary>
    public class MapEdges
    {
        public CoordinatePart north;
        public CoordinatePart east;
        public CoordinatePart south;
        public CoordinatePart west;
    }

    /// <summary>
    /// Provides methods for working with OSM map tiles, working out what square array of tiles are needed
    /// to include a set of coordinates at maximum zoom, and downloading those tiles.
    /// </summary>
    internal class OSM
    {
        /// <summary>
        /// Creates bounding box of tiles needed at given zoom level to include all tiles in list. Each tile
        /// coordinate must be more than boundingBoxTrimMargin pixels from any edge of the bounding box.
        /// </summary>
        /// <param name="tiles">A list of OSM tile references and their associated coordinate</param>
        /// <param name="boundingBox">The bounding box to be populated</param>
        /// <param name="zoom">The zoom level required for the bounding box</param>
        internal static BoundingBox GetBoundingBox(List<Tile> tiles, int zoom)
        {
            // Initialise boundingBox to the first tile
            BoundingBox boundingBox = new();
            List<int> xAxis = [tiles[0].XIndex];
            boundingBox.xAxis = xAxis;
            List<int> yAxis = [tiles[0].YIndex];
            boundingBox.yAxis = yAxis;

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
        static internal void ExtendBoundingBox(Tile newTile, BoundingBox boundingBox, int zoom)
        {
            // New tile is above BB i.e. tileNo < boundingBox[yAxis][0]
            if (newTile.YIndex < boundingBox.yAxis[0])
            {
                ExtendBoundingBoxNorth(newTile, boundingBox);
            }

            // New tile is below BB i.e. tileNo > boundingBox[yAxis][^1]
            if (newTile.YIndex > boundingBox.yAxis[^1])
            {
                ExtendBoundingBoxSouth(newTile, boundingBox);
            }

            // New tile is right of BB i.e. tileNo > boundingBox[xAxis][^1], determine whether to move righthand
            // side of bounding box further to the right (usual case) or lefthand side further to the left (across meridian)
            if (newTile.XIndex > boundingBox.xAxis[^1])
            {
                ExtendBoundingBoxEast(newTile, boundingBox, zoom);
            }

            // New tile is left of BB i.e. tileNo < boundingBox[xAxis][0], determine whether to move lefthand
            // side of bounding box further to the left (usual case) or righthand side further to the right (across meridian)
            if (newTile.XIndex < boundingBox.xAxis[0])
            {
                ExtendBoundingBoxWest(newTile, boundingBox, zoom);
            }
        }

        /// <summary>
        /// New tile is above BB i.e. tileNo less than boundingBox[yAxis][0]
        /// </summary>
        /// <param name="newTile">The tile to be added to bounding box</param>
        /// <param name="boundingBox">The bounding box is two lists of tile numbers, one for x axis the other y axis</param>
        static internal void ExtendBoundingBoxNorth(Tile newTile, BoundingBox boundingBox)
        {
            // Insert extra tile No's at beginning of yAxis list
            for (int tileNo = boundingBox.yAxis[0] - 1; tileNo >= newTile.YIndex; tileNo--)
            {
                boundingBox.yAxis.Insert(0, tileNo);
            }
        }

        /// <summary>
        /// New tile is below BB i.e. tileNo > boundingBox[yAxis][^1]
        /// </summary>
        /// <param name="newTile">The tile to be added to bounding box</param>
        /// <param name="boundingBox">The bounding box is two lists of tile numbers, one for x axis the other y axis</param>
        static internal void ExtendBoundingBoxSouth(Tile newTile, BoundingBox boundingBox)
        {
            // Append extra tileNo's at end of yAxis list
            for (int tileNo = boundingBox.yAxis[^1] + 1; tileNo <= newTile.YIndex; tileNo++)
            {
                boundingBox.yAxis.Add(tileNo);
            }
        }

        /// <summary>
        /// New tile is right of BB i.e. tileNo > boundingBox[xAxis][^1], determine whether to move righthand
        /// side of bounding box further to the right (usual case) or lefthand side further to the left (across meridian)
        /// </summary>
        /// <param name="newTile">The tile to be added to bounding box</param>
        /// <param name="boundingBox">The bounding box is two lists of tile numbers, one for x axis the other y axis</param>
        /// <param name="zoom">The zoom level of the bounding box</param>
        static internal void ExtendBoundingBoxEast(Tile newTile, BoundingBox boundingBox, int zoom)
        {
            int distEast, distWest;

            distEast = newTile.XIndex - boundingBox.xAxis[^1];
            distWest = boundingBox.xAxis[0] + Convert.ToInt32(Math.Pow(2, zoom)) - newTile.XIndex;
            if (distEast <= distWest)
            {
                // Append extra tileNo's at end of xAxis list
                for (int tileNo = boundingBox.xAxis[^1] + 1; tileNo <= newTile.XIndex; tileNo++)
                {
                    boundingBox.xAxis.Add(tileNo);
                }
            }
            else
            {
                // Insert extra tileNo's at beginning of xAxis list
                for (int tileNo = boundingBox.xAxis[0] - 1; tileNo >= 0; tileNo--)
                {
                    boundingBox.xAxis.Insert(0, tileNo);
                }
                for (int tileNo = Convert.ToInt32(Math.Pow(2, zoom)) - 1; tileNo >= newTile.XIndex; tileNo--)
                {
                    boundingBox.xAxis.Insert(0, tileNo);
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
        static internal void ExtendBoundingBoxWest(Tile newTile, BoundingBox boundingBox, int zoom)
        {
            int distEast, distWest;

            distWest = boundingBox.xAxis[0] - newTile.XIndex;
            distEast = Convert.ToInt32(Math.Pow(2, zoom)) - boundingBox.xAxis[0] + newTile.XIndex;
            if (distWest <= distEast)
            {
                // Insert extra tileNo's at front of xAxis list
                for (int tileNo = boundingBox.xAxis[0] - 1; tileNo >= newTile.XIndex; tileNo--)
                {
                    boundingBox.xAxis.Insert(0, tileNo);
                }
            }
            else
            {
                // Append extra tileNo's at end of xAxis list
                for (int tileNo = boundingBox.xAxis[^1] + 1; tileNo < Convert.ToInt32(Math.Pow(2, zoom)); tileNo++)
                {
                    boundingBox.xAxis.Add(tileNo);
                }
                for (int tileNo = 0; tileNo <= newTile.XIndex; tileNo++)
                {
                    boundingBox.xAxis.Add(tileNo);
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
        static internal void CheckBoundingBoxEdges(List<Tile> tiles, BoundingBox boundingBox, int zoom)
        {
            for (int tileNo = 0; tileNo < tiles.Count; tileNo++)
            {
                // Check North edge of bounding box
                if (tiles[tileNo].YIndex == boundingBox.yAxis[0])
                {
                    CheckBBedgesNorth(tiles, boundingBox, tileNo);
                }

                // Check East edge of bounding box
                if (tiles[tileNo].XIndex == boundingBox.xAxis[^1])
                {
                    CheckBBedgesEast(tiles, boundingBox, zoom, tileNo);
                }

                // Check South edge of bounding box
                if (tiles[tileNo].YIndex == boundingBox.yAxis[^1])
                {
                    CheckBBedgesSouth(tiles, boundingBox, zoom, tileNo);
                }

                // Check West edge of bounding box
                if (tiles[tileNo].XIndex == boundingBox.xAxis[0])
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
        static internal void CheckBBedgesNorth(List<Tile> tiles, BoundingBox boundingBox, int tileNo)
        {
            if ((tiles[tileNo].YOffset < Con.boundingBoxTrimMargin) && (tiles[tileNo].YIndex > 0))
            {
                boundingBox.yAxis.Insert(0, tiles[tileNo].YIndex - 1); // Only extend as far as yTile = 0
            }
        }

        /// <summary>
        /// Extend bounding box east if coordinate offset is too close to the east edge. This could involve 
        /// crossing the meridan line.
        /// </summary>
        /// <param name="tiles">A list of OSM tile references and their associated coordinate</param>
        /// <param name="boundingBox">The bounding box to be populated</param>
        /// <param name="zoom">The zoom level required for the bounding box</param>
        /// <param name="tileNo">The index of tile to be checked</param>
        static internal void CheckBBedgesEast(List<Tile> tiles, BoundingBox boundingBox, int zoom, int tileNo)
        {
            int newTileNo;

            if (tiles[tileNo].XOffset > Con.tileSize - Con.boundingBoxTrimMargin)
            {
                newTileNo = MapTileCalculator.IncXtileNo(tiles[tileNo].XIndex, zoom);
                if (newTileNo != boundingBox.xAxis[0])
                {
                    boundingBox.xAxis.Add(newTileNo); // Only extend if not already using all tiles available in x axis
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
        static internal void CheckBBedgesSouth(List<Tile> tiles, BoundingBox boundingBox, int zoom, int tileNo)
        {
            if ((tiles[tileNo].YOffset > Con.tileSize - Con.boundingBoxTrimMargin) && (tiles[tileNo].YIndex < Convert.ToInt32(Math.Pow(2, zoom)) - 1))
            {
                boundingBox.yAxis.Add(tiles[tileNo].YIndex + 1); // Only extend as far as xTile = Math.Pow(2, zoom) - 1
            }
        }

        /// <summary>
        /// Extend bounding box west if coordinate offset is too close to the west edge. This could involve 
        /// crossing the meridan line.
        /// </summary>
        /// <param name="tiles">A list of OSM tile references and their associated coordinate</param>
        /// <param name="boundingBox">The bounding box to be populated</param>
        /// <param name="zoom">The zoom level required for the bounding box</param>
        /// <param name="tileNo">The index of tile to be checked</param>
        static internal void CheckBBedgesWest(List<Tile> tiles, BoundingBox boundingBox, int zoom, int tileNo)
        {
            int newTileNo;

            if (tiles[tileNo].XOffset < Con.boundingBoxTrimMargin)
            {
                newTileNo = MapTileCalculator.DecXtileNo(tiles[tileNo].XIndex, zoom);
                if (newTileNo != boundingBox.xAxis[^1])
                {
                    boundingBox.xAxis.Insert(0, newTileNo); // Only extend if not already using all tiles available in x axis
                }
            }
        }

        #endregion

        #region Download OSM tiles region

        /// <summary>
        /// Orchestrates the retrieval of a single OpenStreetMap (OSM) tile.
        /// It attempts to fetch the tile from a local cache first; if not found, it downloads the tile
        /// from the configured server using the provided API key. The retrieved tile is then stored
        /// at the specified local path.
        /// </summary>
        /// <param name="xTileNo">The X (East/West) coordinate of the required tile at the specified zoom level, corresponding to the OSM tiling scheme.</param>
        /// <param name="yTileNo">The Y (North/South) coordinate of the required tile at the specified zoom level, corresponding to the OSM tiling scheme.</param>
        /// <param name="zoom">The specific zoom level for which the OSM tile is required.</param>
        /// <param name="filename">The full local path and filename where the OSM tile will be stored after retrieval (e.g., "C:\Images\map_tile").</param>
        /// <returns><see langword="true"/> if the OSM tile was successfully retrieved (either from cache or by download) and saved;
        /// otherwise, <see langword="false"/> if any error occurred during the process (errors are logged by underlying methods).</returns>
        static internal bool DownloadOSMtile(int xTileNo, int yTileNo, int zoom, string filename)
        {
            // Construct the full URL for the OSM tile based on configured server URL, tile coordinates, zoom, and API key.
            string url = $"{Parameters.SettingsCacheServerURL}/{zoom}/{xTileNo}/{yTileNo}.png{Parameters.SettingsCacheServerAPIkey}";

            // Delegate the actual retrieval (from cache or download) and saving to the Cache class.
            // The key is constructed using zoom, xTileNo, and yTileNo for cache lookup.
            // Errors are handled and logged by the Cache.GetOrCopyOSMtile method and its dependencies.
            return Cache.GetOrCopyOSMtile($"{zoom}-{xTileNo}-{yTileNo}.png", url, $"{Parameters.ImageFolder}\\{filename}");
        }

        /// <summary>
        /// Downloads a column of OpenStreetMap (OSM) tiles using server and API key specified by user settings.
        /// It first checks whether the OSM tiles are in the tile cache. If not, they are downloaded.
        /// All tiles are stored at the specified path given by filename.
        /// </summary>
        /// <param name="xTileNo">East/West reference number for the required tile column at specified zoom.</param>
        /// <param name="xIndex">Used as part of the filename to uniquely identify tiles within the column.</param>
        /// <param name="boundingBox">The bounding box containing the y-axis tile numbers, used to determine the height of the column of tiles to be downloaded.</param>
        /// <param name="zoom">Required zoom level for the OSM tiles to be downloaded.</param>
        /// <param name="filename">Base path and filename where the individual OSM tiles will be stored.
        /// Each tile's filename will be suffixed with its xIndex and yIndex (e.g., "basefilename_xIndex_yIndex.png").</param>
        /// <returns><see langword="true"/> if all tiles in the column were successfully downloaded or retrieved from cache;
        /// otherwise, <see langword="false"/> if any tile operation failed.</returns>
        static internal bool DownloadOSMtileColumn(int xTileNo, int xIndex, BoundingBox boundingBox, int zoom, string filename)
        {
            // Iterate through each y-axis tile number in the bounding box
            for (int yIndex = 0; yIndex < boundingBox.yAxis.Count; yIndex++)
            {
                // Construct the unique filename for the current tile
                string tileFilename = $"{filename}_{xIndex}_{yIndex}.png";

                // Attempt to download or copy the individual OSM tile.
                // If DownloadOSMtile returns false (indicating a failure),
                // we immediately return false for the entire column download.
                if (!DownloadOSMtile(xTileNo, boundingBox.yAxis[yIndex], zoom, tileFilename))
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
        /// <param name="yIndex">Used as part of the filename to uniquely identify tiles within the row.</param>
        /// <param name="boundingBox">The bounding box containing the x-axis tile numbers, used to determine the width of the row of tiles to be downloaded.</param>
        /// <param name="zoom">Required zoom level for the OSM tiles to be downloaded.</param>
        /// <param name="filename">Base path and filename where the individual OSM tiles will be stored.
        /// Each tile's filename will be suffixed with its xIndex and yIndex (e.g., "basefilename_xIndex_yIndex.png").</param>
        /// <returns><see langword="true"/> if all tiles in the row were successfully downloaded or retrieved from cache;
        /// otherwise, <see langword="false"/> if any tile operation failed.</returns>
        static internal bool DownloadOSMtileRow(int yTileNo, int yIndex, BoundingBox boundingBox, int zoom, string filename)
        {
            // Iterate through each x-axis tile number in the bounding box
            for (int xIndex = 0; xIndex < boundingBox.xAxis.Count; xIndex++)
            {
                // Construct the unique filename for the current tile
                string tileFilename = $"{filename}_{xIndex}_{yIndex}.png";

                // Attempt to download or copy the individual OSM tile.
                // If DownloadOSMtile returns false (indicating a failure),
                // we immediately return false for the entire row download.
                if (!DownloadOSMtile(boundingBox.xAxis[xIndex], yTileNo, zoom, tileFilename))
                {
                    return false; // An individual tile failed to download/copy, so the row download fails.
                }
            }

            // If the loop completes, all individual tiles were successfully downloaded or copied.
            return true;
        }

        // Finds OSM tile numbers and offsets for a sinle coordinate for one zoom level
        // Tile number calculated using https://wiki.openstreetmap.org/wiki/Slippy_map_tilenames
        // $"{tileServer}{zoom}/{xTile}/{yTile}.png?{rapidApiKey}"
        static internal Tile GetOSMtile(string sLon, string sLat, int zoom)
        {
            Tile tile = new(0, 0, 0, 0);
            if (LonToDecimalDegree(sLon, out double dLon) && LatToDecimalDegree(sLat, out double dLat))
            {
                LonToTileX(dLon, zoom, tile);
                LatToTileY(dLat, zoom, tile);
                return tile;
            }
            return null;
        }

        // Finds OSM tile numbers and offsets for a sinle coordinate for one zoom level
        // Tile number calculated using https://wiki.openstreetmap.org/wiki/Slippy_map_tilenames
        // $"{tileServer}{zoom}/{xTile}/{yTile}.png?{rapidApiKey}"
        static internal Tile GetTileInfo(double dLon, double dLat, int zoom)
        {
            Tile tile = new(0, 0, 0, 0);
            LonToTileX(dLon, zoom, tile);
            LatToTileY(dLat, zoom, tile);
            return tile;
        }

        #endregion

        #region Utilities region

        /// <summary>
        /// Uses CordinatePart library to convert longitude of point to decimal degrees.
        /// </summary>
        /// <param name="sLon">The longitude value in format other than decimal degrees.</param>
        /// <param name="dLon">The longitude value converted to decimal degrees.</param>
        /// <returns>True if longitude successfully converted to decimal degrees format.</returns>
        internal static bool LonToDecimalDegree(string sLon, out double dLon)
        {
            if (CoordinatePart.TryParse(sLon, out CoordinatePart cLon))
            {
                dLon = cLon.DecimalDegree;
                return true;
            }
            dLon = 0;
            return false;
        }

        /// <summary>
        /// Uses CordinatePart library to convert latitude of point to decimal degrees.
        /// </summary>
        /// <param name="sLon">The latitude value in format other than decimal degrees.</param>
        /// <param name="dLon">The latitude value converted to decimal degrees.</param>
        /// <returns>True if latitude successfully converted to decimal degrees format.</returns>
        internal static bool LatToDecimalDegree(string sLat, out double dLat)
        {
            if (CoordinatePart.TryParse(sLat, out CoordinatePart cLat))
            {
                dLat = cLat.DecimalDegree;
                return true;
            }
            dLat = 0;
            return false;
        }

        /// <summary>
        /// Converts longitude in decimal degrees to xTile number for OSM tiles at given zoom level. Works
        /// out the xOffset amount of the longitude value on the OSM tile. Uses formulae found at
        /// https://wiki.openstreetmap.org/wiki/Slippy_map_tilenames.
        /// </summary>
        /// <param name="dLon">The longitude value converted to decimal degrees.</param>
        /// <param name="z">The specified zoom level.</param>
        /// <param name="tile">Stores the xTile and xOffset values for the specified longitude at specified zoom level.</param>
        internal static void LonToTileX(double dLon, int z, Tile tile)
        {
            double doubleTileX = (dLon + 180.0) / 360.0 * (1 << z);
            tile.XIndex = Convert.ToInt32(Math.Floor(doubleTileX));
            tile.XOffset = Convert.ToInt32(256 * (doubleTileX - tile.XIndex));
        }

        /// <summary>
        /// Converts latitude in decimal degrees to yTile number for OSM tiles at given zoom level. Works
        /// out the yOffset amount of the latitude value on the OSM tile. Uses formulae found at
        /// https://wiki.openstreetmap.org/wiki/Slippy_map_tilenames.
        /// </summary>
        /// <param name="dLon">The latitude value converted to decimal degrees.</param>
        /// <param name="z">The specified zoom level.</param>
        /// <param name="tile">Stores the yTile and yOffset values for the specified latitude at specified zoom level.</param>
        internal static void LatToTileY(double dLat, int z, Tile tile)
        {
            var latRad = dLat / 180 * Math.PI;
            double doubleTileY = (1 - Math.Log(Math.Tan(latRad) + 1 / Math.Cos(latRad)) / Math.PI) / 2 * (1 << z);
            tile.YIndex = Convert.ToInt32(Math.Floor(doubleTileY));
            tile.YOffset = Convert.ToInt32(256 * (doubleTileY - tile.YIndex));
        }

        /// <summary>
        /// Converts xTile/yTile/zoom combination to a latitude and longitude 
        /// </summary>
        /// <param name="xTile">The OSM xTile number</param>
        /// <param name="yTile">The OSM yTile number</param>
        /// <param name="zoom">The OSM zoom level</param>
        /// <returns>Latitude and longitude for top left corner of tile reference as +/- decimal degrees</returns>
        internal static Coordinate TileNoToLatLon(int xTile, int yTile, int zoom)
        {
            // using formula from https://wiki.openstreetmap.org/wiki/Slippy_map_tilenames
            // note tile numbers run from 0 .. (2 pow zoom - 1), passing 2 pow zoom, 2 pow zoom gives bottom right of tile
            // (2 pow zoom - 1), (2 pow zoom - 1)
            double n = Math.Pow(2, zoom);
            double latitudeRadians = Math.Atan(Math.Sinh(Math.PI * (1 - 2 * yTile / n)));
            Coordinate c = new(latitudeRadians * 180.0 / Math.PI, xTile / n * 360.0 - 180.0);
            return c;
        }

        #endregion
    }
}
