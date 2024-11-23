using ImageMagick;
using ImageMagick.Drawing;
using ImageMagick.Colors;
using System.Reflection;

namespace P3D_Scenario_Generator
{
    internal class Drawing
    {
        #region Pad tiles region

        static internal BoundingBox MakeSquare(BoundingBox boundingBox, string filename, int zoom, int size)
        {
            // Get next tile East and West - allow for possibile wrap around meridian
            int newTileEast = IncXtileNo(boundingBox.xAxis[^1], zoom);
            int newTileWest = DecXtileNo(boundingBox.xAxis[0], zoom);
            // Get next tile South and North - don't go below bottom or top edge of map, -1 means no tile added that direction
            int newTileSouth = IncYtileNo(boundingBox.yAxis[^1], zoom);
            int newTileNorth = DecYtileNo(boundingBox.yAxis[0]);

            if (boundingBox.xAxis.Count < boundingBox.yAxis.Count) // Padding on the x axis
            {
                return PadWestEast(boundingBox, newTileWest, newTileEast, filename, zoom);
            }
            else if (boundingBox.yAxis.Count < boundingBox.xAxis.Count) // Padding on the y axis
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
            else if (boundingBox.yAxis.Count < size) // Padding on both axis 
            {
                return PadNorthSouthWestEast(boundingBox, newTileNorth, newTileSouth, newTileWest, newTileEast, filename, zoom);
            }
            return ZoomIn(boundingBox);
        }

        // Currently all points are on one tile but we want them on four tiles. Adjust boundingBox to include surrounding
        // eight tiles then download and montage them with current tile. Then crop centre area of four tiles.
        static internal BoundingBox PadNorthSouthWestEast(BoundingBox boundingBox, int newTileNorth, int newTileSouth,
            int newTileWest, int newTileEast, string filename, int zoom)
        {
            boundingBox.yAxis.Insert(0, newTileNorth);
            boundingBox.xAxis.Insert(0, newTileWest);
            boundingBox.xAxis.Add(newTileEast);
            boundingBox.yAxis.Add(newTileSouth);

            OSM.DownloadOSMtileRow(newTileSouth, 0, boundingBox, zoom, filename);
            OSM.DownloadOSMtile(newTileWest, boundingBox.yAxis[1], zoom, $"{filename}_0_1.png");
            File.Move($"{Parameters.ImageFolder}\\{filename}.png", $"{Parameters.ImageFolder}\\{filename}_1_1.png");
            OSM.DownloadOSMtile(newTileEast, boundingBox.yAxis[1], zoom, $"{filename}_2_1.png");
            OSM.DownloadOSMtileRow(newTileSouth, 2, boundingBox, zoom, filename);

            MontageTiles(boundingBox, zoom, filename);
            DeleteTempOSMfiles(filename);

            using MagickImage image = new($"{Parameters.ImageFolder}\\{filename}.png");
            IMagickGeometry geometry = new MagickGeometry($"{Con.tileSize * 2},{Con.tileSize * 2}, {Con.tileSize / 2}, {Con.tileSize / 2}");
            image.Crop(geometry);
            image.ResetPage();
            image.Write($"{Parameters.ImageFolder}\\{filename}.png");

            return ZoomInNorthSouthWestEast(boundingBox);
        }

        static internal BoundingBox ZoomInNorthSouthWestEast(BoundingBox boundingBox)
        {
            BoundingBox zoomInBoundingBox = new();
            List<int> ewAxis = [];
            ewAxis.Add(2 * boundingBox.xAxis[0] + 1);
            for (int xIndex = 1; xIndex < boundingBox.xAxis.Count - 1; xIndex++)
            {
                ewAxis.Add(2 * boundingBox.xAxis[xIndex]);
                ewAxis.Add(2 * boundingBox.xAxis[xIndex] + 1);
            }
            ewAxis.Add(2 * boundingBox.xAxis[^1]);
            zoomInBoundingBox.xAxis = ewAxis;

            List<int> nsAxis = [];
            nsAxis.Add(2 * boundingBox.yAxis[0] + 1);
            for (int xIndex = 1; xIndex < boundingBox.yAxis.Count - 1; xIndex++)
            {
                nsAxis.Add(2 * boundingBox.yAxis[xIndex]);
                nsAxis.Add(2 * boundingBox.yAxis[xIndex] + 1);
            }
            nsAxis.Add(2 * boundingBox.yAxis[^1]);
            zoomInBoundingBox.yAxis = nsAxis;
            return zoomInBoundingBox;
        }

        // The file to be padded is 1w x 2h Con.tileSize. Create a column of tiles on left and right side 1w x 2h,
        // montage them together (3w x 2h) then crop a column 0.5w x 2h from outside edges. Resulting imageURL is 2w x 2h Con.tileSize
        // with original imageURL in middle horizontally.)
        static internal BoundingBox PadWestEast(BoundingBox boundingBox, int newTileWest, int newTileEast, string filename, int zoom)
        {
            OSM.DownloadOSMtileColumn(newTileWest, 0, boundingBox, zoom, filename);
            MontageTilesToColumn(boundingBox.yAxis.Count, 0, filename);
            DeleteTempOSMfiles($"{filename}_?");

            File.Move($"{Parameters.ImageFolder}\\{filename}.png", $"{Parameters.ImageFolder}\\{filename}_1.png");

            OSM.DownloadOSMtileColumn(newTileEast, 2, boundingBox, zoom, filename);
            MontageTilesToColumn(boundingBox.yAxis.Count, 2, filename);
            DeleteTempOSMfiles($"{filename}_?");

            MontageColumns(3, boundingBox.yAxis.Count, filename);
            DeleteTempOSMfiles(filename);

            using MagickImage image = new($"{Parameters.ImageFolder}\\{filename}.png");
            IMagickGeometry geometry = new MagickGeometry($"{Con.tileSize * 2},{Con.tileSize * 2}, {Con.tileSize / 2}, 0");
            image.Crop(geometry);
            image.ResetPage();
            image.Write($"{Parameters.ImageFolder}\\{filename}.png");

            return ZoomInWestEast(boundingBox);
        }

        static internal BoundingBox ZoomInWestEast(BoundingBox boundingBox)
        {
            BoundingBox zoomInBoundingBox = new();
            List<int> ewAxis = [];
            ewAxis.Add(2 * boundingBox.xAxis[0] - 1);
            for (int xIndex = 0; xIndex < boundingBox.xAxis.Count; xIndex++)
            {
                ewAxis.Add(2 * boundingBox.xAxis[xIndex]);
                ewAxis.Add(2 * boundingBox.xAxis[xIndex] + 1);
            }
            ewAxis.Add(2 * boundingBox.xAxis[^1] + 2);
            zoomInBoundingBox.xAxis = ewAxis;

            List<int> nsAxis = [];
            for (int xIndex = 0; xIndex < boundingBox.yAxis.Count; xIndex++)
            {
                nsAxis.Add(2 * boundingBox.yAxis[xIndex]);
                nsAxis.Add(2 * boundingBox.yAxis[xIndex] + 1);
            }
            zoomInBoundingBox.yAxis = nsAxis;
            return zoomInBoundingBox;
        }

        // The file to be padded is 2w x 1h Con.tileSize. Create a row of tiles above and below 2w x 1h,
        // montage them together (2w x 3h) then crop a row 2w x 0.5h from outside edges. Resulting imageURL is 2w x 2h Con.tileSize
        // with original imageURL in middle vertically.
        static internal BoundingBox PadNorthSouth(BoundingBox boundingBox, int newTileNorth, int newTileSouth, string filename, int zoom)
        {
            OSM.DownloadOSMtileRow(newTileNorth, 0, boundingBox, zoom, filename);
            MontageTilesToRow(boundingBox.xAxis.Count, 0, filename);
            DeleteTempOSMfiles($"{filename}_?");

            File.Move($"{Parameters.ImageFolder}\\{filename}.png", $"{Parameters.ImageFolder}\\{filename}_1.png");

            OSM.DownloadOSMtileRow(newTileSouth, 2, boundingBox, zoom, filename);
            MontageTilesToRow(boundingBox.xAxis.Count, 2, filename);
            DeleteTempOSMfiles($"{filename}_?");

            MontageRows(boundingBox.xAxis.Count, 3, filename);
            DeleteTempOSMfiles(filename);

            using MagickImage image = new($"{Parameters.ImageFolder}\\{filename}.png");
            IMagickGeometry geometry = new MagickGeometry($"{Con.tileSize * 2},{Con.tileSize * 2}, 0, {Con.tileSize / 2}");
            image.Crop(geometry);
            image.ResetPage();
            image.Write($"{Parameters.ImageFolder}\\{filename}.png");

            return ZoomInNorthSouth(boundingBox);
        }

        static internal BoundingBox ZoomInNorthSouth(BoundingBox boundingBox)
        {
            BoundingBox zoomInBoundingBox = new();
            List<int> ewAxis = [];
            for (int xIndex = 0; xIndex < boundingBox.xAxis.Count; xIndex++)
            {
                ewAxis.Add(2 * boundingBox.xAxis[xIndex]);
                ewAxis.Add(2 * boundingBox.xAxis[xIndex] + 1);
            }
            zoomInBoundingBox.xAxis = ewAxis;

            List<int> nsAxis = [];
            nsAxis.Add(2 * boundingBox.yAxis[0] - 1);
            for (int xIndex = 0; xIndex < boundingBox.yAxis.Count; xIndex++)
            {
                nsAxis.Add(2 * boundingBox.yAxis[xIndex]);
                nsAxis.Add(2 * boundingBox.yAxis[xIndex] + 1);
            }
            nsAxis.Add(2 * boundingBox.yAxis[^1] + 2);
            zoomInBoundingBox.yAxis = nsAxis;
            return zoomInBoundingBox;
        }

        // The file to be padded is 2w x 1h Con.tileSize. Create a row of tiles above 2w x 1h, montage them together.
        // Resulting imageURL is 2w x 2h Con.tileSize with original imageURL at bottom vertically.
        static internal BoundingBox PadNorth(BoundingBox boundingBox, int newTileNorth, string filename, int zoom)
        {
            OSM.DownloadOSMtileRow(newTileNorth, 0, boundingBox, zoom, filename);
            MontageTilesToRow(boundingBox.xAxis.Count, 0, filename);
            DeleteTempOSMfiles($"{filename}_?");

            File.Move($"{Parameters.ImageFolder}\\{filename}.png", $"{Parameters.ImageFolder}\\{filename}_1.png");

            MontageRows(boundingBox.xAxis.Count, 2, filename);
            DeleteTempOSMfiles(filename);

            return ZoomInNorthOrSouth(boundingBox);
        }

        static internal BoundingBox ZoomInNorthOrSouth(BoundingBox boundingBox)
        {
            BoundingBox zoomInBoundingBox = new();
            List<int> ewAxis = [];
            for (int xIndex = 0; xIndex < boundingBox.xAxis.Count; xIndex++)
            {
                ewAxis.Add(2 * boundingBox.xAxis[xIndex]);
                ewAxis.Add(2 * boundingBox.xAxis[xIndex] + 1);
            }
            zoomInBoundingBox.xAxis = ewAxis;

            List<int> nsAxis = [];
            for (int xIndex = 0; xIndex < boundingBox.yAxis.Count; xIndex++)
            {
                nsAxis.Add(2 * boundingBox.yAxis[xIndex]);
                nsAxis.Add(2 * boundingBox.yAxis[xIndex] + 1);
            }
            zoomInBoundingBox.yAxis = nsAxis;
            return zoomInBoundingBox;
        }

        // The file to be padded is 2w x 1h Con.tileSize. Create a row of tiles below 2w x 1h, montage them together.
        // Resulting imageURL is 2w x 2h Con.tileSize with original imageURL at top vertically.
        static internal BoundingBox PadSouth(BoundingBox boundingBox, int newTileSouth, string filename, int zoom)
        {
            File.Move($"{Parameters.ImageFolder}\\{filename}.png", $"{Parameters.ImageFolder}\\{filename}_0.png");

            OSM.DownloadOSMtileRow(newTileSouth, 1, boundingBox, zoom, filename);
            MontageTilesToRow(boundingBox.xAxis.Count, 1, filename);
            DeleteTempOSMfiles($"{filename}_?");

            MontageRows(boundingBox.xAxis.Count, 2, filename);
            DeleteTempOSMfiles(filename);

            return ZoomInNorthOrSouth(boundingBox);
        }

        static internal BoundingBox ZoomIn(BoundingBox boundingBox)
        {
            BoundingBox zoomInBoundingBox = new();
            List<int> ewAxis = [];
            for (int xIndex = 0; xIndex < boundingBox.xAxis.Count; xIndex++)
            {
                ewAxis.Add(2 * boundingBox.xAxis[xIndex]);
                ewAxis.Add(2 * boundingBox.xAxis[xIndex] + 1);
            }
            zoomInBoundingBox.xAxis = ewAxis;

            List<int> nsAxis = [];
            for (int xIndex = 0; xIndex < boundingBox.yAxis.Count; xIndex++)
            {
                nsAxis.Add(2 * boundingBox.yAxis[xIndex]);
                nsAxis.Add(2 * boundingBox.yAxis[xIndex] + 1);
            }
            zoomInBoundingBox.yAxis = nsAxis;
            return zoomInBoundingBox;
        }

        #endregion

        #region Drawing routines region

        static internal void DrawRoute(List<Tile> tiles, BoundingBox boundingBox, string filename)
        {
            using MagickImage image = new($"{Parameters.ImageFolder}\\{filename}.png");
            DrawableStrokeColor strokeColor = new(new MagickColor("blue"));
            DrawableStrokeWidth stokeWidth = new(1);
            DrawableFillColor fillColor = new(MagickColors.Transparent);

            int centrePrevX = 0, centrePrevY = 0;
            for (int tileNo = 0; tileNo < tiles.Count; tileNo++)
            {
                int centreX = (boundingBox.xAxis.IndexOf(tiles[tileNo].xIndex) * Con.tileSize) + tiles[tileNo].xOffset;
                int centreY = (boundingBox.yAxis.IndexOf(tiles[tileNo].yIndex) * Con.tileSize) + tiles[tileNo].yOffset;
                if (tileNo > 0)
                {
                    DrawableLine line = new(centrePrevX, centrePrevY, centreX, centreY);
                    image.Draw(strokeColor, stokeWidth, fillColor, line);
                }
                centrePrevX = centreX;
                centrePrevY = centreY;
            }

            image.Write($"{Parameters.ImageFolder}\\{filename}.png");
        }

        /// <summary>
        /// Draw the complete and incomplete images that display in the load scenario dialog
        /// </summary>
        static internal void DrawScenarioImages()
        {
            DrawScenarioLoadImage("success-icon", "imgM_c");
            DrawScenarioLoadImage("failure-icon", "imgM_i");
        }

        /// <summary>
        /// Draw an imageURL that displays in the load scenario dialog, result extension is bmp
        /// </summary>
        /// <param name="iconName">The name of the icon resource file to be overlayed (no extension)</param>
        /// <param name="outputName">The name of the base resource imageURL file (no extension)</param>
        static internal void DrawScenarioLoadImage(string iconName, string outputName)
        {
            // Make a copy of the base imageURL file
            string sourceFile = $"{Assembly.GetExecutingAssembly().GetName().Name.Replace(" ", "_")}.Resources.Images.imgM.png";
            using (Stream sourceStream = Assembly.Load(Assembly.GetExecutingAssembly().GetName().Name).GetManifestResourceStream(sourceFile))
            {
                using FileStream outputFileStream = new($"{Parameters.ImageFolder}\\{outputName}.png", FileMode.Create);
                    sourceStream.CopyTo(outputFileStream);
            }


#pragma warning disable IDE0063
            using (MagickImage image = new($"{Parameters.ImageFolder}\\{outputName}.png"))
#pragma warning restore IDE0063
            {
                // Write the scenario type on the base imageURL
                uint boundingBoxHeight = Convert.ToUInt32(image.Height / 2);
                uint boundingBoxWidth = image.Width;
                int boundingBoxYoffset = Convert.ToInt32(image.Height * 0.4);
                MagickGeometry geometry = new(0, boundingBoxYoffset, boundingBoxHeight, boundingBoxWidth);
                image.Settings.Font = "SegoeUI";
                image.Settings.FontPointsize = 36;
                image.Annotate(Parameters.SelectedScenario, geometry, Gravity.Center);

                // Overlay the icon imageURL on the base imageURL
                string iconFile = $"{Assembly.GetExecutingAssembly().GetName().Name.Replace(" ", "_")}.Resources.Images.{iconName}.png";
                using (Stream successIconStream = Assembly.Load(Assembly.GetExecutingAssembly().GetName().Name).GetManifestResourceStream(iconFile))
                {
                    using MagickImage imageIcon = new(successIconStream);
                    {
                        int iconXoffset = Convert.ToInt32(image.Width - imageIcon.Width * 2);
                        int iconYoffset = Convert.ToInt32(image.Height / 2 - imageIcon.Height / 2);
                        image.Composite(imageIcon, iconXoffset, iconYoffset, CompositeOperator.Over);
                    }
                }
                image.Write($"{Parameters.ImageFolder}\\{outputName}.png");
                ConvertImageformat(outputName, "png", "bmp");
            }
        }

        #endregion

        #region Montage tiles region

        static internal void MontageTilesToColumn(int yCount, int xIndex, string filename)
        {
            using var images = new MagickImageCollection();
            var settings = new MontageSettings
            {
                Geometry = new MagickGeometry($"{Con.tileSize}x{Con.tileSize}"),
                TileGeometry = new MagickGeometry($"1x{yCount}"),
            };
            for (int yIndex = 0; yIndex < yCount; yIndex++)
            {
                var tileImage = new MagickImage($"{Parameters.ImageFolder}\\{filename}_{xIndex}_{yIndex}.png");
                images.Add(tileImage);
            }
            using var result = images.Montage(settings);
            result.Write($"{Parameters.ImageFolder}\\{filename}_{xIndex}.png");
        }

        static internal void MontageTilesToRow(int xCount, int yIndex, string filename)
        {
            using var images = new MagickImageCollection();
            var settings = new MontageSettings
            {
                Geometry = new MagickGeometry($"{Con.tileSize}x{Con.tileSize}"),
                TileGeometry = new MagickGeometry($"{xCount}x1"),
            };
            for (int xIndex = 0; xIndex < xCount; xIndex++)
            {
                var tileImage = new MagickImage($"{Parameters.ImageFolder}\\{filename}_{xIndex}_{yIndex}.png");
                images.Add(tileImage);
            }
            using var result = images.Montage(settings);
            result.Write($"{Parameters.ImageFolder}\\{filename}_{yIndex}.png");
        }

        static internal void MontageColumns(int xCount, int yCount, string filename)
        {
            using var images = new MagickImageCollection();
            var settings = new MontageSettings
            {
                Geometry = new MagickGeometry($"{Con.tileSize}x{Con.tileSize * yCount}"),
                TileGeometry = new MagickGeometry($"{xCount}x1"),
            };
            for (int xIndex = 0; xIndex < xCount; xIndex++)
            {
                var tileImage = new MagickImage($"{Parameters.ImageFolder}\\{filename}_{xIndex}.png");
                images.Add(tileImage);
            }
            using var result = images.Montage(settings);
            result.Write($"{Parameters.ImageFolder}\\{filename}.png");
        }

        static internal void MontageRows(int xCount, int yCount, string filename)
        {
            using var images = new MagickImageCollection();
            var settings = new MontageSettings
            {
                Geometry = new MagickGeometry($"{Con.tileSize * xCount}x{Con.tileSize}"),
                TileGeometry = new MagickGeometry($"1x{yCount}"),
            };
            for (int yIndex = 0; yIndex < yCount; yIndex++)
            {
                var tileImage = new MagickImage($"{Parameters.ImageFolder}\\{filename}_{yIndex}.png");
                images.Add(tileImage);
            }
            using var result = images.Montage(settings);
            result.Write($"{Parameters.ImageFolder}\\{filename}.png");
        }

        static internal void MontageTiles(BoundingBox boundingBox, int zoom, string filename)
        {
            // Download the tile images from OSM in columns and montage into strips
            for (int xIndex = 0; xIndex < boundingBox.xAxis.Count; xIndex++)
            {
                OSM.DownloadOSMtileColumn(boundingBox.xAxis[xIndex], xIndex, boundingBox, zoom, $"{filename}");
                MontageTilesToColumn(boundingBox.yAxis.Count, xIndex, filename);
            }

            // Montage the OSM column strips to form the final imageURL
            MontageColumns(boundingBox.xAxis.Count, boundingBox.yAxis.Count, filename);

            DeleteTempOSMfiles(filename);
        }

        #endregion

        #region Utilities section

        static internal void ConvertImageformat(string filename, string oldExt, string newExt)
        {
            using MagickImage image = new($"{Parameters.ImageFolder}\\{filename}.{oldExt}");
            switch (newExt)
            {
                case "jpg":
                    image.Quality = 100;
                    break;
            }
            image.Write($"{Parameters.ImageFolder}\\{filename}.{newExt}");
            File.Delete($"{Parameters.ImageFolder}\\{filename}.{oldExt}");
        }

        static internal void DeleteTempOSMfiles(string filename)
        {
            foreach (string f in Directory.EnumerateFiles(Parameters.ImageFolder, $"{filename}_*.png"))
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

        static internal void Resize(string filename, int size)
        {
            uint sizeUint = Convert.ToUInt32(size);
            using MagickImage image = new($"{Parameters.ImageFolder}\\{filename}");
            image.Resize(sizeUint, sizeUint);
            image.Write($"{Parameters.ImageFolder}\\{filename}");
        }

        #endregion
    }
}
