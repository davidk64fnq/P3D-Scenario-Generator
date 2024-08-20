using ImageMagick;

namespace P3D_Scenario_Generator
{
    internal class Drawing
    {
        internal static int xAxis = 0, yAxis = 1;
        internal static int xTile = 0, yTile = 1, xOffset = 2, yOffset = 3;
        internal static int tileSize = 256;
        internal static string imagePath = $"{Path.GetDirectoryName(Parameters.SaveLocation)}\\images\\";

        // The file to be padded is 1w x 2h tileSize. Create a column of tiles on left and right side 1w x 2h,
        // montage them together (3w x 2h) then crop a column 0.5w x 2h from outside edges. Resulting image is 2w x 2h tileSize
        // with original image in middle horizontally.
        static internal void PadWestEast(List<List<int>> boundingBox, int newTileWest, int newTileEast, string filename, int zoom)
        {
            OSM.DownloadOSMtileColumn(newTileWest, 0, boundingBox, zoom, filename);
            MontageTilesToColumn(boundingBox[yAxis].Count, 0, filename);
            DeleteTempOSMfiles(filename);

            File.Move($"{imagePath}{filename}.png", $"{imagePath}{filename}_1.png");

            OSM.DownloadOSMtileColumn(newTileEast, 2, boundingBox, zoom, filename);
            MontageTilesToColumn(boundingBox[yAxis].Count, 2, filename);
            DeleteTempOSMfiles(filename);

            MontageColumns(3, boundingBox[yAxis].Count, filename);
            DeleteTempOSMfiles(filename);

            using MagickImage image = new($"{imagePath}{filename}.png");
            image.Crop(tileSize / 2, tileSize, Gravity.West);
            image.Crop(tileSize / 2, tileSize, Gravity.East);
        }

        // The file to be padded is 2w x 1h tileSize. Create a row of tiles above and below 2w x 1h,
        // montage them together (2w x 3h) then crop a row 2w x 0.5h from outside edges. Resulting image is 2w x 2h tileSize
        // with original image in middle vertically.
        static internal void PadNorthSouth(List<List<int>> boundingBox, int newTileNorth, int newTileSouth, string filename, int zoom)
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
            IMagickGeometry geometry = new MagickGeometry($"{tileSize * 2},{tileSize / 2}");
            image.Crop(geometry);
            geometry = new MagickGeometry($"{tileSize * 2},{tileSize / 2}, 0, {-tileSize / 2}");
            image.Crop(geometry);
            image.RePage();
            image.Write($"{imagePath}{filename}.png");
        }

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
                DrawableCircle circle = new(centreX, centreY, centreX, centreY + 10);
                image.Draw(strokeColor, stokeWidth, fillColor, circle);
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

        static internal void DeleteTempOSMfiles(string filename)
        {
            foreach (string f in Directory.EnumerateFiles(imagePath, $"{filename}_*.png"))
            {
                File.Delete(f);
            }
        }
    }
}
