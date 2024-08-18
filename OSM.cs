using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoordinateSharp;

namespace P3D_Scenario_Generator
{
    internal class OSM
    {
        readonly static string tileServer = "https://maptiles.p.rapidapi.com/en/map/v1/";
        readonly static string rapidapiKey = "rapidapi-key=d9de94c22emsh6dc07cd7103e683p12be01jsn7014f38e1975";
        internal static int xAxis = 0, yAxis = 1;
        internal static int xTile = 0, yTile = 1, xOffset = 2, yOffset = 3;


        // The bounding box is two lists of tile numbers, one for x axis the other y axis. The tile numbers
        // will usually be consecutive within the bounds 0 .. (2 to exp zoom - 1). However it's possible for tiles grouped 
        // across the meridian to have a sequence of x axis tile numbers that goes up to (2 to exp zoom - 1) and then continues
        // from 0
        static internal void GetTilesBoundingBox(List<List<int>> tiles, List<List<int>> BoundingBox, int zoom)
        {
            // Initialise BoundingBox to the first tile
            List<int> xAxis = [tiles[0][xTile]];
            BoundingBox.Add(xAxis);
            List<int> yAxis = [tiles[0][yTile]];
            BoundingBox.Add(yAxis);

            // Adjust BoundingBox as needed to include remaining tiles
            for (int tileNo = 1; tileNo < tiles.Count; tileNo++)
            {
                AddTileToBoundingBox(tiles[tileNo], BoundingBox, zoom);
            }
        }

        static internal void AddTileToBoundingBox(List<int> newTile, List<List<int>> BoundingBox, int zoom)
        {
            // New tile is above BB i.e. tileNo < BoundingBox[yAxis][0]
            if (newTile[yTile] < BoundingBox[yAxis][0])
            {
                // Insert extra tile No's at beginning of yAxis list
                for (int tileNo = BoundingBox[yAxis][0] - 1; tileNo >= newTile[yTile]; tileNo--)
                {
                    BoundingBox[yAxis].Insert(0, tileNo);
                }
            }

            // New tile is below BB i.e. tileNo > BoundingBox[yAxis][^1]
            if (newTile[yTile] > BoundingBox[yAxis][^1])
            {
                // Append extra tileNo's at end of yAxis list
                for (int tileNo = BoundingBox[yAxis][^1] + 1; tileNo <= newTile[yTile]; tileNo++)
                {
                    BoundingBox[yAxis].Add(tileNo);
                }
            }

            // New tile is right of BB i.e. tileNo > BoundingBox[xAxis][^1], determine whether to move righthand
            // side of bounding box further to the right (usual case) or lefthand side further to the left (across meridian)
            int distEast, distWest;
            if (newTile[xTile] > BoundingBox[xAxis][^1])
            {
                distEast = newTile[xTile] - BoundingBox[xAxis][^1];
                distWest = BoundingBox[xAxis][0] + Convert.ToInt32(Math.Pow(2, zoom)) - newTile[xTile];
                if (distEast <= distWest)
                {
                    // Append extra tileNo's at end of xAxis list
                    for (int tileNo = BoundingBox[xAxis][^1] + 1; tileNo <= newTile[xTile]; tileNo++)
                    {
                        BoundingBox[xAxis].Add(tileNo);
                    }
                }
                else
                {
                    // Insert extra tileNo's at beginning of xAxis list
                    for (int tileNo = BoundingBox[xAxis][0] - 1; tileNo >= 0; tileNo--)
                    {
                        BoundingBox[xAxis].Insert(0, tileNo);
                    }
                    for (int tileNo = Convert.ToInt32(Math.Pow(2, zoom)) - 1; tileNo >= newTile[xTile]; tileNo--)
                    {
                        BoundingBox[xAxis].Insert(0, tileNo);
                    }
                }
            }

            // New tile is left of BB i.e. tileNo < BoundingBox[xAxis][0], determine whether to move lefthand
            // side of bounding box further to the left (usual case) or righthand side further to the right (across meridian)
            if (newTile[xTile] < BoundingBox[xAxis][0])
            {
                distWest = BoundingBox[xAxis][0] - newTile[xTile];
                distEast = Convert.ToInt32(Math.Pow(2, zoom)) - BoundingBox[xAxis][0] + newTile[xTile];
                if (distWest <= distEast)
                {
                    // Insert extra tileNo's at front of xAxis list
                    for (int tileNo = BoundingBox[xAxis][0] - 1; tileNo >= newTile[xTile]; tileNo--)
                    {
                        BoundingBox[xAxis].Insert(0, tileNo);
                    }
                }
                else
                {
                    // Append extra tileNo's at end of xAxis list
                    for (int tileNo = BoundingBox[xAxis][^1] + 1; tileNo < Convert.ToInt32(Math.Pow(2, zoom)); tileNo++)
                    {
                        BoundingBox[xAxis].Add(tileNo);
                    }
                    for (int tileNo = 0; tileNo <= newTile[xTile]; tileNo++)
                    {
                        BoundingBox[xAxis].Add(tileNo);
                    }
                }
            }
        }

        // Finds OSM tile numbers and offsets for a sinle coordinate for one zoom level
        static internal List<int> SetOSMtile(string lon, string lat, int zoom)
        {
            if (GetOSMtileURL(lon, lat, zoom, out int xTile, out int yTile, out int xOffset, out int yOffset) != null)
            {
                return [xTile, yTile, xOffset, yOffset];
            }
            return null;
        }

        internal static string GetOSMtileURL(string sLon, string sLat, int zoom, out int xTile, out int yTile, out int xOffset, out int yOffset)
        {
            if (LonToDecimalDegree(sLon, out double dLon) && LatToDecimalDegree(sLat, out double dLat))
            {
                xTile = Long2tileX(dLon, zoom, out xOffset);
                yTile = Lat2tileY(dLat, zoom, out yOffset);
                return $"{tileServer}{zoom}/{xTile}/{yTile}.png?{rapidapiKey}";
            }
            xTile = 0; yTile = 0; xOffset = 0; yOffset = 0;
            return "";
        }

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

        internal static int Long2tileX(double dLon, int z, out int xOffset)
        {
            double doubleTileX = (dLon + 180.0) / 360.0 * (1 << z);
            int intTileX = Convert.ToInt32(Math.Floor(doubleTileX));
            xOffset = Convert.ToInt32(256 * (doubleTileX - intTileX));
            return Convert.ToInt32(Math.Floor(doubleTileX));
        }

        internal static int Lat2tileY(double dLat, int z, out int yOffset)
        {
            var latRad = dLat / 180 * Math.PI;
            double doubleTileY = (1 - Math.Log(Math.Tan(latRad) + 1 / Math.Cos(latRad)) / Math.PI) / 2 * (1 << z);
            int intTileY = Convert.ToInt32(Math.Floor(doubleTileY));
            yOffset = Convert.ToInt32(256 * (doubleTileY - intTileY));
            return Convert.ToInt32(Math.Floor(doubleTileY));
        }

    }
}
