using CoordinateSharp;
using System.Collections.Generic;

namespace P3D_Scenario_Generator
{
    /// <summary>
    /// Provides methods for calculating OpenStreetMap (OSM) tile information
    /// and bounding boxes based on geographic coordinates.
    /// </summary>
    public static class MapTileCalculator
    {
        /// <summary>
        /// Works out the most zoomed-in level that includes all specified coordinates,
        /// where the montage of OSM tiles doesn't exceed the given width and height.
        /// </summary>
        /// <param name="coordinates">A list of geographic coordinates to be covered by the tiles.</param>
        /// <param name="tilesWidth">Maximum number of tiles allowed for the X-axis.</param>
        /// <param name="tilesHeight">Maximum number of tiles allowed for the Y-axis.</param>
        /// <returns>The maximum zoom level that meets the constraints.</returns>
        public static int GetOptimalZoomLevel(IEnumerable<Coordinate> coordinates, int tilesWidth, int tilesHeight)
        {
            List<Tile> tempTiles = [];

            for (int zoom = 2; zoom <= Constants.maxZoomLevel; zoom++)
            {
                tempTiles.Clear();
                SetOSMTilesForCoordinates(tempTiles, zoom, coordinates);

                // Create a BoundingBox instance and populate its Lists from unique tile indices
                BoundingBox boundingBox = MapTileBoundingBoxCalculator.GetBoundingBox(tempTiles, zoom);

                // The Count property works fine for List<T>
                if ((boundingBox.XAxis.Count > tilesWidth) || (boundingBox.YAxis.Count > tilesHeight))
                {
                    return zoom - 1;
                }
            }
            return Constants.maxZoomLevel;
        }

        /// <summary>
        /// Populates a list with unique OpenStreetMap (OSM) tile references for a given set of geographic coordinates at a specified zoom level.
        /// </summary>
        /// <param name="tiles">The list to be populated with the calculated unique OSM tile references (XIndex, YIndex).</param>
        /// <param name="zoom">The OSM tile zoom level for which the tiles are determined.</param>
        /// <param name="coordinates">A collection of geographic coordinates for which the covering tiles are determined.</param>
        public static void SetOSMTilesForCoordinates(List<Tile> tiles, int zoom, IEnumerable<Coordinate> coordinates)
        {
            // Using a HashSet for efficient tracking of unique tiles during population.
            // Because Tile is now a class, I've added custom Equals/GetHashCode to Tile.cs
            // for HashSet to work correctly based on value (XIndex, YIndex) not reference.
            HashSet<Tile> uniqueTiles = [];

            foreach (var coord in coordinates)
            {
                Tile tile = GetTileInfo(coord.Longitude.DecimalDegree, coord.Latitude.DecimalDegree, zoom);
                uniqueTiles.Add(tile);
            }

            // After populating the HashSet, clear the provided list and add all unique tiles to it.
            tiles.Clear();
            tiles.AddRange(uniqueTiles);
        }

        /// <summary>
        /// Finds OSM tile numbers and offsets for a single coordinate for one zoom level.
        /// Tile number calculated using https://wiki.openstreetmap.org/wiki/Slippy_map_tilenames.
        /// </summary>
        /// <param name="sLon">The longitude value as a string.</param>
        /// <param name="sLat">The latitude value as a string.</param>
        /// <param name="zoom">The zoom level.</param>
        /// <returns>A Tile object if conversion is successful, otherwise null.</returns>
        public static Tile GetOSMtile(string sLon, string sLat, int zoom) 
        {
            // Tile is now a class, so it can be null.
            // Initialize with default values, which will then be set by LonToTileX and LatToTileY.
            Tile tile = new();

            if (LonToDecimalDegree(sLon, out double dLon) && LatToDecimalDegree(sLat, out double dLat))
            {
                LonToTileX(dLon, zoom, tile);
                LatToTileY(dLat, zoom, tile);
                return tile;
            }
            return null; // Return null if conversion fails.
        }

        /// <summary>
        /// Finds OSM tile numbers and offsets for a single coordinate for one zoom level.
        /// This version takes decimal degrees directly.
        /// </summary>
        /// <param name="dLon">The longitude in decimal degrees.</param>
        /// <param name="dLat">The latitude in decimal degrees.</param>
        /// <param name="zoom">The zoom level.</param>
        /// <returns>A Tile object containing the XIndex, YIndex, XOffset, and YOffset.</returns>
        public static Tile GetTileInfo(double dLon, double dLat, int zoom)
        {
            // Create a new Tile instance and populate it
            Tile tile = new();
            LonToTileX(dLon, zoom, tile);
            LatToTileY(dLat, zoom, tile);
            return tile;
        }

        /// <summary>
        /// Uses CoordinateSharp library to convert longitude of point to decimal degrees.
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
        /// Uses CoordinateSharp library to convert latitude of point to decimal degrees.
        /// </summary>
        /// <param name="sLat">The latitude value in format other than decimal degrees.</param>
        /// <param name="dLat">The latitude value converted to decimal degrees.</param>
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
            tile.XOffset = Convert.ToInt32(Constants.tileSize * (doubleTileX - tile.XIndex));
        }

        /// <summary>
        /// Converts latitude in decimal degrees to yTile number for OSM tiles at given zoom level. Works
        /// out the yOffset amount of the latitude value on the OSM tile. Uses formulae found at
        /// https://wiki.openstreetmap.org/wiki/Slippy_map_tilenames.
        /// </summary>
        /// <param name="dLat">The latitude value converted to decimal degrees.</param>
        /// <param name="z">The specified zoom level.</param>
        /// <param name="tile">Stores the yTile and yOffset values for the specified latitude at specified zoom level.</param>
        internal static void LatToTileY(double dLat, int z, Tile tile)
        {
            var latRad = dLat / 180 * Math.PI;
            double doubleTileY = (1 - Math.Log(Math.Tan(latRad) + 1 / Math.Cos(latRad)) / Math.PI) / 2 * (1 << z);
            tile.YIndex = Convert.ToInt32(Math.Floor(doubleTileY));
            tile.YOffset = Convert.ToInt32(Constants.tileSize * (doubleTileY - tile.YIndex));
        }

        /// <summary>
        /// Decrements an OpenStreetMap X-tile number (longitude), handling the
        /// wrapping around the Earth. If decrementing results in a negative value,
        /// it wraps to the highest X-tile number for the given zoom level.
        /// </summary>
        /// <param name="tileNo">The current X-tile number.</param>
        /// <param name="zoom">The current zoom level.</param>
        /// <returns>The decremented X-tile number, wrapped if necessary.</returns>
        public static int DecXtileNo(int tileNo, int zoom)
        {
            int newTileNo = tileNo - 1;
            if (newTileNo == -1)
            {
                newTileNo = (1 << zoom) - 1;
            }
            return newTileNo;
        }

        /// <summary>
        /// Decrements an OpenStreetMap Y-tile number (latitude).
        /// This method does not wrap around poles; it returns -1 if decrementing
        /// would go beyond the valid northern boundary (Y-tile 0).
        /// </summary>
        /// <param name="tileNo">The current Y-tile number.</param>
        /// <returns>The decremented Y-tile number, or -1 if the northern boundary is reached.</returns>
        public static int DecYtileNo(int tileNo)
        {
            if (tileNo - 1 >= 0)
            {
                return tileNo - 1;
            }
            return -1;
        }

        /// <summary>
        /// Increments an OpenStreetMap X-tile number (longitude), handling the
        /// wrapping around the Earth. If incrementing exceeds the maximum X-tile number,
        /// it wraps to X-tile 0 for the given zoom level.
        /// </summary>
        /// <param name="tileNo">The current X-tile number.</param>
        /// <param name="zoom">The current zoom level.</param>
        /// <returns>The incremented X-tile number, wrapped if necessary.</returns>
        public static int IncXtileNo(int tileNo, int zoom)
        {
            int newTileNo = tileNo + 1;
            if (newTileNo == (1 << zoom))
            {
                newTileNo = 0;
            }
            return newTileNo;
        }

        /// <summary>
        /// Increments an OpenStreetMap Y-tile number (latitude).
        /// This method does not wrap around poles; it returns -1 if incrementing
        /// would go beyond the valid southern boundary.
        /// </summary>
        /// <param name="tileNo">The current Y-tile number.</param>
        /// <param name="zoom">The current zoom level.</param>
        /// <returns>The incremented Y-tile number, or -1 if the southern boundary is reached.</returns>
        public static int IncYtileNo(int tileNo, int zoom)
        {
            if (tileNo + 1 < (1 << zoom))
            {
                return tileNo + 1;
            }
            return -1;
        }

        /// <summary>
        /// Converts xTile/yTile/zoom combination to a latitude and longitude.
        /// </summary>
        /// <param name="xTile">The OSM xTile number.</param>
        /// <param name="yTile">The OSM yTile number.</param>
        /// <param name="zoom">The OSM zoom level.</param>
        /// <returns>Latitude and longitude for top left corner of tile reference as +/- decimal degrees.</returns>
        public static Coordinate TileNoToLatLon(int xTile, int yTile, int zoom)
        {
            // using formula from https://wiki.openstreetmap.org/wiki/Slippy_map_tilenames
            // note tile numbers run from 0 .. (2 pow zoom - 1), passing 2 pow zoom, 2 pow zoom gives bottom right of tile
            // (2 pow zoom - 1), (2 pow zoom - 1)
            double n = Math.Pow(2, zoom);
            double latitudeRadians = Math.Atan(Math.Sinh(Math.PI * (1 - 2 * yTile / n)));
            Coordinate c = new(latitudeRadians * 180.0 / Math.PI, xTile / n * 360.0 - 180.0);
            return c;
        }
    }
}