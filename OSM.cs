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
    public class Tile
    {
        public int xIndex;
        public int yIndex;
        public int xOffset;
        public int yOffset;
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
        /// <param name="boundingBox">The bounding box to be populated
        /// <param name="zoom">The zoom level required for the bounding box</param>
        static internal BoundingBox GetTilesBoundingBox(List<Tile> tiles, int zoom)
        {
            // Initialise boundingBox to the first tile
            BoundingBox boundingBox = new();
            List<int> xAxis = [tiles[0].xIndex];
            boundingBox.xAxis = xAxis;
            List<int> yAxis = [tiles[0].yIndex];
            boundingBox.yAxis = yAxis;

            // Adjust boundingBox as needed to include remaining tiles
            for (int tileNo = 1; tileNo < tiles.Count; tileNo++)
            {
                AddTileToBoundingBox(tiles[tileNo], boundingBox, zoom);
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
        static internal void AddTileToBoundingBox(Tile newTile, BoundingBox boundingBox, int zoom)
        {
            // New tile is above BB i.e. tileNo < boundingBox[yAxis][0]
            if (newTile.yIndex < boundingBox.yAxis[0])
            {
                AddTileToBBnorth(newTile, boundingBox);
            }

            // New tile is below BB i.e. tileNo > boundingBox[yAxis][^1]
            if (newTile.yIndex > boundingBox.yAxis[^1])
            {
                AddTileToBBsouth(newTile, boundingBox);
            }

            // New tile is right of BB i.e. tileNo > boundingBox[xAxis][^1], determine whether to move righthand
            // side of bounding box further to the right (usual case) or lefthand side further to the left (across meridian)
            if (newTile.xIndex > boundingBox.xAxis[^1])
            {
                AddTileToBBeast(newTile, boundingBox, zoom);
            }

            // New tile is left of BB i.e. tileNo < boundingBox[xAxis][0], determine whether to move lefthand
            // side of bounding box further to the left (usual case) or righthand side further to the right (across meridian)
            if (newTile.xIndex < boundingBox.xAxis[0])
            {
                AddTileToBBwest(newTile, boundingBox, zoom);
            }
        }

        /// <summary>
        /// New tile is above BB i.e. tileNo less than boundingBox[yAxis][0]
        /// </summary>
        /// <param name="newTile">The tile to be added to bounding box</param>
        /// <param name="boundingBox">The bounding box is two lists of tile numbers, one for x axis the other y axis</param>
        static internal void AddTileToBBnorth(Tile newTile, BoundingBox boundingBox)
        {
            // Insert extra tile No's at beginning of yAxis list
            for (int tileNo = boundingBox.yAxis[0] - 1; tileNo >= newTile.yIndex; tileNo--)
            {
                boundingBox.yAxis.Insert(0, tileNo);
            }
        }

        /// <summary>
        /// New tile is below BB i.e. tileNo > boundingBox[yAxis][^1]
        /// </summary>
        /// <param name="newTile">The tile to be added to bounding box</param>
        /// <param name="boundingBox">The bounding box is two lists of tile numbers, one for x axis the other y axis</param>
        static internal void AddTileToBBsouth(Tile newTile, BoundingBox boundingBox)
        {
            // Append extra tileNo's at end of yAxis list
            for (int tileNo = boundingBox.yAxis[^1] + 1; tileNo <= newTile.yIndex; tileNo++)
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
        static internal void AddTileToBBeast(Tile newTile, BoundingBox boundingBox, int zoom)
        {
            int distEast, distWest;

            distEast = newTile.xIndex - boundingBox.xAxis[^1];
            distWest = boundingBox.xAxis[0] + Convert.ToInt32(Math.Pow(2, zoom)) - newTile.xIndex;
            if (distEast <= distWest)
            {
                // Append extra tileNo's at end of xAxis list
                for (int tileNo = boundingBox.xAxis[^1] + 1; tileNo <= newTile.xIndex; tileNo++)
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
                for (int tileNo = Convert.ToInt32(Math.Pow(2, zoom)) - 1; tileNo >= newTile.xIndex; tileNo--)
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
        static internal void AddTileToBBwest(Tile newTile, BoundingBox boundingBox, int zoom)
        {
            int distEast, distWest;

            distWest = boundingBox.xAxis[0] - newTile.xIndex;
            distEast = Convert.ToInt32(Math.Pow(2, zoom)) - boundingBox.xAxis[0] + newTile.xIndex;
            if (distWest <= distEast)
            {
                // Insert extra tileNo's at front of xAxis list
                for (int tileNo = boundingBox.xAxis[0] - 1; tileNo >= newTile.xIndex; tileNo--)
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
                for (int tileNo = 0; tileNo <= newTile.xIndex; tileNo++)
                {
                    boundingBox.xAxis.Add(tileNo);
                }
            }
        }

        #endregion

        #region Checking bounding box edges region

        // Go through list of tiles and for those tiles that are on an edge of the bounding box
        // check that the offset values of tile coordinate are not too close to the bounding box edge
        static internal void CheckBoundingBoxEdges(List<Tile> tiles, BoundingBox boundingBox, int zoom)
        {
            for (int tileNo = 0; tileNo < tiles.Count; tileNo++)
            {
                // Check North edge of bounding box
                if (tiles[tileNo].yIndex == boundingBox.yAxis[0])
                {
                    CheckBBedgesNorth(tiles, boundingBox, tileNo);
                }

                // Check East edge of bounding box
                if (tiles[tileNo].xIndex == boundingBox.xAxis[^1])
                {
                    CheckBBedgesEast(tiles, boundingBox, zoom, tileNo);
                }

                // Check South edge of bounding box
                if (tiles[tileNo].yIndex == boundingBox.yAxis[^1])
                {
                    CheckBBedgesSouth(tiles, boundingBox, zoom, tileNo);
                }

                // Check West edge of bounding box
                if (tiles[tileNo].xIndex == boundingBox.xAxis[0])
                {
                    CheckBBedgesWest(tiles, boundingBox, zoom, tileNo);
                }
            }
        }

        static internal void CheckBBedgesNorth(List<Tile> tiles, BoundingBox boundingBox, int tileNo)
        {
            if ((tiles[tileNo].yOffset < Con.boundingBoxTrimMargin) && (tiles[tileNo].yIndex > 0))
            {
                boundingBox.yAxis.Insert(0, tiles[tileNo].yIndex - 1); // Only extend as far as xTile = 0
            }
        }

        static internal void CheckBBedgesEast(List<Tile> tiles, BoundingBox boundingBox, int zoom, int tileNo)
        {
            int newTileNo;

            if (tiles[tileNo].xOffset > Con.tileSize - Con.boundingBoxTrimMargin)
            {
                newTileNo = Drawing.IncXtileNo(tiles[tileNo].xIndex, zoom);
                if (newTileNo != boundingBox.xAxis[0])
                {
                    boundingBox.xAxis.Add(newTileNo); // Only extend if not already using all tiles available in x axis
                }
            }
        }

        static internal void CheckBBedgesSouth(List<Tile> tiles, BoundingBox boundingBox, int zoom, int tileNo)
        {
            if ((tiles[tileNo].yOffset > Con.tileSize - Con.boundingBoxTrimMargin) && (tiles[tileNo].yIndex < Convert.ToInt32(Math.Pow(2, zoom)) - 1))
            {
                boundingBox.yAxis.Add(tiles[tileNo].yIndex + 1); // Only extend as far as xTile = Math.Pow(2, zoom) - 1
            }
        }

        static internal void CheckBBedgesWest(List<Tile> tiles, BoundingBox boundingBox, int zoom, int tileNo)
        {
            int newTileNo;

            if (tiles[tileNo].xOffset < Con.boundingBoxTrimMargin)
            {
                newTileNo = Drawing.DecXtileNo(tiles[tileNo].xIndex, zoom);
                if (newTileNo != boundingBox.xAxis[^1])
                {
                    boundingBox.xAxis.Insert(0, newTileNo); // Only extend if not already using all tiles available in x axis
                }
            }
        }

        #endregion

        #region Download OSM tiles region

        static internal void DownloadOSMtile(int xTileNo, int yTileNo, int zoom, string filename)
        {
            string imagePath = $"{Path.GetDirectoryName(Parameters.SaveLocation)}\\images\\";
            string url = $"{Con.tileServer}{zoom}/{xTileNo}/{yTileNo}.png{Con.rapidApiKey}";
            Cache.GetOrCopyWebDoc($"{zoom}-{xTileNo}-{yTileNo}.png", url, Path.GetDirectoryName(Parameters.SaveLocation), $"{imagePath}{filename}");
        }

        static internal void DownloadOSMtileColumn(int xTileNo, int xIndex, BoundingBox boundingBox, int zoom, string filename)
        {
            for (int yIndex = 0; yIndex < boundingBox.yAxis.Count; yIndex++)
            {
                DownloadOSMtile(xTileNo, boundingBox.yAxis[yIndex], zoom, $"{filename}_{xIndex}_{yIndex}.png");
            }
        }

        static internal void DownloadOSMtileRow(int yTileNo, int yIndex, BoundingBox boundingBox, int zoom, string filename)
        {
            for (int xIndex = 0; xIndex < boundingBox.xAxis.Count; xIndex++)
            {
                DownloadOSMtile(boundingBox.xAxis[xIndex], yTileNo, zoom, $"{filename}_{xIndex}_{yIndex}.png");
            }
        }

        // Finds OSM tile numbers and offsets for a sinle coordinate for one zoom level
        // Tile number calculated using https://wiki.openstreetmap.org/wiki/Slippy_map_tilenames
        // $"{tileServer}{zoom}/{xTile}/{yTile}.png?{rapidApiKey}"
        static internal Tile GetOSMtile(string sLon, string sLat, int zoom)
        {
            Tile tile = new();
            if (LonToDecimalDegree(sLon, out double dLon) && LatToDecimalDegree(sLat, out double dLat))
            {
                LonToTileX(dLon, zoom, tile);
                LatToTileY(dLat, zoom, tile);
                return tile;
            }
            return null;
        }

        #endregion

        #region Utilities region

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

        internal static void LonToTileX(double dLon, int z, Tile tile)
        {
            double doubleTileX = (dLon + 180.0) / 360.0 * (1 << z);
            tile.xIndex = Convert.ToInt32(Math.Floor(doubleTileX));
            tile.xOffset = Convert.ToInt32(256 * (doubleTileX - tile.xIndex));
        }

        internal static void LatToTileY(double dLat, int z, Tile tile)
        {
            var latRad = dLat / 180 * Math.PI;
            double doubleTileY = (1 - Math.Log(Math.Tan(latRad) + 1 / Math.Cos(latRad)) / Math.PI) / 2 * (1 << z);
            tile.yIndex = Convert.ToInt32(Math.Floor(doubleTileY));
            tile.yOffset = Convert.ToInt32(256 * (doubleTileY - tile.yIndex));
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
