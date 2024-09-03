using ImageMagick;

namespace P3D_Scenario_Generator
{
    internal class Drawing
    {
        internal static int xAxis = 0, yAxis = 1; // Used in bounding box to denote lists that store xTile and yTile reference numbers
        internal static int xTile = 0, yTile = 1, xOffset = 2, yOffset = 3; // Used to define OSM tile, x and y numbers plus position of coord
        internal static int tileSize = 256; // All OSM tiles used in this program are 256x256 pixels
        internal static string imagePath = $"{Path.GetDirectoryName(Parameters.SaveLocation)}\\images\\";
        internal static int topLeft = 0, topRight = 1, bottomLeft = 2, bottomRight = 3; // Used to reference four subtiles of a tile

        #region Pad tiles region

        static internal List<List<int>> MakeSquare(List<List<int>> boundingBox, string filename, int zoom, int size)
        {
            // Get next tile East and West - allow for possibile wrap around meridian
            int newTileEast = IncXtileNo(boundingBox[xAxis][^1], zoom);
            int newTileWest = DecXtileNo(boundingBox[xAxis][0], zoom);
            // Get next tile South and North - don't go below bottom or top edge of map, -1 means no tile added that direction
            int newTileSouth = IncYtileNo(boundingBox[yAxis][^1], zoom);
            int newTileNorth = DecYtileNo(boundingBox[yAxis][0]);

            if (boundingBox[xAxis].Count < boundingBox[yAxis].Count) // Padding on the x axis
            {
                return PadWestEast(boundingBox, newTileWest, newTileEast, filename, zoom);
            }
            else if (boundingBox[yAxis].Count < boundingBox[xAxis].Count) // Padding on the y axis
            {
                if (newTileSouth < 0)
                {
                    return PadNorth(boundingBox, newTileNorth, filename, zoom);
                }
                else if (newTileNorth < 0)
                {
                    return PadSouth(boundingBox, newTileSouth, filename, zoom);
                }
                else
                {
                    return PadNorthSouth(boundingBox, newTileNorth, newTileSouth, filename, zoom);
                }
            }
            else if (boundingBox[yAxis].Count < size) // Padding on both axis 
            {
                return PadNorthSouthWestEast(boundingBox, newTileNorth, newTileSouth, newTileWest, newTileEast, filename, zoom);
            }
            return ZoomIn(boundingBox);
        }

        // Currently all points are on one tile but we want them on four tiles. Adjust boundingBox to include surrounding
        // eight tiles then download and montage them with current tile. Then crop centre area of four tiles.
        static internal List<List<int>> PadNorthSouthWestEast(List<List<int>> boundingBox, int newTileNorth, int newTileSouth,
            int newTileWest, int newTileEast, string filename, int zoom)
        {
            boundingBox[yAxis].Insert(0, newTileNorth);
            boundingBox[xAxis].Insert(0, newTileWest);
            boundingBox[xAxis].Add(newTileEast);
            boundingBox[yAxis].Add(newTileSouth);

            OSM.DownloadOSMtileRow(newTileSouth, 0, boundingBox, zoom, filename);
            OSM.DownloadOSMtile(newTileWest, boundingBox[yAxis][1], zoom, $"{filename}_0_1.png");
            File.Move($"{imagePath}{filename}.png", $"{imagePath}{filename}_1_1.png");
            OSM.DownloadOSMtile(newTileEast, boundingBox[yAxis][1], zoom, $"{filename}_2_1.png");
            OSM.DownloadOSMtileRow(newTileSouth, 2, boundingBox, zoom, filename);

            MontageTiles(boundingBox, zoom, filename);
            DeleteTempOSMfiles(filename);

            using MagickImage image = new($"{imagePath}{filename}.png");
            IMagickGeometry geometry = new MagickGeometry($"{tileSize * 2},{tileSize * 2}, {tileSize / 2}, {tileSize / 2}");
            image.Crop(geometry);
            image.RePage();
            image.Write($"{imagePath}{filename}.png");

            return ZoomInNorthSouthWestEast(boundingBox);
        }

        static internal List<List<int>> ZoomInNorthSouthWestEast(List<List<int>> boundingBox)
        {
            List<List<int>> zoomInBoundingBox = [];
            zoomInBoundingBox[xAxis][0] = 2 * boundingBox[xAxis][0] + 1;
            for (int xIndex = 1; xIndex < boundingBox[xAxis].Count - 1; xIndex++)
            {
                zoomInBoundingBox[xAxis].Add(2 * boundingBox[xAxis][xIndex]);
                zoomInBoundingBox[xAxis].Add(2 * boundingBox[xAxis][xIndex] + 1);
            }
            zoomInBoundingBox[xAxis].Add(2 * boundingBox[xAxis][^1]);

            zoomInBoundingBox[yAxis][0] = 2 * boundingBox[yAxis][0] + 1;
            for (int xIndex = 1; xIndex < boundingBox[yAxis].Count - 1; xIndex++)
            {
                zoomInBoundingBox[yAxis].Add(2 * boundingBox[yAxis][xIndex]);
                zoomInBoundingBox[yAxis].Add(2 * boundingBox[yAxis][xIndex] + 1);
            }
            zoomInBoundingBox[yAxis].Add(2 * boundingBox[yAxis][^1]);
            return zoomInBoundingBox;
        }

        // The file to be padded is 1w x 2h tileSize. Create a column of tiles on left and right side 1w x 2h,
        // montage them together (3w x 2h) then crop a column 0.5w x 2h from outside edges. Resulting image is 2w x 2h tileSize
        // with original image in middle horizontally.)
        static internal List<List<int>> PadWestEast(List<List<int>> boundingBox, int newTileWest, int newTileEast, string filename, int zoom)
        {
            OSM.DownloadOSMtileColumn(newTileWest, 0, boundingBox, zoom, filename);
            MontageTilesToColumn(boundingBox[yAxis].Count, 0, filename);
            DeleteTempOSMfiles($"{filename}_?");

            File.Move($"{imagePath}{filename}.png", $"{imagePath}{filename}_1.png");

            OSM.DownloadOSMtileColumn(newTileEast, 2, boundingBox, zoom, filename);
            MontageTilesToColumn(boundingBox[yAxis].Count, 2, filename);
            DeleteTempOSMfiles($"{filename}_?");

            MontageColumns(3, boundingBox[yAxis].Count, filename);
            DeleteTempOSMfiles(filename);

            using MagickImage image = new($"{imagePath}{filename}.png");
            IMagickGeometry geometry = new MagickGeometry($"{tileSize * 2},{tileSize * 2}, {tileSize / 2}, 0");
            image.Crop(geometry);
            image.RePage();
            image.Write($"{imagePath}{filename}.png");

            return ZoomInWestEast(boundingBox);
        }

        static internal List<List<int>> ZoomInWestEast(List<List<int>> boundingBox)
        {
            List<List<int>> zoomInBoundingBox = [];
            zoomInBoundingBox[xAxis][0] = 2 * boundingBox[xAxis][0] + 1;
            for (int xIndex = 1; xIndex < boundingBox[xAxis].Count - 1; xIndex++)
            {
                zoomInBoundingBox[xAxis].Add(2 * boundingBox[xAxis][xIndex]);
                zoomInBoundingBox[xAxis].Add(2 * boundingBox[xAxis][xIndex] + 1);
            }
            zoomInBoundingBox[xAxis].Add(2 * boundingBox[xAxis][^1]);

            for (int xIndex = 0; xIndex < boundingBox[yAxis].Count; xIndex++)
            {
                zoomInBoundingBox[yAxis].Add(2 * boundingBox[yAxis][xIndex]);
                zoomInBoundingBox[yAxis].Add(2 * boundingBox[yAxis][xIndex] + 1);
            }
            return zoomInBoundingBox;
        }

        // The file to be padded is 2w x 1h tileSize. Create a row of tiles above and below 2w x 1h,
        // montage them together (2w x 3h) then crop a row 2w x 0.5h from outside edges. Resulting image is 2w x 2h tileSize
        // with original image in middle vertically.
        static internal List<List<int>> PadNorthSouth(List<List<int>> boundingBox, int newTileNorth, int newTileSouth, string filename, int zoom)
        {
            OSM.DownloadOSMtileRow(newTileNorth, 0, boundingBox, zoom, filename);
            MontageTilesToRow(boundingBox[xAxis].Count, 0, filename);
            DeleteTempOSMfiles($"{filename}_?");

            File.Move($"{imagePath}{filename}.png", $"{imagePath}{filename}_1.png");

            OSM.DownloadOSMtileRow(newTileSouth, 2, boundingBox, zoom, filename);
            MontageTilesToRow(boundingBox[xAxis].Count, 2, filename);
            DeleteTempOSMfiles($"{filename}_?");

            MontageRows(boundingBox[xAxis].Count, 3, filename);
            DeleteTempOSMfiles(filename);

            using MagickImage image = new($"{imagePath}{filename}.png");
            IMagickGeometry geometry = new MagickGeometry($"{tileSize * 2},{tileSize * 2}, 0, {tileSize / 2}");
            image.Crop(geometry);
            image.RePage();
            image.Write($"{imagePath}{filename}.png");

            return ZoomInNorthSouth(boundingBox);
        }

        static internal List<List<int>> ZoomInNorthSouth(List<List<int>> boundingBox)
        {
            List<List<int>> zoomInBoundingBox = [];
            for (int xIndex = 0; xIndex < boundingBox[xAxis].Count; xIndex++)
            {
                zoomInBoundingBox[xAxis].Add(2 * boundingBox[xAxis][xIndex]);
                zoomInBoundingBox[xAxis].Add(2 * boundingBox[xAxis][xIndex] + 1);
            }

            zoomInBoundingBox[yAxis][0] = 2 * boundingBox[yAxis][0] + 1;
            for (int xIndex = 1; xIndex < boundingBox[yAxis].Count - 1; xIndex++)
            {
                zoomInBoundingBox[yAxis].Add(2 * boundingBox[yAxis][xIndex]);
                zoomInBoundingBox[yAxis].Add(2 * boundingBox[yAxis][xIndex] + 1);
            }
            zoomInBoundingBox[yAxis].Add(2 * boundingBox[yAxis][^1]);
            return zoomInBoundingBox;
        }

        // The file to be padded is 2w x 1h tileSize. Create a row of tiles above 2w x 1h, montage them together.
        // Resulting image is 2w x 2h tileSize with original image at bottom vertically.
        static internal List<List<int>> PadNorth(List<List<int>> boundingBox, int newTileNorth, string filename, int zoom)
        {
            OSM.DownloadOSMtileRow(newTileNorth, 0, boundingBox, zoom, filename);
            MontageTilesToRow(boundingBox[xAxis].Count, 0, filename);
            DeleteTempOSMfiles($"{filename}_?");

            File.Move($"{imagePath}{filename}.png", $"{imagePath}{filename}_1.png");

            MontageRows(boundingBox[xAxis].Count, 2, filename);
            DeleteTempOSMfiles(filename);

            return ZoomInNorthOrSouth(boundingBox);
        }

        static internal List<List<int>> ZoomInNorthOrSouth(List<List<int>> boundingBox)
        {
            List<List<int>> zoomInBoundingBox = [];
            for (int xIndex = 0; xIndex < boundingBox[xAxis].Count; xIndex++)
            {
                zoomInBoundingBox[xAxis].Add(2 * boundingBox[xAxis][xIndex]);
                zoomInBoundingBox[xAxis].Add(2 * boundingBox[xAxis][xIndex] + 1);
            }

            for (int xIndex = 0; xIndex < boundingBox[yAxis].Count; xIndex++)
            {
                zoomInBoundingBox[yAxis].Add(2 * boundingBox[yAxis][xIndex]);
                zoomInBoundingBox[yAxis].Add(2 * boundingBox[yAxis][xIndex] + 1);
            }
            return zoomInBoundingBox;
        }

        // The file to be padded is 2w x 1h tileSize. Create a row of tiles below 2w x 1h, montage them together.
        // Resulting image is 2w x 2h tileSize with original image at top vertically.
        static internal List<List<int>> PadSouth(List<List<int>> boundingBox, int newTileSouth, string filename, int zoom)
        {
            File.Move($"{imagePath}{filename}.png", $"{imagePath}{filename}_0.png");

            OSM.DownloadOSMtileRow(newTileSouth, 1, boundingBox, zoom, filename);
            MontageTilesToRow(boundingBox[xAxis].Count, 1, filename);
            DeleteTempOSMfiles($"{filename}_?");

            MontageRows(boundingBox[xAxis].Count, 2, filename);
            DeleteTempOSMfiles(filename);

            return ZoomInNorthOrSouth(boundingBox);
        }

        static internal List<List<int>> ZoomIn(List<List<int>> boundingBox)
        {
            List<List<int>> zoomInBoundingBox = [];
            for (int xIndex = 0; xIndex < boundingBox[xAxis].Count; xIndex++)
            {
                zoomInBoundingBox[xAxis].Add(2 * boundingBox[xAxis][xIndex]);
                zoomInBoundingBox[xAxis].Add(2 * boundingBox[xAxis][xIndex] + 1);
            }

            for (int xIndex = 0; xIndex < boundingBox[yAxis].Count; xIndex++)
            {
                zoomInBoundingBox[yAxis].Add(2 * boundingBox[yAxis][xIndex]);
                zoomInBoundingBox[yAxis].Add(2 * boundingBox[yAxis][xIndex] + 1);
            }
            return zoomInBoundingBox;
        }

        #endregion

        #region Drawing routines region

        static internal void DrawRoute(List<List<int>> tiles, List<List<int>> boundingBox, string filename)
        {
            using MagickImage image = new($"{imagePath}{filename}.png");
            DrawableStrokeColor strokeColor = new(new MagickColor("blue"));
            DrawableStrokeWidth stokeWidth = new(1);
            DrawableFillColor fillColor = new(MagickColors.Transparent);

            int centrePrevX = 0, centrePrevY = 0;
            for (int tileNo = 0; tileNo < tiles.Count; tileNo++)
            {
                int centreX = (boundingBox[xAxis].IndexOf(tiles[tileNo][xTile]) * tileSize) + tiles[tileNo][xOffset];
                int centreY = (boundingBox[yAxis].IndexOf(tiles[tileNo][yTile]) * tileSize) + tiles[tileNo][yOffset];
                if (tileNo > 0)
                {
                    DrawableLine line = new(centrePrevX, centrePrevY, centreX, centreY);
                    image.Draw(strokeColor, stokeWidth, fillColor, line);
                }
                centrePrevX = centreX;
                centrePrevY = centreY;
            }

            image.Write($"{imagePath}{filename}.png");
        }

        #endregion

        #region Montage tiles region

        static internal void MontageTilesToColumn(int yCount, int xIndex, string filename)
        {
            using var images = new MagickImageCollection();
            var settings = new MontageSettings
            {
                Geometry = new MagickGeometry($"{tileSize}x{tileSize}"),
                TileGeometry = new MagickGeometry($"1x{yCount}"),
            };
            for (int yIndex = 0; yIndex < yCount; yIndex++)
            {
                var tileImage = new MagickImage($"{imagePath}{filename}_{xIndex}_{yIndex}.png");
                images.Add(tileImage);
            }
            using var result = images.Montage(settings);
            result.Write($"{imagePath}{filename}_{xIndex}.png");
        }

        static internal void MontageTilesToRow(int xCount, int yIndex, string filename)
        {
            using var images = new MagickImageCollection();
            var settings = new MontageSettings
            {
                Geometry = new MagickGeometry($"{tileSize}x{tileSize}"),
                TileGeometry = new MagickGeometry($"{xCount}x1"),
            };
            for (int xIndex = 0; xIndex < xCount; xIndex++)
            {
                var tileImage = new MagickImage($"{imagePath}{filename}_{xIndex}_{yIndex}.png");
                images.Add(tileImage);
            }
            using var result = images.Montage(settings);
            result.Write($"{imagePath}{filename}_{yIndex}.png");
        }

        static internal void MontageColumns(int xCount, int yCount, string filename)
        {
            using var images = new MagickImageCollection();
            var settings = new MontageSettings
            {
                Geometry = new MagickGeometry($"{tileSize}x{tileSize * yCount}"),
                TileGeometry = new MagickGeometry($"{xCount}x1"),
            };
            for (int xIndex = 0; xIndex < xCount; xIndex++)
            {
                var tileImage = new MagickImage($"{imagePath}{filename}_{xIndex}.png");
                images.Add(tileImage);
            }
            using var result = images.Montage(settings);
            result.Write($"{imagePath}{filename}.png");
        }

        static internal void MontageRows(int xCount, int yCount, string filename)
        {
            using var images = new MagickImageCollection();
            var settings = new MontageSettings
            {
                Geometry = new MagickGeometry($"{tileSize * xCount}x{tileSize}"),
                TileGeometry = new MagickGeometry($"1x{yCount}"),
            };
            for (int yIndex = 0; yIndex < yCount; yIndex++)
            {
                var tileImage = new MagickImage($"{imagePath}{filename}_{yIndex}.png");
                images.Add(tileImage);
            }
            using var result = images.Montage(settings);
            result.Write($"{imagePath}{filename}.png");
        }

        static internal void MontageTiles(List<List<int>> boundingBox, int zoom, string filename)
        {
            // Download the tile images from OSM in columns and montage into strips
            for (int xIndex = 0; xIndex < boundingBox[xAxis].Count; xIndex++)
            {
                OSM.DownloadOSMtileColumn(boundingBox[xAxis][xIndex], xIndex, boundingBox, zoom, $"{filename}");
                MontageTilesToColumn(boundingBox[yAxis].Count, xIndex, filename);
            }

            // Montage the OSM column strips to form the final image
            MontageColumns(boundingBox[xAxis].Count, boundingBox[yAxis].Count, filename);

            DeleteTempOSMfiles(filename);
        }

        #endregion

        #region Utilities section

        static internal void ConvertImageformat(string filename, string oldExt, string newExt)
        {
            using MagickImage image = new($"{imagePath}{filename}.{oldExt}");
            switch (newExt)
            {
                case "jpg":
                    image.Quality = 100;
                    break;
            }
            image.Write($"{imagePath}{filename}.{newExt}");
            File.Delete($"{imagePath}{filename}.{oldExt}");
        }

        static internal void DeleteTempOSMfiles(string filename)
        {
            foreach (string f in Directory.EnumerateFiles(imagePath, $"{filename}_*.png"))
            {
                File.Delete(f);
            }
        }

        static internal int DecXtileNo(int tileNo, int zoom)
        {
            int newTileNo = tileNo - 1;
            if (newTileNo == -1)
            {
                newTileNo = Convert.ToInt32(Math.Pow(2, zoom)) - 1;
            }
            return newTileNo;
        }

        static internal int DecYtileNo(int tileNo)
        {
            int newTileNo = -1;
            if (tileNo - 1 > 0)
            {
                newTileNo = tileNo - 1;
            }
            return newTileNo;
        }

        static internal int IncXtileNo(int tileNo, int zoom)
        {
            int newTileNo = tileNo + 1;
            if (newTileNo == Convert.ToInt32(Math.Pow(2, zoom)))
            {
                newTileNo = 0;
            }
            return newTileNo;
        }

        static internal int IncYtileNo(int tileNo, int zoom)
        {
            int newTileNo = -1;
            if (tileNo + 1 < Convert.ToInt32(Math.Pow(2, zoom)) - 1)
            {
                newTileNo = tileNo + 1;
            }
            return newTileNo;
        }

        #endregion
    }
}
