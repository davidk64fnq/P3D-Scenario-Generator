using CoordinateSharp;
using ImageMagick;

namespace P3D_Scenario_Generator
{
    internal class OSM
    {
        internal static int xAxis = 0, yAxis = 1; // Used in bounding box to denote lists that store xTile and yTile reference numbers
        internal static int xTile = 0, yTile = 1, xOffset = 2, yOffset = 3; // Used to define OSM tile, x and y numbers plus position of coord
        readonly static string tileServer = "https://maptiles.p.rapidapi.com/en/map/v1/";
        readonly static string rapidapiKey = "?rapidapi-key=d9de94c22emsh6dc07cd7103e683p12be01jsn7014f38e1975";
        internal static int tileSize = 256; // All OSM tiles used in this program are 256x256 pixels
        internal static int boundingBoxTrimMargin = 15; // If tile coordinate is within this many pixels of bounding box edge, extra tiles added

        // The bounding box is two lists of tile numbers, one for x axis the other y axis. The tile numbers
        // will usually be consecutive within the bounds 0 .. (2 to exp zoom - 1). However it's possible for tiles grouped 
        // across the meridian to have a sequence of x axis tile numbers that goes up to (2 to exp zoom - 1) and then continues
        // from 0
        static internal void GetTilesBoundingBox(List<List<int>> tiles, List<List<int>> boundingBox, int zoom)
        {
            // Initialise boundingBox to the first tile
            boundingBox.Clear();
            List<int> xAxis = [tiles[0][xTile]];
            boundingBox.Add(xAxis);
            List<int> yAxis = [tiles[0][yTile]];
            boundingBox.Add(yAxis);

            // Adjust boundingBox as needed to include remaining tiles
            for (int tileNo = 1; tileNo < tiles.Count; tileNo++)
            {
                AddTileToBoundingBox(tiles[tileNo], boundingBox, zoom);
            }

            // Add extra tiles if any tile coordinates are too close to bounding box edge
            CheckBoundingBoxEdges(tiles, boundingBox, zoom);
        }

        #region Adding tiles to bounding box region

        static internal void AddTileToBoundingBox(List<int> newTile, List<List<int>> boundingBox, int zoom)
        {
            // New tile is above BB i.e. tileNo < boundingBox[yAxis][0]
            if (newTile[yTile] < boundingBox[yAxis][0])
            {
                AddTileToBBnorth(newTile, boundingBox);
            }

            // New tile is below BB i.e. tileNo > boundingBox[yAxis][^1]
            if (newTile[yTile] > boundingBox[yAxis][^1])
            {
                AddTileToBBsouth(newTile, boundingBox);
            }

            // New tile is right of BB i.e. tileNo > boundingBox[xAxis][^1], determine whether to move righthand
            // side of bounding box further to the right (usual case) or lefthand side further to the left (across meridian)
            if (newTile[xTile] > boundingBox[xAxis][^1])
            {
                AddTileToBBeast(newTile, boundingBox, zoom);
            }

            // New tile is left of BB i.e. tileNo < boundingBox[xAxis][0], determine whether to move lefthand
            // side of bounding box further to the left (usual case) or righthand side further to the right (across meridian)
            if (newTile[xTile] < boundingBox[xAxis][0])
            {
                AddTileToBBwest(newTile, boundingBox, zoom);
            }
        }

        static internal void AddTileToBBnorth(List<int> newTile, List<List<int>> boundingBox)
        {
            // Insert extra tile No's at beginning of yAxis list
            for (int tileNo = boundingBox[yAxis][0] - 1; tileNo >= newTile[yTile]; tileNo--)
            {
                boundingBox[yAxis].Insert(0, tileNo);
            }
        }

        static internal void AddTileToBBsouth(List<int> newTile, List<List<int>> boundingBox)
        {
            // Append extra tileNo's at end of yAxis list
            for (int tileNo = boundingBox[yAxis][^1] + 1; tileNo <= newTile[yTile]; tileNo++)
            {
                boundingBox[yAxis].Add(tileNo);
            }
        }

        static internal void AddTileToBBeast(List<int> newTile, List<List<int>> boundingBox, int zoom)
        {
            int distEast, distWest;

            distEast = newTile[xTile] - boundingBox[xAxis][^1];
            distWest = boundingBox[xAxis][0] + Convert.ToInt32(Math.Pow(2, zoom)) - newTile[xTile];
            if (distEast <= distWest)
            {
                // Append extra tileNo's at end of xAxis list
                for (int tileNo = boundingBox[xAxis][^1] + 1; tileNo <= newTile[xTile]; tileNo++)
                {
                    boundingBox[xAxis].Add(tileNo);
                }
            }
            else
            {
                // Insert extra tileNo's at beginning of xAxis list
                for (int tileNo = boundingBox[xAxis][0] - 1; tileNo >= 0; tileNo--)
                {
                    boundingBox[xAxis].Insert(0, tileNo);
                }
                for (int tileNo = Convert.ToInt32(Math.Pow(2, zoom)) - 1; tileNo >= newTile[xTile]; tileNo--)
                {
                    boundingBox[xAxis].Insert(0, tileNo);
                }
            }
        }

        static internal void AddTileToBBwest(List<int> newTile, List<List<int>> boundingBox, int zoom)
        {
            int distEast, distWest;

            distWest = boundingBox[xAxis][0] - newTile[xTile];
            distEast = Convert.ToInt32(Math.Pow(2, zoom)) - boundingBox[xAxis][0] + newTile[xTile];
            if (distWest <= distEast)
            {
                // Insert extra tileNo's at front of xAxis list
                for (int tileNo = boundingBox[xAxis][0] - 1; tileNo >= newTile[xTile]; tileNo--)
                {
                    boundingBox[xAxis].Insert(0, tileNo);
                }
            }
            else
            {
                // Append extra tileNo's at end of xAxis list
                for (int tileNo = boundingBox[xAxis][^1] + 1; tileNo < Convert.ToInt32(Math.Pow(2, zoom)); tileNo++)
                {
                    boundingBox[xAxis].Add(tileNo);
                }
                for (int tileNo = 0; tileNo <= newTile[xTile]; tileNo++)
                {
                    boundingBox[xAxis].Add(tileNo);
                }
            }
        }

        #endregion

        #region Checking bounding box edges region

        // Go through list of tiles and for those tiles that are on an edge of the bounding box
        // check that the offset values of tile coordinate are not too close to the bounding box edge
        static internal void CheckBoundingBoxEdges(List<List<int>> tiles, List<List<int>> boundingBox, int zoom)
        {
            for (int tileNo = 0; tileNo < tiles.Count; tileNo++)
            {
                // Check North edge of bounding box
                if (tiles[tileNo][yAxis] == boundingBox[yAxis][0])
                {
                    CheckBBedgesNorth(tiles, boundingBox, tileNo);
                }

                // Check East edge of bounding box
                if (tiles[tileNo][xAxis] == boundingBox[xAxis][^1])
                {
                    CheckBBedgesEast(tiles, boundingBox, zoom, tileNo);
                }

                // Check South edge of bounding box
                if (tiles[tileNo][yAxis] == boundingBox[yAxis][^1])
                {
                    CheckBBedgesSouth(tiles, boundingBox, zoom, tileNo);
                }

                // Check West edge of bounding box
                if (tiles[tileNo][xAxis] == boundingBox[xAxis][0])
                {
                    CheckBBedgesWest(tiles, boundingBox, zoom, tileNo);
                }
            }
        }

        static internal void CheckBBedgesNorth(List<List<int>> tiles, List<List<int>> boundingBox, int tileNo)
        {
            if ((tiles[tileNo][yOffset] < boundingBoxTrimMargin) && (tiles[tileNo][yAxis] > 0))
            {
                boundingBox[yAxis].Insert(0, tiles[tileNo][yAxis] - 1); // Only extend as far as xTile = 0
            }
        }

        static internal void CheckBBedgesEast(List<List<int>> tiles, List<List<int>> boundingBox, int zoom, int tileNo)
        {
            int newTileNo;

            if (tiles[tileNo][xOffset] > tileSize - boundingBoxTrimMargin)
            {
                newTileNo = Drawing.IncXtileNo(tiles[tileNo][xAxis], zoom);
                if (newTileNo != boundingBox[xAxis][0])
                {
                    boundingBox[xAxis].Add(newTileNo); // Only extend if not already using all tiles available in x axis
                }
            }
        }

        static internal void CheckBBedgesSouth(List<List<int>> tiles, List<List<int>> boundingBox, int zoom, int tileNo)
        {
            if ((tiles[tileNo][yOffset] > tileSize - boundingBoxTrimMargin) && (tiles[tileNo][yAxis] < Convert.ToInt32(Math.Pow(2, zoom)) - 1))
            {
                boundingBox[yAxis].Add(tiles[tileNo][yAxis] + 1); // Only extend as far as xTile = Math.Pow(2, zoom) - 1
            }
        }

        static internal void CheckBBedgesWest(List<List<int>> tiles, List<List<int>> boundingBox, int zoom, int tileNo)
        {
            int newTileNo;

            if (tiles[tileNo][xOffset] < boundingBoxTrimMargin)
            {
                newTileNo = Drawing.DecXtileNo(tiles[tileNo][xAxis], zoom);
                if (newTileNo != boundingBox[xAxis][^1])
                {
                    boundingBox[xAxis].Insert(0, newTileNo); // Only extend if not already using all tiles available in x axis
                }
            }
        }

        #endregion

        #region Download OSM tiles region

        static internal void DownloadOSMtile(int xTileNo, int yTileNo, int zoom, string filename)
        {
            string imagePath = $"{Path.GetDirectoryName(Parameters.SaveLocation)}\\images\\";
            string url = $"{tileServer}{zoom}/{xTileNo}/{yTileNo}.png{rapidapiKey}";
            HttpRoutines.GetWebDoc(url, Path.GetDirectoryName(Parameters.SaveLocation), $"{imagePath}{filename}");
        }

        static internal void DownloadOSMtileColumn(int xTileNo, int xIndex, List<List<int>> boundingBox, int zoom, string filename)
        {
            for (int yIndex = 0; yIndex < boundingBox[yAxis].Count; yIndex++)
            {
                DownloadOSMtile(xTileNo, boundingBox[yAxis][yIndex], zoom, $"{filename}_{xIndex}_{yIndex}.png");
            }
        }

        static internal void DownloadOSMtileRow(int yTileNo, int yIndex, List<List<int>> boundingBox, int zoom, string filename)
        {
            for (int xIndex = 0; xIndex < boundingBox[xAxis].Count; xIndex++)
            {
                DownloadOSMtile(boundingBox[xAxis][xIndex], yTileNo, zoom, $"{filename}_{xIndex}_{yIndex}.png");
            }
        }

        // Finds OSM tile numbers and offsets for a sinle coordinate for one zoom level
        // Tile number calculated using https://wiki.openstreetmap.org/wiki/Slippy_map_tilenames
        // $"{tileServer}{zoom}/{xTile}/{yTile}.png?{rapidapiKey}"
        static internal List<int> GetOSMtile(string sLon, string sLat, int zoom)
        {
            int xTile, yTile;
            if (LonToDecimalDegree(sLon, out double dLon) && LatToDecimalDegree(sLat, out double dLat))
            {
                xTile = LonToTileX(dLon, zoom, out int xOffset);
                yTile = LatToTileY(dLat, zoom, out int yOffset);
                return [xTile, yTile, xOffset, yOffset];
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

        internal static int LonToTileX(double dLon, int z, out int xOffset)
        {
            double doubleTileX = (dLon + 180.0) / 360.0 * (1 << z);
            int intTileX = Convert.ToInt32(Math.Floor(doubleTileX));
            xOffset = Convert.ToInt32(256 * (doubleTileX - intTileX));
            return Convert.ToInt32(Math.Floor(doubleTileX));
        }

        internal static int LatToTileY(double dLat, int z, out int yOffset)
        {
            var latRad = dLat / 180 * Math.PI;
            double doubleTileY = (1 - Math.Log(Math.Tan(latRad) + 1 / Math.Cos(latRad)) / Math.PI) / 2 * (1 << z);
            int intTileY = Convert.ToInt32(Math.Floor(doubleTileY));
            yOffset = Convert.ToInt32(256 * (doubleTileY - intTileY));
            return Convert.ToInt32(Math.Floor(doubleTileY));
        }

        /// <summary>
        /// Converts xTile/yTile/zoom combination to a latitude and longitude 
        /// </summary>
        /// <param name="xTile">The OSM xTile number</param>
        /// <param name="yTile">The OSM yTile number</param>
        /// <param name="zoom">The OSM zoom level</param>
        /// <returns>Latitude and longitude for top left corner of tile reference as +/- decimal degrees</returns>
        internal static List<double> TileNoToLatLon(int xTile, int yTile, int zoom)
        {
            // using formula from https://wiki.openstreetmap.org/wiki/Slippy_map_tilenames
            // note tile numbers run from 0 .. (2 pow zoom - 1), passing 2 pow zoom, 2 pow zoom gives bottom right of tile
            // (2 pow zoom - 1), (2 pow zoom - 1)
            List<double> latLonList = [];
            double n = Math.Pow(2, zoom);
            double latitudeRadians = Math.Atan(Math.Sinh(Math.PI * (1 - 2 * yTile / n)));
            latLonList.Add(latitudeRadians * 180.0 / Math.PI);
            latLonList.Add(xTile / n * 360.0 - 180.0);
            return latLonList;
        }

        #endregion
    }
}
